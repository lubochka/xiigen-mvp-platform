using Microsoft.Extensions.Logging.Abstractions;
using Xiigen.Fabrics.InMemory;
using Xiigen.Kernel;
using Xiigen.Platform.Marketplace;

namespace Xiigen.Platform.Marketplace.Tests;

public sealed class TenantAdaptationStudioServiceTests
{
    [Fact]
    public async Task CreateWorkspaceAsync_StoresTenantScopedWorkspaceAndAudit()
    {
        var harness = CreateHarness();
        using var tenant = harness.Tenants.Push(TestTenant("primary-tenant"));
        using var correlation = harness.Correlations.Push(TestCorrelation());

        var workspace = await harness.Service.CreateWorkspaceAsync(CreateWorkspaceRequest());
        var audit = await harness.Service.ListAuditAsync((string)workspace.Data!["id"]!);

        Assert.True(workspace.IsSuccess);
        Assert.Equal("primary-tenant", workspace.Data?["tenant_id"]);
        Assert.Equal("workspace_created", workspace.Data?["status"]);
        Assert.True(audit.IsSuccess);
        Assert.Contains(audit.Data!, row => Equals("WORKSPACE_CREATED", row["action"]));
    }

    [Fact]
    public async Task ConnectProjectSourceAsync_AcceptsRepositoryOrSnapshotEvidence()
    {
        var harness = CreateHarness();
        using var tenant = harness.Tenants.Push(TestTenant("primary-tenant"));
        using var correlation = harness.Correlations.Push(TestCorrelation());
        var workspaceId = await CreateWorkspaceIdAsync(harness);

        var connected = await harness.Service.ConnectProjectSourceAsync(new ConnectProjectSourceRequest(
            workspaceId,
            "https://github.com/lubochka/xiigen-primary-tenant",
            null,
            null,
            "tenant-admin",
            new Dictionary<string, object?> { ["sourceKind"] = "repo" }));

        Assert.True(connected.IsSuccess);
        Assert.Equal(workspaceId, connected.Data?["workspaceId"]);
        Assert.Equal("https://github.com/lubochka/xiigen-primary-tenant", connected.Data?["repositoryUrl"]);
    }

    [Fact]
    public async Task CapturePolicyAsync_StoresCodingAndArchitecturePolicy()
    {
        var harness = CreateHarness();
        using var tenant = harness.Tenants.Push(TestTenant("primary-tenant"));
        using var correlation = harness.Correlations.Push(TestCorrelation());
        var workspaceId = await CreateWorkspaceIdAsync(harness);

        var coding = await harness.Service.CapturePolicyAsync(new CaptureTenantPolicyRequest(
            workspaceId,
            "coding",
            "Use company approved namespaces and no direct cloud SDK calls in flow services.",
            "company-standard",
            "tenant-admin"));
        var architecture = await harness.Service.CapturePolicyAsync(new CaptureTenantPolicyRequest(
            workspaceId,
            "architecture",
            "All generated modules must preserve event-first boundaries and audit exports.",
            "company-standard",
            "tenant-admin"));

        Assert.True(coding.IsSuccess);
        Assert.True(architecture.IsSuccess);
        Assert.Equal("coding", coding.Data?["policyKind"]);
        Assert.Equal("architecture", architecture.Data?["policyKind"]);
    }

    [Fact]
    public async Task RequestAdaptationAsync_CreatesPlanBeforePatchAndReviewableDiff()
    {
        var harness = CreateHarness();
        using var tenant = harness.Tenants.Push(TestTenant("primary-tenant"));
        using var correlation = harness.Correlations.Push(TestCorrelation());
        var workspaceId = await CreateWorkspaceIdAsync(harness);

        var adaptation = await harness.Service.RequestAdaptationAsync(new RequestTenantAdaptationRequest(
            workspaceId,
            "Adapt the launch site to the primary tenant AWS deployment.",
            new[] { "Preserve tenant isolation", "Require approval before export" },
            "tenant-admin"));

        Assert.True(adaptation.IsSuccess);
        Assert.Equal("PLAN_READY_REVIEW_REQUIRED", adaptation.Data?["status"]);
        Assert.NotNull(adaptation.Data?["planBeforePatch"]);
        Assert.NotNull(adaptation.Data?["reviewableDiff"]);
    }

    [Fact]
    public async Task ReviewDiffAsync_ApprovesPlanWithoutSkippingAudit()
    {
        var harness = CreateHarness();
        using var tenant = harness.Tenants.Push(TestTenant("primary-tenant"));
        using var correlation = harness.Correlations.Push(TestCorrelation());
        var workspaceId = await CreateWorkspaceIdAsync(harness);
        var adaptation = await harness.Service.RequestAdaptationAsync(new RequestTenantAdaptationRequest(
            workspaceId,
            "Adapt route labels for primary tenant.",
            Array.Empty<string>(),
            "tenant-admin"));

        var reviewed = await harness.Service.ReviewDiffAsync(new ReviewAdaptationDiffRequest(
            workspaceId,
            (string)adaptation.Data!["id"]!,
            "approved",
            "tenant-admin",
            "Matches company policy."));
        var audit = await harness.Service.ListAuditAsync(workspaceId);

        Assert.True(reviewed.IsSuccess);
        Assert.Equal("APPROVED_FOR_APPLY_OR_EXPORT", reviewed.Data?["status"]);
        Assert.Contains(audit.Data!, row => Equals("ADAPTATION_APPROVED", row["action"]));
    }

    [Fact]
    public async Task GetWorkspaceAsync_ReturnsNullForCrossTenantAccess()
    {
        var harness = CreateHarness();
        string workspaceId;
        using (harness.Tenants.Push(TestTenant("primary-tenant")))
        using (harness.Correlations.Push(TestCorrelation()))
        {
            workspaceId = await CreateWorkspaceIdAsync(harness);
        }

        using var otherTenant = harness.Tenants.Push(TestTenant("other-tenant"));
        using var correlation = harness.Correlations.Push(TestCorrelation());

        var crossTenant = await harness.Service.GetWorkspaceAsync(workspaceId);

        Assert.True(crossTenant.IsSuccess);
        Assert.Null(crossTenant.Data);
    }

    private static async Task<string> CreateWorkspaceIdAsync(Harness harness)
    {
        var created = await harness.Service.CreateWorkspaceAsync(CreateWorkspaceRequest());
        Assert.True(created.IsSuccess);
        return (string)created.Data!["id"]!;
    }

    private static CreateTenantWorkspaceRequest CreateWorkspaceRequest() => new(
        "Primary Tenant Studio",
        "XIIGen Primary Tenant",
        "tenant-admin",
        new Dictionary<string, object?> { ["deployment"] = "aws-primary" });

    private static Harness CreateHarness()
    {
        var tenants = new AsyncLocalTenantContextAccessor();
        var correlations = new AsyncLocalCorrelationContextAccessor();
        var service = new TenantAdaptationStudioService(
            new InMemoryDocumentStore(),
            tenants,
            correlations,
            NullLogger<TenantAdaptationStudioService>.Instance);
        return new Harness(service, tenants, correlations);
    }

    private static TenantContext TestTenant(string tenantId) => new(
        tenantId,
        "tenant-admin",
        new HashSet<string>(StringComparer.Ordinal) { "tenant_admin" },
        "corr-studio-test",
        "test",
        DateTimeOffset.UtcNow);

    private static CorrelationContext TestCorrelation() => new(
        "corr-studio-test",
        "test",
        DateTimeOffset.UtcNow);

    private sealed record Harness(
        TenantAdaptationStudioService Service,
        ITenantContextAccessor Tenants,
        ICorrelationContextAccessor Correlations);
}
