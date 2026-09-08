using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Network;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Spawn;
using RSBot.MobSelector.Objects;

namespace RSBot.MobSelector;

public class MobSelectorManager
{
    private static MobSelectorManager _instance;
    public static MobSelectorManager Instance => _instance ??= new MobSelectorManager();

    private readonly string _configPath;
    private readonly Timer _scanTimer;

    public bool Enabled { get; set; }
    public bool AutoRefreshNearby { get; set; }
    public bool TargetAllUniques { get; set; } = true;
    public bool TargetAllTitans { get; set; } = true;
    public bool TargetAllGiants { get; set; }
    public bool TargetAllElites { get; set; }

    public List<MobSelectorRule> Rules { get; } = new();

    public uint SelectedTargetUID { get; private set; }
    public string StatusText { get; private set; } = "Idle";

    public event System.Action OnStateChanged;
    public event System.Action OnRulesChanged;

    private MobSelectorManager()
    {
        _configPath = Path.Combine(Kernel.BasePath, "Data", "MobSelector.json");
        LoadConfig();

        _scanTimer = new Timer(ScanTick, null, 1000, 400);
    }

    private void ScanTick(object state)
    {
        if (!Enabled || Game.Player == null)
            return;

        try
        {
            SearchAndDestroy();
        }
        catch (Exception ex)
        {
            Log.Debug($"MobSelector scan error: {ex.Message}");
        }
    }

    public void SearchAndDestroy()
    {
        if (!SpawnManager.TryGetEntities<SpawnedMonster>(out var monsters) || !monsters.Any())
        {
            SelectedTargetUID = 0;
            return;
        }

        // Check if currently targeted monster is still valid and alive
        if (SelectedTargetUID != 0)
        {
            var cur = monsters.FirstOrDefault(m => m.UniqueId == SelectedTargetUID);
            if (cur != null && cur.Health > 0 && cur.DistanceToPlayer < 70)
            {
                // Current target is still valid, continue focusing on it
                return;
            }
            SelectedTargetUID = 0;
        }

        SpawnedMonster bestTarget = null;

        // Priority 1: Uniques (if TargetAllUniques enabled)
        if (TargetAllUniques)
        {
            bestTarget = monsters.FirstOrDefault(m =>
                (m.Rarity == MonsterRarity.Unique || m.Rarity == MonsterRarity.Unique2 || m.Rarity == MonsterRarity.UniqueParty) &&
                m.Health > 0);
        }

        // Priority 2: Titans (if TargetAllTitans enabled)
        if (bestTarget == null && TargetAllTitans)
        {
            bestTarget = monsters.FirstOrDefault(m =>
                (m.Rarity == MonsterRarity.Titan || m.Rarity == MonsterRarity.TitanParty) &&
                m.Health > 0);
        }

        // Priority 3: Giants (if TargetAllGiants enabled)
        if (bestTarget == null && TargetAllGiants)
        {
            bestTarget = monsters.FirstOrDefault(m =>
                (m.Rarity == MonsterRarity.Giant || m.Rarity == MonsterRarity.GiantParty) &&
                m.Health > 0);
        }

        // Priority 4: Elites (if TargetAllElites enabled)
        if (bestTarget == null && TargetAllElites)
        {
            bestTarget = monsters.FirstOrDefault(m =>
                (m.Rarity == MonsterRarity.Elite || m.Rarity == MonsterRarity.EliteStrong || m.Rarity == MonsterRarity.EliteParty) &&
                m.Health > 0);
        }

        // Priority 5: Custom user rules
        if (bestTarget == null && Rules.Count > 0)
        {
            foreach (var rule in Rules)
            {
                var match = monsters.FirstOrDefault(m => m.Health > 0 && rule.Matches(m));
                if (match != null)
                {
                    bestTarget = match;
                    break;
                }
            }
        }

        // Target found!
        if (bestTarget != null && bestTarget.UniqueId != SelectedTargetUID)
        {
            InjectSelectTarget(bestTarget.UniqueId);
            SelectedTargetUID = bestTarget.UniqueId;
            StatusText = $"Targeting: {bestTarget.Record.GetRealName()} [{bestTarget.Rarity}] (UID: {bestTarget.UniqueId})";
            OnStateChanged?.Invoke();
        }
    }

    public void InjectSelectTarget(uint targetUID)
    {
        var packet = new Packet(0x7045);
        packet.WriteUInt(targetUID);
        PacketManager.SendPacket(packet, PacketDestination.Server);
    }

    public void HandleTargetResponse(Packet packet)
    {
        var success = packet.ReadByte() == 1;
        if (success)
        {
            var uid = packet.ReadUInt();
            if (SelectedTargetUID == uid)
            {
                StatusText = $"Locked on target UID [{uid}]";
                OnStateChanged?.Invoke();
            }
        }
        else
        {
            SelectedTargetUID = 0;
        }
    }

    public void AddRule(string name, byte rarity = 0xFF)
    {
        if (Rules.Any(r => r.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && r.Rarity == rarity))
            return;

        Rules.Add(new MobSelectorRule(name, rarity));
        SaveConfig();
        OnRulesChanged?.Invoke();
    }

    public void RemoveRuleAt(int index)
    {
        if (index >= 0 && index < Rules.Count)
        {
            Rules.RemoveAt(index);
            SaveConfig();
            OnRulesChanged?.Invoke();
        }
    }

    public void ClearRules()
    {
        Rules.Clear();
        SaveConfig();
        OnRulesChanged?.Invoke();
    }

    public void SaveConfig()
    {
        try
        {
            var dir = Path.GetDirectoryName(_configPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var model = new ConfigModel
            {
                Enabled = Enabled,
                AutoRefreshNearby = AutoRefreshNearby,
                TargetAllUniques = TargetAllUniques,
                TargetAllTitans = TargetAllTitans,
                TargetAllGiants = TargetAllGiants,
                TargetAllElites = TargetAllElites,
                Rules = Rules
            };

            var json = JsonSerializer.Serialize(model, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configPath, json);
        }
        catch (Exception ex)
        {
            Log.Warn($"Failed to save MobSelector config: {ex.Message}");
        }
    }

    public void LoadConfig()
    {
        try
        {
            if (!File.Exists(_configPath))
                return;

            var json = File.ReadAllText(_configPath);
            var model = JsonSerializer.Deserialize<ConfigModel>(json);

            if (model != null)
            {
                Enabled = model.Enabled;
                AutoRefreshNearby = model.AutoRefreshNearby;
                TargetAllUniques = model.TargetAllUniques;
                TargetAllTitans = model.TargetAllTitans;
                TargetAllGiants = model.TargetAllGiants;
                TargetAllElites = model.TargetAllElites;

                Rules.Clear();
                if (model.Rules != null)
                    Rules.AddRange(model.Rules);
            }
        }
        catch (Exception ex)
        {
            Log.Warn($"Failed to load MobSelector config: {ex.Message}");
        }
    }

    private class ConfigModel
    {
        public bool Enabled { get; set; }
        public bool AutoRefreshNearby { get; set; }
        public bool TargetAllUniques { get; set; }
        public bool TargetAllTitans { get; set; }
        public bool TargetAllGiants { get; set; }
        public bool TargetAllElites { get; set; }
        public List<MobSelectorRule> Rules { get; set; }
    }
}
