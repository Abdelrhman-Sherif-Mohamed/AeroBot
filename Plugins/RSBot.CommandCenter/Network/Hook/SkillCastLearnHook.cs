using System.IO;
using RSBot.CommandCenter.Components.Command;
using RSBot.Core;
using RSBot.Core.Network;
using RSBot.Core.Objects;

namespace RSBot.CommandCenter.Network.Hook;

/// <summary>
///     Watches the player's own skill casts (client -&gt; server 0x7074) so
///     "learn mode" (!learn / !learnbuff) can capture the casted skill.
///     Never blocks or modifies the packet.
/// </summary>
internal class SkillCastLearnHook : IPacketHook
{
    public ushort Opcode => 0x7074;

    public PacketDestination Destination => PacketDestination.Server;

    public Packet ReplacePacket(Packet packet)
    {
        try
        {
            if (!LearnMode.IsArmed)
                return packet;

            // Layout for casts: [Execute=1][Cast=4][skillId:uint]...
            // (Attack=1 and Dispel=5 carry no skill id and are ignored.)
            if (packet.ReadByte() != (byte)ActionCommandType.Execute)
                return packet;

            if (packet.ReadByte() != (byte)ActionType.Cast)
                return packet;

            var skillId = packet.ReadUInt();

            // Hand the id over to learn mode (adds + notifies from there)
            LearnMode.TryCapture(skillId);
        }
        catch
        {
            // Never break the player's cast because of learn mode
        }
        finally
        {
            try
            {
                packet.SeekRead(0, SeekOrigin.Begin);
            }
            catch
            {
            }
        }

        return packet;
    }
}
