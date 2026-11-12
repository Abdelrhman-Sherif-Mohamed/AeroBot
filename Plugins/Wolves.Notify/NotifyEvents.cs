using System.Collections.Generic;
using RSBot.Core;

namespace Wolves.Notify;

internal sealed record NotifyEventMeta(
    string Key,
    string Emoji,
    int EmbedColor,
    string EnglishTitle,
    string ArabicTitle,
    bool BypassCooldown,
    bool CrossProcessDedup)
{
    public string GetLocalizedTitle()
    {
        var isArabic = Kernel.Language != null && Kernel.Language.StartsWith("ar", System.StringComparison.OrdinalIgnoreCase);
        return isArabic ? ArabicTitle : EnglishTitle;
    }
}

/// <summary>
///     Canonical notification event keys and presentation metadata.
/// </summary>
internal static class NotifyEvents
{
    public const string Death = "death";
    public const string BotStopped = "bot_stopped";
    public const string Attacked = "attacked";
    public const string TradeStart = "trade_start";
    public const string TradeSettle = "trade_settle";
    public const string HpPotsOut = "hp_pots_out";
    public const string MpPotsOut = "mp_pots_out";
    public const string PrivateMessage = "pm";
    public const string Global = "global";
    public const string Notice = "notice";
    public const string Unique = "unique";
    public const string LevelUp = "level_up";
    public const string RareDrop = "rare_drop";
    public const string PartyChat = "party_chat";
    public const string GuildChat = "guild_chat";
    public const string UnionChat = "union_chat";
    public const string AcademyChat = "academy_chat";
    public const string AllChat = "all_chat";
    public const string StallChat = "stall_chat";

    public static readonly IReadOnlyList<NotifyEventMeta> All = new[]
    {
        new NotifyEventMeta(Death, "⚠️", 0xE74C3C, "Character died", "موت الشخصية", true, false),
        new NotifyEventMeta(BotStopped, "🛑", 0x95A5A6, "Bot stopped", "توقف البوت", true, false),
        new NotifyEventMeta(Attacked, "⚔️", 0xE67E22, "Attacked by player", "هجوم من لاعب", false, false),
        new NotifyEventMeta(TradeStart, "📦", 0x3498DB, "Trade run started", "بداية جولة التجارة", true, false),
        new NotifyEventMeta(TradeSettle, "✅", 0x2ECC71, "Trade settled", "إتمام جولة التجارة", true, false),
        new NotifyEventMeta(HpPotsOut, "🩸", 0xC0392B, "Out of HP potions", "انتهاء جرعات الدم", true, false),
        new NotifyEventMeta(MpPotsOut, "🔵", 0x2980B9, "Out of MP potions", "انتهاء جرعات المانا", true, false),
        new NotifyEventMeta(PrivateMessage, "✉️", 0x9B59B6, "Private message (PM)", "رسالة خاصة", true, false),
        new NotifyEventMeta(Global, "📢", 0xF1C40F, "Global chat", "رسالة جلوبال", false, true),
        new NotifyEventMeta(Notice, "📜", 0xECEC4F, "Server notice", "إعلان السيرفر", false, true),
        new NotifyEventMeta(Unique, "👑", 0xFFD700, "Unique alert", "تنبيه اليونيك", true, true),
        new NotifyEventMeta(LevelUp, "⬆️", 0x1ABC9C, "Level up", "ارتفاع المستوى", true, false),
        new NotifyEventMeta(RareDrop, "💎", 0x00CED1, "Rare item dropped", "سقوط أداة نادرة (SOX)", true, false),
        new NotifyEventMeta(PartyChat, "🎉", 0xE91E63, "Party chat", "دردشة البارتي", false, false),
        new NotifyEventMeta(GuildChat, "🛡️", 0x7B68EE, "Guild chat", "دردشة الجيلد", false, false),
        new NotifyEventMeta(UnionChat, "🤝", 0x4682B4, "Union chat", "دردشة التحالف", false, false),
        new NotifyEventMeta(AcademyChat, "🎓", 0x9370DB, "Academy chat", "دردشة الأكاديمية", false, false),
        new NotifyEventMeta(AllChat, "🗣️", 0xBDC3C7, "All chat", "الدردشة العامة", false, false),
        new NotifyEventMeta(StallChat, "🏪", 0xCD853F, "Stall chat", "دردشة المتجر", false, false)
    };

    private static readonly Dictionary<string, NotifyEventMeta> ByKey = BuildMap();

    private static Dictionary<string, NotifyEventMeta> BuildMap()
    {
        var map = new Dictionary<string, NotifyEventMeta>(System.StringComparer.OrdinalIgnoreCase);
        foreach (var meta in All)
            map[meta.Key] = meta;
        return map;
    }

    public static NotifyEventMeta GetMeta(string key)
    {
        if (!string.IsNullOrEmpty(key) && ByKey.TryGetValue(key, out var meta))
            return meta;

        return new NotifyEventMeta(key ?? "notice", "🔔", 0x5865F2, "Notification", "إشعار", true, false);
    }
}
