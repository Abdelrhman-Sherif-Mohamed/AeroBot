using RSBot.Core;
using RSBot.Core.Event;

namespace RSBot.Inventory.Subscriber;

internal static class InventoryUpdateSubscriber
{
    private static object _lock;

    /// <summary>
    ///     Subscribes the events.
    /// </summary>
    public static void SubscribeEvents()
    {
        _lock = new object();
        EventManager.SubscribeEvent("OnInventoryUpdate", OnInventoryUpdate);
    }

    private static void OnInventoryUpdate()
    {
        var autoOrder = PlayerConfig.Get("RSBot.Inventory.AutoOrder", false);
        var autoSort = PlayerConfig.Get("RSBot.Inventory.AutoSort", false);
        if (!autoOrder && !autoSort)
            return;

        if (RSBot.Core.Components.HappyManager.Running)
            return;

        if (RSBot.Core.Components.ShoppingManager.Running ||
            (Kernel.Bot != null && Kernel.Bot.Running && Kernel.Bot.Botbase?.Name == "RSBot.Trade"))
            return;

        if (Game.Player?.Inventory == null || Game.Player.Inventory.IsSorting || Game.Player.Inventory.IsOrdering)
            return;

        lock (_lock)
        {
            if (autoOrder)
                Game.Player.Inventory.Order();
            else if (autoSort)
                Game.Player.Inventory.Sort();
        }
    }
}