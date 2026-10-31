using System;
using System.Collections.Generic;
using System.Linq;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Network;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Skill;
using RSBot.Core.Objects.Spawn;

namespace Wolves.Players;

public class PlayerTraceManager
{
    private static PlayerTraceManager _instance;

    public static PlayerTraceManager Instance => _instance ??= new PlayerTraceManager();

    private readonly Dictionary<uint, long> _selfBuffCooldowns = new();
    private readonly Dictionary<(uint SkillId, uint TargetId), long> _partyBuffCooldowns = new();

    private System.Threading.Timer _timer;
    private bool _initialized;

    private long _lastMoveTick;
    private long _lastTracePacketTick;
    private long _lastAttackTick;
    private Position _lastMoveTarget;
    private int _tickBusy;

    public bool IsTracing { get; private set; }

    public uint TraceTargetId { get; private set; }

    public string TraceTargetName { get; private set; }

    public string StatusText { get; private set; } = "متوقف";

    public bool InGameTrace { get; set; } = true;

    public bool EnableBuffs { get; set; }

    public bool EnablePartyBuffs { get; set; }

    public bool AttackMonsters { get; set; }

    public int TraceDistance { get; set; } = 5;

    public int AttackRange { get; set; } = 30;

    public event System.Action OnStateChanged;

    public event System.Action<string> OnLogAdded;

    private PlayerTraceManager()
    {
    }

    public void Initialize()
    {
        if (_initialized)
            return;

        _initialized = true;

        EventManager.SubscribeEvent("OnLoadCharacter", OnLoadCharacter);
        EventManager.SubscribeEvent("OnStartBot", OnBotStarted);
        EventManager.SubscribeEvent("OnAgentServerDisconnected", OnConnectionLost);
        EventManager.SubscribeEvent("OnLogout", OnConnectionLost);

        LoadSettings();

        _timer = new System.Threading.Timer(Tick, null, 250, 250);
    }

    public void LoadSettings()
    {
        // PlayerConfig might not be loaded yet when the plugin is initialized.
        // In that case keep the built-in defaults and reload on OnLoadCharacter.
        if (!PlayerConfig.Exists("Wolves.Players.TraceDistance"))
        {
            InGameTrace = true;
            EnableBuffs = false;
            EnablePartyBuffs = false;
            AttackMonsters = false;
            TraceDistance = 5;
            AttackRange = 30;
            return;
        }

        InGameTrace = PlayerConfig.Get("Wolves.Players.InGameTrace", true);
        EnableBuffs = PlayerConfig.Get("Wolves.Players.EnableBuffs", false);
        EnablePartyBuffs = PlayerConfig.Get("Wolves.Players.EnablePartyBuffs", false);
        AttackMonsters = PlayerConfig.Get("Wolves.Players.AttackMonsters", false);
        TraceDistance = Math.Max(1, PlayerConfig.Get("Wolves.Players.TraceDistance", 5));
        AttackRange = Math.Max(1, PlayerConfig.Get("Wolves.Players.AttackRange", 30));
    }

    public void SaveSettings()
    {
        PlayerConfig.Set("Wolves.Players.InGameTrace", InGameTrace);
        PlayerConfig.Set("Wolves.Players.EnableBuffs", EnableBuffs);
        PlayerConfig.Set("Wolves.Players.EnablePartyBuffs", EnablePartyBuffs);
        PlayerConfig.Set("Wolves.Players.AttackMonsters", AttackMonsters);
        PlayerConfig.Set("Wolves.Players.TraceDistance", TraceDistance);
        PlayerConfig.Set("Wolves.Players.AttackRange", AttackRange);
    }

    public bool StartTrace(string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName))
        {
            SetStatus("اختر لاعباً أولاً");
            return false;
        }

        if (!SpawnManager.TryGetEntity<SpawnedPlayer>(
                p => string.Equals(p.Name, playerName.Trim(), StringComparison.OrdinalIgnoreCase),
                out var target))
        {
            SetStatus($"اللاعب غير موجود: {playerName}");
            return false;
        }

        return StartTrace(target);
    }

    public bool StartTrace(SpawnedPlayer target)
    {
        if (target == null || Game.Player == null)
            return false;

        if (Kernel.Bot != null && Kernel.Bot.Running)
            Kernel.Bot.Stop();

        if (IsTracing)
            StopTraceInternal(true, "متوقف");

        TraceTargetId = target.UniqueId;
        TraceTargetName = target.Name;
        IsTracing = true;

        _selfBuffCooldowns.Clear();
        _partyBuffCooldowns.Clear();
        _lastMoveTick = 0;
        _lastTracePacketTick = 0;
        _lastAttackTick = 0;

        if (InGameTrace)
            SendTracePacket(target.UniqueId);

        StatusText = $"يتتبع: {target.Name}";
        OnStateChanged?.Invoke();
        AddLog($"Start tracing {target.Name} ({target.UniqueId})");

        return true;
    }

    public void StopTrace()
    {
        StopTraceInternal(true, "متوقف");
    }

    /// <summary>
    ///     Applies a change of <see cref="InGameTrace" /> while a trace is running.
    /// </summary>
    public void ApplyTraceMode()
    {
        if (!IsTracing || Game.Player == null || !Game.Ready)
            return;

        if (InGameTrace)
        {
            if (TraceTargetId != 0)
                SendTracePacket(TraceTargetId);

            return;
        }

        CancelInGameTrace();

        _lastMoveTick = 0;
        _lastMoveTarget = default;
    }

    public void Reset()
    {
        StopTraceInternal(true, "متوقف");

        _selfBuffCooldowns.Clear();
        _partyBuffCooldowns.Clear();
        _lastMoveTarget = default;
        _lastMoveTick = 0;
        _lastTracePacketTick = 0;
        _lastAttackTick = 0;

        OnStateChanged?.Invoke();
    }

    private void StopTraceInternal(bool sendCancel, string status)
    {
        var wasTracing = IsTracing;
        var targetName = TraceTargetName;

        IsTracing = false;
        TraceTargetId = 0;
        TraceTargetName = null;

        if (sendCancel && Game.Player != null && Game.Ready)
            CancelInGameTrace();

        StatusText = status;
        OnStateChanged?.Invoke();

        if (wasTracing)
            AddLog($"Stop tracing {targetName}");
    }

    private void CancelInGameTrace()
    {
        if (Game.Player == null)
            return;

        try
        {
            var packet = new Packet(0x7074);
            packet.WriteByte(2); // Cancel the current action / trace

            PacketManager.SendPacket(packet, PacketDestination.Server);

            // Stop any movement that is still in progress.
            Game.Player.MoveTo(Game.Player.Movement.Source, false);
        }
        catch (Exception ex)
        {
            Log.Debug($"[Wolves.Players] Failed to cancel trace: {ex.Message}");
        }
    }

    private void Tick(object state)
    {
        if (System.Threading.Interlocked.CompareExchange(ref _tickBusy, 1, 0) != 0)
            return;

        try
        {
            if (!IsTracing)
                return;

            if (Game.Player == null || !Game.Ready)
            {
                StopTraceInternal(false, "توقف: انقطع الاتصال");
                return;
            }

            if (Kernel.Bot != null && Kernel.Bot.Running)
            {
                StopTraceInternal(true, "توقف: بدأ البوت");
                return;
            }

            if (!TryResolveTarget(out var target))
            {
                StopTraceInternal(true, $"توقف: {TraceTargetName} غير موجود");
                return;
            }

            if (!string.Equals(TraceTargetName, target.Name, StringComparison.Ordinal))
            {
                TraceTargetName = target.Name;
                TraceTargetId = target.UniqueId;
                SetStatus($"يتتبع: {target.Name}");
            }

            var now = Environment.TickCount64;

            if (InGameTrace)
            {
                // The server keeps following the target. Re-send occasionally if the
                // target got too far away (for example after a teleport).
                if (now - _lastTracePacketTick > 4000 &&
                    target.DistanceToPlayer > TraceDistance + 10)
                    SendTracePacket(target.UniqueId);
            }
            else
            {
                FollowTarget(target, now);
            }

            if (EnableBuffs)
                ProcessSelfBuffs(now);

            if (EnablePartyBuffs)
                ProcessPartyBuffs(now);

            if (AttackMonsters)
                ProcessAttack(now);
        }
        catch (Exception ex)
        {
            Log.Debug($"[Wolves.Players] Tick error: {ex.Message}");
        }
        finally
        {
            System.Threading.Interlocked.Exchange(ref _tickBusy, 0);
        }
    }

    private bool TryResolveTarget(out SpawnedPlayer target)
    {
        target = null;

        if (TraceTargetId != 0 &&
            SpawnManager.TryGetEntity<SpawnedPlayer>(TraceTargetId, out var byId) &&
            !string.IsNullOrWhiteSpace(byId.Name))
        {
            target = byId;
            return true;
        }

        if (!string.IsNullOrWhiteSpace(TraceTargetName) &&
            SpawnManager.TryGetEntity<SpawnedPlayer>(
                p => string.Equals(p.Name, TraceTargetName, StringComparison.OrdinalIgnoreCase),
                out var byName))
        {
            target = byName;
            return true;
        }

        return false;
    }

    private void SendTracePacket(uint uniqueId)
    {
        var packet = new Packet(0x7074);
        packet.WriteByte(1); // Execute
        packet.WriteByte(3); // Trace
        packet.WriteByte(1); // Target type: entity
        packet.WriteUInt(uniqueId);

        PacketManager.SendPacket(packet, PacketDestination.Server);

        _lastTracePacketTick = Environment.TickCount64;
    }

    private void FollowTarget(SpawnedPlayer target, long now)
    {
        if (Game.Player == null)
            return;

        var from = Game.Player.Movement.Source;
        var to = target.Movement.Source;
        var distance = from.DistanceTo(to);

        if (distance <= Math.Max(2, TraceDistance))
            return;

        // Do not flood the server with movement packets while the current step
        // is still running and the target did not move noticeably.
        if (now - _lastMoveTick < 1000 && _lastMoveTarget.DistanceTo(to) < 8)
            return;

        if (!Game.Player.MoveTo(to, false))
            return;

        _lastMoveTick = now;
        _lastMoveTarget = to;
    }

    private void ProcessSelfBuffs(long now)
    {
        var buffs = SkillManager.Buffs;
        if (buffs == null || buffs.Count == 0 || Game.Player == null)
            return;

        foreach (var skill in buffs)
        {
            if (skill == null || skill.Id == 0 || skill.Record == null)
                continue;

            if (HasBuff(Game.Player, skill.Id))
            {
                _selfBuffCooldowns.Remove(skill.Id);
                continue;
            }

            if (_selfBuffCooldowns.TryGetValue(skill.Id, out var lastCast) && now - lastCast < 8000)
                continue;

            SkillManager.CastBuff(skill, Game.Player.UniqueId, false);
            _selfBuffCooldowns[skill.Id] = now;
            return;
        }
    }

    private void ProcessPartyBuffs(long now)
    {
        var party = Game.Party;
        if (party == null || Game.Player == null)
            return;

        var buffs = SkillManager.Buffs;
        if (buffs == null || buffs.Count == 0)
            return;

        var names = new List<string>();

        if (party.Leader != null && !string.IsNullOrWhiteSpace(party.Leader.Name))
            names.Add(party.Leader.Name);

        if (party.Members != null)
            names.AddRange(party.Members
                .Where(m => m != null && !string.IsNullOrWhiteSpace(m.Name))
                .Select(m => m.Name));

        foreach (var name in names.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (string.Equals(name, Game.Player.Name, StringComparison.OrdinalIgnoreCase))
                continue;

            if (!SpawnManager.TryGetEntity<SpawnedPlayer>(
                    p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase),
                    out var member))
                continue;

            if (member.DistanceToPlayer > 60)
                continue;

            foreach (var skill in buffs)
            {
                if (skill == null || skill.Id == 0 || skill.Record == null)
                    continue;

                if (!skill.Record.TargetGroup_Party)
                    continue;

                if (HasBuff(member, skill.Id))
                    continue;

                var key = (skill.Id, member.UniqueId);
                if (_partyBuffCooldowns.TryGetValue(key, out var lastCast) && now - lastCast < 10000)
                    continue;

                SkillManager.CastBuff(skill, member.UniqueId, false);
                _partyBuffCooldowns[key] = now;
                return;
            }
        }
    }

    private void ProcessAttack(long now)
    {
        if (now - _lastAttackTick < 600)
            return;

        if (!SpawnManager.TryGetEntities<SpawnedMonster>(
                m => m != null &&
                     m.State.LifeState == LifeState.Alive &&
                     m.Health > 0 &&
                     m.DistanceToPlayer <= AttackRange,
                out var monsters))
            return;

        SpawnedMonster target = null;
        var bestDistance = double.MaxValue;

        foreach (var monster in monsters)
        {
            var distance = monster.DistanceToPlayer;
            if (distance >= bestDistance)
                continue;

            bestDistance = distance;
            target = monster;
        }

        if (target == null)
            return;

        if (Game.SelectedEntity?.UniqueId != target.UniqueId && !target.TrySelect())
            return;

        var casted = false;

        if (SkillManager.Skills != null)
        {
            var skill = SkillManager.GetNextSkill();
            if (skill != null)
                casted = SkillManager.CastSkill(skill, target.UniqueId);
        }

        if (!casted)
            casted = SkillManager.CastAutoAttack();

        if (casted)
            _lastAttackTick = now;
    }

    private static bool HasBuff(SpawnedBionic bionic, uint skillId)
    {
        return bionic != null &&
               bionic.State.ActiveBuffs != null &&
               bionic.State.ActiveBuffs.Any(b => b != null && b.Id == skillId);
    }

    private void SetStatus(string status)
    {
        if (string.Equals(StatusText, status, StringComparison.Ordinal))
            return;

        StatusText = status;
        OnStateChanged?.Invoke();
    }

    private void AddLog(string message)
    {
        Log.Notify($"[Wolves.Players] {message}");
        OnLogAdded?.Invoke(message);
    }

    private void OnLoadCharacter()
    {
        LoadSettings();

        if (IsTracing)
            StopTraceInternal(false, "متوقف");

        SetStatus("متوقف");
        OnStateChanged?.Invoke();
    }

    private void OnBotStarted()
    {
        if (IsTracing)
            StopTraceInternal(true, "توقف: بدأ البوت");
    }

    private void OnConnectionLost()
    {
        if (IsTracing)
            StopTraceInternal(false, "توقف: انقطع الاتصال");
    }
}
