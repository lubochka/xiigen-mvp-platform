# xiigen-platform-dot-net-angular::MarketplaceService.CreateTenantForkAsync

## Meaning
xiigen-platform-dot-net-angular::MarketplaceService.CreateTenantForkAsync implements local project behavior with return type object?>>>. It delegates to xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:43::xiigen-platform-dot-net-angular::InMemoryDocumentStore.UpsertAsync, xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\MarketplaceService.cs:83::xiigen-platform-dot-net-angular::MarketplaceService.GetListingAsync. It is called by xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\MarketplaceServiceTests.cs:47::xiigen-platform-dot-net-angular::MarketplaceServiceTests.CreateTenantForkAsync_UsesApprovedTenantRepoNaming, xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\MarketplaceServiceTests.cs:68::xiigen-platform-dot-net-angular::MarketplaceServiceTests.ApplyAdaptationAsync_PreservesAdaptationHistory.

## Location
src\Xiigen.Platform.Marketplace\MarketplaceService.cs:144

## Parameters
- TenantForkRequest request
- CancellationToken cancellationToken

## Return
object?>>> returned by xiigen-platform-dot-net-angular::MarketplaceService.CreateTenantForkAsync

## Called By
- xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\MarketplaceServiceTests.cs:47::xiigen-platform-dot-net-angular::MarketplaceServiceTests.CreateTenantForkAsync_UsesApprovedTenantRepoNaming
- xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\MarketplaceServiceTests.cs:68::xiigen-platform-dot-net-angular::MarketplaceServiceTests.ApplyAdaptationAsync_PreservesAdaptationHistory

## Calls
- xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:43::xiigen-platform-dot-net-angular::InMemoryDocumentStore.UpsertAsync
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\MarketplaceService.cs:83::xiigen-platform-dot-net-angular::MarketplaceService.GetListingAsync

## Stack And Version
Target framework: unknown
C# language version: CSharp5_or_CSharp6_review_required

## Evidence
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\MarketplaceService.cs:144::xiigen-platform-dot-net-angular::MarketplaceService.CreateTenantForkAsync