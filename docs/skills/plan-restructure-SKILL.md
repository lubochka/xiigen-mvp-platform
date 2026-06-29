# Plan Restructure

> Ported from the `llm_mvp_core` planning standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build` / `dotnet test`. Use this when restructuring an **active v3.3
> session file** after the state/matrix changes
> (`docs/plans/enterprise-adaptation/v3.3/...`). List 1 / List 2 fix that the
> v3.3 Beacon and the PLAN→REVIEW_INTERNAL→ARCHITECT_INTERNAL_REPAIR→
> EXECUTOR_INTERNAL_CONTINUE loop stay **unchanged**.

## Core Problem

A plan written before new infrastructure (new fabric contracts, a new endpoint
group, a new gate) was completed can have correct phase content but still be
wrong: it does not reference the new infrastructure, its phases are too large for
one session, and it does not include the new gates. Rewriting the entire plan
introduces errors into things that were correct. This skill defines the **delta
analysis** that preserves correct content while adding only what changed.

**Do NOT use for a capability that has no existing plan.** For new capabilities,
plan from scratch (start at `intent-to-plan-SKILL.md` + `phase-planning-SKILL.md`).

## When to Invoke

- An existing plan/session file was written before new shared infrastructure (a
  new `IDocumentStore`/`IOutboxStore` method, a new minimal-API route group, a new
  validation gate) became available.
- A phase is too large for a single session (context-loss risk).
- A plan's gate checks do not include new gates that now apply.
- The v3.3 state/matrix advanced and the active session file must catch up.

## Section 1 — Read Before Writing

Read the existing plan completely **as if executing it** before writing anything.
While reading, annotate each section:
```
□ Is this still correct given the new infrastructure?
□ Does this assume a fabric/route/DI shape that changed?
□ Is there a new gate/check that should be here?
□ Does this produce output a later phase expects?
```
Do not start writing until you have read the complete plan. The most common
restructure error is editing an early phase before reading what a later phase
expects.

## Section 2 — The Delta Analysis

Write two explicit lists before touching any session file.

### List 1: What does NOT change
Be explicit. Name each thing:
```
UNCHANGED FROM EXISTING PLAN:
- v3.3 Beacon-first rule and the role loop (PLAN_AUTHOR_INTERNAL → PLAN_REVIEW_INTERNAL → ARCHITECT_INTERNAL → EXECUTOR_INTERNAL → REVIEW_INTERNAL → ARCHITECT_INTERNAL_REPAIR → EXECUTOR_INTERNAL_CONTINUE)
- The soft-stop ban (commit done / report written / one child passed is not a stop)
- DataProcessResult<T> as the domain return type (no OperationResult<T>)
- Existing endpoint contracts and their Results.* shapes that are not in scope
- The xUnit zero-regression rule at every gate
- "Executor cannot stop the plan"
```
This list matters: a session executing the file must not re-examine these things.
Without the explicit "unchanged" list, correct content gets "improved" into
incorrect content.

### List 2: What changes and exactly where
For each change record: which phase, whether it is a new step / modified gate /
new output, what new infrastructure drives it, and whether it is additive or a
replacement.
```
PHASE A — New step A0: fabric-signature pre-scan (additive)
  Driven by: new IDocumentStore overload
  Position: before existing A1
  Action: verify the new overload exists (delegate to phase-preflight-SKILL.md)

PHASE B — Modified B-gate: add Gate-B for the new route (additive to the gate)
  Driven by: new minimal-API route group
  Position: at end of Phase B gate
  Action: dotnet test --filter "[NewRoute]" → N pass, 0 fail
  On FAIL: phase completion BLOCKED
```

## Section 3 — Session Split Criteria
A phase needs its own session file when:
- **Context-loss risk:** losing context mid-phase would make correct resumption impossible.
- **Decision-tree depth:** the phase branches into significantly different execution paths.
- **Cognitive load:** executing it needs more in working memory than a fresh session can hold.
- **Companion-document requirement:** it produces a hand-off the next session needs; it needs a clean STATE update + completion package + STOP gate that is easy to locate.

## Section 4 — Document Architecture
Before writing any session file, write a short document-architecture block
naming each file, its purpose, new-vs-existing content, and where it ends (STATE
update + completion package + STOP).

## Section 5 — Unchanged List Hard Rule
Without the explicit UNCHANGED list, a session executing the restructured plan
will re-examine and potentially modify correct content. Write the list. Name each
thing. "Most things are unchanged" is not a list.

## Anti-Patterns
1. **Rewriting the entire plan** — preserve unchanged content verbatim; rewrites introduce errors.
2. **Session files that don't cold-start** — every session file begins: read STATE, verify previous phase complete, load the v3.3 Beacon chain.
3. **"See previous session" hand-off** — the brief must contain the key facts, not a pointer; cross-session context does not flow automatically.
4. **Combining phases with context-loss risk** — split them.
5. **Not writing the unchanged list** — write it; name each thing.

## Integration
- New capabilities: `intent-to-plan-SKILL.md` + `phase-planning-SKILL.md`.
- This skill restructures existing plans (adding new infrastructure) without breaking the v3.3 governance loop.
- `plan-execution-feedback-SKILL.md` log/feedback format applies unchanged across the restructured session files.
