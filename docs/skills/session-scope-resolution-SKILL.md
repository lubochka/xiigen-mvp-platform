# session-scope-resolution-SKILL.md — Dispatch table: session type → docs + chain + blockers

> Ported from the `llm_mvp_core` governance standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build` / `dotnet test`, domain return type `DataProcessResult<T>`
> (`src/Xiigen.Kernel/DataProcessResult.cs`) — **not** core `OperationResult<T>`.
> Minimum documents are the v3.3 set: Beacon, state json, completion matrix, and
> the active `SESSION-XX-*.md`. Prerequisite checks use `dotnet build` (0 errors)
> and `dotnet test` (baseline). Blocking conditions include any required matrix
> row that is `UNKNOWN`/`FAIL`. Build/test commands run only when the active
> session file opens that gate (`CLAUDE.md` "Governance Startup").

## Authority Requirement Binding override
Authority-chain requirement binding applies before every session dispatch.
Requirements from Luba's current unquoted message, the v3.3 Beacon, the v3.3
state/matrix, the active session file, an architect work order, or an accepted
reviewer gate are binding unless that authority explicitly marks them optional,
exploratory, or preference-only. A downstream session must not narrow an upstream
requirement. Load `authority-requirement-binding-SKILL.md` before planning,
reviewing, issuing work orders, or executing a bounded work order.

## Core problem
A session that loads the wrong documents, runs the wrong governance chain, or
misses a blocking condition before starting will either do the wrong work or
produce invalid output. The session type (ARCHITECT, EXECUTOR, INVESTIGATOR,
MAINTAINER, DEBUG) determines which documents to load, which skills to run first,
and what blocking conditions prevent starting. This skill is the dispatch table.

## When to invoke
- At every session start, after the session mode is declared.
- Before loading any project documents (determine what to load first).

---

## Section 1 — Dispatch table

### EXECUTOR SESSION
Work target: implementing a phase/slice in a specific project.
```
PREREQUISITE CHECKS (only when the active session file opens the gate):
  → dotnet build Xiigen.Platform.slnx — must show 0 errors
  → dotnet test — record baseline (must be ≥ prior baseline)
  → If any test fails at session start: STOP. Fix before any implementation.

MINIMUM DOCUMENTS:
  1. v3.3 Beacon + AGENTS.md + CLAUDE.md           (every session)
  2. v3.3 state json + completion matrix           (current phase + required rows)
  3. The active SESSION-XX-*.md                     (what to implement)
  4. docs/DECISIONS-LOCKED.md                       (locked decisions)
  5. src/Xiigen.Kernel/* fabric contracts §relevant (interface shapes for this slice)

GOVERNANCE CHAIN (in order):
  1. authority-requirement-binding-SKILL.md         ← bind matrix rows BEFORE reading files
  2. phase-preflight-SKILL.md                       ← assumptions before code
  3. (per new .cs) Fabric-First + DataProcessResult<T> compliance check
  4. plan-execution-feedback-SKILL.md               ← report slice evidence to REVIEW
  5. verification-before-completion-SKILL.md        ← before claiming any slice done

BLOCKING CONDITIONS:
  - dotnet test failures > 0 at session start → STOP. Fix first.
  - A locked decision (or matrix row) contradicts the session plan → STOP.
    Route to decision-reopening-SKILL.md.
  - Required fabric contract missing from src/Xiigen.Kernel → CONTEXT_INSUFFICIENT.
  - The executor may NOT self-finish (CLAUDE.md): hand slice evidence to REVIEW_INTERNAL.
```

### ARCHITECT SESSION
Work target: designing phases, writing plans, reviewing architecture.
```
PREREQUISITE CHECKS:
  → dotnet build — 0 errors (plans cannot assume a broken baseline), if the session opens it
  → Architect sessions are rarely blocked; check only if a prior session left open issues

MINIMUM DOCUMENTS:
  1. v3.3 Beacon + AGENTS.md + CLAUDE.md
  2. docs/DECISIONS-LOCKED.md                       (load first — do not propose what's locked)
  3. v3.3 completion matrix                          (required-row status)
  4. Master-tenant launch runbook                    (if designing launch-related work)
  5. src/Xiigen.Kernel/* fabric contracts            (for interface design)

GOVERNANCE CHAIN (in order):
  1. planning-session-startup-SKILL.md               ← pre-plan questions/ledgers before first output
  2. authority-requirement-binding-SKILL.md          ← matrix rows → ledger rows
  3. plan-review-SKILL.md                            ← FC gates before any plan is final
  4. multi-reviewer-design-SKILL.md                  ← multi-perspective review on interface/architecture decisions
  5. documentation-sync-SKILL.md                     ← at session end

BLOCKING CONDITIONS:
  - Any proposal contradicts DECISIONS-LOCKED.md or a locked matrix row → STOP. Run decision-reopening-SKILL.md.
  - Plan has no Gate B for any phase → BLOCK (plan-review-SKILL.md).
```

### INVESTIGATOR SESSION
Work target: diagnosing gaps, tracing failures, simulation.
```
PREREQUISITE CHECKS:
  → None. Investigator sessions can start from any state.
  → Load the specific artifact being investigated first, not reference docs wholesale.

MINIMUM DOCUMENTS:
  1. The specific file being investigated            (targeted)
  2. docs/DECISIONS-LOCKED.md + relevant matrix row  (is the gap a known issue?)
  DO NOT load the runbook or all fabric contracts upfront — load sections when needed.

GOVERNANCE CHAIN:
  1. claim verification (claim-as-hypothesis)        ← verify claims before investigation
  2. root-cause-tracing-SKILL.md                     ← after a gap catalog is produced

BLOCKING CONDITIONS:
  - None. Investigation is always safe to start.
  - Any issue found: ISSUE INVENTORY entry required before session ends
    (FIXED / DEFERRED with authorization / EXCEPTION — not "noted").
```

### MAINTAINER SESSION
Work target: fixing files, updating skills, syncing documentation.
```
PREREQUISITE CHECKS:
  → Assess blast radius before any file changes.
  → If the change touches tests: dotnet test must pass after the change.

MINIMUM DOCUMENTS:
  1. docs/DECISIONS-LOCKED.md                        (what's locked)
  2. The specific file being updated                  (read before touching)
  DO NOT load reference docs unless the change requires interface verification.

GOVERNANCE CHAIN:
  1. blast-radius assessment                          ← FIRST, before any change
  2. change propagation                               ← derived views update in the same commit
  3. verification-before-completion-SKILL.md          ← before declaring any change done
  4. documentation-sync-SKILL.md                      ← at session end

BLOCKING CONDITIONS:
  - Any issue found: FIXED / DEFERRED (authorization + DECISIONS-LOCKED note) / EXCEPTION.
    "Noted" / "out of scope" are not valid statuses.
  - If tests touched: dotnet test must show 0 failures before session ends.
```

### DEBUG SESSION
Work target: a specific failing test, broken method, broken endpoint.
```
PREREQUISITE CHECKS:
  → None. Load the failing test output or error directly.
  → Do NOT load any document that doesn't contain the failing code.

MINIMUM DOCUMENTS:
  1. The failing test file or error output            (direct)
  2. The implementation file being debugged           (direct)
  Nothing else unless the root-cause trace leads there.

GOVERNANCE CHAIN:
  1. bug-to-tests (failing tests reproduce the bug first)
  2. root-cause-tracing-SKILL.md                      ← trace backward through the call chain
  3. three-level-verification (unit → integration → suite)
  4. verification-before-completion-SKILL.md          ← before declaring the bug fixed

BLOCKING CONDITIONS:
  - Every issue found: ISSUE INVENTORY entry (FIXED / DEFERRED / EXCEPTION — not "out of scope").
  - The fix must pass dotnet test with 0 failures before session ends.
```

---

## Section 2 — What this skill does NOT contain
```
✗ Bug-fix procedures — those live in the debug-chain skills
✗ Flow-specific decisions — those live in DECISIONS-LOCKED.md / the matrix
✗ Interface contracts — those live in src/Xiigen.Kernel/*
```
This skill is a dispatch table. It routes to the right skills and documents for
the session type; their content is authoritative, this is the lookup.

---

## Anti-patterns
1. **"Load all reference documents at session start regardless of type"** — an
   INVESTIGATOR that loads the runbook upfront is loading what it intends to investigate.
2. **"Start EXECUTOR work without binding the matrix rows"** —
   `authority-requirement-binding-SKILL.md` is the zeroth step for EXECUTOR/ARCHITECT.
3. **"MAINTAINER session ends without ISSUE INVENTORY"** — "noted" means the issue recurs next session.
4. **"ARCHITECT proposes something without loading DECISIONS-LOCKED.md / the matrix"** — a proposal that contradicts a locked row is wasted design work.

## Integration
- `authority-requirement-binding-SKILL.md` is the override loaded first for every type.
- `planning-session-startup-SKILL.md` produces this skill's output for ARCHITECT sessions.
- `verification-before-completion-SKILL.md` is the final step for all session types.
