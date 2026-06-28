# Claim-as-Hypothesis (XIIGen Enterprise Platform)

## Core Problem

A user's assertion about what exists in the codebase ("the design is done," "X
already works," "that interface is implemented") is a hypothesis, not a fact.
Planning built on unverified assertions fails when execution discovers the
assertion was wrong. This skill captures, verifies, and gates on every such
assertion before planning proceeds.

Platform port of the `llm_mvp_core` skill. Stack-bound to **.NET 10 ASP.NET Core
Web API**, **xUnit** (`tests/`), `DataProcessResult<T>`, and the v3.3 governance
triple.

## When to Invoke

- At session start — capture every user assertion about existing state.
- When a user makes a new claim during the session (trigger phrases below).
- Before any plan step that depends on "X exists / works / is done".
- At every STOP — verify no PENDING BLOCKING claims remain.

Zero pending BLOCKING claims at STOP = zero "built on sand" execution failures.

## Section 1 — Trigger Phrases

| Pattern | Example | Why it matters |
|---|---|---|
| Existence | "that interface is already in the project" | May be a stub that throws `NotImplementedException` |
| Completion | "the design is done" | File may exist with empty fields |
| Operational | "that works" / "already works" | Test may be missing or disabled |
| Implementation | "the service is implemented" | Class may exist but methods return canned `DataProcessResult.Success()` |
| Wiring | "X is wired to Y" | Constructor injection may not be registered in DI |
| Readiness | "it's ready" | Undefined — ready means different things |

Phrases that do NOT produce claim entries: goals ("I want X"), preferences
("I prefer Y"), past decisions ("we decided to use X").

## Section 2 — Claim Verification Record Schema

```json
{
  "claims": [
    {
      "id": "C1",
      "statement": "[verbatim user assertion — exact quote]",
      "capturedAt": "session start / turn N",
      "status": "PENDING",
      "importance": "BLOCKING | INFORMATIONAL",
      "verificationAction": "[specific grep/Read/dotnet command]",
      "verdict": null,
      "evidence": null
    }
  ]
}
```

Status: **PENDING** (queued) / **VERIFIED** / **DISCONFIRMED** / **PARTIAL** /
**DEFERRED** (only the architect/Luba may defer; the session cannot self-defer).
Importance: **BLOCKING** (plan changes if wrong — cannot STOP without resolution)
or **INFORMATIONAL**.

## Section 3 — Verification Protocol (.NET 10)

Verification is a **read action**, not a reasoning action.

| Claim shape | Verification action |
|---|---|
| "X file exists" | `Test-Path` / Glob — record presence/absence |
| "X contains Y" | `grep -n [pattern] [path]` — hit count + first 3 matches |
| "interface is implemented" | `grep -n "NotImplementedException\|NotSupportedException" [file]` — should be 0 on the done path |
| "service is wired" | `grep -rn "[InterfaceName]" src/...` near `AddSingleton/AddScoped/AddTransient` — should be >=1 |
| "endpoint exists" | `grep -rn "Map(Get\|Post\|Put\|Delete)\|\[Http" src/Xiigen.Platform.Api/` |
| "operation succeeds" | the method reads its inputs and returns `DataProcessResult.Success(...)` with real data — not a canned success |
| "tests pass" | `dotnet test --filter "FullyQualifiedName~[X]" 2>&1 \| tail -5` — record actual count |
| "method exists" | `grep -n "[MethodName]" [file]` — presence/absence |

What does NOT count as verification: "I know X likely exists", "a prior session
mentioned X", "the path suggests X", or the user confirming their own claim.

## Section 4 — Gate Enforcement

**Before first synthesis output:** every assertion captured PENDING; BLOCKING
claims have a queued verification action; any synthesis depending on a claim
references it by id (C1, C2, ...).

**Before STOP:** no BLOCKING claim remains PENDING; every plan assumption has the
verdict integrated; DEFERRED claims carry a carry-forward flag.

## Section 5 — Worked Examples

### PARTIAL — "the scheduler is done"
```
Test-Path src/Xiigen.Fabrics.Scheduler/Scheduler.cs        -> exists
grep -n "NotImplementedException" .../Scheduler.cs          -> 0
grep -rn "ISchedulerService" src/Xiigen.Platform.Api/       -> 0 (not wired into API)
```
Verdict: PARTIAL — implementation exists, but not registered/exposed via the API.
Consequence: plan must add the DI + endpoint wiring task.

### DISCONFIRMED — "IOutboxStore is wired"
```
grep -rn "IOutboxStore" src/Xiigen.Platform.Api/Program.cs  -> 0
```
Verdict: DISCONFIRMED — registration not present. "Wiring done" becomes a task.

### VERIFIED — "ITenantContextAccessor exists"
```
grep -rn "interface ITenantContextAccessor" src/  -> src/Xiigen.Kernel/TenantContext.cs:20
```
Verdict: VERIFIED.

## Anti-Patterns

1. **"The user confirmed it"** — confirming a claim doubles it; it does not verify.
2. **"It's obviously true"** — obviousness is memory; a stub can "obviously exist".
3. **"Skip verification to save time"** — a 30-second grep prevents a multi-round
   plan failure.
4. **"PARTIAL is close enough to VERIFIED"** — PARTIAL surfaces the gap that fails
   in execution.
5. **"Verify once, assume forever"** — codebase state changes; re-verify each session.
