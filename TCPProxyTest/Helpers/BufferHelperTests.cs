using System.Net;
using System.Net.Sockets;
using System.Text;
using TCPProxy.Helpers;
using System;

namespace TCPProxyTest.Helpers;

public class BufferHelperTests
{
    [Fact]
    public async Task ExecuteReceiveAsync_Should_Return_HttpMessage()
    {
        // Arrange
        var bufferHelper = new BufferHelper();
        var server = new TcpListener(IPAddress.Any, 8000);
        server.Start();

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, 8000);

        var request = "GET / HTTP/1.1\r\nHost: localhost\r\n\r\n"u8.ToArray();
        await client.GetStream().WriteAsync(request);

        using var socket = await server.AcceptSocketAsync();
        var tokenSource = new CancellationTokenSource();

        // Act
        var received = await bufferHelper.ExecuteReceiveAsync(socket, tokenSource.Token);

        // Assert
        Assert.NotNull(received);
        Assert.NotEmpty(received.Buffer);

        // Cleanup
        tokenSource.Cancel();
        server.Stop();
    }

    [Fact]
    public async Task ExecuteSendAsync_Should_Send_HttpMessage()
    {
        // Arrange
        var bufferHelper = new BufferHelper();
        var server = new TcpListener(IPAddress.Any, 8000);
        server.Start();

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, 8000);

        var request = "GET / HTTP/1.1\r\nHost: localhost\r\n\r\n"u8.ToArray();
        await client.GetStream().WriteAsync(request);

        using var socket = await server.AcceptSocketAsync();
        var tokenSource = new CancellationTokenSource();
        var received = await bufferHelper.ExecuteReceiveAsync(socket, tokenSource.Token);

        // Act
        var sent = await bufferHelper.ExecuteSendAsync(socket, received, tokenSource.Token);

        // Assert
        Assert.True(sent > 0);

        // Cleanup
        tokenSource.Cancel();
        server.Stop();
    }

    [Fact]
    public void TakeBuffer_Should_Return_Requested_Size()
    {
        // Arrange
        var bufferHelper = new BufferHelper();

        // Act
        var buffer = bufferHelper.TakeBuffer();

        // Assert
        Assert.NotNull(buffer);
        Assert.Equal(1024, buffer.Length);
    }
}