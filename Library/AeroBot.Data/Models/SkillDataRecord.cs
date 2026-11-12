using System.Runtime.InteropServices;

namespace AeroBot.Data.Models;

/// <summary>
/// Blittable binary record representing a Silkroad <c>SkillData</c> row with full action timing metadata.
/// Strings are stored externally in the contiguous UTF-8 string arena.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly record struct SkillDataRecord : ISnapshotRecord
{
    public readonly ulong CodeNameHash;
    public readonly uint Id;
    public readonly uint GroupId;
    public readonly uint Action_PreparingTime;
    public readonly uint Action_CastingTime;
    public readonly uint Action_ActionDuration;
    public readonly uint Action_ReuseDelay;
    public readonly uint Action_CoolTime;
    public readonly uint Action_OverlapMask;
    public readonly float Action_FlyingSpeed;
    /// <summary>
    /// Skill effective target range in game world meters (<c>Action_Range / 10.0f</c>).
    /// </summary>
    public readonly float TargetRange;
    public readonly int CodeNameOffset;
    public readonly int DisplayNameOffset;
    public readonly ushort CodeNameLength;
    public readonly ushort DisplayNameLength;
    public readonly ushort RequiredMP;
    /// <summary>
    /// Primary skill category from <c>Params[0]</c> (0=Melee, 1=Ranged, 2=Reserved, 3=Buff, 4=Passive).
    /// </summary>
    public readonly byte SkillType;
    public readonly byte BasicActivity;
    /// <summary>
    /// Mastery level requirement (<c>ReqCommon_MasteryLevel1</c>).
    /// </summary>
    public readonly byte RequiredLevel;
    public readonly byte SkillLevel;
    public readonly byte Action_Interruptable;
    /// <summary>
    /// Normalized overlap group byte; full 32-bit Silkroad overlap bitmask is stored in <see cref="Action_OverlapMask"/>.
    /// </summary>
    public readonly byte Action_Overlap;
    public readonly byte RequiredWeapon1;
    public readonly byte RequiredWeapon2;
    public readonly byte TargetRequired;
    private readonly byte _reserved;

    public SkillDataRecord(
        ulong codeNameHash,
        uint id,
        uint groupId,
        uint actionPreparingTime,
        uint actionCastingTime,
        uint actionActionDuration,
        uint actionReuseDelay,
        uint actionCoolTime,
        uint actionOverlapMask,
        float actionFlyingSpeed,
        float targetRange,
        int codeNameOffset,
        ushort codeNameLength,
        int displayNameOffset,
        ushort displayNameLength,
        ushort requiredMP,
        byte skillType,
        byte basicActivity,
        byte requiredLevel,
        byte skillLevel,
        byte actionInterruptable,
        byte actionOverlap,
        byte requiredWeapon1,
        byte requiredWeapon2,
        byte targetRequired)
    {
        CodeNameHash = codeNameHash;
        Id = id;
        GroupId = groupId;
        Action_PreparingTime = actionPreparingTime;
        Action_CastingTime = actionCastingTime;
        Action_ActionDuration = actionActionDuration;
        Action_ReuseDelay = actionReuseDelay;
        Action_CoolTime = actionCoolTime;
        Action_OverlapMask = actionOverlapMask;
        Action_FlyingSpeed = actionFlyingSpeed;
        TargetRange = targetRange;
        CodeNameOffset = codeNameOffset;
        CodeNameLength = codeNameLength;
        DisplayNameOffset = displayNameOffset;
        DisplayNameLength = displayNameLength;
        RequiredMP = requiredMP;
        SkillType = skillType;
        BasicActivity = basicActivity;
        RequiredLevel = requiredLevel;
        SkillLevel = skillLevel;
        Action_Interruptable = actionInterruptable;
        Action_Overlap = actionOverlap;
        RequiredWeapon1 = requiredWeapon1;
        RequiredWeapon2 = requiredWeapon2;
        TargetRequired = targetRequired;
        _reserved = 0;
    }

    /// <summary>
    /// Total animation lock window in milliseconds (<c>Preparing + Casting + ActionDuration</c>).
    /// </summary>
    public uint TotalAnimationTimeMs => Action_PreparingTime + Action_CastingTime + Action_ActionDuration;

    uint ISnapshotRecord.RecordId => Id;
    ulong ISnapshotRecord.RecordHash => CodeNameHash;
    int ISnapshotRecord.CodeOffset => CodeNameOffset;
    ushort ISnapshotRecord.CodeLength => CodeNameLength;
}
