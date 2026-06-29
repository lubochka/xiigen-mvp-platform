# Event Contract Design (XIIGen Enterprise Platform)

## Core Problem

An event schema without correlation fields produces events that cannot be traced to
a request. An event with PII in the payload leaks user data across boundaries. A
forward event without a compensation schema cannot be rolled back. An event designed
as HTTP-endpoint coupling cannot be consumed without coupling to the publisher's
implementation. This skill defines the minimum required schema for every event the
platform appends to the outbox.

Platform port of the core `event-contract-design` skill. On platform the event
transport is **not** an in-memory `IAgentMessageBus` — it is the durable
`IOutboxStore` carrying a `CloudEventEnvelope` (`src/Xiigen.Kernel/FabricContracts.cs`),
with the correlation id supplied by `ICorrelationContextAccessor`
(`src/Xiigen.Kernel/CorrelationContext.cs`). Stack: **.NET 10 ASP.NET Core Web API**,
**xUnit**, `DataProcessResult<T>`. Gate-B is `dotnet build Xiigen.Platform.slnx` +
`dotnet test`.

## When to load

- Before defining any new event type appended through `IOutboxStore`.
- When `domain-event-design-SKILL.md` encounters a complex event with compensation
  requirements.
- Before wiring any compensation (rollback) event chain.
- When designing events that cross project boundaries.

---

## Section 1 — Server Event Schema (required structure)

The platform's wire event is the CloudEvents envelope. Every event uses it:

```csharp
public sealed record CloudEventEnvelope(
    string Id,                                      // unique event id
    string Source,                                  // emitting service, e.g. "xiigen-smart-wiki"
    string Type,                                    // event type STRING constant, e.g. "CapabilityIndexed"
    string SpecVersion,                             // "1.0"
    DateTimeOffset Time,                            // when emitted
    string Subject,                                 // the entity the event is about (stable id)
    string DataContentType,                         // "application/json"
    IReadOnlyDictionary<string, object?> Data,      // typed payload — stable ids, NO PII
    IReadOnlyDictionary<string, string> Extensions);// correlation lives here
```

**Three correlation fields are MACHINE (always present) — carried in `Extensions`:**
- `correlationid` — from `ICorrelationContextAccessor.Current.CorrelationId`; ties
  all events in one execution chain together.
- `tenantid` — from `ITenantContextAccessor.Current.TenantId`; scope isolation.
- `traceparent` — W3C distributed trace for cross-service debugging.

```csharp
var corr = _correlation.Current;   // ICorrelationContextAccessor
var envelope = new CloudEventEnvelope(
    Id: Guid.NewGuid().ToString("n"),
    Source: "xiigen-smart-wiki",
    Type: "CapabilityIndexed",                       // STRING constant, not a class name
    SpecVersion: "1.0",
    Time: DateTimeOffset.UtcNow,
    Subject: capabilityId,                           // stable id, not a DB row key
    DataContentType: "application/json",
    Data: new Dictionary<string, object?> { ["capabilityClass"] = cls, ["nodeCount"] = n },
    Extensions: new Dictionary<string, string>
    {
        ["correlationid"] = corr!.CorrelationId,
        ["tenantid"] = tenant.TenantId,
        ["traceparent"] = traceParent
    });
await _outbox.AppendAsync(envelope, ct);             // durable delivery (Rule 8/9)
```

**No PII in any event payload. Ever.** PII for this project: external-system user
ids, email addresses, user display names, IP addresses not needed for routing. If a
downstream consumer needs user context, it reads from `IDocumentStore` — the event
carries only stable ids and capability-level data.

---

## Section 2 — Compensation Event Rule

For every forward event that modifies state, define a compensation event:

```
Forward:      {EventName}                 e.g. CapabilityIndexed
Compensation: {EventName}Reverted | {EventName}RolledBack

Compensation Data is identical to the forward Data with one added key:
  ["rollbackReason"] = "<why>"

EXCEPTIONS (terminal events — no compensation):
  - CapabilityPromoted        → promotion is proven; only an evidence-based demote
  - SessionCompleted          → terminal state
  - ScheduledJobCompleted     → compensation is ScheduledJobReverted with a reason
```

`IIdempotencyStore` underpins safe compensation: a reserved operation
(`TryReserveAsync`) that fails is marked via `MarkFailedAsync`, so the rollback path
is itself idempotent.

---

## Section 3 — SLA Annotation

Every event the platform waits on must declare timing. On platform, delayed/retried
delivery is owned by `ISchedulerService` (`ScheduleDelayedAsync`) and the outbox
attempt counter (`OutboxRecord.Attempts`):

```csharp
public sealed record EventGateConfig(
    string EventType,           // the event to wait for
    TimeSpan? Timeout,          // null = synchronous, no timeout
    string? TimeoutEvent,       // appended if timeout occurs
    RetryPolicy? RetryPolicy);

public sealed record RetryPolicy(
    string Owner,               // "service" or "caller"
    int MaxAttempts,            // default 3
    string Strategy,            // "exponential_backoff" | "linear"
    string OnMaxRetries);       // "compensate" | "fail" | "escalate"
```

Synchronous, sub-millisecond events: `new EventGateConfig("ScoreComputed", null, null, null)`.

---

## Section 4 — Injectable vs Platform-Only Boundary

For each event handler, declare whether it is injectable (customizable per
tenant/experiment) or platform-only (fixed):

```
INJECTABLE:     a tenant/experiment may provide a different handler implementation
PLATFORM_ONLY:  security-critical or DNA-invariant — no customization allowed
```

Example:
```
// CapabilityIndexed handler: INJECTABLE — a tenant may post-process index events
// IdempotencyReservation handler: PLATFORM_ONLY — dedup is invariant
```

Default for unmarked handlers: PLATFORM_ONLY.

---

## Section 5 — Stack-Neutral Event Contract Check

After designing CONSUMES/EMITS/BOUNDARY for any capability, verify:

```
□ All event type names are string constants (CloudEventEnvelope.Type), not class names
  CORRECT: "CapabilityIndexed"   WRONG: CapabilityIndexedEvent (class reference)

□ CONSUMES references only outbox/event names — no HTTP calls
  CORRECT: "CapabilityIndexed (IOutboxStore — from xiigen-smart-wiki)"
  WRONG: "POST /api/wiki/index" (HTTP coupling)

□ EMITS contains only CloudEvents-compatible string Type values
  CORRECT: "CapabilityPromoted"   WRONG: CapabilityPromotedEvent

□ BOUNDARY uses only "INJECTABLE" or "PLATFORM_ONLY"

□ Correlation extension keys present: correlationid, tenantid, traceparent

□ No PII key in Data
```

If any check fails: produce a gap report before implementation.

---

## Section 6 — Event Contract Document Format

Before any handler implementation, produce:

```markdown
## Event Contract — SmartWikiService

### Server events emitted (via IOutboxStore.AppendAsync):
- `CapabilityIndexed` — emitted after the wiki graph is persisted
  Envelope: Source="xiigen-smart-wiki", Type="CapabilityIndexed", Subject=<capabilityId>
  Extensions (MACHINE): correlationid, tenantid, traceparent
  Data: { capabilityClass: string, nodeCount: int }   // stable ids only, no PII
  Compensation: `CapabilityIndexReverted` (Data + rollbackReason)

- `CapabilityPromoted` — emitted when confidence ≥ threshold
  Terminal — no compensation

### SLA:
  CapabilityIndexed: synchronous, no timeout
  CapabilityPromoted: post-scoring, no timeout

### Boundary:
  CapabilityIndexed handler: INJECTABLE
  Idempotency handler: PLATFORM_ONLY
```

---

## Test Strategy (per branch)

- **Unit (xUnit):** assert the appended `CloudEventEnvelope.Extensions` contains
  `correlationid`/`tenantid`/`traceparent`, and `Data` contains no PII key.
- **Integration:** append via `InMemoryOutboxStore`; `ListAsync` returns the event
  with the correlation id propagated from `ICorrelationContextAccessor`.
- **Compensation:** a forward event followed by its `*Reverted` event round-trips
  through the outbox; the reverted Data carries `rollbackReason`.
- **Negative:** an event constructed without a correlation context fails the contract
  check (test asserts the guard returns `Failure<T>`), never a silent emit.
- **Gate-B:** `dotnet build Xiigen.Platform.slnx` (0/0) + `dotnet test` (0 failed).

---

## Anti-Patterns

1. **"Event Data includes user email"** — PII violation. Remove it; read from
   `IDocumentStore` downstream.
2. **"No compensation event for a state-modifying forward event"** — add
   `{Event}Reverted` before implementation.
3. **"Event type = class name reference"** — use the string `Type` value, not
   `CapabilityIndexedEvent`.
4. **"CONSUMES references an HTTP endpoint"** — events cross boundaries via
   `IOutboxStore`, not HTTP.
5. **"Append the event before persisting the document"** — Rule 8 violation:
   `UpsertAsync` THEN `AppendAsync`.

---

## Integration

- `domain-event-design-SKILL.md` governs naming for events defined here.
- `IOutboxStore` + `CloudEventEnvelope` are the transport (`Xiigen.Kernel`).
- `ICorrelationContextAccessor` / `ITenantContextAccessor` supply correlation +
  tenant for every envelope.
- `dna-compliance-guard-SKILL.md` Rule 8/9 enforce persist-before-dispatch and the
  envelope.
