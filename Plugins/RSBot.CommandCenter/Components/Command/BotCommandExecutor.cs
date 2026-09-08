using RSBot.Core;
using RSBot.Core.Components;

namespace RSBot.CommandCenter.Components.Command;

/// <summary>
///     !bot — toggles the on-demand in-game control panel.
/// </summary>
internal class BotCommandExecutor : RSBot.Core.Components.Command.ICommandExecutor
{
    public string CommandName => "bot";

    public string CommandDescription => "Show/hide the in-game panel (!bot)";

    public bool Execute(bool silent)
    {
        if (Game.Player == null || !Game.Ready)
        {
            if (!silent)
                Game.ShowNotification("[AeroBot] Enter the game first!");
            return false;
        }

        Components.InGamePanel.GamePanelManager.Toggle();
        return true;
    }
}
