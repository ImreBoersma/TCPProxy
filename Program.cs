﻿using CliFx;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TCPProxy;
using TCPProxy.Commands;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

var services = new ServiceCollection();

services.AddSingleton<ProxyProvider>();

services.AddTransient<StartCommand>();

var serviceProvider = services.BuildServiceProvider();

return await new CliApplicationBuilder()
    .SetDescription("TCPProxy - A simple TCP proxy")
    .AddCommandsFromThisAssembly()
    .UseTypeActivator(serviceProvider.GetRequiredService)
    .Build()
    .RunAsync();