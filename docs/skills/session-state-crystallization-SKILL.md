# Session State Crystallization (XIIGen Enterprise Platform)

## When to Invoke

At the end of every planning, investigation, or analysis session — before
reporting complete. The snapshot is the LAST deliverable of every session.

Code sessions produce a commit and a `XIIGEN-ENTERPRISE-ADAPTATION-STATE-v3.3.json`
update instead — the crystallization step is the state write.

Platform port of the `llm_mvp_core` skill. Stack-bound to **.NET 10 ASP.NET Core
Web API**, **xUnit** (`tests/`), `Xiigen.Platform.slnx`, and the v3.3 governance
triple. The Russian-first board fields below are required by the
`output-readability-gate-SKILL.md` Execution Board gate.

---

## The ≤40-Line Rule

The snapshot must be ≤40 lines. Hard cap, not a guideline.

If you cannot fit session state in 40 lines:
1. You are repeating information already in a prior snapshot
2. Write only what CHANGED since the last snapshot
3. Cut anything derivable from a command (test count from `dotnet test`, not memorized)

A 200-line snapshot is a documentation replacement, not a snapshot. The snapshot
must stay lean enough that the next session reads it in 30 seconds.

---

## Snapshot Format

File: `docs/SESSION-STATE-SNAPSHOT--[YYYY-MM-DD].md`
Replaces: any prior snapshot (only the latest is active)

```markdown
# SESSION-STATE-SNAPSHOT — [YYYY-MM-DD]
# Session type: [PLANNING | INVESTIGATION | CODE | ANALYSIS]
# Produced by: [brief session description]

## EXECUTION POSITION
Next work:    [one sentence — the next executable action]
Can start:    [YES | NO — if NO, list what is blocking]

## BLOCKING PREREQUISITES
[For each blocker: description + verify command + expected result]
Example:
  IOutboxStore.AppendAsync must exist before the publisher session
  Verify: grep -n "AppendAsync" src/Xiigen.Kernel/FabricContracts.cs
  Expected: line present

## ACTIVE BUGS
[BUG-NN | one-line description | status: OPEN/FIXED/DEFERRED]
Example:
  BUG-01 | IIdempotencyStore TTL not enforced on reserve | FIXED — expiry guard added
  BUG-02 | OutboxStore ListAsync returns wrong order under tenant scope | OPEN

## ARTIFACT BOUNDARIES
Next matrix row:  [COMPLETION-MATRIX-v3.3 row id — next in sequence]
Test count:       [N — run: dotnet test Xiigen.Platform.slnx --no-build 2>&1 | tail -3]
Last commit:      [hash] [message first line]

## MATRIX / PHASE STATUS
[Row or SESSION-XX | status: COMPLETE/ACTIVE/PENDING/BLOCKED | one-line reason if not COMPLETE]
Example:
  SESSION-03 Outbox | COMPLETE | AppendAsync + ListAsync, 24 xUnit tests passing
  SESSION-04 Idempotency | ACTIVE | Integration tests pending
  SESSION-05 Scheduler | BLOCKED | Waiting for 04 to complete

## DECISIONS MADE THIS SESSION
[one sentence per decision — only decisions made IN THIS SESSION]
Example:
  D-2026-06-01-1: IDocumentStore.UpsertAsync uses optimistic concurrency via expectedVersion
  D-2026-06-01-2: tracker cards serialize to JSON, not binary, for human readability

## SKILLS LOADED
Skills consulted this session: [list]
```

---

## How to Produce Each Field

### EXECUTION POSITION
From: the last explicitly decided "what to do next" in this session. Not from
memory — state what was decided in this session only. If not decided: write
"UNKNOWN — determine at next session start."

### BLOCKING PREREQUISITES
Only unmet prerequisites. Copy the verification command literally. A paraphrased
command cannot be run at the next session start.

### ACTIVE BUGS
From: the bug log or the current session's bug discoveries.
FIXED = fixed in this session (include the fix method).
DEFERRED = explicit decision to defer (include the reason).
OPEN = unresolved.

### ARTIFACT BOUNDARIES
Run fresh at the end of the session — never from memory:
```bash
dotnet test Xiigen.Platform.slnx --no-build 2>&1 | tail -3
git log --oneline -1
```

### MATRIX / PHASE STATUS
From: what was explicitly verified in this session. A row is COMPLETE only when
`dotnet test` shows all its tests passing and the COMPLETION-MATRIX-v3.3 row has
no required `UNKNOWN`/`FAIL`.

### DECISIONS MADE THIS SESSION
ONLY decisions made in this session. Format: `D-[YYYY-MM-DD]-[N]: [one sentence]`.

---

## The Gate

A session cannot claim complete without producing this snapshot.

Line count check before reporting done:
```bash
# Count lines in the snapshot
wc -l "docs/SESSION-STATE-SNAPSHOT--$(date +%Y-%m-%d).md"
# Expected: <= 40
# If > 40: rewrite with only what changed since last snapshot
```

The snapshot replaces:
- Manually maintained "current state" notes
- Hard-to-find information about session execution position

The snapshot does NOT replace:
- `XIIGEN-ENTERPRISE-ADAPTATION-STATE-v3.3.json` (machine phase state — different purpose)
- `XIIGEN-ENTERPRISE-ADAPTATION-COMPLETION-MATRIX-v3.3.md` (full requirement-to-evidence matrix)
- `src/Xiigen.Kernel/FabricContracts.cs` (full fabric interface reference)

---

## Anti-Patterns

1. **Snapshot > 40 lines** — rewrite with only what changed
2. **Verification commands paraphrased** — copy literal commands; paraphrases cannot be run
3. **"No decisions made this session"** — every session makes at least one decision; if genuinely none, write "No architectural decisions — [session type] session"
4. **Snapshot produced after session claims complete** — the snapshot IS the last step; produce it before claiming done
5. **Copying matrix/phase status from the last snapshot without verifying** — status must come from what was actually checked in this session
