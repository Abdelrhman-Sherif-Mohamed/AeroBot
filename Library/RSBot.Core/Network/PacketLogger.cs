using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using RSBot.Core.Extensions;

namespace RSBot.Core.Network;

public static class PacketLogger
{
    private static readonly object _fileLock = new();
    private static string _localLogPath;
    private static bool _initialized;

    /// <summary>
    ///     Number of packets that are kept in memory so a crash dump can be written.
    /// </summary>
    private const int MaxRememberedPackets = 4000;

    private static readonly object _bufferLock = new();
    private static readonly Queue<string> _rememberedPackets = new();

    /// <summary>
    ///     Packet logging to the UI log and to packets.log is a developer tool only.
    ///     It is disabled by default so normal users never see packet dumps.
    ///     Enable it with "RSBot.Log.PacketLogger{True}" in the global profile config.
    /// </summary>
    public static bool PacketLoggingEnabled
    {
        get
        {
            try
            {
                return GlobalConfig.Get("RSBot.Log.PacketLogger", false);
            }
            catch
            {
                return false;
            }
        }
    }

    public static string LocalLogPath
    {
        get
        {
            if (_localLogPath == null)
                _localLogPath = Path.Combine(Kernel.BasePath, "packets.log");
            return _localLogPath;
        }
    }

    private static void EnsureInitialized()
    {
        if (_initialized) return;
        lock (_fileLock)
        {
            if (_initialized) return;
            try
            {
                var header = $"\r\n================================================================================\r\n" +
                             $"=== AeroBot Packet Logger: {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\r\n" +
                             $"================================================================================\r\n\r\n";

                File.WriteAllText(LocalLogPath, header, Encoding.UTF8);

                _initialized = true;
            }
            catch { }
        }
    }

    /// <summary>
    /// Checks if this packet should be logged and surfaced to UI / disk.
    /// </summary>
    private static bool ShouldLog(ushort opcode, PacketDestination destination, bool isBotSent, out string description)
    {
        switch (opcode)
        {
            case 0x7045:
                description = "Select Target (0x7045)";
                return true;
            case 0xB045:
                description = "Select Target Response (0xB045)";
                return true;
            case 0x7046:
                description = "Talk Option (0x7046)";
                return true;
            case 0xB046:
                description = "Talk Option Response (0xB046)";
                return true;
            case 0x30D4:
            case 0x70D4:
                description = "Dialog Action (0x70D4)";
                return true;
            case 0x30D5:
                description = "Dialog Action Response (0x30D5)";
                return true;
            case 0x30D2:
            case 0x70D2:
            case 0x30D3:
            case 0x70D3:
                description = $"Dialog Event State (0x{opcode:X4})";
                return true;
            case 0x704B:
                description = "Close Dialog (0x704B)";
                return true;
            case 0xB04B:
                description = "Close Dialog Response (0xB04B)";
                return true;
            case 0x7034:
                description = "Inventory/Trade Operation Request (0x7034)";
                return true;
            case 0xB034:
                description = "Inventory/Trade Operation Response (0xB034)";
                return true;
            case 0x30E8:
                description = "Job Trade Scale Update (0x30E8)";
                return true;
            case 0x34D5:
                description = "Specialty Trade Notification (0x34D5)";
                return true;
            case 0x30E7:
            case 0x70E7:
                description = $"Specialty Shop Info (0x{opcode:X4})";
                return true;
            case 0x7162:
            case 0xB162:
                description = $"COS Action (0x{opcode:X4})";
                return true;
            case 0x705A:
            case 0xB05A:
                description = $"Job Action (0x{opcode:X4})";
                return true;
            case 0x3040:
                description = "Inventory Item Update (0x3040)";
                return true;
            case 0x3049:
                description = "COS Inventory Item Update (0x3049)";
                return true;
            case 0x303D:
                description = "Gold Update (0x303D)";
                return true;
            case 0x3252:
                description = "Trade State (0x3252)";
                return true;
        }

        // If sent by client to server, catch all actions except movement and keepalive
        if (destination == PacketDestination.Server && !isBotSent)
        {
            if (opcode != 0x7021 && // Movement
                opcode != 0x2002 && // Ping
                opcode != 0x5000 && // Security / Keepalive
                opcode != 0x9000 &&
                opcode != 0x7005 &&
                opcode != 0x7001)   // Handshake
            {
                description = $"Client Action (0x{opcode:X4})";
                return true;
            }
        }

        description = null;
        return false;
    }

    /// <summary>
    ///     Keeps a packet in the in-memory ring buffer that is written to disk when
    ///     the game client crashes or disconnects.
    /// </summary>
    private static void Remember(ushort opcode, string direction, byte[] bytes)
    {
        try
        {
            var hex = bytes == null || bytes.Length == 0
                ? string.Empty
                : BitConverter.ToString(bytes).Replace("-", " ");

            if (hex.Length > 2048)
                hex = hex.Substring(0, 2048) + " ...(truncated)";

            var entry = $"[{DateTime.Now:HH:mm:ss.fff}] [{direction}] 0x{opcode:X4} | Len: {bytes?.Length ?? 0}\r\nHex: {hex}";

            lock (_bufferLock)
            {
                _rememberedPackets.Enqueue(entry);

                while (_rememberedPackets.Count > MaxRememberedPackets)
                    _rememberedPackets.Dequeue();
            }
        }
        catch { }
    }

    /// <summary>
    ///     Remembers a packet that was received from the client or the server.
    /// </summary>
    public static void RememberPacket(Packet packet, PacketDestination destination)
    {
        if (packet == null) return;

        Remember(packet.Opcode,
            destination == PacketDestination.Server ? "Client -> Server" : "Server -> Client",
            packet.GetBytes());
    }

    /// <summary>
    ///     Remembers a packet that was sent by the bot itself (not a forwarded packet).
    /// </summary>
    public static void RememberSentPacket(Packet packet, PacketDestination destination)
    {
        if (packet == null) return;

        Remember(packet.Opcode,
            destination == PacketDestination.Server ? "Bot -> Server" : "Bot -> Client",
            packet.GetBytes());
    }

    /// <summary>
    ///     Writes the last packets that went through the proxy to a file.
    /// </summary>
    public static void DumpRecent(string reason)
    {
        try
        {
            string[] snapshot;
            lock (_bufferLock)
            {
                snapshot = _rememberedPackets.ToArray();
            }

            if (snapshot.Length == 0)
                return;

            var directory = Path.Combine(Kernel.BasePath, "Data", "Logs", "Packets");
            Directory.CreateDirectory(directory);

            var path = Path.Combine(directory, $"dump-{DateTime.Now:yyyyMMdd-HHmmss}.log");

            var sb = new StringBuilder();
            sb.AppendLine("================================================================================");
            sb.AppendLine($"=== AeroBot packet dump: {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
            sb.AppendLine($"=== Reason: {reason} | Entries: {snapshot.Length} ===");
            sb.AppendLine("================================================================================");
            sb.AppendLine();

            foreach (var entry in snapshot)
                sb.AppendLine(entry);

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);

            // Keep the log directory small: only the last five dumps are useful.
            try
            {
                var dumps = Directory.GetFiles(directory, "dump-*.log");
                Array.Sort(dumps, StringComparer.OrdinalIgnoreCase);

                for (var i = 0; i < dumps.Length - 5; i++)
                    File.Delete(dumps[i]);
            }
            catch { }

            Log.Warn($"[PacketLogger] Saved packet dump: {path}");
        }
        catch { }
    }

    public static void LogPacket(Packet packet, PacketDestination destination)
    {
        LogPacketInternal(packet, destination, isBotSent: false);
    }

    public static void LogBotPacket(Packet packet, PacketDestination destination)
    {
        LogPacketInternal(packet, destination, isBotSent: true);
    }

    private static void LogPacketInternal(Packet packet, PacketDestination destination, bool isBotSent)
    {
        if (packet == null) return;

        // The in-memory ring buffer is always filled: it is only written to disk when
        // the game client really crashes / disconnects, so it never shows anywhere.
        if (!isBotSent)
            RememberPacket(packet, destination);

        // No packet output to the UI log and no packets.log file for normal users.
        if (!PacketLoggingEnabled)
            return;

        if (!ShouldLog(packet.Opcode, destination, isBotSent, out var desc))
            return;

        EnsureInitialized();

        try
        {
            var bytes = packet.GetBytes() ?? Array.Empty<byte>();
            var dirStr = isBotSent ? "Bot -> Server" : (destination == PacketDestination.Server ? "Client -> Server" : "Server -> Client");
            var shortDir = isBotSent ? "Bot->S" : (destination == PacketDestination.Server ? "C->S" : "S->C");
            var hexBytes = BitConverter.ToString(bytes).Replace("-", " ");
            var timeStr = DateTime.Now.ToString("HH:mm:ss.fff");

            // Write detailed packet dump to file(s)
            var sb = new StringBuilder();
            sb.AppendLine($"[{timeStr}] [{dirStr}] [{desc}] Opcode: 0x{packet.Opcode:X4} | Length: {bytes.Length}");
            if (bytes.Length > 0)
            {
                sb.AppendLine($"Hex: {hexBytes}");
                try
                {
                    sb.AppendLine("Dump:\r\n" + bytes.HexDump());
                }
                catch { }
            }
            sb.AppendLine(new string('-', 70));

            var logEntry = sb.ToString();

            lock (_fileLock)
            {
                try { File.AppendAllText(LocalLogPath, logEntry, Encoding.UTF8); } catch { }
            }

            // Output clean highlight in UI log
            var displayHex = hexBytes.Length > 90 ? hexBytes.Substring(0, 90) + "..." : hexBytes;
            Log.Warn($"[Packet] [{shortDir}] 0x{packet.Opcode:X4} ({desc}) | Hex: {displayHex}");
        }
        catch { }
    }
}