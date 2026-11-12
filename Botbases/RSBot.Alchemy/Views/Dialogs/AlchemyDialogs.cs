using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RSBot.Alchemy.Bundle.Manufacture;
using RSBot.Alchemy.Client.ReferenceObjects;
using RSBot.Alchemy.Helper;
using RSBot.Alchemy.Models;
using RSBot.Core;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Extensions;
using RSBot.Core.Objects;
using SDUI;

using Button = SDUI.Controls.Button;
using CheckBox = SDUI.Controls.CheckBox;
using ComboBox = SDUI.Controls.ComboBox;
using Label = SDUI.Controls.Label;
using Radio = SDUI.Controls.Radio;

namespace RSBot.Alchemy.Views.Dialogs;

internal static class DialogFactory
{
    public static Form CreateBaseDialog(string title, int width, int height)
    {
        var form = new Form
        {
            Text = title,
            Size = new Size(width, height),
            StartPosition = FormStartPosition.CenterParent,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            ShowInTaskbar = false,
            BackColor = ColorScheme.BackColor,
            ForeColor = ColorScheme.ForeColor,
            Font = AlchemyUi.CreateFont(9f)
        };

        return form;
    }

    public static void WireCheckboxToggle(DataGridView grid)
    {
        grid.CurrentCellDirtyStateChanged += (_, _) =>
        {
            if (grid.IsCurrentCellDirty)
                grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
        };

        grid.CellClick += (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex == 0)
                return;

            var cur = Convert.ToBoolean(grid.Rows[e.RowIndex].Cells[0].Value);
            grid.Rows[e.RowIndex].Cells[0].Value = !cur;
        };
    }
}

internal sealed class MultiAddBlueSelection
{
    public string Group { get; set; }
    public string DisplayName { get; set; }
    public uint CustomTargetValue { get; set; }
}

internal sealed class MultiAddStatSelection
{
    public ItemAttributeGroup Group { get; set; }
    public string DisplayName { get; set; }
    public int TargetPercent { get; set; }
}

/// <summary>
///     Modal picker for adding multiple equipment items to the Multi-Item Alchemy Queue ("ضيف كذا"),
///     supporting Plus, Blues, and Stats in any combination.
/// </summary>
internal sealed class MultiAddEquipmentDialog : Form
{
    private readonly List<InventoryItem> _availableItems;
    private readonly Dictionary<string, (bool Checked, string TargetText)> _blueState = new(StringComparer.Ordinal);
    private readonly Dictionary<ItemAttributeGroup, (bool Checked, string TargetPercent)> _statState = new();

    private readonly CheckBox _chkModePlus;
    private readonly CheckBox _chkModeBlues;
    private readonly CheckBox _chkModeStats;
    private readonly SDUI.Controls.NumUpDown _numTargetPlus;
    private readonly ComboBox _comboStatPercent;

    private readonly DataGridView _gridItems;
    private readonly SDUI.Controls.TabControl _tabConfig;
    private readonly DataGridView _gridBlues;
    private readonly DataGridView _gridStats;

    private bool _suppressItemRebuild;
    private bool _suppressConfigCapture;

    public List<InventoryItem> SelectedItems { get; } = new();
    public bool AddPlus => _chkModePlus.Checked;
    public bool AddBlues => _chkModeBlues.Checked;
    public bool AddStats => _chkModeStats.Checked;
    public byte TargetPlus => (byte)_numTargetPlus.Value;
    public List<MultiAddBlueSelection> SelectedBlues { get; } = new();
    public List<MultiAddStatSelection> SelectedStats { get; } = new();

    public MultiAddEquipmentDialog(
        IEnumerable<InventoryItem> availableItems,
        AlchemyTargetMode initialMode = AlchemyTargetMode.Plus,
        byte initialTargetPlus = 5,
        string initialStatPercent = "22%",
        Dictionary<string, uint> initialCheckedBlues = null,
        Dictionary<ItemAttributeGroup, int> initialCheckedStats = null)
    {
        _availableItems = availableItems?.Where(i => i?.Record != null).ToList() ?? new List<InventoryItem>();

        if (initialCheckedBlues != null)
        {
            foreach (var kv in initialCheckedBlues)
                _blueState[kv.Key] = (true, kv.Value > 0 ? kv.Value.ToString() : "Max");
        }

        if (initialCheckedStats != null)
        {
            foreach (var kv in initialCheckedStats)
                _statState[kv.Key] = (true, NormalizePercentOption(kv.Value));
        }

        var ar = AlchemyUi.IsArabic;
        Text = AlchemyUi.T("Alchemy.Dialog.MultiAdd.Title", "إضافة عدة إيتمات للكمي", "Add Multiple Items");
        Size = new Size(860, 580);
        MinimumSize = new Size(760, 500);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        BackColor = ColorScheme.BackColor;
        ForeColor = ColorScheme.ForeColor;
        Font = AlchemyUi.CreateFont(9f);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(10),
            BackColor = Color.Transparent
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

        // 1. Top mode selector bar (Plus + Blues + Stats)
        var modeCard = CreateBorderedCard(new Padding(10, 8, 10, 8), new Padding(0, 0, 0, 8));
        var modeRow = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 9,
            RowCount = 1,
            BackColor = Color.Transparent
        };
        modeRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 118));
        modeRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 68));
        modeRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 54));
        modeRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 76));
        modeRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
        modeRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 78));
        modeRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 52));
        modeRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92));
        modeRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        modeRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var lblModes = AlchemyUi.CreateLabel(ar ? "أنماط الإضافة:" : "Add Modes:", ContentAlignment.MiddleLeft, bold: true);
        lblModes.Dock = DockStyle.Fill;

        _chkModePlus = AlchemyUi.CreateCheckBox("Plus", initialMode == AlchemyTargetMode.Plus);
        _chkModePlus.Dock = DockStyle.Fill;
        _chkModePlus.Font = AlchemyUi.CreateFont(9f, FontStyle.Bold);

        var lblPlusTarget = AlchemyUi.CreateLabel(ar ? "حتى +" : "To +", ContentAlignment.MiddleRight, secondary: true);
        lblPlusTarget.Dock = DockStyle.Fill;
        lblPlusTarget.Tag = "CustomFore";

        _numTargetPlus = AlchemyUi.CreateNumUpDown(0, 30, initialTargetPlus, 70);
        _numTargetPlus.Dock = DockStyle.Fill;
        _numTargetPlus.Margin = new Padding(4, 2, 12, 2);
        _numTargetPlus.Enabled = _chkModePlus.Checked;

        _chkModePlus.CheckedChanged += (_, _) => _numTargetPlus.Enabled = _chkModePlus.Checked;

        _chkModeBlues = AlchemyUi.CreateCheckBox("Blues", initialMode == AlchemyTargetMode.Blues);
        _chkModeBlues.Dock = DockStyle.Fill;
        _chkModeBlues.Font = AlchemyUi.CreateFont(9f, FontStyle.Bold);

        _chkModeStats = AlchemyUi.CreateCheckBox("Stats", initialMode == AlchemyTargetMode.Stats);
        _chkModeStats.Dock = DockStyle.Fill;
        _chkModeStats.Font = AlchemyUi.CreateFont(9f, FontStyle.Bold);

        var lblStatTarget = AlchemyUi.CreateLabel(ar ? "الهدف:" : "Target:", ContentAlignment.MiddleRight, secondary: true);
        lblStatTarget.Dock = DockStyle.Fill;
        lblStatTarget.Tag = "CustomFore";

        _comboStatPercent = AlchemyUi.CreateComboBox();
        _comboStatPercent.Dock = DockStyle.Fill;
        _comboStatPercent.Margin = new Padding(4, 1, 0, 1);
        _comboStatPercent.Items.AddRange(new object[] { "0%", "22%", "41%", "61%", "80%", "100%" });
        _comboStatPercent.SelectedItem = _comboStatPercent.Items.Contains(initialStatPercent) ? initialStatPercent : "22%";
        _comboStatPercent.Enabled = _chkModeStats.Checked;
        _comboStatPercent.SelectedIndexChanged += (_, _) => ApplyStatPercentToAllRows();

        modeRow.Controls.Add(lblModes, 0, 0);
        modeRow.Controls.Add(_chkModePlus, 1, 0);
        modeRow.Controls.Add(lblPlusTarget, 2, 0);
        modeRow.Controls.Add(_numTargetPlus, 3, 0);
        modeRow.Controls.Add(_chkModeBlues, 4, 0);
        modeRow.Controls.Add(_chkModeStats, 5, 0);
        modeRow.Controls.Add(lblStatTarget, 6, 0);
        modeRow.Controls.Add(_comboStatPercent, 7, 0);
        modeCard.Controls.Add(modeRow);

        // 2. Center split workspace: Equipment items (left) | Blues & Stats targets (right)
        var workspace = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent
        };
        workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 53));
        workspace.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 47));
        workspace.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var grpItems = AlchemyUi.CreateGroupBox(ar ? "اختيار الإيتمات" : "Select Equipment");
        grpItems.Dock = DockStyle.Fill;
        grpItems.Margin = new Padding(0, 0, 6, 0);

        _gridItems = AlchemyUi.CreateGrid(allowEdit: true);
        _gridItems.Columns.Add(new DataGridViewCheckBoxColumn
            { Name = "colCheck", HeaderText = "✓", Width = 34, AutoSizeMode = DataGridViewAutoSizeColumnMode.None });
        _gridItems.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "colSlot", HeaderText = ar ? "الخانة" : "Slot", ReadOnly = true, Width = 46, AutoSizeMode = DataGridViewAutoSizeColumnMode.None });
        _gridItems.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "colName", HeaderText = ar ? "الإيتم" : "Item", ReadOnly = true, FillWeight = 100 });
        _gridItems.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "colPlus", HeaderText = "Plus", ReadOnly = true, Width = 46, AutoSizeMode = DataGridViewAutoSizeColumnMode.None });
        _gridItems.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "colBlues", HeaderText = "Blues", ReadOnly = true, Width = 48, AutoSizeMode = DataGridViewAutoSizeColumnMode.None });
        _gridItems.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "colStats", HeaderText = "Stats", ReadOnly = true, Width = 52, AutoSizeMode = DataGridViewAutoSizeColumnMode.None });
        _gridItems.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "colDeg", HeaderText = "Deg", ReadOnly = true, Width = 38, AutoSizeMode = DataGridViewAutoSizeColumnMode.None });

        foreach (var item in _availableItems)
        {
            var blueCount = item.MagicOptions?.Count ?? 0;
            var rowIdx = _gridItems.Rows.Add(
                false,
                item.Slot,
                AlchemyItemHelper.GetItemDisplayName(item.Record),
                $"+{item.OptLevel}",
                blueCount.ToString(),
                FormatAvgStatPercent(item),
                item.Record.Degree);
            _gridItems.Rows[rowIdx].Tag = item;
        }

        DialogFactory.WireCheckboxToggle(_gridItems);
        _gridItems.CellValueChanged += (_, e) =>
        {
            if (!_suppressItemRebuild && e.RowIndex >= 0 && e.ColumnIndex == 0)
                RebuildConfigGrids();
        };

        grpItems.Controls.Add(_gridItems);

        // Right panel: Blues & Stats configuration tabs
        _tabConfig = AlchemyUi.CreateTabControl(100, 28);
        _tabConfig.Margin = new Padding(6, 0, 0, 0);

        var tabBlues = new TabPage("Blues")
        {
            Name = "tabMultiBlues",
            BackColor = ColorScheme.BackColor,
            Padding = new Padding(4),
            Margin = Padding.Empty
        };

        var tabStats = new TabPage("Stats")
        {
            Name = "tabMultiStats",
            BackColor = ColorScheme.BackColor,
            Padding = new Padding(4),
            Margin = Padding.Empty
        };

        (_gridBlues, var bluesPanel) = BuildConfigTabPanel(
            ar ? "تحديد المتاح" : "With Stones",
            ar ? "تحديد الكل" : "Select All",
            ar ? "إلغاء الكل" : "Clear",
            () => SelectAvailableBlues(),
            () => SetAllConfigRowsChecked(_gridBlues, true),
            () => SetAllConfigRowsChecked(_gridBlues, false));

        _gridBlues.Columns.Add(new DataGridViewCheckBoxColumn
            { Name = "colBUse", HeaderText = "✓", Width = 32, AutoSizeMode = DataGridViewAutoSizeColumnMode.None });
        _gridBlues.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "colBName", HeaderText = ar ? "الـ Blue" : "Blue", ReadOnly = true, FillWeight = 52 });
        _gridBlues.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "colBTarget", HeaderText = ar ? "الهدف" : "Target", ReadOnly = false, FillWeight = 24 });
        _gridBlues.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "colBStones", HeaderText = ar ? "الأحجار" : "Stones", ReadOnly = true, FillWeight = 24 });
        WireSafeConfigToggle(_gridBlues, editableColIndex: 2);
        WireGridEditingTheme(_gridBlues);
        _gridBlues.CellValueChanged += (_, e) =>
        {
            if (!_suppressConfigCapture && e.RowIndex >= 0)
                CaptureBlueRowState(_gridBlues.Rows[e.RowIndex]);
        };
        tabBlues.Controls.Add(bluesPanel);

        (_gridStats, var statsPanel) = BuildConfigTabPanel(
            ar ? "تحديد المتاح" : "With Stones",
            ar ? "تحديد الكل" : "Select All",
            ar ? "إلغاء الكل" : "Clear",
            () => SelectAvailableStats(),
            () => SetAllConfigRowsChecked(_gridStats, true),
            () => SetAllConfigRowsChecked(_gridStats, false));

        _gridStats.Columns.Add(new DataGridViewCheckBoxColumn
            { Name = "colSUse", HeaderText = "✓", Width = 32, AutoSizeMode = DataGridViewAutoSizeColumnMode.None });
        _gridStats.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "colSName", HeaderText = ar ? "الـ Stat" : "Stat", ReadOnly = true, FillWeight = 48 });

        var colStatTarget = new DataGridViewComboBoxColumn
        {
            Name = "colSTarget",
            HeaderText = ar ? "الهدف" : "Target",
            FillWeight = 28,
            FlatStyle = FlatStyle.Flat
        };
        colStatTarget.Items.AddRange("0%", "22%", "41%", "61%", "80%", "100%");
        _gridStats.Columns.Add(colStatTarget);

        _gridStats.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "colSStones", HeaderText = ar ? "الأحجار" : "Stones", ReadOnly = true, FillWeight = 24 });
        WireSafeConfigToggle(_gridStats, editableColIndex: 2);
        WireGridEditingTheme(_gridStats);
        _gridStats.CellValueChanged += (_, e) =>
        {
            if (!_suppressConfigCapture && e.RowIndex >= 0)
                CaptureStatRowState(_gridStats.Rows[e.RowIndex]);
        };
        tabStats.Controls.Add(statsPanel);

        _tabConfig.TabPages.Add(tabBlues);
        _tabConfig.TabPages.Add(tabStats);

        _chkModeBlues.CheckedChanged += (_, _) =>
        {
            if (_chkModeBlues.Checked)
                _tabConfig.SelectedTab = tabBlues;
        };
        _chkModeStats.CheckedChanged += (_, _) =>
        {
            _comboStatPercent.Enabled = _chkModeStats.Checked;
            if (_chkModeStats.Checked)
                _tabConfig.SelectedTab = tabStats;
        };

        if (initialMode == AlchemyTargetMode.Stats)
            _tabConfig.SelectedTab = tabStats;

        workspace.Controls.Add(grpItems, 0, 0);
        workspace.Controls.Add(_tabConfig, 1, 0);

        // 3. Bottom action bar
        var bottomBar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 5,
            RowCount = 1,
            Padding = new Padding(0, 6, 0, 0),
            BackColor = Color.Transparent
        };
        bottomBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112));
        bottomBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112));
        bottomBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        bottomBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 136));
        bottomBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96));
        bottomBar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var btnSelectAll = AlchemyUi.CreateButton(ar ? "تحديد الكل" : "Select All", ButtonTone.Neutral, 32);
        btnSelectAll.Dock = DockStyle.Fill;
        btnSelectAll.Margin = new Padding(0, 2, 4, 2);
        btnSelectAll.Click += (_, _) => SetAllItemsChecked(true);

        var btnClearAll = AlchemyUi.CreateButton(ar ? "إلغاء الكل" : "Clear All", ButtonTone.Neutral, 32);
        btnClearAll.Dock = DockStyle.Fill;
        btnClearAll.Margin = new Padding(2, 2, 4, 2);
        btnClearAll.Click += (_, _) => SetAllItemsChecked(false);

        var btnAdd = AlchemyUi.CreateButton(ar ? "إضافة المحدد" : "Add Selected", ButtonTone.Success, 32);
        btnAdd.Dock = DockStyle.Fill;
        btnAdd.Margin = new Padding(4, 2, 4, 2);
        btnAdd.Click += (_, _) => ConfirmSelection(tabBlues, tabStats);

        var btnCancel = AlchemyUi.CreateButton(ar ? "إلغاء" : "Cancel", ButtonTone.Neutral, 32);
        btnCancel.Dock = DockStyle.Fill;
        btnCancel.Margin = new Padding(4, 2, 0, 2);
        btnCancel.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        bottomBar.Controls.Add(btnSelectAll, 0, 0);
        bottomBar.Controls.Add(btnClearAll, 1, 0);
        bottomBar.Controls.Add(btnAdd, 3, 0);
        bottomBar.Controls.Add(btnCancel, 4, 0);

        root.Controls.Add(modeCard, 0, 0);
        root.Controls.Add(workspace, 0, 1);
        root.Controls.Add(bottomBar, 0, 2);

        Controls.Add(root);
        RebuildConfigGrids();
    }

    private void ConfirmSelection(TabPage tabBlues, TabPage tabStats)
    {
        var ar = AlchemyUi.IsArabic;

        _gridItems.EndEdit();
        _gridBlues.EndEdit();
        _gridStats.EndEdit();

        SelectedItems.Clear();
        foreach (DataGridViewRow row in _gridItems.Rows)
        {
            if (Convert.ToBoolean(row.Cells[0].Value) && row.Tag is InventoryItem invItem)
                SelectedItems.Add(invItem);
        }

        if (SelectedItems.Count == 0)
        {
            MessageBox.Show(
                ar ? "حدد إيتم واحد على الأقل." : "Select at least one item.",
                "Alchemy", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (!AddPlus && !AddBlues && !AddStats)
        {
            MessageBox.Show(
                ar ? "اختر نمط واحد على الأقل (Plus أو Blues أو Stats)." : "Select at least one mode (Plus, Blues, or Stats).",
                "Alchemy", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        SelectedBlues.Clear();
        if (AddBlues)
        {
            foreach (DataGridViewRow row in _gridBlues.Rows)
            {
                if (!Convert.ToBoolean(row.Cells[0].Value) || row.Tag is not string group)
                    continue;

                SelectedBlues.Add(new MultiAddBlueSelection
                {
                    Group = group,
                    DisplayName = row.Cells[1].Value?.ToString() ?? group,
                    CustomTargetValue = ParseBlueTarget(row.Cells[2].Value?.ToString())
                });
            }

            if (SelectedBlues.Count == 0)
            {
                _tabConfig.SelectedTab = tabBlues;
                MessageBox.Show(
                    ar ? "حدد Blue واحد على الأقل من تبويب Blues." : "Select at least one Blue in the Blues tab.",
                    "Blues", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var blueGroups = SelectedBlues.Select(b => b.Group).ToHashSet(StringComparer.Ordinal);
            var anySupportedBlue = SelectedItems.Any(item =>
                (Game.ReferenceManager.GetAssignments(item.Record.TypeID3, item.Record.TypeID4) ?? Enumerable.Empty<RefMagicOpt>())
                .Any(a => a != null && blueGroups.Contains(a.Group)));

            if (!anySupportedBlue)
            {
                _tabConfig.SelectedTab = tabBlues;
                MessageBox.Show(
                    ar ? "الـ Blues المحددة لا تنطبق على الإيتمات المختارة." : "None of the selected Blues apply to the chosen equipment.",
                    "Blues", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
        }

        SelectedStats.Clear();
        if (AddStats)
        {
            foreach (DataGridViewRow row in _gridStats.Rows)
            {
                if (!Convert.ToBoolean(row.Cells[0].Value) || row.Tag is not ItemAttributeGroup grp)
                    continue;

                SelectedStats.Add(new MultiAddStatSelection
                {
                    Group = grp,
                    DisplayName = row.Cells[1].Value?.ToString() ?? grp.GetTranslation(),
                    TargetPercent = ParsePercent(row.Cells[2].Value?.ToString())
                });
            }

            if (SelectedStats.Count == 0)
            {
                _tabConfig.SelectedTab = tabStats;
                MessageBox.Show(
                    ar ? "حدد Stat واحدة على الأقل من تبويب Stats." : "Select at least one Stat in the Stats tab.",
                    "Stats", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var statGroups = SelectedStats.Select(s => s.Group).ToHashSet();
            var anySupportedStat = SelectedItems.Any(item =>
                (ItemAttributesInfo.GetAvailableAttributeGroupsForItem(item.Record) ?? Enumerable.Empty<ItemAttributeGroup>())
                .Any(statGroups.Contains));

            if (!anySupportedStat)
            {
                _tabConfig.SelectedTab = tabStats;
                MessageBox.Show(
                    ar ? "الـ Stats المحددة لا تنطبق على الإيتمات المختارة." : "None of the selected Stats apply to the chosen equipment.",
                    "Stats", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
        }

        if (AddPlus && !AddBlues && !AddStats && SelectedItems.All(i => i.OptLevel >= TargetPlus))
        {
            MessageBox.Show(
                ar ? $"كل الإيتمات المحددة وصلت بالفعل إلى +{TargetPlus} أو أعلى." : $"All selected items are already at +{TargetPlus} or higher.",
                "Plus", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private List<InventoryItem> GetScopeItems()
    {
        var checkedItems = new List<InventoryItem>();
        foreach (DataGridViewRow row in _gridItems.Rows)
        {
            if (Convert.ToBoolean(row.Cells[0].Value) && row.Tag is InventoryItem item)
                checkedItems.Add(item);
        }

        return checkedItems.Count > 0 ? checkedItems : _availableItems;
    }

    private void RebuildConfigGrids()
    {
        CaptureConfigStates();
        _suppressConfigCapture = true;

        try
        {
            var scopeItems = GetScopeItems();
            var degrees = scopeItems.Select(i => (byte)i.Record.Degree).Distinct().ToList();

            // Rebuild Blues grid for the checked (or all) equipment items
            _gridBlues.Rows.Clear();
            var seenBlueGroups = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in scopeItems)
            {
                var assignments = Game.ReferenceManager.GetAssignments(item.Record.TypeID3, item.Record.TypeID4);
                if (assignments == null)
                    continue;

                foreach (var assignment in assignments)
                {
                    if (assignment == null || string.IsNullOrEmpty(assignment.Group) || !seenBlueGroups.Add(assignment.Group))
                        continue;

                    var refMagic = Game.ReferenceManager.GetMagicOption(assignment.Group, (byte)item.Record.Degree);
                    if (refMagic == null)
                        continue;

                    var stones = degrees.Sum(deg => AlchemyItemHelper.GetStonesByGroup(deg, assignment.Group).Sum(s => (int)s.Amount));
                    var hasSaved = _blueState.TryGetValue(assignment.Group, out var saved);
                    var isChecked = hasSaved && saved.Checked;
                    var targetText = hasSaved && !string.IsNullOrWhiteSpace(saved.TargetText) ? saved.TargetText : "Max";

                    var name = assignment.GetGroupTranslation();
                    if (string.IsNullOrWhiteSpace(name))
                        name = assignment.Group;

                    var idx = _gridBlues.Rows.Add(isChecked, name, targetText, $"x{stones}");
                    var row = _gridBlues.Rows[idx];
                    row.Tag = assignment.Group;
                    row.Cells[3].Style.ForeColor = stones > 0
                        ? Color.FromArgb(16, 185, 129)
                        : Color.FromArgb(220, 72, 72);
                }
            }

            // Rebuild Stats grid for the checked (or all) equipment items
            _gridStats.Rows.Clear();
            var defaultPct = _comboStatPercent.SelectedItem?.ToString() ?? "22%";
            var seenStatGroups = new HashSet<ItemAttributeGroup>();
            foreach (var item in scopeItems)
            {
                var groups = ItemAttributesInfo.GetAvailableAttributeGroupsForItem(item.Record);
                if (groups == null)
                    continue;

                foreach (var grp in groups)
                {
                    if (!seenStatGroups.Add(grp))
                        continue;

                    var stones = scopeItems
                        .SelectMany(i => AlchemyItemHelper.GetAttributeStones(i, grp))
                        .GroupBy(s => s.Slot)
                        .Sum(g => (int)g.First().Amount);

                    var hasSaved = _statState.TryGetValue(grp, out var saved);
                    var isChecked = hasSaved && saved.Checked;
                    var targetPct = hasSaved && !string.IsNullOrWhiteSpace(saved.TargetPercent)
                        ? saved.TargetPercent
                        : defaultPct;

                    var idx = _gridStats.Rows.Add(isChecked, grp.GetTranslation(), targetPct, $"x{stones}");
                    var row = _gridStats.Rows[idx];
                    row.Tag = grp;
                    row.Cells[3].Style.ForeColor = stones > 0
                        ? Color.FromArgb(16, 185, 129)
                        : Color.FromArgb(220, 72, 72);
                }
            }
        }
        finally
        {
            _suppressConfigCapture = false;
        }
    }

    private void CaptureConfigStates()
    {
        if (_gridBlues != null)
        {
            foreach (DataGridViewRow row in _gridBlues.Rows)
                CaptureBlueRowState(row);
        }

        if (_gridStats != null)
        {
            foreach (DataGridViewRow row in _gridStats.Rows)
                CaptureStatRowState(row);
        }
    }

    private void CaptureBlueRowState(DataGridViewRow row)
    {
        if (row?.Tag is not string group)
            return;

        _blueState[group] = (
            Convert.ToBoolean(row.Cells[0].Value),
            row.Cells[2].Value?.ToString() ?? "Max");
    }

    private void CaptureStatRowState(DataGridViewRow row)
    {
        if (row?.Tag is not ItemAttributeGroup grp)
            return;

        _statState[grp] = (
            Convert.ToBoolean(row.Cells[0].Value),
            row.Cells[2].Value?.ToString() ?? "22%");
    }

    private void SelectAvailableBlues()
    {
        var scopeItems = GetScopeItems();
        var degrees = scopeItems.Select(i => (byte)i.Record.Degree).Distinct().ToList();

        foreach (DataGridViewRow row in _gridBlues.Rows)
        {
            if (row.Tag is not string group)
                continue;

            var stones = degrees.Sum(deg => AlchemyItemHelper.GetStonesByGroup(deg, group).Sum(s => (int)s.Amount));
            row.Cells[0].Value = stones > 0;
        }
    }

    private void SelectAvailableStats()
    {
        var scopeItems = GetScopeItems();
        foreach (DataGridViewRow row in _gridStats.Rows)
        {
            if (row.Tag is not ItemAttributeGroup grp)
                continue;

            var hasStones = scopeItems.Any(item => AlchemyItemHelper.GetAttributeStones(item, grp).Any());
            row.Cells[0].Value = hasStones;
        }
    }

    private static void SetAllConfigRowsChecked(DataGridView grid, bool check)
    {
        foreach (DataGridViewRow row in grid.Rows)
            row.Cells[0].Value = check;
    }

    private void ApplyStatPercentToAllRows()
    {
        var pct = _comboStatPercent.SelectedItem?.ToString() ?? "22%";
        foreach (DataGridViewRow row in _gridStats.Rows)
            row.Cells[2].Value = pct;
    }

    private void SetAllItemsChecked(bool check)
    {
        _suppressItemRebuild = true;
        try
        {
            foreach (DataGridViewRow row in _gridItems.Rows)
                row.Cells[0].Value = check;
        }
        finally
        {
            _suppressItemRebuild = false;
        }

        RebuildConfigGrids();
    }

    private static (DataGridView grid, Panel container) BuildConfigTabPanel(
        string btn1Text,
        string btn2Text,
        string btn3Text,
        System.Action onBtn1,
        System.Action onBtn2,
        System.Action onBtn3)
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = Color.Transparent
        };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var bar = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = new Padding(0, 0, 0, 4),
            BackColor = Color.Transparent
        };
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36F));
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
        bar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
        bar.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var b1 = AlchemyUi.CreateButton(btn1Text, ButtonTone.Neutral, 28);
        b1.Dock = DockStyle.Fill;
        b1.Margin = new Padding(0, 1, 3, 1);
        b1.Click += (_, _) => onBtn1();

        var b2 = AlchemyUi.CreateButton(btn2Text, ButtonTone.Neutral, 28);
        b2.Dock = DockStyle.Fill;
        b2.Margin = new Padding(2, 1, 2, 1);
        b2.Click += (_, _) => onBtn2();

        var b3 = AlchemyUi.CreateButton(btn3Text, ButtonTone.Neutral, 28);
        b3.Dock = DockStyle.Fill;
        b3.Margin = new Padding(3, 1, 0, 1);
        b3.Click += (_, _) => onBtn3();

        bar.Controls.Add(b1, 0, 0);
        bar.Controls.Add(b2, 1, 0);
        bar.Controls.Add(b3, 2, 0);

        var grid = AlchemyUi.CreateGrid(allowEdit: true);
        panel.Controls.Add(bar, 0, 0);
        panel.Controls.Add(grid, 0, 1);
        return (grid, panel);
    }

    private static Panel CreateBorderedCard(Padding padding, Padding margin)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = padding,
            Margin = margin,
            BackColor = ColorScheme.CardColor
        };
        panel.Paint += (_, e) =>
        {
            using var pen = new Pen(ColorScheme.BorderColor, 1f);
            e.Graphics.DrawRectangle(pen, 0, 0, Math.Max(1, panel.Width - 1), Math.Max(1, panel.Height - 1));
        };
        return panel;
    }

    private static void WireSafeConfigToggle(DataGridView grid, int editableColIndex)
    {
        grid.CellClick += (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex == 0 || e.ColumnIndex == editableColIndex)
                return;

            var cur = Convert.ToBoolean(grid.Rows[e.RowIndex].Cells[0].Value);
            grid.Rows[e.RowIndex].Cells[0].Value = !cur;
        };
    }

    private static void WireGridEditingTheme(DataGridView grid)
    {
        grid.EditingControlShowing += (_, e) =>
        {
            if (e.Control == null)
                return;

            e.Control.BackColor = ColorScheme.InputBackColor;
            e.Control.ForeColor = ColorScheme.ForeColor;
        };
    }

    private static uint ParseBlueTarget(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        var trimmed = text.Trim();
        if (string.Equals(trimmed, "Max", StringComparison.OrdinalIgnoreCase))
            return 0;

        return uint.TryParse(trimmed, out var val) ? val : 0;
    }

    private static int ParsePercent(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 22;

        return int.TryParse(text.Replace("%", string.Empty).Trim(), out var v) ? v : 22;
    }

    private static string NormalizePercentOption(int percent) => percent switch
    {
        <= 0 => "0%",
        <= 22 => "22%",
        <= 41 => "41%",
        <= 61 => "61%",
        <= 80 => "80%",
        _ => "100%"
    };

    private static string FormatAvgStatPercent(InventoryItem item)
    {
        if (item?.Record == null)
            return "--";

        var groups = ItemAttributesInfo.GetAvailableAttributeGroupsForItem(item.Record)?.ToList();
        if (groups == null || groups.Count == 0)
            return "--";

        var sum = 0;
        foreach (var grp in groups)
        {
            var slot = ItemAttributesInfo.GetAttributeSlotForItem(grp, item.Record);
            sum += item.Attributes.GetPercentage(slot);
        }

        return $"{sum / groups.Count}%";
    }
}

/// <summary>
///     Modal dialog for adding a Manufacture craft task from inventory alchemy materials ("ضيف").
/// </summary>
internal sealed class ManufactureAddDialog : Form
{
    private readonly Radio _radioResolve;
    private readonly Radio _radioCompound;
    private readonly DataGridView _grid;
    private readonly SDUI.Controls.NumUpDown _numTarget;
    private readonly Button _btnOk;
    private readonly Button _btnCancel;

    public ManufactureQueueEntry CreatedEntry { get; private set; }

    public ManufactureAddDialog()
    {
        Text = AlchemyUi.IsArabic ? "إضافة تصنيع جديد" : "Add Manufacture Task";
        Size = new Size(520, 420);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        BackColor = ColorScheme.BackColor;
        ForeColor = ColorScheme.ForeColor;
        Font = AlchemyUi.CreateFont(9f);

        _radioResolve = AlchemyUi.CreateRadio(AlchemyUi.IsArabic ? "تفكيك خامات (حتى 4)" : "Resolve Materials (max 4)", true);
        _radioResolve.Width = 190;
        _radioCompound = AlchemyUi.CreateRadio(AlchemyUi.IsArabic ? "تركيب خامة (1)" : "Compound Material (1)", false);
        _radioCompound.Width = 170;

        var modePanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 38,
            Padding = new Padding(8, 6, 8, 2),
            BackColor = Color.Transparent
        };
        modePanel.Controls.AddRange(new Control[] { _radioResolve, _radioCompound });

        _grid = AlchemyUi.CreateGrid(allowEdit: true);
        _grid.Columns.Add(new DataGridViewCheckBoxColumn
            { Name = "colSel", HeaderText = "✓", Width = 36, AutoSizeMode = DataGridViewAutoSizeColumnMode.None });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "colName", HeaderText = AlchemyUi.IsArabic ? "الخامة" : "Material", ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "colAmt", HeaderText = AlchemyUi.IsArabic ? "المتاح" : "Available", ReadOnly = true, Width = 75, AutoSizeMode = DataGridViewAutoSizeColumnMode.None });

        foreach (var group in AlchemyItemHelper.GetManufactureMaterials().GroupBy(m => m.ItemId))
        {
            var first = group.First();
            var total = group.Sum(x => (int)x.Amount);
            var displayName = AlchemyItemHelper.GetItemDisplayName(first.Record);
            var idx = _grid.Rows.Add(false, displayName, total);
            _grid.Rows[idx].Tag = first;
        }

        DialogFactory.WireCheckboxToggle(_grid);

        _grid.CellValueChanged += (_, e) =>
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 0 && _radioCompound.Checked)
            {
                if (Convert.ToBoolean(_grid.Rows[e.RowIndex].Cells[0].Value))
                {
                    for (var i = 0; i < _grid.Rows.Count; i++)
                    {
                        if (i != e.RowIndex)
                            _grid.Rows[i].Cells[0].Value = false;
                    }
                }
            }
        };

        _radioCompound.CheckedChanged += (_, _) =>
        {
            if (!_radioCompound.Checked)
                return;

            var foundFirst = false;
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (!Convert.ToBoolean(row.Cells[0].Value))
                    continue;

                if (!foundFirst)
                    foundFirst = true;
                else
                    row.Cells[0].Value = false;
            }
        };

        var lblTarget = AlchemyUi.CreateLabel(AlchemyUi.IsArabic ? "عدد المرات (الهدف):" : "Target Count:", ContentAlignment.MiddleLeft);
        lblTarget.Width = 120;
        _numTarget = AlchemyUi.CreateNumUpDown(1, 9999, 10, 74);

        var btnSelectAll = AlchemyUi.CreateButton(AlchemyUi.IsArabic ? "تحديد" : "Select All", ButtonTone.Neutral, 30);
        btnSelectAll.Width = 78;
        btnSelectAll.Click += (_, _) =>
        {
            var maxCount = _radioCompound.Checked ? 1 : 4;
            var count = 0;
            foreach (DataGridViewRow row in _grid.Rows)
            {
                var check = count < maxCount;
                row.Cells[0].Value = check;
                if (check) count++;
            }
        };

        _btnOk = AlchemyUi.CreateButton(AlchemyUi.IsArabic ? "إضافة" : "Add", ButtonTone.Success, 30);
        _btnOk.Width = 84;
        _btnOk.Click += (_, _) =>
        {
            _grid.EndEdit();
            var selected = new List<InventoryItem>();
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (Convert.ToBoolean(row.Cells[0].Value) && row.Tag is InventoryItem item)
                    selected.Add(item);
            }

            if (selected.Count == 0)
                return;

            var maxAllowed = _radioCompound.Checked ? 1 : 4;
            selected = selected.Take(maxAllowed).ToList();

            CreatedEntry = new ManufactureQueueEntry
            {
                Mode = _radioCompound.Checked ? ManufactureMode.Compound : ManufactureMode.Resolve,
                ItemName = string.Join(" + ", selected.Select(s => AlchemyItemHelper.GetItemDisplayName(s.Record))),
                TargetCount = (int)_numTarget.Value,
                Requirements = selected.Select(s => new ManufactureMaterialRequirement
                {
                    ItemId = s.ItemId,
                    CodeName = s.Record.CodeName,
                    MaterialName = AlchemyItemHelper.GetItemDisplayName(s.Record),
                    RequiredPerCraft = 1
                }).ToList()
            };

            DialogResult = DialogResult.OK;
            Close();
        };

        _btnCancel = AlchemyUi.CreateButton(AlchemyUi.IsArabic ? "إلغاء" : "Cancel", ButtonTone.Neutral, 30);
        _btnCancel.Width = 80;
        _btnCancel.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        var bottom = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 46,
            Padding = new Padding(8, 6, 8, 6),
            BackColor = Color.Transparent
        };
        bottom.Controls.AddRange(new Control[] { lblTarget, _numTarget, btnSelectAll, _btnOk, _btnCancel });

        var center = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8), BackColor = Color.Transparent };
        center.Controls.Add(_grid);

        Controls.Add(center);
        Controls.Add(modePanel);
        Controls.Add(bottom);
    }
}

/// <summary>
///     Modal picker for adding specific inventory items to the Dismantle filter list ("ضيف").
/// </summary>
internal sealed class DismantleAddItemDialog : Form
{
    private readonly DataGridView _grid;
    private readonly SDUI.Controls.NumUpDown _numPriority;

    public List<DismantleFilterRule> CreatedRules { get; } = new();

    public DismantleAddItemDialog()
    {
        Text = AlchemyUi.IsArabic ? "إضافة إيتمات للتكسير" : "Add Items to Dismantle";
        Size = new Size(520, 400);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        BackColor = ColorScheme.BackColor;
        ForeColor = ColorScheme.ForeColor;
        Font = AlchemyUi.CreateFont(9f);

        _grid = AlchemyUi.CreateGrid(allowEdit: true);
        _grid.Columns.Add(new DataGridViewCheckBoxColumn
            { Name = "colSel", HeaderText = "✓", Width = 36, AutoSizeMode = DataGridViewAutoSizeColumnMode.None });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "colName", HeaderText = "Item Name", ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "colPlus", HeaderText = "+", ReadOnly = true, Width = 50, AutoSizeMode = DataGridViewAutoSizeColumnMode.None });
        _grid.Columns.Add(new DataGridViewTextBoxColumn
            { Name = "colSlot", HeaderText = "Slot", ReadOnly = true, Width = 50, AutoSizeMode = DataGridViewAutoSizeColumnMode.None });

        foreach (var eq in AlchemyItemHelper.GetEligibleEquipmentItems())
        {
            var idx = _grid.Rows.Add(false, AlchemyItemHelper.GetItemDisplayName(eq.Record), $"+{eq.OptLevel}", eq.Slot);
            _grid.Rows[idx].Tag = eq;
        }

        DialogFactory.WireCheckboxToggle(_grid);

        var lblPri = AlchemyUi.CreateLabel(AlchemyUi.IsArabic ? "الأولوية:" : "Priority:", ContentAlignment.MiddleLeft);
        lblPri.Width = 65;
        _numPriority = AlchemyUi.CreateNumUpDown(1, 99, 1, 65);

        var btnSelectAll = AlchemyUi.CreateButton(AlchemyUi.IsArabic ? "تحديد الكل" : "Select All", ButtonTone.Neutral, 30);
        btnSelectAll.Width = 90;
        btnSelectAll.Click += (_, _) =>
        {
            foreach (DataGridViewRow row in _grid.Rows)
                row.Cells[0].Value = true;
        };

        var btnAdd = AlchemyUi.CreateButton(AlchemyUi.IsArabic ? "إضافة" : "Add", ButtonTone.Success, 30);
        btnAdd.Width = 88;
        btnAdd.Click += (_, _) =>
        {
            _grid.EndEdit();
            CreatedRules.Clear();
            foreach (DataGridViewRow row in _grid.Rows)
            {
                if (Convert.ToBoolean(row.Cells[0].Value) && row.Tag is InventoryItem eq)
                {
                    CreatedRules.Add(new DismantleFilterRule
                    {
                        SpecificItemId = eq.ItemId,
                        SpecificSlot = eq.Slot,
                        ItemName = AlchemyItemHelper.GetItemDisplayName(eq.Record),
                        Priority = (int)_numPriority.Value
                    });
                }
            }

            if (CreatedRules.Count == 0)
                return;

            DialogResult = DialogResult.OK;
            Close();
        };

        var btnCancel = AlchemyUi.CreateButton(AlchemyUi.IsArabic ? "إلغاء" : "Cancel", ButtonTone.Neutral, 30);
        btnCancel.Width = 82;
        btnCancel.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        var bottom = new FlowLayoutPanel
            { Dock = DockStyle.Bottom, Height = 46, Padding = new Padding(8, 6, 8, 6), BackColor = Color.Transparent };
        bottom.Controls.AddRange(new Control[] { lblPri, _numPriority, btnSelectAll, btnAdd, btnCancel });

        var center = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8), BackColor = Color.Transparent };
        center.Controls.Add(_grid);

        Controls.Add(center);
        Controls.Add(bottom);
    }
}

/// <summary>
///     Modal dialog for adding a dismantle filter rule by equipment type & degree ("ضيف بالنوع").
/// </summary>
internal sealed class DismantleAddTypeDialog : Form
{
    private sealed class TypeOption
    {
        public string Name { get; }
        public byte[] TypeId3Set { get; }

        public TypeOption(string name, params byte[] typeId3Set)
        {
            Name = name;
            TypeId3Set = typeId3Set is { Length: > 0 } ? typeId3Set : null;
        }

        public override string ToString() => Name;
    }

    public DismantleFilterRule CreatedRule { get; private set; }

    public DismantleAddTypeDialog()
    {
        Text = AlchemyUi.IsArabic ? "إضافة بالنوع للتكسير" : "Add Dismantle Rule by Type";
        Size = new Size(390, 240);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        BackColor = ColorScheme.BackColor;
        ForeColor = ColorScheme.ForeColor;
        Font = AlchemyUi.CreateFont(9f);

        var comboType = AlchemyUi.CreateComboBox();
        comboType.Items.Add(new TypeOption(AlchemyUi.IsArabic ? "كل الأسلحة (Weapons)" : "Weapons", 6));
        comboType.Items.Add(new TypeOption(AlchemyUi.IsArabic ? "كل الدروع (All Armor CH & EU)" : "All Armor (CH & EU)", 1, 2, 3, 9, 10, 11));
        comboType.Items.Add(new TypeOption(AlchemyUi.IsArabic ? "شيلد (Shield)" : "Shield", 4));
        comboType.Items.Add(new TypeOption(AlchemyUi.IsArabic ? "كل الإكسسوارات (All Accessory)" : "All Accessory", 5, 12));
        comboType.Items.Add(new TypeOption(AlchemyUi.IsArabic ? "درع قماش CH (Garment)" : "CH Garment", 1));
        comboType.Items.Add(new TypeOption(AlchemyUi.IsArabic ? "بروتكتر CH (Protector)" : "CH Protector", 2));
        comboType.Items.Add(new TypeOption(AlchemyUi.IsArabic ? "آرمور CH (Armor)" : "CH Armor", 3));
        comboType.Items.Add(new TypeOption(AlchemyUi.IsArabic ? "روب EU (Robe)" : "EU Robe", 9));
        comboType.Items.Add(new TypeOption(AlchemyUi.IsArabic ? "لايت آرمور EU (Light Armor)" : "EU Light Armor", 10));
        comboType.Items.Add(new TypeOption(AlchemyUi.IsArabic ? "هيفي آرمور EU (Heavy Armor)" : "EU Heavy Armor", 11));
        comboType.Items.Add(new TypeOption(AlchemyUi.IsArabic ? "إكسسوار CH (CH Accessory)" : "CH Accessory", 5));
        comboType.Items.Add(new TypeOption(AlchemyUi.IsArabic ? "إكسسوار EU (EU Accessory)" : "EU Accessory", 12));
        comboType.Items.Add(new TypeOption(AlchemyUi.IsArabic ? "كل الأنواع (All Types)" : "All Types"));
        comboType.SelectedIndex = 0;

        var chkAnyDeg = AlchemyUi.CreateCheckBox(AlchemyUi.IsArabic ? "كل الدرجات" : "Any Degree", true);
        var numDegree = AlchemyUi.CreateNumUpDown(1, 15, 11, 70);
        numDegree.Enabled = false;
        chkAnyDeg.CheckedChanged += (_, _) => numDegree.Enabled = !chkAnyDeg.Checked;

        var numPriority = AlchemyUi.CreateNumUpDown(1, 99, 1, 70);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4,
            Padding = new Padding(14),
            BackColor = Color.Transparent
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56));

        layout.Controls.Add(AlchemyUi.CreateLabel(AlchemyUi.IsArabic ? "النوع:" : "Type:"), 0, 0);
        comboType.Dock = DockStyle.Fill;
        layout.Controls.Add(comboType, 1, 0);

        chkAnyDeg.Width = 120;
        layout.Controls.Add(chkAnyDeg, 0, 1);
        layout.Controls.Add(numDegree, 1, 1);

        layout.Controls.Add(AlchemyUi.CreateLabel(AlchemyUi.IsArabic ? "الأولوية:" : "Priority:"), 0, 2);
        layout.Controls.Add(numPriority, 1, 2);

        var btnAdd = AlchemyUi.CreateButton(AlchemyUi.IsArabic ? "إضافة" : "Add", ButtonTone.Success, 30);
        btnAdd.Width = 90;
        btnAdd.Click += (_, _) =>
        {
            var typeOpt = (TypeOption)comboType.SelectedItem;
            byte? deg = chkAnyDeg.Checked ? null : (byte)numDegree.Value;
            var degLabel = deg.HasValue ? $"D{deg.Value}" : (AlchemyUi.IsArabic ? "كل الدرجات" : "All Deg");

            CreatedRule = new DismantleFilterRule
            {
                TypeId3 = typeOpt.TypeId3Set?.Length == 1 ? typeOpt.TypeId3Set[0] : null,
                TypeId3Set = typeOpt.TypeId3Set,
                Degree = deg,
                ItemName = $"{typeOpt.Name} [{degLabel}]",
                Priority = (int)numPriority.Value
            };

            DialogResult = DialogResult.OK;
            Close();
        };

        var btnCancel = AlchemyUi.CreateButton(AlchemyUi.IsArabic ? "إلغاء" : "Cancel", ButtonTone.Neutral, 30);
        btnCancel.Width = 85;
        btnCancel.Click += (_, _) =>
        {
            DialogResult = DialogResult.Cancel;
            Close();
        };

        var buttons = new FlowLayoutPanel
            { Dock = DockStyle.Bottom, Height = 46, Padding = new Padding(12, 6, 12, 6), BackColor = Color.Transparent };
        buttons.Controls.AddRange(new Control[] { btnAdd, btnCancel });

        Controls.Add(layout);
        Controls.Add(buttons);
    }
}
