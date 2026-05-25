using Microsoft.Extensions.Logging;
using Xiigen.Kernel;

namespace Xiigen.Platform.Marketplace;

public sealed class TenantAdaptationStudioService : MicroserviceBase<TenantAdaptationStudioService>, ITenantAdaptationStudioService
{
    private const string WorkspacesCollection = "tenant-adaptation-workspaces";
    private const string SourcesCollection = "tenant-adaptation-project-sources";
    private const string PoliciesCollection = "tenant-adaptation-policies";
    private const string AdaptationsCollection = "tenant-adaptation-requests";
    private const string AuditCollection = "tenant-adaptation-audit";
    private readonly IDocumentStore _documents;

    public TenantAdaptationStudioService(
        IDocumentStore documents,
        ITenantContextAccessor tenantContextAccessor,
        ICorrelationContextAccessor correlationContextAccessor,
        ILogger<TenantAdaptationStudioService> logger)
        : base(
            new ServiceDescriptor("xiigen-tenant-adaptation-studio", "XIIGen Tenant Adaptation Studio", "PLATFORM"),
            tenantContextAccessor,
            correlationContextAccessor,
            logger)
    {
        _documents = documents;
    }

    public async Task<DataProcessResult<IReadOnlyDictionary<string, object?>>> CreateWorkspaceAsync(
        CreateTenantWorkspaceRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.WorkspaceName))
        {
            return Failure<IReadOnlyDictionary<string, object?>>("WORKSPACE_NAME_REQUIRED", "Workspace name is required.");
        }

        var id = StableId("workspace", request.WorkspaceName);
        var body = WithTenant(new Dictionary<string, object?>
        {
            ["id"] = id,
            ["workspaceName"] = request.WorkspaceName,
            ["corporateName"] = request.CorporateName,
            ["requestedBy"] = request.RequestedBy,
            ["status"] = "workspace_created",
            ["metadata"] = new Dictionary<string, object?>(request.Metadata),
            ["createdAt"] = DateTimeOffset.UtcNow,
            ["correlationId"] = CurrentCorrelationId
        });

        var stored = await _documents.UpsertAsync(WorkspacesCollection, id, body, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (stored.IsFailure || stored.Data is null)
        {
            return Failure<IReadOnlyDictionary<string, object?>>(
                stored.Error?.Code ?? "WORKSPACE_STORE_FAILED",
                stored.Error?.Message ?? "Workspace could not be stored.");
        }

        await AuditAsync(id, "WORKSPACE_CREATED", request.RequestedBy, stored.Data.Body, cancellationToken)
            .ConfigureAwait(false);
        return Success(stored.Data.Body);
    }

    public async Task<DataProcessResult<IReadOnlyDictionary<string, object?>>> ConnectProjectSourceAsync(
        ConnectProjectSourceRequest request,
        CancellationToken cancellationToken = default)
    {
        var workspace = await RequireWorkspaceAsync(request.WorkspaceId, cancellationToken).ConfigureAwait(false);
        if (workspace.IsFailure)
        {
            return Failure<IReadOnlyDictionary<string, object?>>(workspace.Error!.Code, workspace.Error.Message, workspace.Error.Details);
        }

        if (string.IsNullOrWhiteSpace(request.RepositoryUrl) && string.IsNullOrWhiteSpace(request.SnapshotName))
        {
            return Failure<IReadOnlyDictionary<string, object?>>("PROJECT_SOURCE_REQUIRED", "Repository URL or project snapshot is required.");
        }

        var id = StableId("source", $"{request.WorkspaceId}:{request.RepositoryUrl}:{request.SnapshotName}");
        var body = WithTenant(new Dictionary<string, object?>
        {
            ["id"] = id,
            ["workspaceId"] = request.WorkspaceId,
            ["repositoryUrl"] = request.RepositoryUrl,
            ["snapshotName"] = request.SnapshotName,
            ["snapshotHash"] = request.SnapshotHash,
            ["requestedBy"] = request.RequestedBy,
            ["metadata"] = new Dictionary<string, object?>(request.Metadata),
            ["connectedAt"] = DateTimeOffset.UtcNow,
            ["correlationId"] = CurrentCorrelationId
        });

        var stored = await _documents.UpsertAsync(SourcesCollection, id, body, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (stored.IsFailure || stored.Data is null)
        {
            return Failure<IReadOnlyDictionary<string, object?>>(
                stored.Error?.Code ?? "PROJECT_SOURCE_STORE_FAILED",
                stored.Error?.Message ?? "Project source could not be stored.");
        }

        await AuditAsync(request.WorkspaceId, "PROJECT_SOURCE_CONNECTED", request.RequestedBy, stored.Data.Body, cancellationToken)
            .ConfigureAwait(false);
        return Success(stored.Data.Body);
    }

    public async Task<DataProcessResult<IReadOnlyDictionary<string, object?>>> CapturePolicyAsync(
        CaptureTenantPolicyRequest request,
        CancellationToken cancellationToken = default)
    {
        var workspace = await RequireWorkspaceAsync(request.WorkspaceId, cancellationToken).ConfigureAwait(false);
        if (workspace.IsFailure)
        {
            return Failure<IReadOnlyDictionary<string, object?>>(workspace.Error!.Code, workspace.Error.Message, workspace.Error.Details);
        }

        if (!string.Equals(request.PolicyKind, "coding", StringComparison.Ordinal) &&
            !string.Equals(request.PolicyKind, "architecture", StringComparison.Ordinal))
        {
            return Failure<IReadOnlyDictionary<string, object?>>("POLICY_KIND_UNSUPPORTED", "Policy kind must be coding or architecture.");
        }

        if (string.IsNullOrWhiteSpace(request.PolicyText))
        {
            return Failure<IReadOnlyDictionary<string, object?>>("POLICY_TEXT_REQUIRED", "Policy text is required.");
        }

        var id = StableId("policy", $"{request.WorkspaceId}:{request.PolicyKind}");
        var body = WithTenant(new Dictionary<string, object?>
        {
            ["id"] = id,
            ["workspaceId"] = request.WorkspaceId,
            ["policyKind"] = request.PolicyKind,
            ["policyText"] = request.PolicyText,
            ["source"] = request.Source,
            ["capturedBy"] = request.CapturedBy,
            ["capturedAt"] = DateTimeOffset.UtcNow,
            ["correlationId"] = CurrentCorrelationId
        });

        var stored = await _documents.UpsertAsync(PoliciesCollection, id, body, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (stored.IsFailure || stored.Data is null)
        {
            return Failure<IReadOnlyDictionary<string, object?>>(
                stored.Error?.Code ?? "POLICY_STORE_FAILED",
                stored.Error?.Message ?? "Policy could not be stored.");
        }

        await AuditAsync(request.WorkspaceId, $"POLICY_{request.PolicyKind.ToUpperInvariant()}_CAPTURED", request.CapturedBy, stored.Data.Body, cancellationToken)
            .ConfigureAwait(false);
        return Success(stored.Data.Body);
    }

    public async Task<DataProcessResult<IReadOnlyDictionary<string, object?>>> RequestAdaptationAsync(
        RequestTenantAdaptationRequest request,
        CancellationToken cancellationToken = default)
    {
        var workspace = await RequireWorkspaceAsync(request.WorkspaceId, cancellationToken).ConfigureAwait(false);
        if (workspace.IsFailure)
        {
            return Failure<IReadOnlyDictionary<string, object?>>(workspace.Error!.Code, workspace.Error.Message, workspace.Error.Details);
        }

        if (string.IsNullOrWhiteSpace(request.Objective))
        {
            return Failure<IReadOnlyDictionary<string, object?>>("ADAPTATION_OBJECTIVE_REQUIRED", "Adaptation objective is required.");
        }

        var id = StableId("adaptation", $"{request.WorkspaceId}:{request.Objective}:{CurrentCorrelationId}");
        var plan = new Dictionary<string, object?>
        {
            ["summary"] = "Plan before patch: inspect linked graph, apply tenant policy, create reviewable diff, wait for approval.",
            ["steps"] = new[]
            {
                "read-linked-system-graph",
                "apply-coding-and-architecture-policy",
                "synthesize-adaptation-plan",
                "prepare-reviewable-diff",
                "wait-for-tenant-approval"
            }
        };
        var diff = new Dictionary<string, object?>
        {
            ["status"] = "PENDING_REVIEW",
            ["files"] = Array.Empty<string>(),
            ["summary"] = "No patch is applied until tenant approval is recorded."
        };
        var body = WithTenant(new Dictionary<string, object?>
        {
            ["id"] = id,
            ["workspaceId"] = request.WorkspaceId,
            ["objective"] = request.Objective,
            ["constraints"] = request.Constraints.ToArray(),
            ["requestedBy"] = request.RequestedBy,
            ["status"] = "PLAN_READY_REVIEW_REQUIRED",
            ["planBeforePatch"] = plan,
            ["reviewableDiff"] = diff,
            ["requestedAt"] = DateTimeOffset.UtcNow,
            ["correlationId"] = CurrentCorrelationId
        });

        var stored = await _documents.UpsertAsync(AdaptationsCollection, id, body, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        if (stored.IsFailure || stored.Data is null)
        {
            return Failure<IReadOnlyDictionary<string, object?>>(
                stored.Error?.Code ?? "ADAPTATION_STORE_FAILED",
                stored.Error?.Message ?? "Adaptation request could not be stored.");
        }

        await AuditAsync(request.WorkspaceId, "ADAPTATION_PLAN_CREATED", request.RequestedBy, stored.Data.Body, cancellationToken)
            .ConfigureAwait(false);
        return Success(stored.Data.Body);
    }

    public async Task<DataProcessResult<IReadOnlyDictionary<string, object?>>> ReviewDiffAsync(
        ReviewAdaptationDiffRequest request,
        CancellationToken cancellationToken = default)
    {
        var workspace = await RequireWorkspaceAsync(request.WorkspaceId, cancellationToken).ConfigureAwait(false);
        if (workspace.IsFailure)
        {
            return Failure<IReadOnlyDictionary<string, object?>>(workspace.Error!.Code, workspace.Error.Message, workspace.Error.Details);
        }

        var existing = await _documents.GetAsync(AdaptationsCollection, request.AdaptationId, cancellationToken)
            .ConfigureAwait(false);
        if (existing.IsFailure || existing.Data is null || !BelongsToCurrentTenant(existing.Data.Body))
        {
            return Failure<IReadOnlyDictionary<string, object?>>("ADAPTATION_NOT_FOUND", "Adaptation request was not found in the current tenant.");
        }

        var decision = request.Decision.Equals("approved", StringComparison.OrdinalIgnoreCase) ? "APPROVED" : "REJECTED";
        var updated = new Dictionary<string, object?>(existing.Data.Body)
        {
            ["status"] = decision == "APPROVED" ? "APPROVED_FOR_APPLY_OR_EXPORT" : "REJECTED_BY_TENANT",
            ["reviewedBy"] = request.ReviewedBy,
            ["reviewNote"] = request.ReviewNote,
            ["reviewedAt"] = DateTimeOffset.UtcNow
        };

        var stored = await _documents.UpsertAsync(AdaptationsCollection, request.AdaptationId, updated, existing.Data.Version, cancellationToken)
            .ConfigureAwait(false);
        if (stored.IsFailure || stored.Data is null)
        {
            return Failure<IReadOnlyDictionary<string, object?>>(
                stored.Error?.Code ?? "ADAPTATION_REVIEW_STORE_FAILED",
                stored.Error?.Message ?? "Adaptation review could not be stored.");
        }

        await AuditAsync(request.WorkspaceId, $"ADAPTATION_{decision}", request.ReviewedBy, stored.Data.Body, cancellationToken)
            .ConfigureAwait(false);
        return Success(stored.Data.Body);
    }

    public async Task<DataProcessResult<IReadOnlyDictionary<string, object?>?>> GetWorkspaceAsync(
        string workspaceId,
        CancellationToken cancellationToken = default)
    {
        var workspace = await _documents.GetAsync(WorkspacesCollection, workspaceId, cancellationToken).ConfigureAwait(false);
        if (workspace.IsFailure)
        {
            return Failure<IReadOnlyDictionary<string, object?>?>(
                workspace.Error?.Code ?? "WORKSPACE_READ_FAILED",
                workspace.Error?.Message ?? "Workspace could not be read.");
        }

        if (workspace.Data is null || !BelongsToCurrentTenant(workspace.Data.Body))
        {
            return Success<IReadOnlyDictionary<string, object?>?>(null);
        }

        return Success<IReadOnlyDictionary<string, object?>?>(workspace.Data.Body);
    }

    public async Task<DataProcessResult<IReadOnlyList<IReadOnlyDictionary<string, object?>>>> ListAuditAsync(
        string workspaceId,
        CancellationToken cancellationToken = default)
    {
        var audit = await _documents.SearchAsync(
            AuditCollection,
            new Dictionary<string, object?> { ["tenant_id"] = CurrentTenant.TenantId, ["workspaceId"] = workspaceId },
            cancellationToken).ConfigureAwait(false);
        return audit.IsSuccess
            ? Success<IReadOnlyList<IReadOnlyDictionary<string, object?>>>(audit.Data?.Select(item => item.Body).ToArray() ?? Array.Empty<IReadOnlyDictionary<string, object?>>())
            : Failure<IReadOnlyList<IReadOnlyDictionary<string, object?>>>(
                audit.Error?.Code ?? "AUDIT_READ_FAILED",
                audit.Error?.Message ?? "Adaptation audit could not be read.");
    }

    private async Task<DataProcessResult<IReadOnlyDictionary<string, object?>>> RequireWorkspaceAsync(
        string workspaceId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(workspaceId))
        {
            return Failure<IReadOnlyDictionary<string, object?>>("WORKSPACE_ID_REQUIRED", "Workspace id is required.");
        }

        var workspace = await GetWorkspaceAsync(workspaceId, cancellationToken).ConfigureAwait(false);
        if (workspace.IsFailure)
        {
            return Failure<IReadOnlyDictionary<string, object?>>(workspace.Error!.Code, workspace.Error.Message, workspace.Error.Details);
        }

        return workspace.Data is null
            ? Failure<IReadOnlyDictionary<string, object?>>("WORKSPACE_NOT_FOUND", "Workspace was not found in the current tenant.")
            : Success(workspace.Data);
    }

    private async Task AuditAsync(
        string workspaceId,
        string action,
        string actor,
        IReadOnlyDictionary<string, object?> subject,
        CancellationToken cancellationToken)
    {
        var id = StableId("audit", $"{workspaceId}:{action}:{DateTimeOffset.UtcNow:O}:{CurrentCorrelationId}");
        var body = WithTenant(new Dictionary<string, object?>
        {
            ["id"] = id,
            ["workspaceId"] = workspaceId,
            ["action"] = action,
            ["actor"] = actor,
            ["subjectId"] = subject.TryGetValue("id", out var subjectId) ? subjectId : null,
            ["policy"] = subject.TryGetValue("policyKind", out var policy) ? policy : null,
            ["diffStatus"] = subject.TryGetValue("reviewableDiff", out var diff) ? diff : null,
            ["result"] = subject.TryGetValue("status", out var status) ? status : action,
            ["correlationId"] = CurrentCorrelationId,
            ["createdAt"] = DateTimeOffset.UtcNow
        });
        await _documents.UpsertAsync(AuditCollection, id, body, cancellationToken: cancellationToken).ConfigureAwait(false);
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

    private static string StableId(string prefix, string value) =>
        $"{prefix}-{Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value))).ToLowerInvariant()[..16]}";
}
