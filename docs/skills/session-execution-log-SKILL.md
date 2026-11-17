# Session Execution Log (XIIGen Enterprise Platform)

## Purpose

Every phase produces a structured execution log. This log is the single source of
truth for what happened in a phase: what files changed, what tests moved, what was
built, and what decisions were made. Future sessions read it. Audit trails
reference it.

Platform port of the `llm_mvp_core` skill. Stack-bound to **.NET 10 ASP.NET Core
Web API**, **xUnit** (`tests/`), `Xiigen.Platform.slnx`, and the v3.3 governance
triple. The mandatory `humanReadablePlanExecutionLog` block satisfies the
English-first Human-Readable Plan/Execution Log requirement (unless the user requests another language) that
`AGENTS.md`/`CLAUDE.md` state only as prose.

---

## When to Write

After every phase gate passes — never before. If the gate fails (tests regress,
build errors), the execution log is NOT written. A log only exists for a phase
that completed successfully.

The gate that must pass first:
```text
dotnet build Xiigen.Platform.slnx -> 0 Error(s)
dotnet test Xiigen.Platform.slnx (or the session-allowed filter) -> Failed: 0
All phase-specific checks from the active SESSION-XX file pass
```

---

## File Location and Name

```
docs/EXECUTION-LOG-[SESSION-XX].json

Examples:
  docs/EXECUTION-LOG-SESSION-05.json
  docs/EXECUTION-LOG-SESSION-06.json
```

---

## Schema

```json
{
  "phaseId": "SESSION-06",
  "phaseName": "Idempotency fabric — IIdempotencyStore reserve/mark via InMemory provider",
  "completedAt": "2026-06-01",
  "durationMinutes": 52,

  "humanReadablePlanExecutionLog": {
    "stage": "SESSION-06 completion",
    "totalMajorPlanBlocksOrCurrentPhaseTasks": "8 current-phase tasks",
    "completedCountAndNames": "6/8: read, implement, targeted tests, docs sync, gate check, package",
    "currentPlanStep": "STEP 4 - package phase evidence",
    "currentActiveTaskAndOwner": "Package evidence - parent architect",
    "remainingCountAndNames": "2/8: review, commit/push boundary",
    "currentResultType": "artifact",
    "whatChanged": "IIdempotencyStore reserve/mark path and tests were added",
    "whatRemains": "Next phase starts the scheduler fabric",
    "claimsOpened": [],
    "claimsClosed": ["SESSION-06 Gate B"],
    "claimsStillClosedAndWhy": ["Full master-tenant readiness remains closed because later phases are not executed"],
    "whyThisStatusIsTrue": "Build and allowed phase gate passed with exact evidence",
    "nextRequiredAction": "Review then commit/push boundary"
  },

  "testBaseline": {
    "entry": 181,
    "exit": 205,
    "delta": 24,
    "command": "dotnet test Xiigen.Platform.slnx --no-build"
  },

  "buildStatus": {
    "errors": 0,
    "command": "dotnet build Xiigen.Platform.slnx"
  },

  "filesChanged": [
    {
      "path": "src/Xiigen.Fabrics.InMemory/InMemoryIdempotencyStore.cs",
      "type": "created",
      "linesAfter": 142,
      "summary": "Reserve + mark-succeeded/mark-failed with TTL expiry"
    },
    {
      "path": "tests/Xiigen.Fabrics.InMemory/IdempotencyStoreTests.cs",
      "type": "created",
      "linesAfter": 230,
      "summary": "24 tests: reserve, replay, expiry, tenant isolation"
    }
  ],

  "artifactsRegistered": {
    "decisionsLocked": [],
    "interfacesAdded": ["IIdempotencyStore"],
    "matrixRowsTouched": ["COMPLETION-MATRIX-v3.3: idempotency-fabric"]
  },

  "gateResults": [
    { "check": "dotnet build Xiigen.Platform.slnx -> 0 Error(s)", "passed": true, "value": "0 Error(s)" },
    { "check": "dotnet test -> Failed: 0", "passed": true, "value": "205 passed, 0 failed" },
    { "check": "DataProcessResult<T> on all public fabric methods (no throw for business logic)", "passed": true },
    { "check": "Fabric-first — provider resolved through DI, no direct instantiation", "passed": true },
    { "check": "COMPLETION-MATRIX-v3.3 row updated", "passed": true }
  ],

  "gatePassed": true
}
```

---

## Writing Rules

1. Write AFTER the gate passes — never speculatively
2. All number fields must be exact — no estimates or "approximately"
3. `delta` must equal `exit - entry` — if negative, the gate should have failed
4. `filesChanged` must list every file touched during the phase
5. `completedAt` is the timestamp of the gate passing, not phase start
6. `gatePassed` is always `true` — a log only exists for a passed phase
7. Human-Readable Plan/Execution Log Gate: every log must include `stage`, total
   major plan blocks and/or current-phase tasks, completed count and names,
   `currentPlanStep`, current active task and owner/sub-agent, remaining count
   and names, current result type, `whatChanged`, `whatRemains`, `claimsOpened`,
   `claimsClosed`, claims still closed and why, `whyThisStatusIsTrue`, and
   `nextRequiredAction` in language a human can read without decoding machine
   status fields. The English-first board (`output-readability-gate-SKILL.md`)
   uses these same fields.
8. Tie `claimsClosed` to rows of `XIIGEN-ENTERPRISE-ADAPTATION-COMPLETION-MATRIX-v3.3.md`.
9. Human gate for chat logs: if Luba cannot tell from the log what is done, what
   is running, what remains, whether the current result is code, artifact,
   review, policy, or evidence-only, which claims remain closed, and what bounded
   action comes next, the log fails and triggers governance/skill repair before
   product continuation.

---

## Positive Example

A SESSION-06 log produced after the idempotency implementation:
- `testBaseline.entry: 181` — confirmed from `dotnet test` before SESSION-06 started
- `testBaseline.exit: 205` — confirmed from `dotnet test` after SESSION-06 gate passed
- `testBaseline.delta: 24` — 205 - 181 = 24
- `buildStatus.errors: 0` — confirmed from `dotnet build` output
- `filesChanged` — lists every `.cs` file created or modified
- `gateResults` — every check from the SESSION-XX file listed with pass/fail

---

## Negative Examples

```
WRONG: Writing the log before the gate runs
  -> Log exists but tests actually regressed — the log is a lie

WRONG: Estimated test counts
  "testBaseline.exit": "approximately 205"
  -> Must be the actual number from `dotnet test` output

WRONG: Missing filesChanged
  "filesChanged": []  when 4 files were actually modified
  -> Every touched file must appear in filesChanged

WRONG: Machine-only primary status
  Gate rows exist, but there is no human-readable stage, current plan step,
  total/done/current/remaining counts and names, current result type, what
  changed, what remains, claims opened/closed, or why.
  -> The log may be parseable, but it fails the Human-Readable Plan/Execution Log Gate.

WRONG: delta is negative
  "entry": 205, "exit": 194, "delta": -11
  -> Negative delta = regression = gate should have failed
```

---

## Integration

```
Written by:      phase-completion-packager-SKILL.md (after gate passes)
Read by:         Next session's preflight (to verify test baseline)
                 chain-arithmetic-audit-SKILL.md (to verify test count chain)
Cross-checks:    implementation-integrity-SKILL.md (gateResults must include doc sync)
```
