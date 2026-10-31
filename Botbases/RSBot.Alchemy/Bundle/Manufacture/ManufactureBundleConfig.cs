using System.Collections.Generic;
using RSBot.Core.Objects;

namespace RSBot.Alchemy.Bundle.Manufacture;

internal enum ManufactureMode
{
    Resolve = 1,
    Compound = 2
}

internal class ManufactureBundleConfig
{
    #region Properties

    /// <summary>
    ///     Gets or sets the make-stuff selector.
    /// </summary>
    public ManufactureMode Mode { get; set; } = ManufactureMode.Resolve;

    /// <summary>
    ///     Gets or sets the materials staged for the operation.
    /// </summary>
    public List<InventoryItem> Materials { get; set; } = new();

    #endregion Properties
}
