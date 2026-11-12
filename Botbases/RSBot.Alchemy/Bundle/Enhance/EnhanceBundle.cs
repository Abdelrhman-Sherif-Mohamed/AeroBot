using System;
using System.Collections.Generic;
using System.Linq;
using RSBot.Alchemy.Bot;
using RSBot.Alchemy.Extension;
using RSBot.Alchemy.Helper;
using RSBot.Core;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Objects;

namespace RSBot.Alchemy.Bundle.Enhance;

internal class EnhanceBundle : IAlchemyBundle
{
    private EnhanceBundleConfig _config;
    private bool _shouldRun = true;
    private bool _isStoneFusing;
    private bool _skipAstralForCurrentItem;

    public EnhanceBundle()
    {
        SubscribeEvents();
    }

    public void Start()
    {
        _shouldRun = true;
        _isStoneFusing = false;
        _skipAstralForCurrentItem = false;
    }

    public void Stop()
    {
        _shouldRun = false;
        _isStoneFusing = false;
        _config = null;

        AlchemyManager.CancelPending();
    }

    private void SubscribeEvents()
    {
        EventManager.SubscribeEvent("OnAlchemyDestroyed",
            new Action<InventoryItem, AlchemyType>(OnElixirAlchemyDestroyed));
        EventManager.SubscribeEvent("OnAlchemySuccess",
            new Action<InventoryItem, InventoryItem, AlchemyType>(OnElixirAlchemySuccess));
        EventManager.SubscribeEvent("OnAlchemy", new Action<AlchemyType>(OnElixirAlchemy));
        EventManager.SubscribeEvent("OnAlchemyFailed",
            new Action<InventoryItem, InventoryItem, AlchemyType>(OnElixirAlchemyFailed));
        EventManager.SubscribeEvent("OnFuseRequest", new Action<AlchemyAction, AlchemyType>(OnFuseRequest));
    }

    public void Run<T>(T engineConfig)
    {
        if (engineConfig is not EnhanceBundleConfig config)
            return;

        if (!_shouldRun || (Globals.Botbase.AlchemyEngine != AlchemyEngine.Enhance &&
                            Globals.Botbase.AlchemyEngine != AlchemyEngine.Queue))
            return;

        if (config.Item == null)
        {
            Globals.Botbase.NotifyItemFatal("No item configured", allowSkipInQueue: true);
            return;
        }

        var item = Game.Player.Inventory.GetItemAt(config.Item.Slot);
        if (item == null || item.ItemId != config.Item.ItemId || item.Amount == 0)
        {
            Globals.Botbase.NotifyItemFatal("Item to enhance is no longer in inventory", allowSkipInQueue: true);
            return;
        }

        config.Item = item;
        _config = config;

        if (item.OptLevel >= config.MaxOptLevel)
        {
            var msg = $"Reached +{item.OptLevel}/{config.MaxOptLevel}";
            Globals.View.AddLog(item.Record.GetRealName(), msg);
            Globals.Botbase.NotifyItemCompleted(msg);
            return;
        }

        var elixir = ResolveElixir(item, config);
        if (elixir == null)
        {
            Globals.Botbase.NotifyItemFatal("No matching elixir available", allowSkipInQueue: false);
            return;
        }

        InventoryItem powder = null;
        if (config.UseLuckyPowder && item.OptLevel >= config.LuckyPowderMinPlus)
        {
            var powders = AlchemyItemHelper.GetLuckyPowders(item).ToList();
            powder = powders.FirstOrDefault();

            if (powder == null && config.StopIfLuckyPowderEmpty)
            {
                Log.Warn("[Alchemy] No Lucky Powder left for degree " + item.Record.Degree);
                Globals.View.AddLog(item.Record.GetRealName(), "No Lucky Powder left — stopped");
                Globals.Botbase.NotifyItemFatal("No Lucky Powder left", allowSkipInQueue: false);
                return;
            }
        }

        // Priority order for support stones: Steady -> Lucky -> Immortal -> Astral
        if (TryFuseSupportStone(item, config.UseSteadyStones, config.SteadyStoneMinPlus,
                RefMagicOpt.MaterialSteady, () => AlchemyItemHelper.GetSteadyStone(item)))
            return;

        if (TryFuseSupportStone(item, config.UseLuckyStones, config.LuckyStoneMinPlus,
                RefMagicOpt.MaterialLuck, () => AlchemyItemHelper.GetLuckyStone(item)))
            return;

        if (TryFuseSupportStone(item, config.UseImmortalStones, config.ImmortalStoneMinPlus,
                RefMagicOpt.MaterialImmortal, () => AlchemyItemHelper.GetImmortalStone(item)))
            return;

        if (config.UseAstralStones && !_skipAstralForCurrentItem && item.OptLevel >= config.AstralStoneMinPlus &&
            !AlchemyItemHelper.HasMagicOption(item, RefMagicOpt.MaterialAstral))
        {
            var astralStone = AlchemyItemHelper.GetAstralStone(item);
            if (astralStone != null && astralStone.Amount > 0)
            {
                var immortalRef = Game.ReferenceManager.GetMagicOption(RefMagicOpt.MaterialImmortal,
                    (byte)item.Record.Degree);
                var immortalInfo = immortalRef != null
                    ? item.MagicOptions?.FirstOrDefault(m => m.Id == immortalRef.Id)
                    : null;

                if (immortalInfo == null)
                {
                    Log.Notify($"[Alchemy] Skipping Astral Stone on {item.Record.GetRealName()} — Immortal required first.");
                    _skipAstralForCurrentItem = true;
                }
                else if (AlchemyManager.TryFuseMagicStone(item, astralStone))
                {
                    _shouldRun = false;
                    _isStoneFusing = true;
                    return;
                }
            }
        }

        if (!AlchemyManager.TryFuseElixir(item, elixir, powder))
            return;

        _shouldRun = false;
    }

    private bool TryFuseSupportStone(InventoryItem item, bool enabled, byte minPlus, string materialGroup,
        Func<InventoryItem> stoneResolver)
    {
        if (!enabled || item.OptLevel < minPlus)
            return false;

        if (AlchemyItemHelper.HasMagicOption(item, materialGroup))
            return false;

        var stone = stoneResolver();
        if (stone == null || stone.Amount == 0)
            return false;

        if (!AlchemyManager.TryFuseMagicStone(item, stone))
            return false;

        _shouldRun = false;
        _isStoneFusing = true;
        return true;
    }

    private static InventoryItem ResolveElixir(InventoryItem item, EnhanceBundleConfig config)
    {
        var elixirType = AlchemyItemHelper.GetElixirTypeForItem(item);
        var matchingElixirs = AlchemyItemHelper.GetElixirItems(elixirType).ToList();

        if (config.PreferredElixirItemId.HasValue && config.PreferredElixirItemId.Value > 0)
        {
            var preferred = matchingElixirs.FirstOrDefault(i => i.ItemId == config.PreferredElixirItemId.Value);
            if (preferred != null)
                return preferred;
        }

        if (config.Elixirs != null)
        {
            foreach (var configured in config.Elixirs)
            {
                if (configured == null) continue;
                var live = matchingElixirs.FirstOrDefault(i => i.ItemId == configured.ItemId);
                if (live != null)
                    return live;
            }
        }

        return matchingElixirs.FirstOrDefault();
    }

    private void OnFuseRequest(AlchemyAction action, AlchemyType type)
    {
        if (!Bootstrap.IsActive)
            return;

        _shouldRun = false;
    }

    private void OnElixirAlchemy(AlchemyType type)
    {
        if (!Bootstrap.IsActive)
            return;

        if (Globals.Botbase.AlchemyEngine is AlchemyEngine.Enhance or AlchemyEngine.Queue)
            _shouldRun = true;
    }

    private void OnElixirAlchemySuccess(InventoryItem oldItem, InventoryItem newItem, AlchemyType type)
    {
        if (!Bootstrap.IsActive || (Globals.Botbase.AlchemyEngine != AlchemyEngine.Enhance &&
                                    Globals.Botbase.AlchemyEngine != AlchemyEngine.Queue))
            return;

        if (_isStoneFusing)
        {
            _isStoneFusing = false;
            _shouldRun = true;
            if (newItem != null && _config != null)
                _config.Item = newItem;
            return;
        }

        if (type != AlchemyType.Elixir || newItem == null)
            return;

        _skipAstralForCurrentItem = false;

        var rawTemplate = Game.ReferenceManager.GetTranslation("UIIT_MSG_REINFORCERR_SUCCESS");
        var message = !string.IsNullOrEmpty(rawTemplate)
            ? rawTemplate.JoymaxFormat(newItem.OptLevel)
            : $"+{newItem.OptLevel} success";

        Log.Notify(message);
        Globals.View.AddLog(newItem.Record.GetRealName(), message);

        if (_config != null)
            _config.Item = newItem;

        _shouldRun = true;
        Globals.Botbase.NotifyItemProgressed(newItem);
    }

    private void OnElixirAlchemyFailed(InventoryItem oldItem, InventoryItem newItem, AlchemyType type)
    {
        if (!Bootstrap.IsActive || (Globals.Botbase.AlchemyEngine != AlchemyEngine.Enhance &&
                                    Globals.Botbase.AlchemyEngine != AlchemyEngine.Queue))
            return;

        if (_isStoneFusing)
        {
            _isStoneFusing = false;
            _shouldRun = true;
            if (newItem != null && _config != null)
                _config.Item = newItem;
            return;
        }

        if (type != AlchemyType.Elixir || newItem == null)
            return;

        _shouldRun = true;

        var failMessage = Game.ReferenceManager.GetTranslation("UIIT_MSG_REINFORCERR_FAIL");
        Log.Warn(failMessage);
        Globals.View.AddLog(newItem.Record.GetRealName(), failMessage);

        if (oldItem != null)
        {
            var detail = string.Empty;
            if (newItem.Durability < oldItem.Durability)
            {
                detail = Game.ReferenceManager.GetTranslation("UIIT_MSG_REINFORCERR_FAILDOWN_DURABILITY")
                    ?.JoymaxFormat(newItem.Durability) ?? string.Empty;
            }
            else if (oldItem.OptLevel > 0 && newItem.OptLevel == 0)
            {
                detail = Game.ReferenceManager.GetTranslation("UIIT_MSG_REINFORCERR_FAIL_RESULT_OPTLV_ZERO") ?? string.Empty;
            }
            else if (oldItem.OptLevel > 0 && newItem.OptLevel < oldItem.OptLevel)
            {
                detail = Game.ReferenceManager.GetTranslation("UIIT_MSG_REINFORCERR_FAIL_RESULT_OPTLV_DOWN")
                    ?.JoymaxFormat(newItem.OptLevel, oldItem.OptLevel - newItem.OptLevel) ?? string.Empty;
            }

            if (!string.IsNullOrEmpty(detail))
            {
                Log.Debug(detail);
                Globals.View.AddLog(newItem.Record.GetRealName(), detail);
            }
        }

        if (_config != null)
            _config.Item = newItem;

        Globals.Botbase.NotifyEnhanceAttemptFailed(newItem);
    }

    private void OnElixirAlchemyDestroyed(InventoryItem oldItem, AlchemyType type)
    {
        _shouldRun = false;
        _isStoneFusing = false;
    }
}
