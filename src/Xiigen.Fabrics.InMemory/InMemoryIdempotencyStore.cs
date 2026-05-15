using Xiigen.Kernel;

namespace Xiigen.Fabrics.InMemory;

public sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    private readonly Lock _gate = new();
    private readonly Dictionary<string, IdempotencyReservation> _records = new(StringComparer.Ordinal);

    public Task<DataProcessResult<IdempotencyReservation>> TryReserveAsync(
        string operationKey,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            if (_records.TryGetValue(operationKey, out var existing) && existing.ExpiresAt > DateTimeOffset.UtcNow)
            {
                return Task.FromResult(DataProcessResult<IdempotencyReservation>.Success(existing, fromCache: existing.Status == "succeeded"));
            }

            var reservation = new IdempotencyReservation(operationKey, "reserved", null, DateTimeOffset.UtcNow.Add(ttl));
            _records[operationKey] = reservation;
            return Task.FromResult(DataProcessResult<IdempotencyReservation>.Success(reservation));
        }
    }

    public Task<DataProcessResult> MarkSucceededAsync(
        string operationKey,
        IReadOnlyDictionary<string, object?> response,
        CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            _records[operationKey] = new IdempotencyReservation(operationKey, "succeeded", new Dictionary<string, object?>(response), DateTimeOffset.UtcNow.AddMinutes(30));
            return Task.FromResult(DataProcessResult.Success());
        }
    }

    public Task<DataProcessResult> MarkFailedAsync(
        string operationKey,
        string code,
        string message,
        CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            _records[operationKey] = new IdempotencyReservation(
                operationKey,
                "failed",
                new Dictionary<string, object?> { ["code"] = code, ["message"] = message },
                DateTimeOffset.UtcNow.AddMinutes(5));
            return Task.FromResult(DataProcessResult.Success());
        }
    }
}
