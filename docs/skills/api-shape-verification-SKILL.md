# API Shape Verification (XIIGen Enterprise Platform)

## When to load

- At infrastructure discovery (Gate 0 of a phase) — verify every class/interface
  before writing tests for it.
- Before writing any xUnit test that calls a method — verify signature and return
  type.
- After any phase whose discovery rate (planned-vs-actual API shape) ran high.

Platform port of the core `api-shape-verification` skill, re-bound to
**.NET 10 ASP.NET Core Web API**, **xUnit**, and the real
`Xiigen.Kernel`/`Xiigen.Fabrics.*`/`Xiigen.Freedom` contracts. The return wrapper
to verify here is `Task<DataProcessResult<T>>` (`src/Xiigen.Kernel/DataProcessResult.cs`),
**not** `OperationResult<T>`.

---

## Why This Exists

Most write-time plan fixes are API shape discoveries:
- Assumed a method takes 2 parameters → it actually takes 5 (the scheduler does).
- Assumed a method is synchronous → it returns `Task<DataProcessResult<T>>`.
- Assumed a property → it is a method, or vice versa.
- Assumed an interface has method X → it does not (renamed or never existed).

Verifying API shapes at Gate 0 costs ~30 seconds per symbol. Discovering a mismatch
mid-implementation costs a blocked session.

**Concrete platform example (this exact trap):** the scheduler method is
`ScheduleDelayedAsync(string action, TimeSpan delay, IReadOnlyDictionary<string,object?> payload, string idempotencyKey, CancellationToken)` —
there is **no** `ScheduleAt(...)` method. A plan that assumes `ScheduleAt` will not
compile. This is exactly why the shape check is mandatory.

---

## Verification Protocol

For each class/interface the plan will use:

```bash
# Step 1: Verify the class/interface exists
grep -rn "public.*class ClassName\|public.*interface IName" src/ --include="*.cs"

# Step 2: Verify the specific method exists, with its full signature
grep -n "MethodNameAsync" src/Xiigen.Kernel/FabricContracts.cs

# Step 3: Confirm async + return wrapper (Task<DataProcessResult<T>> vs T)
grep -n "Task<DataProcessResult" src/Xiigen.Kernel/FabricContracts.cs | head -20

# Step 4: For a property vs method, confirm shape
grep -n "Current" src/Xiigen.Kernel/TenantContext.cs
# -> `TenantContext? Current { get; }` is a PROPERTY, not Current()

# Step 5: Count the parameters (the scheduler trap)
grep -n -A6 "ScheduleDelayedAsync" src/Xiigen.Fabrics.Scheduler/Scheduler.cs
```

Use IDE "Go to definition" as the equivalent of these greps.

---

## Verification Table

Produce this table before writing any test code that uses a class. The verified
real signatures below are the platform ground truth:

| Symbol | Source File | Type | Signature | Return Type | Notes | Verified |
|--------|-------------|------|-----------|-------------|-------|----------|
| `IDocumentStore.GetAsync` | `src/Xiigen.Kernel/FabricContracts.cs` | method | `GetAsync(string collection, string id, CancellationToken=default)` | `Task<DataProcessResult<DocumentRecord?>>` | nullable record | ☐ |
| `IDocumentStore.SearchAsync` | `src/Xiigen.Kernel/FabricContracts.cs` | method | `SearchAsync(string collection, IReadOnlyDictionary<string,object?> filter, CancellationToken=default)` | `Task<DataProcessResult<IReadOnlyList<DocumentRecord>>>` | filter dict | ☐ |
| `IDocumentStore.UpsertAsync` | `src/Xiigen.Kernel/FabricContracts.cs` | method | `UpsertAsync(string collection, string id, IReadOnlyDictionary<string,object?> body, string? expectedVersion=null, CancellationToken=default)` | `Task<DataProcessResult<DocumentRecord>>` | optimistic version | ☐ |
| `IIdempotencyStore.TryReserveAsync` | `src/Xiigen.Kernel/FabricContracts.cs` | method | `TryReserveAsync(string operationKey, TimeSpan ttl, CancellationToken=default)` | `Task<DataProcessResult<IdempotencyReservation>>` | reserve | ☐ |
| `IIdempotencyStore.MarkSucceededAsync` | `src/Xiigen.Kernel/FabricContracts.cs` | method | `MarkSucceededAsync(string operationKey, IReadOnlyDictionary<string,object?> response, CancellationToken=default)` | `Task<DataProcessResult>` | non-generic result | ☐ |
| `IOutboxStore.AppendAsync` | `src/Xiigen.Kernel/FabricContracts.cs` | method | `AppendAsync(CloudEventEnvelope envelope, CancellationToken=default)` | `Task<DataProcessResult<OutboxRecord>>` | event delivery | ☐ |
| `IOutboxStore.ListAsync` | `src/Xiigen.Kernel/FabricContracts.cs` | method | `ListAsync(CancellationToken=default)` | `Task<DataProcessResult<IReadOnlyList<OutboxRecord>>>` | — | ☐ |
| `ISchedulerService.ScheduleDelayedAsync` | `src/Xiigen.Fabrics.Scheduler/Scheduler.cs` | method | `ScheduleDelayedAsync(string action, TimeSpan delay, IReadOnlyDictionary<string,object?> payload, string idempotencyKey, CancellationToken=default)` | `Task<DataProcessResult<ScheduledJobResult>>` | **no `ScheduleAt`** | ☐ |
| `ISchedulerService.ListAsync` | `src/Xiigen.Fabrics.Scheduler/Scheduler.cs` | method | `ListAsync(CancellationToken=default)` | `Task<DataProcessResult<IReadOnlyList<ScheduledJobResult>>>` | — | ☐ |
| `IFreedomConfigService.GetAsync` | `src/Xiigen.Freedom/FreedomConfig.cs` | method | `GetAsync(string key, CancellationToken=default)` | `Task<DataProcessResult<FreedomConfigValue?>>` | nullable value | ☐ |
| `IFreedomConfigService.SetAsync` | `src/Xiigen.Freedom/FreedomConfig.cs` | method | `SetAsync(string key, object? value, string valueType, CancellationToken=default)` | `Task<DataProcessResult>` | non-generic result | ☐ |
| `ITenantContextAccessor.Current` | `src/Xiigen.Kernel/TenantContext.cs` | property | `TenantContext? Current { get; }` | `TenantContext?` | property, not method | ☐ |
| `ITenantContextAccessor.Push` | `src/Xiigen.Kernel/TenantContext.cs` | method | `Push(TenantContext context)` | `IDisposable` | scope-based | ☐ |
| `ICorrelationContextAccessor.Current` | `src/Xiigen.Kernel/CorrelationContext.cs` | property | `CorrelationContext? Current { get; }` | `CorrelationContext?` | property | ☐ |

---

## Rules

1. **Never assume** — even "obvious" APIs may have changed; the scheduler has no
   `ScheduleAt`.
2. **Check the return wrapper** — `Task<DataProcessResult<T>>` vs
   `Task<DataProcessResult>` (non-generic) vs `T` — all different. Some fabric
   methods return the non-generic `DataProcessResult` (`MarkSucceededAsync`,
   `SetAsync`).
3. **Check sync vs async** — every fabric method here returns `Task<...>`; calling it
   without `await` is a bug.
4. **Check parameter order and count** — `ScheduleDelayedAsync` has 5 parameters
   (4 required + ct).
5. **Property vs method** — `Current` is a property; do not write `Current()`.
6. **If a symbol is not found** — STOP. Do not write the test. Report it.
7. **Complete the verification table before writing the first test line.**

---

## Test Strategy (per branch)

- **Unit (xUnit):** the first test for any fabric consumer asserts a real
  `Data` field of the returned `DataProcessResult<T>` (e.g.
  `Assert.Equal(jobId, result.Data!.JobId)`), proving the shape was verified.
- **Negative:** a non-generic `DataProcessResult` path (`MarkFailedAsync`) asserts
  `IsFailure` and `Error!.Code`.
- **Gate-B:** `dotnet build Xiigen.Platform.slnx` (0/0) compiling against the real
  signatures is itself the strongest shape proof; `dotnet test` (0 failed).

---

## Integration with Plan Execution Feedback

After each phase, record any shape mismatches discovered during implementation:

```
API shape discovery (write-time fix):
  Planned: scheduler.ScheduleAt(dueDate, payload)
  Actual:  scheduler.ScheduleDelayedAsync(action, delay, payload, idempotencyKey, ct)
  Fix: compute delay from (dueDate - now); pass action + idempotency key
```

If discovery rate runs high for 2 phases, the next phase must complete this full
verification table at Gate 0.

---

## Integration

- `dna-compliance-guard-SKILL.md` enforces `DataProcessResult<T>` on the methods you
  verify here.
- `blast-radius-SKILL.md` uses the same grep-the-callers step for interface changes.
