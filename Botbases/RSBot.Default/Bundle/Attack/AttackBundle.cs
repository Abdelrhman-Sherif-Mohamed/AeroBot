using System;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Objects;

namespace RSBot.Default.Bundle.Attack;

internal class AttackBundle : IBundle
{
    /// <summary>
    ///     The last tick count for checking func call
    /// </summary>
    private int _lastTick = Kernel.TickCount;

    /// <summary>
    ///     Last target unique identifier for instant reaction on target change
    /// </summary>
    private uint _lastTargetUniqueId;

    /// <summary>
    ///     Invokes this instance.
    /// </summary>
    public void Invoke()
    {
        try
        {
            if (Game.SelectedEntity == null || Game.Player == null || !Game.Player.CanAttack)
                return;

            // A forced target (e.g. the leader's target in Target Support mode)
            // is always attacked, even if it is a player.
            var isForcedTarget = TargetOverride.IsForced(Game.SelectedEntity.UniqueId);

            if (Game.SelectedEntity.State?.LifeState == LifeState.Dead ||
                (!isForcedTarget && Game.SelectedEntity.HasHealth && Game.SelectedEntity.Health <= 0))
            {
                Game.SelectedEntity = null;
                return;
            }

            // Obstacle handling is managed via OnTargetBehindObstacle from server response

            if (SkillManager.ImbueSkill?.Record != null &&
                Game.Player.State != null &&
                !Game.Player.State.HasActiveBuff(SkillManager.ImbueSkill, out _) &&
                SkillManager.ImbueSkill.CanBeCasted)
                SkillManager.ImbueSkill.Cast(buff: true);

            if (Game.Player.InAction)
            {
                if (SkillManager.IsLastCastedBasic)
                    SkillManager.CancelAction();

                return;
            }

            var targetId = Game.SelectedEntity.UniqueId;
            var isNewTarget = targetId != _lastTargetUniqueId;

            if (!isNewTarget && Kernel.TickCount - _lastTick < 100)
                return;

            _lastTargetUniqueId = targetId;
            _lastTick = Kernel.TickCount;

            var useTeleportSkill = PlayerConfig.Get("RSBot.Skills.checkUseTeleportSkill", false);
            if (useTeleportSkill && CastTeleportation())
                return;

            var skill = SkillManager.GetNextSkill();

            if (!Game.Player.InAction)
                Log.Status("Attacking");

            if (skill == null)
            {
                if (isForcedTarget ||
                    (PlayerConfig.Get("RSBot.Skills.checkUseDefaultAttack", false) &&
                     !SkillManager.HasConfiguredSkills))
                {
                    SkillManager.CastAutoAttack();
                }

                return;
            }

            // Skill range check: if target is farther than skill range, avoid wasting Area skills on empty air
            var skillRange = skill.Record != null && skill.Record.Action_Range > 0
                ? (skill.Record.Action_Range / 10f) + 3f
                : 18f;

            if (Game.SelectedEntity.DistanceToPlayer > skillRange)
            {
                if (isForcedTarget)
                {
                    // Keep the forced target and use the basic attack instead
                    // of dropping it.
                    SkillManager.CastAutoAttack();
                    return;
                }

                var walkToCenter = PlayerConfig.Get<bool>("RSBot.Training.radioCenter");
                if (walkToCenter || (Container.Bot?.Area != null && Container.Bot.Area.Radius <= 30))
                {
                    // In stationary/taxi mode, clear target so TargetBundle selects a mob within skill range
                    Game.SelectedEntity = null;
                    return;
                }
            }

            skill.Cast(targetId);
        }
        catch (Exception ex)
        {
            Log.Error($"Error in AttackBundle: {ex.Message}");
        }
    }

    /// <summary>
    ///     Refreshes this instance.
    /// </summary>
    public void Refresh()
    {
        _lastTargetUniqueId = 0;
    }

    public void Stop()
    {
        _lastTargetUniqueId = 0;
    }

    /// <summary>
    ///     Casts the teleportation skill if it's set up.
    /// </summary>
    /// <returns></returns>
    private bool CastTeleportation()
    {
        if (SkillManager.TeleportSkill?.CanBeCasted != true ||
            SkillManager.TeleportSkill?.Record?.Params == null ||
            SkillManager.TeleportSkill.Record.Params.Count <= 3 ||
            Game.SelectedEntity?.State?.LifeState != LifeState.Alive)
            return false;

        var distanceToMonster = Game.SelectedEntity?.DistanceToPlayer;
        if (distanceToMonster == null)
            return false;

        var availableDistance = 0;
        if (SkillManager.TeleportSkill.Record.Params != null && SkillManager.TeleportSkill.Record.Params.Count > 3)
            availableDistance = SkillManager.TeleportSkill.Record.Params[3] / 10;

        if (availableDistance <= 0)
        {
            Log.Warn("The selected teleportation skill does not have a distance. Is this really a teleport skill?");
        }
        else
        {
            var distanceAfterCasting = distanceToMonster.Value - availableDistance;
            if (distanceAfterCasting < 0)
                distanceAfterCasting *= -1;

            if (distanceAfterCasting < distanceToMonster.Value)
            {
                SkillManager.TeleportSkill.CastAt(Game.SelectedEntity.Position);

                Log.Debug(
                    $"Used teleportation skill [{SkillManager.TeleportSkill.Record.GetRealName()}] (before: {distanceToMonster.Value}m, after: {distanceAfterCasting}m, traveled: {availableDistance}m)");

                return true;
            }
        }

        return false;
    }
}
