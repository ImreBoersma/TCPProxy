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
                    Log.Error("Failed to resolve host {Host}:\r\n{Exception}", splitAddress, e.Message);
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

    /// <summary>
    /// Implicitly convert an Endpoint to an IPEndPoint. This is used to convert the endpoint to an address that can be used to connect to.
    /// </summary>
    /// <param name="endpoint">The endpoint to convert</param>
    /// <returns>The converted endpoint</returns>
    public static implicit operator IPEndPoint?(Endpoint endpoint)
    {
        try
        {
            var hostEntry = Dns.GetHostEntry(endpoint.Host);
            var ipAddresses = hostEntry.AddressList.Where(a => a.AddressFamily == AddressFamily.InterNetwork).ToList();

            if (!ipAddresses.Any())
            {
                Log.Warning("No valid addresses found for {Host}", endpoint.Host);
            }

            if (ipAddresses.Count == 1) return new IPEndPoint(ipAddresses.First(), endpoint.Port);

            Log.Warning("\tMultiple addresses found for {Host}:", endpoint.Host);
            foreach (var (address, index) in ipAddresses.Select((v, i) => (v, i)))
            {
                var connected = TryConnectToEndpoint(new IPEndPoint(address, endpoint.Port));
                Log.Debug("\t\t{Index}: {Address} - {Connected}", index, address.ToString(), connected ? "Connected" : "Not connected");
                if (connected)
                {
                    Log.Debug("\t\tSelected {Address} to forward request to", address.ToString());
                    return new IPEndPoint(address, endpoint.Port);
                }
            }
        }
        catch (Exception e)
        {
            Log.Error("Failed to resolve host {Host}:\r\n{Exception}", endpoint.Host, e.Message);
        }

        return null;
    }

    private static bool TryConnectToEndpoint(IPEndPoint endpoint)
    {
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            socket.Connect(endpoint);
            return true;
        }
        catch (Exception e)
        {
            Log.Error("Error connecting to server {ServerIp}\r\n{Exception}", endpoint.ToString(), e.Message);
        }

        return false;
    }
}