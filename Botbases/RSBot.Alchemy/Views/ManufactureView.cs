using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RSBot.Alchemy.Bot;
using RSBot.Alchemy.Bundle.Manufacture;
using RSBot.Alchemy.Helper;
using RSBot.Alchemy.Models;
using RSBot.Alchemy.Views.Dialogs;
using RSBot.Core;
using SDUI;

using Button = SDUI.Controls.Button;
using CheckBox = SDUI.Controls.CheckBox;
using GroupBox = SDUI.Controls.GroupBox;
using Label = SDUI.Controls.Label;
using NumUpDown = SDUI.Controls.NumUpDown;
using ProgressBar = SDUI.Controls.ProgressBar;

namespace RSBot.Alchemy.Views;

internal sealed class ManufactureView : UserControl
{
    private GroupBox _grpSettings;
    private GroupBox _grpStatus;
    private GroupBox _grpQueue;
    private GroupBox _grpMaterials;

    private CheckBox _chkAutoManufacture;
    private Label _lblDelay;
    private NumUpDown _numDelay;

    private Label _lblStatus;
    private Label _lblCurrent;
    private Label _lblManufactured;
    private Label _lblFailed;
    private ProgressBar _progressBar;

    private DataGridView _gridQueue;
    private DataGridView _gridMaterials;

    private Button _btnAdd;
    private Button _btnRemove;
    private Button _btnClearAll;
    private Button _btnStart;
    private Button _btnStop;
    private Button _btnCheckMaterials;

    public ManufactureView()
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
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(8),
            BackColor = Color.Transparent
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 68));

        // Left Column: Settings + Status
        var leftCol = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent
        };
        leftCol.RowStyles.Add(new RowStyle(SizeType.Absolute, 134));
        leftCol.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _grpSettings = AlchemyUi.CreateGroupBox("إعدادات");
        _grpSettings.Dock = DockStyle.Fill;

        var settingsLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(6, 4, 6, 4),
            BackColor = Color.Transparent
        };
        settingsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        settingsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        settingsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

        _chkAutoManufacture = AlchemyUi.CreateCheckBox("فعل التصنيع التلقائي", false);
        _chkAutoManufacture.Dock = DockStyle.Fill;
        _chkAutoManufacture.CheckedChanged += (_, _) => SyncConfig();

        _lblDelay = AlchemyUi.CreateLabel("تأخير (مللي ثانية):", ContentAlignment.MiddleLeft);
        _lblDelay.Dock = DockStyle.Fill;

        _numDelay = AlchemyUi.CreateNumUpDown(100, 10_000, 500, 110);
        _numDelay.ValueChanged += (_, _) => SyncConfig();

        settingsLayout.Controls.Add(_chkAutoManufacture, 0, 0);
        settingsLayout.Controls.Add(_lblDelay, 0, 1);
        settingsLayout.Controls.Add(_numDelay, 0, 2);
        _grpSettings.Controls.Add(settingsLayout);

        _grpStatus = AlchemyUi.CreateGroupBox("الحالة");
        _grpStatus.Dock = DockStyle.Fill;

        var statusLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            Padding = new Padding(8, 6, 8, 6),
            BackColor = Color.Transparent
        };
        for (var i = 0; i < 4; i++)
            statusLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        statusLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        statusLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _lblStatus = AlchemyUi.CreateLabel("Status: Stopped", ContentAlignment.MiddleLeft, bold: true);
        _lblStatus.Dock = DockStyle.Fill;
        _lblCurrent = AlchemyUi.CreateLabel("Current: None", ContentAlignment.MiddleLeft);
        _lblCurrent.Dock = DockStyle.Fill;
        _lblManufactured = AlchemyUi.CreateLabel("Manufactured: 0", ContentAlignment.MiddleLeft);
        _lblManufactured.Dock = DockStyle.Fill;
        _lblFailed = AlchemyUi.CreateLabel("Failed: 0", ContentAlignment.MiddleLeft);
        _lblFailed.Dock = DockStyle.Fill;

        _progressBar = AlchemyUi.CreateProgressBar();
        _progressBar.Dock = DockStyle.Top;
        _progressBar.Height = 20;
        _progressBar.Margin = new Padding(0, 4, 0, 0);

        statusLayout.Controls.Add(_lblStatus, 0, 0);
        statusLayout.Controls.Add(_lblCurrent, 0, 1);
        statusLayout.Controls.Add(_lblManufactured, 0, 2);
        statusLayout.Controls.Add(_lblFailed, 0, 3);
        statusLayout.Controls.Add(_progressBar, 0, 4);
        _grpStatus.Controls.Add(statusLayout);

        leftCol.Controls.Add(_grpSettings, 0, 0);
        leftCol.Controls.Add(_grpStatus, 0, 1);

        // Right Column: Queue + Materials
        var rightCol = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent
        };
        rightCol.RowStyles.Add(new RowStyle(SizeType.Percent, 56));
        rightCol.RowStyles.Add(new RowStyle(SizeType.Percent, 44));

        _grpQueue = AlchemyUi.CreateGroupBox("قائمة التصنيع");
        _grpQueue.Dock = DockStyle.Fill;

        var queuePanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent
        };
        queuePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        queuePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

        _gridQueue = AlchemyUi.CreateGrid(allowEdit: false);
        _gridQueue.Columns.Add(new DataGridViewTextBoxColumn { Name = "colItem", HeaderText = "الإيتم", FillWeight = 45 });
        _gridQueue.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTarget", HeaderText = "الهدف", FillWeight = 15 });
        _gridQueue.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDone", HeaderText = "تم", FillWeight = 15 });
        _gridQueue.Columns.Add(new DataGridViewTextBoxColumn { Name = "colStatus", HeaderText = "الحالة", FillWeight = 25 });
        _gridQueue.SelectionChanged += (_, _) => RefreshMaterialsGrid();

        var queueButtons = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 6,
            RowCount = 1,
            Padding = new Padding(0, 2, 0, 0),
            BackColor = Color.Transparent
        };
        queueButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        queueButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 78));
        queueButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 78));
        queueButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 88));
        queueButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        queueButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 78));
        queueButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 78));

        _btnAdd = AlchemyUi.CreateButton("ضيف", ButtonTone.Neutral, 30);
        _btnAdd.Dock = DockStyle.Fill;
        _btnAdd.Margin = new Padding(2);
        _btnAdd.Click += (_, _) => AddTask();

        _btnRemove = AlchemyUi.CreateButton("شيل", ButtonTone.Neutral, 30);
        _btnRemove.Dock = DockStyle.Fill;
        _btnRemove.Margin = new Padding(2);
        _btnRemove.Click += (_, _) => RemoveSelected();

        _btnClearAll = AlchemyUi.CreateButton("مسح الكل", ButtonTone.Neutral, 30);
        _btnClearAll.Dock = DockStyle.Fill;
        _btnClearAll.Margin = new Padding(2);
        _btnClearAll.Click += (_, _) => ClearAllTasks();

        _btnStart = AlchemyUi.CreateButton("ابدأ", ButtonTone.Success, 30);
        _btnStart.Dock = DockStyle.Fill;
        _btnStart.Margin = new Padding(2);
        _btnStart.Click += (_, _) => StartManufacture();

        _btnStop = AlchemyUi.CreateButton("وقف", ButtonTone.Danger, 30);
        _btnStop.Dock = DockStyle.Fill;
        _btnStop.Margin = new Padding(2);
        _btnStop.Click += (_, _) => AlchemyUi.StopAlchemyBot();

        queueButtons.Controls.Add(_btnAdd, 0, 0);
        queueButtons.Controls.Add(_btnRemove, 1, 0);
        queueButtons.Controls.Add(_btnClearAll, 2, 0);
        queueButtons.Controls.Add(_btnStart, 4, 0);
        queueButtons.Controls.Add(_btnStop, 5, 0);

        queuePanel.Controls.Add(_gridQueue, 0, 0);
        queuePanel.Controls.Add(queueButtons, 0, 1);
        _grpQueue.Controls.Add(queuePanel);

        _grpMaterials = AlchemyUi.CreateGroupBox("الخامات");
        _grpMaterials.Dock = DockStyle.Fill;

        var matPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent
        };
        matPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        matPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));

        _gridMaterials = AlchemyUi.CreateGrid(allowEdit: false);
        _gridMaterials.Columns.Add(new DataGridViewTextBoxColumn { Name = "colMatName", HeaderText = "الخامة", FillWeight = 45 });
        _gridMaterials.Columns.Add(new DataGridViewTextBoxColumn { Name = "colReq", HeaderText = "مطلوب", FillWeight = 18 });
        _gridMaterials.Columns.Add(new DataGridViewTextBoxColumn { Name = "colAvail", HeaderText = "متاح", FillWeight = 18 });
        _gridMaterials.Columns.Add(new DataGridViewTextBoxColumn { Name = "colMatStatus", HeaderText = "الحالة", FillWeight = 19 });

        var matButtons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 4, 0, 0),
            BackColor = Color.Transparent
        };
        _btnCheckMaterials = AlchemyUi.CreateButton("راجع الخامات", ButtonTone.Neutral, 30);
        _btnCheckMaterials.Width = 128;
        _btnCheckMaterials.Click += (_, _) => RefreshMaterialsGrid();
        matButtons.Controls.Add(_btnCheckMaterials);

        matPanel.Controls.Add(_gridMaterials, 0, 0);
        matPanel.Controls.Add(matButtons, 0, 1);
        _grpMaterials.Controls.Add(matPanel);

        rightCol.Controls.Add(_grpQueue, 0, 0);
        rightCol.Controls.Add(_grpMaterials, 0, 1);

        root.Controls.Add(leftCol, 0, 0);
        root.Controls.Add(rightCol, 1, 0);
        Controls.Add(root);
    }

    private void SubscribeBundleEvents()
    {
        if (Globals.Botbase?.ManufactureBundle != null)
            Globals.Botbase.ManufactureBundle.StateChanged += () => SafeInvoke(RefreshAll);
    }

    private void SyncConfig()
    {
        var cfg = Globals.Botbase?.ManufactureBundleConfig;
        if (cfg == null)
            return;

        cfg.AutoManufacture = _chkAutoManufacture.Checked;
        cfg.DelayMs = (int)_numDelay.Value;
    }

    private void AddTask()
    {
        using var dlg = new ManufactureAddDialog();
        if (dlg.ShowDialog(FindForm()) != DialogResult.OK || dlg.CreatedEntry == null)
            return;

        var cfg = Globals.Botbase?.ManufactureBundleConfig;
        if (cfg == null)
            return;

        cfg.Queue.Add(dlg.CreatedEntry);
        RefreshAll();
    }

    private void RemoveSelected()
    {
        var cfg = Globals.Botbase?.ManufactureBundleConfig;
        if (cfg == null || _gridQueue.SelectedRows.Count == 0)
            return;

        if (_gridQueue.SelectedRows[0].Tag is ManufactureQueueEntry entry)
        {
            cfg.Queue.Remove(entry);
            RefreshAll();
        }
    }

    private void ClearAllTasks()
    {
        var cfg = Globals.Botbase?.ManufactureBundleConfig;
        if (cfg == null)
            return;

        cfg.Queue.Clear();
        Globals.Botbase.ManufactureBundle?.ResetCounters();
        RefreshAll();
    }

    private void StartManufacture()
    {
        var cfg = Globals.Botbase?.ManufactureBundleConfig;
        if (cfg == null || cfg.Queue.Count == 0)
        {
            MessageBox.Show(
                AlchemyUi.IsArabic ? "ضيف إيتم لقائمة التصنيع الأول." : "Please add at least one manufacture item first.",
                "Manufacture", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        foreach (var q in cfg.Queue)
        {
            if (q.Status == QueueItemStatus.Failed && q.CompletedCount < q.TargetCount)
            {
                q.Status = QueueItemStatus.Pending;
                q.StatusNote = string.Empty;
            }
        }

        SyncConfig();
        AlchemyUi.EnsureAlchemyBotStarted(AlchemyEngine.Manufacture);
        RefreshStatus();
    }

    public void RefreshAll()
    {
        if (IsDisposed || Disposing)
            return;

        RefreshQueueGrid();
        RefreshMaterialsGrid();
        RefreshStatus();
    }

    private void RefreshQueueGrid()
    {
        var selectedId = _gridQueue.SelectedRows.Count > 0 && _gridQueue.SelectedRows[0].Tag is ManufactureQueueEntry s
            ? s.Id
            : Guid.Empty;

        _gridQueue.Rows.Clear();
        var cfg = Globals.Botbase?.ManufactureBundleConfig;
        if (cfg?.Queue == null)
            return;

        foreach (var entry in cfg.Queue)
        {
            var idx = _gridQueue.Rows.Add(
                entry.ItemName,
                entry.TargetCount,
                entry.CompletedCount,
                entry.GetStatusLabel(AlchemyUi.IsArabic));

            var row = _gridQueue.Rows[idx];
            row.Tag = entry;
            if (entry.Id == selectedId)
                row.Selected = true;
        }

        if (_gridQueue.Rows.Count > 0 && _gridQueue.SelectedRows.Count == 0)
            _gridQueue.Rows[0].Selected = true;
    }

    private void RefreshMaterialsGrid()
    {
        _gridMaterials.Rows.Clear();

        var entry = _gridQueue.SelectedRows.Count > 0
            ? _gridQueue.SelectedRows[0].Tag as ManufactureQueueEntry
            : Globals.Botbase?.ManufactureBundleConfig?.Queue.FirstOrDefault();

        if (entry == null)
            return;

        var ar = AlchemyUi.IsArabic;
        var remaining = Math.Max(0, entry.TargetCount - entry.CompletedCount);

        foreach (var req in entry.Requirements)
        {
            var needed = Math.Max(1, remaining * req.RequiredPerCraft);
            var available = AlchemyItemHelper.GetItemCountById(req.ItemId);
            var ok = available >= needed;

            var rowIdx = _gridMaterials.Rows.Add(
                req.MaterialName,
                needed,
                available,
                ok ? (ar ? "متاح" : "OK") : (ar ? "ناقص" : "Missing"));

            _gridMaterials.Rows[rowIdx].Cells[3].Style.ForeColor =
                ok ? Color.FromArgb(16, 185, 129) : Color.FromArgb(239, 68, 68);
        }

        var rondoAvail = ManufactureBundle.GetRondoCount();
        var rondoOk = rondoAvail >= Math.Max(1, remaining);
        var rIdx = _gridMaterials.Rows.Add(
            ar ? "Void Rondo (للتصنيع)" : "Alchemy Rondo",
            Math.Max(1, remaining),
            rondoAvail,
            rondoOk ? (ar ? "متاح" : "OK") : (ar ? "ناقص" : "Missing"));
        _gridMaterials.Rows[rIdx].Cells[3].Style.ForeColor =
            rondoOk ? Color.FromArgb(16, 185, 129) : Color.FromArgb(239, 68, 68);
    }

    public void RefreshStatus()
    {
        var bundle = Globals.Botbase?.ManufactureBundle;
        var cfg = Globals.Botbase?.ManufactureBundleConfig;
        var isRunning = Bootstrap.IsActive && Globals.Botbase?.AlchemyEngine == AlchemyEngine.Manufacture;

        _btnStart.Enabled = !isRunning;
        _btnStop.Enabled = isRunning;
        _lblStatus.Text = $"Status: {(isRunning ? "Running" : "Stopped")}";
        _lblCurrent.Text = $"Current: {bundle?.CurrentItemName ?? "None"}";
        _lblManufactured.Text = $"Manufactured: {bundle?.ManufacturedCount ?? 0}";
        _lblFailed.Text = $"Failed: {bundle?.FailedCount ?? 0}";

        var totalTarget = cfg?.Queue?.Sum(q => q.TargetCount) ?? 0;
        var totalDone = cfg?.Queue?.Sum(q => q.CompletedCount) ?? 0;

        _progressBar.Maximum = Math.Max(1, totalTarget);
        _progressBar.Value = totalTarget > 0 ? Math.Min(totalDone, totalTarget) : 0;
    }

    public void ApplyLocalization()
    {
        var ar = AlchemyUi.IsArabic;
        _grpSettings.Text = ar ? "إعدادات" : "Settings";
        _grpStatus.Text = ar ? "الحالة" : "Status";
        _grpQueue.Text = ar ? "قائمة التصنيع" : "Manufacture Queue";
        _grpMaterials.Text = ar ? "الخامات" : "Materials";

        _chkAutoManufacture.Text = ar ? "فعل التصنيع التلقائي" : "Enable Auto Manufacture";
        _lblDelay.Text = ar ? "تأخير (مللي ثانية):" : "Delay (ms):";

        _gridQueue.Columns[0].HeaderText = ar ? "الإيتم" : "Item";
        _gridQueue.Columns[1].HeaderText = ar ? "الهدف" : "Target";
        _gridQueue.Columns[2].HeaderText = ar ? "تم" : "Done";
        _gridQueue.Columns[3].HeaderText = ar ? "الحالة" : "Status";

        _gridMaterials.Columns[0].HeaderText = ar ? "الخامة" : "Material";
        _gridMaterials.Columns[1].HeaderText = ar ? "مطلوب" : "Required";
        _gridMaterials.Columns[2].HeaderText = ar ? "متاح" : "Available";
        _gridMaterials.Columns[3].HeaderText = ar ? "الحالة" : "Status";

        _btnAdd.Text = ar ? "ضيف" : "Add";
        _btnRemove.Text = ar ? "شيل" : "Remove";
        _btnClearAll.Text = ar ? "مسح الكل" : "Clear All";
        _btnStart.Text = ar ? "ابدأ" : "Start";
        _btnStop.Text = ar ? "وقف" : "Stop";
        _btnCheckMaterials.Text = ar ? "راجع الخامات" : "Check Materials";

        RefreshAll();
    }

    public void ApplyTheme()
    {
        AlchemyUi.ApplyThemeRecursive(this);
        RefreshMaterialsGrid();
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
