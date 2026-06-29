# Wave Assignment

> Ported from the `llm_mvp_core` wave-assignment standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build Xiigen.Platform.slnx` / `dotnet test`. The unit is a platform
> project (`src/Xiigen.*`). FOUNDATION = `Xiigen.Kernel` (fabric contracts in
> `FabricContracts.cs`) + `Xiigen.Fabrics.*` (everything depends on them via DI);
> Wave 0+ = `Xiigen.Auth` / `Xiigen.Platform.Marketplace` / `Xiigen.Platform.Api`.
> Absolute gate = `dotnet test` (xUnit) 0 failed; delta gate = increase in xUnit
> test count. Wave records go into the v3.3 beacon/state artifact (no
> `DECISIONS-LOCKED.md` on platform). Domain return type is `DataProcessResult<T>`.

## Core Problem

A capability that should be built as shared infrastructure gets built as a
per-project dependency instead. Later, several projects discover they all need
the same capability and each implements it differently. Wave assignment prevents
this: it classifies every project/capability before planning, identifies
infrastructure candidates (N > 2 dependents), and assigns the right sequential vs
parallel execution model for each wave.

---

## When to Invoke

- Before any project/capability phase is planned (wave assignment is an input to planning).
- When a new shared capability is identified that multiple projects might need.
- When `parallel-execution-coordinator-SKILL.md` asks which wave a phase belongs to.
- Before assigning test gate type (absolute vs delta) for a new phase.

---

## Section 1 — Wave Structure

| Wave | Type | Execution | Gate model |
|---|---|---|---|
| FOUNDATION | Shared infrastructure (Kernel + Fabrics) | Sequential, completes before Wave 0 | Absolute (`dotnet test` 0 failed) |
| WAVE 0 | First capability projects | Sequential, one at a time | Absolute |
| WAVE 1 | Second-generation capabilities | Sequential, some pattern data available | Absolute |
| WAVE 2+ | Mature capabilities | May run in parallel (up to 5 simultaneous) | Delta (test count must not decrease) |

---

## Section 2 — Rule 1: Infrastructure Test

A project/capability belongs at FOUNDATION when:

> **More than 2 other planned projects depend on this project's output.**

Apply the test literally. Count downstream dependents.

```
If dependents > 2: FOUNDATION (shared infrastructure — build before Wave 0)
If dependents ≤ 2: Wave 0 or later (not infrastructure)

Examples on platform:
  Xiigen.Kernel (fabric contracts + DataProcessResult + MicroserviceBase):
    Used by: Auth, Freedom, Fabrics.*, Platform.Api, Platform.Marketplace → >2 → FOUNDATION

  Xiigen.Fabrics.InMemory / Xiigen.Fabrics.Scheduler (contract implementations):
    Wired into every host via DI → >2 → FOUNDATION

  Xiigen.Platform.Marketplace (Smart Wiki / Smart Code):
    Consumed by Platform.Api endpoints → 1 dependent → NOT FOUNDATION → Wave 0/1
```

**Edge case: expected vs current dependents.** A project with 1 dependent today
but expected to grow to many should be assigned FOUNDATION based on intended
architecture. Reassignment after implementation is expensive — plan for the
intended architecture.

**Wrong dependency direction:** a capability project should not become FOUNDATION
just because another capability project shares patterns. The rule is directional:
if A needs B's runtime output to function, B is upstream. If A only reuses B's
patterns, they are peers at the same wave.

---

## Section 3 — Rule 2: Sequential vs Parallel (Wave 2+ only)

**Run in parallel when:**
- At least 3 projects of prior waves have completed (pattern data available).
- No cross-project dependency between the parallel projects.
- The delta gate is valid (baseline stable — no pre-existing failures, 0 warnings).
- Review bandwidth exists for simultaneous completion reviews.

**Run sequentially when:**
- Fewer than 3 prior projects have completed.
- Any cross-project dependency exists (A's runtime output feeds B's implementation).
- Pre-existing test failures or build warnings (delta gate unreliable until absolute gate is clean).

---

## Section 4 — Rule 3: Gate Type Selection

```
FOUNDATION:            Absolute gate (failures == 0, warnings == 0)
  Reason: any failure blocks ALL downstream projects

WAVE 0, WAVE 1:        Absolute gate
  Reason: establishing the baseline — no prior history for delta comparison

WAVE 2+ sequential:    Absolute gate
  Reason: sequential is safe; no parallelism conflict

WAVE 2+ parallel:      Delta gate (test count must not decrease)
  Reason: parallel runs may not have merged; absolute total is unknowable mid-wave
```

**Delta gate definition:** `testCount at phase end ≥ testCount at phase start`. It
does NOT pass if tests were deleted to eliminate failures.

**Delta gate risk:** it can pass with 3 new failures if 4 new tests were added.
That is why it is restricted to Wave 2+ parallel where the baseline is stable and
`parallel-execution-coordinator-SKILL.md`'s CAS semantics prevent races.

---

## Section 5 — Wave Assignment Record

For every project and every shared capability, record the wave assignment in the
v3.3 beacon/state artifact (or a wave-assignment log):

```markdown
## Wave Assignment — [ProjectName or Capability]

**Question:** Should [X] be FOUNDATION or Wave N?
**Downstream count:** [N projects depend on this]
**Reasoning:** [why this wave, not another]
**Principle applied:** [N > 2 → FOUNDATION / sequential because no pattern data / etc.]
**Gate type:** [ABSOLUTE / DELTA]
**Outcome:** [FOUNDATION / WAVE 0 / WAVE 1 / WAVE 2 parallel]

**Prerequisites (must be ACTIVE before this project starts):**
  - [FOUNDATION project A — e.g. Xiigen.Kernel]
  - [FOUNDATION project B — e.g. Xiigen.Fabrics.InMemory]
```

---

## Section 6 — Prerequisite Chain Format

Every project declares its prerequisites explicitly:

```json
{
  "projectId": "Xiigen.Platform.Marketplace",
  "wave": 1,
  "hardPrerequisites": ["Xiigen.Kernel", "Xiigen.Fabrics.InMemory"],
  "softPrerequisites": [],
  "downstream": ["Xiigen.Platform.Api"]
}
```

A project cannot become ACTIVE if any `hardPrerequisite` is not ACTIVE.
`softPrerequisites` are recommended but not blocking — document why the soft
dependency exists but is not hard.

---

## Anti-Patterns

1. **"Assigning a wave without counting downstream dependents"** — "this seems like infrastructure" is not the protocol. Count dependents. N > 2 → FOUNDATION.
2. **"Delaying infrastructure projects to keep the critical path shorter"** — infrastructure delays become compound blockers; the most expensive rework is a Wave 0 project discovering it needs a FOUNDATION contract mid-implementation.
3. **"Assigning Wave 2+ parallel without checking cross-project dependencies"** — if A's runtime output feeds B's implementation, they are sequential.
4. **"Using delta gate for Wave 0 or Wave 1"** — no baseline exists for delta comparison in early waves. Always use absolute gate for FOUNDATION, Wave 0, Wave 1.
5. **"Assigning current wave, not intended wave"** — plan for the intended architecture.

---

## Integration

- `parallel-execution-coordinator-SKILL.md` uses the wave assignment to choose the gate model (absolute vs delta).
- `prerequisite-chain-SKILL.md` verifies hard prerequisites are ACTIVE before a project starts.
- `feature-prioritization-SKILL.md` shares the dependency graph (downstream count = Axis 2).
- The v3.3 beacon/state artifact is the canonical record of current wave assignments.
