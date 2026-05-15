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
