using Microsoft.Extensions.Logging.Abstractions;
using Xiigen.Fabrics.InMemory;
using Xiigen.Kernel;
using Xiigen.Platform.Marketplace;

namespace Xiigen.Platform.Marketplace.Tests;

public sealed class LegalPublicationServiceTests
{
    [Fact]
    public async Task GetRouteAsync_ReturnsBlockedShellWhenApprovedDocumentIsAbsent()
    {
        var harness = CreateHarness();
        using var tenant = harness.Tenants.Push(TestTenant());
        using var correlation = harness.Correlations.Push(TestCorrelation());

        var route = await harness.Service.GetRouteAsync("terms-of-use", "en");

        Assert.True(route.IsSuccess);
        Assert.False(route.Data?.CanPublish);
        Assert.Equal(LegalRoutePublicationStatus.LegalReviewRequired, route.Data?.PublicationStatus);
        Assert.Null(route.Data?.Body);
    }

    [Fact]
    public async Task UpsertDocumentAsync_RejectsApprovedDocumentWithoutApprovalMetadata()
    {
        var harness = CreateHarness();
        using var tenant = harness.Tenants.Push(TestTenant());
        using var correlation = harness.Correlations.Push(TestCorrelation());

        var result = await harness.Service.UpsertDocumentAsync(ApprovedTerms() with
        {
            EffectiveDate = null,
            ApprovedBy = null,
            ApprovedAtUtc = null
        });

        Assert.True(result.IsFailure);
        Assert.Equal("LEGAL_APPROVAL_METADATA_REQUIRED", result.Error?.Code);
    }

    [Fact]
    public async Task GetRouteAsync_PublishesOnlyApprovedDocumentWithApprovalMetadata()
    {
        var harness = CreateHarness();
        using var tenant = harness.Tenants.Push(TestTenant());
        using var correlation = harness.Correlations.Push(TestCorrelation());
        var stored = await harness.Service.UpsertDocumentAsync(ApprovedTerms());

        var route = await harness.Service.GetRouteAsync("terms-of-use", "en");

        Assert.True(stored.IsSuccess);
        Assert.True(route.IsSuccess);
        Assert.True(route.Data?.CanPublish);
        Assert.Equal(LegalRoutePublicationStatus.Published, route.Data?.PublicationStatus);
        Assert.Contains("approved terms body", route.Data?.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task GetRouteAsync_RejectsUnsupportedLocale()
    {
        var harness = CreateHarness();
        using var tenant = harness.Tenants.Push(TestTenant());
        using var correlation = harness.Correlations.Push(TestCorrelation());

        var route = await harness.Service.GetRouteAsync("terms-of-use", "de");

        Assert.True(route.IsFailure);
        Assert.Equal("LEGAL_LOCALE_UNSUPPORTED", route.Error?.Code);
    }

    private static Harness CreateHarness()
    {
        var tenants = new AsyncLocalTenantContextAccessor();
        var correlations = new AsyncLocalCorrelationContextAccessor();
        var service = new LegalPublicationService(
            new InMemoryDocumentStore(),
            tenants,
            correlations,
            NullLogger<LegalPublicationService>.Instance);
        return new Harness(service, tenants, correlations);
    }

    private static LegalDocument ApprovedTerms() => new(
        "terms-of-use:en:v1",
        "terms-of-use",
        "terms_of_service",
        "1.0.0",
        LegalDocumentStatus.Approved,
        "en",
        "en",
        "Terms of Use",
        "approved terms body",
        "Approved terms summary",
        new DateOnly(2026, 5, 26),
        "qualified-counsel",
        new DateTimeOffset(2026, 5, 26, 0, 0, 0, TimeSpan.Zero),
        "legal",
        null,
        "https://gyxwjim5ea.eu-west-1.awsapprunner.com/legal/terms-of-use",
        new[] { "signup" },
        new[] { "footer", "signup" },
        "evidence/legal/terms-of-use-en.png");

    private static TenantContext TestTenant() => new(
        "platform",
        "platform-admin",
        new HashSet<string>(StringComparer.Ordinal) { "platform_admin" },
        "corr-legal-test",
        "test",
        DateTimeOffset.UtcNow);

    private static CorrelationContext TestCorrelation() => new(
        "corr-legal-test",
        "test",
        DateTimeOffset.UtcNow);

    private sealed record Harness(
        LegalPublicationService Service,
        ITenantContextAccessor Tenants,
        ICorrelationContextAccessor Correlations);
}
