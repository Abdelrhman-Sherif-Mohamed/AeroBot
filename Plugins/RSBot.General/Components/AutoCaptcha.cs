using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Network;
using RSBot.Core.Network.Protocol;

namespace RSBot.General.Components;

/// <summary>
///     Handles the image based captcha (<c>0x2322</c>) that some gateway
///     servers send during login. The image is zlib compressed and may be
///     split over several packets. Once the complete image is available it is
///     decoded, passed through Windows OCR and the result is sent back using
///     <c>0x6323</c>.
/// </summary>
internal static class AutoCaptcha
{
    private static readonly object Gate = new();
    private static readonly MemoryStream Buffer = new();

    private static int _expectedLength;
    private static int _width = 200;
    private static int _height = 64;
    private static byte _flags;
    private static CancellationTokenSource _flushToken;

    /// <summary>
    ///     Called when a <c>0x2322</c> packet is received.
    /// </summary>
    public static void HandleChallenge(Packet packet)
    {
        try
        {
            var flags = packet.ReadByte();
            var remaining = packet.ReadUShort();
            var compressedLength = packet.ReadUShort();
            var uncompressedLength = packet.ReadUShort();
            var width = packet.ReadUShort();
            var height = packet.ReadUShort();

            if (compressedLength > packet.Remaining)
            {
                Log.Warn($"[AutoCaptcha] Malformed captcha packet (image length {compressedLength}, " +
                         $"packet remaining {packet.Remaining}).");
                return;
            }

            var data = packet.ReadBytes(compressedLength);
            if (data == null || data.Length == 0)
                return;

            byte[] payload = null;
            int expected;
            int packetWidth;
            int packetHeight;
            byte packetFlags;

            lock (Gate)
            {
                if (Buffer.Length == 0)
                {
                    _expectedLength = uncompressedLength;
                    _width = width;
                    _height = height;
                    _flags = flags;
                }

                Buffer.Write(data, 0, data.Length);

                if (remaining == 0)
                {
                    payload = Buffer.ToArray();
                    expected = _expectedLength;
                    packetWidth = _width;
                    packetHeight = _height;
                    packetFlags = _flags;
                    Buffer.SetLength(0);
                    _flushToken?.Cancel();
                    _flushToken = null;
                }
                else
                {
                    // The image is split into more packets. Wait for the rest,
                    // but do not wait forever if a chunk is lost.
                    _flushToken?.Cancel();
                    var source = new CancellationTokenSource();
                    _flushToken = source;
                    var token = source.Token;
                    _ = Task.Delay(5000, token).ContinueWith(task =>
                    {
                        if (!task.IsCanceled)
                            FlushIncomplete();
                    }, TaskScheduler.Default);

                    return;
                }
            }

            Process(payload, expected, packetWidth, packetHeight, packetFlags);
        }
        catch (Exception ex)
        {
            Log.Warn($"[AutoCaptcha] Captcha packet failed: {ex.Message}");
        }
    }

    private static void FlushIncomplete()
    {
        byte[] payload;
        int expected;
        int width;
        int height;
        byte flags;

        lock (Gate)
        {
            if (Buffer.Length == 0)
                return;

            payload = Buffer.ToArray();
            expected = _expectedLength;
            width = _width;
            height = _height;
            flags = _flags;
            Buffer.SetLength(0);
            _flushToken = null;
        }

        Process(payload, expected, width, height, flags);
    }

    private static void Process(byte[] compressed, int expectedLength, int width, int height, byte flags)
    {
        _ = Task.Run(() => ProcessAsync(compressed, expectedLength, width, height, flags));
    }

    private static async Task ProcessAsync(byte[] compressed, int expectedLength, int width, int height, byte flags)
    {
        List<CaptchaImage> images = null;

        try
        {
            if (!GlobalConfig.Get("RSBot.General.EnableAutoCaptcha", true))
            {
                SendStaticFallback();
                return;
            }

            images = CaptchaImageDecoder.Decode(compressed, expectedLength, width, height, out var raw);
            SaveDump(compressed, raw, images, flags);

            if (images.Count == 0)
            {
                Log.Warn("[AutoCaptcha] Could not decode the captcha image " +
                         $"(flags=0x{flags:X2}, compressed={compressed.Length}, " +
                         $"uncompressed={expectedLength}, size={width}x{height}).");
                SendStaticFallback();
                return;
            }

            var candidates = await CaptchaOcr.RecognizeAsync(images);
            if (candidates.Count == 0)
            {
                Log.Warn("[AutoCaptcha] OCR could not read the captcha. " +
                         "The decoded images were saved in the Captcha folder.");
                SendStaticFallback();
                return;
            }

            var code = candidates[0];
            Log.Notify($"[AutoCaptcha] Captcha solved: {code}");

            if (candidates.Count > 1)
                Log.Debug($"[AutoCaptcha] Other candidates: {string.Join(", ", candidates.Skip(1).Take(4))}");

            SendCode(code);
        }
        catch (Exception ex)
        {
            Log.Warn($"[AutoCaptcha] Captcha processing failed: {ex.Message}");
            SendStaticFallback();
        }
        finally
        {
            if (images != null)
            {
                foreach (var image in images)
                    image.Bitmap.Dispose();
            }
        }
    }

    /// <summary>
    ///     Sends the answer for the current captcha challenge.
    /// </summary>
    public static void SendCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return;

        code = code.Trim();
        Log.Notify($"[AutoCaptcha] Sending captcha code: {code}");

        var packet = new Packet(0x6323);
        packet.WriteString(code);
        PacketManager.SendPacket(packet, PacketDestination.Server);
    }

    private static bool SendStaticFallback()
    {
        if (!GlobalConfig.Get<bool>("RSBot.General.EnableStaticCaptcha"))
            return false;

        var captcha = GlobalConfig.Get<string>("RSBot.General.StaticCaptcha");
        if (string.IsNullOrWhiteSpace(captcha))
            return false;

        Log.Notify($"[AutoCaptcha] Using the configured static captcha: {captcha}");
        SendCode(captcha);
        return true;
    }

    private static void SaveDump(byte[] compressed, byte[] raw, IReadOnlyList<CaptchaImage> images, byte flags)
    {
        try
        {
            var profile = ProfileManager.SelectedProfile;
            if (string.IsNullOrWhiteSpace(profile))
                profile = "Default";

            var directory = Path.Combine(Kernel.BasePath, "User", profile, "Captcha");
            Directory.CreateDirectory(directory);

            var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            if (compressed != null && compressed.Length > 0)
                File.WriteAllBytes(Path.Combine(directory, $"{stamp}_flags{flags:X2}.z"), compressed);

            if (raw != null && raw.Length > 0)
                File.WriteAllBytes(Path.Combine(directory, $"{stamp}_flags{flags:X2}.raw"), raw);

            for (var index = 0; index < images.Count && index < 6; index++)
            {
                var image = images[index];
                var name = $"{stamp}_{index}_{Sanitize(image.Label)}.png";
                image.Bitmap.Save(Path.Combine(directory, name), ImageFormat.Png);
            }

            PruneDumps(directory);
        }
        catch (Exception ex)
        {
            Log.Debug($"[AutoCaptcha] Could not save the captcha dump: {ex.Message}");
        }
    }

    private static string Sanitize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "image";

        var invalid = Path.GetInvalidFileNameChars();
        return new string(value.Select(character => invalid.Contains(character) ? '_' : character).ToArray());
    }

    private static void PruneDumps(string directory)
    {
        try
        {
            var files = new DirectoryInfo(directory).GetFiles()
                .OrderByDescending(file => file.LastWriteTimeUtc)
                .Skip(160)
                .ToArray();

            foreach (var file in files)
                file.Delete();
        }
        catch
        {
            // Diagnostics only.
        }
    }
}
