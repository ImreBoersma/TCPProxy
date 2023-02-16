using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using TCPProxy.Models;
using TCPProxy.Providers;

// Disable suggestion for initializing fields in constructor (injected by CliFx)
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

// Disable warning for unused private fields (injected by CliFx)
// ReSharper disable MemberCanBePrivate.Global

namespace TCPProxy.Commands;

[Command(name: "start", Description = "Starts the proxy")]
public class StartCommand : ICommand
{
    private readonly TcpProxyServer _tcpProxyServer;

    [CommandOption("cache", 'c', Description = "Enable cache")]
    public bool Cache { get; init; } = false;

    [CommandOption("mask", 'm', Description = "Enable replacing all images with a placeholder")]
    public bool MaskImages { get; init; } = false;

    [CommandOption("incognito", 'i',
        Description = "Enable incognito mode, removes all optional headers from the request")]
    public bool Incognito { get; init; } = false;

    [CommandOption("port", 'p', Description = "Port to listen on")]
    public ushort Port { get; init; } = 8080;

    public StartCommand(TcpProxyServer tcpProxyServer)
    {
        _tcpProxyServer = tcpProxyServer;
    }

    public async ValueTask ExecuteAsync(IConsole console)
    {
        var options = new ProxyConfigurationModel(Cache, MaskImages, Incognito, Port);
        await _tcpProxyServer.StartProxy(options, new CancellationToken(false));
    }
}