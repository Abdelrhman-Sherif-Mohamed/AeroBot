using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Network;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Cos;
using RSBot.Core.Objects.Inventory;
using RSBot.Core.Objects.Spawn;
using static RSBot.Core.Game;

namespace RSBot.Core.Components;

public static class ShoppingManager
{
    /// <summary>
    ///     Gets or sets the shopping list.
    /// </summary>
    /// <value>
    ///     The shopping list.
    /// </value>
    public static Dictionary<RefShopGood, int> ShoppingList { get; set; }

    /// <summary>
    ///     Gets a value indicating whether this <see cref="ShoppingManager" /> is finished.
    /// </summary>
    /// <value>
    ///     <c>true</c> if finished; otherwise, <c>false</c>.
    /// </value>
    public static bool Finished { get; private set; }

    /// <summary>
    ///     Gets or sets a value indicating whether this <see cref="ShoppingManager" /> is enabled.
    /// </summary>
    /// <value>
    ///     <c>true</c> if enabled; otherwise, <c>false</c>.
    /// </value>
    public static bool Enabled { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether [repair gear].
    /// </summary>
    /// <value>
    ///     <c>true</c> if [repair gear]; otherwise, <c>false</c>.
    /// </value>
    public static bool RepairGear { get; set; }

    /// <summary>
    ///     Gets or sets the sell filter.
    /// </summary>
    /// <value>
    ///     The armor sell filter.
    /// </value>
    public static List<string> SellFilter { get; set; } = new();

    /// <summary>
    ///     Gets or sets the store filter.
    /// </summary>
    /// <value>
    ///     The store filter.
    /// </value>
    public static List<string> StoreFilter { get; set; } = new();

    /// <summary>
    ///     Gets or sets a value indicating whether this <see cref="ShoppingManager" /> is running.
    /// </summary>
    /// <value>
    ///     <c>true</c> if running; otherwise, <c>false</c>.
    /// </value>
    public static bool Running { get; set; }

    private static SpawnedBionic _selectedEntity;

    /// <summary>
    ///     Gets or sets the currently selected entity/shop for shopping operations.
    /// </summary>
    public static SpawnedBionic SelectedEntity
    {
        get => _selectedEntity ?? Game.SelectedEntity;
        set => _selectedEntity = value;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the NPC dialog was opened by the
    ///     bot (through injected packets) instead of by the game client. The client
    ///     does not know about such a dialog and must not receive the packets that
    ///     belong to it (trade goods purchases, trade scale updates, ...), otherwise
    ///     sro_client.exe crashes with an access violation (0xC0000005).
    /// </summary>
    public static bool BotOpenedDialog { get; set; }

    /// <summary>
    ///     Gets the time of the last job trade operation (buy / sell). Used to keep
    ///     other components (auto sort, etc.) away from the inventory right after a
    ///     trade, while the server is still processing the transaction.
    /// </summary>
    public static DateTime LastTradeOperation { get; private set; } = DateTime.MinValue;

    /// <summary>
    ///     Gets a value indicating whether a job trade operation happened recently.
    /// </summary>
    public static bool IsRecentTradeOperation =>
        (DateTime.Now - LastTradeOperation).TotalSeconds < 5;

    /// <summary>
    ///     Remembers that the bot performed a job trade operation now.
    /// </summary>
    public static void MarkTradeOperation()
    {
        LastTradeOperation = DateTime.Now;
    }

    /// <summary>
    ///     Gets or sets a value indicating whether [sell pet items].
    /// </summary>
    /// <value>
    ///     <c>true</c> if [sell pet items]; otherwise, <c>false</c>.
    /// </value>
    public static bool SellPetItems { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether [store pet items].
    /// </summary>
    /// <value>
    ///     <c>true</c> if [store pet items]; otherwise, <c>false</c>.
    /// </value>
    public static bool StorePetItems { get; set; }

    /// <summary>
    ///     Gets or sets the buyback list.
    /// </summary>
    /// <value>
    ///     The buyback list.
    /// </value>
    internal static Dictionary<byte, InventoryItem> BuybackList { get; set; } = new();

    /// <summary>
    ///     Initializes this instance.
    /// </summary>
    internal static void Initialize()
    {
        ShoppingList = new Dictionary<RefShopGood, int>();
        StoreFilter = new List<string>();
        SellFilter = new List<string>();
        BuybackList = new Dictionary<byte, InventoryItem>();

        Log.Debug("Initialized [ShoppingManager]!");
    }

    /// <summary>
    ///     Runs this instance.
    /// </summary>
    public static void Run(string npcCodeName)
    {
        if (!Enabled)
        {
            Finished = true;
            return;
        }

        Finished = false;
        Running = true;

        SelectNPC(npcCodeName);

        Log.Status("Selling items");

        //Prevent modification during the for-each loop
        var tempItemSellList =
            Game.Player.Inventory.GetNormalPartItems(item => SellFilter.Any(p => p == item.Record.CodeName));

        foreach (var item in tempItemSellList)
            SellItem(item);

        if (Game.Player.HasActiveAbilityPet && SellPetItems)
        {
            tempItemSellList =
                Game.Player.AbilityPet.Inventory.GetItems(item => SellFilter.Any(p => p == item.Record.CodeName));

            foreach (var item in tempItemSellList)
            {
                var playerSlot = Game.Player.AbilityPet.MoveItemToPlayer(item.Slot);
                if (playerSlot != 0xFF)
                    SellItem(Game.Player.Inventory.GetItemAt(playerSlot));
            }
        }

        var shopGroup = ReferenceManager.GetRefShopGroup(npcCodeName);
        if (shopGroup == null)
        {
            Log.Warn("Could not buy anything from this NPC - It's not a shop!");
            CloseShop();

            Finished = true;
            Running = false;

            return;
        }

        var shopGoods = ReferenceManager.GetRefShopGoods(shopGroup);

        foreach (var item in ShoppingList)
        {
            if (!Running)
                return;

            var actualItem =
                shopGoods.FirstOrDefault(x => x.RefPackageItemCodeName == item.Key.RefPackageItemCodeName);

            if (actualItem == null)
                continue;

            var tabIndex = ReferenceManager.GetRefShopGoodTabIndex(npcCodeName, actualItem);
            if (tabIndex == 0xFF) //Specified item not available in this shop!
                continue;

            var refPackageItem = ReferenceManager.GetRefPackageItem(item.Key.RefPackageItemCodeName);

            var holdingAmount = Game.Player.Inventory.GetSumAmount(refPackageItem.RefItemCodeName);
            var totalAmountToBuy = item.Value - holdingAmount;

            var refItem = ReferenceManager.GetRefItem(refPackageItem.RefItemCodeName);
            if (refItem == null)
                continue; //Should not happen

            Log.Status("Buying items");

            while (totalAmountToBuy > 0 && !Game.Player.Inventory.Full)
            {
                var amountStep = totalAmountToBuy;
                if (totalAmountToBuy >= refItem.MaxStack)
                    amountStep = refItem.MaxStack; //Buy only one stack

                PurchaseItem(tabIndex, actualItem.SlotIndex, (ushort)amountStep);
                totalAmountToBuy -= amountStep; //One stack bought, substract from total amount!
                Thread.Sleep(100);
            }

            //merge stacks
            if (refItem.MaxStack > 1)
            {
                IList<InventoryItem> getItems()
                {
                    return Game.Player.Inventory.GetItems(i =>
                        i.Record.CodeName == refPackageItem.RefItemCodeName && i.Amount < refItem.MaxStack);
                }

                var nonFullStacks = getItems();
                while (nonFullStacks.Count >= 2)
                {
                    Game.Player.Inventory.MoveItem(nonFullStacks[1].Slot, nonFullStacks[0].Slot,
                        (ushort)Math.Min(refItem.MaxStack - nonFullStacks[0].Amount, nonFullStacks[1].Amount));
                    nonFullStacks = getItems();
                    Thread.Sleep(100);
                }
            }
        }

        CloseShop();

        Finished = true;
        Running = false;
    }

    /// <summary>
    ///     Sells the item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="cos"></param>
    public static bool SellItem(InventoryItem item, SpawnedBionic cos = null, int timeoutMs = 4000)
    {
        if (SelectedEntity == null || item == null)
            return false;

        if (cos != null)
            MarkTradeOperation();

        var packet = new Packet(0x7034);
        packet.WriteByte(cos == null ? InventoryOperation.SP_SELL_ITEM : InventoryOperation.SP_SELL_ITEM_COS);

        if (cos != null)
            packet.WriteUInt(cos.UniqueId);

        packet.WriteByte(item.Slot);
        packet.WriteUShort(item.Amount);
        packet.WriteUInt(SelectedEntity.UniqueId);

        var awaitResult = new AwaitCallback(response =>
        {
            if (response.ReadByte() != 0x01)
                return AwaitCallbackResult.Fail;

            var operation = (InventoryOperation)response.ReadByte();
            if (operation == InventoryOperation.SP_SELL_ITEM ||
                operation == InventoryOperation.SP_SELL_ITEM_COS)
                return AwaitCallbackResult.Success;

            // Some other inventory operation response, keep waiting.
            return AwaitCallbackResult.ConditionFailed;
        }, 0xB034);

        PacketManager.SendPacket(packet, PacketDestination.Server, awaitResult);
        awaitResult.AwaitResponse(timeoutMs);

        if (awaitResult.IsCompleted)
            Log.Debug("[Shopping manager] - Sold item: " + item.Record.GetRealName());
        else
            Log.Warn("[Shopping manager] - Failed to sell item: " + item.Record.GetRealName());

        return awaitResult.IsCompleted;
    }

    /// <summary>
    ///     Purchases the item.
    /// </summary>
    /// <param name="tab">The tab.</param>
    /// <param name="slot">The slot.</param>
    /// <param name="amount">The amount.</param>
    public static void PurchaseItem(int tab, int slot, ushort amount)
    {
        if (SelectedEntity == null)
        {
            Log.Debug("Cannot buy items, because no shop is selected!");
            return;
        }

        var packet = new Packet(0x7034);
        packet.WriteByte(InventoryOperation.SP_BUY_ITEM); //Buy item flag
        packet.WriteByte(tab);
        packet.WriteByte(slot);
        packet.WriteUShort(amount);
        packet.WriteUInt(SelectedEntity.UniqueId);

        var awaitResult = new AwaitCallback(null, 0xB034);
        PacketManager.SendPacket(packet, PacketDestination.Server, awaitResult);
        awaitResult.AwaitResponse();
    }

    /// <summary>
    ///     Purchases the item to the given transport.
    /// </summary>
    /// <param name="transport"></param>
    /// <param name="tab"></param>
    /// <param name="slot"></param>
    /// <param name="amount"></param>
    /// <returns><c>true</c> when the server confirmed the purchase.</returns>
    public static bool PurchaseItem(Cos transport, int tab, int slot, ushort amount)
    {
        var shopEntity = SelectedEntity ?? Game.SelectedEntity ?? Game.Player?.State?.DialogState?.Npc;
        if (shopEntity == null)
        {
            Log.Warn("[Shopping manager] Cannot buy items, because no shop is selected!");
            return false;
        }

        if (transport == null)
            return false;

        MarkTradeOperation();

        bool SendBuyPacket(uint cosId)
        {
            var packet = new Packet(0x7034);
            packet.WriteByte(InventoryOperation.SP_BUY_ITEM_COS);
            packet.WriteUInt(cosId);
            packet.WriteByte(tab);
            packet.WriteByte(slot);
            packet.WriteUShort(amount);
            packet.WriteUInt(shopEntity.UniqueId);

            var awaitResult = new AwaitCallback(response =>
            {
                var res = response.ReadByte();
                if (res != 0x01)
                {
                    var errCode = response.Remaining > 0 ? response.ReadByte() : (byte)0;
                    Log.Debug($"[Trade Purchase] 0xB034 rejected (cosId={cosId}): result=0x{res:X2}, errorCode=0x{errCode:X2}");
                    return AwaitCallbackResult.Fail;
                }

                var operation = (InventoryOperation)response.ReadByte();
                if (operation == InventoryOperation.SP_BUY_ITEM ||
                    operation == InventoryOperation.SP_BUY_ITEM_COS)
                    return AwaitCallbackResult.Success;

                return AwaitCallbackResult.ConditionFailed;
            }, 0xB034);

            PacketManager.SendPacket(packet, PacketDestination.Server, awaitResult);
            awaitResult.AwaitResponse(1500);

            return awaitResult.IsCompleted;
        }

        // Try standard Silkroad client payload first (cosId = 0)
        if (SendBuyPacket(0))
            return true;

        // Fallback: try with transport.UniqueId if cosId=0 was rejected
        if (transport.UniqueId != 0 && SendBuyPacket(transport.UniqueId))
            return true;

        return false;
    }

    /// <summary>
    ///     Repairs the items.
    /// </summary>
    public static void RepairItems(string npcCodeName)
    {
        if (!RepairGear)
            return;

        SelectNPC(npcCodeName);

        if (SelectedEntity == null)
        {
            Log.Debug("Cannot repair items because there is no smith selected!");
            return;
        }

        var packet = new Packet(0x703E);
        packet.WriteUInt(SelectedEntity.UniqueId);
        packet.WriteByte(2); //repair all items

        var awaitCallback = new AwaitCallback(response =>
        {
            var result = packet.ReadByte();

            if (result == 2)
            {
                var errorCode = response.ReadUShort();

                Log.Debug($"Repair of items at NPC {npcCodeName} failed [code={errorCode}]");

                return AwaitCallbackResult.Fail;
            }

            return AwaitCallbackResult.Success;
        }, 0xB03E);

        PacketManager.SendPacket(packet, PacketDestination.Server, awaitCallback);
        awaitCallback.AwaitResponse();

        CloseShop();
    }

    /// <summary>
    ///     Stores the items.
    /// </summary>
    /// <param name="npcCodeName">Name of the NPC code.</param>
    public static void StoreItems(string npcCodeName)
    {
        var tempInventory =
            Game.Player.Inventory.GetItems(item => item.Slot > 13 && StoreFilter.Any(p => p == item.Record.CodeName));

        SelectNPC(npcCodeName);
        var npc = SelectedEntity;
        if (npc == null)
        {
            Log.Debug("Cannot store items because there is no storage NPC selected!");
            return;
        }

        OpenStorage(npc.UniqueId);

        if (Game.Player.Storage == null)
            return;

        Log.Status("Storing items");
        foreach (var item in tempInventory) StoreItem(item, npc);

        if (Game.Player.HasActiveAbilityPet && StorePetItems)
        {
            var petItemStoreList =
                Game.Player.AbilityPet.Inventory.GetItems(item => StoreFilter.Any(p => p == item.Record.CodeName));

            foreach (var item in petItemStoreList)
            {
                var playerSlot = Game.Player.AbilityPet.MoveItemToPlayer(item.Slot);
                if (playerSlot != 0xFF)
                {
                    var movedItem = Game.Player.Inventory.GetItemAt(playerSlot);
                    StoreItem(movedItem, npc);
                }
            }
        }

        CloseShop();
    }

    /// <summary>
    ///     Closes the shop.
    /// </summary>
    public static void CloseShop()
    {
        try
        {
            if (SelectedEntity != null && SelectedEntity.TryDeselect())
            {
                SelectedEntity = null;
                Game.SelectedEntity = null;
            }
        }
        finally
        {
            _selectedEntity = null;
            Running = false;
            BotOpenedDialog = false;
        }
    }

    /// <summary>
    ///     Opens the storage.
    /// </summary>
    private static void OpenStorage(uint uniqueId)
    {
        if (Game.Player.Storage != null)
            return;

        var packet = new Packet(0x703C);
        packet.WriteInt(uniqueId);
        packet.WriteByte(0);

        var awaitResult = new AwaitCallback(null, 0x3049);
        PacketManager.SendPacket(packet, PacketDestination.Server, awaitResult);
        awaitResult.AwaitResponse();

        packet = new Packet(0x7046);
        packet.WriteUInt(uniqueId);
        packet.WriteUInt(0x04);

        awaitResult = new AwaitCallback(null, 0xB046);

        PacketManager.SendPacket(packet, PacketDestination.Server, awaitResult);
        awaitResult.AwaitResponse();
    }

    /// <summary>
    ///     Opens the shop.
    /// </summary>
    /// <param name="npcCodeName">Name of the NPC code.</param>
    public static void SelectNPC(string npcCodeName)
    {
        if (SelectedEntity != null && string.Equals(SelectedEntity.Record?.CodeName, npcCodeName, StringComparison.OrdinalIgnoreCase))
            return;

        if (!SpawnManager.TryGetEntity<SpawnedNpcNpc>(p => p.Record != null && string.Equals(p.Record.CodeName, npcCodeName, StringComparison.OrdinalIgnoreCase), out var entity))
        {
            if (!SpawnManager.TryGetEntity<SpawnedNpcNpc>(p => p.Record != null && (p.Record.CodeName.Contains("SPECIAL", StringComparison.OrdinalIgnoreCase) || p.Record.CodeName.Contains("SMUGGLER", StringComparison.OrdinalIgnoreCase)), out entity))
            {
                Log.Warn("Cannot access the NPC [" + npcCodeName + "] because it does not exist nearby.");
                return;
            }
        }

        entity.TrySelect();
    }

    /// <summary>
    ///     Stores an item.
    /// </summary>
    /// <param name="item">Item to put in storage.</param>
    private static void StoreItem(InventoryItem item, SpawnedBionic npc)
    {
        //Use later to merge item!
        var existingItem = Game.Player.Storage.GetItem(item.Record.ID);

        //Store item
        var destinationSlot = Game.Player.Storage.GetFreeSlot();
        var packet = new Packet(0x7034);
        packet.WriteByte(0x02); //Store Item Flag
        packet.WriteByte(item.Slot);
        packet.WriteByte(destinationSlot);
        packet.WriteUInt(npc.UniqueId);

        var awaitResult = new AwaitCallback(null, 0xB034);
        PacketManager.SendPacket(packet, PacketDestination.Server, awaitResult);
        awaitResult.AwaitResponse();

        if (item.Record.MaxStack <= 1 || existingItem == null)
            return;

        //Merge with existing item.
        var mergeAmount = existingItem.Amount + item.Amount <= existingItem.Record.MaxStack
            ? item.Amount
            : existingItem.Record.MaxStack - existingItem.Amount;

        var mergePacket = new Packet(0x7034);
        mergePacket.WriteByte(0x01); //Store Item Flag
        mergePacket.WriteByte(destinationSlot);
        mergePacket.WriteByte(existingItem.Slot);
        mergePacket.WriteUShort(mergeAmount);
        mergePacket.WriteUInt(npc.UniqueId);

        awaitResult = new AwaitCallback(null, 0xB034);
        PacketManager.SendPacket(mergePacket, PacketDestination.Server, awaitResult);
        awaitResult.AwaitResponse();
    }

    public static void LoadFilters()
    {
        SellFilter ??= new List<string>();
        StoreFilter ??= new List<string>();

        var configSell = PlayerConfig.GetArray<string>("RSBot.Shopping.Sell");
        var configStore = PlayerConfig.GetArray<string>("RSBot.Shopping.Store");

        if (configSell != null)
        {
            foreach (var item in configSell)
            {
                if (!string.IsNullOrWhiteSpace(item) && !SellFilter.Contains(item))
                    SellFilter.Add(item);
            }
        }

        if (configStore != null)
        {
            foreach (var item in configStore)
            {
                if (!string.IsNullOrWhiteSpace(item) && !StoreFilter.Contains(item))
                    StoreFilter.Add(item);
            }
        }
    }

    public static void SaveFilters()
    {
        PlayerConfig.SetArray("RSBot.Shopping.Sell", SellFilter);
        PlayerConfig.SetArray("RSBot.Shopping.Store", StoreFilter);
    }

    /// <summary>
    ///     Selects the given NPC and chooses the specified dialog option.
    /// </summary>
    /// <param name="npcCodeName"></param>
    /// <param name="option"></param>
    public static bool ChooseTalkOption(string npcCodeName, TalkOption option)
    {
        // Mark as bot-driven dialog before sending any select or talk packets
        // so sro_client never receives NPC popups or uninitialized UI responses.
        BotOpenedDialog = true;

        if (!SpawnManager.TryGetEntity<SpawnedNpcNpc>(p => p.Record != null && string.Equals(p.Record.CodeName, npcCodeName, StringComparison.OrdinalIgnoreCase), out var entity))
        {
            if (!SpawnManager.TryGetEntity<SpawnedNpcNpc>(p => p.Record != null && (p.Record.CodeName.Contains("SPECIAL", StringComparison.OrdinalIgnoreCase) || p.Record.CodeName.Contains("SMUGGLER", StringComparison.OrdinalIgnoreCase)), out entity))
            {
                Log.Debug("Cannot access the NPC [" + npcCodeName + "] because it does not exist nearby.");
                BotOpenedDialog = false;
                return false;
            }
        }

        // Make sure the NPC selection was acknowledged by the server before
        // sending the talk option. Sending 0x7046 too early makes the server
        // drop the connection.
        if (Game.SelectedEntity?.UniqueId != entity.UniqueId || SelectedEntity?.UniqueId != entity.UniqueId)
        {
            PacketManager.ClearCallbacks(0xB045);

            var selectPacket = new Packet(0x7045);
            selectPacket.WriteUInt(entity.UniqueId);

            var selectAwait = new AwaitCallback(response =>
                response.ReadByte() == 0x01
                    ? AwaitCallbackResult.Success
                    : AwaitCallbackResult.Fail, 0xB045);

            Game.SelectedEntity = entity;
            SelectedEntity = entity;
            PacketManager.SendPacket(selectPacket, PacketDestination.Server, selectAwait);
            selectAwait.AwaitResponse(1000);

            if (!selectAwait.IsCompleted)
            {
                Log.Debug($"[Shopping manager] NPC [{npcCodeName}] selection was not acknowledged, continuing anyway.");
                Thread.Sleep(150);
            }
        }

        // From this point on the dialog was opened by the bot, the game client has
        // no idea about it. See BotOpenedDialog for details.
        BotOpenedDialog = true;
        SelectedEntity = entity;
        Game.SelectedEntity = entity;

        if (Game.Player?.State != null)
        {
            Game.Player.State.DialogState = new DialogState
            {
                RequestedNpcId = entity.UniqueId,
                TalkOption = option,
                Npc = entity
            };
        }

        var packet = new Packet(0x7046);
        packet.WriteUInt(entity.UniqueId);
        packet.WriteByte(option);

        var awaitResult = new AwaitCallback(response =>
        {
            return response.ReadByte() == 0x01 && response.ReadByte() == (byte)option
                ? AwaitCallbackResult.Success
                : AwaitCallbackResult.ConditionFailed;
        }, 0xB046);
        PacketManager.SendPacket(packet, PacketDestination.Server, awaitResult);
        awaitResult.AwaitResponse(1500);

        return awaitResult.IsCompleted;
    }

    /// <summary>
    ///     Stops this instance.
    /// </summary>
    public static void Stop()
    {
        Running = false;
        Finished = true;
    }
}