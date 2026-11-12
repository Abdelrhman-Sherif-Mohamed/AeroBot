using System.Runtime.InteropServices;

namespace AeroBot.Data.Models;

/// <summary>
/// Blittable binary record representing a Silkroad <c>ItemData</c> row.
/// Strings are stored externally in the contiguous UTF-8 string arena.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly record struct ItemDataRecord : ISnapshotRecord
{
    public readonly ulong CodeNameHash;
    public readonly uint Id;
    public readonly int Price;
    public readonly int MaxStack;
    public readonly int CodeNameOffset;
    public readonly int DisplayNameOffset;
    public readonly ushort CodeNameLength;
    public readonly ushort DisplayNameLength;
    public readonly byte TypeId1;
    public readonly byte TypeId2;
    public readonly byte TypeId3;
    public readonly byte TypeId4;
    public readonly byte ItemClass;
    public readonly byte Degree;
    public readonly byte ReqLevel1;
    public readonly sbyte ReqLevelType1;

    public ItemDataRecord(
        ulong codeNameHash,
        uint id,
        int price,
        int maxStack,
        int codeNameOffset,
        ushort codeNameLength,
        int displayNameOffset,
        ushort displayNameLength,
        byte typeId1,
        byte typeId2,
        byte typeId3,
        byte typeId4,
        byte itemClass,
        byte degree,
        byte reqLevel1,
        sbyte reqLevelType1)
    {
        CodeNameHash = codeNameHash;
        Id = id;
        Price = price;
        MaxStack = maxStack;
        CodeNameOffset = codeNameOffset;
        CodeNameLength = codeNameLength;
        DisplayNameOffset = displayNameOffset;
        DisplayNameLength = displayNameLength;
        TypeId1 = typeId1;
        TypeId2 = typeId2;
        TypeId3 = typeId3;
        TypeId4 = typeId4;
        ItemClass = itemClass;
        Degree = degree;
        ReqLevel1 = reqLevel1;
        ReqLevelType1 = reqLevelType1;
    }

    public bool IsEquip => TypeId2 == 1;
    public bool IsPet => TypeId2 == 2 && TypeId3 == 1;
    public bool IsStackable => TypeId2 == 3;
    public bool IsGold => IsStackable && TypeId3 == 5 && TypeId4 == 0;
    public bool IsPotion => IsStackable && TypeId3 == 1;
    public bool IsWeapon => IsEquip && TypeId3 == 6;
    public bool IsShield => IsEquip && TypeId3 == 4;

    uint ISnapshotRecord.RecordId => Id;
    ulong ISnapshotRecord.RecordHash => CodeNameHash;
    int ISnapshotRecord.CodeOffset => CodeNameOffset;
    ushort ISnapshotRecord.CodeLength => CodeNameLength;
}
