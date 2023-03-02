using System.Net;
using System.Net.Sockets;
using TCPProxy.Models;

namespace TCPProxyTest.Models;

public class EndpointTests
{
    [Theory]
    [InlineData("192.168.0.1", 80)]
    [InlineData("255.255.255.255", 8080)]
    public void Constructor_IPAddress_Valid(string hostAndPort, ushort port)
    {
        // Arrange
        var address = IPAddress.Parse(hostAndPort);

        // Act
        var endpoint = new Endpoint(hostAndPort, port);

        // Assert
        Assert.Equal(address, endpoint.Host);
        Assert.Equal(port, endpoint.Port);
    }

    [Theory]
    [InlineData("harmonieorkestbrummen.nl", 80)]
    [InlineData("portquiz.net", 8080)]
    public void Constructor_HostName_Valid(string hostAndPort, ushort port)
    {
        // Arrange
        var address = Dns.GetHostEntry(hostAndPort.Split(':')[0]).AddressList.First(a => a.AddressFamily == AddressFamily.InterNetwork);

        // Act
        var endpoint = new Endpoint(hostAndPort, port);

        // Assert
        Assert.Equal(address, endpoint.Host);
        Assert.Equal(port, endpoint.Port);
    }

    [Fact]
    public void Constructor_NullOrWhiteSpaceHostAndPort_DefaultsToLocalhost()
    {
        // Arrange

        // Act
        var endpoint = new Endpoint(string.Empty);

        // Assert
        Assert.Equal(IPAddress.Loopback, endpoint.Host);
        Assert.Equal((ushort)80, endpoint.Port);
    }

    [Fact]
    public void Constructor_UnknownHostName_DefaultsToLocalhost()
    {
        // Arrange

        // Act
        var endpoint = new Endpoint("not.a.valid.hostname:1234");

        // Assert
        Assert.Equal(IPAddress.Loopback, endpoint.Host);
        Assert.Equal((ushort)80, endpoint.Port);
    }

    [Theory]
    [InlineData("127.0.0.1:1234", 1234)]
    [InlineData("10.0.0.1", 80)]
    public void Constructor_HostAndPort_Valid(string hostAndPort, ushort port)
    {
        // Arrange
        var expectedHost = hostAndPort.Split(':')[0];

        // Act
        var endpoint = new Endpoint(hostAndPort, port);

        // Assert
        Assert.Equal(expectedHost, endpoint.Host.ToString());
        Assert.Equal(port, endpoint.Port);
    }

    [Fact]
    public void Constructor_InvalidPort_DefaultsTo80()
    {
        // Arrange

        // Act
        var endpoint = new Endpoint("127.0.0.1:not-a-port");

        // Assert
        Assert.Equal(IPAddress.Loopback, endpoint.Host);
        Assert.Equal((ushort)80, endpoint.Port);
    }

    [Fact]
    public void Endpoint_ImplicitlyConvertedToIPEndPoint_CreatesIPEndPointWithCorrectValues()
    {
        // Arrange
        var endpoint = new Endpoint("127.0.0.1", 8080);

        // Act
        var ipEndPoint = (IPEndPoint)endpoint;

        // Assert
        Assert.Equal(endpoint.Host, ipEndPoint.Address);
        Assert.Equal(endpoint.Port, ipEndPoint.Port);
    }

    [Theory]
    [InlineData("localhost", 80)]
    [InlineData("127.0.0.1", 8080)]
    [InlineData("216.239.32.8", 80)]
    public void ImplicitOperator_ConvertsToEndPoint(string host, ushort port)
    {
        // Arrange
        var endpoint = new Endpoint(host, port);

        // Act
        var ipEndPoint = (IPEndPoint)endpoint;

        // Assert
        Assert.NotNull(ipEndPoint);
        Assert.Equal(endpoint.Port, ipEndPoint.Port);
        Assert.Equal(endpoint.Host, ipEndPoint.Address);
    }

    [Fact]
    public void ImplicitOperator_HostHasNoValidAddress_ReturnsDefaultEndPoint()
    {
        // Arrange
        var endpoint = new Endpoint("invalid-host-name");

        // Act
        var ipEndPoint = (IPEndPoint)endpoint;

        // Assert
        Assert.NotNull(ipEndPoint);
        Assert.Equal(80, ipEndPoint.Port);
        Assert.Equal(IPAddress.Loopback, ipEndPoint.Address);
    }

    [Fact]
    public void ToString_ReturnsExpectedString()
    {
        // Arrange
        var endpoint = new Endpoint("127.0.0.1");

        // Act
        var result = endpoint.ToString();

        // Assert
        Assert.Equal("127.0.0.1:80", result);
    }
}