using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;

namespace RSBot.General.Components;

/// <summary>
///     A single possible interpretation of the captcha image that was received
///     in a <c>0x2322</c> packet.
/// </summary>
internal sealed class CaptchaImage
{
    public CaptchaImage(string label, Bitmap bitmap)
    {
        Label = label;
        Bitmap = bitmap;
    }

    public string Label { get; }

    public Bitmap Bitmap { get; }
}

/// <summary>
///     Decodes the zlib compressed image payload of the IBUV captcha packet.
///     The exact pixel format used by all clients is not documented, so the
///     decoder produces every plausible interpretation and lets the OCR stage
///     decide which one contains readable characters.
/// </summary>
internal static class CaptchaImageDecoder
{
    private const int MaxTrailingBytes = 64;

    private enum CompressionKind
    {
        ZLib,
        Deflate,
        GZip
    }

    /// <summary>
    ///     Inflates the compressed captcha payload. Tries zlib first (the
    ///     format used by the official client), then raw deflate and gzip.
    /// </summary>
    public static byte[] Inflate(byte[] compressed, int expectedLength)
    {
        if (compressed == null || compressed.Length < 2)
            return null;

        var candidates = new List<byte[]>(8);

        AddCandidate(candidates, TryInflate(compressed, 0, compressed.Length, CompressionKind.ZLib));
        AddCandidate(candidates, TryInflate(compressed, 0, compressed.Length, CompressionKind.Deflate));
        AddCandidate(candidates, TryInflate(compressed, 0, compressed.Length, CompressionKind.GZip));

        // Some servers prepend their own counters/checksum before the zlib stream.
        if (compressed.Length > 2)
            AddCandidate(candidates, TryInflate(compressed, 2, compressed.Length - 2, CompressionKind.Deflate));

        if (compressed.Length > 4)
            AddCandidate(candidates, TryInflate(compressed, 4, compressed.Length - 4, CompressionKind.Deflate));

        byte[] best = null;
        foreach (var candidate in candidates)
        {
            if (candidate == null || candidate.Length == 0)
                continue;

            if (expectedLength > 0 && candidate.Length == expectedLength)
                return candidate;

            if (best == null || candidate.Length > best.Length)
                best = candidate;
        }

        return best;
    }

    /// <summary>
    ///     Decodes the captcha payload into a list of candidate bitmaps.
    /// </summary>
    /// <param name="compressed">The raw compressed payload.</param>
    /// <param name="expectedLength">The uncompressed length advertised by the packet.</param>
    /// <param name="width">The image width advertised by the packet.</param>
    /// <param name="height">The image height advertised by the packet.</param>
    /// <param name="raw">The decompressed payload (for diagnostics).</param>
    public static List<CaptchaImage> Decode(byte[] compressed, int expectedLength, int width, int height,
        out byte[] raw)
    {
        raw = Inflate(compressed, expectedLength);

        var images = new List<CaptchaImage>(8);
        if (raw == null || raw.Length == 0)
            return images;

        // The payload can be a complete BMP/PNG/JPEG/DDS file.
        if (TryLoadImageFile(raw, "image-file", images))
            return images;

        if (TryLoadDds(raw, images))
            return images;

        if (width <= 0)
            width = 200;

        if (height <= 0)
            height = 64;

        var length = raw.Length;
        if (expectedLength > 0 && expectedLength <= raw.Length &&
            raw.Length - expectedLength <= MaxTrailingBytes)
            length = expectedLength;

        AddRawVariants(raw, length, width, height, images);
        return images;
    }

    private static void AddCandidate(ICollection<byte[]> candidates, byte[] value)
    {
        if (value == null || value.Length == 0)
            return;

        foreach (var candidate in candidates)
        {
            if (candidate.Length == value.Length && ReferenceEquals(candidate, value))
                return;
        }

        candidates.Add(value);
    }

    private static byte[] TryInflate(byte[] data, int offset, int count, CompressionKind kind)
    {
        try
        {
            using var input = new MemoryStream(data, offset, count, false);
            using Stream inflater = kind switch
            {
                CompressionKind.ZLib => new ZLibStream(input, CompressionMode.Decompress),
                CompressionKind.GZip => new GZipStream(input, CompressionMode.Decompress),
                _ => new DeflateStream(input, CompressionMode.Decompress)
            };

            using var output = new MemoryStream(Math.Max(1024, count * 4));
            inflater.CopyTo(output);
            return output.ToArray();
        }
        catch
        {
            return null;
        }
    }

    private static bool Matches(int actual, long expected)
    {
        return actual >= expected && actual - expected <= MaxTrailingBytes;
    }

    private static bool TryLoadImageFile(byte[] raw, string label, ICollection<CaptchaImage> images)
    {
        var known = raw.Length > 2 && raw[0] == 0x42 && raw[1] == 0x4D; // BM
        if (!known && raw.Length > 8)
            known = raw[0] == 0x89 && raw[1] == 0x50 && raw[2] == 0x4E && raw[3] == 0x47; // PNG

        if (!known && raw.Length > 2)
            known = raw[0] == 0xFF && raw[1] == 0xD8; // JPEG

        if (!known && raw.Length > 6)
            known = raw[0] == 0x47 && raw[1] == 0x49 && raw[2] == 0x46; // GIF

        if (!known)
            return false;

        try
        {
            using var stream = new MemoryStream(raw, false);
            using var bitmap = new Bitmap(stream);
            images.Add(new CaptchaImage(label, new Bitmap(bitmap)));
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryLoadDds(byte[] raw, ICollection<CaptchaImage> images)
    {
        if (raw.Length < 128 || raw[0] != (byte)'D' || raw[1] != (byte)'D' || raw[2] != (byte)'S' ||
            raw[3] != (byte)' ')
            return false;

        try
        {
            var height = BitConverter.ToInt32(raw, 12);
            var width = BitConverter.ToInt32(raw, 16);
            if (width <= 0 || height <= 0 || width > 4096 || height > 4096)
                return false;

            var fourCc = Encoding.ASCII.GetString(raw, 84, 4);
            var bitsPerPixel = BitConverter.ToInt32(raw, 88);
            var pixelData = new byte[raw.Length - 128];
            Buffer.BlockCopy(raw, 128, pixelData, 0, pixelData.Length);

            switch (fourCc)
            {
                case "DXT1":
                    images.Add(new CaptchaImage("dds-dxt1", DecodeDxt(pixelData, width, height, 1)));
                    return true;

                case "DXT3":
                    images.Add(new CaptchaImage("dds-dxt3", DecodeDxt(pixelData, width, height, 3)));
                    return true;

                case "DXT5":
                    images.Add(new CaptchaImage("dds-dxt5", DecodeDxt(pixelData, width, height, 5)));
                    return true;
            }

            if (bitsPerPixel == 32)
            {
                images.Add(new CaptchaImage("dds-32bpp-bgra", DecodeBgra(pixelData, width, height, false)));
                images.Add(new CaptchaImage("dds-32bpp-rgba", DecodeBgra(pixelData, width, height, true)));
                return true;
            }

            if (bitsPerPixel == 24)
            {
                images.Add(new CaptchaImage("dds-24bpp-bgr", DecodeBgr(pixelData, width, height, false)));
                images.Add(new CaptchaImage("dds-24bpp-rgb", DecodeBgr(pixelData, width, height, true)));
                return true;
            }
        }
        catch
        {
            // Ignore malformed DDS data and fall back to raw decoding.
        }

        return false;
    }

    private static void AddRawVariants(byte[] raw, int length, int width, int height,
        ICollection<CaptchaImage> images)
    {
        var pixels = (long)width * height;

        if (Matches(length, pixels * 4))
        {
            images.Add(new CaptchaImage("32bpp-bgra", DecodeBgra(raw, width, height, false)));
            images.Add(new CaptchaImage("32bpp-rgba", DecodeBgra(raw, width, height, true)));
        }

        if (Matches(length, pixels * 3))
        {
            images.Add(new CaptchaImage("24bpp-bgr", DecodeBgr(raw, width, height, false)));
            images.Add(new CaptchaImage("24bpp-rgb", DecodeBgr(raw, width, height, true)));
        }

        if (Matches(length, pixels))
        {
            images.Add(new CaptchaImage("8bpp-gray", DecodeGray(raw, width, height)));

            // DXT3 and DXT5 both occupy one byte per pixel.
            if (Matches(length, DxtSize(width, height, false)))
            {
                images.Add(new CaptchaImage("dxt5", DecodeDxt(raw, width, height, 5)));
                images.Add(new CaptchaImage("dxt3", DecodeDxt(raw, width, height, 3)));
            }
        }

        if (Matches(length, pixels * 2))
        {
            images.Add(new CaptchaImage("16bpp-565", DecodeRgb16(raw, width, height, false)));
            images.Add(new CaptchaImage("16bpp-555", DecodeRgb16(raw, width, height, true)));
        }

        if (Matches(length, DxtSize(width, height, true)))
            images.Add(new CaptchaImage("dxt1", DecodeDxt(raw, width, height, 1)));

        if (Matches(length, pixels + 256 * 4))
        {
            images.Add(new CaptchaImage("8bpp-palette", DecodePalette(raw, width, height, false)));
            images.Add(new CaptchaImage("8bpp-palette-last", DecodePalette(raw, width, height, true)));
        }
    }

    private static long DxtSize(int width, int height, bool dxt1)
    {
        var blocks = (long)((width + 3) / 4) * ((height + 3) / 4);
        return blocks * (dxt1 ? 8 : 16);
    }

    private static Bitmap DecodeBgra(byte[] data, int width, int height, bool swapRedBlue)
    {
        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        var bits = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly,
            PixelFormat.Format32bppArgb);
        var target = new byte[bits.Stride * height];

        for (var y = 0; y < height; y++)
        {
            var sourceRow = y * width * 4;
            var targetRow = y * bits.Stride;
            for (var x = 0; x < width; x++)
            {
                var source = sourceRow + x * 4;
                var targetIndex = targetRow + x * 4;
                var b = data[source];
                var g = data[source + 1];
                var r = data[source + 2];
                target[targetIndex] = swapRedBlue ? r : b;
                target[targetIndex + 1] = g;
                target[targetIndex + 2] = swapRedBlue ? b : r;
                target[targetIndex + 3] = 255;
            }
        }

        Marshal.Copy(target, 0, bits.Scan0, target.Length);
        bitmap.UnlockBits(bits);
        return bitmap;
    }

    private static Bitmap DecodeBgr(byte[] data, int width, int height, bool swapRedBlue)
    {
        var stride = (width * 3 + 3) & ~3;
        var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        var bits = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly,
            PixelFormat.Format24bppRgb);
        var target = new byte[bits.Stride * height];

        for (var y = 0; y < height; y++)
        {
            var sourceRow = y * stride;
            var targetRow = y * bits.Stride;
            for (var x = 0; x < width; x++)
            {
                var source = sourceRow + x * 3;
                var targetIndex = targetRow + x * 3;
                var b = data[source];
                var g = data[source + 1];
                var r = data[source + 2];
                target[targetIndex] = swapRedBlue ? r : b;
                target[targetIndex + 1] = g;
                target[targetIndex + 2] = swapRedBlue ? b : r;
            }
        }

        Marshal.Copy(target, 0, bits.Scan0, target.Length);
        bitmap.UnlockBits(bits);
        return bitmap;
    }

    private static Bitmap DecodeGray(byte[] data, int width, int height)
    {
        var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        var bits = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly,
            PixelFormat.Format24bppRgb);
        var target = new byte[bits.Stride * height];

        for (var y = 0; y < height; y++)
        {
            var sourceRow = y * width;
            var targetRow = y * bits.Stride;
            for (var x = 0; x < width; x++)
            {
                var value = data[sourceRow + x];
                var targetIndex = targetRow + x * 3;
                target[targetIndex] = value;
                target[targetIndex + 1] = value;
                target[targetIndex + 2] = value;
            }
        }

        Marshal.Copy(target, 0, bits.Scan0, target.Length);
        bitmap.UnlockBits(bits);
        return bitmap;
    }

    private static Bitmap DecodeRgb16(byte[] data, int width, int height, bool rgb555)
    {
        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        var bits = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly,
            PixelFormat.Format32bppArgb);
        var target = new byte[bits.Stride * height];

        for (var y = 0; y < height; y++)
        {
            var sourceRow = y * width * 2;
            var targetRow = y * bits.Stride;
            for (var x = 0; x < width; x++)
            {
                var value = (ushort)(data[sourceRow + x * 2] | (data[sourceRow + x * 2 + 1] << 8));
                int r, g, b;
                if (rgb555)
                {
                    r = ((value >> 10) & 0x1F) * 255 / 31;
                    g = ((value >> 5) & 0x1F) * 255 / 31;
                    b = (value & 0x1F) * 255 / 31;
                }
                else
                {
                    r = ((value >> 11) & 0x1F) * 255 / 31;
                    g = ((value >> 5) & 0x3F) * 255 / 63;
                    b = (value & 0x1F) * 255 / 31;
                }

                var targetIndex = targetRow + x * 4;
                target[targetIndex] = (byte)b;
                target[targetIndex + 1] = (byte)g;
                target[targetIndex + 2] = (byte)r;
                target[targetIndex + 3] = 255;
            }
        }

        Marshal.Copy(target, 0, bits.Scan0, target.Length);
        bitmap.UnlockBits(bits);
        return bitmap;
    }

    private static Bitmap DecodePalette(byte[] data, int width, int height, bool paletteAtEnd)
    {
        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        var bits = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly,
            PixelFormat.Format32bppArgb);
        var target = new byte[bits.Stride * height];
        var paletteOffset = paletteAtEnd ? width * height : 0;
        var pixelOffset = paletteAtEnd ? 0 : 256 * 4;

        for (var y = 0; y < height; y++)
        {
            var targetRow = y * bits.Stride;
            for (var x = 0; x < width; x++)
            {
                var index = data[pixelOffset + y * width + x];
                var paletteIndex = paletteOffset + index * 4;
                var targetIndex = targetRow + x * 4;
                target[targetIndex] = data[paletteIndex];
                target[targetIndex + 1] = data[paletteIndex + 1];
                target[targetIndex + 2] = data[paletteIndex + 2];
                target[targetIndex + 3] = 255;
            }
        }

        Marshal.Copy(target, 0, bits.Scan0, target.Length);
        bitmap.UnlockBits(bits);
        return bitmap;
    }

    private static Bitmap DecodeDxt(byte[] data, int width, int height, int type)
    {
        var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        var bits = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly,
            PixelFormat.Format32bppArgb);
        var target = new byte[bits.Stride * height];
        var blocksX = (width + 3) / 4;
        var blocksY = (height + 3) / 4;
        var blockSize = type == 1 ? 8 : 16;

        var offset = 0;
        for (var blockY = 0; blockY < blocksY; blockY++)
        {
            for (var blockX = 0; blockX < blocksX; blockX++)
            {
                if (offset + blockSize > data.Length)
                    break;

                var colorOffset = type == 1 ? offset : offset + 8;
                var color0 = ReadUInt16(data, colorOffset);
                var color1 = ReadUInt16(data, colorOffset + 2);
                var indices = ReadUInt32(data, colorOffset + 4);

                var colors = new Color[4];
                colors[0] = From565(color0);
                colors[1] = From565(color1);

                if (type != 1 || color0 > color1)
                {
                    colors[2] = Lerp(colors[0], colors[1], 2, 1, 3);
                    colors[3] = Lerp(colors[0], colors[1], 1, 2, 3);
                }
                else
                {
                    colors[2] = Lerp(colors[0], colors[1], 1, 1, 2);
                    colors[3] = Color.Transparent;
                }

                for (var pixelY = 0; pixelY < 4; pixelY++)
                {
                    for (var pixelX = 0; pixelX < 4; pixelX++)
                    {
                        var x = blockX * 4 + pixelX;
                        var y = blockY * 4 + pixelY;
                        if (x >= width || y >= height)
                            continue;

                        var index = (int)((indices >> (2 * (pixelY * 4 + pixelX))) & 0x3);
                        var color = colors[index];
                        var pixelIndex = pixelY * 4 + pixelX;
                        var alpha = 255;

                        if (type == 3)
                        {
                            var nibble = (data[offset + pixelIndex / 2] >> ((pixelIndex % 2) * 4)) & 0xF;
                            alpha = nibble * 17;
                        }
                        else if (type == 5)
                        {
                            alpha = DecodeDxt5Alpha(data, offset, pixelIndex);
                        }
                        else if (type == 1 && color0 <= color1 && index == 3)
                        {
                            alpha = 0;
                        }

                        var targetIndex = y * bits.Stride + x * 4;
                        target[targetIndex] = color.B;
                        target[targetIndex + 1] = color.G;
                        target[targetIndex + 2] = color.R;
                        target[targetIndex + 3] = (byte)alpha;
                    }
                }

                offset += blockSize;
            }
        }

        Marshal.Copy(target, 0, bits.Scan0, target.Length);
        bitmap.UnlockBits(bits);
        return bitmap;
    }

    private static int DecodeDxt5Alpha(byte[] data, int offset, int pixelIndex)
    {
        var alpha0 = data[offset];
        var alpha1 = data[offset + 1];
        var alphas = new int[8];
        alphas[0] = alpha0;
        alphas[1] = alpha1;

        if (alpha0 > alpha1)
        {
            for (var i = 2; i < 8; i++)
                alphas[i] = ((8 - i) * alpha0 + (i - 1) * alpha1) / 7;
        }
        else
        {
            for (var i = 2; i < 6; i++)
                alphas[i] = ((6 - i) * alpha0 + (i - 1) * alpha1) / 5;

            alphas[6] = 0;
            alphas[7] = 255;
        }

        ulong alphaBits = 0;
        for (var i = 0; i < 6; i++)
            alphaBits |= (ulong)data[offset + 2 + i] << (8 * i);

        var code = (int)((alphaBits >> (3 * pixelIndex)) & 0x7);
        return alphas[code];
    }

    private static ushort ReadUInt16(byte[] data, int offset)
    {
        return (ushort)(data[offset] | (data[offset + 1] << 8));
    }

    private static uint ReadUInt32(byte[] data, int offset)
    {
        return (uint)(data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) |
                      (data[offset + 3] << 24));
    }

    private static Color From565(ushort value)
    {
        var r = ((value >> 11) & 0x1F) * 255 / 31;
        var g = ((value >> 5) & 0x3F) * 255 / 63;
        var b = (value & 0x1F) * 255 / 31;
        return Color.FromArgb(r, g, b);
    }

    private static Color Lerp(Color a, Color b, int weightA, int weightB, int total)
    {
        return Color.FromArgb(
            (a.R * weightA + b.R * weightB) / total,
            (a.G * weightA + b.G * weightB) / total,
            (a.B * weightA + b.B * weightB) / total);
    }
}
