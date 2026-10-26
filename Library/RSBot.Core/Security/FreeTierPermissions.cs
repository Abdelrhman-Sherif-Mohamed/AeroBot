using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RSBot.Core.Security;

/// <summary>
/// Data model representing the permissions and conditions for the Free Tier of Wolves Bot.
/// Stored in Firebase Realtime Database at /free_permissions.json and cached locally.
/// </summary>
public class FreeTierPermissions
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    [JsonPropertyName("notice")]
    public string Notice { get; set; } = "أنت تستخدم النسخة المجانية من Wolves Bot. للاشتراك وفتح جميع المميزات تواصل مع الإدارة.";

    // --- Botbase Modes ---
    [JsonPropertyName("allowGrindingBot")]
    public bool AllowGrindingBot { get; set; } = true;

    [JsonPropertyName("allowTradeBot")]
    public bool AllowTradeBot { get; set; } = false;

    [JsonPropertyName("allowAlchemyBot")]
    public bool AllowAlchemyBot { get; set; } = false;

    [JsonPropertyName("allowLureBot")]
    public bool AllowLureBot { get; set; } = false;

    [JsonIgnore]
    public bool AllowGrinding { get => AllowGrindingBot; set => AllowGrindingBot = value; }
    [JsonIgnore]
    public bool AllowTrade { get => AllowTradeBot; set => AllowTradeBot = value; }
    [JsonIgnore]
    public bool AllowAlchemy { get => AllowAlchemyBot; set => AllowAlchemyBot = value; }
    [JsonIgnore]
    public bool AllowLure { get => AllowLureBot; set => AllowLureBot = value; }

    // --- Core Automation Features ---
    [JsonPropertyName("allowTownLoop")]
    public bool AllowTownLoop { get; set; } = true;

    [JsonPropertyName("allowAutoParty")]
    public bool AllowAutoParty { get; set; } = true;

    [JsonPropertyName("allowAutoPotion")]
    public bool AllowAutoPotion { get; set; } = true;

    [JsonPropertyName("allowAutoPickup")]
    public bool AllowAutoPickup { get; set; } = true;

    [JsonPropertyName("allowAutoResurrect")]
    public bool AllowAutoResurrect { get; set; } = true;

    [JsonPropertyName("allowStall")]
    public bool AllowStall { get; set; } = false;

    [JsonPropertyName("allowRemoteControl")]
    public bool AllowRemoteControl { get; set; } = false;

    // --- Limits ---
    [JsonPropertyName("maxClients")]
    public int MaxClients { get; set; } = 1; // 1 client instance for free tier (0 = unlimited)

    [JsonPropertyName("sessionMinutes")]
    public int SessionMinutes { get; set; } = 0; // 0 = unlimited session time
}

/// <summary>
/// Centralized manager for querying and enforcing feature permissions across the bot.
/// Paid subscribers and administrators bypass all restrictions.
/// </summary>
public static class PermissionManager
{
    private static FreeTierPermissions _permissions;
    public static FreeTierPermissions Permissions
    {
        get => _permissions ??= LoadLocalCache();
        set => _permissions = value;
    }

    public static DateTime SessionStartTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indicates whether the current user is operating under the Free Tier.
    /// Paid subscribers and admins return false (unrestricted).
    /// </summary>
    public static bool IsFreeUser
    {
        get
        {
            var user = AuthService.Instance.CurrentUser;
            if (user == null)
                return true;

            if (user.IsAdmin || user.IsLifetime)
                return false;

            if (user.IsFreeTier)
                return true;

            if (user.IsExpired)
                return true;

            return false;
        }
    }

    /// <summary>
    /// Checks whether a specific Botbase is allowed for the current session.
    /// </summary>
    public static bool CanUseBotbase(string botbaseName, out string reason)
    {
        reason = string.Empty;
        if (!IsFreeUser)
            return true;

        if (!Permissions.Enabled)
        {
            reason = "النسخة المجانية معطلة حالياً من قبل الإدارة. يرجى الاشتراك لتشغيل البوت.";
            return false;
        }

        if (string.Equals(botbaseName, "RSBot.Trade", StringComparison.OrdinalIgnoreCase))
        {
            if (!Permissions.AllowTradeBot)
            {
                reason = "نمط التريدة وتجارة القوافل (Trade Bot) غير متاح في النسخة المجانية.\nيرجى التواصل مع الإدارة للاشتراك وفتح هذه الميزة.";
                return false;
            }
        }
        else if (string.Equals(botbaseName, "RSBot.Alchemy", StringComparison.OrdinalIgnoreCase))
        {
            if (!Permissions.AllowAlchemyBot)
            {
                reason = "نمط الالكيمي والدمج التلقائي (Alchemy Bot) غير متاح في النسخة المجانية.\nيرجى التواصل مع الإدارة للاشتراك وفتح هذه الميزة.";
                return false;
            }
        }
        else if (string.Equals(botbaseName, "RSBot.Lure", StringComparison.OrdinalIgnoreCase))
        {
            if (!Permissions.AllowLureBot)
            {
                reason = "نمط سحب وجلب الوحوش (Lure Bot) غير متاح في النسخة المجانية.\nيرجى التواصل مع الإدارة للاشتراك وفتح هذه الميزة.";
                return false;
            }
        }
        else if (string.Equals(botbaseName, "RSBot.Default", StringComparison.OrdinalIgnoreCase))
        {
            if (!Permissions.AllowGrindingBot)
            {
                reason = "نمط التدريب التلقائي (Grinding Bot) غير متاح في النسخة المجانية حالياً.";
                return false;
            }
        }

        return true;
    }

    public static bool CanUseTownLoop => !IsFreeUser || (Permissions.Enabled && Permissions.AllowTownLoop);
    public static bool CanUseAutoParty => !IsFreeUser || (Permissions.Enabled && Permissions.AllowAutoParty);
    public static bool CanUseAutoPotion => !IsFreeUser || (Permissions.Enabled && Permissions.AllowAutoPotion);
    public static bool CanUseAutoPickup => !IsFreeUser || (Permissions.Enabled && Permissions.AllowAutoPickup);
    public static bool CanUseAutoResurrect => !IsFreeUser || (Permissions.Enabled && Permissions.AllowAutoResurrect);
    public static bool CanUseStall => !IsFreeUser || (Permissions.Enabled && Permissions.AllowStall);
    public static bool CanUseRemoteControl => !IsFreeUser || (Permissions.Enabled && Permissions.AllowRemoteControl);

    /// <summary>
    /// Checks if multi-client instance limit is exceeded for Free tier.
    /// </summary>
    public static bool CheckClientInstanceLimit(out int currentCount, out int maxLimit)
    {
        currentCount = 1;
        maxLimit = Permissions.MaxClients;

        if (!IsFreeUser || maxLimit <= 0)
            return true;

        try
        {
            var procName = Process.GetCurrentProcess().ProcessName;
            var instances = Process.GetProcessesByName(procName);
            currentCount = instances.Length;
            return currentCount <= maxLimit;
        }
        catch
        {
            return true;
        }
    }

    /// <summary>
    /// Checks if session duration limit is reached for Free tier.
    /// </summary>
    public static bool IsSessionExpired(out TimeSpan elapsed, out TimeSpan limit)
    {
        elapsed = DateTime.UtcNow - SessionStartTime;
        limit = TimeSpan.FromMinutes(Permissions.SessionMinutes);

        if (!IsFreeUser || Permissions.SessionMinutes <= 0)
            return false;

        return elapsed >= limit;
    }

    private static string GetCacheFilePath()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var dataDir = Path.Combine(baseDir, "Data");
        if (!Directory.Exists(dataDir))
        {
            try { Directory.CreateDirectory(dataDir); } catch { }
        }
        return Path.Combine(dataDir, "free_permissions.json");
    }

    public static FreeTierPermissions LoadLocalCache()
    {
        try
        {
            var path = GetCacheFilePath();
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path, Encoding.UTF8);
                var perms = JsonSerializer.Deserialize<FreeTierPermissions>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (perms != null)
                    return perms;
            }
        }
        catch { }

        return new FreeTierPermissions();
    }

    public static void SaveLocalCache(FreeTierPermissions perms)
    {
        try
        {
            var path = GetCacheFilePath();
            var json = JsonSerializer.Serialize(perms, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(path, json, Encoding.UTF8);
        }
        catch { }
    }

    /// <summary>
    /// Generates a structured Arabic summary of currently active permissions.
    /// </summary>
    public static string GetPermissionsSummaryArabic()
    {
        var sb = new StringBuilder();
        sb.AppendLine("📋 تفاصيل صلاحيات النسخة المجانية (Free Tier):");
        sb.AppendLine("───────────────────────────");

        if (!Permissions.Enabled)
        {
            sb.AppendLine("❌ النسخة المجانية: معطلة حالياً من قبل الإدارة.");
            return sb.ToString();
        }

        sb.AppendLine("✅ النسخة المجانية: مفعلة ومتاحة للجميع.");
        sb.AppendLine();
        sb.AppendLine("🔹 أنماط البوت:");
        sb.AppendLine($" • التدريب العادي (Grinding): {(Permissions.AllowGrindingBot ? "مفعل 🟢" : "معطل 🔴")}");
        sb.AppendLine($" • التريدة التلقائية (Trade Bot): {(Permissions.AllowTradeBot ? "مفعل 🟢" : "معطل 🔴")}");
        sb.AppendLine($" • الالكيمي التلقائي (Alchemy): {(Permissions.AllowAlchemyBot ? "مفعل 🟢" : "معطل 🔴")}");
        sb.AppendLine($" • السحب واللور (Lure Bot): {(Permissions.AllowLureBot ? "مفعل 🟢" : "معطل 🔴")}");
        sb.AppendLine();
        sb.AppendLine("🔹 الخدمات التلقائية:");
        sb.AppendLine($" • دورة المدينة والشراء: {(Permissions.AllowTownLoop ? "مفعل 🟢" : "معطل 🔴")}");
        sb.AppendLine($" • البارتي التلقائي: {(Permissions.AllowAutoParty ? "مفعل 🟢" : "معطل 🔴")}");
        sb.AppendLine($" • الحماية والجرع التلقائية: {(Permissions.AllowAutoPotion ? "مفعل 🟢" : "معطل 🔴")}");
        sb.AppendLine($" • فلتر اللوت والالتقاط: {(Permissions.AllowAutoPickup ? "مفعل 🟢" : "معطل 🔴")}");
        sb.AppendLine($" • الإنعاش التلقائي: {(Permissions.AllowAutoResurrect ? "مفعل 🟢" : "معطل 🔴")}");
        sb.AppendLine($" • البسطة التلقائية: {(Permissions.AllowStall ? "مفعل 🟢" : "معطل 🔴")}");
        sb.AppendLine();
        sb.AppendLine("🔹 القيود والحدود:");
        sb.AppendLine($" • الحد الأقصى للكلاينت: {(Permissions.MaxClients > 0 ? $"{Permissions.MaxClients} كلاينت" : "غير محدود")}");
        sb.AppendLine($" • مدة الجلسة: {(Permissions.SessionMinutes > 0 ? $"{Permissions.SessionMinutes} دقيقة" : "مفتوحة بدون حد")}");

        if (!string.IsNullOrWhiteSpace(Permissions.Notice))
        {
            sb.AppendLine();
            sb.AppendLine($"📢 إشعار الإدارة:\n{Permissions.Notice}");
        }

        return sb.ToString();
    }
}
