using System.Net;

namespace TCPProxyTest.Models;

public interface IDns
{
    IPHostEntry GetHostEntry(string hostNameOrAddress);
}