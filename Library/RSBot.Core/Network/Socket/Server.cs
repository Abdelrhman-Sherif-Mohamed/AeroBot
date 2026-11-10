using RSBot.Core.Event;
using RSBot.Core.Extensions;
using RSBot.Core.Network.Protocol;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace RSBot.Core.Network;

public class Server : NetBase
{
    /// <summary>
    ///     Captures the socket used for a specific BeginReceive call so a late
    ///     callback from an old connection can never read from a new socket.
    /// </summary>
    private sealed class ReceiveState
    {
        public Socket Socket;
        public byte[] Buffer;
    }

    private int _receiving;
    private int _disconnectedRaised;

    /// <summary>
    ///     Gets or sets the ip.
    /// </summary>
    /// <value>
    ///     The ip.
    /// </value>
    public string IP { get; set; }

    /// <summary>
    ///     Gets or sets the port.
    /// </summary>
    /// <value>
    ///     The port.
    /// </value>
    public ushort Port { get; set; }

    /// <summary>
    ///     Raises the disconnected event only once per established connection.
    /// </summary>
    public override void OnDisconnected()
    {
        if (Interlocked.Exchange(ref _disconnectedRaised, 1) == 1)
            return;

        base.OnDisconnected();
    }

    /// <summary>
    ///     Connects the specified ip.
    /// </summary>
    /// <param name="ip">The ip.</param>
    /// <param name="port">The port.</param>
    public void Connect(string ip, ushort port)
    {
        IP = ip;
        Port = port;

        if (_socket != null)
            Disconnect();

        Interlocked.Exchange(ref _receiving, 0);
        Interlocked.Exchange(ref _disconnectedRaised, 0);

        try
        {
            _protocol = new SecurityProtocol();
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                var bindIp = GlobalConfig.Get("RSBot.Network.BindIp", "0.0.0.0");
                if (!string.IsNullOrWhiteSpace(bindIp) && bindIp != "0.0.0.0")
                    _socket.Bind(new IPEndPoint(IPAddress.Parse(bindIp), port));

                if (ProxyConfig.TryGetProxy(ip, port, out var proxyConfig))
                {
                    if (!_socket.ConnectViaProxy(proxyConfig).Result)
                    {
                        Log.Warn("Proxy receiving has timeout!");
                        Disconnect();
                        return;
                    }
                }
                else
                {
                    _socket.Connect(ip, port, 3000);
                }
            }
            catch (AggregateException s)
            {
                Log.Error(s.Message);
                Disconnect();
                return;
            }
            catch (SocketProxyException s)
            {
                Log.Error(s.Message);
                Disconnect();
                return;
            }
            catch (SocketException s)
            {
                Log.Error($"Could not establish a connection to {ip}:{port}.");
                Log.Error(s.Message);
                Disconnect();
                return;
            }

            StartNetWorker();

            EnablePacketDispatcher = true;

            StartReceive();

            OnConnected();

            EventManager.FireEvent("OnServerConnected");
            Log.Debug("Server connection established!");
        }
        catch
        {
        }
    }

    private void StartReceive()
    {
        var socket = _socket;
        if (socket == null || IsClosing || !EnablePacketDispatcher)
            return;

        if (Interlocked.Exchange(ref _receiving, 1) == 1)
            return;

        try
        {
            socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None,
                OnBeginReceiveCallback, new ReceiveState { Socket = socket, Buffer = _buffer });
        }
        catch
        {
            Interlocked.Exchange(ref _receiving, 0);
            OnDisconnected();
        }
    }

    /// <summary>
    ///     Waits for data.
    /// </summary>
    /// <param name="result">The result.</param>
    private void OnBeginReceiveCallback(IAsyncResult ar)
    {
        Interlocked.Exchange(ref _receiving, 0);

        if (IsClosing || !EnablePacketDispatcher)
            return;

        var state = ar.AsyncState as ReceiveState;
        if (state == null || !ReferenceEquals(state.Socket, _socket))
            return;

        var receivedSize = 0;

        try
        {
            receivedSize = state.Socket.EndReceive(ar, out var error);
            if (receivedSize == 0 || error != SocketError.Success)
            {
                OnDisconnected();
                return;
            }

            _protocol.Recv(state.Buffer, 0, receivedSize);
        }
        catch (SocketException se)
        {
            if (se.SocketErrorCode ==
                SocketError.ConnectionReset) //Client OnDisconnected > Mostly occurs during GW->AS switch
                OnDisconnected();
        }
        catch (HandshakeSecurityException)
        {
            Log.Notify("[Fatal]: Could not handshake the client, restarting client process now...");
            Game.Start();
        }
        catch (ObjectDisposedException)
        {
            OnDisconnected();
        }
        finally
        {
            if (receivedSize != 0)
                StartReceive();
        }
    }

    /// <summary>
    ///     Sends the specified packet.
    /// </summary>
    /// <param name="packet">The packet.</param>
    public void Send(Packet packet)
    {
        OnPacketSent(packet);
        _protocol.Send(packet);
    }

    /// <summary>
    ///     Disconnects this instance.
    /// </summary>
    public void Disconnect()
    {
        EnablePacketDispatcher = false;
        IsClosing = true;

        try
        {
            if (_socket == null)
                return;

            if (_socket.Connected)
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
        }
        catch
        {
        }
        finally
        {
            _socket = null;
            OnDisconnected();
        }

        IsClosing = false;
    }
}