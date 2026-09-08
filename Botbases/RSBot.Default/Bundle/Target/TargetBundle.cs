using System.Collections.Generic;
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
            SpawnManager.TryGetEntity<SpawnedMonster>(Game.SelectedEntity.UniqueId, out var selectedMonster) &&
            (byte)attacker.Rarity < (byte)selectedMonster.Rarity)
        {
            Log.Debug("[TargetBundle] Emergency situation: Found a weaker mob to attack first, switching target!");

            if (attacker.TrySelect())
                Bundles.Movement.LastEntityWasBehindObstacle = false;

            return;
        }

        var warlockModeEnabled = PlayerConfig.Get("RSBot.Skills.checkWarlockMode", false);
        if (warlockModeEnabled && Game.SelectedEntity.State.HasTwoDots())
            return;

        if (Game.SelectedEntity != null && Game.SelectedEntity is not SpawnedMonster)
            Game.SelectedEntity = null;

        if (Game.SelectedEntity?.State.LifeState == LifeState.Alive)
            return;

        var monster = GetNearestEnemy();
        if (monster == null)
            return;

        if (!Container.Bot.Area.IsInSight(monster))
            return;

        if (monster.TrySelect())
            Bundles.Movement.LastEntityWasBehindObstacle = false;
    }

    private SpawnedMonster GetFromCurrentAttackers()
    {
        var attackWeakerFirst = PlayerConfig.Get<bool>("RSBot.Training.checkAttackWeakerFirst");
        if (!attackWeakerFirst || !IsEmergencySituation())
            return null;

        if (!SpawnManager.TryGetEntities<SpawnedMonster>(e => e.AttackingPlayer && e.State.LifeState == LifeState.Alive,
                out var entities))
            return null;

        return entities.OrderBy(e => (byte)e.Rarity)
            .OrderBy(e => e.Record.Level)
            .OrderByDescending(e => e.Position.DistanceToPlayer())
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
    private SpawnedMonster GetNearestEnemy()
    {
        var warlockModeEnabled = PlayerConfig.Get<bool>("RSBot.Skills.checkWarlockMode");
        var ignorePillar = PlayerConfig.Get<bool>("RSBot.Training.checkBoxDimensionPillar");
        var areaPos = Container.Bot.Area.Position;

        if (!SpawnManager.TryGetEntities<SpawnedMonster>(m =>
                m.State.LifeState == LifeState.Alive && //Only alive
                !(warlockModeEnabled && m.State.HasTwoDots()) && //Has two Dots?
                (_blacklist == null || !_blacklist.ContainsKey(m.UniqueId)) && //Is not blacklisted
                (m.AttackingPlayer || !Bundles.Avoidance.AvoidMonster(m.Rarity)) && //Is attacking player or shouldn't be avoided
                !m.Record.IsPandora && //Isn't pandora box
                !(m.Record.IsDimensionPillar && ignorePillar) && //Isn't dimension pillar
                !m.Record.IsSummonFlower, out var entities))
            return default;

        // Keep every attacker + the 8 nearest others (bounded candidate set)
        var attackers = new List<SpawnedMonster>(8);
        var nearest = new List<(SpawnedMonster Mob, double Dist)>(8);

        foreach (var m in entities)
        {
            if (m.AttackingPlayer)
            {
                attackers.Add(m);
                continue;
            }

            var d = m.Movement.Source.DistanceTo(areaPos);
            if (nearest.Count < 8)
            {
                nearest.Add((m, d));
                continue;
            }

            for (var i = 0; i < nearest.Count; i++)
            {
                if (d < nearest[i].Dist)
                {
                    nearest[i] = (m, d);
                    break;
                }
            }
        }

        var candidates = new List<SpawnedMonster>(attackers.Count + nearest.Count);
        candidates.AddRange(attackers);
        foreach (var (mob, _) in nearest)
            candidates.Add(mob);

        return candidates
            .Where(m => m.IsBehindObstacle == false)
            .Where(m => Container.Bot.Area.IsInSight(m))
            .OrderBy(m => m.Movement.Source.DistanceTo(areaPos))
            .OrderBy(m => Bundles.Avoidance.PreferMonster(m.Rarity))
            .OrderByDescending(m => m.AttackingPlayer)
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