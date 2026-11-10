using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

    private static bool IsMovable(InventoryItem item)
    {
        if (item?.Record == null) return false;
        if (item.Record.IsTrading || item.Record.IsSpecialtyGoodBox) return false;
        var code = item.Record.CodeName;
        if (!string.IsNullOrEmpty(code) && (code.Contains("_TRADE_") || code.Contains("SPECIALTY") || code.StartsWith("ITEM_ETC_TRADE_")))
            return false;
        return true;
    }

    /// <summary>
    ///     Merges partial stacks of stackable items inside this collection.
    /// </summary>
    /// <param name="moveItem">Callback used to move an amount from a source slot to a destination slot.</param>
    /// <param name="minimumSlot">The minimum slot to include in the sort operation.</param>
    /// <param name="maxIterations">The maximum number of iterations, used to avoid deadlocks.</param>
    /// <returns>The number of iterations that were performed.</returns>
    protected int SortStackableItems(Func<byte, byte, ushort, bool> moveItem, byte minimumSlot = 0,
        int maxIterations = 500)
    {
        var iterations = 0;

        //Ignore items which move operations failed to avoid endless retries
        var blacklistedItems = new HashSet<uint>();

        // Keep merging until there is not a single possible merge left, so that
        // every partial stack is combined (not just the first group per iteration).
        while (iterations < maxIterations)
        {
            iterations++;

            var movedAny = false;

            var stackGroups = this
                .Where(i => i != null && i.Slot >= minimumSlot && i.Record != null &&
                            i.Record.IsStackable && i.Record.MaxStack > 1 &&
                            IsMovable(i) &&
                            !blacklistedItems.Contains(i.ItemId))
                .GroupBy(i => i.ItemId)
                .ToList();

            foreach (var group in stackGroups)
            {
                var items = group.OrderBy(i => i.Slot).ToList();
                if (items.Count < 2)
                    continue;

                var itemId = group.Key;

                // Use the first non-full stack as the accumulator, then pour every
                // other stack of the same item into it (and continue with the next
                // accumulator when it becomes full).
                foreach (var destinationSeed in items)
                {
                    if (blacklistedItems.Contains(itemId))
                        break;

                    var destination = GetItemAt(destinationSeed.Slot);
                    if (destination?.Record == null || destination.Amount >= destination.Record.MaxStack)
                        continue;

                    var sources = GetItems(i => i != null && i.ItemId == itemId &&
                                                i.Slot >= minimumSlot && i.Slot != destination.Slot &&
                                                i.Record != null && i.Amount < i.Record.MaxStack)
                        .OrderBy(i => i.Slot)
                        .ToList();

                    foreach (var sourceSeed in sources)
                    {
                        var dest = GetItemAt(destination.Slot);
                        if (dest?.Record == null || dest.Amount >= dest.Record.MaxStack)
                            break;

                        var source = GetItemAt(sourceSeed.Slot);
                        if (source == null || source.Amount == 0)
                            continue;

                        var amount = dest.Record.MaxStack - dest.Amount;
                        var actualAmount = source.Amount > amount ? amount : source.Amount;
                        if (actualAmount <= 0)
                            continue;

                        if (moveItem(source.Slot, dest.Slot, (ushort)actualAmount))
                        {
                            movedAny = true;
                        }
                        else
                        {
                            // Do not retry the same item id forever
                            blacklistedItems.Add(itemId);
                            break;
                        }
                    }
                }
            }

            if (!movedAny)
                break;
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

        // 11. Alchemy Elixirs (Reinforce Recipes)
        if (code.Contains("ARCHEMY_REINFORCE_REC") || code.Contains("ARCHEMY_MAGICSTONE_REC"))
        {
            if (code.Contains("WEAPON")) return 110;
            if (code.Contains("SHIELD")) return 111;
            if (code.Contains("ARMOR") || code.Contains("PROT") || code.Contains("GARMENT")) return 112;
            if (code.Contains("ACCESSARY") || code.Contains("ACCESSORY")) return 113;
            return 114;
        }

        // 12. Alchemy Magic & Attribute Stones, Tablets, Elements, Powder
        if (code.Contains("ARCHEMY_MAGICSTONE")) return 120;
        if (code.Contains("ARCHEMY_ATTRSTONE")) return 121;
        if (code.Contains("ARCHEMY_TABLET")) return 122;
        if (code.Contains("MAGIC_POWDER") || code.Contains("LUCKY_POWDER") || code.Contains("STEADY_POWDER")) return 123;
        if (code.Contains("ARCHEMY_MATERIAL") || code.Contains("ELEMENT")) return 124;
        if (code.Contains("ARCHEMY")) return 125;

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
    ///     Gets the storage page category index (0..4) for an item:
    ///     Category 0: Equipment, Weapons, Shields, Armor, Accessories, Avatars, Job/Fellow Equip
    ///     Category 1: Alchemy Elixirs (Reinforce Recipes)
    ///     Category 2: Magic & Attribute Stones, Tablets, Elements, Powder, Materials
    ///     Category 3: Recovery Potions, Pills, Scrolls, Silk & Mall Items
    ///     Category 4: Pet items, Trade Goods, Quests, Ammunition, Misc
    /// </summary>
    public static int GetStoragePageCategory(RefObjItem record)
    {
        if (record == null)
            return 4;

        var cat = GetItemCategory(record);

        // Category 0: Equipment
        if (cat < 90)
            return 0;

        // Category 1: Elixirs
        if (cat >= 110 && cat <= 119)
            return 1;

        // Category 2: Alchemy stones, tablets, elements, powder, materials
        if (cat >= 120 && cat <= 129)
            return 2;

        // Category 3: Recovery Potions, Pills, Speed Potions, Scrolls, Silk & Mall items, Globals
        if ((cat >= 90 && cat <= 99) || (cat >= 140 && cat <= 149))
            return 3;

        // Category 4: Ammunition (100), Pet Items (130, 131), Trade goods (150), Quests (160), Fallbacks
        return 4;
    }

    /// <summary>
    ///     Orders the storage items, optionally separating categories across storage pages (30 slots each).
    /// </summary>
    /// <param name="moveItem">Callback used to move an amount from source to destination slot.</param>
    /// <param name="splitByPages">Whether to separate categories by page.</param>
    /// <param name="maxIterations">Safety limit on movement operations.</param>
    /// <returns>The number of movement operations performed.</returns>
    public int OrderStorageItems(Func<byte, byte, ushort, bool> moveItem, bool splitByPages = true, int maxIterations = 300)
    {
        // Step 1: Merge partial stacks first
        SortStackableItems(moveItem, 0, 100);

        // Step 2: Collect all movable items
        var items = this.Where(i => i != null && i.Record != null && IsMovable(i)).ToList();
        if (items.Count == 0 || Capacity <= 0)
            return 0;

        const int pageSize = 30;
        var totalPages = Math.Max(1, (int)Capacity / pageSize);

        var desiredSlotAtSlot = new byte[Capacity];
        for (var i = 0; i < Capacity; i++)
            desiredSlotAtSlot[i] = 0xFF;

        if (!splitByPages || totalPages <= 1)
        {
            // Sequential Sort: group items strictly by category without empty gaps
            var sorted = items
                .OrderBy(it => GetItemCategory(it.Record))
                .ThenByDescending(it => it.Record?.Degree ?? 0)
                .ThenByDescending(it => it.Record?.ReqLevel1 ?? 0)
                .ThenByDescending(it => it.Record?.Rarity ?? 0)
                .ThenByDescending(it => it.OptLevel)
                .ThenBy(it => it.Record?.GetRealName() ?? it.Record?.CodeName ?? string.Empty)
                .ThenByDescending(it => it.Amount)
                .ToList();

            for (var idx = 0; idx < sorted.Count && idx < Capacity; idx++)
            {
                var it = sorted[idx];
                desiredSlotAtSlot[it.Slot] = (byte)idx;
            }
        }
        else
        {
            // Page-Split Sort: distribute 5 categories across totalPages
            var buckets = new List<InventoryItem>[5];
            for (var b = 0; b < 5; b++)
                buckets[b] = new List<InventoryItem>();

            foreach (var item in items)
            {
                var cat = GetStoragePageCategory(item.Record);
                if (cat < 0 || cat >= 5) cat = 4;
                buckets[cat].Add(item);
            }

            // Sort within each bucket
            for (var b = 0; b < 5; b++)
            {
                buckets[b] = buckets[b]
                    .OrderBy(it => GetItemCategory(it.Record))
                    .ThenByDescending(it => it.Record?.Degree ?? 0)
                    .ThenByDescending(it => it.Record?.ReqLevel1 ?? 0)
                    .ThenByDescending(it => it.Record?.Rarity ?? 0)
                    .ThenByDescending(it => it.OptLevel)
                    .ThenBy(it => it.Record?.GetRealName() ?? it.Record?.CodeName ?? string.Empty)
                    .ThenByDescending(it => it.Amount)
                    .ToList();
            }

            // Map categories to pages based on available page count
            var pageCategories = new List<int>[totalPages];
            for (var p = 0; p < totalPages; p++)
                pageCategories[p] = new List<int>();

            switch (totalPages)
            {
                case 2:
                    pageCategories[0].AddRange(new[] { 0, 1 });       // Page 1: Equipment + Elixirs
                    pageCategories[1].AddRange(new[] { 2, 3, 4 });    // Page 2: Stones + Potions + Misc
                    break;
                case 3:
                    pageCategories[0].AddRange(new[] { 0 });          // Page 1: Equipment
                    pageCategories[1].AddRange(new[] { 1, 2 });       // Page 2: Elixirs + Stones
                    pageCategories[2].AddRange(new[] { 3, 4 });       // Page 3: Potions + Misc
                    break;
                case 4:
                    pageCategories[0].AddRange(new[] { 0 });          // Page 1: Equipment
                    pageCategories[1].AddRange(new[] { 1 });          // Page 2: Elixirs
                    pageCategories[2].AddRange(new[] { 2 });          // Page 3: Stones
                    pageCategories[3].AddRange(new[] { 3, 4 });       // Page 4: Potions + Misc
                    break;
                default: // 5+ pages
                    pageCategories[0].AddRange(new[] { 0 });          // Page 1: Equipment
                    pageCategories[1].AddRange(new[] { 1 });          // Page 2: Elixirs
                    pageCategories[2].AddRange(new[] { 2 });          // Page 3: Stones
                    pageCategories[3].AddRange(new[] { 3 });          // Page 4: Potions / Scrolls / Mall
                    pageCategories[4].AddRange(new[] { 4 });          // Page 5: Pet / Trade / Misc
                    break;
            }

            var reservedSlots = new HashSet<byte>();
            var overflowItems = new List<InventoryItem>();

            for (var p = 0; p < totalPages; p++)
            {
                var pageItems = new List<InventoryItem>();
                foreach (var cat in pageCategories[p])
                    pageItems.AddRange(buckets[cat]);

                var pageBaseSlot = p * pageSize;
                var pageMaxSlot = Math.Min((p + 1) * pageSize, (int)Capacity) - 1;

                var currentSlot = pageBaseSlot;
                var itemIdx = 0;

                while (itemIdx < pageItems.Count && currentSlot <= pageMaxSlot)
                {
                    var it = pageItems[itemIdx];
                    desiredSlotAtSlot[it.Slot] = (byte)currentSlot;
                    reservedSlots.Add((byte)currentSlot);
                    currentSlot++;
                    itemIdx++;
                }

                while (itemIdx < pageItems.Count)
                {
                    overflowItems.Add(pageItems[itemIdx]);
                    itemIdx++;
                }
            }

            // Assign overflow items to remaining unreserved slots starting from the last page
            if (overflowItems.Count > 0)
            {
                var unreservedSlots = new List<byte>();
                for (var p = totalPages - 1; p >= 0; p--)
                {
                    var pageBaseSlot = p * pageSize;
                    var pageMaxSlot = Math.Min((p + 1) * pageSize, (int)Capacity) - 1;
                    for (var s = pageBaseSlot; s <= pageMaxSlot; s++)
                    {
                        if (!reservedSlots.Contains((byte)s))
                            unreservedSlots.Add((byte)s);
                    }
                }

                var ovIdx = 0;
                foreach (var ovItem in overflowItems)
                {
                    if (ovIdx >= unreservedSlots.Count)
                        break;

                    var slot = unreservedSlots[ovIdx++];
                    desiredSlotAtSlot[ovItem.Slot] = slot;
                    reservedSlots.Add(slot);
                }
            }
        }

        return ExecutePermutation(moveItem, desiredSlotAtSlot, 0, (byte)(Capacity - 1), maxIterations);
    }

    /// <summary>
    ///     Orders items starting from <paramref name="minimumSlot"/> by consolidating
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
        SortStackableItems(moveItem, minimumSlot, 100);

        // Step 2: Collect all eligible movable items
        var eligibleItems = this.Where(i => i.Slot >= minimumSlot && i.Record != null && IsMovable(i)).ToList();
        if (eligibleItems.Count == 0 || Capacity <= minimumSlot)
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

        var desiredSlotAtSlot = new byte[Capacity];
        for (var i = 0; i < Capacity; i++)
            desiredSlotAtSlot[i] = 0xFF;

        for (var idx = 0; idx < desiredOrder.Count && (minimumSlot + idx) < Capacity; idx++)
        {
            var item = desiredOrder[idx];
            desiredSlotAtSlot[item.Slot] = (byte)(minimumSlot + idx);
        }

        return ExecutePermutation(moveItem, desiredSlotAtSlot, minimumSlot, (byte)(Capacity - 1), maxIterations);
    }

    /// <summary>
    ///     Executes item moves using the Hole-based Empty-Slot Permutation Algorithm.
    ///     In Silkroad storage/inventory, moves to occupied slots are rejected by the server;
    ///     all moves are executed into empty slots, guaranteeing 100% server acceptance and preventing deadlocks.
    /// </summary>
    private int ExecutePermutation(Func<byte, byte, ushort, bool> moveItem, byte[] desiredSlotAtSlot, byte minSlot, byte maxSlot, int maxIterations)
    {
        // 1. Optimize: identical items at reciprocal or matching positions
        for (var s = minSlot; s <= maxSlot; s++)
        {
            var target = desiredSlotAtSlot[s];
            if (target != 0xFF && target != s && target >= minSlot && target <= maxSlot)
            {
                var itemCurrent = GetItemAt(s);
                var itemAtTarget = GetItemAt(target);
                if (itemCurrent != null && itemAtTarget != null &&
                    itemCurrent.ItemId == itemAtTarget.ItemId &&
                    itemCurrent.Amount == itemAtTarget.Amount &&
                    itemCurrent.OptLevel == itemAtTarget.OptLevel)
                {
                    var targetOfOther = desiredSlotAtSlot[target];
                    desiredSlotAtSlot[s] = targetOfOther;
                    desiredSlotAtSlot[target] = target;
                }
            }
        }

        var moves = 0;
        var maxMoves = Math.Max(maxIterations, (maxSlot - minSlot + 1) * 4);

        while (moves < maxMoves)
        {
            // Collect empty slots in the valid range
            var emptySlots = new List<byte>();
            for (var s = minSlot; s <= maxSlot; s++)
            {
                if (GetItemAt(s) == null)
                    emptySlots.Add(s);
            }

            // Check if any item is still mislocated
            var hasMislocated = false;
            for (var s = minSlot; s <= maxSlot; s++)
            {
                if (desiredSlotAtSlot[s] != 0xFF && desiredSlotAtSlot[s] != s)
                {
                    hasMislocated = true;
                    break;
                }
            }

            if (!hasMislocated)
                break; // All movable items are at their desired slots!

            if (emptySlots.Count == 0)
            {
                Log.Notify("[Inventory/Storage] No empty slot available to rearrange items.");
                break;
            }

            byte fromSlot = 0xFF;
            byte toSlot = 0xFF;

            // Direct placement: find an item whose desired slot is currently an empty slot
            foreach (var emptySlot in emptySlots)
            {
                for (var s = minSlot; s <= maxSlot; s++)
                {
                    if (desiredSlotAtSlot[s] == emptySlot && s != emptySlot)
                    {
                        fromSlot = s;
                        toSlot = emptySlot;
                        break;
                    }
                }
                if (fromSlot != 0xFF)
                    break;
            }

            // Cycle breaking: if no item directly wants any of the empty slots,
            // pick any mislocated item and move it to an empty slot to create a hole
            if (fromSlot == 0xFF)
            {
                for (var s = minSlot; s <= maxSlot; s++)
                {
                    if (desiredSlotAtSlot[s] != 0xFF && desiredSlotAtSlot[s] != s)
                    {
                        fromSlot = s;
                        toSlot = emptySlots[0];
                        break;
                    }
                }
            }

            if (fromSlot == 0xFF || toSlot == 0xFF)
                break;

            var itemToMove = GetItemAt(fromSlot);
            if (itemToMove == null)
            {
                desiredSlotAtSlot[fromSlot] = 0xFF;
                continue;
            }

            if (moveItem(fromSlot, toSlot, itemToMove.Amount))
            {
                moves++;
                desiredSlotAtSlot[toSlot] = desiredSlotAtSlot[fromSlot];
                desiredSlotAtSlot[fromSlot] = 0xFF;
                itemToMove.Slot = toSlot;
            }
            else
            {
                // Retry once after 200ms
                Thread.Sleep(200);
                if (moveItem(fromSlot, toSlot, itemToMove.Amount))
                {
                    moves++;
                    desiredSlotAtSlot[toSlot] = desiredSlotAtSlot[fromSlot];
                    desiredSlotAtSlot[fromSlot] = 0xFF;
                    itemToMove.Slot = toSlot;
                }
                else
                {
                    Log.Debug($"[Inventory/Storage] Move slot {fromSlot} -> {toSlot} failed twice. Stopping.");
                    break;
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
        return GetItem(item => item != null && item.Record != null && item.Record.CodeName == recordCodeName && (item.Record.MaxStack <= 1 || item.Amount > 0));
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