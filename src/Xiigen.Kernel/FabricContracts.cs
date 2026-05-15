namespace Xiigen.Kernel;

public sealed record DocumentRecord(
    string Id,
    string Version,
    IReadOnlyDictionary<string, object?> Body,
    DateTimeOffset UpdatedAt);

public interface IDocumentStore
{
    Task<DataProcessResult<DocumentRecord?>> GetAsync(
        string collection,
        string id,
        CancellationToken cancellationToken = default);

    Task<DataProcessResult<IReadOnlyList<DocumentRecord>>> SearchAsync(
        string collection,
        IReadOnlyDictionary<string, object?> filter,
        CancellationToken cancellationToken = default);

    Task<DataProcessResult<DocumentRecord>> UpsertAsync(
        string collection,
        string id,
        IReadOnlyDictionary<string, object?> body,
        string? expectedVersion = null,
        CancellationToken cancellationToken = default);
}

public sealed record IdempotencyReservation(
    string Key,
    string Status,
    IReadOnlyDictionary<string, object?>? CachedResponse,
    DateTimeOffset ExpiresAt);

public interface IIdempotencyStore
{
    Task<DataProcessResult<IdempotencyReservation>> TryReserveAsync(
        string operationKey,
        TimeSpan ttl,
        CancellationToken cancellationToken = default);

    Task<DataProcessResult> MarkSucceededAsync(
        string operationKey,
        IReadOnlyDictionary<string, object?> response,
        CancellationToken cancellationToken = default);

    Task<DataProcessResult> MarkFailedAsync(
        string operationKey,
        string code,
        string message,
        CancellationToken cancellationToken = default);
}

public sealed record CloudEventEnvelope(
    string Id,
    string Source,
    string Type,
    string SpecVersion,
    DateTimeOffset Time,
    string Subject,
    string DataContentType,
    IReadOnlyDictionary<string, object?> Data,
    IReadOnlyDictionary<string, string> Extensions);

public sealed record OutboxRecord(
    string Id,
    string Status,
    CloudEventEnvelope Event,
    DateTimeOffset CreatedAt,
    int Attempts);

public interface IOutboxStore
{
    Task<DataProcessResult<OutboxRecord>> AppendAsync(
        CloudEventEnvelope envelope,
        CancellationToken cancellationToken = default);

    Task<DataProcessResult<IReadOnlyList<OutboxRecord>>> ListAsync(
        CancellationToken cancellationToken = default);
}
