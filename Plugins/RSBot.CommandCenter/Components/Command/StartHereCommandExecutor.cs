using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Components.Command;

namespace RSBot.CommandCenter.Components.Command;

internal class StartHereCommandExecutor : ICommandExecutor
{
    public string CommandName => "here";

    public string CommandDescription => "Set training area and start bot";

    public bool Execute(bool silent)
    {
        if (Game.Player == null || !Game.Ready)
        {
            SonicLog.Notify(silent, "[AeroBot] Enter the game first!", SonicLog.NeedGame);
            return false;
        }

        SonicLog.Notify(silent, "[AeroBot] Starting bot at the current location",
            "بدأ البوت في المكان الحالي وتم حفظه كمنطقة تدريب");

        return CommandManager.Execute("area", true) && CommandManager.Execute("start", true);
    }
}
