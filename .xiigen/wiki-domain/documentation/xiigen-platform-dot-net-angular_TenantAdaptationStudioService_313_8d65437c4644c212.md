# xiigen-platform-dot-net-angular::TenantAdaptationStudioService.AuditAsync

## Meaning
xiigen-platform-dot-net-angular::TenantAdaptationStudioService.AuditAsync implements local project behavior with return type Task. It delegates to xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:43::xiigen-platform-dot-net-angular::InMemoryDocumentStore.UpsertAsync, xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\SmartCodeAssistantService.cs:181::xiigen-platform-dot-net-angular::SmartCodeAssistantService.WithTenant. No internal caller was detected.

## Location
docs\plans\enterprise-adaptation\v3.3\project-assets\export-package\xiigen-primary-master-enterprise-export-v1.0.0\package-root\app\src\Xiigen.Platform.Marketplace\TenantAdaptationStudioService.cs:313

## Parameters
- string workspaceId
- string action
- string actor
- IReadOnlyDictionary<string
- object?> subject
- CancellationToken cancellationToken

## Return
Task returned by xiigen-platform-dot-net-angular::TenantAdaptationStudioService.AuditAsync

## Called By
- none recorded

## Calls
- xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:43::xiigen-platform-dot-net-angular::InMemoryDocumentStore.UpsertAsync
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\SmartCodeAssistantService.cs:181::xiigen-platform-dot-net-angular::SmartCodeAssistantService.WithTenant

## Stack And Version
Target framework: unknown
C# language version: CSharp5_or_CSharp6_review_required

## Evidence
- xiigen-platform-dot-net-angular::docs\plans\enterprise-adaptation\v3.3\project-assets\export-package\xiigen-primary-master-enterprise-export-v1.0.0\package-root\app\src\Xiigen.Platform.Marketplace\TenantAdaptationStudioService.cs:313::xiigen-platform-dot-net-angular::TenantAdaptationStudioService.AuditAsync