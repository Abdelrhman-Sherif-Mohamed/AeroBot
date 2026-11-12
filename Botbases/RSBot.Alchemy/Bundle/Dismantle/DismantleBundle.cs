using System;
using System.Collections.Generic;
using System.Linq;
using RSBot.Alchemy.Bot;
using RSBot.Alchemy.Helper;
using RSBot.Alchemy.Models;
using RSBot.Core;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Objects;

namespace RSBot.Alchemy.Bundle.Dismantle;

internal class DismantleBundle : IAlchemyBundle
{
    private const string RondoCodeName = "ITEM_ETC_ARCHEMY_RONDO_02";

    private bool _shouldRun;
    private DateTime _nextAttempt = DateTime.MinValue;
    private DismantleBundleConfig _lastConfig;
    private Dictionary<uint, long> _beforeSnapshot;
    private int _pendingBatchCount;

    public int ProcessedCount { get; private set; }
    public int TotalBatchTarget { get; private set; }
    public string StatusText { get; private set; } = string.Empty;

    public event System.Action StateChanged;

    public DismantleBundle()
    {
        SubscribeEvents();
    }

    public void Start()
    {
        _shouldRun = true;
        _nextAttempt = DateTime.UtcNow;
        ProcessedCount = 0;
        TotalBatchTarget = 0;
        _pendingBatchCount = 0;
        _beforeSnapshot = null;
        StateChanged?.Invoke();
    }

    public void Stop()
    {
        _shouldRun = false;
        _pendingBatchCount = 0;
        _beforeSnapshot = null;
        AlchemyManager.CancelPending();
        StateChanged?.Invoke();
    }

    public void Run<T>(T engineConfig)
    {
        if (engineConfig is not DismantleBundleConfig config)
            return;

        _lastConfig = config;

        if (!_shouldRun || Globals.Botbase?.AlchemyEngine != AlchemyEngine.Dismantle)
            return;

        if (DateTime.UtcNow < _nextAttempt)
            return;

        if (config.DismantleWhenBagFull && config.AutoDismantle && Game.Player?.Inventory != null)
        {
            var freeSlots = Game.Player.Inventory.NormalPartSize - Game.Player.Inventory.GetNormalPartItems().Count;
            if (!Game.Player.Inventory.Full && freeSlots > 2)
                return;
        }

        if (GetRondoCount() <= 0)
        {
            Log.Warn("[Alchemy] No dismantling Rondo left in inventory.");
            Globals.View.AddLog("Dismantle", "No dismantling Rondo left");
            Bootstrap.StopRunner();
            return;
        }

        var items = ResolveTargetEquipment(config);
        if (items.Count == 0)
        {
            if (config.AutoDismantle)
                return;

            Log.Notify("[Alchemy] No matching equipment to dismantle.");
            Bootstrap.StopRunner();
            return;
        }

        if (TotalBatchTarget == 0)
            TotalBatchTarget = items.Count;

        var batch = items.Take(9).ToList();
        _pendingBatchCount = batch.Count;
        _beforeSnapshot = CaptureMaterialSnapshot();

        if (!AlchemyManager.TryDismantle(batch))
        {
            _pendingBatchCount = 0;
            _beforeSnapshot = null;
            _nextAttempt = DateTime.UtcNow.AddSeconds(1);
            return;
        }

        _shouldRun = false;
        StateChanged?.Invoke();
    }

    private static List<InventoryItem> ResolveTargetEquipment(DismantleBundleConfig config)
    {
        var queuedKeys = Globals.Botbase?.Queue
            .Select(q => (q.Slot, q.ItemId))
            .ToHashSet() ?? new HashSet<(byte, uint)>();

        var inventoryEquips = AlchemyItemHelper.GetEligibleEquipmentItems()
            .Where(i => !queuedKeys.Contains((i.Slot, i.ItemId)))
            .ToList();

        var orderedFilters = (config.Filters ?? new List<DismantleFilterRule>())
            .OrderBy(f => f.Priority)
            .ToList();

        var selected = new List<InventoryItem>();
        var usedSlots = new HashSet<byte>();

        // 1. Add items matching explicit filters in priority order
        foreach (var filter in orderedFilters)
        {
            foreach (var eq in inventoryEquips)
            {
                if (usedSlots.Contains(eq.Slot))
                    continue;

                if (!filter.Matches(eq))
                    continue;

                if (!filter.SpecificItemId.HasValue && config.UseMaxLevelFilter && eq.Record.ReqLevel1 >= config.MaxItemLevel)
                    continue;

                selected.Add(eq);
                usedSlots.Add(eq.Slot);
            }
        }

        // 2. If not restricted to explicitly selected filter items, also match level filter
        if (!config.OnlySelectedItems && config.UseMaxLevelFilter)
        {
            foreach (var eq in inventoryEquips)
            {
                if (usedSlots.Contains(eq.Slot))
                    continue;

                if (eq.Record.ReqLevel1 < config.MaxItemLevel)
                {
                    selected.Add(eq);
                    usedSlots.Add(eq.Slot);
                }
            }
        }

        return selected;
    }

    public static int GetRondoCount()
    {
        return Game.Player?.Inventory == null
            ? 0
            : Game.Player.Inventory.GetItems(RondoCodeName).Sum(i => (int)i.Amount);
    }

    private static Dictionary<uint, long> CaptureMaterialSnapshot()
    {
        var map = new Dictionary<uint, long>();
        if (Game.Player?.Inventory == null)
            return map;

        foreach (var item in AlchemyItemHelper.GetManufactureMaterials())
        {
            map[item.ItemId] = map.TryGetValue(item.ItemId, out var cur)
                ? cur + item.Amount
                : item.Amount;
        }

        return map;
    }

    private void RecordOutputDelta()
    {
        if (_beforeSnapshot == null || _lastConfig?.Outputs == null || Game.Player?.Inventory == null)
            return;

        var after = CaptureMaterialSnapshot();
        foreach (var kv in after)
        {
            _beforeSnapshot.TryGetValue(kv.Key, out var prevAmount);
            var delta = kv.Value - prevAmount;
            if (delta <= 0)
                continue;

            var refItem = Game.ReferenceManager.GetRefItem(kv.Key);
            var name = refItem != null ? AlchemyItemHelper.GetItemDisplayName(refItem) : $"Item #{kv.Key}";
            var unitValue = refItem != null ? Math.Max(1, refItem.Degree) * 100L : 100L;

            var existing = _lastConfig.Outputs.FirstOrDefault(o => o.ItemId == kv.Key);
            if (existing != null)
            {
                existing.Quantity += delta;
            }
            else
            {
                _lastConfig.Outputs.Add(new DismantleOutputEntry
                {
                    ItemId = kv.Key,
                    MaterialName = name,
                    Quantity = delta,
                    UnitPrice = unitValue
                });
            }
        }

        _beforeSnapshot = null;
    }

    private void SubscribeEvents()
    {
        EventManager.SubscribeEvent("OnDismantleResult", new Action<bool, ushort>(OnDismantleResult));
    }

    private void OnDismantleResult(bool success, ushort errorCode)
    {
        if (!Bootstrap.IsActive || Globals.Botbase?.AlchemyEngine != AlchemyEngine.Dismantle)
            return;

        if (!success)
        {
            _shouldRun = false;
            _pendingBatchCount = 0;
            _beforeSnapshot = null;
            var message = $"[Alchemy] Dismantling failed with error 0x{errorCode:X4}";
            Log.Warn(message);
            Globals.View.AddLog("Dismantle", message);
            StateChanged?.Invoke();
            Bootstrap.StopRunner();
            return;
        }

        ProcessedCount += _pendingBatchCount > 0 ? _pendingBatchCount : 1;
        _pendingBatchCount = 0;
        RecordOutputDelta();

        _shouldRun = true;
        _nextAttempt = DateTime.UtcNow.AddMilliseconds(600);
        Globals.View.AddLog("Dismantle", "Dismantling completed.");
        StateChanged?.Invoke();
    }
}
