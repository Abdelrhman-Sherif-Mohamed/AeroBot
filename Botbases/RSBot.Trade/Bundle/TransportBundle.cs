using System;
using System.Linq;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Spawn;
using RSBot.Trade.Components;

namespace RSBot.Trade.Bundle;

internal class TransportBundle
{
    /// <summary>
    ///     A value indicating if the bot is waiting for transport.
    /// </summary>
    public bool WaitingForTransport { get; private set; }

    /// <summary>
    ///     A value indicating if the transport is stuck.
    /// </summary>
    public bool TransportStuck { get; private set; }


    /// <summary>
    ///     A value indicating if the bundle is currently busy and should block further command execution.
    /// </summary>
    public bool Busy => WaitingForTransport || TransportStuck;

    /// <summary>
    ///     Initializes the bot base
    /// </summary>
    public void Initialize()
    {
        SubscribeEvents();
    }

    private DateTime _lastTeleportTime = DateTime.MinValue;
    private bool _isTeleporting;
    private DateTime _lastMountAttempt = DateTime.MinValue;
    private DateTime _lastDismountAttempt = DateTime.MinValue;
    private DateTime _waitingStartTime = DateTime.MinValue;
    private float _lastDistanceWhenWaiting = float.MaxValue;
    private DateTime _lastTransportProgressTime = DateTime.MinValue;

    /// <summary>
    ///     Starts the bundle.
    /// </summary>
    public void Start()
    {
        WaitingForTransport = false;
        TransportStuck = false;
        _isTeleporting = false;
        _lastTeleportTime = DateTime.MinValue;
        _lastMountAttempt = DateTime.MinValue;
        _lastDismountAttempt = DateTime.MinValue;
        _waitingStartTime = DateTime.MinValue;
    }

    /// <summary>
    ///     Subscribes the events.
    /// </summary>
    private void SubscribeEvents()
    {
        EventManager.SubscribeEvent("OnJobCosStuck", new Action<byte>(OnJobCosStuck));
        EventManager.SubscribeEvent("OnTeleportStart", OnTeleportStart);
        EventManager.SubscribeEvent("OnTeleportComplete", OnTeleportComplete);
    }

    private void OnTeleportStart()
    {
        _isTeleporting = true;
        WaitingForTransport = false;
        TransportStuck = false;
    }

    private void OnTeleportComplete()
    {
        _isTeleporting = false;
        _lastTeleportTime = DateTime.UtcNow;
        WaitingForTransport = false;
        TransportStuck = false;
    }

    /// <summary>
    ///     Triggered when the server sends the cos stuck packet.
    /// </summary>
    /// <param name="reason"></param>
    private void OnJobCosStuck(byte reason)
    {
        if (TransportStuck)
            return;

        //ToDO: Better unstack mechanic for trade transports.
        Log.Warn("[Trade] Your transport is stuck! Go back to your transport and try to unstuck it.");
        Game.ShowNotification("[AeroBot] Your transport is stuck! Go back to your transport and try to unstuck it.");

        //TransportStuck = true;
    }

    public void Tick()
    {
        if (!Game.Ready || Game.Player == null)
            return;

        if (_isTeleporting || Game.Player.Teleportation?.IsTeleporting == true)
            return;

        // Grace period after teleport: allow 10 seconds for surrounding entities (including transport) to spawn
        if ((DateTime.UtcNow - _lastTeleportTime).TotalSeconds < 10)
            return;

        //Summon new transport?
        if (Game.Player.JobTransport == null)
        {
            if (Game.Player.State.BattleState == BattleState.InBattle || Game.Player.InAction)
                return;

            var jobTransportItem = Game.Player.Inventory
                .GetNormalPartItems(i => i.Record.CodeName.Contains("COS_T_") && i.Record.Tid == 4588)
                .FirstOrDefault();

            if (jobTransportItem != null)
            {
                Log.Notify($"[Trade] Summoning transport [{jobTransportItem.Record.GetRealName()}]");
                jobTransportItem.Use();

                return;
            }

            // DO NOT immediately stop the bot! The transport might be spawning or the player might be in dialog.
            return;
        }

        //Wait for certain things?
        if (!CheckDistanceToTransport() || !CheckTransportIsUnderAttack())
            return;

        if (TradeConfig.MountTransport)
        {
            if (!Game.Player.HasActiveVehicle &&
                Game.Player.State.BattleState == BattleState.InPeace && !Game.Player.InAction)
            {
                if ((DateTime.UtcNow - _lastMountAttempt).TotalSeconds < 4)
                {
                    WaitingForTransport = true;
                    return;
                }

                _lastMountAttempt = DateTime.UtcNow;
                Log.Notify("[Trade] Mounting transport");
                WaitingForTransport = true;

                var distToTransport = (float)Game.Player.JobTransport.Position.DistanceToPlayer();
                if (distToTransport > 3.0f)
                {
                    Game.Player.MoveTo(Game.Player.JobTransport.Position, false);
                }

                Game.Player.JobTransport?.Mount();
            }
        }
        else if (Game.Player.HasActiveVehicle &&
                 Game.Player.Vehicle.UniqueId == Game.Player.JobTransport.UniqueId)
        {
            if ((DateTime.UtcNow - _lastDismountAttempt).TotalSeconds >= 4)
            {
                _lastDismountAttempt = DateTime.UtcNow;
                Log.Notify("[Trade] Dismounting transport");
                Game.Player.JobTransport?.Dismount();
            }
        }
    }


    /// <summary>
    ///     Checks the vehicle distance to the player.
    /// </summary>
    private bool CheckDistanceToTransport()
    {
        if (_isTeleporting || (DateTime.UtcNow - _lastTeleportTime).TotalSeconds < 10)
            return true;

        if (Game.Player.JobTransport == null)
        {
            WaitingForTransport = true;
            Log.Debug("[Trade] Waiting for job transport to spawn.");
            return false;
        }

        // Player is mounted on the job transport
        if (Game.Player.HasActiveVehicle && Game.Player.Vehicle.UniqueId == Game.Player.JobTransport.UniqueId)
        {
            WaitingForTransport = false;
            TransportStuck = false;
            return true;
        }

        var currentDistance = (float)Game.Player.JobTransport.Position.DistanceToPlayer();

        // If currently waiting for transport to catch up:
        // Hysteresis: Keep waiting until transport is close (<= 6.0m) before resuming forward march
        if (WaitingForTransport)
        {
            if (currentDistance > 6.0f)
            {
                // Check if transport is making progress towards player
                if (currentDistance < _lastDistanceWhenWaiting - 0.5f)
                {
                    _lastDistanceWhenWaiting = currentDistance;
                    _lastTransportProgressTime = DateTime.UtcNow;
                }
                else
                {
                    // If transport has not gotten any closer for > 15 seconds, it might be genuinely stuck on geometry
                    if ((DateTime.UtcNow - _lastTransportProgressTime).TotalSeconds > 15 &&
                        (DateTime.UtcNow - _waitingStartTime).TotalSeconds > 15)
                    {
                        Log.Warn($"[Trade] Transport appears stuck behind ({currentDistance:F1}m). Walking back to assist.");
                        Game.Player.MoveTo(Game.Player.JobTransport.Position);
                        _lastTransportProgressTime = DateTime.UtcNow;
                        return false;
                    }
                }

                Log.Status($"Waiting for transport ({currentDistance:F1}m)");
                return false;
            }

            Log.Notify($"[Trade] Transport caught up ({currentDistance:F1}m). Resuming route!");
            WaitingForTransport = false;
            return true;
        }

        // Normal walk check: if player outpaced the transport
        if (currentDistance > TradeConfig.MaxTransportDistance)
        {
            WaitingForTransport = true;
            _waitingStartTime = DateTime.UtcNow;
            _lastDistanceWhenWaiting = currentDistance;
            _lastTransportProgressTime = DateTime.UtcNow;

            // Stop player where they stand so camel can catch up! NEVER run backward.
            Game.Player.StopMoving();

            Log.Status($"Waiting for transport to catch up ({currentDistance:F1}m)");
            return false;
        }

        WaitingForTransport = false;
        return true;
    }

    /// <summary>
    ///     Checks if the vehicle is under attack
    /// </summary>
    public bool CheckTransportIsUnderAttack()
    {
        if (!TradeConfig.ProtectTransport)
            return true;

        if (Game.Player.JobTransport == null)
            return true;

        if (!SpawnManager.TryGetEntity<SpawnedBionic>(Game.Player.JobTransport.UniqueId, out var bionic))
            return true;

        return bionic.GetAttackers().Count == 0;
    }

    /// <summary>
    ///     Stops this instance.
    /// </summary>
    public void Stop()
    {
        WaitingForTransport = false;
        TransportStuck = false;
    }
}