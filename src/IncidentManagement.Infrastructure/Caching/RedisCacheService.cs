using System.Text.Json;
using IncidentManagement.Application.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace IncidentManagement.Infrastructure.Caching;

public class RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger) : ICacheService
{
    private IDatabase Db => redis.GetDatabase();

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        try
        {
            var value = await Db.StringGetAsync(key);
            return value.IsNullOrEmpty ? default : JsonSerializer.Deserialize<T>((string)value!);
        }
        catch (RedisConnectionException ex)
        {
            logger.LogWarning(ex, "Redis unavailable, bypassing cache for key {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiry, CancellationToken ct = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await Db.StringSetAsync(key, json, expiry);
        }
        catch (RedisConnectionException ex)
        {
            logger.LogWarning(ex, "Redis unavailable, skipping cache write for key {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        try
        {
            await Db.KeyDeleteAsync(key);
        }
        catch (RedisConnectionException ex)
        {
            logger.LogWarning(ex, "Redis unavailable, skipping cache invalidation for key {Key}", key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        try
        {
            var endpoints = redis.GetEndPoints();
            foreach (var endpoint in endpoints)
            {
                var server = redis.GetServer(endpoint);
                await foreach (var key in server.KeysAsync(pattern: $"{prefix}*"))
                {
                    await Db.KeyDeleteAsync(key);
                }
            }
        }
        catch (RedisConnectionException ex)
        {
            logger.LogWarning(ex, "Redis unavailable, skipping cache invalidation for prefix {Prefix}", prefix);
        }
    }
}
