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
    private readonly BufferHelper _bufferHelper = new();

    public async ValueTask<Task?> StartProxy(ProxyConfigurationModel configuration, CancellationToken stoppingToken)
    {
        Log.Information("Starting proxy...");
        _proxySocket.Bind(new IPEndPoint(IPAddress.Any, configuration.GetPort()));
        _proxySocket.Listen();
        DebugHelper.PrintConfiguration(configuration);

        while (!stoppingToken.IsCancellationRequested)
        {
            using var clientSocket = await _proxySocket.AcceptAsync(stoppingToken);
            var clientHttpMessage = await _bufferHelper.ExecuteReceiveAsync(clientSocket, stoppingToken);

            var serverUrl = ExtractHeader(clientHttpMessage, HttpRequestHeader.Host);
            if (string.IsNullOrEmpty(serverUrl))
            {
                Log.Warning("No host header found, aborting request");
                continue;
            }

            Log.Information("Request received from {ClientIp}", clientSocket.RemoteEndPoint?.ToString());

            var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint serverEndpoint = new Endpoint(serverUrl);
            await serverSocket.ConnectAsync(serverEndpoint, stoppingToken);
            Log.Debug("Connected to server {ServerIp}", serverSocket.RemoteEndPoint?.ToString());

            await _bufferHelper.ExecuteSendAsync(serverSocket, clientHttpMessage, stoppingToken);
            Log.Information("Request forwarded to {Server}", serverSocket.RemoteEndPoint?.ToString());

            var serverHttpMessage = await _bufferHelper.ExecuteReceiveAsync(serverSocket, stoppingToken, SocketFlags.None, clientHttpMessage.Buffer);

            if (configuration.GetMaskImages()) serverHttpMessage = MaskImage(serverHttpMessage);
            if (configuration.GetIncognito()) serverHttpMessage = Incognito(serverHttpMessage);
            if (configuration.GetCache()) serverHttpMessage = Cache(serverHttpMessage);

            Log.Information("Response received from {ServerIp}:\n{Request}", serverSocket.RemoteEndPoint?.ToString(), serverHttpMessage.ToString());

            await _bufferHelper.ExecuteSendAsync(clientSocket, serverHttpMessage, stoppingToken);
            Log.Information("Response forwarded to client");

            Log.Information("Proxying finished, waiting for new request...");
        }

        return Task.CompletedTask;
    }
}