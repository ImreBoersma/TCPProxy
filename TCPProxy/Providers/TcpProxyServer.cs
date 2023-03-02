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
    private readonly IBufferHelper _bufferHelper = new BufferHelper();

    /// <summary>
    ///   Start the proxy server and listen for incoming requests
    /// </summary>
    /// <param name="configuration">The proxy configuration</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A task</returns>
    public async ValueTask<Task?> StartProxy(ProxyConfigurationModel configuration, CancellationToken cancellationToken)
    {
        Log.Information("Starting proxy...");
        _bufferHelper.SetBufferSize(configuration.GetBuffer());
        _proxySocket.Bind(new IPEndPoint(IPAddress.Any, configuration.GetPort()));
        _proxySocket.Listen();
        configuration.PrintConfig();

        while (!cancellationToken.IsCancellationRequested)
        {
            using var clientSocket = await _proxySocket.AcceptAsync(cancellationToken);
            var clientHttpMessage = await _bufferHelper.ExecuteReceiveAsync(clientSocket, cancellationToken);

            if (!TryExtractServerEndpoint(clientHttpMessage, out var serverEndpoint)) continue;

            Log.Information("Request received from {ClientIp}", clientSocket.RemoteEndPoint?.ToString());

            using var serverSocket = await ConnectToServerAsync(serverEndpoint, cancellationToken);
            await ForwardRequestToServerAsync(serverSocket, clientHttpMessage, cancellationToken);

            var serverHttpMessage = await ReceiveResponseFromServerAsync(serverSocket, clientHttpMessage.Buffer, cancellationToken);
            serverHttpMessage = ProcessResponse(serverHttpMessage, configuration);

            Log.Information("Response received from {ServerIp}:\n{Request}", serverSocket.RemoteEndPoint?.ToString(), serverHttpMessage.ToString());

            await ForwardResponseToClientAsync(clientSocket, serverHttpMessage, cancellationToken);

            Log.Information("Proxying finished, waiting for new request...");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    ///    Process the response from the server before sending it to the client
    /// </summary>
    /// <param name="responseMessage">The response from the server</param>
    /// <param name="configuration">The proxy configuration</param>
    /// <returns>The processed response</returns>
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

    /// <summary>
    ///   Extract the server endpoint from the request
    /// </summary>
    /// <param name="message">The request message</param>
    /// <param name="endpoint">The server endpoint</param>
    /// <returns>True if the endpoint was extracted, false otherwise</returns>
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

    /// <summary>
    /// Connect to the server endpoint
    /// </summary>
    /// <param name="endpoint">The server endpoint</param>
    /// <param name="stoppingToken">The cancellation token</param>
    /// <returns>The server socket</returns>
    public static async ValueTask<Socket> ConnectToServerAsync(IPEndPoint endpoint, CancellationToken stoppingToken)
    {
        var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        await serverSocket.ConnectAsync(endpoint, stoppingToken);
        Log.Debug("Connected to server {ServerIp}", serverSocket.RemoteEndPoint?.ToString());
        return serverSocket;
    }

    /// <summary>
    /// Forward the request to the server
    /// </summary>
    /// <param name="serverSocket">The server socket</param>
    /// <param name="requestMessage">The request message</param>
    /// <param name="stoppingToken">The cancellation token</param>
    private async Task ForwardRequestToServerAsync(Socket serverSocket, HttpMessage requestMessage, CancellationToken stoppingToken)
    {
        await _bufferHelper.ExecuteSendAsync(serverSocket, requestMessage, stoppingToken);
        Log.Information("Forward request to {Server}", serverSocket.RemoteEndPoint?.ToString());
    }

    /// <summary>
    /// Receive the response from the server
    /// </summary>
    /// <param name="serverSocket">The server socket</param>
    /// <param name="clientBuffer">The client buffer</param>
    /// <param name="stoppingToken">The cancellation token</param>
    /// <returns>The response message</returns>
    private async Task<HttpMessage> ReceiveResponseFromServerAsync(Socket serverSocket, byte[] clientBuffer, CancellationToken stoppingToken)
    {
        return await _bufferHelper.ExecuteReceiveAsync(serverSocket, stoppingToken, SocketFlags.None, clientBuffer);
    }

    /// <summary>
    /// Forward the response to the client
    /// </summary>
    /// <param name="clientSocket">The client socket</param>
    /// <param name="responseMessage">The response message</param>
    /// <param name="stoppingToken">The cancellation token</param>
    private async Task ForwardResponseToClientAsync(Socket clientSocket, HttpMessage responseMessage, CancellationToken stoppingToken)
    {
        await _bufferHelper.ExecuteSendAsync(clientSocket, responseMessage, stoppingToken);
        Log.Information("Response forwarded to client");
    }
}