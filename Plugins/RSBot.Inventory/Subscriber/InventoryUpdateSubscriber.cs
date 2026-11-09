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
            RSBot.Core.Components.ShoppingManager.BotOpenedDialog ||
            RSBot.Core.Components.ShoppingManager.IsRecentTradeOperation ||
            (Game.Player?.JobTransport != null) ||
            (Game.Player?.TradeInfo != null) ||
            (Kernel.Bot != null && Kernel.Bot.Running && Kernel.Bot.Botbase?.Name == "RSBot.Trade"))
            return;

        // Never sort/order if character has trade goods in inventory (prevents server kick DC)
        if (Game.Player?.Inventory != null &&
            System.Linq.Enumerable.Any(Game.Player.Inventory, i => i?.Record != null && 
                (i.Record.IsTrading || i.Record.IsSpecialtyGoodBox || (!string.IsNullOrEmpty(i.Record.CodeName) && (i.Record.CodeName.Contains("_TRADE_") || i.Record.CodeName.StartsWith("ITEM_ETC_TRADE_"))))))
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