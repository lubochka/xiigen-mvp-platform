# self-extension-session-SKILL.md — Governed session type for closing a capability gap

> Ported from the `llm_mvp_core` governance standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build` / `dotnet test`, domain return type `DataProcessResult<T>`
> (`src/Xiigen.Kernel/DataProcessResult.cs`) — **not** core `OperationResult<T>`.
> Step 3 build = `dotnet build Xiigen.Platform.slnx` / `dotnet test`. **R5:** the
> common trainable model is consumed via manifest/locator, not built here, so the
> Step 5 training-data-gap audit is recorded **N/A** ("no local common-training
> signal to audit") with a one-line justification — the other steps stay full.
> Both ⛔ STOP gates (after the proposal, after closure+audit) are preserved.

## Core problem
When a capability gap is identified — a missing fabric interface, an absent
provider implementation, a named check that does not exist — the natural response
is to fill it immediately during the current session. That shortcut bypasses the
gates that prevent filling a gap with the wrong solution. A SELF-EXTENSION session
is a governed session type specifically for closing capability gaps: read the
current state, classify the gap, propose a resolution, require explicit approval
before building, verify closure, and (N/A here per R5) audit any training data.

## When to invoke
1. A capability-state read detects a MISSING capability that blocks a planned phase.
2. Gap analysis classifies a gap requiring a new interface, provider, or named check.
3. A phase discovers a missing dependency during infrastructure work.
4. A required matrix row cannot be satisfied because a capability is absent.

**Not initiated autonomously.** Detection is systematic; *initiating* a
SELF-EXTENSION session requires explicit direction from Luba.

---

## Section 1 — The governance chain

```
START

  Step 1: Capability-state read
    Read current capability state — what is ACTIVE, STUB, MISSING
    (fabric contracts in src/Xiigen.Kernel/*, DI registrations in Program.cs,
     matrix rows in the v3.3 completion matrix)

  Step 2: Gap → proposal
    Classify the gap (which layer is wrong)
    Produce a resolution proposal at Level 1–5 from solution-scope-gate-SKILL.md

  ⛔ STOP — present proposal — await Luba explicit "yes" before building

  Step 3: Execute build work
    Fabric interface only → no new session required; treat as MAINTENANCE
    New capability class required → use PLANNING session + EXECUTOR session
    IFreedomConfigService / appsettings update → MAINTENANCE
    Build gate: dotnet build Xiigen.Platform.slnx + dotnet test (when the session opens it)

  Step 4: Implementation integrity
    Verify closure: 4 checks
      (a) docs/DECISIONS-LOCKED.md + matrix updated
      (b) xUnit tests added and green (incl. WebApplicationFactory<Program> for an endpoint)
      (c) Fabric-First + DataProcessResult<T> + tenant-scope compliance on every touched .cs
      (d) no NEW gaps introduced (no new stub/TODO/NotImplementedException on the path)
    Produce a JSON integrity report

  Step 5: Training-data-gap audit — N/A in this repo (R5)
    Record: training_data_gap_audit = N/A
    Justification: common trainable models are consumed via manifest/locator, not
      built or trained in xiigen-platform; there is no local common-training
      signal whose gap window could contaminate DPO triples. If a tenant-specific
      ADAPTIVE leg is ever added locally, this step becomes required for it.

  ⛔ STOP — present integrity report + N/A audit note — await Luba sign-off

END
```

---

## Section 2 — Session type vs other types

| Session type | Scope | Gates |
|---|---|---|
| EXECUTOR | Write code for a known plan | Fabric/DataProcessResult check, Gate B per phase |
| PLANNING | Design new capability | Plan gates, self-questioning |
| ARCHITECT | Structural analysis and synthesis | Reconnaissance gate, convergence |
| REVIEW | Audit a plan or code | Plan review, multi-reviewer |
| DEBUG | Fix a confirmed bug | Failing tests first |
| **SELF-EXTENSION** | **Close a capability gap** | **Steps 1→2→STOP→3→4→5(N/A)→STOP** |

The critical distinction from MAINTENANCE: MAINTENANCE applies when the fix is
known and scoped. SELF-EXTENSION applies when the gap is identified but the
resolution is unknown — the proposal step determines it. Treating SELF-EXTENSION
as MAINTENANCE skips the proposal step and the first STOP gate.

---

## Section 3 — What SELF-EXTENSION is not
```
❌ Building a new fabric interface mid-EXECUTOR session without a proposal
   → Stop the executor session. Write the gap. Start a SELF-EXTENSION session.

❌ Adding a named check to a handler without checking if it changes existing behaviour
   → Step 4 integrity must verify no regression (re-run dotnet test).

❌ Treating all MISSING capabilities as gaps requiring SELF-EXTENSION
   → The trigger is "blocks a phase / a required matrix row", not "appears in the MISSING list".

❌ Closing a SELF-EXTENSION session without recording Step 5
   → Even though Step 5 is N/A here, record it explicitly with the R5 justification.
```

---

## Anti-patterns
1. **"I'll fix it inline and note it at the end"** — inline fixes bypass the STOP
   gate and Luba approval; the gate exists to prevent wrong-level fixes (new
   interface where a config change sufficed).
2. **"The gap is small — skip the proposal"** — the proposal step assigns the
   correct scope level; skipping it is how a Level-1 fix becomes a Level-5 one.
3. **"SELF-EXTENSION doesn't need reconnaissance"** — read the current capability
   state (Step 1) before proposing (Step 2). A proposal without evidence is an essay.

## Integration
- `solution-scope-gate-SKILL.md` governs the proposal level (Level 1–5) in Step 2.
- `decision-capture-SKILL.md` records the approved resolution as a D-NN entry.
- `verification-before-completion-SKILL.md` backs the Step 4 closure verification.
- `authority-requirement-binding-SKILL.md` binds the matrix row the gap blocks.
