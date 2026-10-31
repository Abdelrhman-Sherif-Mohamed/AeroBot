using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RSBot.Alchemy.Bot;
using RSBot.Alchemy.Bundle.Magic;
using RSBot.Alchemy.Client.ReferenceObjects;
using RSBot.Alchemy.Helper;
using RSBot.Core;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Item;
using SDUI;
using SDUI.Controls;

namespace RSBot.Alchemy.Views.Settings;

[ToolboxItem(false)]
public partial class MagicOptionsSettingsView : UserControl
{
    #region Members

    private bool _reloadConfig;

    #endregion Members

    #region Constructor

    /// <summary>
    ///     Subscribes several events
    /// </summary>
    public MagicOptionsSettingsView()
    {
        CheckForIllegalCrossThreadCalls = false;

        InitializeComponent();
        DoubleBuffered = true;

        EventManager.SubscribeEvent("OnEnterGame", SubscribeMainFormEvents);
    }

    #endregion Constructor

    private class MagicStoneListViewItemTag
    {
        public InventoryItem? Item { get; set; }
        public RefMagicOpt? MagicOption { get; set; }

        public MagicOptionInfo? MagicOptionInfo { get; set; }

        public bool CanBeIncreased { get; set; }
    }

    #region Events

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

    private void View_EngineChanged(InventoryItem item, AlchemyEngine alchemyEngine)
    {
        PopulateListView();
    }

    /// <summary>
    ///     Will be triggered when the selected item changed
    /// </summary>
    /// <param name="item"></param>
    private void View_ItemChanged(InventoryItem item)
    {
        PopulateListView();
    }

    /// <summary>
    ///     Will be triggered when a list view item was selected
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void lvMagicOptions_ItemChecked(object sender, ItemCheckedEventArgs e)
    {
        var tag = e.Item.Tag as MagicStoneListViewItemTag;

        if (tag == null || !tag.CanBeIncreased || tag.Item == null)
        {
            e.Item.Checked = false;

            return;
        }

        ReloadConfig();
    }

    #endregion Events

    #region Methods

    /// <summary>
    ///     Populate the list view of available magic options for the selected item
    /// </summary>
    public void PopulateListView()
    {
        if (Globals.View.SelectedItem == null)
        {
            if (lvMagicOptions != null)
                lvMagicOptions.Items.Clear();

            return;
        }

        try
        {
            lvMagicOptions.BeginUpdate();
            lvMagicOptions.Items.Clear();
            _reloadConfig = false;

            var selectedItem = Globals.View.SelectedItem;
            if (selectedItem == null)
            {
                lvMagicOptions.EndUpdate();
                return;
            }

            var assignments =
                Game.ReferenceManager.GetAssignments(selectedItem.Record.TypeID3, selectedItem.Record.TypeID4);
            foreach (var assignment in assignments)
            {
                if (assignment == null)
                    continue;

                var matchingMagicStones = AlchemyItemHelper.GetStonesByGroup(selectedItem, assignment.Group);

                MagicOptionInfo currentMagicOptionInfo = null;
                if (selectedItem.MagicOptions != null)
                    foreach (var magicOption in selectedItem.MagicOptions)
                    {
                        ;
                        if (magicOption.Record?.Group != assignment.Group)
                            continue;

                        if (magicOption.Record.Level != selectedItem.Record.Degree)
                        {
                            //Fix in case the server sends a lower magic option id than expected (dunno why this happens)
                            var actualMagicOption = Game.ReferenceManager.GetMagicOption(assignment.Group,
                                (byte)selectedItem.Record.Degree);

                            matchingMagicStones =
                                AlchemyItemHelper.GetStonesByGroup(magicOption.Record.Level, assignment.Group);

                            currentMagicOptionInfo = new MagicOptionInfo
                                { Id = actualMagicOption.Id, Value = magicOption.Value };
                        }
                        else
                        {
                            currentMagicOptionInfo = magicOption;
                        }

                        break;
                    }

                var canBeIncreased = !!matchingMagicStones.Any();

                //Max option
                if (currentMagicOptionInfo != null &&
                    currentMagicOptionInfo.Value >= currentMagicOptionInfo.Record.GetMaxValue())
                    canBeIncreased = false;

                var refMagicOption =
                    Game.ReferenceManager.GetMagicOption(assignment.Group, (byte)selectedItem.Record.Degree);
                if (refMagicOption == null)
                    continue;

                var item = new ListViewItem(assignment.GetGroupTranslation())
                {
                    Tag = new MagicStoneListViewItemTag
                    {
                        Item = matchingMagicStones.FirstOrDefault(),
                        MagicOption = assignment,
                        MagicOptionInfo = currentMagicOptionInfo,
                        CanBeIncreased = canBeIncreased
                    },
                    ForeColor = canBeIncreased ? Color.Green : Color.Red
                };

                item.SubItems.Add(currentMagicOptionInfo == null ? "0" : currentMagicOptionInfo.Value.ToString());
                item.SubItems.Add(refMagicOption.GetMaxValue().ToString());
                item.SubItems.Add(!matchingMagicStones.Any() ? "x0" : $"x{matchingMagicStones.Sum(i => i.Amount)}");

                if (Globals.Botbase.MagicBundleConfig != null &&
                    Globals.Botbase.MagicBundleConfig.MagicStones.Keys.FirstOrDefault(i =>
                        i.Record.ID == matchingMagicStones.FirstOrDefault()?.ItemId) != null && canBeIncreased)
                    item.Checked = true;
                else
                    item.Checked = false;

                item.Font = new Font(Font, FontStyle.Bold);

                lvMagicOptions.Items.Add(item);
            }

            lvMagicOptions.EndUpdate();
            _reloadConfig = true;

            ReloadConfig();
        }
        catch (Exception ex)
        {
            Log.Debug($"[Alchemy] Unknown error while populating magic options settings list: {ex.Message}");
        }
    }

    /// <summary>
    ///     Reloads the configuration with the new selection of magic options
    /// </summary>
    private void ReloadConfig()
    {
        if (!_reloadConfig) return;

        Globals.Botbase.MagicBundleConfig = new MagicBundleConfig
        {
            Item = Globals.View.SelectedItem,
            MagicStones = new Dictionary<InventoryItem, RefMagicOpt>()
        };

        try
        {
            foreach (ListViewItem item in lvMagicOptions.CheckedItems)
            {
                var invItem = (MagicStoneListViewItemTag)item.Tag;

                if (invItem.Item != null)
                    Globals.Botbase.MagicBundleConfig.MagicStones.Add(invItem.Item, invItem.MagicOption);
            }
        }
        catch (Exception e)
        {
            Log.Warn($"[Alchemy] Unhandled exception while reloading magic options configuration: {e.Message}");
        }
    }

    /// <summary>
    ///     Checks every blues row that can still be increased.
    /// </summary>
    public void SelectAll()
    {
        if (lvMagicOptions == null || lvMagicOptions.Items.Count == 0)
            return;

        _reloadConfig = false;

        try
        {
            lvMagicOptions.BeginUpdate();

            foreach (ListViewItem item in lvMagicOptions.Items)
            {
                if (item.Tag is MagicStoneListViewItemTag tag && tag.CanBeIncreased && tag.Item != null)
                    item.Checked = true;
            }

            lvMagicOptions.EndUpdate();
        }
        finally
        {
            _reloadConfig = true;
        }

        ReloadConfig();
    }

    public void ApplyLocalization()
    {
        if (lvMagicOptions == null)
            return;

        columnHeader1.Text = T("Alchemy.Settings.BlueOption", "Option", "الخاصية");
        columnHeader2.Text = T("Alchemy.Settings.BlueCurrent", "Current", "الحالي");
        columnHeader3.Text = T("Alchemy.Settings.BlueMax", "Max", "الأقصى");
        columnHeader4.Text = T("Alchemy.Settings.BlueStones", "Num. Stones", "عدد الأحجار");
    }

    public void ApplyTheme()
    {
        if (lvMagicOptions == null)
            return;

        lvMagicOptions.BackColor = ColorScheme.TableBackColor;
        lvMagicOptions.ForeColor = ColorScheme.ForeColor;

        ApplyColumnWidths();
    }

    private void ApplyColumnWidths()
    {
        if (lvMagicOptions == null)
            return;

        var width = lvMagicOptions.ClientSize.Width;
        if (width <= 200)
            return;

        columnHeader1.Width = (int)(width * 0.42);
        columnHeader2.Width = (int)(width * 0.17);
        columnHeader3.Width = (int)(width * 0.17);
        columnHeader4.Width = Math.Max(60, width - columnHeader1.Width - columnHeader2.Width -
                                           columnHeader3.Width - 24);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        ApplyColumnWidths();
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);

        if (Visible)
            ApplyColumnWidths();
    }

    private static string T(string key, string english, string arabic)
    {
        var value = LanguageManager.GetLangBySpecificKey("RSBot.Alchemy", key, string.Empty);
        if (!string.IsNullOrWhiteSpace(value))
            return value;

        return Kernel.Language == "ar_AR" ? arabic : english;
    }

    #endregion Methods
}