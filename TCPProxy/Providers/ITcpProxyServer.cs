using TCPProxy.Models;

namespace TCPProxy.Providers;

public interface ITcpProxyServer
{
    ValueTask<Task?> StartProxy(ProxyConfigurationModel configuration, CancellationToken cancellationToken);
}