# Quality Improvement Cycle (XIIGen Enterprise Platform)

## Overview

A capability that scores below its threshold on first evaluation must be improved
iteratively. Repeating the same evaluation without changing anything produces the
same score. This skill defines the exact procedure: identify what failed, make a
targeted change, re-evaluate, and either accept or escalate — with a hard limit
of 3 cycles and a recorded Cycle Log.

This is the platform port of the `llm_mvp_core` skill of the same name. It lifts
the **universal score-band + 9-step + max-3-cycles + Cycle-Log** discipline into
a reusable file and binds it to this repository's stack: **.NET 10 ASP.NET Core
Web API** (`net10.0`, SDK `10.0.300`), **xUnit** tests under `tests/`,
`Xiigen.Platform.slnx`, and the result wrapper `DataProcessResult<T>` (in
`Xiigen.Kernel`).

> **What does NOT port (stays in `llm_mvp_core`, G12 / R5 / R6).** The core skill
> ties "evaluation" to `ContinuousLearningPipeline`, `IncrementalTrainer`,
> `DpoTrainingPair`, and DPO promotion of common models. That trainable-model
> machinery is **not** part of this backend. Here "evaluation" is re-interpreted
> as the **Gate-B result**: `dotnet test` pass/fail on the capability's xUnit
> suite (and the behavioral assertions inside it), and "fix" is a change to
> `src/`. Do not introduce `DpoTrainingPair` / DPO training data in this repo.

> **Result-wrapper mapping.** Core `OperationResult<T>` -> platform
> `DataProcessResult<T>`: success `IsSuccess == true` + `Data`; failure
> `Error.Code` / `Error.Message`. Assert the concrete `Data` value, not only
> `IsSuccess`.

UI / Playwright / `ng test` parts of the core skill are **`na`** here — the
platform has no frontend. Evaluation is server-side `dotnet test` only.

## When to Invoke

- When a capability's xUnit suite scores below its declared Gate-B / promotion
  threshold on first run.
- When `dotnet test` passes structurally but a behavioral assertion does not meet
  the expected standard (a wrong `DataProcessResult.Data`, a wrong `Error.Code`,
  a tenant-scope leak, a non-idempotent second call).
- When a generated/changed `src/` output is incorrect and a targeted fix +
  re-evaluation is needed.

Do NOT invoke when the score is already at or above threshold.
Do NOT invoke when `systematic-debugging-SKILL.md` has identified an
architectural root cause — architectural fixes come before iteration cycles.

## Section 1 — Score Band Protocol

Before starting a cycle, classify the score into a band. "Score" here is a
fraction of the capability's declared threshold (e.g. required passing assertions
met / total).

| Score | Band | Action |
|---|---|---|
| ≥ promotion threshold | PASS | Accept / hand to `REVIEW_INTERNAL`. No cycle needed. |
| 70–84% of threshold | NEAR-PASS | Run one targeted improvement. Re-evaluate. |
| 60–69% of threshold | PATCH | Run up to 2 targeted improvements. Escalate after 2. |
| < 60% of threshold | STOP | Do NOT run improvement cycles. Escalate. Root cause is architectural. |

Record `cycle_[N]_score = [value]` before each cycle.

The maximum cycle count per capability is **3**. After 3 cycles without reaching
the threshold: escalate. Three cycles without improvement = the problem is not in
the specific change — it is in the capability design.

## Section 2 — The 9-Step Cycle

### Step 1 — Confirm the Score Band

Read the evaluation output. Extract the score value. Classify into the band table
above. If STOP or PASS, do not continue.

### Step 2 — Read Failing Assertions from `dotnet test` Output

```text
[FAIL] SchedulerTests.ScheduleDelayed_IsIdempotent
  Expected: result.IsSuccess == true && result.FromCache == true
  Actual:   result.IsSuccess == true && result.FromCache == false  (a second job was created)

[FAIL] TenantScopeTests.Search_RespectsTenant
  Actual:   returned a DocumentRecord from another tenant (scope leak)
```

Extract: failing assertion IDs, the exact failure messages, and which `src/`
component is failing.

### Step 3 — Classify the Failure Type

| Failure type | Symptom | Fix direction |
|---|---|---|
| Wrong behavior in capability logic | Wrong `Data` / wrong `Error.Code` repeatably | Fix the `src/` method; add the missing branch |
| Missing guard / validation | Invalid input not rejected with `Failure(code,…)` | Add validation returning `DataProcessResult.Failure` |
| Wrong configuration value | Output has wrong constant, not wrong structure | Fix the config (`appsettings.json` / a `const`), not logic |
| Tenant-scope / idempotency contract gap | Cross-tenant leak, non-idempotent retry | Bind to the fabric contract (`ITenantContextAccessor`, `IIdempotencyStore`) |
| Architectural issue | Same score after 2 cycles | Escalate — not a targeted-fix issue |

### Step 4 — Read Current Capability State

```bash
# Re-run only this capability's suite to confirm which assertions pass/fail
dotnet test --filter "FullyQualifiedName~SchedulerTests" --no-build 2>&1 | tail -20
# Inspect the source under change
grep -n "DataProcessResult\|Failure(\|FromCache" src/Xiigen.Fabrics.Scheduler/Scheduler.cs
```

Run only commands the active `SESSION-XX-*.md` allows.

### Step 5 — Generate the Fix

For **wrong behavior in capability logic** (e.g. non-idempotent path):

```csharp
// Before returning a new job, honor the idempotency key so a repeated call
// returns the cached result instead of creating a duplicate.
if (_jobs.TryGetValue(idempotencyKey, out var existing))
{
    // Same key seen before -> return the existing job, flagged from cache.
    return Task.FromResult(DataProcessResult<ScheduledJobResult>.Success(existing, fromCache: true));
}
```

For a **missing guard** (invalid input must fail, not throw):

```csharp
// Reject empty input with a typed failure rather than throwing for business logic.
if (string.IsNullOrWhiteSpace(action) || string.IsNullOrWhiteSpace(idempotencyKey))
{
    return Task.FromResult(DataProcessResult<ScheduledJobResult>.Failure(
        "VALIDATION_FAILED",
        "Action and idempotency key are required"));
}
```

For a **wrong configuration value**:
- Do NOT change capability logic.
- Fix the `appsettings.json` value or the `const`.
- Re-evaluate — if it still fails, the issue is in capability logic, not config.

### Step 6 — Apply the Fix

Write the fix to the `src/` file. Platform DNA required:
- Return `DataProcessResult<T>` — never throw for business logic, never return
  `null`, never return bare `bool` without context.
- Resolve dependencies through interfaces
  (`ISchedulerService` / `IOutboxStore` / `IIdempotencyStore` / `IDocumentStore`
  / `ITenantContextAccessor`), not concrete types.
- Comment each new line with what it contributes in context.

### Step 7 — Record What Changed (Version / Cycle)

Do not re-evaluate without recording what changed — same fix + same evaluation =
same result.

```text
cycle_[N]_change: "[short description of the src/ change]"
cycle_[N]_files:  "[file paths touched]"
```

### Step 8 — Re-Run Evaluation

```bash
dotnet test --filter "FullyQualifiedName~SchedulerTests" 2>&1 | tail -10
# -> record new pass/fail counts
```

Read the output. Apply the score-band classification again.

### Step 9 — Compare and Decide

| Result | Action |
|---|---|
| Score ≥ threshold | PASS — hand to `REVIEW_INTERNAL`. Record the behavioral delta (improvement-measurement-SKILL). |
| Score improved but below threshold, cycle count < 3 | Record the cycle. Return to Step 2. |
| Score same (±2%) | PLATEAU — not a targeted-fix issue. Escalate. |
| Score worse | REGRESSION — escalate immediately. Note the change that may have caused it. |
| Cycle count = 3, still below threshold | ESCALATE — 3 cycles is the maximum. |

## Section 3 — Cycle Log

Maintain per capability:

```text
[ISchedulerService idempotency] Cycle Log:
  Cycle 1: score=68%, failing=[ScheduleDelayed_IsIdempotent], fix=honor idempotency key in ScheduleDelayedAsync
  Cycle 2: score=82%, failing=[ListAsync_ReturnsTenantScoped], fix=filter ListAsync by ITenantContextAccessor.Current
  Cycle 3: score=100%, failing=[], PASS -> handed to REVIEW_INTERNAL
```

## Section 4 — Escalation Report Format

When escalation is required (score < 60%, plateau, or 3 cycles without
promotion):

```text
ESCALATION REQUIRED
Capability:       [name, e.g. ISchedulerService idempotency]
Final score:      [value] (cycle [N])
Lowest area:      [which assertion category has the lowest pass rate]
Still failing:    [assertion id] — [N] cycles did not fix this
Root cause:       [architectural / contract design / interface]
Fixes tried:      [list of src/ changes made]
Last test output: [attach dotnet test tail output]
Recommended:      [redesign interface / change fabric contract / change architecture]
Hand-off:         ARCHITECT_INTERNAL_REPAIR (per AGENTS.md role loop)
```

## Anti-Patterns

1. **"Run another cycle without changing anything"** — same inputs produce same
   outputs. A cycle without a `src/` change is waste.

2. **"Fix the test output / the expectation"** — fix the `src/` capability, not
   the assertion. Loosening the assertion makes the next evaluation lie.

3. **"Continue past 3 cycles"** — 3 cycles without promotion means the design is
   wrong, not the specific change. Escalate to `ARCHITECT_INTERNAL_REPAIR`.

4. **"Score below 60% -> still run cycles"** — below 60% is architectural. No
   number of targeted fixes will rescue a broken capability design.

5. **"Add `DpoTrainingPair` / training data to fix it"** — that machinery lives
   in `llm_mvp_core` (G12). In this backend the fix is `src/` code + xUnit.

## Integration

- `improvement-measurement-SKILL.md` requires the before-and-after score values
  from this skill's Cycle Log, plus a Layer-2 behavioral delta on PASS.
- `systematic-debugging-SKILL.md` handles the escalation path (architectural
  root cause) when < 60% or after 3 cycles.
- `verification-before-completion-SKILL.md` supplies the Gate-B commands
  (`dotnet build` / `dotnet test`) used as the evaluation in each cycle.
- `phase-retrospective-SKILL.md` Dimension 1 (Cycle Count Accuracy) consumes the
  cycle counts this skill records.
