# xiigen-platform-dot-net-angular::SmartCodeAssistantService.RecordMetricAsync

## Meaning
xiigen-platform-dot-net-angular::SmartCodeAssistantService.RecordMetricAsync implements local project behavior with return type object?>>>. It delegates to xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:43::xiigen-platform-dot-net-angular::InMemoryDocumentStore.UpsertAsync, xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\SmartCodeAssistantService.cs:181::xiigen-platform-dot-net-angular::SmartCodeAssistantService.WithTenant. It is called by xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\SmartCodeAssistantServiceTests.cs:123::xiigen-platform-dot-net-angular::SmartCodeAssistantServiceTests.RecordMetricAsync_StoresRuntimeMetric.

## Location
src\Xiigen.Platform.Marketplace\SmartCodeAssistantService.cs:159

## Parameters
- SmartCodeAssistantMetricRequest request
- CancellationToken cancellationToken

## Return
object?>>> returned by xiigen-platform-dot-net-angular::SmartCodeAssistantService.RecordMetricAsync

## Called By
- xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\SmartCodeAssistantServiceTests.cs:123::xiigen-platform-dot-net-angular::SmartCodeAssistantServiceTests.RecordMetricAsync_StoresRuntimeMetric

## Calls
- xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:43::xiigen-platform-dot-net-angular::InMemoryDocumentStore.UpsertAsync
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\SmartCodeAssistantService.cs:181::xiigen-platform-dot-net-angular::SmartCodeAssistantService.WithTenant

## Stack And Version
Target framework: unknown
C# language version: CSharp5_or_CSharp6_review_required

## Evidence
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\SmartCodeAssistantService.cs:159::xiigen-platform-dot-net-angular::SmartCodeAssistantService.RecordMetricAsync