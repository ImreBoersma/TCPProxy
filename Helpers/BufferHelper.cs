using System.Net.Sockets;
using System.ServiceModel.Channels;
using TCPProxy.Models;

namespace TCPProxy.Helpers;

public class BufferHelper
{
    private readonly BufferManager _bufferManager;

    public BufferHelper(int mtu)
    {
        _bufferManager = BufferManager.CreateBufferManager(100, mtu);
    }

    public delegate ValueTask<HttpMessage> ReceiveDelegate(Socket clientSocket, byte[] buffer,
        CancellationToken stoppingToken,
        SocketFlags socketFlags = SocketFlags.None);

    public delegate ValueTask<int> SendDelegate(Socket clientSocket, byte[] buffer, int count,
        CancellationToken stoppingToken, int offset = 0);

    public static async ValueTask<HttpMessage> ReceiveMethod(Socket socket, byte[] buffer,
        CancellationToken stoppingToken,
        SocketFlags flags = SocketFlags.None)
    {
        var bytesReceived = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), flags, stoppingToken);
        return new HttpMessage(buffer, bytesReceived);
    }

    public static ValueTask<int> SendMethod(Socket socket, byte[] buffer, int count, CancellationToken stoppingToken,
        int offset = 0) => socket.SendAsync(new ArraySegment<byte>(buffer, offset, count), stoppingToken);

    public async ValueTask<HttpMessage> ExecuteReceiveAsync(ReceiveDelegate receiveMethod, Socket clientSocket,
        byte[] buffer,
        CancellationToken stoppingToken)
    {
        var httpMessage = await receiveMethod(clientSocket, buffer, stoppingToken);
        _bufferManager.ReturnBuffer(buffer);
        return new HttpMessage(buffer, httpMessage.Bytes);
    }

    public static async ValueTask<int> ExecuteSendAsync(SendDelegate sendMethod, Socket clientSocket, byte[] buffer,
        int count, CancellationToken stoppingToken, int offset = 0) =>
        await sendMethod(clientSocket, buffer, count, stoppingToken, offset);

    public byte[] TakeBuffer(int size = 1024) => _bufferManager.TakeBuffer(size);
}