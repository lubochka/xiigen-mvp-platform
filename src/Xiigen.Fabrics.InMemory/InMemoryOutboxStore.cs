using Xiigen.Kernel;

namespace Xiigen.Fabrics.InMemory;

public sealed class InMemoryOutboxStore : IOutboxStore
{
    private readonly Lock _gate = new();
    private readonly List<OutboxRecord> _records = [];

    public Task<DataProcessResult<OutboxRecord>> AppendAsync(
        CloudEventEnvelope envelope,
        CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var record = new OutboxRecord(Guid.NewGuid().ToString("n"), "pending", envelope, DateTimeOffset.UtcNow, 0);
            _records.Add(record);
            return Task.FromResult(DataProcessResult<OutboxRecord>.Success(record));
        }
    }

    public Task<DataProcessResult<IReadOnlyList<OutboxRecord>>> ListAsync(CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            return Task.FromResult(DataProcessResult<IReadOnlyList<OutboxRecord>>.Success(_records.ToArray()));
        }
    }
}
