# Retroactive Development (XIIGen Enterprise Platform)

## Overview

**Fix the engine that produces wrong output, not the output itself.** Patching a
generated file or a downstream consumer and leaving the generator broken ships the
same bug next time.

This is the platform port of the `llm_mvp_core` skill of the same name. The
universal content is: trace to the source, fix the engine, re-validate all flows
using that component, prove the fix is **generative** (correct for the whole class
of inputs, tested with >=3 inputs of the same type), and check the blast radius
across all callers. It is bound to **.NET 10 / xUnit** (`[Theory]` + `[InlineData]`
for parametrization), `DataProcessResult<T>`, and the platform projects
(`Xiigen.Kernel`, `Xiigen.Fabrics.*`, `Xiigen.Auth`, `Xiigen.Platform.Marketplace`).
Backend-only — no `na` exclusions.

## Core principle

If `InMemoryDocumentStore` produces a wrong record, fix the store class. If the
scheduler selects the wrong due items, fix the due-selection logic. Patching the
caller (e.g. adding a compensating check in `LegalPublicationService`) or editing
a generated artifact leaves the generator broken.

## Fix propagation protocol

### Step 1 — Identify the source

Trace backward from the symptom to the generator:

```text
Symptom: a published legal document has the wrong version on disk
  | trace
Immediate cause: LegalPublicationService receives a wrong record from the store
  | trace
Root cause: InMemoryDocumentStore.UpsertAsync increments version even on a no-op
  | source found
Fix location: src/Xiigen.Fabrics.InMemory/InMemoryDocumentStore.cs
```

Apply `root-cause-tracing` / `systematic-debugging` if the trace is non-obvious.

### Step 2 — Fix the engine

Fix the source class. Do not patch downstream consumers or generated output.

```csharp
// V Fix in InMemoryDocumentStore — the generator of the record:
public Task<DataProcessResult<DocumentRecord>> UpsertAsync(
    string collection, string id, IReadOnlyDictionary<string, object?> body,
    string? expectedVersion = null, CancellationToken ct = default)
{
    // ... enforce the version rule HERE, return DataProcessResult<DocumentRecord>.Failure(...) on conflict
}

// X Patch in the consumer — treating the symptom:
//   adding a "if version looks wrong, fix it" branch in LegalPublicationService
// X Editing a generated/exported artifact by hand
```

### Step 3 — Re-validate all flows using the same component

```bash
# Find all tests that exercise the fixed class:
grep -rln "InMemoryDocumentStore\|IDocumentStore" tests/ --include="*.cs"

# Run those test classes:
dotnet test --filter "FullyQualifiedName~DocumentStore OR FullyQualifiedName~LegalPublication"
# -> Must show: Failed: 0
```

### Step 4 — Verify the fix is generative, not cosmetic

The fix must make the engine produce the right output for **all** inputs of the
same type, not just the one reported input.

```text
Cosmetic fix: handles the one reported collection/id -> other inputs still broken
Generative fix: fixes the version rule -> all upserts respect optimistic concurrency
```

Test with at least 3 different inputs of the same type using `[Theory]`:

```csharp
[Theory]
[InlineData("1", false)]   // stale expectedVersion -> must fail
[InlineData("2", true)]    // matching version      -> must succeed
[InlineData(null, true)]   // no expectedVersion     -> unconditional upsert succeeds
public async Task DocumentStore_UpsertVersionRule_BehavesForAllInputs(string? expectedVersion, bool shouldSucceed)
{
    // Arrange: real store with a record at version "2"
    IDocumentStore store = new InMemoryDocumentStore();
    await store.UpsertAsync("legal", "tos", new Dictionary<string, object?> { ["v"] = 2 });

    // Act
    var result = await store.UpsertAsync(
        "legal", "tos", new Dictionary<string, object?> { ["v"] = 3 }, expectedVersion);

    // Assert: the rule holds for every input class
    Assert.Equal(shouldSucceed, result.IsSuccess);
    if (!shouldSucceed) Assert.Equal("VERSION_CONFLICT", result.Error!.Code);
}
```

## Blast radius after an engine fix

An engine fix can silently break callers that relied on the wrong output.

```bash
# 1. Find all callers of the fixed method (across Kernel / Fabrics / Marketplace / Api):
grep -rn "UpsertAsync\|IDocumentStore" src/ --include="*.cs"

# 2. For each caller, verify its tests still pass:
dotnet test --filter "FullyQualifiedName~<CallerClass>"

# 3. Run the full project suite:
dotnet test tests/Xiigen.Kernel.Tests 2>&1 | tail -10
# -> Failed: 0
```

## When generated / exported artifacts must be updated

Sometimes fixing the engine makes previously-generated/exported output wrong. That
is correct — it should be regenerated.

```text
Steps:
1. Fix the engine.
2. Re-run the generation/export to produce new output.
3. Verify the new output is correct (not just different).
4. Commit BOTH the engine fix AND the regenerated output together.
5. Document in the commit message: "fix InMemoryDocumentStore version rule; regenerate <artifact>".
```

Do not leave stale generated/exported output after an engine fix.

## Anti-patterns

1. **Patch the output** — editing a generated/exported file instead of the
   generator.
2. **One-case fix** — handling only the reported input, not the class of inputs
   (use `[Theory]` with >=3 inputs).
3. **Skip blast radius** — fixing the engine without verifying callers still pass.
4. **Silent output staleness** — fixing the engine and not regenerating dependent
   output.
5. **Post-hoc test** — writing the regression test after the fix passes (see
   `bug-to-tests-SKILL.md`: tests must FAIL first).

## Integration

- `bug-to-tests-SKILL.md` provides the failing-tests-first protocol; this skill
  governs Step 4 (fix the engine) and the blast radius.
- `three-level-verification-SKILL.md` is the Unit -> Integration -> Suite ladder
  used to confirm the fix and the regression.
- The active `SESSION-XX` file governs when the broad `dotnet test` gate opens.
