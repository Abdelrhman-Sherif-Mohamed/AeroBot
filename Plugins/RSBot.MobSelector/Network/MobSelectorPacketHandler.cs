using RSBot.Core.Network;

namespace RSBot.MobSelector.Network;

public class MobSelectorPacketHandler : IPacketHandler
{
    public ushort Opcode => 0xB045;
    public PacketDestination Destination => PacketDestination.Client;

    public void Invoke(Packet packet)
    {
        try
        {
            MobSelectorManager.Instance.HandleTargetResponse(packet);
        }
        catch
        {
        }
    }
}
