# Goal Context Persistence (XIIGen Enterprise Platform)

## Core Problem

In a long session, the user's original goal drifts out of active attention. By
turn 10 the session answers proxies for the goal rather than the goal itself.
Each individual hop feels rational; the cumulative drift is invisible from inside
the session and obvious to Luba after 20 rounds.

**This skill prevents that by making the goal a first-class persistent artifact.**

Platform port. Stack-bound to **.NET 10 ASP.NET Core Web API** and the v3.3
governance triple. The goal source is the active `SESSION-XX-*.md` file; the
persistent record lives in `STATE-v3.3.json` (or the state file the session
names).

## When to Invoke

- At session start, before first synthesis.
- At every STOP gate (Goal Reminder block required at top of output).
- At designated checkpoints for sessions exceeding 10 turns.
- Whenever output direction feels unclear — re-read the goal.

One explicit goal load at session start + reminder at every STOP = zero "we
forgot what you were asking for" cycles.

## Section 1 — Goal Loading Protocol

At session start:

1. **Capture the goal statement verbatim** — exact characters as stated in the
   active `SESSION-XX` file, not a paraphrase. Word choice encodes constraints.
2. **If no explicit goal is stated** — ask Luba (or read it from the session
   file) before proceeding. Do not infer; a session without a declared goal has
   no anchor against which drift can be detected.
3. **If multiple goals are stated** — capture them as a list and track each
   separately. Untracked multi-goal sessions are exactly how 3-of-4 coverage
   failures happen.
4. **The goal statement is immutable** for the session. If Luba restates or
   refines it, that is a new session, not an edit.

## Section 2 — Goal Reminder Block (required at every STOP)

```markdown
## Goal Reminder (goal-context-persistence-SKILL)

Luba's stated goal for this session:
"[verbatim goal statement]"

This session advances the goal by:
[ONE SENTENCE — concrete observable progress, not abstract framing]
"By [verb] [specific artifact], [specific goal element] moves from [prior state] to [new state]."

What remains to complete the goal: [specific outstanding items or "COMPLETE"]
```

The "This session advances the goal by" field is mandatory. If the session
cannot state concrete observable progress, it produced no goal-relevant output —
surface that explicitly.

FAIL the concrete-progress test:
- "Working on goal X" (no verb of completion)
- "Making progress on Y" (no observable delta)
- "Explored implications of Z" (no state change)

PASS (platform deltas):
- "By adding the ISchedulerService endpoint + 5 xUnit tests, scheduling moves from internal-only to API-reachable."
- "By wiring IOutboxStore into the publish path, job persistence moves from 0 to 1 integration-tested component."
- "By reconciling matrix rows R7-R9, completion coverage moves from 3 open gaps to 0."

## Section 3 — Goal Checkpoint for Long Sessions

| Turn | Action |
|------|--------|
| 5 | Re-read goal statement. Note alignment verdict. |
| 10 | Re-read. Note alignment verdict. |
| 15, 20, 25, ... | Re-read. Checkpoint each 5 turns. |

Alignment verdicts:
- **ON_GOAL** — recent turns produced output traceable to the goal. Continue.
- **DRIFTING** — recent turns respond to prior output rather than the goal.
  Next turn must re-anchor.
- **OFF_GOAL** — recent turns answer a different question. Stop and ask Luba for
  redirection — do not silently continue.

## Section 4 — Goal Context Record Format

Record in `STATE-v3.3.json` (or the active state file):

```json
{
  "goalContext": {
    "statement": "[verbatim goal statement]",
    "capturedAt": "2026-06-29",
    "goalElements": ["element 1", "element 2"],
    "checkpoints": [
      { "turn": 5, "progressStatement": "[what objectively changed]", "alignmentVerdict": "ON_GOAL" }
    ],
    "goalComplete": false
  }
}
```

## Section 5 — Goal Drift Signals

| Signal | Threshold | Action |
|--------|-----------|--------|
| Rounds since last goal reference in output | >= 5 | Re-anchor |
| Consecutive DRIFTING checkpoints | >= 2 | STOP, re-anchor |
| Session length exceeds 10 turns without checkpoint | any | Force checkpoint |
| "This advances the goal by" is empty or abstract | any | Rewrite turn |
| Luba's recent messages contain a correction ("no — X", "stop doing Y") | any | STOP, re-anchor immediately |

The last signal is the most reliable drift detector: if Luba corrects direction,
the session has drifted regardless of its internal sense of alignment.

## Anti-Patterns

1. **"The goal is obvious, I don't need to quote it"** — obvious goals drift
   fastest because no one watches them.
2. **"I'll paraphrase the goal for cleanliness"** — capture verbatim.
3. **"Luba hasn't objected, so we must be on-track"** — silence is not alignment;
   run checkpoints regardless.
4. **"Progress is abstract"** — abstract progress means little goal-relevant
   output; surface that rather than hiding it in vague language.
