using System;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Extensions;
using RSBot.Core.Network;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Spawn;

namespace Wolves.Fortress.Network;

public class FortressChatHandler : IPacketHandler
{
    public ushort Opcode => 0x3026;
    public PacketDestination Destination => PacketDestination.Client;

    public void Invoke(Packet packet)
    {
        if (!FortressManager.Instance.Enabled)
            return;

        try
        {
            var type = (ChatType)packet.ReadByte();
            string sender = string.Empty;
            string message = string.Empty;

            switch (type)
            {
                case ChatType.All:
                case ChatType.AllGM:
                    var senderId = packet.ReadUInt();
                    message = packet.ReadConditonalString();
                    if (senderId == Game.Player?.UniqueId)
                        sender = Game.Player?.Name ?? string.Empty;
                    else if (SpawnManager.TryGetEntity<SpawnedPlayer>(senderId, out var sp))
                        sender = sp.Name;
                    break;

                case ChatType.Notice:
                case ChatType.Npc:
                    return;

                default:
                    sender = packet.ReadString();
                    message = packet.ReadConditonalString();
                    break;
            }

            if (string.IsNullOrWhiteSpace(message))
                return;

            var trimmed = message.Trim();
            if (trimmed.StartsWith("!") || trimmed.StartsWith("\\"))
                trimmed = trimmed.Substring(1).Trim();

            // Check if trimmed starts with b1..b7 or B1..B7
            if (trimmed.Length >= 2 && (trimmed[0] == 'b' || trimmed[0] == 'B') && char.IsDigit(trimmed[1]))
            {
                var slotNum = trimmed[1] - '0';
                if (slotNum >= 1 && slotNum <= 7)
                {
                    // Ensure not a longer word like "before"
                    if (trimmed.Length == 2 || char.IsWhiteSpace(trimmed[2]))
                    {
                        FortressManager.Instance.HandleSlotTrigger(slotNum, sender, type);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Debug($"[FortressChatHandler] Error parsing chat: {ex.Message}");
        }
    }
}
