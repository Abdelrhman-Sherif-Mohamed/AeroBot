using System.Buffers;
using System.Collections.Frozen;
using System.IO.Hashing;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Text;
using AeroBot.Data.Models;

namespace AeroBot.Data;

/// <summary>
/// High-performance, memory-mapped reader for compiled <c>GameData.bin</c> snapshots.
/// Provides zero-copy <see cref="ReadOnlySpan{T}"/> access and <see cref="FrozenDictionary{TKey,TValue}"/> lookups.
/// </summary>
public sealed unsafe class GameDataReader : IDisposable
{
    private readonly MemoryMappedFile _memoryMappedFile;
    private readonly MemoryMappedViewAccessor _accessor;
    private byte* _basePtr;
    private int _disposed;

    private readonly long _fileLength;
    private readonly long _itemsOffset;
    private readonly long _skillsOffset;
    private readonly long _charactersOffset;
    private readonly long _stringArenaOffset;

    private readonly GameDataHeader _header;

    private readonly FrozenDictionary<uint, int> _itemIndexById;
    private readonly FrozenDictionary<ulong, int> _itemIndexByHash;

    private readonly FrozenDictionary<uint, int> _skillIndexById;
    private readonly FrozenDictionary<ulong, int> _skillIndexByHash;

    private readonly FrozenDictionary<uint, int> _characterIndexById;
    private readonly FrozenDictionary<ulong, int> _characterIndexByHash;

    /// <summary>
    /// Gets the validated snapshot file header.
    /// </summary>
    public GameDataHeader Header => _header;

    /// <summary>
    /// Zero-copy span of all <see cref="ItemDataRecord"/> entries backed directly by the memory-mapped file.
    /// </summary>
    public ReadOnlySpan<ItemDataRecord> Items
    {
        get
        {
            ThrowIfDisposed();
            return new ReadOnlySpan<ItemDataRecord>(_basePtr + _itemsOffset, _header.ItemCount);
        }
    }

    /// <summary>
    /// Zero-copy span of all <see cref="SkillDataRecord"/> entries backed directly by the memory-mapped file.
    /// </summary>
    public ReadOnlySpan<SkillDataRecord> Skills
    {
        get
        {
            ThrowIfDisposed();
            return new ReadOnlySpan<SkillDataRecord>(_basePtr + _skillsOffset, _header.SkillCount);
        }
    }

    /// <summary>
    /// Zero-copy span of all <see cref="CharacterDataRecord"/> entries backed directly by the memory-mapped file.
    /// </summary>
    public ReadOnlySpan<CharacterDataRecord> Characters
    {
        get
        {
            ThrowIfDisposed();
            return new ReadOnlySpan<CharacterDataRecord>(_basePtr + _charactersOffset, _header.CharacterCount);
        }
    }

    /// <summary>
    /// Zero-copy span over the entire deduplicated UTF-8 string arena.
    /// </summary>
    public ReadOnlySpan<byte> StringArena
    {
        get
        {
            ThrowIfDisposed();
            return new ReadOnlySpan<byte>(_basePtr + _stringArenaOffset, _header.StringArenaSizeBytes);
        }
    }

    /// <summary>
    /// Maps an existing <c>GameData.bin</c> snapshot for read-only access and builds O(1) <see cref="FrozenDictionary{TKey,TValue}"/> indices.
    /// </summary>
    public GameDataReader(string snapshotFilePath)
    {
        if (string.IsNullOrWhiteSpace(snapshotFilePath))
            throw new ArgumentException("Snapshot file path cannot be empty.", nameof(snapshotFilePath));

        if (!File.Exists(snapshotFilePath))
            throw new FileNotFoundException($"GameData snapshot not found: {snapshotFilePath}", snapshotFilePath);

        _fileLength = new FileInfo(snapshotFilePath).Length;
        if (_fileLength < Unsafe.SizeOf<GameDataHeader>())
            throw new InvalidDataException($"Snapshot file is too small ({_fileLength} bytes).");

        MemoryMappedFile? mmf = null;
        MemoryMappedViewAccessor? accessor = null;
        byte* ptr = null;

        try
        {
            var fs = new FileStream(snapshotFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            mmf = MemoryMappedFile.CreateFromFile(
                fs,
                mapName: null,
                capacity: 0,
                MemoryMappedFileAccess.Read,
                HandleInheritability.None,
                leaveOpen: false);

            accessor = mmf.CreateViewAccessor(0, _fileLength, MemoryMappedFileAccess.Read);
            accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref ptr);
            if (ptr == null)
                throw new InvalidOperationException("Failed to acquire memory-mapped view pointer.");

            _memoryMappedFile = mmf;
            _accessor = accessor;
            _basePtr = ptr;

            _header = Unsafe.ReadUnaligned<GameDataHeader>(ptr);
            if (!_header.IsValid)
            {
                throw new InvalidDataException(
                    $"Invalid GameData snapshot header (Magic=0x{_header.Magic:X8}, Version={_header.Version}).");
            }

            long cursor = Align8(Unsafe.SizeOf<GameDataHeader>());
            _itemsOffset = cursor;
            cursor = Align8(cursor + (long)_header.ItemCount * Unsafe.SizeOf<ItemDataRecord>());

            _skillsOffset = cursor;
            cursor = Align8(cursor + (long)_header.SkillCount * Unsafe.SizeOf<SkillDataRecord>());

            _charactersOffset = cursor;
            cursor = Align8(cursor + (long)_header.CharacterCount * Unsafe.SizeOf<CharacterDataRecord>());

            _stringArenaOffset = cursor;
            long minimumExpectedLength = _stringArenaOffset + _header.StringArenaSizeBytes;
            if (minimumExpectedLength > _fileLength)
            {
                throw new InvalidDataException(
                    $"Corrupted or truncated GameData snapshot: expected >= {minimumExpectedLength} bytes, found {_fileLength} bytes.");
            }

            (_itemIndexById, _itemIndexByHash) = BuildIndices(Items);
            (_skillIndexById, _skillIndexByHash) = BuildIndices(Skills);
            (_characterIndexById, _characterIndexByHash) = BuildIndices(Characters);
        }
        catch
        {
            if (ptr != null && accessor != null)
                accessor.SafeMemoryMappedViewHandle.ReleasePointer();

            accessor?.Dispose();
            mmf?.Dispose();
            throw;
        }
    }

    ~GameDataReader() => Dispose(disposing: false);

    public ref readonly SkillDataRecord GetSkill(uint id)
    {
        ThrowIfDisposed();
        if (!_skillIndexById.TryGetValue(id, out int index))
            throw new KeyNotFoundException($"Skill ID {id} not found in snapshot.");

        return ref GetRecordByIndex<SkillDataRecord>(_skillsOffset, index);
    }

    public bool TryGetSkillIndex(uint id, out int index)
    {
        ThrowIfDisposed();
        return _skillIndexById.TryGetValue(id, out index);
    }

    public bool TryGetSkill(uint id, out SkillDataRecord record)
    {
        ThrowIfDisposed();
        if (_skillIndexById.TryGetValue(id, out int index))
        {
            record = Skills[index];
            return true;
        }

        record = default;
        return false;
    }

    public bool TryGetSkillIndexByCodeName(ReadOnlySpan<char> codeName, out int index)
    {
        ThrowIfDisposed();
        return TryLookupIndexByCodeName(codeName, _skillIndexByHash, Skills, out index);
    }

    public ref readonly SkillDataRecord GetSkillByCodeName(ReadOnlySpan<char> codeName)
    {
        if (!TryGetSkillIndexByCodeName(codeName, out int index))
            throw new KeyNotFoundException($"Skill CodeName '{codeName.ToString()}' not found in snapshot.");

        return ref GetRecordByIndex<SkillDataRecord>(_skillsOffset, index);
    }

    public ref readonly ItemDataRecord GetItem(uint id)
    {
        ThrowIfDisposed();
        if (!_itemIndexById.TryGetValue(id, out int index))
            throw new KeyNotFoundException($"Item ID {id} not found in snapshot.");

        return ref GetRecordByIndex<ItemDataRecord>(_itemsOffset, index);
    }

    public bool TryGetItemIndex(uint id, out int index)
    {
        ThrowIfDisposed();
        return _itemIndexById.TryGetValue(id, out index);
    }

    public bool TryGetItem(uint id, out ItemDataRecord record)
    {
        ThrowIfDisposed();
        if (_itemIndexById.TryGetValue(id, out int index))
        {
            record = Items[index];
            return true;
        }

        record = default;
        return false;
    }

    public bool TryGetItemIndexByCodeName(ReadOnlySpan<char> codeName, out int index)
    {
        ThrowIfDisposed();
        return TryLookupIndexByCodeName(codeName, _itemIndexByHash, Items, out index);
    }

    public ref readonly ItemDataRecord GetItemByCodeName(ReadOnlySpan<char> codeName)
    {
        if (!TryGetItemIndexByCodeName(codeName, out int index))
            throw new KeyNotFoundException($"Item CodeName '{codeName.ToString()}' not found in snapshot.");

        return ref GetRecordByIndex<ItemDataRecord>(_itemsOffset, index);
    }

    public ref readonly CharacterDataRecord GetCharacter(uint id)
    {
        ThrowIfDisposed();
        if (!_characterIndexById.TryGetValue(id, out int index))
            throw new KeyNotFoundException($"Character ID {id} not found in snapshot.");

        return ref GetRecordByIndex<CharacterDataRecord>(_charactersOffset, index);
    }

    public bool TryGetCharacterIndex(uint id, out int index)
    {
        ThrowIfDisposed();
        return _characterIndexById.TryGetValue(id, out index);
    }

    public bool TryGetCharacter(uint id, out CharacterDataRecord record)
    {
        ThrowIfDisposed();
        if (_characterIndexById.TryGetValue(id, out int index))
        {
            record = Characters[index];
            return true;
        }

        record = default;
        return false;
    }

    public bool TryGetCharacterIndexByCodeName(ReadOnlySpan<char> codeName, out int index)
    {
        ThrowIfDisposed();
        return TryLookupIndexByCodeName(codeName, _characterIndexByHash, Characters, out index);
    }

    public ref readonly CharacterDataRecord GetCharacterByCodeName(ReadOnlySpan<char> codeName)
    {
        if (!TryGetCharacterIndexByCodeName(codeName, out int index))
            throw new KeyNotFoundException($"Character CodeName '{codeName.ToString()}' not found in snapshot.");

        return ref GetRecordByIndex<CharacterDataRecord>(_charactersOffset, index);
    }

    /// <summary>
    /// Returns a zero-copy UTF-8 slice directly from the memory-mapped string arena.
    /// </summary>
    public ReadOnlySpan<byte> GetUtf8String(int offset, ushort length)
    {
        ThrowIfDisposed();
        if (length == 0)
            return ReadOnlySpan<byte>.Empty;

        if ((uint)offset > (uint)_header.StringArenaSizeBytes ||
            (uint)(offset + length) > (uint)_header.StringArenaSizeBytes)
        {
            throw new ArgumentOutOfRangeException(
                nameof(offset),
                $"String span [{offset}..{offset + length}) exceeds arena size {_header.StringArenaSizeBytes}.");
        }

        return new ReadOnlySpan<byte>(_basePtr + _stringArenaOffset + offset, length);
    }

    /// <summary>
    /// Materializes a managed <see cref="string"/> from the memory-mapped UTF-8 string arena on demand.
    /// </summary>
    public string GetString(int offset, ushort length)
    {
        if (length == 0)
            return string.Empty;

        return Encoding.UTF8.GetString(GetUtf8String(offset, length));
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return;

        if (_basePtr != null)
        {
            _accessor.SafeMemoryMappedViewHandle.ReleasePointer();
            _basePtr = null;
        }

        if (disposing)
        {
            _accessor.Dispose();
            _memoryMappedFile.Dispose();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(Volatile.Read(ref _disposed) != 0, this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ref readonly T GetRecordByIndex<T>(long sectionOffset, int index) where T : unmanaged
    {
        return ref Unsafe.Add(ref Unsafe.AsRef<T>(_basePtr + sectionOffset), index);
    }

    private bool TryLookupIndexByCodeName<T>(
        ReadOnlySpan<char> codeName,
        FrozenDictionary<ulong, int> hashIndex,
        ReadOnlySpan<T> records,
        out int index)
        where T : unmanaged, ISnapshotRecord
    {
        if (codeName.IsEmpty)
        {
            index = -1;
            return false;
        }

        int maxBytes = Encoding.UTF8.GetMaxByteCount(codeName.Length);
        byte[]? rented = null;
        Span<byte> buffer = maxBytes <= 512
            ? stackalloc byte[512]
            : (rented = ArrayPool<byte>.Shared.Rent(maxBytes));

        try
        {
            int byteCount = Encoding.UTF8.GetBytes(codeName, buffer);
            ReadOnlySpan<byte> utf8Bytes = buffer.Slice(0, byteCount);
            ulong hash = XxHash64.HashToUInt64(utf8Bytes);

            if (hashIndex.TryGetValue(hash, out index))
            {
                ref readonly var rec = ref records[index];
                if (GetUtf8String(rec.CodeOffset, rec.CodeLength).SequenceEqual(utf8Bytes))
                    return true;
            }

            index = -1;
            return false;
        }
        finally
        {
            if (rented != null)
                ArrayPool<byte>.Shared.Return(rented);
        }
    }

    private static (FrozenDictionary<uint, int> ById, FrozenDictionary<ulong, int> ByHash) BuildIndices<T>(
        ReadOnlySpan<T> records)
        where T : unmanaged, ISnapshotRecord
    {
        var byId = new Dictionary<uint, int>(records.Length);
        var byHash = new Dictionary<ulong, int>(records.Length);

        for (int i = 0; i < records.Length; i++)
        {
            ref readonly var rec = ref records[i];
            byId[rec.RecordId] = i;
            byHash.TryAdd(rec.RecordHash, i);
        }

        return (byId.ToFrozenDictionary(), byHash.ToFrozenDictionary());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static long Align8(long offset) => (offset + 7L) & ~7L;
}
