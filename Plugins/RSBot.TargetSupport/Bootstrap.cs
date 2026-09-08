using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Plugins;
using RSBot.TargetSupport.Views;

namespace RSBot.TargetSupport;

public class Bootstrap : IPlugin
{
    public string InternalName => "SonicBot.TargetSupport";

    public string DisplayName => "Target Support";

    public bool DisplayAsTab => true;

    public int Index => 8;

    public bool RequireIngame => true;

    public void Initialize()
    {
    }

    public Control View => Main.Instance;

    public void Translate()
    {
        LanguageManager.Translate(View, Kernel.Language);
    }
}
