using Microsoft.Extensions.Logging;
using Xiigen.Kernel;

namespace Xiigen.Platform.Marketplace;

public sealed class SmartCodeAssistantService : MicroserviceBase<SmartCodeAssistantService>, ISmartCodeAssistantService
{
    private const string UnitsCollection = "smart-code-assistant-units";
    private const string PlansCollection = "smart-code-assistant-plans";
    private const string PatchCollection = "smart-code-assistant-patch-validations";
    private const string PackageCollection = "smart-code-assistant-package-evidence";
    private const string MetricsCollection = "smart-code-assistant-metrics";
    private readonly IDocumentStore _documents;

    public SmartCodeAssistantService(
        IDocumentStore documents,
        ITenantContextAccessor tenantContextAccessor,
        ICorrelationContextAccessor correlationContextAccessor,
        ILogger<SmartCodeAssistantService> logger)
        : base(
            new ServiceDescriptor("xiigen-smart-code-assistant", "XIIGen Smart Code Assistant", "PLATFORM"),
            tenantContextAccessor,
            correlationContextAccessor,
            logger)
    {
        _documents = documents;
    }

    public async Task<DataProcessResult<IReadOnlyDictionary<string, object?>>> CreateUnitAsync(
        SmartCodeAssistantUnitRequest request,
        CancellationToken cancellationToken = default)
    {
        var body = WithTenant(new Dictionary<string, object?>
        {
            ["id"] = UnitId(request.ProjectId),
            ["projectId"] = request.ProjectId,
            ["unitId"] = request.UnitId,
            ["unitType"] = "smart-code-assistant",
            ["locatorEntry"] = request.LocatorEntry,
            ["manifestPath"] = request.ManifestPath,
            ["exportPath"] = request.ExportPath,
            ["importPath"] = request.ImportPath,
            ["checkpointReference"] = request.CheckpointReference,
            ["noEmbeddedPluginWeights"] = true,
            ["trainable"] = true,
            ["exportable"] = true,
            ["importable"] = true,
            ["version"] = "1.0.0",
            ["createdBy"] = request.CreatedBy,
            ["createdAt"] = DateTimeOffset.UtcNow,
            ["correlationId"] = CurrentCorrelationId
        });

        var stored = await _documents.UpsertAsync(UnitsCollection, UnitId(request.ProjectId), body, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return stored.IsSuccess && stored.Data is not null
            ? Success(stored.Data.Body)
            : Failure<IReadOnlyDictionary<string, object?>>("SMART_CODE_UNIT_STORE_FAILED", "Smart Code Assistant unit could not be stored.");
    }

    public async Task<DataProcessResult<IReadOnlyDictionary<string, object?>>> ProposePlanAsync(
        SmartCodeAssistantPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.CodingPolicy) || string.IsNullOrWhiteSpace(request.ArchitecturePolicy))
        {
            return Failure<IReadOnlyDictionary<string, object?>>("TENANT_POLICY_REQUIRED", "Coding and architecture policies are required before patch planning.");
        }

        var planId = StableId("assistant-plan", $"{request.ProjectId}:{request.Objective}:{CurrentCorrelationId}");
        var body = WithTenant(new Dictionary<string, object?>
        {
            ["id"] = planId,
            ["projectId"] = request.ProjectId,
            ["objective"] = request.Objective,
            ["codingPolicy"] = request.CodingPolicy,
            ["architecturePolicy"] = request.ArchitecturePolicy,
            ["scope"] = request.Scope.ToArray(),
            ["constraints"] = request.Constraints.ToArray(),
            ["status"] = "PLAN_READY_PATCH_BLOCKED_UNTIL_VALIDATED",
            ["planBeforePatch"] = new Dictionary<string, object?>
            {
                ["steps"] = new[] { "read-tenant-policy", "read-linked-graph", "validate-scope", "prepare-diff", "wait-for-review" },
                ["policyApplied"] = true
            },
            ["requestedBy"] = request.RequestedBy,
            ["createdAt"] = DateTimeOffset.UtcNow,
            ["correlationId"] = CurrentCorrelationId
        });

        var stored = await _documents.UpsertAsync(PlansCollection, planId, body, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return stored.IsSuccess && stored.Data is not null
            ? Success(stored.Data.Body)
            : Failure<IReadOnlyDictionary<string, object?>>("SMART_CODE_PLAN_STORE_FAILED", "Smart Code Assistant plan could not be stored.");
    }

    public async Task<DataProcessResult<IReadOnlyDictionary<string, object?>>> ValidatePatchAsync(
        SmartCodeAssistantPatchValidationRequest request,
        CancellationToken cancellationToken = default)
    {
        var plan = await _documents.GetAsync(PlansCollection, request.PlanId, cancellationToken).ConfigureAwait(false);
        if (plan.IsFailure || plan.Data is null || !BelongsToCurrentTenant(plan.Data.Body))
        {
            return Failure<IReadOnlyDictionary<string, object?>>("SMART_CODE_PLAN_NOT_FOUND", "Plan was not found in the current tenant.");
        }

        var status = request.ViolatesPolicy ? "ESCALATED_POLICY_VIOLATION" : "PATCH_READY_FOR_REVIEW";
        var body = WithTenant(new Dictionary<string, object?>
        {
            ["id"] = StableId("assistant-patch", $"{request.ProjectId}:{request.PlanId}:{CurrentCorrelationId}"),
            ["projectId"] = request.ProjectId,
            ["planId"] = request.PlanId,
            ["status"] = status,
            ["patchSummary"] = request.PatchSummary,
            ["affectedFiles"] = request.AffectedFiles.ToArray(),
            ["affectedProcedures"] = request.AffectedProcedures.ToArray(),
            ["testsAndChecks"] = request.TestsAndChecks.ToArray(),
            ["rationale"] = "Patch was evaluated against tenant policies and linked graph impact before apply/export.",
            ["policyViolation"] = request.ViolatesPolicy,
            ["reviewedBy"] = request.ReviewedBy,
            ["reviewedAt"] = DateTimeOffset.UtcNow,
            ["correlationId"] = CurrentCorrelationId
        });

        var stored = await _documents.UpsertAsync(PatchCollection, (string)body["id"]!, body, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return stored.IsSuccess && stored.Data is not null
            ? Success(stored.Data.Body)
            : Failure<IReadOnlyDictionary<string, object?>>("SMART_CODE_PATCH_STORE_FAILED", "Patch validation could not be stored.");
    }

    public async Task<DataProcessResult<IReadOnlyDictionary<string, object?>>> RecordExportImportAsync(
        SmartCodeAssistantExportImportRequest request,
        CancellationToken cancellationToken = default)
    {
        var body = WithTenant(new Dictionary<string, object?>
        {
            ["id"] = StableId("assistant-package", $"{request.ProjectId}:{request.UnitId}:{request.PackageVersion}"),
            ["projectId"] = request.ProjectId,
            ["unitId"] = request.UnitId,
            ["packageVersion"] = request.PackageVersion,
            ["locatorEntry"] = request.LocatorEntry,
            ["exportable"] = true,
            ["importable"] = true,
            ["pluginLocatorUpdated"] = true,
            ["noEmbeddedPluginWeights"] = true,
            ["requestedBy"] = request.RequestedBy,
            ["createdAt"] = DateTimeOffset.UtcNow,
            ["correlationId"] = CurrentCorrelationId
        });
        var stored = await _documents.UpsertAsync(PackageCollection, (string)body["id"]!, body, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return stored.IsSuccess && stored.Data is not null
            ? Success(stored.Data.Body)
            : Failure<IReadOnlyDictionary<string, object?>>("SMART_CODE_PACKAGE_STORE_FAILED", "Export/import evidence could not be stored.");
    }

    public async Task<DataProcessResult<IReadOnlyDictionary<string, object?>>> RecordMetricAsync(
        SmartCodeAssistantMetricRequest request,
        CancellationToken cancellationToken = default)
    {
        var body = WithTenant(new Dictionary<string, object?>
        {
            ["id"] = StableId("assistant-metric", $"{request.ProjectId}:{request.UnitId}:{request.MetricName}:{CurrentCorrelationId}"),
            ["projectId"] = request.ProjectId,
            ["unitId"] = request.UnitId,
            ["metricName"] = request.MetricName,
            ["metricValue"] = request.MetricValue,
            ["source"] = request.Source,
            ["recordedAt"] = DateTimeOffset.UtcNow,
            ["correlationId"] = CurrentCorrelationId
        });
        var stored = await _documents.UpsertAsync(MetricsCollection, (string)body["id"]!, body, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return stored.IsSuccess && stored.Data is not null
            ? Success(stored.Data.Body)
            : Failure<IReadOnlyDictionary<string, object?>>("SMART_CODE_METRIC_STORE_FAILED", "Runtime metric could not be stored.");
    }

    private IReadOnlyDictionary<string, object?> WithTenant(IReadOnlyDictionary<string, object?> body)
    {
        var copy = new Dictionary<string, object?>(body)
        {
            ["tenant_id"] = CurrentTenant.TenantId
        };
        return copy;
    }

    private bool BelongsToCurrentTenant(IReadOnlyDictionary<string, object?> body) =>
        body.TryGetValue("tenant_id", out var tenantId) &&
        string.Equals(Convert.ToString(tenantId), CurrentTenant.TenantId, StringComparison.Ordinal);

    private static string UnitId(string projectId) => StableId("assistant-unit", projectId);

    private static string StableId(string prefix, string value) =>
        $"{prefix}-{Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value))).ToLowerInvariant()[..16]}";
}
