using RSBot.Core;
using RSBot.Core.Components;

namespace RSBot.CommandCenter.Components.Command;

/// <summary>
///     Bilingual logging: short English for the game client (its font has no
///     Arabic), full Arabic explanation for the bot Log tab (what happened,
///     where the problem is, how to fix it).
/// </summary>
internal static class SonicLog
{
    public static void Notify(bool silent, string gameMsg, string arabic)
    {
        if (!silent)
            Game.ShowNotification(gameMsg);

        Log.Notify("[Sonic] " + arabic);
    }

    public static void Error(string arabic, string technical = null)
    {
        Log.Error("[Sonic] " + arabic + (string.IsNullOrEmpty(technical) ? "" : " | " + technical));
    }

    public const string NeedGame =
        "لازم تدخل اللعبة الأول (المشكلة: مفيش شخصية داخلة، الحل: شغل العميل وادخل بشخصيتك)";
}
