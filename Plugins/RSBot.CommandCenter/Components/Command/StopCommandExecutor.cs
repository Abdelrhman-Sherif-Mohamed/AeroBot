using RSBot.Core;
using RSBot.Core.Components.Command;

namespace RSBot.CommandCenter.Components.Command;

internal class StopCommandExecutor : ICommandExecutor
{
    public string CommandName => "stop";

    public string CommandDescription => "Stop the bot";

    public bool Execute(bool silent)
    {
        SonicLog.Notify(silent, $"[Sonic] Stopping bot [{Kernel.Bot?.Botbase.DisplayName}]",
            "البوت وقف (تقدر تشغله تاني بـ !start)");

        Kernel.Bot?.Stop();

        return Kernel.Bot?.Running == false;
    }
}
