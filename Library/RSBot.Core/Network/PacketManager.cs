using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RSBot.Core.Objects.Inventory;

namespace RSBot.Core.Network;

public class PacketManager
{
    /// <summary>
    ///     <inheritdoc />
    /// </summary>
    private static readonly object _lock = new();

    /// <summary>
    ///     Gets the handlers.
    /// </summary>
    /// <value>
    ///     The handlers.
    /// </value>
    internal static List<IPacketHandler> Handlers;

    /// <summary>
    ///     Gets the hooks.
    /// </summary>
    internal static List<IPacketHook> Hooks;

    /// <summary>
    ///     The callbacks
    /// </summary>
    private static readonly List<AwaitCallback> _callbacks = new();

    /// <summary>
    ///     Registers the handler.
    /// </summary>
    /// <param name="handler">The handler.</param>
    public static void RegisterHandler(IPacketHandler handler)
    {
        if (Handlers == null)
            Handlers = new List<IPacketHandler>();

        Handlers.Add(handler);
    }

    /// <summary>
    ///     Removes the handler.
    /// </summary>
    /// <param name="handler">The handler.</param>
    public static void RemoveHandler(IPacketHandler handler)
    {
        Handlers?.Remove(handler);
    }

    /// <summary>
    ///     Registers the hook.
    /// </summary>
    /// <param name="hook">The hook.</param>
    public static void RegisterHook(IPacketHook hook)
    {
        if (Hooks == null)
            Hooks = new List<IPacketHook>();

        Hooks.Add(hook);
    }

    /// <summary>
    ///     Removes the hook.
    /// </summary>
    /// <param name="hook">The hook.</param>
    public static void RemoveHook(IPacketHook hook)
    {
        Hooks?.Remove(hook);
    }

    /// <summary>
    ///     Calls the specified packet.
    /// </summary>
    /// <param name="packet">The packet.</param>
    /// <param name="destination">The destination.</param>
    internal static void CallHandler(Packet packet, PacketDestination destination)
    {
        if (Handlers == null)
            return;

        foreach (var handler in Handlers.Where(handler =>
                     handler != null && handler.Opcode == packet.Opcode && handler.Destination == destination))
        {
            try
            {
                handler.Invoke(packet);
            }
            catch (Exception ex)
            {
                Log.Debug($"[PacketManager] Suppressed non-critical handler exception in {handler.GetType().Name} for opcode 0x{packet.Opcode:X4}: {ex.Message}");
            }
            packet.SeekRead(0, SeekOrigin.Begin);
        }
    }

    /// <summary>
    ///     Calls the registered hooks and returns a replaced packet.
    /// </summary>
    /// <param name="packet">The packet.</param>
    /// <param name="destination">The destination.</param>
    /// <returns></returns>
    internal static Packet CallHook(Packet packet, PacketDestination destination)
    {
        var hooks = Hooks?.Where(hook =>
            packet != null && hook.Opcode == packet.Opcode && hook.Destination == destination);
        foreach (var hook in hooks)
            packet = hook.ReplacePacket(packet);

        return packet;
    }

    /// <summary>
    ///     Calls the callback.
    /// </summary>
    /// <param name="packet">The packet.</param>
    internal static void CallCallback(Packet packet)
    {
        lock (_lock)
        {
            var tempCallbacks = _callbacks.Where(c => c.ResponseOpcode == packet.Opcode);

            foreach (var callback in tempCallbacks)
            {
                packet.SeekRead(0, SeekOrigin.Begin);
                callback.Invoke(packet);
            }

            _callbacks.RemoveAll(c => c.IsClosed);
        }
    }

    /// <summary>
    ///     Sends the packet.
    /// </summary>
    /// <param name="packet">The packet.</param>
    /// <param name="destination">The destination.</param>
    public static void SendPacket(Packet packet, PacketDestination destination)
    {
        SendPacket(packet, destination, true);
    }

    /// <summary>
    ///     Sends a packet that was received by the proxy and is forwarded to the
    ///     other side. The packet was already recorded by the packet logger, so it
    ///     is not recorded a second time here.
    /// </summary>
    internal static void SendForwardedPacket(Packet packet, PacketDestination destination)
    {
        SendPacket(packet, destination, false, true);
    }

    /// <summary>
    ///     Sends the packet.
    /// </summary>
    /// <param name="packet">The packet.</param>
    /// <param name="destination">The destination.</param>
    /// <param name="logPacket">
    ///     When <c>true</c> the packet is remembered for the crash dump.
    /// </param>
    /// <param name="isForwarded">
    ///     <c>true</c> when the packet comes from the game client / server and is
    ///     only proxied, <c>false</c> for packets the bot creates itself.
    /// </param>
    private static void SendPacket(Packet packet, PacketDestination destination, bool logPacket,
        bool isForwarded = false)
    {
        if (Kernel.Proxy == null)
            return;

        if (!packet.Locked)
            packet.Lock();

        if (logPacket)
        {
            try
            {
                PacketLogger.RememberSentPacket(packet, destination);
                PacketLogger.LogBotPacket(packet, destination);
            }
            catch { }
        }

        try
        {
            switch (destination)
            {
                case PacketDestination.Client:
                    if (!Game.Clientless)
                    {
                        // When the bot opened the NPC dialog / buys specialty trade goods itself,
                        // do not forward the NPC dialogs, shop windows, trade scale updates, or
                        // cargo transaction responses to sro_client. The game client never initiated
                        // these transactions, so forwarding them causes sro_client.exe to crash
                        // with an access violation (0xC0000005) trying to update uninitialized UI structures.
                        if (IsBotDrivenTrade)
                        {
                            if (packet.Opcode == 0xB045 || // NPC dialog menu (silenced so client UI doesn't pop up)
                                packet.Opcode == 0xB046 || // NPC shop window
                                packet.Opcode == 0xB04B || // NPC close dialog
                                packet.Opcode == 0x34D5 || // Specialty trade notification (causes 0xC0000005 crash in sro_client)
                                packet.Opcode == 0xB034)   // Raw buy response (silenced so client doesn't deselect or error out)
                            {
                                break;
                            }
                        }

                        Kernel.Proxy.Client?.Send(packet);
                    }
                    break;

                case PacketDestination.Server:
                    // Only suppress the close-dialog packet (0x704B) that the game client
                    // sends on its own. sro_client deselects the NPC automatically because
                    // it did not initiate the dialog, and forwarding that would close the
                    // shop in the middle of a bot transaction.
                    //
                    // Packets created by the bot itself MUST always reach the server:
                    // dropping the bot's own 0x704B leaves the shop open on the server,
                    // and the server refuses to move the player while a dialog is open,
                    // so the bot appears stuck at the NPC forever.
                    if (isForwarded && IsBotDrivenTrade && packet.Opcode == 0x704B)
                    {
                        break;
                    }

                    Kernel.Proxy.Server?.Send(packet);
                    break;
            }
        }
        catch (Exception e)
        {
            Log.Fatal(e);
        }
    }

    /// <summary>
    ///     <c>true</c> when the bot opened the NPC dialog / buys goods itself. The
    ///     game client does not know about these dialogs (it never sent the select
    ///     or talk requests) so it can not handle the packets that belong to them.
    /// </summary>
    private static bool IsBotDrivenTrade =>
        Components.ShoppingManager.BotOpenedDialog;

    /// <summary>
    ///     Sends the packet.
    /// </summary>
    /// <param name="packet">The packet.</param>
    /// <param name="destination">The destination.</param>
    /// <param name="callback">The callback.</param>
    public static void SendPacket(Packet packet, PacketDestination destination, params AwaitCallback[] callbacks)
    {
        if (Kernel.Proxy == null)
            return;

        lock (_lock)
        {
            _callbacks.AddRange(callbacks);
        }

        SendPacket(packet, destination);
    }

    /// <summary>
    /// Clears any callbacks registered for a specific opcode or closed callbacks.
    /// </summary>
    public static void ClearCallbacks(ushort opcode)
    {
        lock (_lock)
        {
            _callbacks.RemoveAll(c => c.ResponseOpcode == opcode || c.IsClosed);
        }
    }

    /// <summary>
    ///     Gets the handlers by the specified opcode. If none specified, all handlers will be returned.
    /// </summary>
    /// <param name="opcode">The opcode.</param>
    /// <returns></returns>
    public static List<IPacketHandler> GetHandlers(ushort? opcode = null)
    {
        if (opcode == null)
            return Handlers;

        return Handlers.Where(h => h.Opcode == opcode).ToList();
    }

    /// <summary>
    ///     Gets the hooks by the specified opcode. If none specified, all hooks will be returned.
    /// </summary>
    /// <param name="opcode">The opcode.</param>
    /// <returns></returns>
    public static List<IPacketHook> GetHooks(ushort? opcode = null)
    {
        return opcode == null ? Hooks : Hooks.Where(h => h.Opcode == opcode).ToList();
    }
}