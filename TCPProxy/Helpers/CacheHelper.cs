using Microsoft.Extensions.Caching.Memory;

namespace TCPProxy.Helpers;

public class CacheHelper
{
    private readonly MemoryCache _cache;

    public CacheHelper()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
    }

    public void Add(string key, object value, int seconds = 3600)
    {
        _cache.Set(key, value, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(seconds)));
    }

    public T? Get<T>(string key)
    {
        return _cache.TryGetValue(key, out T? value) ? value : default;
    }

    public void Remove(string key)
    {
        _cache.Remove(key);
    }
}