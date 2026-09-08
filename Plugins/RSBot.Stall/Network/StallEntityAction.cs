using RSBot.Core.Network;
using RSBot.Core.Objects;
using RSBot.Stall.Objects;

namespace RSBot.Stall.Network;

public class StallEntityAction : IPacketHandler
{
    public ushort Opcode => 0x30B7;

    public PacketDestination Destination => PacketDestination.Client;

    public void Invoke(Packet packet)
    {
        var action = packet.ReadByte();
        switch (action)
        {
            case 1: // viewer left
            {
                StallManager.Instance.HandleViewer(false);
                break;
            }
            case 2: // viewer entered
            {
                StallManager.Instance.HandleViewer(true);
                break;
            }
            case 3: // item bought
            {
                var slotBought = packet.ReadByte();
                var buyerName = packet.ReadString();

                var updatedSlots = new StallItem[10];
                byte slotStall;
                while ((slotStall = packet.ReadByte()) != byte.MaxValue)
                {
                    var item = InventoryItem.FromPacket(packet, slotStall);
                    var slotInventory = packet.ReadByte();
                    var quantity = packet.ReadUShort();
                    var price = packet.ReadULong();

                    if (item != null)
                    {
                        item.Slot = slotInventory;
                        item.Amount = quantity;
                    }

                    if (slotStall < 10)
                    {
                        updatedSlots[slotStall] = new StallItem
                        {
                            SlotStall = slotStall,
                            SlotInventory = slotInventory,
                            Item = item,
                            Quantity = quantity,
                            Price = price
                        };
                    }
                }

                StallManager.Instance.HandleItemBought(slotBought, buyerName, updatedSlots);
                break;
            }
        }
    }
}
