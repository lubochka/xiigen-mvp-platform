# xiigen-platform-dot-net-angular::MarketplaceService.PublishListingAsync

## Meaning
xiigen-platform-dot-net-angular::MarketplaceService.PublishListingAsync implements local project behavior with return type object?>>>. It delegates to xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:43::xiigen-platform-dot-net-angular::InMemoryDocumentStore.UpsertAsync. It is called by xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\MarketplaceServiceTests.cs:11::xiigen-platform-dot-net-angular::MarketplaceServiceTests.PublishListingAsync_StoresVisibleListing, xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\MarketplaceServiceTests.cs:47::xiigen-platform-dot-net-angular::MarketplaceServiceTests.CreateTenantForkAsync_UsesApprovedTenantRepoNaming, xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\MarketplaceServiceTests.cs:68::xiigen-platform-dot-net-angular::MarketplaceServiceTests.ApplyAdaptationAsync_PreservesAdaptationHistory.

## Location
src\Xiigen.Platform.Marketplace\MarketplaceService.cs:27

## Parameters
- PublishMarketplaceListingRequest request
- CancellationToken cancellationToken

## Return
object?>>> returned by xiigen-platform-dot-net-angular::MarketplaceService.PublishListingAsync

## Called By
- xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\MarketplaceServiceTests.cs:11::xiigen-platform-dot-net-angular::MarketplaceServiceTests.PublishListingAsync_StoresVisibleListing
- xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\MarketplaceServiceTests.cs:47::xiigen-platform-dot-net-angular::MarketplaceServiceTests.CreateTenantForkAsync_UsesApprovedTenantRepoNaming
- xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\MarketplaceServiceTests.cs:68::xiigen-platform-dot-net-angular::MarketplaceServiceTests.ApplyAdaptationAsync_PreservesAdaptationHistory

## Calls
- xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:43::xiigen-platform-dot-net-angular::InMemoryDocumentStore.UpsertAsync

## Stack And Version
Target framework: unknown
C# language version: CSharp5_or_CSharp6_review_required

## Evidence
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\MarketplaceService.cs:27::xiigen-platform-dot-net-angular::MarketplaceService.PublishListingAsync