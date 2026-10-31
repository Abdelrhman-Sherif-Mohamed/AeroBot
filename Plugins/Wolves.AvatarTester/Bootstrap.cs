using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Plugins;
using Wolves.AvatarTester.Views;

namespace Wolves.AvatarTester;

public class Bootstrap : IPlugin
{
    public string InternalName => "Wolves.AvatarTester";

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
        (View as Main)?.ApplyLanguage();
    }
}
