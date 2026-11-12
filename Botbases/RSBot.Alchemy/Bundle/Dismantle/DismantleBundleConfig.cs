using System.Collections.Generic;
using RSBot.Alchemy.Models;
using RSBot.Core.Objects;

namespace RSBot.Alchemy.Bundle.Dismantle;

internal class DismantleBundleConfig
{
    public bool AutoDismantle { get; set; }
    public bool DismantleWhenBagFull { get; set; }
    public bool OnlySelectedItems { get; set; }
    public bool UseMaxLevelFilter { get; set; }
    public int MaxItemLevel { get; set; } = 100;

    public List<InventoryItem> Equipment { get; set; } = new();
    public List<DismantleFilterRule> Filters { get; set; } = new();
    public List<DismantleOutputEntry> Outputs { get; set; } = new();
}
