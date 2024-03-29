﻿using CliFx;
using CliFx.Attributes;
using CliFx.Infrastructure;
using TCPProxy.Models;
using TCPProxy.Services;

// Disable suggestion to make property get-only (injected by CliFx)
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

// Disable warning for unused private fields (injected by CliFx)
// ReSharper disable MemberCanBePrivate.Global

namespace TCPProxy.Commands;

[Command(name: "start", Description = "Starts the proxy")]
public class StartCommand : ICommand
{
    private readonly SocketProxyServer _socketProxyServer;

    [CommandOption("cache", 'c', Description = "Enable cache")]
    public bool Cache { get; init; } = false;

    [CommandOption("mask", 'm', Description = "Enable replacing all images with a placeholder")]
    public bool MaskImages { get; init; } = false;

    [CommandOption("incognito", 'i',
        Description = "Enable incognito mode, removes all optional headers from the request")]
    public bool Incognito { get; init; } = false;

    [CommandOption("port", 'p', Description = "Port to listen on")]
    public ushort Port { get; init; } = 8080;

    [CommandOption("buffer", 'b', Description = "Buffer size in bytes")]
    public int BufferSize { get; init; } = 1024;

    public StartCommand(SocketProxyServer socketProxyServer)
    {
        _socketProxyServer = socketProxyServer;
    }

    /// <summary>
    ///    Executes the command.
    /// </summary>
    /// <param name="console">Console to use for input/output.</param>
    public async ValueTask ExecuteAsync(IConsole console)
    {
        var options = new ProxyConfigurationModel(Cache, MaskImages, Incognito, BufferSize, Port);
        await _socketProxyServer.StartProxy(options, new CancellationToken(false));
    }
}