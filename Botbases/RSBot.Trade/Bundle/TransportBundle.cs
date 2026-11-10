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

    private DateTime _lastMountAttempt = DateTime.MinValue;
    private DateTime _lastTransportSummonAttempt = DateTime.MinValue;


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

    /// <summary>
    ///     Starts the bundle.
    /// </summary>
    public void Start()
    {
        WaitingForTransport = false;
        TransportStuck = false;
    }

    /// <summary>
    ///     Subscribes the events.
    /// </summary>
    private void SubscribeEvents()
    {
        EventManager.SubscribeEvent("OnJobCosStuck", new Action<byte>(OnJobCosStuck));
    }

    /// <summary>
    ///     Triggered when the server sends the cos stuck packet.
    /// </summary>
    /// <param name="reason"></param>
    private void OnJobCosStuck(byte reason)
    {
        if (TransportStuck)
            return;

        Log.Warn("[Trade] تم استلام إشعار تعليق وسيلة التجارة! جاري التراجع 5 خطوات والالتفاف لتخليصها...");
        if (Game.Player != null)
        {
            var target = Game.Player.Movement.HasDestination ? Game.Player.Movement.Destination : Game.Player.Position;
            System.Threading.Tasks.Task.Run(() =>
            {
                Game.Player.PerformObstacleEvasion(target, 1);
            });
        }
    }

    public void Tick()
    {
        //Summon new transport?
        if (Game.Player.JobTransport == null)
        {
            // Do not attempt to summon transport if dead or in combat/action
            if (Game.Player?.State?.LifeState != LifeState.Alive || Game.Player?.Health == 0 ||
                Game.Player?.State?.BattleState == BattleState.InBattle || Game.Player?.InAction == true)
                return;

            // Manual transport mode: the player is responsible for summoning
            // the transport. Keep the route paused until it appears.
            if (string.Equals(TradeConfig.TransportMode, "Manual", StringComparison.OrdinalIgnoreCase))
            {
                WaitingForTransport = true;
                Log.Status("Waiting for transport (manual mode)...");
                return;
            }

            if ((DateTime.UtcNow - _lastTransportSummonAttempt).TotalSeconds < 5.0)
                return;

            var jobTransportItem = Game.Player.Inventory
                .GetNormalPartItems(i => i.Record.CodeName.Contains("COS_T_") && i.Record.Tid == 4588)
                .FirstOrDefault();

            if (jobTransportItem != null)
            {
                _lastTransportSummonAttempt = DateTime.UtcNow;
                Log.Notify($"[Trade] Summoning transport [{jobTransportItem.Record.GetRealName()}]");
                jobTransportItem.Use();

                return;
            }

            Log.Warn("[Trade] Can not summon transport: No transport scroll in player inventory.");

            Kernel.Bot.Stop();

            return;
        }

        //Wait for certain things?
        if (!CheckDistanceToTransport() || !CheckTransportIsUnderAttack())
            return;

        if (TradeConfig.MountTransport && !Game.Player.HasActiveVehicle &&
            !Bundles.AttackBundle.IsAttacking && !Game.Player.InAction &&
            Game.Player.State?.BattleState != BattleState.InBattle)
        {
            // Do NOT freeze or pause the script! Player and transport march together on foot!
            // When the 20-second combat penalty expires, mounting will succeed while moving!
            if ((DateTime.UtcNow - _lastMountAttempt).TotalSeconds >= 4.0)
            {
                _lastMountAttempt = DateTime.UtcNow;
                Game.Player.JobTransport?.Mount();
            }
        }
        else if (!TradeConfig.MountTransport && Game.Player.HasActiveVehicle &&
                 Game.Player.Vehicle.UniqueId == Game.Player.JobTransport.UniqueId)
        {
            Game.Player.JobTransport?.Dismount();
        }
    }


    /// <summary>
    ///     Checks the vehicle distance to the player.
    /// </summary>
    /// s
    private bool CheckDistanceToTransport()
    {
        if (Game.Player.JobTransport == null)
        {
            WaitingForTransport = true;

            Log.Debug("[Trade] Waiting for job transport to spawn.");

            return false;
        }

        //Player is mounted 
        if (Game.Player.HasActiveVehicle && Game.Player.Vehicle.UniqueId == Game.Player.JobTransport.UniqueId)
        {
            WaitingForTransport = false;
            TransportStuck = false;

            return true;
        }

        var currentDistance = Game.Player.JobTransport.Position.DistanceToPlayer();
        if (currentDistance > TradeConfig.MaxTransportDistance)
        {
            WaitingForTransport = true;

            //In some rare cases, the position of the transport is wrong (e.g. Knock-Back skills)
            // That makes the bot thinking that the cos is actually very far away. To re-run the distance calculation
            // we can move the player to the location were it thinks the COS is located. That way the distance is then re-calculated 
            // and the script can continue.
            if (!Game.Player.JobTransport.Movement.HasDestination)
            {
                Log.Debug("[Trade] Lost track of the transport! Now trying to move to last known location.");
                Game.Player.MoveTo(Game.Player.JobTransport.Position);

                return false;
            }

            Log.Status($"Waiting for transport ({currentDistance:F1}m)");

            return false;
        }

        //ToDo: Check if job transport is stuck. Because the collision files are wrong at certain locations like Hotan
        //it can not be done until the collision has been fixed.
        WaitingForTransport = false;

        return true;
    }

    /// <summary>
    ///     Checks if the vehicle is under attack
    /// </summary>
    public bool CheckTransportIsUnderAttack()
    {
        if (!TradeConfig.ProtectTransport && !TradeConfig.SmartThiefCombat)
            return true;

        if (Game.Player?.JobTransport == null)
            return true;

        var transportId = Game.Player.JobTransport.UniqueId;
        if (transportId == 0)
            return true;

        if (!SpawnManager.TryGetEntities<SpawnedBionic>(e => e.TargetId == transportId, out var attackers) || attackers == null)
            return true;

        return !attackers.Any(AttackBundle.IsThief);
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