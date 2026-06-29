# xiigen-platform-dot-net-angular::SmartWikiService.ReindexAsync

## Meaning
xiigen-platform-dot-net-angular::SmartWikiService.ReindexAsync implements local project behavior with return type object?>>>. It delegates to xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:43::xiigen-platform-dot-net-angular::InMemoryDocumentStore.UpsertAsync, xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\SmartCodeAssistantService.cs:181::xiigen-platform-dot-net-angular::SmartCodeAssistantService.WithTenant, xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\SmartWikiService.cs:174::xiigen-platform-dot-net-angular::SmartWikiService.GetRequiredIndexAsync. It is called by xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\SmartWikiServiceTests.cs:74::xiigen-platform-dot-net-angular::SmartWikiServiceTests.ReindexAsync_RecordsGraphChecksumChange.

## Location
src\Xiigen.Platform.Marketplace\SmartWikiService.cs:108

## Parameters
- SmartWikiReindexRequest request
- CancellationToken cancellationToken

## Return
object?>>> returned by xiigen-platform-dot-net-angular::SmartWikiService.ReindexAsync

## Called By
- xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\SmartWikiServiceTests.cs:74::xiigen-platform-dot-net-angular::SmartWikiServiceTests.ReindexAsync_RecordsGraphChecksumChange

## Calls
- xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:43::xiigen-platform-dot-net-angular::InMemoryDocumentStore.UpsertAsync
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\SmartCodeAssistantService.cs:181::xiigen-platform-dot-net-angular::SmartCodeAssistantService.WithTenant
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\SmartWikiService.cs:174::xiigen-platform-dot-net-angular::SmartWikiService.GetRequiredIndexAsync

## Stack And Version
Target framework: unknown
C# language version: CSharp5_or_CSharp6_review_required

## Evidence
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\SmartWikiService.cs:108::xiigen-platform-dot-net-angular::SmartWikiService.ReindexAsync