using System.Net.Sockets;
using System.ServiceModel.Channels;
using TCPProxy.Models;

namespace TCPProxy.Helpers;

public class BufferHelper
{
    private readonly BufferManager _bufferManager;

    public BufferHelper(int mtu = 8192) => _bufferManager = BufferManager.CreateBufferManager(100, mtu);

    public async ValueTask<HttpMessage> ExecuteReceiveAsync(Socket socket, CancellationToken stoppingToken, SocketFlags flags = SocketFlags.None, byte[]? buffer = null)
    {
        buffer ??= _bufferManager.TakeBuffer(8192);
        var buff = _bufferManager.TakeBuffer(buffer.Length);

        try
        {
            var bytesReceived = await socket.ReceiveAsync(new ArraySegment<byte>(buff), flags, stoppingToken);
            return new HttpMessage(buff, bytesReceived);
        }
        finally
        {
            _bufferManager.ReturnBuffer(buff);
        }
    }

    public async ValueTask<int> ExecuteSendAsync(Socket socket, HttpMessage httpMessage, CancellationToken stoppingToken, int offset = 0)
    {
        var buffer = httpMessage.Buffer;
        try
        {
            var bytesSent = await socket.SendAsync(new ArraySegment<byte>(buffer, offset, httpMessage.Bytes), stoppingToken);
            return bytesSent;
        }
        finally
        {
            _bufferManager.ReturnBuffer(buffer);
        }
    }

    public byte[] TakeBuffer(int size = 1024) => _bufferManager.TakeBuffer(size);
}