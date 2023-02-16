using System.Net;
using System.Net.Sockets;
using Serilog;
using TCPProxy.Helpers;
using TCPProxy.Models;
using static TCPProxy.Helpers.RequestHelper;

namespace TCPProxy.Providers;

public class TcpProxyServer
{
    private readonly Socket _proxySocket = new(IPAddress.Any.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
    private readonly BufferHelper _bufferHelper = new(8192);

    public async ValueTask<Task?> StartProxy(ProxyConfigurationModel configuration, CancellationToken stoppingToken)
    {
        Log.Information("Starting proxy...");
        _proxySocket.Bind(new IPEndPoint(IPAddress.Any, configuration.GetPort()));
        _proxySocket.Listen();
        DebugHelper.PrintConfiguration(configuration);

        while (!stoppingToken.IsCancellationRequested)
        {
            using var clientSocket = await _proxySocket.AcceptAsync(stoppingToken);
            var initialBuffer = _bufferHelper.TakeBuffer();
            var (bufferReceived, bytesReceived) =
                await _bufferHelper.ExecuteReceiveAsync(BufferHelper.ReceiveMethod, clientSocket, initialBuffer,
                    stoppingToken);

            if (bufferReceived is null) continue;

            var serverUrl = ExtractHeader(bufferReceived, bytesReceived, HttpRequestHeader.Host);

            Log.Information("Request received from {ClientIp}", clientSocket.RemoteEndPoint);

            var ipAddress = new Endpoint(serverUrl);

            var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await serverSocket.ConnectAsync(ipAddress, stoppingToken);
            Log.Debug("Connected to server {ServerIp}", serverSocket.RemoteEndPoint);

            await BufferHelper.ExecuteSendAsync(BufferHelper.SendMethod, serverSocket, bufferReceived, bytesReceived,
                stoppingToken);
            Log.Information("Request forwarded to {Server}", serverSocket.RemoteEndPoint);

            var (serverBufferReceived, serverBytesReceived) =
                await _bufferHelper.ExecuteReceiveAsync(BufferHelper.ReceiveMethod, serverSocket, bufferReceived,
                    stoppingToken);
            Log.Information("Response received from {ServerIp}", serverSocket.RemoteEndPoint);

            if (serverBufferReceived == null) continue;
            await BufferHelper.ExecuteSendAsync(BufferHelper.SendMethod, clientSocket, serverBufferReceived,
                serverBytesReceived, stoppingToken);
            Log.Information("Response forwarded to {Client}", clientSocket.RemoteEndPoint?.ToString());

            Log.Information("Proxying finished, waiting for new request...");
        }

        return Task.CompletedTask;
    }
}