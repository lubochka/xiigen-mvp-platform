# Plan Execution Feedback

> Ported from the `llm_mvp_core` planning standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit. Tests = xUnit
> (`dotnet test`) per project; metrics recorded in the active v3.3 STATE file.
> The feedback cycle plugs into the platform loop
> EXECUTOR_INTERNAL_CONTINUE → REVIEW_INTERNAL (see `AGENTS.md` / `CLAUDE.md`).
> Domain return type is `DataProcessResult<T>` (never `OperationResult<T>`).

## Purpose

After each phase, compare planned assumptions vs actual discoveries. Track how
often plans required **write-time fixes** (assumptions that were wrong and had to
be corrected while writing/reading the code).

Execution feedback also preserves **user-origin source evidence**: every user
prompt, feedback item, correction, rejection, user question, and assistant answer
that changes scope, gates, artifacts, evidence, or execution order must be
appended to the active plan's **User Prompt Source Ledger** before the next phase
is planned or reported complete.

Execution feedback must satisfy the **Human-Readable Plan/Execution Log Gate**:
Section A cannot be only machine rows — it must state, in plain English unless the user requests another language, the
stage, the current plan step, what changed, what remains, claims opened/closed,
and why the status is true (machine labels only after the human meaning).

When the discovery rate consistently exceeds 120%, the planning process is
under-investing in infrastructure discovery (Gate 0 / `phase-preflight-SKILL.md`).

## Metrics
```
Discovery rate = (write-time fixes actual) / (write-time fixes planned) × 100%
Test delivery rate = (tests delivered) / (tests planned) × 100%
```
A "write-time fix" is any plan assumption that turned out wrong and required a
correction when actually writing/reading the files (e.g. a fabric method
signature, a DI registration location in `Program.cs`, a `Results.*` shape).

## Interpretation Table
| Discovery Rate | Signal | Required Action for Next Plan |
|---------------|--------|------------------------------|
| ≤ 120% | Normal — planning was accurate | Proceed with standard Gate 0 |
| 121–200% | Gate 0 under-invested | Add explicit `file:line` verification to every plan element |
| > 200% | Plan assumptions are guesses | Do NOT write code without reading every referenced file first |

## Session End Measurement
After each phase, record in the active v3.3 STATE file:
```json
{
  "phase": "P2",
  "status": "COMPLETE",
  "planExecutionFeedback": {
    "writetimeFixesPlanned": 2,
    "writetimeFixesActual": 5,
    "discoveryRate": "250%",
    "signal": "GATE_0_UNDER_INVESTED",
    "examples": [
      "Assumed IDocumentStore.UpsertAsync had no expectedVersion param — it does",
      "Assumed the route was MapPost('/api/marketplace') — it was '/api/marketplace/listings'",
      "Assumed fabrics registered in Module.cs — they are in Program.cs via AddXiigenInMemoryFabrics()"
    ],
    "testsPlanned": 10,
    "testsDelivered": 8,
    "testDeliveryRate": "80%",
    "userPromptSourceEntriesAdded": 0,
    "unmappedUserPromptSourceEntries": 0,
    "note": "2 integration tests deferred — fixture not ready"
  }
}
```

## Project History Tracking
Keep a rolling table across phases. **Pattern detection:** if 2+ consecutive
phases have discovery rate > 120%, the planning process has a systematic problem
— almost always: file paths referenced without verification (Gate 0 not run),
fabric/route signatures assumed without reading the contract, or DI registration
location assumed without checking `Program.cs`.

## Rules
1. Record WF planned vs actual in the v3.3 STATE file after every phase.
2. Record every plan-shaping user prompt / feedback / question / answer in the User Prompt Source Ledger.
3. If discovery rate > 120% for two consecutive phases: the next phase plan MUST include `file:line` citations for every planned element before any code is written.
4. If test delivery rate < 80%: investigate whether tests were deferred for a real reason or because the plan over-counted (`[Fact]` description blocks vs actual test methods).
5. Report the discovery rate and User Prompt Source Ledger deltas in the Section A summary.

## Section A Format (Phase End Summary, English by default)
```
## Section A — Phase [N] Execution Summary

Human meaning: <what this phase delivered to the product, in one sentence>
Stage / current step: <stage / step>

Deliverables:
  - [every file delivered]

Test gate: [N] passed before → [N+M] passed after → +[M] new xUnit tests, 0 failures
           (dotnet test --filter "...")

Write-time fixes: [N] (planned: [M])
  Discovery rate: [N/M × 100%] — [NORMAL | GATE_0_UNDER_INVESTED]
  [If > 120%: top 2-3 examples of what the plan got wrong]
User prompt source entries added: [N]
Unmapped user prompt source entries: [0 required]

Deviations from plan: [none | describe]
Claims opened/closed: [opened] / [closed]
What remains and why: [plain English unless the user requests another language]
Next bounded action: [...]
```

## Anti-Patterns
1. **Not recording write-time fixes** — cannot improve planning without data.
2. **Under-counting planned WF** — "I planned for 0 surprises" is itself a planning failure.
3. **Accepting > 120% as normal** — two consecutive high-rate phases demand a process change.
4. **Attributing high discovery rate to "complex task"** — complexity is not an excuse; complex tasks need deeper Gate 0.

## Integration
Pairs with `phase-preflight-SKILL.md` (the discovery step whose accuracy this
measures) and `phase-planning-SKILL.md` Gate 0.
