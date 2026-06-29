# Mental Debug (XIIGen Enterprise Platform)

## Overview

Runtime debugging discipline: a diagnostic routing table (symptom → first rule →
next rule) plus eight bug classes that cover the failures this stack actually
produces. The anti-pattern this skill kills first: *"the code looks correct, so
the failure must be in the test."* The failure is in the code until proven
otherwise.

Stack-bound to **.NET 10 ASP.NET Core Web API**, **xUnit** (`tests/Xiigen.*.Tests`),
and the native platform result type `DataProcessResult<T>` (`Xiigen.Kernel`) —
**not** `OperationResult<T>` (that type does not exist in platform `.cs` files).
Fabric interfaces: `IDocumentStore`, `IIdempotencyStore`, `IOutboxStore`,
`ISchedulerService`, `ITenantContextAccessor`. Services extend
`MicroserviceBase<TService>`. There is no frontend — multi-tenant appears only as
the server-side `ITenantContextAccessor.Current` ambient context, never UI state.

## When to load

- A test fails unexpectedly, or a `DataProcessResult` carries the wrong value.
- Before changing a service whose sibling processes the same input type.

---

## Section 1 — The 8 Rules

| # | Rule | What it catches |
|---|------|----------------|
| 1 | pattern-recognition | Misidentifying what kind of failure this is |
| 2 | contextual-gap | Missing context the service assumed was injected/pushed |
| 3 | mental-execution | Wrong mental model of how the code actually runs |
| 4 | boundary-data-lifecycle | Data shape changes across Api↔Kernel↔Fabric boundaries |
| 5 | loop-off-by-one | Off-by-one in iteration, `Take`/`Skip`, or count checks |
| 6 | error-silent-swallow | `IsSuccess` not checked — wrong `.Data` silently consumed |
| 7 | sibling-guard-parity | Guard exists in one service but missing in its sibling |
| 8 | interface-resolution-trace | Wrong concrete fabric resolved through an interface |

---

## Section 2 — Diagnostic Routing

| Symptom | Apply first | Then |
|---------|-------------|------|
| Wrong count (off by 1) | pattern-recognition → loop-off-by-one | mental-execution |
| All results identical / collapsed | pattern-recognition → contextual-gap | boundary-data-lifecycle |
| `DataProcessResult` is `Failure` unexpectedly | contextual-gap → error-silent-swallow | boundary-data-lifecycle |
| Test passes but `.Data` value is wrong | error-silent-swallow → mental-execution | sibling-guard-parity |
| DI resolves wrong fabric / `InvalidOperationException` | interface-resolution-trace → contextual-gap | mental-execution |
| `IsSuccess=true` but `.Data` wrong | error-silent-swallow → boundary-data-lifecycle | mental-execution |
| `Tenant context is required` thrown | contextual-gap (tenant not pushed) | mental-execution |
| Idempotent op runs twice | sibling-guard-parity → mental-execution | error-silent-swallow |
| Guard in `MarketplaceService` but not its sibling | sibling-guard-parity → contextual-gap | — |

---

## Rule 1 — Pattern Recognition

Identify the failure class before debugging.

| Pattern | Shape | Starting rule |
|---------|-------|---------------|
| Wrong count | actual=N, expected=M, diff=1 | loop-off-by-one |
| Silent wrong value | no exception, wrong `.Data` | error-silent-swallow |
| Failure / default everywhere | all outputs `Failure` or empty | contextual-gap |
| Wrong tenant scoping | record found for another tenant | boundary-data-lifecycle |
| Works in one service, broken in sibling | `MarketplaceService` OK, `SmartWikiService` fails | sibling-guard-parity |

> Anti-pattern: "The code looks correct so the failure must be in the test." The
> failure is in the code until proven otherwise.

---

## Rule 2 — Contextual Gap

The service assumes something was injected (ctor) or pushed (ambient) that wasn't.

```
Checklist:
☐ Which fabric interfaces does this service's ctor require? Is the test passing them?
☐ Does the ctor call : base(descriptor, tenantContextAccessor, correlationContextAccessor, logger)?
☐ Is ITenantContextAccessor.Current set for this request? (MicroserviceBase.CurrentTenant
  THROWS "Tenant context is required ..." when it is null.)
☐ Is the IDocumentStore/IOutboxStore/IIdempotencyStore stub returning the records the test asserts on?
☐ Is the correct collection string used at the write site?
```

| Missing context | Where needed | Symptom |
|----------------|-------------|---------|
| `ITenantContextAccessor.Current` null | any `MicroserviceBase` service | `InvalidOperationException: Tenant context is required ...` |
| Fabric not registered | `Xiigen.Platform.Api` DI | `InvalidOperationException` resolving the service |
| `IDocumentStore` stub returns empty | service reading a record | `GetAsync` returns `Success(null)` → downstream `Failure` |
| Wrong `collection`/`id` at write | `UpsertAsync` call site | record written but never read back |

---

## Rule 3 — Mental Execution

Wrong mental model. Trace the ACTUAL execution path, not the assumed one.

**Protocol:**
1. Write the exact input to the method under test.
2. At every `if`/`switch`, check the ACTUAL condition value — do not assume.
3. At every `await`, trace what actually returns.
4. At every `DataProcessResult` boundary, check: is `.IsSuccess` true or false here?
5. At every fabric call, check: does the returned `DataProcessResult<T>` type match
   what the caller reads from `.Data`?

```csharp
// Trap: DataProcessResult<T>.Data is null on Failure — reading .Data! crashes.
var doc = await _documents.GetAsync("legal", id);   // returns Success(null) when absent
var body = doc.Data!.Body;                            // NRE if Data is null

// Trap: GetAsync Success(null) ("found nothing") is NOT IsFailure.
// IsSuccess is true with Data == null. Guard the null, not just IsFailure.
if (!doc.IsSuccess || doc.Data is null)
    return Failure<LegalDoc>("NOT_FOUND", "Document missing");
```

---

## Rule 4 — Boundary Data Lifecycle

Data changes shape/meaning as it crosses Api ↔ Kernel ↔ Fabric boundaries.

```
Checklist:
☐ What type does the producer emit at the boundary? What does the consumer read?
☐ DocumentRecord.Body is IReadOnlyDictionary<string, object?> — every read is an
  object? that must be cast/checked; a missing key is null, not an exception.
☐ CloudEventEnvelope.Data is also IReadOnlyDictionary<string, object?> — same rule
  at the outbox boundary.
☐ Does the API endpoint map DataProcessResult<T> to the right HTTP shape (Data on
  success, Error.Code/Message on failure)?
```

| Boundary | Wrong assumption | Correct rule |
|----------|-----------------|-------------|
| Fabric → service | `.Data` is always populated | On `Failure`, `.Data` is `default`; on `GetAsync` "not found", `Data` is null with `IsSuccess=true` |
| `DocumentRecord.Body[key]` | value is a typed object | it is `object?`; cast and null-check |
| service → `Xiigen.Platform.Api` | exception propagates | business outcome is `DataProcessResult`, not a thrown exception |
| outbox → consumer | `Data` typed | `CloudEventEnvelope.Data` is `IReadOnlyDictionary<string, object?>` |

---

## Rule 5 — Loop Off-By-One

Off-by-one in indexing, batch processing, or count assertions.

```
Checklist:
☐ Does the loop start at 0 or 1? Is the first iteration what you expect?
☐ < vs <= — include or exclude the last element?
☐ .Take(n) / .Skip(n): is n a count or an index?
☐ Is the loop mutating the collection it iterates?
☐ Assert.Equal(3, result.Data!.Count) — is 3 actually right?
```

Trace protocol: write the first, second, and last iteration by hand. The bug is
almost always at the last iteration (fencepost error).

---

## Rule 6 — Error Silent Swallow

`IsSuccess` not checked — code reads `.Data!` on a `Failure`, or a `catch` logs and
returns `Success` anyway. On this platform a `Failure` is a `DataProcessResult`,
never a thrown exception for business logic.

```csharp
// Bug: .Data! on a failed result → NullReferenceException
var res = await _outbox.ListAsync();
var n = res.Data!.Count;        // if IsFailure, Data is default(IReadOnlyList<>) → null → crash

// Fix: guard before access, propagate the error code/message
var res = await _outbox.ListAsync();
if (!res.IsSuccess)
    return Failure<int>(res.Error!.Code, res.Error.Message);
var n = res.Data!.Count;
```

```
Checklist:
☐ Every DataProcessResult<T> consumer checks .IsSuccess before reading .Data
☐ No Failure(...) returned with an empty code string
☐ Any catch block that only logs but returns Success(...) = silent swallow → REJECT
☐ Any method returning Success(default/empty) after a Failure path = silent swallow → REJECT
☐ A swallowed Failure that should have propagated through MicroserviceBase is a Rule 6 hit
```

---

## Rule 7 — Sibling Guard Parity

A validation/`IsSuccess` check exists in one service but not in its sibling that
processes the same input type.

| Sibling A | Sibling B | Guard that must be in both |
|-----------|-----------|--------------------------|
| `MarketplaceService` write path | `LegalPublicationService` write path | `IsSuccess` check on `UpsertAsync` result |
| any service calling `TryReserveAsync` | its retry/sibling path | `MarkSucceeded`/`MarkFailed` always called |
| `SmartWikiService` | `SmartCodeAssistantService` | tenant-scope guard via `CurrentTenant` |

```
Checklist:
☐ If service A checks IsSuccess on fabric X, does sibling B also check it?
☐ If A validates tenant via CurrentTenant, does B do the same before the write?
☐ If A reserves idempotency and marks success/failure, does B close the same loop?
```

---

## Rule 8 — Interface Resolution Trace

The wrong concrete fabric is resolved — usually a test stub returning the wrong
value, or a missing/duplicate DI registration.

```
Checklist:
☐ Which concrete type does the test pass for IDocumentStore / IOutboxStore / IIdempotencyStore?
☐ Is the in-memory stub returning the record the test asserts on?
☐ Does the stub match the interface signature exactly (Xiigen.Kernel.FabricContracts)?
☐ In Program.cs: is the fabric registered once, with the intended lifetime?
☐ Is AddXiigenInMemoryScheduler (or the real scheduler) registered for ISchedulerService?
```

> Anti-pattern: "The interface is correct — it must be the algorithm." Check the
> resolved concrete type first. Most failures here are a stub or DI registration
> returning the wrong fabric, not a wrong algorithm.

---

## Trainable-Path Add-On (R5 boundary)

Common/trainable models stay in `llm_mvp_core` and are consumed via
manifests/locators. If a path on this platform (e.g. a `SmartWiki`/
`SmartCodeAssistant` flow) is *claimed* trainable, mental-debug treats it as a
review gate: require numeric held-out metrics, or label it an honest-untrained
scaffold. A deterministic `Contains`/dictionary/regex answer dressed as semantic
is a Rule 1 + Rule 8 finding — see `code-examination-SKILL.md` trainable add-on.

---

## Integration

- `systematic-debugging-SKILL.md` — the four-phase frame; this skill is the per-symptom router.
- `root-cause-tracing-SKILL.md` — backward tracing for Rule 3/Rule 6 hits.
- `data-flow-integrity-SKILL.md` — Rule 4 across a full producer→consumer chain.
- `event-driven-debugging-SKILL.md` — Rule 6/Rule 8 on the outbox/scheduler chain.
- `dev-safety-SKILL.md` — `DataProcessResult<T>`-only + fabric-via-interface DNA quick check.
