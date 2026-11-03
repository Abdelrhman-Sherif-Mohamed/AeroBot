using System;
using System.Collections.Generic;
using System.Linq;
using RSBot.Core.Network;

namespace RSBot.Core.Objects;

/// <summary>
///     The Character's Invetory with EquippedPart and NormalPart.
/// </summary>
public class CharacterInventory : InventoryItemCollection
{
    /// <summary>
    ///     Minimum slot of NormalPart.
    /// </summary>
    public const byte NORMAL_PART_MIN_SLOT = 13;

    /// <summary>
    ///     The constructor.
    /// </summary>
    /// <param name="size">The size.</param>
    public CharacterInventory(Packet packet)
        : base(packet)
    {
    }

    /// <summary>
    ///     Gets the size of NormalPart.
    /// </summary>
    public byte NormalPartSize => (byte)(Capacity - NORMAL_PART_MIN_SLOT);

    /// <summary>
    ///     Gets a value indicating whether the NormalPart is full.
    /// </summary>
    /// <value>
    ///     <c>true</c> if the NormalPart is full; otherwise, <c>false</c>.
    /// </value>
    public override bool Full => GetNormalPartItems().Count >= NormalPartSize;

    /// <summary>
    ///     Gets a value indicating whether this instance is sorting.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance is sorting; otherwise, <c>false</c>.
    /// </value>
    public bool IsSorting { get; private set; }

    /// <summary>
    ///     Gets the first free slot number inside NormalPart.
    /// </summary>
    /// <returns>if found: the first free slot number; otherwise: 0</returns>
    public override byte GetFreeSlot()
    {
        for (var slot = NORMAL_PART_MIN_SLOT; slot < Capacity; slot++)
            if (GetItemAt(slot) == null)
                return slot;

        return 0;
    }

    /// <summary>
    ///     Gets items of EquippedPart, ordered by slot.
    /// </summary>
    /// <returns>If found: list of item(s), ordered by slot; otherwise empty list</returns>
    public ICollection<InventoryItem> GetEquippedPartItems()
    {
        return GetItems(item => item.Slot < NORMAL_PART_MIN_SLOT);
    }

    /// <summary>
    ///     Gets items of NormalPart, ordered by slot.
    /// </summary>
    /// <returns>If found: list of item(s), ordered by slot; otherwise empty list</returns>
    public ICollection<InventoryItem> GetNormalPartItems()
    {
        return GetItems(item => item.Slot >= NORMAL_PART_MIN_SLOT);
    }

    /// <summary>
    ///     Gets items of NormalPart, ordered by slot.
    /// </summary>
    /// <returns>If found: list of item(s), ordered by slot; otherwise empty list</returns>
    public ICollection<InventoryItem> GetNormalPartItems(Predicate<InventoryItem> predicate)
    {
        return GetItems(item => item.Slot >= NORMAL_PART_MIN_SLOT && predicate(item));
    }

    /// <summary>
    ///     Gets items of NormalPart by ItemId, ordered by slot.
    /// </summary>
    /// <param name="itemId">The identifier of item.</param>
    /// <returns>If found: list of item(s), ordered by slot; otherwise empty list</returns>
    public ICollection<InventoryItem> GetNormalPartItems(uint itemId)
    {
        return GetItems(item => item.Slot >= NORMAL_PART_MIN_SLOT && item.ItemId == itemId);
    }

    /// <summary>
    ///     Moves the item inside Character's Inventory.
    /// </summary>
    /// <param name="sourceSlot">The source slot.</param>
    /// <param name="destinationSlot">The destination slot.</param>
    /// <param name="amount">The amount.</param>
    /// <returns><c>true</c> if successfully moved; otherwise, <c>false</c>.</returns>
    public bool MoveItem(byte sourceSlot, byte destinationSlot, ushort amount = 0)
    {
        var itemAtSource = GetItemAt(sourceSlot);
        if (itemAtSource == null)
            return false;

        if (amount == 0)
            amount = itemAtSource.Amount;

        var packet = new Packet(0x7034);
        packet.WriteByte(0); //kinda flag
        packet.WriteByte(sourceSlot);
        packet.WriteByte(destinationSlot);
        packet.WriteUShort(amount);

        var asyncResult = new AwaitCallback(response =>
        {
            var result = response.ReadByte();
            if (result == 0x01)
            {
                var operation = response.ReadByte();
                if (operation != 0)
                    return AwaitCallbackResult.ConditionFailed;

                var source = response.ReadByte();
                var destination = response.ReadByte();
                if (source == sourceSlot && destination == destinationSlot)
                    return AwaitCallbackResult.Success;
            }

            return AwaitCallbackResult.Fail;
        }, 0xB034);

        PacketManager.SendPacket(packet, PacketDestination.Server, asyncResult);
        asyncResult.AwaitResponse(500);

        return asyncResult.IsCompleted;
    }

    /// <summary>
    ///     Gets a value indicating whether the inventory is currently being ordered.
    /// </summary>
    public bool IsOrdering { get; private set; }

    public void Sort()
    {
        if (IsSorting || IsOrdering || Game.Player == null || Game.Player.InAction)
            return;

        IsSorting = true;
        Log.Debug("Sorting the character inventory...");

        var iterations = SortStackableItems(MoveItem, NORMAL_PART_MIN_SLOT);

        IsSorting = false;
        Log.Debug($"Sorting finished after {iterations}/10");
    }

    /// <summary>
    ///     Orders the character inventory by grouping items by category
    ///     and placing them sequentially starting from slot 13 (NORMAL_PART_MIN_SLOT).
    /// </summary>
    public void Order()
    {
        if (IsOrdering || IsSorting || Game.Player == null || Game.Player.InAction)
            return;

        IsOrdering = true;
        Log.Debug("Ordering the character inventory...");

        var moves = OrderItems(MoveItem, NORMAL_PART_MIN_SLOT);

        IsOrdering = false;
        Log.Debug($"Inventory ordering finished after {moves} move operations.");
    }
}