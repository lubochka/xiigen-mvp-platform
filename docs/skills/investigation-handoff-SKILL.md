# Investigation Handoff (XIIGen Enterprise Platform)

## Overview

At the end of any analysis / comparison / complex-debug session, the discoveries
must survive into the next session. A handoff file records every discovered fact
with the exact command that verified it, the rejected claims, the questions that
need Luba's decision, and the numbers established. Discoveries are written
**during** the session, not only at the end — a session that compacts before the
handoff loses everything found in the last hour.

Stack-bound to **.NET 10 ASP.NET Core Web API**, **xUnit** (`tests/Xiigen.*.Tests`),
`DataProcessResult<T>`, and the platform fabric interfaces. `verified_by` is a
`dotnet test Xiigen.Platform.slnx` invocation or a `file:line` over `src/Xiigen.*`
— never `OperationResult<T>` references (that type is not used here). This handoff
feeds, and is fed by, the same governance `state`/beacon described in `AGENTS.md`
(Binding Enterprise Adaptation Beacon) — discoveries that change a contract or a
matrix row are also reflected there.

## When to invoke

At the end of any session that:
- Analyzed the codebase to understand current state.
- Compared two approaches or designs.
- Debugged a complex issue across multiple services/fabrics.
- Discovered facts not in the active state/matrix or docs.

Write to `docs/investigations/` AND reflect contract/matrix-affecting discoveries
in the governance state.

## Sub-Agent-Only Parent Boundary

Under a sub-agent-only / architect-only instruction, the investigating sub-agent
authors the handoff; the parent reads and integrates it after the packet returns.
`sub-agent waiting is not a stop`.

---

## Section 1 — Required Output Format

File: `docs/investigations/INVESTIGATION-HANDOFF-[topic]-[YYYY-MM-DD].md`

```markdown
# INVESTIGATION-HANDOFF — [topic] — [YYYY-MM-DD]
## Session type: [CODE | PLANNING | ANALYSIS | DEBUG]
## For: [next session / Luba]

## CONTEXT
[2-3 sentences: what was investigated, what approach, what the starting state was]

---

## DISCOVERIES

DISCOVERY-1:
  fact:        [precise factual statement — no hedging, no "probably"]
  verified_by: [exact command or file:line that confirmed it]
  implication: [what this means for any session touching this area]
  action:      RESOLVED: [what was done]
             | DEFERRED: [why and what triggers resolution]
             | OPEN: [what needs to happen next]

DISCOVERY-2: ...

---

## REJECTED CLAIMS

REJECTED-1:
  claim:       [exact wrong statement that was assumed]
  source:      [where the assumption came from]
  truth:       [what is actually true]
  verified_by: [command or file:line that disproved it]
  risk:        [what would have gone wrong if accepted]

---

## OPEN QUESTIONS REQUIRING LUBA'S DECISION

OPEN-1:
  question:  [precise question requiring Luba's decision]
  context:   [why this cannot be decided automatically]
  options:   [2-3 concrete options with tradeoffs]
  blocked:   [what cannot proceed until this is answered]

---

## NUMBERS ESTABLISHED THIS SESSION

| Artifact | Verified Value | Verified By |
|----------|---------------|-------------|
| Test count at session start | 312 | dotnet test Xiigen.Platform.slnx \| tail -3 |
| Test count at session end | 318 | dotnet test Xiigen.Platform.slnx \| tail -3 |
| src project count | 7 | find src -maxdepth 1 -mindepth 1 -type d \| wc -l |

---

## WHAT COMES NEXT

1. [task] — [CODE SESSION | PLANNING SESSION | ANALYSIS]
2. ...
```

---

## Section 2 — Write discoveries continuously

Record discoveries to the active state file (or a running `discoveries[]` ledger)
**during** the investigation, not only at the end:

```json
{
  "phase": "...",
  "status": "IN_PROGRESS",
  "discoveries": [
    {
      "foundAt": "step-3",
      "fact": "IDocumentStore.GetAsync returns Success(null) when the record is absent, NOT Failure",
      "verifiedBy": "Read src/Xiigen.Kernel/FabricContracts.cs:11-14",
      "implication": "Consumers must null-check Data even on IsSuccess; treating Success as 'found' is a bug",
      "actionTaken": "RESOLVED — guard added in the consuming service"
    }
  ],
  "rejectedClaims": [
    {
      "claim": "MicroserviceBase tolerates a null tenant context",
      "source": "assumption from reading the base class name",
      "truth": "MicroserviceBase.CurrentTenant throws InvalidOperationException when Current is null",
      "verifiedBy": "Read src/Xiigen.Kernel/MicroserviceBase.cs:27-28",
      "risk": "Would have relied on graceful degradation that does not exist"
    }
  ]
}
```

---

## Section 3 — Real Example

```
DISCOVERY-1:
  fact:        InMemorySchedulerService returns Success(existing, fromCache:true) for a duplicate idempotencyKey
  verified_by: Read src/Xiigen.Fabrics.Scheduler/Scheduler.cs:42-44
  implication: Re-scheduling with the same key is idempotent; callers must not treat a cached hit as a new job
  action:      RESOLVED — documented in the scheduler contract notes

REJECTED-1:
  claim:       "TreatWarningsAsErrors only affects CI, local builds tolerate warnings"
  source:      assumption from typical .NET defaults
  truth:       Directory.Build.props sets TreatWarningsAsErrors=true for all builds
  verified_by: Read Directory.Build.props
  risk:        Would have left a nullable warning unfixed, breaking every local build
```

---

## Anti-Patterns

1. **Writing the handoff only at session end** — compaction destroys earlier discoveries.
2. **"See above" as verified_by** — not a command; not reproducible.
3. **Discoveries without implications** — "test count is 318" with no implication is incomplete.
4. **Omitting rejected claims** — wrong assumptions resurface in future sessions.
5. **Vague facts** — "GetAsync might have issues with missing records" is not a discovery;
   "GetAsync returns Success(null) for a missing record" is.

---

## Integration

- `per-entity-examination-SKILL.md` produces the open-question list this handoff records.
- `debug-session-SKILL.md` Confirmed Facts become DISCOVERY entries at session end.
- `claim-verification-SKILL.md` verifies excerpts before they enter DISCOVERIES.
- `root-cause-ladder-SKILL.md` consumes OPEN questions as Level-2/Level-3 inputs.
