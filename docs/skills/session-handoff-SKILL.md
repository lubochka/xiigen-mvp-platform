# Session Handoff (XIIGen Enterprise Platform)

## Purpose

Work on this platform has two distinct actors that share no context:
- **An execution session** — executes phases, writes code, runs tests
- **A planning/review session (new conversation)** — plans next phases, reviews
  results, makes decisions

Without a structured handoff, every new conversation must re-explain the entire
project state from scratch. A session handoff document is that structured bridge.
Paste it at the start of any new planning conversation and the session immediately
knows: where things are, what just happened, and what needs deciding next.

Platform port of the `llm_mvp_core` skill. Stack-bound to **.NET 10 ASP.NET Core
Web API**, **xUnit** (`tests/`), `Xiigen.Platform.slnx`, and the v3.3 governance
triple. On this platform the brief is the bridge into the binding beacon loop: a
new session reads the brief, then the beacon, then
`XIIGEN-ENTERPRISE-ADAPTATION-STATE-v3.3.json`, then
`XIIGEN-ENTERPRISE-ADAPTATION-COMPLETION-MATRIX-v3.3.md`, then the active
`SESSION-XX-*.md`.

---

## When to Write

After every phase gate passes, as part of the `phase-completion-packager-SKILL.md`
procedure. Write AFTER the gate passes — never speculatively.

---

## File Location and Name

```
docs/SESSION-BRIEF-[SESSION-XX].md

Examples:
  docs/SESSION-BRIEF-SESSION-05.md
  docs/SESSION-BRIEF-SESSION-06.md
```

---

## Required Sections

```markdown
# SESSION BRIEF — [SESSION-XX] -> [SESSION-(XX+1)]
## For: new planning session | Produced by: execution session
## Phase completed: [phase_title] | Next: [next_phase_title]
## Date: [completed_at]

---

## PROJECT STATE

Branch:       [current branch]
Phase done:   [SESSION-XX] — [phase_title]
Phase next:   [SESSION-(XX+1)] — [next_phase_title]
Tests:        [exit] passing (was [entry] at phase start, +[delta] this phase)
Build:        0 Error(s)
Last commit:  [hash] — [SESSION-XX]: [description]

Matrix status (from XIIGEN-ENTERPRISE-ADAPTATION-COMPLETION-MATRIX-v3.3.md):
  [list active rows with current state]

---

## WHAT [SESSION-XX] BUILT

[3-5 sentences. Plain English. What was implemented and why it matters. Focus on
capability value, not file names or test counts.]

---

## DISCOVERIES — FACTS ESTABLISHED THIS SESSION

Each discovery: fact + verification command + implication + action taken/deferred.

  DISCOVERY-1:
    fact:        [precise factual statement]
    verified_by: grep -n "..." or `dotnet test` result
    implication: [what this means for future work]
    action:      RESOLVED: [what was done] | DEFERRED: [why and when]

---

## REJECTED CLAIMS

Claims that were wrong during this session. Prevents the same wrong claim from
being accepted in a future session.

  REJECTED-1:
    claim:       [exact wrong statement]
    source:      [who/what made it]
    truth:       [what is actually true]
    verified_by: [command or observation that disproved it]

---

## OPEN ITEMS — NEED YOUR DECISION

[Include only if items genuinely need Luba's input before the next phase can
start. Omit this section entirely if nothing needs deciding. These usually map to
COMPLETION-MATRIX-v3.3 rows still marked UNKNOWN.]

DECISION 1: [title]
  Context: [one sentence]
  Options:
    A) [option with consequence]
    B) [option with consequence]
  Recommendation: [A or B, and why]

---

## WHAT [SESSION-(XX+1)] WILL BUILD

[2-3 sentences: what the next phase covers, what it produces, what the gate looks like.]

Gate: dotnet test Xiigen.Platform.slnx -> [expected test count] passed, 0 failed

---

## HOW TO USE THIS BRIEF

Paste this file at the start of your planning conversation. The session reads this
brief, then the beacon, state, matrix, and the active SESSION-XX file.

To plan the next phase: say "Plan [SESSION-(XX+1)] using this brief."
To approve: say "yes" or "proceed to [SESSION-(XX+1)]."
```

---

## Writing Rules

1. PROJECT STATE section is always present — always current
2. "WHAT [SESSION-XX] BUILT" is prose — no file lists, no bullet points; explain the meaning
3. OPEN ITEMS omitted if nothing needs deciding — do not write "(none)"
4. The brief must be self-contained — no references to files the reader would need to open
5. Test count must be exact — the number from `dotnet test` output
6. If no discoveries and no rejected claims in the session — omit those sections

---

## Negative Examples

```
WRONG: Brief that references files
  "See docs/EXECUTION-LOG-SESSION-06.json for details."
  -> Brief must be self-contained. Reader may not have repo access.

WRONG: "What Phase Built" as a file list
  "## What SESSION-06 Built
   - Created InMemoryIdempotencyStore.cs
   - Added 24 tests"
  -> This is a manifest, not a brief. Write what it MEANS for the platform.

WRONG: Including OPEN ITEMS when nothing needs deciding
  "## Open Items
   (none)"
  -> Omit the section entirely.
```

---

## Integration

```
Written by:      phase-completion-packager-SKILL.md (as the handoff artifact)
Read by:         Next planning conversation (paste at start)
Feeds from:      session-execution-log-SKILL.md (test counts and file lists)
Complements:     session-state-crystallization-SKILL.md (<=40-line snapshot for
                 within-session continuity vs. this skill's cross-session handoff)
                 investigation-handoff-SKILL.md (when the prior session was an investigation)
```
