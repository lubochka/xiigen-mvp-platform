# xiigen-platform-dot-net-angular::SmartWikiServiceTests.GetIndexAsync_ReturnsNullForCrossTenantAccess

## Meaning
xiigen-platform-dot-net-angular::SmartWikiServiceTests.GetIndexAsync_ReturnsNullForCrossTenantAccess implements local project behavior with return type Task. It delegates to xiigen-platform-dot-net-angular::src\Xiigen.Kernel\CorrelationContext.cs:17::xiigen-platform-dot-net-angular::AsyncLocalCorrelationContextAccessor.Push, xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\SmartWikiService.cs:27::xiigen-platform-dot-net-angular::SmartWikiService.IndexGraphAsync, xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\SmartWikiService.cs:154::xiigen-platform-dot-net-angular::SmartWikiService.GetIndexAsync, xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\LegalPublicationServiceTests.cs:73::xiigen-platform-dot-net-angular::LegalPublicationServiceTests.CreateHarness. No internal caller was detected.

## Location
tests\Xiigen.Platform.Marketplace.Tests\SmartWikiServiceTests.cs:93

## Parameters
- none recorded

## Return
Task returned by xiigen-platform-dot-net-angular::SmartWikiServiceTests.GetIndexAsync_ReturnsNullForCrossTenantAccess

## Called By
- none recorded

## Calls
- xiigen-platform-dot-net-angular::src\Xiigen.Kernel\CorrelationContext.cs:17::xiigen-platform-dot-net-angular::AsyncLocalCorrelationContextAccessor.Push
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\SmartWikiService.cs:27::xiigen-platform-dot-net-angular::SmartWikiService.IndexGraphAsync
- xiigen-platform-dot-net-angular::src\Xiigen.Platform.Marketplace\SmartWikiService.cs:154::xiigen-platform-dot-net-angular::SmartWikiService.GetIndexAsync
- xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\LegalPublicationServiceTests.cs:73::xiigen-platform-dot-net-angular::LegalPublicationServiceTests.CreateHarness

## Stack And Version
Target framework: unknown
C# language version: CSharp5_or_CSharp6_review_required

## Evidence
- xiigen-platform-dot-net-angular::tests\Xiigen.Platform.Marketplace.Tests\SmartWikiServiceTests.cs:93::xiigen-platform-dot-net-angular::SmartWikiServiceTests.GetIndexAsync_ReturnsNullForCrossTenantAccess