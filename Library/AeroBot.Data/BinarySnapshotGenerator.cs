using System.Buffers;
using System.IO.Hashing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using AeroBot.Data.Models;
using AeroBot.Data.Parsing;

namespace AeroBot.Data;

/// <summary>
/// Options specifying explicit text files to compile into <c>GameData.bin</c>.
/// </summary>
public sealed class SnapshotSourceOptions
{
    public IReadOnlyList<string> ItemFiles { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> SkillFiles { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> CharacterFiles { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> TextFiles { get; init; } = Array.Empty<string>();
    public int LanguageColumnIndex { get; init; } = 8;
}

/// <summary>
/// Summary metrics returned after compiling a binary snapshot.
/// </summary>
public readonly record struct SnapshotBuildResult(
    string OutputPath,
    int ItemCount,
    int SkillCount,
    int CharacterCount,
    int StringArenaSizeBytes,
    long FileSizeBytes,
    int SkippedMalformedRows);

/// <summary>
/// Compiles Silkroad Online tab-delimited text files (<c>itemdata</c>, <c>skilldata</c>,
/// <c>characterdata</c>, <c>textdata</c>) into a compact, 8-byte aligned <c>GameData.bin</c>
/// snapshot using <see cref="ArrayPool{T}"/> and <see cref="Span{T}"/> without per-row allocations.
/// </summary>
public sealed class BinarySnapshotGenerator
{
    /// <summary>
    /// Automatically discovers textdata files inside <paramref name="textDataDirectory"/>
    /// (supporting both index files such as <c>itemdata.txt</c>/<c>skilldataenc.txt</c> and direct shard files)
    /// and compiles them into <paramref name="outputFilePath"/>.
    /// </summary>
    public SnapshotBuildResult GenerateFromDirectory(
        string textDataDirectory,
        string outputFilePath,
        int languageColumnIndex = 8,
        bool preferEncryptedSkills = false)
    {
        if (string.IsNullOrWhiteSpace(textDataDirectory))
            throw new ArgumentException("Text data directory cannot be empty.", nameof(textDataDirectory));

        if (!Directory.Exists(textDataDirectory))
            throw new DirectoryNotFoundException($"Text data directory not found: {textDataDirectory}");

        var itemFiles = ResolveTableFiles(textDataDirectory, "itemdata.txt", "itemdata_*.txt");
        var characterFiles = ResolveTableFiles(textDataDirectory, "characterdata.txt", "characterdata_*.txt");

        IReadOnlyList<string> skillFiles;
        if (preferEncryptedSkills)
        {
            skillFiles = ResolveTableFiles(textDataDirectory, "skilldataenc.txt", "skilldata_*enc.txt");
            if (skillFiles.Count == 0)
                skillFiles = ResolveTableFiles(textDataDirectory, "skilldata.txt", "skilldata_*.txt", excludePattern: "enc");
        }
        else
        {
            skillFiles = ResolveTableFiles(textDataDirectory, "skilldata.txt", "skilldata_*.txt", excludePattern: "enc");
            if (skillFiles.Count == 0)
                skillFiles = ResolveTableFiles(textDataDirectory, "skilldataenc.txt", "skilldata_*enc.txt");
        }

        var textFiles = new List<string>(8);
        AddIfExists(textDataDirectory, "textdata_equip&skill.txt", textFiles);
        AddIfExists(textDataDirectory, "textdata_object.txt", textFiles);
        AddIfExists(textDataDirectory, "textdataname.txt", textFiles);
        AddIfExists(textDataDirectory, "textuisystem.txt", textFiles);

        return Generate(new SnapshotSourceOptions
        {
            ItemFiles = itemFiles,
            SkillFiles = skillFiles,
            CharacterFiles = characterFiles,
            TextFiles = textFiles,
            LanguageColumnIndex = languageColumnIndex
        }, outputFilePath);
    }

    /// <summary>
    /// Parses all configured Silkroad text files into blittable structs and writes an atomic,
    /// 8-byte aligned <c>GameData.bin</c> snapshot at <paramref name="outputFilePath"/>.
    /// </summary>
    public SnapshotBuildResult Generate(SnapshotSourceOptions options, string outputFilePath)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(outputFilePath))
            throw new ArgumentException("Output file path cannot be empty.", nameof(outputFilePath));

        var arena = new Utf8StringArenaBuilder();
        var translations = new Dictionary<ulong, (int Offset, ushort Length)>(65536);

        var items = new List<ItemDataRecord>(20000);
        var itemIdToIndex = new Dictionary<uint, int>(20000);

        var skills = new List<SkillDataRecord>(35000);
        var skillIdToIndex = new Dictionary<uint, int>(35000);

        var characters = new List<CharacterDataRecord>(20000);
        var charIdToIndex = new Dictionary<uint, int>(20000);

        int malformedRows = 0;

        foreach (var textFile in options.TextFiles)
        {
            if (File.Exists(textFile))
                malformedRows += ParseTextFile(textFile, options.LanguageColumnIndex, arena, translations);
        }

        foreach (var itemFile in options.ItemFiles)
        {
            if (File.Exists(itemFile))
                malformedRows += ParseItemFile(itemFile, arena, translations, items, itemIdToIndex);
        }

        foreach (var skillFile in options.SkillFiles)
        {
            if (File.Exists(skillFile))
                malformedRows += ParseSkillFile(skillFile, arena, translations, skills, skillIdToIndex);
        }

        foreach (var charFile in options.CharacterFiles)
        {
            if (File.Exists(charFile))
                malformedRows += ParseCharacterFile(charFile, arena, translations, characters, charIdToIndex);
        }

        long bytesWritten = WriteSnapshotAtomic(
            outputFilePath,
            items,
            skills,
            characters,
            arena);

        return new SnapshotBuildResult(
            outputFilePath,
            items.Count,
            skills.Count,
            characters.Count,
            arena.ArenaSizeBytes,
            bytesWritten,
            malformedRows);
    }

    private static int ParseTextFile(
        string filePath,
        int languageColumnIndex,
        Utf8StringArenaBuilder arena,
        Dictionary<ulong, (int Offset, ushort Length)> translations)
    {
        int malformed = 0;
        ProcessTextFileLines(filePath, isEncrypted: false, line =>
        {
            Span<int> starts = stackalloc int[TabRowScanner.MaxStackColumns];
            Span<int> lengths = stackalloc int[TabRowScanner.MaxStackColumns];
            var row = new TabRowScanner(line, starts, lengths);

            if (!row.TryParseByte(0, out byte service) || service == 0)
                return;

            int keyCol = 1;
            var initialKey = row.GetColumn(keyCol);
            if (initialKey.IsEmpty)
            {
                malformed++;
                return;
            }

            // Global clients occasionally place NameStrID in column 2 when column 1 is a numeric ID.
            if (initialKey.Length <= 4 && uint.TryParse(initialKey, out _) && !row.GetColumn(2).IsEmpty)
                keyCol = 2;

            int valueCol = -1;
            int maxCols = row.ColumnCount;
            for (int col = languageColumnIndex; col < maxCols; col++)
            {
                var candidate = row.GetColumn(col);
                if (!candidate.IsEmpty && !candidate.SequenceEqual("0"))
                {
                    valueCol = col;
                    break;
                }
            }

            if (valueCol < 0)
                return;

            ulong keyHash = HashUtf16AsUtf8(row.GetColumn(keyCol));
            var (offset, len, _) = arena.Intern(row.GetColumn(valueCol));
            translations[keyHash] = (offset, len);
        });

        return malformed;
    }

    private static int ParseItemFile(
        string filePath,
        Utf8StringArenaBuilder arena,
        Dictionary<ulong, (int Offset, ushort Length)> translations,
        List<ItemDataRecord> destination,
        Dictionary<uint, int> idToIndex)
    {
        int malformed = 0;
        ProcessTextFileLines(filePath, isEncrypted: false, line =>
        {
            Span<int> starts = stackalloc int[TabRowScanner.MaxStackColumns];
            Span<int> lengths = stackalloc int[TabRowScanner.MaxStackColumns];
            var row = new TabRowScanner(line, starts, lengths);

            if (row.ColumnCount < 62)
            {
                malformed++;
                return;
            }

            if (!row.TryParseByte(0, out byte service) || service == 0)
                return;

            if (!row.TryParseUInt(1, out uint id))
            {
                malformed++;
                return;
            }

            var codeName = row.GetColumn(2);
            if (codeName.IsEmpty)
            {
                malformed++;
                return;
            }

            row.TryParseByte(9, out byte typeId1);
            row.TryParseByte(10, out byte typeId2);
            row.TryParseByte(11, out byte typeId3);
            row.TryParseByte(12, out byte typeId4);
            row.TryParseInt(26, out int price);
            row.TryParseSByte(32, out sbyte reqLevelType1);
            row.TryParseByte(33, out byte reqLevel1);
            row.TryParseInt(57, out int maxStack);
            row.TryParseByte(61, out byte itemClass);

            byte degree = itemClass == 0 ? (byte)0 : (byte)((itemClass - 1) / 3 + 1);

            var (codeOffset, codeLength, codeHash) = arena.Intern(codeName);
            var (dispOffset, dispLength) = ResolveDisplayName(
                row.GetColumn(5),
                row.GetColumn(3),
                codeOffset,
                codeLength,
                arena,
                translations);

            var record = new ItemDataRecord(
                codeHash,
                id,
                price,
                maxStack,
                codeOffset,
                codeLength,
                dispOffset,
                dispLength,
                typeId1,
                typeId2,
                typeId3,
                typeId4,
                itemClass,
                degree,
                reqLevel1,
                reqLevelType1);

            UpsertRecord(destination, idToIndex, id, in record);
        });

        return malformed;
    }

    private static int ParseSkillFile(
        string filePath,
        Utf8StringArenaBuilder arena,
        Dictionary<ulong, (int Offset, ushort Length)> translations,
        List<SkillDataRecord> destination,
        Dictionary<uint, int> idToIndex)
    {
        int malformed = 0;
        bool isEncrypted = Path.GetFileNameWithoutExtension(filePath)
            .EndsWith("enc", StringComparison.OrdinalIgnoreCase);

        ProcessTextFileLines(filePath, isEncrypted, line =>
        {
            Span<int> starts = stackalloc int[TabRowScanner.MaxStackColumns];
            Span<int> lengths = stackalloc int[TabRowScanner.MaxStackColumns];
            var row = new TabRowScanner(line, starts, lengths);

            if (row.ColumnCount < 69)
            {
                malformed++;
                return;
            }

            if (!row.TryParseByte(0, out byte service) || service == 0)
                return;

            if (!row.TryParseUInt(1, out uint id))
            {
                malformed++;
                return;
            }

            if (!row.TryParseUInt(2, out uint groupId))
                groupId = 0;

            var codeName = row.GetColumn(3);
            if (codeName.IsEmpty)
            {
                malformed++;
                return;
            }

            row.TryParseByte(7, out byte skillLevel);
            row.TryParseByte(8, out byte basicActivity);
            row.TryParseUInt(11, out uint preparingTime);
            row.TryParseUInt(12, out uint castingTime);
            row.TryParseUInt(13, out uint actionDuration);
            row.TryParseUInt(14, out uint reuseDelay);
            row.TryParseUInt(15, out uint coolTime);
            row.TryParseFloat(16, out float flyingSpeed);
            row.TryParseByte(17, out byte interruptable);
            row.TryParseUInt(18, out uint overlapMask);
            row.TryParseShort(21, out short rawRange);
            row.TryParseByte(22, out byte targetRequired);
            row.TryParseByte(36, out byte masteryLevel1);
            row.TryParseByte(50, out byte weapon1);
            row.TryParseByte(51, out byte weapon2);
            row.TryParseUShort(53, out ushort consumeMp);
            row.TryParseInt(68, out int param0);

            byte skillType = unchecked((byte)Math.Clamp(param0, 0, byte.MaxValue));
            byte normalizedOverlap = overlapMask == 0
                ? (byte)0
                : (overlapMask <= byte.MaxValue ? (byte)overlapMask : (byte)1);
            float targetRangeMeters = rawRange > 0 ? rawRange / 10.0f : 0f;

            var (codeOffset, codeLength, codeHash) = arena.Intern(codeName);
            var (dispOffset, dispLength) = ResolveDisplayName(
                row.GetColumn(62),
                row.GetColumn(4),
                codeOffset,
                codeLength,
                arena,
                translations);

            var record = new SkillDataRecord(
                codeHash,
                id,
                groupId,
                preparingTime,
                castingTime,
                actionDuration,
                reuseDelay,
                coolTime,
                overlapMask,
                flyingSpeed,
                targetRangeMeters,
                codeOffset,
                codeLength,
                dispOffset,
                dispLength,
                consumeMp,
                skillType,
                basicActivity,
                masteryLevel1,
                skillLevel,
                interruptable,
                normalizedOverlap,
                weapon1,
                weapon2,
                targetRequired);

            UpsertRecord(destination, idToIndex, id, in record);
        });

        return malformed;
    }

    private static int ParseCharacterFile(
        string filePath,
        Utf8StringArenaBuilder arena,
        Dictionary<ulong, (int Offset, ushort Length)> translations,
        List<CharacterDataRecord> destination,
        Dictionary<uint, int> idToIndex)
    {
        int malformed = 0;
        ProcessTextFileLines(filePath, isEncrypted: false, line =>
        {
            Span<int> starts = stackalloc int[TabRowScanner.MaxStackColumns];
            Span<int> lengths = stackalloc int[TabRowScanner.MaxStackColumns];
            var row = new TabRowScanner(line, starts, lengths);

            if (row.ColumnCount < 61)
            {
                malformed++;
                return;
            }

            if (!row.TryParseByte(0, out byte service) || service == 0)
                return;

            if (!row.TryParseUInt(1, out uint id))
            {
                malformed++;
                return;
            }

            var codeName = row.GetColumn(2);
            if (codeName.IsEmpty)
            {
                malformed++;
                return;
            }

            row.TryParseByte(9, out byte typeId1);
            row.TryParseByte(10, out byte typeId2);
            row.TryParseByte(11, out byte typeId3);
            row.TryParseByte(12, out byte typeId4);
            row.TryParseByte(15, out byte rarityClassification);
            row.TryParseFloat(46, out float walkSpeed);
            row.TryParseFloat(47, out float runSpeed);
            row.TryParseUShort(48, out ushort scaleFactor);
            row.TryParseByte(57, out byte level);
            row.TryParseInt(59, out int maxHp);
            row.TryParseInt(60, out int maxMp);

            var (codeOffset, codeLength, codeHash) = arena.Intern(codeName);
            var (dispOffset, dispLength) = ResolveDisplayName(
                row.GetColumn(5),
                row.GetColumn(3),
                codeOffset,
                codeLength,
                arena,
                translations);

            var record = new CharacterDataRecord(
                codeHash,
                id,
                maxHp,
                maxMp,
                runSpeed,
                walkSpeed,
                codeOffset,
                codeLength,
                dispOffset,
                dispLength,
                scaleFactor,
                level,
                rarityClassification,
                typeId1,
                typeId2,
                typeId3,
                typeId4);

            UpsertRecord(destination, idToIndex, id, in record);
        });

        return malformed;
    }

    private static void UpsertRecord<T>(List<T> destination, Dictionary<uint, int> idToIndex, uint id, in T record)
        where T : unmanaged
    {
        if (idToIndex.TryGetValue(id, out int existingIdx))
        {
            destination[existingIdx] = record;
        }
        else
        {
            idToIndex[id] = destination.Count;
            destination.Add(record);
        }
    }

    private static (int Offset, ushort Length) ResolveDisplayName(
        ReadOnlySpan<char> nameStrId,
        ReadOnlySpan<char> fallbackObjName,
        int codeOffset,
        ushort codeLength,
        Utf8StringArenaBuilder arena,
        Dictionary<ulong, (int Offset, ushort Length)> translations)
    {
        if (!nameStrId.IsEmpty && !nameStrId.SequenceEqual("xxx"))
        {
            ulong keyHash = HashUtf16AsUtf8(nameStrId);
            if (translations.TryGetValue(keyHash, out var resolved))
                return resolved;
        }

        if (!fallbackObjName.IsEmpty && !fallbackObjName.SequenceEqual("xxx") && !fallbackObjName.StartsWith("?"))
        {
            var (offset, length, _) = arena.Intern(fallbackObjName);
            return (offset, length);
        }

        return (codeOffset, codeLength);
    }

    private delegate void LineSpanCallback(ReadOnlySpan<char> line);

    private static void ProcessTextFileLines(string filePath, bool isEncrypted, LineSpanCallback onLine)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 65536, FileOptions.SequentialScan);
        if (fs.Length == 0)
            return;

        if (fs.Length > int.MaxValue)
            throw new InvalidDataException($"File exceeds maximum single-buffer size: {filePath}");

        int fileLength = (int)fs.Length;
        byte[] rawBuffer = ArrayPool<byte>.Shared.Rent(fileLength);
        char[]? charBuffer = null;

        try
        {
            int totalRead = 0;
            while (totalRead < fileLength)
            {
                int bytesRead = fs.Read(rawBuffer, totalRead, fileLength - totalRead);
                if (bytesRead == 0)
                    break;
                totalRead += bytesRead;
            }

            Span<byte> payload = rawBuffer.AsSpan(0, totalRead);
            if (isEncrypted)
                SkillDecryption.DecryptInPlace(payload, SkillDecryption.DefaultBaseKey);

            Encoding encoding = DetectEncodingAndAdvanceBom(ref payload);
            int maxChars = encoding.GetMaxCharCount(payload.Length);
            charBuffer = ArrayPool<char>.Shared.Rent(maxChars);

            int charCount = encoding.GetChars(payload, charBuffer);
            ReadOnlySpan<char> remaining = charBuffer.AsSpan(0, charCount);

            while (!remaining.IsEmpty)
            {
                int newlineIdx = remaining.IndexOf('\n');
                ReadOnlySpan<char> line;
                if (newlineIdx < 0)
                {
                    line = remaining;
                    remaining = ReadOnlySpan<char>.Empty;
                }
                else
                {
                    line = remaining.Slice(0, newlineIdx);
                    remaining = remaining.Slice(newlineIdx + 1);
                }

                if (!line.IsEmpty && line[^1] == '\r')
                    line = line.Slice(0, line.Length - 1);

                var trimmedStart = TrimLeadingWhitespace(line);
                if (trimmedStart.IsEmpty || trimmedStart.StartsWith("//"))
                    continue;

                onLine(line);
            }
        }
        finally
        {
            if (charBuffer != null)
                ArrayPool<char>.Shared.Return(charBuffer);

            ArrayPool<byte>.Shared.Return(rawBuffer);
        }
    }

    private static Encoding DetectEncodingAndAdvanceBom(ref Span<byte> payload)
    {
        if (payload.Length >= 2)
        {
            if (payload[0] == 0xFF && payload[1] == 0xFE)
            {
                payload = payload.Slice(2);
                return Encoding.Unicode;
            }

            if (payload[0] == 0xFE && payload[1] == 0xFF)
            {
                payload = payload.Slice(2);
                return Encoding.BigEndianUnicode;
            }
        }

        if (payload.Length >= 3 && payload[0] == 0xEF && payload[1] == 0xBB && payload[2] == 0xBF)
        {
            payload = payload.Slice(3);
            return Encoding.UTF8;
        }

        return Encoding.UTF8;
    }

    private static ReadOnlySpan<char> TrimLeadingWhitespace(ReadOnlySpan<char> span)
    {
        int i = 0;
        while (i < span.Length && (span[i] == ' ' || span[i] == '\t' || span[i] == '\uFEFF'))
            i++;

        return span.Slice(i);
    }

    private static ulong HashUtf16AsUtf8(ReadOnlySpan<char> chars)
    {
        if (chars.IsEmpty)
            return XxHash64.HashToUInt64(ReadOnlySpan<byte>.Empty);

        int maxBytes = Encoding.UTF8.GetMaxByteCount(chars.Length);
        byte[]? rented = null;
        Span<byte> buffer = maxBytes <= 512
            ? stackalloc byte[512]
            : (rented = ArrayPool<byte>.Shared.Rent(maxBytes));

        try
        {
            int len = Encoding.UTF8.GetBytes(chars, buffer);
            return XxHash64.HashToUInt64(buffer.Slice(0, len));
        }
        finally
        {
            if (rented != null)
                ArrayPool<byte>.Shared.Return(rented);
        }
    }

    private static long WriteSnapshotAtomic(
        string outputFilePath,
        List<ItemDataRecord> items,
        List<SkillDataRecord> skills,
        List<CharacterDataRecord> characters,
        Utf8StringArenaBuilder arena)
    {
        string fullOutputPath = Path.GetFullPath(outputFilePath);
        string? directory = Path.GetDirectoryName(fullOutputPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        string tempPath = fullOutputPath + ".tmp";

        var header = new GameDataHeader(
            GameDataHeader.ExpectedMagic,
            GameDataHeader.CurrentVersion,
            items.Count,
            skills.Count,
            characters.Count,
            arena.ArenaSizeBytes);

        long totalBytes;
        using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 65536))
        {
            ReadOnlySpan<byte> headerBytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref header, 1));
            fs.Write(headerBytes);
            WritePaddingToAlign8(fs);

            WriteBlock(fs, CollectionsMarshal.AsSpan(items));
            WritePaddingToAlign8(fs);

            WriteBlock(fs, CollectionsMarshal.AsSpan(skills));
            WritePaddingToAlign8(fs);

            WriteBlock(fs, CollectionsMarshal.AsSpan(characters));
            WritePaddingToAlign8(fs);

            fs.Write(arena.WrittenSpan);
            fs.Flush(flushToDisk: true);
            totalBytes = fs.Length;
        }

        File.Move(tempPath, fullOutputPath, overwrite: true);
        return totalBytes;
    }

    private static void WriteBlock<T>(FileStream fs, Span<T> span) where T : unmanaged
    {
        if (span.IsEmpty)
            return;

        fs.Write(MemoryMarshal.AsBytes(span));
    }

    private static void WritePaddingToAlign8(FileStream fs)
    {
        int remainder = (int)(fs.Position & 7L);
        if (remainder == 0)
            return;

        int padBytes = 8 - remainder;
        Span<byte> zero = stackalloc byte[8];
        zero.Clear();
        fs.Write(zero.Slice(0, padBytes));
    }

    private static IReadOnlyList<string> ResolveTableFiles(
        string directory,
        string indexFileName,
        string globPattern,
        string? excludePattern = null)
    {
        string indexPath = FindFileCaseInsensitive(directory, indexFileName);
        var resolved = new List<string>(16);

        if (!string.IsNullOrEmpty(indexPath) && File.Exists(indexPath))
        {
            ProcessTextFileLines(indexPath, isEncrypted: false, line =>
            {
                string entry = line.Trim().ToString();
                if (entry.Length == 0)
                    return;

                string candidate = FindFileCaseInsensitive(directory, Path.GetFileName(entry));
                if (!string.IsNullOrEmpty(candidate) && File.Exists(candidate))
                    resolved.Add(candidate);
            });

            if (resolved.Count > 0)
                return resolved;
        }

        foreach (var path in Directory.EnumerateFiles(directory, globPattern, SearchOption.TopDirectoryOnly))
        {
            string name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(excludePattern) &&
                name.Contains(excludePattern, StringComparison.OrdinalIgnoreCase))
                continue;

            if (name.Contains("virtual", StringComparison.OrdinalIgnoreCase))
                continue;

            resolved.Add(path);
        }

        resolved.Sort(StringComparer.OrdinalIgnoreCase);
        return resolved;
    }

    private static void AddIfExists(string directory, string fileName, List<string> target)
    {
        string found = FindFileCaseInsensitive(directory, fileName);
        if (!string.IsNullOrEmpty(found))
            target.Add(found);
    }

    private static string FindFileCaseInsensitive(string directory, string targetFileName)
    {
        string direct = Path.Combine(directory, targetFileName);
        if (File.Exists(direct))
            return direct;

        foreach (var path in Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly))
        {
            if (string.Equals(Path.GetFileName(path), targetFileName, StringComparison.OrdinalIgnoreCase))
                return path;
        }

        return string.Empty;
    }
}
