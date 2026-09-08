using System;
using System.ComponentModel;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Client.ReferenceObjects;
using SDUI.Controls;
using SDUI.Helpers;

namespace RSBot.AvatarTester.Views;

[ToolboxItem(false)]
public partial class Main : DoubleBufferedControl
{
    private static Main _instance;
    public static Main Instance => _instance ??= new Main();

    public Main()
    {
        InitializeComponent();
        ControlSpacer.NormalizePage(this);

        AvatarTesterManager.Instance.OnStateChanged += () => SafeInvoke(UpdateUI);
    }

    private void SafeInvoke(System.Action action)
    {
        if (IsHandleCreated && InvokeRequired)
            BeginInvoke(action);
        else
            action();
    }

    private void Main_Load(object sender, EventArgs e)
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        var mgr = AvatarTesterManager.Instance;

        lblSelectedHat.Text = mgr.SelectedHatId != 0
            ? GetItemDisplayName(mgr.SelectedHatId)
            : "(None)";

        lblSelectedDress.Text = mgr.SelectedDressId != 0
            ? GetItemDisplayName(mgr.SelectedDressId)
            : "(None)";

        lblSelectedAccessory.Text = mgr.SelectedAccessoryId != 0
            ? GetItemDisplayName(mgr.SelectedAccessoryId)
            : "(None)";

        lblSelectedFlag.Text = mgr.SelectedFlagId != 0
            ? GetItemDisplayName(mgr.SelectedFlagId)
            : "(None)";

        lblPreviewStatus.Text = mgr.IsPreviewSpawned
            ? "Preview Status: Active (Spawned in-game)"
            : "Preview Status: Not spawned";
    }

    private string GetItemDisplayName(uint itemId)
    {
        if (Game.ReferenceManager?.ItemData != null && Game.ReferenceManager.ItemData.TryGetValue(itemId, out var item))
            return $"{item.GetRealName()} ({item.ID})";

        return $"ID: {itemId}";
    }

    private void btnSearch_Click(object sender, EventArgs e)
    {
        var filter = txtSearch.Text.Trim();
        var items = AvatarTesterManager.Instance.SearchAvatarItems(filter);

        lstSearchResults.BeginUpdate();
        lstSearchResults.Items.Clear();

        foreach (var item in items)
        {
            var lvi = new ListViewItem(item.ID.ToString());
            var code = item.CodeName ?? "";
            var slotType = "Other";
            if (item.TypeID4 == 1 || code.Contains("_HAT", StringComparison.OrdinalIgnoreCase) || code.Contains("_HELM", StringComparison.OrdinalIgnoreCase))
                slotType = "Hat";
            else if (item.TypeID4 == 2 || code.Contains("_DRESS", StringComparison.OrdinalIgnoreCase) || code.Contains("_BODY", StringComparison.OrdinalIgnoreCase) || code.Contains("_SUIT", StringComparison.OrdinalIgnoreCase))
                slotType = "Dress";
            else if (item.TypeID4 == 3 || code.Contains("_ATTACH", StringComparison.OrdinalIgnoreCase) || code.Contains("_WING", StringComparison.OrdinalIgnoreCase) || code.Contains("_ACC", StringComparison.OrdinalIgnoreCase))
                slotType = "Accessory";
            else if (item.TypeID4 == 4 || item.TypeID4 == 5 || code.Contains("_FLAG", StringComparison.OrdinalIgnoreCase) || code.Contains("NASRUN", StringComparison.OrdinalIgnoreCase))
                slotType = "Flag";

            lvi.SubItems.Add(slotType);
            lvi.SubItems.Add(item.GetRealName());
            lvi.SubItems.Add(item.CodeName ?? "");
            lvi.Tag = item;

            lstSearchResults.Items.Add(lvi);
        }

        lstSearchResults.EndUpdate();
    }

    private RefObjItem GetSelectedItem()
    {
        if (lstSearchResults.SelectedItems.Count == 0)
            return null;

        return lstSearchResults.SelectedItems[0].Tag as RefObjItem;
    }

    private void btnSetHat_Click(object sender, EventArgs e)
    {
        var item = GetSelectedItem();
        if (item != null)
        {
            AvatarTesterManager.Instance.SelectedHatId = item.ID;
            UpdateUI();
        }
    }

    private void btnSetDress_Click(object sender, EventArgs e)
    {
        var item = GetSelectedItem();
        if (item != null)
        {
            AvatarTesterManager.Instance.SelectedDressId = item.ID;
            UpdateUI();
        }
    }

    private void btnSetAccessory_Click(object sender, EventArgs e)
    {
        var item = GetSelectedItem();
        if (item != null)
        {
            AvatarTesterManager.Instance.SelectedAccessoryId = item.ID;
            UpdateUI();
        }
    }

    private void btnSetFlag_Click(object sender, EventArgs e)
    {
        var item = GetSelectedItem();
        if (item != null)
        {
            AvatarTesterManager.Instance.SelectedFlagId = item.ID;
            UpdateUI();
        }
    }

    private void btnSpawnPreview_Click(object sender, EventArgs e)
    {
        AvatarTesterManager.Instance.SpawnAvatarPreview();
    }

    private void btnClearPreview_Click(object sender, EventArgs e)
    {
        AvatarTesterManager.Instance.DespawnAvatarPreview();
    }
}
