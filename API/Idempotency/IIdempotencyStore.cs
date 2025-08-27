namespace API.Idempotency
{
    public interface IIdempotencyStore
    {
        bool TryGet(string key, out IdempotencyRecord record);
        void Set(string key, IdempotencyRecord record, TimeSpan ttl);
    }

    public sealed record IdempotencyRecord(string RequestHash, int ResourceId, DateTime SavedAtUtc);

}
