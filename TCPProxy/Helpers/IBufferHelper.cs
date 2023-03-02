using System.Net.Sockets;
using TCPProxy.Models;

namespace TCPProxy.Helpers;

public interface IBufferHelper
{
    /// <summary>
    ///   Sends a message to the socket
    /// </summary>
    /// <param name="socket"> The socket to send the message to </param>
    /// <param name="httpMessage"> The message to send </param>
    /// <param name="stoppingToken"> The cancellation token </param>
    /// <param name="offset"> The offset to start sending from </param>
    /// <returns> The number of bytes sent </returns>
    ValueTask<int> ExecuteSendAsync(Socket socket, HttpMessage httpMessage, CancellationToken stoppingToken, int offset = 0);

    /// <summary>
    ///    Receives a message from the socket
    /// </summary>
    /// <param name="socket"> The socket to receive the message from </param>
    /// <param name="stoppingToken"> The cancellation token </param>
    /// <param name="flags"> The socket flags </param>
    /// <param name="buffer"> The buffer to use if any</param>
    /// <returns> The received message </returns>
    ValueTask<HttpMessage> ExecuteReceiveAsync(Socket socket, CancellationToken stoppingToken, SocketFlags flags = SocketFlags.None, byte[]? buffer = null);

    /// <summary>
    /// Take a buffer from the buffer manager
    /// </summary>
    /// <param name="size">The size of the buffer to take</param>
    /// <returns>The buffer</returns>
    byte[] TakeBuffer(int size = 1024);

    /// <summary>
    /// Set the max buffer size
    /// </summary>
    /// <param name="size">The size of the buffer</param>
    void SetBufferSize(int size);
}