using System;
using System.Collections.Generic;
using RSBot.Core.Network;
using RSBot.Stall.Objects;

namespace RSBot.Stall;

public class StallManager
{
    private static readonly object _lock = new();

    public static StallManager Instance { get; } = new();

    public bool IsCreated { get; internal set; }
    public bool IsOpen { get; internal set; }
    public string Title { get; internal set; } = "SonicBot Shop";
    public string Note { get; internal set; } = "Welcome!";
    public StallItem[] Slots { get; } = new StallItem[10];
    public List<string> ActivityLogs { get; } = new();

    public event Action OnStateChanged;
    public event Action OnItemsChanged;
    public event Action<string> OnLogAdded;

    public void CreateStall(string title, string note)
    {
        Title = title;
        Note = note;

        var pCreate = new Packet(0x70B1);
        pCreate.WriteString(title);
        PacketManager.SendPacket(pCreate, PacketDestination.Server);

        var pNote = new Packet(0x70BA);
        pNote.WriteByte((byte)StallUpdateType.Note);
        pNote.WriteString(note);
        PacketManager.SendPacket(pNote, PacketDestination.Server);
    }

    public void CloseStall()
    {
        var pClose = new Packet(0x70B2);
        PacketManager.SendPacket(pClose, PacketDestination.Server);
    }

    public void SetStallState(bool open)
    {
        var p = new Packet(0x70BA);
        p.WriteByte((byte)StallUpdateType.State);
        p.WriteByte((byte)(open ? 1 : 0));
        p.WriteShort(0);
        PacketManager.SendPacket(p, PacketDestination.Server);
    }

    public void UpdateTitle(string title)
    {
        Title = title;
        var p = new Packet(0x70BA);
        p.WriteByte((byte)StallUpdateType.Title);
        p.WriteString(title);
        PacketManager.SendPacket(p, PacketDestination.Server);
    }

    public void UpdateNote(string note)
    {
        Note = note;
        var p = new Packet(0x70BA);
        p.WriteByte((byte)StallUpdateType.Note);
        p.WriteString(note);
        PacketManager.SendPacket(p, PacketDestination.Server);
    }

    public void AddItem(byte slotStall, byte slotInventory, ushort quantity, ulong price)
    {
        var p = new Packet(0x70BA);
        p.WriteByte((byte)StallUpdateType.ItemAdded);
        p.WriteByte(slotStall);
        p.WriteByte(slotInventory);
        p.WriteUShort(quantity);
        p.WriteULong(price);
        p.WriteUInt(1); // FleaMarketNetworkTidGroup
        p.WriteUShort(0);
        PacketManager.SendPacket(p, PacketDestination.Server);
    }

    public void RemoveItem(byte slotStall)
    {
        var p = new Packet(0x70BA);
        p.WriteByte((byte)StallUpdateType.ItemRemoved);
        p.WriteByte(slotStall);
        p.WriteUShort(0);
        PacketManager.SendPacket(p, PacketDestination.Server);
    }

    public void EditItem(byte slotStall, ushort quantity, ulong price)
    {
        var p = new Packet(0x70BA);
        p.WriteByte((byte)StallUpdateType.ItemUpdate);
        p.WriteByte(slotStall);
        p.WriteUShort(quantity);
        p.WriteULong(price);
        p.WriteUShort(0);
        PacketManager.SendPacket(p, PacketDestination.Server);
    }

    public void HandleStallCreated()
    {
        lock (_lock)
        {
            IsCreated = true;
            IsOpen = false;
        }

        AddLog("Stall created successfully.");
        OnStateChanged?.Invoke();
    }

    public void HandleStallDestroyed()
    {
        lock (_lock)
        {
            IsCreated = false;
            IsOpen = false;
            for (var i = 0; i < Slots.Length; i++)
                Slots[i] = null;
        }

        AddLog("Stall closed.");
        OnStateChanged?.Invoke();
        OnItemsChanged?.Invoke();
    }

    public void HandleStallStateUpdate(bool open)
    {
        lock (_lock)
        {
            IsOpen = open;
        }

        AddLog(open ? "Stall is now OPEN for business." : "Stall is now in MODIFY mode.");
        OnStateChanged?.Invoke();
    }

    public void HandleStallTitleUpdate(string title)
    {
        lock (_lock)
        {
            Title = title;
        }

        AddLog($"Stall title updated to: {title}");
        OnStateChanged?.Invoke();
    }

    public void HandleStallNoteUpdate(string note)
    {
        lock (_lock)
        {
            Note = note;
        }

        AddLog($"Stall note updated to: {note}");
        OnStateChanged?.Invoke();
    }

    public void HandleItemUpdate(byte slotStall, ushort quantity, ulong price)
    {
        lock (_lock)
        {
            if (slotStall < Slots.Length && Slots[slotStall] != null)
            {
                Slots[slotStall].Quantity = quantity;
                Slots[slotStall].Price = price;
                if (Slots[slotStall].Item != null)
                    Slots[slotStall].Item.Amount = quantity;
            }
        }

        AddLog($"Stall slot {slotStall + 1} updated: {quantity}x for {price:N0} Gold.");
        OnItemsChanged?.Invoke();
    }

    public void HandleStallInventoryUpdate(StallItem[] newSlots)
    {
        lock (_lock)
        {
            for (var i = 0; i < 10; i++)
            {
                Slots[i] = i < newSlots.Length ? newSlots[i] : null;
            }
        }

        OnItemsChanged?.Invoke();
    }

    public void HandleViewer(bool entered)
    {
        AddLog(entered ? "A buyer opened your stall to inspect items." : "A buyer closed your stall.");
    }

    public void HandleItemBought(byte slotBought, string buyerName, StallItem[] updatedSlots)
    {
        string itemName = "Item";
        if (slotBought < Slots.Length && Slots[slotBought] != null)
        {
            itemName = Slots[slotBought].Item?.Record?.GetRealName() ?? "Item";
            if (Slots[slotBought].Quantity > 1)
                itemName += $" (x{Slots[slotBought].Quantity})";
        }

        AddLog($"[SALE] {buyerName} bought {itemName} from slot {slotBought + 1}!");

        HandleStallInventoryUpdate(updatedSlots);
    }

    public void AddLog(string message)
    {
        var log = $"[{DateTime.Now:HH:mm:ss}] {message}";
        lock (_lock)
        {
            ActivityLogs.Add(log);
            if (ActivityLogs.Count > 200)
                ActivityLogs.RemoveAt(0);
        }

        OnLogAdded?.Invoke(log);
    }

    public void ClearLogs()
    {
        lock (_lock)
        {
            ActivityLogs.Clear();
        }
    }
}
