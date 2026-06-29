# Debug Session (XIIGen Enterprise Platform)

## Overview

When a fix attempt fails during execution and the investigation spans multiple
sessions, starting the next session from scratch re-investigates paths already
eliminated. The debug-session file persists the investigation state: every
hypothesis with its ELIMINATED / CONFIRMED / INVESTIGATING status, the exact
command that proved or disproved it, and the confirmed API shapes that must not
be re-verified. A session that starts by reading the debug file instead of the
codebase resumes at the exact point debugging stopped.

Stack-bound to **.NET 10 ASP.NET Core Web API**, **xUnit** (`tests/Xiigen.*.Tests`),
`DataProcessResult<T>` (`Xiigen.Kernel`), and the platform fabric interfaces.
"Confirmed Facts" are exact C# signatures (sync/async, the `DataProcessResult<T>`
type argument, the concrete fabric resolved in DI), never `OperationResult<T>`
(that type is not used in platform `.cs` files). This debug-session layer is the
working layer over the same governance `state`/beacon described in `AGENTS.md`
(Binding Enterprise Adaptation Beacon) — it feeds, and is fed by, that state.

## When to invoke

- A fix attempt fails during execution (a test fails after applying the fix).
- A public method signature or `DataProcessResult<T>` return type differs from the plan.
- A C# compiler error cannot be resolved in 2 attempts.
- Context is about to overflow mid-investigation (write Theory + Next Action first).

## Sub-Agent-Only Parent Boundary

Under a sub-agent-only / architect-only instruction, the debug-session file is
maintained by the bounded sub-agent doing the investigation. The parent reads it
only after the packet returns, to verify/integrate. `sub-agent waiting is not a stop`.

---

## Section 1 — File Location and Lifecycle

**Location:** `.claude/debug/active-session.md` (fallback `docs/debug/active-session.md`
if the repo keeps debug artifacts under `docs/`).

**Lifecycle:**
```
1. CREATE the file when a fix attempt fails or an API shape is wrong
2. APPEND to the file after every hypothesis test
3. ARCHIVE the file when the fix is confirmed working:
   rename to .claude/debug/YYYY-MM-DD-[issue-description].md
```

**Load order:** at the start of any session where debugging was in progress, read
`active-session.md` BEFORE any other file (after the beacon/state read that
`AGENTS.md` requires).

```bash
ls .claude/debug/active-session.md 2>/dev/null && echo "ACTIVE DEBUG SESSION — read first"
# If it exists: read it first — skip the codebase read until the hypothesis is current
```

---

## Section 2 — File Format

```markdown
# Debug Session: [issue title]
**Date:** YYYY-MM-DD
**Session:** S[N]
**Phase:** [active SESSION-XX-*.md / matrix row reference]
**Issue:** [one-line description of what is failing]

## Hypotheses

### H1: [hypothesis title]
**Status:** ELIMINATED / CONFIRMED / INVESTIGATING
**Evidence for:** [what supports this]
**Evidence against:** [what disproves this]
**Elimination command:**
```bash
[exact command — e.g. dotnet test Xiigen.Platform.slnx --filter "FullyQualifiedName~X" --no-build]
```
**Result:** [exact output — not summarized]

### H2: [next hypothesis]
...

## Confirmed Facts (do NOT re-investigate)
- [symbol]: [exact signature] — e.g. IDocumentStore.GetAsync(string,string,CancellationToken)
  returns Task<DataProcessResult<DocumentRecord?>> (Success(null) when absent, NOT Failure)
- [service]: is [sync/async]; returns DataProcessResult<[T]> with [T] = [exact]
- [interface]: resolved as [concrete type] in Xiigen.Platform.Api/Program.cs line N

## Current Best Theory
[What the investigation currently believes — one paragraph]

## Next Action
[Exact next step to take on resume — specific enough to execute without re-reading history]
```

---

## Section 3 — Status Values

```
INVESTIGATING: active hypothesis — not yet tested
CONFIRMED:     evidence confirms this IS the cause
ELIMINATED:    evidence proves this is NOT the cause — do not re-investigate
```

An ELIMINATED hypothesis with no supporting evidence record is incomplete. If you
cannot cite the command and exact output that eliminated it, it has not been
eliminated.

---

## Section 4 — Confirmed Facts Protocol

The "Confirmed Facts" section is the most important part of the file. Every
confirmed fact prevents a future session from repeating work.

```
WRONG:  H3 was ELIMINATED (vague — why? what proved it?)
RIGHT:  H3 ELIMINATED: IIdempotencyStore.TryReserveAsync returns
        Task<DataProcessResult<IdempotencyReservation>>, not bool.
        Confirmed by reading src/Xiigen.Kernel/FabricContracts.cs line 37.
        grep -n "TryReserveAsync" src/Xiigen.Kernel/FabricContracts.cs → line 37
```

Add to Confirmed Facts immediately when any of these are verified:
- A method's exact return type, including the `DataProcessResult<T>` type argument
  and whether `Success(null)` is a valid "found nothing" result.
- A fabric interface's concrete implementation in DI registration.
- Whether a method is sync or async (most fabric methods return `Task<...>`).
- The exact key path for reading a nested value from `DocumentRecord.Body`
  (`IReadOnlyDictionary<string, object?>` — missing key is null).

---

## Section 5 — Integration with the platform session protocol

- **At preflight time (`phase-preflight-SKILL.md`):** check for an active debug
  session before writing any code.
  ```bash
  ls .claude/debug/active-session.md 2>/dev/null && echo "ACTIVE DEBUG SESSION — read first"
  ```
- **On context overflow:** before stopping, write "Current Best Theory" and
  "Next Action" — these survive compaction.
- **On completion:** archive only after `dotnet test Xiigen.Platform.slnx` passes
  with the fix applied AND the count is ≥ baseline. Archived sessions are the
  pattern library for `systematic-debugging-SKILL.md`.

---

## Anti-Patterns

1. **Starting from scratch each session** — the most expensive failure mode.
   ELIMINATED hypotheses get re-investigated, confirmed signatures re-verified.
   Create the file when debugging starts, not after the third failed session.
2. **ELIMINATED with no elimination evidence** — the next session cannot trust a
   status without the proving command + output.
3. **Current Best Theory left empty** — the theory at session end is the most
   valuable recovery point.
4. **Archiving before the fix is confirmed** — archive only after a green
   `dotnet test` with the fix and all pre-existing tests still passing.
5. **Reading the codebase before the debug file** — the debug file changes the
   read order. If H7 is CONFIRMED, read the fix site, not the whole project.

---

## Integration

- `phase-preflight-SKILL.md` checks for an active debug session before any code write.
- `code-examination-SKILL.md` provides the reading protocol; this skill tracks investigation state.
- `root-cause-tracing-SKILL.md` produces hypotheses; this skill tracks their status.
- `systematic-debugging-SKILL.md` archives provide the historical pattern library.
- `mental-debug-SKILL.md` supplies the symptom→rule routing each hypothesis tests.
