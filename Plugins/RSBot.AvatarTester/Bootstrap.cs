using System.Windows.Forms;
using RSBot.AvatarTester.Views;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Plugins;

namespace RSBot.AvatarTester;

public class Bootstrap : IPlugin
{
    public string InternalName => "SonicBot.AvatarTester";

    public string DisplayName => "Avatar Tester";

    public bool DisplayAsTab => true;

    public int Index => 10;

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
