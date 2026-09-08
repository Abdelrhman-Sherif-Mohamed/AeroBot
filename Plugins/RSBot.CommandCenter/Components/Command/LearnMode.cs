using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Objects.Skill;

namespace RSBot.CommandCenter.Components.Command;

/// <summary>
///     "Learn mode": type !learn (or !learnbuff), then cast the skill once
///     inside the game. The cast is captured from the network and the skill
///     is added to the bot automatically - no typing needed at all.
///     Armed by the chat executors, captured by SkillCastLearnHook (0x7074).
/// </summary>
internal static class LearnMode
{
    public static bool ArmedAttack { get; private set; }
    public static bool ArmedBuff { get; private set; }

    public static void ArmAttack()
    {
        ArmedAttack = true;
        ArmedBuff = false;
        Game.ShowNotification("[AeroBot] Learn mode: cast any ATTACK skill once in game, I will add it. Type !learn again to cancel.");
    }

    public static void ArmBuff()
    {
        ArmedBuff = true;
        ArmedAttack = false;
        Game.ShowNotification("[AeroBot] Learn mode: cast any BUFF once in game, I will add it. Type !learnbuff again to cancel.");
    }

    public static void Cancel()
    {
        ArmedAttack = false;
        ArmedBuff = false;
    }

    public static bool IsArmed => ArmedAttack || ArmedBuff;

    /// <summary>Called by the 0x7074 hook for every skill cast by the player.</summary>
    public static void TryCapture(uint skillId)
    {
        if (!IsArmed || Game.Player == null)
            return;

        var wantAttack = ArmedAttack;
        Cancel();

        Game.Player.TryGetAbilitySkills(out var abilitySkills);
        SkillInfo info = Game.Player.Skills.GetSkillInfoById(skillId);
        info ??= abilitySkills?.Find(p => p.Id == skillId);

        if (info == null)
        {
            Game.ShowNotification("[AeroBot] Could not recognize that skill (unknown id " + skillId + ")");
            return;
        }

        if (wantAttack && (info.IsAttack || info.Record.TargetGroup_Enemy_M))
        {
            if (SkillChatHelper.AddAttack(info))
                Game.ShowNotification("[AeroBot] Learned attack: " + SkillChatHelper.Describe(info));
            else
                Game.ShowNotification("[AeroBot] Already have: " + SkillChatHelper.Describe(info));
        }
        else if (!wantAttack && !info.IsAttack && !info.Record.TargetGroup_Enemy_M)
        {
            if (SkillChatHelper.AddBuff(info))
                Game.ShowNotification("[AeroBot] Learned buff: " + SkillChatHelper.Describe(info));
            else
                Game.ShowNotification("[AeroBot] Already have: " + SkillChatHelper.Describe(info));
        }
        else
        {
            Game.ShowNotification("[AeroBot] That looks like a " +
                                  (info.IsAttack ? "attack" : "buff") +
                                  " - use " + (info.IsAttack ? "!learn" : "!learnbuff") + " for it: " +
                                  SkillChatHelper.Describe(info));
        }
    }
}
