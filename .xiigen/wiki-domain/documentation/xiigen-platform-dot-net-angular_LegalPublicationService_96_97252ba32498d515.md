# xiigen-platform-dot-net-angular::LegalPublicationService.UpsertDocumentAsync

## Meaning
xiigen-platform-dot-net-angular::LegalPublicationService.UpsertDocumentAsync implements local project behavior with return type Task<DataProcessResult<LegalDocument>>. It delegates to xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:43::xiigen-platform-dot-net-angular::InMemoryDocumentStore.UpsertAsync, xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\LegalPublicationService.cs:145::xiigen-platform-dot-net-angular::LegalPublicationService.NormalizeLocale. It is called by xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\LegalPublicationServiceTests.cs:26::xiigen-platform-dot-net-angular::LegalPublicationServiceTests.UpsertDocumentAsync_RejectsApprovedDocumentWithoutApprovalMetadata, xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\LegalPublicationServiceTests.cs:44::xiigen-platform-dot-net-angular::LegalPublicationServiceTests.GetRouteAsync_PublishesOnlyApprovedDocumentWithApprovalMetadata.

## Location
src\Xiigen.Platform.Marketplace\LegalPublicationService.cs:96

## Parameters
- LegalDocument document
- CancellationToken cancellationToken

## Return
Task<DataProcessResult<LegalDocument>> returned by xiigen-platform-dot-net-angular::LegalPublicationService.UpsertDocumentAsync

## Called By
- xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\LegalPublicationServiceTests.cs:26::xiigen-platform-dot-net-angular::LegalPublicationServiceTests.UpsertDocumentAsync_RejectsApprovedDocumentWithoutApprovalMetadata
- xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\LegalPublicationServiceTests.cs:44::xiigen-platform-dot-net-angular::LegalPublicationServiceTests.GetRouteAsync_PublishesOnlyApprovedDocumentWithApprovalMetadata

## Calls
- xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:43::xiigen-platform-dot-net-angular::InMemoryDocumentStore.UpsertAsync
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\LegalPublicationService.cs:145::xiigen-platform-dot-net-angular::LegalPublicationService.NormalizeLocale

## Stack And Version
Target framework: unknown
C# language version: CSharp5_or_CSharp6_review_required

## Evidence
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\LegalPublicationService.cs:96::xiigen-platform-dot-net-angular::LegalPublicationService.UpsertDocumentAsync