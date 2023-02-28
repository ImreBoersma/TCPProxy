using Microsoft.Extensions.Caching.Memory;

namespace TCPProxy.Helpers;

public abstract class CacheHelper
{
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());

    public void Add(string key, object value, int seconds)
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