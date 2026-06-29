# xiigen-platform-dot-net-angular::InMemoryOutboxStore.AppendAsync

## Meaning
xiigen-platform-dot-net-angular::InMemoryOutboxStore.AppendAsync implements local project behavior with return type Task<DataProcessResult<OutboxRecord>>. It has no detected internal callee. It is called by xiigen-platform-dot-net-angular::tests\Xiigen.Fabrics.InMemory.Tests\InMemoryFabricTests.cs:43::xiigen-platform-dot-net-angular::InMemoryFabricTests.OutboxStore_AppendsCloudEventEnvelope.

## Location
src\Xiigen.Fabrics.InMemory\InMemoryOutboxStore.cs:10

## Parameters
- CloudEventEnvelope envelope
- CancellationToken cancellationToken

## Return
Task<DataProcessResult<OutboxRecord>> returned by xiigen-platform-dot-net-angular::InMemoryOutboxStore.AppendAsync

## Called By
- xiigen-platform-dot-net-angular::tests\Xiigen.Fabrics.InMemory.Tests\InMemoryFabricTests.cs:43::xiigen-platform-dot-net-angular::InMemoryFabricTests.OutboxStore_AppendsCloudEventEnvelope

## Calls
- none recorded

## Stack And Version
Target framework: unknown
C# language version: CSharp5_or_CSharp6_review_required

## Evidence
- xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryOutboxStore.cs:10::xiigen-platform-dot-net-angular::InMemoryOutboxStore.AppendAsync