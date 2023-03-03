using Serilog;

namespace TCPProxy.Models;

public record ProxyConfigurationModel
{
    private bool Cache { get; }
    private bool MaskImages { get; }
    private bool Incognito { get; }
    private int Buffer { get; } = 8192;
    private ushort Port { get; } = 8080;

    public ProxyConfigurationModel(bool cache, bool maskImages, bool incognito, int buffer = 8192, ushort port = 8080)
    {
        Cache = cache;
        MaskImages = maskImages;
        Incognito = incognito;
        Buffer = buffer;
        Port = port;
    }

    public void PrintConfig()
    {
        Log.Debug("Proxy Started with the following configuration:");
        Log.Debug("\tCache: {Cache}", Cache ? "Enabled" : "Disabled");
        Log.Debug("\tMask images: {MaskImages}", MaskImages ? "Enabled" : "Disabled");
        Log.Debug("\tIncognito: {Incognito}", Incognito ? "Enabled" : "Disabled");
        Log.Debug("\tBuffer: {Buffer}", Buffer);
        Log.Debug("\tPort: {Port}", Port);
    }

    public bool GetCache() => Cache;
    public bool GetMaskImages() => MaskImages;
    public bool GetIncognito() => Incognito;
    public int GetBuffer() => Buffer;
    public ushort GetPort() => Port;
}