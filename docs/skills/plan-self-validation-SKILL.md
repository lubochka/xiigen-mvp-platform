# Plan Self-Validation (Phase 0)

> Ported from the `llm_mvp_core` planning standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build` / `dotnet test`. Phase 0 here also verifies the plan references
> the **current** v3.3 Beacon / state / completion-matrix
> (`docs/plans/enterprise-adaptation/v3.3/...`) and that every requirement maps to
> an ASP.NET Core layer + an xUnit gate. Domain result type is
> `DataProcessResult<T>` (never core `OperationResult<T>`).

## Core Problem

A plan that references a skill that does not exist yet, or omits the phase that
creates a skill it later invokes, or assumes a fabric interface signature that
does not match the code, will fail at execution time with no useful diagnostic.
Phase 0 catches those failures before Phase 1 executes.

Phase 0 also validates **authority-chain requirements**. Requirements from the
current human operator / product owner, an architect work order, the approved
active plan, or an accepted reviewer gate are binding unless that authority
explicitly marks them optional, exploratory, or preference-only. They are not
wishlist items.

Before Phase 1, build an **Authority Requirements Ledger**, a **Conversation
Delta Ledger**, a **Replacement Mapping Register**, and a **Contradiction
Sweep**. Every binding requirement must map to a plan section, a gate/test, and
an artifact. Every rejected or superseded mechanism must map to the required
replacement capability. **Deletion-only coverage is not valid** — removing an old
mechanism is not the same as delivering its replacement.

Phase 0 also rejects **requested-artifact substitution**. A plan is invalid if
its literal execution would create validators, reports, JSON bundles, or
PowerShell scripts where the authority asked for runtime Web API code, a fabric
implementation, or host integration.

**Dishonest-claim prevention is part of Phase 0.** A ledger row, checklist row,
generated table, or summary cannot be counted as a review/fix cycle. Parent/self
review cannot be counted as a sub-agent review. If the plan requires N reviews /
cycles / passes, it must define the N independent evidence records (ids, work
orders, reviewed scopes, findings, recheck evidence) that will count.

When a prior implementation already reached the wrong code shape, Phase 0 starts
with **skill/guide gap repair, not plan repair**:
1. Identify the missing architect/planning/review rule.
2. Patch the skill or guide.
3. Mentally replay the plan under the patched rule.
4. Repeat until the replay produces the requested artifact class.
5. Only then approve plan repair or executor work.

## When to Invoke

- Before any multi-phase plan begins execution
- At the end of plan authoring, before handing a plan to an EXECUTOR
- Whenever a plan references skills, interfaces, or artifacts that may not exist yet

One Phase 0 gate before Phase 1 = no phantom-skill / wrong-shape / stale-interface
failures mid-execution.

## The Phase 0 Requirement

Every multi-phase plan (≥ 2 phases) must contain a **Phase 0: Plan
Self-Validation** that runs all checks below before Phase 1 executes. If the plan
does not include Phase 0, add it before execution begins.

Phase 0 gate: all checks pass → Phase 1 may proceed. Any check fails → return to
planning repair (not a stop; see `plan-restructure-SKILL.md` and the
Plan-Not-Ready repair loop in `plan-review-SKILL.md`).

## The Checks

### Check -1 — Requested Artifact Shape Simulation
Simulate the files and entrypoints the plan would create before checking phase
coverage.
```
Target repository: xiigen-platform-dot-net-angular
Requested artifact class:        # e.g. Web API endpoint + domain service + xUnit tests
Expected implementation files:   # e.g. src/Xiigen.Platform.*/*.cs, Program.cs route, tests/*.Tests/*.cs
Forbidden substitute artifacts:  # JSON/PowerShell/report-only as the deliverable
Evidence artifacts location:
Replay result: PASS | FAIL
```
If FAIL, repair the governing skills/guides before editing the plan.

### Check 0 — Authority Requirements Ledger Completeness
```
Requirement ID: R-[short-name]
Authority source: [current Luba instruction / v3.3 Beacon / work order / reviewer gate]
Plan section: [section or UNMAPPED]
Gate/test: [dotnet test --filter ... or dotnet build, or UNMAPPED]
Artifact: [artifact or UNMAPPED]
Status: PASS | UNMAPPED | CONTRADICTED
Next action: [repair action]
```
If any row is unmapped or contradicted → BLOCKED.

### Check 1 — Goal Delivery Completeness
Every element of the stated goal is assigned to exactly one phase with a Gate-B
condition that is an exact `dotnet test` / `dotnet build` result.
```
Goal element: [element text]
Assigned phase: Phase [N] — [phase name]
Gate B: dotnet test --filter "[filter]" → [count] passed, 0 failed   (or: dotnet build → 0 Error)
Status: PASS | BLOCKED
```

### Check 2 — Phantom Skill Detection
Every skill referenced in any phase must either already exist in `docs/skills/`
with a matching filename, or be created by a phase that runs before the
referencing phase.
```
Skill referenced: [skill-name]-SKILL.md
Referenced in: Phase [N]
Exists now (ls docs/skills/[name]-SKILL.md): YES / NO
Created by: Phase [M] (M < N) / NOT CREATED → BLOCKED
```

### Check 3 — Skill Presence Matrix
Every skill created by the plan must be registered in
`docs/HOW-TO-USE-SKILLS.md` by a phase that runs before it is first invoked.
```
New skill: [name]-SKILL.md
Created by: Phase [M]
Registered in HOW-TO-USE by: Phase [R] (R ≤ M)
First invoked by: Phase [N] (N > R)
Status: PASS | BLOCKED
```

### Check 4 — Execution Order + Assumption Consistency
1. HOW-TO-USE update phase runs AFTER the skill-creating phase.
2. No phase depends on an artifact produced by a later phase.
3. Gate-B conditions are testable with `dotnet test` / `dotnet build` — not
   "visually inspect".
4. Every assumption about a fabric interface signature
   (`IDocumentStore` / `IOutboxStore` / `IIdempotencyStore` /
   `ITenantContextAccessor` / `ISchedulerService`) or DI registration in
   `Program.cs` is verified against live code (delegate to
   `phase-preflight-SKILL.md`).
```
Phase [N] dependency: [artifact]
Produced by: Phase [M]
Order: M < N → PASS | M > N → BLOCKED
```

## Phase 0 Verdict
```
PHASE 0 VERDICT: PASS → Phase 1 may proceed
               | FAIL → [list blocking items] → return to planning repair
```

## Anti-Patterns
1. **"Skip Phase 0 for small plans"** — phantom skills in 2-phase plans fail just as hard.
2. **"I'll fix phantom skills / wrong interface signatures as I go"** — that stops the EXECUTOR cold.
3. **"Gate B is 'tests pass'"** — a Gate B is `dotnet test → 47 passed, 0 failed`. Exact count, exact command.
4. **"HOW-TO-USE update doesn't need its own phase"** — it does, and after the skill-creating phase.

## Integration
- `intent-to-plan-SKILL.md` produces the plan structure this skill validates.
- `phase-preflight-SKILL.md` handles the live-code assumption verification in Check 4.
- `phase-planning-SKILL.md` Gate -1 carries the same Authority Requirements Ledger.
- `plan-review-SKILL.md` FC-0 re-checks authority traceability at review time.
