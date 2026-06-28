# Diagnostic Escalation (XIIGen Enterprise Platform)

## Overview

When a capability fails its Gate B, there are only four possible root causes:
wrong test data, broken wiring, a stub, or an incorrect algorithm. Patching the
wrong layer wastes cycles — fixing a test when the wiring is broken, or rewriting
the algorithm when the test expectation was wrong. This skill defines a 4-step
diagnosis that identifies the root-cause layer before any fix, and distinguishes
fixes that require Luba's approval from those that proceed autonomously.

Stack-bound to **.NET 10 ASP.NET Core Web API**, **xUnit** (`tests/Xiigen.*.Tests`),
`DataProcessResult<T>` (`Xiigen.Kernel`), the platform fabric interfaces, and
ASP.NET Core DI in `Xiigen.Platform.Api/Program.cs`. Gate B is
`dotnet test Xiigen.Platform.slnx` producing the declared passing count (and
`dotnet build` with 0 warnings, since `TreatWarningsAsErrors=true`). The
"iron rule" layer (Step 4) is the platform governance rules from `AGENTS.md` /
`CLAUDE.md` / the v3.3 set, treated as named, testable checks. There is no
frontend: no Playwright/Angular diagnosis.

> This is the platform's 4-step Gate-B protocol. The adjacent
> `multi-reviewer-design`-style arbitration panel (the escalation *forum*) is a
> different concern; this skill is the TEST_DATA / WIRING / STUB / ALGORITHM
> diagnosis and does not duplicate that panel.

## When to invoke

- Any Gate B failure (`dotnet test` count below the declared threshold, or a build warning).
- A capability still failing the same category on a second cycle.
- A regression (count dropped since the last passing baseline).
- Before proposing any architectural change to a failing capability.

## Sub-Agent-Only Parent Boundary

Under a sub-agent-only / architect-only instruction, the diagnosis runs inside
the bounded sub-agent; the architect reviews the returned proposal. The parent
does not run the diagnosis commands as local sidecar work.
`sub-agent waiting is not a stop`.

---

## Section 1 — When to Fire

- **Situation 1 — single failure:** one failing test after a change; diagnosis
  runs, one fix proposed.
- **Situation 2 — repeated pattern:** the same category failed in 3+ cycles;
  diagnosis runs across all cycles, up to 3 ranked proposals.
- **Situation 3 — regression:** count dropped since the last passing baseline;
  the regression check runs first.

Does NOT fire when all Gate B conditions pass, or on a capability's very first
cycle (no regression possible — Situation 1 applies).

---

## Section 2 — The 4-Step Diagnosis Protocol

Run steps in order. STOP at the first step that identifies the layer. Do not run
all four and pick the most interesting — the first match is the root cause.

### Step 1 — Check test data (TEST_DATA)

> Is the test failing because of wrong input data, not wrong behavior?

```
Check:
  Are hardcoded Arrange values appropriate for this capability?
  Does the test exercise the declared scenario or a different one?
  Does the fixture (Arrange) match the contract — e.g. is the tenant context
  pushed, is the IDocumentStore seeded with UpsertAsync before the read?

If test data is wrong → layer TEST_DATA → fix the Arrange section → STOP.
Do NOT patch the implementation for a test-data error.
Else → Step 2.
```

### Step 2 — Check wiring (WIRING)

> Is the capability reachable by the test? Is the DI registration present?

```bash
# Is the fabric/service registered?
grep -n "IDocumentStore\|IOutboxStore\|IIdempotencyStore\|ISchedulerService\|AddXiigen" src/Xiigen.Platform.Api/Program.cs
dotnet build Xiigen.Platform.slnx   # 0 Error(s), 0 Warning(s)
```

```
If DI registration missing → WIRING → add it in Program.cs → STOP.
If a project reference is missing → WIRING → add it in the .csproj → STOP.
(An unreachable capability cannot produce correct output regardless of algorithm.)
Else → Step 3.
```

### Step 3 — Check implementation completeness (STUB)

> Does the implementation have real content, or is it a stub?

```bash
grep -rn "NotImplementedException\|throw new\|TODO\|FIXME" src/Xiigen.* --include="*.cs"
```

```
If NotImplementedException / empty method / canned Success(...) found → STUB →
  implement the method → STOP.
Else → Step 4.
```

### Step 4 — Check algorithm correctness (ALGORITHM / IRON_RULE)

> Is the implementation's logic producing the wrong result?

```
Evidence:
  What does the test expect (Assert)?
  What is the capability actually producing (DataProcessResult value / code)?
  Which named governance rule does the test verify?

If the algorithm contradicts a named iron rule, two sub-cases:
  A) The rule was derived correctly → ALGORITHM_ERROR: fix the implementation.
  B) The rule itself is wrong → IRON_RULE_ERROR: re-derive it (decision-reopening-SKILL.md).

Distinguish A from B — apply the rule quality test:
  □ Can it be violated without an exception?
  □ Is it stated without implementation detail?
  □ Does it have a testable failure consequence?
  □ Does it trace to a specific failure mode?
  ANY fail → Sub-case B (re-derive first). ALL pass → Sub-case A (fix code).

If regression (Situation 3):
  git diff against the last passing baseline; if a recent change introduced it,
  the regression fix has highest priority — revert/correct that change, not the
  algorithm generally.
```

---

## Section 3 — Proposal Format

```markdown
## Diagnostic Escalation Proposal — [CapabilityName]

Situation: [1 single / 2 pattern / 3 regression]
Failure layer: [TEST_DATA / WIRING / STUB / ALGORITHM_ERROR / IRON_RULE_ERROR]
Diagnosis step that identified it: [Step N]

Evidence:
  Test name: [exact failing test]
  Expected: [what Assert expects]
  Actual: [DataProcessResult value / code observed]
  Diagnosis evidence: [the exact grep/diff/build line that identified the layer]

Fix proposal:
  Type: [TEST_CORRECTION / WIRING_FIX / STUB_IMPLEMENTATION / ALGORITHM_FIX / RULE_REDERIVATION]
  Target: [exact src/Xiigen.* file:line]
  Change: [specific change — not "fix the bug"]

Expected after fix: dotnet test Xiigen.Platform.slnx --filter "[filter]" → [N] passed, 0 failed
Regression risk: LOW / MEDIUM / HIGH
  HIGH = touches a shared fabric interface (Xiigen.Kernel) or > 2 other test categories
```

---

## Section 4 — Escalation Triggers

Some failures must be escalated to Luba before any fix:

```
Escalate immediately when:
  □ The fix would change a DNA Rule (the platform DNA rules) — no DNA change without approval
  □ The fix would change a locked governance decision (AGENTS.md/CLAUDE.md/v3.3) — cannot override
  □ The fix would change a fabric interface (IDocumentStore/IOutboxStore/IIdempotencyStore/
    ISchedulerService/ITenantContextAccessor) — interface changes break all implementations
  □ The same failure persisted 3+ cycles with different fix attempts — architectural problem
  □ The regression check finds a change that was explicitly reviewed and approved — re-opening approved work

Do NOT escalate:
  □ TEST_DATA fixes (wrong Arrange values, unseeded store, unpushed tenant)
  □ WIRING fixes (missing DI registration / project reference)
  □ STUB implementations (filling a NotImplementedException)
  □ ALGORITHM_FIX affecting only the failing test's scenario
```

---

## Section 5 — Escalation Briefing Format

```markdown
## Diagnostic Escalation to Luba

WHY: [one sentence — specific reason for escalation]
TRAJECTORY: [cycle 1: failed category X / cycle 2: same / cycle 3: same]
BEST STATE: [last passing baseline and what changed since]

Gap analysis:
  Test: [name]
  Violation: [which iron rule or contract is violated]
  Root cause: [the structural issue, not the symptom]
  Likely fix: [what structural change resolves it]
  Fix type: [INTERFACE_CHANGE / DNA_RULE_CHANGE / LOCKED_DECISION_CHANGE]

Options:
  [A] [concrete action — cost, likely outcome]
  [B] [concrete alternative — different tradeoff]
  [C] Accept current state with acknowledged gap: [the known gap]
```

---

## Anti-Patterns

1. **Fixing the algorithm when the wiring is broken** — Steps 1–3 must pass
   before Step 4 is relevant.
2. **Patching the test to match wrong output** — distinguish TEST_DATA (wrong
   input) from ALGORITHM_ERROR (wrong output for correct input).
3. **Three cycles of the same failure, try a fourth** — 3 cycles without
   improvement is architectural. Escalate.
4. **"The iron rule must be wrong because the test fails"** — apply the rule
   quality test before deciding.
5. **Regression fix applied before reading the diff** — always read the diff first.

---

## Integration

- `systematic-debugging-SKILL.md` Phase 4 routes here when a fix fails Gate B.
- `root-cause-ladder-SKILL.md` consumes Step 4 architectural signals as Level 3.
- `decision-reopening-SKILL.md` runs the IRON_RULE_ERROR (Sub-case B) re-derivation.
- `dev-safety-SKILL.md` defines the Gate-B build/test discipline this skill checks.
- `multi-reviewer-design-SKILL.md` provides the escalation forum referenced in Section 5.
