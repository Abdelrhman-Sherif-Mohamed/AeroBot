using System;
using System.Collections.Generic;
using System.Linq;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Event;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Item;
using RSBot.Core.Objects.Spawn;

namespace RSBot.Core.Components;

/// <summary>
///     Tracks player kills (including Thieves and PvP targets) and player deaths,
///     preserving snapshots of equipment enhancement levels and guild details before despawn.
/// </summary>
public static class KillDeathTracker
{
    private const int MaxRecords = 500;
    private const int RecentCombatWindowMs = 15000;
    private const int DuplicateKillWindowMs = 5000;
    private const int PendingActionTtlMs = 30000;
    private const int SnapshotTtlMs = 180000;

    private sealed class PlayerSnapshot
    {
        public uint UniqueId { get; init; }
        public string DisplayName { get; init; } = string.Empty;
        public string RealName { get; init; } = string.Empty;
        public bool IsRealNameHidden { get; init; }
        public JobType Job { get; init; }
        public bool IsThief { get; init; }
        public bool WearsJobSuite { get; init; }
        public byte JobLevel { get; init; }
        public string GuildName { get; init; } = string.Empty;
        public string GuildNickname { get; init; } = string.Empty;
        public PvpState PvpState { get; init; }
        public PvpCapeType PvpCape { get; init; }
        public string WeaponSummary { get; init; } = string.Empty;
        public string ShieldSummary { get; init; } = string.Empty;
        public string ArmorSummary { get; init; } = string.Empty;
        public int HighestWeaponPlus { get; init; } = -1;
        public int HighestArmorPlus { get; init; } = -1;
        public List<EquippedGearSummary> Equipment { get; init; } = new();
        public List<EquippedGearSummary> Avatars { get; init; } = new();
        public int CapturedTick { get; init; }
    }

    private sealed class CombatHitInfo
    {
        public uint ActorId { get; init; }
        public uint SkillId { get; init; }
        public int Damage { get; init; }
        public int Tick { get; init; }
        public PlayerSnapshot PlayerSnapshot { get; init; }
        public string MobName { get; init; } = string.Empty;
    }

    private readonly struct PendingActionInfo
    {
        public uint ExecutorId { get; init; }
        public uint SkillId { get; init; }
        public uint TargetId { get; init; }
        public int Tick { get; init; }
    }

    private static readonly object _lock = new();
    private static readonly List<KillDeathRecord> _records = new();
    private static readonly Dictionary<uint, string> _knownRealNames = new();
    private static readonly Dictionary<uint, PlayerSnapshot> _playerSnapshots = new();
    private static readonly Dictionary<uint, PendingActionInfo> _pendingActions = new();
    private static readonly Dictionary<uint, CombatHitInfo> _recentAttacksByMe = new();
    private static readonly Dictionary<uint, int> _reportedKillTicks = new();

    private static CombatHitInfo _lastAttackerOfMe;
    private static CombatHitInfo _lastPlayerAttackerOfMe;
    private static bool _playerDeathReported;
    private static long _nextRecordId = 1;
    private static bool _initialized;

    /// <summary>
    ///     Subscribes session lifecycle reset handlers once.
    /// </summary>
    public static void Initialize()
    {
        lock (_lock)
        {
            if (_initialized)
                return;

            _initialized = true;
        }

        EventManager.SubscribeEvent("OnEnterGame", ResetCombatState);
        EventManager.SubscribeEvent("OnAgentServerDisconnected", ResetCombatState);
    }

    /// <summary>
    ///     Clears transient combat state between logins/disconnects without discarding recorded history.
    /// </summary>
    public static void ResetCombatState()
    {
        lock (_lock)
        {
            _knownRealNames.Clear();
            _playerSnapshots.Clear();
            _pendingActions.Clear();
            _recentAttacksByMe.Clear();
            _reportedKillTicks.Clear();
            _lastAttackerOfMe = null;
            _lastPlayerAttackerOfMe = null;
            _playerDeathReported = false;
        }
    }

    /// <summary>
    ///     Returns a chronological copy (newest first) of recorded kill and death entries.
    /// </summary>
    public static KillDeathRecord[] GetRecords()
    {
        lock (_lock)
        {
            return _records.AsEnumerable().Reverse().ToArray();
        }
    }

    /// <summary>
    ///     Returns total recorded kills, deaths, and Thief kills.
    /// </summary>
    public static (int Kills, int Deaths, int ThiefKills) GetCounts()
    {
        lock (_lock)
        {
            var kills = 0;
            var deaths = 0;
            var thiefKills = 0;

            foreach (var rec in _records)
            {
                if (rec.EventType == KillDeathType.Kill)
                {
                    kills++;
                    if (rec.IsThief)
                        thiefKills++;
                }
                else
                {
                    deaths++;
                }
            }

            return (kills, deaths, thiefKills);
        }
    }

    /// <summary>
    ///     Clears either all records or only records matching <paramref name="filterType"/>.
    /// </summary>
    public static void Clear(KillDeathType? filterType = null)
    {
        lock (_lock)
        {
            if (filterType == null)
                _records.Clear();
            else
                _records.RemoveAll(r => r.EventType == filterType.Value);
        }

        EventManager.FireEvent("OnKillDeathCleared");
    }

    /// <summary>
    ///     Caches spawned player identity and equipment snapshot as soon as they appear.
    /// </summary>
    public static void OnPlayerSpawned(SpawnedPlayer player)
    {
        if (player == null)
            return;

        lock (_lock)
        {
            CacheRealNameAndSnapshotLocked(player);
            PruneStaleCachesLocked();
        }
    }

    /// <summary>
    ///     Clears active spawn snapshots when <see cref="SpawnManager.Clear"/> executes on teleport.
    /// </summary>
    public static void OnSpawnCleared()
    {
        lock (_lock)
        {
            _knownRealNames.Clear();
            _playerSnapshots.Clear();
            _pendingActions.Clear();
            _recentAttacksByMe.Clear();
            _reportedKillTicks.Clear();
            _playerDeathReported = false;
        }
    }

    /// <summary>
    ///     Records a skill cast start (0xB070) before hit details are parsed so delayed completions retain executor and skill id.
    /// </summary>
    public static void OnSkillCast(Objects.Action action)
    {
        if (action == null)
            return;

        // Read SpawnManager strictly outside _lock to avoid lock-order inversion.
        SpawnManager.TryGetEntity<SpawnedPlayer>(action.ExecutorId, out var execPlayer);
        SpawnManager.TryGetEntity<SpawnedBionic>(action.ExecutorId, out var execBionic);
        SpawnManager.TryGetEntity<SpawnedPlayer>(action.TargetId, out var targetPlayer);

        var now = Kernel.TickCount;

        lock (_lock)
        {
            if (action.Id != 0)
            {
                _pendingActions[action.Id] = new PendingActionInfo
                {
                    ExecutorId = action.ExecutorId,
                    SkillId = action.SkillId,
                    TargetId = action.TargetId,
                    Tick = now
                };
            }

            if (execPlayer != null)
                CacheRealNameAndSnapshotLocked(execPlayer);
            if (targetPlayer != null)
                CacheRealNameAndSnapshotLocked(targetPlayer);

            if (action.TargetId == 0)
                return;

            if (IsMyEntity(action.ExecutorId) && IsOffensiveOrUnknownSkill(action.SkillId) &&
                action.TargetId != Game.Player?.UniqueId)
            {
                if (_playerSnapshots.TryGetValue(action.TargetId, out var snap))
                {
                    _recentAttacksByMe[action.TargetId] = new CombatHitInfo
                    {
                        ActorId = action.ExecutorId,
                        SkillId = action.SkillId,
                        Damage = 0,
                        Tick = now,
                        PlayerSnapshot = snap
                    };
                }
            }
            else if (action.TargetId == Game.Player?.UniqueId && action.ExecutorId != 0 &&
                     !IsMyEntity(action.ExecutorId) && IsOffensiveOrUnknownSkill(action.SkillId))
            {
                TrackAttackerOfMeLocked(action.ExecutorId, execBionic, action.SkillId, 0, now);
            }
        }
    }

    /// <summary>
    ///     Records an individual hit detail from 0xB070 or 0xB071, capturing killing blows immediately.
    /// </summary>
    public static void OnHitDetail(uint actionId, uint executorId, uint targetId, ActionHitStateFlag hitState, int damage)
    {
        if (targetId == 0 || Game.Player == null)
            return;

        // Lookup entities outside _lock.
        SpawnManager.TryGetEntity<SpawnedBionic>(executorId, out var liveExecutor);
        SpawnManager.TryGetEntity<SpawnedPlayer>(targetId, out var liveTargetPlayer);

        KillDeathRecord pendingRecord = null;
        var now = Kernel.TickCount;

        lock (_lock)
        {
            uint skillId = 0;
            if (actionId != 0 && _pendingActions.TryGetValue(actionId, out var pending))
            {
                skillId = pending.SkillId;
                if (executorId == 0)
                    executorId = pending.ExecutorId;
            }

            if (liveExecutor is SpawnedPlayer execPlayer)
                CacheRealNameAndSnapshotLocked(execPlayer);
            if (liveTargetPlayer != null)
                CacheRealNameAndSnapshotLocked(liveTargetPlayer);

            if (IsMyEntity(executorId) && targetId != Game.Player.UniqueId)
            {
                if (_playerSnapshots.TryGetValue(targetId, out var targetSnap))
                {
                    _recentAttacksByMe[targetId] = new CombatHitInfo
                    {
                        ActorId = executorId,
                        SkillId = skillId,
                        Damage = damage,
                        Tick = now,
                        PlayerSnapshot = targetSnap
                    };

                    if (hitState.HasFlag(ActionHitStateFlag.Dead))
                        pendingRecord = TryCreateKillRecordLocked(targetId, targetSnap, skillId, damage, now);
                }
            }
            else if (targetId == Game.Player.UniqueId && executorId != 0 && !IsMyEntity(executorId))
            {
                TrackAttackerOfMeLocked(executorId, liveExecutor, skillId, damage, now);

                if (hitState.HasFlag(ActionHitStateFlag.Dead))
                    pendingRecord = TryCreateDeathRecordLocked(executorId, liveExecutor, null, skillId, damage, now);
            }
        }

        if (pendingRecord != null)
            PublishRecord(pendingRecord);
    }

    /// <summary>
    ///     Processes entity life-state transitions (0x30BF / lethal 0x3057) and resets dedupe when revived.
    /// </summary>
    public static void OnEntityLifeStateChanged(SpawnedEntity entity)
    {
        if (entity?.State == null || Game.Player == null)
            return;

        var isMe = entity.UniqueId == Game.Player.UniqueId;
        List<SpawnedBionic> fallbackAttackers = null;
        if (isMe && entity.State.LifeState == LifeState.Dead)
            fallbackAttackers = Game.Player.GetAttackers();

        KillDeathRecord pendingRecord = null;
        var now = Kernel.TickCount;

        lock (_lock)
        {
            if (isMe)
            {
                if (entity.State.LifeState == LifeState.Alive)
                {
                    _playerDeathReported = false;
                    return;
                }

                if (entity.State.LifeState == LifeState.Dead)
                    pendingRecord = TryCreateDeathRecordLocked(0, null, fallbackAttackers, 0, 0, now);
            }
            else if (entity is SpawnedPlayer enemyPlayer)
            {
                var snap = CacheRealNameAndSnapshotLocked(enemyPlayer);

                if (enemyPlayer.State.LifeState == LifeState.Alive)
                {
                    _reportedKillTicks.Remove(enemyPlayer.UniqueId);
                    return;
                }

                if (enemyPlayer.State.LifeState == LifeState.Dead &&
                    TryGetRecentAttackByMeLocked(enemyPlayer.UniqueId, now, out var attackInfo))
                {
                    pendingRecord = TryCreateKillRecordLocked(
                        enemyPlayer.UniqueId,
                        snap ?? attackInfo.PlayerSnapshot,
                        attackInfo.SkillId,
                        attackInfo.Damage,
                        now);
                }
            }
        }

        if (pendingRecord != null)
            PublishRecord(pendingRecord);
    }

    private static KillDeathRecord TryCreateKillRecordLocked(
        uint targetId,
        PlayerSnapshot snap,
        uint skillId,
        int damage,
        int now)
    {
        if (snap == null)
            return null;

        if (_reportedKillTicks.TryGetValue(targetId, out var lastTick) &&
            IsWithinWindow(lastTick, now, DuplicateKillWindowMs))
            return null;

        _reportedKillTicks[targetId] = now;
        _recentAttacksByMe.Remove(targetId);

        var record = BuildRecordFromSnapshot(
            KillDeathType.Kill,
            snap,
            GetSkillDisplayName(skillId),
            damage);

        AppendRecordLocked(record);
        return record;
    }

    private static KillDeathRecord TryCreateDeathRecordLocked(
        uint explicitKillerId,
        SpawnedBionic explicitKillerEntity,
        List<SpawnedBionic> fallbackAttackers,
        uint explicitSkillId,
        int explicitDamage,
        int now)
    {
        if (_playerDeathReported)
            return null;

        _playerDeathReported = true;

        var killerId = explicitKillerId;
        var skillId = explicitSkillId;
        var damage = explicitDamage;
        PlayerSnapshot playerSnap = null;
        var mobName = string.Empty;

        if (killerId != 0)
        {
            if (explicitKillerEntity is SpawnedPlayer p)
                playerSnap = CacheRealNameAndSnapshotLocked(p);
            else if (!_playerSnapshots.TryGetValue(killerId, out playerSnap) && explicitKillerEntity != null)
                mobName = SafeGetObjName(explicitKillerEntity.Record);
        }
        else
        {
            var chosen = SelectBestAttackerOfMeLocked(now);
            if (chosen != null)
            {
                killerId = chosen.ActorId;
                skillId = chosen.SkillId;
                damage = chosen.Damage;
                playerSnap = chosen.PlayerSnapshot ?? (_playerSnapshots.TryGetValue(killerId, out var s) ? s : null);
                mobName = chosen.MobName;
            }
        }

        KillDeathRecord record;
        if (playerSnap != null)
        {
            record = BuildRecordFromSnapshot(
                KillDeathType.Death,
                playerSnap,
                GetSkillDisplayName(skillId),
                damage);
        }
        else
        {
            var fallbackPlayer = fallbackAttackers?
                .OfType<SpawnedPlayer>()
                .FirstOrDefault(p => p.UniqueId != Game.Player?.UniqueId);

            if (fallbackPlayer != null)
            {
                var fallbackSnap = CacheRealNameAndSnapshotLocked(fallbackPlayer);
                record = BuildRecordFromSnapshot(
                    KillDeathType.Death,
                    fallbackSnap,
                    GetSkillDisplayName(skillId),
                    damage);
            }
            else
            {
                var fallbackMob = string.IsNullOrWhiteSpace(mobName)
                    ? SafeGetObjName(fallbackAttackers?.FirstOrDefault()?.Record)
                    : mobName;

                record = new KillDeathRecord
                {
                    Id = _nextRecordId++,
                    Timestamp = DateTime.Now,
                    EventType = KillDeathType.Death,
                    EntityUniqueId = killerId,
                    IsPlayer = false,
                    DisplayName = string.IsNullOrWhiteSpace(fallbackMob) ? "Unknown Attacker" : fallbackMob,
                    SkillName = GetSkillDisplayName(skillId),
                    Damage = damage
                };
            }
        }

        AppendRecordLocked(record);
        return record;
    }

    private static void AppendRecordLocked(KillDeathRecord record)
    {
        _records.Add(record);
        if (_records.Count > MaxRecords)
            _records.RemoveAt(0);
    }

    private static void PublishRecord(KillDeathRecord record)
    {
        EmitLogNotification(record);
        EventManager.FireEvent("OnKillDeathRecorded", record);
    }

    private static void EmitLogNotification(KillDeathRecord record)
    {
        var roleTag = record.IsThief
            ? "Thief"
            : record.Job != JobType.None
                ? record.Job.ToString()
                : record.IsPlayer ? "PvP" : "Monster";

        var parts = new List<string>
        {
            record.EventType == KillDeathType.Kill
                ? $"[Kill][{roleTag}] {record.DisplayName}"
                : $"[Death][{roleTag}] Killed by {record.DisplayName}"
        };

        if (!string.IsNullOrWhiteSpace(record.RealName) &&
            !string.Equals(record.RealName, record.DisplayName, StringComparison.Ordinal))
        {
            parts.Add($"Real: {record.RealName}");
        }

        if (!string.IsNullOrWhiteSpace(record.GuildName))
            parts.Add($"Guild: {record.GuildName}");

        if (!string.IsNullOrWhiteSpace(record.WeaponSummary) && record.WeaponSummary != "-")
            parts.Add($"Wep: {record.WeaponSummary}");

        if (!string.IsNullOrWhiteSpace(record.ArmorSummary) && record.ArmorSummary != "-")
            parts.Add($"Set: {record.ArmorSummary}");

        if (!string.IsNullOrWhiteSpace(record.SkillName))
            parts.Add($"Skill: {record.SkillName}");

        var line = string.Join(" | ", parts);
        if (record.EventType == KillDeathType.Death)
            Log.Warn(line);
        else
            Log.Notify(line);
    }

    private static KillDeathRecord BuildRecordFromSnapshot(
        KillDeathType eventType,
        PlayerSnapshot snap,
        string skillName,
        int damage)
    {
        return new KillDeathRecord
        {
            Id = _nextRecordId++,
            Timestamp = DateTime.Now,
            EventType = eventType,
            EntityUniqueId = snap.UniqueId,
            IsPlayer = true,
            DisplayName = snap.DisplayName,
            RealName = snap.RealName,
            IsRealNameHidden = snap.IsRealNameHidden,
            Job = snap.Job,
            IsThief = snap.IsThief,
            WearsJobSuite = snap.WearsJobSuite,
            JobLevel = snap.JobLevel,
            GuildName = snap.GuildName,
            GuildNickname = snap.GuildNickname,
            PvpState = snap.PvpState,
            PvpCape = snap.PvpCape,
            SkillName = skillName,
            Damage = damage,
            WeaponSummary = snap.WeaponSummary,
            ShieldSummary = snap.ShieldSummary,
            ArmorSummary = snap.ArmorSummary,
            HighestWeaponPlus = snap.HighestWeaponPlus,
            HighestArmorPlus = snap.HighestArmorPlus,
            Equipment = snap.Equipment.ToList(),
            Avatars = snap.Avatars.ToList()
        };
    }

    private static PlayerSnapshot CacheRealNameAndSnapshotLocked(SpawnedPlayer player)
    {
        var displayName = player.Name ?? string.Empty;
        var hasUnmaskedName = !player.WearsJobSuite &&
                              !string.IsNullOrWhiteSpace(displayName) &&
                              !displayName.StartsWith("*");

        if (hasUnmaskedName)
            _knownRealNames[player.UniqueId] = displayName;

        string realName;
        bool isRealNameHidden;
        if (hasUnmaskedName)
        {
            realName = displayName;
            isRealNameHidden = false;
        }
        else if (_knownRealNames.TryGetValue(player.UniqueId, out var observedName))
        {
            realName = observedName;
            isRealNameHidden = false;
        }
        else
        {
            // Silkroad servers intentionally send only Job Alias in SpawnPlayer when WearsJobSuite is true.
            realName = string.Empty;
            isRealNameHidden = player.WearsJobSuite || displayName.StartsWith("*");
        }

        var isThief = IsThiefPlayer(player);
        var (equipment, avatars, weaponSummary, shieldSummary, armorSummary, highestWepPlus, highestArmorPlus) =
            BuildEquipmentData(player);

        var snapshot = new PlayerSnapshot
        {
            UniqueId = player.UniqueId,
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? $"UID:{player.UniqueId}" : displayName,
            RealName = realName,
            IsRealNameHidden = isRealNameHidden,
            Job = isThief && player.Job == JobType.None ? JobType.Thief : player.Job,
            IsThief = isThief,
            WearsJobSuite = player.WearsJobSuite,
            JobLevel = player.JobLevel,
            GuildName = player.Guild?.Name ?? string.Empty,
            GuildNickname = player.Guild?.Member?.Nickname ?? string.Empty,
            PvpState = player.PvpState,
            PvpCape = player.PvpCape,
            WeaponSummary = weaponSummary,
            ShieldSummary = shieldSummary,
            ArmorSummary = armorSummary,
            HighestWeaponPlus = highestWepPlus,
            HighestArmorPlus = highestArmorPlus,
            Equipment = equipment,
            Avatars = avatars,
            CapturedTick = Kernel.TickCount
        };

        _playerSnapshots[player.UniqueId] = snapshot;
        return snapshot;
    }

    private static (
        List<EquippedGearSummary> Equipment,
        List<EquippedGearSummary> Avatars,
        string WeaponSummary,
        string ShieldSummary,
        string ArmorSummary,
        int HighestWepPlus,
        int HighestArmorPlus) BuildEquipmentData(SpawnedPlayer player)
    {
        var equipment = new List<EquippedGearSummary>();
        var avatars = new List<EquippedGearSummary>();

        if (player.Inventory != null)
        {
            foreach (var pair in player.Inventory)
            {
                if (pair.Key == null)
                    continue;

                var item = pair.Key;
                equipment.Add(new EquippedGearSummary
                {
                    SlotCategory = ClassifyItemCategory(item),
                    ItemName = SafeGetObjName(item, true),
                    CodeName = item.CodeName ?? string.Empty,
                    Degree = item.Degree,
                    TypeID4 = item.TypeID4,
                    OptLevel = pair.Value
                });
            }

            equipment.Sort((a, b) =>
            {
                var catCmp = GetCategorySortOrder(a.SlotCategory).CompareTo(GetCategorySortOrder(b.SlotCategory));
                if (catCmp != 0)
                    return catCmp;

                var optCmp = b.OptLevel.CompareTo(a.OptLevel);
                return optCmp != 0 ? optCmp : a.TypeID4.CompareTo(b.TypeID4);
            });
        }

        if (player.Avatars != null)
        {
            foreach (var pair in player.Avatars)
            {
                if (pair.Key == null)
                    continue;

                avatars.Add(new EquippedGearSummary
                {
                    SlotCategory = "Avatar",
                    ItemName = SafeGetObjName(pair.Key, true),
                    CodeName = pair.Key.CodeName ?? string.Empty,
                    Degree = pair.Key.Degree,
                    TypeID4 = pair.Key.TypeID4,
                    OptLevel = pair.Value
                });
            }
        }

        var weapons = equipment.Where(e => e.SlotCategory == "Weapon").ToList();
        var shields = equipment.Where(e => e.SlotCategory == "Shield").ToList();
        var armors = equipment.Where(e => e.SlotCategory == "Armor").ToList();

        var weaponSummary = weapons.Count > 0
            ? string.Join(" / ", weapons.Select(w => w.FormatDisplay()))
            : "-";

        var shieldSummary = shields.Count > 0
            ? string.Join(" / ", shields.Select(s => s.FormatDisplay()))
            : "-";

        if (shields.Count > 0 && weapons.Count > 0)
            weaponSummary = $"{weaponSummary} | {shields[0].FormatDisplay()}";

        var armorSummary = armors.Count > 0
            ? $"Max +{armors.Max(a => (int)a.OptLevel)} ({string.Join("/", armors.Select(a => $"+{a.OptLevel}"))})"
            : "-";

        var highestWepPlus = weapons.Count > 0 ? weapons.Max(w => (int)w.OptLevel) : -1;
        var highestArmorPlus = armors.Count > 0 ? armors.Max(a => (int)a.OptLevel) : -1;

        return (equipment, avatars, weaponSummary, shieldSummary, armorSummary, highestWepPlus, highestArmorPlus);
    }

    private static string ClassifyItemCategory(RefObjItem item)
    {
        if (!item.IsEquip)
            return "Other";
        if (item.TypeID3 == 6)
            return "Weapon";
        if (item.TypeID3 == 4)
            return "Shield";
        if (item.TypeID3 is 1 or 2 or 3 or 9 or 10 or 11)
            return "Armor";
        if (item.TypeID3 is 5 or 12)
            return "Accessory";
        if (item.TypeID3 == 7)
            return "JobSuit";
        if (item.TypeID3 is 13 or 14)
            return "Avatar";

        return "Other";
    }

    private static int GetCategorySortOrder(string category) => category switch
    {
        "Weapon" => 0,
        "Shield" => 1,
        "Armor" => 2,
        "Accessory" => 3,
        "JobSuit" => 4,
        "Avatar" => 5,
        _ => 6
    };

    private static void TrackAttackerOfMeLocked(
        uint executorId,
        SpawnedBionic liveExecutor,
        uint skillId,
        int damage,
        int now)
    {
        PlayerSnapshot snap = null;
        var mobName = string.Empty;

        if (liveExecutor is SpawnedPlayer p)
            snap = CacheRealNameAndSnapshotLocked(p);
        else if (!_playerSnapshots.TryGetValue(executorId, out snap) && liveExecutor != null)
            mobName = SafeGetObjName(liveExecutor.Record);

        var hit = new CombatHitInfo
        {
            ActorId = executorId,
            SkillId = skillId,
            Damage = damage,
            Tick = now,
            PlayerSnapshot = snap,
            MobName = mobName
        };

        _lastAttackerOfMe = hit;
        if (snap != null)
            _lastPlayerAttackerOfMe = hit;
    }

    private static CombatHitInfo SelectBestAttackerOfMeLocked(int now)
    {
        if (_lastPlayerAttackerOfMe != null &&
            IsWithinWindow(_lastPlayerAttackerOfMe.Tick, now, 6000))
        {
            return _lastPlayerAttackerOfMe;
        }

        if (_lastAttackerOfMe != null &&
            IsWithinWindow(_lastAttackerOfMe.Tick, now, RecentCombatWindowMs))
        {
            return _lastAttackerOfMe;
        }

        return null;
    }

    private static bool TryGetRecentAttackByMeLocked(uint targetId, int now, out CombatHitInfo info)
    {
        if (_recentAttacksByMe.TryGetValue(targetId, out info) &&
            IsWithinWindow(info.Tick, now, RecentCombatWindowMs))
            return true;

        info = null;
        return false;
    }

    private static bool IsThiefPlayer(SpawnedPlayer player)
    {
        if (player == null)
            return false;

        if (player.Job == JobType.Thief)
            return true;

        return player.WearsJobSuite &&
               player.Job != JobType.Trade &&
               player.Job != JobType.Hunter &&
               !string.IsNullOrEmpty(player.Name) &&
               player.Name.StartsWith("*");
    }

    private static bool IsMyEntity(uint uniqueId)
    {
        if (uniqueId == 0 || Game.Player == null)
            return false;

        return uniqueId == Game.Player.UniqueId ||
               uniqueId == Game.Player.Growth?.UniqueId ||
               uniqueId == Game.Player.Fellow?.UniqueId ||
               uniqueId == Game.Player.AbilityPet?.UniqueId ||
               uniqueId == Game.Player.Transport?.UniqueId ||
               uniqueId == Game.Player.JobTransport?.UniqueId;
    }

    private static bool IsOffensiveOrUnknownSkill(uint skillId)
    {
        if (skillId == 0)
            return true;

        var skill = Game.ReferenceManager?.GetRefSkill(skillId);
        if (skill == null)
            return true;

        // Avoid logging benign party/ally buffs as hostile attacks.
        return skill.TargetGroup_Enemy_P ||
               skill.TargetGroup_Enemy_M ||
               (!skill.TargetGroup_Ally && !skill.TargetGroup_Party);
    }

    private static string SafeGetObjName(RefObjCommon obj, bool displayRarity = false)
    {
        if (obj == null)
            return string.Empty;

        try
        {
            var realName = obj.GetRealName(displayRarity);
            if (!string.IsNullOrWhiteSpace(realName))
                return realName;
        }
        catch
        {
        }

        return !string.IsNullOrWhiteSpace(obj.CodeName) ? obj.CodeName : $"ID:{obj.ID}";
    }

    private static string GetSkillDisplayName(uint skillId)
    {
        if (skillId == 0)
            return string.Empty;

        return Game.ReferenceManager?.GetRefSkill(skillId)?.GetRealName() ?? $"Skill #{skillId}";
    }

    private static bool IsWithinWindow(int pastTick, int nowTick, int windowMs)
    {
        var delta = nowTick - pastTick;
        return delta >= 0 && delta <= windowMs;
    }

    private static void PruneStaleCachesLocked()
    {
        var now = Kernel.TickCount;

        if (_pendingActions.Count > 64)
        {
            foreach (var key in _pendingActions.Where(kv => !IsWithinWindow(kv.Value.Tick, now, PendingActionTtlMs)).Select(kv => kv.Key).ToList())
                _pendingActions.Remove(key);
        }

        if (_recentAttacksByMe.Count > 32)
        {
            foreach (var key in _recentAttacksByMe.Where(kv => !IsWithinWindow(kv.Value.Tick, now, RecentCombatWindowMs)).Select(kv => kv.Key).ToList())
                _recentAttacksByMe.Remove(key);
        }

        if (_playerSnapshots.Count > 200)
        {
            foreach (var key in _playerSnapshots.Where(kv => !IsWithinWindow(kv.Value.CapturedTick, now, SnapshotTtlMs)).Select(kv => kv.Key).ToList())
                _playerSnapshots.Remove(key);
        }
    }
}
