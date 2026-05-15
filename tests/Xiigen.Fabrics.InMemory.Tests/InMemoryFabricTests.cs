using Xiigen.Fabrics.InMemory;
using Xiigen.Kernel;

namespace Xiigen.Fabrics.InMemory.Tests;

public sealed class InMemoryFabricTests
{
    [Fact]
    public async Task DocumentStore_RejectsStaleExpectedVersion()
    {
        var store = new InMemoryDocumentStore();
        var first = await store.UpsertAsync("bundles", "core", new Dictionary<string, object?> { ["status"] = "PENDING" });

        var stale = await store.UpsertAsync(
            "bundles",
            "core",
            new Dictionary<string, object?> { ["status"] = "ACTIVE" },
            expectedVersion: "stale");

        Assert.True(first.IsSuccess);
        Assert.True(stale.IsFailure);
        Assert.Equal("CONCURRENT_UPDATE", stale.Error?.Code);
    }

    [Fact]
    public async Task IdempotencyStore_ReplaysSucceededReservation()
    {
        var store = new InMemoryIdempotencyStore();
        var key = Guid.NewGuid().ToString("n");

        var first = await store.TryReserveAsync(key, TimeSpan.FromMinutes(30));
        await store.MarkSucceededAsync(key, new Dictionary<string, object?> { ["id"] = "result-1" });
        var second = await store.TryReserveAsync(key, TimeSpan.FromMinutes(30));

        Assert.True(first.IsSuccess);
        Assert.True(second.IsSuccess);
        Assert.True(second.FromCache);
        Assert.Equal("succeeded", second.Data?.Status);
        Assert.Equal("result-1", second.Data?.CachedResponse?["id"]);
    }

    [Fact]
    public async Task OutboxStore_AppendsCloudEventEnvelope()
    {
        var store = new InMemoryOutboxStore();
        var envelope = new CloudEventEnvelope(
            "event-1",
            "xiigen://test",
            "test.created",
            "1.0",
            DateTimeOffset.UtcNow,
            "subject",
            "application/json",
            new Dictionary<string, object?> { ["ok"] = true },
            new Dictionary<string, string> { ["correlationid"] = "corr-test" });

        var appended = await store.AppendAsync(envelope);
        var records = await store.ListAsync();

        Assert.True(appended.IsSuccess);
        Assert.Contains(records.Data ?? Array.Empty<OutboxRecord>(), record => record.Event.Id == "event-1");
    }

    [Fact]
    public async Task FlowBootstrapper_RecordsCalls()
    {
        var bootstrapper = new InMemoryFlowBootstrapper();

        var result = await bootstrapper.BootstrapAsync("FLOW-01", "DRY_RUN");

        Assert.True(result.IsSuccess);
        Assert.Equal(new[] { ("FLOW-01", "DRY_RUN") }, bootstrapper.Calls);
    }
}
