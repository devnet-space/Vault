using Devnet.Vault.Application.Features.Shared.Cache.Interfaces.Services;
using StackExchange.Redis;
using System.Text.Json;

namespace Devnet.Vault.Infrastructure.Cache.Services;

internal sealed class RedisCacheService(IConnectionMultiplexer redis) : ICacheService
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        if (value == null)
            return;

        var json = JsonSerializer.Serialize(value);

        await _db.StringSetAsync(key, json, expiry, When.Always);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(key);

        if (!value.HasValue)
            return default;

        return JsonSerializer.Deserialize<T>(value.ToString());
    }

    public async Task<bool> ExistsAsync(string key)
    {
        return await _db.KeyExistsAsync(key);
    }

    public async Task RemoveAsync(string key)
    {
        await _db.KeyDeleteAsync(key);
    }

    public async Task<bool> RefreshExpiryAsync(string key, TimeSpan expiry)
    {
        return await _db.KeyExpireAsync(key, expiry);
    }
}
