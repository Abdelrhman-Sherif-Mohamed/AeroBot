using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;
using RSBot.Core;

namespace RSBot.General.Components;

/// <summary>
///     Reads the characters from the decoded captcha images using the OCR
///     engine that ships with Windows 10/11 (<c>Windows.Media.Ocr</c>).
/// </summary>
internal static class CaptchaOcr
{
    private static readonly Lazy<OcrEngine> Engine =
        new(CreateEngine, isThreadSafe: true);

    private static readonly Dictionary<char, char> Replacements = new()
    {
        { 'O', '0' }, { 'Q', '0' }, { 'D', '0' }, { 'U', '0' },
        { 'I', '1' }, { 'L', '1' }, { 'T', '7' }, { 'J', '1' },
        { 'Z', '2' }, { 'S', '5' }, { 'B', '8' }, { 'G', '6' },
        { 'A', '4' }, { 'Y', '4' }, { 'R', '2' }
    };

    private sealed class Result
    {
        public string Code;
        public int Score;
        public string Label;
    }

    /// <summary>
    ///     Runs OCR over every candidate image and returns the most likely
    ///     captcha codes, best candidate first.
    /// </summary>
    public static async Task<List<string>> RecognizeAsync(IReadOnlyList<CaptchaImage> images)
    {
        if (images == null || images.Count == 0)
            return new List<string>();

        var engine = Engine.Value;
        if (engine == null)
        {
            Log.Warn("[AutoCaptcha] Windows OCR is not available on this system.");
            return new List<string>();
        }

        var results = new List<Result>();

        foreach (var image in images.Take(4))
        {
            List<Bitmap> variants = null;
            try
            {
                variants = CreateVariants(image.Bitmap);
                foreach (var variant in variants)
                {
                    var text = await RecognizeBitmapAsync(engine, variant);

                    // Most Silkroad gateways use a numeric code, but a few
                    // servers mix letters in. Try the numeric interpretation
                    // first and keep the literal one as a fallback.
                    var code = NormalizeDigits(text);
                    if (code.Length >= 4 && code.Length <= 8)
                    {
                        results.Add(new Result
                        {
                            Code = code,
                            Score = Score(text, code),
                            Label = image.Label
                        });
                    }

                    var literal = NormalizeLiteral(text);
                    if (literal.Length >= 4 && literal.Length <= 8 && literal != code)
                    {
                        results.Add(new Result
                        {
                            Code = literal,
                            Score = Score(text, literal) - 8,
                            Label = image.Label
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warn($"[AutoCaptcha] OCR failed for '{image.Label}': {ex.Message}");
            }
            finally
            {
                if (variants != null)
                {
                    foreach (var variant in variants)
                        variant.Dispose();
                }
            }
        }

        return Rank(results);
    }

    private static OcrEngine CreateEngine()
    {
        try
        {
            return OcrEngine.TryCreateFromLanguage(new Language("en-US"))
                   ?? OcrEngine.TryCreateFromUserProfileLanguages();
        }
        catch (Exception ex)
        {
            Log.Warn($"[AutoCaptcha] Could not initialize Windows OCR: {ex.Message}");
            return null;
        }
    }

    private static List<Bitmap> CreateVariants(Bitmap source)
    {
        var variants = new List<Bitmap>(4);
        var gray = ToGray(source);

        try
        {
            variants.Add(Scale(source, 3));

            var scaledGray = Scale(gray, 3);
            variants.Add(scaledGray);

            var threshold = Otsu(gray);
            using (var normal = Threshold(gray, threshold, false))
                variants.Add(Scale(normal, 3));

            using (var inverted = Threshold(gray, threshold, true))
                variants.Add(Scale(inverted, 3));
        }
        finally
        {
            gray.Dispose();
        }

        return variants;
    }

    private static async Task<string> RecognizeBitmapAsync(OcrEngine engine, Bitmap bitmap)
    {
        using var memory = new MemoryStream();
        bitmap.Save(memory, ImageFormat.Png);
        var bytes = memory.ToArray();

        using var stream = new InMemoryRandomAccessStream();
        using (var writer = new DataWriter(stream.GetOutputStreamAt(0)))
        {
            writer.WriteBytes(bytes);
            await writer.StoreAsync();
            await writer.FlushAsync();
            writer.DetachStream();
        }

        stream.Seek(0);

        var decoder = await BitmapDecoder.CreateAsync(stream);
        using var softwareBitmap = await decoder.GetSoftwareBitmapAsync();
        var result = await engine.RecognizeAsync(softwareBitmap);
        return result?.Text ?? string.Empty;
    }

    private static Bitmap ToGray(Bitmap source)
    {
        // Normalize to 24bpp first: LockBits requires an exact pixel format,
        // while decoded captcha candidates can be 24bpp, 32bpp, indexed, ...
        using var rgb = new Bitmap(source.Width, source.Height, PixelFormat.Format24bppRgb);
        using (var graphics = Graphics.FromImage(rgb))
        {
            graphics.CompositingMode = CompositingMode.SourceCopy;
            graphics.DrawImageUnscaled(source, 0, 0);
        }

        var bitmap = new Bitmap(source.Width, source.Height, PixelFormat.Format24bppRgb);
        var sourceBits = rgb.LockBits(new Rectangle(0, 0, rgb.Width, rgb.Height),
            ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        var targetBits = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

        try
        {
            var sourceData = new byte[sourceBits.Stride * rgb.Height];
            var targetData = new byte[targetBits.Stride * bitmap.Height];
            Marshal.Copy(sourceBits.Scan0, sourceData, 0, sourceData.Length);

            for (var y = 0; y < rgb.Height; y++)
            {
                var sourceRow = y * sourceBits.Stride;
                var targetRow = y * targetBits.Stride;
                for (var x = 0; x < rgb.Width; x++)
                {
                    var index = sourceRow + x * 3;
                    var value = (byte)((sourceData[index] * 29 + sourceData[index + 1] * 150 +
                                        sourceData[index + 2] * 77) >> 8);
                    var targetIndex = targetRow + x * 3;
                    targetData[targetIndex] = value;
                    targetData[targetIndex + 1] = value;
                    targetData[targetIndex + 2] = value;
                }
            }

            Marshal.Copy(targetData, 0, targetBits.Scan0, targetData.Length);
        }
        finally
        {
            rgb.UnlockBits(sourceBits);
            bitmap.UnlockBits(targetBits);
        }

        return bitmap;
    }

    private static Bitmap Scale(Bitmap source, int factor)
    {
        var bitmap = new Bitmap(source.Width * factor, source.Height * factor, PixelFormat.Format24bppRgb);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.CompositingMode = CompositingMode.SourceCopy;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.DrawImage(source, 0, 0, bitmap.Width, bitmap.Height);
        return bitmap;
    }

    private static Bitmap Threshold(Bitmap source, int threshold, bool invert)
    {
        var bitmap = new Bitmap(source.Width, source.Height, PixelFormat.Format24bppRgb);
        var sourceBits = source.LockBits(new Rectangle(0, 0, source.Width, source.Height),
            ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
        var targetBits = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

        try
        {
            var sourceData = new byte[sourceBits.Stride * source.Height];
            var targetData = new byte[targetBits.Stride * bitmap.Height];
            Marshal.Copy(sourceBits.Scan0, sourceData, 0, sourceData.Length);

            for (var y = 0; y < source.Height; y++)
            {
                var sourceRow = y * sourceBits.Stride;
                var targetRow = y * targetBits.Stride;
                for (var x = 0; x < source.Width; x++)
                {
                    var value = sourceData[sourceRow + x * 3];
                    var white = value > threshold;
                    if (invert)
                        white = !white;

                    var color = white ? (byte)255 : (byte)0;
                    var targetIndex = targetRow + x * 3;
                    targetData[targetIndex] = color;
                    targetData[targetIndex + 1] = color;
                    targetData[targetIndex + 2] = color;
                }
            }

            Marshal.Copy(targetData, 0, targetBits.Scan0, targetData.Length);
        }
        finally
        {
            source.UnlockBits(sourceBits);
            bitmap.UnlockBits(targetBits);
        }

        return bitmap;
    }

    private static int Otsu(Bitmap gray)
    {
        var histogram = new int[256];
        var bits = gray.LockBits(new Rectangle(0, 0, gray.Width, gray.Height), ImageLockMode.ReadOnly,
            PixelFormat.Format24bppRgb);

        try
        {
            var data = new byte[bits.Stride * gray.Height];
            Marshal.Copy(bits.Scan0, data, 0, data.Length);

            for (var y = 0; y < gray.Height; y++)
            {
                var row = y * bits.Stride;
                for (var x = 0; x < gray.Width; x++)
                    histogram[data[row + x * 3]]++;
            }
        }
        finally
        {
            gray.UnlockBits(bits);
        }

        var total = gray.Width * gray.Height;
        double sum = 0;
        for (var i = 0; i < 256; i++)
            sum += i * histogram[i];

        double sumBackground = 0;
        var weightBackground = 0;
        var bestThreshold = 128;
        double bestVariance = 0;

        for (var threshold = 0; threshold < 256; threshold++)
        {
            weightBackground += histogram[threshold];
            if (weightBackground == 0)
                continue;

            var weightForeground = total - weightBackground;
            if (weightForeground == 0)
                break;

            sumBackground += threshold * histogram[threshold];

            var meanBackground = sumBackground / weightBackground;
            var meanForeground = (sum - sumBackground) / weightForeground;
            var variance = (double)weightBackground * weightForeground *
                           (meanBackground - meanForeground) * (meanBackground - meanForeground);

            if (variance > bestVariance)
            {
                bestVariance = variance;
                bestThreshold = threshold;
            }
        }

        return bestThreshold;
    }

    private static string NormalizeDigits(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var builder = new StringBuilder(text.Length);
        foreach (var character in text)
        {
            var value = char.ToUpperInvariant(character);
            if (value >= '0' && value <= '9')
            {
                builder.Append(value);
                continue;
            }

            if (Replacements.TryGetValue(value, out var replacement))
                builder.Append(replacement);
        }

        return builder.ToString();
    }

    /// <summary>
    ///     Keeps the characters as the OCR engine returned them (uppercased).
    ///     Used for gateways whose captcha contains letters.
    /// </summary>
    private static string NormalizeLiteral(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var builder = new StringBuilder(text.Length);
        foreach (var character in text)
        {
            var value = char.ToUpperInvariant(character);
            if ((value >= '0' && value <= '9') || (value >= 'A' && value <= 'Z'))
                builder.Append(value);
        }

        return builder.ToString();
    }

    private static int Score(string text, string code)
    {
        var score = code.Length switch
        {
            4 => 30,
            5 => 24,
            6 => 26,
            7 => 18,
            8 => 10,
            _ => 0
        };

        var digits = text.Count(char.IsDigit);
        var alphanumeric = text.Count(char.IsLetterOrDigit);
        var junk = alphanumeric - digits;
        score -= junk * 2;
        score -= Math.Max(0, text.Length - code.Length - 4);
        return score;
    }

    private static List<string> Rank(IEnumerable<Result> results)
    {
        return results
            .GroupBy(result => result.Code)
            .Select(group => new
            {
                Code = group.Key,
                Count = group.Count(),
                Score = group.Max(result => result.Score) + (group.Count() - 1) * 8
            })
            .OrderByDescending(item => item.Score)
            .ThenByDescending(item => item.Count)
            .ThenByDescending(item => item.Code.Length == 6)
            .ThenByDescending(item => item.Code.Length == 4)
            .Select(item => item.Code)
            .Distinct()
            .ToList();
    }
}
