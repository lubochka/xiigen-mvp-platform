# Skill Injection (XIIGen Enterprise Platform)

## Overview

Governs how skill files are selected and injected as context into planning,
generation, and execution steps. Without injection, produced code may violate DNA
rules or miss required patterns. Platform port of the core skill-injection
contract: a cap on core blocks, mandatory-additional blocks that are never
dropped, activation rules, and the bounded sub-agent work-order boundary.

Stack-bound to **.NET 10 ASP.NET Core Web API**, **xUnit** (`tests/`),
`DataProcessResult<T>`, and the platform fabric interfaces. The skill blocks map
to the platform skill set: `SK-DNA` → `dna-compliance-guard-SKILL.md`
(+ `DNA-DOTNET-TRANSLATION.md`), `SK-TEST` → `self-verification-SKILL.md` +
xUnit unit/integration, `SK-FAB` → the platform fabric interfaces, `SK-VERIFY` →
`generated-code-review-SKILL.md` + `verification-before-completion-SKILL.md`,
`SK-DOCS` → `documentation-sync` (when present).

Коротко для архитектора (RU): в рабочем приказе и пакете возврата нельзя
ограничиться короткими метками `SK-PLAN` / `SK-REVIEW`. Человеческим языком
назови, какие файлы/секции проверяются, что прочитано, где граница артефакта, и
почему это не разрешает прямое исполнение родительской сессией.

## Binding order on this platform

Injection is subordinate to the beacon (`AGENTS.md` / `CLAUDE.md` /
`HOW-TO-USE-SKILLS.md`):

1. Beacon → 2. STATE → 3. completion matrix → 4. active `SESSION-XX-*.md` →
5. only then the relevant skills.

No injected skill may authorize a final report, soften a stop gate, or let the
executor self-finish; the role loop
`PLAN_AUTHOR_INTERNAL → PLAN_REVIEW_INTERNAL → ARCHITECT_INTERNAL →
EXECUTOR_INTERNAL → REVIEW_INTERNAL → ARCHITECT_INTERNAL_REPAIR →
EXECUTOR_INTERNAL_CONTINUE` is binding.

---

## Injectable Skill Blocks: Core Candidates and Conditional Add-Ons

| Block ID | Platform skill file(s) | Content |
|----------|------------------------|---------|
| SK-DNA | `dna-compliance-guard-SKILL.md` (+ `DNA-DOTNET-TRANSLATION.md`) | 10 DNA rules + `.cs` grep detectors |
| SK-FAB | `self-verification-SKILL.md` + the platform fabric interfaces | Fabric coverage + interface contracts (`IDocumentStore`, `IIdempotencyStore`, `IOutboxStore`, `ITenantContextAccessor`, `ISchedulerService`) |
| SK-PLAN | `planning-session-startup-SKILL.md` + `plan-review-SKILL.md` (authority + source map) | Pre-code gates, requirement binding, source map |
| SK-REVIEW | `plan-review-SKILL.md` + `claim-verification-SKILL.md` + `output-skepticism-SKILL.md` + `coverage-completeness-gate-SKILL.md` | Plan review, correction, claim verification, final readiness sweep |
| SK-TEST | `self-verification-SKILL.md` (+ xUnit unit/integration) | Unit + integration + suite (`dotnet test Xiigen.Platform.slnx`) |
| SK-DOCS | `documentation-sync` (when present) + `HOW-TO-USE-SKILLS.md` | File → canonical docs map |
| SK-VERIFY | `generated-code-review-SKILL.md` + `verification-before-completion-SKILL.md` | 5-layer generated-code review + runtime-evidence firewall: reject validation_only/static/contract-only/UNKNOWN; require numeric metrics |
| SK-QA | `self-verification-SKILL.md` + integration tests + `AGENTS.md` role loop | Engineering test strategy: unit/integration/e2e-host/negative + untrained-honesty + trained-evidence |

**Hard limit: maximum 3 core candidate skill blocks per generation call.**
Injecting more than 3 core blocks truncates the core instructions in the context
window.

**SK-VERIFY is mandatory-additional.** Whenever the work touches a trainable unit,
OR makes any "phase done"/completion claim on an algorithm/model, OR reviews
produced code, SK-VERIFY is injected IN ADDITION to the cap and is NEVER dropped.
Skipping it for trainable work is the V20 failure (json/stub passed as phase-done).

**SK-QA is mandatory-additional.** Whenever a work order writes/changes code, a
trainable unit, a host/API endpoint, OR makes any runtime/e2e claim, SK-QA is
injected IN ADDITION to the cap (alongside SK-VERIFY) and is NEVER dropped. A
code/host/trainable work order without SK-QA has no declared test strategy.

---

## Activation Rules (MACHINE — not configurable)

```
SK-VERIFY: trainable-unit work OR any phase-done/completion claim on an algorithm/model OR produced-code review → ALWAYS (exempt from the 3-block cap)
SK-QA:    code/contract change OR trainable-unit work OR host/API endpoint OR any runtime/e2e claim → ALWAYS (exempt from the cap)
SK-DNA:   new class being added OR dna_compliance check failing
SK-FAB:   new fabric interface OR new DI dependency chain
SK-PLAN:  iteration ≤ 2 OR phase boundary
SK-REVIEW: plan review OR user correction OR shallow planning/review challenge OR final readiness sweep
SK-TEST:  test coverage incomplete OR bug-fix (always needs a test first)
SK-DOCS:  last step of session OR canonical doc sync triggered
```

When > 3 core candidates fire, priority is `DNA > FAB > PLAN/REVIEW > TEST > DOCS`;
take the top 3 and append all triggered mandatory-additional blocks.

---

## When to Inject Per Task Type

| Task | Inject |
|------|--------|
| New service class from scratch | SK-DNA + SK-FAB + SK-TEST |
| Bug fix | SK-TEST + SK-DNA |
| Phase planning | SK-PLAN + SK-DNA |
| Plan review or user correction | SK-REVIEW + SK-PLAN |
| Architect work order to sub-agent | SK-PLAN + SK-FAB |
| Fabric interface change | SK-FAB + SK-DNA |
| Produced / generated code | SK-DNA + SK-TEST + SK-VERIFY |
| Phase-done / completion claim on an algorithm or model | SK-VERIFY (always) |
| Host/API endpoint (`Xiigen.Platform.Api`) | SK-QA + SK-VERIFY |
| Any code/host/trainable work order (test strategy required) | SK-QA (always) |
| Session end | SK-DOCS |

> SK-ARCH / SK-DPO from the core registry are **not activated** on this platform:
> per R5, common/trainable models and DPO live in `llm_mvp_core` and are consumed
> via manifests/locators. If a platform path is genuinely claimed trainable, use
> SK-VERIFY's Layer-5 review gate (numeric metrics, else honest-untrained scaffold).

---

## Work-order skill + guide binding

A bounded sub-agent work order MUST enumerate (a) the exact skills it must load
and apply, (b) the required guides, and (c) the required injection blocks. The
returned evidence packet MUST confirm which skills, guides, and blocks were
loaded and applied. A trainable-unit packet without SK-VERIFY loaded and its
Layer-5 numeric metrics is invalid — the architect rejects it. A
code/host/trainable work order MUST additionally enumerate SK-QA, the required
testing guides, the expected test layers, and the untrained/trained behavior
expectations; a packet returning no test-layer results for the declared layers is
invalid.

---

## Bounded work-order boundary fields

Every bounded sub-agent work order must include these fields alongside the skill
list:

```text
CURRENT TASK:
NOT THE TASK:
EVIDENCE-ONLY:
requested_artifact_type
allowed_write_scope
exact_current_luba_quote
quoted_transcripts_state_old_plans_are_evidence_only=true
required_skills_guides_and_injection_blocks
human_readable_status_required=true
human_readable_phase_step_status_template_required=true
machine_labels_secondary_only=true
exact_review_scope_required=true
code_review_completeness_scope_required_for_code_slices=true
Model/Reasoning Configuration Gate
requested_model = gpt-5.5
requested_reasoning_effort = xhigh
runtime_model_or_unavailable = <visible runtime/request metadata or unavailable>
mode_verified_from_work_order = true only when exact runtime metadata is unavailable and no concrete contradictory value is visible
downgrade_authority = explicit current Luba quote only
allowed_verification          # e.g. dotnet build/test Xiigen.Platform.slnx, scoped --filter
forbidden_broad_verification
commit_push_boundary_owner = parent_after_review | separate_commit_only_work_order | allowed_with_exact_quote
work_order_scope
exact_allowed_files
git_authority = forbidden | parent_only | allowed_with_exact_quote
commit_push_authorized
allowed_git_commands
forbidden_commands
parent_will_perform_commit_push_after_review=true for normal artifact/code/content slices
may_derive_next_phase=false unless explicitly granted
must_stop_after_report=true
parent_review_required_after_return=true
parent_direct_write_forbidden_when_subagent_only=true when parent is architect-only/sub-agent-only/non-executor
parent_local_sidecar_work_forbidden_while_waiting=true
parent_read_only_verification_allowed_only_after_packet_return=true
```

If a work order omits `requested_model=gpt-5.5` or
`requested_reasoning_effort=xhigh`, executor/reviewer authority is not granted. A
generic "GPT-5" identity string is not downgrade evidence; if exact runtime
metadata is unavailable but the work order requests `gpt-5.5` / `xhigh`, continue
and record `mode_verified_from_work_order=true` unless a concrete contradictory
value is visible. A stop on generic identity text alone is a false downgrade stop.

---

## Git authority boundary

When `commit_push_authorized=false`, `git_authority` is `forbidden` or
`parent_only`, and `forbidden_commands` explicitly blocks `git add`,
`git commit`, `git push`, `git checkout`, `git reset`, `git branch`, `git tag`,
`git merge`, `git rebase`, and any git write/ref command. Read-only git commands
appear only under `allowed_git_commands`. A packet that ran git write/ref commands
without authority is invalid and is quarantined pending parent validation.

For artifact/code/content/docs/report/plan slices, inject `git_authority=forbidden`
and `allowed_git_commands=NONE` by default. The sub-agent returns evidence and
stops; the parent performs repository checks and the commit/push boundary, or
launches a separate commit-only work order. Commit/push authority is injected
only into a commit-only work order: no artifact edits, exact staged file set,
exact commit message, no next-phase derivation. Every returned packet reports
`git_authority_seen`, `git_commands_run`, `git_write_or_ref_commands_run`,
`git_directives_emitted`.

---

## Context boundary for bounded sub-agents

Inject a clean/minimal context by default
(`subagent_context_mode = clean_minimal`): the agent receives the current work
order, exact files, required skills/guides, and no old continuation/commit
authority. Use `full_history_exception` only when explicitly justified, and then
record `old_history_authority = evidence_only`,
`state_next_action_authority = false`, `prior_continue_or_commit_authority = false`,
`work_order_fields_are_only_authority = true`.

---

## Context Hygiene

Each step receives a CLEAN context — skill blocks are injected fresh, not
accumulated from previous steps. Re-inject the relevant blocks at each step. Old
effectiveness data informs selection, but the blocks themselves are always
injected fresh.

---

## Anti-Patterns

1. **Injecting every available block every step** — select a small core set plus triggered mandatory-additional blocks; injecting everything truncates core instructions.
2. **Not injecting SK-DNA for new classes** — DNA violations are hard to find later.
3. **Carrying skill context between steps** — accumulation pollutes later steps.
4. **Skipping injection because "I know the rules"** — systematic injection prevents drift.
5. **Omitting SK-VERIFY for trainable/phase-done work** — lets a json/stub deliverable pass as phase-done with only build+contract+DNA evidence (V20). Mandatory-additional, never dropped.
6. **Omitting SK-QA for code/host/trainable work** — ships a slice with no declared test strategy. Mandatory-additional, never dropped.
