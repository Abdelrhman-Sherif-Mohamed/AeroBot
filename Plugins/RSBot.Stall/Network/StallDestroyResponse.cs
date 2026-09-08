using RSBot.Core.Network;

namespace RSBot.Stall.Network;

public class StallDestroyResponse : IPacketHandler
{
    public ushort Opcode => 0xB0B2;

    public PacketDestination Destination => PacketDestination.Client;

    public void Invoke(Packet packet)
    {
        if (packet.ReadBool())
            StallManager.Instance.HandleStallDestroyed();
    }
}
