using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Components;
using RSBot.Core.Event;

namespace RSBot.Core.Network.Handler.Agent.Alchemy;

internal class MakeStuffAckResponseHandler : IPacketHandler
{
    public ushort Opcode => 0xB155;

    public PacketDestination Destination => PacketDestination.Client;

    public void Invoke(Packet packet)
    {
        var result = packet.ReadByte();

        if (result == 2)
        {
            var errorCode = packet.ReadUShort();

            EventManager.FireEvent("OnMakeStuffResult", false, errorCode, (byte)0, (uint)0, true);
            AlchemyManager.CompleteOperation();

            EventManager.FireEvent("OnAlchemy", AlchemyType.Manufacture);

            return;
        }

        var action = packet.ReadByte();

        if (action != 2)
        {
            EventManager.FireEvent("OnMakeStuffCanceled");
            AlchemyManager.CompleteOperation();

            EventManager.FireEvent("OnAlchemy", AlchemyType.Manufacture);

            return;
        }

        var extra = packet.ReadByte();
        var value = packet.ReadUInt();

        var completed = AlchemyManager.RegisterMakeStuffResponse();

        EventManager.FireEvent("OnMakeStuffResult", true, (ushort)0, extra, value, completed);

        if (!completed)
            return;

        AlchemyManager.CompleteOperation();

        EventManager.FireEvent("OnAlchemy", AlchemyType.Manufacture);
    }
}
