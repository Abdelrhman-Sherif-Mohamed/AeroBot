using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using RSBot.Core;

namespace Wolves.Notify;

/// <summary>
///     Global persistent configuration for Telegram and Discord notifications.
/// </summary>
internal sealed class NotifyConfig
{
    public const string DefaultUniqueKeywords =
        "Tiger Girl, Cerberus, Captain Ivy, Uruchi, Isyutaru, Lord Yarkan, Demon Shaitan, Medusa, Roc";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private static string ConfigFilePath =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Config", "Wolves.Notify.json");

    public bool Enabled { get; set; } = true;
    public bool TelegramEnabled { get; set; } = true;
    public string TelegramToken { get; set; } = string.Empty;
    public string TelegramChatId { get; set; } = string.Empty;
    public bool DiscordEnabled { get; set; } = true;
    public string DiscordWebhook { get; set; } = string.Empty;
    public int CooldownSeconds { get; set; } = 10;
    public bool RichCards { get; set; } = true;
    public string UniqueKeywords { get; set; } = DefaultUniqueKeywords;

    public Dictionary<string, bool> Events { get; set; } = CreateDefaultEvents();

    public static NotifyConfig Current { get; private set; } = new();

    public static Dictionary<string, bool> CreateDefaultEvents()
    {
        return new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
        {
            [NotifyEvents.Death] = true,
            [NotifyEvents.BotStopped] = true,
            [NotifyEvents.Attacked] = true,
            [NotifyEvents.TradeStart] = true,
            [NotifyEvents.TradeSettle] = true,
            [NotifyEvents.HpPotsOut] = true,
            [NotifyEvents.MpPotsOut] = true,
            [NotifyEvents.PrivateMessage] = true,
            [NotifyEvents.Global] = true,
            [NotifyEvents.Notice] = true,
            [NotifyEvents.Unique] = true,
            [NotifyEvents.LevelUp] = true,
            [NotifyEvents.RareDrop] = true,
            [NotifyEvents.PartyChat] = false,
            [NotifyEvents.GuildChat] = false,
            [NotifyEvents.UnionChat] = false,
            [NotifyEvents.AcademyChat] = false,
            [NotifyEvents.AllChat] = false,
            [NotifyEvents.StallChat] = false
        };
    }

    public bool IsEventEnabled(string key)
    {
        if (!Enabled || string.IsNullOrEmpty(key))
            return false;

        return Events != null && Events.TryGetValue(key, out var enabled) && enabled;
    }

    public static void Load()
    {
        try
        {
            var path = ConfigFilePath;
            if (!File.Exists(path))
            {
                Current = new NotifyConfig();
                return;
            }

            var json = File.ReadAllText(path);
            var loaded = JsonSerializer.Deserialize<NotifyConfig>(json);
            if (loaded == null)
            {
                Current = new NotifyConfig();
                return;
            }

            var defaults = CreateDefaultEvents();
            if (loaded.Events != null)
            {
                foreach (var pair in loaded.Events)
                    defaults[pair.Key] = pair.Value;
            }

            loaded.Events = defaults;
            loaded.CooldownSeconds = Math.Clamp(loaded.CooldownSeconds, 0, 3600);
            loaded.TelegramToken ??= string.Empty;
            loaded.TelegramChatId ??= string.Empty;
            loaded.DiscordWebhook ??= string.Empty;
            if (string.IsNullOrWhiteSpace(loaded.UniqueKeywords))
                loaded.UniqueKeywords = DefaultUniqueKeywords;

            Current = loaded;
        }
        catch (Exception ex)
        {
            Log.Error($"[Wolves.Notify] Failed to load config: {ex.Message}");
            Current = new NotifyConfig();
        }
    }

    public static void Save(NotifyConfig config)
    {
        try
        {
            var path = ConfigFilePath;
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            config.CooldownSeconds = Math.Clamp(config.CooldownSeconds, 0, 3600);
            File.WriteAllText(path, JsonSerializer.Serialize(config, JsonOptions));
            Current = config;
        }
        catch (Exception ex)
        {
            Log.Error($"[Wolves.Notify] Failed to save config: {ex.Message}");
        }
    }

    /// <summary>
    ///     Returns a masked representation safe for displaying in the UI.
    /// </summary>
    public static string MaskSecret(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var trimmed = value.Trim();
        if (trimmed.Length <= 8)
            return new string('*', trimmed.Length);

        return string.Concat(trimmed.AsSpan(0, 4), "...", trimmed.AsSpan(trimmed.Length - 4));
    }

    /// <summary>
    ///     Resolves a textbox value back to the stored secret if the user did not overwrite the mask.
    /// </summary>
    public static string ResolveSecret(string inputValue, string existingValue)
    {
        var trimmed = (inputValue ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(trimmed))
            return string.Empty;

        if (!string.IsNullOrEmpty(existingValue) && trimmed == MaskSecret(existingValue))
            return existingValue;

        return trimmed;
    }
}
