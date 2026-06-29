# HOW-TO-USE-SKILLS.md — single skill index + load order (XIIGen Enterprise Platform)

> Ported from the `llm_mvp_core` governance standard (root `docs/HOW-TO-USE-SKILLS.md`),
> adapted for the **XIIGen Enterprise Platform: .NET 10 ASP.NET Core Web API**, xUnit,
> Gate-B = `dotnet build` / `dotnet test`, domain return type `DataProcessResult<T>`
> (`src/Xiigen.Kernel/DataProcessResult.cs`) — **not** core `OperationResult<T>`
> (`OperationResult<T>` does not exist in this repo's `src/`; it stays in `llm_mvp_core`).
> This repo has **no frontend** — every UI / screen / component / visual-design row is `na`.

This file is the **single source of truth** for "when to load which skill" in this
repository. The `docs/skills/` directory is the canonical skill library; this index
plus the per-skill files replace ad-hoc skill loading. There is exactly one
`HOW-TO-USE-SKILLS.md` of record (this one). The older root `docs/HOW-TO-USE-SKILLS.md`
is the enterprise-adaptation **binding-order** note (beacon → state → matrix → session)
and remains valid for that narrow purpose; this file is the **skill catalogue** index.

---

## Load order: always start here

```text
Step 0: Beacon first (this repo's binding control plane — outranks skills)
        docs/plans/enterprise-adaptation/v3.3/XIIGEN-ENTERPRISE-ADAPTATION-BEACON-PROMPT-v1.md
        docs/plans/enterprise-adaptation/v3.3/XIIGEN-ENTERPRISE-ADAPTATION-STATE-v3.3.json
        docs/plans/enterprise-adaptation/v3.3/XIIGEN-ENTERPRISE-ADAPTATION-COMPLETION-MATRIX-v3.3.md
        the active SESSION-XX-*.md file
Step 1: cat AGENTS.md + CLAUDE.md                                    (every session)
Step 2: read the active plan/state and identify the allowed verification
Step 3: load session-constitution-SKILL.md                          (every session)
Step 4: load the session-type skill (table below)
Step 5: load activation triggers if applicable
```

The beacon read in **Step 0** comes **before** any skill. In `llm_mvp_core` the
load order starts at `cat CLAUDE.md + AGENTS.md`; in this repo the v3.3 beacon /
state / completion-matrix / active session file are read **first**, then the agent
files, then the skills. This is the project adaptation, not a softening.

### Authority precedence (project-local override)

```text
The v3.3 BEACON / STATE / COMPLETION-MATRIX outrank every skill file.
AGENTS.md and CLAUDE.md outrank every skill file.
No skill may introduce a user-facing STOP, approval wait, branch switch, or a full
`dotnet test` run unless the active beacon/plan Gate B or the current instruction
requires it.
```

Skill-local words such as `STOP`, `hard stop`, `must be on main`, `run dotnet test`,
or `wait for answer` are **internal checks only**. They do not override:

```text
continue while next_action is known (executor cannot self-finish — REVIEW_INTERNAL decides);
use the current branch unless explicitly asked to switch;
run only the verification allowed by the active phase Gate B;
the No-Secrets-in-repo MACHINE invariant (AGENTS.md);
commit/push only when the phase deliverable requires it and the matrix has no required UNKNOWN/FAIL rows.
```

### Verification-before-commit is a hard gate

Before any commit/push boundary, failed verification output is a hard fail even if a
later commit command succeeds. Run each pre-commit check (`git diff --check`,
`git diff --cached --check`, `dotnet build`, `dotnet test`, JSON parse) as a **separate**
command and inspect its exit code immediately; abort before staging/commit/push on any
nonzero. Do not chain a verification command and a commit/push command in a way that can
continue after the verification failed.

### Partial progress is not a stop boundary

A skill may package a phase, write a report, or recommend a checkpoint. But if the active
state has `stop_allowed=false` and a non-empty `next_action`, the executor must continue
that next action. Commit/push, report generation, a passing lower-level gate, or
"context getting long" cannot override the active plan's continuation requirement. Soft
stops ("continue next", "wait for approval", "status only", "checkpoint complete",
"handoff", "AWS URL produced", "screenshot captured") must be converted to continued
execution when `next_action` is known.

### Review + architect repair are mandatory internal loop states

The binding role loop (from `AGENTS.md`) is:

```text
PLAN_AUTHOR_INTERNAL -> PLAN_REVIEW_INTERNAL -> ARCHITECT_INTERNAL
  -> EXECUTOR_INTERNAL -> REVIEW_INTERNAL
  -> ARCHITECT_INTERNAL_REPAIR -> EXECUTOR_INTERNAL_CONTINUE
```

The executor may not self-finish. A slice/test/report/commit/push/evidence/matrix result
hands control to `REVIEW_INTERNAL`. Only an architect verdict authorizes final reporting,
and only when `stop_allowed` is true and the completion matrix has no required `UNKNOWN`
or `FAIL` rows.

---

## Session type → skills

| Session type | Load before first output |
|---|---|
| PLAN_AUTHOR | `planning-session-startup-SKILL.md` + `authority-requirement-binding-SKILL.md` + `no-product-decisions-SKILL.md` |
| PLAN_REVIEW / REVIEW | `plan-review-SKILL.md` + `audit-protocol-SKILL.md` + `claim-verification-SKILL.md` |
| ARCHITECT | `plan-review-SKILL.md` + `no-product-decisions-SKILL.md` + `architect-behavior-classifier-SKILL.md` |
| EXECUTOR | `dev-safety-SKILL.md` + `code-execution-SKILL.md` + `dna-compliance-guard-SKILL.md` |
| QA / TEST | `qa-session-type-SKILL.md` + `test-integrity-SKILL.md` + `integration-testing-SKILL.md` |
| ~~UI / FRONTEND~~ | **na** — this repo has no frontend |

---

## Activation triggers — load when the condition is true

| When | Load |
|---|---|
| Session begins before any plan is written | `authority-requirement-binding-SKILL.md` + `planning-session-startup-SKILL.md` |
| Plan has no Gate B / a vague Gate B | `plan-review-SKILL.md` |
| About to declare a plan "ready for execution" | `audit-protocol-SKILL.md` (10-point checklist + NOT-CHECKED) |
| Deriving behavioral invariants for a new capability/service | `iron-rule-derivation-SKILL.md` |
| Predicting where a new capability class will fail per score bracket | `archetype-failure-map-SKILL.md` |
| Authoring a context/code-generation patch (score 0.65–0.84 gap) | `prompt-patch-authoring-SKILL.md` |
| Mapping a core concept to its platform equivalent | `STACK-MAP.md` (a.k.a. `REMOVED-FROM-XIIGEN.md`) |
| New `.cs` service file created | `dev-safety-SKILL.md` (DataProcessResult + Fabric-First) + `dna-compliance-guard-SKILL.md` |
| `ISchedulerService` / `IOutboxStore` / `IIdempotencyStore` / `IDocumentStore` / `ITenantContextAccessor` injected | `dna-compliance-guard-SKILL.md` (Fabric-First) |
| Tenant-scope on a query suspected missing | `dna-compliance-guard-SKILL.md` (`ITenantContextAccessor`) |
| About to claim "done" / "passes" | `verification-before-completion-SKILL.md` (run the named verification, show output) |
| Bug reported or symptom observed | `bug-to-tests-SKILL.md` (failing tests before fix) → `mental-debug-SKILL.md` |
| Commit about to happen | `dev-safety-SKILL.md` + `phase-git-report-SKILL.md` |
| Any architecture/trainable/code audit or report that must be human-readable | `human-readable-architecture-audit-SKILL.md` + `output-readability-gate-SKILL.md` |
| A later broader audit finds BLOCKING/HIGH issues that should have been found | `audit-protocol-SKILL.md` (Delta-Severity Feedback Gate) |
| Any learned/trainable/semantic/DPO/model claim | the model **stays in `llm_mvp_core`** and is consumed via manifest/locator — see `STACK-MAP.md`; do not replicate it here |

---

## Required skill orchestration by lifecycle stage

```text
plan creation:
  authority-requirement-binding
  planning-session-startup
  no-product-decisions
  iron-rule-derivation        (derive invariants before tests)
  archetype-failure-map       (per-capability score→action map)

plan review:
  plan-review
  audit-protocol              (10-point checklist + severity + NOT-CHECKED)
  claim-verification

execution:
  code-execution
  code-examination
  dev-safety
  verification-before-completion

code review:
  generated-code-review
  test-integrity
  dna-compliance-guard
  human-readable-architecture-audit

documentation sync:
  documentation-sync
  document-hierarchy

completion verification:
  verification-before-completion
  audit-protocol
```

> **ML / graph-sync note (R5/R6):** the skill-graph drift engine
> (`GraphDriftDetector` / `SkillCompiler` → `DomainModelRegistry`) is core-ML and
> stays in `llm_mvp_core` (group G12). It is **not** replicated here. The portable
> non-ML authoring discipline (`❌` on its own line, matchable triggers, machine-runnable
> verification blocks) applies to the skills in this directory; verification blocks use
> `dotnet test` / `[Fact]` and `grep ... --include="*.cs"` over `src/`, not `*.ts`/`npm`.

## Integration

- `AGENTS.md` / `CLAUDE.md` / the v3.3 beacon are the authority this index is subordinate to.
- `STACK-MAP.md` (`REMOVED-FROM-XIIGEN.md`) maps core concepts → platform equivalents.
- `audit-protocol-SKILL.md` Gate runs at every "ready for execution" boundary.
- `iron-rule-derivation-SKILL.md`, `archetype-failure-map-SKILL.md`, and
  `prompt-patch-authoring-SKILL.md` are the G11 governance-discipline ports added with this index.
