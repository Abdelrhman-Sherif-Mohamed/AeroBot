using RSBot.Core.Extensions;
using RSBot.Core.Network;
using RSBot.Core.Objects;

namespace RSBot.Trivia.Network;

public class TriviaNoticeHandler : IPacketHandler
{
    public ushort Opcode => 0x3026;
    public PacketDestination Destination => PacketDestination.Client;

    public void Invoke(Packet packet)
    {
        try
        {
            var type = (ChatType)packet.ReadByte();
            string message = string.Empty;

            switch (type)
            {
                case ChatType.Notice:
                    message = packet.ReadConditonalString();
                    break;

                case ChatType.All:
                case ChatType.AllGM:
                    var senderId = packet.ReadUInt();
                    message = packet.ReadConditonalString();
                    break;

                default:
                    var sender = packet.ReadString();
                    message = packet.ReadConditonalString();
                    break;
            }

            TriviaManager.Instance.OnMessageReceived(message);
        }
        catch
        {
        }
    }
}
