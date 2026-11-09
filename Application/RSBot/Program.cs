using System;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Views;

namespace RSBot;

internal static class Program
{
    public static string AssemblyTitle =
        Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyProductAttribute>()?.Product;

    public static string AssemblyVersion =
        $"v{Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version}";

    public static string AssemblyDescription =
        Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;

    [STAThread]
    private static void Main(string[] args)
    {
        CleanupLegacyFiles();

        if (args.Length == 1)
        {
            var profile = args[0];
            if (ProfileManager.ProfileExists(profile))
            {
                ProfileManager.SetSelectedProfile(profile);
                ProfileManager.IsProfileLoadedByArgs = true;
                Log.Debug($"Selected profile by args: {profile}");
            }
        }

        // We need "." instead of "," while saving float numbers
        // Also client data is "." based float digit numbers
        CultureInfo.CurrentCulture = new CultureInfo("en-US");

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // Catch unhandled exceptions gracefully instead of crashing
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (s, e) =>
        {
            try { Log.Fatal(e.Exception); } catch { }
            try { SessionLog.Write("Unhandled UI-thread exception", e.Exception); } catch { }
        };
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            try
            {
                if (e.ExceptionObject is Exception ex)
                {
                    Log.Fatal(ex);
                    SessionLog.Write("Unhandled background-thread exception", ex);
                }
                else
                {
                    SessionLog.Write($"Unhandled non-exception object: {e.ExceptionObject}");
                }
            }
            catch { }
        };
        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            try
            {
                Log.Fatal(e.Exception);
                SessionLog.Write("Unobserved task exception", e.Exception);
                e.SetObserved();
            }
            catch { }
        };

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(true);
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.Run(new SplashScreen());
    }

    private static void CleanupLegacyFiles()
    {
        try
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // 1. Root files
            string[] legacyRootFiles =
            {
                "RSBot.Core.dll", "RSBot.Core.pdb",
                "RSBot.FileSystem.dll", "RSBot.FileSystem.pdb",
                "RSBot.NavMeshApi.dll", "RSBot.NavMeshApi.pdb",
                "Wolves.Updater.deps.json", "Wolves.Updater.dll",
                "Wolves.Updater.exe", "Wolves.Updater.pdb",
                "Wolves.Updater.runtimeconfig.json"
            };

            foreach (var file in legacyRootFiles)
            {
                var p = System.IO.Path.Combine(baseDir, file);
                if (System.IO.File.Exists(p))
                {
                    try
                    {
                        System.IO.File.SetAttributes(p, System.IO.FileAttributes.Normal);
                        System.IO.File.Delete(p);
                    }
                    catch { }
                }
            }

            // 2. Data/Plugins & Data/Bots
            string[] subDirs = { System.IO.Path.Combine(baseDir, "Data", "Plugins"), System.IO.Path.Combine(baseDir, "Data", "Bots") };
            foreach (var dir in subDirs)
            {
                if (!System.IO.Directory.Exists(dir)) continue;
                foreach (var file in System.IO.Directory.GetFiles(dir))
                {
                    var name = System.IO.Path.GetFileName(file);
                    if (name.StartsWith("RSBot.", StringComparison.OrdinalIgnoreCase) ||
                        name.StartsWith("Wolves.", StringComparison.OrdinalIgnoreCase) ||
                        name.StartsWith("SonicBot.", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            System.IO.File.SetAttributes(file, System.IO.FileAttributes.Normal);
                            System.IO.File.Delete(file);
                        }
                        catch { }
                    }
                }
            }

            // 3. Data/Languages
            var langDir = System.IO.Path.Combine(baseDir, "Data", "Languages");
            if (System.IO.Directory.Exists(langDir))
            {
                foreach (var dir in System.IO.Directory.GetDirectories(langDir))
                {
                    var dirName = System.IO.Path.GetFileName(dir);
                    if (dirName.StartsWith("RSBot", StringComparison.OrdinalIgnoreCase) ||
                        dirName.StartsWith("Wolves", StringComparison.OrdinalIgnoreCase) ||
                        dirName.StartsWith("SonicBot", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            foreach (var f in System.IO.Directory.GetFiles(dir, "*", System.IO.SearchOption.AllDirectories))
                            {
                                try { System.IO.File.SetAttributes(f, System.IO.FileAttributes.Normal); } catch { }
                            }
                            System.IO.Directory.Delete(dir, true);
                        }
                        catch { }
                    }
                }
            }
        }
        catch { }
    }

}