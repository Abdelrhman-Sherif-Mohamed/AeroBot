using System;
using System.Linq;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Spawn;
using RSBot.Trade.Components;

namespace RSBot.Trade.Bundle;

internal class AttackBundle
{
    /// <summary>
    ///     Distance (in meters) below which the player stops walking and starts attacking.
    /// </summary>
    private const double EngageDistance = 3.0;

    /// <summary>
    ///     Extra distance (in meters) the bot may chase a thief before it gives
    ///     up and returns to the trade route.
    /// </summary>
    private const double ChaseLeash = 15.0;

    private static readonly TimeSpan MoveInterval = TimeSpan.FromMilliseconds(300);
    private static readonly TimeSpan SelectInterval = TimeSpan.FromMilliseconds(500);
    private static readonly TimeSpan DismountInterval = TimeSpan.FromMilliseconds(750);

    private static readonly TimeSpan DismountTimeout = TimeSpan.FromSeconds(6);
    private static readonly TimeSpan CombatTimeout = TimeSpan.FromSeconds(45);

    private DateTime _lastMove = DateTime.MinValue;
    private DateTime _lastSelect = DateTime.MinValue;
    private DateTime _lastDismount = DateTime.MinValue;
    private DateTime _dismountStartedAt = DateTime.MinValue;
    private DateTime _combatStartedAt = DateTime.MinValue;
    private uint _combatTargetUid;
    private uint _ignoredTargetUid;
    private DateTime _ignoreTargetUntil = DateTime.MinValue;
    private bool _pausedScript;

    /// <summary>
    ///     A value indicating if the bundle is busy attacking the selected target.
    /// </summary>
    public bool IsAttacking { get; private set; }

    /// <summary>
    ///     A value indicating if the bundle is busy and the execution of further commands should be paused.
    /// </summary>
    public bool Busy => IsAttacking;

    /// <summary>
    ///     Whether NPC thieves should be attacked. Smart combat mode enables the
    ///     basic protection behaviours and also enables both attack types.
    /// </summary>
    private static bool AttackThiefNpcs => TradeConfig.AttackThiefNpcs || TradeConfig.SmartThiefCombat;

    /// <summary>
    ///     Whether thief players should be attacked.
    /// </summary>
    private static bool AttackThiefPlayers => TradeConfig.AttackThiefPlayers || TradeConfig.SmartThiefCombat;

    /// <summary>
    ///     Initializes the bundle
    /// </summary>
    public void Initialize()
    {
        Reset(false);
    }

    /// <summary>
    ///     Starts the bundle
    /// </summary>
    public void Start()
    {
        Reset(false);
    }

    private static readonly ushort[] TownRegionIds =
    {
        22106,
        22617,
        22618,
        22874,
        23088, // Hotan
        23603,
        23687, // Samarkand
        25000, // Jangan
        26265, // Donwhang
        26959, // Constantinople
        27244
    };

    /// <summary>
    ///     Returns true if the entity is a "real" NPC (merchant, specialty trader,
    ///     guard, ...).
    ///     <para>
    ///         Important: <see cref="SpawnedMonster" /> derives from
    ///         <see cref="SpawnedNpc" />, so checking for <see cref="SpawnedNpc" />
    ///         would also match every monster (and therefore every thief NPC).
    ///     </para>
    /// </summary>
    public static bool IsRealNpc(SpawnedBionic bionic)
    {
        return bionic is SpawnedNpcNpc;
    }

    /// <summary>
    ///     Checks if an entity is strictly a Thief (either a Thief Player wearing a
    ///     job suit or a Thief NPC). Normal wild monsters will strictly return FALSE.
    /// </summary>
    public static bool IsThief(SpawnedBionic bionic)
    {
        if (bionic == null || bionic.State == null || bionic.State.LifeState != LifeState.Alive)
            return false;

        // 1. Thief player (player wearing the thief job suit / in thief job mode)
        if (bionic is SpawnedPlayer player)
            return IsThiefPlayer(player);

        // 2. Thief NPC (job monsters spawned for trade runs)
        if (bionic is SpawnedMonster monster)
        {
            var record = monster.Record;
            if (record == null)
                return false;

            // NPC_MOB_TIEF - the thief mobs of the job system
            if (record.TypeID4 == 2)
                return true;

            var code = record.CodeName;
            return !string.IsNullOrEmpty(code) &&
                   (code.Contains("TIEF", StringComparison.OrdinalIgnoreCase) ||
                    code.Contains("THIEF", StringComparison.OrdinalIgnoreCase));
        }

        return false;
    }

    /// <summary>
    ///     Checks if a spawned player is a thief.
    /// </summary>
    private static bool IsThiefPlayer(SpawnedPlayer player)
    {
        if (player == null || player.Job != JobType.Thief)
            return false;

        // Regular case: the thief wears the job suit and the server reported it.
        if (player.WearsJobSuite)
            return true;

        // Fallback for servers that do not send the job suit info correctly:
        // a thief-job player that is targeting us or our transport is hostile.
        var playerUniqueId = Game.Player?.UniqueId ?? 0;
        var transportUniqueId = Game.Player?.JobTransport?.UniqueId ?? 0;

        return player.TargetId != 0 &&
               (player.TargetId == playerUniqueId ||
                (transportUniqueId != 0 && player.TargetId == transportUniqueId));
    }

    /// <summary>
    ///     Checks if the player is currently inside a town safe zone where attacks are not permitted.
    /// </summary>
    public static bool IsPlayerInTown()
    {
        if (Game.Player == null)
            return true;

        if (Bundles.RouteBundle.TownscriptRunning)
            return true;

        if (ShoppingManager.Running)
            return true;

        // If player is in dialog with an NPC (merchant, specialty trader, smuggler)
        if (Game.Player.State?.DialogState is { IsInDialog: true })
            return true;

        // A selected real NPC means we are interacting with it, not fighting.
        if (IsRealNpc(Game.SelectedEntity))
            return true;

        var regionId = Game.Player.Position.Region.Id;
        if (TownRegionIds.Contains(regionId))
            return true;

        var xSec = Game.Player.Position.Region.X;
        var ySec = Game.Player.Position.Region.Y;

        // Jangan city sector
        if (xSec == 168 && ySec == 97)
            return true;

        // Constantinople city sector
        if (xSec == 79 && ySec == 105)
            return true;

        return false;
    }

    /// <summary>
    ///     Ticks this bundle. Strictly targets and attacks Thief NPCs and Thief
    ///     Players ONLY.
    /// </summary>
    public void Tick()
    {
        if (!TradeBotbase.IsActive)
        {
            Reset(false);
            return;
        }

        var player = Game.Player;
        if (player == null || player.State == null || player.State.LifeState != LifeState.Alive)
        {
            Reset(false);
            return;
        }

        // Never fight in a safe zone or while a scroll is being used.
        if (IsPlayerInTown() || player.State.ScrollState != ScrollState.Cancel)
        {
            Reset(true);
            return;
        }

        var target = Game.SelectedEntity;

        // A real NPC (merchant, trader, ...) must never be treated as a combat
        // target and must never be deselected here.
        if (IsRealNpc(target))
        {
            if (IsAttacking)
                Reset(false);

            return;
        }

        // Drop an outdated / invalid target (dead, behind an obstacle, not a
        // thief or disabled by the configuration). Also stop chasing a target
        // that dragged us too far away from the route.
        if (target != null &&
            (!IsSelectable(target) ||
             target.DistanceToPlayer > TradeConfig.MaxAttackDistance + ChaseLeash))
        {
            Log.Debug($"[Trade] Dropping invalid combat target [{target.UniqueId}]");

            target.TryDeselect();
            Game.SelectedEntity = null;
            target = null;
        }

        if (target == null)
            target = FindPotentialTarget();

        if (target == null)
        {
            if (IsAttacking)
                FinishCombat();

            return;
        }

        if (Game.SelectedEntity?.UniqueId != target.UniqueId)
        {
            // Throttle the select packets while the server has not answered yet.
            if (DateTime.UtcNow - _lastSelect < SelectInterval)
                return;

            _lastSelect = DateTime.UtcNow;

            if (!target.TrySelect())
            {
                Log.Debug($"[Trade] Could not select thief [{target.UniqueId}]");
                return;
            }
        }

        Engage(target);
    }

    /// <summary>
    ///     Engages the given thief: dismounts if needed, walks into range and
    ///     triggers the attack.
    /// </summary>
    private void Engage(SpawnedBionic target)
    {
        var player = Game.Player;
        if (player == null)
            return;

        // A single target must not keep the trade route paused forever (e.g.
        // attacks rejected by the server inside a safe zone, unreachable
        // target, ...).
        if (_combatTargetUid != target.UniqueId)
        {
            _combatTargetUid = target.UniqueId;
            _combatStartedAt = DateTime.UtcNow;
        }
        else if (DateTime.UtcNow - _combatStartedAt > CombatTimeout)
        {
            Log.Warn($"[Trade] Could not defeat the thief [{target.UniqueId}]. Ignoring it and continuing the route.");

            _ignoredTargetUid = target.UniqueId;
            _ignoreTargetUntil = DateTime.UtcNow.AddSeconds(20);
            _combatTargetUid = 0;
            _combatStartedAt = DateTime.MinValue;

            if (Game.SelectedEntity?.UniqueId == target.UniqueId)
            {
                Game.SelectedEntity.TryDeselect();
                Game.SelectedEntity = null;
            }

            FinishCombat();
            return;
        }

        // A mounted character can not attack at all.
        if (player.HasActiveVehicle)
        {
            if (!TradeConfig.DismountToAttackThief)
            {
                if (IsAttacking)
                    FinishCombat();

                return;
            }

            if (_dismountStartedAt == DateTime.MinValue)
                _dismountStartedAt = DateTime.UtcNow;

            // The server refused to dismount us (or we are stuck on the
            // transport). Give up instead of pausing the trade route forever.
            if (DateTime.UtcNow - _dismountStartedAt > DismountTimeout)
            {
                Log.Warn("[Trade] Could not dismount the transport to fight the thief. Continuing the route.");

                _dismountStartedAt = DateTime.MinValue;
                _ignoredTargetUid = target.UniqueId;
                _ignoreTargetUntil = DateTime.UtcNow.AddSeconds(15);

                if (Game.SelectedEntity?.UniqueId == target.UniqueId)
                {
                    Game.SelectedEntity.TryDeselect();
                    Game.SelectedEntity = null;
                }

                FinishCombat();

                return;
            }

            // Keep the route paused and the transport bundle from remounting
            // while we are getting off the transport.
            IsAttacking = true;
            EnsureScriptPaused();

            if (DateTime.UtcNow - _lastDismount >= DismountInterval)
            {
                _lastDismount = DateTime.UtcNow;
                Log.Notify("[Trade] Thief detected! Dismounting the transport to fight.");

                var vehicle = player.Vehicle;
                if (vehicle != null)
                    vehicle.Dismount();
                else
                    player.JobTransport?.Dismount();

                player.StopMoving();
            }

            return;
        }

        _dismountStartedAt = DateTime.MinValue;

        IsAttacking = true;
        EnsureScriptPaused();

        // Walk to the thief until it is in attack range.
        if (target.DistanceToPlayer > EngageDistance)
        {
            if (DateTime.UtcNow - _lastMove >= MoveInterval)
            {
                _lastMove = DateTime.UtcNow;
                player.MoveTo(target.Position, sleep: false);
            }

            return;
        }

        if (player.Movement.Moving)
            player.StopMoving();

        if (TradeConfig.CastBuffs)
            EventManager.FireEvent("Bundle.Buff.Invoke");

        EventManager.FireEvent("Bundle.Attack.Invoke");
    }

    /// <summary>
    ///     Returns a value indicating whether the given entity is a valid combat
    ///     target according to the current configuration.
    /// </summary>
    private static bool IsValidTarget(SpawnedBionic bionic)
    {
        if (bionic == null || bionic.State == null)
            return false;

        if (bionic.State.LifeState != LifeState.Alive)
            return false;

        if (bionic.IsBehindObstacle)
            return false;

        if (bionic is SpawnedPlayer)
            return AttackThiefPlayers && IsThiefPlayer((SpawnedPlayer)bionic);

        if (bionic is SpawnedMonster)
            return AttackThiefNpcs && IsThief(bionic);

        // Real NPCs and COS can never be combat targets.
        return false;
    }

    /// <summary>
    ///     Returns whether the entity can be picked as a target right now. This
    ///     is <see cref="IsValidTarget" /> plus the temporary blacklist that is
    ///     used when a thief could not be engaged (e.g. a failed dismount).
    /// </summary>
    private bool IsSelectable(SpawnedBionic bionic)
    {
        if (!IsValidTarget(bionic))
            return false;

        return _ignoredTargetUid == 0 ||
               bionic.UniqueId != _ignoredTargetUid ||
               DateTime.UtcNow >= _ignoreTargetUntil;
    }

    /// <summary>
    ///     Finds the next potential thief target according to trade rules.
    ///     STRICTLY targets ONLY Thief Players or Thief NPCs. Normal monsters are NEVER targeted.
    /// </summary>
    private SpawnedBionic FindPotentialTarget()
    {
        var player = Game.Player;
        if (player == null || IsPlayerInTown())
            return null;

        // Cannot attack while mounted and dismounting is disabled - do not
        // select a target we are unable to fight.
        if (player.HasActiveVehicle && !TradeConfig.DismountToAttackThief)
            return null;

        var protectTransport = TradeConfig.ProtectTransport || TradeConfig.SmartThiefCombat;
        var counterAttack = TradeConfig.CounterAttack || TradeConfig.SmartThiefCombat;

        // Priority 1: Protect transport (highest priority - only thieves attacking the vehicle)
        if (protectTransport && player.JobTransport != null)
        {
            var transport = player.JobTransport;

            if (SpawnManager.TryGetEntity<SpawnedBionic>(transport.UniqueId, out var transportBionic))
            {
                var attacker = transportBionic.GetAttackers()
                    .FirstOrDefault(a => a != null && a.UniqueId != transport.UniqueId && IsSelectable(a));

                if (attacker != null)
                    return attacker;
            }

            // Fallback: any thief mob nearby that is targeting our transport.
            if (AttackThiefNpcs && SpawnManager.TryGetEntity<SpawnedMonster>(
                    m => m != null && IsSelectable(m) && m.TargetId == transport.UniqueId && IsInRange(m),
                    out var mobAttackingTransport))
            {
                return mobAttackingTransport;
            }
        }

        // Priority 2: Fight back if the player was attacked directly.
        if (counterAttack)
        {
            var attacker = player.GetAttackers()
                .FirstOrDefault(a => a != null && IsSelectable(a));

            if (attacker != null)
                return attacker;

            if (AttackThiefNpcs && SpawnManager.TryGetEntity<SpawnedMonster>(
                    m => m != null && IsSelectable(m) && m.TargetId == player.UniqueId && IsInRange(m),
                    out var mobAttackingPlayer))
            {
                return mobAttackingPlayer;
            }

            if (AttackThiefPlayers && SpawnManager.TryGetEntity<SpawnedPlayer>(
                    p => p != null && IsSelectable(p) && p.TargetId == player.UniqueId && IsInRange(p),
                    out var playerAttackingPlayer))
            {
                return playerAttackingPlayer;
            }
        }

        // Priority 3: Closest thief NPC nearby.
        if (AttackThiefNpcs && SpawnManager.TryGetEntities<SpawnedMonster>(
                m => m != null && IsSelectable(m) && IsInRange(m), out var thiefMobs))
        {
            var thiefMob = thiefMobs.OrderBy(m => m.DistanceToPlayer).FirstOrDefault();
            if (thiefMob != null)
                return thiefMob;
        }

        // Priority 4: Closest thief player nearby.
        if (AttackThiefPlayers && SpawnManager.TryGetEntities<SpawnedPlayer>(
                p => p != null && IsSelectable(p) && IsInRange(p), out var thiefPlayers))
        {
            var thiefPlayer = thiefPlayers.OrderBy(p => p.DistanceToPlayer).FirstOrDefault();
            if (thiefPlayer != null)
                return thiefPlayer;
        }

        return null;
    }

    /// <summary>
    ///     Checks if the entity is within the configured attack distance, either
    ///     from the player or from the transport.
    /// </summary>
    private static bool IsInRange(SpawnedBionic bionic)
    {
        var maxDistance = TradeConfig.MaxAttackDistance;

        if (bionic.DistanceToPlayer <= maxDistance)
            return true;

        var transport = Game.Player?.JobTransport;
        return transport != null && bionic.Position.DistanceTo(transport.Position) <= maxDistance;
    }

    /// <summary>
    ///     Pauses the running route script while fighting.
    /// </summary>
    private void EnsureScriptPaused()
    {
        if (_pausedScript)
            return;

        if (ScriptManager.Running && !ScriptManager.Paused)
        {
            ScriptManager.Pause();
            _pausedScript = true;
        }
    }

    /// <summary>
    ///     Resumes the route script that was paused by this bundle.
    /// </summary>
    private void ResumeScript()
    {
        if (!_pausedScript)
            return;

        _pausedScript = false;

        if (ScriptManager.Paused)
            ScriptManager.ResumeScript();
    }

    /// <summary>
    ///     Called when no thief is left: resets the combat state, mounts the
    ///     transport again and resumes the trade route.
    /// </summary>
    private void FinishCombat()
    {
        IsAttacking = false;

        _combatTargetUid = 0;
        _combatStartedAt = DateTime.MinValue;

        Log.Notify("[Trade] All nearby thieves defeated!");

        var player = Game.Player;
        if (TradeConfig.MountTransport && player != null && !player.HasActiveVehicle &&
            player.JobTransport != null && !player.InAction &&
            player.JobTransport.Position.DistanceToPlayer() <= TradeConfig.MaxTransportDistance + 5)
        {
            player.JobTransport.Mount();
        }

        ResumeScript();

        Log.Notify("[Trade] Combat finished. Resuming trade route!");
    }

    /// <summary>
    ///     Resets the combat state without attacking anything.
    /// </summary>
    private void Reset(bool deselect)
    {
        var wasAttacking = IsAttacking;
        IsAttacking = false;

        _combatTargetUid = 0;
        _combatStartedAt = DateTime.MinValue;

        if (deselect && wasAttacking && Game.SelectedEntity != null && IsThief(Game.SelectedEntity))
        {
            Game.SelectedEntity.TryDeselect();
            Game.SelectedEntity = null;
        }

        ResumeScript();
    }

    /// <summary>
    ///     Stops the bundle.
    /// </summary>
    public void Stop()
    {
        IsAttacking = false;
        _pausedScript = false;
        _dismountStartedAt = DateTime.MinValue;
        _combatTargetUid = 0;
        _combatStartedAt = DateTime.MinValue;

        if (Game.SelectedEntity != null && IsThief(Game.SelectedEntity))
        {
            Game.SelectedEntity.TryDeselect();
            Game.SelectedEntity = null;
        }
    }
}
