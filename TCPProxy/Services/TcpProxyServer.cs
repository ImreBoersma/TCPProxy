using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using Serilog;
using TCPProxy.Models;
using static TCPProxy.Helpers.RequestHelper;

namespace TCPProxy.Services;

public class TcpProxyServer
{
    private readonly TcpListener _listener;
    private readonly ProxyConfigurationModel _config;

    public TcpProxyServer(ProxyConfigurationModel config)
    {
        _config = config;
        _listener = new TcpListener(IPAddress.Any, _config.GetPort());
    }

    private async Task HandleClient(TcpClient client, CancellationToken stoppingToken)
    {
        var stream = client.GetStream();
        var buffer = new byte[_config.GetBuffer()];
        var bytes = await stream.ReadAsync(buffer, stoppingToken);
        var message = new HttpMessage(buffer, bytes);
        Log.Information("Received message:\r\n{Message}", message.ToString());
        var header = message.GetHeader();
        TryExtractServerEndpoint(message, out var host);
        var firstLine = header.Substring(0, header.IndexOf("\r\n", StringComparison.Ordinal));
        var request = new HttpRequestMessage(HttpMethod.Get, host.ToString());
        if (firstLine.Contains("POST"))
        {
            request = new HttpRequestMessage(HttpMethod.Post, host.ToString());
        }
        else if (firstLine.Contains("PUT"))
        {
            request = new HttpRequestMessage(HttpMethod.Put, host.ToString());
        }
        else if (firstLine.Contains("DELETE"))
        {
            request = new HttpRequestMessage(HttpMethod.Delete, host.ToString());
        }
        else if (firstLine.Contains("HEAD"))
        {
            request = new HttpRequestMessage(HttpMethod.Head, host.ToString());
        }
        else if (firstLine.Contains("OPTIONS"))
        {
            request = new HttpRequestMessage(HttpMethod.Options, host.ToString());
        }
        else if (firstLine.Contains("PATCH"))
        {
            request = new HttpRequestMessage(HttpMethod.Patch, host.ToString());
        }
        else if (firstLine.Contains("TRACE"))
        {
            request = new HttpRequestMessage(HttpMethod.Trace, host.ToString());
        }
        else if (firstLine.Contains("CONNECT"))
        {
            request = new HttpRequestMessage(HttpMethod.Connect, host.ToString());
        }

        request.Content = new StringContent(message.GetBody());

        var serverResponse = await SendToServer(stoppingToken, new HttpMessage(request));
        if (serverResponse is null) return;
        await SendToClient(client, stoppingToken, serverResponse);
    }

    private static bool TryExtractServerEndpoint(HttpMessage message, out IPEndPoint endpoint)
    {
        var serverUrl = ExtractHeader(message, HttpRequestHeader.Host);
        if (string.IsNullOrEmpty(serverUrl))
        {
            Log.Warning("No host header found, aborting request");
            endpoint = new IPEndPoint(IPAddress.Any, 0);
            return false;
        }


        endpoint = new IPEndPoint(IPAddress.Parse(serverUrl.Split(':')[0]), int.Parse(serverUrl.Split(':')[1]));
        return true;
    }

    private async Task<HttpMessage?> SendToServer(CancellationToken stoppingToken, HttpMessage message)
    {
        var server = new TcpClient();

        if (!TryExtractServerEndpoint(message, out var host)) return null;
        await server.ConnectAsync(host, stoppingToken);
        if (server.Connected) Log.Information("Connected to server");
        var serverStream = server.GetStream();
        await serverStream.WriteAsync(message.Buffer, stoppingToken);
        Log.Information("Wrote message to server:\r\n{Message}", message.GetHeader() + message.GetBody());
        var serverBuffer = new byte[1024];
        var serverBytes = await serverStream.ReadAsync(serverBuffer, stoppingToken);
        var serverMessage = new HttpMessage(serverBuffer, serverBytes);
        if (serverBytes > 0) Log.Information("Received bytes from server:\r\n{Message}", serverMessage.ToString());
        else Log.Information("Received no bytes from server");
        return serverMessage;
    }

    private static async Task SendToClient(TcpClient client, CancellationToken stoppingToken, HttpMessage message)
    {
        var clientStream = client.GetStream();
        await clientStream.WriteAsync(message.Buffer, stoppingToken);
    }

    public async ValueTask<Task?> StartProxy(CancellationToken cancellationToken)
    {
        _listener.Start();
        _config.PrintConfig();
        while (!cancellationToken.IsCancellationRequested)
        {
            var client = await _listener.AcceptTcpClientAsync(cancellationToken);
            _ = Task.Run(() => HandleClient(client, cancellationToken), cancellationToken);
        }

        return Task.CompletedTask;
    }
}