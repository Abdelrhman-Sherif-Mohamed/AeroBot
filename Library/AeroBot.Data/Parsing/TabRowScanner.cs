using System.Globalization;

namespace AeroBot.Data.Parsing;

/// <summary>
/// Stack-only, zero-allocation tab-delimited row tokenizer over a <see cref="ReadOnlySpan{Char}"/>.
/// </summary>
internal ref struct TabRowScanner
{
    public const int MaxStackColumns = 256;

    private readonly ReadOnlySpan<char> _line;
    private readonly Span<int> _starts;
    private readonly Span<int> _lengths;

    public int ColumnCount { get; }

    public TabRowScanner(ReadOnlySpan<char> line, Span<int> startsBuffer, Span<int> lengthsBuffer)
    {
        _line = line;
        _starts = startsBuffer;
        _lengths = lengthsBuffer;

        int maxCols = Math.Min(startsBuffer.Length, lengthsBuffer.Length);
        int count = 0;
        int pos = 0;

        while (pos <= line.Length && count < maxCols)
        {
            int tabOffset = line.Slice(pos).IndexOf('\t');
            if (tabOffset < 0)
            {
                _starts[count] = pos;
                _lengths[count] = line.Length - pos;
                count++;
                break;
            }

            _starts[count] = pos;
            _lengths[count] = tabOffset;
            count++;
            pos += tabOffset + 1;
        }

        ColumnCount = count;
    }

    public ReadOnlySpan<char> GetColumn(int index)
    {
        if ((uint)index >= (uint)ColumnCount)
            return ReadOnlySpan<char>.Empty;

        return _line.Slice(_starts[index], _lengths[index]);
    }

    public bool TryParseByte(int index, out byte value)
    {
        var span = TrimSpaces(GetColumn(index));
        if (byte.TryParse(span, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            return true;

        if (TryExtractLeadingToken(span, out var prefix) &&
            byte.TryParse(prefix, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            return true;

        value = 0;
        return false;
    }

    public bool TryParseSByte(int index, out sbyte value)
    {
        var span = TrimSpaces(GetColumn(index));
        if (sbyte.TryParse(span, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            return true;

        if (TryExtractLeadingToken(span, out var prefix) &&
            sbyte.TryParse(prefix, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            return true;

        value = 0;
        return false;
    }

    public bool TryParseUShort(int index, out ushort value)
    {
        var span = TrimSpaces(GetColumn(index));
        if (ushort.TryParse(span, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            return true;

        if (TryExtractLeadingToken(span, out var prefix) &&
            ushort.TryParse(prefix, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            return true;

        value = 0;
        return false;
    }

    public bool TryParseShort(int index, out short value)
    {
        var span = TrimSpaces(GetColumn(index));
        if (short.TryParse(span, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            return true;

        if (TryExtractLeadingToken(span, out var prefix) &&
            short.TryParse(prefix, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            return true;

        value = 0;
        return false;
    }

    public bool TryParseInt(int index, out int value)
    {
        var span = TrimSpaces(GetColumn(index));
        if (int.TryParse(span, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            return true;

        if (TryExtractLeadingToken(span, out var prefix) &&
            int.TryParse(prefix, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            return true;

        value = 0;
        return false;
    }

    public bool TryParseUInt(int index, out uint value)
    {
        var span = TrimSpaces(GetColumn(index));
        if (uint.TryParse(span, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            return true;

        if (TryExtractLeadingToken(span, out var prefix) &&
            uint.TryParse(prefix, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
            return true;

        value = 0;
        return false;
    }

    public bool TryParseFloat(int index, out float value)
    {
        var span = TrimSpaces(GetColumn(index));
        if (float.TryParse(span, NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value))
            return true;

        if (TryExtractLeadingToken(span, out var prefix) &&
            float.TryParse(prefix, NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out value))
            return true;

        value = 0f;
        return false;
    }

    private static ReadOnlySpan<char> TrimSpaces(ReadOnlySpan<char> span)
    {
        int start = 0;
        while (start < span.Length && span[start] == ' ')
            start++;

        int end = span.Length - 1;
        while (end >= start && span[end] == ' ')
            end--;

        return start > end ? ReadOnlySpan<char>.Empty : span.Slice(start, end - start + 1);
    }

    private static bool TryExtractLeadingToken(ReadOnlySpan<char> span, out ReadOnlySpan<char> prefix)
    {
        int spaceIdx = span.IndexOf(' ');
        if (spaceIdx > 0)
        {
            prefix = span.Slice(0, spaceIdx);
            return true;
        }

        prefix = ReadOnlySpan<char>.Empty;
        return false;
    }
}
