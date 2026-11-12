using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using RSBot.Core;
using RSBot.Core.Components;
using RSBot.Core.Extensions;
using RSBot.Core.Network;
using RSBot.Core.Network.Protocol;
using RSBot.General.Components;

namespace RSBot.ServerInfo.Components;

internal enum GatewayQueryStatus
{
    Success,
    RequestedThroughProxy,
    ArchiveNotReady,
    InvalidGateway,
    PatchRequired,
    Timeout,
    Failed
}

internal readonly struct GatewayQueryResult
{
    public GatewayQueryStatus Status { get; init; }
    public int ServerCount { get; init; }
    public string Detail { get; init; }
}

internal static class GatewayQuery
{
    private const int DefaultTimeoutMs = 8000;

    /// <summary>
    ///     Queries the configured Silkroad Gateway server directly over TCP for a fresh 0xA101 server list,
    ///     or uses the active gateway proxy connection if currently at the login screen.
    /// </summary>
    public static async Task<GatewayQueryResult> QueryServerListAsync(CancellationToken cancellationToken = default)
    {
        if (Kernel.Proxy != null && Kernel.Proxy.IsConnectedToGatewayserver)
        {
            ClientlessManager.RequestServerList();
            return new GatewayQueryResult { Status = GatewayQueryStatus.RequestedThroughProxy };
        }

        var divisionInfo = Game.ReferenceManager?.DivisionInfo;
        var gatewayInfo = Game.ReferenceManager?.GatewayInfo;
        var versionInfo = Game.ReferenceManager?.VersionInfo;

        if (divisionInfo?.Divisions == null || divisionInfo.Divisions.Count == 0 ||
            gatewayInfo == null || gatewayInfo.Port == 0 || versionInfo == null)
        {
            return new GatewayQueryResult { Status = GatewayQueryStatus.ArchiveNotReady };
        }

        var divisionIndex = GlobalConfig.Get<int>("RSBot.DivisionIndex");
        if (divisionIndex < 0 || divisionIndex >= divisionInfo.Divisions.Count)
            divisionIndex = 0;

        var division = divisionInfo.Divisions[divisionIndex];
        if (division?.GatewayServers == null || division.GatewayServers.Count == 0)
            return new GatewayQueryResult { Status = GatewayQueryStatus.InvalidGateway };

        var gatewayIndex = GlobalConfig.Get<int>("RSBot.GatewayIndex");
        if (gatewayIndex < 0 || gatewayIndex >= division.GatewayServers.Count)
            gatewayIndex = 0;

        var host = division.GatewayServers[gatewayIndex];
        var port = gatewayInfo.Port;
        if (string.IsNullOrWhiteSpace(host) || port == 0)
            return new GatewayQueryResult { Status = GatewayQueryStatus.InvalidGateway };

        using var timeoutCts = new CancellationTokenSource(DefaultTimeoutMs);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
        var token = linkedCts.Token;

        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            if (ProxyConfig.TryGetProxy(host, port, out var proxyConfig))
            {
                if (!await socket.ConnectViaProxy(proxyConfig).WaitAsync(token))
                    return new GatewayQueryResult { Status = GatewayQueryStatus.Timeout };
            }
            else
            {
                await socket.ConnectAsync(host, port, token);
            }

            var security = new SecurityProtocol();
            var recvBuffer = new byte[8192];
            var patchRequested = false;
            var listRequested = false;

            while (!token.IsCancellationRequested)
            {
                await FlushOutgoingAsync(socket, security, token);

                var bytesRead = await socket.ReceiveAsync(recvBuffer, SocketFlags.None, token);
                if (bytesRead <= 0)
                    break;

                security.Recv(recvBuffer, 0, bytesRead);
                await FlushOutgoingAsync(socket, security, token);

                List<Packet> incoming = security.TransferIncoming();
                if (incoming == null)
                    continue;

                foreach (var packet in incoming)
                {
                    if (packet.Opcode == 0x5000 || packet.Opcode == 0x9000)
                        continue;

                    if (packet.Opcode == 0x2001)
                    {
                        var service = packet.ReadString();
                        if (service == "GatewayServer" && !patchRequested)
                        {
                            patchRequested = true;
                            var patchRequest = new Packet(0x6100);
                            patchRequest.WriteByte(divisionInfo.Locale);
                            patchRequest.WriteString("SR_Client");
                            patchRequest.WriteUInt(versionInfo.Version);
                            security.Send(patchRequest);
                            await FlushOutgoingAsync(socket, security, token);
                        }
                    }
                    else if (packet.Opcode == 0xA100)
                    {
                        var result = packet.ReadByte();
                        if (result == 0x01 && !listRequested)
                        {
                            listRequested = true;
                            security.Send(new Packet(0x6101, true));
                            await FlushOutgoingAsync(socket, security, token);
                        }
                        else if (result != 0x01)
                        {
                            return new GatewayQueryResult { Status = GatewayQueryStatus.PatchRequired };
                        }
                    }
                    else if (packet.Opcode == 0xA101)
                    {
                        var servers = Serverlist.ParseFromPacket(packet, logServers: false);
                        return new GatewayQueryResult
                        {
                            Status = GatewayQueryStatus.Success,
                            ServerCount = servers.Count
                        };
                    }
                }
            }

            return new GatewayQueryResult { Status = GatewayQueryStatus.Failed };
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            return new GatewayQueryResult { Status = GatewayQueryStatus.Timeout };
        }
        catch (OperationCanceledException)
        {
            return new GatewayQueryResult { Status = GatewayQueryStatus.Failed };
        }
        catch (Exception ex)
        {
            return new GatewayQueryResult { Status = GatewayQueryStatus.Failed, Detail = ex.Message };
        }
        finally
        {
            try
            {
                if (socket.Connected)
                    socket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
            }
        }
    }

    private static async Task FlushOutgoingAsync(Socket socket, SecurityProtocol security, CancellationToken token)
    {
        foreach (var buffer in security.TransferOutgoing())
        {
            var sent = 0;
            while (sent < buffer.Length)
            {
                sent += await socket.SendAsync(new ArraySegment<byte>(buffer, sent, buffer.Length - sent), SocketFlags.None, token);
            }
        }
    }
}
