using System.Collections.Generic;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Objects;

namespace RSBot.Alchemy.Bundle.Magic;

internal sealed class MagicBundleTarget
{
    public string Group { get; init; }
    public uint TargetValue { get; init; }
    public RefMagicOpt ReferenceOption { get; init; }
}

internal class MagicBundleConfig
{
    public InventoryItem Item { get; set; }
    public List<MagicBundleTarget> Targets { get; set; } = new();
}
