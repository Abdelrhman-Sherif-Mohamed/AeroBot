using System;
using System.IO;
using RSBot.CommandCenter.Components;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Extensions;
using RSBot.Core.Network;
using RSBot.Core.Objects;

namespace RSBot.CommandCenter.Network.Hook;

internal class ChatRequestHook : IPacketHook
{
    public ushort Opcode => 0x7025;

    public PacketDestination Destination => PacketDestination.Server;

    public Packet ReplacePacket(Packet packet)
    {
        if (!PluginConfig.Enabled || packet == null || packet.Remaining < 4)
            return packet;

        try
        {
            var type = (ChatType)packet.ReadByte();
            if (type == ChatType.Private)
                return packet;

            if (packet.Remaining < 1)
                return packet;

            packet.ReadByte(); // chatIndex

            if (Game.ClientType > GameClientType.Vietnam && packet.Remaining >= 1)
                packet.ReadByte(); // has linking

            if (Game.ClientType >= GameClientType.Chinese && packet.Remaining >= 1)
                packet.ReadByte();

            if (packet.Remaining < 2)
                return packet;

            var message = packet.ReadConditonalString();
            if (string.IsNullOrEmpty(message) || !message.StartsWith("\\", StringComparison.Ordinal))
                return packet;

            var commandName = message.Substring(1).Trim();
            if (string.IsNullOrEmpty(commandName))
                return packet;

            CommandManager.Execute(commandName);

            return null;
        }
        catch
        {
            if (packet.Locked)
                packet.SeekRead(0, SeekOrigin.Begin);
            return packet;
        }
    }
}
