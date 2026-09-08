using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Plugins;
using RSBot.MobSelector.Views;

namespace RSBot.MobSelector;

public class Bootstrap : IPlugin
{
    public string InternalName => "SonicBot.MobSelector";

    public string DisplayName => "Mob Selector";

    public bool DisplayAsTab => true;

    public int Index => 12;

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
