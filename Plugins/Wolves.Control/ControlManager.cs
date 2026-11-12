using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Event;
using RSBot.Core.Extensions;
using RSBot.Core.Network;
using RSBot.Core.Objects;
using RSBot.Core.Objects.Inventory;
using RSBot.Core.Objects.Spawn;

namespace Wolves.Control;

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
        Enabled = PlayerConfig.Get("Wolves.Control.Enabled", PlayerConfig.Get("SonicBot.Control.Enabled", true));
        Leaders.Clear();
        var saved = PlayerConfig.Get("Wolves.Control.Leaders", PlayerConfig.Get("SonicBot.Control.Leaders", string.Empty));
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
        PlayerConfig.Set("Wolves.Control.Enabled", Enabled);
        PlayerConfig.Set("Wolves.Control.Leaders", string.Join(",", Leaders));
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

        bool isAuthorized = IsLeader(sender) ||
            (Game.Party?.Leader != null && string.Equals(Game.Party.Leader.Name, sender, StringComparison.OrdinalIgnoreCase));

        if (!isAuthorized)
            return;

        var trimmed = rawMessage.Trim();
        if (trimmed.StartsWith("formasyon,", StringComparison.OrdinalIgnoreCase))
        {
            var sub = trimmed.Substring(10).Trim();
            trimmed = "FORMASYON_" + sub.ToUpperInvariant();
        }
        else if (trimmed.Equals("benim gibi davran", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = "MIMIC";
        }
        else if (trimmed.Equals("benim gibi davranma", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = "NOMIMIC";
        }

        var parts = trimmed.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return;

        var cmd = parts[0].ToUpperInvariant();
        if (cmd.StartsWith("!") || cmd.StartsWith("/"))
            cmd = cmd.Substring(1);

        Log($"Command from {sender}: {trimmed}");

        try
        {
            switch (cmd)
            {
                case "S":
                case "START":
                    Kernel.Bot.Start();
                    Log("Bot started");
                    break;

                case "SS":
                case "STOP":
                    Kernel.Bot.Stop();
                    ChoreographyManager.Instance.StopFormation();
                    Log("Bot stopped");
                    break;

                case "T":
                case "TRACE":
                    var traceTarget = parts.Length > 1 ? parts[1] : sender;
                    StartTrace(traceTarget);
                    break;

                case "N":
                case "NOTRACE":
                    StopTrace();
                    break;

                case "F":
                case "FOLLOW":
                    var followTarget = parts.Length > 1 && !float.TryParse(parts[1], out _) ? parts[1] : sender;
                    var followDist = 5.0f;
                    if (parts.Length > 2 && float.TryParse(parts[2], out var d2))
                        followDist = d2;
                    else if (parts.Length > 1 && float.TryParse(parts[1], out var d1))
                        followDist = d1;
                    StartFollow(followTarget, followDist);
                    break;

                case "NF":
                case "NOFOLLOW":
                    StopFollow();
                    break;

                case "RE":
                case "RETURN":
                    UseReturnScroll();
                    break;

                case "Z":
                case "ZERK":
                    UseBerserk();
                    break;

                case "G":
                case "GETOUT":
                    LeaveParty();
                    break;

                case "M":
                case "MOUNT":
                    MountVehicle();
                    break;

                case "D":
                case "DS":
                case "DISMOUNT":
                    DismountVehicle();
                    break;

                case "STATUS":
                case "HP":
                case "MP":
                    SendStatusResponse(sender, chatType);
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

                case "SETSCRIPT":
                    if (parts.Length > 1)
                    {
                        var scriptPath = string.Join(" ", parts.Skip(1));
                        SetScript(scriptPath);
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

                case "EQUIP":
                    if (parts.Length > 1)
                        EquipItem(string.Join(" ", parts.Skip(1)));
                    break;

                case "UNEQUIP":
                    if (parts.Length > 1)
                        UnequipItem(string.Join(" ", parts.Skip(1)));
                    break;

                case "TERMINATE":
                    TerminateTransports();
                    break;

                case "SORT":
                    SortInventory();
                    break;

                case "RECALL":
                    if (parts.Length > 1)
                        DesignateRecall(string.Join(" ", parts.Skip(1)));
                    break;

                case "TP":
                    if (parts.Length > 1)
                        TeleportToDestination(string.Join(" ", parts.Skip(1)));
                    break;

                case "R":
                    if (parts.Length > 1)
                        UseReverseScroll("player", string.Join(" ", parts.Skip(1)));
                    break;

                case "REVERSE":
                    if (parts.Length > 1)
                        UseReverseScroll(parts[1], parts.Length > 2 ? string.Join(" ", parts.Skip(2)) : string.Empty);
                    break;

                case "CHAT":
                    if (parts.Length >= 3)
                        ExecuteChatCommand(parts.Skip(1).ToArray());
                    break;

                case "INJECT":
                    if (parts.Length > 1)
                        InjectRawPacket(parts.Skip(1).ToArray());
                    break;

                // Target Support Integration
                case "TARGET":
                    if (parts.Length > 1 && parts[1].Equals("ON", StringComparison.OrdinalIgnoreCase))
                    {
                        TargetSupportManager.Instance.Enabled = true;
                        TargetSupportManager.Instance.SaveConfig();
                        TargetSupportManager.Instance.NotifyState();
                        Log("Target synchronization ENABLED");
                    }
                    else if (parts.Length > 1 && parts[1].Equals("OFF", StringComparison.OrdinalIgnoreCase))
                    {
                        TargetSupportManager.Instance.Enabled = false;
                        TargetSupportManager.Instance.SaveConfig();
                        TargetSupportManager.Instance.NotifyState();
                        Log("Target synchronization DISABLED");
                    }
                    break;

                // Choreography & Formations (الحركات الاستعراضية)
                case "ORBIT":
                    float? orbR = parts.Length > 1 && float.TryParse(parts[1], out var or1) ? or1 : null;
                    ChoreographyManager.Instance.StartFormation(FormationType.Orbit, sender, orbR);
                    break;

                case "COUNTER":
                case "COUNTERORBIT":
                    float? cntR = parts.Length > 1 && float.TryParse(parts[1], out var cr1) ? cr1 : null;
                    ChoreographyManager.Instance.StartFormation(FormationType.CounterOrbit, sender, cntR);
                    break;

                case "LINE":
                    float? lnR = parts.Length > 1 && float.TryParse(parts[1], out var lr1) ? lr1 : null;
                    ChoreographyManager.Instance.StartFormation(FormationType.Line, sender, lnR);
                    break;

                case "V":
                case "WEDGE":
                    float? wdgR = parts.Length > 1 && float.TryParse(parts[1], out var wr1) ? wr1 : null;
                    ChoreographyManager.Instance.StartFormation(FormationType.Wedge, sender, wdgR);
                    break;

                case "CIRCLE":
                case "RING":
                    float? crcR = parts.Length > 1 && float.TryParse(parts[1], out var cir1) ? cir1 : null;
                    ChoreographyManager.Instance.StartFormation(FormationType.Circle, sender, crcR);
                    break;

                case "BOX":
                case "SQUARE":
                    float? bxR = parts.Length > 1 && float.TryParse(parts[1], out var br1) ? br1 : null;
                    ChoreographyManager.Instance.StartFormation(FormationType.Square, sender, bxR);
                    break;

                case "COLUMN":
                case "MARCH":
                    float? clmR = parts.Length > 1 && float.TryParse(parts[1], out var clr1) ? clr1 : null;
                    ChoreographyManager.Instance.StartFormation(FormationType.Column, sender, clmR);
                    break;

                case "PULSE":
                    float? plsR = parts.Length > 1 && float.TryParse(parts[1], out var pr1) ? pr1 : null;
                    ChoreographyManager.Instance.StartFormation(FormationType.Pulse, sender, plsR);
                    break;

                // Agnotobot Formations & Actions
                case "FORMASYON_ICICE":
                case "FORMASYON_IC_ICE":
                case "ICICE":
                case "STACK":
                case "GHOST":
                    ChoreographyManager.Instance.StartFormation(FormationType.Stack, sender, 0f);
                    break;

                case "FORMASYON_NORMAL":
                case "NORMAL":
                    ChoreographyManager.Instance.StartFormation(FormationType.Column, sender, 5f);
                    break;

                case "FORMASYON_RASTGELE":
                case "RASTGELE":
                case "SCATTER":
                    float? sctR = parts.Length > 1 && float.TryParse(parts[1], out var sr1) ? sr1 : null;
                    ChoreographyManager.Instance.StartFormation(FormationType.Scatter, sender, sctR);
                    break;

                case "FORMASYON_RASTGELEX":
                case "RASTGELEX":
                case "SCATTERX":
                    float? sxR = parts.Length > 1 && float.TryParse(parts[1], out var sx1) ? sx1 : null;
                    ChoreographyManager.Instance.StartFormation(FormationType.ScatterX, sender, sxR);
                    break;

                case "FORMASYON_RASTGELEY":
                case "RASTGELEY":
                case "SCATTERY":
                    float? syR = parts.Length > 1 && float.TryParse(parts[1], out var sy1) ? sy1 : null;
                    ChoreographyManager.Instance.StartFormation(FormationType.ScatterY, sender, syR);
                    break;

                case "TRIANGLE":
                case "TRI":
                    float? triR = parts.Length > 1 && float.TryParse(parts[1], out var tr1) ? tr1 : null;
                    ChoreographyManager.Instance.StartFormation(FormationType.Triangle, sender, triR);
                    break;

                case "CROSS":
                case "PLUS":
                    float? crsR = parts.Length > 1 && float.TryParse(parts[1], out var c1) ? c1 : null;
                    ChoreographyManager.Instance.StartFormation(FormationType.Cross, sender, crsR);
                    break;

                case "DIAMOND":
                    float? dmdR = parts.Length > 1 && float.TryParse(parts[1], out var dm1) ? dm1 : null;
                    ChoreographyManager.Instance.StartFormation(FormationType.Diamond, sender, dmdR);
                    break;

                case "GEL":
                case "COME":
                    ChoreographyManager.Instance.ComeToLeader();
                    break;

                case "TEKRARLA":
                case "MIMIC":
                    ChoreographyManager.Instance.MimicLeader = true;
                    Log("تم تفعيل محاكاة تصرفات القائد (Mimic Leader Active)");
                    break;

                case "NOTEKRARLA":
                case "NOMIMIC":
                    ChoreographyManager.Instance.MimicLeader = false;
                    Log("تم إيقاف محاكاة تصرفات القائد (Mimic Leader Inactive)");
                    break;

                case "OTUR":
                case "KALK":
                    SitOrStand();
                    break;

                case "SELAMLA":
                    ChoreographyManager.Instance.PerformSyncEmote(EmoteActionType.Cheer);
                    break;

                case "AFDILE":
                    ChoreographyManager.Instance.PerformSyncEmote(EmoteActionType.Bow);
                    break;

                case "DANS":
                case "DANCE":
                    ChoreographyManager.Instance.PerformSyncEmote(EmoteActionType.Dance);
                    break;

                case "A":
                    if (parts.Length > 1 && byte.TryParse(parts[1], out var aId))
                    {
                        var emote = aId switch
                        {
                            1 => EmoteActionType.Bow,
                            2 => EmoteActionType.Cheer,
                            3 => EmoteActionType.Clap,
                            4 => EmoteActionType.SitStand,
                            5 => EmoteActionType.Cry,
                            6 => EmoteActionType.Anger,
                            7 => EmoteActionType.Dance,
                            _ => EmoteActionType.Cheer
                        };
                        ChoreographyManager.Instance.PerformSyncEmote(emote);
                    }
                    break;

                case "WAVE":
                    ChoreographyManager.Instance.PerformEmoteWave(EmoteActionType.Cheer);
                    break;

                case "CHEER":
                    ChoreographyManager.Instance.PerformSyncEmote(EmoteActionType.Cheer);
                    break;

                case "BOW":
                    ChoreographyManager.Instance.PerformSyncEmote(EmoteActionType.Bow);
                    break;

                case "CLAP":
                    ChoreographyManager.Instance.PerformSyncEmote(EmoteActionType.Clap);
                    break;

                case "DUR":
                case "HALT":
                case "STOPFORMATION":
                case "NOFORMATION":
                    ChoreographyManager.Instance.StopFormation();
                    break;

                case "DIS":
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

    private void MountVehicle()
    {
        if (Game.Player == null) return;
        if (!Game.Player.HasActiveVehicle)
        {
            Game.Player.SummonVehicle();
            Log("Summoned/Mounted vehicle");
        }
    }

    private void DismountVehicle()
    {
        if (Game.Player == null) return;
        if (Game.Player.HasActiveVehicle && Game.Player.Vehicle != null)
        {
            Game.Player.Vehicle.Dismount();
            Log("Dismounted vehicle");
        }
    }

    private System.Threading.Timer _followTimer;
    private string _followTarget;
    private float _followDist = 5.0f;

    private void StartFollow(string targetName, float distance)
    {
        _followTarget = string.IsNullOrWhiteSpace(targetName) ? Leaders.FirstOrDefault() : targetName;
        _followDist = Math.Max(2.0f, distance);
        _followTimer?.Dispose();
        _followTimer = new System.Threading.Timer(FollowTick, null, 0, 1000);
        Log($"Following {_followTarget} (Distance: {_followDist:F0}m)");
    }

    private void StopFollow()
    {
        _followTimer?.Dispose();
        _followTimer = null;
        _followTarget = null;
        Log("Follow stopped");
    }

    private void FollowTick(object state)
    {
        try
        {
            if (string.IsNullOrEmpty(_followTarget) || Game.Player == null) return;
            if (SpawnManager.TryGetEntity<SpawnedPlayer>(p => string.Equals(p.Name, _followTarget, StringComparison.OrdinalIgnoreCase), out var target))
            {
                var dist = Game.Player.Position.DistanceTo(target.Position);
                if (dist > _followDist + 2.0f)
                {
                    Game.Player.MoveTo(target.Position, false);
                }
            }
        }
        catch { }
    }

    private void SendStatusResponse(string receiver, ChatType chatType)
    {
        if (Game.Player == null) return;
        var hpPct = Game.Player.MaximumHealth > 0 ? (int)(Game.Player.Health * 100 / Game.Player.MaximumHealth) : 0;
        var mpPct = Game.Player.MaximumMana > 0 ? (int)(Game.Player.Mana * 100 / Game.Player.MaximumMana) : 0;
        var botState = Kernel.Bot != null && Kernel.Bot.Running ? "Running" : "Stopped";
        var msg = $"Status: {botState} | HP:{hpPct}% MP:{mpPct}% | Lv.{Game.Player.Level} | Gold:{Game.Player.Gold:N0}";

        SendChatPacket(chatType == ChatType.Private ? ChatType.Private : ChatType.Party, msg, receiver);
    }

    private static void SendChatPacket(ChatType chatType, string msg, string receiver = null)
    {
        if (string.IsNullOrWhiteSpace(msg))
            return;

        var chatPacket = new Packet(0x7025);
        chatPacket.WriteByte(chatType);
        chatPacket.WriteByte(1);
        if (Game.ClientType > GameClientType.Vietnam) chatPacket.WriteByte(0);
        if (Game.ClientType >= GameClientType.Chinese) chatPacket.WriteByte(0);
        if (chatType == ChatType.Private)
            chatPacket.WriteString(receiver ?? string.Empty);
        chatPacket.WriteConditonalString(msg);
        PacketManager.SendPacket(chatPacket, PacketDestination.Server);
    }

    private void EquipItem(string keyword)
    {
        if (Game.Player?.Inventory == null || string.IsNullOrWhiteSpace(keyword))
            return;

        var item = FindMatchingItem(keyword, equippedOnly: false);
        if (item == null)
        {
            Log($"EQUIP: no matching item in inventory for \"{keyword}\"");
            return;
        }

        var targetSlot = ResolveEquipSlot(item);
        if (targetSlot == byte.MaxValue)
        {
            Log($"EQUIP: \"{item.Record?.GetRealName()}\" cannot be equipped to a standard equipment slot.");
            return;
        }

        Task.Run(() =>
        {
            var moved = Game.Player.Inventory.MoveItem(item.Slot, targetSlot);
            Log(moved
                ? $"Equipped [{item.Record.GetRealName()}] to slot {targetSlot}"
                : $"EQUIP failed for [{item.Record.GetRealName()}]");
        });
    }

    private void UnequipItem(string keyword)
    {
        if (Game.Player?.Inventory == null || string.IsNullOrWhiteSpace(keyword))
            return;

        var item = FindMatchingItem(keyword, equippedOnly: true);
        if (item == null)
        {
            Log($"UNEQUIP: no matching equipped item for \"{keyword}\"");
            return;
        }

        var freeSlot = Game.Player.Inventory.GetFreeSlot();
        if (freeSlot == 0)
        {
            Log("UNEQUIP: inventory is full.");
            return;
        }

        Task.Run(() =>
        {
            var moved = Game.Player.Inventory.MoveItem(item.Slot, freeSlot);
            Log(moved
                ? $"Unequipped [{item.Record.GetRealName()}] to slot {freeSlot}"
                : $"UNEQUIP failed for [{item.Record.GetRealName()}]");
        });
    }

    private static InventoryItem FindMatchingItem(string keyword, bool equippedOnly)
    {
        var query = keyword.Trim();
        var jobToken = query.ToLowerInvariant() switch
        {
            "trader" or "trade" => "TRADER",
            "hunter" => "HUNTER",
            "thief" => "THIEF",
            "job" => "_JOB_",
            _ => null
        };

        var items = equippedOnly
            ? Game.Player.Inventory.GetEquippedPartItems()
            : Game.Player.Inventory.GetNormalPartItems();

        return items.FirstOrDefault(i =>
        {
            if (i?.Record == null)
                return false;

            if (jobToken != null)
                return (i.Record.CodeName ?? string.Empty).IndexOf(jobToken, StringComparison.OrdinalIgnoreCase) >= 0;

            var realName = i.Record.GetRealName() ?? string.Empty;
            var codeName = i.Record.CodeName ?? string.Empty;
            return realName.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0 ||
                   string.Equals(codeName, query, StringComparison.OrdinalIgnoreCase);
        });
    }

    private static byte ResolveEquipSlot(InventoryItem item)
    {
        var rec = item?.Record;
        if (rec == null || !rec.IsEquip || rec.IsAvatar)
            return byte.MaxValue;

        if (rec.IsArmor)
        {
            return rec.TypeID4 switch
            {
                1 => 0, // Head
                2 => 2, // Shoulders
                3 => 1, // Chest
                4 => 4, // Legs
                5 => 3, // Gloves
                6 => 5, // Boots
                _ => byte.MaxValue
            };
        }

        if (rec.IsShield) return 7;
        if (rec.IsWeapon) return 6;
        if (rec.IsJobOutfit) return 8;

        if (rec.IsAccessory)
        {
            return rec.TypeID4 switch
            {
                1 => 9,  // Earring
                2 => 10, // Necklace
                3 => Game.Player.Inventory.GetItemAt(11) == null ? (byte)11 : (byte)12, // Ring
                _ => byte.MaxValue
            };
        }

        return byte.MaxValue;
    }

    private void TerminateTransports()
    {
        if (Game.Player == null)
            return;

        var transports = new RSBot.Core.Objects.Cos.Cos[] { Game.Player.Transport, Game.Player.JobTransport }
            .Where(c => c != null && c.UniqueId != 0)
            .ToArray();

        if (transports.Length == 0)
        {
            Log("No active transport or trade pet to terminate.");
            return;
        }

        Task.Run(async () =>
        {
            foreach (var cos in transports)
            {
                if (Game.Player.HasActiveVehicle && Game.Player.Vehicle?.UniqueId == cos.UniqueId)
                {
                    cos.Dismount();
                    await Task.Delay(600);
                }

                cos.Terminate();
            }

            Log($"Terminated {transports.Length} transport pet(s).");
        });
    }

    private void SortInventory()
    {
        if (Game.Player?.Inventory == null)
            return;

        Task.Run(() =>
        {
            Game.Player.Inventory.Sort();
            Log("Inventory sorted.");
        });
    }

    private void DesignateRecall(string npcName)
    {
        if (string.IsNullOrWhiteSpace(npcName))
            return;

        var npc = SpawnManager.GetEntity<SpawnedNpcNpc>(n =>
            n?.Record != null &&
            (string.Equals(n.Record.GetRealName(), npcName, StringComparison.OrdinalIgnoreCase) ||
             string.Equals(n.Record.CodeName, npcName, StringComparison.OrdinalIgnoreCase) ||
             (n.Record.GetRealName()?.IndexOf(npcName, StringComparison.OrdinalIgnoreCase) >= 0)));

        if (npc == null)
        {
            Log($"RECALL: NPC \"{npcName}\" not found nearby.");
            return;
        }

        var packet = new Packet(0x7059);
        packet.WriteUInt(npc.UniqueId);
        PacketManager.SendPacket(packet, PacketDestination.Server);
        Log($"Designating recall point to [{npc.Record.GetRealName()}].");
    }

    private void TeleportToDestination(string rawArgs)
    {
        if (Game.ReferenceManager?.TeleportData == null || string.IsNullOrWhiteSpace(rawArgs))
            return;

        var separator = rawArgs.Contains(',') ? new[] { ',' } : new[] { ' ' };
        var args = rawArgs.Split(separator, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToArray();
        if (args.Length == 0)
            return;

        var sourceFilter = args.Length >= 2 ? args[0] : null;
        var destFilter = args.Length >= 2 ? string.Join(" ", args.Skip(1)) : args[0];

        if (!SpawnManager.TryGetEntities<SpawnedNpcNpc>(n => n?.Record != null, out var nearbyNpcs))
            return;

        foreach (var npc in nearbyNpcs)
        {
            var srcTp = Game.ReferenceManager.TeleportData.FirstOrDefault(t => t.AssocRefObjId == npc.Record.ID);
            if (srcTp == null)
                continue;

            if (!string.IsNullOrWhiteSpace(sourceFilter))
            {
                var npcName = npc.Record.GetRealName() ?? string.Empty;
                if (npcName.IndexOf(sourceFilter, StringComparison.OrdinalIgnoreCase) < 0 &&
                    !string.Equals(npc.Record.CodeName, sourceFilter, StringComparison.OrdinalIgnoreCase))
                    continue;
            }

            var matchingLink = srcTp.GetLinks().FirstOrDefault(l =>
            {
                var target = l?.Target;
                if (target == null) return false;
                var zone = target.ZoneName128 ?? string.Empty;
                var code = target.CodeName ?? string.Empty;
                return zone.IndexOf(destFilter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                       code.IndexOf(destFilter, StringComparison.OrdinalIgnoreCase) >= 0;
            });

            if (matchingLink == null)
                continue;

            Task.Run(async () =>
            {
                if (Game.Player?.HasActiveVehicle == true)
                {
                    Game.Player.Vehicle?.Dismount();
                    await Task.Delay(500);
                }

                npc.TrySelect();
                await Task.Delay(250);

                var packet = new Packet(0x705A);
                packet.WriteUInt(npc.UniqueId);
                packet.WriteByte(TeleportType.Destination);
                packet.WriteUInt((uint)matchingLink.TargetTeleport);
                PacketManager.SendPacket(packet, PacketDestination.Server);

                Log($"Teleporting from [{npc.Record.GetRealName()}] to [{matchingLink.Target?.CodeName ?? matchingLink.TargetTeleport.ToString()}].");
            });
            return;
        }

        Log($"TP: no nearby teleporter offers destination \"{destFilter}\".");
    }

    private void UseReverseScroll(string mode, string targetArg)
    {
        if (Game.Player?.Inventory == null || string.IsNullOrWhiteSpace(mode))
            return;

        var reverseScroll = Game.Player.Inventory.GetNormalPartItems(i =>
            i?.Record?.CodeName != null &&
            i.Record.CodeName.IndexOf("SCROLL_REVERSE", StringComparison.OrdinalIgnoreCase) >= 0)
            .FirstOrDefault();

        if (reverseScroll == null)
        {
            Log("REVERSE: no Reverse Return Scroll found in inventory.");
            return;
        }

        var normalizedMode = mode.Trim().ToLowerInvariant();
        if (normalizedMode is "return" or "recall")
        {
            SendReversePacket(reverseScroll, (byte)GuideTeleportType.Recall);
            Log("Using Reverse Return Scroll to last recall point.");
            return;
        }

        if (normalizedMode == "death")
        {
            SendReversePacket(reverseScroll, (byte)GuideTeleportType.Death);
            Log("Using Reverse Return Scroll to last death location.");
            return;
        }

        if (normalizedMode == "zone" && !string.IsNullOrWhiteSpace(targetArg))
        {
            var tp = Game.ReferenceManager?.TeleportData?.FirstOrDefault(t =>
                (t.ZoneName128?.IndexOf(targetArg, StringComparison.OrdinalIgnoreCase) >= 0) ||
                (t.CodeName?.IndexOf(targetArg, StringComparison.OrdinalIgnoreCase) >= 0));

            if (tp == null)
            {
                Log($"REVERSE zone: destination \"{targetArg}\" not found.");
                return;
            }

            reverseScroll.UseTo(0x07, (int)tp.ID);
            Log($"Using Reverse Return Scroll to zone [{tp.CodeName}].");
            return;
        }

        Log($"REVERSE: unsupported mode \"{mode}\" (supported: return, death, zone <name>).");
    }

    private static void SendReversePacket(InventoryItem reverseScroll, byte guideType)
    {
        var packet = new Packet(0x704C);
        packet.WriteByte(reverseScroll.Slot);
        if (Game.ClientType > GameClientType.Vietnam)
            packet.WriteInt(reverseScroll.Record.Tid);
        else
            packet.WriteUShort(reverseScroll.Record.Tid);
        packet.WriteByte(guideType);
        PacketManager.SendPacket(packet, PacketDestination.Server);
    }

    private void ExecuteChatCommand(string[] args)
    {
        if (args.Length < 2)
            return;

        var channel = args[0].Trim().ToLowerInvariant();
        switch (channel)
        {
            case "all":
                SendChatPacket(ChatType.All, string.Join(" ", args.Skip(1)));
                break;
            case "party":
                SendChatPacket(ChatType.Party, string.Join(" ", args.Skip(1)));
                break;
            case "guild":
                SendChatPacket(ChatType.Guild, string.Join(" ", args.Skip(1)));
                break;
            case "union":
                SendChatPacket(ChatType.Union, string.Join(" ", args.Skip(1)));
                break;
            case "academy":
                SendChatPacket(ChatType.Academy, string.Join(" ", args.Skip(1)));
                break;
            case "global":
                SendChatPacket(ChatType.Global, string.Join(" ", args.Skip(1)));
                break;
            case "private" when args.Length >= 3:
                SendChatPacket(ChatType.Private, string.Join(" ", args.Skip(2)), args[1]);
                break;
            default:
                Log($"CHAT: unsupported channel \"{channel}\"");
                break;
        }
    }

    private void InjectRawPacket(string[] args)
    {
        try
        {
            if (args.Length == 0)
                return;

            var opcodeText = args[0].StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? args[0].Substring(2)
                : args[0];
            var opcode = ushort.Parse(opcodeText, System.Globalization.NumberStyles.HexNumber);

            var dataStart = 1;
            var encrypted = false;
            if (args.Length >= 2 && bool.TryParse(args[1], out var parsedEnc))
            {
                encrypted = parsedEnc;
                dataStart = 2;
            }

            var packet = new Packet(opcode, encrypted);
            for (var i = dataStart; i < args.Length; i++)
            {
                var hex = args[i].StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                    ? args[i].Substring(2)
                    : args[i];
                packet.WriteByte(byte.Parse(hex, System.Globalization.NumberStyles.HexNumber));
            }

            PacketManager.SendPacket(packet, PacketDestination.Server);
            Log($"Injected packet 0x{opcode:X4} (encrypted={encrypted}, bytes={Math.Max(0, args.Length - dataStart)})");
        }
        catch (Exception ex)
        {
            Log($"INJECT failed: {ex.Message}");
        }
    }

    private void SetScript(string scriptPath)
    {
        if (string.IsNullOrWhiteSpace(scriptPath)) return;
        var fullPath = Path.IsPathRooted(scriptPath)
            ? scriptPath
            : Path.Combine(Kernel.BasePath, "Data", "Scripts", scriptPath);

        if (File.Exists(fullPath))
        {
            PlayerConfig.Set("RSBot.Default.ScriptPath", fullPath);
            Log($"Loaded script: {Path.GetFileName(fullPath)}");
        }
        else
        {
            Log($"Script file not found: {scriptPath}");
        }
    }
}
