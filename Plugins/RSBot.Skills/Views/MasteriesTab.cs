using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Event;
using RSBot.Core.Objects.Skill;
using RSBot.Skills.Components;
using ContextMenuStrip = SDUI.Controls.ContextMenuStrip;

namespace RSBot.Skills.Views;

/// <summary>
///     Displays the player's masteries and allows leveling them up or enabling auto upgrade.
/// </summary>
internal class MasteriesTab : UserControl
{
    private readonly TreeView _tree;
    private readonly ContextMenuStrip _contextMenu;
    private readonly ToolStripMenuItem _levelMaxItem;
    private readonly ToolStripMenuItem _levelOneItem;
    private readonly ToolStripMenuItem _autoItem;
    private readonly Font _boldFont;

    public MasteriesTab()
    {
        BackColor = Color.White;

        _tree = new TreeView
        {
            BackColor = Color.White,
            BorderStyle = BorderStyle.None,
            Dock = DockStyle.Fill,
            FullRowSelect = true,
            HideSelection = false,
            ItemHeight = 22,
            ShowLines = true,
            ShowPlusMinus = true,
            ShowRootLines = true
        };
        _tree.NodeMouseClick += Tree_NodeMouseClick;

        _boldFont = new Font(_tree.Font, FontStyle.Bold);

        _levelMaxItem = new ToolStripMenuItem("Level Up Mastery (Max Level)");
        _levelMaxItem.Click += LevelMaxItem_Click;

        _levelOneItem = new ToolStripMenuItem("Level Up Mastery (1 Level)");
        _levelOneItem.Click += LevelOneItem_Click;

        _autoItem = new ToolStripMenuItem("Start Auto Upgrade (Mastery and Skills)");
        _autoItem.Click += AutoItem_Click;

        _contextMenu = new ContextMenuStrip();
        _contextMenu.Items.AddRange(new ToolStripItem[]
        {
            _levelMaxItem,
            _levelOneItem,
            new ToolStripSeparator(),
            _autoItem
        });
        _contextMenu.Opening += ContextMenu_Opening;

        _tree.ContextMenuStrip = _contextMenu;
        Controls.Add(_tree);

        var hint = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 30,
            Padding = new Padding(6, 0, 6, 0),
            Text = "Right-click a mastery to level it up or start auto upgrade.",
            TextAlign = ContentAlignment.MiddleLeft
        };
        Controls.Add(hint);

        EventManager.SubscribeEvent("OnLoadCharacter", new Action(OnRefreshRequested));
        EventManager.SubscribeEvent("OnExpSpUpdate", new Action(OnRefreshRequested));
        EventManager.SubscribeEvent("OnUpdateSP", new Action(OnRefreshRequested));
        EventManager.SubscribeEvent("OnLearnSkillMastery", new Action<MasteryInfo>(_ => OnRefreshRequested()));
        EventManager.SubscribeEvent("OnSkillLearned", new Action<SkillInfo>(_ => OnRefreshRequested()));
        EventManager.SubscribeEvent("OnSkillUpgraded", new Action<SkillInfo, SkillInfo>((oldSkill, newSkill) => OnRefreshRequested()));
        EventManager.SubscribeEvent("OnWithdrawSkill", new Action<SkillInfo, SkillInfo>((oldSkill, newSkill) => OnRefreshRequested()));

        SkillUpgradeManager.EnsureSettingsLoaded();
        RefreshTree();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _boldFont.Dispose();

        base.Dispose(disposing);
    }

    private void OnRefreshRequested()
    {
        if (IsDisposed || Disposing)
            return;

        if (InvokeRequired)
        {
            try
            {
                BeginInvoke(new Action(OnRefreshRequested));
            }
            catch
            {
                // The control is being disposed.
            }

            return;
        }

        RefreshTree();
    }

    private void RefreshTree()
    {
        var player = Game.Player;
        var selectedMasteryId = GetSelectedMasteryId();

        _tree.BeginUpdate();

        try
        {
            _tree.Nodes.Clear();

            if (player?.Skills?.Masteries == null)
                return;

            foreach (var mastery in player.Skills.Masteries)
            {
                if (mastery?.Record == null)
                    continue;

                var autoEnabled = SkillUpgradeManager.IsAutoEnabled(mastery.Id);
                var masteryName = string.IsNullOrWhiteSpace(mastery.Record.Name)
                    ? mastery.Record.NameCode
                    : mastery.Record.Name;

                var node = new TreeNode(
                    $"{masteryName} [lv. {mastery.Level}]{(autoEnabled ? "  [AUTO]" : string.Empty)}")
                {
                    Tag = mastery.Id,
                    ToolTipText = autoEnabled
                        ? "Auto upgrade is enabled"
                        : "Right-click to level up or start auto upgrade"
                };

                if (autoEnabled)
                    node.NodeFont = _boldFont;

                _tree.Nodes.Add(node);

                if (mastery.Id == selectedMasteryId)
                    _tree.SelectedNode = node;
            }
        }
        finally
        {
            _tree.EndUpdate();
        }
    }

    private uint GetSelectedMasteryId()
    {
        return _tree.SelectedNode?.Tag is uint masteryId ? masteryId : 0;
    }

    private void Tree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
    {
        if (e.Button != MouseButtons.Right)
            return;

        // Right-clicking empty space should not open the mastery menu.
        _tree.SelectedNode = e.Node;
    }

    private void ContextMenu_Opening(object sender, CancelEventArgs e)
    {
        var player = Game.Player;
        var masteryId = GetSelectedMasteryId();
        var mastery = masteryId == 0 ? null : player?.Skills?.GetMasteryInfoById(masteryId);

        if (mastery == null)
        {
            e.Cancel = true;
            return;
        }

        var canLevelUp = mastery.Level < player.Level;
        _levelMaxItem.Enabled = canLevelUp;
        _levelOneItem.Enabled = canLevelUp;
        _autoItem.Text = SkillUpgradeManager.IsAutoEnabled(masteryId)
            ? "Stop Auto Upgrade (Mastery and Skills)"
            : "Start Auto Upgrade (Mastery and Skills)";
    }

    private void LevelMaxItem_Click(object sender, EventArgs e)
    {
        var masteryId = GetSelectedMasteryId();
        if (masteryId != 0)
            SkillUpgradeManager.LearnMastery(masteryId, true);
    }

    private void LevelOneItem_Click(object sender, EventArgs e)
    {
        var masteryId = GetSelectedMasteryId();
        if (masteryId != 0)
            SkillUpgradeManager.LearnMastery(masteryId, false);
    }

    private void AutoItem_Click(object sender, EventArgs e)
    {
        var masteryId = GetSelectedMasteryId();
        if (masteryId == 0)
            return;

        SkillUpgradeManager.ToggleAuto(masteryId);
        RefreshTree();
    }
}
