using System.Collections.Generic;
using RSBot.Core.Objects;

namespace RSBot.Alchemy.Bundle.Attribute;

internal class AttributeBundleConfig
{
    internal class AttributeBundleConfigItem
    {
        public int MaxValue { get; init; }
        public InventoryItem Stone { get; init; }
        public ItemAttributeGroup Group { get; init; }
    }

    public InventoryItem Item { get; set; }
    public IEnumerable<AttributeBundleConfigItem> Attributes { get; set; }
}
