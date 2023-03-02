using System.Net;
using System.Net.Sockets;
using Serilog;

namespace TCPProxy.Models;

public record Endpoint
{
    public Endpoint(string addressString, ushort port = 80)
    {
        IPAddress? address = null;

        if (!string.IsNullOrWhiteSpace(addressString))
        {
            var splitAddress = addressString;
            if (addressString.Contains(':'))
            {
                splitAddress = addressString.Split(':')[0];
            }

            if (!IPAddress.TryParse(splitAddress, out address))
            {
                try
                {
                    var addresses = Dns.GetHostAddresses(splitAddress);
                    address = addresses.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
                }
                catch (SocketException e)
                {
                    Log.Error(e, "Failed to resolve host {Host}", splitAddress);
                    return;
                }
            }
        }

        if (address == null)
        {
            Log.Warning("Invalid address, defaulting to localhost:{Port}", port);
            Port = port;
            return;
        }

        Host = address;
        Port = port;
    }

    public IPAddress Host { get; } = new(new byte[] {127, 0, 0, 1});
    public ushort Port { get; } = 80;

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
            return new IPEndPoint(address, endpoint.Port);
        }

        return new IPEndPoint(IPAddress.Loopback, endpoint.Port);
    }
}