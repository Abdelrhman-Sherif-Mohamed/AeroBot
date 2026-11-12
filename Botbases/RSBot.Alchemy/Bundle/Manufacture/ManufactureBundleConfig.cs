using System.Collections.Generic;
using RSBot.Alchemy.Models;
using RSBot.Core.Objects;

namespace RSBot.Alchemy.Bundle.Manufacture;

internal enum ManufactureMode
{
    Resolve = 1,
    Compound = 2
}

internal class ManufactureBundleConfig
{
    public bool AutoManufacture { get; set; }
    public int DelayMs { get; set; } = 500;
    public ManufactureMode Mode { get; set; } = ManufactureMode.Resolve;
    public List<InventoryItem> Materials { get; set; } = new();
    public List<ManufactureQueueEntry> Queue { get; set; } = new();
}
