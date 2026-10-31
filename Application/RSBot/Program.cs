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
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (s, e) =>
        {
            Log.Fatal(e.Exception);
        };
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            if (e.ExceptionObject is Exception ex)
                Log.Fatal(ex);
        };
        System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            Log.Fatal(e.Exception);
            e.SetObserved();
        };

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

        //CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        // We need "." instead of "," while saving float numbers
        // Also client data is "." based float digit numbers
        CultureInfo.CurrentCulture = new CultureInfo("en-US");

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        //Catch unhandled exceptions instead of silently terminating the application.
        //This keeps the bot alive (when possible) and writes the details into the log.
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (s, e) =>
        {
            try { Log.Fatal(e.Exception); } catch { }
            try { SessionLog.Write("Unhandled UI-thread exception", e.Exception); } catch { }
            try { MessageBox.Show(e.Exception.ToString(), "Unhandled exception", MessageBoxButtons.OK, MessageBoxIcon.Error); } catch { }
        };
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            try
            {
                var ex = e.ExceptionObject as Exception;
                if (ex != null) { Log.Fatal(ex); SessionLog.Write("Unhandled background-thread exception", ex); }
                else SessionLog.Write($"Unhandled non-exception object: {e.ExceptionObject}");
            }
            catch { }
        };
        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            try { Log.Fatal(e.Exception); SessionLog.Write("Unobserved task exception", e.Exception); e.SetObserved(); } catch { }
        };

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(true);
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.Run(new SplashScreen());
    }
}