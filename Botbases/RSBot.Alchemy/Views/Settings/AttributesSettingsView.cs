using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RSBot.Alchemy.Bot;
using RSBot.Alchemy.Bundle.Attribute;
using RSBot.Alchemy.Helper;
using RSBot.Core;
using RSBot.Core.Event;
using RSBot.Core.Objects;
using SDUI;
using SDUI.Controls;

namespace RSBot.Alchemy.Views.Settings;

[ToolboxItem(false)]
public partial class AttributesSettingsView : UserControl
{
    private List<AttributeInfoPanel> _attributePanels;

    public AttributesSettingsView()
    {
        InitializeComponent();
        DoubleBuffered = true;

        CheckForIllegalCrossThreadCalls = false;

        EventManager.SubscribeEvent("OnEnterGame", SubscribeMainFormEvents);
    }

    internal AttributeBundleConfig BundleConfig
    {
        get
        {
            var result = new AttributeBundleConfig { Item = SelectedItem };

            var attributes = new List<AttributeBundleConfig.AttributeBundleConfigItem>(10);

            foreach (var attributePanel in _attributePanels)
            {
                if (!attributePanel.Checked)
                    continue;

                if (attributePanel.Stones != null && attributePanel.Stones.Any())
                    attributes.Add(new AttributeBundleConfig.AttributeBundleConfigItem
                    {
                        Group = attributePanel.AttributeGroup, MaxValue = attributePanel.MaxValue,
                        Stone = attributePanel.Stones.First()
                    });
            }

            result.Attributes = attributes;

            return result;
        }
    }

    private InventoryItem? SelectedItem { get; set; }

    /// <summary>
    ///     Subscribes to the ItemChanged event
    /// </summary>
    private void SubscribeMainFormEvents()
    {
        if (Globals.View != null)
        {
            Globals.View.ItemChanged += View_ItemChanged;
            Globals.View.EngineChanged += View_EngineChanged;
        }
    }

    public void PopulateView()
    {
        try
        {
            _attributePanels = new List<AttributeInfoPanel>(6);
            var selectedItem = Globals.View.SelectedItem;

            if (selectedItem == null)
            {
                SuspendLayout();
                var old = Controls.Cast<Control>().ToArray();
                Controls.Clear();
                foreach (var control in old)
                    control.Dispose();
                ResumeLayout(true);

                return;
            }

            var availableItemAttributes = ItemAttributesInfo.GetAvailableAttributeGroupsForItem(selectedItem.Record);

            if (availableItemAttributes == null)
            {
                Log.Error(
                    $"[Alchemy] Could not identify the selected item's attribute information. [ItemId = {selectedItem.ItemId}]");

                return;
            }

            if (Globals.Botbase.AttributeBundleConfig == null)
                Globals.Botbase.AttributeBundleConfig = BundleConfig;

            foreach (var attributeGroup in availableItemAttributes)
            {
                var matchingStones = AlchemyItemHelper.GetAttributeStones(selectedItem, attributeGroup);

                var config =
                    Globals.Botbase.AttributeBundleConfig.Attributes.FirstOrDefault(x => x.Group == attributeGroup);

                var panel = new AttributeInfoPanel(attributeGroup, matchingStones, selectedItem,
                    config == null ? 0 : config.MaxValue) { Dock = DockStyle.Top };
                _attributePanels.Add(panel);

                if (config == null || !matchingStones.Any())
                {
                    panel.OnChange += PanelOnChange;

                    continue;
                }

                panel.Checked = true;
                panel.OnChange += PanelOnChange;
            }

            SuspendLayout();
            var oldPanels = Controls.Cast<Control>().ToArray();
            Controls.Clear();
            foreach (var control in oldPanels)
                control.Dispose();
            Controls.AddRange(_attributePanels.ToArray());
            ResumeLayout(true);

            Globals.Botbase.AttributeBundleConfig = BundleConfig;
        }
        catch (Exception ex)
        {
            Log.Debug($"[Alchemy] Error refreshing attributes view: {ex.Message}");
        }
    }

    private void PanelOnChange(bool @checked, int maxValue)
    {
        Globals.Botbase.AttributeBundleConfig = BundleConfig;
    }

    /// <summary>
    ///     Selects every attribute row that still has stones available.
    /// </summary>
    public void SelectAll()
    {
        if (_attributePanels == null)
            return;

        foreach (var panel in _attributePanels)
        {
            if (panel == null || !panel.Enabled)
                continue;

            if (panel.CanBeSelected)
                panel.Checked = true;
        }

        Globals.Botbase.AttributeBundleConfig = BundleConfig;
    }

    /// <summary>
    ///     Applies the same target percentage to every attribute row.
    /// </summary>
    public void SetAllMaxValue(int maxValue)
    {
        if (_attributePanels == null)
            return;

        foreach (var panel in _attributePanels)
        {
            if (panel == null || !panel.Enabled)
                continue;

            panel.SetMaxValue(maxValue);
        }

        Globals.Botbase.AttributeBundleConfig = BundleConfig;
    }

    public void ApplyLocalization()
    {
        // Attribute captions come from the game reference data; only the shared
        // row controls need localization.
        if (_attributePanels == null)
            return;

        foreach (var panel in _attributePanels)
            panel?.ApplyLocalization();
    }

    public void ApplyTheme()
    {
        BackColor = Color.Transparent;

        if (_attributePanels == null)
            return;

        foreach (var panel in _attributePanels)
            panel?.ApplyTheme();
    }

    private void View_EngineChanged(InventoryItem item, AlchemyEngine alchemyEngine)
    {
        PopulateView();
    }

    /// <summary>
    ///     Will be triggered when the selected item changed
    /// </summary>
    /// <param name="item"></param>
    private void View_ItemChanged(InventoryItem item)
    {
        SelectedItem = item;
        PopulateView();
    }
}