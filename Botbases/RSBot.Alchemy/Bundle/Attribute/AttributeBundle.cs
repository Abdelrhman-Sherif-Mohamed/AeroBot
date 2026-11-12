using System;
using System.Linq;
using RSBot.Alchemy.Bot;
using RSBot.Alchemy.Helper;
using RSBot.Core;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Extensions;
using RSBot.Core.Objects;

namespace RSBot.Alchemy.Bundle.Attribute;

internal class AttributeBundle : IAlchemyBundle
{
    private bool _shouldRun;

    public AttributeBundle()
    {
        SubscribeEvents();
    }

    public void Start()
    {
        _shouldRun = true;
    }

    public void Stop()
    {
        _shouldRun = false;
        AlchemyManager.CancelPending();
    }

    private void SubscribeEvents()
    {
        EventManager.SubscribeEvent("OnAlchemySuccess",
            new Action<InventoryItem, InventoryItem, AlchemyType>(OnStoneAlchemySuccess));
        EventManager.SubscribeEvent("OnAlchemyFailed",
            new Action<InventoryItem, InventoryItem, AlchemyType>(OnStoneAlchemyFailed));
        EventManager.SubscribeEvent("OnAlchemyError", new Action<ushort, AlchemyType>(OnStoneAlchemyError));
        EventManager.SubscribeEvent("OnAlchemy", new Action<AlchemyType>(OnStoneAlchemy));
        EventManager.SubscribeEvent("OnFuseRequest", new Action<AlchemyAction, AlchemyType>(OnFuseRequest));
    }

    public void Run<T>(T engineConfig)
    {
        if (engineConfig is not AttributeBundleConfig config)
            return;

        if (!_shouldRun || (Globals.Botbase.AlchemyEngine != AlchemyEngine.Attribute &&
                            Globals.Botbase.AlchemyEngine != AlchemyEngine.Queue))
            return;

        if (config.Item == null)
        {
            Globals.Botbase.NotifyItemFatal("No item configured for Stats", allowSkipInQueue: true);
            return;
        }

        var item = Game.Player.Inventory.GetItemAt(config.Item.Slot);
        if (item == null || item.ItemId != config.Item.ItemId || item.Amount == 0)
        {
            Globals.Botbase.NotifyItemFatal("Item for Stats is unavailable", allowSkipInQueue: true);
            return;
        }

        config.Item = item;

        var attributes = config.Attributes?.ToList();
        if (attributes == null || attributes.Count == 0)
        {
            Globals.Botbase.NotifyItemFatal("No attributes configured", allowSkipInQueue: true);
            return;
        }

        foreach (var attribute in attributes)
        {
            if (!_shouldRun)
                break;

            var slot = ItemAttributesInfo.GetAttributeSlotForItem(attribute.Group, item.Record);
            var currentValue = item.Attributes.GetPercentage(slot);

            if (currentValue >= attribute.MaxValue && attribute.MaxValue > 0)
                continue;

            var liveStone = AlchemyItemHelper.GetAttributeStones(item, attribute.Group).FirstOrDefault();
            if (liveStone == null || liveStone.Amount == 0)
                continue;

            if (!AlchemyManager.TryFuseAttributeStone(item, liveStone))
                continue;

            _shouldRun = false;
            return;
        }

        if (_shouldRun)
        {
            Log.Notify("[Alchemy] Attribute stone fusing finished.");
            Globals.View.AddLog(item.Record.GetRealName(), "Stats finished");
            Globals.Botbase.NotifyItemCompleted("Stats completed");
        }
    }

    private void OnFuseRequest(AlchemyAction action, AlchemyType type)
    {
        if (type != AlchemyType.AttributeStone || !Bootstrap.IsActive)
            return;

        _shouldRun = false;
    }

    private void OnStoneAlchemy(AlchemyType type)
    {
        if (type != AlchemyType.AttributeStone || !Bootstrap.IsActive)
            return;

        _shouldRun = true;
    }

    private void OnStoneAlchemySuccess(InventoryItem oldItem, InventoryItem newItem, AlchemyType type)
    {
        if (type != AlchemyType.AttributeStone || !Bootstrap.IsActive || newItem == null)
            return;

        _shouldRun = true;

        if (Globals.Botbase.AttributeBundleConfig?.Item?.Slot == newItem.Slot)
            Globals.Botbase.AttributeBundleConfig.Item = newItem;

        if (oldItem != null)
        {
            var changedSlots = oldItem.Attributes.CompareSlots(newItem.Attributes);
            foreach (var slot in changedSlots)
            {
                var group = ItemAttributesInfo.GetAttributeGroupBySlot(newItem.Record, slot);
                var val = newItem.Attributes.GetPercentage(slot);
                Globals.View.AddLog(newItem.Record.GetRealName(),
                    $"[{group.GetTranslation()}] -> +{val}%");
            }
        }

        Globals.Botbase.NotifyItemProgressed(newItem);
    }

    private void OnStoneAlchemyFailed(InventoryItem oldItem, InventoryItem newItem, AlchemyType type)
    {
        if (type != AlchemyType.AttributeStone || !Bootstrap.IsActive)
            return;

        _shouldRun = true;
        if (newItem != null)
        {
            if (Globals.Botbase.AttributeBundleConfig?.Item?.Slot == newItem.Slot)
                Globals.Botbase.AttributeBundleConfig.Item = newItem;

            Globals.View.AddLog(newItem.Record.GetRealName(),
                Game.ReferenceManager.GetTranslation("UIIT_MSG_REINFORCERR_FAIL"));
            Globals.Botbase.NotifyItemProgressed(newItem);
        }
    }

    private void OnStoneAlchemyError(ushort errorCode, AlchemyType type)
    {
        if (type != AlchemyType.AttributeStone || !Bootstrap.IsActive)
            return;

        _shouldRun = true;
        Globals.View.AddLog(Globals.Botbase.AttributeBundleConfig?.Item?.Record?.GetRealName() ?? string.Empty,
            Game.ReferenceManager.GetTranslation("UIIT_MSG_REINFORCERR_FAIL"));
    }
}
