using System;
using System.Globalization;
using System.Reflection;
using System.Text;
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

    /// <summary>Boot silently to the system tray (used by SonicMini).</summary>
    public static bool StartMinimized;

    [STAThread]
    private static void Main(string[] args)
    {
        foreach (var arg in args)
        {
            if (arg.Equals("--tray", StringComparison.OrdinalIgnoreCase) ||
                arg.Equals("--mini", StringComparison.OrdinalIgnoreCase))
            {
                StartMinimized = true;
                continue;
            }

            if (ProfileManager.ProfileExists(arg))
            {
                ProfileManager.SetSelectedProfile(arg);
                ProfileManager.IsProfileLoadedByArgs = true;
                Log.Debug($"Selected profile by args: {arg}");
            }
        }

        //CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        // We need "." instead of "," while saving float numbers
        // Also client data is "." based float digit numbers
        CultureInfo.CurrentCulture = new CultureInfo("en-US");

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.Run(new SplashScreen());
    }
}
