using Microsoft.Extensions.Logging.Abstractions;
using Xiigen.Fabrics.InMemory;
using Xiigen.Kernel;
using Xiigen.Platform.Marketplace;

namespace Xiigen.Platform.Marketplace.Tests;

public sealed class MarketplaceServiceTests
{
    [Fact]
    public async Task PublishListingAsync_StoresVisibleListing()
    {
        var harness = CreateHarness();
        using var tenant = harness.Tenants.Push(TestTenant());
        using var correlation = harness.Correlations.Push(TestCorrelation());

        var published = await harness.Service.PublishListingAsync(Flow00Listing());
        var loaded = await harness.Service.GetListingAsync("bundle-activation", "0.1.1");

        Assert.True(published.IsSuccess);
        Assert.True(loaded.IsSuccess);
        Assert.Equal("FLOW-00", loaded.Data?["flowId"]);
        Assert.Equal("listed", loaded.Data?["status"]);
    }

    [Fact]
    public async Task RecordPackageReadbackAsync_RejectsMissingPackage()
    {
        var harness = CreateHarness();
        using var tenant = harness.Tenants.Push(TestTenant());
        using var correlation = harness.Correlations.Push(TestCorrelation());

        var result = await harness.Service.RecordPackageReadbackAsync(new PackageReadbackRequest(
            "bundle-activation",
            "0.1.1",
            "Xiigen.Flow.BundleActivation",
            "http://localhost:5555/v3/index.json",
            "nuget",
            false,
            new Dictionary<string, object?>()));

        Assert.True(result.IsFailure);
        Assert.Equal("PACKAGE_READBACK_MISSING", result.Error?.Code);
    }

    [Fact]
    public async Task CreateTenantForkAsync_UsesApprovedTenantRepoNaming()
    {
        var harness = CreateHarness();
        using var tenant = harness.Tenants.Push(TestTenant());
        using var correlation = harness.Correlations.Push(TestCorrelation());
        await harness.Service.PublishListingAsync(Flow00Listing());

        var fork = await harness.Service.CreateTenantForkAsync(new TenantForkRequest(
            "bundle-activation",
            "0.1.1",
            "tenant-a",
            "bundle-activation",
            "source",
            "platform-admin",
            new Dictionary<string, object?>()));

        Assert.True(fork.IsSuccess);
        Assert.Equal("tenant-a--bundle-activation", fork.Data?["repoName"]);
    }

    [Fact]
    public async Task ApplyAdaptationAsync_PreservesAdaptationHistory()
    {
        var harness = CreateHarness();
        using var tenant = harness.Tenants.Push(TestTenant());
        using var correlation = harness.Correlations.Push(TestCorrelation());
        await harness.Service.PublishListingAsync(Flow00Listing());
        await harness.Service.CreateTenantForkAsync(new TenantForkRequest(
            "bundle-activation",
            "0.1.1",
            "tenant-a",
            "bundle-activation",
            "source",
            "platform-admin",
            new Dictionary<string, object?>()));

        var adapted = await harness.Service.ApplyAdaptationAsync(new ApplyAdaptationRequest(
            "bundle-activation",
            "0.1.1",
            "tenant-a",
            "bundle-activation",
            "flow_module_name",
            "tenant-a-activation",
            "ai-adaptation",
            new Dictionary<string, object?> { ["promptId"] = "FLOW-00-A1" }));

        Assert.True(adapted.IsSuccess);
        Assert.Equal("tenant-a--tenant-a-activation", adapted.Data?["repoName"]);
        Assert.NotNull(adapted.Data?["adaptationHistory"]);
    }

    private static Harness CreateHarness()
    {
        var tenants = new AsyncLocalTenantContextAccessor();
        var correlations = new AsyncLocalCorrelationContextAccessor();
        var service = new MarketplaceService(
            new InMemoryDocumentStore(),
            tenants,
            correlations,
            NullLogger<MarketplaceService>.Instance);
        return new Harness(service, tenants, correlations);
    }

    private static PublishMarketplaceListingRequest Flow00Listing() => new(
        "FLOW-00",
        "bundle-activation",
        "bundle-activation",
        "xiigen-flows-dot-net-angular",
        "flows/FLOW-00-bundle-activation",
        "local-checkpoint",
        new[]
        {
            new MarketplacePackageDescriptor(
                "Xiigen.Flow.BundleActivation",
                "0.1.1",
                "http://localhost:5555/v3/index.json",
                "nuget")
        },
        new Dictionary<string, object?> { ["evidence"] = "local" });

    private static TenantContext TestTenant() => new(
        "platform",
        "platform-admin",
        new HashSet<string>(StringComparer.Ordinal) { "platform_admin" },
        "corr-marketplace-test",
        "test",
        DateTimeOffset.UtcNow);

    private static CorrelationContext TestCorrelation() => new(
        "corr-marketplace-test",
        "test",
        DateTimeOffset.UtcNow);

    private sealed record Harness(
        MarketplaceService Service,
        ITenantContextAccessor Tenants,
        ICorrelationContextAccessor Correlations);
}
