using System;
using System.Collections.Generic;
using System.Linq;
using RSBot.Alchemy.Bundle;
using RSBot.Alchemy.Bundle.Attribute;
using RSBot.Alchemy.Bundle.Dismantle;
using RSBot.Alchemy.Bundle.Enhance;
using RSBot.Alchemy.Bundle.Magic;
using RSBot.Alchemy.Bundle.Manufacture;
using RSBot.Alchemy.Helper;
using RSBot.Alchemy.Models;
using RSBot.Alchemy.Stats;
using RSBot.Core;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Objects;

namespace RSBot.Alchemy.Bot;

internal enum AlchemyEngine
{
    Queue,
    Enhance,
    Magic,
    Attribute,
    Manufacture,
    Dismantle
}

internal class Botbase
{
    private int _queueCursor;

    public Botbase()
    {
        EnhanceBundle = new EnhanceBundle();
        MagicBundle = new MagicBundle();
        AttributeBundle = new AttributeBundle();
        ManufactureBundle = new ManufactureBundle();
        DismantleBundle = new DismantleBundle();

        SessionStats = new AlchemySessionStats();
        SessionStats.SubscribeEvents();
    }

    public AlchemyEngine AlchemyEngine { get; set; } = AlchemyEngine.Queue;

    public EnhanceBundle EnhanceBundle { get; }
    public AttributeBundle AttributeBundle { get; }
    public MagicBundle MagicBundle { get; }
    public ManufactureBundle ManufactureBundle { get; }
    public DismantleBundle DismantleBundle { get; }

    public EnhanceBundleConfig EnhanceBundleConfig { get; set; }
    public MagicBundleConfig MagicBundleConfig { get; set; }
    public AttributeBundleConfig AttributeBundleConfig { get; set; }
    public ManufactureBundleConfig ManufactureBundleConfig { get; set; } = new();
    public DismantleBundleConfig DismantleBundleConfig { get; set; } = new();

    public List<AlchemyQueueEntry> Queue { get; } = new();
    public AlchemyQueueEntry CurrentQueueEntry { get; private set; }
    public AlchemySessionStats SessionStats { get; }

    public event System.Action QueueChanged;

    public void Start()
    {
        SessionStats.NotifyBotStarted();

        switch (AlchemyEngine)
        {
            case AlchemyEngine.Queue:
                if (Queue.Count == 0)
                {
                    Log.Warn("[Alchemy] Alchemy queue is empty.");
                    Bootstrap.StopRunner();
                    return;
                }

                // Reset any stale Running flag back to Pending
                foreach (var entry in Queue)
                {
                    if (entry.Status == QueueItemStatus.Running)
                        entry.Status = QueueItemStatus.Pending;
                }

                _queueCursor = 0;
                CurrentQueueEntry = null;
                EnhanceBundle.Start();
                MagicBundle.Start();
                AttributeBundle.Start();
                QueueChanged?.Invoke();
                break;

            case AlchemyEngine.Enhance:
                if (EnhanceBundleConfig == null)
                {
                    Log.Warn("[Alchemy] Enhance configuration missing.");
                    Bootstrap.StopRunner();
                    return;
                }
                EnhanceBundle.Start();
                break;

            case AlchemyEngine.Magic:
                if (MagicBundleConfig == null)
                {
                    Log.Warn("[Alchemy] Magic configuration missing.");
                    Bootstrap.StopRunner();
                    return;
                }
                MagicBundle.Start();
                break;

            case AlchemyEngine.Attribute:
                if (AttributeBundleConfig == null)
                {
                    Log.Warn("[Alchemy] Attribute configuration missing.");
                    Bootstrap.StopRunner();
                    return;
                }
                AttributeBundle.Start();
                break;

            case AlchemyEngine.Manufacture:
                ManufactureBundleConfig ??= new ManufactureBundleConfig();
                if ((ManufactureBundleConfig.Queue == null || ManufactureBundleConfig.Queue.Count == 0) &&
                    (ManufactureBundleConfig.Materials == null || ManufactureBundleConfig.Materials.Count == 0))
                {
                    Log.Warn("[Alchemy] No items or materials queued for manufacture.");
                    Bootstrap.StopRunner();
                    return;
                }
                ManufactureBundle.Start();
                break;

            case AlchemyEngine.Dismantle:
                DismantleBundleConfig ??= new DismantleBundleConfig();
                DismantleBundle.Start();
                break;
        }
    }

    public void Stop()
    {
        SessionStats.NotifyBotStopped();

        if (CurrentQueueEntry != null && CurrentQueueEntry.Status == QueueItemStatus.Running)
            CurrentQueueEntry.Status = QueueItemStatus.Pending;

        CurrentQueueEntry = null;

        EnhanceBundle?.Stop();
        MagicBundle?.Stop();
        AttributeBundle?.Stop();
        ManufactureBundle?.Stop();
        DismantleBundle?.Stop();

        QueueChanged?.Invoke();
    }

    public void Tick()
    {
        switch (AlchemyEngine)
        {
            case AlchemyEngine.Queue:
                TickQueue();
                break;

            case AlchemyEngine.Enhance:
                if (EnhanceBundleConfig == null)
                {
                    Bootstrap.StopRunner();
                    return;
                }
                EnhanceBundle.Run(EnhanceBundleConfig);
                break;

            case AlchemyEngine.Magic:
                if (MagicBundleConfig == null)
                {
                    Bootstrap.StopRunner();
                    return;
                }
                MagicBundle.Run(MagicBundleConfig);
                break;

            case AlchemyEngine.Attribute:
                if (AttributeBundleConfig == null)
                {
                    Bootstrap.StopRunner();
                    return;
                }
                AttributeBundle.Run(AttributeBundleConfig);
                break;

            case AlchemyEngine.Manufacture:
                ManufactureBundle.Run(ManufactureBundleConfig);
                break;

            case AlchemyEngine.Dismantle:
                DismantleBundle.Run(DismantleBundleConfig);
                break;
        }
    }

    private void TickQueue()
    {
        if (CurrentQueueEntry == null || CurrentQueueEntry.Status != QueueItemStatus.Running)
        {
            CurrentQueueEntry = PickNextPendingEntry();
            if (CurrentQueueEntry == null)
            {
                Log.Notify("[Alchemy] All queued items finished!");
                Globals.View.AddLog("Queue", "All items finished");
                QueueChanged?.Invoke();
                Bootstrap.StopRunner();
                return;
            }

            CurrentQueueEntry.Status = QueueItemStatus.Running;
            CurrentQueueEntry.StatusNote = string.Empty;
            PrepareBundleForEntry(CurrentQueueEntry);
            QueueChanged?.Invoke();
        }

        switch (CurrentQueueEntry.Mode)
        {
            case AlchemyTargetMode.Plus:
                EnhanceBundle.Run(EnhanceBundleConfig);
                break;

            case AlchemyTargetMode.Blues:
                MagicBundle.Run(MagicBundleConfig);
                break;

            case AlchemyTargetMode.Stats:
                AttributeBundle.Run(AttributeBundleConfig);
                break;
        }
    }

    private AlchemyQueueEntry PickNextPendingEntry()
    {
        if (Queue.Count == 0)
            return null;

        for (var offset = 0; offset < Queue.Count; offset++)
        {
            var idx = (_queueCursor + offset) % Queue.Count;
            var candidate = Queue[idx];
            if (candidate.Status == QueueItemStatus.Pending)
            {
                _queueCursor = idx;
                return candidate;
            }
        }

        return null;
    }

    private void PrepareBundleForEntry(AlchemyQueueEntry entry)
    {
        var liveItem = entry.ResolveLiveItem();

        switch (entry.Mode)
        {
            case AlchemyTargetMode.Plus:
                EnhanceBundle.Start();
                EnhanceBundleConfig = new EnhanceBundleConfig
                {
                    Item = liveItem,
                    MaxOptLevel = entry.TargetPlus,
                    PreferredElixirItemId = entry.PreferredElixirItemId,
                    Elixirs = liveItem != null
                        ? AlchemyItemHelper.GetElixirItems(AlchemyItemHelper.GetElixirTypeForItem(liveItem)).ToList()
                        : new List<InventoryItem>(),
                    UseLuckyPowder = entry.UseLuckyPowder,
                    LuckyPowderMinPlus = entry.LuckyPowderFromPlus,
                    StopIfLuckyPowderEmpty = entry.StopIfLuckyPowderEmpty,
                    UseLuckyStones = entry.UseLuckyStone,
                    LuckyStoneMinPlus = entry.LuckyStoneFromPlus,
                    UseImmortalStones = entry.UseImmortalStone,
                    ImmortalStoneMinPlus = entry.ImmortalStoneFromPlus,
                    UseAstralStones = entry.UseAstralStone,
                    AstralStoneMinPlus = entry.AstralStoneFromPlus,
                    UseSteadyStones = entry.UseSteadyStone,
                    SteadyStoneMinPlus = entry.SteadyStoneFromPlus,
                    SkipToNextOnFail = entry.SkipToNextOnFail
                };
                break;

            case AlchemyTargetMode.Blues:
                MagicBundle.Start();
                MagicBundleConfig = new MagicBundleConfig
                {
                    Item = liveItem,
                    Targets = entry.Blues
                        .Where(b => b.Enabled)
                        .Select(b => new MagicBundleTarget
                        {
                            Group = b.Group,
                            TargetValue = b.TargetValue,
                            ReferenceOption = Game.ReferenceManager.GetMagicOption(b.Group, entry.Degree)
                        })
                        .Where(t => t.ReferenceOption != null)
                        .ToList()
                };
                break;

            case AlchemyTargetMode.Stats:
                AttributeBundle.Start();
                AttributeBundleConfig = new AttributeBundleConfig
                {
                    Item = liveItem,
                    Attributes = entry.Stats
                        .Where(s => s.Enabled)
                        .Select(s => new AttributeBundleConfig.AttributeBundleConfigItem
                        {
                            Group = s.Group,
                            MaxValue = s.TargetPercent,
                            Stone = liveItem != null
                                ? AlchemyItemHelper.GetAttributeStones(liveItem, s.Group).FirstOrDefault()
                                : null
                        })
                        .ToList()
                };
                break;
        }
    }

    public void NotifyItemProgressed(InventoryItem updatedItem)
    {
        if (updatedItem != null && CurrentQueueEntry != null)
            CurrentQueueEntry.Slot = updatedItem.Slot;

        QueueChanged?.Invoke();
    }

    public void NotifyItemCompleted(string reason)
    {
        if (AlchemyEngine == AlchemyEngine.Queue && CurrentQueueEntry != null)
        {
            CurrentQueueEntry.Status = QueueItemStatus.Completed;
            CurrentQueueEntry.StatusNote = string.Empty;
            _queueCursor = Queue.Count > 0 ? (_queueCursor + 1) % Queue.Count : 0;
            CurrentQueueEntry = null;
            QueueChanged?.Invoke();
            return;
        }

        Bootstrap.StopRunner();
    }

    public void NotifyEnhanceAttemptFailed(InventoryItem updatedItem)
    {
        if (updatedItem != null && CurrentQueueEntry != null)
            CurrentQueueEntry.Slot = updatedItem.Slot;

        if (AlchemyEngine == AlchemyEngine.Queue && CurrentQueueEntry != null && CurrentQueueEntry.SkipToNextOnFail)
        {
            var anotherPendingExists = Queue.Any(q => q != CurrentQueueEntry && q.Status == QueueItemStatus.Pending);
            if (anotherPendingExists)
            {
                CurrentQueueEntry.Status = QueueItemStatus.Pending;
                _queueCursor = (_queueCursor + 1) % Queue.Count;
                CurrentQueueEntry = null;
            }
        }

        QueueChanged?.Invoke();
    }

    public void NotifyItemDestroyed(InventoryItem oldItem)
    {
        if (AlchemyEngine == AlchemyEngine.Queue && CurrentQueueEntry != null)
        {
            CurrentQueueEntry.Status = QueueItemStatus.Failed;
            CurrentQueueEntry.StatusNote = Kernel.Language == "ar_AR" ? "اتكسر" : "Broken";
            var canSkip = CurrentQueueEntry.SkipToNextOnFail &&
                          Queue.Any(q => q != CurrentQueueEntry && q.Status == QueueItemStatus.Pending);

            _queueCursor = Queue.Count > 0 ? (_queueCursor + 1) % Queue.Count : 0;
            CurrentQueueEntry = null;
            QueueChanged?.Invoke();

            if (canSkip)
                return;
        }

        EnhanceBundleConfig = null;
        MagicBundleConfig = null;
        AttributeBundleConfig = null;
        Bootstrap.StopRunner();
    }

    public void NotifyItemFatal(string reason, bool allowSkipInQueue)
    {
        Log.Warn($"[Alchemy] {reason}");

        if (AlchemyEngine == AlchemyEngine.Queue && CurrentQueueEntry != null)
        {
            CurrentQueueEntry.Status = QueueItemStatus.Failed;
            CurrentQueueEntry.StatusNote = reason;
            var hasMore = Queue.Any(q => q != CurrentQueueEntry && q.Status == QueueItemStatus.Pending);
            _queueCursor = Queue.Count > 0 ? (_queueCursor + 1) % Queue.Count : 0;
            CurrentQueueEntry = null;
            QueueChanged?.Invoke();

            if (allowSkipInQueue && hasMore)
                return;
        }

        Bootstrap.StopRunner();
    }

    public void NotifyManufactureStateChanged()
    {
        QueueChanged?.Invoke();
    }
}
