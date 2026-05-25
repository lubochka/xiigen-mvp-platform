namespace Xiigen.Platform.Marketplace;

public sealed record MarketplacePackageDescriptor(
    string PackageId,
    string Version,
    string RegistryUrl,
    string PackageKind);

public sealed record PublishMarketplaceListingRequest(
    string FlowId,
    string FlowSlug,
    string ModuleName,
    string SourceRepo,
    string SourcePath,
    string CommitHash,
    IReadOnlyList<MarketplacePackageDescriptor> Packages,
    IReadOnlyDictionary<string, object?> Metadata);

public sealed record PackageReadbackRequest(
    string FlowSlug,
    string Version,
    string PackageId,
    string RegistryUrl,
    string PackageKind,
    bool Found,
    IReadOnlyDictionary<string, object?> Metadata);

public sealed record TenantForkRequest(
    string FlowSlug,
    string Version,
    string TargetTenantId,
    string ModuleName,
    string SourceTenantId,
    string RequestedBy,
    IReadOnlyDictionary<string, object?> Metadata);

public sealed record ApplyAdaptationRequest(
    string FlowSlug,
    string Version,
    string TenantId,
    string ModuleName,
    string FreedomKey,
    string ProposedValue,
    string AppliedBy,
    IReadOnlyDictionary<string, object?> Metadata);

public interface IMarketplaceService
{
    Task<Xiigen.Kernel.DataProcessResult<IReadOnlyDictionary<string, object?>>> PublishListingAsync(
        PublishMarketplaceListingRequest request,
        CancellationToken cancellationToken = default);

    Task<Xiigen.Kernel.DataProcessResult<IReadOnlyDictionary<string, object?>?>> GetListingAsync(
        string flowSlug,
        string version,
        CancellationToken cancellationToken = default);

    Task<Xiigen.Kernel.DataProcessResult<IReadOnlyList<IReadOnlyDictionary<string, object?>>>> ListListingsAsync(
        CancellationToken cancellationToken = default);

    Task<Xiigen.Kernel.DataProcessResult<IReadOnlyDictionary<string, object?>>> RecordPackageReadbackAsync(
        PackageReadbackRequest request,
        CancellationToken cancellationToken = default);

    Task<Xiigen.Kernel.DataProcessResult<IReadOnlyDictionary<string, object?>>> CreateTenantForkAsync(
        TenantForkRequest request,
        CancellationToken cancellationToken = default);

    Task<Xiigen.Kernel.DataProcessResult<IReadOnlyDictionary<string, object?>>> ApplyAdaptationAsync(
        ApplyAdaptationRequest request,
        CancellationToken cancellationToken = default);
}

public sealed record CreateTenantWorkspaceRequest(
    string WorkspaceName,
    string CorporateName,
    string RequestedBy,
    IReadOnlyDictionary<string, object?> Metadata);

public sealed record ConnectProjectSourceRequest(
    string WorkspaceId,
    string? RepositoryUrl,
    string? SnapshotName,
    string? SnapshotHash,
    string RequestedBy,
    IReadOnlyDictionary<string, object?> Metadata);

public sealed record CaptureTenantPolicyRequest(
    string WorkspaceId,
    string PolicyKind,
    string PolicyText,
    string Source,
    string CapturedBy);

public sealed record RequestTenantAdaptationRequest(
    string WorkspaceId,
    string Objective,
    IReadOnlyList<string> Constraints,
    string RequestedBy);

public sealed record ReviewAdaptationDiffRequest(
    string WorkspaceId,
    string AdaptationId,
    string Decision,
    string ReviewedBy,
    string ReviewNote);

public interface ITenantAdaptationStudioService
{
    Task<Xiigen.Kernel.DataProcessResult<IReadOnlyDictionary<string, object?>>> CreateWorkspaceAsync(
        CreateTenantWorkspaceRequest request,
        CancellationToken cancellationToken = default);

    Task<Xiigen.Kernel.DataProcessResult<IReadOnlyDictionary<string, object?>>> ConnectProjectSourceAsync(
        ConnectProjectSourceRequest request,
        CancellationToken cancellationToken = default);

    Task<Xiigen.Kernel.DataProcessResult<IReadOnlyDictionary<string, object?>>> CapturePolicyAsync(
        CaptureTenantPolicyRequest request,
        CancellationToken cancellationToken = default);

    Task<Xiigen.Kernel.DataProcessResult<IReadOnlyDictionary<string, object?>>> RequestAdaptationAsync(
        RequestTenantAdaptationRequest request,
        CancellationToken cancellationToken = default);

    Task<Xiigen.Kernel.DataProcessResult<IReadOnlyDictionary<string, object?>>> ReviewDiffAsync(
        ReviewAdaptationDiffRequest request,
        CancellationToken cancellationToken = default);

    Task<Xiigen.Kernel.DataProcessResult<IReadOnlyDictionary<string, object?>?>> GetWorkspaceAsync(
        string workspaceId,
        CancellationToken cancellationToken = default);

    Task<Xiigen.Kernel.DataProcessResult<IReadOnlyList<IReadOnlyDictionary<string, object?>>>> ListAuditAsync(
        string workspaceId,
        CancellationToken cancellationToken = default);
}

public sealed record SmartWikiGraphNode(
    string NodeId,
    string NodeType,
    string SourcePath,
    int? Line,
    IReadOnlyDictionary<string, object?> Metadata);

public sealed record SmartWikiGraphEdge(
    string From,
    string To,
    string EdgeType,
    string SourcePath,
    int? Line,
    IReadOnlyDictionary<string, object?> Metadata);

public sealed record SmartWikiAbstention(
    string SourceFamily,
    string Status,
    string Reason,
    string ScanCommand);

public sealed record SmartWikiIndexRequest(
    string ProjectId,
    string UnitId,
    string SourceGraphPath,
    string SourceGraphChecksum,
    IReadOnlyList<SmartWikiGraphNode> Nodes,
    IReadOnlyList<SmartWikiGraphEdge> Edges,
    IReadOnlyList<SmartWikiAbstention> Abstentions,
    string IndexedBy);

public sealed record SmartWikiQuestionRequest(
    string ProjectId,
    string QuestionKind,
    string Subject,
    string AskedBy);

public sealed record SmartWikiReindexRequest(
    string ProjectId,
    string SourceGraphChecksum,
    string ChangeSummary,
    string RequestedBy);

public interface ISmartWikiService
{
    Task<Xiigen.Kernel.DataProcessResult<IReadOnlyDictionary<string, object?>>> IndexGraphAsync(
        SmartWikiIndexRequest request,
        CancellationToken cancellationToken = default);

    Task<Xiigen.Kernel.DataProcessResult<IReadOnlyDictionary<string, object?>>> AnswerAsync(
        SmartWikiQuestionRequest request,
        CancellationToken cancellationToken = default);

    Task<Xiigen.Kernel.DataProcessResult<IReadOnlyDictionary<string, object?>>> ReindexAsync(
        SmartWikiReindexRequest request,
        CancellationToken cancellationToken = default);

    Task<Xiigen.Kernel.DataProcessResult<IReadOnlyDictionary<string, object?>?>> GetIndexAsync(
        string projectId,
        CancellationToken cancellationToken = default);
}
