using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using RSBot.Alchemy.Bot;
using RSBot.Alchemy.Bundle.Attribute;
using RSBot.Alchemy.Bundle.Enhance;
using RSBot.Alchemy.Bundle.Magic;
using RSBot.Alchemy.Client.ReferenceObjects;
using RSBot.Alchemy.Helper;
using RSBot.Alchemy.Models;
using RSBot.Alchemy.Views.Dialogs;
using RSBot.Core;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Extensions;
using RSBot.Core.Objects;
using SDUI;

using Button = SDUI.Controls.Button;
using CheckBox = SDUI.Controls.CheckBox;
using ComboBox = SDUI.Controls.ComboBox;
using GroupBox = SDUI.Controls.GroupBox;
using Label = SDUI.Controls.Label;
using NumUpDown = SDUI.Controls.NumUpDown;
using Radio = SDUI.Controls.Radio;
using TabControl = SDUI.Controls.TabControl;

namespace RSBot.Alchemy.Views;

internal sealed class MultiItemAlchemyView : UserControl
{
    private sealed class EquipmentComboItem
    {
        public InventoryItem Item { get; }

        public EquipmentComboItem(InventoryItem item)
        {
            Item = item;
        }

        public override string ToString() => AlchemyItemHelper.FormatEquipmentDisplay(Item);
    }

    private sealed class ElixirComboItem
    {
        public uint ItemId { get; }
        public string Display { get; }

        public ElixirComboItem(uint itemId, string display)
        {
            ItemId = itemId;
            Display = display;
        }

        public override string ToString() => Display;
    }

    // Left panel
    private GroupBox _grpSelectItems;
    private Label _lblItem;
    private Button _btnRefreshItems;
    private ComboBox _comboItems;
    private TabControl _tabInspector;
    private TabPage _pageInspectBlues;
    private TabPage _pageInspectStats;
    private DataGridView _gridInspectBlues;
    private DataGridView _gridInspectStats;

    // Center mode switcher & config panels
    private Radio _radioPlus;
    private Radio _radioBlues;
    private Radio _radioStats;
    private Panel _configHost;
    private Panel _panelPlus;
    private Panel _panelBlues;
    private Panel _panelStats;

    // Plus mode controls
    private Label _lblTargetPlusCaption;
    private Label _lblTargetPlusPrefix;
    private NumUpDown _numTargetPlus;
    private Label _lblCurrentPlusCaption;
    private Label _lblCurrentPlusValue;
    private Label _lblElixirCaption;
    private ComboBox _comboElixir;

    private Label _lblStonesHeader;
    private Label _lblFromPlusHeader;
    private Label _lblQtyHeader;

    private CheckBox _chkUseLuckyPowder;
    private NumUpDown _numPowderFrom;
    private Label _lblPowderCount;
    private CheckBox _chkStopNoPowder;

    private CheckBox _chkUseLuckyStone;
    private NumUpDown _numLuckyFrom;
    private Label _lblLuckyCount;

    private CheckBox _chkUseImmortalStone;
    private NumUpDown _numImmortalFrom;
    private Label _lblImmortalCount;

    private CheckBox _chkUseAstralStone;
    private NumUpDown _numAstralFrom;
    private Label _lblAstralCount;

    private CheckBox _chkUseSteadyStone;
    private NumUpDown _numSteadyFrom;
    private Label _lblSteadyCount;

    private CheckBox _chkSkipOnFail;

    // Blues mode controls
    private Button _btnSelectAllBlues;
    private DataGridView _gridBluesConfig;

    // Stats mode controls
    private Button _btnSelectAllStats;
    private Label _lblSetAllTo;
    private ComboBox _comboSetAllPercent;
    private Button _btnSetAllStats;
    private DataGridView _gridStatsConfig;

    // Right Queue Grid
    private GroupBox _grpQueue;
    private DataGridView _gridQueue;

    // Bottom 7 buttons
    private Button _btnAdd;
    private Button _btnAddMulti;
    private Button _btnRemove;
    private Button _btnClearAll;
    private Button _btnStartAlchemy;
    private Button _btnStopAlchemy;
    private Button _btnClearFinished;

    private bool _suppressItemChange;

    public bool IsRefreshing { get; private set; }
    public InventoryItem SelectedItem { get; set; }

    public MultiItemAlchemyView()
    {
        DoubleBuffered = true;
        BackColor = Color.Transparent;

        BuildUi();
        SubscribeEvents();
        ApplyLocalization();
        SwitchMode(AlchemyTargetMode.Plus);
        ReloadItemList();
    }

    private void BuildUi()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(8, 6, 8, 6),
            BackColor = Color.Transparent
        };
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

        // Main 3-column workspace
        var workspace = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = Color.Transparent
        };
        workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));

        workspace.Controls.Add(BuildLeftPanel(), 0, 0);
        workspace.Controls.Add(BuildCenterPanel(), 1, 0);
        workspace.Controls.Add(BuildRightQueuePanel(), 2, 0);

        root.Controls.Add(workspace, 0, 0);
        root.Controls.Add(BuildBottomBar(), 0, 1);

        Controls.Add(root);
    }

    private Control BuildLeftPanel()
    {
        var leftCol = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(0, 0, 8, 0),
            BackColor = Color.Transparent
        };
        leftCol.RowStyles.Add(new RowStyle(SizeType.Absolute, 122));
        leftCol.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _grpSelectItems = AlchemyUi.CreateGroupBox("اختيار الإيتمات");
        _grpSelectItems.Dock = DockStyle.Fill;
        _grpSelectItems.Margin = new Padding(0, 0, 0, 8);

        var topInner = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(2, 0, 2, 2),
            BackColor = Color.Transparent
        };
        topInner.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        topInner.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 36));
        topInner.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        topInner.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));

        _lblItem = AlchemyUi.CreateLabel("الإيتم:", ContentAlignment.MiddleLeft);
        _lblItem.Dock = DockStyle.Fill;

        _btnRefreshItems = AlchemyUi.CreateButton("↻", ButtonTone.Neutral, 22);
        _btnRefreshItems.Dock = DockStyle.Fill;
        _btnRefreshItems.Margin = new Padding(4, 1, 0, 3);
        _btnRefreshItems.Click += (_, _) => ReloadItemList();

        _comboItems = AlchemyUi.CreateComboBox();
        _comboItems.Dock = DockStyle.Fill;
        _comboItems.Margin = new Padding(0, 4, 0, 4);
        _comboItems.SelectedIndexChanged += (_, _) => OnSelectedItemChanged();

        topInner.Controls.Add(_lblItem, 0, 0);
        topInner.Controls.Add(_btnRefreshItems, 1, 0);
        topInner.Controls.Add(_comboItems, 0, 1);
        topInner.SetColumnSpan(_comboItems, 2);
        _grpSelectItems.Controls.Add(topInner);

        _tabInspector = AlchemyUi.CreateTabControl(82, 26);
        _pageInspectBlues = new TabPage("Blues")
        {
            Name = "pageInspectBlues",
            BackColor = ColorScheme.BackColor,
            Padding = Padding.Empty,
            Margin = Padding.Empty
        };
        _pageInspectStats = new TabPage("Stats")
        {
            Name = "pageInspectStats",
            BackColor = ColorScheme.BackColor,
            Padding = Padding.Empty,
            Margin = Padding.Empty
        };

        _gridInspectBlues = AlchemyUi.CreateGrid();
        _gridInspectBlues.Columns.Add(new DataGridViewTextBoxColumn { Name = "colInspBlue", HeaderText = "Blue", FillWeight = 56 });
        _gridInspectBlues.Columns.Add(new DataGridViewTextBoxColumn { Name = "colInspVal", HeaderText = "Val", FillWeight = 22 });
        _gridInspectBlues.Columns.Add(new DataGridViewTextBoxColumn { Name = "colInspMax", HeaderText = "Max", FillWeight = 22 });
        _pageInspectBlues.Controls.Add(_gridInspectBlues);

        _gridInspectStats = AlchemyUi.CreateGrid();
        _gridInspectStats.Columns.Add(new DataGridViewTextBoxColumn { Name = "colInspStat", HeaderText = "Stat", FillWeight = 68 });
        _gridInspectStats.Columns.Add(new DataGridViewTextBoxColumn { Name = "colInspPct", HeaderText = "%", FillWeight = 32 });
        _pageInspectStats.Controls.Add(_gridInspectStats);

        _tabInspector.TabPages.Add(_pageInspectBlues);
        _tabInspector.TabPages.Add(_pageInspectStats);

        leftCol.Controls.Add(_grpSelectItems, 0, 0);
        leftCol.Controls.Add(_tabInspector, 0, 1);
        return leftCol;
    }

    private Control BuildCenterPanel()
    {
        var centerCol = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(2, 0, 2, 0),
            BackColor = Color.Transparent
        };
        centerCol.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        centerCol.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var modeCard = CreateCardPanel(new Padding(12, 4, 12, 4), new Padding(0, 0, 0, 8));

        var modeBar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = Color.Transparent
        };
        modeBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3F));
        modeBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3F));
        modeBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.4F));

        _radioPlus = AlchemyUi.CreateRadio("Plus", true);
        _radioPlus.Dock = DockStyle.Fill;
        _radioPlus.Margin = new Padding(4, 0, 4, 0);
        _radioPlus.CheckedChanged += (_, _) =>
        {
            if (_radioPlus.Checked) SwitchMode(AlchemyTargetMode.Plus);
        };

        _radioBlues = AlchemyUi.CreateRadio("Blues", false);
        _radioBlues.Dock = DockStyle.Fill;
        _radioBlues.Margin = new Padding(4, 0, 4, 0);
        _radioBlues.CheckedChanged += (_, _) =>
        {
            if (_radioBlues.Checked) SwitchMode(AlchemyTargetMode.Blues);
        };

        _radioStats = AlchemyUi.CreateRadio("Stats", false);
        _radioStats.Dock = DockStyle.Fill;
        _radioStats.Margin = new Padding(4, 0, 4, 0);
        _radioStats.CheckedChanged += (_, _) =>
        {
            if (_radioStats.Checked) SwitchMode(AlchemyTargetMode.Stats);
        };

        modeBar.Controls.Add(_radioPlus, 0, 0);
        modeBar.Controls.Add(_radioBlues, 1, 0);
        modeBar.Controls.Add(_radioStats, 2, 0);
        modeCard.Controls.Add(modeBar);

        var configCard = CreateCardPanel(new Padding(10, 8, 10, 8), Padding.Empty);
        _configHost = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        _panelPlus = BuildPlusPanel();
        _panelBlues = BuildBluesPanel();
        _panelStats = BuildStatsPanel();

        _configHost.Controls.Add(_panelPlus);
        _configHost.Controls.Add(_panelBlues);
        _configHost.Controls.Add(_panelStats);
        configCard.Controls.Add(_configHost);

        centerCol.Controls.Add(modeCard, 0, 0);
        centerCol.Controls.Add(configCard, 0, 1);
        return centerCol;
    }

    private Panel BuildPlusPanel()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, AutoScroll = true };

        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 408,
            ColumnCount = 3,
            RowCount = 12,
            Padding = new Padding(2, 2, 4, 4),
            BackColor = Color.Transparent
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 44));

        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36)); // Row 0: Target Plus
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 32)); // Row 1: Current Plus
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); // Row 2: Elixir Combo
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 12)); // Row 3: Section Divider
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 24)); // Row 4: Section Header
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36)); // Row 5: Lucky Powder
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 32)); // Row 6: Stop if Powder empty
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36)); // Row 7: Lucky Stone
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36)); // Row 8: Immortal Stone
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36)); // Row 9: Astral Stone
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 36)); // Row 10: Steady Stone
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 38)); // Row 11: Skip to next item

        // Row 0: Target Plus
        _lblTargetPlusCaption = AlchemyUi.CreateLabel("وقف عند Plus:", ContentAlignment.MiddleLeft);
        _lblTargetPlusCaption.Dock = DockStyle.Fill;
        _lblTargetPlusPrefix = AlchemyUi.CreateLabel("+", ContentAlignment.MiddleLeft, bold: true);
        _lblTargetPlusPrefix.Dock = DockStyle.Fill;
        _lblTargetPlusPrefix.Margin = new Padding(2, 0, 0, 0);
        _numTargetPlus = AlchemyUi.CreateNumUpDown(0, 30, 0, 84);
        _numTargetPlus.Dock = DockStyle.Fill;
        _numTargetPlus.Margin = new Padding(4, 5, 4, 5);

        grid.Controls.Add(_lblTargetPlusCaption, 0, 0);
        grid.Controls.Add(_numTargetPlus, 1, 0);
        grid.Controls.Add(_lblTargetPlusPrefix, 2, 0);

        // Row 1: Current Plus
        _lblCurrentPlusCaption = AlchemyUi.CreateLabel("Plus الحالي:", ContentAlignment.MiddleLeft);
        _lblCurrentPlusCaption.Dock = DockStyle.Fill;
        _lblCurrentPlusValue = AlchemyUi.CreateLabel("+0", ContentAlignment.MiddleLeft, bold: true);
        _lblCurrentPlusValue.Dock = DockStyle.Fill;
        _lblCurrentPlusValue.Margin = new Padding(4, 0, 0, 0);

        grid.Controls.Add(_lblCurrentPlusCaption, 0, 1);
        grid.Controls.Add(_lblCurrentPlusValue, 1, 1);
        grid.SetColumnSpan(_lblCurrentPlusValue, 2);

        // Row 2: Elixir Combo (full-width row for readable Elixir names)
        var elixirRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Margin = Padding.Empty,
            BackColor = Color.Transparent
        };
        elixirRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 66));
        elixirRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        elixirRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _lblElixirCaption = AlchemyUi.CreateLabel("الـ Elixir:", ContentAlignment.MiddleLeft);
        _lblElixirCaption.Dock = DockStyle.Fill;
        _comboElixir = AlchemyUi.CreateComboBox();
        _comboElixir.Dock = DockStyle.Fill;
        _comboElixir.Margin = new Padding(6, 5, 2, 5);

        elixirRow.Controls.Add(_lblElixirCaption, 0, 0);
        elixirRow.Controls.Add(_comboElixir, 1, 0);
        grid.Controls.Add(elixirRow, 0, 2);
        grid.SetColumnSpan(elixirRow, 3);

        // Row 3: Section Divider
        var divider = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Margin = Padding.Empty };
        divider.Paint += (s, e) =>
        {
            var y = divider.Height / 2;
            using var pen = new Pen(ColorScheme.BorderColor, 1);
            e.Graphics.DrawLine(pen, 0, y, divider.Width - 1, y);
        };
        grid.Controls.Add(divider, 0, 3);
        grid.SetColumnSpan(divider, 3);

        // Row 4: Powder & Stones Header
        _lblStonesHeader = AlchemyUi.CreateLabel("البودر والأحجار:", ContentAlignment.MiddleLeft, bold: true);
        _lblStonesHeader.Dock = DockStyle.Fill;
        _lblStonesHeader.Tag = "CustomFore";
        _lblStonesHeader.ForeColor = ColorScheme.SecondaryForeColor;

        _lblFromPlusHeader = AlchemyUi.CreateLabel("من +", ContentAlignment.MiddleCenter, bold: true);
        _lblFromPlusHeader.Dock = DockStyle.Fill;
        _lblFromPlusHeader.Tag = "CustomFore";
        _lblFromPlusHeader.ForeColor = ColorScheme.SecondaryForeColor;

        _lblQtyHeader = AlchemyUi.CreateLabel("المتاح", ContentAlignment.MiddleCenter, bold: true);
        _lblQtyHeader.Dock = DockStyle.Fill;
        _lblQtyHeader.Tag = "CustomFore";
        _lblQtyHeader.ForeColor = ColorScheme.SecondaryForeColor;

        grid.Controls.Add(_lblStonesHeader, 0, 4);
        grid.Controls.Add(_lblFromPlusHeader, 1, 4);
        grid.Controls.Add(_lblQtyHeader, 2, 4);

        // Row 5: Lucky Powder
        (_chkUseLuckyPowder, _numPowderFrom, _lblPowderCount) =
            AddStoneThresholdRow(grid, 5, "Lucky Powder", true, 0);

        // Row 6: Stop if Lucky Powder empty
        _chkStopNoPowder = AlchemyUi.CreateCheckBox("وقف لو خلص Lucky Powder", true);
        _chkStopNoPowder.Dock = DockStyle.Fill;
        _chkStopNoPowder.Margin = new Padding(16, 3, 0, 3);
        grid.Controls.Add(_chkStopNoPowder, 0, 6);
        grid.SetColumnSpan(_chkStopNoPowder, 3);

        // Row 7-10: Magic Stones
        (_chkUseLuckyStone, _numLuckyFrom, _lblLuckyCount) =
            AddStoneThresholdRow(grid, 7, "Lucky Stone", false, 5);
        (_chkUseImmortalStone, _numImmortalFrom, _lblImmortalCount) =
            AddStoneThresholdRow(grid, 8, "Immortal Stone", false, 5);
        (_chkUseAstralStone, _numAstralFrom, _lblAstralCount) =
            AddStoneThresholdRow(grid, 9, "Astral Stone", false, 5);
        (_chkUseSteadyStone, _numSteadyFrom, _lblSteadyCount) =
            AddStoneThresholdRow(grid, 10, "Steady Stone", false, 5);

        // Row 11: Skip to next item on fail
        _chkSkipOnFail = AlchemyUi.CreateCheckBox("روح للإيتم اللي بعده لو فشل", false);
        _chkSkipOnFail.Dock = DockStyle.Fill;
        _chkSkipOnFail.Margin = new Padding(0, 6, 0, 2);
        grid.Controls.Add(_chkSkipOnFail, 0, 11);
        grid.SetColumnSpan(_chkSkipOnFail, 3);

        panel.Controls.Add(grid);
        return panel;
    }

    private static (CheckBox chk, NumUpDown num, Label countLbl) AddStoneThresholdRow(
        TableLayoutPanel table, int row, string caption, bool defaultChecked, byte defaultFrom)
    {
        var chk = AlchemyUi.CreateCheckBox(caption, defaultChecked);
        chk.Dock = DockStyle.Fill;
        chk.Margin = new Padding(0, 4, 6, 4);

        var num = AlchemyUi.CreateNumUpDown(0, 30, defaultFrom, 84);
        num.Dock = DockStyle.Fill;
        num.Margin = new Padding(4, 5, 4, 5);

        var countLbl = AlchemyUi.CreateLabel("x0", ContentAlignment.MiddleCenter);
        countLbl.Dock = DockStyle.Fill;
        countLbl.Margin = new Padding(2, 0, 0, 0);
        countLbl.Tag = "CustomFore";

        table.Controls.Add(chk, 0, row);
        table.Controls.Add(num, 1, row);
        table.Controls.Add(countLbl, 2, row);

        return (chk, num, countLbl);
    }

    private Panel BuildBluesPanel()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _btnSelectAllBlues = AlchemyUi.CreateButton("حدد كل الـ Blues", ButtonTone.Neutral, 30);
        _btnSelectAllBlues.Dock = DockStyle.Fill;
        _btnSelectAllBlues.Margin = new Padding(0, 0, 0, 6);
        _btnSelectAllBlues.Click += (_, _) => SelectAllBlues();

        _gridBluesConfig = AlchemyUi.CreateGrid(allowEdit: true);
        _gridBluesConfig.Columns.Add(new DataGridViewCheckBoxColumn
            { Name = "colBlueUse", HeaderText = "✓", Width = 34, AutoSizeMode = DataGridViewAutoSizeColumnMode.None });
        _gridBluesConfig.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "colBlueName", HeaderText = "الـ Blue", ReadOnly = true, FillWeight = 42 });
        _gridBluesConfig.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "colBlueCur", HeaderText = "الحالي", ReadOnly = true, FillWeight = 18 });
        _gridBluesConfig.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "colBlueMax", HeaderText = "الماكس", ReadOnly = false, FillWeight = 20 });
        _gridBluesConfig.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "colBlueStones", HeaderText = "الأحجار", ReadOnly = true, FillWeight = 20 });

        layout.Controls.Add(_btnSelectAllBlues, 0, 0);
        layout.Controls.Add(_gridBluesConfig, 0, 1);
        panel.Controls.Add(layout);
        return panel;
    }

    private Panel BuildStatsPanel()
    {
        var panel = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent };
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var bar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, 6),
            BackColor = Color.Transparent
        };
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 82));
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64));
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 52));

        _btnSelectAllStats = AlchemyUi.CreateButton("Select all", ButtonTone.Neutral, 30);
        _btnSelectAllStats.Dock = DockStyle.Fill;
        _btnSelectAllStats.Margin = new Padding(0, 2, 6, 2);
        _btnSelectAllStats.Click += (_, _) => SelectAllStats();

        _lblSetAllTo = AlchemyUi.CreateLabel("Set all to", ContentAlignment.MiddleCenter);
        _lblSetAllTo.Dock = DockStyle.Fill;
        _lblSetAllTo.Margin = new Padding(0, 2, 4, 2);

        _comboSetAllPercent = AlchemyUi.CreateComboBox();
        _comboSetAllPercent.Dock = DockStyle.Fill;
        _comboSetAllPercent.Margin = new Padding(0, 2, 6, 2);
        _comboSetAllPercent.Items.AddRange(new object[] { "0%", "22%", "41%", "61%", "80%", "100%" });
        _comboSetAllPercent.SelectedIndex = 1; // 22% default matching vBot screenshot

        _btnSetAllStats = AlchemyUi.CreateButton("Set", ButtonTone.Neutral, 30);
        _btnSetAllStats.Dock = DockStyle.Fill;
        _btnSetAllStats.Margin = new Padding(0, 2, 0, 2);
        _btnSetAllStats.Click += (_, _) => SetAllStatTargets();

        bar.Controls.Add(_btnSelectAllStats, 0, 0);
        bar.Controls.Add(_lblSetAllTo, 1, 0);
        bar.Controls.Add(_comboSetAllPercent, 2, 0);
        bar.Controls.Add(_btnSetAllStats, 3, 0);

        _gridStatsConfig = AlchemyUi.CreateGrid(allowEdit: true);
        _gridStatsConfig.Columns.Add(new DataGridViewCheckBoxColumn
            { Name = "colStatUse", HeaderText = "✓", Width = 34, AutoSizeMode = DataGridViewAutoSizeColumnMode.None });
        _gridStatsConfig.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "colStatName", HeaderText = "Stat", ReadOnly = true, FillWeight = 44 });
        _gridStatsConfig.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "colStatCur", HeaderText = "Current", ReadOnly = true, FillWeight = 18 });

        var colTarget = new DataGridViewComboBoxColumn
        {
            Name = "colStatTarget",
            HeaderText = "Target",
            FillWeight = 22,
            FlatStyle = FlatStyle.Flat
        };
        colTarget.Items.AddRange("0%", "22%", "41%", "61%", "80%", "100%");
        _gridStatsConfig.Columns.Add(colTarget);

        _gridStatsConfig.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "colStatStones", HeaderText = "Stones", ReadOnly = true, FillWeight = 16 });

        layout.Controls.Add(bar, 0, 0);
        layout.Controls.Add(_gridStatsConfig, 0, 1);
        panel.Controls.Add(layout);
        return panel;
    }

    private Control BuildRightQueuePanel()
    {
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 0, 0, 0), BackColor = Color.Transparent };

        _grpQueue = AlchemyUi.CreateGroupBox("قائمة التطوير");
        _grpQueue.Dock = DockStyle.Fill;

        _gridQueue = AlchemyUi.CreateGrid(allowEdit: false);
        _gridQueue.Columns.Add(new DataGridViewTextBoxColumn { Name = "colQItem", HeaderText = "Item", FillWeight = 34 });
        _gridQueue.Columns.Add(new DataGridViewTextBoxColumn { Name = "colQCur", HeaderText = "Cur", FillWeight = 15 });
        _gridQueue.Columns.Add(new DataGridViewTextBoxColumn { Name = "colQTarget", HeaderText = "Target", FillWeight = 15 });
        _gridQueue.Columns.Add(new DataGridViewTextBoxColumn { Name = "colQMode", HeaderText = "Mode", FillWeight = 16 });
        _gridQueue.Columns.Add(new DataGridViewTextBoxColumn { Name = "colQStatus", HeaderText = "Status", FillWeight = 20 });

        _grpQueue.Controls.Add(_gridQueue);
        panel.Controls.Add(_grpQueue);
        return panel;
    }

    private Control BuildBottomBar()
    {
        var bar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 7,
            RowCount = 1,
            Padding = new Padding(0, 6, 0, 0),
            BackColor = Color.Transparent
        };
        var colWidths = new[] { 11F, 13F, 11F, 13F, 17F, 16F, 19F };
        foreach (var w in colWidths)
            bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, w));

        _btnAdd = AlchemyUi.CreateButton("ضيف", ButtonTone.Neutral, 32);
        _btnAdd.Dock = DockStyle.Fill;
        _btnAdd.Margin = new Padding(0, 2, 3, 2);
        _btnAdd.Click += (_, _) => AddCurrentItemToQueue();

        _btnAddMulti = AlchemyUi.CreateButton("ضيف كذا", ButtonTone.Neutral, 32);
        _btnAddMulti.Dock = DockStyle.Fill;
        _btnAddMulti.Margin = new Padding(3, 2, 3, 2);
        _btnAddMulti.Click += (_, _) => AddMultipleItemsToQueue();

        _btnRemove = AlchemyUi.CreateButton("شيل", ButtonTone.Neutral, 32);
        _btnRemove.Dock = DockStyle.Fill;
        _btnRemove.Margin = new Padding(3, 2, 3, 2);
        _btnRemove.Click += (_, _) => RemoveSelectedFromQueue();

        _btnClearAll = AlchemyUi.CreateButton("شيل الكل", ButtonTone.Neutral, 32);
        _btnClearAll.Dock = DockStyle.Fill;
        _btnClearAll.Margin = new Padding(3, 2, 3, 2);
        _btnClearAll.Click += (_, _) => ClearAllQueue();

        _btnStartAlchemy = AlchemyUi.CreateButton("ابدأ الكمي", ButtonTone.Success, 32);
        _btnStartAlchemy.Dock = DockStyle.Fill;
        _btnStartAlchemy.Margin = new Padding(3, 2, 3, 2);
        _btnStartAlchemy.Click += (_, _) => StartMultiAlchemy();

        _btnStopAlchemy = AlchemyUi.CreateButton("وقف الكمي", ButtonTone.Danger, 32);
        _btnStopAlchemy.Dock = DockStyle.Fill;
        _btnStopAlchemy.Margin = new Padding(3, 2, 3, 2);
        _btnStopAlchemy.Click += (_, _) => AlchemyUi.StopAlchemyBot();

        _btnClearFinished = AlchemyUi.CreateButton("امسح اللي خلص", ButtonTone.Neutral, 32);
        _btnClearFinished.Dock = DockStyle.Fill;
        _btnClearFinished.Margin = new Padding(3, 2, 0, 2);
        _btnClearFinished.Click += (_, _) => ClearFinishedQueue();

        bar.Controls.Add(_btnAdd, 0, 0);
        bar.Controls.Add(_btnAddMulti, 1, 0);
        bar.Controls.Add(_btnRemove, 2, 0);
        bar.Controls.Add(_btnClearAll, 3, 0);
        bar.Controls.Add(_btnStartAlchemy, 4, 0);
        bar.Controls.Add(_btnStopAlchemy, 5, 0);
        bar.Controls.Add(_btnClearFinished, 6, 0);

        return bar;
    }

    private static CardPanel CreateCardPanel(Padding padding, Padding margin) =>
        new CardPanel
        {
            Dock = DockStyle.Fill,
            Padding = padding,
            Margin = margin
        };

    private sealed class CardPanel : Panel
    {
        public CardPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.Transparent;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new RectangleF(0.5f, 0.5f, Math.Max(1, Width - 2), Math.Max(1, Height - 2));
            using var path = CreateRoundedRect(rect, 6f);
            using var bgBrush = new SolidBrush(ColorScheme.CardColor);
            using var borderPen = new Pen(ColorScheme.BorderColor, 1f);

            g.FillPath(bgBrush, path);
            g.DrawPath(borderPen, path);
        }

        private static GraphicsPath CreateRoundedRect(RectangleF bounds, float radius)
        {
            var d = radius * 2f;
            var path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    private void SubscribeEvents()
    {
        if (Globals.Botbase != null)
            Globals.Botbase.QueueChanged += () => SafeInvoke(RefreshQueueGrid);
    }

    private void SwitchMode(AlchemyTargetMode mode)
    {
        _panelPlus.Visible = mode == AlchemyTargetMode.Plus;
        _panelBlues.Visible = mode == AlchemyTargetMode.Blues;
        _panelStats.Visible = mode == AlchemyTargetMode.Stats;
    }

    private AlchemyTargetMode GetActiveMode()
    {
        if (_radioBlues.Checked) return AlchemyTargetMode.Blues;
        if (_radioStats.Checked) return AlchemyTargetMode.Stats;
        return AlchemyTargetMode.Plus;
    }

    public void ReloadItemList()
    {
        if (IsDisposed || Disposing)
            return;

        IsRefreshing = true;
        _suppressItemChange = true;

        try
        {
            var prevSlot = SelectedItem?.Slot ?? (_comboItems.SelectedItem as EquipmentComboItem)?.Item?.Slot;
            _comboItems.Items.Clear();

            var items = AlchemyItemHelper.GetEligibleEquipmentItems();
            EquipmentComboItem match = null;

            foreach (var item in items)
            {
                var wrapper = new EquipmentComboItem(item);
                _comboItems.Items.Add(wrapper);
                if (prevSlot.HasValue && item.Slot == prevSlot.Value)
                    match = wrapper;
            }

            if (match != null)
                _comboItems.SelectedItem = match;
            else if (_comboItems.Items.Count > 0)
                _comboItems.SelectedIndex = 0;

            SelectedItem = (_comboItems.SelectedItem as EquipmentComboItem)?.Item;
        }
        finally
        {
            _suppressItemChange = false;
            IsRefreshing = false;
        }

        RefreshSelectedItemViews();
        RefreshQueueGrid();
    }

    private void OnSelectedItemChanged()
    {
        if (_suppressItemChange)
            return;

        SelectedItem = (_comboItems.SelectedItem as EquipmentComboItem)?.Item;
        RefreshSelectedItemViews();
    }

    private void RefreshSelectedItemViews()
    {
        RefreshInspector();
        RefreshPlusPanel();
        RefreshBluesConfigGrid();
        RefreshStatsConfigGrid();
    }

    private void RefreshInspector()
    {
        _gridInspectBlues.Rows.Clear();
        _gridInspectStats.Rows.Clear();

        if (SelectedItem == null)
            return;

        if (SelectedItem.MagicOptions != null)
        {
            foreach (var magicOption in SelectedItem.MagicOptions)
            {
                var option = Game.ReferenceManager.GetMagicOption(magicOption.Id);
                if (option == null)
                    continue;

                _gridInspectBlues.Rows.Add(
                    option.GetGroupTranslation(),
                    magicOption.Value.ToString(),
                    option.GetMaxValue().ToString());
            }
        }

        if (SelectedItem.Attributes != 0)
        {
            var attrs = ItemAttributesInfo.GetAvailableAttributeGroupsForItem(SelectedItem.Record);
            if (attrs != null)
            {
                foreach (var attr in attrs)
                {
                    var slot = ItemAttributesInfo.GetAttributeSlotForItem(attr, SelectedItem.Record);
                    _gridInspectStats.Rows.Add(
                        attr.GetTranslation(),
                        $"+{SelectedItem.Attributes.GetPercentage(slot)}%");
                }
            }
        }
    }

    private void RefreshPlusPanel()
    {
        var item = SelectedItem;
        if (item == null)
        {
            _lblCurrentPlusValue.Text = "+0";
            _comboElixir.Items.Clear();
            SetCountLabel(_lblPowderCount, 0);
            SetCountLabel(_lblLuckyCount, 0);
            SetCountLabel(_lblImmortalCount, 0);
            SetCountLabel(_lblAstralCount, 0);
            SetCountLabel(_lblSteadyCount, 0);
            return;
        }

        _lblCurrentPlusValue.Text = $"+{item.OptLevel}";

        var prevElixirId = (_comboElixir.SelectedItem as ElixirComboItem)?.ItemId;
        _comboElixir.Items.Clear();

        var elixirType = AlchemyItemHelper.GetElixirTypeForItem(item);
        ElixirComboItem match = null;

        foreach (var grp in AlchemyItemHelper.GetElixirItems(elixirType).GroupBy(i => i.ItemId))
        {
            var total = grp.Sum(x => (int)x.Amount);
            var first = grp.First();
            var entry = new ElixirComboItem(grp.Key, $"{total}x {AlchemyItemHelper.GetItemDisplayName(first.Record)}");
            _comboElixir.Items.Add(entry);
            if (prevElixirId.HasValue && grp.Key == prevElixirId.Value)
                match = entry;
        }

        if (match != null)
            _comboElixir.SelectedItem = match;
        else if (_comboElixir.Items.Count > 0)
            _comboElixir.SelectedIndex = 0;

        SetCountLabel(_lblPowderCount, AlchemyItemHelper.GetLuckyPowders(item).Sum(i => (int)i.Amount));
        SetCountLabel(_lblLuckyCount, AlchemyItemHelper.GetTotalStoneCount(item, RefMagicOpt.MaterialLuck));
        SetCountLabel(_lblImmortalCount, AlchemyItemHelper.GetTotalStoneCount(item, RefMagicOpt.MaterialImmortal));
        SetCountLabel(_lblAstralCount, AlchemyItemHelper.GetTotalStoneCount(item, RefMagicOpt.MaterialAstral));
        SetCountLabel(_lblSteadyCount, AlchemyItemHelper.GetTotalStoneCount(item, RefMagicOpt.MaterialSteady));
    }

    private static void SetCountLabel(Label lbl, int count)
    {
        lbl.Text = $"x{count}";
        lbl.ForeColor = count > 0 ? Color.FromArgb(16, 185, 129) : Color.FromArgb(220, 72, 72);
    }

    private void RefreshBluesConfigGrid()
    {
        var previousChecked = new Dictionary<string, uint>();
        foreach (DataGridViewRow row in _gridBluesConfig.Rows)
        {
            if (row.Tag is string group && Convert.ToBoolean(row.Cells[0].Value))
            {
                uint.TryParse(row.Cells[3].Value?.ToString(), out var t);
                previousChecked[group] = t;
            }
        }

        _gridBluesConfig.Rows.Clear();
        if (SelectedItem?.Record == null)
            return;

        var assignments = Game.ReferenceManager.GetAssignments(SelectedItem.Record.TypeID3, SelectedItem.Record.TypeID4);
        if (assignments == null)
            return;

        foreach (var assignment in assignments)
        {
            if (assignment == null)
                continue;

            var refMagic = Game.ReferenceManager.GetMagicOption(assignment.Group, (byte)SelectedItem.Record.Degree);
            if (refMagic == null)
                continue;

            var stoneCount = AlchemyItemHelper.GetTotalStoneCount(SelectedItem, assignment.Group);
            var curOpt = SelectedItem.MagicOptions?.FirstOrDefault(m => m.Record?.Group == assignment.Group);
            var curVal = curOpt?.Value ?? 0;
            var maxVal = refMagic.GetMaxValue();

            var wasChecked = previousChecked.TryGetValue(assignment.Group, out var prevTarget);
            var targetText = wasChecked && prevTarget > 0 ? prevTarget.ToString() : maxVal.ToString();

            var rowIdx = _gridBluesConfig.Rows.Add(
                wasChecked && stoneCount > 0,
                assignment.GetGroupTranslation(),
                curVal.ToString(),
                targetText,
                $"x{stoneCount}");

            var row = _gridBluesConfig.Rows[rowIdx];
            row.Tag = assignment.Group;
            row.Cells[4].Style.ForeColor = stoneCount > 0
                ? Color.FromArgb(16, 185, 129)
                : Color.FromArgb(220, 72, 72);
        }
    }

    private void RefreshStatsConfigGrid()
    {
        var previous = new Dictionary<ItemAttributeGroup, (bool enabled, string target)>();
        foreach (DataGridViewRow row in _gridStatsConfig.Rows)
        {
            if (row.Tag is ItemAttributeGroup grp)
            {
                previous[grp] = (Convert.ToBoolean(row.Cells[0].Value), row.Cells[3].Value?.ToString() ?? "22%");
            }
        }

        _gridStatsConfig.Rows.Clear();
        if (SelectedItem?.Record == null)
            return;

        var groups = ItemAttributesInfo.GetAvailableAttributeGroupsForItem(SelectedItem.Record);
        if (groups == null)
            return;

        foreach (var grp in groups)
        {
            var slot = ItemAttributesInfo.GetAttributeSlotForItem(grp, SelectedItem.Record);
            var curPct = SelectedItem.Attributes.GetPercentage(slot);
            var stones = AlchemyItemHelper.GetAttributeStones(SelectedItem, grp).Sum(s => (int)s.Amount);

            var hasPrev = previous.TryGetValue(grp, out var prev);
            var targetStr = hasPrev ? prev.target : (_comboSetAllPercent.SelectedItem?.ToString() ?? "22%");
            var checkedVal = hasPrev && prev.enabled && stones > 0;

            var rowIdx = _gridStatsConfig.Rows.Add(
                checkedVal,
                grp.GetTranslation(),
                $"{curPct}%",
                targetStr,
                $"x{stones}");

            var row = _gridStatsConfig.Rows[rowIdx];
            row.Tag = grp;
            row.Cells[4].Style.ForeColor = stones > 0
                ? Color.FromArgb(16, 185, 129)
                : Color.FromArgb(220, 72, 72);
        }
    }

    private void SelectAllBlues()
    {
        foreach (DataGridViewRow row in _gridBluesConfig.Rows)
        {
            if (row.Tag is not string group)
                continue;

            var count = SelectedItem != null ? AlchemyItemHelper.GetTotalStoneCount(SelectedItem, group) : 0;
            if (count > 0)
                row.Cells[0].Value = true;
        }
    }

    private void SelectAllStats()
    {
        var anyWithStones = false;
        foreach (DataGridViewRow row in _gridStatsConfig.Rows)
        {
            if (row.Tag is ItemAttributeGroup grp && SelectedItem != null &&
                AlchemyItemHelper.GetAttributeStones(SelectedItem, grp).Any())
            {
                anyWithStones = true;
                break;
            }
        }

        foreach (DataGridViewRow row in _gridStatsConfig.Rows)
        {
            if (row.Tag is ItemAttributeGroup grp)
            {
                var hasStone = SelectedItem != null && AlchemyItemHelper.GetAttributeStones(SelectedItem, grp).Any();
                row.Cells[0].Value = anyWithStones ? hasStone : true;
            }
        }
    }

    private void SetAllStatTargets()
    {
        var target = _comboSetAllPercent.SelectedItem?.ToString() ?? "22%";
        foreach (DataGridViewRow row in _gridStatsConfig.Rows)
            row.Cells[3].Value = target;
    }

    private AlchemyQueueEntry CreateBaseEntry(InventoryItem item, AlchemyTargetMode mode, byte targetPlus)
    {
        uint? preferredElixirId = null;
        if (SelectedItem != null &&
            AlchemyItemHelper.GetElixirTypeForItem(item) == AlchemyItemHelper.GetElixirTypeForItem(SelectedItem))
        {
            preferredElixirId = (_comboElixir.SelectedItem as ElixirComboItem)?.ItemId;
        }

        return new AlchemyQueueEntry
        {
            Slot = item.Slot,
            ItemId = item.ItemId,
            ItemName = AlchemyItemHelper.GetItemDisplayName(item.Record),
            Degree = (byte)item.Record.Degree,
            Mode = mode,
            TargetPlus = targetPlus,
            PreferredElixirItemId = preferredElixirId,
            UseLuckyPowder = _chkUseLuckyPowder.Checked,
            LuckyPowderFromPlus = (byte)_numPowderFrom.Value,
            StopIfLuckyPowderEmpty = _chkStopNoPowder.Checked,
            UseLuckyStone = _chkUseLuckyStone.Checked,
            LuckyStoneFromPlus = (byte)_numLuckyFrom.Value,
            UseImmortalStone = _chkUseImmortalStone.Checked,
            ImmortalStoneFromPlus = (byte)_numImmortalFrom.Value,
            UseAstralStone = _chkUseAstralStone.Checked,
            AstralStoneFromPlus = (byte)_numAstralFrom.Value,
            UseSteadyStone = _chkUseSteadyStone.Checked,
            SteadyStoneFromPlus = (byte)_numSteadyFrom.Value,
            SkipToNextOnFail = _chkSkipOnFail.Checked
        };
    }

    private AlchemyQueueEntry BuildEntryForItem(InventoryItem item, AlchemyTargetMode mode)
    {
        var entry = CreateBaseEntry(item, mode, (byte)_numTargetPlus.Value);

        if (mode == AlchemyTargetMode.Blues && item.Record != null)
        {
            var validGroups = Game.ReferenceManager
                .GetAssignments(item.Record.TypeID3, item.Record.TypeID4)
                .Select(a => a.Group)
                .ToHashSet();

            foreach (DataGridViewRow row in _gridBluesConfig.Rows)
            {
                if (row.Tag is not string group || !validGroups.Contains(group))
                    continue;

                var enabled = Convert.ToBoolean(row.Cells[0].Value);
                uint.TryParse(row.Cells[3].Value?.ToString(), out var targetVal);
                entry.Blues.Add(new BlueTargetConfig
                {
                    Group = group,
                    DisplayName = row.Cells[1].Value?.ToString() ?? group,
                    TargetValue = targetVal,
                    Enabled = enabled
                });
            }
        }
        else if (mode == AlchemyTargetMode.Stats && item.Record != null)
        {
            var defaultPct = ParsePercent(_comboSetAllPercent.SelectedItem?.ToString() ?? "22%");
            var avail = ItemAttributesInfo.GetAvailableAttributeGroupsForItem(item.Record)?.ToHashSet()
                        ?? new HashSet<ItemAttributeGroup>();

            foreach (DataGridViewRow row in _gridStatsConfig.Rows)
            {
                if (row.Tag is not ItemAttributeGroup grp || !avail.Contains(grp))
                    continue;

                var enabled = Convert.ToBoolean(row.Cells[0].Value);
                var pct = ParsePercent(row.Cells[3].Value?.ToString());
                entry.Stats.Add(new StatTargetConfig
                {
                    Group = grp,
                    DisplayName = grp.GetTranslation(),
                    TargetPercent = pct > 0 ? pct : defaultPct,
                    Enabled = enabled
                });
            }
        }

        return entry;
    }

    private static int ParsePercent(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 22;

        return int.TryParse(text.Replace("%", string.Empty).Trim(), out var v) ? v : 22;
    }

    private void AddCurrentItemToQueue()
    {
        if (SelectedItem == null)
        {
            MessageBox.Show(
                AlchemyUi.IsArabic ? "اختر إيتم الأول من القائمة." : "Please select an item first.",
                "Alchemy", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var mode = GetActiveMode();
        if (!ValidateModeSettings(SelectedItem, mode))
            return;

        var entry = BuildEntryForItem(SelectedItem, mode);
        UpsertQueueEntry(entry);
        RefreshQueueGrid();
    }

    private void AddMultipleItemsToQueue()
    {
        var items = AlchemyItemHelper.GetEligibleEquipmentItems();
        if (items.Count == 0)
        {
            MessageBox.Show(
                AlchemyUi.IsArabic ? "مفيش إيتمات متاحة في الشنطة." : "No eligible equipment found in inventory.",
                "Alchemy", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var defaultPctStr = _comboSetAllPercent.SelectedItem?.ToString() ?? "22%";
        var defaultPct = ParsePercent(defaultPctStr);
        var initialCheckedBlues = new Dictionary<string, uint>();
        var selectedDegree = (byte)(SelectedItem?.Record?.Degree ?? 0);

        foreach (DataGridViewRow row in _gridBluesConfig.Rows)
        {
            if (row.Tag is not string group || !Convert.ToBoolean(row.Cells[0].Value))
                continue;

            uint.TryParse(row.Cells[3].Value?.ToString(), out var targetVal);
            var maxForSelected = selectedDegree > 0
                ? Game.ReferenceManager.GetMagicOption(group, selectedDegree)?.GetMaxValue() ?? 0
                : 0;
            initialCheckedBlues[group] = (maxForSelected > 0 && targetVal >= maxForSelected) ? 0 : targetVal;
        }

        var initialCheckedStats = new Dictionary<ItemAttributeGroup, int>();
        foreach (DataGridViewRow row in _gridStatsConfig.Rows)
        {
            if (row.Tag is not ItemAttributeGroup grp || !Convert.ToBoolean(row.Cells[0].Value))
                continue;

            var pct = ParsePercent(row.Cells[3].Value?.ToString());
            initialCheckedStats[grp] = pct > 0 ? pct : defaultPct;
        }

        var mode = GetActiveMode();
        using var dlg = new MultiAddEquipmentDialog(
            items,
            mode,
            (byte)_numTargetPlus.Value,
            defaultPctStr,
            initialCheckedBlues,
            initialCheckedStats);

        if (dlg.ShowDialog(FindForm()) != DialogResult.OK || dlg.SelectedItems.Count == 0)
            return;

        var addedCount = 0;
        foreach (var item in dlg.SelectedItems)
        {
            // Queue Blues -> Stats -> Plus per item so magic/stat stones run before any plus attempt.
            if (dlg.AddBlues && dlg.SelectedBlues.Count > 0 && item.Record != null)
            {
                var blueEntry = BuildMultiBlueEntry(item, dlg.SelectedBlues, dlg.TargetPlus);
                if (blueEntry != null)
                {
                    UpsertQueueEntry(blueEntry);
                    addedCount++;
                }
            }

            if (dlg.AddStats && dlg.SelectedStats.Count > 0 && item.Record != null)
            {
                var statEntry = BuildMultiStatEntry(item, dlg.SelectedStats, dlg.TargetPlus);
                if (statEntry != null)
                {
                    UpsertQueueEntry(statEntry);
                    addedCount++;
                }
            }

            if (dlg.AddPlus && item.OptLevel < dlg.TargetPlus)
            {
                UpsertQueueEntry(CreateBaseEntry(item, AlchemyTargetMode.Plus, dlg.TargetPlus));
                addedCount++;
            }
        }

        RefreshQueueGrid();

        if (addedCount == 0)
        {
            MessageBox.Show(
                AlchemyUi.IsArabic
                    ? "لم يتم إضافة أي عنصر (الإيتمات المحددة وصلت للهدف أو لا تقبل الإعدادات المختارة)."
                    : "No items were queued (selected items already meet the target or don't support the selected options).",
                "Alchemy", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private AlchemyQueueEntry BuildMultiBlueEntry(
        InventoryItem item,
        IReadOnlyCollection<MultiAddBlueSelection> selectedBlues,
        byte targetPlus)
    {
        var validGroups = Game.ReferenceManager
            .GetAssignments(item.Record.TypeID3, item.Record.TypeID4)
            .Select(a => a.Group)
            .ToHashSet();

        var entry = CreateBaseEntry(item, AlchemyTargetMode.Blues, targetPlus);
        foreach (var sel in selectedBlues)
        {
            if (!validGroups.Contains(sel.Group))
                continue;

            var maxVal = Game.ReferenceManager
                .GetMagicOption(sel.Group, (byte)item.Record.Degree)
                ?.GetMaxValue() ?? 0;

            var targetVal = sel.CustomTargetValue > 0
                ? (maxVal > 0 ? Math.Min(sel.CustomTargetValue, maxVal) : sel.CustomTargetValue)
                : maxVal;

            if (targetVal == 0)
                continue;

            entry.Blues.Add(new BlueTargetConfig
            {
                Group = sel.Group,
                DisplayName = sel.DisplayName,
                TargetValue = targetVal,
                Enabled = true
            });
        }

        return entry.Blues.Count > 0 ? entry : null;
    }

    private AlchemyQueueEntry BuildMultiStatEntry(
        InventoryItem item,
        IReadOnlyCollection<MultiAddStatSelection> selectedStats,
        byte targetPlus)
    {
        var validAttrs = ItemAttributesInfo.GetAvailableAttributeGroupsForItem(item.Record)?.ToHashSet()
                         ?? new HashSet<ItemAttributeGroup>();

        var entry = CreateBaseEntry(item, AlchemyTargetMode.Stats, targetPlus);
        foreach (var sel in selectedStats)
        {
            if (!validAttrs.Contains(sel.Group))
                continue;

            entry.Stats.Add(new StatTargetConfig
            {
                Group = sel.Group,
                DisplayName = sel.DisplayName,
                TargetPercent = sel.TargetPercent,
                Enabled = true
            });
        }

        return entry.Stats.Count > 0 ? entry : null;
    }

    private static void UpsertQueueEntry(AlchemyQueueEntry entry)
    {
        var queue = Globals.Botbase?.Queue;
        if (queue == null)
            return;

        var existingIdx = queue.FindIndex(q => q.Slot == entry.Slot && q.ItemId == entry.ItemId && q.Mode == entry.Mode);
        if (existingIdx >= 0)
            queue[existingIdx] = entry;
        else
            queue.Add(entry);
    }

    private bool ValidateModeSettings(InventoryItem item, AlchemyTargetMode mode)
    {
        if (mode == AlchemyTargetMode.Plus && _numTargetPlus.Value <= item.OptLevel)
        {
            MessageBox.Show(
                AlchemyUi.IsArabic
                    ? $"حدد قيمة Plus مستهدفة أعلى من +{item.OptLevel}."
                    : $"Target Plus must be higher than current +{item.OptLevel}.",
                "Plus", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        if (mode == AlchemyTargetMode.Blues && !_gridBluesConfig.Rows.Cast<DataGridViewRow>().Any(r => Convert.ToBoolean(r.Cells[0].Value)))
        {
            MessageBox.Show(
                AlchemyUi.IsArabic ? "حدد Blue واحد على الأقل." : "Select at least one Blue to add.",
                "Blues", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        if (mode == AlchemyTargetMode.Stats && !_gridStatsConfig.Rows.Cast<DataGridViewRow>().Any(r => Convert.ToBoolean(r.Cells[0].Value)))
        {
            MessageBox.Show(
                AlchemyUi.IsArabic ? "حدد Stat واحدة على الأقل." : "Select at least one Stat to add.",
                "Stats", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return false;
        }

        return true;
    }

    private void RemoveSelectedFromQueue()
    {
        if (_gridQueue.SelectedRows.Count == 0 || Globals.Botbase?.Queue == null)
            return;

        if (_gridQueue.SelectedRows[0].Tag is AlchemyQueueEntry entry)
        {
            Globals.Botbase.Queue.Remove(entry);
            RefreshQueueGrid();
        }
    }

    private void ClearAllQueue()
    {
        Globals.Botbase?.Queue.Clear();
        RefreshQueueGrid();
    }

    private void ClearFinishedQueue()
    {
        Globals.Botbase?.Queue.RemoveAll(q => q.Status == QueueItemStatus.Completed || q.Status == QueueItemStatus.Failed);
        RefreshQueueGrid();
    }

    private void StartMultiAlchemy()
    {
        var bot = Globals.Botbase;
        if (bot == null)
            return;

        if (bot.Queue.Count > 0)
        {
            if (!bot.Queue.Any(q => q.Status == QueueItemStatus.Pending))
            {
                // Re-enable surviving non-completed or completed entries only if none remain pending
                foreach (var entry in bot.Queue.Where(e => e.ResolveLiveItem() != null))
                {
                    entry.Status = QueueItemStatus.Pending;
                    entry.StatusNote = string.Empty;
                }
            }

            if (!bot.Queue.Any(q => q.Status == QueueItemStatus.Pending))
            {
                MessageBox.Show(
                    AlchemyUi.IsArabic ? "لا توجد عناصر جاهزة في القائمة." : "No runnable items in the queue.",
                    "Alchemy", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            RefreshQueueGrid();
            AlchemyUi.EnsureAlchemyBotStarted(AlchemyEngine.Queue);
            return;
        }

        // Quick single-item run when queue is empty
        if (SelectedItem == null)
        {
            MessageBox.Show(
                AlchemyUi.IsArabic ? "اختر إيتم أو أضف عناصر للقائمة الأول." : "Select an item or add items to queue first.",
                "Alchemy", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var mode = GetActiveMode();
        if (!ValidateModeSettings(SelectedItem, mode))
            return;

        var single = BuildEntryForItem(SelectedItem, mode);
        bot.Queue.Add(single);
        RefreshQueueGrid();
        AlchemyUi.EnsureAlchemyBotStarted(AlchemyEngine.Queue);
    }

    public void RefreshQueueGrid()
    {
        if (IsDisposed || Disposing)
            return;

        RefreshBotButtons();

        var selectedId = _gridQueue.SelectedRows.Count > 0 && _gridQueue.SelectedRows[0].Tag is AlchemyQueueEntry s
            ? s.Id
            : Guid.Empty;

        _gridQueue.Rows.Clear();
        var queue = Globals.Botbase?.Queue;
        if (queue == null)
            return;

        foreach (var entry in queue)
        {
            var idx = _gridQueue.Rows.Add(
                entry.ItemName,
                entry.GetCurrentSummary(),
                entry.GetTargetSummary(),
                entry.GetModeLabel(),
                entry.GetStatusLabel(AlchemyUi.IsArabic));

            var row = _gridQueue.Rows[idx];
            row.Tag = entry;

            if (entry.Status == QueueItemStatus.Completed)
                row.Cells[4].Style.ForeColor = Color.FromArgb(16, 185, 129);
            else if (entry.Status == QueueItemStatus.Failed)
                row.Cells[4].Style.ForeColor = Color.FromArgb(239, 68, 68);
            else if (entry.Status == QueueItemStatus.Running)
                row.Cells[4].Style.ForeColor = ColorScheme.AccentColor;

            if (entry.Id == selectedId)
                row.Selected = true;
        }
    }

    public void RefreshBotButtons()
    {
        if (IsDisposed || Disposing)
            return;

        var queueRunning = Bootstrap.IsActive && Globals.Botbase?.AlchemyEngine == AlchemyEngine.Queue;
        _btnStartAlchemy.Enabled = !queueRunning;
        _btnStopAlchemy.Enabled = Bootstrap.IsActive;
    }

    public void ApplyLocalization()
    {
        var ar = AlchemyUi.IsArabic;

        _grpSelectItems.Text = ar ? "اختيار الإيتمات" : "Select Items";
        _lblItem.Text = ar ? "الإيتم:" : "Item:";
        _pageInspectBlues.Text = "Blues";
        _pageInspectStats.Text = "Stats";
        if (_grpQueue != null)
            _grpQueue.Text = ar ? "قائمة التطوير" : "Alchemy Queue";

        _lblTargetPlusCaption.Text = ar ? "وقف عند Plus:" : "Stop at Plus:";
        _lblCurrentPlusCaption.Text = ar ? "Plus الحالي:" : "Current Plus:";
        _lblElixirCaption.Text = ar ? "الـ Elixir:" : "Elixir:";

        if (_lblStonesHeader != null)
            _lblStonesHeader.Text = ar ? "البودر والأحجار:" : "Powder & Stones:";
        if (_lblFromPlusHeader != null)
            _lblFromPlusHeader.Text = ar ? "من +" : "From +";
        if (_lblQtyHeader != null)
            _lblQtyHeader.Text = ar ? "المتاح" : "Qty";

        _chkUseLuckyPowder.Text = "Lucky Powder";
        _chkStopNoPowder.Text = ar ? "وقف لو خلص Lucky Powder" : "Stop if Lucky Powder empty";
        _chkUseLuckyStone.Text = "Lucky Stone";
        _chkUseImmortalStone.Text = "Immortal Stone";
        _chkUseAstralStone.Text = "Astral Stone";
        _chkUseSteadyStone.Text = "Steady Stone";
        _chkSkipOnFail.Text = ar ? "روح للإيتم اللي بعده لو فشل" : "Move to next item on fail";

        _btnSelectAllBlues.Text = ar ? "حدد كل الـ Blues" : "Select All Blues";
        _gridBluesConfig.Columns[1].HeaderText = ar ? "الـ Blue" : "Blue";
        _gridBluesConfig.Columns[2].HeaderText = ar ? "الحالي" : "Current";
        _gridBluesConfig.Columns[3].HeaderText = ar ? "الماكس" : "Max";
        _gridBluesConfig.Columns[4].HeaderText = ar ? "الأحجار" : "Stones";

        _btnSelectAllStats.Text = "Select all";
        _lblSetAllTo.Text = "Set all to";
        _btnSetAllStats.Text = "Set";

        _btnAdd.Text = ar ? "ضيف" : "Add";
        _btnAddMulti.Text = ar ? "ضيف كذا" : "Add Multi";
        _btnRemove.Text = ar ? "شيل" : "Remove";
        _btnClearAll.Text = ar ? "شيل الكل" : "Clear All";
        _btnStartAlchemy.Text = ar ? "ابدأ الكمي" : "Start Alchemy";
        _btnStopAlchemy.Text = ar ? "وقف الكمي" : "Stop Alchemy";
        _btnClearFinished.Text = ar ? "امسح اللي خلص" : "Clear Finished";

        _gridQueue.Columns[0].HeaderText = ar ? "الإيتم" : "Item";
        _gridQueue.Columns[1].HeaderText = ar ? "الحالي" : "Cur";
        _gridQueue.Columns[2].HeaderText = ar ? "الهدف" : "Target";
        _gridQueue.Columns[3].HeaderText = ar ? "الوضع" : "Mode";
        _gridQueue.Columns[4].HeaderText = ar ? "الحالة" : "Status";

        RefreshQueueGrid();
    }

    public void ApplyTheme()
    {
        AlchemyUi.ApplyThemeRecursive(this);
        if (_pageInspectBlues != null) _pageInspectBlues.BackColor = ColorScheme.BackColor;
        if (_pageInspectStats != null) _pageInspectStats.BackColor = ColorScheme.BackColor;
        if (_lblStonesHeader != null) _lblStonesHeader.ForeColor = ColorScheme.SecondaryForeColor;
        if (_lblFromPlusHeader != null) _lblFromPlusHeader.ForeColor = ColorScheme.SecondaryForeColor;
        if (_lblQtyHeader != null) _lblQtyHeader.ForeColor = ColorScheme.SecondaryForeColor;
        RefreshPlusPanel();
        RefreshBluesConfigGrid();
        RefreshStatsConfigGrid();
        RefreshQueueGrid();
        Invalidate(true);
    }

    private void SafeInvoke(System.Action action)
    {
        if (IsDisposed || Disposing || !IsHandleCreated)
            return;

        try
        {
            if (InvokeRequired)
                BeginInvoke(action);
            else
                action();
        }
        catch (InvalidOperationException)
        {
        }
    }
}
