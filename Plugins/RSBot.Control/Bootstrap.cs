using System.Windows.Forms;
using RSBot.Control.Views;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Plugins;

namespace RSBot.Control;

public class Bootstrap : IPlugin
{
    public string InternalName => "SonicBot.Control";

    public string DisplayName => "Control";

    public bool DisplayAsTab => true;

    public int Index => 9;

    public bool RequireIngame => true;

    public void Initialize()
    {
    }

    public System.Windows.Forms.Control View => Main.Instance;

    public void Translate()
    {
        LanguageManager.Translate(View, Kernel.Language);
    }
}
