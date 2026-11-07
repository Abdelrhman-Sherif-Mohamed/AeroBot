using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RSBot.Core;

/// <summary>
///     Describes the result of a Silkroad client version detection.
/// </summary>
public sealed class ClientVersionInfo
{
    /// <summary>
    ///     Gets the directory the detection was performed on.
    /// </summary>
    public string Directory { get; init; }

    /// <summary>
    ///     Gets the detected client version (e.g. 194 for 1.194). <c>null</c> if unknown.
    /// </summary>
    public int? Version { get; init; }

    /// <summary>
    ///     Gets the matching <see cref="GameClientType" />. <c>null</c> if the version is unknown or unsupported.
    /// </summary>
    public GameClientType? ClientType { get; init; }

    /// <summary>
    ///     Gets a human readable description of the source the version was read from.
    /// </summary>
    public string Source { get; init; }

    /// <summary>
    ///     Gets a value indicating whether a client type could be resolved.
    /// </summary>
    public bool IsDetected => Version.HasValue && ClientType.HasValue;

    /// <summary>
    ///     Gets the detected version in the "1.xxx" format.
    /// </summary>
    public string VersionText => Version.HasValue ? $"1.{Version.Value:D3}" : "unknown";

    public override string ToString()
    {
        return IsDetected
            ? $"client version {VersionText} -> {ClientType} (source: {Source})"
            : $"client version could not be detected (source: {Source})";
    }
}

/// <summary>
///     Detects the version of a Silkroad game client by looking at the files that ship with it.
///     The launcher revision file (e.g. NewEra's patch.rev with <c>svt=194</c>), the executable
///     version resource and version strings inside the executable are used, in that order.
/// </summary>
public static class ClientVersionDetector
{
    private const int MinimumVersion = 180;
    private const int MaximumVersion = 399;

    /// <summary>
    ///     Files that usually carry the client/launcher revision.
    /// </summary>
    private static readonly string[] KnownVersionFiles =
    {
        "patch.rev", "version.txt", "client.version", "clientver.txt", "version.ini", "sv.version"
    };

    /// <summary>
    ///     Keys that may hold the client version inside a version file.
    /// </summary>
    private static readonly string[] VersionKeys =
    {
        "svt", "sv", "ver", "version", "client_version", "clientversion"
    };

    private static readonly Regex VersionPairRegex = new(@"(?<!\d)1\s*[.,]\s*(?<minor>\d{3})(?!\d)", RegexOptions.Compiled);

    private static readonly Regex KeyValueRegex = new(
        @"^[ \t]*(?<key>[A-Za-z][A-Za-z0-9 _.\-]*?)[ \t]*[=:][ \t]*(?<value>[^\r\n]+?)[ \t]*\r?$",
        RegexOptions.Compiled | RegexOptions.Multiline);

    private static readonly Regex PlainVersionRegex = new(@"(?<![\d.])(?<number>\d{3})(?![\d.])", RegexOptions.Compiled);

    /// <summary>
    ///     Detects the client version of the given Silkroad directory.
    /// </summary>
    /// <param name="directory">The Silkroad client directory.</param>
    public static ClientVersionInfo Detect(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            return Unknown(directory, "client directory not found");

        // 1) Launcher/patch revision files, e.g. NewEra's patch.rev that contains "svt=194".
        foreach (var file in EnumerateVersionFiles(directory))
        {
            if (!TryReadVersionFromFile(file, out var version, out var key))
                continue;

            return Build(directory, version, $"{Path.GetFileName(file)} ({key}={version})");
        }

        var executables = FindClientExecutables(directory).ToList();
        if (executables.Count == 0)
            return Unknown(directory, "no version file or client executable found");

        foreach (var executable in executables)
        {
            // 2) Version resource of the client executable.
            if (TryReadExecutableVersion(executable, out var executableVersion))
                return Build(directory, executableVersion, $"executable version of {Path.GetFileName(executable)}");

            // 3) Version strings embedded in the client executable.
            if (TryScanExecutableStrings(executable, out var embeddedVersion))
                return Build(directory, embeddedVersion, $"version string inside {Path.GetFileName(executable)}");
        }

        return Unknown(directory,
            $"no version information in {string.Join(", ", executables.Select(Path.GetFileName))}");
    }

    /// <summary>
    ///     Maps a Silkroad client version (e.g. 188, 193, 194, 274) to the client type the bot has to use.
    /// </summary>
    /// <param name="version">The client version.</param>
    /// <returns>The matching client type or <c>null</c> if the version is not supported.</returns>
    public static GameClientType? MapVersionToClientType(int version)
    {
        if (version >= 270)
            return GameClientType.Vietnam274;

        if (version >= 193)
            return GameClientType.Vietnam193;

        if (version >= 180)
            return GameClientType.Vietnam;

        // Default to standard vSRO for legacy clients (e.g. 115, 120, etc.)
        return GameClientType.Vietnam;
    }

    /// <summary>
    ///     Checks whether the version is inside the range this detector understands.
    /// </summary>
    private static bool IsSupportedVersion(int version)
    {
        return version >= MinimumVersion && version <= MaximumVersion;
    }

    private static ClientVersionInfo Build(string directory, int version, string source)
    {
        return new ClientVersionInfo
        {
            Directory = directory,
            Version = version,
            ClientType = MapVersionToClientType(version) ?? GameClientType.Vietnam,
            Source = source
        };
    }

    private static ClientVersionInfo Unknown(string directory, string source)
    {
        return new ClientVersionInfo
        {
            Directory = directory,
            ClientType = GameClientType.Vietnam,
            Source = source
        };
    }

    /// <summary>
    ///     Enumerates files that may contain the client revision. Known names are checked first,
    ///     followed by any <c>*.rev</c>/<c>*.ver</c> file in the client root.
    /// </summary>
    private static IEnumerable<string> EnumerateVersionFiles(string directory)
    {
        var files = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var name in KnownVersionFiles)
        {
            var path = Path.Combine(directory, name);
            if (!File.Exists(path) || !seen.Add(path))
                continue;

            files.Add(path);
        }

        try
        {
            foreach (var pattern in new[] { "*.rev", "*.ver" })
                files.AddRange(Directory.GetFiles(directory, pattern)
                    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                    .Where(seen.Add));
        }
        catch
        {
            //Ignore unreadable directories, the known files above are enough.
        }

        return files;
    }

    /// <summary>
    ///     Tries to read the client version from a key/value style revision file.
    /// </summary>
    private static bool TryReadVersionFromFile(string path, out int version, out string key)
    {
        version = 0;
        key = string.Empty;

        try
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream, Encoding.UTF8, true);

            var content = reader.ReadToEnd();
            if (content.Length > 512 * 1024)
                content = content.Substring(0, 512 * 1024);

            foreach (Match match in KeyValueRegex.Matches(content))
            {
                var name = NormalizeKey(match.Groups["key"].Value);
                if (!IsVersionKey(name))
                    continue;

                if (!TryParseVersionValue(match.Groups["value"].Value, out version))
                    continue;

                key = name;

                return true;
            }
        }
        catch
        {
            //Unreadable revision file, try the next source.
        }

        return false;
    }

    private static string NormalizeKey(string value)
    {
        return (value ?? string.Empty).Trim().ToLowerInvariant().Replace(' ', '_').Replace('-', '_');
    }

    private static bool IsVersionKey(string key)
    {
        if (VersionKeys.Contains(key))
            return true;

        return key.EndsWith("_version", StringComparison.Ordinal) ||
               key.StartsWith("client_version", StringComparison.Ordinal);
    }

    /// <summary>
    ///     Parses a version value such as "1.194", "1,194,0,0", "194" or "version 194".
    /// </summary>
    private static bool TryParseVersionValue(string value, out int version)
    {
        version = 0;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        var pair = VersionPairRegex.Match(value);
        if (pair.Success &&
            int.TryParse(pair.Groups["minor"].Value, out var minor) &&
            IsSupportedVersion(minor))
        {
            version = minor;

            return true;
        }

        var plain = PlainVersionRegex.Match(value);
        if (plain.Success &&
            int.TryParse(plain.Groups["number"].Value, out var number) &&
            IsSupportedVersion(number))
        {
            version = number;

            return true;
        }

        return false;
    }

    /// <summary>
    ///     Finds the game client executable(s). The configured executable is preferred, then sro_client.exe,
    ///     then executables whose name looks like a Silkroad client.
    /// </summary>
    private static IEnumerable<string> FindClientExecutables(string directory)
    {
        var executables = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        void Add(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return;

            string fullPath;
            try
            {
                fullPath = Path.GetFullPath(path);
            }
            catch
            {
                return;
            }

            if (seen.Add(fullPath))
                executables.Add(fullPath);
        }

        var configured = GlobalConfig.Get<string>("RSBot.SilkroadExecutable");
        if (!string.IsNullOrWhiteSpace(configured))
            Add(Path.Combine(directory, Path.GetFileName(configured)));

        Add(Path.Combine(directory, "sro_client.exe"));

        try
        {
            foreach (var path in Directory.GetFiles(directory, "*.exe")
                         .Where(path =>
                         {
                             var name = Path.GetFileNameWithoutExtension(path);

                             return name.IndexOf("client", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                    name.IndexOf("silkroad", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                    name.StartsWith("sro", StringComparison.OrdinalIgnoreCase);
                         })
                         .OrderBy(path => Path.GetFileName(path).Length))
                Add(path);
        }
        catch
        {
            //Ignore unreadable directories.
        }

        return executables;
    }

    /// <summary>
    ///     Tries to read the version from the executable's version resource.
    /// </summary>
    private static bool TryReadExecutableVersion(string path, out int version)
    {
        version = 0;

        try
        {
            var info = FileVersionInfo.GetVersionInfo(path);

            if (info.FileMajorPart == 1 && IsSupportedVersion(info.FileMinorPart))
            {
                version = info.FileMinorPart;

                return true;
            }

            if (TryParseVersionValue(info.FileVersion, out version))
                return true;

            if (TryParseVersionValue(info.ProductVersion, out version))
                return true;
        }
        catch
        {
            //Some clients protect/strip the version resource.
        }

        return false;
    }

    /// <summary>
    ///     Scans the executable for an embedded version string such as "1.194". The scan is only
    ///     trusted when exactly one distinct version is found, otherwise it is too ambiguous.
    /// </summary>
    private static bool TryScanExecutableStrings(string path, out int version)
    {
        version = 0;

        try
        {
            var length = new FileInfo(path).Length;
            if (length <= 0 || length > 96L * 1024 * 1024)
                return false;

            var bytes = File.ReadAllBytes(path);

            var candidates = new HashSet<int>();

            foreach (var text in new[] { Encoding.ASCII.GetString(bytes), Encoding.Unicode.GetString(bytes) })
            {
                foreach (Match match in VersionPairRegex.Matches(text))
                {
                    if (int.TryParse(match.Groups["minor"].Value, out var minor) && IsSupportedVersion(minor))
                        candidates.Add(minor);
                }
            }

            if (candidates.Count != 1)
                return false;

            version = candidates.First();

            return true;
        }
        catch
        {
            return false;
        }
    }
}
