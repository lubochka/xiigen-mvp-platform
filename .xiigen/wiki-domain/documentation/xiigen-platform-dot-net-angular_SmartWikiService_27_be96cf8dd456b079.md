# xiigen-platform-dot-net-angular::SmartWikiService.IndexGraphAsync

## Meaning
xiigen-platform-dot-net-angular::SmartWikiService.IndexGraphAsync implements local project behavior with return type object?>>>. It delegates to xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:43::xiigen-platform-dot-net-angular::InMemoryDocumentStore.UpsertAsync, xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\SmartCodeAssistantService.cs:181::xiigen-platform-dot-net-angular::SmartCodeAssistantService.WithTenant. It is called by xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\SmartWikiServiceTests.cs:11::xiigen-platform-dot-net-angular::SmartWikiServiceTests.IndexGraphAsync_StoresLinkedGraphAsTenantLocalWikiData, xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\SmartWikiServiceTests.cs:33::xiigen-platform-dot-net-angular::SmartWikiServiceTests.AnswerAsync_ReturnsCitedAnswersForAdvancedQuestions, xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\SmartWikiServiceTests.cs:55::xiigen-platform-dot-net-angular::SmartWikiServiceTests.AnswerAsync_AbstainsForStoredProcedureImpactWhenSqlEvidenceIsMissing, xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\SmartWikiServiceTests.cs:74::xiigen-platform-dot-net-angular::SmartWikiServiceTests.ReindexAsync_RecordsGraphChecksumChange, xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\SmartWikiServiceTests.cs:93::xiigen-platform-dot-net-angular::SmartWikiServiceTests.GetIndexAsync_ReturnsNullForCrossTenantAccess.

## Location
src\Xiigen.Platform.Marketplace\SmartWikiService.cs:27

## Parameters
- SmartWikiIndexRequest request
- CancellationToken cancellationToken

## Return
object?>>> returned by xiigen-platform-dot-net-angular::SmartWikiService.IndexGraphAsync

## Called By
- xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\SmartWikiServiceTests.cs:11::xiigen-platform-dot-net-angular::SmartWikiServiceTests.IndexGraphAsync_StoresLinkedGraphAsTenantLocalWikiData
- xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\SmartWikiServiceTests.cs:33::xiigen-platform-dot-net-angular::SmartWikiServiceTests.AnswerAsync_ReturnsCitedAnswersForAdvancedQuestions
- xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\SmartWikiServiceTests.cs:55::xiigen-platform-dot-net-angular::SmartWikiServiceTests.AnswerAsync_AbstainsForStoredProcedureImpactWhenSqlEvidenceIsMissing
- xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\SmartWikiServiceTests.cs:74::xiigen-platform-dot-net-angular::SmartWikiServiceTests.ReindexAsync_RecordsGraphChecksumChange
- xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\SmartWikiServiceTests.cs:93::xiigen-platform-dot-net-angular::SmartWikiServiceTests.GetIndexAsync_ReturnsNullForCrossTenantAccess

## Calls
- xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:43::xiigen-platform-dot-net-angular::InMemoryDocumentStore.UpsertAsync
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\SmartCodeAssistantService.cs:181::xiigen-platform-dot-net-angular::SmartCodeAssistantService.WithTenant

## Stack And Version
Target framework: unknown
C# language version: CSharp5_or_CSharp6_review_required

## Evidence
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\SmartWikiService.cs:27::xiigen-platform-dot-net-angular::SmartWikiService.IndexGraphAsync