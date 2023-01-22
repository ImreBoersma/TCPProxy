using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using TCPProxy.Providers;

namespace TCPProxy.Commands;

[Command(name: "start", Description = "Starts the proxy")]
public class StartCommand : ICommand
{
    private readonly ProxyProvider _proxyProvider;

    [CommandOption("cache", 'c', Description = "Enable cache")]
    public bool Cache { get; init; } = false;

    [CommandOption("mask", 'm', Description = "Enable replacing all images with a placeholder")]
    public bool MaskImages { get; init; } = false;

    [CommandOption("incognito", 'i',
        Description = "Enable incognito mode, removes all optional headers from the request")]
    public bool Incognito { get; init; } = false;

    [CommandOption("port", 'p', Description = "Port to listen on")]
    public int Port { get; init; } = 8080;

    public StartCommand(ProxyProvider proxyProvider)
    {
        _proxyProvider = proxyProvider;
    }

    public async ValueTask ExecuteAsync(IConsole console)
    {
        await _proxyProvider.StartProxy(Cache, MaskImages, Incognito, Port);
    }
}