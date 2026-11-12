using System.Collections.Generic;
using RSBot.Core.Objects;

namespace RSBot.Alchemy.Bundle.Enhance;

internal class EnhanceBundleConfig
{
    public InventoryItem Item { get; set; }
    public byte MaxOptLevel { get; set; }
    public uint? PreferredElixirItemId { get; set; }
    public IEnumerable<InventoryItem> Elixirs { get; set; }

    public bool UseLuckyPowder { get; set; } = true;
    public byte LuckyPowderMinPlus { get; set; }
    public bool StopIfLuckyPowderEmpty { get; set; } = true;

    public bool UseLuckyStones { get; set; }
    public byte LuckyStoneMinPlus { get; set; } = 5;

    public bool UseImmortalStones { get; set; }
    public byte ImmortalStoneMinPlus { get; set; } = 5;

    public bool UseAstralStones { get; set; }
    public byte AstralStoneMinPlus { get; set; } = 5;

    public bool UseSteadyStones { get; set; }
    public byte SteadyStoneMinPlus { get; set; } = 5;

    public bool SkipToNextOnFail { get; set; }
}
