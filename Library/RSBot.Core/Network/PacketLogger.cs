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
    private static readonly string _sharedLogPath = @"E:\WolvesBot\packets.log";
    private static bool _initialized;

    /// <summary>
    ///     Number of packets that are kept in memory so a crash dump can be written.
    /// </summary>
    private const int MaxRememberedPackets = 4000;

    private static readonly object _bufferLock = new();
    private static readonly Queue<string> _rememberedPackets = new();

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
                             $"=== AeroBot Packet Logger (Click / Dialog / Inventory): {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\r\n" +
                             $"================================================================================\r\n\r\n";

                File.WriteAllText(LocalLogPath, header, Encoding.UTF8);

                if (Directory.Exists(@"E:\WolvesBot"))
                {
                    File.WriteAllText(_sharedLogPath, header, Encoding.UTF8);
                }

                _initialized = true;
            }
            catch { }
        }
    }

    /// <summary>
    /// Checks if this packet is strictly a click / interaction / dialog packet.
    /// </summary>
    private static bool IsClickPacket(ushort opcode, out string description)
    {
        switch (opcode)
        {
            case 0x7045:
                description = "Click / Select NPC (0x7045)";
                return true;
            case 0xB045:
                description = "Select NPC Response (0xB045)";
                return true;
            case 0x7046:
                description = "Click / Talk Option (0x7046)";
                return true;
            case 0xB046:
                description = "Talk Option Response (0xB046)";
                return true;
            case 0x30D4:
                description = "Click / Dialog Button Action (0x30D4)";
                return true;
            case 0x70D4:
                description = "Click / Dialog Button Action (0x70D4)";
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
                description = "Click / Close Dialog (0x704B)";
                return true;
            case 0xB04B:
                description = "Close Dialog Response (0xB04B)";
                return true;
            case 0x7034:
                description = "Inventory Operation Request (0x7034)";
                return true;
            case 0xB034:
                description = "Inventory Operation Response (0xB034)";
                return true;
            case 0x30E8:
                description = "Job Trade Scale Update (0x30E8)";
                return true;
            default:
                description = null;
                return false;
        }
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
    ///     Writes the last packets that went through the proxy to a file. This is
    ///     called when the game client exits so the packets that caused a crash are
    ///     preserved even when they are not part of the filtered packet log.
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
        if (packet == null) return;

        // Keep every packet in memory for the crash dump, even the ones that are
        // not written to the filtered text log below.
        RememberPacket(packet, destination);

        // STRICT FILTER: Only log click / dialogue / interaction packets!
        if (!IsClickPacket(packet.Opcode, out var desc))
            return;

        EnsureInitialized();

        try
        {
            var bytes = packet.GetBytes() ?? Array.Empty<byte>();
            var isClientToServer = destination == PacketDestination.Server;
            var dirStr = isClientToServer ? "Client -> Server" : "Server -> Client";
            var shortDir = isClientToServer ? "C->S" : "S->C";
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
                try
                {
                    if (Directory.Exists(@"E:\WolvesBot") && !string.Equals(LocalLogPath, _sharedLogPath, StringComparison.OrdinalIgnoreCase))
                    {
                        File.AppendAllText(_sharedLogPath, logEntry, Encoding.UTF8);
                    }
                }
                catch { }
            }

            // Output clean highlight in UI log - suppressed completely as requested by user
            // Log.Warn($"[ضغط] [{shortDir}] 0x{packet.Opcode:X4} ({desc}) | Len: {bytes.Length} | Hex: {hexBytes}");
        }
        catch { }
    }
}
