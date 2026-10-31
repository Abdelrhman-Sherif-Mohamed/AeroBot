using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Plugins;

namespace Wolves.Players;

public class Bootstrap : IPlugin
{
    public string InternalName => "Wolves.Players";

    public string DisplayName => "Players";

    public bool DisplayAsTab => true;

    public int Index => 11;

    public bool RequireIngame => true;

    public void Initialize()
    {
        PlayerTraceManager.Instance.Initialize();
        Log.Notify("[Wolves.Players] Players inspection plugin initialized!");
    }

    public Control View => Views.Main.Instance;

    public void Translate()
    {
        LanguageManager.Translate(View, Kernel.Language);
    }
}
