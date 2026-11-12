using System;
using System.Collections.Generic;
using System.Linq;
using RSBot.Core;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Objects;

namespace RSBot.Alchemy.Helper;

internal class AlchemyItemHelper
{
    public enum ElixirType
    {
        Shield,
        Weapon,
        Protector,
        Accessory,
        Unspecified
    }

    private const int ParamProtectorElixir = 16909056;
    private const int ParamWeaponElixir = 100663296;
    private const int ParamAccessoryElixir = 83886080;
    private const int ParamShieldElixir = 67108864;

    private static readonly byte[] AccessoryTypeId3 = { 5, 12 };
    private static readonly byte[] ArmorTypeId3 = { 1, 2, 3, 9, 10, 11 };

    /// <summary>
    ///     Gets all inventory equipment items eligible for alchemy (excluding equipped slots &lt; 13, avatars, and job outfits).
    /// </summary>
    public static List<InventoryItem> GetEligibleEquipmentItems()
    {
        if (Game.Player?.Inventory == null)
            return new List<InventoryItem>();

        return Game.Player.Inventory
            .Where(i => i?.Record != null
                        && i.Slot > 12
                        && i.Amount > 0
                        && i.Record.IsEquip
                        && !i.Record.IsAvatar
                        && !i.Record.IsJobOutfit)
            .OrderBy(i => i.Slot)
            .ToList();
    }

    /// <summary>
    ///     Returns a non-empty display name for an item, falling back from translated name to ObjName and CodeName.
    /// </summary>
    public static string GetItemDisplayName(RefObjCommon record)
    {
        if (record == null)
            return string.Empty;

        var real = record.GetRealName();
        if (!string.IsNullOrWhiteSpace(real)
            && real.Trim() != "0"
            && !string.Equals(real.Trim(), record.NameStrID, StringComparison.OrdinalIgnoreCase))
        {
            return real.Trim();
        }

        if (!string.IsNullOrWhiteSpace(record.ObjName) && record.ObjName.Trim() != "0")
            return record.ObjName.Trim();

        if (!string.IsNullOrWhiteSpace(record.CodeName))
            return record.CodeName.Trim();

        if (!string.IsNullOrWhiteSpace(real))
            return real.Trim();

        return $"Item #{record.ID}";
    }

    /// <summary>
    ///     Gets all manufacture/alchemy materials in inventory (excluding rondos, elixirs and powders).
    /// </summary>
    public static List<InventoryItem> GetManufactureMaterials()
    {
        if (Game.Player?.Inventory == null)
            return new List<InventoryItem>();

        return Game.Player.Inventory
            .Where(IsManufactureMaterial)
            .OrderBy(i => GetItemDisplayName(i.Record))
            .ThenBy(i => i.Slot)
            .ToList();
    }

    /// <summary>
    ///     Returns true if the item is an alchemy material usable in Manufacture (make-stuff), excluding Rondos and Elixirs.
    /// </summary>
    public static bool IsManufactureMaterial(InventoryItem item)
    {
        var record = item?.Record;
        var codeName = record?.CodeName;
        if (record == null || string.IsNullOrEmpty(codeName) || item.Amount == 0 || item.Slot <= 12)
            return false;

        if (record.TypeID2 == 3 && record.TypeID3 == 10)
            return false;

        if (codeName.IndexOf("RONDO", StringComparison.OrdinalIgnoreCase) >= 0)
            return false;

        return (record.TypeID1 == 3 && record.TypeID2 == 3 && record.TypeID3 == 11)
            || codeName.StartsWith("ITEM_ETC_ARCHEMY_", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Determines the matching elixir category for a piece of equipment.
    /// </summary>
    public static ElixirType GetElixirTypeForItem(InventoryItem item)
    {
        if (item?.Record == null || item.Record.TypeID2 != 1)
            return ElixirType.Unspecified;

        if (item.Record.TypeID3 == 4)
            return ElixirType.Shield;

        if (item.Record.TypeID3 == 6)
            return ElixirType.Weapon;

        if (AccessoryTypeId3.Contains(item.Record.TypeID3))
            return ElixirType.Accessory;

        if (ArmorTypeId3.Contains(item.Record.TypeID3))
            return ElixirType.Protector;

        return ElixirType.Unspecified;
    }

    /// <summary>
    ///     Gets matching lucky powders for the target equipment degree.
    /// </summary>
    public static IEnumerable<InventoryItem> GetLuckyPowders(InventoryItem targetItem)
    {
        if (targetItem?.Record == null || Game.Player?.Inventory == null)
            return Enumerable.Empty<InventoryItem>();

        return Game.Player.Inventory.GetItems(new TypeIdFilter(3, 3, 10, 2))
            .Where(i => i.Record != null && i.Record.ItemClass == targetItem.Record.Degree && i.Amount > 0);
    }

    public static InventoryItem GetLuckyStone(InventoryItem targetItem)
        => GetStonesByGroup(targetItem, RefMagicOpt.MaterialLuck).FirstOrDefault();

    public static InventoryItem GetAstralStone(InventoryItem targetItem)
        => GetStonesByGroup(targetItem, RefMagicOpt.MaterialAstral).FirstOrDefault();

    public static InventoryItem GetImmortalStone(InventoryItem targetItem)
        => GetStonesByGroup(targetItem, RefMagicOpt.MaterialImmortal).FirstOrDefault();

    public static InventoryItem GetSteadyStone(InventoryItem targetItem)
        => GetStonesByGroup(targetItem, RefMagicOpt.MaterialSteady).FirstOrDefault();

    /// <summary>
    ///     Gets total count in inventory of magic stones matching the item degree and magic option group.
    /// </summary>
    public static int GetTotalStoneCount(InventoryItem targetItem, string group)
    {
        if (targetItem?.Record == null)
            return 0;

        return GetStonesByGroup(targetItem, group).Sum(i => (int)i.Amount);
    }

    public static IEnumerable<InventoryItem> GetStonesByGroup(InventoryItem targetItem, string name)
    {
        if (targetItem?.Record == null || string.IsNullOrEmpty(name) || Game.Player?.Inventory == null)
            return Enumerable.Empty<InventoryItem>();

        return Game.Player.Inventory.Where(
            i => i?.Record != null && i.Amount > 0 && i.Record.Desc1 == name && i.Record.ItemClass == targetItem.Record.Degree);
    }

    public static IEnumerable<InventoryItem> GetStonesByGroup(byte level, string name)
    {
        if (string.IsNullOrEmpty(name) || Game.Player?.Inventory == null)
            return Enumerable.Empty<InventoryItem>();

        return Game.Player.Inventory.Where(
            i => i?.Record != null && i.Amount > 0 && i.Record.Desc1 == name && i.Record.ItemClass == level);
    }

    public static bool HasMagicOption(InventoryItem inventoryItem, string materialGroup)
    {
        if (inventoryItem?.MagicOptions == null)
            return false;

        foreach (var i in inventoryItem.MagicOptions)
        {
            var option = Game.ReferenceManager.GetMagicOption(i.Id);
            if (option != null && option.Group == materialGroup)
                return true;
        }

        return false;
    }

    public static IEnumerable<InventoryItem> GetElixirItems(ElixirType elixirType = ElixirType.Unspecified)
    {
        if (Game.Player?.Inventory == null)
            return Enumerable.Empty<InventoryItem>();

        return elixirType switch
        {
            ElixirType.Protector => Game.Player.Inventory.Where(i => i.Record.Param1 == ParamProtectorElixir && i.Amount > 0),
            ElixirType.Weapon => Game.Player.Inventory.Where(i => i.Record.Param1 == ParamWeaponElixir && i.Amount > 0),
            ElixirType.Accessory => Game.Player.Inventory.Where(i => i.Record.Param1 == ParamAccessoryElixir && i.Amount > 0),
            ElixirType.Shield => Game.Player.Inventory.Where(i => i.Record.Param1 == ParamShieldElixir && i.Amount > 0),
            ElixirType.Unspecified => Game.Player.Inventory.GetItems(new TypeIdFilter(3, 3, 10, 1)).Where(i => i.Amount > 0),
            _ => Enumerable.Empty<InventoryItem>()
        };
    }

    public static IEnumerable<InventoryItem> GetAttributeStones(InventoryItem targetItem, ItemAttributeGroup group)
    {
        if (targetItem?.Record == null || Game.Player?.Inventory == null)
            return Enumerable.Empty<InventoryItem>();

        var typeIdFilter = new TypeIdFilter(3, 3, 11, 2);
        var actualGroupName = ItemAttributesInfo.GetActualAttributeGroupNameForItem(targetItem.Record, group);
        if (string.IsNullOrEmpty(actualGroupName))
            return Enumerable.Empty<InventoryItem>();

        return Game.Player.Inventory
            .GetItems(typeIdFilter)
            .Where(i => i?.Record != null && i.Amount > 0 && i.Record.Desc1 == actualGroupName);
    }

    public static int GetItemCountByCodeName(string codeName)
    {
        if (string.IsNullOrEmpty(codeName) || Game.Player?.Inventory == null)
            return 0;

        return Game.Player.Inventory.GetItems(codeName).Sum(i => (int)i.Amount);
    }

    public static int GetItemCountById(uint itemId)
    {
        if (itemId == 0 || Game.Player?.Inventory == null)
            return 0;

        return Game.Player.Inventory.Where(i => i.ItemId == itemId).Sum(i => (int)i.Amount);
    }

    public static string FormatEquipmentDisplay(InventoryItem item)
    {
        if (item?.Record == null)
            return string.Empty;

        var opt = item.OptLevel > 0 ? $"+{item.OptLevel} " : string.Empty;
        return $"[{item.Slot}] {opt}{GetItemDisplayName(item.Record)} (D{item.Record.Degree})";
    }
}
