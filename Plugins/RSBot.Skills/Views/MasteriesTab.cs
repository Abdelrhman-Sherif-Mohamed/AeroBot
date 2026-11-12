using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Event;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Skill;
using RSBot.Skills.Components;
using SDUI.Controls;
using Button = SDUI.Controls.Button;
using CheckBox = SDUI.Controls.CheckBox;
using ContextMenuStrip = SDUI.Controls.ContextMenuStrip;
using GroupBox = SDUI.Controls.GroupBox;
using Label = SDUI.Controls.Label;
using NumUpDown = SDUI.Controls.NumUpDown;
using Panel = SDUI.Controls.Panel;
using TextBox = System.Windows.Forms.TextBox;
using Action = System.Action;

namespace RSBot.Skills.Views;

/// <summary>
///     Standalone Mastery &amp; Skill tree tab allowing manual and automatic upgrades per mastery, skill group, or individual skill.
/// </summary>
[ToolboxItem(false)]
internal class MasteriesTab : UserControl
{
    private static MasteriesTab _instance;

    public static MasteriesTab Instance
    {
        get
        {
            if (_instance == null || _instance.IsDisposed || _instance.Disposing)
                _instance = new MasteriesTab();

            return _instance;
        }
    }

    private readonly Panel _topBar;
    private readonly Panel _contentHost;
    private readonly GroupBox _treeGroup;
    private readonly GroupBox _actionsGroup;
    private readonly TreeView _tree;
    private readonly ImageList _imageList;
    private readonly Dictionary<string, int> _iconIndices = new(StringComparer.OrdinalIgnoreCase);

    private readonly Label _lblStats;
    private readonly Label _lblGap;
    private readonly NumUpDown _numGap;
    private readonly CheckBox _chkBotStopped;
    private readonly TextBox _txtSearch;
    private readonly Button _btnExpandAll;
    private readonly Button _btnCollapseAll;
    private readonly Button _btnRefresh;

    private readonly PictureBox _picSelected;
    private readonly Label _lblSelectedTitle;
    private readonly Label _lblSelectedType;
    private readonly System.Windows.Forms.Label _lblDetails;
    private readonly Button _btnPrimaryMax;
    private readonly Button _btnPrimaryOne;
    private readonly Button _btnBatchMax;
    private readonly Button _btnBatchOne;
    private readonly CheckBox _chkAutoSelected;
    private readonly Label _lblHint;

    private readonly ContextMenuStrip _contextMenu;
    private readonly ToolStripMenuItem _mnuOne;
    private readonly ToolStripMenuItem _mnuMax;
    private readonly ToolStripMenuItem _mnuBatchOne;
    private readonly ToolStripMenuItem _mnuBatchMax;
    private readonly ToolStripSeparator _mnuSep;
    private readonly ToolStripMenuItem _mnuAuto;

    private readonly Font _boldFont;
    private bool _syncingControls;
    private bool _applyingTheme;
    private bool _lastIsDark;

    public MasteriesTab()
    {
        _imageList = new ImageList
        {
            ColorDepth = ColorDepth.Depth32Bit,
            ImageSize = new Size(20, 20)
        };
        EnsurePlaceholderIcon();

        _boldFont = new Font("Segoe UI", 9.25f, FontStyle.Bold);
        Font = new Font("Segoe UI", 9.25f, FontStyle.Regular);

        // Top bar with player SP, gap, bot-stopped toggle, search, and tree actions.
        _topBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 76,
            Padding = new Padding(10, 8, 10, 6),
            Radius = 0,
            ShadowDepth = 0
        };

        _lblStats = new Label
        {
            AutoSize = true,
            Location = new Point(12, 10),
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
        };

        _lblGap = new Label
        {
            AutoSize = true,
            Location = new Point(12, 43)
        };

        _numGap = new NumUpDown
        {
            Location = new Point(78, 39),
            Minimum = 0m,
            Maximum = 20m,
            MinimumSize = new Size(64, 24),
            Size = new Size(64, 24)
        };
        _numGap.ValueChanged += NumGap_ValueChanged;

        _chkBotStopped = new CheckBox
        {
            AutoSize = true,
            BackColor = Color.Transparent,
            Location = new Point(156, 36),
            Ripple = true
        };
        _chkBotStopped.CheckedChanged += ChkBotStopped_CheckedChanged;

        _txtSearch = new TextBox
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Size = new Size(185, 26),
            Location = new Point(425, 38)
        };
        _txtSearch.TextChanged += TxtSearch_TextChanged;

        _btnExpandAll = new Button
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Color = Color.Transparent,
            Radius = 6,
            ShadowDepth = 2f,
            Size = new Size(82, 26),
            Location = new Point(618, 38)
        };
        _btnExpandAll.Click += (_, _) => _tree.ExpandAll();

        _btnCollapseAll = new Button
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Color = Color.Transparent,
            Radius = 6,
            ShadowDepth = 2f,
            Size = new Size(82, 26),
            Location = new Point(706, 38)
        };
        _btnCollapseAll.Click += (_, _) => _tree.CollapseAll();

        _btnRefresh = new Button
        {
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Color = Color.Transparent,
            Radius = 6,
            ShadowDepth = 2f,
            Size = new Size(76, 26),
            Location = new Point(794, 38)
        };
        _btnRefresh.Click += (_, _) =>
        {
            SkillUpgradeManager.ResetBlockedSkills();
            RefreshTree();
        };

        _topBar.Size = new Size(880, 76);
        _topBar.Controls.AddRange(new Control[]
        {
            _lblStats, _lblGap, _numGap, _chkBotStopped,
            _txtSearch, _btnExpandAll, _btnCollapseAll, _btnRefresh
        });

        // Main content area: Tree on left/fill + Action card on right.
        _contentHost = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10, 4, 10, 10),
            Radius = 0,
            ShadowDepth = 0
        };

        _actionsGroup = new GroupBox
        {
            Dock = DockStyle.Right,
            Width = 285,
            Padding = new Padding(12, 24, 12, 12),
            Radius = 10,
            ShadowDepth = 4
        };

        _picSelected = new PictureBox
        {
            Location = new Point(14, 28),
            Size = new Size(34, 34),
            SizeMode = PictureBoxSizeMode.StretchImage,
            BackColor = Color.Transparent
        };

        _lblSelectedTitle = new Label
        {
            Location = new Point(56, 26),
            Size = new Size(216, 22),
            AutoEllipsis = true,
            Font = new Font("Segoe UI", 10f, FontStyle.Bold)
        };

        _lblSelectedType = new Label
        {
            Location = new Point(56, 48),
            Size = new Size(216, 18),
            AutoEllipsis = true
        };

        _lblDetails = new System.Windows.Forms.Label
        {
            Location = new Point(14, 74),
            Size = new Size(256, 108),
            BackColor = Color.Transparent
        };

        _btnPrimaryOne = new Button
        {
            Location = new Point(14, 190),
            Size = new Size(256, 32),
            Radius = 6,
            ShadowDepth = 3f,
            Color = Color.Transparent
        };
        _btnPrimaryOne.Click += BtnPrimaryOne_Click;

        _btnPrimaryMax = new Button
        {
            Location = new Point(14, 228),
            Size = new Size(256, 32),
            Radius = 6,
            ShadowDepth = 3f,
            Color = Color.Transparent
        };
        _btnPrimaryMax.Click += BtnPrimaryMax_Click;

        _btnBatchOne = new Button
        {
            Location = new Point(14, 266),
            Size = new Size(256, 32),
            Radius = 6,
            ShadowDepth = 3f,
            Color = Color.Transparent
        };
        _btnBatchOne.Click += BtnBatchOne_Click;

        _btnBatchMax = new Button
        {
            Location = new Point(14, 304),
            Size = new Size(256, 32),
            Radius = 6,
            ShadowDepth = 3f,
            Color = Color.Transparent
        };
        _btnBatchMax.Click += BtnBatchMax_Click;

        _chkAutoSelected = new CheckBox
        {
            AutoSize = true,
            BackColor = Color.Transparent,
            Location = new Point(14, 344),
            Ripple = true
        };
        _chkAutoSelected.CheckedChanged += ChkAutoSelected_CheckedChanged;

        _lblHint = new Label
        {
            Dock = DockStyle.Bottom,
            Height = 36,
            TextAlign = ContentAlignment.MiddleLeft
        };

        _actionsGroup.Controls.AddRange(new Control[]
        {
            _picSelected, _lblSelectedTitle, _lblSelectedType, _lblDetails,
            _btnPrimaryOne, _btnPrimaryMax, _btnBatchOne, _btnBatchMax,
            _chkAutoSelected, _lblHint
        });

        _treeGroup = new GroupBox
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(8, 22, 8, 8),
            Radius = 10,
            ShadowDepth = 4
        };

        _tree = new TreeView
        {
            BorderStyle = BorderStyle.None,
            Dock = DockStyle.Fill,
            FullRowSelect = true,
            HideSelection = false,
            ImageList = _imageList,
            ItemHeight = 26,
            ShowLines = true,
            ShowNodeToolTips = true,
            ShowPlusMinus = true,
            ShowRootLines = true
        };
        _tree.AfterSelect += (_, _) => UpdateSelectionPanel();
        _tree.NodeMouseClick += Tree_NodeMouseClick;
        _tree.NodeMouseDoubleClick += Tree_NodeMouseDoubleClick;

        _mnuOne = new ToolStripMenuItem();
        _mnuOne.Click += BtnPrimaryOne_Click;

        _mnuMax = new ToolStripMenuItem();
        _mnuMax.Click += BtnPrimaryMax_Click;

        _mnuBatchOne = new ToolStripMenuItem();
        _mnuBatchOne.Click += BtnBatchOne_Click;

        _mnuBatchMax = new ToolStripMenuItem();
        _mnuBatchMax.Click += BtnBatchMax_Click;

        _mnuSep = new ToolStripSeparator();

        _mnuAuto = new ToolStripMenuItem();
        _mnuAuto.Click += MnuAuto_Click;

        _contextMenu = new ContextMenuStrip();
        _contextMenu.Items.AddRange(new ToolStripItem[]
        {
            _mnuOne, _mnuMax, _mnuBatchOne, _mnuBatchMax, _mnuSep, _mnuAuto
        });
        _contextMenu.Opening += ContextMenu_Opening;
        _tree.ContextMenuStrip = _contextMenu;

        _treeGroup.Controls.Add(_tree);

        _contentHost.Controls.Add(_treeGroup);
        _contentHost.Controls.Add(_actionsGroup);

        Controls.Add(_contentHost);
        Controls.Add(_topBar);

        EventManager.SubscribeEvent("OnLoadCharacter", new Action(OnCharacterLoaded));
        EventManager.SubscribeEvent("OnExpSpUpdate", new Action(OnRefreshRequested));
        EventManager.SubscribeEvent("OnUpdateSP", new Action(OnRefreshRequested));
        EventManager.SubscribeEvent("OnLearnSkillMastery", new Action<MasteryInfo>(_ => OnRefreshRequested()));
        EventManager.SubscribeEvent("OnSkillLearned", new Action<SkillInfo>(_ => OnRefreshRequested()));
        EventManager.SubscribeEvent("OnSkillUpgraded", new Action<SkillInfo, SkillInfo>((_, _) => OnRefreshRequested()));
        EventManager.SubscribeEvent("OnWithdrawSkill", new Action<SkillInfo, SkillInfo>((_, _) => OnRefreshRequested()));
        SkillUpgradeManager.BatchCompleted += OnRefreshRequested;

        SkillUpgradeManager.EnsureSettingsLoaded();
        SyncTopSettings();
        ApplyLanguage();
        ApplyTheme();
        RefreshTree();
    }

    private static bool IsArabic => string.Equals(Kernel.Language, "ar_AR", StringComparison.OrdinalIgnoreCase);

    public void ApplyLanguage()
    {
        if (IsDisposed || Disposing)
            return;

        if (InvokeRequired)
        {
            BeginInvoke(new Action(ApplyLanguage));
            return;
        }

        var ar = IsArabic;
        _treeGroup.Text = ar ? "شجرة الماستري والمجموعة والسكلات" : "Masteries & Skills Tree";
        _actionsGroup.Text = ar ? "التحكم والترقية" : "Upgrade Controls";
        _lblGap.Text = ar ? "الفرق:" : "Gap:";
        _chkBotStopped.Text = ar ? "تلقائي عند إيقاف البوت" : "Auto when bot stopped";
        _txtSearch.PlaceholderText = ar ? "بحث عن اسكل أو ماستري..." : "Search skill or mastery...";
        _btnExpandAll.Text = ar ? "توسيع الكل" : "Expand All";
        _btnCollapseAll.Text = ar ? "طي الكل" : "Collapse All";
        _btnRefresh.Text = ar ? "تحديث" : "Refresh";
        _lblHint.Text = ar
            ? "دبل كليك لفتح/ترقية الاسكل أو كليك يمين للخيارات."
            : "Double-click a skill to upgrade (+1), or right-click any item.";

        UpdateStatsLabel();
        UpdateSelectionPanel();
    }

    public void ApplyTheme()
    {
        if (_applyingTheme || IsDisposed || Disposing)
            return;

        _applyingTheme = true;
        try
        {
            var isDark = SDUI.ColorScheme.BackColor.IsDark();
            var themeChanged = isDark != _lastIsDark;
            _lastIsDark = isDark;

            var bg = SDUI.ColorScheme.BackColor;
            var cardBg = isDark ? SDUI.ColorScheme.CardColor : Color.White;
            var fg = isDark ? SDUI.ColorScheme.ForeColor : Color.FromArgb(30, 34, 42);
            var secondaryFg = isDark ? SDUI.ColorScheme.SecondaryForeColor : Color.FromArgb(100, 108, 120);

            BackColor = bg;
            _topBar.BackColor = bg;
            _contentHost.BackColor = bg;

            _tree.BackColor = cardBg;
            _tree.ForeColor = fg;

            _lblStats.ForeColor = fg;
            _lblGap.ForeColor = fg;
            _chkBotStopped.ForeColor = fg;
            _lblSelectedTitle.ForeColor = fg;
            _lblSelectedType.ForeColor = secondaryFg;
            _lblDetails.ForeColor = fg;
            _chkAutoSelected.ForeColor = fg;
            _lblHint.ForeColor = secondaryFg;

            _txtSearch.BackColor = cardBg;
            _txtSearch.ForeColor = fg;

            if (themeChanged)
                RefreshTree();
            else
                UpdateSelectionPanel();
        }
        finally
        {
            _applyingTheme = false;
        }
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);
        if (Visible)
        {
            ApplyLanguage();
            ApplyTheme();
            RefreshTree();
        }
    }

    protected override void OnBackColorChanged(EventArgs e)
    {
        base.OnBackColorChanged(e);
        if (!_applyingTheme && IsHandleCreated)
            ApplyTheme();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            SkillUpgradeManager.BatchCompleted -= OnRefreshRequested;
            _boldFont?.Dispose();
            _imageList?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void OnCharacterLoaded()
    {
        SkillUpgradeManager.LoadSettings();
        if (IsDisposed || Disposing)
            return;

        if (InvokeRequired)
        {
            BeginInvoke(new Action(() =>
            {
                SyncTopSettings();
                RefreshTree();
            }));
            return;
        }

        SyncTopSettings();
        RefreshTree();
    }

    private void SyncTopSettings()
    {
        _syncingControls = true;
        try
        {
            SkillUpgradeManager.EnsureSettingsLoaded();
            _chkBotStopped.Checked = SkillUpgradeManager.AutoWhenBotStopped;
            var gap = Math.Min((decimal)_numGap.Maximum, Math.Max((decimal)_numGap.Minimum, SkillUpgradeManager.AutoMasteryGap));
            _numGap.Value = gap;
        }
        finally
        {
            _syncingControls = false;
        }
    }

    private void NumGap_ValueChanged(object sender, EventArgs e)
    {
        if (_syncingControls)
            return;

        SkillUpgradeManager.AutoMasteryGap = (byte)_numGap.Value;
    }

    private void ChkBotStopped_CheckedChanged(object sender, EventArgs e)
    {
        if (_syncingControls)
            return;

        SkillUpgradeManager.AutoWhenBotStopped = _chkBotStopped.Checked;
    }

    private void TxtSearch_TextChanged(object sender, EventArgs e)
    {
        RefreshTree();
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
            }

            return;
        }

        RefreshTree();
    }

    private void UpdateStatsLabel()
    {
        var ar = IsArabic;
        var player = Game.Player;
        if (player == null)
        {
            _lblStats.Text = ar ? "يرجى تسجيل الدخول بالشخصية لعرض الماستري والسكلات." : "Log in with a character to view masteries and skills.";
            return;
        }

        _lblStats.Text = ar
            ? $"اللفل: {player.Level}   |   نقاط المهارة (SP): {player.SkillPoints:N0}"
            : $"Level: {player.Level}   |   Skill Points (SP): {player.SkillPoints:N0}";
    }

    private void RefreshTree()
    {
        if (IsDisposed || Disposing)
            return;

        UpdateStatsLabel();

        var player = Game.Player;
        var selectedKey = GetNodeKey(_tree.SelectedNode?.Tag);
        var expandedKeys = CollectExpandedKeys(_tree.Nodes);
        var filter = _txtSearch.Text?.Trim();
        var hasFilter = !string.IsNullOrEmpty(filter);

        var isDark = SDUI.ColorScheme.BackColor.IsDark();
        var defaultFore = isDark ? SDUI.ColorScheme.ForeColor : Color.FromArgb(30, 34, 42);
        var groupFore = isDark ? Color.FromArgb(245, 195, 110) : Color.FromArgb(168, 46, 30);
        var availableFore = isDark ? Color.FromArgb(95, 220, 175) : Color.FromArgb(16, 132, 92);
        var maxedFore = isDark ? Color.FromArgb(145, 175, 225) : Color.FromArgb(40, 95, 165);
        var lockedFore = isDark ? Color.FromArgb(130, 138, 152) : Color.FromArgb(135, 142, 152);

        TreeNode nodeToSelect = null;

        _tree.BeginUpdate();
        try
        {
            _tree.Nodes.Clear();

            if (player?.Skills?.Masteries == null)
            {
                UpdateSelectionPanel();
                return;
            }

            foreach (var mastery in player.Skills.Masteries)
            {
                if (mastery?.Record == null)
                    continue;

                var masteryName = string.IsNullOrWhiteSpace(mastery.Record.Name)
                    ? mastery.Record.NameCode
                    : mastery.Record.Name;

                var masteryAuto = SkillUpgradeManager.IsAutoEnabled(mastery.Id);
                var lines = SkillUpgradeManager.GetSkillLinesForMastery(mastery.Id, player);
                var groups = BuildSkillGroups(lines);

                var masteryMatchesFilter = hasFilter &&
                                           masteryName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;

                var groupNodes = new List<TreeNode>();
                foreach (var group in groups)
                {
                    var groupMatchesFilter = hasFilter &&
                                             group.Title.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;

                    var skillNodes = new List<TreeNode>();
                    var learnedInGroup = 0;

                    foreach (var line in group.Lines)
                    {
                        var (knownLevel, knownRecord) = SkillUpgradeManager.GetKnownSkillState(line.CodePrefix, player);
                        if (knownLevel > 0)
                            learnedInGroup++;

                        var skillName = (knownRecord ?? line.FirstSkill).GetRealName();
                        if (hasFilter && !masteryMatchesFilter && !groupMatchesFilter &&
                            skillName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)
                            continue;

                        var maxLevel = line.MaxSkill.Basic_Level;
                        var nextSkill = SkillUpgradeManager.GetNextLevelForLine(line, player);
                        var canLearnNext = nextSkill != null && SkillUpgradeManager.CanLearnNextLevel(nextSkill, player);
                        var skillAuto = SkillUpgradeManager.IsSkillAutoEnabled(line.CodePrefix);
                        var effectiveAuto = masteryAuto || skillAuto;

                        var skillLabel = knownLevel >= maxLevel
                            ? $"{skillName} [Lv {knownLevel}/{maxLevel}]"
                            : $"{skillName} [Lv {knownLevel}/{maxLevel}]";
                        if (skillAuto)
                            skillLabel += "  [AUTO]";
                        else if (masteryAuto)
                            skillLabel += "  [M-AUTO]";

                        var skillIconIndex = EnsureImageIndex($"skill:{line.CodePrefix}", () => (knownRecord ?? line.FirstSkill).GetIcon());
                        var skillTag = new SkillNodeData(mastery.Id, line);
                        var skillNode = new TreeNode(skillLabel)
                        {
                            Tag = skillTag,
                            ImageIndex = skillIconIndex,
                            SelectedImageIndex = skillIconIndex
                        };

                        if (knownLevel >= maxLevel)
                            skillNode.ForeColor = maxedFore;
                        else if (canLearnNext)
                            skillNode.ForeColor = availableFore;
                        else if (knownLevel > 0)
                            skillNode.ForeColor = defaultFore;
                        else
                            skillNode.ForeColor = lockedFore;

                        if (effectiveAuto)
                            skillNode.NodeFont = _boldFont;

                        if (nextSkill != null)
                        {
                            skillNode.ToolTipText = IsArabic
                                ? $"التالي Lv.{nextSkill.Basic_Level} | SP: {nextSkill.ReqLearn_SP:N0} | ماستري: {nextSkill.ReqCommon_MasteryLevel1}"
                                : $"Next Lv.{nextSkill.Basic_Level} | SP: {nextSkill.ReqLearn_SP:N0} | Mastery Lv: {nextSkill.ReqCommon_MasteryLevel1}";
                        }

                        if (selectedKey != null && GetNodeKey(skillTag) == selectedKey)
                            nodeToSelect = skillNode;

                        skillNodes.Add(skillNode);
                    }

                    if (hasFilter && !masteryMatchesFilter && !groupMatchesFilter && skillNodes.Count == 0)
                        continue;

                    var groupTag = new GroupNodeData(mastery.Id, group.Key, group.Title, group.Lines);
                    var groupIconIndex = EnsureImageIndex($"skill:{group.IconLine.CodePrefix}", () => group.IconLine.FirstSkill.GetIcon());
                    var groupNode = new TreeNode($"{group.Title} ({learnedInGroup}/{group.Lines.Count})")
                    {
                        Tag = groupTag,
                        ImageIndex = groupIconIndex,
                        SelectedImageIndex = groupIconIndex,
                        ForeColor = groupFore
                    };

                    foreach (var sn in skillNodes)
                        groupNode.Nodes.Add(sn);

                    if (hasFilter || expandedKeys.Contains(GetNodeKey(groupTag)))
                        groupNode.Expand();

                    if (selectedKey != null && GetNodeKey(groupTag) == selectedKey)
                        nodeToSelect = groupNode;

                    groupNodes.Add(groupNode);
                }

                if (hasFilter && !masteryMatchesFilter && groupNodes.Count == 0)
                    continue;

                var masteryTag = new MasteryNodeData(mastery.Id, masteryName, lines);
                var masteryIconIndex = EnsureImageIndex($"mastery:{mastery.Id}", () => mastery.Record.GetIcon());
                var masteryText = $"{masteryName} [Lv {mastery.Level}]{(masteryAuto ? "  [AUTO]" : string.Empty)}";
                var masteryNode = new TreeNode(masteryText)
                {
                    Tag = masteryTag,
                    ImageIndex = masteryIconIndex,
                    SelectedImageIndex = masteryIconIndex,
                    ForeColor = defaultFore
                };

                if (masteryAuto)
                    masteryNode.NodeFont = _boldFont;

                foreach (var gn in groupNodes)
                    masteryNode.Nodes.Add(gn);

                if (hasFilter || expandedKeys.Contains(GetNodeKey(masteryTag)))
                    masteryNode.Expand();

                if (selectedKey != null && GetNodeKey(masteryTag) == selectedKey)
                    nodeToSelect = masteryNode;

                _tree.Nodes.Add(masteryNode);
            }

            if (nodeToSelect == null && _tree.Nodes.Count > 0)
                nodeToSelect = _tree.Nodes[0];

            if (nodeToSelect != null)
                _tree.SelectedNode = nodeToSelect;
        }
        finally
        {
            _tree.EndUpdate();
        }

        UpdateSelectionPanel();
    }

    private void UpdateSelectionPanel()
    {
        var ar = IsArabic;
        var player = Game.Player;
        var tag = _tree.SelectedNode?.Tag;

        _btnPrimaryOne.Visible = false;
        _btnPrimaryMax.Visible = false;
        _btnBatchOne.Visible = false;
        _btnBatchMax.Visible = false;
        _chkAutoSelected.Visible = false;

        if (player?.Skills == null || tag == null)
        {
            _picSelected.Image = null;
            _lblSelectedTitle.Text = ar ? "لم يتم اختيار عنصر" : "Nothing selected";
            _lblSelectedType.Text = "-";
            _lblDetails.Text = ar ? "اختر ماستري أو مجموعة أو اسكل من الشجرة للتحكم في الفتح والترقية." : "Select a mastery, skill group, or skill from the tree.";
            return;
        }

        if (tag is MasteryNodeData masteryNode)
        {
            var mastery = player.Skills.GetMasteryInfoById(masteryNode.MasteryId);
            var level = mastery?.Level ?? 0;
            var canLevelMastery = mastery != null && level < player.Level;
            var auto = SkillUpgradeManager.IsAutoEnabled(masteryNode.MasteryId);
            var nextLevelInfo = Game.ReferenceManager.GetRefLevel((byte)(level + 1));
            var learnedCount = masteryNode.Lines.Count(l => SkillUpgradeManager.GetKnownSkillState(l.CodePrefix, player).Level > 0);
            var nextSp = nextLevelInfo?.Exp_M ?? 0;

            _picSelected.Image = mastery?.Record?.GetIcon();
            _lblSelectedTitle.Text = $"{masteryNode.Name} [Lv {level}]";
            _lblSelectedType.Text = ar ? "ماستري (Mastery)" : "Mastery";
            _lblDetails.Text = ar
                ? $"المستوى الحالي: {level} / {player.Level}\nSP للمستوى التالي: {nextSp:N0}\nالسكلات المفتوحة: {learnedCount} / {masteryNode.Lines.Count}\nالتطوير التلقائي: {(auto ? "مفعّل" : "متوقف")}"
                : $"Current Level: {level} / {player.Level}\nNext Level SP: {nextSp:N0}\nLearned Skills: {learnedCount} / {masteryNode.Lines.Count}\nAuto Upgrade: {(auto ? "Enabled" : "Disabled")}";

            _btnPrimaryOne.Visible = true;
            _btnPrimaryOne.Enabled = canLevelMastery;
            _btnPrimaryOne.Text = ar ? "+1 مستوى ماستري" : "Level Up Mastery (+1)";

            _btnPrimaryMax.Visible = true;
            _btnPrimaryMax.Enabled = canLevelMastery;
            _btnPrimaryMax.Text = ar ? "ماكس ماستري (Max)" : "Max Mastery";

            _btnBatchOne.Visible = masteryNode.Lines.Count > 0;
            _btnBatchOne.Enabled = masteryNode.Lines.Count > 0;
            _btnBatchOne.Text = ar ? "فتح / ترقية كل السكلات (+1)" : "Learn All Skills (+1)";

            _btnBatchMax.Visible = masteryNode.Lines.Count > 0;
            _btnBatchMax.Enabled = masteryNode.Lines.Count > 0;
            _btnBatchMax.Text = ar ? "فتح / ماكس كل السكلات" : "Learn / Max All Skills";

            _chkAutoSelected.Visible = true;
            _syncingControls = true;
            _chkAutoSelected.Checked = auto;
            _syncingControls = false;
            _chkAutoSelected.Text = ar ? "تطوير تلقائي للماستري والسكلات" : "Auto Upgrade Mastery & Skills";
            return;
        }

        if (tag is GroupNodeData groupNode)
        {
            var learnedCount = groupNode.Lines.Count(l => SkillUpgradeManager.GetKnownSkillState(l.CodePrefix, player).Level > 0);
            var availableCount = groupNode.Lines.Count(l =>
            {
                var next = SkillUpgradeManager.GetNextLevelForLine(l, player);
                return next != null && SkillUpgradeManager.CanLearnNextLevel(next, player);
            });
            var allAuto = groupNode.Lines.Count > 0 && groupNode.Lines.All(l => SkillUpgradeManager.IsSkillAutoEnabled(l.CodePrefix));

            _picSelected.Image = groupNode.Lines.FirstOrDefault()?.FirstSkill.GetIcon();
            _lblSelectedTitle.Text = groupNode.Title;
            _lblSelectedType.Text = ar ? "مجموعة سكلات" : "Skill Group";
            _lblDetails.Text = ar
                ? $"عدد السكلات: {groupNode.Lines.Count}\nالسكلات المفتوحة: {learnedCount}\nمتاح للفتح/الترقية الآن: {availableCount}\nالتطوير التلقائي للمجموعة: {(allAuto ? "مفعّل" : "متوقف")}"
                : $"Total Skills: {groupNode.Lines.Count}\nLearned Skills: {learnedCount}\nReady to Upgrade Now: {availableCount}\nGroup Auto Upgrade: {(allAuto ? "Enabled" : "Disabled")}";

            _btnBatchOne.Visible = true;
            _btnBatchOne.Enabled = groupNode.Lines.Count > 0;
            _btnBatchOne.Text = ar ? "فتح / ترقية المجموعة (+1)" : "Learn Group Skills (+1)";

            _btnBatchMax.Visible = true;
            _btnBatchMax.Enabled = groupNode.Lines.Count > 0;
            _btnBatchMax.Text = ar ? "فتح / ماكس سكلات المجموعة" : "Learn / Max Group Skills";

            _chkAutoSelected.Visible = true;
            _syncingControls = true;
            _chkAutoSelected.Checked = allAuto;
            _syncingControls = false;
            _chkAutoSelected.Text = ar ? "تطوير تلقائي لكل سكلات المجموعة" : "Auto Upgrade Group Skills";
            return;
        }

        if (tag is SkillNodeData skillNode)
        {
            var line = skillNode.Line;
            var (knownLevel, knownRecord) = SkillUpgradeManager.GetKnownSkillState(line.CodePrefix, player);
            var maxLevel = line.MaxSkill.Basic_Level;
            var nextSkill = SkillUpgradeManager.GetNextLevelForLine(line, player);
            var canLearn = nextSkill != null && SkillUpgradeManager.CanLearnNextLevel(nextSkill, player);
            var skillAuto = SkillUpgradeManager.IsSkillAutoEnabled(line.CodePrefix);
            var masteryAuto = SkillUpgradeManager.IsAutoEnabled(skillNode.MasteryId);

            _picSelected.Image = (knownRecord ?? line.FirstSkill).GetIcon();
            _lblSelectedTitle.Text = $"{(knownRecord ?? line.FirstSkill).GetRealName()} [{knownLevel}/{maxLevel}]";
            _lblSelectedType.Text = ar ? "سكل فردي" : "Skill";

            if (nextSkill == null)
            {
                _lblDetails.Text = ar
                    ? $"المستوى: {knownLevel} / {maxLevel} (مكتمل)\nالتطوير التلقائي: {(skillAuto || masteryAuto ? "مفعّل" : "متوقف")}"
                    : $"Level: {knownLevel} / {maxLevel} (Maxed)\nAuto Upgrade: {(skillAuto || masteryAuto ? "Enabled" : "Disabled")}";
            }
            else
            {
                var status = canLearn
                    ? (ar ? "جاهز للفتح / الترقية" : "Ready to learn/upgrade")
                    : GetBlockedReason(nextSkill, player, ar);

                _lblDetails.Text = ar
                    ? $"المستوى الحالي: {knownLevel} / {maxLevel}\nاللفل التالي: {nextSkill.Basic_Level} | SP: {nextSkill.ReqLearn_SP:N0}\nالماستري المطلوب: {nextSkill.ReqCommon_MasteryLevel1}\nالحالة: {status}"
                    : $"Current Level: {knownLevel} / {maxLevel}\nNext Lv: {nextSkill.Basic_Level} | SP: {nextSkill.ReqLearn_SP:N0}\nReq Mastery Lv: {nextSkill.ReqCommon_MasteryLevel1}\nStatus: {status}";
            }

            _btnPrimaryOne.Visible = true;
            _btnPrimaryOne.Enabled = canLearn;
            _btnPrimaryOne.Text = knownLevel == 0
                ? (ar ? "فتح هذا السكل (Lv 1)" : "Open Skill (Lv 1)")
                : (ar ? "ترقية السكل (+1 لفل)" : "Upgrade Skill (+1)");

            _btnPrimaryMax.Visible = true;
            _btnPrimaryMax.Enabled = canLearn;
            _btnPrimaryMax.Text = ar ? "ماكس السكل لأقصى متاح" : "Max Skill (Up to Cap)";

            _chkAutoSelected.Visible = true;
            _syncingControls = true;
            _chkAutoSelected.Checked = skillAuto;
            _syncingControls = false;
            _chkAutoSelected.Text = masteryAuto && !skillAuto
                ? (ar ? "تطوير تلقائي لهذا السكل (مفعّل بالماستري)" : "Auto Upgrade Skill (Mastery Auto On)")
                : (ar ? "تطوير تلقائي لهذا السكل" : "Auto Upgrade This Skill");
        }
    }

    private static string GetBlockedReason(RefSkill skill, Player player, bool ar)
    {
        if (skill.Basic_Level > player.Level)
            return ar ? $"يتطلب لفل شخصية {skill.Basic_Level}" : $"Requires Char Lv {skill.Basic_Level}";

        if (skill.ReqCommon_Mastery1 != 0 && skill.ReqCommon_Mastery1 != 1000)
        {
            var mastery = player.Skills.GetMasteryInfoById((uint)skill.ReqCommon_Mastery1);
            if (mastery == null || mastery.Level < skill.ReqCommon_MasteryLevel1)
                return ar ? $"يتطلب ماستري لفل {skill.ReqCommon_MasteryLevel1}" : $"Requires Mastery Lv {skill.ReqCommon_MasteryLevel1}";
        }

        if ((uint)skill.ReqLearn_SP > player.SkillPoints)
            return ar ? $"SP غير كافي (مطلوب {skill.ReqLearn_SP:N0})" : $"Not enough SP (Need {skill.ReqLearn_SP:N0})";

        return ar ? "متطلبات غير مكتملة" : "Prerequisites not met";
    }

    private void BtnPrimaryOne_Click(object sender, EventArgs e)
    {
        var tag = _tree.SelectedNode?.Tag;
        if (tag is MasteryNodeData masteryNode)
            SkillUpgradeManager.LearnMastery(masteryNode.MasteryId, false);
        else if (tag is SkillNodeData skillNode)
            SkillUpgradeManager.LearnSkillLine(skillNode.MasteryId, skillNode.Line.CodePrefix, false);
    }

    private void BtnPrimaryMax_Click(object sender, EventArgs e)
    {
        var tag = _tree.SelectedNode?.Tag;
        if (tag is MasteryNodeData masteryNode)
            SkillUpgradeManager.LearnMastery(masteryNode.MasteryId, true);
        else if (tag is SkillNodeData skillNode)
            SkillUpgradeManager.LearnSkillLine(skillNode.MasteryId, skillNode.Line.CodePrefix, true);
    }

    private void BtnBatchOne_Click(object sender, EventArgs e)
    {
        var tag = _tree.SelectedNode?.Tag;
        if (tag is MasteryNodeData masteryNode)
            SkillUpgradeManager.LearnSkillsBatch(masteryNode.MasteryId, null, false);
        else if (tag is GroupNodeData groupNode)
            SkillUpgradeManager.LearnSkillsBatch(groupNode.MasteryId, groupNode.Lines.Select(l => l.CodePrefix), false);
    }

    private void BtnBatchMax_Click(object sender, EventArgs e)
    {
        var tag = _tree.SelectedNode?.Tag;
        if (tag is MasteryNodeData masteryNode)
            SkillUpgradeManager.LearnSkillsBatch(masteryNode.MasteryId, null, true);
        else if (tag is GroupNodeData groupNode)
            SkillUpgradeManager.LearnSkillsBatch(groupNode.MasteryId, groupNode.Lines.Select(l => l.CodePrefix), true);
    }

    private void ChkAutoSelected_CheckedChanged(object sender, EventArgs e)
    {
        if (_syncingControls)
            return;

        ApplyAutoToggle(_chkAutoSelected.Checked);
    }

    private void MnuAuto_Click(object sender, EventArgs e)
    {
        var tag = _tree.SelectedNode?.Tag;
        if (tag is MasteryNodeData masteryNode)
            ApplyAutoToggle(!SkillUpgradeManager.IsAutoEnabled(masteryNode.MasteryId));
        else if (tag is GroupNodeData groupNode)
            ApplyAutoToggle(!groupNode.Lines.All(l => SkillUpgradeManager.IsSkillAutoEnabled(l.CodePrefix)));
        else if (tag is SkillNodeData skillNode)
            ApplyAutoToggle(!SkillUpgradeManager.IsSkillAutoEnabled(skillNode.Line.CodePrefix));
    }

    private void ApplyAutoToggle(bool enabled)
    {
        var tag = _tree.SelectedNode?.Tag;
        if (tag is MasteryNodeData masteryNode)
            SkillUpgradeManager.SetAutoEnabled(masteryNode.MasteryId, enabled);
        else if (tag is GroupNodeData groupNode)
            SkillUpgradeManager.SetSkillsAutoEnabled(groupNode.Lines.Select(l => l.CodePrefix), enabled);
        else if (tag is SkillNodeData skillNode)
            SkillUpgradeManager.SetSkillAutoEnabled(skillNode.Line.CodePrefix, enabled);

        RefreshTree();
    }

    private void Tree_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
    {
        if (e.Button == MouseButtons.Right && e.Node != null)
            _tree.SelectedNode = e.Node;
    }

    private void Tree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
    {
        if (e.Node?.Tag is SkillNodeData skillNode)
            SkillUpgradeManager.LearnSkillLine(skillNode.MasteryId, skillNode.Line.CodePrefix, false);
    }

    private void ContextMenu_Opening(object sender, CancelEventArgs e)
    {
        var tag = _tree.SelectedNode?.Tag;
        var player = Game.Player;
        if (tag == null || player?.Skills == null)
        {
            e.Cancel = true;
            return;
        }

        var ar = IsArabic;
        _mnuOne.Visible = false;
        _mnuMax.Visible = false;
        _mnuBatchOne.Visible = false;
        _mnuBatchMax.Visible = false;
        _mnuAuto.Visible = true;

        if (tag is MasteryNodeData masteryNode)
        {
            var mastery = player.Skills.GetMasteryInfoById(masteryNode.MasteryId);
            var canLevel = mastery != null && mastery.Level < player.Level;
            var auto = SkillUpgradeManager.IsAutoEnabled(masteryNode.MasteryId);

            _mnuOne.Visible = true;
            _mnuOne.Enabled = canLevel;
            _mnuOne.Text = ar ? "+1 مستوى ماستري" : "Level Up Mastery (+1)";

            _mnuMax.Visible = true;
            _mnuMax.Enabled = canLevel;
            _mnuMax.Text = ar ? "ماكس ماستري" : "Max Mastery";

            _mnuBatchOne.Visible = true;
            _mnuBatchOne.Text = ar ? "فتح كل السكلات (+1)" : "Learn All Skills (+1)";

            _mnuBatchMax.Visible = true;
            _mnuBatchMax.Text = ar ? "فتح / ماكس كل السكلات" : "Learn / Max All Skills";

            _mnuAuto.Text = auto
                ? (ar ? "إيقاف التطوير التلقائي للماستري" : "Stop Auto Upgrade (Mastery)")
                : (ar ? "تفعيل التطوير التلقائي للماستري" : "Start Auto Upgrade (Mastery)");
        }
        else if (tag is GroupNodeData groupNode)
        {
            var allAuto = groupNode.Lines.Count > 0 && groupNode.Lines.All(l => SkillUpgradeManager.IsSkillAutoEnabled(l.CodePrefix));

            _mnuBatchOne.Visible = true;
            _mnuBatchOne.Text = ar ? "فتح سكلات المجموعة (+1)" : "Learn Group Skills (+1)";

            _mnuBatchMax.Visible = true;
            _mnuBatchMax.Text = ar ? "فتح / ماكس سكلات المجموعة" : "Learn / Max Group Skills";

            _mnuAuto.Text = allAuto
                ? (ar ? "إيقاف التلقائي للمجموعة" : "Stop Auto Upgrade (Group)")
                : (ar ? "تفعيل التلقائي للمجموعة" : "Start Auto Upgrade (Group)");
        }
        else if (tag is SkillNodeData skillNode)
        {
            var next = SkillUpgradeManager.GetNextLevelForLine(skillNode.Line, player);
            var canLearn = next != null && SkillUpgradeManager.CanLearnNextLevel(next, player);
            var auto = SkillUpgradeManager.IsSkillAutoEnabled(skillNode.Line.CodePrefix);

            _mnuOne.Visible = true;
            _mnuOne.Enabled = canLearn;
            _mnuOne.Text = ar ? "فتح / ترقية السكل (+1)" : "Learn / Upgrade Skill (+1)";

            _mnuMax.Visible = true;
            _mnuMax.Enabled = canLearn;
            _mnuMax.Text = ar ? "ماكس السكل" : "Max Skill";

            _mnuAuto.Text = auto
                ? (ar ? "إيقاف التلقائي للسكل" : "Stop Auto Upgrade (Skill)")
                : (ar ? "تفعيل التلقائي للسكل" : "Start Auto Upgrade (Skill)");
        }
    }

    private void EnsurePlaceholderIcon()
    {
        if (!_iconIndices.ContainsKey("__default"))
        {
            _imageList.Images.Add(new Bitmap(20, 20));
            _iconIndices["__default"] = 0;
        }
    }

    private int EnsureImageIndex(string key, Func<Image> loader)
    {
        if (_iconIndices.TryGetValue(key, out var existing))
            return existing;

        try
        {
            using var image = loader?.Invoke();
            if (image != null)
            {
                var idx = _imageList.Images.Count;
                _imageList.Images.Add(image);
                _iconIndices[key] = idx;
                return idx;
            }
        }
        catch
        {
        }

        return 0;
    }

    private static List<SkillGroupInfo> BuildSkillGroups(IReadOnlyList<SkillLineInfo> lines)
    {
        var result = new List<SkillGroupInfo>();
        if (lines == null || lines.Count == 0)
            return result;

        var hasUiPages = lines.Any(l => l.UIPage > 0);
        if (hasUiPages)
        {
            foreach (var grp in lines.GroupBy(l => l.UIPage == 0 ? byte.MaxValue : l.UIPage)
                         .OrderBy(g => g.Key))
            {
                var groupLines = grp.ToList();
                var title = ResolveGroupTitle(groupLines);
                var rep = PickRepresentativeLine(groupLines);
                result.Add(new SkillGroupInfo(grp.Key.ToString(), title, rep, groupLines));
            }

            return result;
        }

        foreach (var grp in lines.GroupBy(l =>
                     !string.IsNullOrWhiteSpace(l.BasicGroup) && !string.Equals(l.BasicGroup, "xxx", StringComparison.OrdinalIgnoreCase)
                         ? l.BasicGroup
                         : "__general"))
        {
            var groupLines = grp.ToList();
            var rep = PickRepresentativeLine(groupLines);
            var title = grp.Key == "__general"
                ? (IsArabic ? "مهارات عامة" : "General")
                : ResolveGroupTitle(groupLines);
            result.Add(new SkillGroupInfo(grp.Key, title, rep, groupLines));
        }

        return result;
    }

    private static SkillLineInfo PickRepresentativeLine(IReadOnlyList<SkillLineInfo> lines)
    {
        return lines.FirstOrDefault(l =>
                   l.FirstSkill.Basic_Code != null &&
                   (l.FirstSkill.Basic_Code.Contains("_PASSIVE_", StringComparison.OrdinalIgnoreCase) ||
                    l.FirstSkill.Basic_Code.Contains("_BASE_", StringComparison.OrdinalIgnoreCase)))
               ?? lines[0];
    }

    private static string ResolveGroupTitle(IReadOnlyList<SkillLineInfo> lines)
    {
        var textData = Game.ReferenceManager.TextData;
        var groupCode = lines
            .Select(l => l.BasicGroup)
            .FirstOrDefault(g => !string.IsNullOrWhiteSpace(g) && !string.Equals(g, "xxx", StringComparison.OrdinalIgnoreCase));

        if (textData != null && !string.IsNullOrEmpty(groupCode))
        {
            foreach (var candidate in new[]
                     {
                         $"UIIT_STT_{groupCode}",
                         $"SN_{groupCode}_GROUP",
                         $"SN_{groupCode}",
                         groupCode
                     })
            {
                if (textData.TryGetValue(candidate, out var translated) && !string.IsNullOrWhiteSpace(translated?.Data))
                    return translated.Data;
            }
        }

        var rep = PickRepresentativeLine(lines);
        return string.IsNullOrWhiteSpace(rep.DisplayName) ? "Skills" : rep.DisplayName;
    }

    private static HashSet<string> CollectExpandedKeys(TreeNodeCollection nodes)
    {
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (nodes == null)
            return keys;

        foreach (TreeNode node in nodes)
        {
            if (node.IsExpanded)
            {
                var key = GetNodeKey(node.Tag);
                if (key != null)
                    keys.Add(key);
            }

            foreach (var childKey in CollectExpandedKeys(node.Nodes))
                keys.Add(childKey);
        }

        return keys;
    }

    private static string GetNodeKey(object tag)
    {
        return tag switch
        {
            MasteryNodeData m => $"M:{m.MasteryId}",
            GroupNodeData g => $"G:{g.MasteryId}:{g.GroupKey}",
            SkillNodeData s => $"S:{s.Line.CodePrefix}",
            _ => null
        };
    }

    private sealed class SkillGroupInfo
    {
        public SkillGroupInfo(string key, string title, SkillLineInfo iconLine, List<SkillLineInfo> lines)
        {
            Key = key;
            Title = title;
            IconLine = iconLine;
            Lines = lines;
        }

        public string Key { get; }
        public string Title { get; }
        public SkillLineInfo IconLine { get; }
        public List<SkillLineInfo> Lines { get; }
    }

    private sealed class MasteryNodeData
    {
        public MasteryNodeData(uint masteryId, string name, IReadOnlyList<SkillLineInfo> lines)
        {
            MasteryId = masteryId;
            Name = name;
            Lines = lines;
        }

        public uint MasteryId { get; }
        public string Name { get; }
        public IReadOnlyList<SkillLineInfo> Lines { get; }
    }

    private sealed class GroupNodeData
    {
        public GroupNodeData(uint masteryId, string groupKey, string title, List<SkillLineInfo> lines)
        {
            MasteryId = masteryId;
            GroupKey = groupKey;
            Title = title;
            Lines = lines;
        }

        public uint MasteryId { get; }
        public string GroupKey { get; }
        public string Title { get; }
        public List<SkillLineInfo> Lines { get; }
    }

    private sealed class SkillNodeData
    {
        public SkillNodeData(uint masteryId, SkillLineInfo line)
        {
            MasteryId = masteryId;
            Line = line;
        }

        public uint MasteryId { get; }
        public SkillLineInfo Line { get; }
    }
}
