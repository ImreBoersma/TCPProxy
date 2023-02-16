using System.Net;
using System.Text;
using Serilog;

namespace TCPProxy.Helpers;

public static class RequestHelper
{
    public static string ExtractHeader(byte[] bufferReceived, int bytesReceived, HttpRequestHeader searchHeader)
    {
        var header = new StringBuilder();
        var request = Encoding.ASCII.GetString(bufferReceived, 0, bytesReceived);
        Log.Information("Request received: \n{Request}", request);

        var lines = request.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        var headerLength = searchHeader.ToString().Length + 1;

        foreach (var line in lines)
        {
            if (!line.StartsWith(searchHeader + ":")) continue;
            header.Append(line[headerLength..].Trim());
            break;
        }

        return header.ToString();
    }
}