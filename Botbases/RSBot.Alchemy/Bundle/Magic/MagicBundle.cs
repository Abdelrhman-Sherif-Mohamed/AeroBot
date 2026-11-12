using System;
using System.Linq;
using RSBot.Alchemy.Bot;
using RSBot.Alchemy.Client.ReferenceObjects;
using RSBot.Alchemy.Extension;
using RSBot.Alchemy.Helper;
using RSBot.Core;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Item;

namespace RSBot.Alchemy.Bundle.Magic;

internal class MagicBundle : IAlchemyBundle
{
    private bool _shouldRun = true;

    public MagicBundle()
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
        if (engineConfig is not MagicBundleConfig config)
            return;

        if (!_shouldRun || (Globals.Botbase.AlchemyEngine != AlchemyEngine.Magic &&
                            Globals.Botbase.AlchemyEngine != AlchemyEngine.Queue))
            return;

        if (config.Item == null)
        {
            Globals.Botbase.NotifyItemFatal("No item configured for Blues", allowSkipInQueue: true);
            return;
        }

        var item = Game.Player.Inventory.GetItemAt(config.Item.Slot);
        if (item == null || item.ItemId != config.Item.ItemId || item.Amount == 0)
        {
            Globals.Botbase.NotifyItemFatal("Item for Blues is unavailable", allowSkipInQueue: true);
            return;
        }

        config.Item = item;

        if (config.Targets == null || config.Targets.Count == 0)
        {
            Globals.Botbase.NotifyItemFatal("No magic stones configured", allowSkipInQueue: true);
            return;
        }

        foreach (var target in config.Targets)
        {
            if (target?.ReferenceOption == null || string.IsNullOrEmpty(target.Group))
                continue;

            var magicStone = AlchemyItemHelper.GetStonesByGroup(item, target.Group).FirstOrDefault();
            if (magicStone == null || magicStone.Amount == 0)
                continue;

            var current = item.MagicOptions?.FirstOrDefault(m => m.Record?.Group == target.Group);

            if (target.Group == RefMagicOpt.MaterialAstral)
            {
                var immortalInfo = item.MagicOptions?.FirstOrDefault(m =>
                    Game.ReferenceManager.GetMagicOption(m.Id)?.Group == RefMagicOpt.MaterialImmortal);

                if (immortalInfo == null || immortalInfo.Value <= (current?.Value ?? 0))
                    continue;
            }

            if (current != null && current.Record != null && current.Record.Level != item.Record.Degree)
            {
                var actualOpt = Game.ReferenceManager.GetMagicOption(current.Record.Group, (byte)item.Record.Degree);
                if (actualOpt != null)
                    current = new MagicOptionInfo { Id = actualOpt.Id, Value = current.Value };
            }

            var maxAllowed = target.ReferenceOption.GetMaxValue();
            var desiredTarget = target.TargetValue > 0 && target.TargetValue <= maxAllowed
                ? target.TargetValue
                : maxAllowed;

            if (current != null && current.Value >= desiredTarget)
                continue;

            if (!AlchemyManager.TryFuseMagicStone(item, magicStone))
                continue;

            _shouldRun = false;
            return;
        }

        if (_shouldRun)
        {
            Log.Notify("[Alchemy] Magic stone fusing finished.");
            Globals.View.AddLog(item.Record.GetRealName(), "Blues finished");
            Globals.Botbase.NotifyItemCompleted("Blues completed");
        }
    }

    private static string GetErrorTranslationName(ushort errorCode) => errorCode switch
    {
        21536 or 21540 or 21543 => "UIIT_MSG_STRGERR_ASTRAL",
        21537 => "UIIT_MSG_ENCHANT_EQUIPCLASS_MISMATCH",
        21538 => "UIIT_MSG_STRGERR_MAGICSTONE2_MAX",
        21541 => "UIIT_MSG_TC_ERROR_ENCHANT_HONOR_ITEM",
        21542 => "UIIT_MSG_REINFORCERR_ALREADY_DONE",
        _ => "UIIT_MSG_ENCHANT_FAILED"
    };

    private void OnFuseRequest(AlchemyAction action, AlchemyType type)
    {
        if (type != AlchemyType.MagicStone || !Bootstrap.IsActive)
            return;

        _shouldRun = false;
    }

    private void OnStoneAlchemy(AlchemyType type)
    {
        if (type != AlchemyType.MagicStone || !Bootstrap.IsActive)
            return;

        _shouldRun = true;
    }

    private void OnStoneAlchemySuccess(InventoryItem oldItem, InventoryItem newItem, AlchemyType type)
    {
        if (type != AlchemyType.MagicStone || !Bootstrap.IsActive || newItem == null)
            return;

        _shouldRun = true;

        if (Globals.Botbase.MagicBundleConfig != null && oldItem != null &&
            Globals.Botbase.MagicBundleConfig.Item?.Slot == oldItem.Slot)
        {
            Globals.Botbase.MagicBundleConfig.Item = newItem;
        }

        if (oldItem?.MagicOptions != null && newItem.MagicOptions != null)
        {
            foreach (var newOpt in newItem.MagicOptions)
            {
                var oldOpt = oldItem.MagicOptions.FirstOrDefault(m => m.Id == newOpt.Id);
                if (oldOpt != null && oldOpt.Value == newOpt.Value)
                    continue;

                var record = Game.ReferenceManager.GetMagicOption(newOpt.Id);
                if (record == null)
                    break;

                var message = oldOpt != null
                    ? Game.ReferenceManager.GetTranslation("UIIT_MSG_ALCHEMY_CHANGE_CATTR")
                        .JoymaxFormat(newItem.Record.GetRealName(), record.GetGroupTranslation(),
                            $"{oldOpt.Value} -> {newOpt.Value}")
                    : Game.ReferenceManager.GetTranslation("UIIT_MSG_ALCHEMY_APPEND_ATTR")
                        .JoymaxFormat(record.GetGroupTranslation(), newItem.Record.GetRealName());

                Globals.View.AddLog(newItem.Record.GetRealName(), message);
                break;
            }
        }

        Globals.Botbase.NotifyItemProgressed(newItem);
    }

    private void OnStoneAlchemyFailed(InventoryItem oldItem, InventoryItem newItem, AlchemyType type)
    {
        if (type != AlchemyType.MagicStone || !Bootstrap.IsActive)
            return;

        _shouldRun = true;
        if (newItem != null)
        {
            if (Globals.Botbase.MagicBundleConfig?.Item?.Slot == newItem.Slot)
                Globals.Botbase.MagicBundleConfig.Item = newItem;

            Globals.View.AddLog(newItem.Record.GetRealName(),
                Game.ReferenceManager.GetTranslation("UIIT_MSG_REINFORCERR_FAIL"));
            Globals.Botbase.NotifyItemProgressed(newItem);
        }
    }

    private void OnStoneAlchemyError(ushort errorCode, AlchemyType type)
    {
        if (type != AlchemyType.MagicStone || !Bootstrap.IsActive)
            return;

        var translationName = GetErrorTranslationName(errorCode);
        var itemName = AlchemyManager.ActiveAlchemyItems?.FirstOrDefault()?.Record?.GetRealName() ?? string.Empty;
        Globals.View.AddLog(itemName, Game.ReferenceManager.GetTranslation(translationName));

        _shouldRun = true;
    }
}
