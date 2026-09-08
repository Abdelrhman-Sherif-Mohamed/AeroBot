using RSBot.CommandCenter.Components;
using RSBot.CommandCenter.Components.Command;
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
        if (!PluginConfig.Enabled)
            return packet;

        var type = (ChatType)packet.ReadByte();
        if (type == ChatType.Private)
            return packet;

        packet.ReadByte(); // chatIndex

        if (Game.ClientType > GameClientType.Vietnam)
            packet.ReadByte(); // has linking

        if (Game.ClientType >= GameClientType.Chinese)
            packet.ReadByte();

        var message = packet.ReadConditonalString();
        if (string.IsNullOrWhiteSpace(message))
            return packet;

        var trimmed = message.Trim();

        // Support both legacy "\" prefix and natural "!" prefix (e.g. "!start", "\start")
        var isLegacyCommand = trimmed.StartsWith("\\");
        var isBangCommand = trimmed.StartsWith("!");
        if (!isLegacyCommand && !isBangCommand)
            return packet;

        var content = trimmed.Substring(1).Trim();
        if (string.IsNullOrEmpty(content))
            return isLegacyCommand ? null : packet;

        var parts = content.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return isLegacyCommand ? null : packet;

        var commandName = parts[0].ToLowerInvariant();

        // Only swallow "!" messages when they match a real bot command,
        // otherwise let normal chat like "!hello guys" pass through.
        var executor = CommandManager.GetExecutor(commandName);
        if (executor == null)
            return isLegacyCommand ? null : packet;

        InGameCommandContext.Args = parts.Length > 1 ? parts[1..] : System.Array.Empty<string>();

        CommandManager.Execute(commandName);

        // Block the command text so it never appears in game chat
        return null;
    }
}