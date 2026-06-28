# Phase Retrospective (XIIGen Enterprise Platform)

## Overview

A set of phases that completes successfully does not automatically produce
calibrated predictions for the next set. Without a retrospective, cycle-count
predictions stay wrong, pattern transfer is unverified, and the next wave of work
starts with stale assumptions. This skill defines a **hard gate**: measure before
proceeding, not after.

This is the platform port of the `llm_mvp_core` skill of the same name. It lifts
the **universal 4-dimension retrospective gate** (Cycle-Count Accuracy / MAE,
Pattern Transfer, Behavioral Test Coverage, Gate-B Adherence) into a reusable
file and binds it to this repository's stack: **.NET 10 ASP.NET Core Web API**
(`net10.0`, SDK `10.0.300`), **xUnit** tests under `tests/`,
`Xiigen.Platform.slnx`, and the **v3.3 Enterprise Adaptation** governance triple
(`BEACON` / `STATE-v3.3.json` / `COMPLETION-MATRIX-v3.3.md`).

> **What does NOT port (stays in `llm_mvp_core`, G12 / R5 / R6).** Core
> Dimension 2 counts `DomainModelRegistry` entries with status `PROVEN` from
> `ContinuousLearningPipeline`. That trainable-model registry is **not** part of
> this backend. Here Dimension 2 is kept as a **general pattern-transfer
> discipline** over domain capabilities (reusable services, fabric adapters,
> session patterns promoted from one phase to the next), recorded in the
> retrospective and reconciled against `COMPLETION-MATRIX-v3.3.md` — not against
> a model registry.

UI / Playwright / `ng build` parts of the core skill are **`na`** here — the
platform has no frontend. Dimension 4 Gate-B is server-side `dotnet build` /
`dotnet test` only.

## When to Invoke

- After the last phase in a sequential batch completes (Gate-B passes for all
  phases in the batch).
- Before the next batch of phases begins.
- This is a hard gate: the next batch does not start until the retrospective is
  recorded and `Next Batch Clear to Proceed: YES`.

```text
Sequential batch:  SESSION-01 -> SESSION-02 -> SESSION-03
                   v
                   Retrospective runs here
                   v
Next batch begins: SESSION-04, SESSION-05, ...
```

## Section 1 — Four Measurement Dimensions

### Dimension 1 — Cycle Count Accuracy (MAE)

For each phase in the completed batch, compare predicted cycles to actual cycles
(actual cycles come from `quality-improvement-cycle-SKILL.md` Cycle Logs):

```text
SESSION-01: predicted=2 cycles, actual=3 cycles
SESSION-02: predicted=2 cycles, actual=1 cycle
SESSION-03: predicted=1 cycle, actual=1 cycle
```

**MAE (Mean Absolute Error):**

```text
errors = [ |predicted - actual| ] for each phase
MAE    = sum(errors) / count(phases)
```

**Threshold:** MAE > 1.0 -> recalibrate cycle predictions before the next batch.

Recalibration means identifying which capability classes consistently need more
cycles than predicted. Common causes on this platform:
- Fabric capabilities that need more infrastructure than anticipated
  (idempotency + tenant-scope + outbox interacting).
- xUnit categories that surface more edge cases than predicted (e.g.
  `WebApplicationFactory` integration paths).
- Tenant-context / `AsyncLocal` wiring that adds unexpected setup work.

### Dimension 2 — Pattern Transfer Rate (general discipline)

Count how many proven, reusable patterns were promoted across phases since the
last retrospective: a reusable service (`shared-service-design-SKILL.md`), a
fabric-adapter pattern, a session/test pattern reused in a later phase.

```text
Promoted patterns since last retrospective: 2
  - DataProcessResult validation-guard pattern reused in SESSION-02 and SESSION-03
  - ITenantContextAccessor scope-filter pattern lifted into a shared helper
```

**Expected rate:** ~1–2 promoted patterns per 3 completed phases.

**If count < expected:** improvement cycles likely stopped at "tests pass"
without lifting the reusable pattern. Verify the pattern is recorded against
`COMPLETION-MATRIX-v3.3.md` so the next batch can reuse it.

> Reconcile against the matrix, not a model registry — the
> `DomainModelRegistry` / `PROVEN` source is `llm_mvp_core` (G12).

### Dimension 3 — Behavioral Test Coverage

For each phase completed, check that all declared xUnit test categories have at
least one passing test (categories from `e2e-test-matrix-SKILL.md` /
`integration-testing-SKILL.md`):

```bash
dotnet test --filter "Category=HappyPath"    2>&1 | tail -3   # -> N passed
dotnet test --filter "Category=Idempotency"  2>&1 | tail -3   # -> N passed
dotnet test --filter "Category=TenantScope"  2>&1 | tail -3   # -> N passed
```

**Threshold:** any N/A category without justification in the session/phase plan
is a gap. Any declared category with 0 passing tests is a completion failure for
that phase. Run only the categories the active `SESSION-XX-*.md` allows.

### Dimension 4 — Gate-B Adherence (binary)

For each phase, verify the Gate-B condition was met **as stated**, not
approximated. Gate-B is binary.

```text
SESSION-01 Gate-B: "dotnet test --filter 'Scheduler' -> 12 passed, 0 failed"
  Actual: 12 passed, 0 failed -> ADHERED
SESSION-02 Gate-B: "dotnet build Xiigen.Platform.slnx -> 0 Error(s)"
  Actual: 2 warnings, 0 errors -> ADHERED (warnings are not errors)
SESSION-03 Gate-B: "dotnet test --filter 'Outbox' -> 8 passed"
  Actual: 7 passed -> NOT_ADHERED
```

**Threshold:** any NOT_ADHERED gate means the phase is not complete. The
retrospective notes it and the phase is re-opened before the next batch proceeds.
7 passed when 8 was declared is not approximate success — it is a failed gate.

## Section 2 — Retrospective Record Format

```markdown
## Phase Retrospective — [SessionId] — Phases [N-M]

Covered phases: [SESSION-01, SESSION-02, SESSION-03]
Date: [date]

### Dimension 1 — Cycle Count Accuracy
| Phase | Predicted | Actual | Error |
|---|---|---|---|
| SESSION-01 | 2 | 3 | 1 |
| SESSION-02 | 2 | 1 | 1 |
| SESSION-03 | 1 | 1 | 0 |

MAE: 0.67 — within threshold (<= 1.0)
Action: no recalibration needed

### Dimension 2 — Pattern Transfer Rate
Promoted patterns since last retrospective: 2 (expected 1–2) — PASS
  - [pattern] reused in [phases]

### Dimension 3 — Behavioral Test Coverage
| Category | Tests | Status |
|---|---|---|
| Happy Path | 8 passed | PASS |
| Idempotency | 3 passed | PASS |
| Tenant Scope | 2 passed | PASS |
| Compensation | N/A — read-only capability | N/A (justified) |

### Dimension 4 — Gate-B Adherence
| Phase | Gate-B | Actual | Adhered |
|---|---|---|---|
| SESSION-01 | 12 passed | 12 passed | YES |
| SESSION-02 | 0 errors | 0 errors | YES |
| SESSION-03 | 8 passed | 7 passed | NO |

Gate-B NOT_ADHERED: SESSION-03 — must be resolved before next batch

### Calibration Adjustments
- Dimension 1: no change
- Dimension 4: SESSION-03 gate not met — reopen SESSION-03, resolve 1 failing test

### Next Batch Clear to Proceed: NO
Blocking: SESSION-03 Gate-B not met
```

## Section 3 — Next Batch Gate

The next batch does not start until:

1. The retrospective record exists (this document, completed).
2. All Gate-B conditions are ADHERED (Dimension 4).
3. MAE <= 1.0 OR recalibration adjustments recorded (Dimension 1).
4. No pattern-transfer blocking issue (Dimension 2).

If `Next Batch Clear to Proceed: NO`:
- Fix the blocking items.
- Re-run the affected verification.
- Update the retrospective record.
- Flip the flag to YES.
- Do not proceed until the flag is YES.

This gate is consistent with AGENTS.md: a NOT_ADHERED gate is a slice handed to
`REVIEW_INTERNAL` -> `ARCHITECT_INTERNAL_REPAIR`, not a final report. The
executor may not declare the batch done while a Gate-B row is NOT_ADHERED.

## Anti-Patterns

1. **"The phases all passed, skip the retrospective"** — passing phases do not
   produce calibration data automatically. The retrospective is the calibration
   step.

2. **"Gate-B was approximately met"** — Gate-B is binary. 7 passed when 8 was
   declared is a failed gate. Re-open the phase.

3. **"We'll do the retrospective at the end of the project"** — retrospectives
   are per-batch, not per-project. Waiting until the end means the next batch
   runs on stale calibration from the very first batch.

4. **"MAE of 1.5 is close enough"** — MAE > 1.0 means predictions are
   consistently off by more than one cycle. Recalibrate now; otherwise the next
   batch's timeline is already wrong before it starts.

## Integration

- `quality-improvement-cycle-SKILL.md` Cycle Logs produce the actual cycle counts
  for Dimension 1.
- `e2e-test-matrix-SKILL.md` / `integration-testing-SKILL.md` declare the xUnit
  categories verified in Dimension 3.
- `verification-before-completion-SKILL.md` supplies the binary Gate-B commands
  (`dotnet build`: `0 Error(s)`; `dotnet test`: `Failed: 0`) checked in
  Dimension 4.
- `COMPLETION-MATRIX-v3.3.md` is the data source/sink for Dimension 2 pattern
  transfer (in place of the core `DomainModelRegistry`).
