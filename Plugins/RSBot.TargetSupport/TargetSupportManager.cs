using System;
using System.Collections.Generic;
using System.Linq;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Network;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Party;
using RSBot.Core.Objects.Spawn;

namespace RSBot.TargetSupport;

public class TargetSupportManager
{
    private static TargetSupportManager _instance;
    public static TargetSupportManager Instance => _instance ??= new TargetSupportManager();

    public bool Enabled { get; set; }
    public bool DefensiveMode { get; set; }
    public bool AutoAttack { get; set; }

    public List<string> Leaders { get; } = new();

    public uint LastTargetUID { get; private set; }
    public string LastTargetName { get; private set; } = string.Empty;
    public string StatusText { get; private set; } = "Idle";

    public event System.Action OnStateChanged;

    private TargetSupportManager()
    {
        LoadConfig();
    }

    public void LoadConfig()
    {
        Enabled = PlayerConfig.Get("SonicBot.TargetSupport.Enabled", false);
        DefensiveMode = PlayerConfig.Get("SonicBot.TargetSupport.DefensiveMode", true);
        AutoAttack = PlayerConfig.Get("SonicBot.TargetSupport.AutoAttack", true);

        Leaders.Clear();
        var saved = PlayerConfig.Get("SonicBot.TargetSupport.Leaders", string.Empty);
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
        PlayerConfig.Set("SonicBot.TargetSupport.Enabled", Enabled);
        PlayerConfig.Set("SonicBot.TargetSupport.DefensiveMode", DefensiveMode);
        PlayerConfig.Set("SonicBot.TargetSupport.AutoAttack", AutoAttack);
        PlayerConfig.Set("SonicBot.TargetSupport.Leaders", string.Join(",", Leaders));
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

        return Leaders.Any(l => string.Equals(l, name.Trim(), StringComparison.OrdinalIgnoreCase));
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
                StatusText = $"Focusing target from leader {leaderName}";
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
                StatusText = $"Defending leader {leaderName} from attacker";
                TargetAndAttack(attackerUID);
            }
        }
    }

    public void TargetAndAttack(uint targetUID)
    {
        if (targetUID == 0 || (Game.Player != null && targetUID == Game.Player.UniqueId))
            return;

        if (!SpawnManager.TryGetEntity<SpawnedBionic>(targetUID, out var target))
            return;

        if (target.State.LifeState == LifeState.Dead)
            return;

        LastTargetUID = targetUID;
        LastTargetName = target.Record != null ? target.Record.GetRealName() : targetUID.ToString();

        // Select target via 0x7045
        var selectPacket = new Packet(0x7045);
        selectPacket.WriteUInt(targetUID);
        PacketManager.SendPacket(selectPacket, PacketDestination.Server);

        // Auto Attack
        if (AutoAttack)
        {
            // Cast first available attack skill, or standard attack
            var attackSkills = Game.Player?.Skills?.KnownSkills?
                .Where(s => s.CanBeCasted && s.IsAttack && !s.IsPassive)
                .ToList();

            if (attackSkills != null && attackSkills.Count > 0)
            {
                attackSkills[0].Cast(targetUID);
            }
        }

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
            StatusText = $"Target sync enabled by {sender}";
            OnStateChanged?.Invoke();
        }
        else if (cmd == "TARGET OFF")
        {
            Enabled = false;
            SaveConfig();
            StatusText = $"Target sync disabled by {sender}";
            OnStateChanged?.Invoke();
        }
    }
}
