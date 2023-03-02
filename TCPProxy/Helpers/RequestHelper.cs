using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Serilog;
using TCPProxy.Models;

namespace TCPProxy.Helpers;

public abstract partial class RequestHelper
{
    /// <summary>
    /// Extracts the header from the request
    /// </summary>
    /// <param name="httpMessage">The request</param>
    /// <param name="searchHeader">The header to search for</param>
    /// <returns>The header value</returns>
    public static string? ExtractHeader(HttpMessage httpMessage, HttpRequestHeader searchHeader)
    {
        try
        {
            var match = Regex.Match(httpMessage.ToString(), $"{searchHeader}:\\s*(.*)\r\n");
            return match.Success ? match.Groups[1].Value : null;
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to extract header with error: {Error}, returning null", e.Message);
            return null;
        }
    }

    /// <summary>
    /// Masks all images in the response with a placeholder image
    /// </summary>
    /// <param name="input">The response</param>
    /// <returns>The masked response</returns>
    public static HttpMessage MaskImage(HttpMessage input)
    {
        var body = input.GetBody();
        try
        {
            var maskedHtml = SourceRegex().Replace(body, match => !match.Success ? body : "https://via.placeholder.com/150");
            var updatedHeader = ContentLengthRegex().Replace(input.GetHeader(), $"Content-Length: {Encoding.ASCII.GetBytes(maskedHtml).Length}");
            return new HttpMessage(updatedHeader, maskedHtml);
        }
        catch (Exception e)
        {
            Log.Error(e, "Failed to mask image with error: {Error}, returning original response", e.Message);
            return input;
        }
    }

    public static HttpMessage Incognito(HttpMessage input)
    {
        var allowedHeaders = new HashSet<string> {"Date", "Server", "Content-Type"};

        var header = new StringBuilder();

        Log.Information("Attempting to remove headers from response");

        var lines = input.GetHeader().Split("\r\n");

        foreach (var line in lines)
        {
            try
            {
                if (line.StartsWith("HTTP/")) header.Append($"{line}\r\n");
                if (!allowedHeaders.Contains(line.Split(':')[0])) continue;
                header.Append($"{line}\r\n");
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to remove header with error: {Error}, returning original response", e.Message);
                return input;
            }
        }

        return new HttpMessage(header.ToString(), input.GetBody());
    }

    public static HttpMessage Cache(HttpMessage input)
    {
        var cacheHelper = new CacheHelper();

        var id = ExtractHeader(input, HttpRequestHeader.Host);
        if (id is not null)
        {
            cacheHelper.Add("", input);
            return cacheHelper.Get<HttpMessage>(id) is not null ? cacheHelper.Get<HttpMessage>(id)! : input;
        }

        return input;
    }

    [GeneratedRegex("(?<=src=\")([^\\\"']+(jpe?g|png|gif|bmp))", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-GB")]
    private static partial Regex SourceRegex();

    [GeneratedRegex("(Content-Length:\\s*)(\\d+)")]
    private static partial Regex ContentLengthRegex();
}