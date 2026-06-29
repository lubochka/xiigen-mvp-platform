# xiigen-platform-dot-net-angular::SmartWikiService.AnswerAsync

## Meaning
xiigen-platform-dot-net-angular::SmartWikiService.AnswerAsync implements local project behavior with return type object?>>>. It delegates to xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:43::xiigen-platform-dot-net-angular::InMemoryDocumentStore.UpsertAsync, xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\SmartCodeAssistantService.cs:181::xiigen-platform-dot-net-angular::SmartCodeAssistantService.WithTenant, xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\SmartWikiService.cs:174::xiigen-platform-dot-net-angular::SmartWikiService.GetRequiredIndexAsync, xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\SmartWikiService.cs:189::xiigen-platform-dot-net-angular::SmartWikiService.BuildAnswer. It is called by xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\SmartWikiServiceTests.cs:33::xiigen-platform-dot-net-angular::SmartWikiServiceTests.AnswerAsync_ReturnsCitedAnswersForAdvancedQuestions, xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\SmartWikiServiceTests.cs:55::xiigen-platform-dot-net-angular::SmartWikiServiceTests.AnswerAsync_AbstainsForStoredProcedureImpactWhenSqlEvidenceIsMissing.

## Location
src\Xiigen.Platform.Marketplace\SmartWikiService.cs:69

## Parameters
- SmartWikiQuestionRequest request
- CancellationToken cancellationToken

## Return
object?>>> returned by xiigen-platform-dot-net-angular::SmartWikiService.AnswerAsync

## Called By
- xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\SmartWikiServiceTests.cs:33::xiigen-platform-dot-net-angular::SmartWikiServiceTests.AnswerAsync_ReturnsCitedAnswersForAdvancedQuestions
- xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\SmartWikiServiceTests.cs:55::xiigen-platform-dot-net-angular::SmartWikiServiceTests.AnswerAsync_AbstainsForStoredProcedureImpactWhenSqlEvidenceIsMissing

## Calls
- xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryDocumentStore.cs:43::xiigen-platform-dot-net-angular::InMemoryDocumentStore.UpsertAsync
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\SmartCodeAssistantService.cs:181::xiigen-platform-dot-net-angular::SmartCodeAssistantService.WithTenant
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\SmartWikiService.cs:174::xiigen-platform-dot-net-angular::SmartWikiService.GetRequiredIndexAsync
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\SmartWikiService.cs:189::xiigen-platform-dot-net-angular::SmartWikiService.BuildAnswer

## Stack And Version
Target framework: unknown
C# language version: CSharp5_or_CSharp6_review_required

## Evidence
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\SmartWikiService.cs:69::xiigen-platform-dot-net-angular::SmartWikiService.AnswerAsync