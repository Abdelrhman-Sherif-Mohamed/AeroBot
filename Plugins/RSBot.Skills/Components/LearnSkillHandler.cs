using RSBot.Core.Network;

namespace RSBot.Skills.Components;

internal class LearnSkillHandler
{
    public static bool LearnSkill(uint skillId)
    {
        var packet = new Packet(0x70A1);
        packet.WriteUInt(skillId);

        var callback = new AwaitCallback(response =>
            response.ReadByte() == 0x01 ? AwaitCallbackResult.Success : AwaitCallbackResult.Fail, 0xB0A1);

        PacketManager.SendPacket(packet, PacketDestination.Server, callback);
        callback.AwaitResponse(3000);

        return callback.IsCompleted;
    }
}
