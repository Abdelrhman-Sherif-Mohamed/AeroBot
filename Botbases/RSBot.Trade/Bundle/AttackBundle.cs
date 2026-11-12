using System;
using System.Linq;
using System.Threading;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Network;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Skill;
using RSBot.Core.Objects.Spawn;
using RSBot.Trade.Components;

namespace RSBot.Trade.Bundle;

internal class AttackBundle
{
    /// <summary>
    ///     Extra distance (in meters) the bot may chase a thief before it gives
    ///     up and returns to the trade route.
    /// </summary>
    private const double ChaseLeash = 15.0;

    private static readonly TimeSpan MoveInterval = TimeSpan.FromMilliseconds(350);
    private static readonly TimeSpan SelectInterval = TimeSpan.FromMilliseconds(450);
    private static readonly TimeSpan DismountInterval = TimeSpan.FromMilliseconds(750);
    private static readonly TimeSpan AttackInterval = TimeSpan.FromMilliseconds(150);
    private static readonly TimeSpan ObstacleEvasionInterval = TimeSpan.FromSeconds(2.5);

    private static readonly TimeSpan DismountTimeout = TimeSpan.FromSeconds(8);
    private static readonly TimeSpan CombatTimeout = TimeSpan.FromSeconds(45);

    private DateTime _lastMove = DateTime.MinValue;
    private DateTime _lastSelect = DateTime.MinValue;
    private DateTime _lastDismount = DateTime.MinValue;
    private DateTime _lastAttackTick = DateTime.MinValue;
    private DateTime _lastObstacleEvasion = DateTime.MinValue;
    private DateTime _dismountStartedAt = DateTime.MinValue;
    private DateTime _combatStartedAt = DateTime.MinValue;
    private uint _combatTargetUid;
    private uint _ignoredTargetUid;
    private DateTime _ignoreTargetUntil = DateTime.MinValue;
    private bool _pausedScript;

    /// <summary>
    ///     A value indicating if the bundle is busy attacking the selected target.
    /// </summary>
    public bool IsAttacking { get; private set; }

    /// <summary>
    ///     A value indicating if the bundle is busy and the execution of further commands should be paused.
    /// </summary>
    public bool Busy => IsAttacking;

    /// <summary>
    ///     UTC timestamp of when the last thief combat ended, used as a fallback anchor
    ///     for the 20-second post-combat remount penalty.
    /// </summary>
    public DateTime LastCombatEndedAt { get; private set; } = DateTime.MinValue;

    /// <summary>
    ///     Whether NPC thieves should be attacked. Smart combat mode enables the
    ///     basic protection behaviours and also enables both attack types.
    /// </summary>
    private static bool AttackThiefNpcs => TradeConfig.AttackThiefNpcs || TradeConfig.SmartThiefCombat;

    /// <summary>
    ///     Whether thief players should be attacked.
    /// </summary>
    private static bool AttackThiefPlayers => TradeConfig.AttackThiefPlayers || TradeConfig.SmartThiefCombat;

    public void Initialize()
    {
        LastCombatEndedAt = DateTime.MinValue;
        Reset(false);
    }

    public void Start()
    {
        LastCombatEndedAt = DateTime.MinValue;
        Reset(false);
    }

    private static readonly ushort[] TownRegionIds =
    {
        22106,
        22617,
        22618,
        22874,
        23088, // Hotan
        23603,
        23687, // Samarkand
        25000, // Jangan
        26265, // Donwhang
        26959, // Constantinople
        27244
    };

    /// <summary>
    ///     Returns true if the entity is a "real" NPC (merchant, specialty trader, guard, ...).
    ///     Important: SpawnedMonster derives from SpawnedNpc, so only SpawnedNpcNpc is a town NPC.
    /// </summary>
    public static bool IsRealNpc(SpawnedBionic bionic)
    {
        return bionic is SpawnedNpcNpc;
    }

    /// <summary>
    ///     Checks if an entity is strictly a Thief (either a Thief Player wearing a
    ///     job suit or a Thief NPC). Normal wild monsters strictly return false.
    /// </summary>
    public static bool IsThief(SpawnedBionic bionic)
    {
        if (bionic == null || bionic.State == null || bionic.State.LifeState != LifeState.Alive)
            return false;

        if (bionic is SpawnedPlayer player)
            return IsThiefPlayer(player);

        if (bionic is SpawnedMonster monster)
        {
            var record = monster.Record;
            if (record == null)
                return false;

            if (record.TypeID4 == 2)
                return true;

            var code = record.CodeName;
            return !string.IsNullOrEmpty(code) &&
                   (code.Contains("TIEF", StringComparison.OrdinalIgnoreCase) ||
                    code.Contains("THIEF", StringComparison.OrdinalIgnoreCase));
        }

        return false;
    }

    private static bool IsThiefPlayer(SpawnedPlayer player)
    {
        if (player == null || player.State == null || player.State.LifeState != LifeState.Alive)
            return false;

        if (player.Job == JobType.Thief)
            return true;

        var wearsJob = player.WearsJobSuite || (!string.IsNullOrEmpty(player.Name) && player.Name.StartsWith("*"));
        if (!wearsJob)
            return false;

        // If any equipped or avatar item indicates Thief suit
        if (player.Inventory?.Keys.Any(i => i != null && (i.TypeID4 == 2 || i.CodeName.Contains("THIEF", StringComparison.OrdinalIgnoreCase) || i.CodeName.Contains("TIEF", StringComparison.OrdinalIgnoreCase))) == true ||
            player.Avatars?.Keys.Any(i => i != null && (i.TypeID4 == 2 || i.CodeName.Contains("THIEF", StringComparison.OrdinalIgnoreCase) || i.CodeName.Contains("TIEF", StringComparison.OrdinalIgnoreCase))) == true)
        {
            return true;
        }

        // Check if player is an explicit ally (fellow Trader or Hunter)
        if (player.Job == JobType.Trade || player.Job == JobType.Hunter)
            return false;

        // Check if player has an active trade transport (camel, elephant, horse, behemoth, etc.)
        if (player.OnTransport && player.TransportUniqueId != 0)
        {
            if (SpawnManager.TryGetEntity<SpawnedCos>(player.TransportUniqueId, out var cos) &&
                cos.Record != null && cos.Record.TypeID3 == 3)
            {
                // Fellow trader with trade transport
                return false;
            }
        }

        // Check if player wears a Trader or Hunter suit
        var hasAllySuit =
            (player.Inventory?.Keys.Any(i => i != null && i.IsJobOutfit && (i.TypeID4 == 1 || i.TypeID4 == 3 || i.CodeName.Contains("TRADER", StringComparison.OrdinalIgnoreCase) || i.CodeName.Contains("HUNTER", StringComparison.OrdinalIgnoreCase))) == true) ||
            (player.Avatars?.Keys.Any(i => i != null && i.IsJobOutfit && (i.TypeID4 == 1 || i.TypeID4 == 3 || i.CodeName.Contains("TRADER", StringComparison.OrdinalIgnoreCase) || i.CodeName.Contains("HUNTER", StringComparison.OrdinalIgnoreCase))) == true);

        if (hasAllySuit)
            return false;

        // In Silkroad, any job-suited player that is not a fellow Trader or Hunter is an enemy Thief
        return true;
    }

    /// <summary>
    ///     Checks if the player is currently inside a town safe zone or active NPC dialog where attacks are not permitted.
    /// </summary>
    public static bool IsPlayerInTown()
    {
        if (Game.Player == null)
            return true;

        if (Bundles.RouteBundle.TownscriptRunning)
            return true;

        if (ShoppingManager.Running)
            return true;

        if (Game.Player.State?.DialogState is { IsInDialog: true })
            return true;

        var regionId = Game.Player.Position.Region.Id;
        if (TownRegionIds.Contains(regionId))
            return true;

        var xSec = Game.Player.Position.Region.X;
        var ySec = Game.Player.Position.Region.Y;

        if (xSec == 168 && ySec == 97) // Jangan
            return true;

        if (xSec == 79 && ySec == 105) // Constantinople
            return true;

        return false;
    }

    /// <summary>
    ///     Ticks this bundle. Strictly targets and attacks Thief NPCs and Thief Players only.
    /// </summary>
    public void Tick()
    {
        if (!TradeBotbase.IsActive)
        {
            Reset(false);
            return;
        }

        var player = Game.Player;
        if (player == null || player.State == null || player.State.LifeState != LifeState.Alive)
        {
            Reset(false);
            return;
        }

        if (IsPlayerInTown() || player.State.ScrollState != ScrollState.Cancel)
        {
            Reset(true);
            return;
        }

        var target = Game.SelectedEntity;

        // If an NPC remained selected after leaving town without an active dialog,
        // clear local selection so it does not block thief detection on the road.
        if (IsRealNpc(target))
        {
            Game.SelectedEntity = null;
            target = null;
        }

        // Drop outdated or out-of-leash targets cleanly (allow temporary obstacle overlap while
        // already engaged so obstacle evasion can steer around it instead of remounting mid-fight).
        if (target != null &&
            (!IsSelectable(target, allowBehindObstacle: target.UniqueId == _combatTargetUid) ||
             target.DistanceToPlayer > TradeConfig.MaxAttackDistance + ChaseLeash))
        {
            Log.Debug($"[Trade] Dropping invalid combat target [{target.UniqueId}]");
            DeselectTarget(target);
            target = null;
        }

        if (target == null)
            target = FindPotentialTarget();

        if (target == null)
        {
            if (IsAttacking)
                FinishCombat();

            return;
        }

        if (Game.SelectedEntity?.UniqueId != target.UniqueId)
        {
            if (DateTime.UtcNow - _lastSelect < SelectInterval)
                return;

            _lastSelect = DateTime.UtcNow;

            if (!target.TrySelect())
            {
                Log.Debug($"[Trade] Could not select thief [{target.UniqueId}]");
                return;
            }
        }

        Engage(target);
    }

    private static void DeselectTarget(SpawnedBionic target)
    {
        if (target is SpawnedNpcNpc)
            target.TryDeselect();

        if (Game.SelectedEntity?.UniqueId == target?.UniqueId)
            Game.SelectedEntity = null;
    }

    private void Engage(SpawnedBionic target)
    {
        var player = Game.Player;
        if (player == null)
            return;

        if (_combatTargetUid != target.UniqueId)
        {
            _combatTargetUid = target.UniqueId;
            _combatStartedAt = DateTime.UtcNow;
        }
        else if (DateTime.UtcNow - _combatStartedAt > CombatTimeout)
        {
            Log.Warn($"[Trade] Could not defeat the thief [{target.UniqueId}]. Ignoring temporarily and continuing route.");

            _ignoredTargetUid = target.UniqueId;
            _ignoreTargetUntil = DateTime.UtcNow.AddSeconds(10);
            _combatTargetUid = 0;
            _combatStartedAt = DateTime.MinValue;

            DeselectTarget(target);
            FinishCombat();
            return;
        }

        if (player.HasActiveVehicle)
        {
            if (!TradeConfig.DismountToAttackThief)
            {
                if (IsAttacking)
                    FinishCombat();

                return;
            }

            if (_dismountStartedAt == DateTime.MinValue)
                _dismountStartedAt = DateTime.UtcNow;

            if (DateTime.UtcNow - _dismountStartedAt > DismountTimeout)
            {
                Log.Warn("[Trade] Could not dismount transport to fight thief. Retrying shortly.");

                _dismountStartedAt = DateTime.MinValue;
                _ignoredTargetUid = target.UniqueId;
                _ignoreTargetUntil = DateTime.UtcNow.AddSeconds(5);

                DeselectTarget(target);
                FinishCombat();
                return;
            }

            IsAttacking = true;
            EnsureScriptPaused();

            if (DateTime.UtcNow - _lastDismount >= DismountInterval)
            {
                _lastDismount = DateTime.UtcNow;
                Log.Notify("[Trade] Thief detected! Stopping transport and dismounting to fight.");

                var vehicle = player.Vehicle ?? player.JobTransport;
                if (vehicle != null)
                {
                    if (vehicle.Movement.Moving || vehicle.Movement.HasDestination)
                    {
                        vehicle.MoveTo(vehicle.Position, sleep: false);
                        vehicle.StopMoving();
                    }

                    player.StopMoving();
                    SendDismountPacket(vehicle.UniqueId);
                }

                _lastMove = DateTime.MinValue;
            }

            return;
        }

        _dismountStartedAt = DateTime.MinValue;

        IsAttacking = true;
        EnsureScriptPaused();

        if (TradeConfig.CastBuffs)
            EventManager.FireEvent("Bundle.Buff.Invoke");

        if (SkillManager.ImbueSkill?.Record != null &&
            player.State != null &&
            !player.State.HasActiveBuff(SkillManager.ImbueSkill, out _) &&
            SkillManager.ImbueSkill.CanBeCasted)
        {
            SkillManager.ImbueSkill.Cast(buff: true);
        }

        AttackTarget(target);
    }

    private static void SendDismountPacket(uint vehicleUniqueId)
    {
        if (vehicleUniqueId == 0)
            return;

        var packet = new Packet(0x70CB);
        packet.WriteByte(0);
        packet.WriteUInt(vehicleUniqueId);
        PacketManager.SendPacket(packet, PacketDestination.Server);
    }

    private static double GetSkillRange(SkillInfo skill)
    {
        if (skill?.Record != null && skill.Record.Action_Range > 0)
            return Math.Max(2.0, (skill.Record.Action_Range / 10.0) - 0.5);

        var weapon = Game.Player?.Weapon;
        if (weapon?.Record != null)
        {
            var type4 = weapon.Record.TypeID4;
            if (type4 == 6 || type4 == 12) // Bow / Crossbow
                return 14.5;
            if (type4 == 10 || type4 == 11) // Staff / Warlock Rod
                return 13.5;
        }

        return 2.8;
    }

    private void AttackTarget(SpawnedBionic target)
    {
        var player = Game.Player;
        if (player == null || !player.CanAttack)
            return;

        if (DateTime.UtcNow - _lastAttackTick < AttackInterval)
            return;

        _lastAttackTick = DateTime.UtcNow;

        if (player.InAction)
        {
            if (SkillManager.IsLastCastedBasic)
                SkillManager.CancelAction();

            return;
        }

        if (Game.SelectedEntity?.UniqueId != target.UniqueId)
        {
            if (DateTime.UtcNow - _lastSelect >= SelectInterval)
            {
                _lastSelect = DateTime.UtcNow;
                target.TrySelect();
            }

            return;
        }

        try
        {
            if (target.IsBehindObstacle)
            {
                if (DateTime.UtcNow - _lastObstacleEvasion >= ObstacleEvasionInterval)
                {
                    _lastObstacleEvasion = DateTime.UtcNow;
                    if (TradeConfig.SmartObstacleEvasion)
                    {
                        player.PerformObstacleEvasion(target.Position, 1);
                        return;
                    }
                }
            }

            var skill = SkillManager.GetNextSkill();
            var effectiveRange = GetSkillRange(skill);

            if (target.DistanceToPlayer > effectiveRange)
            {
                if (DateTime.UtcNow - _lastMove >= MoveInterval)
                {
                    _lastMove = DateTime.UtcNow;
                    var dirX = target.Position.X - player.Position.X;
                    var dirY = target.Position.Y - player.Position.Y;
                    var dist = Math.Sqrt(dirX * dirX + dirY * dirY);
                    if (dist > 1.0)
                    {
                        var desiredDist = Math.Max(1.0, effectiveRange - 1.5);
                        var ratio = Math.Max(0.0, (dist - desiredDist) / dist);
                        var targetX = player.Position.X + dirX * ratio;
                        var targetY = player.Position.Y + dirY * ratio;
                        player.MoveTo(new Position((float)targetX, (float)targetY, target.Position.Region), sleep: false);
                    }
                }

                return;
            }

            if (player.Movement.Moving || player.Movement.HasDestination)
            {
                player.MoveTo(player.Position, sleep: false);
                player.StopMoving();
                Thread.Sleep(50);
            }

            if (skill != null && SkillManager.CastSkill(skill, target.UniqueId))
                return;

            // Fallback to basic attack for all weapon types when skills are unavailable/on cooldown.
            SkillManager.CastAutoAttack(target.UniqueId);
        }
        catch (Exception ex)
        {
            Log.Debug($"[Trade] Skill cast failed: {ex.Message}");
        }
    }

    private static bool IsValidTarget(SpawnedBionic bionic, bool allowBehindObstacle = false)
    {
        if (bionic == null || bionic.State == null)
            return false;

        if (bionic.State.LifeState != LifeState.Alive)
            return false;

        if (!allowBehindObstacle && bionic.IsBehindObstacle)
            return false;

        if (bionic is SpawnedPlayer player)
            return AttackThiefPlayers && IsThiefPlayer(player);

        if (bionic is SpawnedMonster)
            return AttackThiefNpcs && IsThief(bionic);

        return false;
    }

    private bool IsSelectable(SpawnedBionic bionic, bool allowBehindObstacle = false)
    {
        if (!IsValidTarget(bionic, allowBehindObstacle))
            return false;

        return _ignoredTargetUid == 0 ||
               bionic.UniqueId != _ignoredTargetUid ||
               DateTime.UtcNow >= _ignoreTargetUntil;
    }

    private SpawnedBionic FindPotentialTarget()
    {
        var player = Game.Player;
        if (player == null || IsPlayerInTown())
            return null;

        if (player.HasActiveVehicle && !TradeConfig.DismountToAttackThief)
            return null;

        var protectTransport = TradeConfig.ProtectTransport || TradeConfig.SmartThiefCombat;
        var counterAttack = TradeConfig.CounterAttack || TradeConfig.SmartThiefCombat;

        // Priority 1: Protect transport (even if momentarily near an obstacle edge)
        if (protectTransport && player.JobTransport != null)
        {
            var transport = player.JobTransport;

            if (SpawnManager.TryGetEntity<SpawnedBionic>(transport.UniqueId, out var transportBionic))
            {
                var attacker = transportBionic.GetAttackers()
                    .FirstOrDefault(a => a != null && a.UniqueId != transport.UniqueId &&
                                         IsSelectable(a, allowBehindObstacle: true) && IsWithinChaseRange(a));

                if (attacker != null)
                    return attacker;
            }

            if (AttackThiefPlayers && SpawnManager.TryGetEntity<SpawnedPlayer>(
                    p => p != null && IsSelectable(p, allowBehindObstacle: true) &&
                         (p.TargetId == transport.UniqueId || p.AttackingPlayer) && IsWithinChaseRange(p),
                    out var playerAttackingTransport))
            {
                return playerAttackingTransport;
            }

            if (AttackThiefNpcs && SpawnManager.TryGetEntity<SpawnedMonster>(
                    m => m != null && IsSelectable(m, allowBehindObstacle: true) &&
                         m.TargetId == transport.UniqueId && IsWithinChaseRange(m),
                    out var mobAttackingTransport))
            {
                return mobAttackingTransport;
            }
        }

        // Priority 2: Counter-attack thieves directly attacking the player
        if (counterAttack)
        {
            var attacker = player.GetAttackers()
                .FirstOrDefault(a => a != null && IsSelectable(a, allowBehindObstacle: true) && IsWithinChaseRange(a));

            if (attacker != null)
                return attacker;

            if (AttackThiefPlayers && SpawnManager.TryGetEntity<SpawnedPlayer>(
                    p => p != null && IsSelectable(p, allowBehindObstacle: true) &&
                         (p.TargetId == player.UniqueId || p.AttackingPlayer) && IsWithinChaseRange(p),
                    out var playerAttackingPlayer))
            {
                return playerAttackingPlayer;
            }

            if (AttackThiefNpcs && SpawnManager.TryGetEntity<SpawnedMonster>(
                    m => m != null && IsSelectable(m, allowBehindObstacle: true) &&
                         m.TargetId == player.UniqueId && IsWithinChaseRange(m),
                    out var mobAttackingPlayer))
            {
                return mobAttackingPlayer;
            }
        }

        // Priority 3: Closest thief player nearby (HIGH THREAT: Player thieves must be engaged immediately)
        if (AttackThiefPlayers && SpawnManager.TryGetEntities<SpawnedPlayer>(
                p => p != null && IsSelectable(p) && IsInRange(p), out var thiefPlayers))
        {
            var thiefPlayer = thiefPlayers.OrderBy(p => p.DistanceToPlayer).FirstOrDefault();
            if (thiefPlayer != null)
                return thiefPlayer;
        }

        // Priority 4: Closest thief NPC nearby
        if (AttackThiefNpcs && SpawnManager.TryGetEntities<SpawnedMonster>(
                m => m != null && IsSelectable(m) && IsInRange(m), out var thiefMobs))
        {
            var thiefMob = thiefMobs.OrderBy(m => m.DistanceToPlayer).FirstOrDefault();
            if (thiefMob != null)
                return thiefMob;
        }

        return null;
    }

    private static bool IsWithinChaseRange(SpawnedBionic bionic)
    {
        return bionic.DistanceToPlayer <= TradeConfig.MaxAttackDistance + ChaseLeash;
    }

    private static bool IsInRange(SpawnedBionic bionic)
    {
        var maxDistance = TradeConfig.MaxAttackDistance;

        if (bionic.DistanceToPlayer <= maxDistance)
            return true;

        var transport = Game.Player?.JobTransport;
        return transport != null && bionic.Position.DistanceTo(transport.Position) <= maxDistance;
    }

    private void EnsureScriptPaused()
    {
        if (_pausedScript)
            return;

        if (ScriptManager.Running && !ScriptManager.Paused)
        {
            ScriptManager.Pause();
            _pausedScript = true;
        }
    }

    private void ResumeScript()
    {
        if (!_pausedScript)
            return;

        _pausedScript = false;

        if (ScriptManager.Paused)
            ScriptManager.ResumeScript();
    }

    private void FinishCombat()
    {
        IsAttacking = false;
        LastCombatEndedAt = DateTime.UtcNow;

        _combatTargetUid = 0;
        _combatStartedAt = DateTime.MinValue;
        _dismountStartedAt = DateTime.MinValue;

        Log.Notify("[Trade] All nearby thieves defeated!");

        ResumeScript();

        Log.Notify("[Trade] Combat finished. Resuming trade route!");
    }

    private void Reset(bool deselect)
    {
        var wasAttacking = IsAttacking;
        IsAttacking = false;

        _combatTargetUid = 0;
        _combatStartedAt = DateTime.MinValue;
        _dismountStartedAt = DateTime.MinValue;

        if (deselect && wasAttacking && Game.SelectedEntity != null && IsThief(Game.SelectedEntity))
        {
            DeselectTarget(Game.SelectedEntity);
        }

        ResumeScript();
    }

    public void Stop()
    {
        IsAttacking = false;
        _pausedScript = false;
        _dismountStartedAt = DateTime.MinValue;
        _combatTargetUid = 0;
        _combatStartedAt = DateTime.MinValue;
        LastCombatEndedAt = DateTime.MinValue;

        if (Game.SelectedEntity != null && IsThief(Game.SelectedEntity))
        {
            DeselectTarget(Game.SelectedEntity);
        }
    }
}
