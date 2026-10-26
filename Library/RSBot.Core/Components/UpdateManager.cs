using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using RSBot.Core.Security;

namespace RSBot.Core.Components;

public class AppUpdateInfo
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "2.9.9";

    [JsonPropertyName("downloadUrl")]
    public string DownloadUrl { get; set; } = string.Empty;

    [JsonPropertyName("mandatory")]
    public bool Mandatory { get; set; } = false;

    [JsonPropertyName("changelog_ar")]
    public string ChangelogAr { get; set; } = string.Empty;

    [JsonPropertyName("changelog_en")]
    public string ChangelogEn { get; set; } = string.Empty;

    [JsonPropertyName("releasedAt")]
    public string ReleasedAt { get; set; } = string.Empty;

    [JsonIgnore]
    public bool HasNewVersion { get; set; }

    public string GetChangelog(bool isArabic)
    {
        if (isArabic)
        {
            return !string.IsNullOrWhiteSpace(ChangelogAr) ? ChangelogAr : ChangelogEn;
        }
        return !string.IsNullOrWhiteSpace(ChangelogEn) ? ChangelogEn : ChangelogAr;
    }
}

public static class UpdateManager
{
    private static readonly HttpClient _httpClient = new() { Timeout = TimeSpan.FromSeconds(60) };

    public static string LastError { get; set; } = string.Empty;

    static UpdateManager()
    {
        try
        {
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) WolvesBot/2.9");
        }
        catch { }
    }

    public static Version CurrentVersion
    {
        get
        {
            try
            {
                var entryAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
                var ver = entryAssembly.GetName().Version;
                if (ver != null && ver > new Version(0, 0, 0))
                    return new Version(ver.Major, ver.Minor, Math.Max(0, ver.Build));
            }
            catch { }
            return new Version(2, 9, 17);
        }
    }

    public static string DisplayVersion => $"{CurrentVersion.Major}.{CurrentVersion.Minor}.{CurrentVersion.Build}";

    public static bool IsArabic
    {
        get
        {
            var lang = Kernel.Language;
            if (string.IsNullOrWhiteSpace(lang))
                lang = GlobalConfig.Get("RSBot.Language", "ar_AR");

            return (lang ?? string.Empty).StartsWith("ar", StringComparison.OrdinalIgnoreCase);
        }
    }

    private static string GetJustUpdatedFilePath()
    {
        var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        if (!Directory.Exists(dataDir))
        {
            try { Directory.CreateDirectory(dataDir); } catch { }
        }
        return Path.Combine(dataDir, "just_updated.json");
    }

    /// <summary>
    /// Checks Firebase Realtime Database for an updated version.
    /// </summary>
    public static async Task<AppUpdateInfo> CheckForUpdateAsync()
    {
        try
        {
            var firebaseUrl = AuthConfig.Instance.FirebaseUrl;
            if (string.IsNullOrWhiteSpace(firebaseUrl))
                firebaseUrl = "https://wolves-1eb1f-default-rtdb.firebaseio.com/";

            var endpoint = firebaseUrl.TrimEnd('/') + "/update.json";
            if (!string.IsNullOrWhiteSpace(AuthConfig.Instance.AuthSecret))
                endpoint += "?auth=" + AuthConfig.Instance.AuthSecret;

            var response = await _httpClient.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json) || json == "null")
                return null;

            var info = JsonSerializer.Deserialize<AppUpdateInfo>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (info == null || string.IsNullOrWhiteSpace(info.Version))
                return null;

            if (TryParseVersion(info.Version, out var remoteVer))
            {
                if (remoteVer > CurrentVersion)
                {
                    info.HasNewVersion = true;
                    return info;
                }
            }

            return info;
        }
        catch (Exception ex)
        {
            Log.Debug($"[UpdateManager] Check update error: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Publishes or updates the latest release info in Firebase.
    /// </summary>
    public static async Task<bool> PublishUpdateAsync(AppUpdateInfo info)
    {
        try
        {
            var firebaseUrl = AuthConfig.Instance.FirebaseUrl;
            if (string.IsNullOrWhiteSpace(firebaseUrl))
                firebaseUrl = "https://wolves-1eb1f-default-rtdb.firebaseio.com/";

            var endpoint = firebaseUrl.TrimEnd('/') + "/update.json";
            if (!string.IsNullOrWhiteSpace(AuthConfig.Instance.AuthSecret))
                endpoint += "?auth=" + AuthConfig.Instance.AuthSecret;

            var json = JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true });
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync(endpoint, content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Log.Error($"[UpdateManager] Publish failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Downloads the update package (.zip) from downloadUrl to destinationPath with progress reporting (0-100%).
    /// </summary>
    public static async Task<bool> DownloadUpdateAsync(string downloadUrl, string destinationPath, IProgress<int> progress, CancellationToken ct = default)
    {
        LastError = string.Empty;
        if (string.IsNullOrWhiteSpace(downloadUrl))
        {
            LastError = "Download URL is empty";
            return false;
        }

        try
        {
            var dir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (File.Exists(destinationPath))
            {
                try
                {
                    var attr = File.GetAttributes(destinationPath);
                    if ((attr & (FileAttributes.ReadOnly | FileAttributes.Hidden)) != 0)
                        File.SetAttributes(destinationPath, FileAttributes.Normal);
                    File.Delete(destinationPath);
                }
                catch { }
            }

            // 1. Support local file paths (e.g. E:\... or file:///...)
            var isHttp = downloadUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                         downloadUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

            if (!isHttp && (File.Exists(downloadUrl) || downloadUrl.StartsWith("file:", StringComparison.OrdinalIgnoreCase)))
            {
                var localFilePath = downloadUrl.StartsWith("file:", StringComparison.OrdinalIgnoreCase)
                    ? new Uri(downloadUrl).LocalPath
                    : downloadUrl;

                if (File.Exists(localFilePath))
                {
                    var fileInfo = new FileInfo(localFilePath);
                    var totalBytes = fileInfo.Length;

                    using var sourceStream = new FileStream(localFilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, true);
                    using var destStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);

                    var buffer = new byte[81920];
                    long totalRead = 0;
                    int bytesRead;

                    while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
                    {
                        await destStream.WriteAsync(buffer, 0, bytesRead, ct);
                        totalRead += bytesRead;
                        if (totalBytes > 0)
                        {
                            var percent = (int)((totalRead * 100) / totalBytes);
                            progress?.Report(Math.Clamp(percent, 0, 100));
                        }
                    }

                    progress?.Report(100);
                    return true;
                }
            }

            // 2. HTTP / HTTPS download
            using var response = await _httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!response.IsSuccessStatusCode)
            {
                LastError = $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}";
                return false;
            }

            var totalBytesHttp = response.Content.Headers.ContentLength ?? -1L;
            var canReportProgress = totalBytesHttp > 0 && progress != null;

            using var contentStream = await response.Content.ReadAsStreamAsync(ct);
            using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true);

            var httpBuffer = new byte[81920];
            long httpTotalRead = 0;
            int httpBytesRead;

            while ((httpBytesRead = await contentStream.ReadAsync(httpBuffer, 0, httpBuffer.Length, ct)) > 0)
            {
                await fileStream.WriteAsync(httpBuffer, 0, httpBytesRead, ct);
                httpTotalRead += httpBytesRead;

                if (canReportProgress)
                {
                    var percent = (int)((httpTotalRead * 100) / totalBytesHttp);
                    progress.Report(Math.Clamp(percent, 0, 100));
                }
            }

            progress?.Report(100);
            return true;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            Log.Error($"[UpdateManager] Download failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Writes the just_updated marker file, launches Wolves.Updater.exe, and exits the current process.
    /// </summary>
    public static void ApplyUpdateAndRestart(string zipPath, AppUpdateInfo info)
    {
        try
        {
            // 1. Record what was updated so the new version can show the What's New box
            var markerPath = GetJustUpdatedFilePath();
            var json = JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(markerPath, json, Encoding.UTF8);

            // 2. Locate Wolves.Updater.exe
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var updaterExe = Path.Combine(baseDir, "Wolves.Updater.exe");
            var currentExe = Process.GetCurrentProcess().MainModule?.FileName ?? Path.Combine(baseDir, "AeroBot.exe");
            var currentPid = Process.GetCurrentProcess().Id;

            if (!File.Exists(updaterExe))
            {
                Log.Error("[UpdateManager] Wolves.Updater.exe not found!");
                return;
            }

            var startInfo = new ProcessStartInfo(updaterExe)
            {
                Arguments = $"{currentPid} \"{zipPath}\" \"{baseDir.TrimEnd('\\')}\" \"{currentExe}\"",
                WorkingDirectory = baseDir,
                UseShellExecute = true
            };

            Process.Start(startInfo);
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Log.Error($"[UpdateManager] Failed to launch updater: {ex.Message}");
        }
    }

    /// <summary>
    /// Checks if the application was just updated and returns the update info.
    /// </summary>
    public static AppUpdateInfo GetJustUpdatedInfo()
    {
        try
        {
            var markerPath = GetJustUpdatedFilePath();
            if (!File.Exists(markerPath))
                return null;

            var json = File.ReadAllText(markerPath, Encoding.UTF8);
            if (string.IsNullOrWhiteSpace(json))
                return null;

            return JsonSerializer.Deserialize<AppUpdateInfo>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Clears the just_updated marker file.
    /// </summary>
    public static void ClearJustUpdatedMarker()
    {
        try
        {
            var markerPath = GetJustUpdatedFilePath();
            if (File.Exists(markerPath))
                File.Delete(markerPath);
        }
        catch { }
    }

    private static bool TryParseVersion(string input, out Version version)
    {
        version = null;
        if (string.IsNullOrWhiteSpace(input)) return false;

        input = input.Trim().TrimStart('v', 'V');
        var parts = input.Split('.');
        if (parts.Length == 1) input += ".0.0";
        else if (parts.Length == 2) input += ".0";

        return Version.TryParse(input, out version);
    }
}
