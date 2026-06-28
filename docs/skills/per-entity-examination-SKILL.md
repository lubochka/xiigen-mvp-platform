# Per-Entity Examination (XIIGen Enterprise Platform)

## Overview

A session that examines 3 of 14 projects and synthesizes architectural
conclusions has a ~21% evidence base producing a 100% claim scope. The correction
arrives downstream and forces re-examination — but the wrong synthesis is already
in the plan. This skill installs the 7-step protocol that makes multi-entity
examination rigorous: declare hypotheses before examining instance 1, examine all
N in a fixed sequence, synthesize only after all N are complete.

Stack-bound to **.NET 10 ASP.NET Core Web API**, **xUnit** (`tests/Xiigen.*.Tests`),
`DataProcessResult<T>`, and the platform fabric interfaces. Entity types this
protocol governs here: the `src/Xiigen.*` projects (`Xiigen.Auth`,
`Xiigen.Fabrics.InMemory`, `Xiigen.Fabrics.Scheduler`, `Xiigen.Freedom`,
`Xiigen.Kernel`, `Xiigen.Platform.Api`, `Xiigen.Platform.Marketplace`), the
matching `tests/Xiigen.*.Tests` projects, the fabric interfaces
(`IDocumentStore` / `IOutboxStore` / `IIdempotencyStore` / `ISchedulerService` /
`ITenantContextAccessor`), and the marketplace services. N is counted with
`ls`/`find`, not estimated. Citations are `file:line` over `*.cs`. There is no
frontend: no UI/Angular entities.

## When to invoke

- Any architecture / planning / investigation session examining N≥5 instances of
  the same entity type to answer a system-level question (e.g. "do all services
  return `DataProcessResult<T>` and inherit `MicroserviceBase`?", "is every fabric
  injected via interface in every project?").

## Sub-Agent-Only Parent Boundary

Under a sub-agent-only / architect-only instruction, the examination (the per-
instance reads and tally) is the bounded sub-agent's job; the architect runs the
separate synthesis after the Freeze Gate passes. The parent does not read all N
instances as local sidecar work. `sub-agent waiting is not a stop`.

---

## Section 1 — The 7-Step Protocol

### Step 1 — Declare entity type and universe size (N)

```
Entity type: [src projects | test projects | fabric interfaces | services]
Total universe (N): [integer] — source: [exact command, e.g. ls src/ | wc -l]
```

N is counted, not estimated. Record the command.

```bash
# Example: count src projects
find src -maxdepth 1 -mindepth 1 -type d | wc -l
```

### Step 2 — Declare hypotheses (min 1, max 8) before instance 1

```
H-[N] — [short name]
Claim:        [one sentence about the entity type as a whole]
Confirms if:  [specific observable if H-N holds]
Refutes if:   [specific observable if H-N fails]
Scope:        [the architectural question this tests, or "cross-cutting"]
```

Platform examples:
- "H-1 result-type: every public service method returns `DataProcessResult<T>` (no void/bool/throw for business logic)."
- "H-2 base-class: every platform service extends `MicroserviceBase<TService>`."
- "H-3 fabric-di: every fabric dependency is injected via its `Xiigen.Kernel` interface, never `new`-ed."
- "H-4 tests-present: every `src/Xiigen.*` project has a matching `tests/Xiigen.*.Tests`."

**Why before instance 1:** hypotheses formed after reading the first instances
carry confirmation bias. Pre-declaration removes the feedback loop. **Max 8**:
more makes per-instance records too long to fill consistently.

### Step 3 — Define the per-instance step sequence (max 5 steps)

```
Step 0 — Inventory: list the project's .cs files without reading
Step 1 — [name]: what to read, which hypotheses it tests
Step 2 — [name]: ...
Step N — Verdict: how H verdicts are assembled
```

Max 5 ensures the procedure fits without consuming a full context window per
instance. The same steps apply to every instance in the same order.

### Step 4 — Define the result-record format before instance 1

No field may be added mid-examination.

```
ENTITY_ID:   [project / interface / service id]
TITLE:       [human-readable name]

STEP_0_INVENTORY:
[inventory items — YES / NO / [count]]

H_VERDICTS:
H-1 (result-type): [CONSISTENT | INCONSISTENT | PARTIAL | NOT_TESTABLE] — [file:line evidence]
H-2 (base-class):  [...] — [evidence]
[... one row per hypothesis]

OPEN_QUESTIONS:
Q[N]: [question] — raised by: [specific finding]
```

Verdict vocabulary — four values only:

```
CONSISTENT    Evidence supports the hypothesis. Cite file:line.
INCONSISTENT  Evidence contradicts it. Cite the finding.
PARTIAL       Holds for part of this instance. Name which part holds/does not.
NOT_TESTABLE  No implemented code/docs to test it. NOT_TESTABLE is a finding, not a shortcut.
```

### Step 5 — Examine all N instances in the declared sequence

No instance is skipped or gets a reduced procedure. `NOT_APPLICABLE` (step has no
material) and `NOT_TESTABLE` (hypothesis untestable here) are valid findings; an
instance with these is still a complete instance.

### Step 6 — Running tally after every 10 instances (or at the end if N<10)

```
RUNNING TALLY — after instance [N]
H-1 (result-type): CONSISTENT: [n]  INCONSISTENT: [n]  PARTIAL: [n]  NOT_TESTABLE: [n]
H-2 (base-class):  CONSISTENT: [n]  INCONSISTENT: [n]  PARTIAL: [n]  NOT_TESTABLE: [n]
No conclusions drawn. Tally only.
```

### Step 7 — Synthesis only after all N are complete

The examining session produces result records, running tally, and the open-question
list — no architectural conclusions. Synthesis is a separate session after the
Freeze Gate passes.

---

## Section 2 — Freeze Gate

Examination is done when all three hold simultaneously:

```
Condition 1 — Completeness
  Every instance has a complete result record (NOT_APPLICABLE / NOT_TESTABLE are
  populated values, not omissions). Count records and fully-populated instances;
  both must equal N.

Condition 2 — Convergence
  The second half (N/2+1 … N) produced no finding CLASS absent from the first half.
  A finding class is a structural pattern type, not an instance.
  "A service returns void instead of DataProcessResult<T>" is a finding class.
  "Xiigen.Auth.LoginService does it" is an instance of that class.
  If Condition 2 fails: add H-N+1; re-examine from the first instance where it
  appeared; re-evaluate earlier instances for H-N+1.

Condition 3 — Implementation Evidence
  At least one finding per non-NOT_TESTABLE hypothesis cites an IMPLEMENTATION or
  TEST excerpt (file:line in *.cs). If it fails: produce a HANDOFF naming the exact
  grep that would confirm/deny each unverified hypothesis. The examination is
  FROZEN (a named state, not "going in circles") until the HANDOFF returns.
```

**Examination complete** = all three met. **Synthesis authorized** when complete.

---

## Section 3 — Examiner vs Synthesis Session

| Role | Produces | Does NOT produce |
|------|----------|-----------------|
| Examiner | Result records, running tally, open-question list | Architectural conclusions, design proposals |
| Synthesis | Verdict matrix, recommendations from the tally | Per-instance re-examination, new instance reads |

---

## Anti-Patterns

1. **"Pattern is obvious after 5 instances — synthesize now"** — early-instance
   patterns fail in the later instances; that is why the protocol exists.
2. **"NOT_TESTABLE is a shortcut"** — it is a finding about that instance's state.
3. **"I'll define the result format as I go"** — mid-examination format changes
   produce records that cannot be aggregated.
4. **"The synthesis session can re-examine instances"** — no; the examining
   session finishes incomplete records.
5. **"I'll declare hypotheses after I see the first few"** — post-hoc hypotheses
   carry the bias of the first instances.

---

## Integration

- `code-examination-SKILL.md` provides the per-file reading protocol used in Steps 1–5.
- `investigation-handoff-SKILL.md` formats the HANDOFF when Condition 3 fails.
- `solution-scope-gate-SKILL.md` classifies the fix class for each OPEN_QUESTION.
- `root-cause-ladder-SKILL.md` turns a recurring finding class into Level-2/Level-3 fixes.
