# Score-Zero Investigation (XIIGen Enterprise Platform)

## Overview

A named check that produces a zero score — or consistently fires when it should
not — must be investigated before any resolution is applied. The most dangerous
mistake is removing the named check: it exists because a rule exists, and removing
it to clear the score creates a silent failure (code/governance that passes its
gate, compiles, and looks correct, but encodes a wrong practice). This skill
defines the three fix classes, the six-step investigation, and the silent-failure
detection that runs before any resolution.

Stack-bound to **.NET 10 ASP.NET Core Web API** and `tests/Xiigen.*.Tests` (xUnit).
**R5 boundary:** this platform is a Web API with no in-repo model training or DPO
— common/trainable models stay in `llm_mvp_core` and are consumed via
manifests/locators. So "score" here is NOT an ML metric. It is a **named
completion/governance check** — a required row of
`docs/plans/enterprise-adaptation/v3.3/XIIGEN-ENTERPRISE-ADAPTATION-COMPLETION-MATRIX-v3.3.md`,
a DNA-guard check, or a named test gate. The protocol is the universal one: a
named check is never deleted just to turn a red row green.

## When to invoke

- A required completion-matrix row scores zero / FAIL and the reflex is to delete the row.
- A DNA-guard check or named test gate consistently fires for 3+ sessions.
- A previously green gate suddenly drops to FAIL after a change.

## Sub-Agent-Only Parent Boundary

Under a sub-agent-only / architect-only instruction, the investigation runs inside
the bounded sub-agent; the architect reviews the resolution record before any
check is modified. The parent never deletes a check directly.
`sub-agent waiting is not a stop`.

---

## Rule 0 — Named Checks Are Never Removed

A named check (a matrix row id, a DNA-guard rule, a named gate) is never removed
until the investigation completes and a Class B or Class C resolution is explicitly
approved.

```
Investigation in progress → named check stays active
Class A resolved          → check stays (the gap was context, not the check)
Class B resolved          → check stays (trigger narrowed, not removed)
Class C resolved          → check may be modified (governance revision required)
```

The only Class C path that touches the check still requires a full governance
revision (`decision-reopening-SKILL.md` + the affected governance doc) before the
check is modified. Deletion is never the resolution.

---

## Section 1 — Three Fix Classes

### Class A — Context Addition

The actor did not know the rule. The check is correct; the work order / context
was missing the instruction.

```
Symptom:   Score = 0 the first time a phase hits this requirement,
           OR a new rule added after the work order was written.
Diagnosis: The actor cannot satisfy the check because the rule it references is
           not present in the active session file / work order / skill injection.
Fix:       Add the rule + a WRONG/CORRECT example to the work order or the
           governing skill (skill-injection-SKILL.md).
Named check stays: YES — the gap was in the context, not the check.
```

### Class B — Check Over-Fires

The check fires on cases that legitimately satisfy the underlying rule through a
different (correct) path.

```
Symptom:   Score = 0 but the artifact appears correct,
           OR fires only for a specific valid variant
           (e.g. a fabric registered as Scoped when the check expects Singleton,
            but Scoped is correct for that fabric).
Diagnosis: The trigger is too narrow — it matches only one valid form.
Fix:       Narrow the trigger: ADD an exception clause to the check.
           DO NOT remove the check — add the exception, e.g.
           "Fabric-lifetime check fires UNLESS the fabric is documented as
            request-scoped in the decision entry."
Named check stays: YES — with the exception clause.
```

### Class C — The Rule Itself Is Wrong

The rule encoded in the check is incorrect or has been superseded by a locked
governance decision.

```
Symptom:   The check fires on patterns explicitly permitted by AGENTS.md/CLAUDE.md/v3.3,
           OR the rule was written for an older version of the platform contract.
Diagnosis: The governance needs revision, not the code/artifact that the check grades.
Fix:       Full governance revision:
             1. Load the relevant governance doc — verify the conflict is real.
             2. Propose a decision-reopening-SKILL.md challenge.
             3. Update the rule with the correct constraint.
             4. Cross-validate against every phase/row that uses this check.
             5. Re-score all affected rows.
Named check stays: YES — modified, never deleted.
```

---

## Section 2 — Six-Step Investigation Protocol

```
Step 1: Identify which named check fired.
  Find the matrix row id / DNA rule / gate name and how many sessions it has fired.
  Record: CheckId, Phase/Capability, ObservationCount.

Step 2: Trace to the rule behind the check.
  Load the rule from the governing doc / skill.
  Confirm: is it a DNA rule, a locked governance decision, or a phase-specific gate?

Step 3: Locate the exact failing artifact.
  Re-run the check; identify the exact file/row/line that triggers it
  (e.g. grep the offending registration, read the failing xUnit assertion).

Step 4: Classify the fix.
  □ Is the rule present in the work order / session context? NO → Class A
  □ Does the artifact satisfy the rule through a different valid path? YES → Class B
  □ Does the governance explicitly permit the pattern? YES → Class C
  (If none: re-examine Step 2 — the rule trace may be wrong.)

Step 5: Silent-failure simulation (required for all Class B and Class C).
  Silent-failure definition: an artifact that passes its gate, builds, and looks
  correct — but encodes a wrong practice that later phases will copy.
  Simulation:
    Produce the artifact under the current check.
    Produce it under the proposed exception/modification.
    Compare: does the modified form let any wrong practice through?
    If yes: the exception is too broad — narrow it before applying.

Step 6: Resolution record.
  Record in the matrix / state:
  { "checkId": "...", "fixClass": "B", "session": "...",
    "resolutionApplied": "added request-scoped exception",
    "silentFailureSimulation": "PASSED",
    "checkModified": false, "checkDeleted": false }
```

---

## Section 3 — Silent-Failure Subtypes (platform)

```
Subtype 1: Passes the gate but wrong contract
  Example: a service returns void/throws where DataProcessResult<T> is required,
           but every test uses the throwing path so the gate stays green.
  Detection: assert the result type and .IsSuccess in the tests, not just "no exception".

Subtype 2: Correct structure but wrong semantics
  Example: DataProcessResult.Success(default) returned where a populated Data is required.
  Detection: tests assert .Data, not just .IsSuccess.

Subtype 3: Correct pattern but a configurable value hardcoded
  Example: a TTL / threshold hardcoded instead of read from IConfiguration / IFreedomConfigService.
  Detection: the named "no hardcoded config" check — do not delete it to pass.
```

---

## Anti-Patterns

1. **Removing the named check to clear the score** — the most destructive action.
   The check encodes a rule; deleting it lets every later phase repeat the wrong
   practice.
2. **Skipping the silent-failure simulation for Class B** — a too-broad exception
   lets wrong artifacts through.
3. **Treating a Class C (rule wrong) as a Class A (context missing)** — adding
   context to fix a governance error makes the context self-contradictory.
4. **Applying a Class B exception without cross-row impact check** — an exception
   in a shared check affects every row that uses it.
5. **Skipping the investigation because another row is green** — a single FAIL row
   blocks completion regardless of the others (completion-coverage rule).

---

## Integration

- `diagnostic-escalation-SKILL.md` Step 4 routes a structural failure here before patching.
- `decision-reopening-SKILL.md` is required for any Class C governance revision.
- `skill-injection-SKILL.md` carries the Class A context addition into work orders.
- `coverage-completeness-gate-SKILL.md` — a single FAIL row is not cleared by deleting it.
- `dna-compliance-guard-SKILL.md` provides the named DNA checks this skill protects.
