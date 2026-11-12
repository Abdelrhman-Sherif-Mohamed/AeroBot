using System;
using System.Collections.Generic;
using RSBot.Core.Objects.Item;

namespace RSBot.Core.Objects;

/// <summary>
///     Indicates whether a recorded combat event represents an enemy player kill or a player death.
/// </summary>
public enum KillDeathType
{
    Kill = 0,
    Death = 1
}

/// <summary>
///     Summarizes a single equipped item or avatar piece captured from a spawned player.
/// </summary>
public sealed class EquippedGearSummary
{
    public string SlotCategory { get; init; } = string.Empty;
    public string ItemName { get; init; } = string.Empty;
    public string CodeName { get; init; } = string.Empty;
    public int Degree { get; init; }
    public byte TypeID4 { get; init; }
    public byte OptLevel { get; init; }

    /// <summary>
    ///     Formats the item with optional degree prefix and enhancement level.
    /// </summary>
    public string FormatDisplay()
    {
        var degPrefix = Degree > 0 ? $"[D{Degree}] " : string.Empty;
        return $"{degPrefix}{ItemName} +{OptLevel}";
    }

    public override string ToString() => FormatDisplay();
}

/// <summary>
///     Captures a structured PvP/Job kill or death entry for logs and notification panels.
/// </summary>
public sealed class KillDeathRecord
{
    public long Id { get; init; }
    public DateTime Timestamp { get; init; }
    public KillDeathType EventType { get; init; }
    public uint EntityUniqueId { get; init; }
    public bool IsPlayer { get; init; }

    public string DisplayName { get; init; } = string.Empty;
    public string RealName { get; init; } = string.Empty;
    public bool IsRealNameHidden { get; init; }

    public JobType Job { get; init; }
    public bool IsThief { get; init; }
    public bool WearsJobSuite { get; init; }
    public byte JobLevel { get; init; }

    public string GuildName { get; init; } = string.Empty;
    public string GuildNickname { get; init; } = string.Empty;

    public PvpState PvpState { get; init; }
    public PvpCapeType PvpCape { get; init; }

    public string SkillName { get; init; } = string.Empty;
    public int Damage { get; init; }

    public string WeaponSummary { get; init; } = string.Empty;
    public string ShieldSummary { get; init; } = string.Empty;
    public string ArmorSummary { get; init; } = string.Empty;
    public int HighestWeaponPlus { get; init; } = -1;
    public int HighestArmorPlus { get; init; } = -1;

    public List<EquippedGearSummary> Equipment { get; init; } = new();
    public List<EquippedGearSummary> Avatars { get; init; } = new();
}
