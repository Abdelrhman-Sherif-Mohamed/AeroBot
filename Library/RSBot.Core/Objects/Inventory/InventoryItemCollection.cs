using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Network;

namespace RSBot.Core.Objects;

public class InventoryItemCollection : ICollection<InventoryItem>
{
    /// <summary>
    ///     The list of items.
    /// </summary>
    protected List<InventoryItem> _collection;

    /// <summary>
    ///     The constructor.
    /// </summary>
    /// <param name="size">The size.</param>
    public InventoryItemCollection(byte size)
    {
        _collection = new List<InventoryItem>(size + 1);
    }

    /// <summary>
    ///     The constructor.
    /// </summary>
    /// <param name="size">The size.</param>
    public InventoryItemCollection(Packet packet)
    {
        _collection = new List<InventoryItem>();
        Deserialize(packet);
    }

    /// <summary>
    ///     Gets the number of free slots in this inventory.
    /// </summary>
    public byte FreeSlots => (byte)(Capacity - Count);

    /// <summary>
    ///     Gets or sets the size.
    /// </summary>
    /// <value>
    ///     The size.
    /// </value>
    public byte Capacity
    {
        get => (byte)(_collection.Capacity - 1);
        set => _collection.Capacity = value + 1;
    }

    /// <summary>
    ///     Gets a value indicating whether this is full.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this is full; otherwise, <c>false</c>.
    /// </value>
    public virtual bool Full => Count >= Capacity;

    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    /// <param name="index">The index of the element</param>
    public InventoryItem this[int index]
    {
        get => _collection[index];
        set => _collection[index] = value;
    }

    /// <summary>
    ///     The Count of Items.
    /// </summary>
    public int Count => _collection.Count;

    /// <summary>
    ///     Gets a value indicating whether this is readonly.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this is readonly; otherwise, <c>false</c>.
    /// </value>
    public bool IsReadOnly => false;

    /// <summary>
    ///     Adds new item.
    /// </summary>
    /// <param name="newItem">The new item.</param>
    public void Add(InventoryItem newItem)
    {
        _collection.Add(newItem);
    }

    /// <summary>
    ///     Removes the item.
    /// </summary>
    /// <param name="item">The Inventory Item.</param>
    public bool Remove(InventoryItem item)
    {
        return _collection.Remove(item);
    }

    /// <summary>
    ///     Determines whether [contains] the item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>if contains: true; otherwise: false</returns>
    public bool Contains(InventoryItem item)
    {
        return _collection.Contains(item);
    }

    /// <summary>
    ///     Copy item to the array
    /// </summary>
    /// <param name="array">The array</param>
    /// <param name="arrayIndex">The start index</param>
    public void CopyTo(InventoryItem[] array, int arrayIndex)
    {
        _collection.CopyTo(array, arrayIndex);
    }

    /// <summary>
    ///     Clear the collection
    /// </summary>
    public void Clear()
    {
        _collection.Clear();
    }

    /// <summary>
    ///     Get enumerator of this collection
    /// </summary>
    /// <returns>The enumerator</returns>
    public IEnumerator<InventoryItem> GetEnumerator()
    {
        //OrderBySlot();
        return _collection.GetEnumerator();
    }

    /// <summary>
    ///     Get enumerator of this collection
    /// </summary>
    /// <returns>The enumerator</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        //OrderBySlot();
        return _collection.GetEnumerator();
    }

    /// <summary>
    ///     Removes the item at the slot.
    /// </summary>
    /// <param name="slot">The slot.</param>
    public void RemoveAt(byte slot)
    {
        _collection.RemoveAll(p => p.Slot == slot);
    }

    /// <summary>
    ///     Gets the item at the slot.
    /// </summary>
    /// <param name="slot">The slot.</param>
    /// <returns>if found: the item at the slot; otherwise: null</returns>
    public InventoryItem GetItemAt(byte slot)
    {
        return GetItem(item => item?.Slot == slot);
    }

    /// <summary>
    ///     Determines whether [has item at] the slot.
    /// </summary>
    /// <param name="slot">The slot.</param>
    /// <returns>if has item at the slot: true; otherwise: false</returns>
    public bool Contains(byte slot)
    {
        return GetItem(item => item.Slot == slot) != null;
    }

    /// <summary>
    ///     Updates the item slot by slot.
    /// </summary>
    /// <param name="slot">The slot of the item.</param>
    /// <param name="newSlot">The new slot.</param>
    public void UpdateItemSlot(byte slot, byte newSlot)
    {
        if (GetItemAt(slot) is InventoryItem itemToUpdate)
            itemToUpdate.Slot = newSlot;
    }

    /// <summary>
    ///     Updates the item amount by slot.
    /// </summary>
    /// <param name="slot">The slot of the item.</param>
    /// <param name="newAmount">The new amount.</param>
    public void UpdateItemAmount(byte slot, ushort newAmount)
    {
        if (newAmount <= 0)
        {
            RemoveAt(slot);
            return;
        }

        if (GetItemAt(slot) is InventoryItem itemToUpdate)
            itemToUpdate.Amount = newAmount;
    }

    /// <summary>
    ///     Orders the items by slot.
    /// </summary>
    protected void OrderBySlot()
    {
        _collection.Sort((a, b) => a.Slot.CompareTo(b.Slot));
    }

    /// <summary>
    ///     Merges partial stacks of stackable items inside this collection.
    /// </summary>
    /// <param name="moveItem">Callback used to move an amount from a source slot to a destination slot.</param>
    /// <param name="minimumSlot">The minimum slot to include in the sort operation.</param>
    /// <param name="maxIterations">The maximum number of iterations, used to avoid deadlocks.</param>
    /// <returns>The number of iterations that were performed.</returns>
    protected int SortStackableItems(Func<byte, byte, ushort, bool> moveItem, byte minimumSlot = 0,
        int maxIterations = 10)
    {
        var iterations = 0;

        //Ignore items which move operations failed in the next iteration
        var blacklistedItems = new List<uint>(4);

        for (var iIteration = 0; iIteration < maxIterations; iIteration++)
        {
            iterations++;

            var itemsToStackGroups = this.Where(i =>
                    i.Slot >= minimumSlot && i.Record.IsStackable && i.Record.MaxStack > i.Amount &&
                    !blacklistedItems.Contains(i.ItemId))
                .GroupBy(i => i.ItemId);

            if (!itemsToStackGroups.Any())
                break;

            var itemsToStack = itemsToStackGroups.FirstOrDefault(g => g.Count() >= 2)
                ?.OrderBy(i => i.Slot)
                .ToList();

            if (itemsToStack == null)
                break;

            var source = itemsToStack.FirstOrDefault();
            if (source == null)
                continue;

            var destination = itemsToStack.FirstOrDefault(i => i.Record.ID == source.ItemId && i.Slot != source.Slot);
            if (destination == null)
                continue;

            var amount = destination.Record.MaxStack - destination.Amount;
            var actualAmount = source.Amount > amount ? amount : source.Amount;

            if (!moveItem(source.Slot, destination.Slot, (ushort)actualAmount))
                blacklistedItems.Add(source.ItemId);
        }

        return iterations;
    }

    /// <summary>
    ///     Gets the category priority score for sorting items logically.
    ///     Lower values appear first.
    /// </summary>
    protected static int GetItemCategory(RefObjItem record)
    {
        if (record == null)
            return 999;

        var code = record.CodeName ?? string.Empty;

        // 1. Equipment - Weapons
        if (record.IsWeapon) return 10;
        // 2. Equipment - Shields
        if (record.IsShield) return 20;
        // 3. Equipment - Armor
        if (record.IsArmor) return 30;
        // 4. Equipment - Accessories
        if (record.IsAccessory) return 40;
        // 5. Avatars & Devil Spirit
        if (record.IsAvatar) return 50;
        // 6. Job Equipment & Outfits
        if (record.IsJobEquip || record.IsJobOutfit) return 60;
        // 7. Fellow Equipment
        if (record.IsFellowEquip) return 70;
        // 8. Other Wearable
        if (record.IsEquip) return 80;

        // 9. Recovery Potions & Pills
        if (record.IsHpPotion) return 90;
        if (record.IsMpPotion) return 91;
        if (record.IsAllPotion) return 92; // Vigor
        if (record.IsUniversalPill) return 93;
        if (record.IsPurificationPill) return 94;
        if (record.IsPotion) return 95;
        if (code.Contains("POTION_SPEED") || code.Contains("SCROLL_SPEED")) return 96;

        // 10. Ammunition (Arrows, Bolts)
        if (record.IsAmmunition) return 100;

        // 11. Alchemy Elixirs
        if (code.Contains("ARCHEMY_REINFORCE_REC"))
        {
            if (code.Contains("WEAPON")) return 110;
            if (code.Contains("SHIELD")) return 111;
            if (code.Contains("ARMOR")) return 112;
            if (code.Contains("ACCESSARY")) return 113;
            return 114;
        }

        // 12. Alchemy Magic & Attribute Stones, Tablets, Elements
        if (code.Contains("ARCHEMY_MAGICSTONE")) return 120;
        if (code.Contains("ARCHEMY_ATTRSTONE")) return 121;
        if (code.Contains("ARCHEMY_TABLET")) return 122;
        if (code.Contains("ARCHEMY_MATERIAL") || code.Contains("ELEMENT")) return 123;
        if (code.Contains("ARCHEMY")) return 124;

        // 13. Pet Items & Transports
        if (record.IsPet || record.IsGrabPet || record.IsGrowthPet || record.IsFellowPet) return 130;
        if (record.IsCosHpPotion || record.IsFellowHpPotion || record.IsCosBadStatusPotion || record.IsCosRevivalPotion || record.IsHgpPotion || record.IsPet2SatietyPotion) return 131;

        // 14. Scrolls & Teleport & Silk Items
        if (code.Contains("SCROLL_RETURN")) return 140;
        if (code.Contains("SCROLL_REVERSE")) return 141;
        if (code.Contains("GLOBAL_CHATTING")) return 142;
        if (code.StartsWith("ITEM_MALL_") || record.CashItem == 1) return 145;
        if (code.Contains("SCROLL")) return 146;

        // 15. Trade Goods & Quests
        if (record.IsSpecialtyGoodBox || record.IsTrading) return 150;
        if (record.IsQuest) return 160;

        // 16. Fallback based on TypeID
        return 200 + (record.TypeID2 * 10) + record.TypeID3;
    }

    /// <summary>
    ///     Orders the items starting from <paramref name="minimumSlot"/> by consolidating
    ///     partial stacks first, then grouping items by category and placing them into
    ///     sequential slots without gaps.
    /// </summary>
    /// <param name="moveItem">Callback used to move an amount from source to destination slot.</param>
    /// <param name="minimumSlot">The minimum slot to include.</param>
    /// <param name="maxIterations">Safety limit on movement operations.</param>
    /// <returns>The number of movement operations performed.</returns>
    protected int OrderItems(Func<byte, byte, ushort, bool> moveItem, byte minimumSlot = 0, int maxIterations = 200)
    {
        // Step 1: Merge partial stacks first
        SortStackableItems(moveItem, minimumSlot, 10);

        // Step 2: Collect all eligible items
        var eligibleItems = this.Where(i => i.Slot >= minimumSlot).ToList();
        if (eligibleItems.Count == 0)
            return 0;

        // Step 3: Establish the desired sequential order
        var desiredOrder = eligibleItems
            .OrderBy(i => GetItemCategory(i.Record))
            .ThenByDescending(i => i.Record?.Degree ?? 0)
            .ThenByDescending(i => i.Record?.ReqLevel1 ?? 0)
            .ThenByDescending(i => i.Record?.Rarity ?? 0)
            .ThenByDescending(i => i.OptLevel)
            .ThenBy(i => i.Record?.GetRealName() ?? i.Record?.CodeName ?? string.Empty)
            .ThenByDescending(i => i.Amount)
            .ToList();

        var moves = 0;
        var blacklistedSlots = new HashSet<byte>();

        // Step 4: Move items to their target slots
        for (var index = 0; index < desiredOrder.Count && moves < maxIterations; index++)
        {
            var targetSlot = (byte)(minimumSlot + index);
            var desiredItem = desiredOrder[index];

            if (desiredItem.Slot == targetSlot)
                continue;

            var currentSlot = desiredItem.Slot;
            var itemAtTarget = GetItemAt(targetSlot);

            // Case A: The item currently at targetSlot is identical to desiredItem
            if (itemAtTarget != null &&
                itemAtTarget.ItemId == desiredItem.ItemId &&
                itemAtTarget.Amount == desiredItem.Amount &&
                itemAtTarget.OptLevel == desiredItem.OptLevel)
            {
                var existingIndex = desiredOrder.IndexOf(itemAtTarget);
                if (existingIndex >= 0)
                {
                    desiredOrder[index] = itemAtTarget;
                    desiredOrder[existingIndex] = desiredItem;
                }
                continue;
            }

            // Case B: Target slot is empty
            if (itemAtTarget == null)
            {
                if (moveItem(currentSlot, targetSlot, desiredItem.Amount))
                {
                    moves++;
                    desiredItem.Slot = targetSlot;
                }
                else
                {
                    blacklistedSlots.Add(currentSlot);
                }
                continue;
            }

            // Case C: Target slot is occupied. Try direct swap first
            var swapSucceeded = false;
            if (desiredItem.ItemId != itemAtTarget.ItemId || !desiredItem.Record.IsStackable)
            {
                if (moveItem(currentSlot, targetSlot, desiredItem.Amount))
                {
                    moves++;
                    desiredItem.Slot = targetSlot;
                    itemAtTarget.Slot = currentSlot;
                    swapSucceeded = true;
                }
            }

            // If direct swap failed or not possible, move target item to an empty buffer slot first
            if (!swapSucceeded)
            {
                byte freeSlot = 0xFF;
                // Prefer free slots outside the ordered items range
                for (var s = (byte)(minimumSlot + desiredOrder.Count); s < Capacity; s++)
                {
                    if (GetItemAt(s) == null && !blacklistedSlots.Contains(s))
                    {
                        freeSlot = s;
                        break;
                    }
                }

                if (freeSlot == 0xFF)
                {
                    for (var s = minimumSlot; s < Capacity; s++)
                    {
                        if (s != targetSlot && s != currentSlot && GetItemAt(s) == null && !blacklistedSlots.Contains(s))
                        {
                            freeSlot = s;
                            break;
                        }
                    }
                }

                if (freeSlot != 0xFF)
                {
                    if (moveItem(targetSlot, freeSlot, itemAtTarget.Amount))
                    {
                        moves++;
                        itemAtTarget.Slot = freeSlot;

                        if (moveItem(currentSlot, targetSlot, desiredItem.Amount))
                        {
                            moves++;
                            desiredItem.Slot = targetSlot;
                        }
                        else
                        {
                            blacklistedSlots.Add(currentSlot);
                        }
                    }
                    else
                    {
                        blacklistedSlots.Add(targetSlot);
                    }
                }
                else
                {
                    blacklistedSlots.Add(currentSlot);
                }
            }
        }

        return moves;
    }

    /// <summary>
    ///     Gets all items by predicate, oredered by slot.
    /// </summary>
    /// <param name="predicate">The predicate.</param>
    /// <returns>If found: list of item(s); otherwise empty list</returns>
    public IList<InventoryItem> GetItems(Predicate<InventoryItem> predicate)
    {
        //OrderBySlot();
        return _collection.FindAll(predicate);
    }

    /// <summary>
    ///     Gets all items by Record.ID
    /// </summary>
    /// <param name="objId">The Record.ID of item.</param>
    /// <returns>If found: list of item(s); otherwise empty list</returns>
    public ICollection<InventoryItem> GetItems(uint objId)
    {
        return GetItems(item => item.Record.ID == objId);
    }

    /// <summary>
    ///     Gets all items by Record.CodeName of item
    /// </summary>
    /// <param name="recordCodeName">The Record.CodeName of item.</param>
    /// <returns>If found: list of item(s), oredered by slot; otherwise empty list</returns>
    public ICollection<InventoryItem> GetItems(string recordCodeName)
    {
        return GetItems(item => item.Record.CodeName == recordCodeName);
    }

    /// <summary>
    ///     Gets all items by <see cref="TypeIdFilter" />
    /// </summary>
    /// <param name="filter">The filter.</param>
    /// <returns>If found: list of item(s); otherwise empty list</returns>
    public ICollection<InventoryItem> GetItems(TypeIdFilter filter)
    {
        return GetItems(item => filter.EqualsRefItem(item.Record));
    }

    /// <summary>
    ///     Gets all items by <see cref="TypeIdFilter" />
    /// </summary>
    /// <param name="filter">The filter.</param>
    /// <returns>If found: list of item(s); otherwise empty list</returns>
    public ICollection<InventoryItem> GetItems(TypeIdFilter filter, Predicate<InventoryItem> action)
    {
        return GetItems(item => filter.EqualsRefItem(item.Record) && action(item));
    }

    /// <summary>
    ///     Gets the item from first slot by predicate.
    /// </summary>
    /// <param name="predicate">The predicate.</param>
    /// <returns>if found: the item from first slot; otherwise: null</returns>
    public InventoryItem GetItem(Predicate<InventoryItem> predicate)
    {
        return GetItems(predicate).FirstOrDefault();
    }

    /// <summary>
    ///     Gets the item from first slot by Record.ID.
    /// </summary>
    /// <param name="objId">The Record.ID of item.</param>
    /// <returns>If found: the item from first slot; otherwise null</returns>
    public InventoryItem GetItem(uint objId)
    {
        return GetItem(item => item.Record.ID == objId);
    }

    /// <summary>
    ///     Gets the item from first slot by <see cref="TypeIdFilter" />.
    /// </summary>
    /// <param name="filter">The <see cref="TypeIdFilter" />.</param>
    /// <returns>if found: the item from first slot; otherwise: null</returns>
    public InventoryItem GetItem(TypeIdFilter filter)
    {
        return GetItem(item => filter.EqualsRefItem(item.Record));
    }

    /// <summary>
    ///     Gets all items by <see cref="TypeIdFilter" />
    /// </summary>
    /// <param name="filter">The filter.</param>
    /// <returns>If found: list of item(s); otherwise empty list</returns>
    public InventoryItem GetItem(TypeIdFilter filter, Predicate<InventoryItem> action)
    {
        return GetItem(item => filter.EqualsRefItem(item.Record) && action(item));
    }

    /// <summary>
    ///     Gets the item from first slot by Record.CodeName.
    /// </summary>
    /// <param name="recordCodeName">The Record.CodeName of item.</param>
    /// <returns>If found: the item from first slot; otherwise null</returns>
    public InventoryItem GetItem(string recordCodeName)
    {
        return GetItem(item => item.Record.CodeName == recordCodeName);
    }

    /// <summary>
    ///     Gets the SumAmount by Record.CodeName of item.
    /// </summary>
    /// <param name="recordCodeName">The Record.CodeName of item.</param>
    /// <returns>if found: the SumAmount; otherwise: 0</returns>
    public int GetSumAmount(string recordCodeName)
    {
        return GetItems(recordCodeName).Aggregate(0, (current, item) => current + item.Amount);
    }

    /// <summary>
    ///     Gets the SumAmount by <see cref="TypeIdFilter" />.
    /// </summary>
    /// <param name="filter">The <see cref="TypeIdFilter" />.</param>
    /// <returns>if found: the SumAmount; otherwise: 0</returns>
    public int GetSumAmount(TypeIdFilter filter)
    {
        var sum = 0;
        foreach (var item in GetItems(filter))
            sum += item.Amount;

        return sum;
    }

    /// <summary>
    ///     Gets the best item from the first slot by <see cref="TypeIdFilter" />.
    /// </summary>
    /// <param name="filter">The <see cref="TypeIdFilter" />.</param>
    /// <returns>If found: the best item from the first slot; otherwise null</returns>
    public InventoryItem GetItemBest(TypeIdFilter filter)
    {
        InventoryItem nearestItem = null;
        foreach (var item in GetItems(filter))
            if (nearestItem == null)
            {
                nearestItem = item;
            }
            else
            {
                if (item.Record.ReqLevel1 > nearestItem.Record.ReqLevel1 &&
                    item.OptLevel >= nearestItem.OptLevel &&
                    item.CanBeEquipped())
                    nearestItem = item;
            }

        return nearestItem;
    }

    /// <summary>
    ///     Gets the first free slot number.
    /// </summary>
    /// <returns>if found: the first free slot number; otherwise: 0xFF</returns>
    public virtual byte GetFreeSlot()
    {
        for (byte slot = 0; slot < Capacity; slot++)
            if (GetItemAt(slot) == null)
                return slot;

        return 0xFF;
    }

    /// <summary>
    ///     Move via packet operation
    /// </summary>
    /// <param name="justMove">Just move the item to new slot without control others</param>
    public void Move(Packet packet)
    {
        var sourceSlot = packet.ReadByte();
        var destinationSlot = packet.ReadByte();
        var amount = packet.ReadUShort();

        var itemAtSource = GetItemAt(sourceSlot);
        if (itemAtSource == null)
            return;

        var itemAtDestination = GetItemAt(destinationSlot);

        //Move the item from A -> B
        if (itemAtDestination == null)
        {
            //Split item
            if (itemAtSource.Amount != amount && amount != 0)
            {
                itemAtSource.Amount -= amount;

                var newInventoryItem = itemAtSource.Clone();
                newInventoryItem.Slot = destinationSlot;
                newInventoryItem.Amount = amount;

                Add(newInventoryItem);

                Log.Debug(
                    $"[InventoryItemCollection::Move] Split item {itemAtSource.Record.GetRealName()} (slot={itemAtSource.Slot}, amount={itemAtSource.Amount}) to (slot={destinationSlot}, amount={amount}");
                return;
            }

            Log.Debug(
                $"[InventoryItemCollection::Move]  Move item {itemAtSource.Record.GetRealName()} (slot={itemAtSource.Slot}, amount={amount}) to (slot= {destinationSlot})");

            itemAtSource.Slot = destinationSlot;
        }
        else
        {
            //The items will be merged!
            if (itemAtDestination.ItemId == itemAtSource.ItemId)
            {
                //Check if the items can stack, otherwise move A to B and B to A
                var newItemAmount = itemAtDestination.Amount + amount;
                if (newItemAmount > itemAtDestination.Record.MaxStack)
                {
                    itemAtDestination.Slot = sourceSlot;
                    itemAtSource.Slot = destinationSlot;

                    Log.Debug(
                        $"[InventoryItemCollection::Move]  Switch item {itemAtSource.Record.GetRealName()} (slot={itemAtSource.Slot}) with (slot={itemAtDestination.Slot}) because the max stack was reached.");
                }
                else
                {
                    itemAtDestination.Amount = (ushort)newItemAmount;
                    itemAtSource.Amount -= amount;

                    if (itemAtSource.Amount <= 0)
                        RemoveAt(sourceSlot);

                    Log.Debug(
                        $"[InventoryItemCollection::Move]  Merge item {itemAtSource.Record.GetRealName()} (slot={itemAtSource.Slot}, amount={itemAtSource.Amount}) with (slot={itemAtDestination.Slot}, amount={itemAtDestination.Amount})");
                }
            }
            else
            {
                itemAtDestination.Slot = sourceSlot;
                itemAtSource.Slot = destinationSlot;
                Log.Debug(
                    $"[InventoryItemCollection::Move]  Switch item {itemAtSource.Record.GetRealName()} (slot={itemAtSource.Slot}) with (slot={itemAtDestination.Slot}) because the items are not identically.");
            }
        }
    }

    public void MoveTo(InventoryItemCollection inventory, Packet packet)
    {
        var sourceSlot = packet.ReadByte();
        var destinationSlot = packet.ReadByte();

        var sourceItem = GetItemAt(sourceSlot);
        if (sourceItem == null)
            return;

        RemoveAt(sourceSlot);

        sourceItem.Slot = destinationSlot;
        inventory.Add(sourceItem);

        Log.Debug(
            $"[InventoryItemCollection::MoveTo] Move item {sourceItem.Record.GetRealName()} (slot={sourceSlot}) to storage (slot={destinationSlot}");
    }

    /// <summary>
    ///     Deserialize the storage packet
    /// </summary>
    /// <param name="packet">The storage packet</param>
    public void Deserialize(Packet packet)
    {
        Capacity = packet.ReadByte();
        if (Capacity <= 0)
            return;

        var amount = packet.ReadByte();
        for (var i = 0; i < amount; i++)
        {
            var item = InventoryItem.FromPacket(packet);

            if (item == null)
            {
            }

            _collection.Add(item);
        }
    }
}