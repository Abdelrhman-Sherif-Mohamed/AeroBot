using System.Collections.Generic;
using System.Linq;
using RSBot.Core;
using RSBot.Core.Event;
using RSBot.Core.Network;
using RSBot.General.Models;
using Server = RSBot.General.Models.Server;

namespace RSBot.General.Components;

public static class Serverlist
{
    private enum ServerStatusModern
    {
        Full,
        Crowded,
        Populate,
        Easy,
        Check
    }

    /// <summary>
    ///     Gets or sets the servers.
    /// </summary>
    /// <value>
    ///     The servers.
    /// </value>
    public static List<Server> Servers { get; set; }

    // <summary>
    /// Gets or sets the joining server.
    /// </summary>
    /// <value>
    ///     The server.
    /// </value>
    public static Server Joining { get; set; }

    /// <summary>
    ///     Parses the server list from a 0xA101 Gateway packet, updates <see cref="Servers" />,
    ///     and notifies subscribers via <c>OnServerListUpdated</c>.
    /// </summary>
    public static List<Server> ParseFromPacket(Packet packet, bool logServers = true)
    {
        var parsed = new List<Server>();

        while (packet.ReadByte() != 0)
        {
            packet.ReadByte();
            packet.ReadString();
        }

        while (packet.ReadByte() == 1)
        {
            var id = packet.ReadUShort();
            var serverName = Game.ClientType == GameClientType.Turkey ||
                             Game.ClientType == GameClientType.Global ||
                             Game.ClientType == GameClientType.VTC_Game
                ? packet.ReadUnicode()
                : packet.ReadString();

            ushort currentCapacity = 0,
                maxCapacity = 0;

            byte status;

            if (Game.ClientType < GameClientType.Global)
            {
                currentCapacity = packet.ReadUShort();
                maxCapacity = packet.ReadUShort();
                status = packet.ReadByte();
            }
            else
            {
                status = packet.ReadByte();
                packet.ReadByte();
            }

            if (Game.ClientType == GameClientType.Global && serverName.Length > 0)
                serverName = serverName.Remove(0, 1);

            if (Game.ClientType == GameClientType.VTC_Game && serverName.EndsWith("Thien_Kim") && serverName.Length >= 3)
                serverName = serverName.Remove(0, 3);

            var state = Game.ClientType >= GameClientType.Global
                ? $"{(ServerStatusModern)status}"
                : $"{currentCapacity}/{maxCapacity}";

            parsed.Add(new Server
            {
                Id = id,
                Name = serverName,
                CurrentCapacity = currentCapacity,
                MaxCapacity = maxCapacity,
                Status = Game.ClientType >= GameClientType.Global ? status != 4 : status == 1,
                State = state
            });

            if (Game.ClientType == GameClientType.Vietnam)
                packet.ReadByte(); // FarmId

            if (logServers)
                Log.Notify($"Found server: {serverName} ({state})");
        }

        Servers = parsed;
        EventManager.FireEvent("OnServerListUpdated");
        return parsed;
    }

    /// <summary>
    ///     Gets the server by its name
    /// </summary>
    /// <param name="name">The name.</param>
    /// <returns></returns>
    public static Server GetServerByName(string name)
    {
        return Servers?.FirstOrDefault(s => string.Equals(s.Name, name, System.StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    ///     Set joining the server by shard id
    /// </summary>
    /// <param name="shardId">The shard id</param>
    internal static void SetJoining(ushort shardId)
    {
        Joining = Servers?.FirstOrDefault(p => p.Id == shardId);
    }
}