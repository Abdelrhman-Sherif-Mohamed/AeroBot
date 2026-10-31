using System;
using System.Collections.Generic;
using System.Linq;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Skill;

namespace RSBot.Default.Bundle.Buff;

internal class BuffBundle : IBundle
{
    private bool _invoked;

    /// <summary>
    ///     Invokes this instance.
    /// </summary>
    public void Invoke()
    {
        if (_invoked)
            return;

        if (Game.Player == null || Game.Player.State == null || Game.Player.Untouchable || Game.Player.InAction)
            return;

        try
        {
            _invoked = true;

            /*
             * #377
             * I think the bug now fixed but As a precaution, I find it appropriate to keep this solution here.
             * If the last fix is working, I will remove this code block from here in v2.4
             * Temporary fixer:
             * Issue: Sometimes the buffs dont removing with token from the active buffs list
             *      Problems:
             *          ActionBuffRemoveResponse:0xb072 does not calling
             *          There have another opcode for remove buff with token
             */
            var allBuffs = (SkillManager.Buffs ?? new List<SkillInfo>()).Union(new[]
            {
                SkillManager.ImbueSkill,
                SkillManager.ResurrectionSkill
            });

            foreach (var buff in allBuffs)
            {
                if (buff == null || buff.Record == null)
                    continue;

                var isActive = Game.Player.State.HasActiveBuff(buff, out var info);
                if (isActive && buff.Isbugged && info != null && info.Isbugged)
                {
                    //#377 bug detected!
                    Log.Notify($"[#377] The buff [{buff.Token}-{buff.Record.GetRealName()}] expired");

                    EventManager.FireEvent("OnRemoveBuff", buff);

                    var playerSkill = Game.Player.Skills?.GetSkillInfoById(buff.Id);
                    playerSkill?.Reset();
                    Game.Player.State.TryRemoveActiveBuff(info.Token, out _);
                }
            }

            if (SkillManager.Buffs == null)
                return;

            var buffs = SkillManager.Buffs.FindAll(p => p != null && p.Record != null && !Game.Player.State.HasActiveBuff(p, out _) && p.CanBeCasted);
            if (buffs == null || buffs.Count == 0)
                return;

            Log.Status("Buffing");

            foreach (var buff in buffs)
            {
                if (Game.Player.State.LifeState != LifeState.Alive || Game.Player.HasActiveVehicle)
                    break;

                if (Game.Player.InAction)
                    break;

                if (Game.Player.State.HasActiveBuff(buff, out _) && !buff.HasCooldown)
                    continue;

                Log.Debug($"Trying to cast buff: {buff} {buff.Record.Basic_Code}");

                buff.Cast(buff: true);
                break;
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Error in BuffBundle.Invoke: {ex.Message}");
        }
        finally
        {
            _invoked = false;
        }
    }

    /// <summary>
    ///     Refreshes this instance.
    /// </summary>
    public void Refresh()
    {
        _invoked = false;
    }

    public void Stop()
    {
        _invoked = false;
    }
}