﻿using CliFx;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TCPProxy.Commands;
using TCPProxy.Services;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

var services = new ServiceCollection();

services.AddSingleton<SocketProxyServer>();

services.AddTransient<StartCommand>();

var serviceProvider = services.BuildServiceProvider();

return await new CliApplicationBuilder()
    .SetDescription("TCPProxy - A simple TCP proxy")
    .AddCommandsFromThisAssembly()
    .UseTypeActivator(serviceProvider.GetRequiredService)
    .Build()
    .RunAsync();