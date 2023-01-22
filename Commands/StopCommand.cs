using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using TCPProxy.Providers;

namespace TCPProxy.Commands;

[Command("stop", Description = "Stop the proxy")]
public class StopCommand : ICommand
{
    private readonly ProxyProvider _proxyProvider;

    [CommandOption("force", 'f', Description = "Force stop the proxy")]
    public bool Force { get; init; } = false;

    public StopCommand(ProxyProvider proxyProvider)
    {
        _proxyProvider = proxyProvider;
    }

    public ValueTask ExecuteAsync(IConsole console)
    {
        _proxyProvider.StopProxy(Force);
        return default;
    }
}