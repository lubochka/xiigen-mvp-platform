# Goal Delivery Completeness (XIIGen Enterprise Platform)

## Core Problem

A plan's internal consistency can be strong while its coverage of the user's
stated goals is incomplete. A plan can accumulate many review rounds checking
internal correctness — count drift, path errors, skill inversion — and ship
missing goals, because every review asked "is this plan internally consistent?"
and none asked "does this plan deliver what was asked for?" This skill installs
the check that asks that question first.

Platform port. Stack-bound to **.NET 10 ASP.NET Core Web API**, **xUnit**
(`tests/`), `Xiigen.Platform.slnx`, and the v3.3 governance triple. The source of
session goals is the active `SESSION-XX-*.md` file plus `STATE-v3.3.json`.

## When to Invoke

- At every plan review, as the FIRST check — before all other checks.
- When a plan changes its phase list — re-run to confirm goal coverage holds.
- Before any STOP — confirm nominated phases actually map to goals.
- Any time the user asks "does this plan cover everything?"

One pass before plan approval = zero "plan passed review but shipped missing
goals" failures.

## Section 1 — Position in Review Order

This check runs **first**. If it returns BLOCK, no other check runs. Auditing
internal consistency of a plan that misses goals is wasted effort — fix coverage
first.

## Section 2 — Inputs (strict isolation)

**Input 1 — verbatim goal statement:** the exact text stated as the session goal
(from the active `SESSION-XX` file / `STATE-v3.3.json`), not paraphrased. If no
goal is recorded, the check halts with a request to state the goal explicitly.

**Input 2 — phase list with 1-line purpose per phase.** Not full phase content,
not test lists, not file lists. This check evaluates coverage at the phase-name
level.

Inputs NOT received: codebase state, prior plan versions, other check verdicts,
session history. The isolation is intentional — a 2-input check produces a
verdict anyone can re-verify in minutes.

## Section 3 — Protocol (5 steps)

**Step 1 — Decompose the goal** into discrete goal elements (GE-1..N), each
independently deliverable and verifiable.

```
Goal: "Expose ISchedulerService through the API and persist jobs via IOutboxStore"
GE-1: scheduler reachable through an ASP.NET Core endpoint
GE-2: scheduled jobs are persisted through IOutboxStore
```

**Step 2 — Parse the phase list** (phase name + 1-line purpose).

**Step 3 — Build Goal -> Phase mapping** (which phase(s) deliver each GE).

**Step 4 — Verify each mapped goal has a verification step** in the plan:
- a `dotnet test` filter and count: `dotnet test --filter "FullyQualifiedName~Scheduler" -> 5 passed`;
- a build condition: `dotnet build Xiigen.Platform.slnx -> 0 Error(s)`;
- a concrete observable: `POST /schedule returns 200 and a job id; job row visible via IOutboxStore.ListAsync`.

For runtime / API / persistence goals, the verification step must prove runtime
delivery, not only report existence. Reject goal coverage if the verification
allows any of these as final proof:

```text
validation_only: true
metric_type contains static, contract, or mixed
cpu_ms: UNKNOWN
ram_mb: UNKNOWN
build/static-contract check without an executed runtime/integration probe
compatibility_report_ref without an executed compatibility probe
rollback_ref without an executed rollback/load probe
DI/registry field presence without a load/invoke probe
```

If a goal says "works", "exposes", "persists", "schedules", or "end-to-end", the
plan must name the runtime gate (e.g. a WebApplicationFactory integration test)
that executes that path; otherwise it is `BLOCK - no runtime proof`.

For final completion, the plan must also name a completion coverage matrix that
compares each GE and each `COMPLETION-MATRIX-v3.3` literal to actual evidence.

Reject goal coverage when the plan allows: "at least one" for a GE that requires
all classes/projects; category buckets instead of required subtype coverage; a
final readiness report that omits a child gate as source of truth;
`goal_reached: true` without a zero-gap matrix.

**Step 5 — Produce verdict table.**

```markdown
| Goal Element | Phase(s) | Verification Step | Verdict |
|---|---|---|---|
| GE-1: scheduler endpoint | P1, P3 | dotnet test --filter ~Scheduler -> 5 passed | APPROVED |
| GE-2: outbox persistence | P2, P3 | integration test asserts IOutboxStore row | APPROVED |
```

## Section 4 — Verdict Classes

- **APPROVED** — GE has >=1 phase assigned and a verification step.
- **BLOCK — goal element not covered** — GE has zero phases. Resolution: add
  phases, or defer explicitly with the architect/Luba's approval.
- **BLOCK — no way to verify** — GE has phases but no verification step.
- **CHALLENGE** — phase assignment is ambiguous from names/purposes.

Aggregate: any BLOCK -> plan rejected, no other checks run; any CHALLENGE (no
BLOCK) -> returned for clarification; all APPROVED -> proceed.

## Section 5 — Output Format

```markdown
## Goal Delivery Completeness Verdict
Goal statement (verbatim): "[exact text]"
Goal elements: GE-1 ... GE-2 ...
Plan phases: P1 [name — purpose] ...
Goal -> Phase mapping: [table]
Aggregate verdict: APPROVED | BLOCK | CHALLENGE
Unmapped goals: x of N   Unverified goals: y of N   Ambiguous: z of N
Next action: [reject / clarify / proceed]
```

## Anti-Patterns

1. **"The goal is obviously covered by Phase 3"** — obvious coverage is where
   drift hides. The mapping is mechanical on purpose.
2. **"I'll add the verification step during implementation"** — a step in the
   plan is a commitment; a step "in implementation" is a wish. This check
   evaluates the plan.
3. **"CHALLENGE is easier than BLOCK"** — that is gaming; write clear phases.
4. **"Multi-goal statements can share one phase"** — they can, but both goals
   must be explicitly mapped; implicit sharing is not.
5. **"The plan has 10 review rounds, surely one caught coverage"** — correctness
   rounds do not audit goal coverage; only this check does.
