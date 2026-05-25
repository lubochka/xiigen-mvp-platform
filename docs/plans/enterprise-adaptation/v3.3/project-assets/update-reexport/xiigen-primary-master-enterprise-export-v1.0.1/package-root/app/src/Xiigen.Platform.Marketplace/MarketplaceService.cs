using Microsoft.Extensions.Logging;
using Xiigen.Kernel;

namespace Xiigen.Platform.Marketplace;

public sealed class MarketplaceService : MicroserviceBase<MarketplaceService>, IMarketplaceService
{
    private const string ListingsCollection = "marketplace-listings";
    private const string ReadbacksCollection = "marketplace-package-readbacks";
    private const string ForksCollection = "tenant-fork-provenance";
    private readonly IDocumentStore _documents;

    public MarketplaceService(
        IDocumentStore documents,
        ITenantContextAccessor tenantContextAccessor,
        ICorrelationContextAccessor correlationContextAccessor,
        ILogger<MarketplaceService> logger)
        : base(
            new ServiceDescriptor("xiigen-platform-marketplace", "XIIGen Platform Marketplace", "PLATFORM"),
            tenantContextAccessor,
            correlationContextAccessor,
            logger)
    {
        _documents = documents;
    }

    public async Task<DataProcessResult<IReadOnlyDictionary<string, object?>>> PublishListingAsync(
        PublishMarketplaceListingRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FlowId))
        {
            return Failure<IReadOnlyDictionary<string, object?>>("FLOW_ID_REQUIRED", "Flow id is required.");
        }

        if (string.IsNullOrWhiteSpace(request.FlowSlug))
        {
            return Failure<IReadOnlyDictionary<string, object?>>("FLOW_SLUG_REQUIRED", "Flow slug is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ModuleName))
        {
            return Failure<IReadOnlyDictionary<string, object?>>("MODULE_NAME_REQUIRED", "Module name is required.");
        }

        if (request.Packages.Count == 0)
        {
            return Failure<IReadOnlyDictionary<string, object?>>("PACKAGE_REQUIRED", "At least one package is required.");
        }

        var now = DateTimeOffset.UtcNow;
        var version = request.Packages[0].Version;
        var id = ListingId(request.FlowSlug, version);
        var body = new Dictionary<string, object?>
        {
            ["id"] = id,
            ["flowId"] = request.FlowId,
            ["flowSlug"] = request.FlowSlug,
            ["version"] = version,
            ["moduleName"] = request.ModuleName,
            ["sourceRepo"] = request.SourceRepo,
            ["sourcePath"] = request.SourcePath,
            ["commitHash"] = request.CommitHash,
            ["publisherTenantId"] = CurrentTenant.TenantId,
            ["status"] = "listed",
            ["packages"] = request.Packages.Select(ToDictionary).ToArray(),
            ["metadata"] = new Dictionary<string, object?>(request.Metadata),
            ["adaptationHistory"] = Array.Empty<IReadOnlyDictionary<string, object?>>(),
            ["createdAt"] = now,
            ["updatedAt"] = now,
            ["correlationId"] = CurrentCorrelationId
        };

        var stored = await _documents.UpsertAsync(ListingsCollection, id, body, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return stored.IsSuccess && stored.Data is not null
            ? Success(stored.Data.Body)
            : Failure<IReadOnlyDictionary<string, object?>>(
                stored.Error?.Code ?? "LISTING_STORE_FAILED",
                stored.Error?.Message ?? "Marketplace listing could not be stored.");
    }

    public async Task<DataProcessResult<IReadOnlyDictionary<string, object?>?>> GetListingAsync(
        string flowSlug,
        string version,
        CancellationToken cancellationToken = default)
    {
        var result = await _documents.GetAsync(ListingsCollection, ListingId(flowSlug, version), cancellationToken)
            .ConfigureAwait(false);
        return result.IsSuccess
            ? Success<IReadOnlyDictionary<string, object?>?>(result.Data?.Body)
            : Failure<IReadOnlyDictionary<string, object?>?>(
                result.Error?.Code ?? "LISTING_READ_FAILED",
                result.Error?.Message ?? "Marketplace listing could not be read.");
    }

    public async Task<DataProcessResult<IReadOnlyList<IReadOnlyDictionary<string, object?>>>> ListListingsAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await _documents.SearchAsync(ListingsCollection, new Dictionary<string, object?>(), cancellationToken)
            .ConfigureAwait(false);
        return result.IsSuccess
            ? Success<IReadOnlyList<IReadOnlyDictionary<string, object?>>>(result.Data?.Select(item => item.Body).ToArray()
                ?? Array.Empty<IReadOnlyDictionary<string, object?>>())
            : Failure<IReadOnlyList<IReadOnlyDictionary<string, object?>>>(
                result.Error?.Code ?? "LISTING_SEARCH_FAILED",
                result.Error?.Message ?? "Marketplace listings could not be searched.");
    }

    public async Task<DataProcessResult<IReadOnlyDictionary<string, object?>>> RecordPackageReadbackAsync(
        PackageReadbackRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!request.Found)
        {
            return Failure<IReadOnlyDictionary<string, object?>>("PACKAGE_READBACK_MISSING", "Package registry readback did not find the package.");
        }

        var id = $"{ListingId(request.FlowSlug, request.Version)}:{request.PackageKind}:{request.PackageId}";
        var body = new Dictionary<string, object?>
        {
            ["id"] = id,
            ["flowSlug"] = request.FlowSlug,
            ["version"] = request.Version,
            ["packageId"] = request.PackageId,
            ["registryUrl"] = request.RegistryUrl,
            ["packageKind"] = request.PackageKind,
            ["found"] = request.Found,
            ["metadata"] = new Dictionary<string, object?>(request.Metadata),
            ["tenantId"] = CurrentTenant.TenantId,
            ["verifiedAt"] = DateTimeOffset.UtcNow,
            ["correlationId"] = CurrentCorrelationId
        };

        var stored = await _documents.UpsertAsync(ReadbacksCollection, id, body, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return stored.IsSuccess && stored.Data is not null
            ? Success(stored.Data.Body)
            : Failure<IReadOnlyDictionary<string, object?>>(
                stored.Error?.Code ?? "READBACK_STORE_FAILED",
                stored.Error?.Message ?? "Package readback evidence could not be stored.");
    }

    public async Task<DataProcessResult<IReadOnlyDictionary<string, object?>>> CreateTenantForkAsync(
        TenantForkRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.TargetTenantId))
        {
            return Failure<IReadOnlyDictionary<string, object?>>("TARGET_TENANT_REQUIRED", "Target tenant id is required.");
        }

        if (string.IsNullOrWhiteSpace(request.ModuleName))
        {
            return Failure<IReadOnlyDictionary<string, object?>>("MODULE_NAME_REQUIRED", "Module name is required.");
        }

        var listing = await GetListingAsync(request.FlowSlug, request.Version, cancellationToken).ConfigureAwait(false);
        if (listing.IsFailure || listing.Data is null)
        {
            return Failure<IReadOnlyDictionary<string, object?>>("LISTING_NOT_FOUND", "Cannot fork a missing marketplace listing.");
        }

        var repoName = $"{request.TargetTenantId}--{request.ModuleName}";
        var id = $"{ListingId(request.FlowSlug, request.Version)}:{request.TargetTenantId}";
        var body = new Dictionary<string, object?>
        {
            ["id"] = id,
            ["flowSlug"] = request.FlowSlug,
            ["version"] = request.Version,
            ["sourceTenantId"] = request.SourceTenantId,
            ["targetTenantId"] = request.TargetTenantId,
            ["moduleName"] = request.ModuleName,
            ["repoName"] = repoName,
            ["sourceListingId"] = listing.Data["id"],
            ["requestedBy"] = request.RequestedBy,
            ["metadata"] = new Dictionary<string, object?>(request.Metadata),
            ["adaptationHistory"] = Array.Empty<IReadOnlyDictionary<string, object?>>(),
            ["createdAt"] = DateTimeOffset.UtcNow,
            ["correlationId"] = CurrentCorrelationId
        };

        var stored = await _documents.UpsertAsync(ForksCollection, id, body, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return stored.IsSuccess && stored.Data is not null
            ? Success(stored.Data.Body)
            : Failure<IReadOnlyDictionary<string, object?>>(
                stored.Error?.Code ?? "FORK_STORE_FAILED",
                stored.Error?.Message ?? "Tenant fork provenance could not be stored.");
    }

    public async Task<DataProcessResult<IReadOnlyDictionary<string, object?>>> ApplyAdaptationAsync(
        ApplyAdaptationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!string.Equals(request.FreedomKey, "flow_module_name", StringComparison.Ordinal))
        {
            return Failure<IReadOnlyDictionary<string, object?>>("UNSUPPORTED_ADAPTATION_KEY", "Only flow_module_name adaptation is supported for Layer 8.");
        }

        var forkId = $"{ListingId(request.FlowSlug, request.Version)}:{request.TenantId}";
        var fork = await _documents.GetAsync(ForksCollection, forkId, cancellationToken).ConfigureAwait(false);
        if (fork.IsFailure || fork.Data is null)
        {
            return Failure<IReadOnlyDictionary<string, object?>>("FORK_NOT_FOUND", "Cannot adapt a missing tenant fork.");
        }

        var history = ReadHistory(fork.Data.Body)
            .Append(new Dictionary<string, object?>
            {
                ["freedomKey"] = request.FreedomKey,
                ["previousModuleName"] = request.ModuleName,
                ["newModuleName"] = request.ProposedValue,
                ["appliedBy"] = request.AppliedBy,
                ["metadata"] = new Dictionary<string, object?>(request.Metadata),
                ["appliedAt"] = DateTimeOffset.UtcNow
            })
            .ToArray();
        var updated = new Dictionary<string, object?>(fork.Data.Body)
        {
            ["moduleName"] = request.ProposedValue,
            ["repoName"] = $"{request.TenantId}--{request.ProposedValue}",
            ["adaptationHistory"] = history,
            ["updatedAt"] = DateTimeOffset.UtcNow
        };

        var stored = await _documents.UpsertAsync(ForksCollection, forkId, updated, fork.Data.Version, cancellationToken)
            .ConfigureAwait(false);
        return stored.IsSuccess && stored.Data is not null
            ? Success(stored.Data.Body)
            : Failure<IReadOnlyDictionary<string, object?>>(
                stored.Error?.Code ?? "ADAPTATION_STORE_FAILED",
                stored.Error?.Message ?? "Tenant adaptation could not be stored.");
    }

    private static string ListingId(string flowSlug, string version) => $"{flowSlug}:{version}";

    private static Dictionary<string, object?> ToDictionary(MarketplacePackageDescriptor package) => new()
    {
        ["packageId"] = package.PackageId,
        ["version"] = package.Version,
        ["registryUrl"] = package.RegistryUrl,
        ["packageKind"] = package.PackageKind
    };

    private static IEnumerable<IReadOnlyDictionary<string, object?>> ReadHistory(IReadOnlyDictionary<string, object?> body)
    {
        return body.TryGetValue("adaptationHistory", out var value) &&
            value is IEnumerable<IReadOnlyDictionary<string, object?>> history
                ? history
                : Array.Empty<IReadOnlyDictionary<string, object?>>();
    }
}
