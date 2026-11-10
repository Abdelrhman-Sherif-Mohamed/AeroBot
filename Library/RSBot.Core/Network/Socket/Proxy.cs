using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using RSBot.Core.Components;
using RSBot.Core.Event;

namespace RSBot.Core.Network;

public class Proxy
{
    /// <summary>
    ///     Gets a value indicating whether [client connected].
    /// </summary>
    /// <value>
    ///     <c>true</c> if [client connected]; otherwise, <c>false</c>.
    /// </value>
    public bool ClientConnected { get; private set; }

    /// <summary>
    ///     Gets the port.
    /// </summary>
    /// <value>
    ///     The port.
    /// </value>
    public ushort Port { get; private set; }

    /// <summary>
    ///     Gets the client.
    /// </summary>
    /// <value>
    ///     The client.
    /// </value>
    public Client Client { get; private set; }

    /// <summary>
    ///     Gets the server.
    /// </summary>
    /// <value>
    ///     The server.
    /// </value>
    public Server Server { get; private set; }

    /// <summary>
    ///     Gets a value indicating whether [connected to gatewayserver].
    /// </summary>
    /// <value>
    ///     <c>true</c> if [connected to gatewayserver]; otherwise, <c>false</c>.
    /// </value>
    public bool IsConnectedToGatewayserver { get; private set; }

    /// <summary>
    ///     Gets a value indicating whether [connected to agentserver].
    /// </summary>
    /// <value>
    ///     <c>true</c> if [connected to agentserver]; otherwise, <c>false</c>.
    /// </value>
    public bool IsConnectedToAgentserver { get; private set; }

    /// <summary>
    ///     Gets the session.
    /// </summary>
    /// <value>
    ///     The session.
    /// </value>
    public uint Token { get; internal set; }

    /// <summary>
    ///     Starts the specified client port.
    /// </summary>
    /// <param name="clientPort">The client port.</param>
    /// <param name="gatewayIp">The gateway ip.</param>
    /// <param name="gatewayPort">The gateway port.</param>
    public void Start(ushort clientPort, string gatewayIp, ushort gatewayPort)
    {
        Port = clientPort;
        _gatewayIp = gatewayIp;
        _gatewayPort = gatewayPort;

        SessionLog.Write($"Proxy started on 127.0.0.1:{clientPort} -> {gatewayIp}:{gatewayPort} (clientless: {Game.Clientless})");

        if (!Game.Clientless)
            CreateNewClientInstance(clientPort);
        else
            ConnectToGatewayserver();
    }

    /// <summary>
    ///     Shutdowns this instance.
    /// </summary>
    public void Shutdown()
    {
        DetachClient(Client);
        Client?.Shutdown();
        Client = null;

        DetachServer(Server);
        Server?.Disconnect();
        Server = null;

        IsConnectedToGatewayserver = false;
        IsConnectedToAgentserver = false;

        _agentIp = null;
        _agentPort = 0;
        _gatewayIp = null;
        _gatewayPort = 0;
    }

    /// <summary>
    ///     Removes every event subscription from the given server instance so a
    ///     late/stale socket callback can not report a bogus disconnect after a
    ///     new server instance has been created.
    /// </summary>
    private void DetachServer(Server server)
    {
        if (server == null)
            return;

        server.Connected -= Server_OnConnected;
        server.Disconnected -= Server_OnDisconnected;
        server.PacketReceived -= Server_OnPacketReceived;
    }

    /// <summary>
    ///     Removes every event subscription from the given client instance.
    /// </summary>
    private void DetachClient(Client client)
    {
        if (client == null)
            return;

        client.Connected -= Client_OnConnected;
        client.Disconnected -= Client_OnDisconnected;
        client.PacketReceived -= Client_OnPacketReceived;
    }

    /// <summary>
    ///     Connects to gatewayserver.
    /// </summary>
    private void ConnectToGatewayserver()
    {
        var ports = GetGatewayPortCandidates();

        foreach (var port in ports)
        {
            //the first attempt may fail because of a short network hickup
            for (var attempt = 1; attempt <= 2; attempt++)
            {
                Log.Notify($"Connecting to gateway server [{_gatewayIp}:{port}]...");
                SessionLog.Write($"Connecting to gateway server {_gatewayIp}:{port} (attempt {attempt})");

                CreateNewServerInstance();

                IsConnectedToAgentserver = false;
                IsConnectedToGatewayserver = true;

                _suppressServerDisconnect = true;
                Server.Connect(_gatewayIp, port);
                _suppressServerDisconnect = false;

                if (Server.IsConnected)
                    return;

                Thread.Sleep(250);
            }
        }

        SessionLog.Write($"Could not connect to any of the gateway ports ({string.Join(", ", ports)})");
        Log.Warn("Could not connect to any gateway server.");

        IsConnectedToGatewayserver = false;
        EventManager.FireEvent("OnGatewayServerDisconnected");
    }

    /// <summary>
    ///     Gets the gateway ports that should be tried, the configured port comes first.
    ///     Some clients use more than one gateway port, the loader writes the port the client
    ///     wanted to use into a small file.
    /// </summary>
    private List<ushort> GetGatewayPortCandidates()
    {
        var ports = new List<ushort> { _gatewayPort };

        foreach (var port in GetClientGatewayPorts())
        {
            if (!ports.Contains(port))
                ports.Add(port);
        }

        return ports;
    }

    /// <summary>
    ///     Reads the gateway ports the client tried to connect to (written by the loader).
    /// </summary>
    private List<ushort> GetClientGatewayPorts()
    {
        var ports = new List<ushort>();
        if (Game.ClientProcessId == 0)
            return ports;

        try
        {
            var path = Path.Combine(Path.GetTempPath(), $"RSBot_{Game.ClientProcessId}.port");
            if (!File.Exists(path))
                return ports;

            var data = File.ReadAllBytes(path);
            File.Delete(path);

            if (data.Length < sizeof(ushort))
                return ports;

            var port = BitConverter.ToUInt16(data, 0);
            if (port >= 1024)
            {
                SessionLog.Write($"The client requested gateway port {port}");
                ports.Add(port);
            }

            return ports;
        }
        catch (Exception exception)
        {
            SessionLog.Write("Could not read the gateway port of the client", exception);

            return ports;
        }
    }

    /// <summary>
    ///     Connects to agentserver.
    /// </summary>
    private void ConnectToAgentserver()
    {
        Log.Notify($"Connecting to agentserver [{_agentIp}:{_agentPort}]...");
        CreateNewServerInstance();

        IsConnectedToGatewayserver = false;
        IsConnectedToAgentserver = true;

        _suppressServerDisconnect = true;
        Server.Connect(_agentIp, _agentPort);
        _suppressServerDisconnect = false;

        if (Server.IsConnected)
            return;

        Log.Warn("Could not connect to the game (agent) server.");
        SessionLog.Write("Could not connect to the game (agent) server");

        IsConnectedToAgentserver = false;
        Game.Ready = false;
        Game.Started = false;

        EventManager.FireEvent("OnAgentServerDisconnected");
    }

    /// <summary>
    ///     Initializes the new server instance.
    /// </summary>
    private void CreateNewServerInstance()
    {
        // Reset the connection flags *before* disconnecting the old server.
        // Otherwise the old server's Disconnected event fires while the flags
        // still describe the previous connection and triggers a bogus
        // "agent server disconnected" (which makes the client restart loop).
        IsConnectedToGatewayserver = false;
        IsConnectedToAgentserver = false;

        var oldServer = Server;
        if (oldServer != null)
        {
            DetachServer(oldServer);
            oldServer.Disconnect();
        }

        Server = new Server();
        Server.Connected += Server_OnConnected;
        Server.Disconnected += Server_OnDisconnected;
        Server.PacketReceived += Server_OnPacketReceived;
    }

    /// <summary>
    ///     Creates the new client instance.
    /// </summary>
    /// <param name="clientPort">The client port.</param>
    private void CreateNewClientInstance(ushort clientPort)
    {
        var oldClient = Client;
        if (oldClient != null)
        {
            DetachClient(oldClient);
            oldClient.Shutdown();
        }

        Client = new Client();
        Client.Connected += Client_OnConnected;
        Client.PacketReceived += Client_OnPacketReceived;
        Client.Disconnected += Client_OnDisconnected;
        Client.Listen(clientPort);
    }

    /// <summary>
    ///     Handle received packet
    /// </summary>
    /// <param name="packet">The packet</param>
    private void HandleReceivedPacket(Packet packet, PacketDestination destination)
    {
        try
        {
            PacketLogger.LogPacket(packet, destination);
        }
        catch { }

        try
        {
            packet = PacketManager.CallHook(packet, destination);
        }
        catch (Exception e)
        {
            Log.Fatal(e);
        }
        finally
        {
            if (packet != null)
                try
                {
                    // The packet was already recorded by PacketLogger.LogPacket above,
                    // so it must not be recorded a second time here.
                    PacketManager.SendForwardedPacket(packet, destination);

                    packet.SeekRead(0, SeekOrigin.Begin);

                    PacketManager.CallHandler(packet, destination);
                    PacketManager.CallCallback(packet);
                }
                catch (Exception e)
                {
                    Log.Fatal(e);
                }
        }
    }

    #region Fields

    private string _agentIp;
    private ushort _agentPort;
    private string _gatewayIp;
    private ushort _gatewayPort;

    /// <summary>
    ///     When true, a server disconnect event is treated as an expected
    ///     connect/reconnect failure and is not forwarded to the UI (forwarding
    ///     it would shut the whole proxy down in the middle of a reconnect).
    /// </summary>
    private bool _suppressServerDisconnect;

    #endregion Fields

    #region Event Listeners

    #region Client

    /// <summary>
    ///     Fired when the client connected.
    /// </summary>
    private void Client_OnConnected()
    {
        Log.Notify("A new silkroad client connected.");
        SessionLog.Write("Client connected to the proxy");

        EventManager.FireEvent("OnClientConnected");

        ClientConnected = true;

        if (!IsConnectedToGatewayserver && !IsConnectedToAgentserver)
            ConnectToGatewayserver();
    }

    /// <summary>
    ///     Fired when the client disconnected.
    /// </summary>
    private void Client_OnDisconnected()
    {
        SessionLog.Write("Client disconnected from the proxy");

        ClientConnected = false;
    }

    /// <summary>
    ///     Fired when a client packet was received
    /// </summary>
    /// <param name="packet">The packet.</param>
    private void Client_OnPacketReceived(Packet packet)
    {
        if (IsConnectedToAgentserver && packet.Opcode == 0x6100)
            return;

        HandleReceivedPacket(packet, PacketDestination.Server);
        EventManager.FireEvent("OnClientPacketReceive", packet);
    }

    #endregion Client

    #region Server

    /// <summary>
    ///     Fired when a server packet was received
    /// </summary>
    /// <param name="packet">The packet.</param>
    private void Server_OnPacketReceived(Packet packet)
    {
        HandleReceivedPacket(packet, PacketDestination.Client);

        EventManager.FireEvent("OnServerPacketReceive", packet);
    }

    /// <summary>
    ///     Sets the agentserver address.
    /// </summary>
    /// <param name="ip">The ip.</param>
    /// <param name="port">The port.</param>
    internal void SetAgentserverAddress(string ip, ushort port)
    {
        _agentIp = ip;
        _agentPort = port;

        SessionLog.Write($"Agent server address received: {ip}:{port}");

        ConnectToAgentserver();
    }

    /// <summary>
    ///     Fired when the server disconnected
    /// </summary>
    private void Server_OnDisconnected()
    {
        if (_suppressServerDisconnect)
            return;

        if (IsConnectedToAgentserver)
        {
            Log.Warn("Disconnected from game server!");
            SessionLog.Write("Disconnected from the game (agent) server");

            IsConnectedToAgentserver = false;

            Game.Ready = false;
            Game.Started = false;

            EventManager.FireEvent("OnAgentServerDisconnected");
        }
        else if (IsConnectedToGatewayserver)
        {
            Log.Debug("Disconnected from login server!");
            SessionLog.Write("Disconnected from the login (gateway) server");

            IsConnectedToGatewayserver = false;

            EventManager.FireEvent("OnGatewayServerDisconnected");
        }
    }

    /// <summary>
    ///     Fired when the server conntected
    /// </summary>
    private void Server_OnConnected()
    {
        if (IsConnectedToGatewayserver)
        {
            SessionLog.Write("Connected to the login (gateway) server");
            EventManager.FireEvent("OnGatewayServerConntected");
        }
        else if (IsConnectedToAgentserver)
        {
            SessionLog.Write("Connected to the game (agent) server");
            EventManager.FireEvent("OnAgentServerConnected");
        }
    }

    #endregion Server

    #endregion Event Listeners
}