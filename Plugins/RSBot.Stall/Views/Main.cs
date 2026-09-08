using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Event;
using RSBot.Core.Extensions;
using RSBot.Core.Objects;
using RSBot.Stall.Objects;
using SDUI;
using SDUI.Controls;
using SDUI.Helpers;
using ListViewExtensions = RSBot.Core.Extensions.ListViewExtensions;

namespace RSBot.Stall.Views;

[ToolboxItem(false)]
public partial class Main : DoubleBufferedControl
{
    private static Main _instance;
    public static Main Instance => _instance ??= new Main();

    public Main()
    {
        CheckForIllegalCrossThreadCalls = false;
        InitializeComponent();
        ControlSpacer.NormalizePage(this);

        listViewInventory.SmallImageList = ListViewExtensions.StaticItemsImageList;
        listViewStall.SmallImageList = ListViewExtensions.StaticItemsImageList;

        SubscribeEvents();
        InitializeStallSlots();
    }

    private void SubscribeEvents()
    {
        EventManager.SubscribeEvent("OnLoadCharacter", OnLoadCharacter);
        EventManager.SubscribeEvent("OnInventoryUpdate", OnInventoryUpdate);

        StallManager.Instance.OnStateChanged += () => SafeInvoke(UpdateStallStateUI);
        StallManager.Instance.OnItemsChanged += () => SafeInvoke(UpdateStallItemsUI);
        StallManager.Instance.OnLogAdded += log => SafeInvoke(() => AppendLog(log));
    }

    private void SafeInvoke(System.Action action)
    {
        if (IsHandleCreated && InvokeRequired)
            BeginInvoke(action);
        else
            action();
    }

    private void OnLoadCharacter()
    {
        SafeInvoke(() =>
        {
            RefreshInventory();
            UpdateStallStateUI();
            UpdateStallItemsUI();
        });
    }

    private void OnInventoryUpdate()
    {
        SafeInvoke(RefreshInventory);
    }

    private void InitializeStallSlots()
    {
        var emptyText = RSBot.Core.Components.LanguageManager.GetLangBySpecificKey("SonicBot.Stall", "StallEmptySlot", "- فارغ -");
        if (string.IsNullOrEmpty(emptyText)) emptyText = "- فارغ -";

        listViewStall.Items.Clear();
        for (var i = 0; i < 10; i++)
        {
            var lvi = new ListViewItem((i + 1).ToString());
            lvi.SubItems.Add(emptyText);
            lvi.SubItems.Add("-");
            lvi.SubItems.Add("-");
            listViewStall.Items.Add(lvi);
        }
    }

    private void RefreshInventory()
    {
        if (Game.Player?.Inventory == null)
            return;

        listViewInventory.BeginUpdate();
        listViewInventory.Items.Clear();

        var items = Game.Player.Inventory.GetNormalPartItems();
        foreach (var item in items)
        {
            if (item?.Record == null)
                continue;

            var name = item.Record.GetRealName();
            if (item.OptLevel > 0)
                name += $" (+{item.OptLevel})";

            var lvi = new ListViewItem(name);
            lvi.SubItems.Add(item.Amount.ToString());
            lvi.SubItems.Add(item.Slot.ToString());
            lvi.Tag = item;

            lvi.LoadItemImageAsync(item.Record);
            listViewInventory.Items.Add(lvi);
        }

        listViewInventory.EndUpdate();
    }

    private void listViewInventory_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (listViewInventory.SelectedItems.Count != 1)
            return;

        if (listViewInventory.SelectedItems[0].Tag is InventoryItem item)
        {
            var maxAmount = Math.Max(1, (int)item.Amount);
            numQuantity.Maximum = maxAmount;
            numQuantity.Value = maxAmount;
        }
    }

    private void btnAddToStall_Click(object sender, EventArgs e)
    {
        if (!StallManager.Instance.IsCreated)
        {
            MessageBox.Show("Please create the stall first.", "SonicBot Stall", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (StallManager.Instance.IsOpen)
        {
            MessageBox.Show("Please switch the stall to Modify mode before adding items.", "SonicBot Stall", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (listViewInventory.SelectedItems.Count != 1)
        {
            MessageBox.Show("Please select an item from your inventory.", "SonicBot Stall", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (listViewInventory.SelectedItems[0].Tag is not InventoryItem invItem)
            return;

        var priceStr = txtPrice.Text.Replace(",", "").Replace(".", "").Trim();
        if (!ulong.TryParse(priceStr, out var price) || price == 0)
        {
            MessageBox.Show("Please enter a valid price in Gold.", "SonicBot Stall", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtPrice.Focus();
            return;
        }

        // Find next empty stall slot (0 to 9)
        byte emptySlot = byte.MaxValue;
        for (byte i = 0; i < StallManager.Instance.Slots.Length; i++)
        {
            if (StallManager.Instance.Slots[i] == null)
            {
                emptySlot = i;
                break;
            }
        }

        if (emptySlot == byte.MaxValue)
        {
            MessageBox.Show("The stall is full (10 slots maximum).", "SonicBot Stall", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var qty = (ushort)numQuantity.Value;
        StallManager.Instance.AddItem(emptySlot, invItem.Slot, qty, price);
    }

    private void btnRefreshInventory_Click(object sender, EventArgs e)
    {
        RefreshInventory();
    }

    private void btnCreateStall_Click(object sender, EventArgs e)
    {
        var title = txtTitle.Text.Trim();
        if (string.IsNullOrEmpty(title))
            title = "SonicBot Shop";

        var note = txtNote.Text.Trim();
        if (string.IsNullOrEmpty(note))
            note = "Welcome!";

        StallManager.Instance.CreateStall(title, note);
    }

    private void btnToggleState_Click(object sender, EventArgs e)
    {
        if (!StallManager.Instance.IsCreated)
            return;

        StallManager.Instance.SetStallState(!StallManager.Instance.IsOpen);
    }

    private void btnCloseStall_Click(object sender, EventArgs e)
    {
        if (!StallManager.Instance.IsCreated)
            return;

        StallManager.Instance.CloseStall();
    }

    private void btnUpdateTitleNote_Click(object sender, EventArgs e)
    {
        if (!StallManager.Instance.IsCreated)
            return;

        var title = txtTitle.Text.Trim();
        if (!string.IsNullOrEmpty(title))
            StallManager.Instance.UpdateTitle(title);

        var note = txtNote.Text.Trim();
        if (!string.IsNullOrEmpty(note))
            StallManager.Instance.UpdateNote(note);
    }

    private void menuEditPrice_Click(object sender, EventArgs e)
    {
        if (!StallManager.Instance.IsCreated || StallManager.Instance.IsOpen)
        {
            MessageBox.Show("Stall must be in Modify mode to edit prices.", "SonicBot Stall", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (listViewStall.SelectedIndices.Count != 1)
            return;

        var slotIndex = (byte)listViewStall.SelectedIndices[0];
        if (slotIndex >= StallManager.Instance.Slots.Length || StallManager.Instance.Slots[slotIndex] == null)
            return;

        var stallItem = StallManager.Instance.Slots[slotIndex];
        var prompt = new InputDialog("Edit Stall Item", "New Price", "Enter new price in Gold:", InputDialog.InputType.Textbox, stallItem.Price.ToString());
        if (prompt.ShowDialog(this) == DialogResult.OK)
        {
            var valStr = prompt.Value?.ToString()?.Replace(",", "")?.Replace(".", "")?.Trim();
            if (ulong.TryParse(valStr, out var newPrice) && newPrice > 0)
            {
                StallManager.Instance.EditItem(slotIndex, stallItem.Quantity, newPrice);
            }
        }
    }

    private void menuRemoveItem_Click(object sender, EventArgs e)
    {
        if (!StallManager.Instance.IsCreated || StallManager.Instance.IsOpen)
        {
            MessageBox.Show("Stall must be in Modify mode to remove items.", "SonicBot Stall", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (listViewStall.SelectedIndices.Count != 1)
            return;

        var slotIndex = (byte)listViewStall.SelectedIndices[0];
        if (slotIndex >= StallManager.Instance.Slots.Length || StallManager.Instance.Slots[slotIndex] == null)
            return;

        StallManager.Instance.RemoveItem(slotIndex);
    }

    private void btnClearLog_Click(object sender, EventArgs e)
    {
        txtLog.Clear();
        StallManager.Instance.ClearLogs();
    }

    private void UpdateStallStateUI()
    {
        if (StallManager.Instance.IsCreated)
        {
            var openStatus = RSBot.Core.Components.LanguageManager.GetLangBySpecificKey("SonicBot.Stall", "StatusOpen", "الحالة: مفتوح");
            var modifyStatus = RSBot.Core.Components.LanguageManager.GetLangBySpecificKey("SonicBot.Stall", "StatusModify", "الحالة: تعديل");
            lblStatus.Text = StallManager.Instance.IsOpen ? openStatus : modifyStatus;
            lblStatus.ForeColor = StallManager.Instance.IsOpen ? Color.LimeGreen : Color.Orange;
            btnCreateStall.Enabled = false;
            btnToggleState.Enabled = true;

            var modifyBtn = RSBot.Core.Components.LanguageManager.GetLangBySpecificKey("SonicBot.Stall", "btnModifyStall", "تعديل الكشك");
            var openBtn = RSBot.Core.Components.LanguageManager.GetLangBySpecificKey("SonicBot.Stall", "btnToggleState", "فتح الكشك");
            btnToggleState.Text = StallManager.Instance.IsOpen ? modifyBtn : openBtn;
            btnCloseStall.Enabled = true;
            btnUpdateTitleNote.Enabled = true;
            btnAddToStall.Enabled = !StallManager.Instance.IsOpen;
        }
        else
        {
            var notCreatedStatus = RSBot.Core.Components.LanguageManager.GetLangBySpecificKey("SonicBot.Stall", "lblStatus", "الحالة: غير منشأ");
            lblStatus.Text = notCreatedStatus;
            lblStatus.ForeColor = Color.Gray;
            btnCreateStall.Enabled = true;
            btnToggleState.Enabled = false;

            var openBtn = RSBot.Core.Components.LanguageManager.GetLangBySpecificKey("SonicBot.Stall", "btnToggleState", "فتح الكشك");
            btnToggleState.Text = openBtn;
            btnCloseStall.Enabled = false;
            btnUpdateTitleNote.Enabled = false;
            btnAddToStall.Enabled = false;
        }
    }

    private void UpdateStallItemsUI()
    {
        var emptyText = RSBot.Core.Components.LanguageManager.GetLangBySpecificKey("SonicBot.Stall", "StallEmptySlot", "- فارغ -");
        if (string.IsNullOrEmpty(emptyText)) emptyText = "- فارغ -";

        listViewStall.BeginUpdate();
        for (var i = 0; i < 10; i++)
        {
            var lvi = listViewStall.Items[i];
            var stallItem = StallManager.Instance.Slots[i];

            if (stallItem?.Item?.Record != null)
            {
                var name = stallItem.Item.Record.GetRealName();
                if (stallItem.Item.OptLevel > 0)
                    name += $" (+{stallItem.Item.OptLevel})";

                lvi.SubItems[1].Text = name;
                lvi.SubItems[2].Text = stallItem.Quantity.ToString();
                lvi.SubItems[3].Text = stallItem.Price.ToString("N0", CultureInfo.InvariantCulture) + " ذهب";
                lvi.LoadItemImageAsync(stallItem.Item.Record);
            }
            else
            {
                lvi.SubItems[1].Text = emptyText;
                lvi.SubItems[2].Text = "-";
                lvi.SubItems[3].Text = "-";
                lvi.ImageIndex = -1;
            }
        }
        listViewStall.EndUpdate();

        RefreshInventory();
    }

    private void AppendLog(string log)
    {
        txtLog.AppendText(log + Environment.NewLine);
        txtLog.SelectionStart = txtLog.Text.Length;
        txtLog.ScrollToCaret();
    }
}
