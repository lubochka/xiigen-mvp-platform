# Root Cause Ladder (XIIGen Enterprise Platform)

## Overview

Every gap has three explanations at different levels: the immediate fix, the
structural practice that let it recur, and the architectural decision that made
it possible. A session that applies only the immediate fix sees the same gap
re-emerge; one that addresses the wrong level over-engineers a symptom or
under-solves a deep pattern. This skill defines the three-level structure, the
cross-gap convergence protocol, and the rejection-response protocol.

Stack-bound to **.NET 10 ASP.NET Core Web API**, `tests/Xiigen.*.Tests` (xUnit),
and the platform governance artifacts. On this platform the three levels map to:
- **Level 1** — a code/config fix in `src/Xiigen.*`.
- **Level 2** — a new or updated skill file in `docs/skills/`.
- **Level 3** — an architectural decision. Today these decisions live as prose in
  `AGENTS.md` / `CLAUDE.md` (role loop, No-Secrets, Enterprise Scope) and in the
  v3.3 governance set referenced by the beacon. When you record a Level-3 fix,
  promote the relevant prose into an explicit decision entry and track its
  closure as a required row of the completion matrix
  (`docs/plans/enterprise-adaptation/v3.3/XIIGEN-ENTERPRISE-ADAPTATION-COMPLETION-MATRIX-v3.3.md`).

## When to invoke

- After a Gate B fails — before deciding what to fix.
- When the same category of error appears in more than one phase/session.
- When a prior remediation was applied but the problem recurred.
- When `diagnostic-escalation-SKILL.md` Step 4 produces an architectural signal.

## Sub-Agent-Only Parent Boundary

Under a sub-agent-only / architect-only instruction, the architect classifies
gaps at the three levels and issues bounded work orders for the fixes; the
parent does not author the fixes directly. `sub-agent waiting is not a stop`.

---

## Section 1 — The Three-Level Structure

Every gap is classified at all three levels before any fix is written.

### Level 1 — Immediate Fix

```
What failed:   [exact method or assertion that failed]
Why it failed: [proximate cause — the concrete missing/wrong thing]
Fix:           [file:line in src/Xiigen.* + what to change]
Test:          [the xUnit test that is green after this fix and not before]
```

Necessary but not sufficient. Level-1-only means the gap recurs next phase.

### Level 2 — Structural Fix

The skill, rule, or practice that, if present, would have caught Level 1 before
it reached Gate B.

```
Missing practice: [what should have been checked but wasn't]
Skill or rule:    [which docs/skills file — or "NEW SKILL NEEDED"]
Action:           [update skill / add a named iron rule / add a preflight check]
```

Level 2 examples on this platform:
- "`dna-compliance-guard-SKILL.md` Rule 2 (fabric via interface) was not run before the service was written."
- "No named rule for 'a swallowed `DataProcessResult.Failure` must propagate via `MicroserviceBase`'."
- "`code-examination-SKILL.md` Step 2 (map callers/DI) was skipped."

### Level 3 — Architectural Decision

The design decision or document that, if correct, would have prevented the Level-2
gap.

```
Root cause:  [missing/incorrect design decision or artifact]
Document:    [AGENTS.md / CLAUDE.md prose to promote, or a v3.3 governance doc]
Matrix row:  [the completion-matrix row that tracks closure]
Action:      [write the explicit decision entry / update the contract / new pattern]
```

Level 3 examples on this platform:
- "No explicit decision defining when a fabric is registered `Singleton` vs `Scoped`."
- "The contract for `IDocumentStore.GetAsync` returning `Success(null)` (found nothing) vs `Failure` is implicit, not written."
- "Role loop ordering (`PLAN → REVIEW → ARCHITECT → EXECUTOR`) exists as prose only, with no matrix row enforcing executor-cannot-self-finish per phase."

---

## Section 2 — Root Cause Record Format

```markdown
### Root Cause Record — [Gap Name]

**Level 1 — Immediate Fix**
- Failed:   [method/assertion]
- Why:      [proximate cause]
- Fix:      [src/Xiigen.* file:line change]
- Test:     [xUnit test name that verifies the fix]

**Level 2 — Structural Fix**
- Gap:      [missing practice or skill]
- Action:   [docs/skills update / new iron rule / preflight check]

**Level 3 — Architectural Decision**
- Root:     [missing/incorrect design artifact]
- Document: [AGENTS.md/CLAUDE.md prose to promote, or v3.3 doc]
- Matrix:   [completion-matrix row that records closure]
- Action:   [explicit decision entry / contract update]
```

Do not start Level-1 fix work until the record is complete. A record missing
Level 3 means the architectural root was not identified.

---

## Section 3 — Cross-Gap Convergence Protocol

When multiple gaps appear in one session, check for a shared Level-3 root before
fixing each individually.

```
Step 1 — List all gaps (Level 3 only):
  Gap 1: [Level 3 root]
  Gap 2: [Level 3 root]
  Gap 3: [Level 3 root]

Step 2 — Group by shared root:
  If Gap 1 and Gap 3 share a Level 3 root → one Level 3 fix closes both.
  Priority: the Level 3 fix first, then the Level 1 fixes for both.

Step 3 — Report shared-root count:
  "3 gaps. 2 share a Level 3 root: implicit fabric-lifetime decision. One decision
   entry + matrix row closes gaps 1 and 3. Gap 2 has an independent Level 3 root."
```

A shared Level-3 root is always fixed before the individual Level-1 fixes.

---

## Section 4 — Rejection Response Protocol

When a proposed fix is challenged (by a reviewer or by Luba):

```
Step 1: State which level the challenger is operating at.
Step 2: Agree or disagree at each level explicitly.
Step 3: If the challenge is correct — restate the fix at the right level.
Step 4: If incorrect — show the Level 3 root-cause record and explain why it
        confirms the current level.

NEVER abandon a valid Level 2 or Level 3 fix because it "feels over-engineered"
for the immediate symptom. The symptom is Level 1. Structural and architectural
fixes operate at different scopes.
```

Example:

```
Challenge: "This is a one-line fix — adding a decision entry is overkill."
Response:
  Level 1 fix (one line): correct — I will apply it.
  Level 3 fix (decision entry + matrix row): also required.
  Reason: this is the second session where the GetAsync-null contract was
  ambiguous. Promoting it to an explicit decision costs ~5 lines; not doing it
  costs a future re-investigation.
```

---

## Section 5 — Escalation to DESIGN_FLAW

When Level-3 analysis produces a root that contradicts an existing locked
decision (in `AGENTS.md`/`CLAUDE.md` or the v3.3 governance set), escalate via
`decision-reopening-SKILL.md` rather than writing a competing Level-3 fix.

```
Signal: DESIGN_FLAW
Trigger: Level 3 root contradicts a locked governance decision

Action:
  1. Apply the Level 1 fix only (stops the bleed).
  2. Do NOT apply the Level 3 fix.
  3. Emit: "DESIGN_FLAW: Level 3 root [X] contradicts locked decision [Y].
           Routing to decision-reopening-SKILL.md."
  4. decision-reopening-SKILL.md runs its test before any change.
```

---

## Anti-Patterns

1. **"Level 1 fix applied — issue resolved"** — Level 1 stops the bleeding; an
   unfixed Level 2/3 root produces the same symptom next phase.
2. **"Level 3 analysis skipped — too slow"** — Level 3 is a few lines; a missing
   decision that forces a future re-investigation is a full session.
3. **"Rejected the Level 3 fix to avoid scope creep"** — writing a decision entry
   is documentation of a decision already made, not a feature.
4. **"Each gap fixed independently without a convergence check"** — shared roots
   left unclosed reproduce the same cluster of gaps next session.

---

## Integration

- `diagnostic-escalation-SKILL.md` produces architectural escalation signals → Level 3.
- `decision-reopening-SKILL.md` handles Level-3 roots that contradict locked decisions.
- `remediation-session-design-SKILL.md` consumes this skill's K root causes to order fix sessions.
- `solution-scope-gate-SKILL.md` classifies the fix class per gap.
- `planning-session-startup-SKILL.md` surfaces Level-3 roots carried from prior sessions.
