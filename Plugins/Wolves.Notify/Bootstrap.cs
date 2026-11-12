using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Plugins;

namespace Wolves.Notify;

/// <summary>
///     Bootstrap entry point for the Telegram & Discord notifications plugin.
/// </summary>
public class Bootstrap : IPlugin
{
    public string InternalName => "Wolves.Notify";

    public string DisplayName => Kernel.Language != null && Kernel.Language.StartsWith("ar", System.StringComparison.OrdinalIgnoreCase)
        ? "تليجرام / ديسكورد"
        : "Telegram & Discord";

    public bool DisplayAsTab => true;

    public int Index => 18;

    public bool RequireIngame => false;

    public void Initialize()
    {
        NotifyMonitor.Initialize();
        Log.Notify("[Wolves.Notify] Telegram & Discord notification plugin initialized!");
    }

    public Control View => Views.Main.Instance;

    public void Translate()
    {
        Views.Main.Instance.Translate();
    }
}
