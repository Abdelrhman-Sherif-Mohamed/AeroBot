using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RSBot.Alchemy.Bot;
using RSBot.Alchemy.Client.ReferenceObjects;
using RSBot.Alchemy.Views.Settings;
using RSBot.Core;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Extensions;
using RSBot.Core.Objects;
using SDUI;
using SDUI.Controls;

using GroupBox = SDUI.Controls.GroupBox;
using Label = SDUI.Controls.Label;

namespace RSBot.Alchemy.Views;

[ToolboxItem(false)]
public partial class Main : DoubleBufferedControl
{
    #region Constructor

    /// <summary>
    ///     Subscribes to several events and adds the user controls to the settings panel
    /// </summary>
    public Main()
    {
        CheckForIllegalCrossThreadCalls = false;

        InitializeComponent();
        SetStyle(
            ControlStyles.UserPaint |
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer,
            true);

        EventManager.SubscribeEvent("OnLoadCharacter", () =>
        {
            if (IsDisposed || Disposing)
                return;

            ReloadItemList();
        });

        EventManager.SubscribeEvent("OnAlchemy", new Action<AlchemyType>(OnAlchemy));

        _enhanceSettingsView = new EnhanceSettingsView { Visible = true, Dock = DockStyle.Fill };
        _magicOptionsSettingsView = new MagicOptionsSettingsView { Visible = false, Dock = DockStyle.Fill };
        _attributeSettingsView = new AttributesSettingsView { Visible = false, Dock = DockStyle.Fill };
        _manufactureSettingsView = new ManufactureSettingsView { Visible = false, Dock = DockStyle.None };
        _dismantleSettingsView = new DismantleSettingsView { Visible = false, Dock = DockStyle.None };

        panelEnhance.Controls.Add(_enhanceSettingsView);
        panelBlues.Controls.Add(_magicOptionsSettingsView);
        panelAttributes.Controls.Add(_attributeSettingsView);
        tabManufacture.Controls.Add(_manufactureSettingsView);
        tabBreak.Controls.Add(_dismantleSettingsView);

        ApplyLocalization();
        ApplyTheme();
        ApplyAlchemyLayout();
    }

    #endregion Constructor

    #region Properties

    public bool IsRefreshing { get; private set; }

    #endregion Properties

    /// <summary>
    ///     The selected inventory item
    /// </summary>
    public InventoryItem SelectedItem { get; set; }

    internal class InventoryItemComboboxItem
    {
        /// <param name="inventoryItem">The inventory item linked to this combobox item</param>
        public InventoryItemComboboxItem(InventoryItem inventoryItem)
        {
            InventoryItem = inventoryItem;
        }

        /// <summary>
        ///     The inventory item linked to this combobox item
        /// </summary>
        public InventoryItem InventoryItem { get; set; }

        public override string ToString()
        {
            return $"[{InventoryItem?.Slot}] {InventoryItem?.Record?.GetRealName()} (+{InventoryItem?.OptLevel})";
        }
    }

    #region Delegates

    /// <param name="item">The item that has been selected</param>
    internal delegate void SelectedItemChanged(InventoryItem item);

    /// <summary>
    ///     Will be triggered if the inventory item changes
    /// </summary>
    internal event SelectedItemChanged ItemChanged;

    /// <summary>
    ///     Will be triggered when the user selects a different engine
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="alchemyEngine">The alchemy engine.</param>
    internal delegate void SelectedEngineChanged(InventoryItem item, AlchemyEngine alchemyEngine);

    /// <summary>
    ///     Will be triggered when the user selects a different engine
    /// </summary>
    internal event SelectedEngineChanged EngineChanged;

    #endregion Delegates

    #region Members

    private readonly EnhanceSettingsView _enhanceSettingsView;
    private readonly MagicOptionsSettingsView _magicOptionsSettingsView;
    private readonly AttributesSettingsView _attributeSettingsView;
    private readonly ManufactureSettingsView _manufactureSettingsView;
    private readonly DismantleSettingsView _dismantleSettingsView;

    private bool _applyingLayout;
    private bool _applyingEngine;
    private bool _localizing;
    private bool _layoutQueued;

    #endregion Members

    #region Methods

    private static string T(string key, string english, string arabic)
    {
        var value = LanguageManager.GetLangBySpecificKey("RSBot.Alchemy", key, string.Empty);
        if (!string.IsNullOrWhiteSpace(value))
            return value;

        return Kernel.Language == "ar_AR" ? arabic : english;
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
        if (label == null)
            return;

        Place(label, x, y, width, height);
        label.TextAlign = alignment;
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        QueueAlchemyLayout();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        QueueAlchemyLayout();
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);

        if (!Visible)
            return;

        ApplyLocalization();
        ApplyTheme();
        ApplyAlchemyLayout();
        QueueAlchemyLayout();
    }

    /// <summary>
    ///     Runs the reflow after WinForms finished resizing the selected tab page.
    ///     Laying out synchronously from OnResize/OnVisibleChanged uses stale tab
    ///     page bounds, which leaves the page partly at its designer coordinates.
    /// </summary>
    private void QueueAlchemyLayout()
    {
        if (_layoutQueued || IsDisposed || Disposing || !IsHandleCreated)
            return;

        _layoutQueued = true;

        try
        {
            BeginInvoke(new System.Action(() =>
            {
                _layoutQueued = false;

                if (!IsDisposed && !Disposing)
                    ApplyAlchemyLayout();
            }));
        }
        catch (InvalidOperationException)
        {
            _layoutQueued = false;
        }
    }

    /// <summary>
    ///     Reflows the page into the actual size of the tab area. The designer
    ///     surface is a reference only; all coordinates below are logical pixels
    ///     and are scaled by the current DPI.
    /// </summary>
    private void ApplyAlchemyLayout()
    {
        if (_applyingLayout || IsDisposed || tabMain == null)
            return;

        var scale = DeviceDpi / 96f;
        var pageW = (int)(ClientSize.Width / scale);
        var pageH = (int)(ClientSize.Height / scale);

        if (pageW < 720 || pageH < 460)
            return;

        _applyingLayout = true;

        try
        {
            tabMain.ItemSize = new Size(S(150), S(34));
            tabItemInfo.ItemSize = new Size(S(130), S(28));
            tabEngine.ItemSize = new Size(S(140), S(30));

            const int margin = 10;
            Place(tabMain, margin, margin, pageW - margin * 2, pageH - margin * 2);

            // TabControl resizes its pages lazily, so reading tabAlchemy/tabStats
            // right after resizing tabMain returns stale bounds. The display
            // rectangle is always computed from the current tab control size.
            var pageArea = tabMain.DisplayRectangle.Size;

            LayoutAlchemyTab(pageArea.Width, pageArea.Height);
            LayoutStatsTab(pageArea.Width, pageArea.Height);
            LayoutBundleTab(pageArea, _manufactureSettingsView);
            LayoutBundleTab(pageArea, _dismantleSettingsView);
            ApplyColumnWidths();
        }
        finally
        {
            _applyingLayout = false;
        }
    }

    private void LayoutAlchemyTab(int pageWidth, int pageHeight)
    {
        var scale = DeviceDpi / 96f;
        var width = (int)(pageWidth / scale);
        var height = (int)(pageHeight / scale);

        if (width < 600 || height < 360)
            return;

        const int pad = 10;
        const int gap = 10;
        const int logHeight = 148;

        var topHeight = height - pad * 2 - gap - logHeight;
        if (topHeight < 240)
            return;

        var itemWidth = Math.Max(330, Math.Min(430, (int)(width * 0.32)));
        var settingsWidth = width - pad * 2 - gap - itemWidth;

        Place(groupItem, pad, pad, itemWidth, topHeight);
        Place(groupSettings, pad + itemWidth + gap, pad, settingsWidth, topHeight);
        Place(groupLog, pad, pad + topHeight + gap, width - pad * 2, logHeight);

        LayoutItemGroup();
        LayoutSettingsGroup();
        LayoutLogGroup();
    }

    private void LayoutItemGroup()
    {
        var scale = DeviceDpi / 96f;
        var width = (int)(groupItem.ClientSize.Width / scale);
        var height = (int)(groupItem.ClientSize.Height / scale);

        if (width < 300 || height < 220)
            return;

        const int pad = 12;
        const int rowHeight = 24;

        var labelWidth = Math.Min(120, Math.Max(84, width / 4));
        var comboWidth = width - pad * 2 - 36 - labelWidth - 8;

        Place(linkRefreshItemList, pad, 30, 34, 30);
        Place(comboItem, pad + 38, 33, comboWidth, 26);
        PlaceLabel(lblItemSelection, width - pad - labelWidth, 32, labelWidth, 26,
            ContentAlignment.MiddleRight);

        PlaceLabel(lblDegreeText, width - pad - labelWidth, 68, labelWidth, rowHeight,
            ContentAlignment.MiddleRight);
        PlaceLabel(lblDegree, pad, 68, width - pad * 2 - labelWidth - 6, rowHeight,
            ContentAlignment.MiddleLeft);

        PlaceLabel(lblOptLevelText, width - pad - labelWidth, 94, labelWidth, rowHeight,
            ContentAlignment.MiddleRight);
        PlaceLabel(lblOptLevel, pad, 94, width - pad * 2 - labelWidth - 6, rowHeight,
            ContentAlignment.MiddleLeft);

        Place(tabItemInfo, pad, 126, width - pad * 2, height - 138);
    }

    private void LayoutSettingsGroup()
    {
        var scale = DeviceDpi / 96f;
        var width = (int)(groupSettings.ClientSize.Width / scale);
        var height = (int)(groupSettings.ClientSize.Height / scale);

        if (width < 300 || height < 200)
            return;

        Place(tabEngine, 10, 28, width - 20, height - 38);

        // The engine pages are resized by their own TabControl; use its display
        // rectangle so the Blues toolbar is positioned for the real page size.
        var engineArea = tabEngine.DisplayRectangle.Size;
        var bluesWidth = (int)(engineArea.Width / scale);
        var bluesHeight = (int)(engineArea.Height / scale);
        if (bluesWidth < 200 || bluesHeight < 120)
            return;

        Place(btnSelectAllBlues, bluesWidth - 234, bluesHeight - 40, 218, 30);
        Place(panelBlues, 6, 4, bluesWidth - 12, bluesHeight - 48);

        _magicOptionsSettingsView?.ApplyTheme();
    }

    private void LayoutLogGroup()
    {
        var scale = DeviceDpi / 96f;
        var width = (int)(groupLog.ClientSize.Width / scale);
        var height = (int)(groupLog.ClientSize.Height / scale);

        if (width < 300 || height < 80)
            return;

        Place(btnClearLog, width - 90, 24, 78, 28);
        Place(lvLog, 10, 28, width - 110, height - 38);
    }

    private void LayoutStatsTab(int pageWidth, int pageHeight)
    {
        var scale = DeviceDpi / 96f;
        var width = (int)(pageWidth / scale);
        var height = (int)(pageHeight / scale);

        if (width < 600 || height < 300)
            return;

        Place(groupStatsTools, 10, 10, width - 20, 78);
        Place(panelAttributes, 10, 98, width - 20, height - 108);

        var toolsWidth = (int)(groupStatsTools.ClientSize.Width / scale);
        Place(btnSelectAllStats, toolsWidth - 164, 30, 148, 30);
        Place(btnApplyStats, toolsWidth - 272, 30, 96, 30);
        Place(numStatsValue, toolsWidth - 360, 32, 74, 26);
        PlaceLabel(lblStatsSet, toolsWidth - 486, 32, 116, 28, ContentAlignment.MiddleRight);
        PlaceLabel(lblStatsItem, 16, 32, Math.Max(120, toolsWidth - 520), 28,
            ContentAlignment.MiddleLeft);
    }

    private void LayoutBundleTab(Size pageArea, Control view)
    {
        if (view == null)
            return;

        var scale = DeviceDpi / 96f;
        var pageWidth = (int)(pageArea.Width / scale);
        var pageHeight = (int)(pageArea.Height / scale);

        if (pageWidth < 400 || pageHeight < 240)
            return;

        Place(view, 0, 0, pageWidth, pageHeight);
    }

    private void ApplyColumnWidths()
    {
        var blueWidth = (int)(lvItemBlues.ClientSize.Width / (DeviceDpi / 96f));
        if (blueWidth > 120)
        {
            colBlueName.Width = Math.Max(80, (int)(blueWidth * 0.56));
            colBlueValue.Width = Math.Max(46, (int)(blueWidth * 0.20));
            colBlueMax.Width = Math.Max(46, blueWidth - colBlueName.Width - colBlueValue.Width - 24);
        }

        var statWidth = (int)(lvItemStats.ClientSize.Width / (DeviceDpi / 96f));
        if (statWidth > 120)
        {
            colStatName.Width = Math.Max(100, (int)(statWidth * 0.72));
            colStatValue.Width = Math.Max(50, statWidth - colStatName.Width - 24);
        }

        var logWidth = (int)(lvLog.ClientSize.Width / (DeviceDpi / 96f));
        if (logWidth > 160)
        {
            colItem.Width = Math.Max(120, (int)(logWidth * 0.28));
            colResult.Width = Math.Max(160, logWidth - colItem.Width - 24);
        }
    }

    private void ApplyLocalization()
    {
        if (_localizing || tabMain == null)
            return;

        _localizing = true;

        try
        {
            tabAlchemy.Text = T("Alchemy.Tab.Main", "Alchemy", "الكيمياء");
            tabStats.Text = T("Alchemy.Tab.Stats", "Stats", "الإحصائيات");
            tabManufacture.Text = T("Alchemy.Tab.Manufacture", "Manufacture", "تصنيع");
            tabBreak.Text = T("Alchemy.Tab.Break", "Dismantle", "تكسير");

            groupItem.Text = T("Alchemy.Item.Title", "Item", "العنصر");
            lblItemSelection.Text = T("Alchemy.Item.Select", "Item:", "العنصر:");
            lblDegreeText.Text = T("Alchemy.Item.Degree", "Degree:", "الدرجة:");
            lblOptLevelText.Text = T("Alchemy.Item.Enhancement", "Enhancement:", "التحسين:");

            tabItemBlues.Text = T("Alchemy.Item.Blues", "Blues", "الـ Blues");
            tabItemStats.Text = T("Alchemy.Item.Stats", "Stats", "الإحصائيات");
            colBlueName.Text = T("Alchemy.Item.BlueOption", "Option", "الخاصية");
            colBlueValue.Text = T("Alchemy.Item.BlueValue", "Value", "القيمة");
            colBlueMax.Text = T("Alchemy.Item.BlueMax", "Max", "الأقصى");
            colStatName.Text = T("Alchemy.Item.StatName", "Attribute", "الإحصائية");
            colStatValue.Text = T("Alchemy.Item.StatValue", "Value", "النسبة");

            groupSettings.Text = T("Alchemy.Settings.Title", "Settings", "إعدادات العملية");
            tabEnhance.Text = T("Alchemy.Settings.Enhance", "Enhance +", "تحسين +");
            tabBlues.Text = T("Alchemy.Settings.Blues", "Blues", "الـ Blues");
            btnSelectAllBlues.Text = T("Alchemy.Settings.SelectAllBlues", "Select all Blues", "حدد كل الـ Blues");

            groupLog.Text = T("Alchemy.Log.Title", "Log", "سجل العمليات");
            colItem.Text = T("Alchemy.Log.Item", "Item", "العنصر");
            colResult.Text = T("Alchemy.Log.Message", "Message", "الرسالة");
            btnClearLog.Text = T("Alchemy.Log.Clear", "Clear", "مسح");

            groupStatsTools.Text = T("Alchemy.Stats.Title", "Stats settings", "إعدادات الإحصائيات");
            btnSelectAllStats.Text = T("Alchemy.Stats.SelectAll", "Select all", "تحديد الكل");
            lblStatsSet.Text = T("Alchemy.Stats.SetAll", "Set all to:", "تعيين الكل إلى:");
            btnApplyStats.Text = T("Alchemy.Stats.Apply", "Set", "تعيين");
            lblStatsItem.Text = T("Alchemy.Stats.Item", "Item:", "العنصر:") + " —";

            _enhanceSettingsView?.ApplyLocalization();
            _magicOptionsSettingsView?.ApplyLocalization();
            _attributeSettingsView?.ApplyLocalization();
            _manufactureSettingsView?.ApplyLocalization();
            _dismantleSettingsView?.ApplyLocalization();
        }
        finally
        {
            _localizing = false;
        }
    }

    private void ApplyTheme()
    {
        if (tabMain == null)
            return;

        var foreColor = ColorScheme.ForeColor;
        var mutedColor = ColorScheme.BackColor.IsDark()
            ? Color.FromArgb(168, 184, 204)
            : Color.FromArgb(90, 104, 124);

        BackColor = ColorScheme.BackColor;

        foreach (var group in new[] { groupItem, groupSettings, groupLog, groupStatsTools })
        {
            if (group == null)
                continue;

            group.ForeColor = foreColor;
        }

        foreach (var label in new[]
                 {
                     lblItemSelection, lblDegreeText, lblOptLevelText, lblDegree, lblOptLevel,
                     lblStatsSet, lblStatsItem
                 })
        {
            if (label == null)
                continue;

            label.ForeColor = mutedColor;
        }

        lblDegree.ForeColor = foreColor;
        lblOptLevel.ForeColor = foreColor;
        lblStatsItem.ForeColor = ColorScheme.AccentColor;
        linkRefreshItemList.ForeColor = ColorScheme.AccentColor;

        comboItem.BackColor = ColorScheme.InputBackColor;
        comboItem.ForeColor = foreColor;

        foreach (var list in new[] { lvItemBlues, lvItemStats, lvLog })
        {
            if (list == null)
                continue;

            list.BackColor = ColorScheme.TableBackColor;
            list.ForeColor = foreColor;
        }

        foreach (var button in new[] { btnSelectAllBlues, btnSelectAllStats, btnApplyStats, btnClearLog })
        {
            if (button == null)
                continue;

            button.ForeColor = foreColor;
        }

        numStatsValue.BackColor = ColorScheme.InputBackColor;
        numStatsValue.ForeColor = foreColor;

        tabMain.Invalidate();
        tabEngine.Invalidate();
        tabItemInfo.Invalidate();

        _enhanceSettingsView?.ApplyTheme();
        _magicOptionsSettingsView?.ApplyTheme();
        _attributeSettingsView?.ApplyTheme();
        _manufactureSettingsView?.ApplyTheme();
        _dismantleSettingsView?.ApplyTheme();
    }

    private void OnAlchemy(AlchemyType type)
    {
        if (IsDisposed || Disposing)
            return;

        ReloadItemList();
    }

    /// <summary>
    ///     Reloads the list of inventory items that can be used for alchemy actions.
    /// </summary>
    private void ReloadItemList()
    {
        IsRefreshing = true;

        comboItem.Items.Clear();

        var items = Game.Player.Inventory.Where(i => i.Record.IsEquip && !i.Record.IsAvatar && !i.Record.IsJobOutfit)
            .ToList();

        try
        {
            foreach (var item in items)
            {
                var newComboItem = new InventoryItemComboboxItem(item);

                if (item.Slot > 12)
                    comboItem.Items.Add(newComboItem);

                if (SelectedItem != null && item.Slot == SelectedItem.Slot)
                    comboItem.SelectedItem = newComboItem;
            }
        }
        catch (Exception e)
        {
            Log.Debug($"[Alchemy] Unknown error while reloading item list: {e.Message}");
        }
        finally
        {
            IsRefreshing = false;
        }

        if (comboItem.SelectedItem == null)
        {
            SelectedItem = null;
            ResetItemDetails();
        }
    }

    private void ResetItemDetails()
    {
        lblDegree.Text = "—";
        lblOptLevel.Text = "—";
        lvItemBlues.Items.Clear();
        lvItemStats.Items.Clear();
        lblStatsItem.Text = T("Alchemy.Stats.Item", "Item:", "العنصر:") + " —";
    }

    /// <summary>
    ///     Populates the attributes list.
    /// </summary>
    /// <param name="item">The item.</param>
    private void PopulateAttributes(InventoryItem item)
    {
        lvItemStats.Items.Clear();

        if (item.Attributes == 0)
            return;

        var availableAttributes = ItemAttributesInfo.GetAvailableAttributeGroupsForItem(item.Record);

        if (availableAttributes == null)
            return;

        foreach (var attribute in availableAttributes)
        {
            var slot = ItemAttributesInfo.GetAttributeSlotForItem(attribute, item.Record);
            var translation = attribute.GetTranslation();

            var row = new ListViewItem(translation);
            row.SubItems.Add($"+{item.Attributes.GetPercentage(slot)}%");
            lvItemStats.Items.Add(row);
        }
    }

    /// <summary>
    ///     Populates the list of magic options of the selected item
    /// </summary>
    /// <param name="item">The selected item</param>
    private void PopulateMagicOptions(InventoryItem item)
    {
        lvItemBlues.Items.Clear();

        if (item.MagicOptions == null)
            return;

        foreach (var magicOption in item.MagicOptions)
        {
            var option = Game.ReferenceManager.GetMagicOption(magicOption.Id);
            if (option == null)
                continue;

            var row = new ListViewItem(option.GetGroupTranslation());
            row.SubItems.Add(magicOption.Value.ToString());
            row.SubItems.Add(option.GetMaxValue().ToString());
            lvItemBlues.Items.Add(row);
        }
    }

    /// <summary>
    ///     Adds a new log message to the alchemy log listview
    /// </summary>
    /// <param name="itemName"></param>
    /// <param name="message"></param>
    public void AddLog(string itemName, string message)
    {
        if (lvLog == null || lvLog.IsDisposed)
            return;

        var item = new ListViewItem(itemName);

        item.SubItems.Add(message);

        lvLog.Items.Add(item);

        //Scroll to bottom of the list
        lvLog.Items[lvLog.Items.Count - 1].EnsureVisible();

        Log.Notify($"[Alchemy] [{itemName}]: {message}");
    }

    #endregion Methods

    #region Events

    /// <summary>
    ///     Will be triggered when the user clicks the refresh link
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void linkRefreshItemList_Click(object sender, EventArgs e)
    {
        Invoke(ReloadItemList);
    }

    /// <summary>
    ///     Will be triggered when the user selects a new item or if an item was destroyed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void comboItem_SelectedIndexChanged(object sender, EventArgs e)
    {
        IsRefreshing = true;

        try
        {
            if (comboItem.SelectedIndex < 0)
            {
                SelectedItem = null;
                ResetItemDetails();
                return;
            }

            var selectedItem = comboItem.Items[comboItem.SelectedIndex] as InventoryItemComboboxItem;
            SelectedItem = selectedItem?.InventoryItem;

            if (SelectedItem == null)
                return;

            Invoke(() => PopulateAttributes(SelectedItem));
            Invoke(() => PopulateMagicOptions(SelectedItem));

            lblDegree.Text = SelectedItem.Record.Degree.ToString();
            lblOptLevel.Text = $"+{SelectedItem.OptLevel}";
            lblStatsItem.Text = $"{T("Alchemy.Stats.Item", "Item:", "العنصر:")} {SelectedItem.Record.GetRealName()} (+{SelectedItem.OptLevel})";
            ItemChanged?.Invoke(SelectedItem);
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    /// <summary>
    ///     Reloads the engine settings depending on the selected engine
    /// </summary>
    /// <param name="alchemyEngine">The engine to load the settings for</param>
    private void LoadEngineSettings(AlchemyEngine alchemyEngine)
    {
        if (_applyingEngine)
            return;

        if (_enhanceSettingsView == null || _magicOptionsSettingsView == null || _attributeSettingsView == null)
            return;

        _applyingEngine = true;
        var wasRefreshing = IsRefreshing;
        IsRefreshing = true;

        try
        {
            if (Globals.Botbase != null && !Kernel.Bot.Running)
                Globals.Botbase.AlchemyEngine = alchemyEngine;

            _enhanceSettingsView.Hide();
            _magicOptionsSettingsView.Hide();
            _attributeSettingsView.Hide();
            _manufactureSettingsView?.Hide();
            _dismantleSettingsView?.Hide();

            if (alchemyEngine == AlchemyEngine.Enhance)
            {
                _enhanceSettingsView.Show();

                if (tabEngine.SelectedIndex != 0)
                    tabEngine.SelectedIndex = 0;

                if (tabMain.SelectedIndex != 0)
                    tabMain.SelectedIndex = 0;
            }

            if (alchemyEngine == AlchemyEngine.Magic)
            {
                _magicOptionsSettingsView.Show();

                if (tabEngine.SelectedIndex != 1)
                    tabEngine.SelectedIndex = 1;

                if (tabMain.SelectedIndex != 0)
                    tabMain.SelectedIndex = 0;
            }

            if (alchemyEngine == AlchemyEngine.Attribute)
            {
                _attributeSettingsView.Show();

                if (tabMain.SelectedIndex != 1)
                    tabMain.SelectedIndex = 1;
            }

            if (alchemyEngine == AlchemyEngine.Manufacture)
            {
                _manufactureSettingsView?.Show();

                if (tabMain.SelectedIndex != 2)
                    tabMain.SelectedIndex = 2;
            }

            if (alchemyEngine == AlchemyEngine.Dismantle)
            {
                _dismantleSettingsView?.Show();

                if (tabMain.SelectedIndex != 3)
                    tabMain.SelectedIndex = 3;
            }

            EngineChanged?.Invoke(SelectedItem, alchemyEngine);
        }
        finally
        {
            IsRefreshing = wasRefreshing;
            _applyingEngine = false;
        }
    }

    private void tabEngine_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_applyingEngine || _enhanceSettingsView == null)
            return;

        LoadEngineSettings(tabEngine.SelectedIndex == 0 ? AlchemyEngine.Enhance : AlchemyEngine.Magic);
        QueueAlchemyLayout();
    }

    private void tabMain_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_applyingEngine || _enhanceSettingsView == null)
            return;

        try
        {
            if (tabMain.SelectedIndex == 0)
                LoadEngineSettings(tabEngine.SelectedIndex == 0 ? AlchemyEngine.Enhance : AlchemyEngine.Magic);
            else if (tabMain.SelectedIndex == 1)
                LoadEngineSettings(AlchemyEngine.Attribute);
            else if (tabMain.SelectedIndex == 2)
                LoadEngineSettings(AlchemyEngine.Manufacture);
            else if (tabMain.SelectedIndex == 3)
                LoadEngineSettings(AlchemyEngine.Dismantle);

            QueueAlchemyLayout();
        }
        catch (Exception ex)
        {
            DebugTabException(ex);
            throw;
        }
    }

    internal static void DebugTabException(Exception ex)
    {
        try
        {
            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "alchemy-tab-error.log");
            System.IO.File.AppendAllText(path, DateTime.Now + " " + ex + Environment.NewLine + Environment.NewLine);
        }
        catch
        {
            // ignored
        }
    }

    private void btnSelectAllBlues_Click(object sender, EventArgs e)
    {
        _magicOptionsSettingsView?.SelectAll();
    }

    private void btnSelectAllStats_Click(object sender, EventArgs e)
    {
        _attributeSettingsView?.SelectAll();
    }

    private void btnApplyStats_Click(object sender, EventArgs e)
    {
        _attributeSettingsView?.SetAllMaxValue((int)numStatsValue.Value);
    }

    private void btnClearLog_Click(object sender, EventArgs e)
    {
        lvLog?.Items.Clear();
    }

    #endregion Events
}
