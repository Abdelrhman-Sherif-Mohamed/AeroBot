using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Spawn;
using SDUI.Controls;
using SDUI.Helpers;

namespace RSBot.MobSelector.Views;

[ToolboxItem(false)]
public partial class Main : DoubleBufferedControl
{
    private static Main _instance;
    public static Main Instance => _instance ??= new Main();

    private readonly Timer _nearbyRefreshTimer = new() { Interval = 1000 };

    public Main()
    {
        InitializeComponent();
        ControlSpacer.NormalizePage(this);

        MobSelectorManager.Instance.OnStateChanged += () => SafeInvoke(UpdateStateUI);
        MobSelectorManager.Instance.OnRulesChanged += () => SafeInvoke(UpdateRulesUI);

        _nearbyRefreshTimer.Tick += (s, e) =>
        {
            if (chkAutoRefreshNearby.Checked && Visible)
                RefreshNearbyMonsters();
        };
        _nearbyRefreshTimer.Start();
    }

    private void SafeInvoke(System.Action action)
    {
        if (IsHandleCreated && InvokeRequired)
            BeginInvoke(action);
        else
            action();
    }

    private void Main_Load(object sender, EventArgs e)
    {
        chkEnabled.Checked = MobSelectorManager.Instance.Enabled;
        chkAutoRefreshNearby.Checked = MobSelectorManager.Instance.AutoRefreshNearby;
        chkTargetUniques.Checked = MobSelectorManager.Instance.TargetAllUniques;
        chkTargetTitans.Checked = MobSelectorManager.Instance.TargetAllTitans;
        chkTargetGiants.Checked = MobSelectorManager.Instance.TargetAllGiants;
        chkTargetElites.Checked = MobSelectorManager.Instance.TargetAllElites;

        PopulateRarityComboBox();
        UpdateStateUI();
        UpdateRulesUI();
        RefreshNearbyMonsters();
    }

    private void PopulateRarityComboBox()
    {
        cboCustomMobRarity.Items.Clear();
        cboCustomMobRarity.Items.Add("Any Rarity");
        foreach (MonsterRarity rarity in Enum.GetValues(typeof(MonsterRarity)))
        {
            cboCustomMobRarity.Items.Add(rarity.ToString());
        }
        cboCustomMobRarity.SelectedIndex = 0;
    }

    private void UpdateStateUI()
    {
        lblStatus.Text = $"Status: {MobSelectorManager.Instance.StatusText}";
    }

    private void UpdateRulesUI()
    {
        lstTargetRules.BeginUpdate();
        lstTargetRules.Items.Clear();

        foreach (var rule in MobSelectorManager.Instance.Rules)
        {
            var lvi = new ListViewItem(string.IsNullOrEmpty(rule.Name) ? "(Any Name)" : rule.Name);
            var rarityText = rule.Rarity == 0xFF ? "Any Rarity" : ((MonsterRarity)rule.Rarity).ToString();
            lvi.SubItems.Add(rarityText);
            lvi.Tag = rule;
            lstTargetRules.Items.Add(lvi);
        }

        lstTargetRules.EndUpdate();
    }

    private void RefreshNearbyMonsters()
    {
        if (Game.Player == null)
            return;

        lstNearby.BeginUpdate();
        lstNearby.Items.Clear();

        if (SpawnManager.TryGetEntities<SpawnedMonster>(out var monsters) && monsters.Any())
        {
            var sorted = monsters
                .Where(m => m.Health > 0)
                .OrderBy(m => m.DistanceToPlayer)
                .ToList();

            foreach (var mob in sorted)
            {
                var lvi = new ListViewItem(mob.Record?.GetRealName() ?? "Unknown");
                lvi.SubItems.Add(mob.Rarity.ToString());
                lvi.SubItems.Add(mob.Record?.Level.ToString() ?? "0");
                lvi.SubItems.Add($"{Math.Round(mob.DistanceToPlayer, 1)}m");
                lvi.Tag = mob;

                lstNearby.Items.Add(lvi);
            }
        }

        lstNearby.EndUpdate();
    }

    private void chkEnabled_CheckedChanged(object sender, EventArgs e)
    {
        MobSelectorManager.Instance.Enabled = chkEnabled.Checked;
        MobSelectorManager.Instance.SaveConfig();
    }

    private void chkTargetUniques_CheckedChanged(object sender, EventArgs e)
    {
        MobSelectorManager.Instance.TargetAllUniques = chkTargetUniques.Checked;
        MobSelectorManager.Instance.SaveConfig();
    }

    private void chkTargetTitans_CheckedChanged(object sender, EventArgs e)
    {
        MobSelectorManager.Instance.TargetAllTitans = chkTargetTitans.Checked;
        MobSelectorManager.Instance.SaveConfig();
    }

    private void chkTargetGiants_CheckedChanged(object sender, EventArgs e)
    {
        MobSelectorManager.Instance.TargetAllGiants = chkTargetGiants.Checked;
        MobSelectorManager.Instance.SaveConfig();
    }

    private void chkTargetElites_CheckedChanged(object sender, EventArgs e)
    {
        MobSelectorManager.Instance.TargetAllElites = chkTargetElites.Checked;
        MobSelectorManager.Instance.SaveConfig();
    }

    private void chkAutoRefreshNearby_CheckedChanged(object sender, EventArgs e)
    {
        MobSelectorManager.Instance.AutoRefreshNearby = chkAutoRefreshNearby.Checked;
        MobSelectorManager.Instance.SaveConfig();
    }

    private void btnRefreshNearby_Click(object sender, EventArgs e)
    {
        RefreshNearbyMonsters();
    }

    private void btnAddSelectedMob_Click(object sender, EventArgs e)
    {
        if (lstNearby.SelectedItems.Count > 0)
        {
            if (lstNearby.SelectedItems[0].Tag is SpawnedMonster mob)
            {
                var mobName = mob.Record?.GetRealName();
                if (!string.IsNullOrEmpty(mobName))
                {
                    MobSelectorManager.Instance.AddRule(mobName, (byte)mob.Rarity);
                }
            }
        }
    }

    private void btnAddCustomRule_Click(object sender, EventArgs e)
    {
        var name = txtCustomMobName.Text.Trim();
        byte rarity = 0xFF;

        if (cboCustomMobRarity.SelectedIndex > 0)
        {
            var rarityName = cboCustomMobRarity.SelectedItem.ToString();
            if (Enum.TryParse<MonsterRarity>(rarityName, out var parsedRarity))
                rarity = (byte)parsedRarity;
        }

        if (!string.IsNullOrEmpty(name) || rarity != 0xFF)
        {
            MobSelectorManager.Instance.AddRule(name, rarity);
            txtCustomMobName.Text = string.Empty;
        }
    }

    private void btnRemoveRule_Click(object sender, EventArgs e)
    {
        if (lstTargetRules.SelectedIndices.Count > 0)
        {
            var idx = lstTargetRules.SelectedIndices[0];
            MobSelectorManager.Instance.RemoveRuleAt(idx);
        }
    }

    private void btnClearRules_Click(object sender, EventArgs e)
    {
        MobSelectorManager.Instance.ClearRules();
    }
}
