# Session Mode Declaration (XIIGen Enterprise Platform)

## Purpose

A session that drifts from one mode to another mid-stream produces contaminated
output. The contamination is invisible when it happens and costly to unwind later.
This skill makes mode an explicit, declared property with enforceable scope boundaries.

Platform port of the `llm_mvp_core` skill. Stack-bound to **.NET 10 ASP.NET Core
Web API**, **xUnit** (`tests/`), `Xiigen.Platform.slnx`, and the v3.3 governance
triple (`XIIGEN-ENTERPRISE-ADAPTATION-BEACON-PROMPT-v1.md` /
`XIIGEN-ENTERPRISE-ADAPTATION-STATE-v3.3.json` /
`XIIGEN-ENTERPRISE-ADAPTATION-COMPLETION-MATRIX-v3.3.md`). The mode declaration is
the explicit form of the prose role loop in `AGENTS.md`/`CLAUDE.md`:
`PLAN_AUTHOR_INTERNAL -> PLAN_REVIEW_INTERNAL -> ARCHITECT_INTERNAL ->
EXECUTOR_INTERNAL -> REVIEW_INTERNAL -> ARCHITECT_INTERNAL_REPAIR ->
EXECUTOR_INTERNAL_CONTINUE`.

---

## When to Invoke

- At session start, before first synthesis
- When a session's output visibly leaves its declared mode's scope
- When Luba corrects "wrong level of output" — that is a mode drift signal

One explicit declaration at session start = zero slow-drift correction cycles.

---

## Section 1 — The 5 Session Modes

Every session declares exactly one of these modes.

| Mode | Produces | Scope-in | Scope-out |
|------|----------|----------|-----------|
| **ARCHITECT** | decisions, trade-off analyses, interface contracts | goals, interfaces, design invariants, acceptance criteria | file paths, method signatures, test code, implementation diffs |
| **PLANNER** | phase sequences, dependency graphs, SESSION-XX execution files | phases, gates, Gate B conditions, dependencies, scope declarations | new design work, code implementation |
| **REVIEWER** | verdicts, gap reports, plan approval/rejection | plan evaluation, gate checks, DNA compliance, evidence verification | plan authoring, code changes, new design proposals |
| **EXECUTOR** | file changes, test runs, build outputs | edits in `src/` and `tests/`, line-precise changes, `dotnet build`/`dotnet test` runs | new design decisions, scope changes, architectural trade-offs |
| **INVESTIGATION** | discovery findings, artifact boundaries, capability maps | reading files, search commands, test runs, verified facts | implementing changes, writing new code |

**The scope boundaries are enforceable gates, not preferences.**

If ARCHITECT mode output contains method signatures or `file:line` references, the
session has drifted to EXECUTOR. If EXECUTOR mode proposes a new design pattern,
that is ARCHITECT scope. Drift is detected, not tolerated. ARCHITECT scope on this
platform is "decisions without file paths"; EXECUTOR scope is "edits in `src/`
and `tests/` ASP.NET Core Web API + xUnit".

---

## Section 2 — Declaration Protocol

At session start:

1. **Declare exactly one mode** — not two, not "mostly X but also Y"
2. **Write 1-2 sentence justification** — why this mode fits the question; why
   the adjacent mode is wrong
3. **Log immutably** — mode cannot silently change mid-session
4. **No output before declaration** — declaration is a prerequisite

Declaration template:
```markdown
## Session Mode (session-mode-declaration-SKILL)

Declared mode: [ARCHITECT | PLANNER | REVIEWER | EXECUTOR | INVESTIGATION]

Justification:
[1-2 sentences. What makes this mode correct for Luba's question?
Why is the adjacent mode wrong?]

Scope-out for this session:
[explicit list of what this session will NOT produce]
```

If Luba's question doesn't fit any single mode cleanly — stop and ask. Do not guess.
Multi-mode sessions are exactly how drift happens.

---

## Section 3 — Drift Detection Signals

Self-check before every STOP. Each mode has specific signals:

### ARCHITECT drifting to EXECUTOR
- Output contains `file:line` references or C# method signatures
- Output proposes a specific code change or diff under `src/`
- Output prescribes an execution order ("first do X, then Y, then Z")

### ARCHITECT drifting to PLANNER
- Output produces a phase sequence or `SESSION-XX-*.md` file
- Output names specific Gate B conditions or `dotnet test` counts

### PLANNER drifting to EXECUTOR
- Output contains diffs or C# file contents
- Output runs or proposes running specific `dotnet build`/`dotnet test` commands

### EXECUTOR drifting to ARCHITECT
- Output proposes a design pattern instead of implementing one
- Output debates the approach instead of executing it

### INVESTIGATION drifting to EXECUTOR
- Output starts modifying files before completing the discovery
- Output writes new C# code instead of reading existing code

---

## Section 4 — Resolution on Drift

**Detected drift is NOT corrected by continuing in the correct mode.**

Slow-drift correction after a long stretch of drifted output doesn't produce a
clean result — it produces a hybrid with the wrong frame baked in.

Resolution steps:
1. Session STOPS immediately when drift is detected, even mid-response
2. Log the drift: what scope-out signal was detected, what output triggered it
3. Luba decides: restart in correct mode (fastest), or change the declaration
   with new justification (if initial declaration was wrong)

A fresh session with explicit mode declaration is faster than correcting mid-course.

This complements the binding rule in `CLAUDE.md` "Executor Cannot Stop The Plan":
the executor produces slice evidence and hands off to `REVIEW_INTERNAL`; mode
declaration makes that handoff a typed transition rather than an implicit one.

---

## Section 5 — Session Mode Record

Record the declared mode in the active state file
(`XIIGEN-ENTERPRISE-ADAPTATION-STATE-v3.3.json`, or the state file the active
`SESSION-XX-*.md` names):

```json
{
  "sessionMode": {
    "declared": "EXECUTOR",
    "justification": "Luba asked to implement the IOutboxStore append path in Xiigen.Fabrics.InMemory. The question is about code implementation, not design trade-offs.",
    "scopeOut": ["new design decisions", "architectural trade-offs", "scope changes"],
    "driftEvents": []
  }
}
```

---

## Anti-Patterns

1. **"I'll pick the mode as I go"** — mode declaration is before first synthesis,
   not inferred retrospectively.
2. **"It's multi-mode"** — no session is multi-mode. If the question requires two
   modes, handle one and defer the other to a separate session.
3. **"Implicit mode"** — implicit modes drift. Declare explicitly.
4. **"Mode changed because the question evolved"** — if the question genuinely
   evolved, stop the current session and restart with the new mode.
5. **"Let me correct the drift in the next response"** — corrections leave
   drifted context intact. Restart is faster than correction.
