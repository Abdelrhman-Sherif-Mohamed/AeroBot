using RSBot.Core.Network;

namespace RSBot.Stall.Network;

public class StallCreateResponse : IPacketHandler
{
    public ushort Opcode => 0xB0B1;

    public PacketDestination Destination => PacketDestination.Client;

    public void Invoke(Packet packet)
    {
        if (packet.ReadBool())
            StallManager.Instance.HandleStallCreated();
    }
}
