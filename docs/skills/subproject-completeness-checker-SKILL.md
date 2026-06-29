# Subproject Completeness Checker

> Ported from the `llm_mvp_core` completeness-checker standard, adapted for the
> XIIGen Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build Xiigen.Platform.slnx` / `dotnet test`. Gate B is
> `dotnet test --filter "FullyQualifiedName~..." → N passed, 0 failed`. T4 = every
> new public service method returns `DataProcessResult<T>` (never core
> `OperationResult<T>`). T5 = registration in `Xiigen.Platform.Api/Program.cs`. T9
> DNA is taken from `CLAUDE.md` + `DNA-DOTNET-TRANSLATION.md`. **UI checks are na
> — platform has no frontend.**

## Core Problem

A project plan that passes internal consistency checks can still fail to deliver
the stated goals — if those goals were never mapped to specific phases. And a plan
that maps goals to phases can still fail if the design artifacts those phases
depend on are empty stubs. This skill adds two structural pre-gates that run
before all other checks: goal-to-phase mapping and design-artifact population.

---

## When to Invoke

- "Is this project/capability plan ready for implementation?"
- "What's missing from this phase plan?"
- After any phase plan is produced or updated.
- Before starting Phase 1 of any multi-phase plan.
- When `plan-self-validation-SKILL.md` returns incomplete results.

One completeness run before implementation = zero "phases complete but goals not
delivered" failures.

---

## Section 1 — The Pre-Gate Checks (Run First)

These two checks block everything else. A plan that fails either is not ready
regardless of how well the remaining checks pass.

### Pre-Gate 1 — Goal-to-Phase Mapping

**What it verifies:** every goal element stated by Luba (or the active v3.3
session/contract) has at least one phase assigned and a verification step.

**Protocol:**
1. Load the verbatim goal statement from the session's goal context / v3.3 contract.
2. Decompose into discrete goal elements.
3. For each goal element, identify which phase(s) deliver it.
4. Verify each mapped goal has a concrete verification step (`dotnet test` filter, build count, or observable output).

**Output:**

```markdown
| Goal Element | Phase(s) | Verification Step | Verdict |
|---|---|---|---|
| GE-1: [text] | Phase 2, Phase 3 | dotnet test --filter → 5 passed | APPROVED |
| GE-2: [text] | — | — | BLOCK — goal not covered |
```

**FAIL** = any goal element has zero phases or no verification step. Pre-Gate 1
failure blocks the plan regardless of all other checks.

**Resolution:** add phases that deliver the uncovered goal element, OR explicitly
defer with Luba's approval (record as DEFERRED with reason in the session).

### Pre-Gate 2 — Design Artifact Populated

**What it verifies:** every artifact referenced in the plan passes
`design-artifact-completeness-SKILL.md` Check 1 (exists) and Check 2 (populated).

**Protocol:**
- For each fabric contract referenced: Check 1 + Check 2.
- For each test class referenced: Check 2 (≥1 `[Fact]`).
- For each DI registration referenced: Check 5 (reachable in Program.cs).

**FAIL** = any referenced artifact is MISSING or STUB. Resolution: implement the
stub first, OR scope the plan around the gap with explicit acknowledgment.

---

## Section 2 — Technical Completeness Checks (Run After Pre-Gates Pass)

### T1 — Phase structure declared
Each phase has: goal, output files, Gate B condition, and prerequisite list.

### T2 — Gate B is measurable
Every phase has exactly one Gate B expressed as:
- `dotnet test --filter "[filter]" → [N] passed, 0 failed`, or
- `dotnet build → 0 Error(s), 0 Warning(s)`, or
- a specific count or assertion.

"Works correctly" is not a Gate B. "Passes" without a filter is not a Gate B.

### T3 — Prerequisites verified
Every phase depending on another phase's output lists it as a prerequisite. No
phase assumes an artifact that does not exist.

### T4 — DataProcessResult<T> usage declared
Any new public service method returns `DataProcessResult<T>` / `DataProcessResult`.
Phases that add throw-based business logic fail T4 (infrastructure exceptions
caught at the boundary and wrapped are fine).

### T5 — Fabric/DI registration planned
Any new class that implements a contract has a registration step in its phase plan
(or the DI registration phase in `Xiigen.Platform.Api/Program.cs` is explicitly
called out).

### T6 — Test categories declared
For any capability with writes, timers, idempotency, or multi-step operations, the
plan names which `e2e-test-matrix-SKILL.md` / `integration-testing-SKILL.md`
categories apply.

### T7 — Time-based tests use injectable clock
Any capability with a TTL, timer, or scheduled operation declares an injected time
provider (never `Thread.Sleep`). The scheduler models due time via
`DateTimeOffset.UtcNow.Add(delay)` — tests must control that, not sleep.

### T8 — Compensation chain order declared
Any capability with multi-step rollback declares LIFO order explicitly. Persist
before dispatch (DNA Rule 8): `UpsertAsync` before `AppendAsync`/`ScheduleDelayedAsync`.

### T9 — DNA Rule compliance
DNA rules satisfied (`DNA-DOTNET-TRANSLATION.md`):
- Rule 3: every public service method returns `DataProcessResult<T>`.
- Rule 4: fabric-first (interfaces, not concrete types) + `MicroserviceBase<T>`.
- Rule 5: tenant from `ITenantContextAccessor`; no shared mutable static.
- Rule 8: persist before dispatch.

### T10 — Scope discipline
The plan does not touch projects outside its declared scope. Any cross-project
effect is called out with explicit justification.

---

## Section 3 — Completeness Score Format

```markdown
## Subproject Completeness Check — [Plan Name]

Pre-Gate 1 (Goal-to-Phase Mapping): PASS / FAIL
Pre-Gate 2 (Design Artifact Populated): PASS / FAIL

If Pre-Gate 1 or Pre-Gate 2 FAIL: stop here. Do not evaluate T1–T10.

Technical completeness:
| Check | Status | Note |
|---|---|---|
| T1 — Phase structure | PASS / FAIL | [detail] |
| T2 — Gate B measurable | PASS / FAIL | [detail] |
| T3 — Prerequisites verified | PASS / FAIL | [detail] |
| T4 — DataProcessResult<T> | PASS / FAIL | [detail] |
| T5 — Fabric/DI registration | PASS / FAIL | [detail] |
| T6 — Test categories | PASS / FAIL | [detail] |
| T7 — Injectable clock | PASS / FAIL | [detail if applicable] |
| T8 — Compensation LIFO / persist-before-dispatch | PASS / FAIL | [detail if applicable] |
| T9 — DNA compliance | PASS / FAIL | [detail] |
| T10 — Scope discipline | PASS / FAIL | [detail] |

Score: [N]/12 checks pass (2 pre-gates + T1–T10)

Overall verdict: READY / NOT_READY
Blocking items: [list]
Next step: [resolve blocking items / proceed to Phase 1]
```

---

## Anti-Patterns

1. **"The plan covers the goals implicitly"** — implicit coverage is the failure mode. Goal-to-phase mapping must be explicit.
2. **"Gate B is 'the tests pass'"** — a Gate B without a filter and count is not checkable.
3. **"We can add the test categories after implementation"** — the matrix is declared before coding so implementation satisfies the declared behavior.
4. **"The artifact will be implemented in a later phase"** — if Phase 2 depends on a Phase 1 artifact, Phase 1 must be a listed prerequisite.

---

## Integration

- `plan-self-validation-SKILL.md` Check 1 is the goal-delivery version of Pre-Gate 1.
- `design-artifact-completeness-SKILL.md` provides Checks 1–5 for Pre-Gate 2.
- `integration-testing-SKILL.md` / `e2e-test-matrix-SKILL.md` govern T6.
- `goal-delivery-completeness-SKILL.md` provides the arbiter that runs Pre-Gate 1 formally.
- `subproject-examination-SKILL.md` produces the phase plan this checker validates.
