# Pipeline Position Check

> Ported from the `llm_mvp_core` pipeline-position standard, adapted for the
> XIIGen Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build Xiigen.Platform.slnx` / `dotnet test`. This is a general planning
> discipline (Q0 / stage contract + `CONTEXT_INSUFFICIENT` stop), **not** an
> ML-engine topology. A stage position is the input/output of a domain service in
> `src/Xiigen.*` (Api → Kernel/fabric → store). A missing fabric contract or an
> unresolved v3.3 decision ⇒ `CONTEXT_INSUFFICIENT` stop, not a blind synthesis.
> Domain return type is `DataProcessResult<T>` (never core `OperationResult<T>`).

## Core Problem

A phase that implements a component without knowing what its downstream consumer
needs from it produces locally correct output that is globally useless. The
component compiles, passes tests, and still fails to serve its purpose because the
wrong shape was produced. This skill defines the four pre-phase questions that
must be answered before any file is read or any code is written.

---

## When to Invoke

- Before Q1 of any implementation session.
- Before Phase A of any phase plan that produces output consumed by a downstream phase.
- When the session scope is "fix the boundary between component N and N+1".
- When a component passes Gate B but the downstream phase still fails.

---

## Section 1 — The 4-Part Pipeline Position Declaration

Answer all four explicitly before any file is read or any code is written. If Q0d
cannot be answered with specificity, emit a `CONTEXT_INSUFFICIENT` signal
(Section 2) instead of proceeding.

### Q0a — What does this phase RECEIVE as input?

State: which upstream component produces the input, field names, required shape.

```
Example: "SmartWikiService.IndexAsync() receives a SmartWikiIndexRequest from
          a Program.cs MapPost endpoint after tenant context is pushed.
          Required fields: ProjectId, Documents[], with ITenantContextAccessor.Current set."
```

### Q0b — What does this phase PRODUCE as output?

State: field names, required shape, persistence destination.

```
Example: "IndexAsync() persists each document via IDocumentStore.UpsertAsync
          (collection per tenant), then appends a CloudEventEnvelope via
          IOutboxStore.AppendAsync, and returns DataProcessResult<IndexSummary>."
```

### Q0c — Which component CONSUMES this output?

State: which downstream component reads this output and what it does with it.

```
Example: "An outbox dispatcher reads the appended CloudEventEnvelope to notify
          downstream subscribers that the tenant's wiki index changed."
```

### Q0d — What does the consumer NEED from this output?

Answer from the consumer's perspective, not the producer's.

```
BAD:  "The dispatcher needs the outbox record"  ← restatement
GOOD: "The dispatcher needs the document to be PERSISTED before the event is
      appended (DNA Rule 8: persist-before-dispatch). If AppendAsync runs before
      UpsertAsync and the process crashes between them, the dispatcher will emit a
      'wiki updated' event for a document that was never stored — downstream
      consumers then fetch a missing document."
```

**Gate rule:** if Q0d produces "I'm not sure" or a generic restatement of the
output fields, the session is not ready to start. Emit `CONTEXT_INSUFFICIENT`.

---

## Section 2 — CONTEXT_INSUFFICIENT Signal

When Q0d cannot be answered with specificity, emit this signal. This is correct
behavior, not a failure. The session halts here and waits.

**Signal format — all fields required:**

```
CONTEXT_INSUFFICIENT

phase:    [which phase this session is implementing]
gap:      [what specific information is missing — not vague, not "more context"]
consumer: [which downstream component is affected]
impact:   [what quality failure occurs if this proceeds without resolution — be specific]
request:  [3-6 concrete questions Luba can answer in one sentence each]
```

**Example:**

```
CONTEXT_INSUFFICIENT

phase:    SmartWikiService.IndexAsync() persistence ordering
gap:      Unknown whether the outbox event must include the stored DocumentRecord
          Version, or only the document id. The v3.3 SMART-WIKI-CONTRACT is silent.
consumer: The outbox dispatcher / downstream subscribers
impact:   If the event omits Version, a subscriber cannot detect a stale read and
          may overwrite a newer version. If it includes Version, the event schema
          must change and all subscribers must update.
request:
  1. Must the wiki-updated CloudEvent carry the document Version, or just the id?
  2. Do downstream subscribers do optimistic concurrency on Version?
  3. Is there a v3.3 contract row I should read before implementing this?
```

---

## Section 3 — Resolution Protocol

**Step 1 — Session halts.** After emitting `CONTEXT_INSUFFICIENT`, the session
halts completely. No files read, no code written. State: "Session halted.
Awaiting context resolution."

**Step 2 — Luba provides resolution.** One sentence per question is sufficient.

**Step 3 — Resumption gate.**
```
Before resuming, verify:
□ Each question in `request` has a specific answer (not "use your judgment")
□ The answers together close the `gap` stated in the signal
□ Q0d can now be answered with specificity using the answers
```
If any item fails → re-emit `CONTEXT_INSUFFICIENT` with the remaining gap.

**Step 4 — Update design artifacts.** Before writing code, update the relevant
v3.3 contract / interface documentation with the resolved information so the same
gap does not recur.

---

## Section 4 — Pipeline Contract Check (Before Gate B)

After implementation, before declaring Gate B passed:

```
□ Q0a verified: input shape received matches what was declared
□ Q0b verified: output shape produced matches what was declared
□ Q0c verified: consumer can receive this output (no contract mismatch)
□ Q0d verified: consumer requirement is met by what was produced
               If not → surface the gap, resolve, then proceed to Gate B
```

A passing test count is necessary but not sufficient. A phase that produces
locally correct output but fails Q0d has produced noise, not a deliverable.

---

## Section 5 — Common Platform Stage Boundaries

```
STAGE BOUNDARY                          CONSUMER REQUIREMENT
──────────────────────────────────────────────────────────────────────────────
Program.cs endpoint → service:
  The service needs ITenantContextAccessor.Current already pushed (the host
  middleware pushes it). Tenant must not be a method parameter.

Service → IDocumentStore:
  UpsertAsync must run BEFORE any IOutboxStore.AppendAsync / ScheduleDelayedAsync
  (persist-before-dispatch, DNA Rule 8).

Service → IIdempotencyStore:
  A repeatable operation must TryReserveAsync first, then MarkSucceeded/MarkFailed.
  The reservation carries a cached response for the duplicate-request path.

Service → IOutboxStore:
  The consumer needs the CloudEventEnvelope, not an ad-hoc payload, so the
  dispatcher can route by Type/Subject and apply Extensions consistently.
```

---

## Anti-Patterns

1. **"Implementing a phase without stating Q0d first"** — if Q0d is unknown, emit `CONTEXT_INSUFFICIENT` before touching any file.
2. **"Q0d answered as a restatement: 'the dispatcher needs the output'"** — state what specific property the consumer depends on (ordering, Version, cumulative count).
3. **"'Looks correct' means done"** — output that is well-formed but fails Q0d's consumer requirement is incorrect.
4. **"Vague CONTEXT_INSUFFICIENT requests"** — "please provide more context" shifts work back to Luba. Name the specific field and why it matters.
5. **"Resuming without updating design artifacts"** — record the resolution so the question does not recur.

---

## Integration

- `planning-session-startup-SKILL.md` Five Questions (blockers) produce the prerequisite context for Q0a.
- The v3.3 `*-CONTRACT.md` files are the canonical source for the shapes needed in Q0b-Q0c.
- `system-intake-SKILL.md` Section 5 reuses this `CONTEXT_INSUFFICIENT` signal format for intake gaps.
- `data-flow-integrity-SKILL.md` / `api-shape-verification-SKILL.md` verify Q0c's contract-mismatch check.
