# product-scope-validation-SKILL.md — Post-wave end-to-end product check (API scenarios)

> Ported from the `llm_mvp_core` governance standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build` / `dotnet test`, domain return type `DataProcessResult<T>`
> (`src/Xiigen.Kernel/DataProcessResult.cs`) — **not** core `OperationResult<T>`.
> **This repo has no frontend.** A "user-facing scenario" is an **API scenario**:
> a sequence of HTTP requests to the Web API that exercises ≥ 2 modules
> end-to-end. Tracing is by `file:line` across `.cs`; business outcomes are
> verified with xUnit integration tests via `WebApplicationFactory<Program>`
> (sending `X-Tenant-Id` / `X-Roles` headers) and real endpoint runs. Tie the
> scenario to the master-tenant launch runbook
> (`docs/plans/enterprise-adaptation/v3.3/MASTER-TENANT-LAUNCH-RUNBOOK-v1.md`).

## Core problem
A wave of phases can each pass Gate B individually while the end-to-end product
story is broken. Each module tested its own output; no module tested the joint
output. This skill defines the 5-step post-wave validation that answers: "Does
this wave advance a real product scenario a consumer of the Web API can experience
end-to-end?"

## When to invoke
- After all phases in a parallel wave complete and merge.
- Before declaring a wave "feature complete" / runbook-ready.
- When individual modules pass but the joint API call sequence fails.
- Before writing the session handoff that closes a multi-module batch.

---

## Section 1 — Five-step validation protocol

Run all five in order. A FAIL at any step produces an INCOMPLETE verdict and
halts — do not skip ahead to a partial PRODUCT-READY verdict.

### Step 1 — Retrieve original product intent
```
Source: the goal at the top of the session / the v3.3 matrix row / the runbook
        step that initiated this wave. Do not paraphrase. Quote verbatim.

Record:
  intent_statement: "[verbatim quote]"
  wave_scope:       "[list of modules/flows in this wave]"
```
If the intent cannot be found: emit CONTEXT_INSUFFICIENT before proceeding.

### Step 2 — Define a product scenario (API scenario)
Translate the intent into a single concrete API scenario: a sequence of HTTP
requests a consumer makes against the Web API that exercises this wave end-to-end.
```
Scenario format:
  Request:         [HTTP method + route + headers (X-Tenant-Id, X-Roles) + body]
  System response: [status + DataProcessResult<T> payload the consumer receives]
  Exercises:       [which modules/fabric stores are exercised]
```
The scenario must name at least **two** module outputs (e.g. an endpoint that
writes via `IDocumentStore` AND emits via `IOutboxStore`). One module only = not
testing cross-module integration.

### Step 3 — Trace the scenario against live state
For each request in the scenario:
```
□ Does the required endpoint/route exist?          (grep the minimal-API map / controller in src/)
□ Is the handler implemented, not stubbed?         (no TODO/NotImplementedException/not_ready_* body)
□ Does the handler reach the downstream consumer?
  (trace: endpoint delegate → domain service → IOutboxStore.AppendAsync / IDocumentStore.UpsertAsync)
□ Is there an integration test that exercises this path? (WebApplicationFactory<Program>)
```
A STUB at any trace step = INCOMPLETE. An absent integration test = INCOMPLETE
(document the gap, not a FAIL).

### Step 4 — Validate business outcomes
This step checks product correctness, not code correctness.
```
WRONG question: "Does the handler return DataProcessResult.Success()?"
RIGHT question:  "Does the returned value / emitted event produce a result the
                 consumer would recognize as correct for their request?"
```
Validation heuristics by archetype:
```
WRITE-THEN-EMIT archetype (e.g. registration):
  Wrong: UpsertAsync returns Success
  Right: after the request, IDocumentStore has the record AND IOutboxStore has the
         matching CloudEvent (assert both in the integration test)

IDEMPOTENCY archetype (e.g. resend):
  Wrong: TryReserveAsync returns Success
  Right: the SAME request twice yields one effect; the second returns the cached
         reservation (Status reflects already-reserved), verified with 2 calls

SCHEDULING archetype:
  Wrong: ScheduleDelayedAsync returns Success
  Right: ListAsync shows exactly one job for the idempotencyKey; a duplicate
         schedule with the same key does not add a second job (fromCache=true)

TENANT-ISOLATION archetype:
  Wrong: the call succeeds under X-Tenant-Id: A
  Right: data written under tenant A is NOT visible under X-Tenant-Id: B
         (contrast two tenant headers — this is a security outcome)
```

### Step 5 — Produce the Product Validation Report
```markdown
## Product Validation Report — Wave [N]

### Intent Statement
[verbatim from Step 1]

### Scenario Tested (API)
[from Step 2]

### Verdict
PRODUCT-READY | INCOMPLETE | BROKEN_DEPENDENCY

### Evidence
| Trace step | Status | Evidence |
|---|---|---|
| [module A endpoint] → [module B store] | PASS/FAIL/STUB | [grep result or xUnit test name] |

### Gaps (INCOMPLETE only)
| Gap | Type | Blocking? |
|---|---|---|
| [missing integration test] | DOCUMENTATION | No |
| [stub in critical path] | IMPLEMENTATION | Yes |

### Business Outcome Check
| Outcome | Correct? | Evidence |
|---|---|---|
| [scenario outcome 1] | YES/NO | [contrasting-input spot-check result] |
```

---

## Section 2 — Verdict definitions
**PRODUCT-READY:** all trace steps pass, no stubs in the critical path, business
outcomes verified with at least two contrasting inputs (e.g. two tenant headers,
or duplicate idempotent request).
**INCOMPLETE:** some trace steps fail or stubs exist in the path. List all gaps;
classify each DOCUMENTATION (missing test, not blocking) or IMPLEMENTATION (blocking).
**BROKEN_DEPENDENCY:** a downstream component/endpoint does not exist or its
contract changed since the upstream was written. Requires a new phase, not just a gap doc.

---

## Section 3 — Common failure patterns
```
1. "Wave passed Gate B but the API call sequence fails"
   Cause: Gate B checked each module in isolation.
   Fix:   Run Step 3 trace — find the first stub/missing endpoint in the path.

2. "Endpoint returns Success but the event was never emitted"
   Cause: Step 4 business-outcome check skipped.
   Fix:   Assert BOTH IDocumentStore and IOutboxStore in the integration test.

3. "Correct for tenant A, leaks to tenant B"
   Cause: tenant-isolation outcome not contrasted.
   Fix:   Re-run the scenario with a second X-Tenant-Id; data must not cross.

4. "Can't find original intent statement"
   Cause: goal never written verbatim.
   Fix:   Quote the matrix row / runbook step before Step 1.
```

---

## Anti-patterns
1. **"All Gate B checks passed — wave is ready"** — Gate B is per-phase; product scope validation is post-wave. Different gate, different scope.
2. **"Business outcome is 'the handler runs without error'"** — that is Step 3 (code correctness), not Step 4 (product correctness). Run both.
3. **"Only one module in the scenario"** — a single-module scenario does not test integration. Add the downstream consumer.
4. **"Verdict is INCOMPLETE but no gaps listed"** — name every gap; classify each DOCUMENTATION or IMPLEMENTATION.
5. **"Proceed to next wave before closing BROKEN_DEPENDENCY gaps"** — a broken dependency compounds in the next wave.

## Integration
- `verification-before-completion-SKILL.md` covers the per-claim end-to-end observable principle.
- `session-scope-resolution-SKILL.md` closes the wave by including this report in the session handoff.
- `multi-reviewer-design-SKILL.md` Completeness/Business reviewers validate the API scenario when there is no UI.
