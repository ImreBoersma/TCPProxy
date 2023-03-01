using TCPProxy.Models;

namespace TCPProxyTest.Models;

public class ProxyConfigurationModelTests
{
    [Fact]
    public void CanCreateProxyConfigurationModel()
    {
        // Arrange
        const bool cache = true;
        const bool maskImages = false;
        const bool incognito = true;
        const ushort port = 1234;

        // Act
        var config = new ProxyConfigurationModel(cache, maskImages, incognito, port);

        // Assert
        Assert.Equal(cache, config.GetCache());
        Assert.Equal(maskImages, config.GetMaskImages());
        Assert.Equal(incognito, config.GetIncognito());
        Assert.Equal(port, config.GetPort());
    }

    [Fact]
    public void DefaultPortIs8080()
    {
        // Arrange
        const bool cache = true;
        const bool maskImages = false;
        const bool incognito = true;

        // Act
        var config = new ProxyConfigurationModel(cache, maskImages, incognito);

        // Assert
        Assert.Equal(8080, config.GetPort());
    }

    [Fact]
    public void CanGetProperties()
    {
        // Arrange
        const bool cache = true;
        const bool maskImages = false;
        const bool incognito = true;
        const ushort port = 1234;

        var config = new ProxyConfigurationModel(cache, maskImages, incognito, port);

        // Act & Assert
        Assert.Equal(cache, config.GetCache());
        Assert.Equal(maskImages, config.GetMaskImages());
        Assert.Equal(incognito, config.GetIncognito());
        Assert.Equal(port, config.GetPort());
    }
}