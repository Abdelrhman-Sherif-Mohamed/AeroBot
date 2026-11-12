using System.Buffers;
using System.IO.Hashing;
using System.Numerics;
using System.Text;

namespace AeroBot.Data.Parsing;

/// <summary>
/// Builds a deduplicated, contiguous UTF-8 byte arena without allocating managed <see cref="string"/> instances.
/// </summary>
internal sealed class Utf8StringArenaBuilder
{
    private struct Entry
    {
        public ulong Hash;
        public int Offset;
        public ushort Length;
        public int Next;
    }

    private byte[] _arena;
    private int _arenaLength;
    private int[] _buckets;
    private Entry[] _entries;
    private int _entryCount;

    public int ArenaSizeBytes => _arenaLength;
    public ReadOnlySpan<byte> WrittenSpan => _arena.AsSpan(0, _arenaLength);

    public Utf8StringArenaBuilder(int initialArenaCapacity = 2 * 1024 * 1024, int initialEntryCapacity = 65536)
    {
        _arena = new byte[Math.Max(4096, initialArenaCapacity)];
        int bucketCount = (int)BitOperations.RoundUpToPowerOf2((uint)Math.Max(1024, initialEntryCapacity));
        _buckets = new int[bucketCount];
        Array.Fill(_buckets, -1);
        _entries = new Entry[Math.Max(1024, initialEntryCapacity)];
        _arenaLength = 0;
        _entryCount = 0;
    }

    public (int Offset, ushort Length, ulong Hash) Intern(ReadOnlySpan<char> chars)
    {
        if (chars.IsEmpty)
            return (0, 0, XxHash64.HashToUInt64(ReadOnlySpan<byte>.Empty));

        int maxBytes = Encoding.UTF8.GetMaxByteCount(chars.Length);
        byte[]? rented = null;
        Span<byte> utf8Scratch = maxBytes <= 1024
            ? stackalloc byte[1024]
            : (rented = ArrayPool<byte>.Shared.Rent(maxBytes));

        try
        {
            int utf8Len = Encoding.UTF8.GetBytes(chars, utf8Scratch);
            var utf8Slice = utf8Scratch.Slice(0, utf8Len);
            return InternUtf8(utf8Slice);
        }
        finally
        {
            if (rented != null)
                ArrayPool<byte>.Shared.Return(rented);
        }
    }

    public (int Offset, ushort Length, ulong Hash) InternUtf8(ReadOnlySpan<byte> utf8Bytes)
    {
        ulong hash = XxHash64.HashToUInt64(utf8Bytes);
        if (utf8Bytes.IsEmpty)
            return (0, 0, hash);

        if (utf8Bytes.Length > ushort.MaxValue)
            utf8Bytes = utf8Bytes.Slice(0, ushort.MaxValue);

        int bucketIndex = (int)(hash & (uint)(_buckets.Length - 1));
        for (int i = _buckets[bucketIndex]; i >= 0; i = _entries[i].Next)
        {
            ref readonly var existing = ref _entries[i];
            if (existing.Hash == hash &&
                existing.Length == utf8Bytes.Length &&
                _arena.AsSpan(existing.Offset, existing.Length).SequenceEqual(utf8Bytes))
            {
                return (existing.Offset, existing.Length, hash);
            }
        }

        int offset = _arenaLength;
        EnsureArenaCapacity(offset + utf8Bytes.Length);
        utf8Bytes.CopyTo(_arena.AsSpan(offset));
        _arenaLength = offset + utf8Bytes.Length;

        if (_entryCount == _entries.Length)
            ResizeEntries();

        if (_entryCount >= _buckets.Length * 0.75)
        {
            Rehash();
            bucketIndex = (int)(hash & (uint)(_buckets.Length - 1));
        }

        ushort length = (ushort)utf8Bytes.Length;
        int entryIndex = _entryCount++;
        _entries[entryIndex] = new Entry
        {
            Hash = hash,
            Offset = offset,
            Length = length,
            Next = _buckets[bucketIndex]
        };
        _buckets[bucketIndex] = entryIndex;

        return (offset, length, hash);
    }

    public ReadOnlySpan<byte> GetUtf8Span(int offset, ushort length)
    {
        if (length == 0)
            return ReadOnlySpan<byte>.Empty;

        return _arena.AsSpan(offset, length);
    }

    private void EnsureArenaCapacity(int required)
    {
        if (required <= _arena.Length)
            return;

        int newCap = Math.Max(_arena.Length * 2, required);
        Array.Resize(ref _arena, newCap);
    }

    private void ResizeEntries()
    {
        Array.Resize(ref _entries, _entries.Length * 2);
    }

    private void Rehash()
    {
        int newSize = _buckets.Length * 2;
        var newBuckets = new int[newSize];
        Array.Fill(newBuckets, -1);
        uint mask = (uint)(newSize - 1);

        for (int i = 0; i < _entryCount; i++)
        {
            int b = (int)(_entries[i].Hash & mask);
            _entries[i].Next = newBuckets[b];
            newBuckets[b] = i;
        }

        _buckets = newBuckets;
    }
}
