using System;
using System.Globalization;
using System.Text.Json.Serialization;

namespace RSBot.Core.Security;

/// <summary>
/// Represents a user account stored in Firebase Realtime Database.
/// </summary>
public class UserAccount
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("passwordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [JsonPropertyName("salt")]
    public string Salt { get; set; } = string.Empty;

    [JsonPropertyName("hwid")]
    public string HWID { get; set; } = string.Empty;

    [JsonPropertyName("registeredIp")]
    public string RegisteredIP { get; set; } = string.Empty;

    [JsonPropertyName("lastLoginIp")]
    public string LastLoginIP { get; set; } = string.Empty;

    [JsonPropertyName("isBlocked")]
    public bool IsBlocked { get; set; }

    [JsonPropertyName("blockedReason")]
    public string BlockedReason { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("lastLoginAt")]
    public string LastLoginAt { get; set; } = string.Empty;

    [JsonPropertyName("isOnline")]
    public bool IsOnline { get; set; }

    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;

    [JsonPropertyName("expiresAt")]
    public string ExpiresAt { get; set; } = string.Empty;

    [JsonPropertyName("isLifetime")]
    public bool IsLifetime { get; set; }

    [JsonPropertyName("isAdmin")]
    public bool IsAdmin { get; set; }

    [JsonPropertyName("isFreeTier")]
    public bool IsFreeTier { get; set; }

    [JsonIgnore]
    public DateTime? ExpirationDate
    {
        get
        {
            if (string.IsNullOrWhiteSpace(ExpiresAt))
                return null;

            if (DateTime.TryParse(ExpiresAt, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dtUtc))
                return dtUtc;

            if (DateTime.TryParse(ExpiresAt, out var dtLocal))
                return dtLocal.ToUniversalTime();

            return null;
        }
    }

    [JsonIgnore]
    public bool IsExpired
    {
        get
        {
            // Subscription expiration disabled per admin request - all accounts work normally
            return false;
        }
    }

    [JsonIgnore]
    public TimeSpan? RemainingTime
    {
        get
        {
            if (IsLifetime || IsAdmin || IsFreeTier)
                return null;

            var exp = ExpirationDate;
            if (!exp.HasValue)
                return null;

            var diff = exp.Value - DateTime.UtcNow;
            return diff > TimeSpan.Zero ? diff : null;
        }
    }

    public string GetFormattedRemainingTime()
    {
        if (IsFreeTier)
            return "نسخة مجانية (Free Tier)";

        if (IsLifetime)
            return "اشتراك دائم (Lifetime)";

        if (IsAdmin)
            return "حساب مسؤول (Admin)";

        var exp = ExpirationDate;
        if (exp.HasValue && DateTime.UtcNow < exp.Value)
        {
            var diff = exp.Value - DateTime.UtcNow;
            if (diff.TotalDays >= 1)
            {
                var days = (int)diff.TotalDays;
                var hours = diff.Hours;
                return $"{days} يوم و {hours} ساعة";
            }
            if (diff.TotalHours >= 1)
            {
                var hours = (int)diff.TotalHours;
                var minutes = diff.Minutes;
                return $"{hours} ساعة و {minutes} دقيقة";
            }
            var mins = Math.Max(1, (int)diff.TotalMinutes);
            return $"{mins} دقيقة";
        }

        return "حساب نشط (Active)";
    }
}

/// <summary>
/// Security violation and activity audit log entry.
/// </summary>
public class SecurityLogEntry
{
    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("action")]
    public string Action { get; set; } = string.Empty;

    [JsonPropertyName("hwid")]
    public string HWID { get; set; } = string.Empty;

    [JsonPropertyName("ip")]
    public string IP { get; set; } = string.Empty;

    [JsonPropertyName("details")]
    public string Details { get; set; } = string.Empty;
}

/// <summary>
/// Global application and licensing settings stored on Firebase.
/// </summary>
public class GlobalAuthSettings
{
    [JsonPropertyName("maxAccounts")]
    public int MaxAccounts { get; set; } = 16;

    [JsonPropertyName("notice")]
    public string Notice { get; set; } = string.Empty;

    [JsonPropertyName("maintenanceMode")]
    public bool MaintenanceMode { get; set; } = false;

    [JsonPropertyName("whatsAppNumber")]
    public string WhatsAppNumber { get; set; } = "+201150238481";

    [JsonPropertyName("whatsAppUrl")]
    public string WhatsAppUrl { get; set; } = "https://wa.me/201150238481";

    [JsonPropertyName("rechargeMessage")]
    public string RechargeMessage { get; set; } = "برجاء إرسال رسالة لنا لإعادة شحن البوت";

    [JsonPropertyName("defaultTrialHours")]
    public int DefaultTrialHours { get; set; } = 0;
}

/// <summary>
/// Result of an authentication or registration attempt.
/// </summary>
public class AuthResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public UserAccount Account { get; set; }
    public string ErrorCode { get; set; } = string.Empty;

    public static AuthResult Ok(string message, UserAccount account = null) =>
        new() { Success = true, Message = message, Account = account };

    public static AuthResult Fail(string message, string errorCode = "") =>
        new() { Success = false, Message = message, ErrorCode = errorCode };
}
