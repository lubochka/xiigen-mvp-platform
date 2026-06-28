# Session Output Contract (XIIGen Enterprise Platform)

## Core Problem

A session that starts without defining what "done" means will either over-deliver
(produces things not requested, consuming time) or under-deliver (misses implied
deliverables, causing rework). Both failure modes recur across sessions. The
output contract is the zeroth deliverable — it must exist before any code is
written or any file is read.

Platform port of the `llm_mvp_core` skill. Stack-bound to **.NET 10 ASP.NET Core
Web API**, **xUnit** (`tests/`), `Xiigen.Platform.slnx`, `DataProcessResult<T>`,
and the v3.3 governance triple. Verification commands are native: `dotnet test
--filter`, `dotnet build Xiigen.Platform.slnx` (Gate-B), `ls`/`grep`/`Test-Path`.
The host-extension Dual-Path Host Verification material from the core skill
(Claude/Cursor/Codex live-host scenario counters) is **`na`** for this repository —
the platform has no host extension and no frontend.

---

## When to Invoke

- Before any session starts — produce the contract before reading any files
- Before writing any phase plan (to scope what the plan must contain)
- Before claiming a session complete (verify against the contract)
- When a session begins with "write me a plan for..." or "implement SESSION-XX"

---

## Section 1 — What an Output Contract Is

A machine-verifiable specification of exactly what "done" means for this session.

Three parts:

### Part 1 — Exact File List

```markdown
## Output Contract — [Session Name]

### Deliverables
| # | File | Must contain |
|---|---|---|
| 1 | src/Xiigen.Fabrics.InMemory/InMemoryIdempotencyStore.cs | Reserve + mark path returning DataProcessResult |
| 2 | tests/Xiigen.Fabrics.InMemory/IdempotencyStoreTests.cs | reserve/replay/expiry/tenant-isolation tests |
| 3 | docs/plans/enterprise-adaptation/v3.3/XIIGEN-ENTERPRISE-ADAPTATION-COMPLETION-MATRIX-v3.3.md | row updated for the implemented capability |
```

No ambiguity. No "and any other relevant files." Every deliverable is named, or
the session is not scoped correctly.

### Part 2 — Verification Commands

For each deliverable, one verification command that produces a binary result:

```bash
# Source file exists and has the required member:
grep -n "TryReserveAsync" src/Xiigen.Fabrics.InMemory/InMemoryIdempotencyStore.cs
# Expected: 1+ hits

# Tests are discovered and pass:
dotnet test Xiigen.Platform.slnx --filter "FullyQualifiedName~IdempotencyStore"
# Expected: Failed: 0, Passed: >= 1

# Test baseline preserved (Gate-B):
dotnet test Xiigen.Platform.slnx --no-build 2>&1 | tail -3
# Expected: baseline count or higher, Failed: 0
```

### Part 3 — Specification-Level Question

After verification commands pass, answer this before claiming done:

> "Does this deliverable match what was originally requested?"

Not: "Are the file internals consistent?" Not: "Does the content make sense?" The
question is about the original request. Re-read the request. Does the deliverable
satisfy it exactly?

---

## Section 2 — Contract Format

```markdown
## Output Contract — [Session Name]
Requested by: [reference to user instruction / active SESSION-XX file]
Session type: IMPLEMENTATION | PLANNING | SKILL_PORT | REVIEW

### Deliverables
| # | File | Verification command | Expected output |
|---|---|---|---|
| 1 | [path] | [command] | [expected result] |
| 2 | [path] | [command] | [expected result] |

### Specification gate (run last, before claiming done)
- [ ] Re-read the original request
- [ ] Every named deliverable in the request exists
- [ ] No deliverable was silently merged into another file
- [ ] Files updated from memory of prior content were verified against the original source
- [ ] Every count claim is backed by matching independent evidence records
- [ ] Every sub-agent claim names the sub-agent id/nickname and returned packet
- [ ] Ledger/checklist rows are not described as cycles
- [ ] Spawned bounded sub-agent is already the sub-agent executor when a work
      order gives allowed write scope; the sub-agent directly creates or edits
      scoped artifacts and reports evidence instead of invoking nested agents
- [ ] Parent direct execution remains forbidden, and sub-agent execution is not
      parent direct execution
- [ ] Every execution report/status board row is human-first (Russian where the
      governance asks for it) and includes phase/step meaning, status
      (DONE | IN_PROGRESS | NEEDS_FIX | REVIEWING | BLOCKED_WITH_REASON),
      total project position, done, in-progress, numbered remaining, review
      findings, fixes, next action, and whether commit/continuation is allowed —
      before any opaque code, JSON, or STATE label
- [ ] Machine-only statuses are invalid: the packet includes phase/step meaning,
      counts, done/in-progress/remaining, review findings, fixes, and next action
      before any machine label, path, packet id, JSON, STATE, or validator code
- [ ] Plan-readiness continuation is explicit: do not stop is not executor
      authorization; plan-not-ready returns to planning repair, not execution;
      a premature executor must be stopped and replaced by bounded governance
      repair; the packet shows human-readable phase counts before execution
      continuation
- [ ] No stop gates from protocol repair: after bounded protocol/governance/skill
      repair review, the next bounded phase remains the next action unless
      missing authority, unsafe action, or a real blocker remains
- [ ] Downgrade, blocker, not proven, not ready, needs verification, no claim, or
      status-only artifact is not completion; it creates a next concrete work item
      unless all requirements are already proven or Luba explicitly stops
- [ ] dotnet test baseline >= pre-session baseline (zero regression)

### What this session does NOT produce
[Explicit list of in-scope-but-not-this-session items]
```

The "does NOT produce" section is as important as the deliverables list. It
prevents scope creep ("while I'm here I'll also fix...") and prevents the next
session from assuming the current session already did something it didn't.

`na` for this platform: the core skill's extension/runtime Dual-Path Host
Verification deliverables and scenario counters (total scenarios, Path A, Path B,
blocked-with-next-action) do not apply here. The platform is an ASP.NET Core Web
API with no host UI and no Claude/Cursor/Codex extension. The platform's
end-to-end proof path is an integration xUnit test (for example through
`WebApplicationFactory`), not a live-host screenshot path.

---

## Section 3 — Read-Before-Write Rule

If a deliverable updates an existing file, the source file must be read completely
before any character is changed. Updating from memory produces the prior version,
not the current state.

```
□ For each file in the deliverables list that is an UPDATE (not a new file):
  -> Read the complete file before writing
  -> Record one specific fact from the end of the file as proof
  -> Only then: apply the update
```

---

## Section 4 — Two-Deliverable Minimum Verification

A session that produces one deliverable can verify it by reading it. A session
that produces multiple deliverables must verify each independently.

If any deliverable cannot be verified independently — either it is incomplete, or
it is not a real deliverable (it should be part of another file).

For skill-port batches specifically: each skill file and the index/registry update
are independent deliverables. Verify them separately:
```bash
# New skill file:
ls docs/skills/[new-skill]-SKILL.md
# Expected: file exists

# Index updated (if the active session names an index deliverable):
grep -c "[new-skill]" docs/HOW-TO-USE-SKILLS.md
# Expected: matches the index contract for that session
```

---

## Anti-Patterns

1. **"Starting to write code before the output contract exists"** — the contract
   is the zeroth step. It exists before reading any files. Code is at least the
   second step.
2. **"Output contract written as prose: 'This session will produce updated
   services...'"** — prose cannot be verified. Name the files. Write the
   verification commands.
3. **"'And any other relevant files' in the deliverables list"** — this is not a
   contract. It is permission to produce anything. If it's relevant, name it.
4. **"Claiming done before running verification commands"** — run them. If they
   don't pass, the session is not done.
5. **"Two deliverables merged into one file without noting it"** — the contract
   must list each requested deliverable separately. Silent merging is a scope
   violation.
6. **"N ledger rows means N cycles"** — false. The contract must require N
   independent evidence records for N cycles/reviews/passes.
7. **"Sub-agent did it" without a sub-agent packet** — false. Name the sub-agent
   id/nickname and returned packet, or downgrade to parent/self work.
8. **Opaque execution status as the user report** — false. A bare matrix-row id,
   `packet accepted`, `boundary pass`, or a JSON/STATE code is not the output
   contract unless it is preceded by the human-readable
   phase/step/count/review/fix/next-action contract above.

---

## Integration

- `agent-output-format-SKILL.md` governs what SESSION files look like internally; this skill governs whether the right files are produced
- `verification-before-completion-SKILL.md` enforces the specification gate at session end
- `plan-self-validation-SKILL.md` Phase 0 produces the output contract for a multi-phase plan
- `documentation-sync-SKILL.md` / `documentation-sync` uses the contract's deliverables list to verify sync completeness
- the read-before-write rule of Part 3 enforces complete-read before any UPDATE
