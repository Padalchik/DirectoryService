using System.Collections.Concurrent;
using System.Text.Json;
using DirectoryService.Application.Abstractions;
using Microsoft.Extensions.Caching.Distributed;

namespace DirectoryService.Infrastructure.Cache;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ConcurrentDictionary<string, bool> _keys = new();

    public RedisCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
    {
        var cachedValue = await _cache.GetStringAsync(key, cancellationToken);
        if (cachedValue is null)
            return default;

        return JsonSerializer.Deserialize<T>(cachedValue);
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan ttl,
        CancellationToken cancellationToken)
    {
        var options = new DistributedCacheEntryOptions
        {
            SlidingExpiration = ttl,
        };

        string serializedValue = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(key, serializedValue, options, cancellationToken);
        _keys.TryAdd(key, true);
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken ct)
    {
        var tasks = _keys.Keys
            .Where(k => k.StartsWith(prefix))
            .Select(k => _cache.RemoveAsync(k, ct));

        await Task.WhenAll(tasks);
    }
}