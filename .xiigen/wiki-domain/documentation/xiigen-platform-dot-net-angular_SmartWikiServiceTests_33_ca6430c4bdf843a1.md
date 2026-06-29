# xiigen-platform-dot-net-angular::SmartWikiServiceTests.AnswerAsync_ReturnsCitedAnswersForAdvancedQuestions

## Meaning
xiigen-platform-dot-net-angular::SmartWikiServiceTests.AnswerAsync_ReturnsCitedAnswersForAdvancedQuestions implements local project behavior with return type Task. It delegates to xiigen-platform-dot-net-angular::src\Xiigen.Kernel\CorrelationContext.cs:17::xiigen-platform-dot-net-angular::AsyncLocalCorrelationContextAccessor.Push, xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\SmartWikiService.cs:27::xiigen-platform-dot-net-angular::SmartWikiService.IndexGraphAsync, xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\SmartWikiService.cs:69::xiigen-platform-dot-net-angular::SmartWikiService.AnswerAsync, xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\LegalPublicationServiceTests.cs:73::xiigen-platform-dot-net-angular::LegalPublicationServiceTests.CreateHarness. No internal caller was detected.

## Location
tests\Xiigen.Platform.Marketplace.Tests\SmartWikiServiceTests.cs:33

## Parameters
- string questionKind
- string subject
- string expectedStatus

## Return
Task returned by xiigen-platform-dot-net-angular::SmartWikiServiceTests.AnswerAsync_ReturnsCitedAnswersForAdvancedQuestions

## Called By
- none recorded

## Calls
- xiigen-platform-dot-net-angular::src\Xiigen.Kernel\CorrelationContext.cs:17::xiigen-platform-dot-net-angular::AsyncLocalCorrelationContextAccessor.Push
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\SmartWikiService.cs:27::xiigen-platform-dot-net-angular::SmartWikiService.IndexGraphAsync
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\SmartWikiService.cs:69::xiigen-platform-dot-net-angular::SmartWikiService.AnswerAsync
- xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\LegalPublicationServiceTests.cs:73::xiigen-platform-dot-net-angular::LegalPublicationServiceTests.CreateHarness

## Stack And Version
Target framework: unknown
C# language version: CSharp5_or_CSharp6_review_required

## Evidence
- xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\SmartWikiServiceTests.cs:33::xiigen-platform-dot-net-angular::SmartWikiServiceTests.AnswerAsync_ReturnsCitedAnswersForAdvancedQuestions