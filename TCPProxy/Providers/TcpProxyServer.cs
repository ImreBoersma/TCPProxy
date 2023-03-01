﻿using System.Net;
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

            if (!TryExtractServerEndpoint(clientHttpMessage, out var serverEndpoint)) continue;

            Log.Information("Request received from {ClientIp}", clientSocket.RemoteEndPoint?.ToString());

            using var serverSocket = await ConnectToServerAsync(serverEndpoint, stoppingToken);
            await ForwardRequestToServerAsync(serverSocket, clientHttpMessage, stoppingToken);

            var serverHttpMessage = await ReceiveResponseFromServerAsync(serverSocket, clientHttpMessage.Buffer, stoppingToken);
            serverHttpMessage = ProcessResponse(serverHttpMessage, configuration);

            Log.Information("Response received from {ServerIp}:\n{Request}", serverSocket.RemoteEndPoint?.ToString(), serverHttpMessage.ToString());

            await ForwardResponseToClientAsync(clientSocket, serverHttpMessage, stoppingToken);

            Log.Information("Proxying finished, waiting for new request...");
        }

        return Task.CompletedTask;
    }

    public static HttpMessage ProcessResponse(HttpMessage responseMessage, ProxyConfigurationModel configuration)
    {
        var processedMessage = responseMessage;

        if (configuration.GetMaskImages())
        {
            processedMessage = MaskImage(responseMessage);
        }

        if (configuration.GetIncognito())
        {
            processedMessage = Incognito(responseMessage);
        }

        if (configuration.GetCache())
        {
            processedMessage = Cache(responseMessage);
        }

        return processedMessage;
    }

    public static bool TryExtractServerEndpoint(HttpMessage message, out IPEndPoint endpoint)
    {
        var serverUrl = ExtractHeader(message, HttpRequestHeader.Host);
        if (string.IsNullOrEmpty(serverUrl))
        {
            Log.Warning("No host header found, aborting request");
            endpoint = new IPEndPoint(IPAddress.Any, 0);
            return false;
        }

        endpoint = new Endpoint(serverUrl);
        return true;
    }

    private static async ValueTask<Socket> ConnectToServerAsync(IPEndPoint endpoint, CancellationToken stoppingToken)
    {
        var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        await serverSocket.ConnectAsync(endpoint, stoppingToken);
        Log.Debug("Connected to server {ServerIp}", serverSocket.RemoteEndPoint?.ToString());
        return serverSocket;
    }

    private async Task ForwardRequestToServerAsync(Socket serverSocket, HttpMessage requestMessage, CancellationToken stoppingToken)
    {
        await _bufferHelper.ExecuteSendAsync(serverSocket, requestMessage, stoppingToken);
        Log.Information("Forward request to {Server}", serverSocket.RemoteEndPoint?.ToString());
    }

    private async Task<HttpMessage> ReceiveResponseFromServerAsync(Socket serverSocket, byte[] clientBuffer, CancellationToken stoppingToken)
    {
        return await _bufferHelper.ExecuteReceiveAsync(serverSocket, stoppingToken, SocketFlags.None, clientBuffer);
    }

    private async Task ForwardResponseToClientAsync(Socket clientSocket, HttpMessage responseMessage, CancellationToken stoppingToken)
    {
        await _bufferHelper.ExecuteSendAsync(clientSocket, responseMessage, stoppingToken);
        Log.Information("Response forwarded to client");
    }
}