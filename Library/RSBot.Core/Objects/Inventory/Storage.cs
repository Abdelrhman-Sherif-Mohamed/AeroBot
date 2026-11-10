using System;
using System.Linq;
using System.Threading;
using RSBot.Core.Components;
using RSBot.Core.Network;
using RSBot.Core.Objects.Spawn;

namespace RSBot.Core.Objects.Inventory;

public class Storage : InventoryItemCollection
{
    /// <summary>
    ///     The gold amount in the storage
    /// </summary>
    public ulong Gold;

    /// <summary>
    ///     Gets a value indicating whether this storage is currently being sorted.
    /// </summary>
    public bool IsSorting { get; private set; }

    /// <summary>
    ///     Gets a value indicating whether this storage is currently being ordered.
    /// </summary>
    public bool IsOrdering { get; private set; }

    /// <summary>
    ///     Create instance of the <seealso cref="Storage" />
    /// </summary>
    /// <param name="size">The standard 150 (5 pages)</param>
    public Storage(byte size = 150)
        : base(size)
    {
    }

    /// <summary>
    ///     Sorts the storage by merging partial stacks of stackable items.
    /// </summary>
    /// <param name="npcUniqueId">The unique id of the storage NPC.</param>
    /// <param name="isGuildStorage">Whether this is the guild storage.</param>
    public void Sort(uint npcUniqueId, bool isGuildStorage)
    {
        if (IsSorting || IsOrdering || Game.Player == null || Game.Player.InAction)
            return;

        IsSorting = true;
        Log.Debug(isGuildStorage ? "Sorting the guild storage..." : "Sorting the storage...");

        var iterations = SortStackableItems(
            (sourceSlot, destinationSlot, amount) =>
                MoveItem(sourceSlot, destinationSlot, amount, npcUniqueId, isGuildStorage));

        IsSorting = false;
        Log.Debug($"Sorting finished after {iterations} iteration(s).");
    }

    /// <summary>
    ///     Orders the storage by arranging items sequentially into slots starting from 0,
    ///     grouped logically by category (Weapons, Shields, Armor, Accessories, Potions, Alchemy, etc.).
    /// </summary>
    /// <param name="npcUniqueId">The unique id of the storage NPC.</param>
    /// <param name="isGuildStorage">Whether this is the guild storage.</param>
    public void Order(uint npcUniqueId, bool isGuildStorage)
    {
        if (IsOrdering || IsSorting || Game.Player == null || Game.Player.InAction)
            return;

        IsOrdering = true;
        Log.Notify(isGuildStorage ? $"Ordering guild storage ({Count} items, capacity {Capacity})..." : $"Ordering storage ({Count} items, capacity {Capacity})...");

        var splitByPages = isGuildStorage
            ? PlayerConfig.Get("RSBot.GuildStorage.SplitByPages", PlayerConfig.Get("RSBot.Storage.SplitByPages", true))
            : PlayerConfig.Get("RSBot.Storage.SplitByPages", true);

        var moves = OrderStorageItems(
            (sourceSlot, destinationSlot, amount) =>
                MoveItem(sourceSlot, destinationSlot, amount, npcUniqueId, isGuildStorage),
            splitByPages);

        IsOrdering = false;
        Log.Notify(isGuildStorage ? $"Guild storage ordering finished! Moved {moves} item(s)." : $"Storage ordering finished! Moved {moves} item(s).");
    }

    /// <summary>
    ///     Moves an amount of an item from one slot to another inside this storage.
    /// </summary>
    /// <param name="sourceSlot">The source slot.</param>
    /// <param name="destinationSlot">The destination slot.</param>
    /// <param name="amount">The amount to move.</param>
    /// <param name="npcUniqueId">The unique id of the storage NPC.</param>
    /// <param name="isGuildStorage">Whether this is the guild storage.</param>
    /// <returns><c>true</c> if the item was moved successfully; otherwise <c>false</c>.</returns>
    private bool MoveItem(byte sourceSlot, byte destinationSlot, ushort amount, uint npcUniqueId,
        bool isGuildStorage)
    {
        var itemAtSource = GetItemAt(sourceSlot);
        if (itemAtSource == null)
            return false;

        if (amount == 0)
            amount = itemAtSource.Amount;

        // Auto-resolve NPC ID if 0
        if (npcUniqueId == 0)
        {
            if (Game.Player?.State?.DialogState?.RequestedNpcId > 0)
                npcUniqueId = Game.Player.State.DialogState.RequestedNpcId;
            else if (Game.Player?.State?.DialogState?.Npc != null)
                npcUniqueId = Game.Player.State.DialogState.Npc.UniqueId;
            else if (Game.SelectedEntity != null)
                npcUniqueId = Game.SelectedEntity.UniqueId;
            else if (SpawnManager.TryGetEntities<SpawnedNpc>(out var npcs))
            {
                var nearestNpc = npcs
                    .Where(n => n.DistanceToPlayer <= 35f)
                    .OrderBy(n => n.DistanceToPlayer)
                    .FirstOrDefault();

                if (nearestNpc != null)
                    npcUniqueId = nearestNpc.UniqueId;
            }
        }

        var operation = isGuildStorage
            ? InventoryOperation.SP_GUILD_CHEST_UPDATE_SLOT
            : InventoryOperation.SP_UPDATE_SLOTS_CHEST;

        var packet = new Packet(0x7034);
        packet.WriteByte(operation);
        packet.WriteByte(sourceSlot);
        packet.WriteByte(destinationSlot);
        packet.WriteUShort(amount);
        packet.WriteUInt(npcUniqueId);

        var asyncResult = new AwaitCallback(response =>
        {
            var res = response.ReadByte();
            if (res != 0x01)
            {
                var errCode = response.Remaining > 0 ? response.ReadByte() : (byte)0;
                Log.Debug($"[Storage::MoveItem] Server error 0xB034: result=0x{res:X2}, errCode=0x{errCode:X2}, op={operation}");
                return AwaitCallbackResult.Fail;
            }

            var op = response.ReadByte();
            if (op != (byte)operation &&
                op != (byte)InventoryOperation.SP_UPDATE_SLOTS_CHEST &&
                op != (byte)InventoryOperation.SP_GUILD_CHEST_UPDATE_SLOT)
            {
                return AwaitCallbackResult.ConditionFailed;
            }

            var source = response.ReadByte();
            var destination = response.ReadByte();

            return (source == sourceSlot && destination == destinationSlot) || (source == destinationSlot && destination == sourceSlot)
                ? AwaitCallbackResult.Success
                : AwaitCallbackResult.ConditionFailed;
        }, 0xB034);

        PacketManager.SendPacket(packet, PacketDestination.Server, asyncResult);
        asyncResult.AwaitResponse(2500);

        if (asyncResult.IsCompleted)
        {
            Thread.Sleep(120);
            return true;
        }

        Log.Debug($"[Storage::MoveItem] Move timed out or failed (slot {sourceSlot} -> {destinationSlot}, amount={amount}, npc={npcUniqueId}, guild={isGuildStorage})");
        return false;
    }
}
