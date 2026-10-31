using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RSBot.Alchemy.Bot;
using RSBot.Alchemy.Bundle.Dismantle;
using RSBot.Core;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Objects;
using SDUI;
using SDUI.Controls;

using Button = SDUI.Controls.Button;
using GroupBox = SDUI.Controls.GroupBox;
using Label = SDUI.Controls.Label;
using ListView = SDUI.Controls.ListView;

namespace RSBot.Alchemy.Views.Settings;

[ToolboxItem(false)]
public class DismantleSettingsView : UserControl
{
    #region Constants

    private const int MaxEquipment = 9;
    private const string RondoCodeName = "ITEM_ETC_ARCHEMY_RONDO_02";

    #endregion Constants

    #region Members

    private readonly GroupBox _groupEquipment;
    private readonly GroupBox _groupOptions;
    private readonly ListView _lvEquipment;
    private readonly ColumnHeader _colName;
    private readonly ColumnHeader _colOptLevel;
    private readonly ColumnHeader _colSlot;
    private readonly Label _lblSelectedCaption;
    private readonly Label _lblSelectedValue;
    private readonly Label _lblRondoCaption;
    private readonly Label _lblRondoValue;
    private readonly Label _lblWarning;
    private readonly Label _lblStatus;
    private readonly Label _lblHint;
    private readonly Button _btnRefresh;
    private readonly Button _btnSelectAll;
    private readonly Button _btnClear;
    private readonly Dictionary<byte, uint> _selected = new();

    private bool _updating;
    private bool _localizing;

    #endregion Members

    #region Constructor

    public DismantleSettingsView()
    {
        CheckForIllegalCrossThreadCalls = false;
        DoubleBuffered = true;
        BackColor = Color.Transparent;

        _groupEquipment = new GroupBox
        {
            BackColor = Color.Transparent,
            Radius = 10,
            ShadowDepth = 4,
            TabStop = false
        };

        _groupOptions = new GroupBox
        {
            BackColor = Color.Transparent,
            Radius = 10,
            ShadowDepth = 4,
            TabStop = false
        };

        _lvEquipment = new ListView
        {
            CheckBoxes = true,
            FullRowSelect = true,
            HideSelection = false,
            MultiSelect = false,
            View = View.Details
        };

        _colName = new ColumnHeader();
        _colOptLevel = new ColumnHeader();
        _colSlot = new ColumnHeader();
        _lvEquipment.Columns.AddRange(new[] { _colName, _colOptLevel, _colSlot });
        _lvEquipment.ItemChecked += LvEquipment_ItemChecked;
        _lvEquipment.Resize += (_, _) => ApplyColumnWidths();

        _btnRefresh = new Button { Text = "↻", TabStop = false };
        _btnRefresh.Click += (_, _) => PopulateItems();

        _btnSelectAll = new Button { TabStop = false };
        _btnSelectAll.Click += (_, _) => SelectAll();

        _btnClear = new Button { TabStop = false };
        _btnClear.Click += (_, _) => ClearSelection();

        _lblSelectedCaption = CreateLabel();
        _lblSelectedValue = CreateLabel(false);
        _lblRondoCaption = CreateLabel();
        _lblRondoValue = CreateLabel(false);
        _lblWarning = CreateLabel(false);
        _lblStatus = CreateLabel(false);
        _lblHint = CreateLabel(false);

        _groupEquipment.Controls.Add(_lvEquipment);
        _groupEquipment.Controls.Add(_btnRefresh);
        _groupEquipment.Controls.Add(_btnSelectAll);
        _groupEquipment.Controls.Add(_btnClear);

        _groupOptions.Controls.Add(_lblSelectedCaption);
        _groupOptions.Controls.Add(_lblSelectedValue);
        _groupOptions.Controls.Add(_lblRondoCaption);
        _groupOptions.Controls.Add(_lblRondoValue);
        _groupOptions.Controls.Add(_lblWarning);
        _groupOptions.Controls.Add(_lblStatus);
        _groupOptions.Controls.Add(_lblHint);

        Controls.Add(_groupOptions);
        Controls.Add(_groupEquipment);

        EventManager.SubscribeEvent("OnEnterGame", new System.Action(OnEnterGame));
        EventManager.SubscribeEvent("OnLoadCharacter", new System.Action(OnEnterGame));
        EventManager.SubscribeEvent("OnInventoryUpdate", new System.Action(OnInventoryUpdate));
        EventManager.SubscribeEvent("OnAlchemy", new System.Action<AlchemyType>(OnAlchemy));
        EventManager.SubscribeEvent("OnDismantleResult", new System.Action<bool, ushort>(OnDismantleResult));

        ApplyLocalization();
        ApplyTheme();
        PopulateItems();
    }

    #endregion Constructor

    #region Methods

    private static Label CreateLabel(bool autoSize = true)
    {
        return new Label
        {
            AutoSize = autoSize,
            BackColor = Color.Transparent
        };
    }

    private void OnEnterGame()
    {
        PostRefresh();
    }

    private void OnInventoryUpdate()
    {
        PostRefresh();
    }

    private void OnAlchemy(AlchemyType type)
    {
        PostRefresh();
    }

    private void PostRefresh()
    {
        if (IsDisposed || Disposing)
            return;

        RunOnUi(() =>
        {
            PopulateItems();
            UpdateSummary();
        });
    }

    private void RunOnUi(System.Action action)
    {
        if (IsDisposed || Disposing)
            return;

        try
        {
            if (IsHandleCreated)
                BeginInvoke(action);
            else
                action();
        }
        catch (InvalidOperationException)
        {
            // The control is shutting down.
        }
    }

    /// <summary>
    ///     Rebuilds the equipment list while keeping the current selection.
    /// </summary>
    public void PopulateItems()
    {
        if (IsDisposed || _lvEquipment == null || _updating)
            return;

        _updating = true;

        try
        {
            _lvEquipment.BeginUpdate();
            _lvEquipment.Items.Clear();

            if (Game.Player?.Inventory == null)
            {
                _selected.Clear();

                return;
            }

            var items = Game.Player.Inventory
                .Where(i => IsDismantleEquipment(i))
                .OrderBy(i => i.Record.GetRealName())
                .ThenByDescending(i => i.OptLevel)
                .ToList();

            var availableSlots = new HashSet<byte>();

            foreach (var item in items)
            {
                availableSlots.Add(item.Slot);

                var row = new ListViewItem(item.Record.GetRealName()) { Tag = item };
                row.SubItems.Add($"+{item.OptLevel}");
                row.SubItems.Add(item.Slot.ToString());

                _lvEquipment.Items.Add(row);

                if (_selected.TryGetValue(item.Slot, out var itemId))
                {
                    if (itemId == item.ItemId)
                        row.Checked = true;
                    else
                        _selected.Remove(item.Slot);
                }
            }

            foreach (var slot in _selected.Keys.ToList())
            {
                if (availableSlots.Contains(slot))
                    continue;

                _selected.Remove(slot);
            }
        }
        finally
        {
            _lvEquipment.EndUpdate();
            _updating = false;
        }

        UpdateConfig();
        UpdateSummary();
        ApplyColumnWidths();
    }

    private static bool IsDismantleEquipment(InventoryItem item)
    {
        var record = item?.Record;

        if (record == null || !record.IsEquip || record.IsAvatar || record.IsJobOutfit)
            return false;

        return item.Amount > 0 && item.Slot >= 13;
    }

    private void LvEquipment_ItemChecked(object sender, ItemCheckedEventArgs e)
    {
        if (_updating)
            return;

        var item = e.Item.Tag as InventoryItem;
        if (item == null)
        {
            _updating = true;

            try
            {
                e.Item.Checked = false;
            }
            finally
            {
                _updating = false;
            }

            return;
        }

        if (e.Item.Checked)
        {
            if (_selected.Count >= MaxEquipment && !_selected.ContainsKey(item.Slot))
            {
                SetStatus(string.Format(
                    T("Alchemy.Dismantle.Limit", "You can select at most {0} pieces of equipment.",
                        "لا يمكنك تحديد أكثر من {0} قطع في هذا الوضع."), MaxEquipment));

                _updating = true;

                try
                {
                    e.Item.Checked = false;
                }
                finally
                {
                    _updating = false;
                }

                return;
            }

            _selected[item.Slot] = item.ItemId;
        }
        else
        {
            _selected.Remove(item.Slot);
        }

        UpdateConfig();
        UpdateSummary();
    }

    public void SelectAll()
    {
        var count = 0;

        _updating = true;

        try
        {
            _selected.Clear();

            foreach (ListViewItem row in _lvEquipment.Items)
            {
                var item = row.Tag as InventoryItem;

                if (item == null)
                    continue;

                var shouldCheck = count < MaxEquipment;
                row.Checked = shouldCheck;

                if (!shouldCheck)
                    continue;

                _selected[item.Slot] = item.ItemId;
                count++;
            }
        }
        finally
        {
            _updating = false;
        }

        UpdateConfig();
        UpdateSummary();
    }

    public void ClearSelection()
    {
        _selected.Clear();

        _updating = true;

        try
        {
            foreach (ListViewItem row in _lvEquipment.Items)
                row.Checked = false;
        }
        finally
        {
            _updating = false;
        }

        UpdateConfig();
        UpdateSummary();
    }

    private void UpdateConfig()
    {
        if (Globals.Botbase == null)
            return;

        var equipment = new List<InventoryItem>(_selected.Count);

        foreach (var pair in _selected)
        {
            var item = Game.Player?.Inventory?.GetItemAt(pair.Key);

            if (item == null || item.ItemId != pair.Value)
                continue;

            equipment.Add(item);
        }

        Globals.Botbase.DismantleBundleConfig = new DismantleBundleConfig
        {
            Equipment = equipment
        };
    }

    private void UpdateSummary()
    {
        if (_lblSelectedValue == null)
            return;

        var rondoCount = GetRondoCount();

        _lblSelectedValue.Text = $"{_selected.Count} / {MaxEquipment}";
        _lblRondoValue.Text = $"x{rondoCount}";

        if (rondoCount <= 0)
        {
            _lblWarning.Text = T("Alchemy.Dismantle.NoRondo",
                "No dismantling Rondo in the inventory. The bot will stop.",
                "لا يوجد روندو تكسير في الحقيبة. سيتوقف التنفيذ.");
            _lblWarning.ForeColor = GetWarningColor();
        }
        else if (_selected.Count == 0)
        {
            _lblWarning.Text = T("Alchemy.Dismantle.NothingSelected",
                "Select the equipment you want to dismantle first.",
                "حدد المعدات التي تريد تكسيرها أولًا.");
            _lblWarning.ForeColor = GetMutedColor();
        }
        else
        {
            _lblWarning.Text = string.Empty;
            _lblWarning.ForeColor = GetMutedColor();
        }
    }

    private static int GetRondoCount()
    {
        if (Game.Player?.Inventory == null)
            return 0;

        return Game.Player.Inventory.GetItems(RondoCodeName).Sum(i => (int)i.Amount);
    }

    private void SetStatus(string text)
    {
        if (_lblStatus == null)
            return;

        _lblStatus.Text = text;
        _lblStatus.ForeColor = ColorScheme.ForeColor;
    }

    private void OnDismantleResult(bool success, ushort errorCode)
    {
        RunOnUi(() =>
        {
            if (!success)
            {
                SetStatus(errorCode == 0x5807
                    ? T("Alchemy.Dismantle.NoRondo",
                        "No dismantling Rondo in the inventory. The bot will stop.",
                        "لا يوجد روندو تكسير في الحقيبة. سيتوقف التنفيذ.")
                    : string.Format(
                        T("Alchemy.Dismantle.Failed", "Dismantling failed (error 0x{0}).",
                            "فشل التكسير (الرمز 0x{0})."), $"{errorCode:X4}"));
            }
            else
            {
                SetStatus(T("Alchemy.Dismantle.Success", "The equipment was dismantled successfully.",
                    "تم تكسير القطعة بنجاح."));
            }

            PopulateItems();
        });
    }

    private void ApplyColumnWidths()
    {
        if (_lvEquipment == null)
            return;

        var width = (int)(_lvEquipment.ClientSize.Width / (DeviceDpi / 96f));

        if (width < 120)
            return;

        _colName.Width = Math.Max(140, width - 164);
        _colOptLevel.Width = 74;
        _colSlot.Width = 56;
    }

    #endregion Methods

    #region Layout & Theme

    public void ApplyLocalization()
    {
        if (_localizing || _lblSelectedCaption == null)
            return;

        _localizing = true;

        try
        {
            _groupEquipment.Text = T("Alchemy.Dismantle.Equipment", "Available equipment", "المعدات المتاحة");
            _groupOptions.Text = T("Alchemy.Dismantle.Options", "Dismantle settings", "إعدادات التكسير");

            _lblSelectedCaption.Text = T("Alchemy.Dismantle.Selected", "Selected equipment", "القطع المحددة");
            _lblRondoCaption.Text = T("Alchemy.Dismantle.Rondo", "Dismantling Rondo", "روندو التكسير");

            _btnRefresh.Text = "↻";
            _btnSelectAll.Text = T("Alchemy.Selection.SelectAll", "Select all", "تحديد الكل");
            _btnClear.Text = T("Alchemy.Selection.Clear", "Clear selection", "مسح التحديد");

            _colName.Text = T("Alchemy.Selection.Name", "Name", "الاسم");
            _colOptLevel.Text = T("Alchemy.Selection.Enhancement", "Enhancement", "التحسين");
            _colSlot.Text = T("Alchemy.Selection.Slot", "Slot", "الخانة");

            _lblHint.Text = T("Alchemy.Dismantle.StartHint",
                "One piece is dismantled per cycle. Select the equipment, then start the bot with the global Start button above.",
                "يتم تكسير قطعة واحدة في كل دورة. حدد المعدات، ثم شغّل البوت من زر البدء العام بالأعلى.");

            if (string.IsNullOrEmpty(_lblStatus.Text))
                _lblStatus.Text = T("Alchemy.Dismantle.Ready", "Ready. Start the bot to begin.",
                    "جاهز. شغّل البوت للبدء.");

            UpdateSummary();
            ApplyLayout();
        }
        finally
        {
            _localizing = false;
        }
    }

    public void ApplyTheme()
    {
        if (_lblSelectedCaption == null)
            return;

        var foreColor = ColorScheme.ForeColor;

        BackColor = Color.Transparent;
        _groupEquipment.ForeColor = foreColor;
        _groupOptions.ForeColor = foreColor;

        foreach (var label in new[]
                 {
                     _lblSelectedCaption, _lblRondoCaption, _lblWarning, _lblStatus, _lblHint
                 })
        {
            if (label != null)
                label.ForeColor = GetMutedColor();
        }

        _lblSelectedValue.ForeColor = ColorScheme.AccentColor;
        _lblRondoValue.ForeColor = ColorScheme.AccentColor;
        _lblStatus.ForeColor = foreColor;
        _btnRefresh.ForeColor = ColorScheme.AccentColor;

        foreach (var button in new[] { _btnSelectAll, _btnClear, _btnRefresh })
            button.ForeColor = foreColor;

        _lvEquipment.BackColor = ColorScheme.TableBackColor;
        _lvEquipment.ForeColor = foreColor;

        ApplyColumnWidths();
        Invalidate();
    }

    private static Color GetMutedColor()
    {
        return ColorScheme.BackColor.IsDark()
            ? Color.FromArgb(168, 184, 204)
            : Color.FromArgb(90, 104, 124);
    }

    private static Color GetWarningColor()
    {
        return ColorScheme.BackColor.IsDark()
            ? Color.FromArgb(240, 148, 140)
            : Color.FromArgb(176, 58, 48);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        ApplyLayout();
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);

        if (!Visible)
            return;

        ApplyLocalization();
        ApplyTheme();
        ApplyLayout();
        PopulateItems();
    }

    private int S(float value)
    {
        return (int)Math.Round(value * DeviceDpi / 96f);
    }

    private void Place(Control control, int x, int y, int width, int height)
    {
        if (control == null)
            return;

        control.Anchor = AnchorStyles.None;
        control.Dock = DockStyle.None;
        control.AutoSize = false;
        control.SetBounds(S(x), S(y), Math.Max(1, S(width)), Math.Max(1, S(height)));
    }

    private void PlaceLabel(Label label, int x, int y, int width, int height, ContentAlignment alignment)
    {
        Place(label, x, y, width, height);
        label.TextAlign = alignment;
    }

    private void ApplyLayout()
    {
        if (IsDisposed || _groupEquipment == null)
            return;

        var scale = DeviceDpi / 96f;
        var width = (int)(ClientSize.Width / scale);
        var height = (int)(ClientSize.Height / scale);

        if (width < 420 || height < 300)
            return;

        const int pad = 10;
        const int gap = 10;

        var equipmentWidth = Math.Max(400, (int)(width * 0.58));
        var optionsWidth = width - pad * 2 - gap - equipmentWidth;

        if (optionsWidth < 300)
        {
            equipmentWidth = width - pad * 2 - gap - 300;
            optionsWidth = 300;
        }

        Place(_groupEquipment, pad, pad, equipmentWidth, height - pad * 2);
        Place(_groupOptions, pad + equipmentWidth + gap, pad, optionsWidth, height - pad * 2);

        LayoutEquipmentGroup();
        LayoutOptionsGroup();
    }

    private void LayoutEquipmentGroup()
    {
        var scale = DeviceDpi / 96f;
        var width = (int)(_groupEquipment.ClientSize.Width / scale);
        var height = (int)(_groupEquipment.ClientSize.Height / scale);

        if (width < 300 || height < 220)
            return;

        const int pad = 10;

        Place(_btnRefresh, pad, 28, 34, 30);
        Place(_btnClear, width - pad - 112, 28, 112, 30);
        Place(_btnSelectAll, width - pad - 230, 28, 112, 30);

        Place(_lvEquipment, pad, 66, width - pad * 2, height - 76);
    }

    private void LayoutOptionsGroup()
    {
        var scale = DeviceDpi / 96f;
        var width = (int)(_groupOptions.ClientSize.Width / scale);
        var height = (int)(_groupOptions.ClientSize.Height / scale);

        if (width < 240 || height < 260)
            return;

        const int pad = 16;

        PlaceLabel(_lblSelectedCaption, pad, 34, width - pad * 2 - 84, 30, ContentAlignment.MiddleRight);
        PlaceLabel(_lblSelectedValue, width - pad - 84, 34, 84, 30, ContentAlignment.MiddleLeft);

        PlaceLabel(_lblRondoCaption, pad, 70, width - pad * 2 - 84, 30, ContentAlignment.MiddleRight);
        PlaceLabel(_lblRondoValue, width - pad - 84, 70, 84, 30, ContentAlignment.MiddleLeft);

        PlaceLabel(_lblWarning, pad, 112, width - pad * 2, 52, ContentAlignment.MiddleRight);
        PlaceLabel(_lblStatus, pad, 176, width - pad * 2, 64, ContentAlignment.TopRight);

        PlaceLabel(_lblHint, pad, height - 104, width - pad * 2, 88, ContentAlignment.MiddleRight);
    }

    #endregion Layout & Theme

    private static string T(string key, string english, string arabic)
    {
        var value = LanguageManager.GetLangBySpecificKey("RSBot.Alchemy", key, string.Empty);
        if (!string.IsNullOrWhiteSpace(value))
            return value;

        return Kernel.Language == "ar_AR" ? arabic : english;
    }
}
