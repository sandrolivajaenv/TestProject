using Microsoft.Extensions.Caching.Memory;

namespace API.Idempotency;

public sealed class MemoryIdempotencyStore(IMemoryCache cache) : IIdempotencyStore
{
    private readonly IMemoryCache _cache = cache;

    public bool TryGet(string key, out IdempotencyRecord record)
        => _cache.TryGetValue(key, out record!);

    public void Set(string key, IdempotencyRecord record, TimeSpan ttl)
        => _cache.Set(key, record, ttl);
}