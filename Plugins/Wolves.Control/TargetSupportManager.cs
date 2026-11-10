using System;
using System.Collections.Generic;
using System.Linq;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Network;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Party;
using RSBot.Core.Objects.Spawn;

namespace Wolves.Control;

public class TargetSupportManager
{
    private static TargetSupportManager _instance;
    public static TargetSupportManager Instance => _instance ??= new TargetSupportManager();

    public bool Enabled { get; set; }
    public bool DefensiveMode { get; set; } = true;
    public bool AutoAttack { get; set; } = true;

    public List<string> Leaders { get; } = new();

    public uint LastTargetUID { get; private set; }
    public string LastTargetName { get; private set; } = string.Empty;
    public string StatusText { get; private set; } = "جاهز (Idle)";

    public event System.Action OnStateChanged;

    private int _lastTargetPollTick;
    private int _lastAttackTick;

    private TargetSupportManager()
    {
        LoadConfig();
    }

    public void LoadConfig()
    {
        Enabled = PlayerConfig.Get("Wolves.Control.TargetSupport.Enabled", PlayerConfig.Get("SonicBot.TargetSupport.Enabled", false));
        DefensiveMode = PlayerConfig.Get("Wolves.Control.TargetSupport.DefensiveMode", PlayerConfig.Get("SonicBot.TargetSupport.DefensiveMode", true));
        AutoAttack = PlayerConfig.Get("Wolves.Control.TargetSupport.AutoAttack", PlayerConfig.Get("SonicBot.TargetSupport.AutoAttack", true));

        Leaders.Clear();
        var saved = PlayerConfig.Get("Wolves.Control.TargetSupport.Leaders", PlayerConfig.Get("SonicBot.TargetSupport.Leaders", string.Empty));
        if (!string.IsNullOrWhiteSpace(saved))
        {
            var names = saved.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var n in names)
            {
                var trimmed = n.Trim();
                if (!string.IsNullOrEmpty(trimmed) && !Leaders.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
                    Leaders.Add(trimmed);
            }
        }
    }

    public void SaveConfig()
    {
        PlayerConfig.Set("Wolves.Control.TargetSupport.Enabled", Enabled);
        PlayerConfig.Set("Wolves.Control.TargetSupport.DefensiveMode", DefensiveMode);
        PlayerConfig.Set("Wolves.Control.TargetSupport.AutoAttack", AutoAttack);
        PlayerConfig.Set("Wolves.Control.TargetSupport.Leaders", string.Join(",", Leaders));
    }

    public void NotifyState()
    {
        OnStateChanged?.Invoke();
    }

    public void AddLeader(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        var trimmed = name.Trim();
        if (!Leaders.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
        {
            Leaders.Add(trimmed);
            SaveConfig();
            OnStateChanged?.Invoke();
        }
    }

    public void RemoveLeader(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        var trimmed = name.Trim();
        var found = Leaders.FirstOrDefault(l => string.Equals(l, trimmed, StringComparison.OrdinalIgnoreCase));
        if (found != null)
        {
            Leaders.Remove(found);
            SaveConfig();
            OnStateChanged?.Invoke();
        }
    }

    public bool IsLeader(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var trimmed = name.Trim();
        return Leaders.Any(l => string.Equals(l, trimmed, StringComparison.OrdinalIgnoreCase)) ||
               ControlManager.Instance.IsLeader(trimmed);
    }

    public bool IsLeader(uint uniqueId)
    {
        if (uniqueId == 0)
            return false;

        if (Game.Player != null && Game.Player.UniqueId == uniqueId)
            return false;

        // Check Party members
        if (Game.Party?.Members != null)
        {
            foreach (var m in Game.Party.Members)
            {
                if (m.Player != null && m.Player.UniqueId == uniqueId && IsLeader(m.Name))
                    return true;
            }
        }

        // Check Spawned players
        if (SpawnManager.TryGetEntity<SpawnedPlayer>(uniqueId, out var sp) && IsLeader(sp.Name))
            return true;

        return false;
    }

    public string GetPlayerName(uint uniqueId)
    {
        if (Game.Player != null && Game.Player.UniqueId == uniqueId)
            return Game.Player.Name;

        if (Game.Party?.Members != null)
        {
            var m = Game.Party.Members.FirstOrDefault(p => p.Player != null && p.Player.UniqueId == uniqueId);
            if (m != null)
                return m.Name;
        }

        if (SpawnManager.TryGetEntity<SpawnedPlayer>(uniqueId, out var sp))
            return sp.Name;

        return string.Empty;
    }

    public void OnSkillCastAction(uint attackerUID, uint targetUID)
    {
        if (!Enabled || Game.Player == null)
            return;

        // Ignore actions from ourself
        if (attackerUID == Game.Player.UniqueId)
            return;

        // Check if attacker is leader -> focus leader's target
        if (IsLeader(attackerUID))
        {
            if (targetUID != 0 && targetUID != Game.Player.UniqueId)
            {
                var leaderName = GetPlayerName(attackerUID);
                StatusText = $"تركيز الهدف مع القائد {leaderName}";
                TargetAndAttack(targetUID);
            }
            return;
        }

        // Defensive mode: leader is attacked -> focus attacker!
        if (DefensiveMode && IsLeader(targetUID))
        {
            if (attackerUID != 0 && attackerUID != Game.Player.UniqueId)
            {
                var leaderName = GetPlayerName(targetUID);
                StatusText = $"الدفاع عن القائد {leaderName} ضد المهاجم";
                TargetAndAttack(attackerUID);
            }
        }
    }

    /// <summary>
    ///     Polls the leader's current target every 250 ms and mirrors it,
    ///     including when the target is a player. Called from the kernel tick.
    /// </summary>
    public void Tick()
    {
        if (!Enabled || Game.Player == null)
            return;

        var now = Kernel.TickCount;
        if (now - _lastTargetPollTick < 250)
            return;

        _lastTargetPollTick = now;

        var leader = FindLeader();
        if (leader == null)
        {
            ClearForcedTarget();
            return;
        }

        var targetId = leader.TargetId;
        if (targetId == 0 || targetId == Game.Player.UniqueId)
        {
            ClearForcedTarget();
            SetStatus($"بانتظار هدف القائد {leader.Name}", 0, string.Empty);
            return;
        }

        if (!SpawnManager.TryGetEntity<SpawnedBionic>(targetId, out var target) || target == null)
        {
            ClearForcedTarget();
            SetStatus($"بانتظار هدف القائد {leader.Name}", 0, string.Empty);
            return;
        }

        // Never mirror a dead target or one of our own leaders.
        if (target.State == null || target.State.LifeState != LifeState.Alive ||
            (target.HasHealth && target.Health <= 0) ||
            IsLeader(target.UniqueId))
        {
            ClearForcedTarget();
            SetStatus($"بانتظار هدف القائد {leader.Name}", 0, string.Empty);
            return;
        }

        // Keep the target forced so the default bot base can not deselect it
        // (the default targeting only knows monsters).
        TargetOverride.Set(targetId, 1200);

        SetStatus($"تركيز الهدف مع القائد {leader.Name}", targetId,
            target.Record != null ? target.Record.GetRealName() : targetId.ToString());

        if (Game.SelectedEntity?.UniqueId != targetId)
        {
            var selectPacket = new Packet(0x7045);
            selectPacket.WriteUInt(targetId);
            PacketManager.SendPacket(selectPacket, PacketDestination.Server);
            Game.SelectedEntity = target;
        }

        if (AutoAttack)
            TryAutoAttack(target);
    }

    /// <summary>
    ///     Updates the exposed status and only raises the change event when
    ///     something actually changed (avoids flickering the UI every tick).
    /// </summary>
    private void SetStatus(string status, uint targetUid, string targetName)
    {
        var changed = false;

        if (StatusText != status)
        {
            StatusText = status;
            changed = true;
        }

        if (LastTargetUID != targetUid)
        {
            LastTargetUID = targetUid;
            changed = true;
        }

        if (LastTargetName != targetName)
        {
            LastTargetName = targetName;
            changed = true;
        }

        if (changed)
            OnStateChanged?.Invoke();
    }

    /// <summary>
    ///     Finds the first spawned leader from the configured leader list.
    /// </summary>
    private SpawnedPlayer FindLeader()
    {
        foreach (var leader in Leaders)
        {
            if (SpawnManager.TryGetEntity<SpawnedPlayer>(
                    p => p != null && string.Equals(p.Name, leader, StringComparison.OrdinalIgnoreCase),
                    out var player))
                return player;
        }

        return null;
    }

    /// <summary>
    ///     Casts a skill or the basic attack, throttled so we do not spam the
    ///     server with attack packets.
    /// </summary>
    private void TryAutoAttack(SpawnedBionic target)
    {
        if (Game.Player == null || Game.Player.InAction)
            return;

        var now = Kernel.TickCount;
        if (now - _lastAttackTick < 500)
            return;

        _lastAttackTick = now;

        if (Game.SelectedEntity?.UniqueId != target.UniqueId)
            Game.SelectedEntity = target;

        var skill = SkillManager.GetNextSkill();
        if (skill != null)
        {
            skill.Cast(target.UniqueId);
            return;
        }

        // No configured skill: fall back to the basic attack.
        SkillManager.CastAutoAttack();
    }

    private static void ClearForcedTarget()
    {
        TargetOverride.Clear();
    }

    public void TargetAndAttack(uint targetUID)
    {
        if (targetUID == 0 || (Game.Player != null && targetUID == Game.Player.UniqueId))
            return;

        if (!SpawnManager.TryGetEntity<SpawnedBionic>(targetUID, out var target))
            return;

        if (target.State == null || target.State.LifeState == LifeState.Dead)
            return;

        LastTargetUID = targetUID;
        LastTargetName = target.Record != null ? target.Record.GetRealName() : targetUID.ToString();

        // Force the target so the default bot base does not clear it.
        TargetOverride.Set(targetUID, 1200);

        // Select target via 0x7045
        var selectPacket = new Packet(0x7045);
        selectPacket.WriteUInt(targetUID);
        PacketManager.SendPacket(selectPacket, PacketDestination.Server);
        Game.SelectedEntity = target;

        // Auto Attack
        if (AutoAttack)
            TryAutoAttack(target);

        OnStateChanged?.Invoke();
    }

    public void HandleChatCommand(string sender, string message)
    {
        if (string.IsNullOrWhiteSpace(sender) || string.IsNullOrWhiteSpace(message))
            return;

        if (!IsLeader(sender))
            return;

        var cmd = message.Trim().ToUpperInvariant();
        if (cmd == "TARGET ON")
        {
            Enabled = true;
            SaveConfig();
            StatusText = $"تم تفعيل مزامنة الهدف بواسطة {sender}";
            OnStateChanged?.Invoke();
        }
        else if (cmd == "TARGET OFF")
        {
            Enabled = false;
            SaveConfig();
            StatusText = $"تم إيقاف مزامنة الهدف بواسطة {sender}";
            OnStateChanged?.Invoke();
        }
    }
}
