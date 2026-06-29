# xiigen-platform-dot-net-angular::InMemoryIdempotencyStore.TryReserveAsync

## Meaning
xiigen-platform-dot-net-angular::InMemoryIdempotencyStore.TryReserveAsync implements local project behavior with return type Task<DataProcessResult<IdempotencyReservation>>. It has no detected internal callee. It is called by xiigen-platform-dot-net-angular::tests\Xiigen.Fabrics.InMemory.Tests\InMemoryFabricTests.cs:26::xiigen-platform-dot-net-angular::InMemoryFabricTests.IdempotencyStore_ReplaysSucceededReservation.

## Location
src\Xiigen.Fabrics.InMemory\InMemoryIdempotencyStore.cs:10

## Parameters
- string operationKey
- TimeSpan ttl
- CancellationToken cancellationToken

## Return
Task<DataProcessResult<IdempotencyReservation>> returned by xiigen-platform-dot-net-angular::InMemoryIdempotencyStore.TryReserveAsync

## Called By
- xiigen-platform-dot-net-angular::tests\Xiigen.Fabrics.InMemory.Tests\InMemoryFabricTests.cs:26::xiigen-platform-dot-net-angular::InMemoryFabricTests.IdempotencyStore_ReplaysSucceededReservation

## Calls
- none recorded

## Stack And Version
Target framework: unknown
C# language version: CSharp5_or_CSharp6_review_required

## Evidence
- xiigen-platform-dot-net-angular::src\Xiigen.Fabrics.InMemory\InMemoryIdempotencyStore.cs:10::xiigen-platform-dot-net-angular::InMemoryIdempotencyStore.TryReserveAsync