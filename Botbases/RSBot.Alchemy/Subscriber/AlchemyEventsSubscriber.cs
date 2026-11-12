using System;
using System.Linq;
using RSBot.Core;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Objects;

namespace RSBot.Alchemy.Subscriber;

internal class AlchemyEventsSubscriber
{
    public static void Subscribe()
    {
        EventManager.SubscribeEvent("OnAlchemyError", new Action<ushort, AlchemyType>(OnAlchemyError));
        EventManager.SubscribeEvent("OnAlchemyDestroyed", new Action<InventoryItem, AlchemyType>(OnAlchemyDestroyed));
        EventManager.SubscribeEvent("OnFuseRequest", new Action<AlchemyAction, AlchemyType>(OnFuseRequest));
    }

    private static void OnAlchemyDestroyed(InventoryItem oldItem, AlchemyType type)
    {
        if (!Bootstrap.IsActive)
            return;

        var itemName = oldItem?.Record?.GetRealName() ?? "Item";
        Globals.View.AddLog(itemName,
            Game.ReferenceManager.GetTranslation("UIIT_MSG_REINFORCERR_BREAKDOWN"));
        Log.Warn($"[Alchemy] {itemName} has been destroyed.");

        Globals.Botbase.NotifyItemDestroyed(oldItem);
    }

    private static void OnAlchemyError(ushort errorCode, AlchemyType type)
    {
        if (!Bootstrap.IsActive)
            return;

        if (errorCode is 0x5423)
            return;

        if (type == AlchemyType.MagicStone || type == AlchemyType.AttributeStone)
            return;

        Log.Error($"[Alchemy] Alchemy fusion error: 0x{errorCode:X4}");
        Globals.Botbase.NotifyItemFatal($"Server error 0x{errorCode:X4}", allowSkipInQueue: true);
    }

    private static void OnFuseRequest(AlchemyAction action, AlchemyType type)
    {
        if (AlchemyManager.ActiveAlchemyItems == null)
            return;

        var ingredient = AlchemyManager.ActiveAlchemyItems.ElementAtOrDefault(1);
        var item = AlchemyManager.ActiveAlchemyItems.ElementAtOrDefault(0);
        var ingName = ingredient?.Record?.GetRealName() ?? string.Empty;
        var itemName = item?.Record?.GetRealName() ?? string.Empty;

        switch (type)
        {
            case AlchemyType.Elixir:
                Globals.View.AddLog(itemName, $"Fusing elixir [{ingName}]");
                break;

            case AlchemyType.MagicStone:
                Globals.View.AddLog(itemName, $"Fusing magic stone [{ingName}]");
                break;

            case AlchemyType.AttributeStone:
                Globals.View.AddLog(itemName, $"Fusing attribute stone [{ingName}]");
                break;

            default:
                Globals.View.AddLog(itemName, $"Fusing [{ingName}]");
                break;
        }
    }
}
