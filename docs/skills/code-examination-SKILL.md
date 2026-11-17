# Code Examination (XIIGen Enterprise Platform)

## Overview

Understand before touching. A file modified without being fully read produces two
failure modes: the fix corrects the symptom at the call site while the root cause
is in the caller; or the fix is correct in isolation but breaks a consumer that
depended on the old behavior. This skill defines the 5-step examination protocol
that makes reading effective.

Stack-bound to **.NET 10 ASP.NET Core Web API**, **xUnit** (`tests/`),
`DataProcessResult<T>`, the platform fabric interfaces, and Roslyn for
generated-code checks. Callers/consumers are mapped through the ASP.NET Core DI
graph; tests live in `tests/Xiigen.*.Tests`.

## When to invoke

- Before modifying any service class, fabric interface, or test in `src/`.
- Before any phase implementation (preflight gate).
- When a bug report references a file not read this session.

## The rule

> **Understand before touching.** Never modify a file you have not read and
> understood in the context of its callers and consumers.

## Sub-Agent-Only Parent Boundary

When the parent/current session is sub-agent-only or architect-only, this
protocol belongs inside the bounded sub-agent work order. The parent must not use
code examination as local sidecar work while a sub-agent runs. Parent may read
governed files/code only after the packet returns, and only to verify/integrate
it. `sub-agent waiting is not a stop`.

---

## Section 1 — Five-Step Protocol

### Step 1 — Read the file completely

```bash
# The whole file — not just the method the bug is in
cat src/Xiigen.Platform.Marketplace/SmartWikiService.cs
# Its interface contract
cat src/Xiigen.Platform.Marketplace/MarketplaceContracts.cs
```

A hidden assumption three methods above the bug site is often the actual cause.

### Step 2 — Map upstream callers

```bash
grep -rn "SmartWikiService\|ISmartWikiService\|AnswerAsync" src/ --include="*.cs"
# DI registrations (who composes it)
grep -rn "AddSingleton\|AddScoped\|AddTransient" src/ --include="*.cs" | grep "SmartWiki"
```

A change to an output shape (`DataProcessResult<T>` type argument, returned
fields) affects every downstream consumer; a signature change affects every
caller. Know both before touching anything.

### Step 3 — Map downstream consumers

For service methods: identify what reads the return value (endpoints in
`Xiigen.Platform.Api`, application services composing fabrics). For outbox
events: identify every consumer of the `CloudEventEnvelope`.

```
SmartWikiService.IndexGraphAsync() -> IDocumentStore.UpsertAsync()
  -> the API endpoint mapping DataProcessResult<T> to an HTTP result
```

A change to the middle of the chain affects both ends.

### Step 4 — Form a hypothesis BEFORE reading tests

State in plain language: "I believe this code does X because Y. If I change Z,
then W will break." Write it before opening the test file.

### Step 5 — Read the test file

```bash
cat tests/Xiigen.Platform.Marketplace.Tests/SmartWikiServiceTests.cs
```

Does the test match your mental model? If it asserts behavior your hypothesis
says should not happen, your model is wrong — fix the model first, then the code.

### Step 6 — Final diff and structured-file recheck

After edits, examine the final diff. If you edited JSON, `.csproj`, or generated
reports, run the matching parse/build check **after** the final state update. For
project-boundary work, production code must live in the requested project and
tests must reference it normally; a test-only `Compile Include` or source link is
evidence only, not the implementation boundary.

### Step 6A — Code review completeness scope

Before a code slice is accepted, name the exact files/sections/endpoints/claims
inspected and explicitly check: project coding standards, allowed tests / Gate-B,
whether core algorithm logic stays in `llm_mvp_core` (consumed via
manifests/locators) rather than leaking into the platform host/API layer, whether
`Xiigen.Platform.Api` endpoints stay thin, whether hidden deterministic shortcuts
exist, whether deterministic helpers are labelled eval-only/helper/baseline, and
whether docs/QA must change. Passing tests or a machine label alone is not a
complete review.

### Step 7 — Independent review/fix cycle count

When several/multiple/repeated review-fix cycles are asked, the minimum is two
independent post-implementation passes after the final fix.

```text
review_cycle_1: { reviewer_or_pass_id, scope, findings, fixes_applied_or_zero_findings_evidence }
review_cycle_2: { reviewer_or_pass_id, scope, findings, fixes_applied_or_zero_findings_evidence }
```

A single final summary review is not "several cycles". Completion is blocked
until every finding is fixed or each pass records explicit zero-findings evidence.

---

## Section 2 — Diagnostic Routing Table

| Symptom | First action | Then |
|---------|-------------|------|
| `DataProcessResult.Failure(...)` returned unexpectedly | Read the guard clauses (the early `Failure<T>` returns) | Trace backward to the caller / request binding |
| `IDocumentStore` returns no record | Check `collection` + `id` at the write site (`UpsertAsync`) | Verify tenant scoping in the stored body |
| Idempotent op runs twice | Check the `operationKey` passed to `TryReserveAsync` | Verify `MarkSucceeded/MarkFailed` is called |
| Tenant is wrong / null | Check `ITenantContextAccessor.Current` is pushed for the request | Verify the API middleware/runner sets the context |
| Outbox event missing | Confirm `UpsertAsync` ran before `AppendAsync` (Rule 8) | Inspect `IOutboxStore.ListAsync` state |
| `dotnet test` passes but behavior wrong | Check for a stub returning a canned `Success(...)` | Add a MACHINE-constant `[Fact]` (`generated-code-review-SKILL.md` Layer 3) |
| Build fails on a warning | Fix the warning — `TreatWarningsAsErrors=true` makes it a build break | Re-run `dotnet build Xiigen.Platform.slnx` |

---

## Section 3 — Reading Checklist

```
□ Read the complete file — not just the method under investigation
□ Identified all callers of the method being changed (DI graph included)
□ Identified all consumers of the method's DataProcessResult<T> return type
□ Wrote a hypothesis in plain language before opening tests
□ Read the test file — does it match the hypothesis?
□ If hypothesis was wrong: re-read the implementation
□ Final diff reviewed after edits; structured files parsed after the final edit
□ Completeness scope recorded: standards, tests, core boundary, thin API layer,
  deterministic-shortcut sweep, deterministic labels, docs/QA impact
□ New project boundaries use real project references, not test-only source links
□ When several review/fix cycles were required, ≥ 2 independent post-fix passes recorded
```

---

## Trainable Architecture Examination Add-On

When examining learned/semantic/trainable/adaptive/DPO/selector/router/
classifier/scorer/retrieval/KNN/dictionary/counter/pattern-matching code (e.g. a
`SmartWiki`/`SmartCodeAssistant` path claimed to be trainable), also search for:

```bash
grep -rn "TryDecide\|ChooseBestPattern\|SelectBestPattern\|CountMatchingSignals\|FindNearest\|Jaccard\|BM25\|Dictionary<string, int>\|Regex\|\.Contains(" src/ --include="*.cs"
```

For each hit, trace whether the mechanism sits inside an allowed level
(`ILearningMachine`, `ILogicalOrchestrator`, `ILogicalFlow`, `ILogicalUnit`,
`ILearningMachinePair`) or outside the allowed hierarchy. Report each finding as:

```text
Human explanation in English (unless the user requests another language):
Code quote with file:line:
current behavior:
why this is wrong/acceptable:
required behavior:
Allowed location / Current location:
Verdict:
Concrete repair: current -> target; replacement level; replacement class/unit;
  replacement algorithm; input/output contracts; typed trained state;
  KNN/nearest role; training source roots + licenses; DPO pair-building rule;
  load user/cache first then repo/common fallback; backup/export/import/share;
  continue-training from checkpoint; affected sets + rebuild rule; affected tests;
  documentation/report wording.
```

> R5 boundary: common/trainable models stay in `llm_mvp_core` and are consumed
> via manifests/locators. On platform, Layer-5 appears as a **review gate**: if a
> path is claimed trainable, require numeric held-out metrics; otherwise label it
> an honest-untrained scaffold. "Make it trainable", "wrap it", or "add tests" is
> not a complete repair.
