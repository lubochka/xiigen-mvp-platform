# Phase Completion Packager (XIIGen Enterprise Platform)

## Purpose

When a phase gate passes, produce a consistent completion package: a machine
state update, a human-readable phase report, and a session handoff. The package
is a **checkpoint**, not automatically a STOP.

Platform port of the `llm_mvp_core` skill. Stack-bound to **.NET 10 ASP.NET Core
Web API**, **xUnit** (`tests/`), `Xiigen.Platform.slnx`, `DataProcessResult<T>`,
and the v3.3 governance triple (`BEACON` / `STATE-v3.3.json` /
`COMPLETION-MATRIX-v3.3.md`).

UI/UX/Dashboard/Host-Session-Auth packaging gates from the core skill are
**`na`** for this repository — the platform has no frontend.

## Gate Model — Read STATE-v3.3.json First

Never invoke if any gate check fails. Before writing any STOP line, read the
active session file and `STATE-v3.3.json` and check:

```text
goal_reached == true
next_action is empty
completion coverage matrix (COMPLETION-MATRIX-v3.3) has zero required UNKNOWN/FAIL rows
ARCHITECT_VERDICT_ALLOW_FINAL is recorded for the round
Luba did not ask to continue / do not stop
```

If any condition fails, do not write a STOP instruction. Write
`IN_PROGRESS_NEXT_STEP_KNOWN`, update state with the next plan-derived action,
and continue.

## Continuous Plan Exception

Some plans explicitly forbid stopping between internal roles or phase boundaries
while the full goal remains open. In those plans the package is a checkpoint.

Soft stops are invalid (from `CLAUDE.md` "Executor Cannot Stop The Plan"):

```text
commit done, report written, one child session passed, AWS URL produced,
screenshot captured, plugin harness passed, or context getting long.
```

`commit/push boundary is not a stop`. A completed bounded slice is packaged,
remaining work is reported, and the next parent-issued bounded work order
continues. "Do not stop" never authorizes one executor to run unbounded until
"everything is done"; each worker has a narrow scope, a max active time of <= 10
minutes, an evidence packet, and a return to parent review.

## When to Invoke

After every phase gate passes — specifically after:
```
□ dotnet build Xiigen.Platform.slnx -> 0 Error(s)
□ dotnet test (full or the session-allowed filter) -> Failed: 0
□ All phase-specific checks from the active SESSION-XX file pass
□ The platform coding-standard checks pass (implementation-integrity-SKILL.md)
```

## Anti-Stub Gate — REQUIRED before status=DONE for any code/runtime phase

Build + test-count is NOT sufficient to mark a phase DONE. A canned-return service
passes both. Before writing `status: DONE` for a code/runtime/fabric/endpoint
phase, REQUIRE:

```text
1. Implementation-reality evidence (implementation-integrity-SKILL.md Check 5):
   real code path file:line refs API -> service -> fabric interface
   (ISchedulerService / IOutboxStore / IIdempotencyStore / IDocumentStore /
    ITenantContextAccessor) -> persisted/returned DataProcessResult.
2. Runtime Evidence Firewall (verification-before-completion-SKILL.md) PASS:
   reject validation_only:true, static/contract/mixed metric, UNKNOWN cpu/ram.
3. No stub on the done path: no NotImplementedException, no canned
   DataProcessResult.Success() that skips the work, no TODO/PLACEHOLDER.
```

If any is missing, write `IN_PROGRESS_NEXT_STEP_KNOWN`, set `next_action` to the
missing evidence, and continue.

## Two-PASS status gate — which PASS may you write?

A capability phase has TWO distinct legitimate PASS states. Never write a bare
`status: DONE` for a capability the plan calls "working"; write which PASS it is
(record it in `STATE-v3.3.json`, e.g. `passState`):

```text
PASS_AS_HONEST_UNTRAINED_SCAFFOLD  (platform reading: honest skeleton)
    Allowed when: contract/skeleton exists AND it honestly reports not-ready,
    hides no deterministic shortcut, and its honesty is proven by xUnit tests
    (the endpoint/service returns an honest DataProcessResult.Failure with a
    "NOT_READY"/"NOT_IMPLEMENTED" code, not a fake success; no canned data
    dressed as a real result). A real milestone — NOT a working feature. It must
    NOT satisfy a matrix row that asks for the working capability.

PASS_AS_REAL_FEATURE
    Allowed only with full evidence: the real code path runs, the endpoint/fabric
    is invoked through an integration test (e.g. WebApplicationFactory), the
    DataProcessResult success/failure is asserted on real data, and the result is
    persisted/observable (e.g. via IOutboxStore.ListAsync / IDocumentStore.GetAsync).
```

Writing `PASS_AS_REAL_FEATURE` (or bare DONE) on a path that only meets the
scaffold bar is the masquerade class — the most dangerous false positive, because
the project then plans from a capability that does not exist.

## Review/fix cycle gate (count honesty)

A requested review/fix cycle is a real review action with its own inspectable
evidence packet, not a ledger row. For "at least N" cycles, count only packets
that name the reviewer/sub-agent id, artifact before/after refs, loaded
skills/gates, findings or no-fix justification, fix refs when needed, and recheck
evidence. N ledger rows after one real review is a failed package.

## Sequential Execution-Control Gate

Long execution status must show a plain board where only the **first
unfinished** row is "current":

```text
| # | task | status: done | in-progress | remaining | review-block | blocked-with-reason |
```

The board must include total count, completed count and names, current task and
owner/sub-agent, remaining count and names, current result type (code / artifact
/ review / policy / evidence-only / commit boundary), accepted evidence, open
gaps, and the next bounded action.

## Procedure — Three Artifacts in Order

### Artifact 1 — STATE update (machine-readable)

Update `STATE-v3.3.json` (or the active state file the session names): phase id,
`status`, `passState`, gate results, files changed, `goal_reached`,
`stop_allowed`, `next_action`, and the matrix row references touched.

### Artifact 2 — PHASE-COMPLETE report (human-readable, for Luba)

```markdown
# Phase [id] — [name] — [completed_at]
## What Was Built        (plain language)
## Gate Results          (dotnet build / dotnet test output, matrix rows)
## Files Changed
## What's Next            (next_action, or "COMPLETE" only if the gate model allows)
## STOP OR CONTINUE — stop only when the gate model above is satisfied
```

The visible packet starts with human meaning: total phases/steps,
done/in-progress/remaining, what the phase solves, what the current step solved,
what evidence exists, what is not proven, and the next bounded action. Machine
labels, packet ids, STATE flags, and paths come after that explanation.

### Artifact 3 — Session handoff note (for the next session)

What was achieved, current `next_action`, open holes, and the exact next bounded
work order.

## Anti-Patterns

1. **Packaging a STOP while `next_action` is non-empty** — that is a soft stop.
2. **Bare `status: DONE` for a capability** — write the two-PASS state.
3. **`PASS_AS_REAL_FEATURE` on a scaffold** — masquerade; only honest-skeleton PASS.
4. **N ledger rows reported as N review cycles** — count honesty fails.
5. **Machine-only status** — Luba-first human meaning before any opaque label.
