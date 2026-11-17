# plan-review-SKILL.md — Check a plan before implementation

> Ported from the `llm_mvp_core` planning standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build` / `dotnet test`. This skill is bound to the existing `CLAUDE.md`
> v3.3 Beacon and the loop
> `PLAN_AUTHOR_INTERNAL → PLAN_REVIEW_INTERNAL → ARCHITECT_INTERNAL →
> EXECUTOR_INTERNAL → REVIEW_INTERNAL → ARCHITECT_INTERNAL_REPAIR →
> EXECUTOR_INTERNAL_CONTINUE`. It also **raises the prose soft-stop ban from
> `CLAUDE.md`/`AGENTS.md` into a checkable FC checklist** (FC-EX below). The
> reviewer checks ASP.NET Core DI boundaries and that the domain return type is
> `DataProcessResult<T>` (`src/Xiigen.Kernel/DataProcessResult.cs`) — **not** core
> `OperationResult<T>` (which stays in `llm_mvp_core`).

## When to load
- Load for ARCHITECT / PLAN_AUTHOR sessions before writing a plan.
- Load for PLAN_REVIEW / REVIEW sessions when evaluating a submitted plan.

## Gate 0 — Reviewer Reads The Plan Directly (first-class)

Plan review is a first-class sibling of planning. The reviewer must **read the
target plan and cited references directly** before accepting, rejecting, or
summarizing any writer/sub-agent report. A writer report, state file, old plan,
or sub-agent packet is **evidence only** until direct read-back confirms it.

Required review packet for every finding:
```
Gate 0 verdict: PASS | FAIL
target_plan_read_directly=true
cited_refs_read_directly=true
writer_report_is_evidence_only=true
exact_line_or_absence_evidence:
impacted_requirement:
impacted_skill_guide_or_plan_section:
required_fix:
bounded_subagent_fix_target_owner_when_repair_authorized:
completion_evidence_after_fix:
re_review_evidence_after_fix:
re_review_trigger:
```
A review that only paraphrases a writer packet or says "looks covered" without
direct source evidence **fails Gate 0**.

## Dishonest Claim Rejection Gate

Reject any plan, status, packet, or final response that inflates the evidence
behind a claim.
```
claim_scope:
evidence_scope:
evidence_type:
subagent_id_or_NONE:
count_claimed:
independent_evidence_count:
ledger_rows_counted_as_cycles=false
parent_self_review_counted_as_subagent_review=false
commands_claimed_without_execution=false
downgrade_or_repair_required:
```
Hard fail when: "N cycles/reviews/passes" is claimed without N independent
evidence records; checklist/generated/ledger rows are counted as cycles;
parent/self review is counted as a sub-agent review; a sub-agent claim omits id /
work order / returned packet / reviewed scope / acceptance; or `dotnet test` /
`dotnet build` / runtime probe / git action / file edit is described as executed
without actual execution evidence. The reviewer must downgrade the claim to the
exact evidence-supported wording or require governing skill/guide/agent-file
repair before approval.

## Current-Task Anchor / Benchmark Focus Review Gate

Reject any parent status update, sub-agent prompt, writer work order, or
benchmark-comparison packet that lacks:
```
CURRENT TASK:
NOT THE TASK:
EVIDENCE-ONLY:
```
Reject when any active task sentence, target artifact, work order, cleanup,
continuation, repair scope, or git scope came from `evidence_text_only` (a quoted
transcript, attachment, old plan, or sub-agent packet) without exact reactivation
in Luba's latest unquoted message. Continuation/do-not-stop rules do not apply to
benchmark evidence artifacts.

## FC — Failure Classes (run the admission gates before the technical gates)

Run **FC-20, FC-14, FC-15, FC-16, FC-19, FC-26, FC-EX** before the
technical-feasibility checks. A fail on any admission-blocking FC is a HARD FAIL:
return the plan with the missing-section list; admission is forbidden until fixed.

### FC-0 — Authority Requirements Ledger traceability
Every plan must start from authority-chain requirements (current Luba instruction
/ v3.3 Beacon / work order / reviewer gate), recorded in an Authority
Requirements Ledger + Conversation Delta Ledger + User Prompt Source Ledger, with
every requirement mapped to plan section + gate/test + artifact, and every
rejected/superseded mechanism mapped to its replacement. Reject if any ledger is
missing, any binding requirement is unmapped, a mechanism was removed without its
replacement, or any plan sentence contradicts a binding requirement.

### FC-20 — Readable-Plan Admission Gate: **HARD FAIL — admission blocked**
The plan ALREADY CONTAINS (not merely promises) Part A:
- §0 — Mermaid `classDiagram` (draw.io importable), `note for` (English unless the user requests another language) on every class, legend table.
- §1 — Algorithm descriptions in plain English unless the user requests another language, WITHOUT code fragments, WITH class references.
- §2 — Per algorithm: exact file + class + method + parameters + step-by-step algorithm (not a functional summary), human analogy, concrete example values, "what it does NOT do".
- §3 — Exact short `path:line` citations reflecting code AS ACTUALLY IMPLEMENTED; every threshold/condition has WHY; every non-obvious call has HOW.
- §4 — Status table: one row per component in the affected area, status symbol + concrete "what works / what is stub".

Hard fail when Part A is absent, present as a heading only ("will be written in a
future documentation phase"), §0 is a flowchart image instead of a class diagram,
§0 lacks `note for` / legend, or §1 does not use the required language (English by default, or the language requested by the user). (§5 hierarchy is
core-specific / not in this group's scope — omit unless the plan touches an
ILearningMachine-style hierarchy.)

### FC-14 — UML diagrams (per-connection simulation): **HARD FAIL**
§0 has `note for` on every class + legend; §1 flowcharts ask human questions; AND
there is a worked simulation for **every §0 connection** and **every §1 branch**
(concrete input + expected `Results.*` / `DataProcessResult<T>` result). See
`implementation-doc-uml-SKILL.md`.

### FC-15 — Algorithm descriptions: **HARD FAIL**
Every component in §2 has a human analogy, concrete example values, a "what it does NOT
do" statement, a `**Signatures and data types:**` block with `DataProcessResult<T>`,
and a status symbol. See `implementation-doc-algorithms-SKILL.md`.

### FC-16 — Code references: **HARD FAIL**
Every §3 code block has WHY for every threshold/HTTP-status/condition, HOW for
non-obvious calls, and example values; citations are precise `path:line` for
`Program.cs` delegates / domain services as actually implemented. See
`implementation-doc-code-refs-SKILL.md`.

### FC-17 — Status table
§4 has a row for every component in the affected area with a valid status symbol
(no `Bootstrap` — core-specific) and concrete "what works / what is stub". See
`implementation-doc-status-SKILL.md`.

### FC-19 — Plan section parity (Part A == Part B on core): **HARD FAIL**
Bidirectional branch-level parity. Direction 1 — Part B must not diverge: Part A
says a decision/algorithm/route shape → Part B implements a different one = FAIL;
Part B adds a component/route/decision not in Part A = FAIL. Direction 2 — Part A
must not diverge: a Part A branch/component/data-flow leg with no covering Part B
work order/gate = FAIL. Part B may add **only** small technical details (exact
`dotnet test` filters, gate numbers, CI steps, error-handling/retry, performance
thresholds) without failing. A whole missing branch in either direction is a FAIL.

### FC-25 — Implementation Reality / Anti-Stub Completion Gate
For any implementation/code/phase-done claim: the returned evidence must prove the
**actual implementation path** (file:line from the minimal-API delegate through
the domain service to its result), not only static artifacts. Hard fail when JSON,
PowerShell, report presence, manifest fields, or build/test alone are used as
completion evidence for a phase that asked for real Web API / fabric code, or when
the claimed path still returns `not_ready_*` / `TODO` / `FIXME` /
`NotImplementedException`. A non-PASS completion claim sets
`IN_PROGRESS_NEXT_STEP_KNOWN` with the first missing implementation-reality
evidence as next action.

### FC-26 — Plan Testability Strategy: **HARD FAIL**
Every Part A/Part B branch declares a test strategy across: unit, integration,
e2e/host, QA automation, negative, regression. For a new route the e2e/host layer
names the real minimal-API entrypoint and the in-process HTTP path
(`WebApplicationFactory<Program>` with `X-Tenant-Id` / `X-Roles` headers — this
repo has no UI). Hard fail when any branch has no test strategy or the plan says
"tests later".

### FC-EX — Execution-Control + Soft-Stop Ban (lifts `CLAUDE.md`/`AGENTS.md` prose into a check)
This FC raises the platform's prose governance into a checkable gate.
Reject the plan / status / packet when:
- it lacks `current_execution_pointer` + `ordered_phase_queue` with exact
  `phase.step.status` rows (`done | in-progress | remaining | review-block |
  blocked`) and the first-unfinished-row-is-the-only-target rule;
- a Luba-facing status omits the human-readable block (phase/step meaning + counts),
  in English or the language requested by the user, before machine labels;
- it treats a **soft stop** as completion: "commit done", "report written", "one
  child session/test passed", "AWS URL produced", "screenshot captured", "context
  getting long", or "I will continue next" **as a final pause** while an
  authorized next action is known. Per `CLAUDE.md` "Executor Cannot Stop The
  Plan": the executor produces slice evidence and hands off to REVIEW_INTERNAL;
  if review finds a gap, control moves to ARCHITECT_INTERNAL_REPAIR then back to
  EXECUTOR_INTERNAL_CONTINUE. Only an architect verdict with `stop_allowed=true`
  and a completion matrix with no required `UNKNOWN`/`FAIL` rows authorizes a
  final report;
- it uses `next_action` / STATE / an old "continue" as execution authorization
  for a plan-only deliverable (a fresh current-turn Luba approval quote is
  required).

## Plan-Not-Ready Is Repair Loop, Not Stop

Do not approve execution of an unready plan, but do not turn the finding into a
final pause. The correct outcome is: return to architecture/planning repair with
the required skills/context (bounded sub-agents when the boundary is
sub-agent-only), repair the exact active plan/scenario artifact, run the required
clarity/review passes, and resume the nearest authorized execution only after the
plan-ready criteria pass. If a premature executor already began, stop it, treat
its output as quarantine evidence, and replace it with bounded governance repair.

## Three-Gate Approval

A plan is approved for execution only when all three pass:
1. **Admission gate** — FC-20, FC-14, FC-15, FC-16, FC-19, FC-26, FC-EX all PASS (Part A present + parity + test strategy + execution-control).
2. **Traceability gate** — FC-0 PASS (every authority requirement mapped; no contradiction; no deletion-only coverage).
3. **Honesty gate** — Gate 0 + Dishonest Claim Rejection PASS (reviewer read the plan directly; no inflated counts; no soft-stop completion).
Any FAIL → return the plan with the exact missing items; admission forbidden until fixed. Approval to execute still requires Luba's fresh current-turn quote (FC-EX / Plan-Only Approval Quote Gate).

## Integration
Enforces the admission depth defined by the implementation-doc skills
(`implementation-doc-uml/algorithms/code-refs/status`) and
`uml-simulation-to-testcase-SKILL.md`; pairs with `phase-planning-SKILL.md`
(authoring-time gates) and `planning-session-startup-SKILL.md` (startup ledgers);
routing for plan-quality/benchmark work: `codex-planning-quality-GUIDE.md`.
