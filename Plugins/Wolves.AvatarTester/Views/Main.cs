using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Extensions;
using SDUI.Controls;
using SDUI.Helpers;
using CoreListViewExtensions = RSBot.Core.Extensions.ListViewExtensions;

namespace Wolves.AvatarTester.Views;

[ToolboxItem(false)]
public partial class Main : DoubleBufferedControl
{
    private static Main _instance;
    public static Main Instance => _instance ??= new Main();

    private SwitchCategory _currentSwitchCategory = SwitchCategory.All;
    private System.Windows.Forms.Timer _searchDebounceTimer;

    public Main()
    {
        InitializeComponent();
        ControlSpacer.NormalizePage(this);

        AvatarTesterManager.Instance.OnStateChanged += () => SafeInvoke(UpdateUI);

        _searchDebounceTimer = new System.Windows.Forms.Timer { Interval = 350 };
        _searchDebounceTimer.Tick += (s, ev) =>
        {
            _searchDebounceTimer.Stop();
            TriggerSearch();
        };

        txtSearch.TextChanged += (s, ev) =>
        {
            _searchDebounceTimer.Stop();
            _searchDebounceTimer.Start();
        };

        txtSearch.KeyDown += (s, ev) =>
        {
            if (ev.KeyCode == Keys.Enter)
            {
                ev.SuppressKeyPress = true;
                _searchDebounceTimer.Stop();
                TriggerSearch();
            }
        };
    }

    private void SafeInvoke(Action action)
    {
        if (IsHandleCreated && InvokeRequired)
            BeginInvoke(action);
        else
            action();
    }

    private void Main_Load(object sender, EventArgs e)
    {
        // Bind item icon image list
        lstSearchResults.SmallImageList = CoreListViewExtensions.StaticItemsImageList;
        lstSwitches.SmallImageList = CoreListViewExtensions.StaticItemsImageList;

        if (cmbOptLevel.Items.Count > 0)
        {
            cmbOptLevel.SelectedIndex = 6; // Default to +16
            cmbOptLevel.SelectedIndexChanged += cmbOptLevel_SelectedIndexChanged;
        }

        ShowAvatarsTab();
        UpdateUI();

        // Performance: start with empty list and search prompt
        ShowEmptySearchPrompt();
    }

    #region Navigation Tab Switching

    private void btnTabAvatars_Click(object sender, EventArgs e)
    {
        ShowAvatarsTab();
    }

    private void btnTabSwitches_Click(object sender, EventArgs e)
    {
        ShowSwitchesTab();
    }

    private void ShowAvatarsTab()
    {
        pnlAvatars.Visible = true;
        pnlSwitches.Visible = false;

        btnTabAvatars.Font = new Font(btnTabAvatars.Font, FontStyle.Bold);
        btnTabSwitches.Font = new Font(btnTabSwitches.Font, FontStyle.Regular);
    }

    private void ShowSwitchesTab()
    {
        pnlAvatars.Visible = false;
        pnlSwitches.Visible = true;

        btnTabSwitches.Font = new Font(btnTabSwitches.Font, FontStyle.Bold);
        btnTabAvatars.Font = new Font(btnTabAvatars.Font, FontStyle.Regular);

        if (lstSwitches.Items.Count == 0 && Game.ReferenceManager?.ItemData != null)
        {
            RefreshSwitchesList();
        }
    }

    #endregion

    #region Avatar Tab Logic

    private void UpdateUI()
    {
        var mgr = AvatarTesterManager.Instance;

        lblSelectedHat.Text = mgr.SelectedHatId != 0
            ? GetItemLabelText(mgr.SelectedHatId)
            : "(لا يوجد)";

        lblSelectedDress.Text = mgr.SelectedDressId != 0
            ? GetItemLabelText(mgr.SelectedDressId)
            : "(لا يوجد)";

        lblSelectedAccessory.Text = mgr.SelectedAccessoryId != 0
            ? GetItemLabelText(mgr.SelectedAccessoryId)
            : "(لا يوجد)";

        lblSelectedFlag.Text = mgr.SelectedFlagId != 0
            ? GetItemLabelText(mgr.SelectedFlagId)
            : "(لا يوجد)";

        lblPreviewStatus.Text = mgr.HasAnyApplied
            ? "الحالة: مطبق على الشخصية في اللعبة"
            : "الحالة: المظهر الأصلي للشخصية";
    }

    private string GetItemLabelText(uint itemId)
    {
        if (Game.ReferenceManager?.ItemData != null && Game.ReferenceManager.ItemData.TryGetValue(itemId, out var item))
            return $"{AvatarTesterManager.GetItemDisplayName(item)} ({item.ID})";

        return $"ID: {itemId}";
    }

    private void ShowEmptySearchPrompt()
    {
        lstSearchResults.BeginUpdate();
        lstSearchResults.Items.Clear();

        var placeholder = new ListViewItem("يرجى إدخال اسم أو كود الزي في خانة البحث (حرفين على الأقل)...");
        placeholder.ForeColor = Color.DarkGray;
        placeholder.SubItems.Add("");
        placeholder.SubItems.Add("");
        placeholder.SubItems.Add("");
        lstSearchResults.Items.Add(placeholder);

        lstSearchResults.EndUpdate();
    }

    private void TriggerSearch()
    {
        var query = txtSearch.Text.Trim();
        if (query.Length < 2)
        {
            ShowEmptySearchPrompt();
            return;
        }

        SearchAvatars(query);
    }

    private void btnSearch_Click(object sender, EventArgs e)
    {
        _searchDebounceTimer?.Stop();
        TriggerSearch();
    }

    private void SearchAvatars(string filter)
    {
        if (string.IsNullOrWhiteSpace(filter) || filter.Length < 2)
        {
            ShowEmptySearchPrompt();
            return;
        }

        var allItems = AvatarTesterManager.Instance.SearchAvatarItems(filter);
        const int maxResults = 100;
        var items = allItems.Count > maxResults ? allItems.GetRange(0, maxResults) : allItems;

        lstSearchResults.BeginUpdate();
        lstSearchResults.Items.Clear();

        if (items.Count == 0)
        {
            var notFound = new ListViewItem("لم يتم العثور على أزياء مطابقة للبحث.");
            notFound.ForeColor = Color.OrangeRed;
            notFound.SubItems.Add("");
            notFound.SubItems.Add("");
            notFound.SubItems.Add("");
            lstSearchResults.Items.Add(notFound);
            lstSearchResults.EndUpdate();
            return;
        }

        foreach (var item in items)
        {
            var displayName = AvatarTesterManager.GetItemDisplayName(item);
            var lvi = new ListViewItem(displayName);
            var slotType = AvatarTesterManager.GetSlotTypeString(item);

            lvi.SubItems.Add(slotType);
            lvi.SubItems.Add(item.ID.ToString());
            lvi.SubItems.Add(item.CodeName ?? "");
            lvi.Tag = item;

            lvi.LoadItemImageAsync(item);
            lstSearchResults.Items.Add(lvi);
        }

        if (allItems.Count > maxResults)
        {
            var footer = new ListViewItem($"... تم عرض أول {maxResults} نتيجة من أصل {allItems.Count}. يرجى تخصيص البحث أكثر.");
            footer.ForeColor = Color.FromArgb(100, 180, 255);
            footer.SubItems.Add("");
            footer.SubItems.Add("");
            footer.SubItems.Add("");
            lstSearchResults.Items.Add(footer);
        }

        lstSearchResults.EndUpdate();
    }

    private RefObjItem GetSelectedAvatarItem()
    {
        if (lstSearchResults.SelectedItems.Count == 0)
            return null;

        return lstSearchResults.SelectedItems[0].Tag as RefObjItem;
    }

    private void btnSetHat_Click(object sender, EventArgs e)
    {
        var item = GetSelectedAvatarItem();
        if (item != null)
        {
            AvatarTesterManager.Instance.SelectedHatId = item.ID;
            AvatarTesterManager.Instance.EquipAvatarOnPlayer(item.ID, 0);
            UpdateUI();
        }
    }

    private void btnSetDress_Click(object sender, EventArgs e)
    {
        var item = GetSelectedAvatarItem();
        if (item != null)
        {
            AvatarTesterManager.Instance.SelectedDressId = item.ID;
            AvatarTesterManager.Instance.EquipAvatarOnPlayer(item.ID, 1);
            UpdateUI();
        }
    }

    private void btnSetAccessory_Click(object sender, EventArgs e)
    {
        var item = GetSelectedAvatarItem();
        if (item != null)
        {
            AvatarTesterManager.Instance.SelectedAccessoryId = item.ID;
            AvatarTesterManager.Instance.EquipAvatarOnPlayer(item.ID, 2);
            UpdateUI();
        }
    }

    private void btnSetFlag_Click(object sender, EventArgs e)
    {
        var item = GetSelectedAvatarItem();
        if (item != null)
        {
            AvatarTesterManager.Instance.SelectedFlagId = item.ID;
            AvatarTesterManager.Instance.EquipAvatarOnPlayer(item.ID, 3);
            UpdateUI();
        }
    }

    private void lstSearchResults_DoubleClick(object sender, EventArgs e)
    {
        var item = GetSelectedAvatarItem();
        if (item == null) return;

        var slot = AvatarTesterManager.GetSlotIndex(item);
        switch (slot)
        {
            case 0:
                AvatarTesterManager.Instance.SelectedHatId = item.ID;
                AvatarTesterManager.Instance.EquipAvatarOnPlayer(item.ID, 0);
                break;
            case 1:
                AvatarTesterManager.Instance.SelectedDressId = item.ID;
                AvatarTesterManager.Instance.EquipAvatarOnPlayer(item.ID, 1);
                break;
            case 2:
                AvatarTesterManager.Instance.SelectedAccessoryId = item.ID;
                AvatarTesterManager.Instance.EquipAvatarOnPlayer(item.ID, 2);
                break;
            case 3:
                AvatarTesterManager.Instance.SelectedFlagId = item.ID;
                AvatarTesterManager.Instance.EquipAvatarOnPlayer(item.ID, 3);
                break;
        }
        UpdateUI();
    }

    private void btnSpawnPreview_Click(object sender, EventArgs e)
    {
        AvatarTesterManager.Instance.ApplyAll();
    }

    private void btnClearPreview_Click(object sender, EventArgs e)
    {
        AvatarTesterManager.Instance.ClearAll();
        AvatarTesterManager.Instance.DespawnPreviewClone();
    }

    #endregion

    #region Switches Tab Logic

    private void btnFilterAll_Click(object sender, EventArgs e)
    {
        _currentSwitchCategory = SwitchCategory.All;
        HighlightCategoryButton(btnFilterAll);
        RefreshSwitchesList();
    }

    private void btnFilterWepGlow_Click(object sender, EventArgs e)
    {
        _currentSwitchCategory = SwitchCategory.WeaponGlow;
        HighlightCategoryButton(btnFilterWepGlow);
        RefreshSwitchesList();
    }

    private void btnFilterShGlow_Click(object sender, EventArgs e)
    {
        _currentSwitchCategory = SwitchCategory.ShieldGlow;
        HighlightCategoryButton(btnFilterShGlow);
        RefreshSwitchesList();
    }

    private void btnFilterD1D16_Click(object sender, EventArgs e)
    {
        _currentSwitchCategory = SwitchCategory.ModelD1D16;
        HighlightCategoryButton(btnFilterD1D16);
        RefreshSwitchesList();
    }

    private void btnFilterSpecial_Click(object sender, EventArgs e)
    {
        _currentSwitchCategory = SwitchCategory.SpecialSwitcher;
        HighlightCategoryButton(btnFilterSpecial);
        RefreshSwitchesList();
    }

    private void HighlightCategoryButton(SDUI.Controls.Button activeBtn)
    {
        btnFilterAll.Font = new Font(btnFilterAll.Font, btnFilterAll == activeBtn ? FontStyle.Bold : FontStyle.Regular);
        btnFilterWepGlow.Font = new Font(btnFilterWepGlow.Font, btnFilterWepGlow == activeBtn ? FontStyle.Bold : FontStyle.Regular);
        btnFilterShGlow.Font = new Font(btnFilterShGlow.Font, btnFilterShGlow == activeBtn ? FontStyle.Bold : FontStyle.Regular);
        btnFilterD1D16.Font = new Font(btnFilterD1D16.Font, btnFilterD1D16 == activeBtn ? FontStyle.Bold : FontStyle.Regular);
        btnFilterSpecial.Font = new Font(btnFilterSpecial.Font, btnFilterSpecial == activeBtn ? FontStyle.Bold : FontStyle.Regular);
    }

    private void btnSwitchSearch_Click(object sender, EventArgs e)
    {
        RefreshSwitchesList();
    }

    private void txtSwitchSearch_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            RefreshSwitchesList();
        }
    }

    private void RefreshSwitchesList()
    {
        var filter = txtSwitchSearch.Text.Trim();
        var items = AvatarTesterManager.Instance.SearchSwitchItems(filter, _currentSwitchCategory);

        lstSwitches.BeginUpdate();
        lstSwitches.Items.Clear();

        foreach (var item in items)
        {
            var displayName = AvatarTesterManager.GetItemDisplayName(item);
            var lvi = new ListViewItem(displayName);
            var categoryStr = AvatarTesterManager.GetSwitchCategoryString(item);

            lvi.SubItems.Add(categoryStr);
            lvi.SubItems.Add(item.ID.ToString());
            lvi.SubItems.Add(item.CodeName ?? "");
            lvi.Tag = item;

            lvi.LoadItemImageAsync(item);
            lstSwitches.Items.Add(lvi);
        }

        lstSwitches.EndUpdate();
        lblSwitchCount.Text = $"العدد: {items.Count} أداة";
    }

    private RefObjItem GetSelectedSwitchItem()
    {
        if (lstSwitches.SelectedItems.Count == 0)
            return null;

        return lstSwitches.SelectedItems[0].Tag as RefObjItem;
    }

    private byte GetSelectedOptLevel()
    {
        if (cmbOptLevel.SelectedItem == null)
            return 12;

        var text = cmbOptLevel.SelectedItem.ToString().Replace("+", "").Trim();
        return byte.TryParse(text, out var val) ? val : (byte)12;
    }

    private void btnTestWeapon_Click(object sender, EventArgs e)
    {
        var item = GetSelectedSwitchItem();
        if (item == null)
        {
            lblSwitchStatus.Text = "يرجى تحديد أداة سوتش من القائمة أولاً.";
            return;
        }

        if (Game.Player == null || Game.Player.UniqueId == 0)
        {
            lblSwitchStatus.Text = "يجب أن تكون الشخصية داخل اللعبة لمعاينة السوتش!";
            return;
        }

        var curWep = Game.Player.Inventory?.GetItemAt(6);
        if (curWep == null || curWep.Record == null || !curWep.Record.IsWeapon)
        {
            lblSwitchStatus.Text = "يرجى ارتداء سلاح في اللعبة أولاً لمعاينة السوتش عليه!";
            return;
        }

        byte opt = GetSelectedOptLevel();
        var cat = AvatarTesterManager.GetSwitchCategory(item);

        if (cat == SwitchCategory.WeaponGlow)
        {
            AvatarTesterManager.Instance.EquipWeaponOnPlayer(curWep.ItemId, opt);
            lblSwitchStatus.Text = $"✨ تم تطبيق توهج السلاح على شخصيتك: {AvatarTesterManager.GetItemDisplayName(item)} (+{opt})";
        }
        else
        {
            var targetWeapon = AvatarTesterManager.FindMatchingWeapon(curWep.Record, item);
            var wepToPreview = targetWeapon ?? curWep.Record;

            AvatarTesterManager.Instance.EquipWeaponOnPlayer(wepToPreview.ID, opt);

            var wepName = AvatarTesterManager.GetItemDisplayName(wepToPreview);
            lblSwitchStatus.Text = $"⚔️ تم تطبيق مظهر السلاح على شخصيتك: {wepName} (+{opt})";
        }
    }

    private void btnTestShield_Click(object sender, EventArgs e)
    {
        var item = GetSelectedSwitchItem();
        if (item == null)
        {
            lblSwitchStatus.Text = "يرجى تحديد أداة سوتش من القائمة أولاً.";
            return;
        }

        if (Game.Player == null || Game.Player.UniqueId == 0)
        {
            lblSwitchStatus.Text = "يجب أن تكون الشخصية داخل اللعبة لمعاينة السوتش!";
            return;
        }

        var curShield = Game.Player.Inventory?.GetItemAt(7);
        if (curShield == null || curShield.Record == null || !curShield.Record.IsShield)
        {
            lblSwitchStatus.Text = "يرجى ارتداء درع (Shield) في اللعبة أولاً لمعاينة السوتش عليه!";
            return;
        }

        byte opt = GetSelectedOptLevel();
        var cat = AvatarTesterManager.GetSwitchCategory(item);

        if (cat == SwitchCategory.ShieldGlow)
        {
            AvatarTesterManager.Instance.EquipShieldOnPlayer(curShield.ItemId, opt);
            lblSwitchStatus.Text = $"🛡️ تم تطبيق توهج الدرع على شخصيتك: {AvatarTesterManager.GetItemDisplayName(item)} (+{opt})";
        }
        else
        {
            var targetShield = AvatarTesterManager.FindMatchingShield(curShield.Record, item);
            var shieldToPreview = targetShield ?? curShield.Record;

            AvatarTesterManager.Instance.EquipShieldOnPlayer(shieldToPreview.ID, opt);

            var shieldName = AvatarTesterManager.GetItemDisplayName(shieldToPreview);
            lblSwitchStatus.Text = $"🛡️ تم تطبيق مظهر الدرع على شخصيتك: {shieldName} (+{opt})";
        }
    }

    private void cmbOptLevel_SelectedIndexChanged(object sender, EventArgs e)
    {
        byte opt = GetSelectedOptLevel();

        if (AvatarTesterManager.Instance.AppliedWeaponId != 0)
        {
            AvatarTesterManager.Instance.EquipWeaponOnPlayer(AvatarTesterManager.Instance.AppliedWeaponId, opt);
            lblSwitchStatus.Text = $"تم تحديث توهج السلاح على شخصيتك إلى (+{opt}).";
        }
        else if (AvatarTesterManager.Instance.AppliedShieldId != 0)
        {
            AvatarTesterManager.Instance.EquipShieldOnPlayer(AvatarTesterManager.Instance.AppliedShieldId, opt);
            lblSwitchStatus.Text = $"تم تحديث توهج الدرع على شخصيتك إلى (+{opt}).";
        }
    }

    private void btnUseFromInv_Click(object sender, EventArgs e)
    {
        var item = GetSelectedSwitchItem();
        if (item == null)
        {
            lblSwitchStatus.Text = "يرجى تحديد أداة سوتش من القائمة أولاً.";
            return;
        }

        bool used = AvatarTesterManager.Instance.UseSwitchItemFromInventory(item.ID);
        if (used)
        {
            lblSwitchStatus.Text = $"تم استخدام {AvatarTesterManager.GetItemDisplayName(item)} من الحقيبة!";
        }
        else
        {
            lblSwitchStatus.Text = "هذه الأداة غير متوفرة في حقيبتك حالياً.";
        }
    }

    private void btnRestoreWep_Click(object sender, EventArgs e)
    {
        AvatarTesterManager.Instance.RestoreWeaponAndShield();
        lblSwitchStatus.Text = "تمت استعادة مظهر السلاح والدرع الأصلي للشخصية.";
    }

    private void lstSwitches_DoubleClick(object sender, EventArgs e)
    {
        var item = GetSelectedSwitchItem();
        if (item == null) return;

        var cat = AvatarTesterManager.GetSwitchCategory(item);
        if (cat == SwitchCategory.ShieldGlow)
        {
            btnTestShield_Click(sender, e);
        }
        else
        {
            btnTestWeapon_Click(sender, e);
        }
    }

    #endregion
}
