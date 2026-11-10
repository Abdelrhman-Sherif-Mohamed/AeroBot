using System;
using System.Collections.Generic;
using System.Threading;
using RSBot.Core.Event;
using RSBot.Core.Network;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Spawn;
using Action = System.Action;

namespace RSBot.Core.Components.Scripting.Commands;

internal class TeleportScriptCommand : IScriptCommand
{
    #region Properties

    public string Name => "teleport";

    public bool IsBusy { get; private set; }

    public Dictionary<string, string> Arguments => new()
    {
        { "Codename", "The code name of the NPC" },
        { "Destination", "The id of the destination" }
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
        if (arguments == null || arguments.Length < 2)
        {
            Log.Warn("[Script] Invalid teleport command: Arguments missing (expected: teleport <npcCodeName> <destinationId>).");

            return false;
        }

        try
        {
            IsBusy = true;

            Log.Notify($"[Script] Initiating teleport via [{arguments[0]}] to destination [{arguments[1]}]...");
            return ExecuteTeleport(arguments);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    ///     Executes the teleport with approach, vehicle dismount, and completion verification.
    /// </summary>
    /// <param name="arguments">The arguments.</param>
    private static bool ExecuteTeleport(IReadOnlyList<string> arguments)
    {
        if (arguments.Count < 2)
            return false;

        var npcCodeName = arguments[0];
        if (!uint.TryParse(arguments[1], out var destination))
        {
            Log.Warn($"[Script] Invalid destination ID format: {arguments[1]}");
            return false;
        }

        // 1. Locate the NPC (wait up to 5s if still loading into range)
        SpawnedBionic entity = null;
        var findStart = DateTime.UtcNow;
        while ((DateTime.UtcNow - findStart).TotalSeconds < 5)
        {
            if (SpawnManager.TryGetEntity<SpawnedBionic>(p => p.Record != null && string.Equals(p.Record.CodeName, npcCodeName, StringComparison.OrdinalIgnoreCase), out entity))
                break;

            if (!ScriptManager.Running || ScriptManager.Paused || (Kernel.Bot != null && !Kernel.Bot.Running))
                return false;

            Thread.Sleep(200);
        }

        if (entity == null)
        {
            Log.Warn($"[Script] Could not find teleporter NPC [{npcCodeName}] nearby.");
            return false;
        }

        // 2. Approach the NPC if further than interaction range (3m)
        if (entity.Position.DistanceToPlayer() > 3.0f)
        {
            Log.Notify($"[Script] Approaching teleporter NPC [{npcCodeName}] (distance={Math.Round(entity.Position.DistanceToPlayer(), 1)}m)...");
            Game.Player?.MoveTo(entity.Position, sleep: false);
            var approachStart = DateTime.UtcNow;
            while (entity.Position.DistanceToPlayer() > 3.0f && (DateTime.UtcNow - approachStart).TotalSeconds < 10)
            {
                if (!ScriptManager.Running || ScriptManager.Paused || (Kernel.Bot != null && !Kernel.Bot.Running))
                    return false;

                Thread.Sleep(100);
            }
        }

        // 3. Dismount if mounted on transport or horse (Silkroad servers reject teleport if mounted!)
        if (Game.Player != null && Game.Player.HasActiveVehicle)
        {
            Log.Notify("[Script] Dismounting vehicle before teleportation...");
            Game.Player.Vehicle?.Dismount();
            var dismountStart = DateTime.UtcNow;
            while (Game.Player.HasActiveVehicle && (DateTime.UtcNow - dismountStart).TotalSeconds < 3)
            {
                Thread.Sleep(100);
            }
        }

        // 4. Select the NPC
        entity.TrySelect();
        Thread.Sleep(300);

        // 5. Track starting position and register teleport completion listener
        var startSectorX = Game.Player != null ? Game.Player.Position.Region.X : (byte)0;
        var startSectorY = Game.Player != null ? Game.Player.Position.Region.Y : (byte)0;
        Position? startPos = Game.Player != null ? Game.Player.Position : null;

        var teleportCompletedEvent = new ManualResetEventSlim(false);
        Action onTeleportComplete = () => teleportCompletedEvent.Set();
        EventManager.SubscribeEvent("OnTeleportComplete", onTeleportComplete);

        try
        {
            // 6. Send 0x705A teleport request packet with 0xB05A callback
            var packet = new Packet(0x705A);
            packet.WriteUInt(entity.UniqueId);
            packet.WriteByte(0x02); // TeleportType.NPC
            packet.WriteUInt(destination);

            var b05aCallback = new AwaitCallback(response =>
            {
                var result = response.ReadByte();
                if (result == 0x01)
                    return AwaitCallbackResult.Success;

                Log.Warn($"[Script] Teleport rejected by server (0xB05A result={result}).");
                return AwaitCallbackResult.Fail;
            }, 0xB05A);

            PacketManager.SendPacket(packet, PacketDestination.Server, b05aCallback);
            b05aCallback.AwaitResponse(3000);

            // 7. Wait for teleport completion (event fired, sector changed, or player moved across map)
            var waitStart = DateTime.UtcNow;
            var completed = false;

            while ((DateTime.UtcNow - waitStart).TotalSeconds < 25)
            {
                if (teleportCompletedEvent.IsSet)
                {
                    completed = true;
                    break;
                }

                if (Game.Player != null &&
                    (Game.Player.Position.Region.X != startSectorX ||
                     Game.Player.Position.Region.Y != startSectorY ||
                     (startPos.HasValue && Game.Player.Position.DistanceTo(startPos.Value) > 100)))
                {
                    completed = true;
                    break;
                }

                if (!ScriptManager.Running || ScriptManager.Paused || (Kernel.Bot != null && !Kernel.Bot.Running))
                    break;

                Thread.Sleep(100);
            }

            if (!completed)
            {
                Log.Warn($"[Script] Teleportation via [{npcCodeName}] (destination {destination}) timed out or failed!");
                return false;
            }

            Log.Notify($"[Script] Teleportation completed successfully to sector ({Game.Player?.Position.Region.X},{Game.Player?.Position.Region.Y})!");
            Thread.Sleep(2000);
            return true;
        }
        finally
        {
            EventManager.UnsubscribeEvent("OnTeleportComplete", onTeleportComplete);
            teleportCompletedEvent.Dispose();
        }
    }

    public void Stop()
    {
        IsBusy = false;
    }

    #endregion Methods
}