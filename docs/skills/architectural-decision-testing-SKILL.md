# Architectural Decision Testing (XIIGen Enterprise Platform)

## Overview

A planning session that proposes something contradicting a locked decision wastes
the session. A decision made once and never re-tested may have been right then but
wrong after 10 uses. This skill defines the decision-loading protocol (load before
proposing), the three testing timeframes (immediate / short-term / long-term), and
the invalidation procedure (negative knowledge is preserved, not deleted).

This is the platform port of the `llm_mvp_core` skill of the same name. The
universal content is: load locked decisions before any proposal; three horizons;
invalidation without deletion (status `INVALIDATED`, kept as negative knowledge).
It is bound to this repository's stack — **.NET 10**, `DataProcessResult<T>`,
fabric-first DI — and to the platform's locked-decision source.

**Locked-decision source on this platform** is the binding **v3.3 Enterprise
Adaptation** governance triple, not a core `DECISIONS-LOCKED.md`:

```text
docs/plans/enterprise-adaptation/v3.3/XIIGEN-ENTERPRISE-ADAPTATION-BEACON-PROMPT-v1.md
docs/plans/enterprise-adaptation/v3.3/XIIGEN-ENTERPRISE-ADAPTATION-STATE-v3.3.json
docs/plans/enterprise-adaptation/v3.3/XIIGEN-ENTERPRISE-ADAPTATION-COMPLETION-MATRIX-v3.3.md
```

Resolve decisions against these platform governance files (and the active
`SESSION-XX`). Do not resolve against `llm_mvp_core` decision names.

> **Note-only (`na` here):** the core skill's Section 5 Step 2 "seed the
> invalidation as a DPO triple" is **core-specific** — the common trainable models
> live in `llm_mvp_core` (R5). On this platform, record an invalidation as
> governance text (below); do **not** emit DPO triples here.

## When to load

- At the start of every architect/planning session — before any proposal.
- After implementing the 4th capability that uses a given decision (short-term).
- During any architectural review after 10+ capabilities (long-term).
- When a test reveals a decision was applied in the wrong context.

## Section 1 — Decision loading (before any proposal)

A proposal that contradicts a locked decision without knowing it exists creates
planning waste. Load first.

### Step 1 — Find locked decisions for this domain
```bash
# Read the platform governance triple and the active session file:
sed -n '1,200p' docs/plans/enterprise-adaptation/v3.3/XIIGEN-ENTERPRISE-ADAPTATION-COMPLETION-MATRIX-v3.3.md
grep -ri "scheduler\|outbox\|idempotency\|document store\|tenant\|fabric" \
  docs/plans/enterprise-adaptation/v3.3/
```

### Step 2 — Classify each decision

| Status | Definition | Rule |
|---|---|---|
| LOCKED | Implemented, tested, or explicitly closed in a prior session/matrix row | Requires a reopening challenge to change |
| OPEN | Stated as direction but not yet implemented or tested | Can be changed with evidence and rationale this session |
| PENDING | Flagged as an open question, not yet resolved | Must resolve before dependent work proceeds |

### Step 3 — Check every proposal against locked decisions

For every proposed solution: does it contradict a LOCKED decision? If yes, **STOP**
and produce a reopening challenge before proceeding:

```markdown
## DECISION_REOPENING Challenge

**Locked decision:** [exact text / matrix row]
**New evidence:** [what was found that warrants reconsideration]
**Proposed change:** [what would change if reopened]
**Blast radius:** [which capabilities, fabrics, and interfaces depend on this]
**Recommendation:** reopen | confirm locked | defer
```

Do not "work around" a locked decision.

## Section 2 — Immediate testing (same session as the decision)

Run right after the decision is made and the phase plan is written.

### For INTERFACE_INTRODUCTION decisions (most common on this platform):
```text
[ ] The new interface is defined in the kernel/contracts (e.g. src/Xiigen.Kernel/FabricContracts.cs)
[ ] The interface follows fabric-first: I-prefix, injectable, no concrete leakage
[ ] Consumers inject the interface, not a concrete type
[ ] Both the interface and at least one implementation (e.g. in Xiigen.Fabrics.InMemory) exist before any consumer
```

### For DEPENDENCY_DIRECTION decisions:
```text
[ ] No circular dependency introduced (A -> B and B -> A is a BLOCK)
[ ] Direction follows the project layering (Marketplace/Api -> Kernel -> Fabrics, not the reverse)
[ ] A consumer references the producer's interface, not its concrete type
```

### For ARCHITECTURE_PATTERN decisions (fabric-first / result-wrapper):
```text
[ ] Services return DataProcessResult<T> (no thrown business exceptions)
[ ] No concrete fabric is constructed inside a consumer (fabric-first, Rule 2)
```

## Section 3 — Short-term testing (after the first 3 uses)

Run during the 4th capability that touches the same decision.

### For INTERFACE_INTRODUCTION decisions:
```bash
# Did the interface stay stable across 3+ implementors?
grep -rn "IDocumentStore\|IIdempotencyStore\|IOutboxStore\|ISchedulerService\|ITenantContextAccessor" \
  src/ --include="*.cs" | grep -v "/tests/"
# Expected: multiple implementors/consumers, same interface shape.

# Are there concrete injections that bypass the interface (fabric-first violation, Rule 2)?
grep -rn "new InMemoryDocumentStore\|new InMemoryIdempotencyStore\|new .*Scheduler(" \
  src/Xiigen.Platform.Marketplace src/Xiigen.Platform.Api --include="*.cs"
# Expected: 0 hits in production code (DI provides the implementation).
```

A non-zero hit in `Xiigen.Platform.Marketplace` or `Xiigen.Platform.Api` means a
consumer constructs a concrete fabric in production — a fabric-first violation to
fix or to reopen the decision for.

### For DEPENDENCY_DIRECTION / ARCHITECTURE_PATTERN decisions:
```bash
# Did the pattern hold across 3+ uses? e.g. result-wrapper discipline:
grep -rn "throw new" src/ --include="*.cs"
# Expected: zero on business paths (use DataProcessResult<T>.Failure instead).
```

## Section 4 — Long-term testing (after 10+ capabilities)

Run during any architectural review.

```text
HEALTHY decision:
  [ ] Same problem solved consistently across 5+ capabilities
  [ ] No capability needed to reopen it to proceed
  [ ] It is referenced, not re-derived

WEAKENING decision:
  [ ] 2+ capabilities deviated with justification
  [ ] The deviation pattern is the same across deviations (it may be too rigid)
  [ ] Consider updating the decision to include the deviation as a valid case

FAILED decision:
  [ ] Most capabilities needed to work around it to succeed
  [ ] The problem it was meant to solve still recurs
  [ ] Action: invalidate and reopen (Section 5)
```

## Section 5 — Decision invalidation

If testing shows a decision was wrong:

**Step 1 — Record the invalidation in the platform governance text** (the matrix
or the relevant session decision log), keeping it with `INVALIDATED` status:

```markdown
## D-NN — [Decision Name] [INVALIDATED 2026-MM-DD]

**Status:** INVALIDATED
**Original resolution:** [what was decided]
**Invalidation evidence:** [what testing revealed — specific, not vague]
**Negative teaching point:** [what NOT to do in similar situations]
```

The invalidated decision **stays** in the document with `INVALIDATED` status.
Future sessions must know it was tried and found wrong. Negative knowledge is
valuable — do not delete it.

**Step 2 — (note-only / `na` here)** The core skill seeds a DPO triple at this
point. On this platform, do **not** — trainable models live in `llm_mvp_core`
(R5). Record the invalidation as governance text only.

**Step 3 — Reopen the decision space.** Design a replacement decision that
accounts for the invalidation evidence, through the platform role loop
(`PLAN_AUTHOR_INTERNAL -> PLAN_REVIEW_INTERNAL -> ARCHITECT_INTERNAL`).

## Anti-patterns

1. **Proposing without loading the governance triple first** — the proposal may
   contradict a locked matrix row already encoded in several capabilities.
2. **Short-term testing skipped because "it seems to work"** — the 3-use threshold
   exists because one use may be the happy path; run the check at the 4th use.
3. **Invalidated decision deleted to "clean up"** — negative knowledge is valuable;
   mark `INVALIDATED`, do not delete.
4. **Long-term testing skipped because no review is scheduled** — run the bash
   checks during any session that touches a capability using the decision.

## Integration

- The platform governance triple (BEACON / STATE-v3.3 / COMPLETION-MATRIX-v3.3) is
  the authoritative locked-decision source.
- `test-integrity-SKILL.md` enforces the fabric-first / no-thrown-business-error
  checks used in Sections 2-3.
- `verification-before-completion-SKILL.md` governs the same "verify before
  claiming" principle for any decision-health claim.
