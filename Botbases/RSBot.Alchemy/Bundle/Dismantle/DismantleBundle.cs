using System;
using System.Collections.Generic;
using System.Linq;
using RSBot.Alchemy.Bot;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Objects;

namespace RSBot.Alchemy.Bundle.Dismantle;

internal class DismantleBundle : IAlchemyBundle
{
    #region Constants

    private const string RondoCodeName = "ITEM_ETC_ARCHEMY_RONDO_02";

    #endregion Constants

    #region Members

    private bool _shouldRun;
    private DateTime _nextAttempt = DateTime.MinValue;

    #endregion Members

    #region Constructor

    public DismantleBundle()
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
        if (engineConfig is not DismantleBundleConfig config)
            return;

        if (!_shouldRun || Globals.Botbase?.AlchemyEngine != AlchemyEngine.Dismantle)
            return;

        if (DateTime.UtcNow < _nextAttempt)
            return;

        if (config.Equipment == null || config.Equipment.Count == 0)
        {
            Log.Warn("[Alchemy] No equipment selected for dismantling, stopping now!");

            Kernel.Bot.Stop();

            return;
        }

        var item = ResolveEquipment(config.Equipment);

        if (item == null)
        {
            Log.Warn("[Alchemy] The selected equipment is no longer in the inventory, stopping now!");

            Kernel.Bot.Stop();

            return;
        }

        if (GetRondoCount() <= 0)
        {
            Log.Warn("[Alchemy] No dismantling Rondo left in the inventory, stopping now!");

            Kernel.Bot.Stop();

            return;
        }

        if (!AlchemyManager.TryDismantle(new[] { item }))
        {
            _nextAttempt = DateTime.UtcNow.AddSeconds(1);

            return;
        }

        _shouldRun = false;
    }

    /// <summary>
    ///     Returns the first configured equipment item that is still available.
    /// </summary>
    private static InventoryItem ResolveEquipment(IEnumerable<InventoryItem> configured)
    {
        foreach (var item in configured)
        {
            if (item == null)
                continue;

            var available = Game.Player.Inventory.GetItem(item.ItemId);

            if (available != null && available.Amount > 0)
                return available;
        }

        return null;
    }

    private static int GetRondoCount()
    {
        return Game.Player.Inventory.GetItems(RondoCodeName).Sum(i => (int)i.Amount);
    }

    private void SubscribeEvents()
    {
        EventManager.SubscribeEvent("OnDismantleResult", new System.Action<bool, ushort>(OnDismantleResult));
    }

    #endregion Methods

    #region Events

    private void OnDismantleResult(bool success, ushort errorCode)
    {
        if (!Bootstrap.IsActive || Globals.Botbase?.AlchemyEngine != AlchemyEngine.Dismantle)
            return;

        if (!success)
        {
            _shouldRun = false;

            var message = $"[Alchemy] Dismantling failed with error 0x{errorCode:X4}";

            Log.Warn(message);
            Globals.View.AddLog(string.Empty, message);

            Kernel.Bot.Stop();

            return;
        }

        _shouldRun = true;
        _nextAttempt = DateTime.UtcNow.AddMilliseconds(600);

        Globals.View.AddLog(string.Empty, "[Alchemy] Dismantling completed.");
    }

    #endregion Events
}
