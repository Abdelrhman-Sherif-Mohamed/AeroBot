using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RSBot.Core.Security;

/// <summary>
/// Core authentication and licensing service connecting to Firebase Realtime Database.
/// Enforces maximum 16 accounts and hardware-locked (HWID) security with auto-block.
/// </summary>
public class AuthService
{
    private static AuthService _instance;
    public static AuthService Instance => _instance ??= new AuthService();

    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public UserAccount CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser != null;

    public bool IsAdmin =>
        CurrentUser != null &&
        (CurrentUser.IsAdmin ||
         string.Equals(CurrentUser.Username, "abdosonek", StringComparison.OrdinalIgnoreCase));

    public static event Action OnSubscriptionExpiredEvent;
    public static void TriggerSubscriptionExpired() => OnSubscriptionExpiredEvent?.Invoke();

    public AuthService()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };
    }

    private string BuildUrl(string path)
    {
        var config = AuthConfig.Instance;
        var baseUrl = (config.FirebaseUrl ?? string.Empty).Trim().TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
            baseUrl = "https://wolves-1eb1f-default-rtdb.firebaseio.com/";

        if (!baseUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !baseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            baseUrl = "https://" + baseUrl;
        }

        path = path.TrimStart('/');
        var url = $"{baseUrl}/{path}";

        if (!string.IsNullOrWhiteSpace(config.AuthSecret))
        {
            var separator = url.Contains("?") ? "&" : "?";
            url += $"{separator}auth={Uri.EscapeDataString(config.AuthSecret)}";
        }

        return url;
    }

    public static string ComputeHash(string password, string salt)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes($"{salt}:{password}:WOLVES_SECURE_SALT_2026");
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }

    /// <summary>
    /// Checks connectivity to the Firebase database.
    /// </summary>
    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            var url = BuildUrl(".json?shallow=true");
            using var response = await _httpClient.GetAsync(url);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Fetches global auth settings (e.g. MaxAccounts).
    /// </summary>
    public async Task<GlobalAuthSettings> GetSettingsAsync()
    {
        try
        {
            var url = BuildUrl("settings.json");
            var json = await _httpClient.GetStringAsync(url);
            if (string.IsNullOrWhiteSpace(json) || json.Trim() == "null")
                return new GlobalAuthSettings();

            return JsonSerializer.Deserialize<GlobalAuthSettings>(json, _jsonOptions) ?? new GlobalAuthSettings();
        }
        catch
        {
            return new GlobalAuthSettings();
        }
    }

    /// <summary>
    /// Saves global auth settings.
    /// </summary>
    public async Task<bool> SaveSettingsAsync(GlobalAuthSettings settings)
    {
        try
        {
            var url = BuildUrl("settings.json");
            var json = JsonSerializer.Serialize(settings, _jsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await _httpClient.PutAsync(url, content);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Retrieves all registered accounts.
    /// </summary>
    public async Task<Dictionary<string, UserAccount>> GetAllAccountsAsync()
    {
        try
        {
            var url = BuildUrl("accounts.json");
            var json = await _httpClient.GetStringAsync(url);
            if (string.IsNullOrWhiteSpace(json) || json.Trim() == "null")
                return new Dictionary<string, UserAccount>(StringComparer.OrdinalIgnoreCase);

            var dict = JsonSerializer.Deserialize<Dictionary<string, UserAccount>>(json, _jsonOptions);
            return dict ?? new Dictionary<string, UserAccount>(StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new Dictionary<string, UserAccount>(StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Retrieves a single account by username.
    /// </summary>
    public async Task<UserAccount> GetAccountAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return null;

        try
        {
            var safeUser = Uri.EscapeDataString(username.Trim().ToLowerInvariant());
            var url = BuildUrl($"accounts/{safeUser}.json");
            var json = await _httpClient.GetStringAsync(url);
            if (string.IsNullOrWhiteSpace(json) || json.Trim() == "null")
                return null;

            return JsonSerializer.Deserialize<UserAccount>(json, _jsonOptions);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Saves or updates an account.
    /// </summary>
    public async Task<bool> SaveAccountAsync(UserAccount account)
    {
        if (account == null || string.IsNullOrWhiteSpace(account.Username))
            return false;

        try
        {
            var safeUser = Uri.EscapeDataString(account.Username.Trim().ToLowerInvariant());
            var url = BuildUrl($"accounts/{safeUser}.json");
            var json = JsonSerializer.Serialize(account, _jsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await _httpClient.PutAsync(url, content);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Deletes an account (frees up a slot).
    /// </summary>
    public async Task<bool> DeleteAccountAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;

        try
        {
            var safeUser = Uri.EscapeDataString(username.Trim().ToLowerInvariant());
            var url = BuildUrl($"accounts/{safeUser}.json");
            using var response = await _httpClient.DeleteAsync(url);
            if (response.IsSuccessStatusCode)
            {
                await LogSecurityEventAsync(username, "ACCOUNT_DELETED", "", "", "تم حذف الحساب بواسطة الأدمن");
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Appends a security audit or activity log entry.
    /// </summary>
    public async Task LogSecurityEventAsync(string username, string action, string hwid, string ip, string details)
    {
        try
        {
            var entry = new SecurityLogEntry
            {
                Timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                Username = username,
                Action = action,
                HWID = hwid,
                IP = ip,
                Details = details
            };

            var url = BuildUrl("logs.json");
            var json = JsonSerializer.Serialize(entry, _jsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            await _httpClient.PostAsync(url, content);
        }
        catch
        {
            // Logging failure should never crash the main operation
        }
    }

    /// <summary>
    /// Retrieves recent security logs.
    /// </summary>
    public async Task<List<SecurityLogEntry>> GetLogsAsync()
    {
        var list = new List<SecurityLogEntry>();
        try
        {
            var url = BuildUrl("logs.json");
            var json = await _httpClient.GetStringAsync(url);
            if (string.IsNullOrWhiteSpace(json) || json.Trim() == "null")
                return list;

            var dict = JsonSerializer.Deserialize<Dictionary<string, SecurityLogEntry>>(json, _jsonOptions);
            if (dict != null)
            {
                list.AddRange(dict.Values);
                list.Sort((a, b) => string.Compare(b.Timestamp, a.Timestamp, StringComparison.Ordinal));
            }
        }
        catch { }
        return list;
    }

    /// <summary>
    /// Registers a new account. Enforces the strict 16-account maximum and locks to current HWID.
    /// </summary>
    public async Task<AuthResult> RegisterAsync(string username, string password, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(username))
            return AuthResult.Fail("يرجى إدخال اسم المستخدم.");

        username = username.Trim();
        if (username.Length < 3 || username.Length > 20)
            return AuthResult.Fail("يجب أن يكون اسم المستخدم بين 3 إلى 20 حرفاً.");

        if (string.IsNullOrWhiteSpace(password) || password.Length < 4)
            return AuthResult.Fail("كلمة المرور يجب أن تتكون من 4 أحرف أو أرقام على الأقل.");

        if (password != confirmPassword)
            return AuthResult.Fail("كلمة المرور وتأكيد كلمة المرور غير متطابقين.");

        try
        {
            // 1. Check Global Settings and Max Accounts limit (16 max)
            var settings = await GetSettingsAsync();
            if (settings.MaintenanceMode)
                return AuthResult.Fail("النظام في وضع الصيانة حالياً. يرجى المحاولة لاحقاً.");

            var allAccounts = await GetAllAccountsAsync();
            var maxCap = settings.MaxAccounts > 0 ? settings.MaxAccounts : 16;

            if (allAccounts.Count >= maxCap)
            {
                return AuthResult.Fail($"تم الوصول إلى الحد الأقصى للحسابات المسموح بها ({maxCap} حساباً فقط).\nلا يمكن تسجيل أي حساب جديد حالياً، يرجى التواصل مع الإدارة.");
            }

            // 2. Check if username is already taken
            var key = username.ToLowerInvariant();
            if (allAccounts.ContainsKey(key))
            {
                return AuthResult.Fail("اسم المستخدم هذا مسجل بالفعل. يرجى اختيار اسم مستخدم آخر.");
            }

            // 3. Extract device hardware ID and IP
            var hwid = HardwareId.GetHwid();
            var ip = await HardwareId.GetPublicIpAsync();

            // 4. Generate salt and hash
            var salt = Guid.NewGuid().ToString("N");
            var hash = ComputeHash(password, salt);

            var initialExpiresAt = settings.DefaultTrialHours > 0
                ? DateTime.UtcNow.AddHours(settings.DefaultTrialHours).ToString("yyyy-MM-dd HH:mm:ss")
                : string.Empty;

            var account = new UserAccount
            {
                Username = username,
                PasswordHash = hash,
                Salt = salt,
                HWID = hwid,
                RegisteredIP = ip,
                LastLoginIP = ip,
                IsBlocked = false,
                BlockedReason = string.Empty,
                CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                LastLoginAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                IsOnline = true,
                Notes = string.Empty,
                ExpiresAt = initialExpiresAt,
                IsLifetime = false,
                IsAdmin = false
            };

            var saved = await SaveAccountAsync(account);
            if (!saved)
            {
                return AuthResult.Fail("حدث خطأ أثناء حفظ الحساب في السيرفر. يرجى المحاولة مرة أخرى.");
            }

            await LogSecurityEventAsync(username, "REGISTER", hwid, ip, $"تم تسجيل حساب جديد بنجاح وربطه بالجهاز ({allAccounts.Count + 1}/{maxCap})");

            CurrentUser = account;
            return AuthResult.Ok($"تم إنشاء الحساب بنجاح وربطه بهذا الجهاز!\n(الحساب {allAccounts.Count + 1} من أصل {maxCap})", account);
        }
        catch (Exception ex)
        {
            return AuthResult.Fail($"تعذر الاتصال بالسيرفر: {ex.Message}");
        }
    }

    /// <summary>
    /// Authenticates a user. Validates password and enforces single-device HWID lock with Auto-Block.
    /// </summary>
    public async Task<AuthResult> LoginAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            return AuthResult.Fail("يرجى إدخال اسم المستخدم.");

        if (string.IsNullOrWhiteSpace(password))
            return AuthResult.Fail("يرجى إدخال كلمة المرور.");

        username = username.Trim();

        try
        {
            var account = await GetAccountAsync(username);
            if (account == null)
            {
                return AuthResult.Fail("اسم المستخدم أو كلمة المرور غير صحيحة.");
            }

            var currentHwid = HardwareId.GetHwid();
            var currentIp = await HardwareId.GetPublicIpAsync();

            // Check password
            var expectedHash = ComputeHash(password, account.Salt ?? string.Empty);
            if (!string.Equals(expectedHash, account.PasswordHash, StringComparison.OrdinalIgnoreCase))
            {
                await LogSecurityEventAsync(username, "LOGIN_FAILED_PASSWORD", currentHwid, currentIp, "محاولة تسجيل دخول بكلمة مرور خاطئة");
                return AuthResult.Fail("اسم المستخدم أو كلمة المرور غير صحيحة.");
            }

            // Check if already blocked
            if (account.IsBlocked)
            {
                await LogSecurityEventAsync(username, "LOGIN_REJECTED_BLOCKED", currentHwid, currentIp, "محاولة تسجيل دخول لحساب محظور مسبقاً");
                var reason = string.IsNullOrWhiteSpace(account.BlockedReason) ? "مخالفة شروط الاستخدام" : account.BlockedReason;
                return AuthResult.Fail($"هذا الحساب محظور!\n\nسبب الحظر:\n{reason}\n\nيرجى التواصل مع إدارة Wolves لفك الحظر.");
            }

            // Case A: HWID is empty (Admin performed Reset HWID to allow PC transfer)
            if (string.IsNullOrWhiteSpace(account.HWID))
            {
                account.HWID = currentHwid;
                account.LastLoginIP = currentIp;
                account.LastLoginAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                account.IsOnline = true;

                await SaveAccountAsync(account);
                await LogSecurityEventAsync(username, "HWID_REBIND", currentHwid, currentIp, "تم ربط جهاز جديد بالحساب بعد إعادة التعيين من الأدمن");

                CurrentUser = account;
                return AuthResult.Ok("تم تسجيل الدخول وربط جهازك الجديد بنجاح!", account);
            }

            // Case B: HWID matches current machine
            if (string.Equals(account.HWID, currentHwid, StringComparison.OrdinalIgnoreCase))
            {
                account.LastLoginIP = currentIp;
                account.LastLoginAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
                account.IsOnline = true;

                await SaveAccountAsync(account);
                await LogSecurityEventAsync(username, "LOGIN_SUCCESS", currentHwid, currentIp, "تسجيل دخول ناجح");

                CurrentUser = account;
                return AuthResult.Ok("تم تسجيل الدخول بنجاح!", account);
            }

            // Case C: HWID DOES NOT MATCH! Multi-Device Security Violation!
            // AUTO-BLOCK THE ACCOUNT IMMEDIATELY!
            account.IsBlocked = true;
            account.BlockedReason = $"حظر تلقائي: محاولة فتح الحساب من جهاز ثانٍ غير مصرح به!\n(جهاز مسجل: {account.HWID})\n(جهاز حاول الدخول: {currentHwid}, IP: {currentIp})\nبتاريخ: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
            account.IsOnline = false;

            await SaveAccountAsync(account);

            await LogSecurityEventAsync(
                username,
                "AUTO_BLOCK_HWID_MISMATCH",
                currentHwid,
                currentIp,
                $"🚨 تنبيه أمني: محاولة فتح حساب {username} من جهاز آخر! تم حظر الحساب تلقائياً."
            );

            return AuthResult.Fail(
                "🚨 تم حظر هذا الحساب تلقائياً!\n\n" +
                "السبب: محاولة تسجيل الدخول من جهاز آخر مختلف عن الجهاز المسجل عليه الحساب.\n" +
                "النظام يسمح بجهاز واحد فقط لكل حساب.\n\n" +
                "تم تسجيل بصمة الجهاز (HWID) وعنوان الـ IP وإرسال تنبيه فوري للإدارة.\n" +
                "يرجى التواصل مع الأدمن لفك الحظر."
            );
        }
        catch (Exception ex)
        {
            return AuthResult.Fail($"تعذر الاتصال بسيرفر التحقق:\n{ex.Message}\nيرجى التأكد من اتصال الإنترنت وإعدادات السيرفر.");
        }
    }

    /// <summary>
    /// Admin: Unblocks an account.
    /// </summary>
    public async Task<bool> UnblockAccountAsync(string username)
    {
        var account = await GetAccountAsync(username);
        if (account == null) return false;

        account.IsBlocked = false;
        account.BlockedReason = string.Empty;
        var res = await SaveAccountAsync(account);
        if (res)
        {
            await LogSecurityEventAsync(username, "UNBLOCK", "", "", "تم فك الحظر عن الحساب بواسطة الأدمن");
        }
        return res;
    }

    /// <summary>
    /// Admin: Manually blocks an account with a custom reason.
    /// </summary>
    public async Task<bool> BlockAccountAsync(string username, string reason)
    {
        var account = await GetAccountAsync(username);
        if (account == null) return false;

        account.IsBlocked = true;
        account.BlockedReason = string.IsNullOrWhiteSpace(reason) ? "تم الحظر بواسطة الأدمن" : reason;
        account.IsOnline = false;
        var res = await SaveAccountAsync(account);
        if (res)
        {
            await LogSecurityEventAsync(username, "MANUAL_BLOCK", "", "", $"تم حظر الحساب يدوياً: {account.BlockedReason}");
        }
        return res;
    }

    /// <summary>
    /// Admin: Resets HWID binding, allowing the user to bind a new computer on next login.
    /// </summary>
    public async Task<bool> ResetHwidAsync(string username)
    {
        var account = await GetAccountAsync(username);
        if (account == null) return false;

        var oldHwid = account.HWID;
        account.HWID = string.Empty;
        var res = await SaveAccountAsync(account);
        if (res)
        {
            await LogSecurityEventAsync(username, "HWID_RESET", oldHwid, "", "تمت إعادة تعيين بصمة الجهاز بواسطة الأدمن");
        }
        return res;
    }

    /// <summary>
    /// Admin: Sets a new password for an account.
    /// </summary>
    public async Task<bool> ChangePasswordAsync(string username, string newPassword)
    {
        var account = await GetAccountAsync(username);
        if (account == null) return false;

        var salt = Guid.NewGuid().ToString("N");
        account.Salt = salt;
        account.PasswordHash = ComputeHash(newPassword, salt);
        var res = await SaveAccountAsync(account);
        if (res)
        {
            await LogSecurityEventAsync(username, "PASSWORD_CHANGED", "", "", "تم تغيير كلمة المرور بواسطة الأدمن");
        }
        return res;
    }

    /// <summary>
    /// Admin: Creates an account directly without HWID (first login will bind).
    /// </summary>
    public async Task<AuthResult> AdminCreateAccountAsync(string username, string password, string notes = "", bool isAdmin = false)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return AuthResult.Fail("اسم المستخدم وكلمة المرور مطلوبان.");

        username = username.Trim();
        var all = await GetAllAccountsAsync();
        if (all.Count >= 16)
            return AuthResult.Fail("تم الوصول للحد الأقصى للحسابات (16 حساباً).");

        if (all.ContainsKey(username.ToLowerInvariant()))
            return AuthResult.Fail("اسم المستخدم موجود مسبقاً.");

        var salt = Guid.NewGuid().ToString("N");
        var hash = ComputeHash(password, salt);

        var account = new UserAccount
        {
            Username = username,
            PasswordHash = hash,
            Salt = salt,
            HWID = string.Empty, // Will bind on user's first login
            RegisteredIP = "Admin Created",
            LastLoginIP = "",
            IsBlocked = false,
            BlockedReason = "",
            CreatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            LastLoginAt = "",
            IsOnline = false,
            Notes = notes,
            IsAdmin = isAdmin
        };

        var ok = await SaveAccountAsync(account);
        if (ok)
        {
            await LogSecurityEventAsync(username, "ADMIN_CREATE", "", "", "تم إنشاء الحساب يدوياً بواسطة الأدمن");
            return AuthResult.Ok("تم إنشاء الحساب بنجاح!", account);
        }

        return AuthResult.Fail("فشل حفظ الحساب.");
    }

    /// <summary>
    /// Admin: Adds subscription days to an account.
    /// If currently expired, starts from now. If active, extends from current expiry date.
    /// </summary>
    public async Task<bool> AddSubscriptionDaysAsync(string username, int days)
    {
        var account = await GetAccountAsync(username);
        if (account == null) return false;

        DateTime baseDate = DateTime.UtcNow;
        if (!account.IsExpired && account.ExpirationDate.HasValue && account.ExpirationDate.Value > DateTime.UtcNow)
        {
            baseDate = account.ExpirationDate.Value;
        }

        account.ExpiresAt = baseDate.AddDays(days).ToString("yyyy-MM-dd HH:mm:ss");
        account.IsLifetime = false;

        var res = await SaveAccountAsync(account);
        if (res)
        {
            await LogSecurityEventAsync(username, "SUBSCRIPTION_ADD_DAYS", "", "", $"تمت إضافة {days} يوم للاشتراك (ينتهي في: {account.ExpiresAt})");
            if (CurrentUser != null && string.Equals(CurrentUser.Username, username, StringComparison.OrdinalIgnoreCase))
                CurrentUser = account;
        }
        return res;
    }

    /// <summary>
    /// Admin: Adds subscription months (30 days * months) to an account.
    /// </summary>
    public async Task<bool> AddSubscriptionMonthsAsync(string username, int months)
    {
        var account = await GetAccountAsync(username);
        if (account == null) return false;

        DateTime baseDate = DateTime.UtcNow;
        if (!account.IsExpired && account.ExpirationDate.HasValue && account.ExpirationDate.Value > DateTime.UtcNow)
        {
            baseDate = account.ExpirationDate.Value;
        }

        account.ExpiresAt = baseDate.AddMonths(months).ToString("yyyy-MM-dd HH:mm:ss");
        account.IsLifetime = false;

        var res = await SaveAccountAsync(account);
        if (res)
        {
            await LogSecurityEventAsync(username, "SUBSCRIPTION_ADD_MONTHS", "", "", $"تمت إضافة {months} شهر للاشتراك (ينتهي في: {account.ExpiresAt})");
            if (CurrentUser != null && string.Equals(CurrentUser.Username, username, StringComparison.OrdinalIgnoreCase))
                CurrentUser = account;
        }
        return res;
    }

    /// <summary>
    /// Admin: Adds subscription hours to an account.
    /// </summary>
    public async Task<bool> AddSubscriptionHoursAsync(string username, int hours)
    {
        var account = await GetAccountAsync(username);
        if (account == null) return false;

        DateTime baseDate = DateTime.UtcNow;
        if (!account.IsExpired && account.ExpirationDate.HasValue && account.ExpirationDate.Value > DateTime.UtcNow)
        {
            baseDate = account.ExpirationDate.Value;
        }

        account.ExpiresAt = baseDate.AddHours(hours).ToString("yyyy-MM-dd HH:mm:ss");
        account.IsLifetime = false;

        var res = await SaveAccountAsync(account);
        if (res)
        {
            await LogSecurityEventAsync(username, "SUBSCRIPTION_ADD_HOURS", "", "", $"تمت إضافة {hours} ساعة للاشتراك (ينتهي في: {account.ExpiresAt})");
            if (CurrentUser != null && string.Equals(CurrentUser.Username, username, StringComparison.OrdinalIgnoreCase))
                CurrentUser = account;
        }
        return res;
    }

    /// <summary>
    /// Admin: Sets an exact subscription expiration date and time.
    /// </summary>
    public async Task<bool> SetSubscriptionExpiryAsync(string username, DateTime expiresAtUtc)
    {
        var account = await GetAccountAsync(username);
        if (account == null) return false;

        account.ExpiresAt = expiresAtUtc.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");
        account.IsLifetime = false;

        var res = await SaveAccountAsync(account);
        if (res)
        {
            await LogSecurityEventAsync(username, "SUBSCRIPTION_SET_DATE", "", "", $"تم تحديد تاريخ انتهاء الاشتراك إلى: {account.ExpiresAt}");
            if (CurrentUser != null && string.Equals(CurrentUser.Username, username, StringComparison.OrdinalIgnoreCase))
                CurrentUser = account;
        }
        return res;
    }

    /// <summary>
    /// Admin: Sets or unsets lifetime subscription status.
    /// </summary>
    public async Task<bool> SetLifetimeAsync(string username, bool isLifetime)
    {
        var account = await GetAccountAsync(username);
        if (account == null) return false;

        account.IsLifetime = isLifetime;
        var res = await SaveAccountAsync(account);
        if (res)
        {
            await LogSecurityEventAsync(username, "SUBSCRIPTION_SET_LIFETIME", "", "", isLifetime ? "تم تفعيل اشتراك دائم (Lifetime)" : "تم إلغاء الاشتراك الدائم");
            if (CurrentUser != null && string.Equals(CurrentUser.Username, username, StringComparison.OrdinalIgnoreCase))
                CurrentUser = account;
        }
        return res;
    }

    /// <summary>
    /// Admin: Immediately expires an account subscription.
    /// </summary>
    public async Task<bool> ExpireAccountNowAsync(string username)
    {
        var account = await GetAccountAsync(username);
        if (account == null) return false;

        account.ExpiresAt = DateTime.UtcNow.AddMinutes(-5).ToString("yyyy-MM-dd HH:mm:ss");
        account.IsLifetime = false;

        var res = await SaveAccountAsync(account);
        if (res)
        {
            await LogSecurityEventAsync(username, "SUBSCRIPTION_EXPIRED_NOW", "", "", "تم إنهاء صلاحية الاشتراك فورياً بواسطة الأدمن");
            if (CurrentUser != null && string.Equals(CurrentUser.Username, username, StringComparison.OrdinalIgnoreCase))
            {
                CurrentUser = account;
                TriggerSubscriptionExpired();
            }
        }
        return res;
    }

    /// <summary>
    /// Admin: Sets admin privileges for an account.
    /// </summary>
    public async Task<bool> SetAdminStatusAsync(string username, bool isAdmin)
    {
        var account = await GetAccountAsync(username);
        if (account == null) return false;

        account.IsAdmin = isAdmin;
        var res = await SaveAccountAsync(account);
        if (res)
        {
            await LogSecurityEventAsync(username, "SET_ADMIN", "", "", isAdmin ? "تم منح صلاحيات الأدمن" : "تم سحب صلاحيات الأدمن");
            if (CurrentUser != null && string.Equals(CurrentUser.Username, username, StringComparison.OrdinalIgnoreCase))
                CurrentUser = account;
        }
        return res;
    }

    /// <summary>
    /// Validates current session's subscription against Firebase.
    /// Returns true if valid; false if expired or blocked.
    /// </summary>
    public async Task<bool> CheckCurrentSubscriptionAsync()
    {
        if (CurrentUser == null)
            return false;

        try
        {
            var fresh = await GetAccountAsync(CurrentUser.Username);
            if (fresh == null || fresh.IsBlocked)
            {
                TriggerSubscriptionExpired();
                return false;
            }

            CurrentUser = fresh;
            return true;
        }
        catch
        {
            // If offline, check local cached expiration
            if (CurrentUser.IsExpired)
            {
                TriggerSubscriptionExpired();
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Starts a session in Free Tier mode.
    /// </summary>
    public void SetFreeTierSession()
    {
        var hwid = HardwareId.GetHwid();
        var shortHwid = hwid.Length > 8 ? hwid.Substring(0, 8) : hwid;
        CurrentUser = new UserAccount
        {
            Username = $"FreeUser_{shortHwid}",
            IsFreeTier = true,
            IsLifetime = false,
            IsAdmin = false,
            HWID = hwid
        };
        PermissionManager.SessionStartTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Fetches free tier permissions from Firebase.
    /// </summary>
    public async Task<FreeTierPermissions> GetFreeTierPermissionsAsync()
    {
        try
        {
            var url = BuildUrl("free_permissions.json");
            var json = await _httpClient.GetStringAsync(url);
            if (string.IsNullOrWhiteSpace(json) || json.Trim() == "null")
                return PermissionManager.Permissions;

            var perms = JsonSerializer.Deserialize<FreeTierPermissions>(json, _jsonOptions) ?? new FreeTierPermissions();
            PermissionManager.Permissions = perms;
            PermissionManager.SaveLocalCache(perms);
            return perms;
        }
        catch
        {
            var cached = PermissionManager.LoadLocalCache();
            PermissionManager.Permissions = cached;
            return cached;
        }
    }

    /// <summary>
    /// Saves free tier permissions to Firebase.
    /// </summary>
    public async Task<bool> SaveFreeTierPermissionsAsync(FreeTierPermissions perms)
    {
        try
        {
            var url = BuildUrl("free_permissions.json");
            var json = JsonSerializer.Serialize(perms, _jsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await _httpClient.PutAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                PermissionManager.Permissions = perms;
                PermissionManager.SaveLocalCache(perms);
                await LogSecurityEventAsync("Admin", "UPDATE_FREE_PERMISSIONS", "", "", "تم تحديث صلاحيات ومميزات النسخة المجانية");
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
}
