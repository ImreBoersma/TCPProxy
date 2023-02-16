using Serilog;
using TCPProxy.Models;

namespace TCPProxy.Helpers;

public static class DebugHelper
{
    public static void PrintConfiguration(ProxyConfigurationModel configuration)
    {
        Log.Debug("Proxy Started with the following configuration:");
        Log.Debug("Cache: {Cache}", configuration.GetCache() ? "Enabled" : "Disabled");
        Log.Debug("Mask images: {MaskImages}", configuration.GetMaskImages() ? "Enabled" : "Disabled");
        Log.Debug("Incognito: {Incognito}", configuration.GetIncognito() ? "Enabled" : "Disabled");
        Log.Debug("Port: {Port}", configuration.GetPort());
    }
}