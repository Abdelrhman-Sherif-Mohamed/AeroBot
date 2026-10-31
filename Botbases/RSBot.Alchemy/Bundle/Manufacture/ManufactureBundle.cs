using System;
using System.Collections.Generic;
using System.Linq;
using RSBot.Alchemy.Bot;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Objects;

namespace RSBot.Alchemy.Bundle.Manufacture;

internal class ManufactureBundle : IAlchemyBundle
{
    #region Constants

    private const string RondoCodeName = "ITEM_ETC_ARCHEMY_RONDO_01";

    #endregion Constants

    #region Members

    private bool _shouldRun;
    private DateTime _nextAttempt = DateTime.MinValue;

    #endregion Members

    #region Constructor

    public ManufactureBundle()
    {
        SubscribeEvents();
    }

    #endregion Constructor

    #region Methods

    public void Start()
    {
        _shouldRun = true;
        _nextAttempt = DateTime.UtcNow;
    }

    public void Stop()
    {
        _shouldRun = false;

        AlchemyManager.CancelPending();
    }

    public void Run<T>(T engineConfig)
    {
        if (engineConfig is not ManufactureBundleConfig config)
            return;

        if (!_shouldRun || Globals.Botbase?.AlchemyEngine != AlchemyEngine.Manufacture)
            return;

        if (DateTime.UtcNow < _nextAttempt)
            return;

        if (config.Materials == null || config.Materials.Count == 0)
        {
            Log.Warn("[Alchemy] No materials selected for manufacture, stopping now!");

            Kernel.Bot.Stop();

            return;
        }

        var maxCount = config.Mode == ManufactureMode.Compound ? 1 : 4;
        var materials = ResolveMaterials(config.Materials, maxCount);

        if (materials.Count == 0)
        {
            Log.Warn("[Alchemy] The selected manufacture materials are no longer in the inventory, stopping now!");

            Kernel.Bot.Stop();

            return;
        }

        if (GetRondoCount() <= 0)
        {
            Log.Warn("[Alchemy] No manufacture Rondo left in the inventory, stopping now!");

            Kernel.Bot.Stop();

            return;
        }

        var started = config.Mode == ManufactureMode.Compound
            ? AlchemyManager.TryCompoundMaterials(materials)
            : AlchemyManager.TryResolveMaterials(materials);

        if (!started)
        {
            _nextAttempt = DateTime.UtcNow.AddSeconds(1);

            return;
        }

        _shouldRun = false;
    }

    /// <summary>
    ///     Returns fresh inventory items matching the configured material ids.
    /// </summary>
    private static List<InventoryItem> ResolveMaterials(IEnumerable<InventoryItem> configured, int maxCount)
    {
        var result = new List<InventoryItem>(4);
        var seen = new HashSet<uint>();

        foreach (var item in configured)
        {
            if (item == null || !seen.Add(item.ItemId))
                continue;

            var available = Game.Player.Inventory.GetItem(item.ItemId);

            if (available == null || available.Amount == 0)
                continue;

            result.Add(available);

            if (result.Count >= maxCount)
                break;
        }

        return result;
    }

    private static int GetRondoCount()
    {
        return Game.Player.Inventory.GetItems(RondoCodeName).Sum(i => (int)i.Amount);
    }

    private void SubscribeEvents()
    {
        EventManager.SubscribeEvent("OnMakeStuffResult",
            new System.Action<bool, ushort, byte, uint, bool>(OnMakeStuffResult));
        EventManager.SubscribeEvent("OnMakeStuffCanceled", new System.Action(OnMakeStuffCanceled));
    }

    #endregion Methods

    #region Events

    private void OnMakeStuffResult(bool success, ushort errorCode, byte extra, uint value, bool completed)
    {
        if (!Bootstrap.IsActive || Globals.Botbase?.AlchemyEngine != AlchemyEngine.Manufacture)
            return;

        if (!success)
        {
            _shouldRun = false;

            var message = $"[Alchemy] Manufacture failed with error 0x{errorCode:X4}";

            Log.Warn(message);
            Globals.View.AddLog(string.Empty, message);

            Kernel.Bot.Stop();

            return;
        }

        if (!completed)
            return;

        _shouldRun = true;
        _nextAttempt = DateTime.UtcNow.AddMilliseconds(500);

        Globals.View.AddLog(string.Empty, "[Alchemy] Manufacture completed.");
    }

    private void OnMakeStuffCanceled()
    {
        if (!Bootstrap.IsActive || Globals.Botbase?.AlchemyEngine != AlchemyEngine.Manufacture)
            return;

        _shouldRun = true;
        _nextAttempt = DateTime.UtcNow.AddMilliseconds(500);

        Globals.View.AddLog(string.Empty, "[Alchemy] Manufacture canceled.");
    }

    #endregion Events
}
