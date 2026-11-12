using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Network;
using View = RSBot.Chat.Views.View;

namespace RSBot.Chat.Network;

internal class AgentNotifyResponse : IPacketHandler
{
    /// <summary>
    ///     Gets or sets the opcode.
    /// </summary>
    /// <value>
    ///     The opcode.
    /// </value>
    public ushort Opcode => 0x300C;

    /// <summary>
    ///     Gets or sets the destination.
    /// </summary>
    /// <value>
    ///     The destination.
    /// </value>
    public PacketDestination Destination => PacketDestination.Client;

    /// <summary>
    ///     Handles the packet.
    /// </summary>
    /// <param name="packet">The packet.</param>
    public void Invoke(Packet packet)
    {
        if (packet == null || packet.Remaining < 1)
            return;

        var noticeType = packet.ReadByte();
        if (Game.ClientType > GameClientType.Thailand && packet.Remaining >= 1)
            packet.ReadByte();

        var characterData = Game.ReferenceManager?.CharacterData;
        if (characterData == null)
            return;

        switch (noticeType)
        {
            //[%s] has appeared
            case 0x5:
            {
                if (packet.Remaining < 4)
                    return;

                var refObjId = packet.ReadUInt();
                if (!characterData.TryGetValue(refObjId, out var obj))
                    return;

                View.Instance.AppendUniqueNotice(LanguageManager.GetLang("UniqueAppeared", obj.GetRealName()));
                break;
            }

            //[%s] has disappeared or killed
            case 0x6:
            {
                if (packet.Remaining < 4)
                    return;

                var refObjId = packet.ReadUInt();
                if (!characterData.TryGetValue(refObjId, out var obj))
                    return;

                var characterName = Bundle.Chat.ReadSenderName(packet);

                if (characterName == "???")
                {
                    View.Instance.AppendUniqueNotice($"{obj.GetRealName()} has disappeared.");
                    return;
                }

                View.Instance.AppendUniqueNotice(
                    LanguageManager.GetLang("UniqueKilled", characterName, obj.GetRealName()));
                break;
            }
        }
    }
}
