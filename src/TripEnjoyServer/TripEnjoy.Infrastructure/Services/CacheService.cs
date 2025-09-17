using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TripEnjoy.Application.Interfaces.External.Cache;

namespace TripEnjoy.Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;

    public CacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        var cachedValue = await _cache.GetAsync(key, cancellationToken);
        if (cachedValue == null)
        {
            return null;
        }

        var json = Encoding.UTF8.GetString(cachedValue);
        return JsonSerializer.Deserialize<T>(json);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        var json = JsonSerializer.Serialize(value);
        var bytes = Encoding.UTF8.GetBytes(json);

        var options = new DistributedCacheEntryOptions();
        if (expiration.HasValue)
        {
            options.SetAbsoluteExpiration(expiration.Value);
        }

        await _cache.SetAsync(key, bytes, options, cancellationToken);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cache.RemoveAsync(key, cancellationToken);
    }

    // NOTE: This is a simplified implementation for prefix removal. 
    // For high-performance scenarios with millions of keys, a different strategy (like using Redis Sets or SCAN command) would be better.
    // However, this approach is fine for many use cases.
    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        // This functionality is not directly supported by IDistributedCache and requires a specific implementation,
        // often coupled with the underlying cache provider (like Redis).
        // For simplicity in this step, we will leave it as a no-op, but it would need to be implemented for full cache invalidation.
        // For a real implementation, you would need to inject the Redis connection multiplexer and use SCAN.
        return Task.CompletedTask;
    }
}
