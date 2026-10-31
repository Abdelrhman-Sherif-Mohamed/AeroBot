using System;
using System.Collections.Generic;
using System.Threading;
using RSBot.Core.Objects;

namespace RSBot.Core.Components.Scripting.Commands;

internal class MoveScriptCommand : IScriptCommand
{
    #region Properties

    /// <summary>
    ///     Gets the name.
    /// </summary>
    /// <value>
    ///     The name.
    /// </value>
    public string Name => "move";

    /// <summary>
    ///     Gets a value indicating whether this instance is running.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance is running; otherwise, <c>false</c>.
    /// </value>
    public bool IsBusy { get; private set; }

    /// <summary>
    ///     Gets the arguments.
    /// </summary>
    /// <value>
    ///     The arguments.
    /// </value>
    public Dictionary<string, string> Arguments => new()
    {
        { "XSector", "The X sector of the region" },
        { "YSector", "The Y sector of the region" },
        { "XOffset", "The X offset inside the region" },
        { "YOffset", "The Y offset inside the region" },
        { "ZOffset", "The Z offset inside the region" }
    };

    #endregion Properties

    #region Methods

    /// <summary>
    ///     Executes this instance.
    /// </summary>
    /// <param name="arguments"></param>
    /// <returns>
    ///     A value indicating if the command has been executed successfully.
    /// </returns>
    public bool Execute(string[] arguments = null)
    {
        if (arguments == null || arguments.Length != Arguments.Count)
        {
            Log.Warn("[Script] Invalid move command: Position information missing / invalid format.");

            return false;
        }

        if (IsBusy)
            return false;

        try
        {
            IsBusy = true;

            while (Game.Player.InAction)
            {
                if (!IsBusy || !ScriptManager.Running || ScriptManager.Paused || (Kernel.Bot != null && !Kernel.Bot.Running))
                    return false;
                Thread.Sleep(50);
            }

            const int retryAttempts = 2;
            var stepRetryCounter = 0;

            while (!ExecuteMove(arguments))
            {
                if (!IsBusy || !ScriptManager.Running || ScriptManager.Paused || (Kernel.Bot != null && !Kernel.Bot.Running))
                    return false;

                if (stepRetryCounter++ >= retryAttempts)
                {
                    // Proceed to maintain continuous march without stopping
                    return true;
                }

                Thread.Sleep(50);
            }

            return true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    ///     Executes the movement with smart collision detection and obstacle evasion.
    /// </summary>
    /// <param name="arguments">The arguments.</param>
    private bool ExecuteMove(IReadOnlyList<string> arguments)
    {
        if (!float.TryParse(arguments[0], out var xOffset)
            || !float.TryParse(arguments[1], out var yOffset)
            || !float.TryParse(arguments[2], out var zOffset)
            || !byte.TryParse(arguments[3], out var xSector)
            || !byte.TryParse(arguments[4], out var ySector))
        {
            IsBusy = false;

            return false; //Invalid format
        }

        Position pos = new(xSector, ySector, xOffset, yOffset, zOffset);

        if (PlayerConfig.Get("RSBot.Training.checkUseMount", true))
            if (!Game.Player.HasActiveVehicle && !Game.Player.IsInDungeon && !Game.Player.InAction)
                Game.Player.SummonVehicle();

        var distance = pos.DistanceTo(Game.Player.Position);
        if (distance > 120)
        {
            Log.Warn("[Script] Target position too far away, bot logic aborted!");

            IsBusy = false;
            return false;
        }

        Log.Debug($"[Script] Move to position {pos.Region}({pos.Region.X},{pos.Region.Y}) X={pos.X}, Y={pos.Y}");

        return MoveWithObstacleEvasion(pos);
    }

    /// <summary>
    ///     Moves towards target with continuous motion blending and smart obstacle evasion.
    /// </summary>
    private bool MoveWithObstacleEvasion(Position targetPos)
    {
        // Continuous motion threshold: advance to next waypoint early to avoid any stopping between steps
        var threshold = Game.Player.HasActiveVehicle ? 7.0f : 5.0f;
        if (targetPos.DistanceToPlayer() <= threshold)
            return true;

        var startDistance = targetPos.DistanceToPlayer();
        var speed = Game.Player.ActualSpeed > 0 ? Game.Player.ActualSpeed : 5.0f;
        var maxTimeoutMs = (int)(startDistance / speed * 1000) + 3500;
        var startTime = DateTime.UtcNow;

        // Send non-blocking movement packet to server immediately
        if (!Game.Player.MoveTo(targetPos, sleep: false))
            return false;

        var lastPos = Game.Player.Position;
        var stuckMs = 0;
        var evasionCount = 0;

        while ((DateTime.UtcNow - startTime).TotalMilliseconds < maxTimeoutMs)
        {
            if (!IsBusy || !ScriptManager.Running || ScriptManager.Paused || (Kernel.Bot != null && !Kernel.Bot.Running))
            {
                Game.Player?.MoveTo(Game.Player.Position, sleep: false);
                return false;
            }

            Thread.Sleep(80);

            var currentDist = targetPos.DistanceToPlayer();
            // Continuous motion: when within threshold, immediately proceed to next waypoint without deceleration!
            if (currentDist <= threshold)
                return true;

            var currentPos = Game.Player.Position;
            var progress = currentPos.DistanceTo(lastPos);

            if (progress < 0.45f)
            {
                stuckMs += 80;
            }
            else
            {
                stuckMs = 0;
                lastPos = currentPos;
            }

            // Stuck against a tree, rock, or barrier for >= 1000ms
            if (stuckMs >= 1000)
            {
                evasionCount++;
                Game.Player.PerformObstacleEvasion(targetPos, evasionCount);
                stuckMs = 0;
                lastPos = Game.Player.Position;
                startTime = DateTime.UtcNow; // Reset timeout for continued march
            }
        }

        // If reasonably close at timeout expiration, proceed smoothly to next waypoint
        return targetPos.DistanceToPlayer() <= (threshold + 4.0f);
    }

    public void Stop()
    {
        IsBusy = false;
    }

    #endregion Methods
}
