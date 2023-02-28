using System.Net;
using System.Text;
using TCPProxy.Helpers;
using TCPProxy.Models;

namespace TCPProxyTest;

public class UnitTest1
{
    [Fact]
    public void ExtractHeader_ReturnsNullWhenSearchHeaderNotFound()
    {
        // Arrange
        var httpMessage = new HttpMessage("GET / HTTP/1.1\r\nHost: www.example.com\r\n\r\n");
        const HttpRequestHeader searchHeader = HttpRequestHeader.UserAgent;

        // Act
        var result = RequestHelper.ExtractHeader(httpMessage, searchHeader);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ExtractHeader_ReturnsNullWhenSearchHeaderIsInvalid()
    {
        // Arrange
        var httpMessage = new HttpMessage("GET / HTTP/1.1\r\nHost: www.example.com\r\n\r\n");
        const HttpRequestHeader searchHeader = (HttpRequestHeader)999;

        // Act
        var result = RequestHelper.ExtractHeader(httpMessage, searchHeader);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ExtractHeader_ReturnsHeaderWhenSearchHeaderIsFound()
    {
        // Arrange
        var httpMessage = new HttpMessage("GET / HTTP/1.1\r\nHost: www.example.com\r\n\r\n");
        const HttpRequestHeader searchHeader = HttpRequestHeader.Host;

        // Act
        var result = RequestHelper.ExtractHeader(httpMessage, searchHeader);

        // Assert
        Assert.Equal("www.example.com", result);
    }

    [Fact]
    public void MaskImage_ReturnsOriginalResponseOnError()
    {
        // Arrange
        var httpMessage = new HttpMessage("GET / HTTP/1.1\r\nHost: www.example.com\r\n\r\n");
        var originalBody = httpMessage.GetBody();
        var expected = new HttpMessage(httpMessage.GetHeader(), originalBody);

        // Act
        var result = RequestHelper.MaskImage(httpMessage);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Incognito_RemovesNonAllowedHeaders()
    {
        // Arrange
        var httpMessage = new HttpMessage(
            "HTTP/1.1 200 OK\r\n" +
            "Date: Mon, 20 Dec 2021 17:05:30 GMT\r\n" +
            "Server: Apache\r\n" +
            "Content-Type: text/html; charset=UTF-8\r\n" +
            "X-Custom-Header: test\r\n" +
            "Content-Length: 16\r\n" +
            "\r\n" +
            "<h1>Hello</h1>");

        var expectedHeader = new StringBuilder()
            .Append("HTTP/1.1 200 OK\r\n")
            .Append("Date: Mon, 20 Dec 2021 17:05:30 GMT\r\n")
            .Append("Server: Apache\r\n")
            .Append("Content-Type: text/html; charset=UTF-8\r\n")
            .ToString();

        // Act
        var result = RequestHelper.Incognito(httpMessage);

        // Assert
        Assert.Equal(expectedHeader + "<h1>Hello</h1>", result.ToString());
    }

    [Fact]
    public void Cache_ReturnsOriginalResponseWhenHostHeaderIsNotFound()
    {
        // Arrange
        var httpMessage = new HttpMessage("GET / HTTP/1.1\r\n\r\n");

        // Act
        var result = RequestHelper.Cache(httpMessage);

        // Assert
        Assert.Equal(httpMessage, result);
    }
}