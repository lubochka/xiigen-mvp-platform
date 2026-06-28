# Codex Planning Quality Guide

> Ported from the `llm_mvp_core` planning standard, adapted for the XIIGen
> Enterprise Platform: **backend-only .NET 10 ASP.NET Core Web API**, xUnit,
> Gate-B = `dotnet build` / `dotnet test`. Use when asked to improve plan quality,
> diagnose missing plan-review quality, diagnose missing skills/settings/guides,
> or compare against a stronger planning/review model. Its job is to force the
> plan/review to **start from architecture decisions and the current target
> artifact (the active v3.3 session file), not from an old packet, old plan, or a
> convenient repair**. Domain result type is `DataProcessResult<T>` (never core
> `OperationResult<T>`).

## Start From Architecture And Current Target

A planning/review response must begin from the current architecture decisions and
the **current target artifact** named by Luba's latest unquoted message (or a
bounded work order), not from an old packet/plan/state. On this platform the
current target is normally the active `SESSION-XX-*.md` under
`docs/plans/enterprise-adaptation/v3.3/`, and the Beacon/state/matrix are the
governing context (`CLAUDE.md` "Beacon First"). If the response drifts to
continuing a benchmark, an old plan, or a foreign artifact, that is the failure.

## Dual-Path Verification (backend adaptation)

The core "Dual-Path Host Verification" rule exists for extension/runtime host
scenarios. **This platform has no UI host** (it is a backend Web API). The
backend adaptation of dual-path is:
- **Path A — Automated boundary:** xUnit on the domain service / fabric +
  an **endpoint/API probe** — an in-process HTTP test via
  `WebApplicationFactory<Program>` (with `X-Tenant-Id` / `X-Roles` headers)
  exercising the real minimal-API route → `Results.*` / `DataProcessResult<T>`.
- **Path B — Real runtime path:** a real API-call / deployment path from the v3.3
  Beacon runbook (the master-tenant launch runbook / AWS path), with the request,
  response, status code, and correlation id captured as evidence.

Do not let a plan reduce a scenario to a single fragile chain, and do not accept
JSON-only or report-only evidence as a substitute for Path A. If Path B (the real
runtime/AWS path) cannot be reached, record the exact blocker and the next bounded
action; do not convert the blocker into a final stop, and do not claim runtime
proof from a localhost-only or static artifact.

## No Dishonest Planning Claims

Planning quality includes claim honesty. Before reporting:
```
claim_scope:
evidence_scope:
count_claimed:
independent_evidence_count:
subagent_id_or_NONE:
ledger_or_checklist_only:
allowed_claim:
```
Rules:
- Ledger rows, checklist rows, generated tables, and summaries are **not** cycles.
- Parent/self review is **not** sub-agent review.
- "N cycles/reviews/passes" requires N independently inspectable evidence records
  (reviewed scope, finding/no-fix result, repair action when any, recheck
  evidence).
- A `dotnet test` / `dotnet build` / git / file-edit claim requires fresh
  execution evidence.
- If a prior response overclaimed, mark it as an overclaim in the artifact and add
  a regression rule to the relevant planning/review skill.

## Soft-Stop Ban (matches CLAUDE.md)

This guide's stop discipline matches the platform `CLAUDE.md` "Executor Cannot
Stop The Plan": commit done / report written / one child session or test passed /
AWS URL produced / screenshot captured / context getting long are **invalid soft
stops**. The executor produces slice evidence and hands off to REVIEW_INTERNAL; a
gap moves control to ARCHITECT_INTERNAL_REPAIR then back to
EXECUTOR_INTERNAL_CONTINUE. Only an architect verdict with `stop_allowed=true`
and a completion matrix with no required `UNKNOWN`/`FAIL` rows authorizes a final
report.

## Architect Sub-Agent-Only Firewall (preserve exactly)

When the current instruction keeps the parent/current session architect-only,
governance-only, or sub-agent-only, preserve this language exactly:
```
Parent Architect Subagent-Only Firewall
parent local sidecar work ban
read-only verification after a sub-agent packet returns
sub-agent waiting is not a stop
no local non-overlapping work escape
```
The parent must not do direct artifact/code/plan/protocol/report/skill authorship;
it orchestrates, spawns/waits/closes bounded sub-agents, reviews returned packets,
and runs read-only verification only after a packet returns. Waiting for a required
sub-agent result is not a stop gate.

## Plan/Review Quality Checklist (what a strong response must contain)

When asked to improve plan or review quality, the response must:
1. Confirm Beacon-first and name the current target artifact (the active v3.3
   session file), with `NOT THE TASK` / `EVIDENCE-ONLY` separating benchmarks and
   old plans.
2. Diagnose the missing/weak **skill / guide / agent-file rule** that allowed the
   shallow output, naming the exact file and the missing rule (answer-first,
   before any artifact edit).
3. Require Part A §0-§4 at full depth with per-connection / per-branch simulation
   (the implementation-doc skills + FC-14).
4. Require Part A ↔ Part B branch-level parity (FC-19).
5. Require a test strategy per branch (unit / integration / e2e-host via
   `WebApplicationFactory<Program>` / negative / regression) — FC-26.
6. Require exact Gate B per phase (a `dotnet test`/`dotnet build` command +
   expected output), never "works correctly".
7. Require the dual-path (Path A boundary + Path B real runtime) framing for any
   runtime claim, with exact blockers recorded instead of a stop.
8. Reject inflated counts and soft-stop completion (Dishonest Claim + Soft-Stop
   Ban above).

## Anti-Patterns
1. **Starting from an old packet/plan** instead of the current v3.3 target artifact.
2. **Localhost/static evidence as runtime proof** — Path B needs the real runtime/AWS path or an exact recorded blocker.
3. **"N cycles" from ledger rows** — count only independent evidence records.
4. **Soft-stop as completion** — only an architect ALLOW_FINAL verdict ends a round.

## Integration
Routing hook from `planning-session-startup-SKILL.md`; enforced at review by
`plan-review-SKILL.md` (Gate 0, Dishonest Claim, FC-14/FC-19/FC-26/FC-EX);
regression-tested by `planning-quality-forward-test-PROMPT.md`.
