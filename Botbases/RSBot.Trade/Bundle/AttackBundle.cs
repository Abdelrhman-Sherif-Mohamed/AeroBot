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
    ///     A value indicating if the bundle is busy attacking the selected target.
    /// </summary>
    public bool IsAttacking { get; private set; }

    /// <summary>
    ///     A value indicating if the bundle is busy and the execution of further commands should be paused.
    /// </summary>
    public bool Busy => IsAttacking;

    /// <summary>
    ///     Initializes the bundle
    /// </summary>
    public void Initialize()
    {
        IsAttacking = false;
    }

    /// <summary>
    ///     Starts the bundle
    /// </summary>
    public void Start()
    {
        IsAttacking = false;
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
    ///     Checks if an entity is strictly a Thief (either a Thief Player wearing Job suite or a Thief NPC).
    ///     Normal wild monsters will strictly return FALSE.
    /// </summary>
    public static bool IsThief(SpawnedBionic bionic)
    {
        if (bionic == null || bionic.State.LifeState != LifeState.Alive)
            return false;

        // 1. Thief Player (player wearing thief job suite)
        if (bionic is SpawnedPlayer player)
        {
            return player.WearsJobSuite && player.Job == JobType.Thief;
        }

        // 2. Thief NPC (Monster spawned for trade runs: TypeID4 == 2 or thief codenames)
        if (bionic is SpawnedMonster monster && monster.Record != null)
        {
            if (monster.Record.TypeID4 == 2)
                return true;

            var code = monster.Record.CodeName;
            if (!string.IsNullOrEmpty(code))
            {
                if (code.Contains("THIEF", StringComparison.OrdinalIgnoreCase) ||
                    code.Contains("BANDIT", StringComparison.OrdinalIgnoreCase) ||
                    code.Contains("BRIGAND", StringComparison.OrdinalIgnoreCase) ||
                    code.Contains("SMUGGLER", StringComparison.OrdinalIgnoreCase) ||
                    code.Contains("ROBBER", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
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

        // If player is in dialog with an NPC or has an NPC selected (merchant, specialty trader, smuggler)
        if (Game.Player.State?.DialogState is { IsInDialog: true })
            return true;

        if (Game.SelectedEntity is SpawnedNpc)
            return true;

        var regionId = Game.Player.Position.Region.Id;
        if (TownRegionIds.Contains(regionId))
            return true;

        var xSec = Game.Player.Position.Region.X;
        var ySec = Game.Player.Position.Region.Y;

        // Jangan city sector
        if (xSec == 168 && ySec == 97)
            return true;

        // Donwhang city sector
        if (xSec == 167 && ySec == 96)
            return true;

        // Hotan city sector
        if (xSec == 163 && ySec == 93)
            return true;

        // Samarkand city sector
        if (xSec == 165 && ySec == 99)
            return true;

        // Constantinople city sectors
        if ((xSec <= 162 && ySec >= 100) || (xSec == 79 && ySec == 105))
            return true;

        return false;
    }

    /// <summary>
    ///     Ticks this bundle.
    ///     Strictly targets and attacks Thief NPCs and Thief Players ONLY.
    /// </summary>
    public void Tick()
    {
        if (!TradeBotbase.IsActive || IsPlayerInTown())
        {
            if (IsAttacking)
            {
                IsAttacking = false;
                if (Game.SelectedEntity != null && IsThief(Game.SelectedEntity))
                {
                    Game.SelectedEntity.TryDeselect();
                }
            }
            return;
        }

        bool smartCombat = TradeConfig.SmartThiefCombat;
        bool attackThiefPlayers = TradeConfig.AttackThiefPlayers || smartCombat;
        bool attackThiefNpcs = TradeConfig.AttackThiefNpcs || smartCombat;

        // Check if we have an active target
        var target = Game.SelectedEntity;

        // If an NPC is currently selected (talking, trading, shopping), never deselect it or treat it as combat target!
        if (target is SpawnedNpc)
            return;

        if (target is { State.LifeState: LifeState.Dead } or { IsBehindObstacle: true } || (target != null && !IsThief(target)))
        {
            if (target is not SpawnedNpc)
                target?.TryDeselect();
            target = null;
        }

        if (target != null && IsThief(target))
        {
            // If target is player but player attack is disabled, skip
            if (target is SpawnedPlayer && !attackThiefPlayers)
            {
                target.TryDeselect();
                target = null;
            }
            // If target is NPC but NPC attack is disabled, skip
            else if (target is SpawnedMonster && !attackThiefNpcs)
            {
                target.TryDeselect();
                target = null;
            }
            else
            {
                // If we have an active thief target but are mounted, dismount to attack if configured!
                if (TradeConfig.DismountToAttackThief && Game.Player.HasActiveVehicle)
                {
                    Log.Notify("[Trade] تم رصد لص! الترجل من وسيلة النقل لبدء القتال.");
                    Game.Player.JobTransport?.Dismount();
                    Game.Player.StopMoving();
                    if (ScriptManager.Running && !ScriptManager.Paused)
                        ScriptManager.Pause();
                }

                IsAttacking = true;

                if (TradeConfig.CastBuffs)
                    EventManager.FireEvent("Bundle.Buff.Invoke");

                EventManager.FireEvent("Bundle.Attack.Invoke");
                return;
            }
        }

        // Target is null or invalid: search for potential thief target
        var potentialTarget = FindPotentialTarget();
        if (potentialTarget != null)
        {
            if (TradeConfig.DismountToAttackThief && Game.Player.HasActiveVehicle)
            {
                Log.Notify("[Trade] تم رصد لص قريب! الترجل من وسيلة النقل للمهاجمة.");
                Game.Player.JobTransport?.Dismount();
                Game.Player.StopMoving();
                if (ScriptManager.Running && !ScriptManager.Paused)
                    ScriptManager.Pause();
            }

            potentialTarget.TrySelect();
            IsAttacking = true;

            if (TradeConfig.CastBuffs)
                EventManager.FireEvent("Bundle.Buff.Invoke");

            EventManager.FireEvent("Bundle.Attack.Invoke");
            return;
        }

        // No more thieves! If we were attacking, clean up and resume!
        if (IsAttacking)
        {
            IsAttacking = false;
            Log.Notify("[Trade] All nearby thieves defeated!");

            // Remount transport if possible (otherwise TransportBundle will keep trying periodically while moving)
            if (TradeConfig.MountTransport && !Game.Player.HasActiveVehicle && Game.Player.JobTransport != null && !Game.Player.InAction)
            {
                Game.Player.JobTransport.Mount();
            }

            // Resume trade route script immediately from where we left off!
            Log.Notify("[Trade] Combat finished. Resuming trade route!");
            ScriptManager.ResumeScript();
        }
    }

    /// <summary>
    ///     Finds the next potential thief target according to trade rules.
    ///     STRICTLY targets ONLY Thief Players or Thief NPCs. Normal monsters are NEVER targeted.
    /// </summary>
    private SpawnedBionic FindPotentialTarget()
    {
        if (IsPlayerInTown() || Game.Player == null)
            return null;

        bool smartCombat = TradeConfig.SmartThiefCombat;
        bool protectTransport = TradeConfig.ProtectTransport || smartCombat;
        bool counterAttack = TradeConfig.CounterAttack || smartCombat;
        bool attackThiefNpcs = TradeConfig.AttackThiefNpcs || smartCombat;
        bool attackThiefPlayers = TradeConfig.AttackThiefPlayers || smartCombat;

        // Priority 1: Protect transport (highest priority - only thieves attacking vehicle)
        if (protectTransport && Game.Player.JobTransport != null)
        {
            if (SpawnManager.TryGetEntity<SpawnedBionic>(Game.Player.JobTransport.UniqueId, out var transportBionic))
            {
                var attackers = transportBionic.GetAttackers();
                if (attackers != null && attackers.Count > 0)
                {
                    var attacker = attackers.FirstOrDefault(a => a != null && !a.IsBehindObstacle && IsThief(a));
                    if (attacker != null)
                    {
                        if (attacker is SpawnedPlayer && !attackThiefPlayers) { }
                        else if (attacker is SpawnedMonster && !attackThiefNpcs) { }
                        else return attacker;
                    }
                }
            }

            // Fallback for transport: check if any thief mob nearby is targeting our transport
            if (attackThiefNpcs)
            {
                var transportUid = Game.Player.JobTransport.UniqueId;
                if (SpawnManager.TryGetEntity<SpawnedMonster>(
                        m => m != null && IsThief(m) && m.TargetId == transportUid &&
                             !m.IsBehindObstacle && m.DistanceToPlayer < TradeConfig.MaxAttackDistance,
                        out var mobAttackingTransport))
                {
                    return mobAttackingTransport;
                }
            }
        }

        // Priority 2: Fight back if player attacked directly by a thief
        if (counterAttack)
        {
            var playerAttackers = Game.Player.GetAttackers();
            if (playerAttackers != null && playerAttackers.Count > 0)
            {
                var attacker = playerAttackers.FirstOrDefault(a => a != null && !a.IsBehindObstacle && IsThief(a));
                if (attacker != null)
                {
                    if (attacker is SpawnedPlayer && !attackThiefPlayers) { }
                    else if (attacker is SpawnedMonster && !attackThiefNpcs) { }
                    else return attacker;
                }
            }

            // Fallback for player: check if any thief is targeting player
            if (attackThiefNpcs)
            {
                var playerUid = Game.Player.UniqueId;
                if (SpawnManager.TryGetEntity<SpawnedMonster>(
                        m => m != null && IsThief(m) && m.TargetId == playerUid &&
                             !m.IsBehindObstacle && m.DistanceToPlayer < TradeConfig.MaxAttackDistance,
                        out var mobAttackingPlayer))
                {
                    return mobAttackingPlayer;
                }
            }
        }

        // Priority 3: Thief NPCs nearby
        if (attackThiefNpcs)
        {
            if (SpawnManager.TryGetEntity<SpawnedMonster>(
                    m => m != null && IsThief(m) && !m.IsBehindObstacle &&
                         (m.DistanceToPlayer < TradeConfig.MaxAttackDistance ||
                          (Game.Player.JobTransport != null &&
                           m.Position.DistanceTo(Game.Player.JobTransport.Position) < TradeConfig.MaxAttackDistance)),
                    out var thiefMob))
            {
                return thiefMob;
            }
        }

        // Priority 4: Thief players nearby
        if (attackThiefPlayers)
        {
            if (SpawnManager.TryGetEntity<SpawnedPlayer>(
                    p => p != null && IsThief(p) && !p.IsBehindObstacle && p.DistanceToPlayer < TradeConfig.MaxAttackDistance,
                    out var nearbyThiefPlayer))
            {
                return nearbyThiefPlayer;
            }
        }

        return null;
    }

    /// <summary>
    ///     Stops the bundle.
    /// </summary>
    public void Stop()
    {
        IsAttacking = false;
    }
}
