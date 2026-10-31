using System;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Extensions;
using RSBot.Core.Network;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Spawn;

namespace Wolves.Control.Network;

public class ControlChatHandler : IPacketHandler
{
    public ushort Opcode => 0x3026;
    public PacketDestination Destination => PacketDestination.Client;

    public void Invoke(Packet packet)
    {
        try
        {
            var type = (ChatType)packet.ReadByte();
            string sender = string.Empty;
            string message = string.Empty;

            switch (type)
            {
                case ChatType.All:
                case ChatType.AllGM:
                    var senderId = packet.ReadUInt();
                    message = packet.ReadConditonalString();
                    if (senderId == Game.Player?.UniqueId)
                        sender = Game.Player?.Name ?? string.Empty;
                    else if (SpawnManager.TryGetEntity<SpawnedPlayer>(senderId, out var sp))
                        sender = sp.Name;
                    break;

                case ChatType.Notice:
                case ChatType.Npc:
                    return;

                default:
                    sender = packet.ReadString();
                    message = packet.ReadConditonalString();
                    break;
            }

            if (string.IsNullOrWhiteSpace(message) || string.IsNullOrWhiteSpace(sender))
                return;

            // Execute control command
            ControlManager.Instance.ExecuteCommand(sender, message, type);

            // Execute target support command (like TARGET ON / TARGET OFF)
            TargetSupportManager.Instance.HandleChatCommand(sender, message);
        }
        catch (Exception ex)
        {
            Log.Debug($"[ControlChatHandler] Error parsing chat: {ex.Message}");
        }
    }
}
