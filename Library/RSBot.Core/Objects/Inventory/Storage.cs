using RSBot.Core.Network;

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
    ///     Create instance of the <seealso cref="Storage" />
    /// </summary>
    /// <param name="size">The standart 150(5 page)</param>
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
        if (IsSorting || Game.Player == null || Game.Player.InAction)
            return;

        IsSorting = true;
        Log.Debug(isGuildStorage ? "Sorting the guild storage..." : "Sorting the storage...");

        var iterations = SortStackableItems(
            (sourceSlot, destinationSlot, amount) =>
                MoveItem(sourceSlot, destinationSlot, amount, npcUniqueId, isGuildStorage));

        IsSorting = false;
        Log.Debug($"Sorting finished after {iterations}/10");
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
            if (response.ReadByte() != 0x01)
                return AwaitCallbackResult.Fail;

            if (response.ReadByte() != (byte)operation)
                return AwaitCallbackResult.ConditionFailed;

            var source = response.ReadByte();
            var destination = response.ReadByte();

            return source == sourceSlot && destination == destinationSlot
                ? AwaitCallbackResult.Success
                : AwaitCallbackResult.ConditionFailed;
        }, 0xB034);

        PacketManager.SendPacket(packet, PacketDestination.Server, asyncResult);
        asyncResult.AwaitResponse(500);

        return asyncResult.IsCompleted;
    }
}
