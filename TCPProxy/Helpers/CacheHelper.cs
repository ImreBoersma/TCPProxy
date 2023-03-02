using Microsoft.Extensions.Caching.Memory;

namespace TCPProxy.Helpers;

public class CacheHelper
{
    private readonly MemoryCache _cache;

    public CacheHelper()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
    }

    /// <summary>
    /// Add an item to the cache
    /// </summary>
    /// <param name="key">The key to set the value to</param>
    /// <param name="value">The value to set</param>
    /// <param name="seconds">The number of seconds to keep the value in the cache</param>
    public void Add(string key, object value, int seconds = 3600)
    {
        _cache.Set(key, value, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromSeconds(seconds)));
    }

    /// <summary>
    /// Get an item from the cache
    /// </summary>
    /// <param name="key">The key to get the value from</param>
    /// <typeparam name="T">The type of the value</typeparam>
    /// <returns>The value from the cache</returns>
    public T? Get<T>(string key)
    {
        return _cache.TryGetValue(key, out T? value) ? value : default;
    }
}