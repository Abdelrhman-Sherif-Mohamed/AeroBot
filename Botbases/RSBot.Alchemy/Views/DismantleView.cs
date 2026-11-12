using System;
using System.Drawing;
using System.Windows.Forms;
using RSBot.Alchemy.Bot;
using RSBot.Alchemy.Bundle.Dismantle;
using RSBot.Alchemy.Models;
using RSBot.Alchemy.Views.Dialogs;
using SDUI;

using Button = SDUI.Controls.Button;
using CheckBox = SDUI.Controls.CheckBox;
using GroupBox = SDUI.Controls.GroupBox;
using Label = SDUI.Controls.Label;
using NumUpDown = SDUI.Controls.NumUpDown;
using ProgressBar = SDUI.Controls.ProgressBar;

namespace RSBot.Alchemy.Views;

internal sealed class DismantleView : UserControl
{
    private GroupBox _grpSettings;
    private GroupBox _grpCurrentStatus;
    private GroupBox _grpFilter;
    private GroupBox _grpResults;

    private CheckBox _chkAutoDismantle;
    private CheckBox _chkWhenBagFull;
    private CheckBox _chkOnlySelected;

    private ProgressBar _progressBar;
    private Label _lblState;
    private Button _btnStart;
    private Button _btnStop;

    private DataGridView _gridFilter;
    private Button _btnAddItem;
    private Button _btnRemove;
    private Button _btnAddByType;
    private CheckBox _chkLessThanLevel;
    private NumUpDown _numMaxLevel;

    private DataGridView _gridOutputs;

    public DismantleView()
    {
        DoubleBuffered = true;
        BackColor = Color.Transparent;
        BuildUi();
        SubscribeBundleEvents();
        ApplyLocalization();
        RefreshAll();
    }

    private void BuildUi()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(8),
            BackColor = Color.Transparent
        };
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 64));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 36));

        // Top section: Left (Settings + Current Status), Right (Filter)
        var topSplit = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent
        };
        topSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48));
        topSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52));

        var leftCol = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent
        };
        leftCol.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        leftCol.RowStyles.Add(new RowStyle(SizeType.Absolute, 108));

        _grpSettings = AlchemyUi.CreateGroupBox("إعدادات التكسير التلقائي");
        _grpSettings.Dock = DockStyle.Fill;

        var settingsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(8, 4, 8, 4),
            BackColor = Color.Transparent
        };
        for (var i = 0; i < 3; i++)
            settingsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3F));

        _chkAutoDismantle = AlchemyUi.CreateCheckBox("فعل التكسير التلقائي", false);
        _chkAutoDismantle.Dock = DockStyle.Fill;
        _chkAutoDismantle.CheckedChanged += (_, _) => SyncConfig();

        _chkWhenBagFull = AlchemyUi.CreateCheckBox("كسر لما الشنطة تتملي", false);
        _chkWhenBagFull.Dock = DockStyle.Fill;
        _chkWhenBagFull.CheckedChanged += (_, _) => SyncConfig();

        _chkOnlySelected = AlchemyUi.CreateCheckBox("اكسر الإيتمات المحددة بس", false);
        _chkOnlySelected.Dock = DockStyle.Fill;
        _chkOnlySelected.CheckedChanged += (_, _) => SyncConfig();

        settingsLayout.Controls.Add(_chkAutoDismantle, 0, 0);
        settingsLayout.Controls.Add(_chkWhenBagFull, 0, 1);
        settingsLayout.Controls.Add(_chkOnlySelected, 0, 2);
        _grpSettings.Controls.Add(settingsLayout);

        _grpCurrentStatus = AlchemyUi.CreateGroupBox("الحالة دلوقتي");
        _grpCurrentStatus.Dock = DockStyle.Fill;

        var statusLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 3,
            Padding = new Padding(8, 4, 8, 4),
            BackColor = Color.Transparent
        };
        statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 78));
        statusLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 78));
        statusLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        statusLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        statusLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _progressBar = AlchemyUi.CreateProgressBar();
        _progressBar.Dock = DockStyle.None;
        _progressBar.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        _progressBar.Height = 20;
        _progressBar.Margin = new Padding(0, 7, 6, 7);

        _btnStart = AlchemyUi.CreateButton("ابدأ", ButtonTone.Success, 28);
        _btnStart.Dock = DockStyle.Fill;
        _btnStart.Margin = new Padding(2, 3, 2, 3);
        _btnStart.Click += (_, _) => StartDismantle();

        _btnStop = AlchemyUi.CreateButton("وقف", ButtonTone.Danger, 28);
        _btnStop.Dock = DockStyle.Fill;
        _btnStop.Margin = new Padding(2, 3, 0, 3);
        _btnStop.Click += (_, _) => AlchemyUi.StopAlchemyBot();

        _lblState = AlchemyUi.CreateLabel("الحالة: واقف", ContentAlignment.MiddleLeft);
        _lblState.Dock = DockStyle.Fill;

        statusLayout.Controls.Add(_progressBar, 0, 0);
        statusLayout.Controls.Add(_btnStart, 1, 0);
        statusLayout.Controls.Add(_btnStop, 2, 0);
        statusLayout.Controls.Add(_lblState, 0, 1);
        statusLayout.SetColumnSpan(_lblState, 3);
        _grpCurrentStatus.Controls.Add(statusLayout);

        leftCol.Controls.Add(_grpSettings, 0, 0);
        leftCol.Controls.Add(_grpCurrentStatus, 0, 1);

        // Right Column: Item Filter
        _grpFilter = AlchemyUi.CreateGroupBox("فلتر الإيتمات");
        _grpFilter.Dock = DockStyle.Fill;

        var filterPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = Color.Transparent
        };
        filterPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        filterPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        filterPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

        _gridFilter = AlchemyUi.CreateGrid(allowEdit: false);
        _gridFilter.Columns.Add(new DataGridViewTextBoxColumn { Name = "colItemName", HeaderText = "Item Name", FillWeight = 55 });
        _gridFilter.Columns.Add(new DataGridViewTextBoxColumn { Name = "colQuantity", HeaderText = "Quantity", FillWeight = 22 });
        _gridFilter.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPriority", HeaderText = "Priority", FillWeight = 23 });

        var filterButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 3, 0, 0),
            BackColor = Color.Transparent
        };
        _btnAddItem = AlchemyUi.CreateButton("ضيف", ButtonTone.Neutral, 30);
        _btnAddItem.Width = 86;
        _btnAddItem.Click += (_, _) => AddItemFilter();

        _btnRemove = AlchemyUi.CreateButton("شيل", ButtonTone.Neutral, 30);
        _btnRemove.Width = 86;
        _btnRemove.Click += (_, _) => RemoveSelectedFilter();

        _btnAddByType = AlchemyUi.CreateButton("ضيف بالنوع", ButtonTone.Neutral, 30);
        _btnAddByType.Width = 110;
        _btnAddByType.Click += (_, _) => AddTypeFilter();

        filterButtons.Controls.AddRange(new Control[] { _btnAddItem, _btnRemove, _btnAddByType });

        var levelFilterRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(0, 2, 0, 0),
            BackColor = Color.Transparent
        };
        levelFilterRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        levelFilterRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 86));
        levelFilterRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _chkLessThanLevel = AlchemyUi.CreateCheckBox("أقل من Level:", false);
        _chkLessThanLevel.Dock = DockStyle.Fill;
        _chkLessThanLevel.CheckedChanged += (_, _) => SyncConfig();

        _numMaxLevel = AlchemyUi.CreateNumUpDown(1, 150, 100, 82);
        _numMaxLevel.Dock = DockStyle.Fill;
        _numMaxLevel.Margin = new Padding(4, 2, 0, 2);
        _numMaxLevel.ValueChanged += (_, _) => SyncConfig();

        levelFilterRow.Controls.Add(_chkLessThanLevel, 0, 0);
        levelFilterRow.Controls.Add(_numMaxLevel, 1, 0);

        filterPanel.Controls.Add(_gridFilter, 0, 0);
        filterPanel.Controls.Add(filterButtons, 0, 1);
        filterPanel.Controls.Add(levelFilterRow, 0, 2);
        _grpFilter.Controls.Add(filterPanel);

        topSplit.Controls.Add(leftCol, 0, 0);
        topSplit.Controls.Add(_grpFilter, 1, 0);

        // Bottom full-width: Dismantle Outputs
        _grpResults = AlchemyUi.CreateGroupBox("ناتج التكسير");
        _grpResults.Dock = DockStyle.Fill;

        _gridOutputs = AlchemyUi.CreateGrid(allowEdit: false);
        _gridOutputs.Columns.Add(new DataGridViewTextBoxColumn { Name = "colMat", HeaderText = "Material Name", FillWeight = 50 });
        _gridOutputs.Columns.Add(new DataGridViewTextBoxColumn { Name = "colQty", HeaderText = "Quantity", FillWeight = 25 });
        _gridOutputs.Columns.Add(new DataGridViewTextBoxColumn { Name = "colVal", HeaderText = "Total Value", FillWeight = 25 });
        _grpResults.Controls.Add(_gridOutputs);

        root.Controls.Add(topSplit, 0, 0);
        root.Controls.Add(_grpResults, 0, 1);
        Controls.Add(root);
    }

    private void SubscribeBundleEvents()
    {
        if (Globals.Botbase?.DismantleBundle != null)
            Globals.Botbase.DismantleBundle.StateChanged += () => SafeInvoke(RefreshAll);
    }

    private void SyncConfig()
    {
        var cfg = Globals.Botbase?.DismantleBundleConfig;
        if (cfg == null)
            return;

        cfg.AutoDismantle = _chkAutoDismantle.Checked;
        cfg.DismantleWhenBagFull = _chkWhenBagFull.Checked;
        cfg.OnlySelectedItems = _chkOnlySelected.Checked;
        cfg.UseMaxLevelFilter = _chkLessThanLevel.Checked;
        cfg.MaxItemLevel = (int)_numMaxLevel.Value;
    }

    private void AddItemFilter()
    {
        using var dlg = new DismantleAddItemDialog();
        if (dlg.ShowDialog(FindForm()) != DialogResult.OK || dlg.CreatedRules.Count == 0)
            return;

        var cfg = Globals.Botbase?.DismantleBundleConfig;
        if (cfg == null)
            return;

        cfg.Filters.AddRange(dlg.CreatedRules);
        RefreshFilterGrid();
    }

    private void AddTypeFilter()
    {
        using var dlg = new DismantleAddTypeDialog();
        if (dlg.ShowDialog(FindForm()) != DialogResult.OK || dlg.CreatedRule == null)
            return;

        var cfg = Globals.Botbase?.DismantleBundleConfig;
        if (cfg == null)
            return;

        cfg.Filters.Add(dlg.CreatedRule);
        RefreshFilterGrid();
    }

    private void RemoveSelectedFilter()
    {
        var cfg = Globals.Botbase?.DismantleBundleConfig;
        if (cfg == null || _gridFilter.SelectedRows.Count == 0)
            return;

        if (_gridFilter.SelectedRows[0].Tag is DismantleFilterRule rule)
        {
            cfg.Filters.Remove(rule);
            RefreshFilterGrid();
        }
    }

    private void StartDismantle()
    {
        SyncConfig();
        var cfg = Globals.Botbase?.DismantleBundleConfig;
        if (cfg != null && cfg.Filters.Count == 0 && !cfg.UseMaxLevelFilter)
        {
            MessageBox.Show(
                AlchemyUi.IsArabic
                    ? "ضيف إيتمات للفلتر أو فعّل خيار أقل من Level الأول."
                    : "Please add at least one item filter or enable the Max Level filter.",
                "Dismantle", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        AlchemyUi.EnsureAlchemyBotStarted(AlchemyEngine.Dismantle);
        RefreshStatus();
    }

    public void RefreshAll()
    {
        if (IsDisposed || Disposing)
            return;

        RefreshFilterGrid();
        RefreshOutputsGrid();
        RefreshStatus();
    }

    private void RefreshFilterGrid()
    {
        _gridFilter.Rows.Clear();
        var cfg = Globals.Botbase?.DismantleBundleConfig;
        if (cfg?.Filters == null)
            return;

        foreach (var rule in cfg.Filters)
        {
            var idx = _gridFilter.Rows.Add(rule.ItemName, rule.GetMatchingInventoryCount(), rule.Priority);
            _gridFilter.Rows[idx].Tag = rule;
        }
    }

    private void RefreshOutputsGrid()
    {
        _gridOutputs.Rows.Clear();
        var cfg = Globals.Botbase?.DismantleBundleConfig;
        if (cfg?.Outputs == null)
            return;

        foreach (var output in cfg.Outputs)
            _gridOutputs.Rows.Add(output.MaterialName, output.Quantity, output.TotalValue.ToString("N0"));
    }

    public void RefreshStatus()
    {
        var bundle = Globals.Botbase?.DismantleBundle;
        var running = Bootstrap.IsActive && Globals.Botbase?.AlchemyEngine == AlchemyEngine.Dismantle;

        _btnStart.Enabled = !running;
        _btnStop.Enabled = running;
        _lblState.Text = running
            ? (AlchemyUi.IsArabic ? "الحالة: شغال..." : "Status: Running...")
            : (AlchemyUi.IsArabic ? "الحالة: واقف" : "Status: Stopped");

        var max = bundle?.TotalBatchTarget > 0 ? bundle.TotalBatchTarget : 100;
        var val = bundle?.TotalBatchTarget > 0 ? Math.Min(bundle.ProcessedCount, max) : 0;
        _progressBar.Maximum = max;
        _progressBar.Value = val;
    }

    public void ApplyLocalization()
    {
        var ar = AlchemyUi.IsArabic;
        _grpSettings.Text = ar ? "إعدادات التكسير التلقائي" : "Auto Dismantle Settings";
        _grpCurrentStatus.Text = ar ? "الحالة دلوقتي" : "Current Status";
        _grpFilter.Text = ar ? "فلتر الإيتمات" : "Item Filter";
        _grpResults.Text = ar ? "ناتج التكسير" : "Dismantle Output";

        _chkAutoDismantle.Text = ar ? "فعل التكسير التلقائي" : "Enable Auto Dismantle";
        _chkWhenBagFull.Text = ar ? "كسر لما الشنطة تتملي" : "Dismantle When Inventory Full";
        _chkOnlySelected.Text = ar ? "اكسر الإيتمات المحددة بس" : "Only Dismantle Filtered Items";

        _btnAddItem.Text = ar ? "ضيف" : "Add";
        _btnRemove.Text = ar ? "شيل" : "Remove";
        _btnAddByType.Text = ar ? "ضيف بالنوع" : "Add By Type";
        _chkLessThanLevel.Text = ar ? "أقل من Level:" : "Less than Level:";

        _btnStart.Text = ar ? "ابدأ" : "Start";
        _btnStop.Text = ar ? "وقف" : "Stop";

        RefreshAll();
    }

    public void ApplyTheme()
    {
        AlchemyUi.ApplyThemeRecursive(this);
    }

    private void SafeInvoke(Action action)
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
