using System.Net.Sockets;
using TCPProxy.Models;

namespace TCPProxy.Helpers;

public interface IBufferHelper
{
    ValueTask<int> ExecuteSendAsync(Socket socket, HttpMessage httpMessage, CancellationToken stoppingToken, int offset = 0);
    ValueTask<HttpMessage> ExecuteReceiveAsync(Socket socket, CancellationToken stoppingToken, SocketFlags flags = SocketFlags.None, byte[]? buffer = null);
    byte[] TakeBuffer(int size = 1024);
    void SetBufferSize(int size);
}