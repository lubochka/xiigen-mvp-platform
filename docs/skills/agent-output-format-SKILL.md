# Agent Output Format (XIIGen Enterprise Platform)

## The Three-File Rule

Every plan produces exactly three files:

| File | Audience | Purpose |
|------|---------|---------|
| `PLAN-[PHASE]-REFERENCE.md` | Humans (Luba) | What we're building and why — architecture, decisions, context |
| `XIIGEN-ENTERPRISE-ADAPTATION-STATE-v3.3.json` (or the state file the active SESSION-XX names) | Navigation — resuming sessions | Phase state, test baseline, completed deliverables, next step |
| `PLAN-[PHASE]-EXECUTION.md` (the active `SESSION-XX-*.md`) | The execution session | Step-by-step, file paths, code blocks, Gate B conditions |

Never merge these. Each file has one audience and one purpose.

Platform port of the `llm_mvp_core` skill. Stack-bound to **.NET 10 ASP.NET Core
Web API**, **xUnit** (`tests/`), `Xiigen.Platform.slnx`, and the v3.3 governance
triple. The REFERENCE/STATE/EXECUTION trio binds to the beacon loop: REFERENCE and
EXECUTION tie to the active `SESSION-XX-*.md`, STATE is
`XIIGEN-ENTERPRISE-ADAPTATION-STATE-v3.3.json`, and Gate-B is native
`dotnet build/test Xiigen.Platform.slnx`. The host-extension subset of the core
gates (the Claude/Cursor/Codex Full Extension / Final Execution Review gates) is
**`na`** here — the platform has no host extension and no frontend.

Every plan file set must also carry authority-chain requirement evidence:
Authority Requirements Ledger, Conversation Delta Ledger, Replacement Mapping
Register, and Contradiction Sweep. Requirements from the current human operator /
product owner, architect work order, approved active plan, or accepted reviewer
gate are binding unless that authority explicitly marks them optional,
exploratory, or preference-only. Removing an old mechanism is not coverage for a
required replacement: the plan must name the replacement capability, where it
lives, and the test that proves it.

## Bounded Sub-Agent Execution Output

Spawned bounded sub-agent is already the sub-agent executor. If the work order
gives allowed write scope, the receiving sub-agent must directly create or edit
those scoped artifacts in its workspace and then report evidence in the packet. It
must not attempt to invoke a nested CLI agent or another sub-agent to satisfy
"through sub-agents" unless the work order explicitly asks it to delegate. If the
sub-agent cannot write an allowed file for filesystem reasons, it reports the
exact path, attempted operation, and filesystem error. Parent direct execution
remains forbidden, and sub-agent execution is not parent direct execution.

## Machine-Only Status Is Invalid

Any phase, review, fix, or bounded sub-agent packet shown to Luba must include
phase/step meaning, counts, done/in-progress/remaining, review findings, fixes,
and next action before any machine label, JSON path, packet id, STATE code, or
validator code.

Keep evidence boundaries explicit. Build/test/contract evidence is not full
master-tenant readiness; an integration test is not a deploy proof; a schema is
not behavior. If the packet only has one side of proof, write the narrow claim and
`claim=false` for the broader claim.

Downgrade, blocker, not proven, not ready, needs verification, no claim, or
status-only artifact is not completion. Output must turn it into the next concrete
work item unless all requirements are already proven or Luba explicitly stops.
Separate `evidence_accounting_done` from `requirement_proven`.

For untracked new artifacts, a review may use current-state invariants + evidence
packet instead of an impossible git baseline. The packet must still list exact
paths, explicit counts, exact search terms, and `claim=false` for any
readiness/full-pass claim not proven.

No stop gates from protocol repair: after a bounded protocol/governance/skill
repair is reviewed, the output names the next bounded phase and continues unless
missing authority, unsafe action, or a real blocker remains.

---

## REFERENCE File Format

```markdown
# [Phase Name] — Reference

## Goal
One paragraph: what this phase accomplishes and why it matters.

## Architecture Decisions
- DR-01: [decision and rationale]
- DR-02: [decision and rationale]

## Component Map
| Component | Location | Interface |
|-----------|---------|-----------|
| InMemoryOutboxStore | src/Xiigen.Fabrics.InMemory/ | IOutboxStore |

## Dependencies
What must exist before this phase starts.

## What Success Looks Like
How Luba will know this phase is complete. Measurable outcomes.
```

---

## STATE File Format

```json
{
  "phaseId": "SESSION-06",
  "status": "IN_PROGRESS",
  "startedAt": "2026-06-01T10:00:00Z",
  "testBaselineAtStart": 181,
  "testCountNow": 199,
  "completedDeliverables": [
    "src/Xiigen.Fabrics.InMemory/InMemoryIdempotencyStore.cs",
    "tests/Xiigen.Fabrics.InMemory/IdempotencyStoreTests.cs"
  ],
  "pendingDeliverables": [
    "src/Xiigen.Fabrics.InMemory/InMemorySchedulerStore.cs"
  ],
  "nextStep": "STEP 3 — Write the scheduler store",
  "gateResults": {
    "gate0_discovery": "PASS",
    "gate1_result_type": "PASS",
    "gate2_fabric": "PASS",
    "gate3_tenant_scope": "PASS",
    "gate4_decisions": "PASS",
    "gate5_tests": "PASS",
    "gate6_docs": "PASS"
  },
  "phaseGatesPassed": true
}
```

---

## EXECUTION File Format

```markdown
# [Phase Name] — Execution

## SESSION GATE
- Build baseline: dotnet build Xiigen.Platform.slnx -> 0 errors
- Test baseline: dotnet test Xiigen.Platform.slnx -> 181 passed, 0 failed
- Branch: [current working branch]

---

## STEP 1 — [Description]

**File:** `src/Xiigen.Fabrics.InMemory/InMemoryIdempotencyStore.cs`

**Gate B:** `dotnet test --filter "FullyQualifiedName~IdempotencyStore" shows Failed: 0, Passed: >= 5`

**Code:**
[exact code block]

**Test:**
[exact test block]

---

## SESSION GATE (End)

```
dotnet build Xiigen.Platform.slnx -> 0 errors
dotnet test Xiigen.Platform.slnx --no-build -> Failed: 0, Total >= [baseline + N new tests]
```

## STATE UPDATE
[What to write to the state file after this step]

## STOP — hand off to REVIEW_INTERNAL (executor cannot self-finish)
```

---

## SESSION-0 Plan Review Gate

Before any execution plan is approved, run three gates.

### Gate A — Format Checks (FC)

```
FC-0: Does the file include an Authority Requirements Ledger mapping every binding
      requirement from the active human/product/architect/plan/reviewer authority
      to plan section + test/gate + artifact, with no unmapped, deletion-only, or
      contradicted rows?
FC-0a: Does the file include a Conversation Delta Ledger, Replacement Mapping
       Register, and Contradiction Sweep, and do required replacements map to
       actual replacement capabilities rather than deletion-only changes?
FC-0b: Does every status/count/sub-agent claim have evidence matching its exact
       scope: ledger rows are not cycles, parent/self review is not sub-agent
       review, and N cycles/reviews/passes require N independent evidence records?
FC-1: Does every STEP have a Gate B that is a command + expected output?
FC-2: Does every STEP reference a real file path under src/ or tests/ (not a placeholder)?
FC-3: Does every new fabric interface appear in the FabricContracts / DI registration update plan?
FC-4: Does the plan keep DataProcessResult<T> on every public method and never throw for business logic?
FC-5: Does every Step that creates a class include a test step?
FC-6: Does the plan include a final `dotnet test Xiigen.Platform.slnx` count check?
FC-7: Are all abbreviations expanded on first use?
FC-8: Does every code block carry a comment per line where the platform standard requires it?
FC-9: Does the plan list every file it will create or modify?
FC-10: Does the SESSION GATE section list the exact `dotnet test` command?
FC-11: Is the STOP / hand-off-to-REVIEW marker present before the phase transition?
FC-12: Is the STATE UPDATE section present after every session gate?
FC-31: If the output is architecture, code-review, documentation, or audit
       material, does it pass the Human-Readable Architecture Artifact Gate and
       Concrete Repair Blueprint Gate: human summary, UML importable to draw.io,
       simulations, no-code algorithms, exact code quotes, adjacent concrete
       solutions, and no schema-only or prompt/spec substitute? (see
       human-readable-architecture-audit-SKILL.md)
FC-32: If an audit claims sufficiency/readiness/completion, does it pass the Audit
       Sufficiency Boundary Gate (exact bounded scope, green breadth challenge,
       Delta-Severity Feedback Gate recorded for later same-domain misses)?
FC-39: Does every long execution status require the Human-Readable Execution Board
       Gate with total task count, completed count and names, current task and
       owner/sub-agent, remaining count and names, current result type, accepted
       evidence, closed claims with reasons, open gaps, next action, and execution
       pause state?
FC-39a: Does every execution report/status board row use the human-first contract
        before any opaque codes (phase/step meaning, status vocabulary
        DONE | IN_PROGRESS | NEEDS_FIX | REVIEWING | BLOCKED_WITH_REASON, total
        project position, done, in-progress, numbered remaining, review findings,
        fixes, next action, commit/continuation permission)?
FC-39b: Does every status reject bare matrix-row ids, `packet accepted`,
        `boundary pass`, JSON/state labels, or other machine-only codes unless
        translated into human meaning and done/in-progress/remaining counts?
FC-43: Does the completion format include the False Completion / Stop Repair Rule
       so "ready/done" without full requested-goal proof becomes plan/skills/gate
       repair plus continued review/fix work?
FC-45: Does every code slice review require the Execution/Code Review Completeness
       Gate: exact review scope, project standards/tests, fabric boundary, thin
       host adapters, hidden deterministic shortcut sweep, and docs/QA impact?
FC-46: Does every chat log/status/final-or-near-final report pass the human gate:
       Luba can tell what is done, what is running, what remains, current result
       type, closed claims and why, and next bounded action without opening
       machine logs?

`na` for this platform (host-extension gates from the core skill): FC-30 (allowed
trainable architecture level mapping), FC-33/FC-34/FC-35 (model/reasoning
gate, 10-review gate, execution-plan visibility gate for host-extension runtime),
FC-36/FC-37/FC-44 (Full Extension Definition / Final Execution Review / Full
Extensions Readiness for Claude, Cursor, Codex), and FC-38 (deterministic-boundary
trainable-unit review). These guard local trainable models and three live host
extensions that do not exist in this repository. If a future plan adds a host
extension or a local trainable model here, re-enable the corresponding gate.
```

### Gate B — AI Review

Get independent review from a second model / bounded sub-agent for plans that:
- Span more than one project (src) area
- Add or change a public fabric interface
- Introduce a new persistence or concurrency behavior

### Gate C — Luba Approval

Present the plan in plan mode. Wait for explicit written approval. Do not begin
until it is received.

---

## Anti-Patterns

1. **Merging REFERENCE and EXECUTION into one file** — audiences and purposes conflict
2. **EXECUTION without Gate B per step** — steps without gates have no completion criteria
3. **STATE without test count** — resuming sessions cannot establish regression baseline
4. **STOP / hand-off marker missing** — the executor continues past the phase boundary and self-finishes
5. **Placeholder file paths** — `src/TODO/` is not a file path; do not put it in EXECUTION
6. **Machine-readable audit as human plan** — table-only, shortcode-heavy, schema-only, or prompt/spec output is not a solution when Luba needs a human-readable artifact
7. **Narrow audit as complete audit** — "v1 scope completed" requires exact bounded scope; "audit sufficient/complete" requires a green breadth challenge
8. **Machine log replaces user status** — a STATE row, JSON report, hidden review transcript, or bare matrix-row id without phase/step meaning, status vocabulary, counts, review findings, fixes, next action, and commit/continuation permission is not a valid user-facing status
9. **Ledger rows called cycles** — a generated table with N rows is not N review/fix cycles; count claims require N independent evidence records
10. **Parent work called sub-agent work** — parent/self review cannot be described as sub-agent review without a named sub-agent packet
