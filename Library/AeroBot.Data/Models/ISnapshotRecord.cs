namespace AeroBot.Data.Models;

internal interface ISnapshotRecord
{
    uint RecordId { get; }
    ulong RecordHash { get; }
    int CodeOffset { get; }
    ushort CodeLength { get; }
}
