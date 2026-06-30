using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace JobsMarketplace.Api.Caching;

public static class DistributedCacheExtensions
{
    public static async Task<T?> GetAsync<T>(this IDistributedCache cache, string key, CancellationToken cancellationToken = default)
    {
        var cachedValue = await cache.GetStringAsync(key, cancellationToken);
        return cachedValue is null ? default : JsonSerializer.Deserialize<T>(cachedValue);
    }

    public static Task SetAsync<T>(
        this IDistributedCache cache,
        string key,
        T value,
        DistributedCacheEntryOptions options,
        CancellationToken cancellationToken = default)
    {
        var serializedValue = JsonSerializer.Serialize(value);
        return cache.SetStringAsync(key, serializedValue, options, cancellationToken);
    }
}

