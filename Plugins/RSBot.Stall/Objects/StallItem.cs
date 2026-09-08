using RSBot.Core.Objects;

namespace RSBot.Stall.Objects;

public class StallItem
{
    /// <summary>
    /// The stall slot index (0 - 9).
    /// </summary>
    public byte SlotStall { get; set; }

    /// <summary>
    /// The player inventory slot index where the item originated.
    /// </summary>
    public byte SlotInventory { get; set; }

    /// <summary>
    /// The underlying inventory item data.
    /// </summary>
    public InventoryItem Item { get; set; }

    /// <summary>
    /// The quantity being sold.
    /// </summary>
    public ushort Quantity { get; set; }

    /// <summary>
    /// The price in gold.
    /// </summary>
    public ulong Price { get; set; }
}
