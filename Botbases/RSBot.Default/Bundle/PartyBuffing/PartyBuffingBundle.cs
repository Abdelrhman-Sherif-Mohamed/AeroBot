using System;
using System.Collections.Generic;
using System.Linq;
using RSBot.Core;
using RSBot.Core.Event;
using RSBot.Core.Objects.Party;

namespace RSBot.Default.Bundle.PartyBuffing;

internal class PartyBuffingBundle : IBundle
{
    /// <summary>
    ///     <c>true</c> if refreshing this bundle; otherwise <c>false</c>
    /// </summary>
    private bool _refreshing;

    /// <summary>
    ///     The buffing party members
    /// </summary>
    private List<BuffingPartyMember> BuffingPartyMembers = new();

    /// <summary>
    ///     Initialize the instance of <seealso cref="PartyBuffingBundle" />
    /// </summary>
    public PartyBuffingBundle()
    {
        EventManager.SubscribeEvent("OnPartyBuffSettingsChanged", OnPartyBuffSettingsChanged);
        Refresh();
    }

    /// <summary>
    ///     Invoke the bundle
    /// </summary>
    public void Invoke()
    {
        if (_refreshing)
            return;

        if (Game.Party == null ||
            Game.Party.Members == null ||
            Game.Player == null ||
            Game.Player.InAction ||
            Game.Player.HasActiveVehicle ||
            BuffingPartyMembers == null)
            return;

        try
        {
            var selectedGroup = PlayerConfig.Get("RSBot.Party.Buffing.SelectedGroup", "Default");

            var members = Game.Party.Members
                .Where(p => BuffingPartyMembers.Any(s => s.Group == selectedGroup && s.Name == p.Name));
            foreach (var member in members)
            {
                if (member.Player == null || member.Player.State == null)
                    continue;

                var buffingMember = BuffingPartyMembers.Find(p => p.Name == member.Name);
                if (buffingMember == null || buffingMember.Buffs == null || buffingMember.Buffs.Count == 0)
                    continue;

                //if party member is dead, dont try to buff
                var memberHPMP = member.HealthMana.ToString("X2");
                if (memberHPMP.Length == 0)
                    continue;
                int hpPer = Convert.ToByte(memberHPMP[0].ToString(), 16) * 10;
                if (hpPer == 0)
                    continue;

                Log.Status($"Buffing party");

                foreach (var buff in buffingMember.Buffs)
                {
                    try
                    {
                        var skill = Game.Player.Skills?.GetSkillInfoById(buff);

                        var record = skill?.Record;
                        if (skill == null || record == null || skill.HasCooldown)
                            continue;
                        var isActive = member.Player.State.HasActiveBuff(skill, out var info);
                        if (skill.Isbugged && info != null && info.Isbugged)
                        {
                            Log.Notify($"The buff on {member.Name} [{skill.Token}-{record.GetRealName()}] expired");

                            skill.Reset();
                            continue;
                        }

                        if (isActive)
                            continue;

                        if (member.Player != null)
                        {
                            Log.Status($"Buffing {record.GetRealName()} party member {member.Name}");
                            skill.Cast(member.Player.UniqueId, true);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Debug($"PartyBuffing: skipped buff {buff} ({ex.Message})");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Error in PartyBuffingBundle: {ex.Message}");
        }
    }

    /// <summary>
    ///     Refresh the bundle
    /// </summary>
    public void Refresh()
    {
        _refreshing = true;

        // Don't need to use clear, because gc will handle the unnecessary objects
        BuffingPartyMembers = new List<BuffingPartyMember>();

        var settings = PlayerConfig.Get("RSBot.Party.Buffing", string.Empty);
        var collection = settings.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var item in collection)
            BuffingPartyMembers.Add(new BuffingPartyMember(item));

        _refreshing = false;
    }

    public void Stop()
    {
        //Nothing to do here
    }

    /// <summary>
    ///     Update the <see cref="BuffingPartyMembers" /> list when settings changed from another plugins
    /// </summary>
    private void OnPartyBuffSettingsChanged()
    {
        Refresh();
    }
}