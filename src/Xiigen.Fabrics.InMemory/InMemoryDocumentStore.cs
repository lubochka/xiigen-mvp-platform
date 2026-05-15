using System.Globalization;
using Xiigen.Kernel;

namespace Xiigen.Fabrics.InMemory;

public sealed class InMemoryDocumentStore : IDocumentStore
{
    private readonly Lock _gate = new();
    private readonly Dictionary<string, Dictionary<string, DocumentRecord>> _collections = new(StringComparer.Ordinal);

    public Task<DataProcessResult<DocumentRecord?>> GetAsync(
        string collection,
        string id,
        CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var item = _collections.TryGetValue(collection, out var records) &&
                records.TryGetValue(id, out var record)
                    ? record
                    : null;
            return Task.FromResult(DataProcessResult<DocumentRecord?>.Success(item));
        }
    }

    public Task<DataProcessResult<IReadOnlyList<DocumentRecord>>> SearchAsync(
        string collection,
        IReadOnlyDictionary<string, object?> filter,
        CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            if (!_collections.TryGetValue(collection, out var records))
            {
                return Task.FromResult(DataProcessResult<IReadOnlyList<DocumentRecord>>.Success(Array.Empty<DocumentRecord>()));
            }

            var matches = records.Values.Where(record => Matches(record.Body, filter)).ToArray();
            return Task.FromResult(DataProcessResult<IReadOnlyList<DocumentRecord>>.Success(matches));
        }
    }

    public Task<DataProcessResult<DocumentRecord>> UpsertAsync(
        string collection,
        string id,
        IReadOnlyDictionary<string, object?> body,
        string? expectedVersion = null,
        CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            if (!_collections.TryGetValue(collection, out var records))
            {
                records = new Dictionary<string, DocumentRecord>(StringComparer.Ordinal);
                _collections[collection] = records;
            }

            if (expectedVersion is not null &&
                records.TryGetValue(id, out var current) &&
                !string.Equals(current.Version, expectedVersion, StringComparison.Ordinal))
            {
                return Task.FromResult(DataProcessResult<DocumentRecord>.Failure(
                    "CONCURRENT_UPDATE",
                    "Document version does not match",
                    new Dictionary<string, object?> { ["currentVersion"] = current.Version }));
            }

            var nextVersion = records.TryGetValue(id, out var existing)
                ? (int.Parse(existing.Version, CultureInfo.InvariantCulture) + 1).ToString(CultureInfo.InvariantCulture)
                : "1";
            var record = new DocumentRecord(id, nextVersion, new Dictionary<string, object?>(body), DateTimeOffset.UtcNow);
            records[id] = record;
            return Task.FromResult(DataProcessResult<DocumentRecord>.Success(record));
        }
    }

    private static bool Matches(IReadOnlyDictionary<string, object?> body, IReadOnlyDictionary<string, object?> filter)
    {
        foreach (var pair in filter)
        {
            if (!body.TryGetValue(pair.Key, out var value))
            {
                return false;
            }

            if (value is IEnumerable<string> strings && pair.Value is string expectedString)
            {
                if (!strings.Contains(expectedString, StringComparer.Ordinal))
                {
                    return false;
                }

                continue;
            }

            if (!Equals(value, pair.Value))
            {
                return false;
            }
        }

        return true;
    }
}
