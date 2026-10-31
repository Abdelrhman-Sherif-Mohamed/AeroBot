using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Components;
using RSBot.Core.Event;

namespace RSBot.Core.Network.Handler.Agent.Alchemy;

internal class DismantleAckResponseHandler : IPacketHandler
{
    public ushort Opcode => 0xB157;

    public PacketDestination Destination => PacketDestination.Client;

    public void Invoke(Packet packet)
    {
        var result = packet.ReadByte();
        var success = result != 2;
        var errorCode = success ? (ushort)0 : packet.ReadUShort();

        EventManager.FireEvent("OnDismantleResult", success, errorCode);

        AlchemyManager.CompleteOperation();

        EventManager.FireEvent("OnAlchemy", AlchemyType.Disjoin);
    }
}
