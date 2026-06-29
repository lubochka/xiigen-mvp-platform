# xiigen-platform-dot-net-angular::InMemoryIdempotencyStore.MarkSucceededAsync

## Meaning
xiigen-platform-dot-net-angular::InMemoryIdempotencyStore.MarkSucceededAsync implements local project behavior with return type Task<DataProcessResult>. It has no detected internal callee. It is called by xiigen-platform-dot-net-angular::tests\Xiigen.Fabrics.InMemory.Tests\InMemoryFabricTests.cs:26::xiigen-platform-dot-net-angular::InMemoryFabricTests.IdempotencyStore_ReplaysSucceededReservation.

## Location
src\Xiigen.Fabrics.InMemory\InMemoryIdempotencyStore.cs:28

## Parameters
- string operationKey
- IReadOnlyDictionary<string
- object?> response
- CancellationToken cancellationToken

## Return
Task<DataProcessResult> returned by xiigen-platform-dot-net-angular::InMemoryIdempotencyStore.MarkSucceededAsync

## Called By
- xiigen-platform-dot-net-angular::tests\Xiigen.Fabrics.InMemory.Tests\InMemoryFabricTests.cs:26::xiigen-platform-dot-net-angular::InMemoryFabricTests.IdempotencyStore_ReplaysSucceededReservation

## Calls
- none recorded

## Stack And Version
Target framework: unknown
C# language version: CSharp5_or_CSharp6_review_required

## Evidence
- xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryIdempotencyStore.cs:28::xiigen-platform-dot-net-angular::InMemoryIdempotencyStore.MarkSucceededAsync