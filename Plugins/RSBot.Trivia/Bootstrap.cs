using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Plugins;
using RSBot.Trivia.Views;

namespace RSBot.Trivia;

public class Bootstrap : IPlugin
{
    public string InternalName => "SonicBot.Trivia";

    public string DisplayName => "Trivia";

    public bool DisplayAsTab => true;

    public int Index => 11;

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
