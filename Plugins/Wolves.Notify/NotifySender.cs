using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using RSBot.Core;

namespace Wolves.Notify;

internal sealed record NotifyField(string Name, string Value);

internal sealed record NotifyPayload(
    string EventKey,
    string CharacterName,
    string Message,
    IReadOnlyList<NotifyField> Fields,
    string DedupKey = null);

/// <summary>
///     Thread-safe async dispatcher for Telegram and Discord notifications with
///     per-event cooldown and cross-process deduplication for server-wide alerts.
/// </summary>
internal static class NotifySender
{
    private const int DedupWindowSeconds = 8;
    private static readonly string DedupDirectory = Path.Combine(Path.GetTempPath(), "WolvesBot.Notify.Dedup");

    private static readonly HttpClient Http = CreateHttpClient();
    private static readonly ConcurrentDictionary<string, DateTime> LastSentUtc = new(StringComparer.OrdinalIgnoreCase);

    private static readonly Channel<NotifyPayload> Queue = Channel.CreateBounded<NotifyPayload>(
        new BoundedChannelOptions(200)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        });

    private static int _workerStarted;
    private static DateTime _lastDedupCleanupUtc = DateTime.MinValue;

    public static event Action<string> OnHistoryAdded;

    private static HttpClient CreateHttpClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        client.DefaultRequestHeaders.UserAgent.Clear();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("WolvesBotNotify", "1.0"));
        return client;
    }

    public static void StartWorker()
    {
        if (Interlocked.Exchange(ref _workerStarted, 1) == 1)
            return;

        Task.Run(ProcessQueueAsync);
    }

    public static void Enqueue(
        string eventKey,
        string message,
        IReadOnlyList<NotifyField> fields = null,
        string dedupKey = null,
        bool ignoreToggle = false)
    {
        var config = NotifyConfig.Current;
        if (!ignoreToggle && !config.IsEventEnabled(eventKey))
            return;

        if (!HasAnyDestinationConfigured(config))
            return;

        var meta = NotifyEvents.GetMeta(eventKey);
        if (!ignoreToggle && !meta.BypassCooldown && config.CooldownSeconds > 0)
        {
            var now = DateTime.UtcNow;
            if (LastSentUtc.TryGetValue(eventKey, out var previous) &&
                (now - previous).TotalSeconds < config.CooldownSeconds)
            {
                return;
            }
            LastSentUtc[eventKey] = now;
        }

        if (!ignoreToggle && meta.CrossProcessDedup && !TryClaimCrossProcessDedup(eventKey, dedupKey ?? message))
            return;

        var charName = Game.Player?.Name;
        if (string.IsNullOrWhiteSpace(charName))
            charName = "WolvesBot";

        Queue.Writer.TryWrite(new NotifyPayload(eventKey, charName, message ?? string.Empty, fields, dedupKey));
    }

    /// <summary>
    ///     Sends a test notification immediately using the provided config so the UI can report exact results.
    /// </summary>
    public static async Task<(bool Success, string Summary)> SendDirectTestAsync(NotifyConfig config, NotifyPayload payload)
    {
        var results = new List<string>();
        var anyConfigured = false;
        var allSucceeded = true;

        var (tgText, tgHtml, dcText, dcEmbed) = RenderMessages(config, payload);

        if (config.TelegramEnabled && !string.IsNullOrWhiteSpace(config.TelegramToken) && !string.IsNullOrWhiteSpace(config.TelegramChatId))
        {
            anyConfigured = true;
            try
            {
                await SendTelegramAsync(config.TelegramToken, config.TelegramChatId, tgText, tgHtml).ConfigureAwait(false);
                results.Add("Telegram OK");
            }
            catch (Exception ex)
            {
                allSucceeded = false;
                results.Add($"Telegram FAIL: {ex.Message}");
            }
        }

        if (config.DiscordEnabled && !string.IsNullOrWhiteSpace(config.DiscordWebhook))
        {
            anyConfigured = true;
            try
            {
                await SendDiscordAsync(config.DiscordWebhook, dcText, dcEmbed).ConfigureAwait(false);
                results.Add("Discord OK");
            }
            catch (Exception ex)
            {
                allSucceeded = false;
                results.Add($"Discord FAIL: {ex.Message}");
            }
        }

        if (!anyConfigured)
            return (false, "No destination enabled/configured");

        var summary = string.Join(" | ", results);
        RecordHistory($"[{DateTime.Now:HH:mm:ss}] TEST -> {summary}");
        return (allSucceeded, summary);
    }

    private static bool HasAnyDestinationConfigured(NotifyConfig config)
    {
        var tgReady = config.TelegramEnabled &&
                      !string.IsNullOrWhiteSpace(config.TelegramToken) &&
                      !string.IsNullOrWhiteSpace(config.TelegramChatId);
        var dcReady = config.DiscordEnabled &&
                      !string.IsNullOrWhiteSpace(config.DiscordWebhook);
        return tgReady || dcReady;
    }

    private static async Task ProcessQueueAsync()
    {
        await foreach (var payload in Queue.Reader.ReadAllAsync().ConfigureAwait(false))
        {
            var config = NotifyConfig.Current;
            var meta = NotifyEvents.GetMeta(payload.EventKey);
            var (tgText, tgHtml, dcText, dcEmbed) = RenderMessages(config, payload);

            var deliveredAny = false;
            if (config.TelegramEnabled &&
                !string.IsNullOrWhiteSpace(config.TelegramToken) &&
                !string.IsNullOrWhiteSpace(config.TelegramChatId))
            {
                deliveredAny |= await SendWithRetryAsync("Telegram", () =>
                    SendTelegramAsync(config.TelegramToken, config.TelegramChatId, tgText, tgHtml)).ConfigureAwait(false);
            }

            if (config.DiscordEnabled && !string.IsNullOrWhiteSpace(config.DiscordWebhook))
            {
                deliveredAny |= await SendWithRetryAsync("Discord", () =>
                    SendDiscordAsync(config.DiscordWebhook, dcText, dcEmbed)).ConfigureAwait(false);
            }

            if (deliveredAny)
            {
                var line = $"[{DateTime.Now:HH:mm:ss}] {meta.Emoji} [{payload.CharacterName}] {meta.GetLocalizedTitle()}: {payload.Message}";
                RecordHistory(line);
            }
        }
    }

    private static async Task<bool> SendWithRetryAsync(string channelName, Func<Task> sendAction)
    {
        for (var attempt = 1; attempt <= 2; attempt++)
        {
            try
            {
                await sendAction().ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                Log.Debug($"[Wolves.Notify] {channelName} attempt {attempt} failed: {ex.Message}");
                if (attempt < 2)
                    await Task.Delay(1500).ConfigureAwait(false);
            }
        }

        return false;
    }

    private static async Task SendTelegramAsync(string token, string chatId, string text, bool parseAsHtml)
    {
        var url = $"https://api.telegram.org/bot{token.Trim()}/sendMessage";
        var formFields = new Dictionary<string, string>
        {
            ["chat_id"] = chatId.Trim(),
            ["text"] = text,
            ["disable_web_page_preview"] = "true"
        };
        if (parseAsHtml)
            formFields["parse_mode"] = "HTML";

        using var content = new FormUrlEncodedContent(formFields);
        using var response = await Http.PostAsync(url, content).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            throw new HttpRequestException($"HTTP {(int)response.StatusCode}: {Truncate(body, 180)}");
        }
    }

    private static async Task SendDiscordAsync(string webhookUrl, string plainText, object embedObject)
    {
        object bodyObj = embedObject != null
            ? new { embeds = new[] { embedObject } }
            : new { content = plainText };

        var json = JsonSerializer.Serialize(bodyObj);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await Http.PostAsync(webhookUrl.Trim(), content).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            throw new HttpRequestException($"HTTP {(int)response.StatusCode}: {Truncate(body, 180)}");
        }
    }

    private static (string TgText, bool TgHtml, string DcText, object DcEmbed) RenderMessages(
        NotifyConfig config,
        NotifyPayload payload)
    {
        var meta = NotifyEvents.GetMeta(payload.EventKey);
        var title = meta.GetLocalizedTitle();

        var plainSb = new StringBuilder();
        plainSb.Append(meta.Emoji).Append(' ').Append(title).Append(" — ").AppendLine(payload.CharacterName);
        if (!string.IsNullOrWhiteSpace(payload.Message))
            plainSb.AppendLine(payload.Message);
        if (payload.Fields != null)
        {
            foreach (var f in payload.Fields)
                plainSb.Append(f.Name).Append(": ").AppendLine(f.Value);
        }

        var plainText = plainSb.ToString().Trim();
        if (!config.RichCards)
            return (plainText, false, plainText, null);

        var tgSb = new StringBuilder();
        tgSb.Append(meta.Emoji)
            .Append(" <b>").Append(WebUtility.HtmlEncode(title)).Append("</b> — <code>")
            .Append(WebUtility.HtmlEncode(payload.CharacterName)).AppendLine("</code>");
        if (!string.IsNullOrWhiteSpace(payload.Message))
            tgSb.AppendLine(WebUtility.HtmlEncode(payload.Message));
        if (payload.Fields != null && payload.Fields.Count > 0)
        {
            foreach (var f in payload.Fields)
            {
                tgSb.Append("<b>").Append(WebUtility.HtmlEncode(f.Name)).Append(":</b> ")
                    .AppendLine(WebUtility.HtmlEncode(f.Value));
            }
        }

        var embedFields = new List<object>();
        if (payload.Fields != null)
        {
            foreach (var f in payload.Fields)
            {
                embedFields.Add(new { name = f.Name, value = string.IsNullOrEmpty(f.Value) ? "—" : f.Value, inline = true });
            }
        }

        var discordEmbed = new
        {
            title = $"{meta.Emoji} {title}",
            description = payload.Message,
            color = meta.EmbedColor,
            author = new { name = payload.CharacterName },
            fields = embedFields,
            footer = new { text = $"WolvesBot Notify • {DateTime.Now:HH:mm:ss}" }
        };

        return (tgSb.ToString().Trim(), true, plainText, discordEmbed);
    }

    private static bool TryClaimCrossProcessDedup(string eventKey, string detail)
    {
        try
        {
            Directory.CreateDirectory(DedupDirectory);
            CleanupDedupDirectory();

            var raw = $"{eventKey}:{detail}".Trim();
            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
            var hashHex = Convert.ToHexString(hashBytes, 0, 12);
            var markerPath = Path.Combine(DedupDirectory, hashHex);

            var nowUtc = DateTime.UtcNow;
            if (File.Exists(markerPath))
            {
                var age = nowUtc - File.GetLastWriteTimeUtc(markerPath);
                if (age.TotalSeconds < DedupWindowSeconds)
                    return false;
            }

            using var stream = new FileStream(markerPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            File.SetLastWriteTimeUtc(markerPath, nowUtc);
            return true;
        }
        catch
        {
            return true;
        }
    }

    private static void CleanupDedupDirectory()
    {
        var now = DateTime.UtcNow;
        if ((now - _lastDedupCleanupUtc).TotalMinutes < 5)
            return;
        _lastDedupCleanupUtc = now;

        try
        {
            foreach (var file in Directory.GetFiles(DedupDirectory))
            {
                if ((now - File.GetLastWriteTimeUtc(file)).TotalMinutes > 30)
                    File.Delete(file);
            }
        }
        catch
        {
        }
    }

    private static void RecordHistory(string line)
    {
        OnHistoryAdded?.Invoke(line);
    }

    private static string Truncate(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text ?? string.Empty;

        return text[..maxLength];
    }
}
