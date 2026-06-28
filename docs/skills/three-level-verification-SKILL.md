# Three-Level Verification (XIIGen Enterprise Platform)

## Overview

A fix can pass every unit test and still break at a higher level, because unit
tests use stubs that do not exercise the real code paths. A fix is **done** when
three levels confirm it — not when the unit test alone goes green.

This is the platform port of the `llm_mvp_core` skill of the same name. It binds
the universal Unit -> Integration -> Suite protocol to this repository's stack:
**.NET 10 ASP.NET Core Web API**, **xUnit** test projects under `tests/`, the
`DataProcessResult<T>` wrapper in `Xiigen.Kernel`, and the real fabric/kernel
classes (`Xiigen.Fabrics.InMemory`, `Xiigen.Fabrics.Scheduler`, `Xiigen.Auth`,
`Xiigen.Kernel`). The platform is backend-only, so there are **no `na`
exclusions** — all three levels apply.

## When to load

- After any fix to a fabric, kernel, auth, or marketplace service.
- After any change to a constructor or a `DataProcessResult<T>` method signature.
- Before declaring any bug fix complete.
- Before a phase-end gate that the active `SESSION-XX` file opens.

## The problem this skill solves

Three failure scenarios this protocol catches:

1. **Unit passes, integration fails.** The fix works in isolation but breaks when
   real dependencies are wired together. The stub was too permissive and accepted
   inputs the real type never produces (e.g. a stub `IDocumentStore` that always
   returns `Success`, hiding a real optimistic-concurrency conflict).
2. **Integration passes, suite fails.** The integration test used specific inputs;
   the full project suite covers edge cases (empty body, idempotency replay,
   tenant-scope mismatch) the narrow test missed.
3. **All pass locally but the wrong thing was fixed.** The fix corrects the
   test-visible output but leaves the root cause in place; a later test exposes it.

## The 3-level protocol

Run in sequence. Stop at the first level that fails — do not skip ahead.

### Level 1 — Unit test (`[Fact]` / `[Theory]` with stubs)

**Command:**
```bash
dotnet test --filter "FullyQualifiedName~<ClassName>"
```

**Passes Level 1 when:**
- the specific `[Fact]` for the fixed behaviour passes;
- 0 test failures in the class;
- the assertion covers the specific output path that was fixed — assert
  `result.Data!.<field>` and/or `result.Error!.Code`, not just `result.IsSuccess`.

**Fails Level 1 when:**
- any test in the class fails;
- the test passes only because a stub intercepts (bypasses) the fixed path.

**Stop rule:** if Level 1 fails, fix the unit, re-run Level 1, do not proceed.

### Level 2 — Integration test (real classes, no stubs on the fixed path)

Write a `[Fact]`/`[Theory]` that wires **real** classes together for the fixed
path. No stubs on the interfaces involved in the fix — use real implementations,
constructed directly or through a `ServiceCollection`.

```csharp
// Level 2 — real Xiigen.Kernel idempotency semantics over the real InMemory fabric
[Fact]
public async Task IdempotencyStore_SameKeyTwice_SecondReservationReflectsExisting()
{
    // Arrange: real fabric, not a stub
    IIdempotencyStore store = new InMemoryIdempotencyStore();   // real

    // Act: reserve the same operation key twice
    var first  = await store.TryReserveAsync("op-1", TimeSpan.FromMinutes(5));
    var second = await store.TryReserveAsync("op-1", TimeSpan.FromMinutes(5));

    // Assert: read the real DataProcessResult shape, not the absence of an exception
    Assert.True(first.IsSuccess);
    Assert.True(second.IsSuccess);
    Assert.Equal("op-1", second.Data!.Key);                     // real reservation, real key
}
```

**Passes Level 2 when:**
- real classes are wired for the fixed path;
- the fix holds with real-shaped inputs (not stub-shaped ones);
- the `DataProcessResult<T>` shape is correct throughout the chain
  (`IsSuccess` / `Data` / `Error.Code`).

**Fails Level 2 when:**
- the test fails with real inputs even though the Level 1 stub test passed;
- `DataProcessResult.Failure` is returned on a path that should succeed;
- a field that one service produces does not match what the next expects.

**Stop rule:** a Level 2 failure after a Level 1 pass means the unit stub was too
permissive and was masking the real problem.

### Level 3 — Full project suite

**Command:**
```bash
dotnet test tests/<Project>.Tests 2>&1 | tail -5
# or, when the session opens the broad gate:
dotnet test Xiigen.Platform.slnx 2>&1 | tail -10
```

**Passes Level 3 when:**
- 0 failures;
- count >= the session-start baseline for that project (record the baseline at
  session start; do not assume a fixed number across the solution);
- no test that passed before the fix is now failing.

**Fails Level 3 when:**
- any test fails;
- count decreased (a test was accidentally broken or deleted — find it).

**Stop rule:** a Level 3 failure after Levels 1 and 2 passed means the fix has a
side effect. The target behaviour is correct but something else broke; find the
newly failing test and fix its root cause.

## Quick routing table

| What was fixed | Level 1 spec | Level 2 spec | Level 3 command |
|---------------|-------------|-------------|-----------------|
| `IDocumentStore` upsert/version logic | `[Fact]` over the document store | Real `InMemoryDocumentStore` + real consumer service | `dotnet test tests/Xiigen.Kernel.Tests` |
| `IIdempotencyStore` reserve/replay | `[Fact]` over the idempotency store | Real fabric, two reservations | `dotnet test tests/Xiigen.Kernel.Tests` |
| `ISchedulerService` due/enqueue path | scheduler `[Fact]` with `MockDateTimeProvider` | Real scheduler + controlled clock | `dotnet test tests/Xiigen.Fabrics.Scheduler.Tests` |
| `Xiigen.Auth` token/guard logic | auth service `[Fact]` | Real auth service + real fabric | `dotnet test tests/Xiigen.Auth.Tests` |
| `DataProcessResult<T>` method signature | all `[Fact]`s for that service | Caller + callee wired together | `dotnet test tests/<Project>.Tests` |
| `Xiigen.Platform.Api` endpoint | handler `[Fact]` | `WebApplicationFactory<Program>` request | `dotnet test tests/Xiigen.Platform.Api.Tests` |

## Anti-patterns

### Anti-pattern 1: Declaring done after Level 1 only
```text
X "the specific [Fact] passes" -> phase complete
V Run Level 2 (integration) before declaring done — stubs may be too permissive
```

### Anti-pattern 2: Skipping Level 2 because "it takes longer"
```text
X Level 1 passes -> skip to Level 3
V Level 2 catches stub-masking failures that Level 3 cannot diagnose as quickly
```

### Anti-pattern 3: Level 2 with the same stubs as Level 1
```text
X Level 2 reuses the same stub IDocumentStore as Level 1 and never exercises the fixed path
V Level 2 uses real classes for the components involved in the fix; only stub
  things that are genuinely not under test
```

### Anti-pattern 4: Trusting Level 3 without Level 2
```text
X Full suite passes -> the fix must be correct
V Full suite passing only means no regression; Level 2 provides the proof of
  correctness for the specific scenario
```

## Integration with platform governance

A green three-level pass is slice evidence, not final authority. Under
`AGENTS.md` / `CLAUDE.md`, the executor hands the result to `REVIEW_INTERNAL`;
only an architect verdict authorizes final reporting, and only when
`STATE-v3.3.json` `stop_allowed` is true and `COMPLETION-MATRIX-v3.3` has no
required `UNKNOWN`/`FAIL` rows. Run only the Gate-B commands the active
`SESSION-XX` file opens.
