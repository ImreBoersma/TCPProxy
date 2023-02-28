using System.Collections;

namespace TCPProxy.Models;

public record ProxyConfigurationModel
{
    private bool Cache { get; }
    private bool MaskImages { get; }
    private bool Incognito { get; }
    private ushort Port { get; } = 8080;

    public ProxyConfigurationModel(bool cache, bool maskImages, bool incognito, ushort port)
    {
        Cache = cache;
        MaskImages = maskImages;
        Incognito = incognito;
        Port = port;
    }

    public bool GetCache() => Cache;
    public bool GetMaskImages() => MaskImages;
    public bool GetIncognito() => Incognito;
    public ushort GetPort() => Port;
}