# xiigen-platform-dot-net-angular::MarketplaceService.ApplyAdaptationAsync

## Meaning
xiigen-platform-dot-net-angular::MarketplaceService.ApplyAdaptationAsync implements local project behavior with return type object?>>>. It delegates to xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:11::xiigen-platform-dot-net-angular::InMemoryDocumentStore.GetAsync, xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:43::xiigen-platform-dot-net-angular::InMemoryDocumentStore.UpsertAsync, xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\MarketplaceService.cs:246::xiigen-platform-dot-net-angular::MarketplaceService.ReadHistory. It is called by xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\MarketplaceServiceTests.cs:68::xiigen-platform-dot-net-angular::MarketplaceServiceTests.ApplyAdaptationAsync_PreservesAdaptationHistory.

## Location
src\Xiigen.Platform.Marketplace\MarketplaceService.cs:192

## Parameters
- ApplyAdaptationRequest request
- CancellationToken cancellationToken

## Return
object?>>> returned by xiigen-platform-dot-net-angular::MarketplaceService.ApplyAdaptationAsync

## Called By
- xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\MarketplaceServiceTests.cs:68::xiigen-platform-dot-net-angular::MarketplaceServiceTests.ApplyAdaptationAsync_PreservesAdaptationHistory

## Calls
- xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:11::xiigen-platform-dot-net-angular::InMemoryDocumentStore.GetAsync
- xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:43::xiigen-platform-dot-net-angular::InMemoryDocumentStore.UpsertAsync
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\MarketplaceService.cs:246::xiigen-platform-dot-net-angular::MarketplaceService.ReadHistory

## Stack And Version
Target framework: unknown
C# language version: CSharp5_or_CSharp6_review_required

## Evidence
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\MarketplaceService.cs:192::xiigen-platform-dot-net-angular::MarketplaceService.ApplyAdaptationAsync