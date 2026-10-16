using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Tapio.Application.DTOs.Public;
using Tapio.Application.Services;

namespace Tapio.Infrastructure.Caching;

public class RedisPublicBusinessCache(IDistributedCache cache) : IPublicBusinessCache
{
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(5);

    public async Task<PublicBusinessResponse?> GetAsync(string slug, CancellationToken ct = default)
    {
        var bytes = await cache.GetAsync(Key(slug), ct);
        return bytes is null ? null : JsonSerializer.Deserialize<PublicBusinessResponse>(bytes);
    }

    public async Task SetAsync(string slug, PublicBusinessResponse response, CancellationToken ct = default)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(response);
        await cache.SetAsync(Key(slug), bytes, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = Ttl }, ct);
    }

    public Task InvalidateAsync(string slug, CancellationToken ct = default) =>
        cache.RemoveAsync(Key(slug), ct);

    private static string Key(string slug) => $"tapio:public:business:{slug}";
}

public class NullPublicBusinessCache : IPublicBusinessCache
{
    public Task<PublicBusinessResponse?> GetAsync(string slug, CancellationToken ct = default) =>
        Task.FromResult<PublicBusinessResponse?>(null);

    public Task SetAsync(string slug, PublicBusinessResponse response, CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task InvalidateAsync(string slug, CancellationToken ct = default) =>
        Task.CompletedTask;
}
