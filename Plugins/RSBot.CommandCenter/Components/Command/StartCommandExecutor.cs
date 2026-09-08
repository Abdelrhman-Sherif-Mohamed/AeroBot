using RSBot.Core;
using RSBot.Core.Components.Command;

namespace RSBot.CommandCenter.Components.Command;

internal class StartCommandExecutor : ICommandExecutor
{
    public string CommandName => "start";

    public string CommandDescription => "Start the bot";

    public bool Execute(bool silent)
    {
        if (Game.Player == null || !Game.Ready)
        {
            SonicLog.Notify(silent, "[AeroBot] Enter the game first!", SonicLog.NeedGame);
            return false;
        }

        SonicLog.Notify(silent, $"[AeroBot] Starting bot [{Kernel.Bot?.Botbase.DisplayName}]",
            $"البوت اشتغل (النظام: {Kernel.Bot?.Botbase.DisplayName})");

        Kernel.Bot?.Start();

        return Kernel.Bot?.Running == true;
    }
}
