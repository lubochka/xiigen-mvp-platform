# Parallel Execution Coordinator

> Ported from the `llm_mvp_core` parallel-execution standard, adapted for the
> XIIGen Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build Xiigen.Platform.slnx` / `dotnet test`. The delta gate is the
> increase in xUnit tests for the target project
> (`dotnet test --filter "FullyQualifiedName~..."`). Pre-allocated artifact
> ranges and the CAS lifecycle status are recorded in the v3.3 state artifact
> (`expected current = NOT_STARTED`); the prerequisite Gate B is read from the
> v3.3 completion matrix. Domain return type is `DataProcessResult<T>` (never
> core `OperationResult<T>`).

## Core Problem

Sequential and parallel execution look identical from the outside — both have
Phase A..N, both produce Gate B completions. The difference is entirely in three
behaviors that must switch when parallel execution is active:
(1) artifact numbering must use pre-allocated ranges rather than live queries,
(2) gate checks must measure deltas rather than absolute baselines, and
(3) lifecycle writes must use compare-and-swap (CAS) rather than blind writes.
Without this skill, a parallel executor races for artifact numbers, applies stale
baseline checks, and overwrites concurrent lifecycle state.

---

## When to Invoke

- When a session begins with `parallelWave` set in the v3.3 state artifact.
- Before Phase A of any parallel project execution.
- When multiple project phases are planned to run concurrently.
- When any gate check runs in a parallel execution session.

---

## Section 1 — Three Questions at Session Start

Read the v3.3 state artifact. Answer all three before any Phase A action.

### Question 1: Am I in parallel mode?

```
parallelWave: null → SEQUENTIAL mode
  Use absolute gate, live artifact boundaries, blind lifecycle write.
  This skill is dormant.

parallelWave: N → PARALLEL mode
  Switch all three behaviors in Section 2. This skill is active.
```

### Question 2: What are my artifact ranges?

```
PARALLEL mode only:

1. Read the v3.3 state/matrix → pre-allocated artifact ranges for this project.

2. If the project has no pre-allocated range entry:
   STOP — pre-allocation planning has not run for this wave.
   Do NOT query live docs. Do NOT estimate ranges.
   Report: "[ProjectName] has no pre-allocated artifact ranges.
            Run parallel pre-allocation planning first."

3. If an entry exists: use those test-count / capability / phase ranges exclusively.
   Do NOT verify against live docs — ranges are immutable once allocated.

4. If a collision is detected during Phase A (artifact ID already claimed):
   STOP — report conflict. Do not overwrite. Await instruction.
```

### Question 3: Have I verified the prerequisite project?

```
PARALLEL mode only (Phase A, before any other action):

Read prerequisiteProject from the v3.3 state artifact.
If present: verify its Gate B shows PASS in the v3.3 completion matrix before proceeding.
If the prerequisite Gate B has not passed:
   STOP — prerequisite project phase is incomplete.
   Report which project and phase is blocking. Do not proceed.
```

---

## Section 2 — Three Behavior Switches in Parallel Mode

### Switch 1: Gate model (absolute → delta)

```
SEQUENTIAL (parallelWave: null):
  dotnet test Xiigen.Platform.slnx
  Assert: total passing ≥ {absolute_baseline}

PARALLEL (parallelWave: N):
  dotnet test --filter "FullyQualifiedName~{ProjectName}"
  Assert: {project_tests} shows {expected_delta} new tests passing
  Total baseline is NOT checked here — CI verifies it post-merge.
```

Why: in parallel mode, other project phases may not yet be merged. The absolute
total is unknowable until all parallel phases complete and merge.

### Switch 2: Artifact number assignment (live query → pre-allocated)

```
SEQUENTIAL (parallelWave: null):
  Read the v3.3 decision log → last number + 1 (live query, safe)

PARALLEL (parallelWave: N):
  Read the pre-allocated range for this project from the v3.3 state/matrix
  Use the next number from the range only — do NOT query the live log for "last used"
  (another parallel instance may have claimed numbers from the live range)
```

### Switch 3: Lifecycle status write (blind write → CAS)

```
SEQUENTIAL (parallelWave: null):
  Write project status → ACTIVE  (blind write — no concurrent writers possible)

PARALLEL (parallelWave: N):
  Write project status → ACTIVE, expected current status = NOT_STARTED (CAS)
  If CAS fails (another instance already claimed):
   STOP — conflict detected. Report and await instruction.
  If CAS succeeds: continue.
```

---

## Section 3 — Execution Log in Parallel Mode

In parallel mode the execution log includes the delta, not the absolute:

```json
{
  "parallelWave": 2,
  "waveBaselineEntry": 916,
  "testDelta": 12,
  "testBaselineExit": null
}
```

`testBaselineExit` is null in parallel mode — the final total is unknown until
all parallel phases complete. `testDelta` is the count this phase added.

---

## Section 4 — Worked Examples

**Correct parallel execution sequence:**
```
1. Read v3.3 state → parallelWave = 2 → PARALLEL mode active
2. Check prerequisite → Xiigen.Kernel Phase 3 → matrix Gate B = PASS → proceed
3. Read pre-allocated ranges → use the assigned decision numbers for this project
4. CAS write: project status NOT_STARTED → ACTIVE → { success: true } → continue
5. Implement phase, run tests
6. Gate check: dotnet test --filter "FullyQualifiedName~Marketplace" → +12 new tests pass
7. Record testDelta = 12, testBaselineExit = null
```

**Wrong — parallel mode using sequential behaviors:**
```
1. Read v3.3 state → parallelWave = 2
2. Query the live decision log → returns the last used number
   But another parallel instance already claimed the next two numbers
3. Register a duplicate → COLLISION with the other instance
→ Decision log corrupted, duplicate entries
```

---

## Anti-Patterns

1. **"In parallel mode, running absolute gate check"** — absolute baseline is unknowable in parallel mode. Use delta gate only.
2. **"Reading live docs for artifact numbers in parallel mode"** — another parallel instance may read the same docs simultaneously. Use pre-allocated ranges.
3. **"Blind lifecycle status write in parallel mode"** — CAS is mandatory; a blind write overwrites another instance's claim.
4. **"Proceeding when pre-allocated ranges are missing"** — STOP. Missing pre-allocation means the range-assigning planning session has not run. Do not estimate.

---

## Integration

- The v3.3 state/matrix artifact stores the pre-allocated ranges and CAS lifecycle status.
- `wave-assignment-SKILL.md` decides whether a phase is Wave 2+ parallel (delta) or earlier (absolute).
- Gate B per phase (`plan-review-SKILL.md`) switches to delta measurement in parallel mode.
- `phase-completion-packager-SKILL.md` notes delta vs absolute gate in the completion packet.
