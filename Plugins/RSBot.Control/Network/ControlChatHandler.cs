using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Extensions;
using RSBot.Core.Network;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Spawn;

namespace RSBot.Control.Network;

public class ControlChatHandler : IPacketHandler
{
    public ushort Opcode => 0x3026;
    public PacketDestination Destination => PacketDestination.Client;

    public void Invoke(Packet packet)
    {
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

            ControlManager.Instance.ExecuteCommand(sender, message, type);
        }
        catch
        {
        }
    }
}
