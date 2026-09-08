using System;
using System.Collections.Generic;
using System.Linq;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Extensions;
using RSBot.Core.Network;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Spawn;

namespace RSBot.Control;

public class ControlManager
{
    private static ControlManager _instance;
    public static ControlManager Instance => _instance ??= new ControlManager();

    public bool Enabled { get; set; } = true;
    public List<string> Leaders { get; } = new();
    public List<string> Logs { get; } = new();

    public event System.Action OnStateChanged;
    public event System.Action<string> OnLogAdded;

    private ControlManager()
    {
        LoadConfig();
    }

    public void LoadConfig()
    {
        Enabled = PlayerConfig.Get("SonicBot.Control.Enabled", true);
        Leaders.Clear();
        var saved = PlayerConfig.Get("SonicBot.Control.Leaders", string.Empty);
        if (!string.IsNullOrWhiteSpace(saved))
        {
            var names = saved.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var n in names)
            {
                var trimmed = n.Trim();
                if (!string.IsNullOrEmpty(trimmed) && !Leaders.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
                    Leaders.Add(trimmed);
            }
        }
    }

    public void SaveConfig()
    {
        PlayerConfig.Set("SonicBot.Control.Enabled", Enabled);
        PlayerConfig.Set("SonicBot.Control.Leaders", string.Join(",", Leaders));
    }

    public void AddLeader(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        var trimmed = name.Trim();
        if (!Leaders.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
        {
            Leaders.Add(trimmed);
            SaveConfig();
            OnStateChanged?.Invoke();
        }
    }

    public void RemoveLeader(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        var trimmed = name.Trim();
        var found = Leaders.FirstOrDefault(l => string.Equals(l, trimmed, StringComparison.OrdinalIgnoreCase));
        if (found != null)
        {
            Leaders.Remove(found);
            SaveConfig();
            OnStateChanged?.Invoke();
        }
    }

    public bool IsLeader(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        return Leaders.Any(l => string.Equals(l, name.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public void Log(string msg)
    {
        var entry = $"[{DateTime.Now:HH:mm:ss}] {msg}";
        Logs.Add(entry);
        if (Logs.Count > 100)
            Logs.RemoveAt(0);

        OnLogAdded?.Invoke(entry);
    }

    public void ExecuteCommand(string sender, string rawMessage, ChatType chatType)
    {
        if (!Enabled || string.IsNullOrWhiteSpace(rawMessage) || string.IsNullOrWhiteSpace(sender))
            return;

        if (!IsLeader(sender))
            return;

        var trimmed = rawMessage.Trim();
        var parts = trimmed.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return;

        var cmd = parts[0].ToUpperInvariant();
        Log($"Command from {sender}: {trimmed}");

        try
        {
            switch (cmd)
            {
                case "START":
                    Kernel.Bot.Start();
                    Log("Bot started");
                    break;

                case "STOP":
                    Kernel.Bot.Stop();
                    Log("Bot stopped");
                    break;

                case "TRACE":
                    var traceTarget = parts.Length > 1 ? parts[1] : sender;
                    StartTrace(traceTarget);
                    break;

                case "NOTRACE":
                    StopTrace();
                    break;

                case "RETURN":
                    UseReturnScroll();
                    break;

                case "ZERK":
                    UseBerserk();
                    break;

                case "GETOUT":
                    LeaveParty();
                    break;

                case "MOVEON":
                    var radius = parts.Length > 1 && float.TryParse(parts[1], out var r) ? r : 15f;
                    MoveRandom(radius);
                    break;

                case "SETPOS":
                    SetPosition(parts);
                    break;

                case "SETRADIUS":
                    if (parts.Length > 1 && int.TryParse(parts[1], out var rad))
                    {
                        var absRad = Math.Abs(rad);
                        PlayerConfig.Set("RSBot.Area.Radius", absRad);
                        Log($"Training radius set to {absRad}m");
                    }
                    break;

                case "GETPOS":
                    SendPositionResponse(sender, chatType);
                    break;

                case "SIT":
                    SitOrStand();
                    break;

                case "CAPE":
                    var color = parts.Length > 1 ? parts[1].ToLowerInvariant() : "yellow";
                    SetCape(color);
                    break;

                case "USE":
                    if (parts.Length > 1)
                    {
                        var itemName = string.Join(" ", parts.Skip(1));
                        UseItem(itemName);
                    }
                    break;

                case "DC":
                    Log("Disconnecting client...");
                    Kernel.Proxy?.Server?.Disconnect();
                    break;
            }
        }
        catch (Exception ex)
        {
            Log($"Error executing command {cmd}: {ex.Message}");
        }
    }

    private void StartTrace(string playerName)
    {
        if (SpawnManager.TryGetEntity<SpawnedPlayer>(p => string.Equals(p.Name, playerName, StringComparison.OrdinalIgnoreCase), out var target))
        {
            var packet = new Packet(0x7074);
            packet.WriteByte(1); // Execute
            packet.WriteByte(3); // Trace
            packet.WriteByte(1); // Target type: entity
            packet.WriteUInt(target.UniqueId);
            PacketManager.SendPacket(packet, PacketDestination.Server);
            Log($"Tracing player {playerName} (UID: {target.UniqueId})");
        }
        else
        {
            Log($"Cannot trace {playerName}: Player not found in vicinity");
        }
    }

    private void StopTrace()
    {
        if (Game.Player != null)
        {
            Game.Player.MoveTo(Game.Player.Movement.Source, false);
            Log("Trace stopped");
        }
    }

    private void UseReturnScroll()
    {
        if (Game.Player == null)
            return;

        if (Game.Player.Health == 0)
        {
            // Resurrect at town
            var packet = new Packet(0x3053);
            packet.WriteByte(1);
            PacketManager.SendPacket(packet, PacketDestination.Server);
            Log("Resurrecting at nearest town...");
        }
        else
        {
            Game.Player.UseReturnScroll();
            Log("Using return scroll...");
        }
    }

    private void UseBerserk()
    {
        var packet = new Packet(0x70A7);
        packet.WriteByte(1);
        PacketManager.SendPacket(packet, PacketDestination.Server);
        Log("Activated Berserker mode");
    }

    private void LeaveParty()
    {
        if (Game.Party != null)
        {
            Game.Party.Leave();
            Log("Left the party");
        }
    }

    private void MoveRandom(float radius)
    {
        if (Game.Player == null)
            return;

        var angle = new Random().NextDouble() * Math.PI * 2;
        var p = Game.Player.Movement.Source;
        var targetX = p.X + (float)(Math.Cos(angle) * radius);
        var targetY = p.Y + (float)(Math.Sin(angle) * radius);
        var targetPos = new Position(targetX, targetY, p.Region);

        Game.Player.MoveTo(targetPos, false);
        Log($"Random move step ({radius:0}m)");
    }

    private void SetPosition(string[] parts)
    {
        if (Game.Player == null)
            return;

        if (parts.Length >= 3 && float.TryParse(parts[1], out var x) && float.TryParse(parts[2], out var y))
        {
            var reg = Game.Player.Movement.Source.Region;
            var pos = new Position(x, y, reg);
            PlayerConfig.Set("RSBot.Area.Region", pos.Region);
            PlayerConfig.Set("RSBot.Area.X", pos.XOffset.ToString("0.0"));
            PlayerConfig.Set("RSBot.Area.Y", pos.YOffset.ToString("0.0"));
            Log($"Training position set to X:{x:0.0} Y:{y:0.0}");
        }
        else
        {
            var pos = Game.Player.Movement.Source;
            PlayerConfig.Set("RSBot.Area.Region", pos.Region);
            PlayerConfig.Set("RSBot.Area.X", pos.XOffset.ToString("0.0"));
            PlayerConfig.Set("RSBot.Area.Y", pos.YOffset.ToString("0.0"));
            Log($"Training position set to current position (X:{pos.X:0.0} Y:{pos.Y:0.0})");
        }
    }

    private void SendPositionResponse(string receiver, ChatType chatType)
    {
        if (Game.Player == null)
            return;

        var pos = Game.Player.Movement.Source;
        var msg = $"Pos: X:{pos.X:0.0} Y:{pos.Y:0.0} (Region:{pos.Region})";

        var chatPacket = new Packet(0x7025);
        if (chatType == ChatType.Private)
        {
            chatPacket.WriteByte(ChatType.Private);
            chatPacket.WriteByte(1);
            if (Game.ClientType > GameClientType.Vietnam) chatPacket.WriteByte(0);
            if (Game.ClientType >= GameClientType.Chinese) chatPacket.WriteByte(0);
            chatPacket.WriteString(receiver);
            chatPacket.WriteConditonalString(msg);
        }
        else
        {
            chatPacket.WriteByte(ChatType.Party);
            chatPacket.WriteByte(1);
            if (Game.ClientType > GameClientType.Vietnam) chatPacket.WriteByte(0);
            if (Game.ClientType >= GameClientType.Chinese) chatPacket.WriteByte(0);
            chatPacket.WriteConditonalString(msg);
        }

        PacketManager.SendPacket(chatPacket, PacketDestination.Server);
    }

    private void SitOrStand()
    {
        var packet = new Packet(0x704F);
        packet.WriteByte(4);
        PacketManager.SendPacket(packet, PacketDestination.Server);
        Log("Sit/Stand toggled");
    }

    private void SetCape(string color)
    {
        byte capeCode = color switch
        {
            "off" => 0,
            "red" => 1,
            "gray" => 2,
            "blue" => 3,
            "white" => 4,
            _ => 5 // yellow
        };

        var packet = new Packet(0x7516);
        packet.WriteByte(capeCode);
        PacketManager.SendPacket(packet, PacketDestination.Server);
        Log($"PVP Cape set to {color}");
    }

    private void UseItem(string itemName)
    {
        if (Game.Player?.Inventory == null)
            return;

        var item = Game.Player.Inventory.FirstOrDefault(i =>
            i != null && i.Record != null &&
            (string.Equals(i.Record.GetRealName(), itemName, StringComparison.OrdinalIgnoreCase) ||
             i.Record.GetRealName().Contains(itemName, StringComparison.OrdinalIgnoreCase)));

        if (item != null)
        {
            item.Use();
            Log($"Used item {item.Record.GetRealName()} from slot {item.Slot}");
        }
        else
        {
            Log($"Item not found in inventory: {itemName}");
        }
    }
}
