using System;
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
                             $"=== AeroBot Packet Logger (Click / Dialog Only): {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===\r\n" +
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
            default:
                description = null;
                return false;
        }
    }

    public static void LogPacket(Packet packet, PacketDestination destination)
    {
        if (packet == null) return;

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
