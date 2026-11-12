using System;
using System.Collections.Generic;
using System.Linq;
using RSBot.Core;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Extensions;
using RSBot.Core.Network;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Spawn;

namespace Wolves.Notify;

/// <summary>
///     Subscribes to core events and incoming packets to trigger Telegram/Discord notifications.
/// </summary>
internal static class NotifyMonitor
{
    private static int _initialized;
    private static bool _hadHpPotions;
    private static bool _hadMpPotions;

    public static void Initialize()
    {
        if (System.Threading.Interlocked.Exchange(ref _initialized, 1) == 1)
            return;

        NotifyConfig.Load();
        NotifySender.StartWorker();

        EventManager.SubscribeEvent("OnPlayerDied", OnPlayerDied);
        EventManager.SubscribeEvent("OnStopBot", OnStopBot);
        EventManager.SubscribeEvent("OnLevelUp", new Action<byte>(OnLevelUp));
        EventManager.SubscribeEvent("OnTradeRunStarted", OnTradeRunStarted);
        EventManager.SubscribeEvent("OnTradeRunSettled", new Action<int, long, long>(OnTradeRunSettled));
        EventManager.SubscribeEvent("OnEntityHit", new Action<uint, uint, uint, uint, bool>(OnEntityHit));
        EventManager.SubscribeEvent("OnSpawnMonster", new Action<SpawnedMonster>(OnSpawnMonster));
        EventManager.SubscribeEvent("OnSpawnItem", new Action<SpawnedItem>(OnSpawnItem));
        EventManager.SubscribeEvent("OnClock", OnClock);
        EventManager.SubscribeEvent("OnLogout", ResetSessionState);
        EventManager.SubscribeEvent("OnAgentServerDisconnected", ResetSessionState);

        PacketManager.RegisterHandler(new NotifyChatPacketHandler());
        PacketManager.RegisterHandler(new NotifyUniquePacketHandler());
    }

    private static void ResetSessionState()
    {
        _hadHpPotions = false;
        _hadMpPotions = false;
    }

    private static void OnPlayerDied()
    {
        var player = Game.Player;
        var pos = player != null ? $"({player.Position.X:0}, {player.Position.Y:0})" : "—";
        NotifySender.Enqueue(
            NotifyEvents.Death,
            "Character died.",
            new[] { new NotifyField("Position", pos) });
    }

    private static void OnStopBot()
    {
        var botbase = Kernel.Bot?.Botbase?.Name;
        if (string.IsNullOrWhiteSpace(botbase))
            botbase = "Normal";

        NotifySender.Enqueue(
            NotifyEvents.BotStopped,
            "Bot execution stopped.",
            new[] { new NotifyField("Botbase", botbase) });
    }

    private static void OnLevelUp(byte oldLevel)
    {
        var newLevel = Game.Player?.Level ?? (byte)(oldLevel + 1);
        NotifySender.Enqueue(
            NotifyEvents.LevelUp,
            $"Reached level {newLevel}.",
            new[]
            {
                new NotifyField("Previous", oldLevel.ToString()),
                new NotifyField("New", newLevel.ToString())
            });
    }

    private static void OnTradeRunStarted()
    {
        var pos = Game.Player != null ? $"({Game.Player.Position.X:0}, {Game.Player.Position.Y:0})" : "—";
        NotifySender.Enqueue(
            NotifyEvents.TradeStart,
            "Trade caravan started.",
            new[] { new NotifyField("Position", pos) });
    }

    private static void OnTradeRunSettled(int runs, long durationMs, long profitGold)
    {
        var duration = TimeSpan.FromMilliseconds(Math.Max(0, durationMs));
        var durationText = duration.TotalHours >= 1
            ? duration.ToString(@"hh\:mm\:ss")
            : duration.ToString(@"mm\:ss");
        var sign = profitGold >= 0 ? "+" : string.Empty;

        NotifySender.Enqueue(
            NotifyEvents.TradeSettle,
            $"Completed trade run #{runs} in {durationText}.",
            new[]
            {
                new NotifyField("Runs", runs.ToString("N0")),
                new NotifyField("Duration", durationText),
                new NotifyField("Profit", $"{sign}{profitGold:N0} Gold")
            });
    }

    private static void OnEntityHit(uint actionId, uint executorId, uint targetId, uint damage, bool crit)
    {
        var player = Game.Player;
        if (player == null || targetId != player.UniqueId || executorId == player.UniqueId)
            return;

        if (!SpawnManager.TryGetEntity<SpawnedPlayer>(executorId, out var attacker) || attacker == null)
            return;

        var hpPct = player.MaximumHealth > 0
            ? $"{player.Health * 100.0 / player.MaximumHealth:0}%"
            : "—";

        NotifySender.Enqueue(
            NotifyEvents.Attacked,
            $"Attacked by player {attacker.Name}.",
            new[]
            {
                new NotifyField("Attacker", string.IsNullOrWhiteSpace(attacker.Name) ? $"[{executorId}]" : attacker.Name),
                new NotifyField("Damage", $"{damage:N0}{(crit ? " (Crit)" : string.Empty)}"),
                new NotifyField("HP", hpPct)
            });
    }

    private static void OnSpawnMonster(SpawnedMonster monster)
    {
        if (monster == null)
            return;

        var isUniqueRarity = monster.Rarity is MonsterRarity.Unique or MonsterRarity.Unique2
            or MonsterRarity.UniqueParty or MonsterRarity.Unique2Party;
        var mobName = monster.Record?.GetRealName() ?? monster.Record?.CodeName ?? string.Empty;

        if (!isUniqueRarity && !MatchesUniqueKeyword(mobName))
            return;

        var dist = Game.Player != null ? $"{monster.Position.DistanceTo(Game.Player.Position):0}m" : "—";
        NotifySender.Enqueue(
            NotifyEvents.Unique,
            $"Unique nearby: {mobName}",
            new[]
            {
                new NotifyField("Mob", mobName),
                new NotifyField("Max HP", $"{monster.MaxHealth:N0}"),
                new NotifyField("Distance", dist)
            },
            dedupKey: $"spawn:{mobName}");
    }

    private static void OnSpawnItem(SpawnedItem item)
    {
        if (item?.Record == null || item.Record.IsGold)
            return;

        if ((int)item.Rarity < (int)ObjectRarity.ClassC)
            return;

        var itemName = item.Record.GetRealName(true);
        var owner = string.IsNullOrWhiteSpace(item.OwnerName) ? "Free" : item.OwnerName;
        NotifySender.Enqueue(
            NotifyEvents.RareDrop,
            $"Dropped: {itemName}",
            new[]
            {
                new NotifyField("Item", itemName),
                new NotifyField("Owner", owner)
            });
    }

    private static void OnClock()
    {
        var player = Game.Player;
        if (player?.Inventory == null || player.Health == 0)
            return;

        long hpCount = 0;
        long mpCount = 0;
        foreach (var item in player.Inventory.ToArray())
        {
            if (item?.Record == null || item.Slot < CharacterInventory.NORMAL_PART_MIN_SLOT)
                continue;

            if (item.Record.IsHpPotion || item.Record.IsAllPotion)
                hpCount += item.Amount;
            else if (item.Record.IsMpPotion)
                mpCount += item.Amount;
        }

        if (hpCount > 0)
            _hadHpPotions = true;
        else if (_hadHpPotions)
        {
            _hadHpPotions = false;
            NotifySender.Enqueue(NotifyEvents.HpPotsOut, "HP potions depleted (0 remaining).");
        }

        if (mpCount > 0)
            _hadMpPotions = true;
        else if (_hadMpPotions)
        {
            _hadMpPotions = false;
            NotifySender.Enqueue(NotifyEvents.MpPotsOut, "MP potions depleted (0 remaining).");
        }
    }

    internal static bool MatchesUniqueKeyword(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var keywords = NotifyConfig.Current.UniqueKeywords;
        if (string.IsNullOrWhiteSpace(keywords))
            return false;

        var parts = keywords.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var kw in parts)
        {
            if (kw.Length >= 3 && text.Contains(kw, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    internal static bool UsesUnicodeChat()
    {
        return Game.ClientType is GameClientType.Thailand or GameClientType.Global
            or GameClientType.Turkey or GameClientType.Korean
            or GameClientType.Rigid or GameClientType.VTC_Game;
    }

    internal static string ReadChatString(Packet packet)
    {
        if (packet == null || packet.Remaining < 2)
            return string.Empty;

        if (!UsesUnicodeChat() &&
            Kernel.Language != null &&
            Kernel.Language.StartsWith("ar", StringComparison.OrdinalIgnoreCase))
        {
            return packet.ReadString(1256);
        }

        return packet.ReadConditonalString();
    }

    internal static string ReadSenderName(Packet packet)
    {
        if (packet == null || packet.Remaining < 2)
            return string.Empty;

        if (!UsesUnicodeChat() &&
            Kernel.Language != null &&
            Kernel.Language.StartsWith("ar", StringComparison.OrdinalIgnoreCase))
        {
            return packet.ReadString(1256);
        }

        return packet.ReadString();
    }
}

internal sealed class NotifyChatPacketHandler : IPacketHandler
{
    public ushort Opcode => 0x3026;
    public PacketDestination Destination => PacketDestination.Client;

    public void Invoke(Packet packet)
    {
        if (packet == null || packet.Remaining < 1)
            return;

        var type = (ChatType)packet.ReadByte();
        if (type == ChatType.Npc)
            return;

        string sender;
        string message;

        switch (type)
        {
            case ChatType.All:
            case ChatType.AllGM:
            {
                if (packet.Remaining < 6)
                    return;

                var senderId = packet.ReadUInt();
                message = NotifyMonitor.ReadChatString(packet);

                if (Game.Player != null && senderId == Game.Player.UniqueId)
                    return; // Skip own outgoing All chat echo

                if (SpawnManager.TryGetEntity<SpawnedPlayer>(senderId, out var player) && !string.IsNullOrWhiteSpace(player?.Name))
                    sender = player.Name;
                else
                    sender = type == ChatType.AllGM ? "GM" : $"[{senderId}]";
                break;
            }

            case ChatType.Notice:
            {
                sender = "Notice";
                message = NotifyMonitor.ReadChatString(packet);
                break;
            }

            default:
            {
                sender = NotifyMonitor.ReadSenderName(packet);
                message = NotifyMonitor.ReadChatString(packet);
                if (Game.Player != null && string.Equals(sender, Game.Player.Name, StringComparison.OrdinalIgnoreCase))
                    return; // Skip own chat echo
                break;
            }
        }

        if (string.IsNullOrWhiteSpace(message))
            return;

        if (type is ChatType.Global or ChatType.Notice && NotifyMonitor.MatchesUniqueKeyword(message))
        {
            NotifySender.Enqueue(
                NotifyEvents.Unique,
                message,
                new[] { new NotifyField("Source", type.ToString()) },
                dedupKey: $"chat:{message}");
            return;
        }

        var eventKey = type switch
        {
            ChatType.Private => NotifyEvents.PrivateMessage,
            ChatType.Global => NotifyEvents.Global,
            ChatType.Notice or ChatType.AllGM => NotifyEvents.Notice,
            ChatType.Party => NotifyEvents.PartyChat,
            ChatType.Guild => NotifyEvents.GuildChat,
            ChatType.Union => NotifyEvents.UnionChat,
            ChatType.Academy => NotifyEvents.AcademyChat,
            ChatType.All => NotifyEvents.AllChat,
            ChatType.Stall => NotifyEvents.StallChat,
            _ => NotifyEvents.Notice
        };

        var fields = string.IsNullOrWhiteSpace(sender)
            ? null
            : new[] { new NotifyField("From", sender) };

        NotifySender.Enqueue(eventKey, message, fields, dedupKey: $"{sender}:{message}");
    }
}

internal sealed class NotifyUniquePacketHandler : IPacketHandler
{
    public ushort Opcode => 0x300C;
    public PacketDestination Destination => PacketDestination.Client;

    public void Invoke(Packet packet)
    {
        if (packet == null || packet.Remaining < 1)
            return;

        var noticeType = packet.ReadByte();
        if (Game.ClientType > GameClientType.Thailand && packet.Remaining >= 1)
            packet.ReadByte();

        var characterData = Game.ReferenceManager?.CharacterData;
        if (characterData == null || packet.Remaining < 4)
            return;

        if (noticeType is 0x05 or 0x06)
        {
            var refObjId = packet.ReadUInt();
            if (!characterData.TryGetValue(refObjId, out var obj))
                return;

            var mobName = obj.GetRealName();
            if (noticeType == 0x05)
            {
                NotifySender.Enqueue(
                    NotifyEvents.Unique,
                    $"{mobName} has appeared!",
                    new[] { new NotifyField("Unique", mobName), new NotifyField("Status", "Spawned") },
                    dedupKey: $"u_spawn:{mobName}");
            }
            else
            {
                var killer = NotifyMonitor.ReadSenderName(packet);
                var msg = killer == "???" || string.IsNullOrWhiteSpace(killer)
                    ? $"{mobName} has disappeared."
                    : $"{mobName} was killed by [{killer}].";

                NotifySender.Enqueue(
                    NotifyEvents.Unique,
                    msg,
                    new[]
                    {
                        new NotifyField("Unique", mobName),
                        new NotifyField("Killer", string.IsNullOrWhiteSpace(killer) ? "—" : killer)
                    },
                    dedupKey: $"u_kill:{mobName}:{killer}");
            }
        }
    }
}
