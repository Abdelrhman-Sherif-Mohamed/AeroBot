using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Extensions;
using RSBot.Core.Objects;
using SDUI;
using SDUI.Controls;
using CheckBox = SDUI.Controls.CheckBox;
using GroupBox = SDUI.Controls.GroupBox;
using ListViewExtensions = RSBot.Core.Extensions.ListViewExtensions;

namespace RSBot.Items.Views;

[ToolboxItem(false)]
public partial class Main : DoubleBufferedControl
{
    private List<RefShopGroup> _accessoryTrader;
    private List<RefShopGroup> _potionTrader;
    private List<RefShopGroup> _protectorTrader;
    private List<RefShopGroup> _stableKeeper;
    private List<RefShopGroup> _weaponTrader;
    private bool _loadingSettings;
    private bool _applyingItemsLayout;
    private SDUI.Controls.Label _shoppingListTitle;
    private SDUI.Controls.Label _shoppingListHint;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Main" /> class.
    /// </summary>
    public Main()
    {
        CheckForIllegalCrossThreadCalls = false;

        InitializeComponent();
        SubscribeEvents();

        listShoppingList.SmallImageList = ListViewExtensions.StaticItemsImageList;
        listAvailableProducts.SmallImageList = ListViewExtensions.StaticItemsImageList;
        listFilter.SmallImageList = ListViewExtensions.StaticItemsImageList;

        CreateShoppingListLabels();
        tabMain.SelectedIndexChanged += TabMain_SelectedIndexChanged;

        ApplyItemsLocalization();
        ApplyItemsLayout();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        ApplyItemsLayout();
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);

        if (!Visible)
            return;

        ApplyItemsLocalization();
        ApplyItemsLayout();
    }

    private int S(float value)
    {
        return (int)Math.Round(value * DeviceDpi / 96f);
    }

    private static string T(string key, string fallback)
    {
        return LanguageManager.GetLangBySpecificKey("RSBot.Items", key, fallback);
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

    private void PlaceLabel(SDUI.Controls.Label label, int x, int y, int width, int height,
        ContentAlignment alignment)
    {
        if (label == null)
            return;

        Place(label, x, y, width, height);
        label.TextAlign = alignment;
    }

    /// <summary>
    ///     Builds the two labels that fill the previously empty panel above the
    ///     shopping list. They are created in code so the designer hierarchy and
    ///     the existing control names stay untouched.
    /// </summary>
    private void CreateShoppingListLabels()
    {
        _shoppingListTitle = new SDUI.Controls.Label
        {
            Name = "shoppingListTitle",
            AutoSize = false,
            ApplyGradient = false,
            BackColor = Color.Transparent,
            ForeColor = ColorScheme.ForeColor,
            Font = new Font("Segoe UI", 11.5F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };

        _shoppingListHint = new SDUI.Controls.Label
        {
            Name = "shoppingListHint",
            AutoSize = false,
            ApplyGradient = false,
            BackColor = Color.Transparent,
            ForeColor = ColorScheme.SecondaryForeColor,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
            TextAlign = ContentAlignment.MiddleLeft
        };

        panel2.Controls.Add(_shoppingListTitle);
        panel2.Controls.Add(_shoppingListHint);
    }

    private void TabMain_SelectedIndexChanged(object sender, EventArgs e)
    {
        ApplyItemsLayout();
    }

    /// <summary>
    ///     Lays the page out for the actual tab surface. The designer used fixed
    ///     934x559 coordinates, while the running application shows the page at
    ///     roughly 1116x577. Re-flowing the controls keeps every Arabic caption
    ///     readable and removes the vertical scrollbar the old filter panel
    ///     always forced.
    /// </summary>
    private void ApplyItemsLayout()
    {
        if (_applyingItemsLayout || IsDisposed || tabMain == null || tabMain.TabPages.Count < 3)
            return;

        var area = tabMain.DisplayRectangle;
        if (area.Width < 200 || area.Height < 200)
            return;

        _applyingItemsLayout = true;
        try
        {
            var scale = DeviceDpi / 96f;
            var pageW = (int)(area.Width / scale);
            var pageH = (int)(area.Height / scale);
            var layoutW = Math.Max(pageW, 1000);
            var layoutH = Math.Max(pageH, 560);

            LayoutShopping(layoutW, layoutH);
            LayoutFilter(layoutW, layoutH);
            LayoutPickup(layoutW, layoutH);

            var needsScroll = layoutW > pageW || layoutH > pageH;
            foreach (TabPage page in tabMain.TabPages)
            {
                page.Padding = new Padding(0);
                page.BackColor = ColorScheme.BackColor;
                page.AutoScroll = true;
                page.AutoScrollMinSize = needsScroll
                    ? new Size(S(layoutW), S(layoutH))
                    : Size.Empty;
            }
        }
        finally
        {
            _applyingItemsLayout = false;
        }
    }

    private void LayoutShopping(int width, int height)
    {
        const int margin = 14;
        const int gap = 12;
        const int panelH = 70;

        var contentW = width - margin * 2;
        var oneRow = contentW >= 1040;
        var bottomH = oneRow ? 84 : 116;
        var bottomY = height - margin - bottomH;
        var bodyH = bottomY - gap - margin;

        separator5.Visible = false;

        // Top: available products / shopping list in two resizable columns.
        Place(splitContainer, margin, margin, contentW, bodyH);
        splitContainer.SplitterWidth = S(1);

        var leftW = Math.Max(360, (int)(contentW * 0.42f));
        splitContainer.SplitterDistance = S(leftW);
        var rightW = contentW - leftW - 1;

        Place(panel1, 0, 0, leftW, panelH);
        Place(listAvailableProducts, 0, panelH, leftW, bodyH - panelH);
        Place(panel2, 0, 0, rightW, panelH);
        Place(listShoppingList, 0, panelH, rightW, bodyH - panelH);

        PlaceLabel(label1, 12, 6, 52, 24, ContentAlignment.MiddleLeft);
        Place(comboStore, 66, 5, Math.Max(150, leftW - 66 - 166), 25);
        Place(checkShowEquipment, leftW - 162, 2, 150, 28);
        PlaceLabel(label6, 12, 38, 52, 24, ContentAlignment.MiddleLeft);
        Place(txtShopSearch, 66, 37, Math.Max(120, leftW - 80), 25);

        Place(_shoppingListTitle, 14, 2, rightW - 28, 30);
        Place(_shoppingListHint, 14, 33, rightW - 28, 30);

        colAvailableName.Width = Math.Max(120, leftW - 24);
        colName.Width = Math.Max(140, rightW - 112);
        colAmount.Width = 92;

        // Bottom: general setup card, four options in a single row when there
        // is enough width, otherwise a readable 2x2 grid.
        Place(groupBox1, margin, bottomY, contentW, bottomH);

        var cols = oneRow ? 4 : 2;
        var cbGap = 16;
        var cbW = Math.Max(120, (contentW - 32 - (cols - 1) * cbGap) / cols);
        var checkBoxes = new[] { checkEnable, checkRepairGear, checkSellItemsFromPet, checkStoreItemsFromPet };

        for (var i = 0; i < checkBoxes.Length; i++)
        {
            var row = i / cols;
            var col = i % cols;
            Place(checkBoxes[i], 16 + col * (cbW + cbGap), 34 + row * 32, cbW, 28);
        }
    }

    private void LayoutFilter(int width, int height)
    {
        const int margin = 14;
        const int gap = 12;
        const int footerH = 46;

        var contentW = width - margin * 2;
        var contentH = height - margin * 2 - gap - footerH;
        var filterW = Math.Max(380, Math.Min(430, (int)(contentW * 0.38f)));
        var listX = margin + filterW + gap;
        var listW = contentW - filterW - gap;

        // The designer had a permanently scrolling 720 px filter column. It is
        // compacted below so it fits the real page height without a scrollbar.
        Place(filterPanel, margin, margin, filterW, contentH);
        filterPanel.AutoScroll = false;
        filterPanel.AutoScrollMinSize = Size.Empty;
        filterPanel.Padding = new Padding(8, 8, 8, 8);

        Place(listFilter, listX, margin, listW, contentH);
        Place(panel3, margin, height - margin - footerH, contentW, footerH);

        pictureBox1.Visible = false;
        panel7.Visible = false;

        Place(btnResetFilter, 12, 8, 110, 30);
        Place(btnReload, 130, 8, 150, 30);

        var searchButtonW = 96;
        var searchButtonX = contentW - 12 - searchButtonW;
        var searchW = Math.Max(180, Math.Min(320, (int)(contentW * 0.28f)));
        var searchX = searchButtonX - 8 - searchW;

        Place(btnSearch, searchButtonX, 8, searchButtonW, 30);
        Place(txtSellSearch, searchX, 9, searchW, 26);
        PlaceLabel(labelResult, Math.Max(300, searchX - 78), 8, 66, 30, ContentAlignment.MiddleRight);

        var innerW = filterW - 16;
        var colW = Math.Max(80, (innerW - 28) / 3);

        void PlaceGrid(GroupBox box, int y, int boxH, CheckBox[] boxes, int columns)
        {
            Place(box, 8, y, innerW, boxH);
            for (var i = 0; i < boxes.Length; i++)
            {
                var row = i / columns;
                var col = i % columns;
                Place(boxes[i], 8 + col * (colW + 6), 28 + row * 21, colW, 21);
            }
        }

        var y = 8;

        // Gender and degree, with the numeric fields on their own compact row.
        Place(groupGender, 8, y, innerW, 98);
        Place(checkMale, 8, 28, colW, 21);
        Place(checkFemale, 8 + colW + 6, 28, colW, 21);
        Place(checkBoxRareItems, 8 + (colW + 6) * 2, 28, colW, 21);
        Place(checkChinese, 8, 49, colW, 21);
        Place(checkEuropean, 8 + colW + 6, 49, colW, 21);
        PlaceLabel(label3, 8, 71, 44, 22, ContentAlignment.MiddleLeft);
        Place(numDegreeFrom, 54, 69, 62, 26);
        PlaceLabel(label4, 120, 71, 14, 22, ContentAlignment.MiddleCenter);
        Place(numDegreeTo, 138, 69, 62, 26);
        y += 102;

        PlaceGrid(groupClothes, y, 93, new[]
        {
            checkClothes, checkLight, checkHeavy,
            checkHead, checkShoulder, checkChest,
            checkLegs, checkHand, checkBoot
        }, 3);
        y += 97;

        PlaceGrid(groupAccessories, y, 51, new[]
        {
            checkRing, checkNecklace, checkEarring
        }, 3);
        y += 55;

        PlaceGrid(groupWeapons, y, 135, new[]
        {
            checkBlade, checkSword, checkSpear,
            checkGlave, checkBow, checkXBow,
            check1HSword, check2HSword, checkAxe,
            checkWRod, checkCRod, checkStaff,
            checkDagger, checkHarp, checkShield
        }, 3);
        y += 139;

        PlaceGrid(groupOthers, y, 72, new[]
        {
            checkQuest, checkCoin, checkAmmo,
            checkAlchemy, checkOther
        }, 3);

        // The old separators are replaced by the spacing between the cards.
        separator1.Visible = false;
        separator2.Visible = false;
        separator3.Visible = false;
        separator4.Visible = false;

        colItemName.Width = Math.Max(140, listW - 348 - 24);
        colItemLevel.Width = 72;
        colGender.Width = 66;
        colPickup.Width = 74;
        colSell.Width = 68;
        collStore.Width = 68;

        numDegreeFrom.BackColor = Color.Transparent;
        numDegreeFrom.ForeColor = ColorScheme.ForeColor;
        numDegreeTo.BackColor = Color.Transparent;
        numDegreeTo.ForeColor = ColorScheme.ForeColor;

        listFilter.BackColor = ColorScheme.TableBackColor;
        listFilter.ForeColor = ColorScheme.ForeColor;
        listAvailableProducts.BackColor = ColorScheme.TableBackColor;
        listAvailableProducts.ForeColor = ColorScheme.ForeColor;
        listShoppingList.BackColor = ColorScheme.TableBackColor;
        listShoppingList.ForeColor = ColorScheme.ForeColor;
    }

    private void LayoutPickup(int width, int height)
    {
        const int margin = 14;
        const int gap = 12;

        var contentW = width - margin * 2;

        Place(groupBoxGeneral, margin, margin, contentW, 150);
        var generalColW = Math.Max(240, (contentW - 48) / 2);
        Place(checkEnableAbilityPet, 16, 36, generalColW, 30);
        Place(cbJustpickmyitems, 16 + generalColW + 16, 36, generalColW, 30);
        Place(checkDontPickupInBerzerk, 16, 78, generalColW, 30);
        Place(cbDontPickupWhileBotting, 16 + generalColW + 16, 78, generalColW, 30);

        var optionsY = margin + 150 + gap;
        Place(groupBoxOptions, margin, optionsY, contentW, 150);
        var optionColW = Math.Max(200, (contentW - 64) / 3);
        Place(checkPickupGold, 16, 36, optionColW, 30);
        Place(checkPickupBlue, 16 + optionColW + 16, 36, optionColW, 30);
        Place(checkPickupRare, 16 + (optionColW + 16) * 2, 36, optionColW, 30);
        Place(checkQuestItems, 16, 78, optionColW, 30);
        Place(checkAllEquips, 16 + optionColW + 16, 78, optionColW, 30);
        Place(checkEverything, 16 + (optionColW + 16) * 2, 78, optionColW, 30);
    }

    /// <summary>
    ///     Translates the common trader tab captions that come from the game
    ///     reference data. Unknown captions are kept exactly as the client
    ///     provides them.
    /// </summary>
    private static string TranslateShopGroup(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return name;

        switch (name.Trim().ToLowerInvariant())
        {
            case "potion":
                return T("RSBot.Items.ShopTab.Potion", name);
            case "weapon":
                return T("RSBot.Items.ShopTab.Weapon", name);
            case "shield":
                return T("RSBot.Items.ShopTab.Shield", name);
            case "armor":
                return T("RSBot.Items.ShopTab.Armor", name);
            case "protector":
                return T("RSBot.Items.ShopTab.Armor", name);
            case "accessory":
                return T("RSBot.Items.ShopTab.Accessory", name);
            case "ammo":
                return T("RSBot.Items.ShopTab.Ammo", name);
            case "quest":
                return T("RSBot.Items.ShopTab.Quest", name);
            case "special":
                return T("RSBot.Items.ShopTab.Special", name);
            case "other":
                return T("RSBot.Items.ShopTab.Other", name);
            case "avatar":
                return T("RSBot.Items.ShopTab.Avatar", name);
            case "ability pet":
                return T("RSBot.Items.ShopTab.AbilityPet", name);
            case "alchemy":
                return T("RSBot.Items.ShopTab.Alchemy", name);
            default:
                return name;
        }
    }

    /// <summary>
    ///     Localizes strings that the generic language walker cannot reach:
    ///     list column headers, trader names, list group captions, context
    ///     menus and the runtime labels above the shopping list.
    /// </summary>
    private void ApplyItemsLocalization()
    {
        if (IsDisposed || listFilter == null)
            return;

        colAvailableName.Text = T("RSBot.Items.Columns.Name", "Name");
        colName.Text = T("RSBot.Items.Columns.Name", "Name");
        colAmount.Text = T("RSBot.Items.Columns.Amount", "Amount");
        colItemName.Text = T("RSBot.Items.Columns.Name", "Name");
        colItemLevel.Text = T("RSBot.Items.Columns.Level", "Level");
        colGender.Text = T("RSBot.Items.Columns.Gender", "Gender");
        colPickup.Text = T("RSBot.Items.Columns.Pickup", "Pickup");
        colSell.Text = T("RSBot.Items.Columns.Sell", "Sell");
        collStore.Text = T("RSBot.Items.Columns.Store", "Store");

        menuChangeAmount.Text = T("RSBot.Items.ContextMenu.ChangeAmount", "Change amount");
        menuRemoveItem.Text = T("RSBot.Items.ContextMenu.Remove", "Remove");
        menuAddToShoppingList.Text = T("RSBot.Items.ContextMenu.AddToShoppingList", "Add to shopping list");

        btnAddToSell.Text = T("RSBot.Items.ContextMenu.AddToSell", "Sell");
        btnAddToStore.Text = T("RSBot.Items.ContextMenu.AddToStore", "Store");
        btnPickup.Text = T("RSBot.Items.ContextMenu.Pickup", "Pickup");
        btnPickOnlyCharacter.Text = T("RSBot.Items.ContextMenu.PickupOnlyCharacter", "Pickup only character");
        btnDontSell.Text = T("RSBot.Items.ContextMenu.DontSell", "Don't sell");
        btnDontStore.Text = T("RSBot.Items.ContextMenu.DontStore", "Don't store");
        btnDontPickup.Text = T("RSBot.Items.ContextMenu.DontPickup", "Don't pickup");

        var selectedTrader = comboStore.SelectedIndex;
        comboStore.BeginUpdate();
        comboStore.Items.Clear();
        comboStore.Items.AddRange(new object[]
        {
            T("RSBot.Items.Shopping.PotionTrader", "Potion trader"),
            T("RSBot.Items.Shopping.StableKeeper", "Stable keeper"),
            T("RSBot.Items.Shopping.ProtectorTrader", "Protector trader"),
            T("RSBot.Items.Shopping.WeaponTrader", "Weapon trader"),
            T("RSBot.Items.Shopping.AccessoryTrader", "Accessory trader")
        });
        comboStore.EndUpdate();

        if (selectedTrader >= 0 && selectedTrader < comboStore.Items.Count)
            comboStore.SelectedIndex = selectedTrader;
        else if (_potionTrader != null && comboStore.Items.Count > 0)
            comboStore.SelectedIndex = 0;

        foreach (ListViewGroup group in listShoppingList.Groups)
        {
            switch (group.Name)
            {
                case "groupPotion":
                    group.Header = T("RSBot.Items.Shopping.GroupPotion", "Potion trader");
                    break;
                case "groupStable":
                    group.Header = T("RSBot.Items.Shopping.GroupStable", "Stable keeper");
                    break;
                case "groupProtector":
                    group.Header = T("RSBot.Items.Shopping.GroupProtector", "Protector trader");
                    break;
                case "groupWeapon":
                    group.Header = T("RSBot.Items.Shopping.GroupWeapon", "Weapon trader");
                    break;
                case "groupAccessory":
                    group.Header = T("RSBot.Items.Shopping.GroupAccessory", "Accessory trader");
                    break;
            }
        }

        if (_shoppingListTitle != null)
            _shoppingListTitle.Text = T("RSBot.Items.Shopping.ListTitle", "Shopping list");

        if (_shoppingListHint != null)
            _shoppingListHint.Text = T("RSBot.Items.Shopping.ListHint",
                "Right-click an item to change the amount or remove it");
    }

    /// <summary>
    ///     Subscribes the events.
    /// </summary>
    private void SubscribeEvents()
    {
        EventManager.SubscribeEvent("OnLoadGameData", OnLoadGameData);
        EventManager.SubscribeEvent("OnEnterGame", LoadSettings);
    }

    /// <summary>
    ///     Loads the npcs
    /// </summary>
    private void LoadGroups()
    {
        _accessoryTrader = new List<RefShopGroup>();
        _potionTrader = new List<RefShopGroup>();
        _protectorTrader = new List<RefShopGroup>();
        _weaponTrader = new List<RefShopGroup>();
        _stableKeeper = new List<RefShopGroup>();

        foreach (var group in Game.ReferenceManager.ShopGroups)
        {
            if (group.Value.CodeName.Contains("SMITH") && !group.Value.CodeName.Contains("MALL_"))
                _weaponTrader.Add(group.Value);

            if (group.Value.CodeName.Contains("POTION") && !group.Value.CodeName.Contains("MALL_"))
                _potionTrader.Add(group.Value);

            if (group.Value.CodeName.Contains("ARMOR") && !group.Value.CodeName.Contains("MALL_"))
                _protectorTrader.Add(group.Value);

            if (group.Value.CodeName.Contains("ACCESSORY") && !group.Value.CodeName.Contains("MALL_"))
                _accessoryTrader.Add(group.Value);

            if (group.Value.CodeName.Contains("STABLE") && !group.Value.CodeName.Contains("MALL_"))
                _stableKeeper.Add(group.Value);
        }
    }

    /// <summary>
    ///     Populates the product list.
    /// </summary>
    private void PopulateProductList(int index, string filter = "")
    {
        if (index < 0)
            return;

        listAvailableProducts.BeginUpdate();
        listAvailableProducts.Items.Clear();

        List<RefShopGroup> groups;
        switch (index)
        {
            case 0:
                groups = _potionTrader;
                break;

            case 1:
                groups = _stableKeeper;
                break;

            case 2:
                groups = _protectorTrader;
                break;

            case 3:
                groups = _weaponTrader;
                break;

            case 4:
                groups = _accessoryTrader;
                break;

            default:
                return;
        }

        foreach (var group in groups)
        {
            var goods = Game.ReferenceManager.GetRefShopGoods(group);

            if (goods == null) continue;

            foreach (var good in goods)
            {
                var tabName =
                    Game.ReferenceManager.GetTranslation(Game.ReferenceManager.GetTab(good.RefTabCodeName)
                        .StrID128_Tab);

                var lvGroup = new ListViewGroup(TranslateShopGroup(tabName)) { Name = tabName };
                if (!listAvailableProducts.Groups.Contains(lvGroup))
                    listAvailableProducts.Groups.Add(lvGroup);

                var refPackageItem = Game.ReferenceManager.GetRefPackageItem(good.RefPackageItemCodeName);
                var item = Game.ReferenceManager.GetRefItem(refPackageItem.RefItemCodeName);

                if (item == null)
                    continue;

                if (!checkShowEquipment.Checked && item.TypeID2 == 1)
                    continue;

                var realItemName = item.GetRealName();

                //Apply filters
                if (refPackageItem.RefItemCodeName.Contains("_MALL_")) continue;
                if (filter != "" && !realItemName.Contains(filter)) continue;

                var listItem = new ListViewItem(realItemName)
                { Tag = good, Name = item.CodeName, Group = listAvailableProducts.Groups[tabName] };
                listItem.LoadItemImageAsync(good);

                if (!listAvailableProducts.Items.ContainsKey(item.CodeName))
                    listAvailableProducts.Items.Add(listItem);
            }
        }

        listAvailableProducts.EndUpdate();
    }

    /// <summary>
    ///     Saves the shopping list.
    /// </summary>
    private void SaveShoppingList()
    {
        ShoppingManager.ShoppingList.Clear();
        foreach (ListViewGroup grp in listShoppingList.Groups)
        {
            var values = new List<string>(grp.Items.Count);

            foreach (ListViewItem item in grp.Items)
            {
                if (!(item.Tag is RefShopGood packageItem))
                    continue;

                if (ShoppingManager.ShoppingList.ContainsKey(packageItem))
                    continue;

                if (!int.TryParse(item.SubItems[1].Text.Substring(1), out var amount))
                    continue;

                ShoppingManager.ShoppingList.Add(packageItem, amount);

                values.Add(packageItem.RefPackageItemCodeName + "|" + amount);
            }

            PlayerConfig.SetArray("RSBot.Shopping." + grp.Name, values);
        }
    }

    /// <summary>
    ///     Loads the shopping list.
    /// </summary>
    private void LoadShoppingList()
    {
        listShoppingList.BeginUpdate();
        listShoppingList.Items.Clear();

        foreach (ListViewGroup group in listShoppingList.Groups)
        {
            var values = PlayerConfig.GetArray<string>("RSBot.Shopping." + group.Name);

            foreach (var value in values)
            {
                var packageCodeName = value.Split('|')[0];
                var amount = value.Split('|')[1];
                var good = Game.ReferenceManager.GetRefShopGood(packageCodeName);

                if (good == null)
                    continue;

                var refPackageItem = Game.ReferenceManager.GetRefPackageItem(good.RefPackageItemCodeName);
                var item = Game.ReferenceManager.GetRefItem(refPackageItem.RefItemCodeName);

                var listItem = new ListViewItem(item.GetRealName()) { Name = item.CodeName, Tag = good };
                listItem.SubItems.Add("x" + amount);
                listItem.Group = group;
                listShoppingList.Items.Add(listItem);

                listItem.LoadItemImageAsync(good);

                if (!ShoppingManager.ShoppingList.ContainsKey(good))
                    ShoppingManager.ShoppingList.Add(good, int.Parse(amount));
            }
        }

        listShoppingList.EndUpdate();
    }

    /// <summary>
    ///     Queries the sell items.
    /// </summary>
    private async Task QuerySellItemsAsync()
    {
        await Task.Delay(1).ConfigureAwait(false);

        listFilter.Visible = false;
        listFilter.BeginUpdate();
        listFilter.Items.Clear();
        listFilter.EndUpdate();

        var filters = new List<TypeIdFilter>();

        #region Weapons

        if (checkSword.Checked)
            filters.Add(new TypeIdFilter(3, 1, 6, 2));

        if (checkBlade.Checked)
            filters.Add(new TypeIdFilter(3, 1, 6, 3));

        if (checkSpear.Checked)
            filters.Add(new TypeIdFilter(3, 1, 6, 4));

        if (checkGlave.Checked)
            filters.Add(new TypeIdFilter(3, 1, 6, 5));

        if (checkBow.Checked)
            filters.Add(new TypeIdFilter(3, 1, 6, 6));

        if (check1HSword.Checked)
            filters.Add(new TypeIdFilter(3, 1, 6, 7));

        if (check2HSword.Checked)
            filters.Add(new TypeIdFilter(3, 1, 6, 8));

        if (checkAxe.Checked)
            filters.Add(new TypeIdFilter(3, 1, 6, 9));

        if (checkWRod.Checked)
            filters.Add(new TypeIdFilter(3, 1, 6, 10));

        if (checkStaff.Checked)
            filters.Add(new TypeIdFilter(3, 1, 6, 11));

        if (checkXBow.Checked)
            filters.Add(new TypeIdFilter(3, 1, 6, 12));

        if (checkDagger.Checked)
            filters.Add(new TypeIdFilter(3, 1, 6, 13));

        if (checkHarp.Checked)
            filters.Add(new TypeIdFilter(3, 1, 6, 14));

        if (checkCRod.Checked)
            filters.Add(new TypeIdFilter(3, 1, 6, 15));

        #endregion Weapons

        #region Equipment

        var clothTypes = new byte[3, 2];

        if (checkClothes.Checked)
        {
            if (checkEuropean.Checked)
                clothTypes[0, 0] = 9;

            if (checkChinese.Checked)
                clothTypes[0, 1] = 1;
        }

        if (checkLight.Checked)
        {
            if (checkEuropean.Checked)
                clothTypes[1, 0] = 10;

            if (checkChinese.Checked)
                clothTypes[1, 1] = 2;
        }

        if (checkHeavy.Checked)
        {
            if (checkEuropean.Checked)
                clothTypes[2, 0] = 11;

            if (checkChinese.Checked)
                clothTypes[2, 1] = 3;
        }

        for (var x = 0; x < 3; x++)
            for (var z = 0; z < 2; z++)
            {
                var cloth = clothTypes[x, z];
                if (cloth == 0)
                    continue;

                if (checkHead.Checked)
                    filters.Add(new TypeIdFilter(3, 1, cloth, 1));

                if (checkShoulder.Checked)
                    filters.Add(new TypeIdFilter(3, 1, cloth, 2));

                if (checkChest.Checked)
                    filters.Add(new TypeIdFilter(3, 1, cloth, 3));

                if (checkLegs.Checked)
                    filters.Add(new TypeIdFilter(3, 1, cloth, 4));

                if (checkHand.Checked)
                    filters.Add(new TypeIdFilter(3, 1, cloth, 5));

                if (checkBoot.Checked)
                    filters.Add(new TypeIdFilter(3, 1, cloth, 6));
            }

        #region Accessory

        if (checkRing.Checked && checkEuropean.Checked)
            filters.Add(new TypeIdFilter(3, 1, 12, 3));

        if (checkRing.Checked && checkChinese.Checked)
            filters.Add(new TypeIdFilter(3, 1, 5, 3));

        if (checkRing.Checked && !checkEuropean.Checked && !checkChinese.Checked)
        {
            filters.Add(new TypeIdFilter(3, 1, 5, 3));
            filters.Add(new TypeIdFilter(3, 1, 12, 3));
        }

        if (checkNecklace.Checked && checkEuropean.Checked)
            filters.Add(new TypeIdFilter(3, 1, 12, 2));

        if (checkNecklace.Checked && checkChinese.Checked)
            filters.Add(new TypeIdFilter(3, 1, 5, 2));

        if (checkNecklace.Checked && !checkEuropean.Checked && !checkChinese.Checked)
        {
            filters.Add(new TypeIdFilter(3, 1, 5, 2));
            filters.Add(new TypeIdFilter(3, 1, 12, 2));
        }

        if (checkEarring.Checked && checkEuropean.Checked)
            filters.Add(new TypeIdFilter(3, 1, 12, 1));

        if (checkEarring.Checked && checkChinese.Checked)
            filters.Add(new TypeIdFilter(3, 1, 5, 1));

        if (checkEarring.Checked && !checkEuropean.Checked && !checkChinese.Checked)
        {
            filters.Add(new TypeIdFilter(3, 1, 5, 1));
            filters.Add(new TypeIdFilter(3, 1, 12, 1));
        }

        #endregion Accessory

        #region Shields

        if (checkShield.Checked && checkChinese.Checked)
            filters.Add(new TypeIdFilter(3, 1, 4, 1));

        if (checkShield.Checked && checkEuropean.Checked)
            filters.Add(new TypeIdFilter(3, 1, 4, 2));

        if (checkShield.Checked && !checkEuropean.Checked && !checkChinese.Checked)
        {
            filters.Add(new TypeIdFilter(3, 1, 4, 1));
            filters.Add(new TypeIdFilter(3, 1, 4, 2));
        }

        #endregion Shields

        #endregion Equipment

        if (checkAlchemy.Checked)
            filters.AddRange(GetAlchemyFilters());

        if (checkQuest.Checked)
            filters.Add(new TypeIdFilter(p => (p as RefObjItem).IsQuest));

        if (checkAmmo.Checked)
        {
            filters.Add(new TypeIdFilter(3, 3, 4, 1));
            filters.Add(new TypeIdFilter(3, 3, 4, 2));
        }

        if (checkCoin.Checked)
            filters.Add(new TypeIdFilter(3, 3, 5, 1));

        if (checkOther.Checked)
            filters.Add(new TypeIdFilter
            {
                CompareByTypeID2 = true,
                TypeID2 = 3
            });

        if (filters.Count == 0)
            filters.Add(new TypeIdFilter { CompareByTypeID1 = true, TypeID1 = 3 });

        var gender = ObjectGender.Neutral;

        if (checkMale.Checked && checkFemale.Checked)
        {
            /* nothing do anything, already neutral */
        }
        else if (checkMale.Checked)
        {
            gender = ObjectGender.Male;
        }
        else if (checkFemale.Checked)
        {
            gender = ObjectGender.Female;
        }

        var items = Game.ReferenceManager.GetFilteredItems(filters, Convert.ToByte(numDegreeFrom.Value),
            Convert.ToByte(numDegreeTo.Value), gender, checkBoxRareItems.Checked, txtSellSearch.Text);
        if (items.Count == 0)
        {
            listFilter.Visible = true;
            MessageBox.Show(this, LanguageManager.GetLang("NoResultsFound"), T("RSBot.Items.Warning", "Warning"));
            return;
        }

        await PopulateSellListAsync(items);
        labelResult.Text = $"{items.Count}";
    }

    /// <summary>
    ///     Populates the sell list.
    /// </summary>
    /// <param name="items">The items.</param>
    private async Task PopulateSellListAsync(List<RefObjItem> items)
    {
        listFilter.BeginUpdate();

        string getSubItemString(RefObjItem item)
        {
            var filter = PickupManager.PickupFilter.Find(p => p.CodeName == item.CodeName);
            if (string.IsNullOrWhiteSpace(filter.CodeName))
                return "•";

            if (filter.PickOnlyChar)
                return "√ (C)";

            return "√";
        }

        var listViewItems = new ListViewItem[items.Count];
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];

            var listViewItem = new ListViewItem
            {
                Text = item.GetRealName(true),
                Tag = item.CodeName,
                SubItems =
                {
                    $"{item.ReqLevel1} (Dg.{item.Degree})",
                    ((ObjectGender)item.ReqGender).ToString(),
                    getSubItemString(item),
                    ShoppingManager.SellFilter.Contains(item.CodeName) ? "√" : "•",
                    ShoppingManager.StoreFilter.Contains(item.CodeName) ? "√" : "•"
                }
            };

            await listViewItem.LoadItemImage(item);
            listViewItems[i] = listViewItem;
        }

        listFilter.Items.AddRange(listViewItems);
        listFilter.EndUpdate();


        listFilter.Visible = true;

        await Task.Delay(1).ConfigureAwait(false);
    }

    /// <summary>
    ///     Gets the chinese gear filters.
    /// </summary>
    /// <returns></returns>
    private List<TypeIdFilter> GetAlchemyFilters()
    {
        var result = new List<TypeIdFilter>();

        for (byte i = 1; i <= 3; i++)
            result.Add(new TypeIdFilter { TypeID1 = 3, TypeID2 = 3, TypeID3 = 10, TypeID4 = i });

        for (byte i = 1; i <= 10; i++)
            result.Add(new TypeIdFilter { TypeID1 = 3, TypeID2 = 3, TypeID3 = 11, TypeID4 = i });

        return result;
    }

    /// <summary>
    ///     Fired when the core finished to load the game data
    /// </summary>
    private void OnLoadGameData()
    {
        LoadGroups();

        Invoke(() =>
        {
            if (IsDisposed || comboStore.Items.Count == 0)
                return;

            if (comboStore.SelectedIndex < 0)
                comboStore.SelectedIndex = 0;
        });
    }

    /// <summary>
    ///     Loads the settings.
    /// </summary>
    private void LoadSettings()
    {
        _loadingSettings = true;

        Invoke(() =>
        {
            checkEnable.Checked = PlayerConfig.Get("RSBot.Shopping.Enabled", true);
            checkRepairGear.Checked = PlayerConfig.Get("RSBot.Shopping.RepairGear", true);
            checkSellItemsFromPet.Checked = PlayerConfig.Get("RSBot.Shopping.SellPetItems", true);
            checkPickupGold.Checked = PlayerConfig.Get("RSBot.Items.Pickup.Gold", true);
            checkPickupRare.Checked = PlayerConfig.Get("RSBot.Items.Pickup.Rare", true);
            checkPickupBlue.Checked = PlayerConfig.Get("RSBot.Items.Pickup.Blue", true);
            checkEnableAbilityPet.Checked = PlayerConfig.Get("RSBot.Items.Pickup.EnableAbilityPet", true);
            checkStoreItemsFromPet.Checked = PlayerConfig.Get("RSBot.Shopping.StorePetItems", true);
            checkDontPickupInBerzerk.Checked = PlayerConfig.Get("RSBot.Items.Pickup.DontPickupInBerzerk", true);
            cbJustpickmyitems.Checked = PlayerConfig.Get("RSBot.Items.Pickup.JustPickMyItems", false);
            cbDontPickupWhileBotting.Checked = PlayerConfig.Get<bool>("RSBot.Items.Pickup.DontPickupWhileBotting");

            checkQuestItems.Checked = PlayerConfig.Get<bool>("RSBot.Items.Pickup.Quest", true);
            checkAllEquips.Checked = PlayerConfig.Get<bool>("RSBot.Items.Pickup.AnyEquips");
            checkEverything.Checked = PlayerConfig.Get<bool>("RSBot.Items.Pickup.Everything");

            ShoppingManager.Enabled = checkEnable.Checked;
            ShoppingManager.RepairGear = checkRepairGear.Checked;
            ShoppingManager.SellPetItems = checkSellItemsFromPet.Checked;
            ShoppingManager.StorePetItems = checkStoreItemsFromPet.Checked;

            LoadShoppingList();

            ShoppingManager.LoadFilters();
            PickupManager.LoadFilter();
        });

        _loadingSettings = false;
    }

    private async void txtSellSearch_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
            await QuerySellItemsAsync();
    }

    private void contextList_Opening(object sender, CancelEventArgs e)
    {
        if (listFilter.SelectedItems.Count != 1)
        {
            btnAddToSell.Checked = false;
            btnAddToStore.Checked = false;
            btnPickup.Checked = false;
            btnPickOnlyCharacter.Checked = false;
            return;
        }

        var item = listFilter.SelectedItems[0];
        var codeName = (string)item.Tag;

        btnAddToSell.Checked = ShoppingManager.SellFilter.Contains(codeName);
        btnAddToStore.Checked = ShoppingManager.StoreFilter.Contains(codeName);
        btnPickup.Checked = PickupManager.PickupFilter.Any(p => p.CodeName == codeName && !p.PickOnlyChar);
        btnPickOnlyCharacter.Checked = PickupManager.PickupFilter.Any(p => p.CodeName == codeName && p.PickOnlyChar);
    }

    #region Shopping manager

    /// <summary>
    ///     Handles the SelectedIndexChanged event of the comboStore control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void comboStore_SelectedIndexChanged(object sender, EventArgs e)
    {
        PopulateProductList(comboStore.SelectedIndex);
    }

    /// <summary>
    ///     Handles the Click event of the menuAddToShoppingList control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void menuAddToShoppingList_Click(object sender, EventArgs e)
    {
        foreach (ListViewItem listItem in listAvailableProducts.SelectedItems)
        {
            var refItem = Game.ReferenceManager.GetRefItem(listItem.Name);
            var title = LanguageManager.GetLang("InputDialogTitle");
            var content = LanguageManager.GetLang("InputDialogContent");
            var itemNameTrans = LanguageManager.GetLang("InputDialogItemName", refItem.GetRealName(), refItem.MaxStack);
            var dialog = new InputDialog(title, itemNameTrans, content, InputDialog.InputType.Numeric);
            if (dialog.ShowDialog(this) == DialogResult.Cancel)
                return;

            if (listShoppingList.Items.ContainsKey(listItem.Name))
            {
                var item = listShoppingList.Items[listItem.Name];
                if (!int.TryParse(item.SubItems[1].Text.Substring(1), out var amount))
                    continue;

                item.SubItems[1].Text = $"x{Convert.ToInt32(dialog.Value) + amount}";
                continue;
            }

            var newListItem = (ListViewItem)listItem.Clone();
            newListItem.Group = listShoppingList.Groups[comboStore.SelectedIndex];

            newListItem.SubItems.Add("x" + dialog.Value);
            listShoppingList.Items.Add(newListItem);
        }

        SaveShoppingList();
    }

    /// <summary>
    ///     Handles the Click event of the menuRemoveItem control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void menuRemoveItem_Click(object sender, EventArgs e)
    {
        foreach (ListViewItem item in listShoppingList.SelectedItems)
            listShoppingList.Items.Remove(item);

        SaveShoppingList();
    }

    /// <summary>
    ///     Handles the Click event of the menuChangeAmount control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>
    private void menuChangeAmount_Click(object sender, EventArgs e)
    {
        foreach (ListViewItem item in listShoppingList.SelectedItems)
        {
            var defaultValue = int.Parse(item.SubItems[1].Text.Substring(1, item.SubItems[1].Text.Length - 1));

            var title = LanguageManager.GetLang("InputDialogTitle");
            var content = LanguageManager.GetLang("InputDialogContent");
            var dialog = new InputDialog(title, item.Text, content, InputDialog.InputType.Numeric);
            dialog.Numeric.Value = defaultValue;

            if (dialog.ShowDialog(this) == DialogResult.Cancel)
                return;

            item.SubItems[1].Text = "x" + dialog.Value;
        }

        SaveShoppingList();
    }


    private void checkShoppingSetting_CheckedChanged(object sender, EventArgs e)
    {
        if (_loadingSettings)
            return;

        ShoppingManager.RepairGear = checkRepairGear.Checked;
        PlayerConfig.Set("RSBot.Shopping.RepairGear", checkRepairGear.Checked);

        ShoppingManager.Enabled = checkEnable.Checked;
        PlayerConfig.Set("RSBot.Shopping.Enabled", checkEnable.Checked);

        PlayerConfig.Set("RSBot.Shopping.StorePetItems", checkStoreItemsFromPet.Checked);
        ShoppingManager.StorePetItems = checkStoreItemsFromPet.Checked;

        PlayerConfig.Set("RSBot.Shopping.SellPetItems", checkSellItemsFromPet.Checked);
        ShoppingManager.SellPetItems = checkSellItemsFromPet.Checked;
    }

    /// <summary>
    ///     Handles the TextChanged event of the txtShopSearch control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void txtShopSearch_TextChanged(object sender, EventArgs e)
    {
        PopulateProductList(comboStore.SelectedIndex, txtShopSearch.Text);
    }

    #endregion Shopping manager

    #region SellFilter

    /// <summary>
    ///     Handles the Click event of the btnReload control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private async void btnApply_Click(object sender, EventArgs e)
    {
        await QuerySellItemsAsync();
    }

    /// <summary>
    ///     Handles the Click event of the btnAddToSell control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void btnAddToSell_Click(object sender, EventArgs e)
    {
        foreach (ListViewItem item in listFilter.SelectedItems)
        {
            item.SubItems[4].Text = "√";

            ShoppingManager.SellFilter.Remove((string)item.Tag);
            ShoppingManager.SellFilter.Add((string)item.Tag);
        }

        ShoppingManager.SaveFilters();
    }

    /// <summary>
    ///     Handles the Click event of the btnPickup control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void btnPickup_Click(object sender, EventArgs e)
    {
        foreach (ListViewItem item in listFilter.SelectedItems)
        {
            item.SubItems[3].Text = "√";

            PickupManager.RemoveFilter((string)item.Tag);
            PickupManager.AddFilter((string)item.Tag);
        }
    }

    /// <summary>
    ///     Handles the Click event of the btnPickOnlyCharacter control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void btnPickOnlyCharacter_Click(object sender, EventArgs e)
    {
        foreach (ListViewItem item in listFilter.SelectedItems)
        {
            item.SubItems[3].Text = "√ (C)";
            PickupManager.RemoveFilter((string)item.Tag);
            PickupManager.AddFilter((string)item.Tag, true);
        }
    }

    /// <summary>
    ///     Handles the Click event of the btnAddToStore control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void btnAddToStore_Click(object sender, EventArgs e)
    {
        foreach (ListViewItem item in listFilter.SelectedItems)
        {
            item.SubItems[5].Text = "√";

            ShoppingManager.StoreFilter.Remove((string)item.Tag);
            ShoppingManager.StoreFilter.Add((string)item.Tag);
        }

        ShoppingManager.SaveFilters();
    }

    /// <summary>
    ///     Handles the Click event of the btnDontSell control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void btnDontSell_Click(object sender, EventArgs e)
    {
        foreach (ListViewItem item in listFilter.SelectedItems)
        {
            item.SubItems[4].Text = "•";
            ShoppingManager.SellFilter.Remove((string)item.Tag);
        }

        ShoppingManager.SaveFilters();
    }

    /// <summary>
    ///     Handles the Click event of the btnDontStore control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void btnDontStore_Click(object sender, EventArgs e)
    {
        foreach (ListViewItem item in listFilter.SelectedItems)
        {
            item.SubItems[5].Text = "•";
            ShoppingManager.StoreFilter.Remove((string)item.Tag);
        }

        ShoppingManager.SaveFilters();
    }

    /// <summary>
    ///     Handles the Click event of the btnDontPickup control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void btnDontPickup_Click(object sender, EventArgs e)
    {
        foreach (ListViewItem item in listFilter.SelectedItems)
        {
            item.SubItems[3].Text = "•";
            PickupManager.RemoveFilter((string)item.Tag);
        }

        ShoppingManager.SaveFilters();
    }

    /// <summary>
    ///     Handles the Click event of the btnResetFilter control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void btnResetFilter_Click(object sender, EventArgs e)
    {
        foreach (var group in filterPanel.Controls.OfType<GroupBox>())
            foreach (var checkBox in group.Controls.OfType<CheckBox>())
                checkBox.Checked = false;

        listFilter.Items.Clear();
        numDegreeFrom.Value = 0;
        numDegreeTo.Value = 0;
        labelResult.Text = string.Empty;
    }

    #endregion SellFilter

    #region Pickup

    /// <summary>
    ///     Handles the CheckedChanged event of the checkShowEquipment control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
    private void checkShowEquipment_CheckedChanged(object sender, EventArgs e)
    {
        PopulateProductList(comboStore.SelectedIndex, txtShopSearch.Text);
    }

    private void checkPickupSettings_CheckedChanged(object sender, EventArgs e)
    {
        if (_loadingSettings)
            return;

        PlayerConfig.Set("RSBot.Items.Pickup.Everything", checkEverything.Checked);
        PlayerConfig.Set("RSBot.Items.Pickup.AnyEquips", checkAllEquips.Checked);
        PlayerConfig.Set("RSBot.Items.Pickup.Quest", checkQuestItems.Checked);
        PlayerConfig.Set("RSBot.Items.Pickup.DontPickupWhileAttacking", cbDontPickupWhileBotting.Checked);
        PlayerConfig.Set("RSBot.Items.Pickup.DontPickupWhileBotting", cbDontPickupWhileBotting.Checked);
        PlayerConfig.Set("RSBot.Items.Pickup.JustPickMyItems", cbJustpickmyitems.Checked);
        PlayerConfig.Set("RSBot.Items.Pickup.DontPickupInBerzerk", checkDontPickupInBerzerk.Checked);
        PlayerConfig.Set("RSBot.Items.Pickup.EnableAbilityPet", checkEnableAbilityPet.Checked);
        PlayerConfig.Set("RSBot.Items.Pickup.Blue", checkPickupBlue.Checked);
        PlayerConfig.Set("RSBot.Items.Pickup.Rare", checkPickupRare.Checked);
        PlayerConfig.Set("RSBot.Items.Pickup.Gold", checkPickupGold.Checked);
    }
    #endregion Pickup
}