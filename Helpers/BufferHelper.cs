using System.Net.Sockets;
using System.ServiceModel.Channels;

namespace TCPProxy.Helpers;

public class BufferHelper
{
    private readonly BufferManager _bufferManager;

    public BufferHelper(int mtu)
    {
        _bufferManager = BufferManager.CreateBufferManager(100, mtu);
    }

    public delegate ValueTask<(byte[]?, int)> ReceiveDelegate(Socket clientSocket, byte[] buffer,
        CancellationToken stoppingToken,
        SocketFlags socketFlags = SocketFlags.None);

    public delegate ValueTask<int> SendDelegate(Socket clientSocket, byte[] buffer, int count,
        CancellationToken stoppingToken, int offset = 0);

    public static async ValueTask<(byte[]?, int)> ReceiveMethod(Socket socket, byte[] buffer,
        CancellationToken stoppingToken,
        SocketFlags flags = SocketFlags.None)
    {
        var bytesReceived = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), flags, stoppingToken);
        return (buffer, bytesReceived);
    }

    public static ValueTask<int> SendMethod(Socket socket, byte[] buffer, int count, CancellationToken stoppingToken,
        int offset = 0) => socket.SendAsync(new ArraySegment<byte>(buffer, offset, count), stoppingToken);

    public async ValueTask<(byte[]?, int)> ExecuteReceiveAsync(ReceiveDelegate receiveMethod, Socket clientSocket,
        byte[] buffer,
        CancellationToken stoppingToken)
    {
        var (_, bytesReceived) = await receiveMethod(clientSocket, buffer, stoppingToken);
        _bufferManager.ReturnBuffer(buffer);
        return (buffer, bytesReceived);
    }

    public static async ValueTask<int> ExecuteSendAsync(SendDelegate sendMethod, Socket clientSocket, byte[] buffer,
        int count, CancellationToken stoppingToken, int offset = 0) =>
        await sendMethod(clientSocket, buffer, count, stoppingToken, offset);

    public byte[] TakeBuffer(int size = 1024) => _bufferManager.TakeBuffer(size);
}