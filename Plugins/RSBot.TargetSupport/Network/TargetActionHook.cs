using RSBot.Core.Network;

namespace RSBot.TargetSupport.Network;

public class TargetActionHook : IPacketHandler
{
    public ushort Opcode => 0xB070;
    public PacketDestination Destination => PacketDestination.Client;

    public void Invoke(Packet packet)
    {
        try
        {
            var result = packet.ReadByte();
            if (result != 1)
                return;

            var action = RSBot.Core.Objects.Action.DeserializeBegin(packet);
            if (action == null)
                return;

            TargetSupportManager.Instance.OnSkillCastAction(action.ExecutorId, action.TargetId);
        }
        catch
        {
        }
    }
}
