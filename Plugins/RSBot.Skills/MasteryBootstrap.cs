using System.Windows.Forms;
using RSBot.Core.Plugins;
using RSBot.Skills.Views;

namespace RSBot.Skills;

public class MasteryBootstrap : IPlugin
{
    public string InternalName => "RSBot.Mastery";

    public string DisplayName => "Mastery";

    public bool DisplayAsTab => true;

    public int Index => 2;

    public bool RequireIngame => true;

    public void Initialize()
    {
    }

    public Control View => MasteriesTab.Instance;

    public void Translate()
    {
        MasteriesTab.Instance.ApplyLanguage();
    }
}
