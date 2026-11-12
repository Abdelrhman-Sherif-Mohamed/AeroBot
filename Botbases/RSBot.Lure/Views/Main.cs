using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Objects;
using RSBot.Lure.Components;
using SDUI;
using SDUI.Controls;

namespace RSBot.Lure.Views;

[ToolboxItem(false)]
public partial class Main : DoubleBufferedControl
{
    private bool _configLocked;
    private bool _updatingLayout;
    private bool _runnerSubscribed;
    private TableLayoutPanel _rootLayout;
    private TableLayoutPanel _lureControlBar;
    private TableLayoutPanel _leftColumn;
    private System.Windows.Forms.Label _lblLureStatus;
    private SDUI.Controls.Button _btnStartLure;
    private SDUI.Controls.Button _btnStopLure;
    private TableLayoutPanel _partyGrid;
    private TableLayoutPanel _rowPartyCoords;
    private TableLayoutPanel _rowPartyWait;
    private TableLayoutPanel _rowWalkscript;
    private TableLayoutPanel _spotsGrid;
    private TableLayoutPanel _rowSpotActions;
    private TableLayoutPanel _settingsGrid;
    private TableLayoutPanel _rowStopDead;
    private TableLayoutPanel _rowStopParty;
    private TableLayoutPanel _rowMonsterType;

    public Main()
    {
        InitializeComponent();
        lstLureSpots.ListViewItemSorter = null;
        lstLureSpots.SizeChanged += (_, _) => AdjustSpotColumns();
        if (comboMonsterType.Items.Count > 0 && comboMonsterType.SelectedIndex < 0)
            comboMonsterType.SelectedIndex = 0;

        BuildResponsiveLayout();
        ApplyLocalization();
        ApplyTheme();
        UpdateLayoutMetrics();
        SubscribeEvents();
        RefreshUiState();
        Disposed += (_, _) =>
        {
            if (_runnerSubscribed && LureBotbase.Runner != null)
                LureBotbase.Runner.StateChanged -= OnRunnerStateChanged;
        };
    }

    private void SubscribeEvents()
    {
        EventManager.SubscribeEvent("OnLoadCharacter", OnLoadCharacter);
        EventManager.SubscribeEvent("OnChangeTheme", new Action<bool>(_ => RunOnUi(ApplyTheme)));
        EnsureRunnerSubscription();
    }

    private void EnsureRunnerSubscription()
    {
        if (_runnerSubscribed)
            return;

        var runner = LureBotbase.EnsureRunner();
        if (runner == null)
            return;

        runner.StateChanged += OnRunnerStateChanged;
        _runnerSubscribed = true;
    }

    private void OnRunnerStateChanged()
    {
        RunOnUi(RefreshUiState);
    }

    private void RunOnUi(System.Action action)
    {
        if (IsDisposed || Disposing)
            return;

        if (InvokeRequired)
        {
            try
            {
                BeginInvoke(new System.Action(() =>
                {
                    if (!IsDisposed && !Disposing)
                        action();
                }));
            }
            catch
            {
            }
            return;
        }

        action();
    }

    private void OnLoadCharacter()
    {
        if (IsDisposed || Disposing || !Game.Ready)
            return;

        checkUseHowlingShout.Enabled = Game.Player != null && Game.Player.Race == ObjectCountry.Europe;
        checkNoHowlingAtCenter.Enabled = Game.Player != null && Game.Player.Race == ObjectCountry.Europe;

        EnsureRunnerSubscription();
        LoadConfig();
        RefreshUiState();
    }

    private void LoadConfig()
    {
        if (_configLocked)
            return;

        _configLocked = true;

        bool isAr = Kernel.Language == "ar_AR";
        ApplyLanguage(isAr);

        lblPartyX.Text = LureConfig.Area.Position.X.ToString("0.0");
        lblPartyY.Text = LureConfig.Area.Position.Y.ToString("0.0");
        numPartyRadius.Value = Math.Max(5, LureConfig.Area.Radius > 0 ? LureConfig.Area.Radius : 50);
        numPartyWaitSeconds.Value = Math.Max(1, LureConfig.StayAtCenterForSeconds);
        txtWalkbackScript.Text = LureConfig.WalkscriptPath;

        checkWalkRandomly.Checked = LureConfig.WalkRandomly;

        checkUseHowlingShout.Checked = LureConfig.UseHowlingShout;
        checkNoHowlingAtCenter.Checked = LureConfig.NoHowlingAtCenter;
        checkUseNormalAttack.Checked = LureConfig.UseNormalAttack;
        checkUseAttackingSkills.Checked = LureConfig.UseAttackingSkills;

        checkStopPartyMemberDead.Checked = LureConfig.StopIfNumPartyMemberDead;
        numPartyMemberDead.Value = Math.Max(1, LureConfig.NumPartyMemberDead);

        checkStopPartyMember.Checked = LureConfig.StopIfNumPartyMember;
        numPartyMember.Value = Math.Max(1, LureConfig.NumPartyMember);

        checkStopMonsterType.Checked = LureConfig.StopIfNumMonsterType;
        numMonsterType.Value = Math.Max(0, LureConfig.NumMonsterType);

        comboMonsterType.SelectedIndex = LureConfig.SelectedMonsterType switch
        {
            MonsterRarity.General => 0,
            MonsterRarity.Champion => 1,
            MonsterRarity.Giant => 2,
            MonsterRarity.GeneralParty => 3,
            MonsterRarity.ChampionParty => 4,
            MonsterRarity.GiantParty => 5,
            MonsterRarity.Elite => 6,
            MonsterRarity.EliteStrong => 7,
            MonsterRarity.Unique => 8,
            MonsterRarity.Event => 9,
            _ => 0
        };

        RefreshSpotsList();

        _configLocked = false;
    }

    public void ApplyLocalization()
    {
        ApplyLanguage(Kernel.Language == "ar_AR");
    }

    public void ApplyTheme()
    {
        var back = ColorScheme.BackColor;
        var fore = ColorScheme.ForeColor;
        var card = ColorScheme.CardColor;
        var inputBack = ColorScheme.InputBackColor;
        var tableBack = ColorScheme.TableBackColor;

        BackColor = back;
        ForeColor = fore;

        foreach (Control gb in new Control[] { groupBoxParty, groupBoxLureSpots, groupBox3 })
        {
            gb.BackColor = Color.Transparent;
            gb.ForeColor = fore;
            gb.Invalidate();
        }

        txtWalkbackScript.BackColor = inputBack;
        txtWalkbackScript.ForeColor = fore;
        txtWalkbackScript.Invalidate();

        comboMonsterType.BackColor = inputBack;
        comboMonsterType.ForeColor = fore;
        comboMonsterType.Invalidate();

        lstLureSpots.BackColor = tableBack;
        lstLureSpots.ForeColor = fore;
        lstLureSpots.GridLines = !ColorScheme.BackColor.IsDark();
        lstLureSpots.Invalidate(true);

        foreach (Control num in new Control[] { numPartyRadius, numPartyWaitSeconds, numSpotRadius, numPartyMemberDead, numPartyMember, numMonsterType })
        {
            num.BackColor = card;
            num.ForeColor = fore;
            num.Invalidate();
        }

        foreach (Control lbl in new Control[] { lblPartyXTitle, lblPartyX, lblPartyYTitle, lblPartyY, lblPartyRadiusTitle, lblPartyWaitTitle, lblWalkscriptTitle, lblSpotRadiusTitle, label6 })
        {
            lbl.BackColor = Color.Transparent;
            lbl.ForeColor = fore;
            lbl.Invalidate();
        }

        foreach (Control chk in new Control[] { checkWalkRandomly, checkUseHowlingShout, checkNoHowlingAtCenter, checkUseNormalAttack, checkUseAttackingSkills, checkStopPartyMemberDead, checkStopPartyMember, checkStopMonsterType })
        {
            chk.BackColor = Color.Transparent;
            chk.ForeColor = fore;
            chk.Invalidate();
        }

        foreach (SDUI.Controls.Button btn in new[] { btnSetPartySpot, btnBrowseWalkscript, btnAddLureSpot, btnDeleteLureSpot, btnClearLureSpots })
        {
            btn.BackColor = Color.Transparent;
            btn.Color = Color.Transparent;
            btn.ForeColor = fore;
            btn.Invalidate();
        }

        ApplyLureControlBarTheme();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        UpdateLayoutMetrics();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        UpdateLayoutMetrics();
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);
        if (!Visible)
            return;

        EnsureRunnerSubscription();
        ApplyLocalization();
        ApplyTheme();
        RefreshUiState();
        UpdateLayoutMetrics();
    }

    protected override void OnDpiChangedAfterParent(EventArgs e)
    {
        base.OnDpiChangedAfterParent(e);
        UpdateLayoutMetrics();
    }

    private int S(int px)
    {
        var dpi = DeviceDpi > 0 ? DeviceDpi : 96;
        return Math.Max(1, (int)Math.Round(px * (dpi / 96f)));
    }

    private static TableLayoutPanel CreateGrid(int columns, int rows)
    {
        var grid = new TableLayoutPanel
        {
            ColumnCount = columns,
            RowCount = rows,
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            GrowStyle = TableLayoutPanelGrowStyle.FixedSize
        };

        for (int i = 0; i < columns; i++)
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columns));

        for (int i = 0; i < rows; i++)
            grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        return grid;
    }

    private static void ConfigureFillControl(Control control, ContentAlignment? labelAlign = null)
    {
        control.AutoSize = false;
        control.Dock = DockStyle.Fill;
        control.Margin = Padding.Empty;

        if (labelAlign.HasValue && control is System.Windows.Forms.Label lbl)
            lbl.TextAlign = labelAlign.Value;
        else if (control is System.Windows.Forms.CheckBox chk)
            chk.UseVisualStyleBackColor = false;
    }

    private static void ConfigureAnchoredInput(Control control)
    {
        control.AutoSize = false;
        control.Dock = DockStyle.None;
        control.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        control.Margin = Padding.Empty;
    }

    private void BuildResponsiveLayout()
    {
        SuspendLayout();

        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScroll = true;

        _rootLayout = CreateGrid(2, 2);
        _rootLayout.Dock = DockStyle.None;
        _rootLayout.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 50f);
        _rootLayout.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, 50f);
        _rootLayout.RowStyles[0] = new RowStyle(SizeType.Absolute, S(38));
        _rootLayout.RowStyles[1] = new RowStyle(SizeType.Percent, 100f);

        BuildLureControlBar();

        _leftColumn = CreateGrid(1, 2);
        _leftColumn.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 100f);
        _leftColumn.RowStyles[0] = new RowStyle(SizeType.Absolute, S(210));
        _leftColumn.RowStyles[1] = new RowStyle(SizeType.Percent, 100f);

        BuildPartyGroup();
        BuildLureSpotsGroup();
        BuildSettingsGroup();

        groupBoxParty.Dock = DockStyle.Fill;
        groupBoxLureSpots.Dock = DockStyle.Fill;
        groupBox3.Dock = DockStyle.Fill;

        _leftColumn.Controls.Add(groupBoxParty, 0, 0);
        _leftColumn.Controls.Add(groupBoxLureSpots, 0, 1);

        Controls.Clear();
        _rootLayout.Controls.Add(_lureControlBar, 0, 0);
        _rootLayout.SetColumnSpan(_lureControlBar, 2);
        _rootLayout.Controls.Add(_leftColumn, 0, 1);
        _rootLayout.Controls.Add(groupBox3, 1, 1);
        Controls.Add(_rootLayout);

        ResumeLayout(false);
    }

    private void BuildLureControlBar()
    {
        _lureControlBar = CreateGrid(3, 1);
        _lureControlBar.RowStyles[0] = new RowStyle(SizeType.Percent, 100f);

        _lblLureStatus = new System.Windows.Forms.Label
        {
            BackColor = Color.Transparent,
            Font = new Font(Font, FontStyle.Bold)
        };
        ConfigureFillControl(_lblLureStatus, ContentAlignment.MiddleLeft);

        _btnStartLure = new SDUI.Controls.Button
        {
            ShadowDepth = 0,
            Radius = 6,
            Cursor = Cursors.Hand
        };
        ConfigureFillControl(_btnStartLure);
        _btnStartLure.Click += BtnStartLure_Click;

        _btnStopLure = new SDUI.Controls.Button
        {
            ShadowDepth = 0,
            Radius = 6,
            Cursor = Cursors.Hand
        };
        ConfigureFillControl(_btnStopLure);
        _btnStopLure.Click += BtnStopLure_Click;

        _lureControlBar.Controls.Add(_lblLureStatus, 0, 0);
        _lureControlBar.Controls.Add(_btnStartLure, 1, 0);
        _lureControlBar.Controls.Add(_btnStopLure, 2, 0);
    }

    private void BuildPartyGroup()
    {
        groupBoxParty.Controls.Clear();
        _partyGrid = CreateGrid(1, 5);

        _rowPartyCoords = CreateGrid(6, 1);
        _rowPartyCoords.RowStyles[0] = new RowStyle(SizeType.Percent, 100f);
        ConfigureFillControl(lblPartyXTitle, ContentAlignment.MiddleLeft);
        ConfigureFillControl(lblPartyX, ContentAlignment.MiddleLeft);
        ConfigureFillControl(lblPartyYTitle, ContentAlignment.MiddleLeft);
        ConfigureFillControl(lblPartyY, ContentAlignment.MiddleLeft);
        ConfigureFillControl(lblPartyRadiusTitle, ContentAlignment.MiddleRight);
        ConfigureAnchoredInput(numPartyRadius);
        _rowPartyCoords.Controls.Add(lblPartyXTitle, 0, 0);
        _rowPartyCoords.Controls.Add(lblPartyX, 1, 0);
        _rowPartyCoords.Controls.Add(lblPartyYTitle, 2, 0);
        _rowPartyCoords.Controls.Add(lblPartyY, 3, 0);
        _rowPartyCoords.Controls.Add(lblPartyRadiusTitle, 4, 0);
        _rowPartyCoords.Controls.Add(numPartyRadius, 5, 0);

        ConfigureFillControl(btnSetPartySpot);

        _rowPartyWait = CreateGrid(2, 1);
        _rowPartyWait.RowStyles[0] = new RowStyle(SizeType.Percent, 100f);
        ConfigureFillControl(lblPartyWaitTitle, ContentAlignment.MiddleLeft);
        ConfigureAnchoredInput(numPartyWaitSeconds);
        _rowPartyWait.Controls.Add(lblPartyWaitTitle, 0, 0);
        _rowPartyWait.Controls.Add(numPartyWaitSeconds, 1, 0);

        ConfigureFillControl(lblWalkscriptTitle, ContentAlignment.BottomLeft);

        _rowWalkscript = CreateGrid(2, 1);
        _rowWalkscript.RowStyles[0] = new RowStyle(SizeType.Percent, 100f);
        ConfigureAnchoredInput(txtWalkbackScript);
        ConfigureFillControl(btnBrowseWalkscript);
        _rowWalkscript.Controls.Add(txtWalkbackScript, 0, 0);
        _rowWalkscript.Controls.Add(btnBrowseWalkscript, 1, 0);

        _partyGrid.Controls.Add(_rowPartyCoords, 0, 0);
        _partyGrid.Controls.Add(btnSetPartySpot, 0, 1);
        _partyGrid.Controls.Add(_rowPartyWait, 0, 2);
        _partyGrid.Controls.Add(lblWalkscriptTitle, 0, 3);
        _partyGrid.Controls.Add(_rowWalkscript, 0, 4);

        groupBoxParty.Controls.Add(_partyGrid);
    }

    private void BuildLureSpotsGroup()
    {
        groupBoxLureSpots.Controls.Clear();
        _spotsGrid = CreateGrid(1, 4);

        ConfigureFillControl(btnAddLureSpot);
        lstLureSpots.Dock = DockStyle.Fill;
        lstLureSpots.Margin = Padding.Empty;

        _rowSpotActions = CreateGrid(5, 1);
        _rowSpotActions.RowStyles[0] = new RowStyle(SizeType.Percent, 100f);
        ConfigureFillControl(lblSpotRadiusTitle, ContentAlignment.MiddleLeft);
        ConfigureAnchoredInput(numSpotRadius);
        ConfigureFillControl(btnDeleteLureSpot);
        ConfigureFillControl(btnClearLureSpots);
        _rowSpotActions.Controls.Add(lblSpotRadiusTitle, 0, 0);
        _rowSpotActions.Controls.Add(numSpotRadius, 1, 0);
        _rowSpotActions.Controls.Add(btnDeleteLureSpot, 3, 0);
        _rowSpotActions.Controls.Add(btnClearLureSpots, 4, 0);

        ConfigureFillControl(checkWalkRandomly);

        _spotsGrid.Controls.Add(btnAddLureSpot, 0, 0);
        _spotsGrid.Controls.Add(lstLureSpots, 0, 1);
        _spotsGrid.Controls.Add(_rowSpotActions, 0, 2);
        _spotsGrid.Controls.Add(checkWalkRandomly, 0, 3);

        groupBoxLureSpots.Controls.Add(_spotsGrid);
    }

    private void BuildSettingsGroup()
    {
        groupBox3.Controls.Clear();
        _settingsGrid = CreateGrid(1, 11);

        ConfigureFillControl(checkUseHowlingShout);
        ConfigureFillControl(checkNoHowlingAtCenter);
        ConfigureFillControl(checkUseNormalAttack);
        ConfigureFillControl(checkUseAttackingSkills);
        ConfigureAnchoredInput(separator1);

        _rowStopDead = CreateGrid(2, 1);
        _rowStopDead.RowStyles[0] = new RowStyle(SizeType.Percent, 100f);
        ConfigureFillControl(checkStopPartyMemberDead);
        ConfigureAnchoredInput(numPartyMemberDead);
        _rowStopDead.Controls.Add(checkStopPartyMemberDead, 0, 0);
        _rowStopDead.Controls.Add(numPartyMemberDead, 1, 0);

        _rowStopParty = CreateGrid(2, 1);
        _rowStopParty.RowStyles[0] = new RowStyle(SizeType.Percent, 100f);
        ConfigureFillControl(checkStopPartyMember);
        ConfigureAnchoredInput(numPartyMember);
        _rowStopParty.Controls.Add(checkStopPartyMember, 0, 0);
        _rowStopParty.Controls.Add(numPartyMember, 1, 0);

        ConfigureAnchoredInput(separator2);
        ConfigureFillControl(checkStopMonsterType);

        _rowMonsterType = CreateGrid(3, 1);
        _rowMonsterType.RowStyles[0] = new RowStyle(SizeType.Percent, 100f);
        ConfigureAnchoredInput(comboMonsterType);
        ConfigureFillControl(label6, ContentAlignment.MiddleCenter);
        ConfigureAnchoredInput(numMonsterType);
        _rowMonsterType.Controls.Add(comboMonsterType, 0, 0);
        _rowMonsterType.Controls.Add(label6, 1, 0);
        _rowMonsterType.Controls.Add(numMonsterType, 2, 0);

        _settingsGrid.Controls.Add(checkUseHowlingShout, 0, 0);
        _settingsGrid.Controls.Add(checkNoHowlingAtCenter, 0, 1);
        _settingsGrid.Controls.Add(checkUseNormalAttack, 0, 2);
        _settingsGrid.Controls.Add(checkUseAttackingSkills, 0, 3);
        _settingsGrid.Controls.Add(separator1, 0, 4);
        _settingsGrid.Controls.Add(_rowStopDead, 0, 5);
        _settingsGrid.Controls.Add(_rowStopParty, 0, 6);
        _settingsGrid.Controls.Add(separator2, 0, 7);
        _settingsGrid.Controls.Add(checkStopMonsterType, 0, 8);
        _settingsGrid.Controls.Add(_rowMonsterType, 0, 9);

        groupBox3.Controls.Add(_settingsGrid);
    }

    private void UpdateLayoutMetrics()
    {
        if (_updatingLayout || IsDisposed || _rootLayout == null)
            return;

        _updatingLayout = true;
        SuspendLayout();
        _rootLayout.SuspendLayout();

        try
        {
            var groupPadding = new Padding(S(10), S(10), S(10), S(8));
            var innerTopPad = new Padding(0, S(6), 0, 0);

            groupBoxParty.Padding = groupPadding;
            groupBoxLureSpots.Padding = groupPadding;
            groupBox3.Padding = groupPadding;
            groupBoxParty.Margin = new Padding(0, 0, 0, S(8));
            groupBoxLureSpots.Margin = Padding.Empty;
            groupBox3.Margin = new Padding(S(6), 0, 0, 0);
            _leftColumn.Margin = new Padding(0, 0, S(6), 0);
            _rootLayout.Padding = new Padding(S(10));

            var controlBarH = S(38);
            _rootLayout.RowStyles[0] = new RowStyle(SizeType.Absolute, controlBarH);
            _rootLayout.RowStyles[1] = new RowStyle(SizeType.Percent, 100f);

            _lureControlBar.Margin = new Padding(0, 0, 0, S(8));
            _lureControlBar.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 100f);
            _lureControlBar.ColumnStyles[1] = new ColumnStyle(SizeType.Absolute, S(140));
            _lureControlBar.ColumnStyles[2] = new ColumnStyle(SizeType.Absolute, S(140));
            _btnStartLure.Margin = new Padding(0, S(2), S(6), S(2));
            _btnStopLure.Margin = new Padding(0, S(2), 0, S(2));

            _partyGrid.Padding = innerTopPad;
            _spotsGrid.Padding = innerTopPad;
            _settingsGrid.Padding = innerTopPad;

            // Party card metrics
            _partyGrid.RowStyles[0] = new RowStyle(SizeType.Absolute, S(28));
            _partyGrid.RowStyles[1] = new RowStyle(SizeType.Absolute, S(36));
            _partyGrid.RowStyles[2] = new RowStyle(SizeType.Absolute, S(30));
            _partyGrid.RowStyles[3] = new RowStyle(SizeType.Absolute, S(22));
            _partyGrid.RowStyles[4] = new RowStyle(SizeType.Absolute, S(30));

            btnSetPartySpot.Margin = new Padding(0, S(3), 0, S(3));
            _rowPartyCoords.ColumnStyles[0] = new ColumnStyle(SizeType.Absolute, S(24));
            _rowPartyCoords.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, 50f);
            _rowPartyCoords.ColumnStyles[2] = new ColumnStyle(SizeType.Absolute, S(24));
            _rowPartyCoords.ColumnStyles[3] = new ColumnStyle(SizeType.Percent, 50f);
            _rowPartyCoords.ColumnStyles[4] = new ColumnStyle(SizeType.Absolute, S(86));
            _rowPartyCoords.ColumnStyles[5] = new ColumnStyle(SizeType.Absolute, S(92));
            numPartyRadius.Margin = new Padding(S(6), 0, 0, 0);
            numPartyRadius.Height = S(24);

            _rowPartyWait.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 100f);
            _rowPartyWait.ColumnStyles[1] = new ColumnStyle(SizeType.Absolute, S(92));
            numPartyWaitSeconds.Margin = new Padding(S(6), 0, 0, 0);
            numPartyWaitSeconds.Height = S(24);

            _rowWalkscript.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 100f);
            _rowWalkscript.ColumnStyles[1] = new ColumnStyle(SizeType.Absolute, S(86));
            txtWalkbackScript.Margin = new Padding(0, 0, S(6), 0);
            txtWalkbackScript.Height = S(24);
            btnBrowseWalkscript.Margin = new Padding(0, S(2), 0, S(2));

            // Lure spots card metrics
            _spotsGrid.RowStyles[0] = new RowStyle(SizeType.Absolute, S(36));
            _spotsGrid.RowStyles[1] = new RowStyle(SizeType.Percent, 100f);
            _spotsGrid.RowStyles[2] = new RowStyle(SizeType.Absolute, S(32));
            _spotsGrid.RowStyles[3] = new RowStyle(SizeType.Absolute, S(28));

            btnAddLureSpot.Margin = new Padding(0, 0, 0, S(6));
            lstLureSpots.Margin = new Padding(0, 0, 0, S(6));
            _rowSpotActions.ColumnStyles[0] = new ColumnStyle(SizeType.Absolute, S(84));
            _rowSpotActions.ColumnStyles[1] = new ColumnStyle(SizeType.Absolute, S(92));
            _rowSpotActions.ColumnStyles[2] = new ColumnStyle(SizeType.Percent, 100f);
            _rowSpotActions.ColumnStyles[3] = new ColumnStyle(SizeType.Absolute, S(116));
            _rowSpotActions.ColumnStyles[4] = new ColumnStyle(SizeType.Absolute, S(92));
            numSpotRadius.Margin = new Padding(S(2), 0, S(6), 0);
            numSpotRadius.Height = S(24);
            btnDeleteLureSpot.Margin = new Padding(S(4), S(1), S(4), S(1));
            btnClearLureSpots.Margin = new Padding(S(4), S(1), 0, S(1));
            checkWalkRandomly.Margin = new Padding(0, S(2), 0, 0);

            // Settings card metrics
            _settingsGrid.RowStyles[0] = new RowStyle(SizeType.Absolute, S(28));
            _settingsGrid.RowStyles[1] = new RowStyle(SizeType.Absolute, S(28));
            _settingsGrid.RowStyles[2] = new RowStyle(SizeType.Absolute, S(28));
            _settingsGrid.RowStyles[3] = new RowStyle(SizeType.Absolute, S(28));
            _settingsGrid.RowStyles[4] = new RowStyle(SizeType.Absolute, S(12));
            _settingsGrid.RowStyles[5] = new RowStyle(SizeType.Absolute, S(30));
            _settingsGrid.RowStyles[6] = new RowStyle(SizeType.Absolute, S(30));
            _settingsGrid.RowStyles[7] = new RowStyle(SizeType.Absolute, S(12));
            _settingsGrid.RowStyles[8] = new RowStyle(SizeType.Absolute, S(28));
            _settingsGrid.RowStyles[9] = new RowStyle(SizeType.Absolute, S(30));
            _settingsGrid.RowStyles[10] = new RowStyle(SizeType.Percent, 100f);

            checkNoHowlingAtCenter.Margin = new Padding(S(20), 0, 0, 0);
            separator1.Height = S(6);
            separator2.Height = S(6);

            _rowStopDead.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 100f);
            _rowStopDead.ColumnStyles[1] = new ColumnStyle(SizeType.Absolute, S(92));
            numPartyMemberDead.Margin = new Padding(S(6), 0, 0, 0);
            numPartyMemberDead.Height = S(24);

            _rowStopParty.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 100f);
            _rowStopParty.ColumnStyles[1] = new ColumnStyle(SizeType.Absolute, S(92));
            numPartyMember.Margin = new Padding(S(6), 0, 0, 0);
            numPartyMember.Height = S(24);

            _rowMonsterType.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 100f);
            _rowMonsterType.ColumnStyles[1] = new ColumnStyle(SizeType.Absolute, S(30));
            _rowMonsterType.ColumnStyles[2] = new ColumnStyle(SizeType.Absolute, S(92));
            comboMonsterType.Margin = new Padding(S(20), 0, S(6), 0);
            numMonsterType.Margin = new Padding(S(6), 0, 0, 0);
            numMonsterType.Height = S(24);

            // Exact party card height derived from font header + 5 content rows + padding
            var headerAllowance = groupBoxParty.Font.Height + S(14);
            var partyCardH = headerAllowance + S(146) + S(18);
            _leftColumn.RowStyles[0] = new RowStyle(SizeType.Absolute, partyCardH);

            var minW = S(920);
            var minH = controlBarH + S(8) + partyCardH + S(8) + S(236) + S(20);
            AutoScrollMinSize = new Size(minW, minH);

            var targetW = Math.Max(ClientSize.Width, minW);
            var targetH = Math.Max(ClientSize.Height, minH);
            var scroll = AutoScrollPosition;
            _rootLayout.SetBounds(scroll.X, scroll.Y, targetW, targetH);
        }
        finally
        {
            _rootLayout.ResumeLayout(true);
            ResumeLayout(true);
            _updatingLayout = false;
        }

        AdjustSpotColumns();
    }

    private void AdjustSpotColumns()
    {
        if (lstLureSpots == null || lstLureSpots.IsDisposed || lstLureSpots.Columns.Count < 4)
            return;

        var width = lstLureSpots.ClientSize.Width;
        if (width <= 40)
            return;

        var idxW = Math.Max(S(36), (int)(width * 0.13));
        var xW = Math.Max(S(70), (int)(width * 0.30));
        var yW = Math.Max(S(70), (int)(width * 0.30));
        var radW = Math.Max(S(60), width - idxW - xW - yW);

        colIndex.Width = idxW;
        colX.Width = xW;
        colY.Width = yW;
        colRadius.Width = radW;
    }

    private void ApplyLanguage(bool isAr)
    {
        groupBoxParty.Text = isAr ? "مكان البارتي - منطقة العودة" : "Party & Return Area";
        lblPartyRadiusTitle.Text = isAr ? "نصف القطر:" : "Radius:";
        btnSetPartySpot.Text = isAr ? "تحديد مكان البارتي بالموقع الحالي" : "Set Party Spot (Current)";
        lblPartyWaitTitle.Text = isAr ? "الانتظار عند البارتي بالثواني:" : "Wait at party (seconds):";
        lblWalkscriptTitle.Text = isAr ? "مسار السير للبارتي:" : "Script to location:";
        btnBrowseWalkscript.Text = isAr ? "تصفح..." : "Browse";

        groupBoxLureSpots.Text = isAr ? "أماكن سحب الوحوش" : "Lure Locations";
        btnAddLureSpot.Text = isAr ? "إضافة موقع السحب - الموقع الحالي" : "+ Add Current Location";
        colIndex.Text = "#";
        colX.Text = "X";
        colY.Text = "Y";
        colRadius.Text = isAr ? "نصف القطر" : "Radius";
        lblSpotRadiusTitle.Text = isAr ? "نصف القطر:" : "Radius:";
        btnDeleteLureSpot.Text = isAr ? "حذف المحدد" : "Delete Selected";
        btnClearLureSpots.Text = isAr ? "مسح الكل" : "Clear All";
        checkWalkRandomly.Text = isAr ? "سحب عشوائي إذا لم يتم تحديد مواقع" : "Walk randomly if no spots added";

        groupBox3.Text = isAr ? "إعدادات السحب والمهارات" : "Settings";
        checkUseHowlingShout.Text = isAr ? "استخدام Howling Shout للمحارب الأوروبي" : "Cast howling shout (only EU)";
        checkNoHowlingAtCenter.Text = isAr ? "عدم تفعيل Howling Shout قرب البارتي" : "Don't cast near center/party";
        checkUseNormalAttack.Text = isAr ? "ضرب الوحوش بالضربة العادية لسحبها" : "Use normal attack to pull";
        checkUseAttackingSkills.Text = isAr ? "استخدام مهارة هجومية لسحب الوحوش" : "Use attacking skill to pull";
        checkStopPartyMemberDead.Text = isAr ? "إيقاف السحب عند وفاة أعضاء >=" : "Stop if dead party members >=";
        checkStopPartyMember.Text = isAr ? "إيقاف السحب إذا قل أعضاء البارتي <=" : "Stop if num party members <=";
        checkStopMonsterType.Text = isAr ? "إيقاف السحب عند زيادة عدد وحوش >=" : "Stop if num monster of type >=";

        if (_btnStartLure != null)
            _btnStartLure.Text = isAr ? "تشغيل السحب" : "Start Lure";
        if (_btnStopLure != null)
            _btnStopLure.Text = isAr ? "إيقاف السحب" : "Stop Lure";

        RefreshUiState();
    }

    private void RefreshUiState()
    {
        if (_btnStartLure == null || _btnStopLure == null || _lblLureStatus == null)
            return;

        bool running = LureBotbase.IsActive;
        bool isAr = Kernel.Language == "ar_AR";

        _btnStartLure.Enabled = !running;
        _btnStopLure.Enabled = running;
        _lblLureStatus.Text = running
            ? (isAr ? "الحالة: السحب يعمل الآن (مستقل عن بوت التدريب)" : "Status: Lure is running (independent from Training)")
            : (isAr ? "الحالة: متوقف" : "Status: Stopped");

        ApplyLureControlBarTheme();
    }

    private void ApplyLureControlBarTheme()
    {
        if (_btnStartLure == null || _btnStopLure == null || _lblLureStatus == null)
            return;

        bool running = LureBotbase.IsActive;
        var fore = ColorScheme.ForeColor;

        _lblLureStatus.BackColor = Color.Transparent;
        _lblLureStatus.ForeColor = running ? Color.FromArgb(46, 204, 113) : fore;
        _lblLureStatus.Invalidate();

        _btnStartLure.BackColor = Color.Transparent;
        _btnStartLure.Color = running ? Color.Transparent : Color.FromArgb(35, 134, 54);
        _btnStartLure.ForeColor = running ? fore : Color.White;
        _btnStartLure.Invalidate();

        _btnStopLure.BackColor = Color.Transparent;
        _btnStopLure.Color = running ? Color.FromArgb(207, 34, 46) : Color.Transparent;
        _btnStopLure.ForeColor = running ? Color.White : fore;
        _btnStopLure.Invalidate();
    }

    private void BtnStartLure_Click(object sender, EventArgs e)
    {
        bool isAr = Kernel.Language == "ar_AR";
        if (!Game.Ready || Game.Player == null)
        {
            MessageBox.Show(
                isAr ? "يجب تسجيل الدخول إلى الشخصية أولاً قبل تشغيل السحب." : "Please log in with a character before starting Lure.",
                "AeroBot",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        EnsureRunnerSubscription();
        var runner = LureBotbase.EnsureRunner();
        if (runner == null)
            return;

        if (!runner.TryStart(out var errorMessage))
        {
            MessageBox.Show(errorMessage, "AeroBot", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            RefreshUiState();
            return;
        }

        RefreshUiState();
    }

    private void BtnStopLure_Click(object sender, EventArgs e)
    {
        _btnStopLure.Enabled = false;
        Task.Run(() =>
        {
            LureBotbase.StopRunner();
            if (Game.Ready && Game.Player != null)
                Game.Player.MoveTo(Game.Player.Position, false);

            RunOnUi(RefreshUiState);
        });
    }

    private void RefreshSpotsList()
    {
        lstLureSpots.Items.Clear();
        var spots = LureConfig.Spots;
        for (int i = 0; i < spots.Count; i++)
        {
            var spot = spots[i];
            var item = new ListViewItem((i + 1).ToString());
            item.SubItems.Add(spot.X.ToString("0.0"));
            item.SubItems.Add(spot.Y.ToString("0.0"));
            item.SubItems.Add(spot.Radius.ToString());
            lstLureSpots.Items.Add(item);
        }
    }

    private void btnSetPartySpot_Click(object sender, EventArgs e)
    {
        if (Game.Player == null)
            return;

        var pos = Game.Player.Position;
        LureConfig.Area = new Area
        {
            Name = "Party",
            Position = pos,
            Radius = (int)numPartyRadius.Value
        };

        lblPartyX.Text = pos.X.ToString("0.0");
        lblPartyY.Text = pos.Y.ToString("0.0");
        Log.Notify($"[Lure] Party location set to X: {pos.X:0.0}, Y: {pos.Y:0.0}, Radius: {numPartyRadius.Value}");
    }

    private void btnAddLureSpot_Click(object sender, EventArgs e)
    {
        if (Game.Player == null)
            return;

        var pos = Game.Player.Position;
        var spots = LureConfig.Spots;
        var newSpot = new LureSpot
        {
            Region = pos.Region,
            XOffset = pos.XOffset,
            YOffset = pos.YOffset,
            ZOffset = pos.ZOffset,
            Radius = (int)numSpotRadius.Value
        };

        spots.Add(newSpot);
        LureConfig.Spots = spots;

        RefreshSpotsList();
        Log.Notify($"[Lure] Added lure spot #{spots.Count} at X: {newSpot.X:0.0}, Y: {newSpot.Y:0.0}, Radius: {newSpot.Radius}");
    }

    private void btnDeleteLureSpot_Click(object sender, EventArgs e)
    {
        if (lstLureSpots.SelectedIndices.Count == 0)
            return;

        int index = lstLureSpots.SelectedIndices[0];
        var spots = LureConfig.Spots;
        if (index >= 0 && index < spots.Count)
        {
            spots.RemoveAt(index);
            LureConfig.Spots = spots;
            RefreshSpotsList();
        }
    }

    private void btnClearLureSpots_Click(object sender, EventArgs e)
    {
        bool isAr = Kernel.Language == "ar_AR";
        var msg = isAr ? "هل أنت متأكد من مسح جميع أماكن السحب؟" : "Are you sure you want to clear all lure spots?";
        var title = isAr ? "تأكيد" : "Confirm";

        if (MessageBox.Show(msg, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        LureConfig.Spots = new List<LureSpot>();
        RefreshSpotsList();
    }

    private void OnSettingsChanged(object sender, EventArgs e)
    {
        if (_configLocked)
            return;

        _configLocked = true;

        LureConfig.Area = new Area
        {
            Name = "Party",
            Position = LureConfig.Area.Position,
            Radius = (int)numPartyRadius.Value
        };

        LureConfig.StayAtCenterForSeconds = (int)numPartyWaitSeconds.Value;
        LureConfig.WalkRandomly = checkWalkRandomly.Checked;
        LureConfig.UseNormalAttack = checkUseNormalAttack.Checked;
        LureConfig.UseHowlingShout = checkUseHowlingShout.Checked;
        LureConfig.NoHowlingAtCenter = checkNoHowlingAtCenter.Checked;
        LureConfig.UseAttackingSkills = checkUseAttackingSkills.Checked;

        LureConfig.StopIfNumMonsterType = checkStopMonsterType.Checked;
        LureConfig.StopIfNumPartyMemberDead = checkStopPartyMemberDead.Checked;
        LureConfig.StopIfNumPartyMember = checkStopPartyMember.Checked;
        LureConfig.NumMonsterType = (int)numMonsterType.Value;
        LureConfig.NumPartyMember = (int)numPartyMember.Value;
        LureConfig.NumPartyMemberDead = (int)numPartyMemberDead.Value;

        _configLocked = false;
    }

    private void btnBrowseWalkscript_Click(object sender, EventArgs e)
    {
        var fileBrowser = new OpenFileDialog
        {
            Title = "RSBot - Choose walkback script",
            Filter = "RSBot script (*.rbs)|*.rbs",
            Multiselect = false
        };

        if (fileBrowser.ShowDialog() != DialogResult.OK)
            return;

        txtWalkbackScript.Text = fileBrowser.FileName;
        LureConfig.WalkscriptPath = fileBrowser.FileName;
    }

    private void comboMonsterType_SelectedIndexChanged(object sender, EventArgs e)
    {
        LureConfig.SelectedMonsterType = comboMonsterType.SelectedIndex switch
        {
            0 => MonsterRarity.General,
            1 => MonsterRarity.Champion,
            2 => MonsterRarity.Giant,
            3 => MonsterRarity.GeneralParty,
            4 => MonsterRarity.ChampionParty,
            5 => MonsterRarity.GiantParty,
            6 => MonsterRarity.Elite,
            7 => MonsterRarity.EliteStrong,
            8 => MonsterRarity.Unique,
            9 => MonsterRarity.Event,
            _ => LureConfig.SelectedMonsterType
        };
    }

    private void Main_Load(object sender, EventArgs e)
    {
        OnLoadCharacter();
    }
}
