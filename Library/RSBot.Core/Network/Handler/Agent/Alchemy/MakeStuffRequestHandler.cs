using System.Collections.Generic;
using RSBot.Core.Components;
using RSBot.Core.Objects;

namespace RSBot.Core.Network.Handler.Agent.Alchemy;

internal class MakeStuffRequestHandler : IPacketHandler
{
    public ushort Opcode => 0x7155;

    public PacketDestination Destination => PacketDestination.Server;

    public void Invoke(Packet packet)
    {
        var action = packet.ReadByte();

        if (action != 2)
            return;

        packet.ReadByte(); // selector (resolve material / compound)
        var count = packet.ReadByte();

        var items = new List<InventoryItem>(count);

        for (var i = 0; i < count; i++)
        {
            var item = Game.Player.Inventory.GetItemAt(packet.ReadByte());

            if (item != null)
                items.Add(item);
        }

        if (items.Count > 0)
            AlchemyManager.TrackMakeStuffRequest(items);
    }
}
