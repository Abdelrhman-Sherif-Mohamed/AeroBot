using RSBot.Core;
using RSBot.Core.Components.Command;

namespace RSBot.CommandCenter.Components.Command;

internal class TownCommandExecutor : ICommandExecutor
{
    public string CommandName => "town";

    public string CommandDescription => "Use return scroll and go to town";

    public bool Execute(bool silent)
    {
        if (Game.Player == null || !Game.Ready)
        {
            SonicLog.Notify(silent, "[Sonic] Enter the game first!", SonicLog.NeedGame);
            return false;
        }

        Kernel.Bot?.Stop();

        if (Game.Player.UseReturnScroll())
        {
            SonicLog.Notify(silent, "[Sonic] Returning to town...",
                "استخدم سكرول الرجوع وراجع المدينة (البوت وقف، شغله تاني بـ !here لما توصل)");
            return true;
        }

        SonicLog.Notify(silent, "[Sonic] No return scroll found!",
            "فشل الرجوع للمدينة: مفيش سكرول رجوع مناسب في الشنطة (المشكلة: شنطة الشخصية، الحل: اشتري Return Scroll على قد لفلك وحطه في الشنطة)");
        return false;
    }
}
