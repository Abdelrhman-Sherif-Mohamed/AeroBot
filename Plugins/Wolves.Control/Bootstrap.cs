using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Plugins;
using Wolves.Control.Views;

namespace Wolves.Control;

public class Bootstrap : IPlugin
{
    public string InternalName => "Wolves.Control";

    public string DisplayName => "Control";

    public bool DisplayAsTab => true;

    public int Index => 8;

    public bool RequireIngame => true;

    public void Initialize()
    {
        // Mirror the leader's target continuously (players included).
        RSBot.Core.Event.EventManager.SubscribeEvent("OnTick", TargetSupportManager.Instance.Tick);

        Log.Notify("[Wolves.Control] Control and Target Support plugin initialized!");
    }

    public System.Windows.Forms.Control View => Main.Instance;

    public void Translate()
    {
        LanguageManager.Translate(View, Kernel.Language);
        (View as Main)?.ApplyLanguage();
    }
}
