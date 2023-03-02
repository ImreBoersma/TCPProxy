using System.Text;

namespace TCPProxy.Models;

public class HttpMessage
{
    public HttpMessage(byte[] buffer, int bytes)
    {
        Buffer = buffer;
        Bytes = bytes;
    }

    public HttpMessage(string message)
    {
        Buffer = Encoding.ASCII.GetBytes(message);
        Bytes = Buffer.Length;
    }

    public HttpMessage(string header, string body)
    {
        Buffer = Encoding.ASCII.GetBytes(header + "\r\n\r\n" + body);
        Bytes = Buffer.Length;
    }

    public byte[] Buffer { get; }
    public int Bytes { get; }

    /// <returns>The message as a string using ASCII encoding.</returns>
    public override string ToString()
    {
        return Encoding.ASCII.GetString(Buffer, 0, Bytes);
    }

    /// <summary>
    /// Gets the header from the message
    /// </summary>
    /// <returns>The header</returns>
    public string GetHeader() => ToString()[..ToString().IndexOf("\r\n\r\n", StringComparison.Ordinal)];

    /// <summary>
    /// Gets the body from the message
    /// </summary>
    /// <returns>The body</returns>
    public string GetBody() => ToString()[(ToString().IndexOf("\r\n\r\n", StringComparison.Ordinal) + 4)..];
}