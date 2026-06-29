# User Journey Testing (XIIGen Enterprise Platform)

## Overview

This skill prevents the "all unit tests pass but the feature does not work"
failure. Individual class tests validate classes; this skill validates a complete
feature **flow** as acceptance criteria. The scenarios are written **before** the
implementation, not after, and become the phase Gate-B conditions.

This is the platform port of the `llm_mvp_core` skill of the same name. The
universal content is: scenario-first authoring (happy / guard / edge), guard
tests that assert a **specific error code** (not just "an error returned"), a
`SCENARIO-MANIFEST` with statuses, and binding each scenario to Gate-B.

It is a **distinct, added** discipline on this platform — it is not covered by
`e2e-test-matrix-SKILL.md` (which gives the 8-category matrix) or
`qa-session-type-SKILL.md` (which gives a post-Gate-B verdict). Neither carries
"scenario-as-acceptance-before-code + manifest".

Stack binding:

- a **"journey"** = an HTTP sequence through `Xiigen.Platform.Api` ->
  `Xiigen.Kernel` -> a fabric (`Xiigen.Fabrics.InMemory` / `Xiigen.Fabrics.Scheduler`),
  i.e. **at least two modules** wired together (use `WebApplicationFactory<Program>`
  for the API entry, or construct the service chain directly);
- a **guard** = a `[Fact]` asserting a concrete `result.Error!.Code` from
  `DataProcessResult<T>` (`Xiigen.Kernel`);
- acceptance commands = `dotnet test --filter "FullyQualifiedName~<Class>"`;
- `SCENARIO-MANIFEST.md` lists scenarios per endpoint-flow with statuses.

> **UI part = `na`.** There is no React/Angular user-visible layer in this repo.
> Only the **server-side journey** (HTTP request -> kernel -> fabric -> response)
> applies. The canonical core example is a backend `[Fact]`
> (`DataProcessResult.Success` / a `DIMENSION_MISMATCH`-style code), not a UI flow.

## When to load

- At the start of any phase that implements a server-visible feature/endpoint.
- Produce test stubs **before** writing production code; the stubs become
  acceptance criteria that constrain what the implementation must produce.

## Step 1 — Extract scenarios from the feature description

Every feature description contains a happy path, guard conditions (invalid input,
missing data, permission denied), and edge cases (empty result, single item,
maximum size). Map each to a scenario:

```text
Feature: Publish a legal document version through Xiigen.Platform.Api

HAPPY PATH:
  S-01: Valid publish request stores the version and returns the published record
  Expected: result.IsSuccess == true; Data.Version increments; record visible via IDocumentStore

GUARD CONDITIONS:
  S-02: Publish with a stale expectedVersion (optimistic-concurrency conflict)
    Expected: result.IsFailure == true; result.Error!.Code == "VERSION_CONFLICT"
  S-03: Publish from the wrong tenant context
    Expected: result.IsFailure == true; result.Error!.Code == "UNAUTHORIZED"

EDGE CASES:
  S-04: Publish an already-published identical body (idempotent replay)
    Expected: result.IsSuccess == true; no duplicate effect (one logical version)
```

## Step 2 — Write test stubs before implementation

Each scenario becomes a stub written **before** the production class. The stub
fails until the production path exists — that is correct; it is an acceptance
criterion.

```csharp
// S-01: Happy path — valid publish returns the published record
[Fact]
public async Task PublishLegalDocument_ValidRequest_ReturnsPublishedRecord()
{
    // Arrange: TODO — build the service chain once the endpoint/service exists
    // Act: TODO — send a valid publish request
    // Assert: result.IsSuccess; result.Data.Version incremented
    Assert.Fail("TODO: implement after the publish path exists — S-01 acceptance criteria");
}
```

`Assert.Fail(...)` (xUnit) makes the unacceptance explicit. These stubs are
**Class 1** failures in `test-failure-triage-SKILL.md` until the production path
is implemented.

## Step 3 — Write guard-condition tests (the most valuable tests)

Guard conditions are the failure modes that matter most. Each guard test must
(1) set up the guard condition, (2) execute the action, (3) verify the **specific
expected behaviour** — not just "an error returned".

```csharp
// S-02: Guard condition — optimistic-concurrency conflict
[Fact]
public async Task PublishLegalDocument_StaleVersion_ReturnsVersionConflict()
{
    // Arrange: real IDocumentStore with an existing record at version "2"
    IDocumentStore store = new InMemoryDocumentStore();
    await store.UpsertAsync("legal", "tos", new Dictionary<string, object?> { ["v"] = 2 });

    // Act: upsert with a stale expectedVersion
    var result = await store.UpsertAsync(
        "legal", "tos",
        new Dictionary<string, object?> { ["v"] = 3 },
        expectedVersion: "1");   // stale

    // Assert: SPECIFIC failure (behavioral-assertion-gate: assert Error.Code, not just IsFailure)
    Assert.True(result.IsFailure);
    Assert.Equal("VERSION_CONFLICT", result.Error!.Code);   // specific code, not any error
}
```

**Behavioral-assertion-gate.** A guard test that only asserts `result.IsFailure`
(or only `result.IsSuccess`) is too weak — failures are not interchangeable.
Always assert the specific `result.Error!.Code`. (This is the same gate the
platform `test-integrity-SKILL.md` enforces.)

## Step 4 — Attach to phase Gate-B

The scenarios become the Gate-B conditions for the phase:

```text
Gate B for the publish phase:
  dotnet test --filter "FullyQualifiedName~PublishLegalDocument" shows:
    S-01 (valid publish):        PASS
    S-02 (stale version):        PASS  (Error.Code == "VERSION_CONFLICT")
    S-03 (wrong tenant):         PASS  (Error.Code == "UNAUTHORIZED")
    S-04 (idempotent replay):    PASS
  Total: Failed: 0, Passed: >= 4
```

## Scenario file structure

One file per feature-flow in the test project, plus a manifest:

```text
tests/Xiigen.Platform.Marketplace.Tests/
  Legal/
    PublishLegalDocumentTests.cs   <- happy path + guard conditions + edge cases
    SCENARIO-MANIFEST.md           <- lists all scenarios with status
```

`SCENARIO-MANIFEST.md`:

```markdown
## Publish Legal Document — Scenario Manifest

| Scenario | Test | Status |
|---------|------|--------|
| S-01: Valid publish -> published record | PublishLegalDocument_ValidRequest_ReturnsPublishedRecord | PASS |
| S-02: Stale version -> VERSION_CONFLICT | PublishLegalDocument_StaleVersion_ReturnsVersionConflict | PASS |
| S-03: Wrong tenant -> UNAUTHORIZED | PublishLegalDocument_WrongTenant_ReturnsUnauthorized | PASS |
| S-04: Idempotent replay -> single effect | PublishLegalDocument_IdempotentReplay_NoDuplicate | PASS |
```

## Anti-patterns

1. **Writing acceptance tests after implementation** — "tests to match the code"
   validate nothing.
2. **Guard tests that check only the return flag, not the error code** —
   specificity is the point; assert `result.Error!.Code`.
3. **Happy path only** — guard conditions are where most bugs live.
4. **Tests that only check `result.IsFailure` / `result.IsSuccess`** — too weak;
   failures are not interchangeable.
5. **Not linking scenarios to Gate-B** — a scenario not in Gate-B can be forgotten.

## Integration

- `e2e-test-matrix-SKILL.md` supplies the 8-category coverage for each journey.
- `qa-session-type-SKILL.md` consumes this `SCENARIO-MANIFEST` as its acceptance
  criteria (Step 2).
- `test-integrity-SKILL.md` enforces the same behavioral-assertion-gate.
- The active `SESSION-XX` file governs when the broad `dotnet test` Gate-B opens.
