using System;
using System.Collections.Generic;
using System.Globalization;
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
        { "Position", "Supports 5 args (XOffset, YOffset, ZOffset, XSector, YSector) or 3 args (WorldX, WorldY, WorldZ)" }
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
        if (arguments == null || (arguments.Length != 3 && arguments.Length != 5))
        {
            Log.Warn("[Script] Invalid move command: Position information missing / invalid format (expected 3 or 5 arguments).");

            return false;
        }

        if (IsBusy)
            return false;

        try
        {
            IsBusy = true;

            while (Game.Player?.InAction == true)
            {
                if (ScriptManager.Paused)
                {
                    if (!ScriptManager.WaitWhilePaused(extraCondition: () => IsBusy))
                        return false;
                    continue;
                }

                if (!IsBusy || !ScriptManager.Running || (Kernel.Bot != null && !Kernel.Bot.Running))
                    return false;
                Thread.Sleep(50);
            }

            const int retryAttempts = 2;
            var stepRetryCounter = 0;
            Position? targetPos = null;

            while (!ExecuteMove(arguments, out targetPos))
            {
                if (ScriptManager.Paused)
                {
                    if (!ScriptManager.WaitWhilePaused(extraCondition: () => IsBusy))
                        return false;
                    continue;
                }

                if (!IsBusy || !ScriptManager.Running || (Kernel.Bot != null && !Kernel.Bot.Running))
                    return false;

                // If target could not be parsed, or player is far (> 35m), movement failure is fatal and cannot be skipped
                if (!targetPos.HasValue || targetPos.Value.DistanceToPlayer() > 35.0f)
                {
                    return false;
                }

                if (stepRetryCounter++ >= retryAttempts)
                {
                    // Minor waypoint within 35m can proceed to maintain continuous march
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
    /// <param name="pos">The resolved target position.</param>
    private bool ExecuteMove(IReadOnlyList<string> arguments, out Position? pos)
    {
        pos = null;

        if (arguments.Count == 3)
        {
            // vBot 3-argument syntax: move, WorldX, WorldY, WorldZ
            if (!float.TryParse(arguments[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var worldX)
                || !float.TryParse(arguments[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var worldY)
                || !float.TryParse(arguments[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var worldZ))
            {
                return false;
            }

            pos = Position.FromWorld(worldX, worldY, worldZ);
        }
        else if (arguments.Count >= 5)
        {
            // RSBot native 5-argument syntax: move XOffset YOffset ZOffset XSector YSector
            if (!float.TryParse(arguments[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var xOffset)
                || !float.TryParse(arguments[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var yOffset)
                || !float.TryParse(arguments[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var zOffset)
                || !byte.TryParse(arguments[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var xSector)
                || !byte.TryParse(arguments[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out var ySector))
            {
                return false;
            }

            pos = new Position(xSector, ySector, xOffset, yOffset, zOffset);
        }
        else
        {
            return false;
        }

        if (Game.Player == null)
            return false;

        // Avoid conflicting riding-pet summon in RSBot.Trade (TransportBundle owns job transport COS_T).
        var isTradeBot = Kernel.Bot?.Botbase?.Name == "RSBot.Trade";
        if (!isTradeBot && PlayerConfig.Get("RSBot.Training.checkUseMount", true))
            if (!Game.Player.HasActiveVehicle && !Game.Player.IsInDungeon && !Game.Player.InAction && Game.Player.JobTransport == null)
                Game.Player.SummonVehicle();

        var distance = pos.Value.DistanceTo(Game.Player.Position);
        if (distance > 350)
        {
            Log.Warn($"[Script] Target position too far away ({Math.Round(distance, 1)}m), bot logic aborted!");
            return false;
        }

        if (distance > 100)
        {
            Log.Debug($"[Script] Target position is at {Math.Round(distance, 1)}m, approaching in intermediate steps...");
            var steps = (int)Math.Ceiling(distance / 70.0);
            var startWorldX = Game.Player.Position.X;
            var startWorldY = Game.Player.Position.Y;
            var targetWorldX = pos.Value.X;
            var targetWorldY = pos.Value.Y;

            for (var s = 1; s < steps; s++)
            {
                if (ScriptManager.Paused)
                {
                    if (!ScriptManager.WaitWhilePaused(extraCondition: () => IsBusy))
                        return false;
                }

                if (!IsBusy || !ScriptManager.Running || (Kernel.Bot != null && !Kernel.Bot.Running))
                    return false;

                var ratio = (float)s / steps;
                var stepWorldX = startWorldX + (targetWorldX - startWorldX) * ratio;
                var stepWorldY = startWorldY + (targetWorldY - startWorldY) * ratio;

                var intermediatePos = Position.CreateWorldSafe(stepWorldX, stepWorldY, pos.Value.ZOffset, pos.Value.Region);
                if (!MoveWithObstacleEvasion(intermediatePos))
                    return false;
            }
        }

        Log.Debug($"[Script] Move to position {pos.Value.Region}({pos.Value.Region.X},{pos.Value.Region.Y}) X={pos.Value.X}, Y={pos.Value.Y}");

        return MoveWithObstacleEvasion(pos.Value);
    }

    /// <summary>
    ///     Moves towards target with continuous motion blending and smart obstacle evasion.
    /// </summary>
    private static float GetEffectiveSpeedMetersPerSecond()
    {
        var player = Game.Player;
        if (player == null)
            return 5.0f;

        var rawSpeed = player.HasActiveVehicle && player.Vehicle is { ActualSpeed: > 0 }
            ? player.Vehicle.ActualSpeed
            : player.ActualSpeed;

        // Silkroad entity speeds (WalkSpeed / RunSpeed) are stored in 10x meters/sec.
        return Math.Max(2.5f, rawSpeed * 0.1f);
    }

    private static int ComputeMoveTimeoutMs(double distanceMeters)
    {
        var speedMps = GetEffectiveSpeedMetersPerSecond();
        return (int)(distanceMeters / speedMps * 1000.0 * 1.6) + 6000;
    }

    private bool MoveWithObstacleEvasion(Position targetPos)
    {
        if (Game.Player == null)
            return false;

        // Continuous motion threshold: advance to next waypoint early to avoid any stopping between steps
        var threshold = Game.Player.HasActiveVehicle ? 7.0f : 5.0f;
        if (targetPos.DistanceToPlayer() <= threshold)
            return true;

        var startDistance = targetPos.DistanceToPlayer();
        var maxTimeoutMs = ComputeMoveTimeoutMs(startDistance);
        var startTime = DateTime.UtcNow;

        // Send non-blocking movement packet to server immediately
        if (!Game.Player.MoveTo(targetPos, sleep: false))
            return false;

        var anchorPos = Game.Player.Position;
        var lastProgressTime = DateTime.UtcNow;
        var evasionCount = 0;

        while ((DateTime.UtcNow - startTime).TotalMilliseconds < maxTimeoutMs)
        {
            if (ScriptManager.Paused)
            {
                if (Game.Player?.State?.BattleState != BattleState.InBattle)
                    Game.Player?.MoveTo(Game.Player.Position, sleep: false);

                if (!ScriptManager.WaitWhilePaused(extraCondition: () => IsBusy))
                    return false;

                if (targetPos.DistanceToPlayer() <= threshold)
                    return true;

                // If combat or a wait moved us beyond single-packet range, let ExecuteMove re-segment cleanly.
                if (targetPos.DistanceToPlayer() > 140.0f)
                    return false;

                maxTimeoutMs = ComputeMoveTimeoutMs(targetPos.DistanceToPlayer());
                startTime = DateTime.UtcNow;
                anchorPos = Game.Player.Position;
                lastProgressTime = DateTime.UtcNow;

                if (!Game.Player.MoveTo(targetPos, sleep: false))
                    return false;

                continue;
            }

            if (!IsBusy || !ScriptManager.Running || (Kernel.Bot != null && !Kernel.Bot.Running))
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
            // Measure net displacement across a rolling 1.5s window to avoid false obstacle triggers on slower transports.
            if (currentPos.DistanceTo(anchorPos) >= 1.8f)
            {
                anchorPos = currentPos;
                lastProgressTime = DateTime.UtcNow;
            }
            else if ((DateTime.UtcNow - lastProgressTime).TotalMilliseconds >= 1500)
            {
                evasionCount++;
                Game.Player.PerformObstacleEvasion(targetPos, evasionCount);
                anchorPos = Game.Player.Position;
                lastProgressTime = DateTime.UtcNow;
                startTime = DateTime.UtcNow;
                maxTimeoutMs = ComputeMoveTimeoutMs(targetPos.DistanceToPlayer());
            }
        }

        // If reasonably close at timeout expiration, proceed smoothly to next waypoint
        return targetPos.DistanceToPlayer() <= (threshold + 5.0f);
    }

    public void Stop()
    {
        IsBusy = false;
    }

    #endregion Methods
}
