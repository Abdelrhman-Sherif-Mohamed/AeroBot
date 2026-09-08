using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Components.Command;
using RSBot.Core.Event;

namespace RSBot.CommandCenter.Components.Command;

internal class AreaCommandExecutor : ICommandExecutor
{
    public string CommandName => "area";

    public string CommandDescription => "Set the training area";

    public bool Execute(bool silent)
    {
        if (Game.Player == null || !Game.Ready)
        {
            SonicLog.Notify(silent, "[AeroBot] Enter the game first!", SonicLog.NeedGame);
            return false;
        }

        var radius = PlayerConfig.Get("RSBot.Area.Radius", 50);

        if (!silent)
            Game.ShowNotification(
                $"[AeroBot] Setting training area to X={Game.Player.Position.X:0.00} Y={Game.Player.Position.Y:0.00} R={radius}");

        PlayerConfig.Set("RSBot.Area.Region", Game.Player.Position.Region);
        PlayerConfig.Set("RSBot.Area.X", Game.Player.Position.XOffset.ToString("0.0"));
        PlayerConfig.Set("RSBot.Area.Y", Game.Player.Position.YOffset.ToString("0.0"));
        PlayerConfig.Set("RSBot.Area.Z", Game.Player.Position.ZOffset.ToString("0.0"));
        PlayerConfig.Get("RSBot.Area.Radius", 50);

        EventManager.FireEvent("OnSetTrainingArea");

        Log.Notify($"[AeroBot] تم حفظ منطقة التدريب: X={Game.Player.Position.X:0} Y={Game.Player.Position.Y:0} R={radius}");

        return true;
    }
}
