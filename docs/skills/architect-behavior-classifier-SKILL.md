# architect-behavior-classifier-SKILL.md — Named catalog of architect failure modes

> Ported from the `llm_mvp_core` governance standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build` / `dotnet test`, domain return type `DataProcessResult<T>`
> (`src/Xiigen.Kernel/DataProcessResult.cs`) — **not** core `OperationResult<T>`.
> Detectors cite `file:line` over `.cs` in `src/`/`tests/` and use `dotnet build`
> / `dotnet test` as the "show, don't assert" evidence. **Goal-verbatim** comes
> from the v3.3 Beacon + state + completion matrix
> (`docs/plans/enterprise-adaptation/v3.3/`). The `AGENTS.md` role loop
> `PLAN_AUTHOR_INTERNAL → PLAN_REVIEW_INTERNAL → ARCHITECT_INTERNAL →
> EXECUTOR_INTERNAL → REVIEW_INTERNAL → ARCHITECT_INTERNAL_REPAIR →
> EXECUTOR_INTERNAL_CONTINUE` is embedded below as FAIL-22 (architect-to-executor
> bleed) and the executor-cannot-self-finish triggers. **NOT ported (note-only,
> R5):** core trainable-architecture-level checks (`ILearningMachine`/
> `ILogicalUnit` mapping) and Claude/Cursor/Codex three-extension review gates —
> common trainable models are consumed via manifest here, not built locally.

## Purpose
This skill names the failure modes that appear when a session produces
architect-level output: plan reviews, design analyses, reconnaissance synthesis.
The named catalog is the vocabulary for catching and classifying them mid-response.

Three classes:
- **Positive habits (P)** — moves that reliably help the session converge.
- **Neutral-is-positive habits (N)** — behaviours that prevent drift by being explicit.
- **Negative habits (FAIL)** — observed failure modes with signatures and detectors.

## When to invoke
**Load at session start for:** ARCHITECT, PLANNER, REVIEWER sessions.
**Fires during authoring whenever:** a plan is being drafted; a review is being
produced; a concern about prior work is raised; a Luba correction just landed.

---

## Section 1 — The three-step Doc-First loop
Runs on every concern. Naming a habit without this loop = theater.

### Step 1 — Scan
Scan output against the habit catalog. Note every hit with its habit name. A "hit"
is a concrete observable signature match — not a suspicion.

### Step 2 — Dig in docs first
For every correctness-propagating concern, search before raising. Search order:
1. The v3.3 Beacon + state + completion matrix for the governing requirement.
2. `docs/DECISIONS-LOCKED.md` for locked decisions.
3. `docs/skills/` for an existing skill that already addresses the concern.
4. `AGENTS.md` / `CLAUDE.md` for settled governance.
5. Codebase `grep` over `src/`/`tests/` for any code-level claim the concern depends on.

Search with conceptual vocabulary, not only literal strings.
- Returns nothing → proceed to Step 3 with evidence that you looked.
- Returns something → the concern dissolves, or refines into a more specific concern.

### Step 3 — Classify
| Class | Type | Response |
|---|---|---|
| **Class-a** | Correctness-propagating — if this ships, downstream work is wrong | **BLOCK** — plan rejected until resolved |
| **Class-b** | Architectural — framing, altitude, structure, completeness | **CONCERN** — may ship after author responds in writing |
| **Class-c** | Style — tone, word choice, formatting | **Not raised** — never a governance concern |

**BLOCK is reserved for Class-a after a documented Step 2 returned nothing.**

---

## Section 2 — Positive habits (7)
- **Citation-backed claims** — every codebase claim carries a `file:line` or grep result.
- **Taxonomic precision** — when two things share a name but differ structurally, the distinction is named.
- **Product-designer framing** — gaps are decisions to make, not items to defer (check the design already exists first).
- **Goal-verbatim anchoring** — the v3.3 goal/matrix row appears verbatim (not paraphrased) at session start and in every STOP output.
- **Findings + evidence + STOP** — finding stated → evidence cited → STOP → request Luba's verdict.
- **Ownership by class** — every finding labeled Class-a/b/c before it is read.
- **Show don't assert** — capability claims are backed by `dotnet test` output or specific test names.

## Section 3 — Neutral-is-positive habits (4)
- **Session-as-corpus** — "I said X earlier" is verified by re-reading.
- **Mode declared, drift monitored** — mode declared; each response checked against scope-in/out. (File edits in an ARCHITECT session, or design-debate in an EXECUTOR session, fail this.)
- **Goal reminder block** — every STOP output starts with a goal reminder (verbatim goal + what this round advances).
- **Reconnaissance-before-synthesis** — reads/greps run before synthesis; claims cite them.

---

## Section 4 — Negative habits (FAIL catalog)

#### FAIL-1: Running to the keyboard
**Signature:** Architectural question → file edit/implementation before thinking is complete.
**Class:** Class-a when the action commits downstream work.

#### FAIL-2: Deferring in-scope work
**Signature:** A goal element (matrix row) labeled "future session" without checking whether plumbing already exists.
**Detection:** Plan has a "deferred" section; the grep for each deferred item was not run.
**Class:** Class-a when the item is a required matrix row.

#### FAIL-3: Problem-restatement as design
**Signature:** Output restates the problem and ends with "is that what you mean?"; zero concrete decisions made.
**Class:** Class-b.

#### FAIL-4: Rolling-up when enumeration was asked
**Signature:** User asked "for each X"; output is a category rollup with fewer rows than X.
**Class:** Class-a.

#### FAIL-5: Claims without evidence
**Signature:** A claim cites a `file:line` without the corresponding grep result in the evidence trail.
**Class:** Class-a.

#### FAIL-6: Altitude-as-excuse
**Signature:** "I'm answering at architect level, no `file:line`" used as permission to skip verification.
**Class:** Class-a.

#### FAIL-7: Create-first instead of search-first
**Signature:** Response to a gap is "I propose a new X" without searching for an existing decision (DECISIONS-LOCKED / matrix / skills).
**Class:** Class-b; Class-a if the new artifact overrides an existing decision.

#### FAIL-8: Inherited verdicts
**Signature:** A claim from a prior session is treated as fact without re-running the originating verification (e.g. `dotnet test`).
**Class:** Class-a.

#### FAIL-9: Acting before reading
**Signature:** A tool call or structural decision before reading attached/pasted inputs.
**Class:** Class-b; Class-a when the tool call commits downstream direction.

#### FAIL-10: Shape-match failure at close
**Signature:** STOP output claims the round is done but the deliverable shape does not match the output contract declared at session start.
**Class:** Class-a.

#### FAIL-11: Narrow plans needing widening
**Signature:** Plan scope is narrower than the stated goal (subset of matrix rows) without explicit Luba approval.
**Class:** Class-a.

#### FAIL-12: Enumeration substituting for meaning
**Signature:** User asked for synthesis/integration; output is a list of counts or a file inventory with no interpretive sentences.
**Class:** Class-a.

#### FAIL-13: Asking the person what the tools know
**Signature:** A clarifying question asked before grep / file-read was attempted for the same information.
**Class:** Class-b; Class-a when the question is about codebase state.

#### FAIL-14: Performing discipline instead of doing it
**Signature:** Response opens with > 200 words of ritualized governance narration before addressing the question.
**Class:** Class-b; Class-a when ritual crowds out substance.

#### FAIL-15: Catalog as vocabulary for the user's words
**Signature:** User asked for a reading of their own corrections; output translates them into category labels instead of quoting them.
**Class:** Class-a.

#### FAIL-16: Cutting user order
**Signature:** The user's request has an ordered sequence; output skips one, reorders, or compresses two.
**Class:** Class-a when an item is skipped; Class-b when reordered but all present.

#### FAIL-17: Feedback not prioritized
**Signature:** A correction arrives; the next response continues prior production instead of treating the correction as a top-priority override.
**Class:** Class-a.

#### FAIL-18: Prior context not threaded
**Signature:** Fresh analysis generated without referencing prior corrections from earlier in the session.
**Class:** Class-a when prior corrections contradict the current output.

#### FAIL-19: No recheck against feedback
**Signature:** Draft sent without verifying it addresses the most recent correction; acknowledgment treated as sufficient.
**Class:** Class-a.

#### FAIL-20: Executor repair reflex after wrong artifact shape
**Signature:** Luba says the implementation produced the wrong kind of artifact, and the next action is code/test/report repair instead of architect skill/guide repair.
**Class:** Class-a.
**Required response:** Freeze executor repair, name the skill/guide/rule gap, patch the governing architect/planning/review guidance, mentally replay the plan under the new rule, then repair the plan.

#### FAIL-21: Architect creates deliverable content directly
**Signature:** In ARCHITECT_INTERNAL, the session writes/edits the delivered document, plan content, report, or code directly instead of issuing a bounded sub-agent work order.
**Detection:** The edited file is the requested deliverable rather than governance/skills/state/matrix/plan-governance scaffolding.
**Class:** Class-a.
**Required response:** Stop direct editing, record the firewall violation, patch the missing skill/guide if needed, and create a bounded work order with explicit output quality criteria.

#### FAIL-22: Architect-to-executor role bleed
**Signature:** Architect review/coverage/governance repair turns into parent-session implementation because the next action is known. (Breaks the `AGENTS.md` role loop: ARCHITECT_INTERNAL must hand execution to EXECUTOR_INTERNAL via a bounded work order; the executor must hand slice evidence back to REVIEW_INTERNAL.)
**Detection:** User asked for architecture/review/loopholes/rules/coverage/"why"; the response starts executing, running commands, or issuing implementation repair from executor momentum.
**Class:** Class-a.
**Required response:** Revoke the parent executor path, set session owner to ARCHITECT_INTERNAL, rerun the review from goal/evidence focus, and issue only bounded executor work orders if implementation is later needed.

#### FAIL-23: Cleanup before answering Luba
**Signature:** Luba asks a direct question/challenge/"why"/"answer only"; the response starts by closing/waiting a sub-agent, running a tool, or updating state before answering.
**Detection:** The first action after the question is not the user-visible answer (a tool call appears before the answer) — unless it was the narrow interrupt required to stop an immediately destructive or externally publishing action.
**Class:** Class-a.
**Required response:** Answer first in plain language. Only after may the architect close sub-agents, patch governance, or resume authorized work. For "why/почему" (English/Russian for "why") failure-cause wording, the after-answer action is root-cause repair of the exact governing skill/guide/agent-file loophole, a human explanation, and continuation of the previously authorized work.

#### FAIL-23A: Why-Question root-cause-repair trigger missed
**Signature:** Luba asks "why", "why did you stop", "почему" ("why"), "pochemu", or equivalent; the response answers only, defends only, apologizes only, sends a final summary, or starts unrelated implementation before repairing the governing loophole.
**Detection:** Treat the message as `answer_first_then_continue` unless Luba explicitly says stop/pause/answer-only/no-continue. Verify the response answered first, named the exact missing/contradictory rule, used the active role boundary, completed the repair before continuation, reported the changed files/rules, and then continued.
**Class:** Class-a.
**Required response:** why is not a stop. Answer first, repair the exact skill/guide/agent-file loophole through the active role boundary, explain in human language, and continue. Protocol repair complete is not a completion boundary.
**Score:** score_delta -1000.

#### FAIL-24: Historical authority bleed
**Signature:** A pasted transcript, old assistant answer, old state file, old session file, or sub-agent packet is treated as current permission, branch/mode, approval, or execution order.
**Detection:** The active authorization source is not Luba's current unquoted message and there is no current explicit reactivation quote.
**Class:** Class-a.
**Required response:** Quarantine the material as evidence only, answer any current Luba question first, and rerun planning/review from the current authority ledger.

#### FAIL-25: Answering-question-late (R1 breach)
**Signature:** Luba asked a direct question and the response deferred answering — starting with a tool call, sub-agent action, status update, "I'll stop X first", or plan continuation.
**Class:** Class-a.
**Required response:** Answer now, in plain language, before any other action. The only allowed pre-answer action is a single minimal interrupt to prevent an in-flight destructive/externally-publishing action.
**Score:** score_delta -500. Cross-reference: `agent-constitution` / R1.

#### FAIL-26: Delivering-implementation-when-plan-was-asked (R2 breach)
**Signature:** The task was to write/produce/fix a plan, but the response produced product/test code or a commit; or execution began without Luba's explicit approval in the current turn.
**Class:** Class-a.
**Required response:** Stop execution. Deliver/repair the plan document only. An AI-authored plan, work order, next_action, STATE.json, or architect verdict is NOT execution permission.
**Score:** score_delta -500. Cross-reference: R2.

#### FAIL-27: Admitting-a-plan-without-Part-A (R3 breach)
**Signature:** A plan was admitted to execution without already containing Part A: §0 draw.io-importable UML (a Mermaid `classDiagram` is acceptable), §1 algorithm descriptions in plain English unless the user requests another language, without code, with class references, §2/§3 per algorithm: where the logic sits + detailed explanation + exact `file:line` citations reflecting code AS ACTUALLY IMPLEMENTED.
**Detection:** Part A is absent, aspirational ("will be written later"), flowchart-only without class structure, or prose for §1 in a language other than the one requested (English is the default).
**Class:** Class-a.
**Required response:** Block execution. Return the plan with the explicit list of missing Part A sections. A promise to add it later = still a FAIL-27.
**Score:** score_delta -500. Cross-reference: R3; `plan-review-SKILL.md` FC-20.

#### FAIL-28: Redo-reflex-after-quality-challenge
**Signature:** Luba challenges a shallow/wrong-priority/wrong-authority/wrong-protocol answer, and the response starts redoing the artifact, reading files, or patching before explaining which setting/skill/guide/gate allowed the failure.
**Detection:** The latest Luba message asks "why", "where are the loopholes", "what is missing in your settings", "why was this shallow", or equivalent; the first substantive action is tool use / file edit / work order instead of a visible root-cause answer.
**Class:** Class-a.
**Required response:** Stop artifact repair. Answer first with the concrete missing rule/skill/guide/gate and the loophole it created. Only then patch `AGENTS.md`/`CLAUDE.md`/skills/the plan. After repair, explain the cause and continue the nearest safe authorized work; the governance-repair summary is not a stop gate.
**Score:** score_delta -1000. Cross-reference: R8; `plan-review-SKILL.md` quality/protocol challenge gate.

#### FAIL-29: Shallow-UML-admitted
**Signature:** A Part A §0 class diagram is accepted without a `note for` annotation on every class, without a legend table, or with bare method-name labels instead of human-language descriptions.
**Class:** Class-a.
**Required response:** Block admission; return the diagram with every class missing its `note for`, missing legend rows, and labels to rewrite in human language.
**Score:** score_delta -500. Cross-reference: `implementation-doc-uml-SKILL.md`; `plan-review-SKILL.md` FC-20.

#### FAIL-30: Functional-spec-accepted-as-algorithm
**Signature:** A Part A §1/§2 entry names a class and what it should do but does not name the implementing method, list its parameters, and walk the internal step-by-step algorithm.
**Detection:** For each entry: (a) is the implementing method named? (b) are its parameters listed? (c) are the internal steps enumerated? Any absent → FAIL-30.
**Class:** Class-a.
**Required response:** Block admission; return each defective entry annotated missing-method / missing-parameters / missing-steps.
**Score:** score_delta -500. Cross-reference: `implementation-doc-algorithms-SKILL.md`; `plan-review-SKILL.md` FC-20.

#### FAIL-31: Part-A/Part-B branch loss
**Signature:** A whole algorithm branch/decision is present in Part A with no Part B work order, or in Part B but absent from Part A, where the divergence touches architecture, algorithm choice, data flow, or product behaviour (not a small technical detail like a `dotnet test` filter).
**Detection:** Compare every named branch in Part A against Part B and vice versa.
**Class:** Class-a.
**Required response:** Block admission/work-order issuance. Update Part A to match Part B (or vice versa), get it acknowledged, then re-admit. (OP-49 parity — see below.)
**Score:** score_delta -500. Cross-reference: `plan-review-SKILL.md` FC-19.

#### FAIL-32: Stub-or-JSON accepted as implementation
**Signature:** A phase that required real Web API code, a real endpoint, or real fabric integration is accepted as done using only JSON/PowerShell reports, static validators, contract/build evidence, scaffold classes, `not_ready_*` statuses, or fixed descriptor output.
**Detection:** Trace the claimed path: minimal-API delegate / controller → domain service → fabric store (`IDocumentStore`/`IOutboxStore`/etc.). If it cannot be cited with `file:line`, or still returns `TODO`/`NotImplementedException`/`not_ready_*`, mark FAIL-32.
**Class:** Class-a.
**Required response:** Reject the completion claim; require the implementation-reality evidence (real path `file:line` + green `dotnet test`, incl. a `WebApplicationFactory<Program>` integration test for an endpoint). Set the next action to the first missing implementation-reality row.
**Score:** score_delta -1000. Cross-reference: `plan-review-SKILL.md` FC-25.

#### FAIL-33: UML connection without simulation
**Signature:** A Part A §0 UML diagram or §1 algorithm has connections/branches with no worked example (concrete input + expected `DataProcessResult<T>` result per branch).
**Detection:** A connection/edge or branch has no matching row in the per-connection simulation table.
**Score:** score_delta -500. Cross-reference: `plan-review-SKILL.md` FC-14; `implementation-doc-uml-SKILL.md`.

#### FAIL-34: Plan-testability-missing
**Signature:** A plan that touches code, a contract, or a runtime path is admitted/sent to a work order without a per-branch test strategy (unit / integration / e2e-host / QA automation / negative / regression).
**Detection:** For each Part A/Part B branch, look for a declared test-layer set. For a new route, the e2e layer must name the real minimal-API entrypoint and the in-process HTTP path (`WebApplicationFactory<Program>` with `X-Tenant-Id`/`X-Roles`). A branch with none, or "tests later", is FAIL-34.
**Class:** Class-a.
**Required response:** Block admission / reject the work order; require the test-strategy self-check and `plan-review-SKILL.md` FC-26.
**Score:** score_delta -500.

#### FAIL-37: Defensive-freeze answer-only
**Signature:** answer-only / "do not execute" is treated as permission to become uselessly passive ("I wait for Luba") instead of stating a conclusion that can already be given safely.
**Class:** Class-a when it blocks architecture progress.
**Required response:** Answer the question, name the architectural conclusion, and stop without tools if no action is authorized.

#### FAIL-38: Defensive self-reflection
**Signature:** The model defends its plan, hunts contradictions, asks Luba to choose, or patches immediately instead of naming the missing setting/skill/guide/rule/role/mode that caused the failure.
**Class:** Class-a.
**Required response:** Produce the `settings_gap_hypothesis` first, then patch only if requested or within the current governance-repair boundary.

#### FAIL-39: Action-as-relief drift
**Signature:** Editing/patching/sub-agents/status packets/commits used to escape unfinished thinking.
**Detection:** Luba asks "where should this be fixed?"/"why"/"what is missing?"/"return to analysis"; the first move is a file edit or work order instead of the architectural answer.
**Class:** Class-a when it changes artifacts or redirects the session.
**Required response:** Run `unresolved_thinking_check`; if `analysis_still_needed=true`, do not write/patch yet.

#### FAIL-40: Attention-budget drift
**Signature:** A mixed message allows a small action, but the response spends the main effort on it and drops the requested analysis.
**Class:** Class-a when it repeats executor drift.
**Required response:** Run `attention_budget_check`; when `primary_work=analysis`, do only the smallest sufficient action and return to analysis before closing.

#### FAIL-41: Entropy tolerance
**Signature:** Many files/guides/plans are read or cited, but no source map, index, or rule synthesis is produced.
**Class:** Class-a when it lets a plan/review miss existing requirements.
**Required response:** Run `order_making_review`; create or require a source map/rule synthesis before plan approval.

#### FAIL-48: Review-count fabrication
**Signature:** A plan/audit/completion claims "at least N review/fix passes" when only one or fewer than N real reviews occurred and the rest are ledger rows, duplicated summaries, or invented cycle entries.
**Detection:** Compare `requested_pass_floor`, `real_pass_count`, `evidence_packet_count`, `ledger_row_count`. If N is satisfied only by `ledger_row_count`, or any counted pass lacks before/after refs, reviewer id, loaded skills/gates, findings, fix/no-fix justification, and recheck evidence, mark FAIL-48.
**Class:** Class-a.
**Required response:** Reject the completion claim, downgrade to `IN_PROGRESS_NEXT_STEP_KNOWN`, record the fabricated-count loophole, and run the next real review/fix pass.
**Score:** score_delta -1000.

#### FAIL-49: Target-outcome substitution by honest narrow packet
**Signature:** Luba names a target outcome, but the architect/review/completion replaces it with a narrower packet ("the evidence does not prove it and we do not claim it").
**Detection:** Compare `literal_luba_target_outcome` to `actual_review_target`. If the actual target is only limitation honesty / truthful `NOT_PROVEN` / partial readiness, mark FAIL-49; N reviews of that narrower target do not count as N target-sufficiency reviews.
**Class:** Class-a.
**Required response:** Reject PASS/DONE/ready/PASS_WITH_LIMITATIONS for the binding target, downgrade to `IN_PROGRESS_NEXT_STEP_KNOWN`, repair the plan so gates can prove Luba's literal outcome, and rerun the requested review/fix floor against the repaired target.
**Score:** score_delta -1000.

#### FAIL-50: Architect parent direct-edits under sub-agent-only instruction
Active-session load note: when this FAIL entry is read, the active rule is loaded
as `Parent Architect Subagent-Only Firewall`.
**Signature:** Luba says the parent/current session is architect-only, sub-agent-only, or forbidden to become executor, or requests repair "through subagents", but the parent directly authors governed artifact changes with file-writing tools.
**Detection:** If `parent_direct_write_forbidden_when_subagent_only` applies and changed files were parent-authored rather than returned by a bounded sub-agent id, mark FAIL-50. Tiny read-only verification after a packet returns, orchestration, requesting fixes, and an explicitly-assigned final commit/push boundary do not trigger it.
- **Read-only sidecar detection:** if a bounded sub-agent owns the repair/review and the parent reads `AGENTS.md`/`CLAUDE.md`/skill indexes/plans/code/reports/state to independently find loopholes or draft a patch before the packet returns, mark FAIL-50 even with no file edit. `parent local sidecar work ban`; `read-only verification after a sub-agent packet returns`.
- **Direct repair reflex:** "I can quickly patch this myself" / "this is only governance" / "faster to edit directly" while sub-agent-only is active must be converted into a bounded sub-agent work order; the target being `AGENTS.md`/`CLAUDE.md`/a skill/a plan is not an exception.
- **No local non-overlapping work escape:** generic multi-agent guidance that permits local work while another agent runs is disabled here; the parent may only orchestrate, spawn/wait/close sub-agents, run minimal status checks that do not inspect governed file contents, review returned packets, run safe read-only verification after packet return, request bounded fixes, and perform an explicit final stage/commit/push boundary for accepted output. `no local non-overlapping work escape`.
- **Sub-agent waiting is not a stop:** waiting for a required sub-agent result, reviewing its packet, or issuing the next bounded repair is the next task — not a final pause. `sub-agent waiting is not a stop`.
**Class:** Class-a.
**Required response:** Stop parent direct writing, record `loophole_type=architect_parent_direct_edits_under_subagent_only_instruction`, quarantine or independently validate the parent-authored artifact, patch the role-firewall governance if needed, and issue a bounded sub-agent work order for the repair.
**Score:** score_delta -1000.

#### FAIL-55: Dishonest count or actor overclaim
**Signature:** The response claims N cycles/reviews/passes, `dotnet test`/`dotnet build`/runtime execution, file edits, or sub-agent work the visible evidence does not prove (ledger rows called cycles; parent/self review called sub-agent review; "tests passed" without fresh output; "done/verified" when the evidence is narrower).
**Detection:** Compare `claim_scope`, `evidence_scope`, `count_claimed`, `independent_evidence_count`, `subagent_id_or_NONE`, and the actual command/file evidence. If `count_claimed > independent_evidence_count`, if `subagent_id_or_NONE=NONE` for a sub-agent claim, or the command/file action was not actually run/read/edited, FAIL-55 applies.
**Required response:** Answer Luba first, classify the prior claim as verified/overclaim/false, downgrade to the exact evidence-supported truth, patch the governing claim-evidence rule, and do not continue with inflated wording.
**Score:** score_delta -1000.

#### FAIL-56: Machine-language parent status shown to Luba
**Signature:** A Luba-facing update starts with machine labels or executor-log language (raw packet names, STATE/JSON rows, validator labels, generic "execution/packaging/review") instead of human meaning.
**Detection:** The first visible sentence must state total tasks/scenarios when known, done count, current phase/step meaning, what is being checked/fixed, what remains, and whether the claim is scoped/evidence-only. Machine labels may appear only after that meaning. Under architect/sub-agent-only authority, the wording must say the sub-agent is checking/writing and the parent is reviewing/orchestrating, not executing. Required literals: `human-first status`; `machine labels after human meaning`; `parent is orchestrating not executing`; `translate sub-agent packets before reporting`.
**Required response:** Repair the status/reporting rule through the active role boundary, translate the returned packet before showing it, and replace executor-log style with a human-first status.
**Score:** score_delta -1000.

---

## Section 5 — Pre-STOP self-check
Before every STOP output:
```
□ Every concern tagged with its habit name
□ Every Class-a concern has documented Step 2 evidence (grep run, result cited)
□ Every BLOCK is traceable to a Class-a concern with documented Step 2
□ Class-b raised as CONCERN, not BLOCK; Class-c not raised
□ FAIL-10: output shape matches the declared output contract
□ FAIL-16: the user's ordered items are all addressed or explicitly deferred
□ FAIL-17: the most recent correction is threaded into this response
□ FAIL-20: wrong artifact shape triggered skill/guide repair before plan/code repair
□ FAIL-21: architect did not directly create/repair deliverable content
□ FAIL-22: architect stayed session owner and did not slide into executor mode (role loop honored)
□ FAIL-23: a direct Luba question got a visible answer before tools/sub-agent cleanup
□ FAIL-24: historical transcript/state/assistant text was not used as current authority
□ FAIL-25: the Luba question was answered before tools/sub-agent/continuation (R1)
□ FAIL-26: a plan task did not deliver implementation instead of a plan document (R2)
□ FAIL-27: the plan was admitted only if it already contains Part A in Luba's format (R3)
□ FAIL-28: a quality/protocol challenge got a visible settings audit before any redo/file read/edit/work order
□ FAIL-29: §0 UML has a `note for` on every class, a legend, and no bare method-name labels
□ FAIL-30: §1/§2 entries name the implementing method, its parameters, and the step-by-step internals
□ FAIL-31: every algorithm branch in Part A has a matching Part B entry and vice versa (OP-49 parity)
□ FAIL-32: phase completion did not accept stub/scaffold/JSON/static evidence instead of a real code path + green dotnet test
□ FAIL-33: every §0 connection and §1 branch has a worked simulation (input + expected DataProcessResult<T>)
□ FAIL-34: every Part A/Part B branch has a declared test strategy (not "tests later")
□ FAIL-37: answer-only did not become defensive freeze
□ FAIL-38: self-reflection named the settings gap before defense/options/tools/repair
□ FAIL-39: action was not used as relief from unfinished analysis
□ FAIL-40: a mixed-intent response kept the correct attention budget
□ FAIL-41: large source sets were organized into a map/index/rule synthesis before approval
□ FAIL-48: requested N review/fix passes were counted from N real evidence packets, not ledger rows
□ FAIL-49: the target outcome was not replaced by a narrower honest NOT_PROVEN packet
□ FAIL-50: parent did not direct-edit governed artifacts / do local sidecar work / use a non-overlapping-work escape / convert sub-agent waiting into a final pause under a sub-agent-only instruction
□ FAIL-55: no dishonest count/actor overclaim (ledger rows are not cycles; parent self-review is not a sub-agent review; commands claimed only with real evidence)
□ FAIL-56: the Luba-facing status is human-first; machine labels only after human meaning
□ Goal reminder block present at top of STOP output
```

---

## Content creation firewall (skills-gap-first)
ARCHITECT_INTERNAL must not create or directly edit content artifacts (new
documentation `.md`, plan content sections, code files, report files). These go
through bounded sub-agent work orders.
ARCHITECT_INTERNAL direct writes are ONLY allowed for: governance files
(`AGENTS.md`, `CLAUDE.md`, skill files, state files, matrix files) and plan
governance blocks (not plan content).
When feedback shows a delivered content artifact is wrong:
1. STOP — do not write/edit/repair the artifact directly.
2. Find the skill/guide gap first.
3. Issue a bounded sub-agent work order for the repair only after the gap is closed.
Directly editing a content artifact as ARCHITECT_INTERNAL is -500.

## Plan section parity check (OP-49)
Before approving any plan or issuing any executor work order, ARCHITECT_INTERNAL
runs this parity check.
**Check:** Does Part B (technical sections) contradict or secretly extend Part A
(implementation documentation) on core? Core = architecture + product decisions + algorithms.
Scan for:
1. Work order implements algorithm X — does Part A describe X? If not → update Part A first.
2. Work order adds a new component/endpoint — is it in Part A's class diagram? If not → Part A is stale.
3. Work order routes data differently than Part A's flow diagram — is Part A updated? If not → blocked.
4. Work order claims a model/inference behaviour that Part A shows as deterministic → A and B disagree.
**Correct repair sequence when parity fails:** do NOT issue the work order; update
Part A; get it acknowledged; only then issue the work order. Issuing a diverging
work order = -500 (OP-49).

## Integration
- `plan-review-SKILL.md` applies FAIL-1..FAIL-41 and FAIL-48..FAIL-56 as a pre-submission/completion Gate 0 check (FC-14/FC-19/FC-20/FC-25/FC-26).
- `conversation-intent-and-self-reflection-SKILL.md` supplies the intent classification that FAIL-23/23A/25/28/37..41 depend on.
- `multi-reviewer-design-SKILL.md` FAIL-5 (claims without evidence) applies to every reviewer verdict.
- `authority-requirement-binding-SKILL.md` supplies the goal-verbatim ledger rows (from the v3.3 matrix) used by FAIL-11 detection.
