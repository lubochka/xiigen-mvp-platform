# xiigen-platform-dot-net-angular::TenantAdaptationStudioService.ReviewDiffAsync

## Meaning
xiigen-platform-dot-net-angular::TenantAdaptationStudioService.ReviewDiffAsync implements local project behavior with return type object?>>>. It delegates to xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:11::xiigen-platform-dot-net-angular::InMemoryDocumentStore.GetAsync, xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:43::xiigen-platform-dot-net-angular::InMemoryDocumentStore.UpsertAsync, xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\TenantAdaptationStudioService.cs:293::xiigen-platform-dot-net-angular::TenantAdaptationStudioService.RequireWorkspaceAsync, xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\TenantAdaptationStudioService.cs:313::xiigen-platform-dot-net-angular::TenantAdaptationStudioService.AuditAsync. It is called by xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\TenantAdaptationStudioServiceTests.cs:96::xiigen-platform-dot-net-angular::TenantAdaptationStudioServiceTests.ReviewDiffAsync_ApprovesPlanWithoutSkippingAudit.

## Location
src\Xiigen.Platform.Marketplace\TenantAdaptationStudioService.cs:218

## Parameters
- ReviewAdaptationDiffRequest request
- CancellationToken cancellationToken

## Return
object?>>> returned by xiigen-platform-dot-net-angular::TenantAdaptationStudioService.ReviewDiffAsync

## Called By
- xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\TenantAdaptationStudioServiceTests.cs:96::xiigen-platform-dot-net-angular::TenantAdaptationStudioServiceTests.ReviewDiffAsync_ApprovesPlanWithoutSkippingAudit

## Calls
- xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:11::xiigen-platform-dot-net-angular::InMemoryDocumentStore.GetAsync
- xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:43::xiigen-platform-dot-net-angular::InMemoryDocumentStore.UpsertAsync
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\TenantAdaptationStudioService.cs:293::xiigen-platform-dot-net-angular::TenantAdaptationStudioService.RequireWorkspaceAsync
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\TenantAdaptationStudioService.cs:313::xiigen-platform-dot-net-angular::TenantAdaptationStudioService.AuditAsync

## Stack And Version
Target framework: unknown
C# language version: CSharp5_or_CSharp6_review_required

## Evidence
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\TenantAdaptationStudioService.cs:218::xiigen-platform-dot-net-angular::TenantAdaptationStudioService.ReviewDiffAsync