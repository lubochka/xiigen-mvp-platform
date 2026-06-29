# xiigen-platform-dot-net-angular::TenantAdaptationStudioService.ListAuditAsync

## Meaning
xiigen-platform-dot-net-angular::TenantAdaptationStudioService.ListAuditAsync implements local project behavior with return type object?>>>>. It delegates to xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:26::xiigen-platform-dot-net-angular::InMemoryDocumentStore.SearchAsync. It is called by xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\TenantAdaptationStudioServiceTests.cs:11::xiigen-platform-dot-net-angular::TenantAdaptationStudioServiceTests.CreateWorkspaceAsync_StoresTenantScopedWorkspaceAndAudit, xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\TenantAdaptationStudioServiceTests.cs:96::xiigen-platform-dot-net-angular::TenantAdaptationStudioServiceTests.ReviewDiffAsync_ApprovesPlanWithoutSkippingAudit.

## Location
src\Xiigen.Platform.Marketplace\TenantAdaptationStudioService.cs:278

## Parameters
- string workspaceId
- CancellationToken cancellationToken

## Return
object?>>>> returned by xiigen-platform-dot-net-angular::TenantAdaptationStudioService.ListAuditAsync

## Called By
- xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\TenantAdaptationStudioServiceTests.cs:11::xiigen-platform-dot-net-angular::TenantAdaptationStudioServiceTests.CreateWorkspaceAsync_StoresTenantScopedWorkspaceAndAudit
- xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\TenantAdaptationStudioServiceTests.cs:96::xiigen-platform-dot-net-angular::TenantAdaptationStudioServiceTests.ReviewDiffAsync_ApprovesPlanWithoutSkippingAudit

## Calls
- xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:26::xiigen-platform-dot-net-angular::InMemoryDocumentStore.SearchAsync

## Stack And Version
Target framework: unknown
C# language version: CSharp5_or_CSharp6_review_required

## Evidence
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\TenantAdaptationStudioService.cs:278::xiigen-platform-dot-net-angular::TenantAdaptationStudioService.ListAuditAsync