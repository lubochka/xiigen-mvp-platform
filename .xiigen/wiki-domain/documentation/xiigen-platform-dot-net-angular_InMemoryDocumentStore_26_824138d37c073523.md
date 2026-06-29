# xiigen-platform-dot-net-angular::InMemoryDocumentStore.SearchAsync

## Meaning
xiigen-platform-dot-net-angular::InMemoryDocumentStore.SearchAsync implements local project behavior with return type Task<DataProcessResult<IReadOnlyList<DocumentRecord>>>. It delegates to xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:77::xiigen-platform-dot-net-angular::InMemoryDocumentStore.Matches. It is called by xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\MarketplaceService.cs:97::xiigen-platform-dot-net-angular::MarketplaceService.ListListingsAsync, xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\TenantAdaptationStudioService.cs:278::xiigen-platform-dot-net-angular::TenantAdaptationStudioService.ListAuditAsync, xiigen-platform-dot-net-angular::docs\plans\enterprise-adaptation\v3.3\project-assets\export-package\xiigen-primary-master-enterprise-export-v1.0.0\package-root\app\src\Xiigen.Platform.Marketplace\MarketplaceService.cs:97::xiigen-platform-dot-net-angular::MarketplaceService.ListListingsAsync, xiigen-platform-dot-net-angular::docs\plans\enterprise-adaptation\v3.3\project-assets\export-package\xiigen-primary-master-enterprise-export-v1.0.0\package-root\app\src\Xiigen.Platform.Marketplace\TenantAdaptationStudioService.cs:278::xiigen-platform-dot-net-angular::TenantAdaptationStudioService.ListAuditAsync, xiigen-platform-dot-net-angular::docs\plans\enterprise-adaptation\v3.3\project-assets\update-reexport\xiigen-primary-master-enterprise-export-v1.0.1\package-root\app\src\Xiigen.Platform.Marketplace\MarketplaceService.cs:97::xiigen-platform-dot-net-angular::MarketplaceService.ListListingsAsync, xiigen-platform-dot-net-angular::docs\plans\enterprise-adaptation\v3.3\project-assets\update-reexport\xiigen-primary-master-enterprise-export-v1.0.1\package-root\app\src\Xiigen.Platform.Marketplace\TenantAdaptationStudioService.cs:278::xiigen-platform-dot-net-angular::TenantAdaptationStudioService.ListAuditAsync.

## Location
src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:26

## Parameters
- string collection
- IReadOnlyDictionary<string
- object?> filter
- CancellationToken cancellationToken

## Return
Task<DataProcessResult<IReadOnlyList<DocumentRecord>>> returned by xiigen-platform-dot-net-angular::InMemoryDocumentStore.SearchAsync

## Called By
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\MarketplaceService.cs:97::xiigen-platform-dot-net-angular::MarketplaceService.ListListingsAsync
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\TenantAdaptationStudioService.cs:278::xiigen-platform-dot-net-angular::TenantAdaptationStudioService.ListAuditAsync
- xiigen-platform-dot-net-angular::docs\plans\enterprise-adaptation\v3.3\project-assets\export-package\xiigen-primary-master-enterprise-export-v1.0.0\package-root\app\src\Xiigen.Platform.Marketplace\MarketplaceService.cs:97::xiigen-platform-dot-net-angular::MarketplaceService.ListListingsAsync
- xiigen-platform-dot-net-angular::docs\plans\enterprise-adaptation\v3.3\project-assets\export-package\xiigen-primary-master-enterprise-export-v1.0.0\package-root\app\src\Xiigen.Platform.Marketplace\TenantAdaptationStudioService.cs:278::xiigen-platform-dot-net-angular::TenantAdaptationStudioService.ListAuditAsync
- xiigen-platform-dot-net-angular::docs\plans\enterprise-adaptation\v3.3\project-assets\update-reexport\xiigen-primary-master-enterprise-export-v1.0.1\package-root\app\src\Xiigen.Platform.Marketplace\MarketplaceService.cs:97::xiigen-platform-dot-net-angular::MarketplaceService.ListListingsAsync
- xiigen-platform-dot-net-angular::docs\plans\enterprise-adaptation\v3.3\project-assets\update-reexport\xiigen-primary-master-enterprise-export-v1.0.1\package-root\app\src\Xiigen.Platform.Marketplace\TenantAdaptationStudioService.cs:278::xiigen-platform-dot-net-angular::TenantAdaptationStudioService.ListAuditAsync

## Calls
- xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:77::xiigen-platform-dot-net-angular::InMemoryDocumentStore.Matches

## Stack And Version
Target framework: unknown
C# language version: CSharp5_or_CSharp6_review_required

## Evidence
- xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:26::xiigen-platform-dot-net-angular::InMemoryDocumentStore.SearchAsync