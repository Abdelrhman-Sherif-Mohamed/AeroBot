using System;

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

using RSBot.Core.Network;

using RSBot.Core.Objects;

using RSBot.Core.Objects.Inventory;

using RSBot.Core.Objects.Spawn;



using SDUI;

using SDUI.Controls;

using Button = SDUI.Controls.Button;

using ListViewExtensions = RSBot.Core.Extensions.ListViewExtensions;



namespace RSBot.Inventory.Views;



[ToolboxItem(false)]

public partial class Main : DoubleBufferedControl

{

    /// <summary>

    ///     <inheritdoc />

    /// </summary>

    private readonly object _lock;



    /// <summary>

    ///     <inheritdoc />

    /// </summary>

    private int _selectedIndex;



    /// <summary>

    ///     The index of the page that shows the happy event controls.

    /// </summary>

    private const int HappyPageIndex = 10;

    /// <summary>
    ///     The index of the page that shows storage ordering & page segregation.
    /// </summary>
    private const int StorageOrderPageIndex = 11;



    /// <summary>

    ///     The entity id of the last NPC that was used to open a storage.

    /// </summary>

    private uint _lastInteractedNpcId;

    private uint _lastStorageOpenEntityId;



    /// <summary>

    ///     The entity id of the NPC that opened the personal storage.

    /// </summary>

    private uint _storageNpcId;



    /// <summary>

    ///     The entity id of the NPC that opened the guild storage.

    /// </summary>

    private uint _guildStorageNpcId;



    /// <summary>

    ///     Initializes a new instance of the <see cref="Main" /> class.

    /// </summary>

    public Main()

    {

        _lock = new object();

        InitializeComponent();

        SubscribeEvents();



        listViewMain.SmallImageList = ListViewExtensions.StaticItemsImageList;

        listViewMain.BackColor = ColorScheme.TableBackColor;

        listViewMain.ForeColor = ColorScheme.ForeColor;



        var backColor = ColorScheme.BorderColor.Determine().Alpha(85);

        buttonInventory.ForeColor = backColor.Determine();

        buttonInventory.Color = backColor;

    }



    /// <summary>

    ///     Subscribes the events.

    /// </summary>

    private void SubscribeEvents()

    {

        EventManager.SubscribeEvent("OnLoadCharacter", OnLoadCharacter);

        EventManager.SubscribeEvent("OnUpdateInventoryItem", new Action<byte>(OnUpdateInventoryItem));

        EventManager.SubscribeEvent("OnUseItem", new Action<byte>(OnUpdateInventoryItem));

        EventManager.SubscribeEvent("OnInventoryUpdate", UpdateInventoryList);

        EventManager.SubscribeEvent("OnStorageOpenRequest", new Action<uint>(OnStorageOpenRequest));
        EventManager.SubscribeEvent("OnTalkRequest", new Action<uint, TalkOption>(OnTalkRequest));
        EventManager.SubscribeEvent("OnTalkToNpc", new Action<uint>(OnTalkToNpc));

        EventManager.SubscribeEvent("OnStorageData", OnStorageData);

        EventManager.SubscribeEvent("OnGuildStorageData", OnGuildStorageData);

    }



    /// <summary>

    ///     Stores the entity id of the NPC that is used to open a storage.

    /// </summary>

    /// <param name="entityId">The unique id of the NPC.</param>

        private void OnTalkRequest(uint entityId, TalkOption option)
    {
        _lastInteractedNpcId = entityId;
        _lastStorageOpenEntityId = entityId;
    }

    private void OnTalkToNpc(uint entityId)
    {
        _lastInteractedNpcId = entityId;
        _lastStorageOpenEntityId = entityId;
    }

    private void OnStorageOpenRequest(uint entityId)
    {
        _lastStorageOpenEntityId = entityId;
        _lastInteractedNpcId = entityId;
    }



    /// <summary>

    ///     Remembers the NPC that opened the personal storage and executes auto-order/sort if enabled.

    /// </summary>

        private void OnStorageData()
    {
        _storageNpcId = ResolveStorageNpcId(_storageNpcId != 0 ? _storageNpcId : _lastStorageOpenEntityId);
        Log.Debug($"[Inventory] Personal storage data received. Resolved NPC ID: {_storageNpcId}");

        var autoOrder = PlayerConfig.Get("RSBot.Storage.AutoOrder", false);
        var autoSort = PlayerConfig.Get("RSBot.Storage.AutoSort", false);

        if (autoOrder)
        {
            var npcId = ResolveStorageNpcId(_storageNpcId);
            if (Game.Player?.Storage != null)
            {
                Task.Run(() => Game.Player.Storage.Order(npcId, false));
            }
        }
        else if (autoSort)
        {
            var npcId = ResolveStorageNpcId(_storageNpcId);
            if (Game.Player?.Storage != null)
            {
                Task.Run(() => Game.Player.Storage.Sort(npcId, false));
            }
        }
    }

    private void OnGuildStorageData()
    {
        _guildStorageNpcId = ResolveStorageNpcId(_guildStorageNpcId != 0 ? _guildStorageNpcId : _lastInteractedNpcId);
        Log.Debug($"[Inventory] Guild storage data received. Resolved NPC ID: {_guildStorageNpcId}");

        var autoOrder = PlayerConfig.Get("RSBot.GuildStorage.AutoOrder", false);
        var autoSort = PlayerConfig.Get("RSBot.GuildStorage.AutoSort", false);

        if (autoOrder)
        {
            var npcId = ResolveStorageNpcId(_guildStorageNpcId);
            if (Game.Player?.GuildStorage != null)
            {
                Task.Run(() => Game.Player.GuildStorage.Order(npcId, true));
            }
        }
        else if (autoSort)
        {
            var npcId = ResolveStorageNpcId(_guildStorageNpcId);
            if (Game.Player?.GuildStorage != null)
            {
                Task.Run(() => Game.Player.GuildStorage.Sort(npcId, true));
            }
        }
    }



        private uint ResolveStorageNpcId(uint knownNpcId)
    {
        if (knownNpcId != 0)
            return knownNpcId;

        if (_lastInteractedNpcId != 0)
            return _lastInteractedNpcId;

        if (Game.Player?.State?.DialogState?.RequestedNpcId > 0)
            return Game.Player.State.DialogState.RequestedNpcId;

        if (Game.Player?.State?.DialogState?.Npc != null)
            return Game.Player.State.DialogState.Npc.UniqueId;

        if (Game.SelectedEntity != null)
            return Game.SelectedEntity.UniqueId;

        if (_lastStorageOpenEntityId != 0)
            return _lastStorageOpenEntityId;

        if (_storageNpcId != 0)
            return _storageNpcId;

        if (_guildStorageNpcId != 0)
            return _guildStorageNpcId;

        // Auto-detect nearest NPC around player (within 35m)
        if (SpawnManager.TryGetEntities<SpawnedNpc>(out var npcs))
        {
            var nearestNpc = npcs
                .Where(n => n.DistanceToPlayer <= 35f)
                .OrderBy(n => n.DistanceToPlayer)
                .FirstOrDefault();

            if (nearestNpc != null)
                return nearestNpc.UniqueId;
        }

        return 0;
    }



    private void OnLoadCharacter()

    {

        UpdateInventoryList();

    }



    /// <summary>

    ///     Calling when update inventory item

    /// </summary>

    /// <param name="slot"></param>

    private void OnUpdateInventoryItem(byte slot)

    {

        var key = slot.ToString();

        if (!listViewMain.Items.ContainsKey(key))

            return;



        lock (_lock)

        {

            var inventoryItem = Game.Player.Inventory.GetItemAt(slot);

            if (inventoryItem == null || inventoryItem.Amount == 0)
            {
                listViewMain.Items.RemoveByKey(key);
                return;
            }



            var listViewItem = listViewMain.Items[key];



            var name = inventoryItem.Record.GetRealName();

            if (inventoryItem.OptLevel > 0)

                name += " (+" + inventoryItem.OptLevel + ")";



            listViewItem.SubItems[0].Text = name;

            listViewItem.SubItems[1].Text = inventoryItem.Amount.ToString();



            if (inventoryItem.Record.IsEquip)

                listViewItem.SubItems[2].Text = inventoryItem.Record.GetRarityName();



            listViewItem.LoadItemImageAsync(inventoryItem.Record);

        }

    }



    /// <summary>

    ///     Updates the inventory list.

    /// </summary>

    public void UpdateInventoryList()

    {
        if (InvokeRequired)
        {
            try { BeginInvoke(new System.Action(UpdateInventoryList)); } catch { }
            return;
        }


        if (!Visible)

            return;



        if (Game.Player == null)

            return;



        if (_selectedIndex == HappyPageIndex || _selectedIndex == StorageOrderPageIndex)

            return;



        lock (_lock)

        {

            listViewMain.BeginUpdate();

            listViewMain.Items.Clear();



            switch (_selectedIndex)

            {

                case 0:

                    var itemsPlayer = Game.Player.Inventory.GetNormalPartItems();

                    foreach (var item in itemsPlayer)

                        AddItem(item);



                    lblFreeSlots.Text = Game.Player.Inventory.FreeSlots + "/" + Game.Player.Inventory.Capacity;

                    pbInventoryStatus.Value = Game.Player.Inventory.FreeSlots;

                    pbInventoryStatus.Maximum = Game.Player.Inventory.Capacity;

                    break;



                case 1:



                    var items = Game.Player.Inventory.GetEquippedPartItems();

                    foreach (var item in items)

                        AddItem(item);



                    lblFreeSlots.Text = items.Count + " / 13";



                    pbInventoryStatus.Value = items.Count;

                    pbInventoryStatus.Maximum = 13;



                    break;



                case 2:



                    foreach (var item in Game.Player.Avatars)

                        AddItem(item);



                    lblFreeSlots.Text = "0";



                    pbInventoryStatus.Value = Game.Player.Avatars.FreeSlots;

                    pbInventoryStatus.Maximum = Game.Player.Avatars.Capacity;



                    break;



                case 3:



                    if (!Game.Player.HasActiveAbilityPet)

                    {

                        listViewMain.EndUpdate();

                        return;

                    }



                    foreach (var item in Game.Player.AbilityPet.Inventory)

                        AddItem(item);



                    lblFreeSlots.Text = Game.Player.AbilityPet.Inventory.FreeSlots + "/" +

                                        Game.Player.AbilityPet.Inventory.Capacity;



                    pbInventoryStatus.Value = Game.Player.AbilityPet.Inventory.FreeSlots;

                    pbInventoryStatus.Maximum = Game.Player.AbilityPet.Inventory.Capacity;



                    break;



                case 4:



                    if (Game.Player.Storage == null)

                    {

                        listViewMain.EndUpdate();

                        return;

                    }



                    foreach (var item in Game.Player.Storage)

                        AddItem(item);



                    lblFreeSlots.Text = Game.Player.Storage.FreeSlots + "/" + Game.Player.Storage.Capacity;



                    pbInventoryStatus.Value = Game.Player.Storage.FreeSlots;

                    pbInventoryStatus.Maximum = Game.Player.Storage.Capacity;



                    break;



                case 5:



                    if (Game.Player.GuildStorage == null)

                    {

                        listViewMain.EndUpdate();

                        return;

                    }



                    foreach (var item in Game.Player.GuildStorage)

                        AddItem(item);



                    lblFreeSlots.Text = Game.Player.GuildStorage.FreeSlots + "/" + Game.Player.GuildStorage.Capacity;



                    pbInventoryStatus.Value = Game.Player.GuildStorage.FreeSlots;

                    pbInventoryStatus.Maximum = Game.Player.GuildStorage.Capacity;



                    break;



                case 6:



                    if (Game.Player.JobTransport == null)

                    {

                        listViewMain.EndUpdate();

                        return;

                    }



                    foreach (var item in Game.Player.JobTransport.Inventory)

                        AddItem(item);



                    lblFreeSlots.Text = Game.Player.JobTransport.Inventory.FreeSlots + "/" +

                                        Game.Player.JobTransport.Inventory.Capacity;



                    pbInventoryStatus.Value = Game.Player.JobTransport.Inventory.FreeSlots;

                    pbInventoryStatus.Maximum = Game.Player.JobTransport.Inventory.Capacity;



                    break;



                case 7:



                    if (Game.Player.Job2SpecialtyBag == null)

                    {

                        listViewMain.EndUpdate();

                        return;

                    }



                    foreach (var item in Game.Player.Job2SpecialtyBag)

                        AddItem(item);



                    lblFreeSlots.Text = Game.Player.Job2SpecialtyBag.FreeSlots + "/" +

                                        Game.Player.Job2SpecialtyBag.Capacity;



                    pbInventoryStatus.Value = Game.Player.Job2SpecialtyBag.FreeSlots;

                    pbInventoryStatus.Maximum = Game.Player.Job2SpecialtyBag.Capacity;



                    break;



                case 8:



                    if (Game.Player.Job2 == null)

                    {

                        listViewMain.EndUpdate();

                        return;

                    }



                    foreach (var item in Game.Player.Job2)

                        AddItem(item);



                    lblFreeSlots.Text = Game.Player.Job2.FreeSlots + "/" + Game.Player.Job2.Capacity;



                    pbInventoryStatus.Value = Game.Player.Job2.FreeSlots;

                    pbInventoryStatus.Maximum = Game.Player.Job2.Capacity;



                    break;



                case 9:



                    if (!Game.Player.HasActiveFellowPet)

                    {

                        listViewMain.EndUpdate();

                        return;

                    }



                    foreach (var item in Game.Player.Fellow.Inventory)

                        AddItem(item);



                    lblFreeSlots.Text = Game.Player.Fellow.Inventory.FreeSlots + "/" +

                                        Game.Player.Fellow.Inventory.Capacity;



                    pbInventoryStatus.Value = Game.Player.Fellow.Inventory.FreeSlots;

                    pbInventoryStatus.Maximum = Game.Player.Fellow.Inventory.Capacity;



                    break;

            }



            listViewMain.EndUpdate();

        }

    }



    /// <summary>

    ///     Adds the item.

    /// </summary>

    /// <param name="item">The item.</param>

    private void AddItem(InventoryItem item)

    {

        if (item == null)

            return;



        var name = item.Record?.GetRealName() ?? "";

        if (item.OptLevel > 0)

            name += " (+" + item.OptLevel + ")";



        var lvItem = listViewMain.Items.Add(item.Slot.ToString(), name, 0);

        lvItem.Tag = item;

        lvItem.SubItems.Add(item.Amount.ToString());



        if (item.Record.IsEquip)

            lvItem.SubItems.Add(item.Record.GetRarityName());



        if (_selectedIndex == 0)

        {

            var useItemsAtTrainingPlace =

                PlayerConfig.GetArray<string>("RSBot.Inventory.ItemsAtTrainplace");



            if (useItemsAtTrainingPlace.Contains(item.Record.CodeName))

                lvItem.Font = new Font(lvItem.Font, FontStyle.Bold);

        }



        lvItem.LoadItemImageAsync(item.Record);

    }



    /// <summary>

    ///     Handles the visible changed event of the parent.

    /// </summary>

    /// <param name="sender">The source of the event.</param>

    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>

    private void Main_VisibleChanged(object sender, EventArgs e)

    {

        UpdateInventoryList();

    }



    /// <summary>

    ///     Handles the Click event of the btnReload control.

    /// </summary>

    /// <param name="sender">The source of the event.</param>

    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>

    private void buttonUseItem_Click(object sender, EventArgs e)

    {

        if (listViewMain.SelectedIndices.Count != 1)

            return;



        var listViewItem = listViewMain.SelectedItems[0];

        var inventoryItem = listViewItem.Tag as InventoryItem;

        inventoryItem?.Use();

    }



    /// <summary>

    ///     Handles the mouse double click event of the listviewmain control.

    /// </summary>

    /// <param name="sender">The source of the event.</param>

    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>

    private void listViewMain_MouseDoubleClick(object sender, MouseEventArgs e)

    {

        if (listViewMain.SelectedItems.Count <= 0)

            return;



        if (!Kernel.Debug)

            return;



        var itemForm = new ItemProperties(listViewMain.SelectedItems[0].Tag as InventoryItem);

        itemForm.Show();

    }



    /// <summary>

    ///     Handles the selected index changed event of the button's control.

    /// </summary>

    /// <param name="sender">The source of the event.</param>

    /// <param name="e">The <see cref="System.EventArgs" /> instance containing the event data.</param>

    private void ButtonSwitcher(object sender, EventArgs e)

    {

        var button = sender as Button;

        if (_selectedIndex == button.TabIndex)

            return;



        _selectedIndex = button.TabIndex;



                var isHappyPage = _selectedIndex == HappyPageIndex;
        var isStorageOrderPage = _selectedIndex == StorageOrderPageIndex;

        panelHappy.Visible = isHappyPage;
        panelStorageOrder.Visible = isStorageOrderPage;
        listViewMain.Visible = !isHappyPage && !isStorageOrderPage;
        panel1.Visible = !isHappyPage && !isStorageOrderPage;



        // Inventory (0), Storage (4), Guild Storage (5) all support Sort and Order

        var supportsSortAndOrder = _selectedIndex == 0 || _selectedIndex == 4 || _selectedIndex == 5;

        btnSort.Visible = supportsSortAndOrder;

        btnOrder.Visible = supportsSortAndOrder;

        checkAutoSort.Visible = supportsSortAndOrder;

        checkAutoOrder.Visible = supportsSortAndOrder;



        if (supportsSortAndOrder)

        {

            UpdateCheckboxesForCurrentTab();

        }



        foreach (var control in topPanel.Controls.OfType<Button>())

        {

            if (control.TabIndex > StorageOrderPageIndex)

                continue;



            control.Color = Color.Transparent;



            if (control == button)

            {

                var backColor = ColorScheme.BorderColor.Determine().Alpha(85);

                control.ForeColor = backColor.Determine();

                control.Color = backColor;

            }



            control.Invalidate();

        }



        UpdateInventoryList();

    }



    private void dropToolStripMenuItem_Click(object sender, EventArgs e)

    {

        if (listViewMain.SelectedIndices.Count != 1)

            return;



        var listViewItem = listViewMain.SelectedItems[0];

        var inventoryItem = listViewItem.Tag as InventoryItem;

        if (inventoryItem == null)

            return;



        var cos = _selectedIndex == 3;

        inventoryItem?.Drop(cos, Game.Player.AbilityPet.UniqueId);

    }



    private void contextMenuStrip_Opening(object sender, CancelEventArgs e)

    {

        if (listViewMain.SelectedIndices.Count != 1)

        {

            e.Cancel = true;

            return;

        }



        var listViewItem = listViewMain.SelectedItems[0];

        if (listViewItem.Tag is not InventoryItem inventoryItem)

            return;



        if (_selectedIndex != 0)

        {

            useToolStripMenuItem.Visible = false;

            moveToLastDeathPositionToolStripMenuItem.Visible = false;

            moveToLastRecallPositionToolStripMenuItem.Visible = false;

            moveToPetToolStripMenuItem.Visible = false;

            moveToPlayerToolStripMenuItem.Visible = _selectedIndex == 3;

            selectMapLocationToolStripMenuItem.Visible = false;

            return;

        }



        var canUse = (inventoryItem.Record.CanUse & ObjectUseType.Yes) != 0;

        if (canUse)

        {

            var useItems = PlayerConfig.GetArray<string>("RSBot.Inventory.ItemsAtTrainplace");

            useItemAtTrainingPlaceMenuItem.Checked = useItems.Contains(inventoryItem.Record.CodeName);

            useItemAtTrainingPlaceMenuItem.Enabled = true;



            var purposiveItems = PlayerConfig.GetArray<string>("RSBot.Inventory.AutoUseAccordingToPurpose");

            autoUseAccordingToPurposeToolStripMenuItem.Checked = purposiveItems.Contains(inventoryItem.Record.CodeName);

            autoUseAccordingToPurposeToolStripMenuItem.Enabled = true;

        }

        else

        {

            useItemAtTrainingPlaceMenuItem.Checked = false;

            useItemAtTrainingPlaceMenuItem.Enabled = false;



            autoUseAccordingToPurposeToolStripMenuItem.Checked = false;

            autoUseAccordingToPurposeToolStripMenuItem.Enabled = false;

        }



        var isReverseScroll = inventoryItem.Equals(new TypeIdFilter(3, 3, 3, 3));

        useToolStripMenuItem.Visible = !isReverseScroll;

        useToolStripMenuItem.Enabled = inventoryItem.Record.CanUse != ObjectUseType.No;

        moveToLastDeathPositionToolStripMenuItem.Visible = isReverseScroll;

        moveToLastRecallPositionToolStripMenuItem.Visible = isReverseScroll;

        selectMapLocationToolStripMenuItem.Visible = isReverseScroll;

        dropToolStripMenuItem.Visible = inventoryItem.Record.CanDrop != ObjectDropType.No;



        moveToPetToolStripMenuItem.Visible = Game.Player.AbilityPet != null && _selectedIndex != 3;

        moveToPlayerToolStripMenuItem.Visible = _selectedIndex == 3;



        if (isReverseScroll)

        {

            var tagItem = selectMapLocationToolStripMenuItem.Tag as InventoryItem;

            if (tagItem != inventoryItem)

            {

                selectMapLocationToolStripMenuItem.Tag = inventoryItem;

                selectMapLocationToolStripMenuItem.DropDownItems.Clear();



                foreach (var item in Game.ReferenceManager.OptionalTeleports)

                {

                    var mapName = Game.ReferenceManager.GetTranslation(item.Value.Region.ToString());



                    var menuItem = new ToolStripMenuItem { Text = mapName };



                    menuItem.Click += (itemSender, itemEvent) => { inventoryItem.UseTo(7, item.Value.ID); };



                    selectMapLocationToolStripMenuItem.DropDownItems.Add(menuItem);

                }

            }

        }

    }



    private void moveToLastRecallPositionToolStripMenuItem_Click(object sender, EventArgs e)

    {

        if (listViewMain.SelectedIndices.Count != 1)

            return;



        var listViewItem = listViewMain.SelectedItems[0];

        var inventoryItem = listViewItem.Tag as InventoryItem;

        if (inventoryItem == null)

            return;



        inventoryItem.UseTo(2);

    }



    private void moveToLastDeathPositionToolStripMenuItem_Click(object sender, EventArgs e)

    {

        if (listViewMain.SelectedIndices.Count != 1)

            return;



        var listViewItem = listViewMain.SelectedItems[0];

        var inventoryItem = listViewItem.Tag as InventoryItem;

        if (inventoryItem == null)

            return;



        inventoryItem.UseTo(3);

    }



    private void moveToPetToolStripMenuItem_Click(object sender, EventArgs e)

    {

        if (listViewMain.SelectedIndices.Count != 1)

            return;



        var listViewItem = listViewMain.SelectedItems[0];

        var inventoryItem = listViewItem.Tag as InventoryItem;

        if (inventoryItem == null)

            return;



        if (Game.Player.AbilityPet != null)

            return;



        var freeSlot = Game.Player.AbilityPet.Inventory.GetFreeSlot();

        if (freeSlot == 0xFF)

            return;



        var packet = new Packet(0x7034);

        packet.WriteByte(InventoryOperation.SP_MOVE_ITEM_PC_PET);

        packet.WriteUInt(Game.Player.AbilityPet.UniqueId);

        packet.WriteByte(inventoryItem.Slot);

        packet.WriteByte(freeSlot);

        PacketManager.SendPacket(packet, PacketDestination.Server);

    }



    private void moveToPlayerToolStripMenuItem_Click(object sender, EventArgs e)

    {

        if (listViewMain.SelectedIndices.Count != 1)

            return;



        var listViewItem = listViewMain.SelectedItems[0];

        var inventoryItem = listViewItem.Tag as InventoryItem;

        if (inventoryItem == null)

            return;



        if (Game.Player.AbilityPet == null)

            return;



        var freeSlot = Game.Player.Inventory.GetFreeSlot();

        if (freeSlot == 0xFF)

            return;



        var packet = new Packet(0x7034);

        packet.WriteByte(InventoryOperation.SP_MOVE_ITEM_PET_PC);

        packet.WriteUInt(Game.Player.AbilityPet.UniqueId);

        packet.WriteByte(inventoryItem.Slot);

        packet.WriteByte(freeSlot);

        PacketManager.SendPacket(packet, PacketDestination.Server);

    }



    private void btnSort_Click(object sender, EventArgs e)

    {

        switch (_selectedIndex)

        {

            case 0:

                Task.Run(() => Game.Player?.Inventory?.Sort());

                break;



            case 4:

                {

                    var storage = Game.Player?.Storage;

                    if (storage == null)

                    {

                        Log.Warn("[Inventory] Cannot sort the storage: the storage is not open.");

                        return;

                    }



                    var npcId = ResolveStorageNpcId(_storageNpcId);

                    if (npcId == 0)

                    {

                        Log.Warn("[Inventory] Cannot sort the storage: the storage NPC was not detected.");

                        return;

                    }



                    Task.Run(() => storage.Sort(npcId, false));

                    break;

                }



            case 5:
                {
                    var guildStorage = Game.Player?.GuildStorage;
                    if (guildStorage == null)
                    {
                        Log.Warn("[Inventory] Cannot sort the guild storage: the guild storage is not open.");
                        return;
                    }

                    var npcId = ResolveStorageNpcId(_guildStorageNpcId);
                    if (_guildStorageNpcId == 0 && npcId != 0)
                        _guildStorageNpcId = npcId;

                    Task.Run(() => guildStorage.Sort(npcId, true));
                    break;
                }

        }

    }



    private void btnOrder_Click(object sender, EventArgs e)

    {

        switch (_selectedIndex)

        {

            case 0:

                Task.Run(() => Game.Player?.Inventory?.Order());

                break;



            case 4:

                {

                    var storage = Game.Player?.Storage;

                    if (storage == null)

                    {

                        Log.Warn("[Inventory] Cannot order the storage: the storage is not open.");

                        return;

                    }



                    var npcId = ResolveStorageNpcId(_storageNpcId);

                    if (npcId == 0)

                    {

                        Log.Warn("[Inventory] Cannot order the storage: the storage NPC was not detected.");

                        return;

                    }



                    Task.Run(() => storage.Order(npcId, false));

                    break;

                }



            case 5:
                {
                    var guildStorage = Game.Player?.GuildStorage;
                    if (guildStorage == null)
                    {
                        Log.Warn("[Inventory] Cannot order the guild storage: the guild storage is not open.");
                        return;
                    }

                    var npcId = ResolveStorageNpcId(_guildStorageNpcId);
                    if (_guildStorageNpcId == 0 && npcId != 0)
                        _guildStorageNpcId = npcId;

                    Task.Run(() => guildStorage.Order(npcId, true));
                    break;
                }

        }

    }



    private void checkAutoSort_CheckedChanged(object sender, EventArgs e)

    {

        switch (_selectedIndex)

        {

            case 0:

                PlayerConfig.Set("RSBot.Inventory.AutoSort", checkAutoSort.Checked);

                break;

            case 4:

                PlayerConfig.Set("RSBot.Storage.AutoSort", checkAutoSort.Checked);

                break;

            case 5:

                PlayerConfig.Set("RSBot.GuildStorage.AutoSort", checkAutoSort.Checked);

                break;

        }

    }



    private void checkAutoOrder_CheckedChanged(object sender, EventArgs e)

    {

        switch (_selectedIndex)

        {

            case 0:

                PlayerConfig.Set("RSBot.Inventory.AutoOrder", checkAutoOrder.Checked);

                break;

            case 4:

                PlayerConfig.Set("RSBot.Storage.AutoOrder", checkAutoOrder.Checked);

                break;

            case 5:

                PlayerConfig.Set("RSBot.GuildStorage.AutoOrder", checkAutoOrder.Checked);

                break;

        }

    }



    private void UpdateCheckboxesForCurrentTab()

    {

        switch (_selectedIndex)

        {

            case 0:

                checkAutoSort.Checked = PlayerConfig.Get("RSBot.Inventory.AutoSort", false);

        checkStorageSplitPages.Checked = PlayerConfig.Get("RSBot.Storage.SplitByPages", true);
        checkStorageAutoOrder.Checked = PlayerConfig.Get("RSBot.Storage.AutoOrder", false);
        checkStorageAutoSort.Checked = PlayerConfig.Get("RSBot.Storage.AutoSort", false);
        checkGuildStorageAutoOrder.Checked = PlayerConfig.Get("RSBot.GuildStorage.AutoOrder", false);
        checkGuildStorageAutoSort.Checked = PlayerConfig.Get("RSBot.GuildStorage.AutoSort", false);

                checkAutoOrder.Checked = PlayerConfig.Get("RSBot.Inventory.AutoOrder", false);

                break;

            case 4:

                checkAutoSort.Checked = PlayerConfig.Get("RSBot.Storage.AutoSort", false);

                checkAutoOrder.Checked = PlayerConfig.Get("RSBot.Storage.AutoOrder", false);

                break;

            case 5:

                checkAutoSort.Checked = PlayerConfig.Get("RSBot.GuildStorage.AutoSort", false);

                checkAutoOrder.Checked = PlayerConfig.Get("RSBot.GuildStorage.AutoOrder", false);

                break;

        }

    }



    private void useItemAtTrainingPlaceMenuItem_Click(object sender, EventArgs e)

    {

        if (listViewMain.SelectedItems.Count == 0)

            return;



        var lvItem = listViewMain.SelectedItems[0];

        var itemsToUse = PlayerConfig.GetArray<string>("RSBot.Inventory.ItemsAtTrainplace").ToList();

        var selectedItem = (InventoryItem)lvItem.Tag;

        if (selectedItem == null)

            return;



        var useSelectedItem = itemsToUse.Contains(selectedItem.Record.CodeName);



        if (useSelectedItem)

        {

            lvItem.Font = Font;

            itemsToUse.Remove(selectedItem.Record.CodeName);

        }

        else

        {

            lvItem.Font = new Font(lvItem.Font, FontStyle.Bold);

            itemsToUse.Add(selectedItem.Record.CodeName);

        }



        useItemAtTrainingPlaceMenuItem.Checked = !useItemAtTrainingPlaceMenuItem.Checked;

        PlayerConfig.SetArray("RSBot.Inventory.ItemsAtTrainplace", itemsToUse);

        PlayerConfig.Save();

    }



    private void autoUseAccordingToPurposeToolStripMenuItem_Click(object sender, EventArgs e)

    {

        if (listViewMain.SelectedItems.Count == 0)

            return;



        var lvItem = listViewMain.SelectedItems[0];



        var itemsToUse = PlayerConfig.GetArray<string>("RSBot.Inventory.AutoUseAccordingToPurpose").ToList();

        var selectedItem = (InventoryItem)lvItem.Tag;

        if (selectedItem == null)

            return;



        var useSelectedItem = itemsToUse.Contains(selectedItem.Record.CodeName);



        if (useSelectedItem)

        {

            lvItem.Font = Font;

            itemsToUse.Remove(selectedItem.Record.CodeName);

        }

        else

        {

            lvItem.Font = new Font(lvItem.Font, FontStyle.Bold);

            itemsToUse.Add(selectedItem.Record.CodeName);

        }



        autoUseAccordingToPurposeToolStripMenuItem.Checked = !autoUseAccordingToPurposeToolStripMenuItem.Checked;

        PlayerConfig.SetArray("RSBot.Inventory.AutoUseAccordingToPurpose", itemsToUse);

        PlayerConfig.Save();

    }



    /// <summary>

    ///     Occurs before Main form is displayed.

    /// </summary>

    /// <param name="sender"></param>

    /// <param name="e"></param>

    private void Main_Load(object sender, EventArgs e)

    {

        checkAutoSort.Checked = PlayerConfig.Get("RSBot.Inventory.AutoSort", false);

        checkAutoHappy.Checked = PlayerConfig.Get("RSBot.Inventory.Happy", false);

        var happyDelay = PlayerConfig.Get<int>("RSBot.Inventory.HappyDelay", 1);
        numHappyDelay.Value = Math.Min(numHappyDelay.Maximum, Math.Max(numHappyDelay.Minimum, (decimal)happyDelay));
        lblHappyDelay.Text = Kernel.Language == "ar_AR" ? "تأخير التحدث (ثواني):" : "NPC Delay (sec):";

        UpdateHappyUI();



        timerHappy.Interval = 1500;

        timerHappy.Tick += (s, ev) => OnTimerHappyTick();

        timerHappy.Start();

        checkStorageSplitPages.Checked = PlayerConfig.Get("RSBot.Storage.SplitByPages", true);
        checkStorageAutoOrder.Checked = PlayerConfig.Get("RSBot.Storage.AutoOrder", false);
        checkStorageAutoSort.Checked = PlayerConfig.Get("RSBot.Storage.AutoSort", false);
        checkGuildStorageAutoOrder.Checked = PlayerConfig.Get("RSBot.GuildStorage.AutoOrder", false);
        checkGuildStorageAutoSort.Checked = PlayerConfig.Get("RSBot.GuildStorage.AutoSort", false);

        panelStorageOrder.Visible = false;
        panelHappy.Visible = false;
        listViewMain.Visible = true;
        panel1.Visible = true;
        topPanel.BringToFront();

    }



    private void btnStartHappy_Click(object sender, EventArgs e)

    {

        if (HappyManager.Running)

            HappyManager.Stop();

        else

            HappyManager.Run(true);



        UpdateHappyUI();

    }



    private void btnStopHappy_Click(object sender, EventArgs e)

    {

        HappyManager.Stop();

        UpdateHappyUI();

    }



    private void checkAutoHappy_CheckedChanged(object sender, EventArgs e)

    {

        PlayerConfig.Set("RSBot.Inventory.Happy", checkAutoHappy.Checked);

        if (checkAutoHappy.Checked && !HappyManager.Running && Game.Player != null)

        {

            var letters = HappyManager.GetLetterCounts();

            var jewelCount = HappyManager.GetJewelBoxCount();

            if (letters.CanExchange || jewelCount >= 10)

            {

                var npc = HappyManager.GetEventSoOkNpc();

                if (npc != null && npc.DistanceToPlayer <= 20.0f)

                {

                    HappyManager.Run(true);

                }

            }

        }

        else if (!checkAutoHappy.Checked && HappyManager.Running)

        {

            HappyManager.Stop();

        }

        UpdateHappyUI();

    }



    private void OnTimerHappyTick()

    {

        UpdateHappyUI();



        if (checkAutoHappy.Checked && !HappyManager.Running && Game.Player != null)

        {

            var letters = HappyManager.GetLetterCounts();

            var jewelCount = HappyManager.GetJewelBoxCount();

            if (letters.CanExchange || jewelCount >= 10)

            {

                var npc = HappyManager.GetEventSoOkNpc();

                if (npc != null && npc.DistanceToPlayer <= 20.0f)

                {

                    HappyManager.Run(true);

                }

            }

        }

    }



    private void UpdateHappyUI()

    {

        if (IsDisposed) return;

        try

        {

            var letters = HappyManager.GetLetterCounts();

            var jewelCount = HappyManager.GetJewelBoxCount();

            lblHappyCounts.Text = $"H:{letters.H} A:{letters.A} P1:{letters.P1} P2:{letters.P2} Y:{letters.Y} | Jewel:{jewelCount}";



            if (HappyManager.Running)

            {

                lblHappyStatus.Text = "Running...";

                lblHappyStatus.ForeColor = Color.DarkOrange;

                btnStartHappy.Text = "Running...";

            }

            else

            {

                lblHappyStatus.Text = letters.CanExchange ? "Ready" : "Idle";

                lblHappyStatus.ForeColor = letters.CanExchange ? Color.SeaGreen : Color.Gray;

                btnStartHappy.Text = "Start Exchange";

            }

        }

        catch { }

    }


    private void numHappyDelay_ValueChanged(object sender, EventArgs e)
    {
        PlayerConfig.Set("RSBot.Inventory.HappyDelay", (int)numHappyDelay.Value);
        PlayerConfig.Save();
    }


    private void checkStorageSplitPages_CheckedChanged(object sender, EventArgs e)
    {
        PlayerConfig.Set("RSBot.Storage.SplitByPages", checkStorageSplitPages.Checked);
        PlayerConfig.Save();
    }

    private void checkStorageAutoOrder_CheckedChanged(object sender, EventArgs e)
    {
        PlayerConfig.Set("RSBot.Storage.AutoOrder", checkStorageAutoOrder.Checked);
        PlayerConfig.Save();
        if (_selectedIndex == 4)
            checkAutoOrder.Checked = checkStorageAutoOrder.Checked;
    }

    private void checkStorageAutoSort_CheckedChanged(object sender, EventArgs e)
    {
        PlayerConfig.Set("RSBot.Storage.AutoSort", checkStorageAutoSort.Checked);
        PlayerConfig.Save();
        if (_selectedIndex == 4)
            checkAutoSort.Checked = checkStorageAutoSort.Checked;
    }

    private void checkGuildStorageAutoOrder_CheckedChanged(object sender, EventArgs e)
    {
        PlayerConfig.Set("RSBot.GuildStorage.AutoOrder", checkGuildStorageAutoOrder.Checked);
        PlayerConfig.Save();
        if (_selectedIndex == 5)
            checkAutoOrder.Checked = checkGuildStorageAutoOrder.Checked;
    }

    private void checkGuildStorageAutoSort_CheckedChanged(object sender, EventArgs e)
    {
        PlayerConfig.Set("RSBot.GuildStorage.AutoSort", checkGuildStorageAutoSort.Checked);
        PlayerConfig.Save();
        if (_selectedIndex == 5)
            checkAutoSort.Checked = checkGuildStorageAutoSort.Checked;
    }

    private void btnOrderStorageNow_Click(object sender, EventArgs e)
    {
        var storage = Game.Player?.Storage;
        if (storage == null)
        {
            lblStorageOrderStatus.Text = "Storage is not open! (المستودع غير مفتوح)";
            lblStorageOrderStatus.ForeColor = Color.Red;
            return;
        }

        var npcId = ResolveStorageNpcId(_storageNpcId);
        if (npcId == 0)
        {
            lblStorageOrderStatus.Text = "Storage NPC not detected! (لم يتم رصد حارس المستودع)";
            lblStorageOrderStatus.ForeColor = Color.Red;
            return;
        }

        if (storage.IsOrdering || storage.IsSorting)
        {
            lblStorageOrderStatus.Text = "Storage busy... (جاري تنفيذ عملية أخرى في المستودع)";
            lblStorageOrderStatus.ForeColor = Color.DarkOrange;
            return;
        }

        lblStorageOrderStatus.Text = "Ordering storage by pages... (جاري ترتيب وتقسيم المستودع...)";
        lblStorageOrderStatus.ForeColor = Color.DodgerBlue;

        Task.Run(() =>
        {
            storage.Order(npcId, false);
            try
            {
                Invoke(new System.Action(() =>
                {
                    lblStorageOrderStatus.Text = "Storage ordered successfully! (تم ترتيب المستودع بنجاح)";
                    lblStorageOrderStatus.ForeColor = Color.SeaGreen;
                }));
            }
            catch { }
        });
    }

    private void btnSortStorageNow_Click(object sender, EventArgs e)
    {
        var storage = Game.Player?.Storage;
        if (storage == null)
        {
            lblStorageOrderStatus.Text = "Storage is not open! (المستودع غير مفتوح)";
            lblStorageOrderStatus.ForeColor = Color.Red;
            return;
        }

        var npcId = ResolveStorageNpcId(_storageNpcId);
        if (npcId == 0)
        {
            lblStorageOrderStatus.Text = "Storage NPC not detected! (لم يتم رصد حارس المستودع)";
            lblStorageOrderStatus.ForeColor = Color.Red;
            return;
        }

        if (storage.IsOrdering || storage.IsSorting)
        {
            lblStorageOrderStatus.Text = "Storage busy... (جاري تنفيذ عملية أخرى في المستودع)";
            lblStorageOrderStatus.ForeColor = Color.DarkOrange;
            return;
        }

        lblStorageOrderStatus.Text = "Merging duplicate stacks... (جاري دمج الأكوام المتطابقة...)";
        lblStorageOrderStatus.ForeColor = Color.DodgerBlue;

        Task.Run(() =>
        {
            storage.Sort(npcId, false);
            try
            {
                Invoke(new System.Action(() =>
                {
                    lblStorageOrderStatus.Text = "Stacks merged successfully! (تم دمج الأكوام بنجاح)";
                    lblStorageOrderStatus.ForeColor = Color.SeaGreen;
                }));
            }
            catch { }
        });
    }

        private void btnOrderGuildStorageNow_Click(object sender, EventArgs e)
    {
        var guildStorage = Game.Player?.GuildStorage;
        if (guildStorage == null || guildStorage.Count == 0)
        {
            lblStorageOrderStatus.Text = "Please open guild storage in-game first! (يرجى فتح مستودع الجيلد أولاً)";
            lblStorageOrderStatus.ForeColor = Color.Red;
            return;
        }

        var npcId = ResolveStorageNpcId(_guildStorageNpcId);
        if (_guildStorageNpcId == 0 && npcId != 0)
            _guildStorageNpcId = npcId;

        if (guildStorage.IsOrdering || guildStorage.IsSorting)
        {
            lblStorageOrderStatus.Text = "Guild storage busy... (جاري تنفيذ عملية أخرى...)";
            lblStorageOrderStatus.ForeColor = Color.DarkOrange;
            return;
        }

        lblStorageOrderStatus.Text = "Ordering guild storage... (جاري ترتيب مستودع الجيلد...)";
        lblStorageOrderStatus.ForeColor = Color.DodgerBlue;

        Task.Run(() =>
        {
            guildStorage.Order(npcId, true);
            try
            {
                Invoke(new System.Action(() =>
                {
                    lblStorageOrderStatus.Text = "Guild storage ordered successfully! (تم ترتيب مستودع الجيلد بنجاح)";
                    lblStorageOrderStatus.ForeColor = Color.SeaGreen;
                }));
            }
            catch { }
        });
    }
}
