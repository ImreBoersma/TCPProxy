using CliFx.Infrastructure;
using TCPProxy.Commands;
using TCPProxy.Models;
using TCPProxy.Providers;
using Moq;

namespace TCPProxyTest.Commands;

public class StartCommandTests
{
    // [Fact]
    // public async Task ExecuteAsync_CallsStartProxyWithCorrectOptions()
    // {
    //     // Arrange
    //     var tcpProxyServer = new Mock<TcpProxyServer>();
    //     var console = new Mock<IConsole>();
    //     var options = new ProxyConfigurationModel(true, false, true, 8888);
    //     var command = new StartCommand(tcpProxyServer.Object)
    //     {
    //         Cache = options.GetCache(),
    //         MaskImages = options.GetMaskImages(),
    //         Incognito = options.GetIncognito(),
    //         Port = options.GetPort()
    //     };
    //
    //     // Act
    //     await command.ExecuteAsync(console.Object);
    //
    //     // Assert
    //     tcpProxyServer.Verify(t => t.StartProxy(options, It.IsAny<CancellationToken>()), Times.Once);
    // }

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