# Root Cause Tracing (XIIGen Enterprise Platform)

## Overview

Bugs often manifest deep in the call stack. Fixing where the error appears treats
a symptom. **Core principle:** trace backward through the call chain until you
find the original trigger, then fix at the source — and add defense-in-depth at
every layer.

Stack-bound to **.NET 10 ASP.NET Core Web API**, **xUnit** (`tests/Xiigen.*.Tests`),
`DataProcessResult<T>` (`Xiigen.Kernel`), the platform fabric interfaces, and
ASP.NET Core DI. The call chain for a platform request is
`Xiigen.Platform.Api endpoint → application service (MicroserviceBase) → fabric
(IDocumentStore / IOutboxStore / IIdempotencyStore / ISchedulerService) → store`.
A stack-trace line number is the crash site, NOT the root cause.

## When to invoke

- The error appears in a method that is not the entry point.
- The xUnit stack trace shows more than 2 levels.
- It is unclear where invalid data originated.
- A test fails but the test code looks correct.

## Sub-Agent-Only Parent Boundary

Under a sub-agent-only / architect-only instruction, the tracing (grep,
instrumentation, test runs) belongs in the bounded sub-agent. The parent reads
results only after the packet returns. `sub-agent waiting is not a stop`.

---

## Section 1 — The Tracing Process

### 1. Observe the symptom

```
DataProcessResult is Failure("INDEX_OUT_OF_RANGE", "...") from SmartWikiService.AnswerAsync()
  at src/Xiigen.Platform.Marketplace/SmartWikiService.cs:120
  expected at least 1 indexed document, got 0
```

### 2. Find the immediate cause

What line directly produces this? Read it in context — a hidden assumption a few
methods above the failure site is often the real cause.

### 3. Ask: what called this?

```bash
grep -rn "SmartWikiService\|ISmartWikiService\|AnswerAsync" src/ tests/ --include="*.cs"
```

```
SmartWikiService.AnswerAsync() ← Xiigen.Platform.Api endpoint mapping
SmartWikiService.AnswerAsync() ← SmartWikiServiceTests.cs:67 (direct unit test)
```

### 4. Keep tracing up — what value was passed at each level?

```bash
# Where is the document collection populated before AnswerAsync reads it?
grep -rn "UpsertAsync\|IndexGraphAsync\|collection" src/Xiigen.Platform.Marketplace/ --include="*.cs"
```

Find the level where the empty/invalid value was first introduced (e.g. a test
that calls `AnswerAsync` before the index `UpsertAsync` ran).

### 5. Find the original trigger

```
Root cause: the test queried before any UpsertAsync wrote a document — the
  IDocumentStore.SearchAsync returns Success(empty list), and AnswerAsync treats
  empty as a Failure.

Fix location, either:
  A. AnswerAsync returns a well-typed Failure("NO_DOCUMENTS", ...) (fail fast) — and
  B. the test seeds the index with UpsertAsync before calling AnswerAsync.
```

---

## Section 2 — Adding Instrumentation

When manual tracing fails, add output before the failing operation. Prefer the
injected `ILogger<TService>` (available on every `MicroserviceBase`); use
`System.Diagnostics.Debug.WriteLine` for a throwaway probe.

```csharp
// Inside the service — log inputs to trace the empty-input caller
Logger.LogDebug("SmartWiki.AnswerAsync: tenant={Tenant} docCount={Count}",
    CurrentTenant.TenantId, search.Data?.Count ?? -1);

// Or a throwaway probe with a stack trace:
System.Diagnostics.Debug.WriteLine(
    $"DEBUG AnswerAsync: docCount={search.Data?.Count ?? -1}\n{new System.Diagnostics.StackTrace()}");
```

Run the test with detailed logging and filter:

```bash
dotnet test Xiigen.Platform.slnx --filter "FullyQualifiedName~SmartWiki" \
  --logger "console;verbosity=detailed" 2>&1 | grep "AnswerAsync"
```

The stack trace shows which test file and line triggered the empty input.

---

## Section 3 — Finding Which Test Causes Pollution

If a test creates side effects (a shared in-memory store, a static, a file) that
affect other tests, bisect:

```bash
# Run a single test class in isolation; if it passes alone but fails in the suite,
# another test polluted shared state.
dotnet test Xiigen.Platform.slnx --filter "FullyQualifiedName~SuspectClass" --logger "console;verbosity=detailed"
```

Common platform polluters: an `InMemoryDocumentStore`/scheduler shared across
tests instead of constructed per-test; an `AsyncLocal` tenant context left pushed
without disposing the `IDisposable` from `ITenantContextAccessor.Push`. Fix by
constructing fresh fabrics per test and disposing the push scope (`using var _ =
accessor.Push(ctx);`).

---

## Section 4 — Defense-in-Depth After the Root-Cause Fix

After fixing the source, add validation at every layer in the chain so the bug
cannot recur if one layer is bypassed:

```
Layer 1 (source):  the producer/seed validates its output before returning
Layer 2 (Api):     the Xiigen.Platform.Api endpoint validates the request binding
Layer 3 (service): the MicroserviceBase service returns DataProcessResult.Failure for the bad input
Layer 4 (fabric):  the fabric call's IsSuccess is checked before .Data is read
Layer 5 (test):    an xUnit [Fact] asserts the precondition and the Failure code
```

Each layer catches the bug independently. See `defense-in-depth-SKILL.md`.

---

## Section 5 — Real Tracing Commands

```bash
# All callers of a method
grep -rn "MethodName\|IFabric\.Method" src/ tests/ --include="*.cs"

# Where a field/variable is assigned
grep -rn "CollectionName\s*=" src/ tests/ --include="*.cs"

# All test files referencing a fabric
grep -rln "IDocumentStore\|IOutboxStore\|IIdempotencyStore" tests/ --include="*.cs"

# Everywhere a service/fabric is constructed
grep -rn "new .*Service(\|new InMemory" src/ tests/ --include="*.cs"

# DI registrations (who composes what)
grep -rn "AddSingleton\|AddScoped\|AddTransient\|AddXiigen" src/Xiigen.Platform.Api/ --include="*.cs"
```

---

## Anti-Patterns

1. **Fix at the symptom only** — the root cause remains; the bug recurs.
2. **Single-layer defense** — if the source fix is bypassed, nothing else catches it.
3. **Add a null check at the caller** — silences the error without understanding
   why null was passed.
4. **Trust the stack-trace line number** — the crash site is not the root cause.
5. **Leave a pushed tenant context undisposed** — pollutes every later test via `AsyncLocal`.

---

## Integration

- `systematic-debugging-SKILL.md` Phase 1.5 routes here for backward tracing.
- `defense-in-depth-SKILL.md` — the multi-layer validation added after the source fix.
- `debug-session-SKILL.md` — records each traced hypothesis and its status.
- `mental-debug-SKILL.md` — Rule 3 (mental execution) and Rule 6 (silent swallow) drive the trace.
- `data-flow-integrity-SKILL.md` — when the trace crosses a producer→consumer schema boundary.
