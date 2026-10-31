using System.Collections.Generic;
using RSBot.Core.Objects;

namespace RSBot.Alchemy.Bundle.Dismantle;

internal class DismantleBundleConfig
{
    #region Properties

    /// <summary>
    ///     Gets or sets the equipment selected for dismantling.
    /// </summary>
    public List<InventoryItem> Equipment { get; set; } = new();

    #endregion Properties
}
