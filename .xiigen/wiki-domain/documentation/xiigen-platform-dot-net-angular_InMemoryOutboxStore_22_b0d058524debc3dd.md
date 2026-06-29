# xiigen-platform-dot-net-angular::InMemoryOutboxStore.ListAsync

## Meaning
xiigen-platform-dot-net-angular::InMemoryOutboxStore.ListAsync implements local project behavior with return type Task<DataProcessResult<IReadOnlyList<OutboxRecord>>>. It has no detected internal callee. It is called by xiigen-platform-dot-net-angular::tests\Xiigen.Fabrics.InMemory.Tests\InMemoryFabricTests.cs:43::xiigen-platform-dot-net-angular::InMemoryFabricTests.OutboxStore_AppendsCloudEventEnvelope.

## Location
src\Xiigen.Fabrics.InMemory\InMemoryOutboxStore.cs:22

## Parameters
- CancellationToken cancellationToken

## Return
Task<DataProcessResult<IReadOnlyList<OutboxRecord>>> returned by xiigen-platform-dot-net-angular::InMemoryOutboxStore.ListAsync

## Called By
- xiigen-platform-dot-net-angular::tests\Xiigen.Fabrics.InMemory.Tests\InMemoryFabricTests.cs:43::xiigen-platform-dot-net-angular::InMemoryFabricTests.OutboxStore_AppendsCloudEventEnvelope

## Calls
- none recorded

## Stack And Version
Target framework: unknown
C# language version: CSharp5_or_CSharp6_review_required

## Evidence
- xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryOutboxStore.cs:22::xiigen-platform-dot-net-angular::InMemoryOutboxStore.ListAsync