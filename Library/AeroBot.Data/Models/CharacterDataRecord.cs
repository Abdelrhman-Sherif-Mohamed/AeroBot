using System.Runtime.InteropServices;

namespace AeroBot.Data.Models;

/// <summary>
/// Blittable binary record representing a Silkroad <c>CharacterData</c> row (NPCs, Monsters, Players, COS).
/// Strings are stored externally in the contiguous UTF-8 string arena.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly record struct CharacterDataRecord : ISnapshotRecord
{
    public readonly ulong CodeNameHash;
    public readonly uint Id;
    public readonly int MaxHP;
    public readonly int MaxMP;
    public readonly float SpeedRun;
    public readonly float SpeedWalk;
    public readonly int CodeNameOffset;
    public readonly int DisplayNameOffset;
    public readonly ushort CodeNameLength;
    public readonly ushort DisplayNameLength;
    /// <summary>
    /// Visual model scale percentage (column 48, typically 100 for standard units).
    /// </summary>
    public readonly ushort ScaleFactor;
    public readonly byte Level;
    /// <summary>
    /// Monster classification / rarity from column 15 (0=General, 1=Champion, 3=Unique, 4=Giant, 5=Titan, 6=Elite, etc.).
    /// </summary>
    public readonly byte Scale;
    public readonly byte TypeId1;
    public readonly byte TypeId2;
    public readonly byte TypeId3;
    public readonly byte TypeId4;

    public CharacterDataRecord(
        ulong codeNameHash,
        uint id,
        int maxHP,
        int maxMP,
        float speedRun,
        float speedWalk,
        int codeNameOffset,
        ushort codeNameLength,
        int displayNameOffset,
        ushort displayNameLength,
        ushort scaleFactor,
        byte level,
        byte scale,
        byte typeId1,
        byte typeId2,
        byte typeId3,
        byte typeId4)
    {
        CodeNameHash = codeNameHash;
        Id = id;
        MaxHP = maxHP;
        MaxMP = maxMP;
        SpeedRun = speedRun;
        SpeedWalk = speedWalk;
        CodeNameOffset = codeNameOffset;
        CodeNameLength = codeNameLength;
        DisplayNameOffset = displayNameOffset;
        DisplayNameLength = displayNameLength;
        ScaleFactor = scaleFactor;
        Level = level;
        Scale = scale;
        TypeId1 = typeId1;
        TypeId2 = typeId2;
        TypeId3 = typeId3;
        TypeId4 = typeId4;
    }

    public bool IsMonster => TypeId2 == 2 && TypeId3 == 1 && TypeId4 == 1;
    public bool IsNpc => TypeId2 == 2 && TypeId3 == 2;
    public bool IsVehicleOrPet => TypeId2 == 2 && TypeId3 == 3;

    uint ISnapshotRecord.RecordId => Id;
    ulong ISnapshotRecord.RecordHash => CodeNameHash;
    int ISnapshotRecord.CodeOffset => CodeNameOffset;
    ushort ISnapshotRecord.CodeLength => CodeNameLength;
}
