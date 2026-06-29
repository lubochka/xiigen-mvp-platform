# Improvement Measurement Protocol (XIIGen Enterprise Platform)

## Overview

An internal metric that improved is not an improvement until a user-facing or
behavioral change can be described. Test counts can rise while the capability
regresses. `dotnet build` passes while the algorithm produces wrong results.

**Observable behavioral delta is required before any improvement claim is permitted.**

This is the platform port of the `llm_mvp_core` skill of the same name. It lifts
the universal two-layer honesty axis into a reusable file and binds it to this
repository's stack: **.NET 10 ASP.NET Core Web API** (`net10.0`, SDK `10.0.300`),
**xUnit** tests under `tests/`, the domain result wrapper `DataProcessResult<T>`
(in `Xiigen.Kernel`), and the binding **v3.3 Enterprise Adaptation** governance
triple (`BEACON` / `STATE-v3.3.json` / `COMPLETION-MATRIX-v3.3.md`).

> **Result-wrapper mapping.** The core skill is written against
> `OperationResult<T>`. This repository's equivalent is `DataProcessResult<T>`
> (in `src/Xiigen.Kernel/DataProcessResult.cs`): success is
> `result.IsSuccess == true` with `result.Data`; failure carries
> `result.Error.Code` / `result.Error.Message`. Read `IsSuccess` — never infer
> success from the absence of an exception.

UI / extension / dashboard / Dual-Path-Host parts of the core skill are **`na`**
for this repository: the platform has no frontend (no `angular.json`,
no `package.json`, only `src/` Web API + `tests/` xUnit). Only runtime / API /
`dotnet` evidence applies here. There is no Playwright / `ng build` layer.

## Origin

Extracted from sessions where "+N tests" was reported as a capability
improvement. The tests counted what they counted — but whether the algorithm
produced better results for any input was never established. Internal metric
improved. Whether the system became more capable was never verified.

## When to Invoke

- Any session claiming a capability improvement (a fabric provider, an API
  endpoint, a scheduler/outbox/idempotency behavior, a tenant-scope rule).
- Any STOP that includes an improvement percentage or before/after metric.
- Before any claim of "X is now complete" or "X improved by N%".
- Before updating `COMPLETION-MATRIX-v3.3.md` with a capability-improvement row.

## Section 1 — The Two-Layer Protocol

For any capability change, two layers must be present before an improvement
claim is valid.

### Layer 1 — Internal metrics (necessary but not sufficient)

Internal metrics confirm structural correctness. Required. Not sufficient alone.

| Metric type | What it confirms | What it does NOT confirm |
|---|---|---|
| `dotnet test` count increased | More scenarios covered | Endpoint/fabric produces better results |
| `dotnet build` errors = 0 | Code compiles | Correct behavior |
| Code coverage % | Tests reach the code | Tests assert the right thing |
| New interface method added (e.g. on `ISchedulerService`) | Contract extended | Callers benefit from the extension |

Layer 1 format (required in every STOP including capability claims):

```text
LAYER 1 — Internal metrics:
  dotnet test count: [before] -> [after] ([delta])
  dotnet build errors: 0
  New tests: [list test names]
  Command run: dotnet test Xiigen.Platform.slnx (or the session-allowed filter)
```

> **Scope of commands.** This skill does not authorize a broad
> `dotnet build` / `dotnet test`. During governance acceptance, run only the
> commands the active `SESSION-XX-*.md` file explicitly allows (CLAUDE.md
> "Governance Startup"). Broad build/test open only when the relevant session
> opens that gate.

### Layer 2 — Observable behavioral delta (required for improvement claim)

For every changed component, one concrete statement describing what the system
does differently for a specific input. On this backend that is a request ->
response delta on an API endpoint, or an input -> `DataProcessResult` delta on a
fabric/service method.

Layer 2 format (required for any improvement claim):

```text
LAYER 2 — Observable behavioral delta:
  [Component]: [what input now produces what different output]
  ISchedulerService.ScheduleDelayedAsync("send-reminder", 5m, payload, key)
    now returns DataProcessResult<ScheduledJobResult>.IsSuccess == true with
    Data.DueAt = now+5m (was: VALIDATION_FAILED because key was unchecked).
  IIdempotencyStore.TryReserveAsync(key, ttl) second call now returns
    Data.Status == "DUPLICATE" with FromCache == true (previously created a
    second reservation).
```

**"No sentence = no improvement claim"** — if the session cannot state what the
system does differently for a concrete input/request, the improvement has not
been measured.

## Section 2 — Claim Validity Rules

### Component-level claim: "ISchedulerService capability improved"

Requires:
- Layer 1: `dotnet test` count delta with passing tests listed.
- Layer 2: one sentence per changed method describing input -> `DataProcessResult`
  change (read `IsSuccess` / `Error.Code` / `Data`).
- Gate-B evidence from `verification-before-completion-SKILL.md`
  (`dotnet build`: `0 Error(s)`; `dotnet test`: `Failed: 0`).

Example of a valid component-level claim:

```text
ISchedulerService idempotency improved.
LAYER 1: 4 -> 9 tests (+5); all 9 pass; Failed: 0
  (dotnet test --filter "FullyQualifiedName~Scheduler").
LAYER 2: ScheduleDelayedAsync(action, delay, payload, key) called twice with the
  same key now returns the same ScheduledJobResult with FromCache == true on the
  second call (DataProcessResult<ScheduledJobResult>.IsSuccess == true), instead
  of a second job. Verified in SchedulerIdempotencyTests.
```

### Pipeline-level claim: "Outbox dispatch pipeline capability improved"

Requires:
- Layer 1: `dotnet test` count delta across all pipeline components
  (`IOutboxStore.AppendAsync` -> dispatch -> `CloudEventEnvelope` emission).
- Layer 2: one sentence per pipeline stage describing integration behavior,
  ideally through a `WebApplicationFactory` integration test (request enters,
  outbox row appended, event observed).
- Coverage completeness: all N pipeline stages have test evidence.

## Section 3 — The STOP Gate

Before any STOP that includes an improvement percentage or before/after metric:

```text
IMPROVEMENT CLAIM GATE (improvement-measurement-SKILL):

Claim: "[stated improvement]"

Layer 1 check:
  [ ] Internal metrics present (dotnet test counts, dotnet build status)
  [ ] Layer 1 format block present in this response
  [ ] Command was one the active SESSION-XX file allows

Layer 2 check:
  [ ] Observable delta present for each changed component
  [ ] At least one input/request -> output/DataProcessResult example per component
  [ ] "Behavioral delta not yet verified" stated explicitly if not established

Coverage check:
  [ ] Component-level: all affected tests listed
  [ ] Pipeline-level: all stages have test evidence
  [ ] No validation_only / static / contract evidence used for a runtime claim
      (Runtime Evidence Firewall, see Section 5)

Verdict: VALID | INVALID — [reason if invalid]

If INVALID: revise the claim before STOP fires.
  Option A: Provide the behavioral observable (run a specific test or probe that
            demonstrates the behavior, quote the assertion / DataProcessResult).
  Option B: Downgrade the claim to what Layer 1 actually establishes:
            "Test count improved — behavioral improvement not yet verified".
```

## Section 4 — "No Behavioral Test" Declaration

If the session added tests but none verify a behavioral observable:

```text
LAYER 2 STATUS: UNIT TESTS ONLY.
Test count improved. Behavioral improvement not yet verified.
Claim revised to: "[test count] tests added.
  Behavioral delta: not established — [what specific integration test / probe
  would verify it, e.g. a WebApplicationFactory call asserting the response]."
```

This is not a failure. It is an honest claim. A session that correctly states
"tests added, behavior not yet verified" produces more useful output than one
that claims "+15 tests = capability improved" without establishing observable
delta. This mirrors the platform Runtime Evidence Firewall: contract/static
evidence proves only contract/static readiness.

## Section 5 — Anti-Patterns

1. **"Test count rose — capability improved"** — tests measure coverage, not
   correctness of results. Establish the behavioral delta on the endpoint/fabric.

2. **"`dotnet build` passes — feature is complete"** — build confirms
   compilation, not correct behavior. State what the component does for a
   specific input.

3. **"It's obvious the scheduler got better — I added idempotency"** —
   "obvious" is prediction. State the concrete delta:
   "ScheduleDelayedAsync called twice with the same key now returns the cached
   job (FromCache == true) instead of a duplicate."

4. **"The test asserts the behavior"** — quote the assertion / the resulting
   `DataProcessResult`. Do not reference the test by name only. The claim must
   stand without reading the test file.

5. **"`passed: true` in a JSON artifact = behavioral delta"** — a static or
   `validation_only` artifact proves only artifact creation. Runtime claims need
   the endpoint/fabric path actually invoked and `IsSuccess` observed.

## Integration

- `verification-before-completion-SKILL.md` provides the platform Gate-B command
  table and the Runtime Evidence Firewall (no `validation_only` / `static` /
  `contract` / `cpu_ms: UNKNOWN` evidence for a runtime claim). The behavioral
  layer here is what that firewall demands.
- `quality-improvement-cycle-SKILL.md` supplies the before-and-after score
  values (cycle log) that this skill's improvement claim must cite.
- `phase-retrospective-SKILL.md` Dimension 3 (Behavioral Test Coverage) reuses
  the same "declared category has at least one passing test" rule.

## Why This Matters

A false "improved by N%" makes the platform plan future work from confidence
instead of evidence. Per AGENTS.md, the executor may not self-finish a claim;
the observable behavioral delta is the evidence `REVIEW_INTERNAL` checks before
any architect verdict can authorize a completion or improvement claim.
