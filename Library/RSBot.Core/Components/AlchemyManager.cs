using System.Collections.Generic;
using System.Timers;
using RSBot.Core.Client.ReferenceObjects;
using RSBot.Core.Network;
using RSBot.Core.Network.Handler.Agent.Alchemy;
using RSBot.Core.Objects;

namespace RSBot.Core.Components;

/// <summary>
///     Handles the different alchemy operation families supported by the
///     client protocol: reinforcement, make-stuff (manufacture) and dismantle.
///     Socket alchemy is not implemented yet.
/// </summary>
public class AlchemyManager
{
    #region Members

    private static Timer _fusingTimer;

    #endregion Members

    #region Properties

    /// <summary>
    ///     Gets or sets a list of inventory items currently used in an alchemy operation.
    ///     Attention! This object is only set during alchemy operations and will be set to NULL if any ACK response has been
    ///     received!
    /// </summary>
    public static List<InventoryItem>? ActiveAlchemyItems { get; internal set; }

    /// <summary>
    ///     Gets the operation family that is currently waiting for a server response.
    /// </summary>
    public static AlchemyOperation ActiveOperation { get; internal set; }

    /// <summary>
    ///     Gets the number of result packets expected for the current make-stuff request.
    /// </summary>
    public static int ExpectedMakeStuffResponses { get; internal set; }

    /// <summary>
    ///     Gets the number of make-stuff result packets received so far.
    /// </summary>
    public static int CompletedMakeStuffResponses { get; internal set; }

    /// <summary>
    ///     Gets a value indicating whether this instance is fusing.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance is fusing; otherwise, <c>false</c>.
    /// </value>
    public static bool IsFusing { get; internal set; }

    /// <summary>
    ///     Gets a value indicating whether an alchemy operation is currently in progress.
    /// </summary>
    public static bool IsBusy => ActiveOperation != AlchemyOperation.None;

    #endregion Properties

    #region Methods

    /// <summary>
    ///     Cancels the pending alchemy operation.
    /// </summary>
    public static void CancelPending()
    {
        if (ActiveOperation == AlchemyOperation.None)
            return;

        var opcode = ActiveOperation switch
        {
            AlchemyOperation.Fuse => (ushort)0x7150,
            AlchemyOperation.MakeStuff => (ushort)0x7155,
            _ => (ushort)0
        };

        if (opcode != 0)
        {
            var packet = new Packet(opcode);
            packet.WriteByte(0x01);

            packet.Lock();
            Kernel.Proxy.Server.Send(packet);
        }

        CompleteOperation();
    }

    /// <summary>
    ///     Starts a make-stuff operation in "resolve material" mode.
    /// </summary>
    /// <param name="materials">The materials staged for the operation.</param>
    public static bool TryResolveMaterials(IReadOnlyList<InventoryItem> materials)
    {
        return TryStartMakeStuff(1, materials, 4);
    }

    /// <summary>
    ///     Starts a make-stuff operation in "compound" mode.
    /// </summary>
    /// <param name="materials">The material staged for the operation.</param>
    public static bool TryCompoundMaterials(IReadOnlyList<InventoryItem> materials)
    {
        return TryStartMakeStuff(2, materials, 1);
    }

    /// <summary>
    ///     Starts a dismantle (disjoin) operation.
    /// </summary>
    /// <param name="equipment">The equipment staged for dismantling.</param>
    public static bool TryDismantle(IReadOnlyList<InventoryItem> equipment)
    {
        if (!ValidateItems(equipment, 1, 9, "dismantle"))
            return false;

        Log.Notify($"[Alchemy] Dismantling {equipment[0].Record.GetRealName()}");

        var packet = new Packet(0x7157);
        packet.WriteByte((byte)equipment.Count);

        foreach (var item in equipment)
            packet.WriteByte(item.Slot);

        packet.Lock();
        Kernel.Proxy.Server.Send(packet);

        BeginOperation(AlchemyOperation.Dismantle, equipment, 30_000);

        return true;
    }

    /// <summary>
    ///     Registers one make-stuff result and returns true when every expected
    ///     result packet has been received.
    /// </summary>
    internal static bool RegisterMakeStuffResponse()
    {
        if (ActiveOperation != AlchemyOperation.MakeStuff)
            return true;

        CompletedMakeStuffResponses++;

        if (CompletedMakeStuffResponses >= ExpectedMakeStuffResponses)
            return true;

        // Resolving several materials produces one result per component. Reset
        // the timeout after each component so a long recipe is not cancelled.
        StartTimer(30_000);

        return false;
    }

    /// <summary>
    ///     Tracks a make-stuff request that was sent by the game client.
    /// </summary>
    internal static void TrackMakeStuffRequest(IReadOnlyList<InventoryItem> items)
    {
        BeginOperation(AlchemyOperation.MakeStuff, items, 30_000);
    }

    /// <summary>
    ///     Tracks a dismantle request that was sent by the game client.
    /// </summary>
    internal static void TrackDismantleRequest(IReadOnlyList<InventoryItem> items)
    {
        BeginOperation(AlchemyOperation.Dismantle, items, 30_000);
    }

    /// <summary>
    ///     Clears the active operation and releases the pending timer.
    /// </summary>
    internal static void CompleteOperation()
    {
        StopTimer();

        ActiveOperation = AlchemyOperation.None;
        ActiveAlchemyItems = null;
        ExpectedMakeStuffResponses = 0;
        CompletedMakeStuffResponses = 0;
    }

    private static bool TryStartMakeStuff(byte selector, IReadOnlyList<InventoryItem> materials, int maxCount)
    {
        if (!ValidateItems(materials, 1, maxCount, "make-stuff"))
            return false;

        Log.Notify(selector == 1
            ? $"[Alchemy] Resolving {materials.Count} alchemy material(s)"
            : "[Alchemy] Compounding an alchemy material");

        var packet = new Packet(0x7155);
        packet.WriteByte(2); // start
        packet.WriteByte(selector); // 1 = resolve material, 2 = compound
        packet.WriteByte((byte)materials.Count);

        foreach (var material in materials)
            packet.WriteByte(material.Slot);

        packet.Lock();
        Kernel.Proxy.Server.Send(packet);

        BeginOperation(AlchemyOperation.MakeStuff, materials, 30_000);

        return true;
    }

    private static void BeginOperation(AlchemyOperation operation, IReadOnlyList<InventoryItem> items, int timeoutMs)
    {
        ActiveOperation = operation;
        ActiveAlchemyItems = new List<InventoryItem>(items);
        ExpectedMakeStuffResponses = operation == AlchemyOperation.MakeStuff ? items.Count : 0;
        CompletedMakeStuffResponses = 0;

        StartTimer(timeoutMs);
    }

    private static bool ValidateItems(IReadOnlyList<InventoryItem> items, int min, int max, string operation)
    {
        if (items == null || items.Count < min || items.Count > max)
        {
            Log.Warn($"[Alchemy] Invalid {operation} item count.");

            return false;
        }

        var slots = new HashSet<byte>();

        foreach (var item in items)
        {
            if (item == null || !slots.Add(item.Slot))
            {
                Log.Warn($"[Alchemy] Invalid {operation} item selection.");

                return false;
            }

            var current = Game.Player.Inventory.GetItemAt(item.Slot);

            if (current == null || current.ItemId != item.ItemId || current.Amount == 0)
            {
                Log.Warn($"[Alchemy] The {operation} item is no longer in the specified slot.");

                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///     Fuses the elixir with the specified item using the given lucky powder ingredient.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="elixir">The elixir.</param>
    /// <param name="powder">The powder.</param>
    public static bool TryFuseElixir(InventoryItem item, InventoryItem elixir, InventoryItem? powder)
    {
        var itemInInventory = Game.Player.Inventory.GetItemAt(item.Slot);
        var elixirInInventory = Game.Player.Inventory.GetItemAt(elixir.Slot);
        var powderInInventory = Game.Player.Inventory.GetItemAt(powder.Slot);

        if (itemInInventory?.ItemId != item.ItemId ||
            elixirInInventory?.ItemId != elixir.ItemId ||
            (powderInInventory != null && powderInInventory.ItemId != powder.ItemId))
        {
            Log.Warn("[Alchemy] Requested to fuse an item that does not match the current item at the specified slot.");

            return false;
        }

        Log.Notify(powder == null
            ? $"[Alchemy] Fusing elixir {elixir.Record.GetRealName()} to {item.Record.GetRealName()}"
            : $"[Alchemy] Fusing elixir {elixir.Record.GetRealName()} to {item.Record.GetRealName()} using powder {powder.Record.GetRealName()}");

        var packet = new Packet(0x7150);
        packet.WriteByte(AlchemyAction.Fuse); //fuse
        packet.WriteByte(AlchemyType.Elixir); //type (Elixir)
        packet.WriteByte(powder != null ? (byte)3 : (byte)2); //Slot count
        packet.WriteByte(item.Slot);
        packet.WriteByte(elixir.Slot);

        if (powder != null)
            packet.WriteByte(powder.Slot);

        packet.Lock();

        Kernel.Proxy.Server.Send(packet);

        GenericAlchemyRequestHandler.Invoke(packet);

        return true;
    }

    /// <summary>
    ///     Fuses the magic stone with the specified item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="magicStone">The elixir.</param>
    public static bool TryFuseMagicStone(InventoryItem item, InventoryItem magicStone)
    {
        var itemInInventory = Game.Player.Inventory.GetItemAt(item.Slot);
        var stoneInInventory = Game.Player.Inventory.GetItemAt(magicStone.Slot);

        if (itemInInventory?.ItemId != item.ItemId ||
            stoneInInventory?.ItemId != magicStone.ItemId)
        {
            Log.Debug(
                "[Alchemy] Requested to fuse an item that does not match the current item at the specified slot.");

            return false;
        }

        Log.Notify(
            $"[Alchemy] Fusing magic stone {magicStone.Record.GetRealName()} to item {item.Record.GetRealName()}");

        var packet = new Packet(0x7151);

        packet.WriteByte(AlchemyAction.Fuse); //Fuse
        packet.WriteByte(AlchemyType.MagicStone); //MagicStone
        packet.WriteByte(2); //Slot count

        packet.WriteByte(item.Slot);
        packet.WriteByte(magicStone.Slot);

        packet.Lock();

        Kernel.Proxy.Server.Send(packet);

        GenericAlchemyRequestHandler.Invoke(packet);

        return true;
    }

    /// <summary>
    ///     Fuses the attribute stone with the specified item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="attributeStone">The attribute stone.</param>
    public static bool TryFuseAttributeStone(InventoryItem item, InventoryItem attributeStone)
    {
        var itemInInventory = Game.Player.Inventory.GetItemAt(item.Slot);
        var stoneInInventory = Game.Player.Inventory.GetItemAt(attributeStone.Slot);

        if (itemInInventory?.ItemId != item.ItemId ||
            stoneInInventory?.ItemId != attributeStone.ItemId)
        {
            Log.Warn("[Alchemy] Requested to fuse an item that does not match the current item at the specified slot.");

            return false;
        }

        Log.Notify(
            $"[Alchemy] Fusing attribute stone {attributeStone.Record.GetRealName()} to item {item.Record.GetRealName()}");

        var packet = new Packet(0x7151);

        packet.WriteByte(AlchemyAction.Fuse); //Fuse
        packet.WriteByte(AlchemyType.AttributeStone); //MagicStone
        packet.WriteByte(2); //Slot count

        packet.WriteByte(item.Slot);
        packet.WriteByte(attributeStone.Slot);

        packet.Lock();
        Kernel.Proxy.Server.Send(packet);

        GenericAlchemyRequestHandler.Invoke(packet);

        return true;
    }

    /// <summary>
    ///     Starts the alchemy timer.
    /// </summary>
    internal static void StartTimer(int timeoutMs = 10_000)
    {
        _fusingTimer?.Dispose();
        _fusingTimer = null;

        IsFusing = true;

        _fusingTimer = new Timer(timeoutMs)
        {
            AutoReset = false,
            Enabled = false
        };

        _fusingTimer.Elapsed += FusingActionFusingTimerElapsed;
        _fusingTimer.Start();
    }

    /// <summary>
    ///     Stops the alchemy timer.
    /// </summary>
    internal static void StopTimer()
    {
        IsFusing = false;

        _fusingTimer?.Dispose();
        _fusingTimer = null;
    }

    /// <summary>
    ///     Triggered when the alchemy timer elapses. Stops the timer.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="ElapsedEventArgs" /> instance containing the event data.</param>
    private static void FusingActionFusingTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        Log.Warn("[Alchemy] The alchemy operation timed out.");

        CompleteOperation();
    }

    #endregion Methods
}

public enum AlchemyOperation
{
    None,
    Fuse,
    MakeStuff,
    Dismantle
}