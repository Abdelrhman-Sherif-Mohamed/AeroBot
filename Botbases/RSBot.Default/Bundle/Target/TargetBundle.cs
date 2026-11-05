using System;
﻿using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Extensions;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Spawn;

namespace RSBot.Default.Bundle.Target;

internal class TargetBundle : IBundle
{
    private const int BLACKLIST_TIMEOUT = 5_000;

    #region Fields

    private Dictionary<uint, int> _blacklist;

    #endregion Fields

    #region Constructor

    public TargetBundle()
    {
        SubscribeEvents();
    }

    #endregion Constructor

    #region Events

    private void OnTargetBehindObstacle()
    {
        if (Game.SelectedEntity == null)
            return;

        var selectedEntityUniqueId = Game.SelectedEntity.UniqueId;
        Game.SelectedEntity?.TryDeselect();
        Game.SelectedEntity = null;

        Bundles.Movement.LastEntityWasBehindObstacle = true;

        if (_blacklist?.TryAdd(selectedEntityUniqueId, Kernel.TickCount) == true)
            Log.Debug($"Add mob [{selectedEntityUniqueId} to blacklist for {BLACKLIST_TIMEOUT}ms");
    }

    #endregion Events

    #region Methods

    private void SubscribeEvents()
    {
        EventManager.SubscribeEvent("OnTargetBehindObstacle", OnTargetBehindObstacle);
    }

    /// <summary>
    ///     Invokes this instance.
    /// </summary>
    public void Invoke()
    {
        try
        {
            _blacklist?.RemoveAll((uniqueId, tick) =>
            {
                var flag = Kernel.TickCount - tick > BLACKLIST_TIMEOUT;
                if (flag)
                    Log.Debug($"Removed mob [{uniqueId} from blacklist!");

                return flag;
            });

            var attacker = GetFromCurrentAttackers();
            if (attacker != null && Game.SelectedEntity == null)
            {
                Log.Debug("[TargetBundle] Emergency situation: Attacking the weaker mob first!");

                if (attacker.TrySelect())
                    Bundles.Movement.LastEntityWasBehindObstacle = false;

                return;
            }

            if (attacker != null &&
                Game.SelectedEntity != null &&
                SpawnManager.TryGetEntity<SpawnedMonster>(Game.SelectedEntity.UniqueId, out var selectedMonster) &&
                (byte)attacker.Rarity < (byte)selectedMonster.Rarity)
            {
                Log.Debug("[TargetBundle] Emergency situation: Found a weaker mob to attack first, switching target!");

                if (attacker.TrySelect())
                    Bundles.Movement.LastEntityWasBehindObstacle = false;

                return;
            }

            var warlockModeEnabled = PlayerConfig.Get("RSBot.Skills.checkWarlockMode", false);
            if (warlockModeEnabled && Game.SelectedEntity?.State != null && Game.SelectedEntity.State.HasTwoDots())
                return;

            if (Game.SelectedEntity != null && (Game.SelectedEntity is not SpawnedMonster ||
                Game.SelectedEntity.State?.LifeState == LifeState.Dead ||
                !Game.SelectedEntity.HasHealth ||
                Game.SelectedEntity.Health <= 0))
            {
                Game.SelectedEntity = null;
            }

            if (Game.SelectedEntity?.State?.LifeState == LifeState.Alive && Game.SelectedEntity.HasHealth && Game.SelectedEntity.Health > 0)
                return;

            var monster = GetNearestEnemy();
            if (monster == null)
                return;

            if (Container.Bot?.Area != null && !Container.Bot.Area.IsInSight(monster))
                return;

            if (monster.TrySelect())
                Bundles.Movement.LastEntityWasBehindObstacle = false;
        }
        catch (System.Exception ex)
        {
            Log.Error($"Error in TargetBundle.Invoke: {ex.Message}");
        }
    }

    private SpawnedMonster GetFromCurrentAttackers()
    {
        var attackWeakerFirst = PlayerConfig.Get<bool>("RSBot.Training.checkAttackWeakerFirst");
        if (!attackWeakerFirst || !IsEmergencySituation())
            return null;

        if (!SpawnManager.TryGetEntities<SpawnedMonster>(e => e.AttackingPlayer && e.State.LifeState == LifeState.Alive && e.HasHealth && e.Health > 0,
                out var entities))
            return null;

        return entities.OrderBy(e => (byte)e.Rarity)
            .ThenBy(e => e.Record.Level)
            .ThenBy(e => e.Position.DistanceToPlayer())
            .FirstOrDefault();
    }

    private bool IsEmergencySituation()
    {
        return SpawnManager.Any<SpawnedMonster>(e =>
            e.AttackingPlayer && e.State.LifeState == LifeState.Alive && Bundles.Avoidance.AvoidMonster(e.Rarity));
    }

    /// <summary>
    ///     Gets the nearest enemy.
    ///     Fast path: cheap filters on all mobs, keep attackers + 8 nearest,
    ///     expensive checks (collision) only on those, then rank.
    /// </summary>
    /// <returns></returns>
    private SpawnedMonster GetNearestEnemy()
    {
        if (Container.Bot?.Area == null)
            return default;

        var warlockModeEnabled = PlayerConfig.Get<bool>("RSBot.Skills.checkWarlockMode");
        var ignorePillar = PlayerConfig.Get<bool>("RSBot.Training.checkBoxDimensionPillar");
        var area = Container.Bot.Area;

        // Fallback to player position if area center is not set
        var centerPos = (area.Position.X != 0 || area.Position.Y != 0)
            ? area.Position
            : (Game.Player?.Position ?? default);
        var radius = area.Radius > 0 ? area.Radius : 50;

        if (!SpawnManager.TryGetEntities<SpawnedMonster>(m =>
                m != null &&
                m.State != null &&
                m.State.LifeState == LifeState.Alive && // Only alive
                m.HasHealth &&
                m.Health > 0 &&
                !(warlockModeEnabled && m.State.HasTwoDots()) && // Has two Dots?
                (_blacklist == null || !_blacklist.ContainsKey(m.UniqueId)) && // Is not blacklisted
                (m.AttackingPlayer || !Bundles.Avoidance.AvoidMonster(m.Rarity)) && // Is attacking player or shouldn't be avoided
                m.Record != null &&
                !m.Record.IsPandora && // Isn't pandora box
                !(m.Record.IsDimensionPillar && ignorePillar) && // Isn't dimension pillar
                !m.Record.IsSummonFlower &&
                (m.AttackingPlayer || m.Position.DistanceTo(centerPos) <= radius), out var entities))
            return default;

        // Priority 1: any monster attacking the player within reasonable distance
        var attacker = entities
            .Where(m => m.AttackingPlayer && m.Position.DistanceToPlayer() <= Math.Max(radius, 30))
            .OrderByDescending(m => Bundles.Avoidance.PreferMonster(m.Rarity))
            .ThenBy(m => m.Position.DistanceToPlayer())
            .FirstOrDefault();

        if (attacker != null)
            return attacker;

        // Priority 2: closest monster within training area to the player
        return entities
            .Where(m => m.Position.DistanceTo(centerPos) <= radius)
            .OrderByDescending(m => Bundles.Avoidance.PreferMonster(m.Rarity))
            .ThenBy(m => m.Position.DistanceToPlayer())
            .FirstOrDefault();
    }

    /// <summary>
    ///     Refreshes this instance.
    /// </summary>
    public void Refresh()
    {
        _blacklist = new Dictionary<uint, int>(8);
    }

    public void Stop()
    {
        _blacklist = null;
    }

    #endregion Methods
}
