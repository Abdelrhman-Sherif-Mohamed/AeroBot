using System.Windows.Forms;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Plugins;

namespace RSBot.Stall;

public class Bootstrap : IPlugin
{
    public string InternalName => "SonicBot.Stall";

    public string DisplayName => "Stall";

    public bool DisplayAsTab => true;

    public int Index => 7;

    public bool RequireIngame => true;

    public void Initialize()
    {
    }

    public Control View => Views.Main.Instance;

    public void Translate()
    {
        LanguageManager.Translate(View, Kernel.Language);
    }
}
