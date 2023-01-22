using System.Net;
using System.Net.Sockets;
using System.ServiceModel.Channels;
using System.Text;
using Serilog;

namespace TCPProxy.Providers;

public class ProxyProvider
{
    private readonly Socket _proxySocket = new(IPAddress.Any.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
    private readonly BufferManager _bufferManager = BufferManager.CreateBufferManager(100, 8192);

    public async ValueTask<Task> StartProxy(bool cache, bool maskImages, bool incognito, int port)
    {
        Log.Information("Starting proxy...");
        _proxySocket.Bind(new IPEndPoint(IPAddress.Any, port));
        _proxySocket.Listen(10);
        Log.Information("Proxy started with the following settings:");
        Log.Information("Cache: {Cache}", cache);
        Log.Information("MaskImages: {MaskImages}", maskImages);
        Log.Information("Private: {Incognito}", incognito);
        Log.Information("Proxy listening on {IPAddress}:{Port}", IPAddress.Any, port);

        while (true)
        {
            Socket clientSocket;
            try
            {
                clientSocket = await _proxySocket.AcceptAsync();
            }
            catch (ObjectDisposedException)
            {
                return Task.CompletedTask;
            }

            var buffer = _bufferManager.TakeBuffer(8192);
            var bytesReceived = await clientSocket.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
            var request = Encoding.ASCII.GetString(buffer, 0, bytesReceived);
            Log.Information("Request received: \n{Request}", request);
            var headers = request.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            var hostHeader = headers.FirstOrDefault(x => x.StartsWith("Host: "));
            var serverUrl = hostHeader?["Host: ".Length..];
            Log.Information("Request from {Client} to {Server}", clientSocket.RemoteEndPoint?.ToString(), serverUrl);

            if (serverUrl is null) break;
            if ((await Dns.GetHostEntryAsync(serverUrl)).AddressList.Length == 0)
            {
                Log.Error("No such host is known \"{ServerUrl}\"", serverUrl);
                StopProxy(false);
                return Task.CompletedTask;
            }

            var serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await serverSocket.ConnectAsync(serverUrl, 80);

            serverSocket.Send(buffer, 0, bytesReceived, SocketFlags.None);
            bytesReceived = serverSocket.Receive(buffer);
            clientSocket.Send(buffer, 0, bytesReceived, SocketFlags.None);
        }

        return Task.CompletedTask;
    }

    public void StopProxy(bool force)
    {
        if (force) _proxySocket.Shutdown(SocketShutdown.Both);
        _proxySocket.Close();
        Log.Information("Stopping proxy " + (force ? "with force" : ""));
    }
}