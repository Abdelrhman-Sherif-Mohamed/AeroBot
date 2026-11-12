using System;
using System.Linq;
using System.Threading;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Spawn;
using RSBot.Trade.Components;

namespace RSBot.Trade.Bundle;

internal class TransportBundle
{
    private const float MountRangeMeters = 6.0f;
    private const double MountCooldownSeconds = 3.5;
    private const double RemountTimeoutSeconds = 12.0;
    private const double BattleFallbackSeconds = 20.0;

    /// <summary>
    ///     A value indicating if the bot is waiting for transport.
    /// </summary>
    public bool WaitingForTransport { get; private set; }

    /// <summary>
    ///     A value indicating if the transport is stuck.
    /// </summary>
    public bool TransportStuck { get; private set; }

    /// <summary>
    ///     A value indicating if the bot is stopping and approaching the transport to mount.
    /// </summary>
    public bool IsRemounting { get; private set; }

    private DateTime _lastMountAttempt = DateTime.MinValue;
    private DateTime _lastTransportSummonAttempt = DateTime.MinValue;
    private DateTime _inBattleSince = DateTime.MinValue;
    private DateTime _remountStartedAt = DateTime.MinValue;
    private DateTime _lastRemountMoveAt = DateTime.MinValue;
    private bool _pausedScript;


    /// <summary>
    ///     A value indicating if the bundle is currently busy and should block further command execution.
    /// </summary>
    public bool Busy => WaitingForTransport || TransportStuck || IsRemounting;

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
        _inBattleSince = DateTime.MinValue;
        _lastMountAttempt = DateTime.MinValue;
        CancelRemount();
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
        var player = Game.Player;
        if (player == null)
            return;

        //Summon new transport?
        if (player.JobTransport == null)
        {
            CancelRemount();
            _inBattleSince = DateTime.MinValue;

            // Do not attempt to summon transport if dead or in combat/action
            if (player.State?.LifeState != LifeState.Alive || player.Health == 0 ||
                player.State?.BattleState == BattleState.InBattle || player.InAction)
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

            var jobTransportItem = player.Inventory
                .GetNormalPartItems(i => i?.Record != null &&
                                         i.Record.CodeName.Contains("COS_T_", StringComparison.OrdinalIgnoreCase) &&
                                         i.Record.TypeID1 == 3 && i.Record.TypeID2 == 3 && i.Record.TypeID3 == 3 && i.Record.TypeID4 == 2)
                .FirstOrDefault();

            if (jobTransportItem != null)
            {
                _lastTransportSummonAttempt = DateTime.UtcNow;
                Log.Notify($"[Trade] Summoning transport [{jobTransportItem.Record.GetRealName()}]");
                jobTransportItem.Use();

                return;
            }

            Log.Warn("[Trade] Can not summon transport: No transport scroll in player inventory.");

            TradeBotbase.StopRunner();

            return;
        }

        if (player.State?.LifeState != LifeState.Alive || player.Health == 0)
        {
            CancelRemount();
            WaitingForTransport = false;
            return;
        }

        if (Bundles.AttackBundle.IsAttacking)
        {
            CancelRemount();
            if (player.State?.BattleState == BattleState.InBattle)
                _inBattleSince = DateTime.UtcNow;

            return;
        }

        if (player.State?.BattleState == BattleState.InBattle)
        {
            if (_inBattleSince == DateTime.MinValue)
                _inBattleSince = DateTime.UtcNow;
        }
        else
        {
            _inBattleSince = DateTime.MinValue;
        }

        //Wait for certain things?
        if (!CheckDistanceToTransport() || !CheckTransportIsUnderAttack())
        {
            CancelRemount();
            return;
        }

        if (!TradeConfig.MountTransport)
        {
            CancelRemount();
            if (player.HasActiveVehicle && player.Vehicle?.UniqueId == player.JobTransport.UniqueId)
                player.JobTransport?.Dismount();

            return;
        }

        if (player.HasActiveVehicle)
        {
            CancelRemount();
            return;
        }

        TryRemountTransport();
    }

    private void TryRemountTransport()
    {
        var player = Game.Player;
        var transport = player?.JobTransport;
        if (player == null || transport == null || ShoppingManager.Running || player.InAction)
        {
            CancelRemount();
            return;
        }

        var inBattle = player.State?.BattleState == BattleState.InBattle;
        if (inBattle)
        {
            if (_inBattleSince == DateTime.MinValue)
                _inBattleSince = DateTime.UtcNow;

            var combatEndedAt = Bundles.AttackBundle.LastCombatEndedAt;
            var battleAnchor = combatEndedAt > _inBattleSince ? combatEndedAt : _inBattleSince;
            if ((DateTime.UtcNow - battleAnchor).TotalSeconds < BattleFallbackSeconds)
            {
                CancelRemount();
                return;
            }
        }

        if (!IsRemounting)
        {
            if ((DateTime.UtcNow - _lastMountAttempt).TotalSeconds < MountCooldownSeconds)
                return;

            IsRemounting = true;
            _remountStartedAt = DateTime.UtcNow;
            _lastRemountMoveAt = DateTime.MinValue;

            if (ScriptManager.Running && !ScriptManager.Paused)
            {
                ScriptManager.Pause();
                _pausedScript = true;
            }
        }

        if ((DateTime.UtcNow - _remountStartedAt).TotalSeconds >= RemountTimeoutSeconds)
        {
            _lastMountAttempt = DateTime.UtcNow;
            CancelRemount();
            return;
        }

        var distance = transport.Position.DistanceToPlayer();
        if (distance > MountRangeMeters)
        {
            if ((DateTime.UtcNow - _lastRemountMoveAt).TotalMilliseconds >= 350)
            {
                _lastRemountMoveAt = DateTime.UtcNow;
                player.MoveTo(transport.Position, sleep: false);
            }

            Log.Status($"[Trade] الاقتراب لركوب وسيلة النقل ({distance:F1}م)...");
            return;
        }

        // Silkroad servers reject 0x70CB (Mount) while moving or beyond interact range.
        if (player.Movement.Moving || player.Movement.HasDestination)
        {
            player.MoveTo(player.Position, sleep: false);
            player.StopMoving();
            Thread.Sleep(120);
        }

        transport.Mount(1500);

        var mounted = player.HasActiveVehicle && player.Vehicle?.UniqueId == transport.UniqueId;
        _lastMountAttempt = DateTime.UtcNow;
        CancelRemount();

        if (mounted)
        {
            if (player.State != null && player.State.BattleState == BattleState.InBattle)
                player.State.BattleState = BattleState.InPeace;

            _inBattleSince = DateTime.MinValue;
            Log.Notify("[Trade] تم امتطاء وسيلة النقل بنجاح!");
        }
    }

    private void CancelRemount()
    {
        IsRemounting = false;
        _remountStartedAt = DateTime.MinValue;
        _lastRemountMoveAt = DateTime.MinValue;

        if (_pausedScript)
        {
            _pausedScript = false;
            if (!Bundles.AttackBundle.IsAttacking && !WaitingForTransport && !TransportStuck &&
                ScriptManager.Running && ScriptManager.Paused)
            {
                ScriptManager.ResumeScript();
            }
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

        if (!SpawnManager.TryGetEntities<SpawnedBionic>(
                e => e.TargetId == transportId && e.State?.LifeState == LifeState.Alive, out var attackers) ||
            attackers == null)
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
        _inBattleSince = DateTime.MinValue;
        CancelRemount();
    }
}