# xiigen-platform-dot-net-angular::LegalPublicationService.GetRouteAsync

## Meaning
xiigen-platform-dot-net-angular::LegalPublicationService.GetRouteAsync implements local project behavior with return type Task<DataProcessResult<LegalRouteDocumentView>>. It delegates to xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:11::xiigen-platform-dot-net-angular::InMemoryDocumentStore.GetAsync, xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\LegalPublicationService.cs:145::xiigen-platform-dot-net-angular::LegalPublicationService.NormalizeLocale. It is called by xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\LegalPublicationServiceTests.cs:11::xiigen-platform-dot-net-angular::LegalPublicationServiceTests.GetRouteAsync_ReturnsBlockedShellWhenApprovedDocumentIsAbsent, xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\LegalPublicationServiceTests.cs:44::xiigen-platform-dot-net-angular::LegalPublicationServiceTests.GetRouteAsync_PublishesOnlyApprovedDocumentWithApprovalMetadata, xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\LegalPublicationServiceTests.cs:61::xiigen-platform-dot-net-angular::LegalPublicationServiceTests.GetRouteAsync_RejectsUnsupportedLocale.

## Location
src\Xiigen.Platform.Marketplace\LegalPublicationService.cs:43

## Parameters
- string slug
- string locale
- CancellationToken cancellationToken

## Return
Task<DataProcessResult<LegalRouteDocumentView>> returned by xiigen-platform-dot-net-angular::LegalPublicationService.GetRouteAsync

## Called By
- xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\LegalPublicationServiceTests.cs:11::xiigen-platform-dot-net-angular::LegalPublicationServiceTests.GetRouteAsync_ReturnsBlockedShellWhenApprovedDocumentIsAbsent
- xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\LegalPublicationServiceTests.cs:44::xiigen-platform-dot-net-angular::LegalPublicationServiceTests.GetRouteAsync_PublishesOnlyApprovedDocumentWithApprovalMetadata
- xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\LegalPublicationServiceTests.cs:61::xiigen-platform-dot-net-angular::LegalPublicationServiceTests.GetRouteAsync_RejectsUnsupportedLocale

## Calls
- xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:11::xiigen-platform-dot-net-angular::InMemoryDocumentStore.GetAsync
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\LegalPublicationService.cs:145::xiigen-platform-dot-net-angular::LegalPublicationService.NormalizeLocale

## Stack And Version
Target framework: unknown
C# language version: CSharp5_or_CSharp6_review_required

## Evidence
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\LegalPublicationService.cs:43::xiigen-platform-dot-net-angular::LegalPublicationService.GetRouteAsync