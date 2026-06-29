# xiigen-platform-dot-net-angular::TenantAdaptationStudioService.CreateWorkspaceAsync

## Meaning
xiigen-platform-dot-net-angular::TenantAdaptationStudioService.CreateWorkspaceAsync implements local project behavior with return type object?>>>. It delegates to xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:43::xiigen-platform-dot-net-angular::InMemoryDocumentStore.UpsertAsync, xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\SmartCodeAssistantService.cs:181::xiigen-platform-dot-net-angular::SmartCodeAssistantService.WithTenant, xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\TenantAdaptationStudioService.cs:313::xiigen-platform-dot-net-angular::TenantAdaptationStudioService.AuditAsync. It is called by xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\TenantAdaptationStudioServiceTests.cs:11::xiigen-platform-dot-net-angular::TenantAdaptationStudioServiceTests.CreateWorkspaceAsync_StoresTenantScopedWorkspaceAndAudit, xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\TenantAdaptationStudioServiceTests.cs:141::xiigen-platform-dot-net-angular::TenantAdaptationStudioServiceTests.CreateWorkspaceIdAsync.

## Location
src\Xiigen.Platform.Marketplace\TenantAdaptationStudioService.cs:29

## Parameters
- CreateTenantWorkspaceRequest request
- CancellationToken cancellationToken

## Return
object?>>> returned by xiigen-platform-dot-net-angular::TenantAdaptationStudioService.CreateWorkspaceAsync

## Called By
- xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\TenantAdaptationStudioServiceTests.cs:11::xiigen-platform-dot-net-angular::TenantAdaptationStudioServiceTests.CreateWorkspaceAsync_StoresTenantScopedWorkspaceAndAudit
- xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\TenantAdaptationStudioServiceTests.cs:141::xiigen-platform-dot-net-angular::TenantAdaptationStudioServiceTests.CreateWorkspaceIdAsync

## Calls
- xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:43::xiigen-platform-dot-net-angular::InMemoryDocumentStore.UpsertAsync
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\SmartCodeAssistantService.cs:181::xiigen-platform-dot-net-angular::SmartCodeAssistantService.WithTenant
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\TenantAdaptationStudioService.cs:313::xiigen-platform-dot-net-angular::TenantAdaptationStudioService.AuditAsync

## Stack And Version
Target framework: unknown
C# language version: CSharp5_or_CSharp6_review_required

## Evidence
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\TenantAdaptationStudioService.cs:29::xiigen-platform-dot-net-angular::TenantAdaptationStudioService.CreateWorkspaceAsync