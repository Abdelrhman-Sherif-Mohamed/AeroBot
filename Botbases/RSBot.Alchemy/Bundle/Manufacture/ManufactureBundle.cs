using System;
using System.Collections.Generic;
using System.Linq;
using RSBot.Alchemy.Bot;
using RSBot.Alchemy.Models;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Objects;

namespace RSBot.Alchemy.Bundle.Manufacture;

internal class ManufactureBundle : IAlchemyBundle
{
    private const string RondoCodeName = "ITEM_ETC_ARCHEMY_RONDO_01";

    private bool _shouldRun;
    private DateTime _nextAttempt = DateTime.MinValue;
    private ManufactureBundleConfig _activeConfig;
    private ManufactureQueueEntry _currentEntry;

    public string CurrentItemName { get; private set; } = "None";
    public int ManufacturedCount { get; private set; }
    public int FailedCount { get; private set; }

    public event System.Action StateChanged;

    public ManufactureBundle()
    {
        SubscribeEvents();
    }

    public void Start()
    {
        _shouldRun = true;
        _nextAttempt = DateTime.UtcNow;
        _currentEntry = null;
        CurrentItemName = "None";
        StateChanged?.Invoke();
    }

    public void Stop()
    {
        _shouldRun = false;
        if (_currentEntry != null && _currentEntry.Status == QueueItemStatus.Running)
            _currentEntry.Status = QueueItemStatus.Pending;

        _currentEntry = null;
        CurrentItemName = "None";
        AlchemyManager.CancelPending();
        StateChanged?.Invoke();
    }

    public void ResetCounters()
    {
        ManufacturedCount = 0;
        FailedCount = 0;
        CurrentItemName = "None";
        StateChanged?.Invoke();
    }

    public void Run<T>(T engineConfig)
    {
        if (engineConfig is not ManufactureBundleConfig config)
            return;

        _activeConfig = config;

        if (!_shouldRun || Globals.Botbase?.AlchemyEngine != AlchemyEngine.Manufacture)
            return;

        if (DateTime.UtcNow < _nextAttempt)
            return;

        // Pick next pending entry from Queue
        var entry = GetNextPendingEntry(config);
        if (entry != null)
        {
            RunQueueEntry(config, entry);
            return;
        }

        // Legacy fallback if direct materials list is set without queue entries
        if (config.Materials != null && config.Materials.Count > 0)
        {
            var maxCount = config.Mode == ManufactureMode.Compound ? 1 : 4;
            var materials = ResolveMaterials(config.Materials.Select(m => m.ItemId), maxCount);
            var craftName = materials.FirstOrDefault()?.Record != null
                ? Helper.AlchemyItemHelper.GetItemDisplayName(materials.First().Record)
                : "Craft";
            ExecuteCraft(materials, config.Mode, craftName);
            return;
        }

        Log.Notify("[Alchemy] Manufacture queue finished.");
        CurrentItemName = "None";
        StateChanged?.Invoke();
        Bootstrap.StopRunner();
    }

    private ManufactureQueueEntry GetNextPendingEntry(ManufactureBundleConfig config)
    {
        if (config.Queue == null || config.Queue.Count == 0)
            return null;

        if (_currentEntry != null && config.Queue.Contains(_currentEntry) &&
            _currentEntry.CompletedCount < _currentEntry.TargetCount &&
            _currentEntry.Status != QueueItemStatus.Failed &&
            _currentEntry.Status != QueueItemStatus.Completed)
        {
            return _currentEntry;
        }

        return config.Queue.FirstOrDefault(q =>
            q.Status is QueueItemStatus.Pending or QueueItemStatus.Running &&
            q.CompletedCount < q.TargetCount);
    }

    private void RunQueueEntry(ManufactureBundleConfig config, ManufactureQueueEntry entry)
    {
        _currentEntry = entry;
        entry.Status = QueueItemStatus.Running;
        CurrentItemName = entry.ItemName;

        if (GetRondoCount() <= 0)
        {
            Log.Warn("[Alchemy] No manufacture Rondo left in inventory.");
            Globals.View.AddLog(entry.ItemName, "No manufacture Rondo left");
            entry.Status = QueueItemStatus.Failed;
            entry.StatusNote = Kernel.Language == "ar_AR" ? "ناقص Rondo" : "No Rondo";
            _currentEntry = null;
            StateChanged?.Invoke();
            Bootstrap.StopRunner();
            return;
        }

        var maxCount = entry.Mode == ManufactureMode.Compound ? 1 : 4;
        var materials = ResolveMaterials(entry.Requirements.Select(r => r.ItemId), maxCount);

        if (materials.Count == 0)
        {
            entry.Status = QueueItemStatus.Failed;
            entry.StatusNote = Kernel.Language == "ar_AR" ? "ناقص خامات" : "No Materials";
            FailedCount++;
            _currentEntry = null;
            StateChanged?.Invoke();
            return;
        }

        ExecuteCraft(materials, entry.Mode, entry.ItemName);
    }

    private void ExecuteCraft(List<InventoryItem> materials, ManufactureMode mode, string label)
    {
        if (materials.Count == 0 || GetRondoCount() <= 0)
        {
            Bootstrap.StopRunner();
            return;
        }

        var started = mode == ManufactureMode.Compound
            ? AlchemyManager.TryCompoundMaterials(materials)
            : AlchemyManager.TryResolveMaterials(materials);

        if (!started)
        {
            _nextAttempt = DateTime.UtcNow.AddSeconds(1);
            return;
        }

        CurrentItemName = label;
        _shouldRun = false;
        StateChanged?.Invoke();
    }

    private static List<InventoryItem> ResolveMaterials(IEnumerable<uint> itemIds, int maxCount)
    {
        var result = new List<InventoryItem>(4);
        var seen = new HashSet<uint>();

        foreach (var itemId in itemIds)
        {
            if (itemId == 0 || !seen.Add(itemId))
                continue;

            var available = Game.Player.Inventory.FirstOrDefault(i => i.ItemId == itemId && i.Amount > 0);
            if (available == null)
                continue;

            result.Add(available);
            if (result.Count >= maxCount)
                break;
        }

        return result;
    }

    public static int GetRondoCount()
    {
        return Game.Player?.Inventory == null
            ? 0
            : Game.Player.Inventory.GetItems(RondoCodeName).Sum(i => (int)i.Amount);
    }

    private void SubscribeEvents()
    {
        EventManager.SubscribeEvent("OnMakeStuffResult",
            new Action<bool, ushort, byte, uint, bool>(OnMakeStuffResult));
        EventManager.SubscribeEvent("OnMakeStuffCanceled", new System.Action(OnMakeStuffCanceled));
    }

    private void OnMakeStuffResult(bool success, ushort errorCode, byte extra, uint value, bool completed)
    {
        if (!Bootstrap.IsActive || Globals.Botbase?.AlchemyEngine != AlchemyEngine.Manufacture)
            return;

        if (!success)
        {
            _shouldRun = false;
            FailedCount++;
            if (_currentEntry != null)
            {
                _currentEntry.FailedCount++;
                _currentEntry.Status = QueueItemStatus.Failed;
                _currentEntry.StatusNote = $"Err 0x{errorCode:X4}";
                _currentEntry = null;
            }

            var message = $"[Alchemy] Manufacture failed with error 0x{errorCode:X4}";
            Log.Warn(message);
            Globals.View.AddLog(CurrentItemName, message);
            StateChanged?.Invoke();
            Bootstrap.StopRunner();
            return;
        }

        if (!completed)
            return;

        ManufacturedCount++;
        if (_currentEntry != null)
        {
            _currentEntry.CompletedCount++;
            if (_currentEntry.CompletedCount >= _currentEntry.TargetCount)
            {
                _currentEntry.Status = QueueItemStatus.Completed;
                _currentEntry = null;
            }
        }

        var delayMs = _activeConfig != null ? Math.Clamp(_activeConfig.DelayMs, 100, 10_000) : 500;
        _shouldRun = true;
        _nextAttempt = DateTime.UtcNow.AddMilliseconds(delayMs);

        Globals.View.AddLog(CurrentItemName, "[Alchemy] Manufacture completed.");
        StateChanged?.Invoke();
    }

    private void OnMakeStuffCanceled()
    {
        if (!Bootstrap.IsActive || Globals.Botbase?.AlchemyEngine != AlchemyEngine.Manufacture)
            return;

        _shouldRun = true;
        _nextAttempt = DateTime.UtcNow.AddMilliseconds(500);
        Globals.View.AddLog(CurrentItemName, "[Alchemy] Manufacture canceled.");
        StateChanged?.Invoke();
    }
}
