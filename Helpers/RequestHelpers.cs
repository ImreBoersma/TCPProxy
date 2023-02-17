using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Serilog;
using TCPProxy.Models;
using WebMarkupMin.Core;

namespace TCPProxy.Helpers;

public static partial class RequestHelper
{
    /// <summary>
    /// Extracts the header from the request
    /// </summary>
    /// <param name="httpMessage">The request</param>
    /// <param name="searchHeader">The header to search for</param>
    /// <returns>The header value</returns>
    public static string ExtractHeader(HttpMessage httpMessage, HttpRequestHeader searchHeader)
    {
        var header = new StringBuilder();
        var request = httpMessage.ToString();
        Log.Information("Request received: \n{Request}", request);

        var lines = request.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
        var headerLength = searchHeader.ToString().Length + 1;

        foreach (var line in lines)
        {
            if (!line.StartsWith(searchHeader + ":")) continue;
            header.Append(line[headerLength..].Trim());
            break;
        }

        return header.ToString();
    }


    public static HttpMessage MaskImage(HttpMessage input)
    {
        var html = new HtmlMinifier().Minify(input.ToString()).ToString();
        if (html is null) return input;
        var maskedHtml = SourceRegex().Replace(html, match =>
        {
            if (!match.Success)
            {
                
                return html;
            }
            var src = match.Groups["src"].Value;
            return $"src=\"https://via.placeholder.com/150\" data-original-src=\"{src}\"";
        });
        return new HttpMessage(maskedHtml);
    }

    [GeneratedRegex("(?<=src=\")([^\\\"']+(jpe?g|png|gif|bmp))", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-GB")]
    private static partial Regex SourceRegex();
}