using System.Net;
using TCPProxy.Models;

namespace TCPProxyTest.Models;

public class EndpointTests
{
    [Theory]
    [InlineData("localhost", "localhost:80")]
    [InlineData("127.0.0.1", "127.0.0.1:80")]
    [InlineData("localhost:8080", "localhost:8080")]
    [InlineData("127.0.0.1:8080", "127.0.0.1:8080")]
    public void Endpoint_ConstructorWithHostAndPort_ReturnsCorrectValues(string hostAndPort, string expectedToString)
    {
        // Arrange

        // Act
        var endpoint = new Endpoint(hostAndPort);

        // Assert
        Assert.Equal(expectedToString, endpoint.ToString());
    }

    [Fact]
    public void Endpoint_ConstructorWithIPAddress_ReturnsCorrectValues()
    {
        // Arrange
        var address = IPAddress.Parse("127.0.0.1");

        // Act
        var endpoint = new Endpoint(address);

        // Assert
        Assert.Equal("127.0.0.1:8080", endpoint.ToString());
    }

    [Fact]
    public void Endpoint_ConstructorWithIPAddressAndPort_ReturnsCorrectValues()
    {
        // Arrange
        var address = IPAddress.Parse("127.0.0.1");
        const ushort port = 1234;

        // Act
        var endpoint = new Endpoint(address, port);

        // Assert
        Assert.Equal("127.0.0.1:1234", endpoint.ToString());
    }

    [Fact]
    public void Endpoint_ImplicitOperator_ReturnsValidIPEndPoint()
    {
        // Arrange
        var endpoint = new Endpoint("localhost:80");

        // Act
        var ipEndPoint = (IPEndPoint)endpoint;

        // Assert
        Assert.NotNull(ipEndPoint);
        Assert.Equal(IPAddress.Loopback, ipEndPoint.Address);
        Assert.Equal(80, ipEndPoint.Port);
    }

    [Fact]
    public void Endpoint_ConstructorWithHostAndPort_WithEmptyString_ShouldSetHostTo_localhost_AndPortTo_80()
    {
        // Arrange
        var endpoint = new Endpoint("");

        // Assert
        Assert.Equal("localhost", endpoint.Host);
        Assert.Equal((ushort)80, endpoint.Port);
    }

    [Fact]
    public void Endpoint_ConstructorWithHostAndPort_WithInvalidString_ShouldSetHostTo_localhost_AndPortTo_80()
    {
        // Arrange
        var endpoint = new Endpoint("invalid-host-and-port");

        // Assert
        Assert.Equal("localhost", endpoint.Host);
        Assert.Equal((ushort)80, endpoint.Port);
    }
}