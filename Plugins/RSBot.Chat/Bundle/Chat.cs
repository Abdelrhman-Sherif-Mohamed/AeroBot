using System;
using System.Collections.Generic;
using RSBot.Core;
using RSBot.Core.Extensions;
using RSBot.Core.Network;
using RSBot.Core.Objects;

namespace RSBot.Chat.Bundle;

internal static class Chat
{
    private const long PendingAckExpiryMs = 2500;
    private static readonly object _ackLock = new();
    private static readonly Queue<long> _pendingBotAckTimestamps = new();
    private static byte _chatIndex = 1;

    /// <summary>
    ///     Clears any pending bot-originated 0xB025 response suppressions (e.g. on disconnect).
    /// </summary>
    internal static void ClearPendingAcks()
    {
        lock (_ackLock)
        {
            _pendingBotAckTimestamps.Clear();
        }
    }

    /// <summary>
    ///     Consumes a pending bot-sent chat confirmation if one is within its short response window.
    /// </summary>
    internal static bool TryConsumePendingAck()
    {
        var now = Environment.TickCount64;
        lock (_ackLock)
        {
            while (_pendingBotAckTimestamps.Count > 0 && now - _pendingBotAckTimestamps.Peek() > PendingAckExpiryMs)
                _pendingBotAckTimestamps.Dequeue();

            if (_pendingBotAckTimestamps.Count == 0)
                return false;

            _pendingBotAckTimestamps.Dequeue();
            return true;
        }
    }

    /// <summary>
    ///     Sends a chat packet through the AgentServer proxy connection.
    /// </summary>
    internal static bool SendChatPacket(ChatType type, string message, string reciever, out string errorReason)
    {
        errorReason = null;

        if (Kernel.Proxy == null || !Kernel.Proxy.IsConnectedToAgentserver || !Game.Ready || Game.Player == null)
        {
            errorReason = "NotInGame";
            return false;
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            errorReason = "EmptyMessage";
            return false;
        }

        var trimmedTarget = reciever?.Trim();
        if (type == ChatType.Private && string.IsNullOrWhiteSpace(trimmedTarget))
        {
            errorReason = "MissingReceiver";
            return false;
        }

        byte index;
        lock (_ackLock)
        {
            index = _chatIndex;
            _chatIndex = _chatIndex == byte.MaxValue ? (byte)1 : (byte)(_chatIndex + 1);
            if (!Game.Clientless)
                _pendingBotAckTimestamps.Enqueue(Environment.TickCount64);
        }

        var chatPacket = new Packet(0x7025);
        chatPacket.WriteByte(type);
        chatPacket.WriteByte(index);

        if (Game.ClientType > GameClientType.Vietnam)
            chatPacket.WriteByte(0); // has linking

        if (Game.ClientType >= GameClientType.Chinese)
            chatPacket.WriteByte(0);

        if (type == ChatType.Private)
        {
            if (ContainsArabic(trimmedTarget) && !UsesUnicodeChat())
                chatPacket.WriteString(trimmedTarget, 1256);
            else
                chatPacket.WriteString(trimmedTarget);
        }

        if (!UsesUnicodeChat() && ContainsArabic(message))
            chatPacket.WriteString(message, 1256);
        else
            chatPacket.WriteConditonalString(message);

        PacketManager.SendPacket(chatPacket, PacketDestination.Server);
        return true;
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

    internal static bool UsesUnicodeChat()
    {
        return Game.ClientType == GameClientType.Thailand ||
               Game.ClientType == GameClientType.Global ||
               Game.ClientType == GameClientType.Turkey ||
               Game.ClientType == GameClientType.Korean ||
               Game.ClientType == GameClientType.Rigid ||
               Game.ClientType == GameClientType.VTC_Game;
    }

    internal static bool ContainsArabic(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        foreach (var ch in text)
        {
            if (ch >= '\u0600' && ch <= '\u06FF')
                return true;
        }

        return false;
    }
}
