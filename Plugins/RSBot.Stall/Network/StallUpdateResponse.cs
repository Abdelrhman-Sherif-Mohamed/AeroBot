using System;
using RSBot.Core.Network;
using RSBot.Core.Objects;
using RSBot.Stall.Objects;

namespace RSBot.Stall.Network;

public class StallUpdateResponse : IPacketHandler
{
    public ushort Opcode => 0xB0BA;

    public PacketDestination Destination => PacketDestination.Client;

    public void Invoke(Packet packet)
    {
        if (!packet.ReadBool())
            return;

        var type = (StallUpdateType)packet.ReadByte();
        switch (type)
        {
            case StallUpdateType.ItemUpdate:
            {
                var slotStall = packet.ReadByte();
                var quantity = packet.ReadUShort();
                var price = packet.ReadULong();
                StallManager.Instance.HandleItemUpdate(slotStall, quantity, price);
                break;
            }
            case StallUpdateType.ItemAdded:
            case StallUpdateType.ItemRemoved:
            {
                var errorCode = packet.ReadUShort();
                var slots = new StallItem[10];
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
                        slots[slotStall] = new StallItem
                        {
                            SlotStall = slotStall,
                            SlotInventory = slotInventory,
                            Item = item,
                            Quantity = quantity,
                            Price = price
                        };
                    }
                }
                StallManager.Instance.HandleStallInventoryUpdate(slots);
                break;
            }
            case StallUpdateType.State:
            {
                var open = packet.ReadBool();
                StallManager.Instance.HandleStallStateUpdate(open);
                break;
            }
            case StallUpdateType.Note:
            {
                var note = packet.ReadString();
                StallManager.Instance.HandleStallNoteUpdate(note);
                break;
            }
            case StallUpdateType.Title:
            {
                var title = packet.ReadString();
                StallManager.Instance.HandleStallTitleUpdate(title);
                break;
            }
        }
    }
}
