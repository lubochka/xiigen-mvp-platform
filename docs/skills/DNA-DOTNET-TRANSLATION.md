# DNA Compliance — .NET Translation (XIIGen Enterprise Platform)

## Overview

The original nine DNA coding rules were written for XIIGen's NestJS/TypeScript
SaaS platform (`DataProcessResult<T>`, `MicroserviceBase`, `tenantId` from
async context, idempotent dedup, persist-before-dispatch, CloudEvents). This
document is the **seed translation** of those rules to the platform's .NET stack
so `dna-compliance-guard-SKILL.md` and `dev-safety-SKILL.md` can reuse it
verbatim.

Stack-bound to **.NET 10 ASP.NET Core Web API**, **xUnit** (`tests/`), the real
Result type `DataProcessResult<T>` (`src/Xiigen.Kernel/DataProcessResult.cs`),
`MicroserviceBase<TService>` (`src/Xiigen.Kernel/MicroserviceBase.cs`), and the
platform fabric interfaces. Gate-B is `dotnet build Xiigen.Platform.slnx` +
`dotnet test`.

This is **not** a separate target skill — it is the shared lift the two .NET DNA
skills depend on. It is the platform analogue of the `core/.skills/`
`DNA-DOTNET-TRANSLATION.md` bridge, re-bound to the real types that already ship
in this repository.

> **Result type note.** The core mvp bridge proposes a generic `Result<T>`. The
> platform actually implements `DataProcessResult<T>` (the same NestJS-derived
> idiom as mvp). Use `DataProcessResult<T>` everywhere below — **not**
> `OperationResult<T>` and **not** a bare `Result<T>`.

---

## Rule 1 — Configuration/options data is not buried as inline constants

**Original (TS):** business data uses `Record<string, unknown>`; no typed model
classes for freedom/config data.

**In .NET:** machine contracts (request/response records such as
`SmartWikiIndexRequest`, `ScheduledJobResult`) ARE typed because they are the
contract. But tunable values a caller might override must come from
options/config objects, not from `private const` magic numbers buried in logic.

```csharp
// BAD — hidden magic number inside a service method
var ttl = TimeSpan.FromMinutes(15);   // buried constant

// GOOD — comes from an injected options object
var ttl = _options.ReservationTtl;
```

**Detection:**
```bash
grep -rn "TimeSpan.From\|= 0\.[0-9]\+\|= [0-9]\{3,\}" src/ --include="*.cs" \
  | grep -v "readonly\|const\|//\|Tests\|test"
```

---

## Rule 2 — Dynamic filter building (no hardcoded selectors)

**Original:** all searches use a dynamic filter builder; empty fields auto-excluded.

**In .NET:** document/search reads go through `IDocumentStore.SearchAsync` with a
`filter` dictionary; do not hardcode field equality inside business logic.

```csharp
// BAD — hardcoded selector
await _documents.SearchAsync(coll, new Dictionary<string, object?> { ["status"] = "Active" });

// GOOD — caller-supplied filter, empty entries excluded by the store
await _documents.SearchAsync(coll, request.Filter);
```

`IDocumentStore.SearchAsync(string collection, IReadOnlyDictionary<string, object?> filter, …)`
lives in `src/Xiigen.Kernel/FabricContracts.cs`.

---

## Rule 3 — Result pattern, never throw for business logic

**Original:** every service method returns `DataProcessResult<T>`; exceptions are
for infrastructure failures only.

**In .NET:** every public service method that handles an expected failure returns
`DataProcessResult<T>` / `DataProcessResult`. Use `MicroserviceBase<TService>`'s
`Success<T>(...)` / `Failure<T>(code, message)` helpers. Throw only for
programmer errors (null refs, impossible internal state).

```csharp
// BAD
if (string.IsNullOrWhiteSpace(request.ProjectId))
    throw new ArgumentException("Project id required");

// GOOD (matches src/Xiigen.Platform.Marketplace/SmartWikiService.cs)
if (string.IsNullOrWhiteSpace(request.ProjectId))
    return Failure<IReadOnlyDictionary<string, object?>>("PROJECT_ID_REQUIRED", "Project id is required.");
```

**Note:** infrastructure exceptions (HTTP client, DB driver) are caught at the
boundary and wrapped in a `DataProcessResult.Failure(code, message)`.

---

## Rule 4 — Interface / base-class contracts

**Original:** all services extend `MicroserviceBase`.

**In .NET:** platform services extend `MicroserviceBase<TService>` and depend on
fabric **interfaces**, never on orphan concrete classes. Fabric contracts:
`IDocumentStore`, `IIdempotencyStore`, `IOutboxStore`
(`src/Xiigen.Kernel/FabricContracts.cs`), `ITenantContextAccessor`,
`ICorrelationContextAccessor` (`src/Xiigen.Kernel/`), `ISchedulerService`
(`src/Xiigen.Fabrics.Scheduler/Scheduler.cs`).

```csharp
// BAD — standalone service, no base, concrete dependency
public class SmartWikiService { private readonly InMemoryDocumentStore _docs = new(); }

// GOOD — MicroserviceBase<T> + injected interface
public sealed class SmartWikiService : MicroserviceBase<SmartWikiService>, ISmartWikiService
{
    private readonly IDocumentStore _documents;
    public SmartWikiService(IDocumentStore documents, ITenantContextAccessor t,
        ICorrelationContextAccessor c, ILogger<SmartWikiService> l)
        : base(new ServiceDescriptor("xiigen-smart-wiki", "XIIGen Smart Wiki", "PLATFORM"), t, c, l)
        => _documents = documents;
}
```

---

## Rule 5 — Scope isolation (tenant + thread safety)

**Original:** every query scoped to tenant via AsyncLocalStorage.

**In .NET:** tenant identity comes from `ITenantContextAccessor.Current`
(`AsyncLocalTenantContextAccessor` uses `AsyncLocal<TenantContext?>`), never a
method parameter. Services are registered as singletons, so they must hold **no
per-request mutable static state**. In-memory fabrics guard mutation with a
`Lock` (see `InMemorySchedulerService._gate`).

```csharp
// BAD — tenant passed in, shared mutable static
public Task DoAsync(string tenantId) { _lastTenant = tenantId; ... }

// GOOD — tenant from context, no shared mutation
var tenant = CurrentTenant.TenantId;   // MicroserviceBase<T>.CurrentTenant
```

**Detection:**
```bash
grep -rn "static .*=" src/ --include="*.cs" | grep -v "readonly\|const\|//\|Tests"
```

---

## Rule 6 — Generic controllers

**In .NET:** ASP.NET Core controllers/minimal-API endpoints in
`src/Xiigen.Platform.Api` must stay thin: bind the request, resolve the tenant
context, call the application/fabric service, map `DataProcessResult<T>` to an
HTTP result. No business logic, no per-entity hand-written controllers with
embedded rules.

---

## Rule 7 — Idempotency for repeated operations

**Original:** all queue consumers dedup using idempotency keys.

**In .NET:** repeatable operations reserve through
`IIdempotencyStore.TryReserveAsync(operationKey, ttl, …)` and finish with
`MarkSucceededAsync` / `MarkFailedAsync` (`src/Xiigen.Kernel/FabricContracts.cs`;
`InMemoryIdempotencyStore` in `src/Xiigen.Fabrics.InMemory/`). The scheduler
already returns the cached job for a repeated `idempotencyKey`
(`InMemorySchedulerService.ScheduleDelayedAsync` returns `fromCache: true`).

For file/checkpoint/export writes use atomic temp-then-rename:
```csharp
File.WriteAllBytes(path + ".tmp", data);
File.Move(path + ".tmp", path, overwrite: true);
```

---

## Rule 8 — Persist state before dispatching work (outbox)

**Original:** `storeDocument()` MUST happen before `enqueue()`.

**In .NET:** store the document via `IDocumentStore.UpsertAsync` **before**
appending the event via `IOutboxStore.AppendAsync` and before scheduling
follow-up work via `ISchedulerService.ScheduleDelayedAsync`. If the process
crashes, no event/job exists for a document that was never persisted.

```csharp
var stored = await _documents.UpsertAsync(coll, id, body, ct: ct);   // 1. persist
if (stored.IsFailure) return Failure<T>(stored.Error!.Code, stored.Error.Message);
await _outbox.AppendAsync(envelope, ct);                              // 2. then dispatch
```

---

## Rule 9 — CloudEvents envelope

**In .NET:** cross-service messages are wrapped in `CloudEventEnvelope`
(`src/Xiigen.Kernel/FabricContracts.cs`) and appended through `IOutboxStore`.
Do not emit ad-hoc event payloads.

---

## Dev Safety Gate (.NET)

Run at the start AND end of every phase:

```bash
dotnet build Xiigen.Platform.slnx
# Directory.Build.props sets TreatWarningsAsErrors=true -> expect 0 errors AND 0 warnings

dotnet test Xiigen.Platform.slnx --logger "console;verbosity=minimal"
# Expected: 0 failed, all passed, count >= baseline
```

**Blocked condition:** any build error/warning or test failure blocks the phase
from closing.

---

## Test Integrity Rules (.NET)

- Tests assert domain outcomes, not just "ran without exception".
- For `DataProcessResult<T>`, assert `IsSuccess` AND a real `Data` field value
  (`Assert.Equal(expected, result.Data!.Field)`), not only `Assert.True(result.IsSuccess)`.
- Value-range / structural assertions on known properties are behavioral
  assertions — include them.
- Zero failures = zero failures. No "pre-existing" carve-outs.

---

## Applicable Rules Summary

| Rule | Applies on platform | How |
|------|---------------------|-----|
| 1 — config not inline | Yes | Tunables from options objects, not buried constants |
| 2 — dynamic filters | Yes | `IDocumentStore.SearchAsync(filter)` |
| 3 — Result pattern | Yes | All public service methods return `DataProcessResult<T>` |
| 4 — interface/base contracts | Yes | `MicroserviceBase<TService>` + fabric interfaces |
| 5 — scope isolation | Yes | Tenant from `ITenantContextAccessor`; no shared mutable static |
| 6 — thin controllers | Yes | `Xiigen.Platform.Api` endpoints stay thin |
| 7 — idempotent writes | Yes | `IIdempotencyStore` + atomic temp-then-rename |
| 8 — persist before dispatch | Yes | `UpsertAsync` before `AppendAsync` / `ScheduleDelayedAsync` |
| 9 — event envelope | Yes | `CloudEventEnvelope` via `IOutboxStore` |
