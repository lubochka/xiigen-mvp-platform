# Bug To Tests (XIIGen Enterprise Platform)

## Overview

Every confirmed bug gets three **failing** tests (unit + integration + suite)
**before** any fix is written. The tests are confirmed failing, then the engine
that produces the wrong output is fixed — never the tests. Tests before fix,
always.

This is the platform port of the `llm_mvp_core` skill of the same name. The
universal content is: the **Iron Rule** (changing a test assertion requires
Luba's approval), a BUG-ID before work starts, the 6-step protocol, and the
zero-regression gate (count must not drop below baseline). It is bound to **.NET
10 / xUnit**, `DataProcessResult<T>` (`IsSuccess` / `Data` / `Error.Code`),
`ServiceCollection`-based integration, and the platform projects. Backend-only —
no `na` exclusions.

## The Iron Rule

**Test assertion change = Luba approval required.**

Changing an assertion, relaxing a comparison, or removing a test to make it pass
is not a fix. All require explicit written approval.

## Bug ID format

Every bug gets an ID before work starts, written to the active state file or a
dedicated `BUG-LOG.md`:

```text
BUG-NN: [one-line description]
Component: [project or class name]
Symptom: [what was observed]
Hypothesis: [suspected root cause — one sentence]
```

Examples:
```text
BUG-01: IDocumentStore.UpsertAsync overwrites despite a stale expectedVersion
Component: Xiigen.Fabrics.InMemory / InMemoryDocumentStore
Symptom: UpsertAsync(..., expectedVersion: "1") succeeds when stored version is "2"
Hypothesis: the optimistic-concurrency check compares the wrong field (or is missing)

BUG-02: IIdempotencyStore.TryReserveAsync double-processes after a crash window
Component: Xiigen.Fabrics.InMemory / InMemoryIdempotencyStore
Symptom: two reservations of the same key both return a fresh PENDING status
Hypothesis: reservations are not persisted before the second TryReserveAsync reads
```

## The 6-step protocol

### Step 1 — Assign the bug ID
Write `BUG-NN` to the active `STATE-v3.3.json` (or a dedicated `BUG-LOG.md` if
volume warrants).

### Step 2 — Write 3 FAILING tests (all three levels) before touching production code

```csharp
// Unit test — pure logic, real object, no DI
[Fact]
public async Task DocumentStore_StaleVersion_ReturnsVersionConflict()
{
    // Arrange: real fabric with an existing record
    IDocumentStore store = new InMemoryDocumentStore();
    await store.UpsertAsync("legal", "tos", new Dictionary<string, object?> { ["v"] = 2 });

    // Act: upsert with a stale expectedVersion
    var result = await store.UpsertAsync(
        "legal", "tos",
        new Dictionary<string, object?> { ["v"] = 3 },
        expectedVersion: "1");

    // Assert: SPECIFIC failure (read DataProcessResult, not exceptions)
    Assert.True(result.IsFailure);
    Assert.Equal("VERSION_CONFLICT", result.Error!.Code);
}

// Integration test — real DI via ServiceCollection, in-process
[Fact]
public async Task Marketplace_PublishWithStaleVersion_PropagatesConflict()
{
    var services = new ServiceCollection();
    services.AddSingleton<IDocumentStore, InMemoryDocumentStore>();
    services.AddSingleton<LegalPublicationService>();
    await using var provider = services.BuildServiceProvider();
    var publisher = provider.GetRequiredService<LegalPublicationService>();

    // Act: publish twice with a stale version on the second call
    // Assert: the conflict surfaces through the service (read result.Error!.Code)
    // ...
}

// Full-suite smoke — documents the zero-regression gate (the gate is the run, not this assertion)
[Fact]
public void AllMarketplaceTests_PassAfterFix()
{
    // Gate B: dotnet test --filter "FullyQualifiedName~Marketplace" shows Failed: 0
    Assert.True(true); // placeholder — the gate is the dotnet test run
}
```

### Step 3 — Confirm all tests FAIL (before any fix)

```bash
dotnet test --filter "FullyQualifiedName~DocumentStore_StaleVersion OR FullyQualifiedName~Marketplace_PublishWithStaleVersion"
# Must show: Failed: 2, Passed: 0
```

**If a test passes before the fix, the test is wrong — rewrite it.**

### Step 4 — Fix the engine, not the tests

Fix the production class that produces the wrong output. Never change the
assertions to make them pass.

```text
Fix location: src/Xiigen.Fabrics.InMemory/InMemoryDocumentStore.cs
Fix: enforce the optimistic-concurrency check — when expectedVersion is provided
     and does not match the stored version, return
     DataProcessResult<DocumentRecord>.Failure("VERSION_CONFLICT", "...").
```

### Step 5 — Confirm all tests PASS

```bash
dotnet test --filter "FullyQualifiedName~DocumentStore_StaleVersion OR FullyQualifiedName~Marketplace_PublishWithStaleVersion"
# Must show: Passed: 2, Failed: 0
```

### Step 6 — Zero-regression gate

```bash
dotnet test tests/Xiigen.Kernel.Tests 2>&1 | tail -10
# (or the broader suite when the active SESSION-XX file opens that gate)
# -> Failed: 0
# -> total count >= session-start baseline
```

**If the total count is LOWER than baseline, a test was accidentally deleted —
find it.**

## Test level reference

| Level | xUnit pattern | What it tests |
|-------|--------------|---------------|
| Unit | `[Fact]` + `new` object directly | Pure logic, no I/O, no DI |
| Integration | `[Fact]` + `ServiceCollection` DI (or `WebApplicationFactory<Program>` for the API) | Component wiring, in-process |
| Regression gate | `dotnet test --filter "FullyQualifiedName~<Component>"` | The full component suite |

## What "fix the engine" means

Fix the class that **produces** the wrong output, not the code that consumes it.

```text
X Wrong: change the test to accept either result.IsSuccess or result.IsFailure
X Wrong: add a compensating check in the caller (e.g. in LegalPublicationService)
V Right: fix the version check inside InMemoryDocumentStore itself
```

If fixing the engine would require changing a test assertion (because the original
assertion was wrong), that needs Luba's written approval first (Iron Rule).

## Anti-patterns

1. **Fix first, write tests after** — post-hoc tests are justifications, not
   regression guards.
2. **Write tests that already pass** — Step 3 must show FAIL; immediate-pass tests
   are wrong.
3. **Change the assertion to match broken output** — that documents the bug, it
   does not fix it.
4. **One test for a bug** — bugs need unit + integration coverage minimum.
5. **Skip the zero-regression gate** — a fix that adds a new failure (or drops the
   count) is not a fix.

## Integration

- `test-failure-triage-SKILL.md` classifies the failing tests (the bug's tests are
  Class 1 until the fix lands).
- `retroactive-development-SKILL.md` governs fixing the engine + propagating across
  callers (Step 4 / blast radius).
- `three-level-verification-SKILL.md` is the Unit -> Integration -> Suite ladder
  used in Steps 2-6.
- The Iron Rule is shared with `test-failure-triage-SKILL.md` (Class 3).
