# Context Overflow (XIIGen Enterprise Platform)

## Overview

When a session runs out of context, in-progress work may be lost. This skill
governs what to do at three threshold levels so nothing important is lost.

Platform port of the `llm_mvp_core` skill. Stack-bound to **.NET 10 ASP.NET Core
Web API**, **xUnit** (`tests/`), `Xiigen.Platform.slnx`, and the v3.3 governance
triple. Resume and emergency-stop commands are native (`dotnet build`/`dotnet
test Xiigen.Platform.slnx`); the paused status is written to
`XIIGEN-ENTERPRISE-ADAPTATION-STATE-v3.3.json`.

---

## The Three Thresholds

### Threshold 1: > 35% Context Remaining — Continue Normally

Continue executing the plan. No special action required.

### Threshold 2: <= 35% Context — Wrap-Up Mode

Switch to wrap-up mode:

```
1. Finish the current step completely (do not start a new step)
2. Save XIIGEN-ENTERPRISE-ADAPTATION-STATE-v3.3.json with status: "PAUSED_CONTEXT_LIMIT"
3. Record exactly which step was in progress
4. Run `dotnet test Xiigen.Platform.slnx` and record the current count
5. Write the "Resume From" note (see format below)
6. Commit whatever is complete (even partial phase progress) to the current branch
7. Do NOT start the next step
```

Resume From note format:
```
RESUME FROM: [timestamp]
  Last completed step: STEP N — [description]
  Current test count: [N] passed, 0 failed
  Next step: STEP N+1 — [description, file, Gate B]
  In-progress work: [describe anything done but not committed]
  Blockers: [any unresolved issues]
```

### Threshold 3: <= 25% Context — Emergency Stop

Stop immediately. Save and commit regardless of step completion status.

```
1. Stop mid-step if necessary
2. Save XIIGEN-ENTERPRISE-ADAPTATION-STATE-v3.3.json:
   {
     "status": "INCOMPLETE_CONTEXT_OVERFLOW",
     "pausedAt": "[timestamp]",
     "lastCompletedStep": "STEP N",
     "partialWork": "[describe what was done but not finished]",
     "testCount": "[N] from last run",
     "nextStep": "STEP N+1"
   }
3. git add [all changed files]
4. git commit -m "WIP: context limit — SESSION-XX partial, step N+1 pending"
5. git push to the current working branch (never main/master unless the active
   session explicitly owns that boundary)
6. Report to Luba with the state file path
```

The commit at wrap-up is a checkpoint, not a stop of the plan
(`CLAUDE.md` "Executor Cannot Stop The Plan"). The next session resumes.

---

## How to Estimate Context Remaining

The session has no direct token counter visible. Use these proxies:

| Signal | Likely Threshold |
|--------|-----------------|
| Can still see early session messages in context | > 35% |
| System reminder shows compressed earlier messages | ~ 35% |
| Large blocks of conversation appear summarized | <= 25% |
| Responses feel slower or shorter | Possible limit approaching |

When uncertain: assume lower estimate and wrap up.

---

## State File at Context Wrap-Up

```json
{
  "phaseId": "SESSION-06",
  "status": "PAUSED_CONTEXT_LIMIT",
  "pausedAt": "2026-06-01T14:30:00Z",
  "testBaselineAtStart": 181,
  "testCountNow": 199,
  "completedDeliverables": [
    "src/Xiigen.Fabrics.InMemory/InMemoryIdempotencyStore.cs",
    "tests/Xiigen.Fabrics.InMemory/IdempotencyStoreTests.cs"
  ],
  "lastCompletedStep": "STEP 2 — idempotency reserve tests",
  "nextStep": "STEP 3 — mark-succeeded/mark-failed path",
  "partialWork": "none",
  "blockers": "none"
}
```

---

## Session Resumption Protocol

At the start of the next session, when resuming from context wrap-up:

```
1. git branch --show-current -> confirm on the working branch the wrap-up used
2. dotnet build Xiigen.Platform.slnx -> 0 errors
3. dotnet test Xiigen.Platform.slnx --no-build -> record new baseline
4. Read XIIGEN-ENTERPRISE-ADAPTATION-STATE-v3.3.json -> find lastCompletedStep and nextStep
5. Read all files referenced in completedDeliverables before touching them
6. Continue from nextStep
```

Do not re-do completed steps. The state file is the source of truth about what
was done.

---

## Anti-Patterns

1. **Trying to finish the phase before the context limit** — rushing causes mistakes; wrap cleanly
2. **Not saving state** — the next session has no way to know where to resume
3. **Not committing at wrap-up** — uncommitted work can be lost
4. **Over-estimating context remaining** — when uncertain, use the emergency stop protocol
5. **Pushing the wrap-up commit to main/master** — push to the working branch unless the active session explicitly owns the main boundary
