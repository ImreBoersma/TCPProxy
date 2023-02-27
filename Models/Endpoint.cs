using System.Net;
using System.Net.Sockets;
using Serilog;

namespace TCPProxy.Models;

public record Endpoint
{
    public Endpoint(string hostAndPort)
    {
        var parts = hostAndPort.Split(':');
        switch (parts.Length)
        {
            case 1:
                Host = hostAndPort;
                Port = 80;
                break;
            case 2:
                Host = parts[0];
                Port = ushort.TryParse(parts[1], out var port) ? port : (ushort)80;
                break;
            default:
                Log.Warning("Invalid host and port: {HostAndPort}", hostAndPort);
                Host = "localhost";
                Port = 80;
                break;
        }
    }

    public Endpoint(IPAddress address, ushort port = 8080)
    {
        Host = address.ToString();
        Port = port;
    }

    private string Host { get; }
    private ushort Port { get; }

    public override string ToString()
    {
        return $"{Host}:{Port}";
    }

    public static implicit operator IPEndPoint(Endpoint endpoint)
    {
        var hostEntry = Dns.GetHostEntry(endpoint.Host);
        var ipAddresses = hostEntry.AddressList.Where(a => a.AddressFamily == AddressFamily.InterNetwork).ToList();

        if (!ipAddresses.Any())
        {
            Log.Warning("No valid addresses found for {Host}", endpoint.Host);
            return new IPEndPoint(IPAddress.Loopback, 80);
        }

        if (ipAddresses.Count == 1)
        {
            var address = ipAddresses.First();
            Log.Debug("Selected {Address} to forward request to", address.ToString());
            return new IPEndPoint(address, endpoint.Port);
        }

        Log.Warning("Multiple addresses found for {Host}:", endpoint.Host);
        foreach (var (address, index) in ipAddresses.Select((v, i) => (v, i)))
        {
            Log.Debug("{Index}: {Address}", index, address.ToString());
        }

        return new IPEndPoint(IPAddress.Loopback, 8080);
    }
}