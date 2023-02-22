using System.Net.Sockets;
using System.ServiceModel.Channels;
using TCPProxy.Models;

namespace TCPProxy.Helpers;

public class BufferHelper
{
    private readonly BufferManager _bufferManager;

    public BufferHelper(int mtu) => _bufferManager = BufferManager.CreateBufferManager(100, mtu);

    public async ValueTask<HttpMessage> ExecuteReceiveAsync(Socket socket, byte[] buffer, CancellationToken stoppingToken, SocketFlags flags = SocketFlags.None)
    {
        var bytesReceived = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), flags, stoppingToken);
        _bufferManager.ReturnBuffer(buffer);
        return new HttpMessage(buffer, bytesReceived);
    }

    public static async ValueTask<int> ExecuteSendAsync(Socket socket, byte[] buffer, int count, CancellationToken stoppingToken, int offset = 0) =>
        await socket.SendAsync(new ArraySegment<byte>(buffer, offset, count), stoppingToken);

    public byte[] TakeBuffer(int size = 1024) => _bufferManager.TakeBuffer(size);
}