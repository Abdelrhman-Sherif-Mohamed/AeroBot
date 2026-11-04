using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace AeroBot.Updater;

internal static class Program
{
    private static void Main(string[] args)
    {
        if (args == null || args.Length < 4)
            return;

        if (!int.TryParse(args[0], out var parentPid))
            return;

        var zipPath = args[1];
        var targetDir = args[2];
        var exeToLaunch = args[3];

        try
        {
            // 1. Wait for parent process to exit
            try
            {
                var parent = Process.GetProcessById(parentPid);
                if (!parent.WaitForExit(15000))
                {
                    try { parent.Kill(); } catch { }
                    parent.WaitForExit(3000);
                }
            }
            catch { }

            Thread.Sleep(600);

            // 2. Extract update zip over the target directory
            if (File.Exists(zipPath) && Directory.Exists(targetDir))
            {
                using (var archive = ZipFile.OpenRead(zipPath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (string.IsNullOrEmpty(entry.Name))
                        {
                            var dirPath = Path.Combine(targetDir, entry.FullName);
                            if (!Directory.Exists(dirPath))
                                Directory.CreateDirectory(dirPath);
                            continue;
                        }

                        var destPath = Path.Combine(targetDir, entry.FullName);
                        var destDir = Path.GetDirectoryName(destPath);
                        if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                            Directory.CreateDirectory(destDir);

                        var fileName = Path.GetFileName(destPath);
                        if (fileName.Equals("AeroBot.Updater.exe", StringComparison.OrdinalIgnoreCase) ||
                            fileName.Equals("AeroBot.Updater.dll", StringComparison.OrdinalIgnoreCase) ||
                            fileName.Equals("Wolves.Updater.exe", StringComparison.OrdinalIgnoreCase) ||
                            fileName.Equals("Wolves.Updater.dll", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (File.Exists(destPath))
                        {
                            try
                            {
                                File.SetAttributes(destPath, FileAttributes.Normal);
                            }
                            catch { }
                        }

                        for (var retry = 0; retry < 10; retry++)
                        {
                            try
                            {
                                entry.ExtractToFile(destPath, overwrite: true);
                                break;
                            }
                            catch
                            {
                                Thread.Sleep(300);
                            }
                        }
                    }
                }

                // 2.5 Clean up legacy files and directories
                try
                {
                    string[] legacyFiles =
                    {
                        "RSBot.Core.dll", "RSBot.Core.pdb",
                        "RSBot.FileSystem.dll", "RSBot.FileSystem.pdb",
                        "RSBot.NavMeshApi.dll", "RSBot.NavMeshApi.pdb",
                        "Wolves.Updater.deps.json", "Wolves.Updater.dll",
                        "Wolves.Updater.exe", "Wolves.Updater.pdb",
                        "Wolves.Updater.runtimeconfig.json"
                    };

                    foreach (var lf in legacyFiles)
                    {
                        var p = Path.Combine(targetDir, lf);
                        if (File.Exists(p))
                        {
                            try
                            {
                                File.SetAttributes(p, FileAttributes.Normal);
                                File.Delete(p);
                            }
                            catch { }
                        }
                    }

                    string[] subDirs = { Path.Combine(targetDir, "Data", "Plugins"), Path.Combine(targetDir, "Data", "Bots") };
                    foreach (var dir in subDirs)
                    {
                        if (!Directory.Exists(dir)) continue;
                        foreach (var file in Directory.GetFiles(dir))
                        {
                            var name = Path.GetFileName(file);
                            if (name.StartsWith("RSBot.", StringComparison.OrdinalIgnoreCase) ||
                                name.StartsWith("Wolves.", StringComparison.OrdinalIgnoreCase) ||
                                name.StartsWith("SonicBot.", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    File.SetAttributes(file, FileAttributes.Normal);
                                    File.Delete(file);
                                }
                                catch { }
                            }
                        }
                    }

                    var langDir = Path.Combine(targetDir, "Data", "Languages");
                    if (Directory.Exists(langDir))
                    {
                        foreach (var dir in Directory.GetDirectories(langDir))
                        {
                            var dirName = Path.GetFileName(dir);
                            if (dirName.StartsWith("RSBot", StringComparison.OrdinalIgnoreCase) ||
                                dirName.StartsWith("Wolves", StringComparison.OrdinalIgnoreCase) ||
                                dirName.StartsWith("SonicBot", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    foreach (var f in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
                                    {
                                        try { File.SetAttributes(f, FileAttributes.Normal); } catch { }
                                    }
                                    Directory.Delete(dir, true);
                                }
                                catch { }
                            }
                        }
                    }
                }
                catch { }

                try
                {
                    File.Delete(zipPath);
                }
                catch { }
            }
        }
        catch { }
        finally
        {
            // 3. Relaunch the bot
            try
            {
                if (File.Exists(exeToLaunch))
                {
                    var psi = new ProcessStartInfo(exeToLaunch)
                    {
                        WorkingDirectory = targetDir,
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }
            }
            catch { }
        }
    }
}
