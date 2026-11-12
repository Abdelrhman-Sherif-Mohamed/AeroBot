using RSBot.Chat.Views;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Network;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Spawn;

namespace RSBot.Chat.Network;

internal class ChatResponse : IPacketHandler
{
    /// <summary>
    ///     Gets or sets the opcode.
    /// </summary>
    /// <value>
    ///     The opcode.
    /// </value>
    public ushort Opcode => 0x3026;

    /// <summary>
    ///     Gets or sets the destination.
    /// </summary>
    /// <value>
    ///     The destination.
    /// </value>
    public PacketDestination Destination => PacketDestination.Client;

    /// <summary>
    ///     Handles the packet.
    /// </summary>
    /// <param name="packet">The packet.</param>
    public void Invoke(Packet packet)
    {
        if (packet == null || packet.Remaining < 1)
            return;

        var type = (ChatType)packet.ReadByte();

        switch (type)
        {
            case ChatType.All:
            case ChatType.AllGM:
            {
                if (packet.Remaining < 6)
                    return;

                var senderId = packet.ReadUInt();
                var message = Bundle.Chat.ReadChatString(packet);

                string senderName;
                if (Game.Player != null && senderId == Game.Player.UniqueId)
                {
                    senderName = Game.Player.Name;
                }
                else if (SpawnManager.TryGetEntity<SpawnedPlayer>(senderId, out var player) && !string.IsNullOrWhiteSpace(player?.Name))
                {
                    senderName = player.Name;
                }
                else
                {
                    senderName = type == ChatType.AllGM ? "GM" : $"[{senderId}]";
                }

                View.Instance.AppendMessage(message, senderName, type);
                break;
            }

            case ChatType.Notice:
            {
                var message = Bundle.Chat.ReadChatString(packet);
                View.Instance.AppendMessage(message, "Notice", type);
                break;
            }

            case ChatType.Npc:
                break;

            default:
            {
                var sender = Bundle.Chat.ReadSenderName(packet);
                var message = Bundle.Chat.ReadChatString(packet);
                if (string.IsNullOrEmpty(sender))
                    sender = type.ToString();

                View.Instance.AppendMessage(message, sender, type);
                break;
            }
        }
    }
}
