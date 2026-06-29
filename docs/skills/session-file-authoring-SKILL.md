# Session File Authoring (XIIGen Enterprise Platform)

## Purpose

A session file that references another session file, assumes prior context, or
contains implicit state is not a session file — it is a fragment. A fragment
cannot be handed off to a cold session. This skill defines the crystallization
protocol (what makes a session file self-contained), the 8 self-containment
checks (with verification commands), and Gate D (the goal advancement block
every session file must contain).

Platform port of the `llm_mvp_core` skill. Stack-bound to **.NET 10 ASP.NET Core
Web API**, **xUnit** (`tests/`), `Xiigen.Platform.slnx`, and the v3.3 governance
triple. On this platform the session files are the
`docs/plans/enterprise-adaptation/v3.3/SESSION-XX-*.md` files of the binding
beacon loop. They already exist as a practice; this skill adds the formal Gate D
(goal advancement) block that the prose beacon loop in `AGENTS.md`/`CLAUDE.md`
does not yet enforce.

---

## When to Invoke

- Before writing any `SESSION-XX-*.md`, session handoff brief, or state snapshot
- When `agent-output-format-SKILL.md` SESSION-0 gate requires a new execution file
- When handing off to another session (`session-handoff-SKILL.md`)
- When a cold-session reader says "I can't understand this without the prior context"

---

## Section 1 — Crystallization Protocol

A session file is "crystallized" when a reader who has never seen this project
before can understand: what the session was trying to accomplish, what state the
project was in when it started, what the session produced, and what the next
session must do.

**Four crystallization requirements:**

1. **No cross-references to prior session files**
   "See SESSION-04 for context" is not crystallized. The context must be present
   in this file, not pointed to.
2. **No implicit state**
   "Continuing from last time" is not crystallized. State the relevant prior
   state explicitly in the file header.
3. **No assumed knowledge**
   Abbreviations must be expanded on first use. Capability names must be defined.
   Fabric interface names (`ISchedulerService`, `IOutboxStore`,
   `IIdempotencyStore`, `IDocumentStore`, `ITenantContextAccessor`) must be
   explained on first use.
4. **No dangling references**
   Every file mentioned by name must either be readable from the project
   directory or explained inline. "See `XIIGEN-ENTERPRISE-ADAPTATION-BEACON-PROMPT-v1.md`"
   is acceptable because that file always exists. "See my earlier analysis" is not.

---

## Section 2 — Eight Self-Containment Checks

Run these checks on every session file before committing or handing off. The
session files live under `docs/plans/enterprise-adaptation/v3.3/`.

```bash
# Check 1: No cross-references to prior session files
grep -nE "SESSION-[0-9]" docs/plans/enterprise-adaptation/v3.3/SESSION-XX-NAME.md
# Expected: 0 hits (if any hits: replace with inline context)

# Check 2: No "continuing from" or "as before" phrases
grep -in "continuing from\|as before\|as discussed\|from last time" \
    docs/plans/enterprise-adaptation/v3.3/SESSION-XX-NAME.md
# Expected: 0 hits

# Check 3: All abbreviations expanded on first use
# Manual: scan for any uppercase abbreviation not followed by "(full form)"
# Example: "WAF (Web Application Firewall)" first use, then "WAF"

# Check 4: File references exist
grep -oE '[A-Za-z0-9_./-]+\.(md|cs|json|slnx)' \
    docs/plans/enterprise-adaptation/v3.3/SESSION-XX-NAME.md \
    | while read f; do [ -f "$f" ] || echo "MISSING: $f"; done
# Expected: 0 MISSING lines

# Check 5: Goal advancement block present
grep -n "Goal advancement" docs/plans/enterprise-adaptation/v3.3/SESSION-XX-NAME.md
# Expected: 1 hit (Gate D — see Section 3)

# Check 6: Verbatim goal quote present
grep -n "verbatim\|\".*goal.*\"" docs/plans/enterprise-adaptation/v3.3/SESSION-XX-NAME.md
# Expected: 1+ hits (at least one quoted goal string)

# Check 7: Test baseline recorded (native platform Gate-B)
grep -n "testBaseline\|Failed: 0\|Passed:" docs/plans/enterprise-adaptation/v3.3/SESSION-XX-NAME.md
# Expected: 1+ hits (pre-session and post-session baselines from
#           `dotnet test Xiigen.Platform.slnx`)

# Check 8: Next session requirements stated
grep -n "Next session\|prerequisite\|blocked by" docs/plans/enterprise-adaptation/v3.3/SESSION-XX-NAME.md
# Expected: 1+ hits
```

All 8 checks must pass before the file is handed off. A check that cannot pass
means the file is not crystallized. Fix the file, not the check.

---

## Section 3 — Gate D: Goal Advancement Block

Every session file must contain a Gate D block. Gate D is a binary test: did this
session advance the stated goal? It is a mandatory addition on this platform —
the beacon-loop session files do not yet formalize it.

```markdown
## Goal Advancement

**Stated goal (verbatim):** "[exact quote from session start — never paraphrased]"

**Advancement:** [one sentence with a state-change verb]
Examples of valid advancement sentences:
  - "Phase 5 complete: IOutboxStore.AppendAsync now persists a CloudEventEnvelope and ListAsync returns it."
  - "Gate B passed: 12 new xUnit tests added, all green, baseline preserved."
  - "BLOCKED: missing locked decision for idempotency TTL semantics; session halted."

**Next session must:** [one sentence stating the concrete starting condition]
```

**State-change verb requirement:** The advancement sentence must contain a verb
that names a state change: "complete," "added," "fixed," "blocked," "produced,"
"removed," "passed." Verbs like "worked on," "investigated," "reviewed" describe
activity, not outcomes.

**Gate D FAIL conditions:**
- Advancement sentence has no state-change verb → rewrite
- Goal quote is paraphrased, not verbatim → go back to source, quote exactly
- "Next session must" is vague ("continue the work") → name the specific file,
  method, fabric interface, or completion-matrix row the next session starts from

---

## Section 4 — Session File Header Template

Every session execution file begins with this header:

```markdown
# SESSION-XX — [Phase Name]

## Session State (pre-session)
- **Branch:** [git branch name]
- **Test baseline:** [N passed, 0 failed — from `dotnet test Xiigen.Platform.slnx` at session start]
- **Last commit:** [hash + message]
- **Matrix rows in scope:** [COMPLETION-MATRIX-v3.3 row references]

## Goal Advancement
**Stated goal (verbatim):** "[from session start or the active SESSION-XX file]"
**Advancement:** [state-change sentence — written at session END]
**Next session must:** [specific starting condition — written at session END]

## Session Scope
This session implements: [specific phases or capabilities]
This session does NOT cover: [explicit list of out-of-scope items]
```

The "does NOT cover" section prevents scope creep and prevents the next session
from assuming this session completed something it didn't.

---

## Section 5 — Anti-Patterns

1. **"Session file says 'see SESSION-04 for background'"** — the background must
   be in this file. A cold reader does not have SESSION-04. Inline it.
2. **"Goal advancement sentence: 'Made progress on the outbox'"** — "made
   progress" has no state change. What specifically changed? What passes now that
   didn't before?
3. **"Test baseline not recorded"** — without the pre-session baseline, the
   post-session baseline is meaningless. Record both, from `dotnet test`.
4. **"Abbreviations used without expansion"** — the next session reader may not
   know what "TTL" means. Write "Time To Live (TTL)" on first use.
5. **"'Next session must: continue implementation'"** — this is not actionable.
   "Next session must: implement IIdempotencyStore.MarkSucceededAsync starting
   from `src/Xiigen.Kernel/FabricContracts.cs`" is actionable.
6. **"Gate D written before the session ends"** — the advancement sentence must
   describe what actually happened, not what was planned. Write it last.

---

## Integration

- `agent-output-format-SKILL.md` defines the three-file rule (REFERENCE/STATE/EXECUTION)
- `session-handoff-SKILL.md` uses this skill's Gate D block as the handoff summary
- `session-output-contract-SKILL.md` Section 3 (read-before-write rule) applies to all UPDATE deliverables
- `goal-context-persistence-SKILL.md` governs the verbatim goal capture that feeds Gate D
- `verification-before-completion-SKILL.md` runs before any session is declared done; Gate D is its output
