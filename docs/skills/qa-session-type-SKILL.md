# QA Session Type (XIIGen Enterprise Platform)

## Overview

A phase that declares itself complete may have passed Gate-B on the wrong
baseline, or delivered output that passes unit tests but fails cross-phase
integration. A QA session is a distinct mode that verifies completion claims
**independently** before phase progression is approved. It is **not** the same as
re-running `dotnet test` — it is a structured review of the claim, not just the
code.

This is the platform port of the `llm_mvp_core` skill of the same name. The
universal content is: three required inputs; the five-step protocol; a strictly
**binary verdict** (`APPROVED` | `DEFECTS_FOUND`, no "partial"/"conditional"); and
`BLOCKING` / `NON-BLOCKING` severity. It is bound to **.NET 10 / xUnit**,
`DataProcessResult<T>`, and the cross-project surfaces `Xiigen.Platform.Api` <->
`Xiigen.Kernel` <-> `Xiigen.Fabrics.*`.

> **Note-only (`na` here):** the core "Step 4 — Training data quality (DPO)" is
> **core-specific** — the trainable models live in `llm_mvp_core` (R5). This
> platform has no DPO triples to audit. The step is retained as a placeholder
> marked `na`; do not invent training-data evidence.

## When to invoke

- Optional for a single sequential phase (one developer, one project).
- Recommended/required for parallel phases (several projects completing together).
- When another reviewer or Luba questions a completion claim from a prior session.

Relationship to the role loop: the QA session is the `REVIEW_INTERNAL` evidence
that precedes an architect verdict. It does not authorize final reporting by
itself.

## Section 1 — QA session inputs

A QA session cannot start without all three:

1. **Phase completion package** — produced by `phase-completion-packager-SKILL.md`
   (the completion claim, changed files, Gate-B evidence).
2. **Acceptance criteria** — from `user-journey-testing-SKILL.md`
   `SCENARIO-MANIFEST.md`, or from the quality gates declared in the phase plan.
3. **Prior issue inventory** — any open issues from previous phases this phase was
   expected to close.

## Section 2 — Five-step QA protocol

### Step 1 — Verify completion claims (do not trust the package)

```bash
# Verify the test gate was actually zero failures (not just "passed"):
dotnet test tests/<Project>.Tests 2>&1 | tail -5
# Must show: Failed: 0

# Verify the state/matrix reflects actual completion (no stale next_action / UNKNOWN rows):
grep -n "stop_allowed\|goal_reached\|next_action" \
  docs/plans/enterprise-adaptation/v3.3/XIIGEN-ENTERPRISE-ADAPTATION-STATE-v3.3.json
```

### Step 2 — Run acceptance-criteria checks

For each acceptance criterion (each row of the `SCENARIO-MANIFEST`), run a command
that yields a **binary** result:

```bash
# Example: "Publish returns the published record for a valid request"
dotnet test --filter "FullyQualifiedName~PublishLegalDocument_ValidRequest"
# Expected: 1 passed, 0 failed

# Example: "Stale version is rejected with VERSION_CONFLICT"
dotnet test --filter "FullyQualifiedName~PublishLegalDocument_StaleVersion"
# Expected: 1 passed, 0 failed
```

A failing verification command is a DEFECT that must appear in the defect log.

### Step 3 — Cross-project integration path check

For phases whose output is consumed downstream (the `Xiigen.Platform.Api` <->
`Xiigen.Kernel` <-> `Xiigen.Fabrics.*` chain):

```bash
# Verify the interface the downstream consumer calls exists:
grep -rn "IDocumentStore\|IIdempotencyStore\|IOutboxStore\|ISchedulerService" \
  src/Xiigen.Kernel/FabricContracts.cs src/Xiigen.Fabrics.Scheduler/Scheduler.cs

# Verify the implementation is NOT a stub on a path Gate-B claims complete:
grep -rn "TODO\|NotImplementedException\|return default" \
  src/Xiigen.Fabrics.InMemory/ src/Xiigen.Platform.Marketplace/ --include="*.cs"
# Expected: 0 hits in production code on a "complete" path.
```

A `NotImplementedException` or `TODO` in a method that Gate-B marked complete is a
BLOCKING defect.

### Step 4 — Training-data quality (`na` on this platform)

```text
na: this platform produces no DPO triples; the trainable models live in
    llm_mvp_core (R5). Skip with this justification. Do not fabricate evidence.
```

### Step 5 — Produce the QA report

```markdown
# QA REPORT — [Project] Phase [P]
## Date: [date]
## Reviewer: [who ran the QA session — id/nickname]
## Verdict: APPROVED | DEFECTS_FOUND

## ACCEPTANCE CRITERIA RESULTS
| Criterion | Command | Result |
|---|---|---|
| [criterion 1] | dotnet test --filter "..." | PASS |
| [criterion 2] | dotnet test --filter "..." | FAIL — see DEFECTS |

## GATE / INTEGRATION VERIFICATION
- dotnet test (project): Failed: 0  [yes/no]
- Cross-project integration path: PASS | FAIL | N/A
- Training-data quality: N/A (na — no DPO on this platform)

## DEFECTS FOUND
| ID | Description | Severity | Blocking? |
|---|---|---|---|
| QA-001 | [description] | HIGH | YES |

## VERDICT: APPROVED | DEFECTS_FOUND
```

## Section 3 — Defect severity classification

**BLOCKING** — the phase may not progress until resolved:
- any `dotnet test` failure;
- a stub / `NotImplementedException` / `TODO` in a method Gate-B claims complete;
- a missing issue-inventory entry for a known open issue;
- an acceptance criterion that fails its verification command.

**NON-BLOCKING** — noted and tracked; may progress with explicit authorization:
- documentation gaps (a missing XML doc comment);
- test coverage below ideal but not zero;
- style issues with no behavioural impact;
- a single missing line-comment (warning-level DNA compliance).

## Section 4 — Relationship to Gate-B

The QA session does **not** replace Gate-B; it is a post-Gate-B review step.

```text
Phase runs -> Gate B passes (dotnet test green, acceptance tests green)
-> developer produces the PHASE-COMPLETE package
-> QA session runs (validates the package, not just the tests)
-> QA report -> APPROVED or DEFECTS_FOUND
-> REVIEW_INTERNAL feeds the architect verdict; only an ALLOW_FINAL verdict with
   stop_allowed=true and no UNKNOWN/FAIL matrix rows authorizes final reporting
```

## Anti-patterns

1. **"Gate B passed — same as QA approved"** — Gate-B is per-phase; QA checks
   cross-project integration. Different scope.
2. **"QA found a non-blocking defect — session failed"** — NON-BLOCKING defects
   are tracked, not progression-blockers; only BLOCKING defects block.
3. **"Verdict is PARTIAL / CONDITIONAL"** — the verdict is always `APPROVED` or
   `DEFECTS_FOUND`. No middle ground, no "approved if X is fixed next session".
   Fix X, re-run QA, get `APPROVED`.
4. **"Integration path check skipped — code looked fine"** — a downstream
   consumer calling a stubbed interface fails at runtime, not at unit-test time.

## Integration

- `phase-completion-packager-SKILL.md` produces the package that is QA's input.
- `user-journey-testing-SKILL.md` `SCENARIO-MANIFEST.md` supplies Step 2's
  acceptance criteria.
- `verification-before-completion-SKILL.md` governs the same "verify before
  claiming done" principle and the binary-verdict honesty.
