using TCPProxy.Models;

namespace TCPProxyTest.Models;

public class HttpMessageTests
{
    private static readonly byte[] HelloWorld = "Hello, World!"u8.ToArray();

    [Fact]
    public void ToString_ShouldReturnString()
    {
        // Arrange
        var message = new HttpMessage(HelloWorld, HelloWorld.Length);

        // Act
        var result = message.ToString();

        // Assert
        Assert.Equal("Hello, World!", result);
    }

    [Fact]
    public void GetHeader_ShouldReturnHeader()
    {
        // Arrange
        var message = new HttpMessage("Content-Type: text/html\r\n\r\n<html><body><h1>Hello, World!</h1></body></html>");

        // Act
        var result = message.GetHeader();

        // Assert
        Assert.Equal("Content-Type: text/html", result);
    }

    [Fact]
    public void GetBody_ShouldReturnBody()
    {
        // Arrange
        var message = new HttpMessage("Content-Type: text/html\r\n\r\n<html><body><h1>Hello, World!</h1></body></html>");

        // Act
        var result = message.GetBody();

        // Assert
        Assert.Equal("<html><body><h1>Hello, World!</h1></body></html>", result);
    }

    [Fact]
    public void Constructor_WithBufferAndBytes_ShouldInitializeProperties()
    {
        // Arrange
        var buffer = HelloWorld;
        var bytes = buffer.Length;

        // Act
        var message = new HttpMessage(buffer, bytes);

        // Assert
        Assert.Equal(buffer, message.Buffer);
        Assert.Equal(bytes, message.Bytes);
    }

    [Fact]
    public void Constructor_WithString_ShouldInitializeProperties()
    {
        // Arrange
        const string messageString = "Content-Type: text/html\r\n\r\n<html><body><h1>Hello, World!</h1></body></html>";

        // Act
        var message = new HttpMessage(messageString);

        // Assert
        Assert.Equal(messageString, message.ToString());
    }

    [Fact]
    public void Constructor_WithHeaderAndBody_ShouldInitializeProperties()
    {
        // Arrange
        const string header = "Content-Type: text/html";
        const string body = "<html><body><h1>Hello, World!</h1></body></html>";

        // Act
        var message = new HttpMessage(header, body);

        // Assert
        Assert.Equal($"{header}\r\n\r\n{body}", message.ToString());
    }
}