using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace RSBot.Core.Security;

public static class HardwareId
{
    private static string _cachedHwid;

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool GetVolumeInformation(
        string rootPathName,
        StringBuilder volumeNameBuffer,
        int volumeNameSize,
        out uint volumeSerialNumber,
        out uint maximumComponentLength,
        out uint fileSystemFlags,
        StringBuilder fileSystemNameBuffer,
        int nFileSystemNameSize);

    /// <summary>
    /// Generates a unique, persistent hardware fingerprint for the current PC.
    /// Combines Windows MachineGuid, Processor identifier, and Volume serial number.
    /// </summary>
    public static string GetHwid()
    {
        if (!string.IsNullOrEmpty(_cachedHwid))
            return _cachedHwid;

        try
        {
            var sb = new StringBuilder();

            // 1. Windows MachineGuid (Unique per PC installation)
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
                var guid = key?.GetValue("MachineGuid")?.ToString();
                if (!string.IsNullOrEmpty(guid))
                    sb.Append("GUID:").Append(guid).Append(";");
            }
            catch { }

            // 2. Processor Identifier
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
                var cpuId = key?.GetValue("ProcessorNameString")?.ToString() ?? key?.GetValue("Identifier")?.ToString();
                if (!string.IsNullOrEmpty(cpuId))
                    sb.Append("CPU:").Append(cpuId).Append(";");
            }
            catch { }

            // 3. System Drive Volume Serial Number
            try
            {
                var systemDrive = Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\";
                if (GetVolumeInformation(systemDrive, null, 0, out uint serialNumber, out _, out _, null, 0))
                {
                    sb.Append("VOL:").Append(serialNumber.ToString("X8")).Append(";");
                }
            }
            catch { }

            // 4. Machine Name fallback
            sb.Append("NAME:").Append(Environment.MachineName);

            // Compute SHA-256 hash
            using var sha = SHA256.Create();
            var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()));
            var hex = Convert.ToHexString(hashBytes);

            // Format as readable segmented HWID: WLV-XXXXXXXX-XXXXXXXX-XXXXXXXX
            _cachedHwid = $"WLV-{hex.Substring(0, 8)}-{hex.Substring(8, 8)}-{hex.Substring(16, 8)}";
            return _cachedHwid;
        }
        catch
        {
            // Ultimate fallback
            _cachedHwid = $"WLV-{Math.Abs(Environment.MachineName.GetHashCode()):X8}-{Math.Abs(Environment.UserName.GetHashCode()):X8}";
            return _cachedHwid;
        }
    }

    /// <summary>
    /// Asynchronously fetches public IP address with a fast 2.5s timeout.
    /// </summary>
    public static async Task<string> GetPublicIpAsync()
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2.5) };
            var ip = (await client.GetStringAsync("https://api.ipify.org")).Trim();
            if (!string.IsNullOrWhiteSpace(ip) && ip.Length < 45)
                return ip;
        }
        catch { }

        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2.5) };
            var ip = (await client.GetStringAsync("https://icanhazip.com")).Trim();
            if (!string.IsNullOrWhiteSpace(ip) && ip.Length < 45)
                return ip;
        }
        catch { }

        return "Unknown";
    }
}
