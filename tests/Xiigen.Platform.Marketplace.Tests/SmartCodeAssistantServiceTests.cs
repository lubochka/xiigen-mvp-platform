using Microsoft.Extensions.Logging.Abstractions;
using Xiigen.Fabrics.InMemory;
using Xiigen.Kernel;
using Xiigen.Platform.Marketplace;

namespace Xiigen.Platform.Marketplace.Tests;

public sealed class SmartCodeAssistantServiceTests
{
    [Fact]
    public async Task CreateUnitAsync_StoresTrainableExportableImportableUnit()
    {
        var harness = CreateHarness();
        using var tenant = harness.Tenants.Push(TestTenant());
        using var correlation = harness.Correlations.Push(TestCorrelation());

        var unit = await harness.Service.CreateUnitAsync(UnitRequest());

        Assert.True(unit.IsSuccess);
        Assert.Equal("smart-code-assistant", unit.Data?["unitType"]);
        Assert.Equal(true, unit.Data?["trainable"]);
        Assert.Equal(true, unit.Data?["exportable"]);
        Assert.Equal(true, unit.Data?["importable"]);
        Assert.Equal(true, unit.Data?["noEmbeddedPluginWeights"]);
    }

    [Fact]
    public async Task ProposePlanAsync_RequiresTenantPoliciesAndCreatesPlanBeforePatch()
    {
        var harness = CreateHarness();
        using var tenant = harness.Tenants.Push(TestTenant());
        using var correlation = harness.Correlations.Push(TestCorrelation());

        var missing = await harness.Service.ProposePlanAsync(new SmartCodeAssistantPlanRequest(
            "xiigen-enterprise-master",
            "Adapt export package labels",
            "",
            "Preserve event-first architecture.",
            Array.Empty<string>(),
            Array.Empty<string>(),
            "tenant-admin"));
        var plan = await harness.Service.ProposePlanAsync(PlanRequest());

        Assert.True(missing.IsFailure);
        Assert.Equal("TENANT_POLICY_REQUIRED", missing.Error?.Code);
        Assert.True(plan.IsSuccess);
        Assert.Equal("PLAN_READY_PATCH_BLOCKED_UNTIL_VALIDATED", plan.Data?["status"]);
        Assert.NotNull(plan.Data?["planBeforePatch"]);
        Assert.Equal("Use company approved namespaces.", plan.Data?["codingPolicy"]);
        Assert.Equal("Preserve event-first architecture.", plan.Data?["architecturePolicy"]);
    }

    [Fact]
    public async Task ValidatePatchAsync_RecordsImpactRationaleAndChecks()
    {
        var harness = CreateHarness();
        using var tenant = harness.Tenants.Push(TestTenant());
        using var correlation = harness.Correlations.Push(TestCorrelation());
        var plan = await harness.Service.ProposePlanAsync(PlanRequest());

        var validation = await harness.Service.ValidatePatchAsync(new SmartCodeAssistantPatchValidationRequest(
            "xiigen-enterprise-master",
            (string)plan.Data!["id"]!,
            new[] { "client/angular/src/app/app.routes.ts", "server/Xiigen.Platform.Api/Program.cs" },
            new[] { "ABSTAIN_NO_SOURCE: no stored procedure source" },
            new[] { "scripts/pre-commit-check.ps1", "AWS /assistant screenshot" },
            "Reviewable patch for assistant route and API evidence.",
            false,
            "tenant-admin"));

        Assert.True(validation.IsSuccess);
        Assert.Equal("PATCH_READY_FOR_REVIEW", validation.Data?["status"]);
        Assert.NotNull(validation.Data?["rationale"]);
        Assert.NotEmpty((string[])validation.Data!["affectedFiles"]!);
        Assert.NotEmpty((string[])validation.Data!["testsAndChecks"]!);
    }

    [Fact]
    public async Task ValidatePatchAsync_EscalatesPolicyViolation()
    {
        var harness = CreateHarness();
        using var tenant = harness.Tenants.Push(TestTenant());
        using var correlation = harness.Correlations.Push(TestCorrelation());
        var plan = await harness.Service.ProposePlanAsync(PlanRequest());

        var validation = await harness.Service.ValidatePatchAsync(new SmartCodeAssistantPatchValidationRequest(
            "xiigen-enterprise-master",
            (string)plan.Data!["id"]!,
            new[] { "server/ForbiddenDirectSdkCall.cs" },
            Array.Empty<string>(),
            new[] { "policy-review" },
            "Direct cloud SDK call inside generated flow service.",
            true,
            "tenant-admin"));

        Assert.True(validation.IsSuccess);
        Assert.Equal("ESCALATED_POLICY_VIOLATION", validation.Data?["status"]);
        Assert.Equal(true, validation.Data?["policyViolation"]);
    }

    [Fact]
    public async Task RecordExportImportAsync_StoresLocatorProofWithoutEmbeddedWeights()
    {
        var harness = CreateHarness();
        using var tenant = harness.Tenants.Push(TestTenant());
        using var correlation = harness.Correlations.Push(TestCorrelation());

        var package = await harness.Service.RecordExportImportAsync(new SmartCodeAssistantExportImportRequest(
            "xiigen-enterprise-master",
            "primary-master-tenant.xiigen-enterprise-master.smart-code-assistant",
            "1.0.0",
            "xiigen-unit://primary-master-tenant/xiigen-enterprise-master/smart-code-assistant/1.0.0",
            "tenant-admin"));

        Assert.True(package.IsSuccess);
        Assert.Equal(true, package.Data?["exportable"]);
        Assert.Equal(true, package.Data?["importable"]);
        Assert.Equal(true, package.Data?["pluginLocatorUpdated"]);
        Assert.Equal(true, package.Data?["noEmbeddedPluginWeights"]);
    }

    [Fact]
    public async Task RecordMetricAsync_StoresRuntimeMetric()
    {
        var harness = CreateHarness();
        using var tenant = harness.Tenants.Push(TestTenant());
        using var correlation = harness.Correlations.Push(TestCorrelation());

        var metric = await harness.Service.RecordMetricAsync(new SmartCodeAssistantMetricRequest(
            "xiigen-enterprise-master",
            "primary-master-tenant.xiigen-enterprise-master.smart-code-assistant",
            "policy_validation_ms",
            42,
            "aws-runtime"));

        Assert.True(metric.IsSuccess);
        Assert.Equal("policy_validation_ms", metric.Data?["metricName"]);
        Assert.Equal(42d, metric.Data?["metricValue"]);
    }

    private static SmartCodeAssistantUnitRequest UnitRequest() => new(
        "xiigen-enterprise-master",
        "primary-master-tenant.xiigen-enterprise-master.smart-code-assistant",
        "xiigen-unit://primary-master-tenant/xiigen-enterprise-master/smart-code-assistant/1.0.0",
        "docs/plans/enterprise-adaptation/v3.3/project-assets/trainable-units/manifests/smart-code-assistant.manifest.json",
        "docs/plans/enterprise-adaptation/v3.3/project-assets/trainable-units/export/smart-code-assistant.export.json",
        "docs/plans/enterprise-adaptation/v3.3/project-assets/trainable-units/import/smart-code-assistant.import.json",
        "docs/plans/enterprise-adaptation/v3.3/project-assets/trainable-units/checkpoints/smart-code-assistant.checkpoint-reference.json",
        "tenant-admin");

    private static SmartCodeAssistantPlanRequest PlanRequest() => new(
        "xiigen-enterprise-master",
        "Adapt export package labels",
        "Use company approved namespaces.",
        "Preserve event-first architecture.",
        new[] { "client/angular/src/app/app.routes.ts", "server/Xiigen.Platform.Api/Program.cs" },
        new[] { "No direct cloud SDK calls in generated flow services" },
        "tenant-admin");

    private static Harness CreateHarness()
    {
        var tenants = new AsyncLocalTenantContextAccessor();
        var correlations = new AsyncLocalCorrelationContextAccessor();
        var service = new SmartCodeAssistantService(
            new InMemoryDocumentStore(),
            tenants,
            correlations,
            NullLogger<SmartCodeAssistantService>.Instance);
        return new Harness(service, tenants, correlations);
    }

    private static TenantContext TestTenant() => new(
        "primary-master-tenant",
        "tenant-admin",
        new HashSet<string>(StringComparer.Ordinal) { "tenant_admin" },
        "corr-smart-code-assistant-test",
        "test",
        DateTimeOffset.UtcNow);

    private static CorrelationContext TestCorrelation() => new(
        "corr-smart-code-assistant-test",
        "test",
        DateTimeOffset.UtcNow);

    private sealed record Harness(
        SmartCodeAssistantService Service,
        ITenantContextAccessor Tenants,
        ICorrelationContextAccessor Correlations);
}
