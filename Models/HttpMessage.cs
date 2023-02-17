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

    public byte[] Buffer { get; }
    public int Bytes { get; }

    /// <returns>The message as a string using ASCII encoding.</returns>
    public override string ToString()
    {
        return Encoding.ASCII.GetString(Buffer, 0, Bytes);
    }
}