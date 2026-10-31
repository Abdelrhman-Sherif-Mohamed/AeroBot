using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Plugins;
using Wolves.Fortress.Views;

namespace Wolves.Fortress;

public class Bootstrap : IPlugin
{
    public string InternalName => "Wolves.Fortress";

    public string DisplayName => "Fortress";

    public bool DisplayAsTab => true;

    public int Index => 7;

    public bool RequireIngame => true;

    public void Initialize()
    {
        Log.Notify("[Fortress] Fortress plugin initialized!");
    }

    public System.Windows.Forms.Control View => Main.Instance;

    public void Translate()
    {
        var view = Main.Instance;
        LanguageManager.Translate(view, Kernel.Language);
        view.ApplyFortressLocalization();
    }
}
