using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Components.Scripting;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Spawn;

namespace RSBot.Quest.Components;

public class QuestScriptCommand : IScriptCommand
{
    #region Properties

    /// <summary>
    ///     Gets the command name.
    /// </summary>
    public string Name => "quest";

    /// <summary>
    ///     Gets a value indicating whether this instance is running.
    /// </summary>
    public bool IsBusy { get; private set; }

    /// <summary>
    ///     Gets the command arguments.
    /// </summary>
    public Dictionary<string, string> Arguments => new()
    {
        { "Codename", "The code name of the NPC (optional)" }
    };

    #endregion Properties

    #region Methods

    /// <summary>
    ///     Executes this instance.
    /// </summary>
    /// <param name="arguments">The arguments.</param>
    /// <returns>True if successfully executed, otherwise false.</returns>
    public bool Execute(string[] arguments = null)
    {
        if (!ScriptManager.Running)
        {
            IsBusy = false;
            return false;
        }

        try
        {
            IsBusy = true;
            string npcCode = null;

            if (arguments != null && arguments.Length > 0 && !string.IsNullOrWhiteSpace(arguments[0]))
            {
                npcCode = arguments[0].Trim();
            }
            else
            {
                // Try finding the nearest NPC
                if (SpawnManager.TryGetEntities<SpawnedNpcNpc>(out var npcs))
                {
                    var nearest = npcs.OrderBy(n => n.DistanceToPlayer).FirstOrDefault();
                    if (nearest?.Record != null)
                        npcCode = nearest.Record.CodeName;
                }
            }

            if (string.IsNullOrWhiteSpace(npcCode))
            {
                Log.Warn("[Script] Quest command: No nearby NPC found to interact with.");
                return false;
            }

            Log.Notify($"[Script] Interacting with NPC [{npcCode}] for quests...");
            ShoppingManager.ChooseTalkOption(npcCode, TalkOption.Quest);
            Thread.Sleep(500);

            return true;
        }
        catch (Exception ex)
        {
            Log.Error($"[Script] Quest command failed: {ex.Message}");
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    ///     Stops this instance.
    /// </summary>
    public void Stop()
    {
        IsBusy = false;
    }

    #endregion Methods
}
