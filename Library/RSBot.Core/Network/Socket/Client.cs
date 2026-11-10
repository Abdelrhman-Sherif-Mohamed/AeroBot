using RSBot.Core.Network.Protocol;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace RSBot.Core.Network;

public class Client() : NetBase(isClient: true)
{
    /// <summary>
    ///     Captures the socket used for a specific BeginReceive call so a late
    ///     callback can never read from a newer connection.
    /// </summary>
    private sealed class ReceiveState
    {
        public Socket Socket;
        public byte[] Buffer;
    }

    private int _accepting;
    private int _receiving;

    /// <summary>
    ///     Gets or sets the listener.
    /// </summary>
    /// <value>
    ///     The listener.
    /// </value>
    public Socket _listener;

    /// <summary>
    ///     Listens the specified port.
    /// </summary>
    /// <param name="port">The port.</param>
    public void Listen(ushort port)
    {
        try
        {
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            StartNetWorker();

            _listener.Bind(new IPEndPoint(IPAddress.Loopback, port));
            _listener.Listen(1);
            BeginAccept();
        }
        catch
        {
        }
    }

    private void BeginAccept()
    {
        var listener = _listener;
        if (listener == null || IsClosing)
            return;

        if (Interlocked.Exchange(ref _accepting, 1) == 1)
            return;

        try
        {
            listener.BeginAccept(OnClientConnect, listener);
        }
        catch
        {
            Interlocked.Exchange(ref _accepting, 0);
        }
    }

    /// <summary>
    ///     Listens this instance.
    /// </summary>
    private void Listen()
    {
        try
        {
            EnablePacketDispatcher = false;

            var socket = _socket;
            _socket = null;
            _protocol = null;

            if (socket != null)
            {
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                catch
                {
                }

                socket.Close();
            }

            BeginAccept();
        }
        catch
        {
        }
    }

    /// <summary>
    ///     Shutdowns this instance.
    /// </summary>
    public void Shutdown()
    {
        try
        {
            EnablePacketDispatcher = false;
            IsClosing = true;

            // Never join ourselves when Shutdown is called from the packet
            // dispatcher thread (it would deadlock the whole bot).
            if (_dispatcherThread != null && _dispatcherThread != Thread.CurrentThread)
                _dispatcherThread.Join(1000);

            //Close Socket
            if (_socket != null)
            {
                if (_socket.Connected)
                    _socket.Shutdown(SocketShutdown.Both);

                _socket.Close();
                _socket = null;
            }

            //Close listener
            if (_listener != null)
            {
                _listener.Close();
                _listener = null;
            }

            _protocol = null;
            _dispatcherThread = null;
        }
        catch
        {
        }
    }

    /// <summary>
    ///     Called when [client connect].
    /// </summary>
    /// <param name="ar">The ar.</param>
    private void OnClientConnect(IAsyncResult ar)
    {
        Interlocked.Exchange(ref _accepting, 0);

        try
        {
            var listener = ar.AsyncState as Socket;
            if (IsClosing || listener == null || !ReferenceEquals(listener, _listener))
                return;

            var socket = listener.EndAccept(ar);
            _socket = socket;

            _protocol = new SecurityProtocol();
            _protocol.GenerateSecurity(true, true, true);

            EnablePacketDispatcher = true;
            Interlocked.Exchange(ref _receiving, 0);
            StartReceive();

            OnConnected();
        }
        catch (Exception)
        {
            OnDisconnected();
            Listen();
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
            Listen();
        }
    }

    /// <summary>
    ///     Waits for data.
    /// </summary>
    /// <param name="ar">The ar.</param>
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
                Listen();
                return;
            }

            _protocol.Recv(state.Buffer, 0, receivedSize);
        }
        catch (SocketException se)
        {
            if (se.SocketErrorCode ==
                SocketError.ConnectionReset) //Client OnDisconnected > Mostly occurs during GW->AS switch
            {
                OnDisconnected();

                Listen();
            }
        }
        catch (HandshakeSecurityException ex)
        {
            Log.Error("[Fatal]: Could not handshake the client, restarting client process now...");

            Kernel.Proxy?.Shutdown();
        }
        catch (ObjectDisposedException)
        {
            OnDisconnected();
            Listen();
        }
        finally
        {
            if (receivedSize != 0)
                StartReceive();
        }
    }
}