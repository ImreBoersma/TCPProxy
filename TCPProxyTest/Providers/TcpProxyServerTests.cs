using System.Net;
using System.Net.Sockets;
using Moq;
using TCPProxy.Helpers;
using TCPProxy.Models;
using TCPProxy.Providers;

namespace TCPProxyTest.Providers;

public class TcpProxyServerTests
{
    private const string TestMessage =
        "HTTP/1.1 200 OK\r\nETag: \"1d98-5df1eea3666c0\"\r\nHost:\"127.0.0.1\"\r\n\r\n<!DOCTYPE html><html><body><h2>HTML Image</h2><img src=\"pic_trulli.jpg\"></body></html>";

    private const string TestMessageWithoutHostHeader =
        "HTTP/1.1 200 OK\r\nETag: \"1d98-5df1eea3666c0\"\r\n\r\n<!DOCTYPE html><html><body><h2>HTML Image</h2><img src=\"pic_trulli.jpg\"></body></html>";

    [Fact]
    public void ProcessResponse_ReturnsOriginalMessage_WhenAllFlagsAreFalse()
    {
        // Arrange
        var message = new HttpMessage(TestMessage);
        var config = new ProxyConfigurationModel(false, false, false);

        // Act
        var result = TcpProxyServer.ProcessResponse(message, config);

        // Assert
        Assert.Same(message, result);
    }

    [Fact]
    public void ProcessResponse_MasksImages_WhenConfigFlagIsTrue()
    {
        // Arrange
        var message = new HttpMessage(TestMessage);
        var config = new ProxyConfigurationModel(false, true, false);

        // Act
        var result = TcpProxyServer.ProcessResponse(message, config);

        // Assert
        Assert.NotSame(message, result);
        Assert.DoesNotContain(message.ToString(), "pic_trulli.jpg");
    }

    [Fact]
    public void ProcessResponse_IncognitoMode_WhenConfigFlagIsTrue()
    {
        // Arrange
        var message = new HttpMessage(TestMessage);
        var config = new ProxyConfigurationModel(false, false, true);

        // Act
        var result = TcpProxyServer.ProcessResponse(message, config);

        // Assert
        Assert.NotSame(message, result);
        Assert.DoesNotContain(message.ToString(), "ETag");
    }


    [Fact]
    public void ProcessResponse_CachesResponse_WhenConfigFlagIsTrue()
    {
        // Arrange
        var message = new HttpMessage(TestMessage);
        var config = new ProxyConfigurationModel(true, false, false);


        // Act
        var result = TcpProxyServer.ProcessResponse(message, config);

        // Assert
        Assert.Same(message, result);
    }

    [Fact]
    public void TryExtractServerEndpoint_ReturnsFalse_WhenHostHeaderIsMissing()
    {
        // Arrange
        var message = new HttpMessage(TestMessageWithoutHostHeader);

        // Act
        var result = TcpProxyServer.TryExtractServerEndpoint(message, out var endpoint);

        // Assert
        Assert.False(result);
        Assert.Equal(IPAddress.Any, endpoint.Address);
        Assert.Equal(0, endpoint.Port);
    }

    [Fact]
    public void TryExtractServerEndpoint_ReturnsTrue_WhenHostHeaderIsPresent()
    {
        // Arrange
        var message = new HttpMessage(TestMessage);

        // Act
        var result = TcpProxyServer.TryExtractServerEndpoint(message, out var endpoint);

        // Assert
        Assert.True(result);
        Assert.Equal(IPAddress.Parse("127.0.0.1"), endpoint.Address);
        Assert.Equal(80, endpoint.Port);
    }

    private readonly Mock<IBufferHelper> _bufferHelperMock;

    public TcpProxyServerTests()
    {
        _bufferHelperMock = new Mock<IBufferHelper>();
    }

    [Fact]
    public async Task ConnectToServerAsync_ShouldCreateSocketAndConnectToServer()
    {
        // Arrange
        var endpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 80);
        var cancellationToken = new CancellationToken(false);

        var expectedSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        await expectedSocket.ConnectAsync(endpoint, cancellationToken);

        var buffer = new byte[1024];
        _bufferHelperMock.Setup(x => x.TakeBuffer(It.IsAny<int>())).Returns(buffer);

        // Act
        var actualSocket = await TcpProxyServer.ConnectToServerAsync(endpoint, cancellationToken);

        // Assert
        Assert.Equal(expectedSocket.RemoteEndPoint?.ToString(), actualSocket.RemoteEndPoint?.ToString());
    }
}