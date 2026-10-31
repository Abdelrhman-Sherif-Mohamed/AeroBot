using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RSBot.Alchemy.Bot;
using RSBot.Alchemy.Bundle.Manufacture;
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
using Radio = SDUI.Controls.Radio;

namespace RSBot.Alchemy.Views.Settings;

[ToolboxItem(false)]
public class ManufactureSettingsView : UserControl
{
    #region Constants

    private const int MaxResolveMaterials = 4;
    private const int MaxCompoundMaterials = 1;
    private const string RondoCodeName = "ITEM_ETC_ARCHEMY_RONDO_01";

    #endregion Constants

    #region Members

    private readonly GroupBox _groupMaterials;
    private readonly GroupBox _groupOptions;
    private readonly ListView _lvMaterials;
    private readonly ColumnHeader _colName;
    private readonly ColumnHeader _colAmount;
    private readonly ColumnHeader _colSlot;
    private readonly Label _lblMode;
    private readonly Radio _radioResolve;
    private readonly Radio _radioCompound;
    private readonly Label _lblModeHint;
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

    public ManufactureSettingsView()
    {
        CheckForIllegalCrossThreadCalls = false;
        DoubleBuffered = true;
        BackColor = Color.Transparent;

        _groupMaterials = new GroupBox
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

        _lvMaterials = new ListView
        {
            CheckBoxes = true,
            FullRowSelect = true,
            HideSelection = false,
            MultiSelect = false,
            View = View.Details
        };

        _colName = new ColumnHeader();
        _colAmount = new ColumnHeader();
        _colSlot = new ColumnHeader();
        _lvMaterials.Columns.AddRange(new[] { _colName, _colAmount, _colSlot });
        _lvMaterials.ItemChecked += LvMaterials_ItemChecked;
        _lvMaterials.Resize += (_, _) => ApplyColumnWidths();

        _btnRefresh = new Button { Text = "↻", TabStop = false };
        _btnRefresh.Click += (_, _) => PopulateItems();

        _btnSelectAll = new Button { TabStop = false };
        _btnSelectAll.Click += (_, _) => SelectAll();

        _btnClear = new Button { TabStop = false };
        _btnClear.Click += (_, _) => ClearSelection();

        _lblMode = CreateLabel();
        _lblModeHint = CreateLabel();
        _lblSelectedCaption = CreateLabel();
        _lblSelectedValue = CreateLabel(false);
        _lblRondoCaption = CreateLabel();
        _lblRondoValue = CreateLabel(false);
        _lblWarning = CreateLabel(false);
        _lblStatus = CreateLabel(false);
        _lblHint = CreateLabel(false);

        _radioResolve = new Radio
        {
            AutoSize = false,
            Checked = true,
            TabStop = true
        };
        _radioCompound = new Radio
        {
            AutoSize = false,
            TabStop = true
        };

        _radioResolve.CheckedChanged += (_, _) => ModeChanged();
        _radioCompound.CheckedChanged += (_, _) => ModeChanged();

        _groupMaterials.Controls.Add(_lvMaterials);
        _groupMaterials.Controls.Add(_btnRefresh);
        _groupMaterials.Controls.Add(_btnSelectAll);
        _groupMaterials.Controls.Add(_btnClear);

        _groupOptions.Controls.Add(_lblMode);
        _groupOptions.Controls.Add(_radioResolve);
        _groupOptions.Controls.Add(_radioCompound);
        _groupOptions.Controls.Add(_lblModeHint);
        _groupOptions.Controls.Add(_lblSelectedCaption);
        _groupOptions.Controls.Add(_lblSelectedValue);
        _groupOptions.Controls.Add(_lblRondoCaption);
        _groupOptions.Controls.Add(_lblRondoValue);
        _groupOptions.Controls.Add(_lblWarning);
        _groupOptions.Controls.Add(_lblStatus);
        _groupOptions.Controls.Add(_lblHint);

        Controls.Add(_groupOptions);
        Controls.Add(_groupMaterials);

        EventManager.SubscribeEvent("OnEnterGame", new System.Action(OnEnterGame));
        EventManager.SubscribeEvent("OnLoadCharacter", new System.Action(OnEnterGame));
        EventManager.SubscribeEvent("OnInventoryUpdate", new System.Action(OnInventoryUpdate));
        EventManager.SubscribeEvent("OnAlchemy", new System.Action<AlchemyType>(OnAlchemy));
        EventManager.SubscribeEvent("OnMakeStuffResult",
            new System.Action<bool, ushort, byte, uint, bool>(OnMakeStuffResult));
        EventManager.SubscribeEvent("OnMakeStuffCanceled", new System.Action(OnMakeStuffCanceled));

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
    ///     Rebuilds the material list while keeping the current selection.
    /// </summary>
    public void PopulateItems()
    {
        if (IsDisposed || _lvMaterials == null || _updating)
            return;

        _updating = true;

        try
        {
            _lvMaterials.BeginUpdate();
            _lvMaterials.Items.Clear();

            if (Game.Player?.Inventory == null)
            {
                _selected.Clear();

                return;
            }

            var items = Game.Player.Inventory
                .Where(i => IsManufactureMaterial(i))
                .OrderBy(i => i.Record.GetRealName())
                .ToList();

            var availableSlots = new HashSet<byte>();

            foreach (var item in items)
            {
                availableSlots.Add(item.Slot);

                var row = new ListViewItem(item.Record.GetRealName()) { Tag = item };
                row.SubItems.Add(item.Amount.ToString());
                row.SubItems.Add(item.Slot.ToString());

                _lvMaterials.Items.Add(row);

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
            _lvMaterials.EndUpdate();
            _updating = false;
        }

        UpdateConfig();
        UpdateSummary();
        ApplyColumnWidths();
    }

    private static bool IsManufactureMaterial(InventoryItem item)
    {
        var codeName = item?.Record?.CodeName;

        if (string.IsNullOrEmpty(codeName) || item.Amount == 0)
            return false;

        if (!codeName.StartsWith("ITEM_ETC_ARCHEMY_", StringComparison.OrdinalIgnoreCase))
            return false;

        return codeName.IndexOf("RONDO", StringComparison.OrdinalIgnoreCase) < 0;
    }

    private void LvMaterials_ItemChecked(object sender, ItemCheckedEventArgs e)
    {
        if (_updating)
            return;

        var item = e.Item.Tag as InventoryItem;
        if (item == null)
        {
            e.Item.Checked = false;

            return;
        }

        if (e.Item.Checked)
        {
            var maxCount = GetMaxSelectionCount();

            if (_selected.Count >= maxCount && !_selected.ContainsKey(item.Slot))
            {
                SetStatus(string.Format(
                    T("Alchemy.Manufacture.Limit", "You can select at most {0} material(s) in this mode.",
                        "لا يمكنك تحديد أكثر من {0} مواد في هذا الوضع."), maxCount));

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

    private int GetMaxSelectionCount()
    {
        return _radioCompound.Checked ? MaxCompoundMaterials : MaxResolveMaterials;
    }

    private void ModeChanged()
    {
        if (_updating)
            return;

        var maxCount = GetMaxSelectionCount();

        if (_selected.Count > maxCount)
        {
            var keep = _selected.Take(maxCount).ToDictionary(pair => pair.Key, pair => pair.Value);
            _selected.Clear();

            foreach (var pair in keep)
                _selected[pair.Key] = pair.Value;

            _updating = true;

            try
            {
                foreach (ListViewItem row in _lvMaterials.Items)
                {
                    var item = row.Tag as InventoryItem;
                    row.Checked = item != null && _selected.ContainsKey(item.Slot);
                }
            }
            finally
            {
                _updating = false;
            }
        }

        UpdateConfig();
        UpdateSummary();
        ApplyLocalization();
    }

    public void SelectAll()
    {
        var maxCount = GetMaxSelectionCount();
        var count = 0;

        _updating = true;

        try
        {
            _selected.Clear();

            foreach (ListViewItem row in _lvMaterials.Items)
            {
                var item = row.Tag as InventoryItem;

                if (item == null)
                    continue;

                var shouldCheck = count < maxCount;
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
            foreach (ListViewItem row in _lvMaterials.Items)
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

        var materials = new List<InventoryItem>(_selected.Count);

        foreach (var pair in _selected)
        {
            var item = Game.Player?.Inventory?.GetItemAt(pair.Key);

            if (item == null || item.ItemId != pair.Value)
                continue;

            materials.Add(item);
        }

        Globals.Botbase.ManufactureBundleConfig = new ManufactureBundleConfig
        {
            Mode = _radioCompound.Checked ? ManufactureMode.Compound : ManufactureMode.Resolve,
            Materials = materials
        };
    }

    private void UpdateSummary()
    {
        if (_lblSelectedValue == null)
            return;

        var maxCount = GetMaxSelectionCount();
        var rondoCount = GetRondoCount();

        _lblSelectedValue.Text = $"{_selected.Count} / {maxCount}";
        _lblRondoValue.Text = $"x{rondoCount}";

        if (rondoCount <= 0)
        {
            _lblWarning.Text = T("Alchemy.Manufacture.NoRondo",
                "No manufacture Rondo in the inventory. The bot will stop.",
                "لا يوجد روندو تصنيع في الحقيبة. سيتوقف التنفيذ.");
            _lblWarning.ForeColor = GetWarningColor();
        }
        else if (_selected.Count == 0)
        {
            _lblWarning.Text = T("Alchemy.Manufacture.NothingSelected",
                "Select the materials you want to process first.",
                "حدد المواد التي تريد معالجتها أولًا.");
            _lblWarning.ForeColor = GetMutedColor();
        }
        else
        {
            _lblWarning.Text = string.Empty;
            _lblWarning.ForeColor = GetMutedColor();
        }

        _lblModeHint.Text = _radioCompound.Checked
            ? T("Alchemy.Manufacture.CompoundHint", "Select one material to compound.",
                "حدد مادة واحدة لتركيبها.")
            : T("Alchemy.Manufacture.ResolveHint", "Select 1 to 4 materials to resolve.",
                "حدد من 1 إلى 4 مواد لتحضيرها.");
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

    private void OnMakeStuffResult(bool success, ushort errorCode, byte extra, uint value, bool completed)
    {
        RunOnUi(() =>
        {
            if (!success)
            {
                SetStatus(errorCode == 0x5806
                    ? T("Alchemy.Manufacture.NoRondo",
                        "No manufacture Rondo in the inventory. The bot will stop.",
                        "لا يوجد روندو تصنيع في الحقيبة. سيتوقف التنفيذ.")
                    : string.Format(
                        T("Alchemy.Manufacture.Failed", "Manufacture failed (error 0x{0}).",
                            "فشل التصنيع (الرمز 0x{0})."), $"{errorCode:X4}"));
            }
            else if (completed)
            {
                SetStatus(T("Alchemy.Manufacture.Success", "Manufacture completed successfully.",
                    "تم تنفيذ عملية التصنيع بنجاح."));
            }

            PopulateItems();
        });
    }

    private void OnMakeStuffCanceled()
    {
        RunOnUi(() =>
        {
            SetStatus(T("Alchemy.Manufacture.Canceled", "Manufacture was canceled.",
                "أُلغيت عملية التصنيع."));

            PopulateItems();
        });
    }

    private void ApplyColumnWidths()
    {
        if (_lvMaterials == null)
            return;

        var width = (int)(_lvMaterials.ClientSize.Width / (DeviceDpi / 96f));

        if (width < 120)
            return;

        _colName.Width = Math.Max(120, width - 150);
        _colAmount.Width = 70;
        _colSlot.Width = 56;
    }

    #endregion Methods

    #region Layout & Theme

    public void ApplyLocalization()
    {
        if (_localizing || _lblMode == null)
            return;

        _localizing = true;

        try
        {
            _groupMaterials.Text = T("Alchemy.Manufacture.Materials", "Available materials", "المواد المتاحة");
            _groupOptions.Text = T("Alchemy.Manufacture.Options", "Manufacture settings", "إعدادات التصنيع");

            _lblMode.Text = T("Alchemy.Manufacture.Mode", "Mode:", "الوضع:");
            _radioResolve.Text = T("Alchemy.Manufacture.Resolve", "Resolve materials", "تحضير مواد");
            _radioCompound.Text = T("Alchemy.Manufacture.Compound", "Compound", "تركيب");

            _lblSelectedCaption.Text = T("Alchemy.Manufacture.Selected", "Selected materials", "المواد المحددة");
            _lblRondoCaption.Text = T("Alchemy.Manufacture.Rondo", "Manufacture Rondo", "روندو التصنيع");

            _btnRefresh.Text = "↻";
            _btnSelectAll.Text = T("Alchemy.Selection.SelectAll", "Select all", "تحديد الكل");
            _btnClear.Text = T("Alchemy.Selection.Clear", "Clear selection", "مسح التحديد");

            _colName.Text = T("Alchemy.Selection.Name", "Name", "الاسم");
            _colAmount.Text = T("Alchemy.Selection.Amount", "Amount", "الكمية");
            _colSlot.Text = T("Alchemy.Selection.Slot", "Slot", "الخانة");

            _lblHint.Text = T("Alchemy.Manufacture.StartHint",
                "Select the materials, then start the bot with the global Start button above.",
                "اختر المواد، ثم شغّل البوت من زر البدء العام بالأعلى.");

            if (string.IsNullOrEmpty(_lblStatus.Text))
                _lblStatus.Text = T("Alchemy.Manufacture.Ready", "Ready. Start the bot to begin.",
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
        if (_lblMode == null)
            return;

        var foreColor = ColorScheme.ForeColor;
        var mutedColor = GetMutedColor();

        BackColor = Color.Transparent;
        _groupMaterials.ForeColor = foreColor;
        _groupOptions.ForeColor = foreColor;

        foreach (var label in new[]
                 {
                     _lblMode, _lblModeHint, _lblSelectedCaption, _lblRondoCaption,
                     _lblWarning, _lblStatus, _lblHint
                 })
        {
            if (label != null)
                label.ForeColor = mutedColor;
        }

        _lblSelectedValue.ForeColor = ColorScheme.AccentColor;
        _lblRondoValue.ForeColor = ColorScheme.AccentColor;
        _lblStatus.ForeColor = foreColor;
        _btnRefresh.ForeColor = ColorScheme.AccentColor;

        foreach (var button in new[] { _btnSelectAll, _btnClear, _btnRefresh })
            button.ForeColor = foreColor;

        foreach (var radio in new[] { _radioResolve, _radioCompound })
            radio.ForeColor = foreColor;

        _lvMaterials.BackColor = ColorScheme.TableBackColor;
        _lvMaterials.ForeColor = foreColor;

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
        if (IsDisposed || _groupMaterials == null)
            return;

        var scale = DeviceDpi / 96f;
        var width = (int)(ClientSize.Width / scale);
        var height = (int)(ClientSize.Height / scale);

        if (width < 420 || height < 300)
            return;

        const int pad = 10;
        const int gap = 10;

        var materialsWidth = Math.Max(400, (int)(width * 0.58));
        var optionsWidth = width - pad * 2 - gap - materialsWidth;

        if (optionsWidth < 300)
        {
            materialsWidth = width - pad * 2 - gap - 300;
            optionsWidth = 300;
        }

        Place(_groupMaterials, pad, pad, materialsWidth, height - pad * 2);
        Place(_groupOptions, pad + materialsWidth + gap, pad, optionsWidth, height - pad * 2);

        LayoutMaterialsGroup();
        LayoutOptionsGroup();
    }

    private void LayoutMaterialsGroup()
    {
        var scale = DeviceDpi / 96f;
        var width = (int)(_groupMaterials.ClientSize.Width / scale);
        var height = (int)(_groupMaterials.ClientSize.Height / scale);

        if (width < 300 || height < 220)
            return;

        const int pad = 10;

        Place(_btnRefresh, pad, 28, 34, 30);
        Place(_btnClear, width - pad - 112, 28, 112, 30);
        Place(_btnSelectAll, width - pad - 230, 28, 112, 30);

        Place(_lvMaterials, pad, 66, width - pad * 2, height - 76);
    }

    private void LayoutOptionsGroup()
    {
        var scale = DeviceDpi / 96f;
        var width = (int)(_groupOptions.ClientSize.Width / scale);
        var height = (int)(_groupOptions.ClientSize.Height / scale);

        if (width < 240 || height < 300)
            return;

        const int pad = 16;

        PlaceLabel(_lblMode, pad, 30, width - pad * 2, 26, ContentAlignment.MiddleRight);
        Place(_radioResolve, pad, 60, width - pad * 2, 26);
        Place(_radioCompound, pad, 90, width - pad * 2, 26);
        Place(_lblModeHint, pad, 122, width - pad * 2, 46);

        PlaceLabel(_lblSelectedCaption, pad, 186, width - pad * 2 - 84, 30, ContentAlignment.MiddleRight);
        PlaceLabel(_lblSelectedValue, width - pad - 84, 186, 84, 30, ContentAlignment.MiddleLeft);

        PlaceLabel(_lblRondoCaption, pad, 222, width - pad * 2 - 84, 30, ContentAlignment.MiddleRight);
        PlaceLabel(_lblRondoValue, width - pad - 84, 222, 84, 30, ContentAlignment.MiddleLeft);

        PlaceLabel(_lblWarning, pad, 260, width - pad * 2, 44, ContentAlignment.MiddleRight);
        PlaceLabel(_lblStatus, pad, 310, width - pad * 2, 56, ContentAlignment.TopRight);

        PlaceLabel(_lblHint, pad, height - 82, width - pad * 2, 66, ContentAlignment.MiddleRight);
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
