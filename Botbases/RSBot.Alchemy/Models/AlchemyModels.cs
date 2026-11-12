using System;
using System.Collections.Generic;
using System.Linq;
using RSBot.Alchemy.Bundle.Manufacture;
using RSBot.Core;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Extensions;
using RSBot.Core.Objects;

namespace RSBot.Alchemy.Models;

internal enum AlchemyTargetMode
{
    Plus,
    Blues,
    Stats
}

internal enum QueueItemStatus
{
    Pending,
    Running,
    Completed,
    Failed
}

internal sealed class BlueTargetConfig
{
    public string Group { get; set; }
    public string DisplayName { get; set; }
    public uint TargetValue { get; set; }
    public bool Enabled { get; set; }
}

internal sealed class StatTargetConfig
{
    public ItemAttributeGroup Group { get; set; }
    public string DisplayName { get; set; }
    public int TargetPercent { get; set; } = 22;
    public bool Enabled { get; set; }
}

internal sealed class AlchemyQueueEntry
{
    public Guid Id { get; } = Guid.NewGuid();
    public byte Slot { get; set; }
    public uint ItemId { get; set; }
    public string ItemName { get; set; }
    public byte Degree { get; set; }
    public AlchemyTargetMode Mode { get; set; } = AlchemyTargetMode.Plus;

    // Plus mode settings
    public byte TargetPlus { get; set; }
    public uint? PreferredElixirItemId { get; set; }
    public bool UseLuckyPowder { get; set; } = true;
    public byte LuckyPowderFromPlus { get; set; }
    public bool StopIfLuckyPowderEmpty { get; set; } = true;
    public bool UseLuckyStone { get; set; }
    public byte LuckyStoneFromPlus { get; set; } = 5;
    public bool UseImmortalStone { get; set; }
    public byte ImmortalStoneFromPlus { get; set; } = 5;
    public bool UseAstralStone { get; set; }
    public byte AstralStoneFromPlus { get; set; } = 5;
    public bool UseSteadyStone { get; set; }
    public byte SteadyStoneFromPlus { get; set; } = 5;
    public bool SkipToNextOnFail { get; set; }

    // Blues & Stats target settings
    public List<BlueTargetConfig> Blues { get; set; } = new();
    public List<StatTargetConfig> Stats { get; set; } = new();

    public QueueItemStatus Status { get; set; } = QueueItemStatus.Pending;
    public string StatusNote { get; set; } = string.Empty;

    public InventoryItem ResolveLiveItem()
    {
        var inv = Game.Player?.Inventory;
        if (inv == null)
            return null;

        var atSlot = inv.GetItemAt(Slot);
        if (atSlot != null && atSlot.ItemId == ItemId && atSlot.Amount > 0)
            return atSlot;

        return null;
    }

    public string GetModeLabel() => Mode switch
    {
        AlchemyTargetMode.Plus => "Plus",
        AlchemyTargetMode.Blues => "Blues",
        AlchemyTargetMode.Stats => "Stats",
        _ => "Plus"
    };

    public string GetCurrentSummary()
    {
        var live = ResolveLiveItem();
        if (live == null)
            return Status == QueueItemStatus.Failed ? "Destroyed" : "--";

        return Mode switch
        {
            AlchemyTargetMode.Plus => $"+{live.OptLevel}",
            AlchemyTargetMode.Blues => GetBluesCurrentSummary(live),
            AlchemyTargetMode.Stats => GetStatsCurrentSummary(live),
            _ => $"+{live.OptLevel}"
        };
    }

    public string GetTargetSummary()
    {
        return Mode switch
        {
            AlchemyTargetMode.Plus => $"+{TargetPlus}",
            AlchemyTargetMode.Blues => $"{Blues.Count(b => b.Enabled)} Blues",
            AlchemyTargetMode.Stats => GetStatsTargetSummary(),
            _ => $"+{TargetPlus}"
        };
    }

    public string GetStatusLabel(bool arabic)
    {
        if (!string.IsNullOrWhiteSpace(StatusNote))
            return StatusNote;

        return Status switch
        {
            QueueItemStatus.Pending => arabic ? "في الانتظار" : "Pending",
            QueueItemStatus.Running => arabic ? "شغال" : "Running",
            QueueItemStatus.Completed => arabic ? "تم" : "Done",
            QueueItemStatus.Failed => arabic ? "فشل" : "Failed",
            _ => string.Empty
        };
    }

    private string GetBluesCurrentSummary(InventoryItem live)
    {
        var enabled = Blues.Where(b => b.Enabled).ToList();
        if (enabled.Count == 0)
            return $"{live.MagicOptions?.Count ?? 0} Blues";

        var satisfied = 0;
        foreach (var target in enabled)
        {
            var cur = live.MagicOptions?.FirstOrDefault(m => m.Record?.Group == target.Group);
            if (cur != null && cur.Value >= target.TargetValue && target.TargetValue > 0)
                satisfied++;
        }

        return $"{satisfied}/{enabled.Count}";
    }

    private string GetStatsCurrentSummary(InventoryItem live)
    {
        var enabled = Stats.Where(s => s.Enabled).ToList();
        if (enabled.Count == 0)
            return "--";

        var sum = 0;
        var count = 0;
        foreach (var target in enabled)
        {
            var groups = ItemAttributesInfo.GetAvailableAttributeGroupsForItem(live.Record);
            if (groups == null || !groups.Contains(target.Group))
                continue;

            var slot = ItemAttributesInfo.GetAttributeSlotForItem(target.Group, live.Record);
            sum += live.Attributes.GetPercentage(slot);
            count++;
        }

        return count == 0 ? "--" : $"{sum / count}%";
    }

    private string GetStatsTargetSummary()
    {
        var enabled = Stats.Where(s => s.Enabled).ToList();
        if (enabled.Count == 0)
            return "0 Stats";

        var maxTarget = enabled.Max(s => s.TargetPercent);
        return $"{enabled.Count} ({maxTarget}%)";
    }
}

internal sealed class ManufactureMaterialRequirement
{
    public uint ItemId { get; set; }
    public string CodeName { get; set; }
    public string MaterialName { get; set; }
    public int RequiredPerCraft { get; set; } = 1;
}

internal sealed class ManufactureQueueEntry
{
    public Guid Id { get; } = Guid.NewGuid();
    public string ItemName { get; set; }
    public ManufactureMode Mode { get; set; } = ManufactureMode.Resolve;
    public int TargetCount { get; set; } = 1;
    public int CompletedCount { get; set; }
    public int FailedCount { get; set; }
    public QueueItemStatus Status { get; set; } = QueueItemStatus.Pending;
    public string StatusNote { get; set; } = string.Empty;
    public List<ManufactureMaterialRequirement> Requirements { get; set; } = new();

    public string GetStatusLabel(bool arabic)
    {
        if (!string.IsNullOrWhiteSpace(StatusNote))
            return StatusNote;

        return Status switch
        {
            QueueItemStatus.Pending => arabic ? "في الانتظار" : "Pending",
            QueueItemStatus.Running => arabic ? "شغال" : "Running",
            QueueItemStatus.Completed => arabic ? "تم" : "Done",
            QueueItemStatus.Failed => arabic ? "فشل" : "Failed",
            _ => string.Empty
        };
    }
}

internal sealed class DismantleFilterRule
{
    public Guid Id { get; } = Guid.NewGuid();
    public uint? SpecificItemId { get; set; }
    public byte? SpecificSlot { get; set; }
    public byte? TypeId3 { get; set; }
    public byte[] TypeId3Set { get; set; }
    public byte? Degree { get; set; }
    public string ItemName { get; set; }
    public int Priority { get; set; } = 1;

    public bool Matches(InventoryItem item)
    {
        if (item?.Record == null || item.Amount == 0)
            return false;

        if (SpecificItemId.HasValue)
        {
            if (SpecificSlot.HasValue)
                return item.ItemId == SpecificItemId.Value && item.Slot == SpecificSlot.Value;

            return item.ItemId == SpecificItemId.Value;
        }

        if (TypeId3Set != null && TypeId3Set.Length > 0)
        {
            if (Array.IndexOf(TypeId3Set, item.Record.TypeID3) < 0)
                return false;
        }
        else if (TypeId3.HasValue && item.Record.TypeID3 != TypeId3.Value)
        {
            return false;
        }

        if (Degree.HasValue && item.Record.Degree != Degree.Value)
            return false;

        return true;
    }

    public int GetMatchingInventoryCount()
    {
        if (Game.Player?.Inventory == null)
            return 0;

        return Game.Player.Inventory
            .Where(i => i != null && i.Slot > 12 && i.Record != null && i.Record.IsEquip && !i.Record.IsAvatar && !i.Record.IsJobOutfit)
            .Count(Matches);
    }
}

internal sealed class DismantleOutputEntry
{
    public uint ItemId { get; set; }
    public string MaterialName { get; set; }
    public long Quantity { get; set; }
    public long UnitPrice { get; set; }
    public long TotalValue => Quantity * UnitPrice;
}
