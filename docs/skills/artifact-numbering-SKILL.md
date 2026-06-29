# Artifact Numbering (XIIGen Enterprise Platform)

## The Core Rule

**NEVER use artifact numbers from a plan file, a session summary, or memory.**
Numbers in plans decay. Live canonical documents are the only authoritative source.

Platform port of the `llm_mvp_core` skill. Stack-bound to **.NET 10 ASP.NET Core
Web API**, **xUnit** (`tests/`), `Xiigen.Platform.slnx`, and the v3.3 governance
triple. On this platform the live canonical documents are
`XIIGEN-ENTERPRISE-ADAPTATION-COMPLETION-MATRIX-v3.3.md` and
`XIIGEN-ENTERPRISE-ADAPTATION-STATE-v3.3.json`. Phase / `SESSION-XX` numbers come
from those, not from prose or memory.

At the start of every session that assigns new numbers:
```bash
# Read live canonical docs — do not use any cached or plan-file numbers
grep -nE "SESSION-[0-9]+" docs/plans/enterprise-adaptation/v3.3/XIIGEN-ENTERPRISE-ADAPTATION-COMPLETION-MATRIX-v3.3.md | tail -10
grep -nE "\"phaseId\"|SESSION-[0-9]+" docs/plans/enterprise-adaptation/v3.3/XIIGEN-ENTERPRISE-ADAPTATION-STATE-v3.3.json | tail -5
grep -rnE "D-[0-9]+|BUG-[0-9]+" docs/plans/enterprise-adaptation/v3.3/ | tail -10
```

---

## When to Invoke

- When assigning a new phase / `SESSION-XX` number
- When assigning a new decision number (D-NN)
- When assigning a new bug number (BUG-NN)
- Before any state file is saved with artifact numbers

---

## Numbering Protocol

### Step 1: Read live numbers (MANDATORY before assigning)

```bash
# Highest SESSION-XX in the completion matrix:
grep -oE "SESSION-[0-9]+" docs/plans/enterprise-adaptation/v3.3/XIIGEN-ENTERPRISE-ADAPTATION-COMPLETION-MATRIX-v3.3.md | sort -t- -k2 -n | tail -1

# Highest decision/bug number across the v3.3 governance docs:
grep -rhoE "D-[0-9]+|BUG-[0-9]+" docs/plans/enterprise-adaptation/v3.3/ | sort | tail -5
```

Record in the state file before assigning new numbers:
```json
{
  "artifactNumbersRead": {
    "readAt": "session-start",
    "highestSessionNumber": "SESSION-12 (from COMPLETION-MATRIX-v3.3)",
    "highestDecisionNumber": "D-07 (from v3.3 governance docs)",
    "highestBugNumber": "BUG-03 (from v3.3 governance docs)"
  }
}
```

### Step 2: Cross-check before claiming a number

Before assigning SESSION-13, verify SESSION-13 is not already in use:
```bash
grep -n "SESSION-13" docs/plans/enterprise-adaptation/v3.3/XIIGEN-ENTERPRISE-ADAPTATION-COMPLETION-MATRIX-v3.3.md
# Expected: 0 matches — safe to assign
```

### Step 3: Assign sequentially

- Phase / SESSION numbers: next sequential integer after the highest in use
- Decision numbers: next sequential integer after the highest D-NN in use
- BUG numbers: next sequential integer after the highest BUG-NN in use
- Never skip numbers — gaps create audit confusion
- Never reuse a number from a deleted or abandoned artifact

### Step 4: Record the assignment in the state file immediately

```json
{
  "numbersAssignedThisSession": {
    "sessions": ["SESSION-13"],
    "decisions": [],
    "bugs": []
  }
}
```

This prevents a second session from claiming the same number before the first
session's commit is pushed.

---

## Collision Prevention

| Collision type | Prevention |
|---|---|
| Two sessions assign same SESSION-NN | Both check the state file at session start; higher session uses next available |
| Plan file says SESSION-13 but live COMPLETION-MATRIX-v3.3 already has SESSION-13 | Live doc wins; plan file number is stale |
| BUG-04 assigned but not yet committed | Reserve in the state file; do not reuse until commit confirmed |

---

## Numbers at Plan Creation (REFERENCE ONLY — DO NOT USE)

Numbers recorded in plan or session files at plan creation time may be stale by
the time the session runs. Always read live docs before assigning.

```
SESSION-12 (plan creation) -> READ DOCS before assigning the next SESSION-NN
BUG-03 (plan creation)     -> READ DOCS before assigning the next BUG-NN
```

---

## Anti-Patterns

1. **"The plan says the next phase is SESSION-13."** Plan numbers decay. Read live docs.
2. **"I'll assign the number and update the matrix later."** Assign only after reading live. Record in the state file immediately.
3. **"This number is unused — I'll reclaim it."** Never reuse a number. Use next sequential.
4. **"Only one session runs at a time."** Multiple sessions may run in different worktrees. State-file reservation prevents collision.
