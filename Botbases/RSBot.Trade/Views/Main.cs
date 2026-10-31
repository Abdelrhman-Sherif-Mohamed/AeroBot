using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Objects;
using RSBot.Trade.Bundle;
using RSBot.Trade.Components;
using SDUI;
using SDUI.Controls;
using Button = SDUI.Controls.Button;
using CheckBox = SDUI.Controls.CheckBox;
using ComboBox = SDUI.Controls.ComboBox;
using ContextMenuStrip = SDUI.Controls.ContextMenuStrip;
using GroupBox = SDUI.Controls.GroupBox;
using Label = SDUI.Controls.Label;
using ListView = SDUI.Controls.ListView;
using TabControl = SDUI.Controls.TabControl;
using TextBox = SDUI.Controls.TextBox;

namespace RSBot.Trade.Views;

public partial class Main : DoubleBufferedControl
{
    private bool _loadingConfig;

    public Main()
    {
        InitializeComponent();
        Load += Main_Load;
        SubscribeEvents();
    }

    #region UI construction

    private void BuildUi()
    {
        _loadingConfig = true;

        SuspendLayout();

        BackColor = ColorScheme.BackColor;
        ForeColor = ColorScheme.ForeColor;
        Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

        tabControlMain = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = Font,
            SizeMode = TabSizeMode.Fixed,
            ItemSize = new Size(112, 30),
            Margin = new Padding(0)
        };

        tabPageSettings = CreateTabPage("الإعدادات");
        tabPageGoods = CreateTabPage("البضائع");
        tabPageRoute = CreateTabPage("الطريق");

        tabControlMain.TabPages.AddRange(new[] { tabPageSettings, tabPageGoods, tabPageRoute });

        BuildSettingsTab();
        BuildGoodsTab();
        BuildRouteTab();

        panelLog = BuildLogPanel();

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = ColorScheme.BackColor,
            Margin = new Padding(0),
            Padding = new Padding(0)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 70F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
        root.Controls.Add(tabControlMain, 0, 0);
        root.Controls.Add(panelLog, 0, 1);

        Controls.Add(root);

        // The route page is the main page, exactly like vBot.
        tabControlMain.SelectedIndex = 2;

        uiTimer = new Timer(components) { Interval = 1000 };
        uiTimer.Tick += UiTimer_Tick;
        uiTimer.Start();

        ApplyLanguage();
        _loadingConfig = false;

        ResumeLayout(true);
    }

    private static TabPage CreateTabPage(string text)
    {
        return new TabPage(text)
        {
            BackColor = ColorScheme.BackColor,
            ForeColor = ColorScheme.ForeColor,
            Padding = new Padding(6),
            UseVisualStyleBackColor = false
        };
    }

    private void BuildSettingsTab()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent,
            Padding = new Padding(2)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        groupSettings = new GroupBox
        {
            Text = "الإعدادات",
            Dock = DockStyle.Fill,
            Padding = new Padding(10, 28, 10, 8),
            Margin = new Padding(0, 0, 4, 0)
        };

        groupLoopOptions = new GroupBox
        {
            Text = "خيارات التشغيل",
            Dock = DockStyle.Fill,
            Padding = new Padding(10, 28, 10, 8),
            Margin = new Padding(4, 0, 0, 0)
        };

        BuildSettingsGroup();
        BuildLoopOptionsGroup();

        layout.Controls.Add(groupSettings, 0, 0);
        layout.Controls.Add(groupLoopOptions, 1, 0);
        tabPageSettings.Controls.Add(layout);
    }

    private void BuildSettingsGroup()
    {
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 0,
            BackColor = Color.Transparent
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 84F));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 28F));

        checkAttackThiefNpc = MakeCheckBox("مهاجمة لصوص NPC");
        checkAttackThiefNpc.CheckedChanged += checkSettings_CheckedChanged;
        AddCheckRow(table, checkAttackThiefNpc, 25);

        checkAttackThiefPlayers = MakeCheckBox("مهاجمة لصوص اللاعبين");
        checkAttackThiefPlayers.CheckedChanged += checkSettings_CheckedChanged;
        AddCheckRow(table, checkAttackThiefPlayers, 25);

        checkCounterAttack = MakeCheckBox("الهجوم المضاد");
        checkCounterAttack.CheckedChanged += checkSettings_CheckedChanged;
        AddCheckRow(table, checkCounterAttack, 25);

        checkProtectTransport = MakeCheckBox("حماية وسيلة النقل");
        checkProtectTransport.CheckedChanged += checkSettings_CheckedChanged;
        AddCheckRow(table, checkProtectTransport, 25);

        checkMountTransport = MakeCheckBox("امتطاء وسيلة النقل");
        checkMountTransport.CheckedChanged += checkSettings_CheckedChanged;
        AddCheckRow(table, checkMountTransport, 25);

        checkCastBuffs = MakeCheckBox("تفعيل البفات");
        checkCastBuffs.CheckedChanged += checkSettings_CheckedChanged;
        AddCheckRow(table, checkCastBuffs, 25);

        checkWaitForHunter = MakeCheckBox("انتظار الصياد");
        checkWaitForHunter.CheckedChanged += checkSettings_CheckedChanged;
        AddCheckRow(table, checkWaitForHunter, 25);

        checkAutoStartAfterLogin = MakeCheckBox("بدء تلقائي بعد تسجيل الدخول");
        checkAutoStartAfterLogin.CheckedChanged += checkAutoStartAfterLogin_CheckedChanged;
        AddCheckRow(table, checkAutoStartAfterLogin, 25);

        lblMaxAttackDistance = MakeLabel("أقصى مسافة هجوم:");
        numMaxAttackDistance = MakeNum(5, 200, 50);
        numMaxAttackDistance.ValueChanged += numSetting_ValueChanged;
        lblMaxAttackUnit = MakeLabel("م", ContentAlignment.MiddleCenter);
        AddNumericRow(table, lblMaxAttackDistance, numMaxAttackDistance, lblMaxAttackUnit, 30);

        lblMaxTransportDistance = MakeLabel("أقصى مسافة نقل:");
        numMaxTransportDistance = MakeNum(1, 50, 15);
        numMaxTransportDistance.ValueChanged += numSetting_ValueChanged;
        lblMaxTransportUnit = MakeLabel("م", ContentAlignment.MiddleCenter);
        AddNumericRow(table, lblMaxTransportDistance, numMaxTransportDistance, lblMaxTransportUnit, 30);

        var spacer = table.RowStyles.Count;
        table.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        table.RowCount = table.RowStyles.Count;

        groupSettings.Controls.Add(table);
    }

    private void BuildLoopOptionsGroup()
    {
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 0,
            BackColor = Color.Transparent
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72F));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70F));

        var loopRow = table.RowStyles.Count;
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        table.RowCount = table.RowStyles.Count;

        lblTradeLoops = MakeLabel("تكرار جولات التجارة:");
        numTradeLoops = MakeNum(0, 999, 1);
        numTradeLoops.ValueChanged += numTradeLoops_ValueChanged;
        lblInfiniteLoops = MakeLabel("0 = ∞", ContentAlignment.MiddleCenter);
        lblInfiniteLoops.ForeColor = Color.Gray;

        table.Controls.Add(lblTradeLoops, 0, loopRow);
        table.Controls.Add(numTradeLoops, 1, loopRow);
        table.Controls.Add(lblInfiniteLoops, 2, loopRow);

        checkUseReturnScroll = MakeCheckBox("استخدام سكرول العودة بعد الجولة");
        checkUseReturnScroll.CheckedChanged += checkSettings_CheckedChanged;
        AddCheckRow(table, checkUseReturnScroll, 25);

        checkSkipTownScript = MakeCheckBox("تخطي سكرول المدينة");
        checkSkipTownScript.CheckedChanged += checkSettings_CheckedChanged;
        AddCheckRow(table, checkSkipTownScript, 25);

        checkSmartThiefCombat = MakeCheckBox("قتال اللصوص الذكي");
        checkSmartThiefCombat.CheckedChanged += checkSettings_CheckedChanged;
        AddCheckRow(table, checkSmartThiefCombat, 25);

        checkSkipNpcIfHasGoods = MakeCheckBox("تخطي NPC إذا كانت البضاعة محمّلة");
        checkSkipNpcIfHasGoods.CheckedChanged += checkSettings_CheckedChanged;
        AddCheckRow(table, checkSkipNpcIfHasGoods, 25);

        checkSmartObstacleEvasion = MakeCheckBox("تجنب العوائق الذكي");
        checkSmartObstacleEvasion.CheckedChanged += checkSettings_CheckedChanged;
        AddCheckRow(table, checkSmartObstacleEvasion, 25);

        var runModeRow = table.RowStyles.Count;
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        table.RowCount = table.RowStyles.Count;
        lblRunMode = MakeLabel("وضع التشغيل:");
        table.Controls.Add(lblRunMode, 0, runModeRow);
        table.SetColumnSpan(lblRunMode, 3);

        radioCityToCity = MakeRadio("من مدينة إلى مدينة");
        radioCityToCity.CheckedChanged += radioSetting_CheckedChanged;
        AddRadioRow(table, radioCityToCity);

        radioRouteList = MakeRadio("قائمة المسارات");
        radioRouteList.CheckedChanged += radioSetting_CheckedChanged;
        AddRadioRow(table, radioRouteList);

        radioTracePlayer = MakeRadio("تتبع لاعب");
        radioTracePlayer.CheckedChanged += radioSetting_CheckedChanged;
        AddRadioRow(table, radioTracePlayer);

        var traceRow = table.RowStyles.Count;
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        table.RowCount = table.RowStyles.Count;

        lblTracePlayerName = MakeLabel("اسم اللاعب:");
        txtTracePlayerName = new TextBox
        {
            Dock = DockStyle.Fill,
            Height = 28,
            Margin = new Padding(0, 2, 0, 2),
            Font = Font
        };
        txtTracePlayerName.TextChanged += txtTracePlayerName_TextChanged;

        table.Controls.Add(lblTracePlayerName, 0, traceRow);
        table.Controls.Add(txtTracePlayerName, 1, traceRow);
        table.SetColumnSpan(txtTracePlayerName, 2);

        var hintRow = table.RowStyles.Count;
        table.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        table.RowCount = table.RowStyles.Count;

        lblSettingsHint = MakeLabel("اختر وضع التشغيل ثم اضغط ابدأ. القيمة 0 في الحلقات تعني تكراراً بلا نهاية.");
        lblSettingsHint.ForeColor = Color.Gray;
        lblSettingsHint.TextAlign = ContentAlignment.TopLeft;
        table.Controls.Add(lblSettingsHint, 0, hintRow);
        table.SetColumnSpan(lblSettingsHint, 3);

        groupLoopOptions.Controls.Add(table);
    }

    private void BuildGoodsTab()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.Transparent,
            Padding = new Padding(2)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));

        var top = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent
        };
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));

        lblGoodsStatus = MakeLabel("وسيلة النقل: غير متصلة");
        lblGoodsStatus.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnRefreshGoods = MakeButton("تحديث", 88, 28);
        btnRefreshGoods.Click += (_, _) => RefreshGoods();

        top.Controls.Add(lblGoodsStatus, 0, 0);
        top.Controls.Add(btnRefreshGoods, 1, 0);

        lvGoods = new ListView
        {
            Dock = DockStyle.Fill,
            View = System.Windows.Forms.View.Details,
            FullRowSelect = true,
            GridLines = true,
            BorderStyle = BorderStyle.None,
            MultiSelect = false,
            HideSelection = false,
            Margin = new Padding(0, 2, 0, 2)
        };

        colGoodsName = new ColumnHeader { Text = "البضاعة", Width = 360 };
        colGoodsAmount = new ColumnHeader { Text = "الكمية", Width = 90, TextAlign = HorizontalAlignment.Center };
        colGoodsMax = new ColumnHeader { Text = "الحد الأقصى", Width = 100, TextAlign = HorizontalAlignment.Center };
        lvGoods.Columns.AddRange(new[] { colGoodsName, colGoodsAmount, colGoodsMax });

        lblGoodsHint = MakeLabel("تُعرض هنا بضائع وسيلة النقل. استخدم زر التحديث لتحديث القائمة. 0 في كمية الشراء يعني شراء الكل.");
        lblGoodsHint.ForeColor = Color.Gray;
        lblGoodsHint.TextAlign = ContentAlignment.TopLeft;

        layout.Controls.Add(top, 0, 0);
        layout.Controls.Add(lvGoods, 0, 1);
        layout.Controls.Add(lblGoodsHint, 0, 2);
        tabPageGoods.Controls.Add(layout);
    }

    private void BuildRouteTab()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent,
            Padding = new Padding(2)
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 73F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 27F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var left = BuildRouteLeftPanel();
        groupBuySell = BuildSpecialGoodsGroup();

        layout.Controls.Add(left, 0, 0);
        layout.Controls.Add(groupBuySell, 1, 0);
        tabPageRoute.Controls.Add(layout);
    }

    private Control BuildRouteLeftPanel()
    {
        var left = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            BackColor = Color.Transparent,
            Margin = new Padding(0, 0, 4, 0)
        };
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 54F));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        left.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        left.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));

        var cities = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = Color.Transparent
        };
        cities.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
        cities.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        cities.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));

        comboStartCity = MakeCombo();
        comboStartCity.SelectedIndexChanged += comboStartCity_SelectedIndexChanged;

        comboDestCity = MakeCombo();
        comboDestCity.SelectedIndexChanged += comboDestCity_SelectedIndexChanged;

        comboTransport = MakeCombo();
        comboTransport.SelectedIndexChanged += comboTransport_SelectedIndexChanged;

        lblStartCity = MakeLabel("مدينة البداية");
        lblStartCity.Font = new Font("Segoe UI", 8.5F);
        lblStartCity.ForeColor = Color.Gray;

        lblDestCity = MakeLabel("مدينة النهاية");
        lblDestCity.Font = new Font("Segoe UI", 8.5F);
        lblDestCity.ForeColor = Color.Gray;

        lblTransport = MakeLabel("وسيلة النقل");
        lblTransport.Font = new Font("Segoe UI", 8.5F);
        lblTransport.ForeColor = Color.Gray;

        cities.Controls.Add(BuildCityCell(lblStartCity, comboStartCity), 0, 0);
        cities.Controls.Add(BuildCityCell(lblDestCity, comboDestCity), 1, 0);
        cities.Controls.Add(BuildCityCell(lblTransport, comboTransport), 2, 0);

        var buttons = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = Color.Transparent
        };
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92F));
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92F));
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92F));

        btnAddRoute = MakeButton("إضافة", 86, 30);
        btnAddRoute.Click += btnAddRoute_Click;

        btnStartTrade = MakeButton("ابدأ", 86, 30);
        btnStartTrade.Color = Color.FromArgb(34, 197, 94);
        btnStartTrade.ForeColor = Color.White;
        btnStartTrade.Click += btnStartTrade_Click;

        btnStopTrade = MakeButton("توقف", 86, 30);
        btnStopTrade.Color = Color.FromArgb(220, 38, 38);
        btnStopTrade.ForeColor = Color.White;
        btnStopTrade.Click += btnStopTrade_Click;

        buttons.Controls.Add(btnAddRoute, 1, 0);
        buttons.Controls.Add(btnStartTrade, 2, 0);
        buttons.Controls.Add(btnStopTrade, 3, 0);

        var listBar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = Color.Transparent
        };
        listBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72F));
        listBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        listBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 34F));
        listBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 34F));

        lblRouteList = MakeLabel("القائمة:");
        comboRouteList = MakeCombo();
        comboRouteList.SelectedIndexChanged += comboRouteList_SelectedIndexChanged;

        btnNewRouteList = MakeButton("+", 30, 26);
        btnNewRouteList.Click += btnNewRouteList_Click;

        btnDeleteRouteList = MakeButton("×", 30, 26);
        btnDeleteRouteList.Click += btnDeleteRouteList_Click;

        listBar.Controls.Add(lblRouteList, 0, 0);
        listBar.Controls.Add(comboRouteList, 1, 0);
        listBar.Controls.Add(btnNewRouteList, 2, 0);
        listBar.Controls.Add(btnDeleteRouteList, 3, 0);

        contextMenuRoutes = new ContextMenuStrip();
        menuToggleRoute = new ToolStripMenuItem("تفعيل / تعطيل المسار");
        menuEditLoops = new ToolStripMenuItem("تعديل عدد الحلقات (عام)");
        menuRemoveRoute = new ToolStripMenuItem("حذف المسار من القائمة");

        menuToggleRoute.Click += menuToggleRoute_Click;
        menuEditLoops.Click += menuEditLoops_Click;
        menuRemoveRoute.Click += menuRemoveRoute_Click;

        contextMenuRoutes.Items.AddRange(new ToolStripItem[]
        {
            menuToggleRoute,
            menuEditLoops,
            menuRemoveRoute
        });

        lvRoutes = new ListView
        {
            Dock = DockStyle.Fill,
            View = System.Windows.Forms.View.Details,
            FullRowSelect = true,
            GridLines = true,
            BorderStyle = BorderStyle.None,
            MultiSelect = false,
            HideSelection = false,
            HeaderStyle = ColumnHeaderStyle.Nonclickable,
            ContextMenuStrip = contextMenuRoutes,
            Margin = new Padding(0, 2, 0, 2)
        };

        colIndex = new ColumnHeader { Text = "#", Width = 34, TextAlign = HorizontalAlignment.Center };
        colStart = new ColumnHeader { Text = "بدء", Width = 82 };
        colEnd = new ColumnHeader { Text = "نهاية", Width = 82 };
        colScript = new ColumnHeader { Text = "السكريبت", Width = 226 };
        colActive = new ColumnHeader { Text = "نشط", Width = 58, TextAlign = HorizontalAlignment.Center };
        colLoop = new ColumnHeader { Text = "حلقة", Width = 52, TextAlign = HorizontalAlignment.Center };

        lvRoutes.Columns.AddRange(new[]
        {
            colIndex, colStart, colEnd, colScript, colActive, colLoop
        });

        lvRoutes.DoubleClick += (_, _) => ToggleSelectedRoute();
        lvRoutes.MouseDown += lvRoutes_MouseDown;

        var bottom = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 6,
            RowCount = 1,
            BackColor = Color.Transparent
        };
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 36F));
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 36F));
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190F));
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92F));
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 262F));

        btnMoveUp = MakeButton("↑", 30, 28);
        btnMoveUp.Click += (_, _) => MoveRoute(-1);

        btnMoveDown = MakeButton("↓", 30, 28);
        btnMoveDown.Click += (_, _) => MoveRoute(1);

        checkDisableAutoRun = MakeCheckBox("تعطيل التشغيل التلقائي");
        checkDisableAutoRun.CheckedChanged += checkDisableAutoRun_CheckedChanged;

        linkGuide = new LinkLabel
        {
            Text = "دليل التشغيل",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            LinkBehavior = LinkBehavior.HoverUnderline,
            LinkColor = Color.FromArgb(96, 165, 250),
            ActiveLinkColor = Color.FromArgb(147, 197, 253),
            VisitedLinkColor = Color.FromArgb(96, 165, 250),
            BackColor = Color.Transparent,
            AutoSize = false
        };
        linkGuide.LinkClicked += linkGuide_LinkClicked;

        lblRouteStatus = MakeLabel("مدينة → مدينة | 1 جولة", ContentAlignment.MiddleRight);
        lblRouteStatus.ForeColor = Color.FromArgb(96, 165, 250);
        lblRouteStatus.Font = new Font("Segoe UI", 8.5F);

        bottom.Controls.Add(btnMoveUp, 0, 0);
        bottom.Controls.Add(btnMoveDown, 1, 0);
        bottom.Controls.Add(checkDisableAutoRun, 2, 0);
        bottom.Controls.Add(linkGuide, 4, 0);
        bottom.Controls.Add(lblRouteStatus, 5, 0);

        left.Controls.Add(cities, 0, 0);
        left.Controls.Add(buttons, 0, 1);
        left.Controls.Add(listBar, 0, 2);
        left.Controls.Add(lvRoutes, 0, 3);
        left.Controls.Add(bottom, 0, 4);

        return left;
    }

    private GroupBox BuildSpecialGoodsGroup()
    {
        var group = new GroupBox
        {
            Text = "البضائع الخاصة",
            Dock = DockStyle.Fill,
            Padding = new Padding(10, 28, 10, 8),
            Margin = new Padding(4, 0, 0, 0)
        };

        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            BackColor = Color.Transparent
        };
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        table.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        checkSellGoods = MakeCheckBox("بيع البضائع الخاصة");
        checkSellGoods.CheckedChanged += checkGoods_CheckedChanged;

        checkBuyGoods = MakeCheckBox("شراء البضائع الخاصة");
        checkBuyGoods.CheckedChanged += checkGoods_CheckedChanged;

        lblGoods = MakeLabel("البضاعة:");

        numAmountGoods = MakeNum(0, 999, 0);
        numAmountGoods.ValueChanged += numAmountGoods_ValueChanged;

        lblNumGoodsDesc = MakeLabel("0 = جميع البضائع");
        lblNumGoodsDesc.ForeColor = Color.Gray;

        table.Controls.Add(checkSellGoods, 0, 0);
        table.Controls.Add(checkBuyGoods, 0, 1);
        table.Controls.Add(lblGoods, 0, 2);
        table.Controls.Add(numAmountGoods, 0, 3);
        table.Controls.Add(lblNumGoodsDesc, 0, 4);

        group.Controls.Add(table);

        return group;
    }

    private System.Windows.Forms.Panel BuildLogPanel()
    {
        var panel = new System.Windows.Forms.Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(15, 23, 42),
            Padding = new Padding(8, 4, 8, 6),
            Margin = new Padding(0)
        };

        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.Transparent
        };
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        panelLogHeader = new System.Windows.Forms.Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Margin = new Padding(0)
        };

        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));

        lblJobInfo = new System.Windows.Forms.Label
        {
            Text = "الوظيفة: غير متصل",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = Color.FromArgb(148, 163, 184),
            AutoSize = false,
            Font = new Font("Segoe UI", 8.5F)
        };

        lblTradeStatus = new System.Windows.Forms.Label
        {
            Text = "الحالة: متوقف",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = Color.FromArgb(248, 113, 113),
            AutoSize = false,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
        };

        header.Controls.Add(lblJobInfo, 0, 0);
        header.Controls.Add(lblTradeStatus, 1, 0);
        panelLogHeader.Controls.Add(header);

        panelLogButtons = new System.Windows.Forms.Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Margin = new Padding(0)
        };

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 2, 0, 2)
        };

        btnStartBot = MakeLogButton("شغل البوت", Color.FromArgb(34, 197, 94));
        btnStartBot.Click += btnStartBot_Click;

        btnStopBot = MakeLogButton("وقف البوت", Color.FromArgb(220, 38, 38));
        btnStopBot.Click += btnStopBot_Click;

        btnShowClient = MakeLogButton("إظهار الكلاينت", Color.FromArgb(37, 99, 235));
        btnShowClient.Click += btnShowClient_Click;

        btnClearLogs = MakeLogButton("مسح السجلات", Color.FromArgb(71, 85, 105));
        btnClearLogs.Click += btnClearLogs_Click;

        buttons.Controls.AddRange(new Control[]
        {
            btnStartBot, btnStopBot, btnShowClient, btnClearLogs
        });

        panelLogButtons.Controls.Add(buttons);

        panelLogBody = new System.Windows.Forms.Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Margin = new Padding(0)
        };

        rtbLog = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            BackColor = Color.FromArgb(2, 6, 23),
            ForeColor = Color.FromArgb(226, 232, 240),
            Font = new Font("Consolas", 9F),
            WordWrap = false,
            HideSelection = false,
            DetectUrls = false,
            ScrollBars = RichTextBoxScrollBars.Vertical,
            Margin = new Padding(0)
        };

        panelLogBody.Controls.Add(rtbLog);

        grid.Controls.Add(panelLogHeader, 0, 0);
        grid.Controls.Add(panelLogButtons, 0, 1);
        grid.Controls.Add(panelLogBody, 0, 2);
        panel.Controls.Add(grid);

        return panel;
    }

    private Control BuildCityCell(Label label, ComboBox combo)
    {
        var cell = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent,
            Margin = new Padding(0, 0, 6, 0)
        };
        cell.RowStyles.Add(new RowStyle(SizeType.Absolute, 18F));
        cell.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        cell.Controls.Add(label, 0, 0);
        cell.Controls.Add(combo, 0, 1);

        return cell;
    }

    private Label MakeLabel(string text, ContentAlignment align = ContentAlignment.MiddleLeft)
    {
        return new Label
        {
            Text = text,
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = align,
            ForeColor = ColorScheme.ForeColor,
            Margin = new Padding(0, 0, 4, 0),
            Font = new Font("Segoe UI", 9F)
        };
    }

    private CheckBox MakeCheckBox(string text)
    {
        return new CheckBox
        {
            Text = text,
            AutoSize = false,
            Dock = DockStyle.Fill,
            ForeColor = ColorScheme.ForeColor,
            Margin = new Padding(0, 0, 0, 1),
            Font = new Font("Segoe UI", 9F)
        };
    }

    private Radio MakeRadio(string text)
    {
        return new Radio
        {
            Text = text,
            AutoSize = false,
            Dock = DockStyle.Fill,
            ForeColor = ColorScheme.ForeColor,
            Margin = new Padding(0),
            Font = new Font("Segoe UI", 9F)
        };
    }

    private ComboBox MakeCombo()
    {
        return new ComboBox
        {
            Dock = DockStyle.Fill,
            Height = 28,
            Margin = new Padding(0, 2, 0, 2),
            Font = new Font("Segoe UI", 9F)
        };
    }

    private NumUpDown MakeNum(int min, int max, int value)
    {
        return new NumUpDown
        {
            Minimum = min,
            Maximum = max,
            Value = value,
            Dock = DockStyle.Fill,
            Height = 28,
            Margin = new Padding(0, 2, 4, 2),
            Font = new Font("Segoe UI", 9F)
        };
    }

    private Button MakeButton(string text, int width, int height)
    {
        return new Button
        {
            Text = text,
            Width = width,
            Height = height,
            Dock = DockStyle.Fill,
            Margin = new Padding(3),
            Radius = 4,
            ShadowDepth = 0,
            Font = new Font("Segoe UI", 9F)
        };
    }

    private Button MakeLogButton(string text, Color color)
    {
        return new Button
        {
            Text = text,
            Width = 118,
            Height = 28,
            Color = color,
            ForeColor = Color.White,
            Radius = 4,
            ShadowDepth = 0,
            Margin = new Padding(0, 0, 8, 0),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold)
        };
    }

    private static void AddCheckRow(TableLayoutPanel table, CheckBox check, int height)
    {
        var row = table.RowStyles.Count;
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
        table.RowCount = table.RowStyles.Count;

        check.Dock = DockStyle.Fill;
        table.Controls.Add(check, 0, row);
        table.SetColumnSpan(check, 3);
    }

    private static void AddRadioRow(TableLayoutPanel table, Radio radio)
    {
        var row = table.RowStyles.Count;
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
        table.RowCount = table.RowStyles.Count;

        radio.Dock = DockStyle.Fill;
        table.Controls.Add(radio, 0, row);
        table.SetColumnSpan(radio, 3);
    }

    private static void AddNumericRow(TableLayoutPanel table, Label label, NumUpDown num, Label unit, int height)
    {
        var row = table.RowStyles.Count;
        table.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
        table.RowCount = table.RowStyles.Count;

        label.Dock = DockStyle.Fill;
        label.TextAlign = ContentAlignment.MiddleLeft;
        label.Margin = new Padding(0, 0, 4, 0);

        num.Dock = DockStyle.Fill;
        num.Margin = new Padding(0, 2, 4, 2);

        unit.Dock = DockStyle.Fill;
        unit.TextAlign = ContentAlignment.MiddleCenter;
        unit.Margin = new Padding(0);

        table.Controls.Add(label, 0, row);
        table.Controls.Add(num, 1, row);
        table.Controls.Add(unit, 2, row);
    }

    #endregion

    #region Events and configuration

    private void SubscribeEvents()
    {
        EventManager.SubscribeEvent("OnTalkToNpc", new Action<uint>(OnTalkToNpc));
        EventManager.SubscribeEvent("OnJobScaleUpdate", new System.Action(OnUpdateJobInfo));
        EventManager.SubscribeEvent("OnJobExperienceUpdate", new System.Action(OnUpdateJobInfo));
        EventManager.SubscribeEvent("OnJobJoin", new System.Action(OnUpdateJobInfo));
        EventManager.SubscribeEvent("OnJobLeave", new System.Action(OnUpdateJobInfo));
        EventManager.SubscribeEvent("OnJobAliasUpdate", new System.Action(OnUpdateJobInfo));
        EventManager.SubscribeEvent("OnStartBot", new System.Action(OnBotStateChanged));
        EventManager.SubscribeEvent("OnStopBot", new System.Action(OnBotStateChanged));
        EventManager.SubscribeEvent("OnAddLog", new Action<string, LogLevel>(AppendLog));
    }

    private void Main_Load(object sender, EventArgs e)
    {
        ReloadView();
    }

    private void ReloadView()
    {
        if (IsDisposed)
            return;

        _loadingConfig = true;

        try
        {
            PopulateCityCombos();
            PopulateTransportCombo();
            PopulateRouteListComboBox();
            LoadSettings();
        }
        finally
        {
            _loadingConfig = false;
        }

        RefreshRoutes();
        RefreshGoods();
        UpdateUiState();
        UpdateJobInfo();
    }

    private void PopulateCityCombos()
    {
        if (comboStartCity.Items.Count == 0)
        {
            comboStartCity.Items.AddRange(new object[]
            {
                "Auto-Detect",
                "Jangan",
                "Donwhang",
                "Hotan",
                "Samarkand",
                "Constantinople"
            });
        }

        if (comboDestCity.Items.Count == 0)
        {
            comboDestCity.Items.AddRange(new object[]
            {
                "Jangan",
                "Donwhang",
                "Hotan",
                "Samarkand",
                "Constantinople"
            });
        }

        var startIndex = comboStartCity.Items.IndexOf(TradeConfig.StartCity);
        comboStartCity.SelectedIndex = startIndex >= 0 ? startIndex : 0;

        var destIndex = comboDestCity.Items.IndexOf(TradeConfig.DestinationCity);
        comboDestCity.SelectedIndex = destIndex >= 0
            ? destIndex
            : comboDestCity.Items.Count > 1 ? 1 : 0;
    }

    private void PopulateTransportCombo()
    {
        if (comboTransport.Items.Count == 0)
        {
            comboTransport.Items.Add("None - manual");
            comboTransport.Items.Add("Auto");
        }

        comboTransport.SelectedIndex =
            string.Equals(TradeConfig.TransportMode, "Auto", StringComparison.OrdinalIgnoreCase) ? 1 : 0;
    }

    private void PopulateRouteListComboBox()
    {
        comboRouteList.Items.Clear();

        foreach (var name in TradeConfig.RouteScriptList)
            comboRouteList.Items.Add(name);

        if (comboRouteList.Items.Count == 0)
            return;

        var index = TradeConfig.SelectedRouteListIndex;
        if (index < 0 || index >= comboRouteList.Items.Count)
            index = 0;

        comboRouteList.SelectedIndex = index;
        btnDeleteRouteList.Enabled = comboRouteList.SelectedIndex > 0;
    }

    private void LoadSettings()
    {
        checkAttackThiefNpc.Checked = TradeConfig.AttackThiefNpcs;
        checkAttackThiefPlayers.Checked = TradeConfig.AttackThiefPlayers;
        checkCounterAttack.Checked = TradeConfig.CounterAttack;
        checkProtectTransport.Checked = TradeConfig.ProtectTransport;
        checkMountTransport.Checked = TradeConfig.MountTransport;
        checkCastBuffs.Checked = TradeConfig.CastBuffs;
        checkWaitForHunter.Checked = TradeConfig.WaitForHunter;
        checkAutoStartAfterLogin.Checked = TradeConfig.AutoStartAfterLogin;
        checkDisableAutoRun.Checked = !TradeConfig.AutoStartAfterLogin;

        numMaxAttackDistance.Value = TradeConfig.MaxAttackDistance;
        numMaxTransportDistance.Value = TradeConfig.MaxTransportDistance;

        checkUseReturnScroll.Checked = TradeConfig.UseReturnScrollAfterLoop;
        checkSkipTownScript.Checked = TradeConfig.SkipTownScript;
        checkSmartThiefCombat.Checked = TradeConfig.SmartThiefCombat;
        checkSkipNpcIfHasGoods.Checked = TradeConfig.SkipNpcIfHasGoods;
        checkSmartObstacleEvasion.Checked = TradeConfig.SmartObstacleEvasion;

        numTradeLoops.Value = TradeConfig.TradeLoops;
        lblInfiniteLoops.Text = TradeConfig.TradeLoops == 0 ? "∞ لا نهائي" : "0 = ∞";

        checkBuyGoods.Checked = TradeConfig.BuyGoods;
        checkSellGoods.Checked = TradeConfig.SellGoods;
        numAmountGoods.Value = TradeConfig.BuyGoodsQuantity;

        // Exactly one run mode can be active. City-to-city wins when the stored
        // configuration still has the old defaults (both flags enabled).
        if (TradeConfig.UseCityToCity)
            radioCityToCity.Checked = true;
        else if (TradeConfig.TracePlayer)
            radioTracePlayer.Checked = true;
        else if (TradeConfig.UseRouteScripts)
            radioRouteList.Checked = true;
        else
            radioCityToCity.Checked = true;

        txtTracePlayerName.Text = TradeConfig.TracePlayerName;
        txtTracePlayerName.Enabled = radioTracePlayer.Checked;

        UpdateModeHint();
    }

    private void UpdateModeHint()
    {
        if (lblSettingsHint == null)
            return;

        if (radioCityToCity.Checked)
            lblSettingsHint.Text = "يتنقل البوت تلقائياً بين مدينة البداية ومدينة النهاية، ويشتري ويبيع البضائع في كل جولة.";
        else if (radioRouteList.Checked)
            lblSettingsHint.Text = "يختار البوت مساراً نشطاً من القائمة المحددة، ثم يبدأ من أقرب نقطة لموقعك الحالي.";
        else
            lblSettingsHint.Text = "يتتبع البوت اللاعب المكتوب اسمه ويبقى خلفه. تأكد من وجود اللاعب في نفس المنطقة.";
    }

    private void comboStartCity_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_loadingConfig)
            return;

        TradeConfig.StartCity = comboStartCity.SelectedItem?.ToString() ?? "Auto-Detect";

        if (Kernel.Bot?.Running != true)
            TradeConfig.CurrentTradeLoop = 0;

        UpdateUiState();
    }

    private void comboDestCity_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_loadingConfig)
            return;

        TradeConfig.DestinationCity = comboDestCity.SelectedItem?.ToString() ?? "Donwhang";

        if (Kernel.Bot?.Running != true)
            TradeConfig.CurrentTradeLoop = 0;

        UpdateUiState();
    }

    private void comboTransport_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_loadingConfig)
            return;

        TradeConfig.TransportMode = comboTransport.SelectedIndex == 1 ? "Auto" : "Manual";
    }

    private void comboRouteList_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_loadingConfig)
            return;

        TradeConfig.SelectedRouteListIndex = comboRouteList.SelectedIndex;
        btnDeleteRouteList.Enabled = comboRouteList.SelectedIndex > 0;
        RefreshRoutes();
    }

    private void checkSettings_CheckedChanged(object sender, EventArgs e)
    {
        if (_loadingConfig)
            return;

        TradeConfig.AttackThiefNpcs = checkAttackThiefNpc.Checked;
        TradeConfig.AttackThiefPlayers = checkAttackThiefPlayers.Checked;
        TradeConfig.CounterAttack = checkCounterAttack.Checked;
        TradeConfig.ProtectTransport = checkProtectTransport.Checked;
        TradeConfig.MountTransport = checkMountTransport.Checked;
        TradeConfig.CastBuffs = checkCastBuffs.Checked;
        TradeConfig.WaitForHunter = checkWaitForHunter.Checked;
        TradeConfig.UseReturnScrollAfterLoop = checkUseReturnScroll.Checked;
        TradeConfig.SkipTownScript = checkSkipTownScript.Checked;
        TradeConfig.SmartThiefCombat = checkSmartThiefCombat.Checked;
        TradeConfig.SkipNpcIfHasGoods = checkSkipNpcIfHasGoods.Checked;
        TradeConfig.SmartObstacleEvasion = checkSmartObstacleEvasion.Checked;
    }

    private void checkGoods_CheckedChanged(object sender, EventArgs e)
    {
        if (_loadingConfig)
            return;

        TradeConfig.BuyGoods = checkBuyGoods.Checked;
        TradeConfig.SellGoods = checkSellGoods.Checked;
    }

    private void checkAutoStartAfterLogin_CheckedChanged(object sender, EventArgs e)
    {
        if (_loadingConfig)
            return;

        TradeConfig.AutoStartAfterLogin = checkAutoStartAfterLogin.Checked;

        _loadingConfig = true;
        checkDisableAutoRun.Checked = !checkAutoStartAfterLogin.Checked;
        _loadingConfig = false;
    }

    private void checkDisableAutoRun_CheckedChanged(object sender, EventArgs e)
    {
        if (_loadingConfig)
            return;

        TradeConfig.AutoStartAfterLogin = !checkDisableAutoRun.Checked;

        _loadingConfig = true;
        checkAutoStartAfterLogin.Checked = !checkDisableAutoRun.Checked;
        _loadingConfig = false;
    }

    private void numSetting_ValueChanged(object sender, EventArgs e)
    {
        if (_loadingConfig)
            return;

        TradeConfig.MaxAttackDistance = (int)numMaxAttackDistance.Value;
        TradeConfig.MaxTransportDistance = (int)numMaxTransportDistance.Value;
    }

    private void numTradeLoops_ValueChanged(object sender, EventArgs e)
    {
        if (_loadingConfig)
            return;

        TradeConfig.TradeLoops = (int)numTradeLoops.Value;
        lblInfiniteLoops.Text = TradeConfig.TradeLoops == 0 ? "∞ لا نهائي" : "0 = ∞";
        UpdateLoopColumn();
        UpdateUiState();
    }

    private void numAmountGoods_ValueChanged(object sender, EventArgs e)
    {
        if (_loadingConfig)
            return;

        TradeConfig.BuyGoodsQuantity = (int)numAmountGoods.Value;
    }

    private void radioSetting_CheckedChanged(object sender, EventArgs e)
    {
        if (_loadingConfig)
            return;

        // Keep the three modes mutually exclusive even if the theme's radio
        // control does not group them automatically.
        if (sender is Radio radio && radio.Checked)
        {
            _loadingConfig = true;

            if (!ReferenceEquals(radio, radioCityToCity))
                radioCityToCity.Checked = false;

            if (!ReferenceEquals(radio, radioRouteList))
                radioRouteList.Checked = false;

            if (!ReferenceEquals(radio, radioTracePlayer))
                radioTracePlayer.Checked = false;

            _loadingConfig = false;
        }

        TradeConfig.UseCityToCity = radioCityToCity.Checked;
        TradeConfig.UseRouteScripts = radioRouteList.Checked;
        TradeConfig.TracePlayer = radioTracePlayer.Checked;

        txtTracePlayerName.Enabled = radioTracePlayer.Checked;
        UpdateModeHint();
        UpdateUiState();
    }

    private void txtTracePlayerName_TextChanged(object sender, EventArgs e)
    {
        if (_loadingConfig)
            return;

        TradeConfig.TracePlayerName = txtTracePlayerName.Text;
    }

    #endregion

    #region Routes

    private void RefreshRoutes()
    {
        if (lvRoutes == null || IsDisposed)
            return;

        lvRoutes.BeginUpdate();

        try
        {
            lvRoutes.Items.Clear();

            var selectedList = comboRouteList.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedList) ||
                !TradeConfig.RouteScripts.TryGetValue(selectedList, out var scripts))
                return;

            var index = 1;
            foreach (var file in scripts)
            {
                if (!File.Exists(file))
                    continue;

                var start = string.Empty;
                var end = string.Empty;

                // The endpoints are resolved by reading the file directly so
                // that refreshing this table never replaces the script that is
                // currently loaded in the main window.
                var endpoints = TryReadRouteEndpoints(file);
                if (endpoints.HasValue && Game.ReferenceManager != null)
                {
                    start = Game.ReferenceManager.GetTranslation(endpoints.Value.start.Region.ToString());
                    end = Game.ReferenceManager.GetTranslation(endpoints.Value.end.Region.ToString());
                }

                var active = TradeConfig.IsRouteActive(file);
                var item = new ListViewItem(index.ToString()) { Tag = file };

                item.SubItems.Add(start);
                item.SubItems.Add(end);
                item.SubItems.Add(Path.GetFileName(file));
                item.SubItems.Add(active ? "نعم" : "لا");
                item.SubItems.Add(FormatLoops(TradeConfig.TradeLoops));
                item.ForeColor = active ? lvRoutes.ForeColor : Color.Gray;

                lvRoutes.Items.Add(item);
                index++;
            }
        }
        finally
        {
            lvRoutes.EndUpdate();
        }
    }

    /// <summary>
    ///     Reads the first and the last walk point of a route script without
    ///     touching <see cref="ScriptManager" />.
    /// </summary>
    private static (Position start, Position end)? TryReadRouteEndpoints(string file)
    {
        try
        {
            var moves = new List<Position>();

            foreach (var line in File.ReadLines(file))
            {
                var trimmed = line.Trim();
                if (!trimmed.StartsWith("move", StringComparison.OrdinalIgnoreCase))
                    continue;

                var args = trimmed.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (args.Length < 6)
                    continue;

                if (!float.TryParse(args[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var xOffset) ||
                    !float.TryParse(args[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var yOffset) ||
                    !float.TryParse(args[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var zOffset) ||
                    !byte.TryParse(args[4], out var xSector) ||
                    !byte.TryParse(args[5], out var ySector))
                    continue;

                moves.Add(new Position(xSector, ySector, xOffset, yOffset, zOffset));
            }

            if (moves.Count == 0)
                return null;

            return (moves[0], moves[moves.Count - 1]);
        }
        catch
        {
            return null;
        }
    }

    private void UpdateLoopColumn()
    {
        if (lvRoutes == null)
            return;

        foreach (ListViewItem item in lvRoutes.Items)
        {
            if (item.SubItems.Count > 5)
                item.SubItems[5].Text = FormatLoops(TradeConfig.TradeLoops);
        }
    }

    private static string FormatLoops(int loops)
    {
        return loops <= 0 ? "∞" : loops.ToString();
    }

    private string GetSelectedRouteFile()
    {
        return lvRoutes?.SelectedItems.Count > 0
            ? lvRoutes.SelectedItems[0].Tag as string
            : null;
    }

    private void lvRoutes_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Right)
            return;

        var item = lvRoutes.GetItemAt(e.X, e.Y);
        if (item != null)
            item.Selected = true;
    }

    private void ToggleSelectedRoute()
    {
        var file = GetSelectedRouteFile();
        if (string.IsNullOrEmpty(file))
            return;

        var active = !TradeConfig.IsRouteActive(file);
        TradeConfig.SetRouteActive(file, active);

        if (lvRoutes.SelectedItems.Count == 0)
            return;

        var item = lvRoutes.SelectedItems[0];
        if (item.SubItems.Count > 4)
            item.SubItems[4].Text = active ? "نعم" : "لا";

        item.ForeColor = active ? lvRoutes.ForeColor : Color.Gray;
    }

    private void MoveRoute(int delta)
    {
        var selectedList = comboRouteList.SelectedItem?.ToString();
        if (string.IsNullOrEmpty(selectedList))
            return;

        var dict = TradeConfig.RouteScripts;
        if (!dict.TryGetValue(selectedList, out var routes))
            return;

        var index = lvRoutes.SelectedItems.Count > 0 ? lvRoutes.SelectedItems[0].Index : -1;
        var newIndex = index + delta;

        if (index < 0 || newIndex < 0 || newIndex >= routes.Count)
            return;

        var file = routes[index];
        routes.RemoveAt(index);
        routes.Insert(newIndex, file);

        dict[selectedList] = routes;
        TradeConfig.RouteScripts = dict;

        RefreshRoutes();

        if (newIndex < lvRoutes.Items.Count)
        {
            lvRoutes.Items[newIndex].Selected = true;
            lvRoutes.EnsureVisible(newIndex);
            lvRoutes.Focus();
        }
    }

    private void btnAddRoute_Click(object sender, EventArgs e)
    {
        var selectedList = comboRouteList.SelectedItem?.ToString();
        if (string.IsNullOrEmpty(selectedList))
        {
            MessageBox.Show("يرجى إنشاء قائمة مسارات أولاً.", "قائمة المسارات",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new OpenFileDialog
        {
            Title = "اختر ملفات مسارات التجارة",
            Filter = "RSBot Script (*.rbs)|*.rbs|كل الملفات (*.*)|*.*",
            AddExtension = true,
            CheckFileExists = true,
            Multiselect = true
        };

        var tradeDirectory = Path.Combine(ScriptManager.InitialDirectory, "Trade");
        if (Directory.Exists(tradeDirectory))
            dialog.InitialDirectory = tradeDirectory;

        var owner = FindForm();
        var result = owner != null ? dialog.ShowDialog(owner) : dialog.ShowDialog();
        if (result != DialogResult.OK)
            return;

        var dict = TradeConfig.RouteScripts;
        if (!dict.TryGetValue(selectedList, out var routes))
            return;

        var added = 0;
        foreach (var file in dialog.FileNames)
        {
            if (routes.Any(r => string.Equals(r, file, StringComparison.OrdinalIgnoreCase)))
                continue;

            routes.Add(file);
            added++;
        }

        if (added == 0)
            return;

        dict[selectedList] = routes;
        TradeConfig.RouteScripts = dict;

        RefreshRoutes();
        Log.Notify($"[Trade] تمت إضافة {added} مسار إلى القائمة {selectedList}.");
    }

    private void btnNewRouteList_Click(object sender, EventArgs e)
    {
        var dialog = new InputDialog("قائمة مسارات جديدة", "قائمة مسارات جديدة",
            "أدخل اسم القائمة الجديدة");

        var owner = FindForm();
        var result = owner != null ? dialog.ShowDialog(owner) : dialog.ShowDialog();
        if (result != DialogResult.OK)
            return;

        var name = (dialog.Value as string ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(name))
            return;

        if (name.Contains(';'))
        {
            MessageBox.Show("لا يمكن استخدام الفاصلة المنقوطة (;) في اسم القائمة.", "اسم غير صالح",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (TradeConfig.RouteScriptList.Any(n => string.Equals(n, name, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show("توجد قائمة مسارات بهذا الاسم بالفعل.", "اسم مكرر",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var lists = TradeConfig.RouteScriptList;
        lists.Add(name);
        TradeConfig.RouteScriptList = lists;

        PopulateRouteListComboBox();
        comboRouteList.SelectedIndex = comboRouteList.Items.Count - 1;
        RefreshRoutes();
    }

    private void btnDeleteRouteList_Click(object sender, EventArgs e)
    {
        // The default list can never be deleted.
        if (comboRouteList.SelectedIndex <= 0)
            return;

        var name = comboRouteList.SelectedItem?.ToString();
        if (string.IsNullOrEmpty(name))
            return;

        if (MessageBox.Show($"هل تريد حذف قائمة المسارات [{name}]؟", "حذف القائمة",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        TradeConfig.RemoveRouteList(name);

        PopulateRouteListComboBox();
        comboRouteList.SelectedIndex = 0;
        RefreshRoutes();
    }

    private void menuToggleRoute_Click(object sender, EventArgs e)
    {
        ToggleSelectedRoute();
    }

    private void menuEditLoops_Click(object sender, EventArgs e)
    {
        var dialog = new InputDialog("عدد الحلقات", "عدد حلقات التجارة",
            "أدخل عدد الحلقات المطلوبة. القيمة 0 تعني تكراراً بلا نهاية.",
            InputDialog.InputType.Numeric, (decimal)TradeConfig.TradeLoops);

        dialog.Numeric.Minimum = 0;
        dialog.Numeric.Maximum = 999;
        dialog.Numeric.Value = TradeConfig.TradeLoops;

        var owner = FindForm();
        var result = owner != null ? dialog.ShowDialog(owner) : dialog.ShowDialog();
        if (result != DialogResult.OK)
            return;

        var loops = (int)(decimal)dialog.Value;
        TradeConfig.TradeLoops = loops;

        _loadingConfig = true;
        numTradeLoops.Value = loops;
        _loadingConfig = false;

        lblInfiniteLoops.Text = loops == 0 ? "∞ لا نهائي" : "0 = ∞";
        UpdateLoopColumn();
        UpdateUiState();
    }

    private void menuRemoveRoute_Click(object sender, EventArgs e)
    {
        var file = GetSelectedRouteFile();
        var selectedList = comboRouteList.SelectedItem?.ToString();

        if (string.IsNullOrEmpty(file) || string.IsNullOrEmpty(selectedList))
            return;

        var dict = TradeConfig.RouteScripts;
        if (!dict.TryGetValue(selectedList, out var routes))
            return;

        routes.Remove(file);
        dict[selectedList] = routes;
        TradeConfig.RouteScripts = dict;

        RefreshRoutes();
    }

    #endregion

    #region Goods

    private void RefreshGoods()
    {
        if (lvGoods == null || IsDisposed)
            return;

        lvGoods.BeginUpdate();

        try
        {
            lvGoods.Items.Clear();

            var transport = Game.Player?.JobTransport;
            if (transport == null)
            {
                lblGoodsStatus.Text = "وسيلة النقل: غير مستدعاة";
                lblGoodsStatus.ForeColor = Color.Gray;
                return;
            }

            var items = transport.Inventory?
                .Where(i => i != null && i.Record != null)
                .ToList() ?? new List<InventoryItem>();

            foreach (var item in items)
            {
                var name = item.Record.GetRealName();
                if (string.IsNullOrEmpty(name))
                    name = item.Record.CodeName;

                var listItem = new ListViewItem(name);
                listItem.SubItems.Add(item.Amount.ToString());
                listItem.SubItems.Add(item.Record.MaxStack > 0 ? item.Record.MaxStack.ToString() : "-");

                lvGoods.Items.Add(listItem);
            }

            var loaded = items.Count(i => i.Amount > 0);
            lblGoodsStatus.Text = loaded > 0
                ? $"وسيلة النقل: محمّلة ({loaded} نوع)"
                : "وسيلة النقل: فارغة";
            lblGoodsStatus.ForeColor = loaded > 0
                ? Color.FromArgb(74, 222, 128)
                : Color.Gray;
        }
        finally
        {
            lvGoods.EndUpdate();
        }
    }

    #endregion

    #region Start / stop

    private void btnStartTrade_Click(object sender, EventArgs e)
    {
        StartTradeBot();
    }

    private void btnStartBot_Click(object sender, EventArgs e)
    {
        StartTradeBot();
    }

    private void btnStopTrade_Click(object sender, EventArgs e)
    {
        StopTradeBot();
    }

    private void btnStopBot_Click(object sender, EventArgs e)
    {
        StopTradeBot();
    }

    private void StartTradeBot()
    {
        if (Kernel.Bot == null)
            return;

        if (!Game.Ready || Game.Player == null)
        {
            MessageBox.Show("يرجى تسجيل الدخول إلى اللعبة أولاً.", "التداول",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (Kernel.Bot.Running)
            return;

        var tradeBotbase = Kernel.BotbaseManager?.Bots?
            .FirstOrDefault(b => b.Value != null && b.Value.Name == "RSBot.Trade").Value;

        if (tradeBotbase == null)
        {
            MessageBox.Show("لم يتم العثور على بوت التداول.", "التداول",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (!ReferenceEquals(Kernel.Bot.Botbase, tradeBotbase))
            Kernel.Bot.SetBotbase(tradeBotbase);

        // A manual start always begins a fresh loop counter.
        TradeConfig.CurrentTradeLoop = 0;

        if (radioCityToCity.Checked)
        {
            TradeConfig.UseCityToCity = true;
            TradeConfig.UseRouteScripts = false;
            TradeConfig.TracePlayer = false;

            RouteBundle.InitializeCityPair();

            var routeFile = RouteBundle.GetCityToCityRouteFile();
            if (string.IsNullOrEmpty(routeFile) || !File.Exists(routeFile))
            {
                MessageBox.Show(
                    $"لم يتم العثور على مسار صالح بين:\n{TradeConfig.StartCity} ← {TradeConfig.DestinationCity}\n\n" +
                    "تأكد من اختيار المدن بشكل صحيح أو أضف ملف مسار في مجلد Build\\Data\\Scripts\\Trade.",
                    "مسار التجارة", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        else if (radioRouteList.Checked)
        {
            TradeConfig.UseCityToCity = false;
            TradeConfig.UseRouteScripts = true;
            TradeConfig.TracePlayer = false;

            var selectedList = comboRouteList.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedList) ||
                !TradeConfig.RouteScripts.TryGetValue(selectedList, out var routes) ||
                !routes.Any(TradeConfig.IsRouteActive))
            {
                MessageBox.Show("قائمة المسارات المحددة لا تحتوي على أي مسار نشط.", "قائمة المسارات",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }
        else
        {
            TradeConfig.UseCityToCity = false;
            TradeConfig.UseRouteScripts = false;
            TradeConfig.TracePlayer = true;

            if (string.IsNullOrWhiteSpace(TradeConfig.TracePlayerName))
            {
                MessageBox.Show("يرجى إدخال اسم اللاعب المراد تتبعه.", "تتبع لاعب",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
        }

        Log.Notify($"[Trade] تشغيل البوت — الوضع: {GetModeName()} | الحلقات: {FormatLoops(TradeConfig.TradeLoops)}");

        Kernel.Bot.Start();
        UpdateUiState();
    }

    private void StopTradeBot()
    {
        btnStartBot.Enabled = true;
        btnStopBot.Enabled = false;
        btnStartTrade.Enabled = true;
        btnStopTrade.Enabled = false;

        lblTradeStatus.Text = "الحالة: جاري الإيقاف...";
        lblTradeStatus.ForeColor = Color.FromArgb(251, 191, 36);

        Task.Run(() =>
        {
            try
            {
                ScriptManager.Stop(error: true);
            }
            catch
            {
                // ignored
            }

            try
            {
                Kernel.Bot?.Stop();
            }
            catch
            {
                // ignored
            }

            try
            {
                Bundles.Stop();
            }
            catch
            {
                // ignored
            }

            try
            {
                Bundles.RouteBundle.CurrentRouteFile = null;
            }
            catch
            {
                // ignored
            }

            try
            {
                if (Game.Ready && Game.Player != null)
                    Game.Player.MoveTo(Game.Player.Position, false);
            }
            catch
            {
                // ignored
            }

            Log.Notify("[Trade] تم إيقاف التداول والبوت بنجاح.");
            Log.Status("Ready");
        });
    }

    private string GetModeName()
    {
        if (radioCityToCity.Checked)
            return "مدينة → مدينة";

        return radioRouteList.Checked ? "قائمة المسارات" : "تتبع لاعب";
    }

    private void btnShowClient_Click(object sender, EventArgs e)
    {
        try
        {
            ClientManager.Show();
        }
        catch (Exception ex)
        {
            Log.Warn($"[Trade] تعذر إظهار الكلاينت: {ex.Message}");
        }
    }

    private void btnClearLogs_Click(object sender, EventArgs e)
    {
        rtbLog?.Clear();
    }

    #endregion

    #region Status / log

    private void UiTimer_Tick(object sender, EventArgs e)
    {
        if (IsDisposed || !Visible)
            return;

        UpdateUiState();
        UpdateJobInfo();

        if (tabControlMain?.SelectedTab == tabPageGoods)
            RefreshGoods();
    }

    private void OnBotStateChanged()
    {
        if (IsDisposed || !IsHandleCreated)
            return;

        if (InvokeRequired)
        {
            try
            {
                BeginInvoke(new System.Action(OnBotStateChanged));
            }
            catch
            {
                // The control may be disposed while the bot is stopping.
            }

            return;
        }

        UpdateUiState();
    }

    private void UpdateUiState()
    {
        if (IsDisposed || btnStartBot == null)
            return;

        var running = Kernel.Bot?.Running == true;

        btnStartBot.Enabled = !running;
        btnStopBot.Enabled = running;
        btnStartTrade.Enabled = !running;
        btnStopTrade.Enabled = running;

        lblTradeStatus.Text = running ? "الحالة: يعمل" : "الحالة: متوقف";
        lblTradeStatus.ForeColor = running
            ? Color.FromArgb(74, 222, 128)
            : Color.FromArgb(248, 113, 113);

        UpdateRouteStatus();
    }

    private void UpdateRouteStatus()
    {
        if (lblRouteStatus == null)
            return;

        var loops = TradeConfig.TradeLoops <= 0 ? "لا نهائي" : TradeConfig.TradeLoops.ToString();
        var current = TradeConfig.CurrentTradeLoop + 1;

        if (TradeConfig.TradeLoops > 0 && current > TradeConfig.TradeLoops)
            current = TradeConfig.TradeLoops;

        var routeFile = Bundles.RouteBundle.CurrentRouteFile;
        var routeName = string.IsNullOrEmpty(routeFile)
            ? string.Empty
            : $" | {Path.GetFileNameWithoutExtension(routeFile)}";

        lblRouteStatus.Text = $"{GetModeName()} | الجولة {current} من {loops}{routeName}";
    }

    private void OnUpdateJobInfo()
    {
        if (IsDisposed || !IsHandleCreated)
            return;

        if (InvokeRequired)
        {
            try
            {
                BeginInvoke(new System.Action(OnUpdateJobInfo));
            }
            catch
            {
                // ignored
            }

            return;
        }

        UpdateJobInfo();
    }

    private void UpdateJobInfo()
    {
        if (lblJobInfo == null)
            return;

        if (!Game.Ready || Game.Player == null)
        {
            lblJobInfo.Text = "الوظيفة: غير متصل";
            return;
        }

        var job = Game.Player.JobInformation;
        if (job == null || job.Type == JobType.None)
        {
            lblJobInfo.Text = "الوظيفة: لا يوجد";
            return;
        }

        var name = string.IsNullOrEmpty(job.Name) ? job.Type.ToString() : job.Name;
        lblJobInfo.Text = $"الوظيفة: {name} | المستوى: {job.Level} | الخبرة: {job.Experience:N0}";
    }

    private void AppendLog(string message, LogLevel level)
    {
        if (IsDisposed || !IsHandleCreated || rtbLog == null)
            return;

        if (InvokeRequired)
        {
            try
            {
                BeginInvoke(new Action<string, LogLevel>(AppendLog), message, level);
            }
            catch
            {
                // ignored
            }

            return;
        }

        try
        {
            var color = level switch
            {
                LogLevel.Error or LogLevel.Fatal => Color.FromArgb(248, 113, 113),
                LogLevel.Warning => Color.FromArgb(251, 191, 36),
                LogLevel.Debug => Color.FromArgb(148, 163, 184),
                _ => Color.FromArgb(226, 232, 240)
            };

            // Keep the console bounded so a long session cannot eat memory.
            if (rtbLog.TextLength > 120_000)
                rtbLog.Clear();

            rtbLog.SelectionStart = rtbLog.TextLength;
            rtbLog.SelectionLength = 0;
            rtbLog.SelectionColor = color;
            rtbLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
            rtbLog.SelectionColor = rtbLog.ForeColor;
            rtbLog.ScrollToCaret();
        }
        catch
        {
            // Logging must never crash the page.
        }
    }

    private void OnTalkToNpc(uint uniqueId)
    {
        if (Game.Player.State.DialogState is not { IsInDialog: true, TalkOption: TalkOption.Trade })
            return;

        EventManager.FireEvent("AppendScriptCommand",
            $"buy-goods {Game.Player.State.DialogState.Npc.Record.CodeName}");
    }

    private void linkGuide_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        const string guide =
            "دليل التشغيل السريع:\n\n" +
            "1) اختر مدينة البداية ومدينة النهاية من صفحة الطريق.\n" +
            "2) اختر وسيلة النقل: None - manual للاستدعاء اليدوي، أو Auto للاستدعاء التلقائي.\n" +
            "3) أضف مساراتك إلى القائمة باستخدام زر إضافة، أو استخدم المسارات الجاهزة.\n" +
            "4) اضبط عدد الحلقات في الإعدادات. القيمة 0 تعني تكراراً بلا نهاية.\n" +
            "5) اضغط ابدأ، ثم شغل البوت من الشريط السفلي.\n\n" +
            "ملاحظة: يجب أن يكون الحساب تاجراً ويرتدي زي التجارة.";

        MessageBox.Show(guide, "دليل التشغيل", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    #endregion

    public void ApplyLanguage()
    {
        bool ar = Kernel.Language == "ar_AR";

        tabPageSettings.Text = ar ? "الإعدادات" : "Settings";
        tabPageGoods.Text = ar ? "البضائع" : "Special Goods";
        tabPageRoute.Text = ar ? "الطريق" : "Route";

        groupSettings.Text = ar ? "إعدادات التجارة" : "Trade Settings";
        groupLoopOptions.Text = ar ? "خيارات التشغيل" : "Loop Options";

        checkAttackThiefNpc.Text = ar ? "مهاجمة لصوص NPC" : "Attack NPC Thieves";
        checkAttackThiefPlayers.Text = ar ? "مهاجمة لصوص اللاعبين" : "Attack Thief Players";
        checkCounterAttack.Text = ar ? "الهجوم المضاد" : "Counter Attack";
        checkProtectTransport.Text = ar ? "حماية وسيلة النقل" : "Protect Transport";
        checkMountTransport.Text = ar ? "امتطاء وسيلة النقل" : "Mount Transport";
        checkCastBuffs.Text = ar ? "تفعيل البفات" : "Cast Buffs";
        checkWaitForHunter.Text = ar ? "انتظار الصياد" : "Wait for Hunter nearby";
        checkAutoStartAfterLogin.Text = ar ? "بدء تلقائي بعد تسجيل الدخول" : "Start Trade after login";

        lblMaxAttackDistance.Text = ar ? "أقصى مسافة هجوم:" : "Max Attack Distance:";
        lblMaxAttackUnit.Text = ar ? "م" : "m";
        lblMaxTransportDistance.Text = ar ? "أقصى مسافة نقل:" : "Max Transport Distance:";
        lblMaxTransportUnit.Text = ar ? "م" : "m";

        lblTradeLoops.Text = ar ? "تكرار جولات التجارة:" : "Repeat Trade Cycle:";
        lblInfiniteLoops.Text = ar ? "0 = ∞ لا نهائي" : "0 = ∞ infinite";

        checkUseReturnScroll.Text = ar ? "استخدام سكرول العودة بعد الجولة" : "Use Return Scroll after loop";
        checkSkipTownScript.Text = ar ? "تخطي سكريبت المدينة" : "Skip town scripts";
        checkSmartThiefCombat.Text = ar ? "قتال اللصوص الذكي" : "Smart Thief Combat";
        checkSkipNpcIfHasGoods.Text = ar ? "تخطي NPC إذا كانت البضاعة محمّلة" : "Skip NPC if already loaded";
        checkSmartObstacleEvasion.Text = ar ? "تجنب العوائق الذكي" : "Smart Obstacle Evasion";

        lblRunMode.Text = ar ? "وضع التشغيل:" : "Run Mode:";
        radioCityToCity.Text = ar ? "من مدينة إلى أخرى" : "City to City";
        radioRouteList.Text = ar ? "قائمة مسارات مخصصة" : "Custom Route List";
        radioTracePlayer.Text = ar ? "تتبع لاعب" : "Trace Player";

        lblTracePlayerName.Text = ar ? "اسم اللاعب:" : "Player Name:";

        checkDisableAutoRun.Text = ar ? "تعطيل التشغيل التلقائي" : "Disable auto route activation";
        linkGuide.Text = ar ? "دليل التجارة" : "Trade Guide (?)";

        colIndex.Text = "#";
        colStart.Text = ar ? "بدء" : "Start";
        colEnd.Text = ar ? "نهاية" : "End";
        colScript.Text = ar ? "السكريبت" : "Script";
        colActive.Text = ar ? "نشط" : "Active";
        colLoop.Text = ar ? "حلقة" : "Loop";

        btnAddRoute.Text = ar ? "إضافة مسار" : "Add Route";
        btnStartTrade.Text = ar ? "بدء التجارة" : "Start Trade";
        btnStopTrade.Text = ar ? "إيقاف التجارة" : "Stop Trade";

        checkSellGoods.Text = ar ? "بيع البضائع" : "Sell goods";
        checkBuyGoods.Text = ar ? "شراء البضائع" : "Buy Special Goods";
        lblGoods.Text = ar ? "الكمية:" : "Qty:";
        lblNumGoodsDesc.Text = ar ? "0 = أقصى حد ممكن" : "0 = max. possible";

        colGoodsName.Text = ar ? "اسم البضاعة" : "Goods Name";
        colGoodsAmount.Text = ar ? "الكمية" : "Qty";
        colGoodsMax.Text = ar ? "أقصى كمية" : "Max Qty";

        btnStartBot.Text = ar ? "شغل البوت" : "Start Bot";
        btnStopBot.Text = ar ? "وقف البوت" : "Stop Bot";
        btnShowClient.Text = ar ? "إظهار الكلاينت" : "Show Client";
        btnClearLogs.Text = ar ? "مسح السجلات" : "Clear Logs";

        menuToggleRoute.Text = ar ? "تفعيل / تعطيل المسار" : "Toggle Active";
        menuEditLoops.Text = ar ? "تعديل عدد الحلقات" : "Edit Loop Count";
        menuRemoveRoute.Text = ar ? "حذف المسار" : "Remove Route";
    }

}
