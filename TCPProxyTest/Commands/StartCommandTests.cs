using CliFx.Infrastructure;
using TCPProxy.Commands;
using TCPProxy.Models;
using TCPProxy.Providers;
using Moq;

namespace TCPProxyTest.Commands;

public class StartCommandTests
{
    [Fact]
    public void Constructor_InitializesProperties()
    {
        // Arrange
        var tcpProxyServer = new Mock<TcpProxyServer>();

        // Act
        var command = new StartCommand(tcpProxyServer.Object);

        // Assert
        Assert.NotNull(command);
        Assert.False(command.Cache);
        Assert.False(command.MaskImages);
        Assert.False(command.Incognito);
        Assert.Equal(8080, command.Port);
    }
}