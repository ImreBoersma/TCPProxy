using System.Net.Sockets;
using System.ServiceModel.Channels;
using TCPProxy.Models;

namespace TCPProxy.Helpers;

public class BufferHelper : IBufferHelper
{
    private readonly BufferManager _bufferManager;
    private int _maxBufferSize = 8192;

    public BufferHelper(int mtu = default) => _bufferManager = BufferManager.CreateBufferManager(100, mtu <= _maxBufferSize ? mtu : _maxBufferSize);

    public async ValueTask<HttpMessage> ExecuteReceiveAsync(Socket socket, CancellationToken stoppingToken, SocketFlags flags = SocketFlags.None, byte[]? buffer = null)
    {
        buffer ??= _bufferManager.TakeBuffer(_maxBufferSize);
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
    public void SetBufferSize(int size) => _maxBufferSize = size;
}