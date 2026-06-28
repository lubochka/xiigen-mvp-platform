# DNA Compliance Guard (XIIGen Enterprise Platform)

## Overview

Per-commit compliance gate for every new or modified `.cs` file in `src/` (not
in `tests/`). Platform port of the core 10-rule DNA guard, re-bound through
`DNA-DOTNET-TRANSLATION.md` to the types that actually ship here.

Stack-bound to **.NET 10 ASP.NET Core Web API**, **xUnit** (`tests/`),
`DataProcessResult<T>` (`src/Xiigen.Kernel/DataProcessResult.cs`),
`MicroserviceBase<TService>` (`src/Xiigen.Kernel/MicroserviceBase.cs`), and the
fabric interfaces in `src/Xiigen.Kernel/FabricContracts.cs`,
`src/Xiigen.Kernel/TenantContext.cs`, and `src/Xiigen.Fabrics.Scheduler/Scheduler.cs`.
Gate-B is `dotnet build Xiigen.Platform.slnx` + `dotnet test`.

This is a real production gate: `MicroserviceBase<TService>` and
`DataProcessResult<T>` are production code in `src/Xiigen.Kernel/` and
`src/Xiigen.Platform.Marketplace/*Service.cs`, not only fixtures.

## When to load

- Before committing any new or modified `.cs` file under `src/`.
- During `EXECUTOR_INTERNAL` when a new service class is created.
- After any refactor that touches a public method signature or a fabric contract.

---

## The 10 DNA Guard Patterns (platform)

| Rule | Check | Detection |
|------|-------|-----------|
| Rule 1 | Every public service method returns `DataProcessResult<T>` (no throw for business logic) | see below |
| Rule 2 | Dependencies injected through fabric **interfaces**, no `new ConcreteFabric()` | see below |
| Rule 3 | Tunables come from options/config objects, not buried `const` magic numbers | `DNA-DOTNET-TRANSLATION.md` Rule 1 |
| Rule 4 | `dotnet test` green before commit | `dotnet test Xiigen.Platform.slnx 2>&1 \| tail -3` |
| Rule 5 | Tenant from `ITenantContextAccessor.Current`, no shared mutable static (singletons) | see below |
| Rule 6 | One commit per phase, correct message format | `git log --oneline -1` |
| Rule 7 | Idempotency (`IIdempotencyStore`) + atomic temp-then-rename for file writes | see below |
| Rule 8 | Persist (`UpsertAsync`) before dispatch (`AppendAsync` / `ScheduleDelayedAsync`) | read the flow |
| Rule 9 | Gate B is a runnable assertion (not English prose) | read the plan phase |
| Rule 10 | Thin controllers: business logic stays in services, not `Xiigen.Platform.Api` endpoints | see below |
| Base | Every platform service extends `MicroserviceBase<TService>` and an `I…` contract | see below |
| Result | `DataProcessResult<T>` is the only Result type (no raw return, no bool, no `OperationResult<T>`) | see below |

> No external ML frameworks are added here — learned/common models live in
> `llm_mvp_core` and are consumed through manifests/locators
> (`.xiigen/wiki-domain`). On platform, "no ML framework" stays a note-only
> rule; the active gate is "no business logic in the host/API layer".

---

## Pre-commit gate — run on every modified file

```bash
FILE="<path-to-modified-file>"

# Rule 1: public methods must return DataProcessResult<T> (or DataProcessResult)
grep -n "public .* [A-Za-z]\+(" "$FILE" | grep -v "DataProcessResult\|//\|ctor\|=>.*base\|Dispose"
# Review: any public business method without DataProcessResult is a violation

# Rule 1 continued: no throw for business logic
grep -n "throw new" "$FILE" | grep -v "InvalidOperationException(\"Tenant context"
# Expected: only programmer-error throws (null/impossible state), with a comment

# Rule 2: no concrete fabric instantiation
grep -n "new [A-Z][A-Za-z]*\(Store\|Service\|Accessor\|Scheduler\|Client\)(" "$FILE"
# Any `new InMemoryDocumentStore()` etc. inside a service is a violation — inject the interface

# Rule 5: no shared mutable static state in a singleton service
grep -n "static .*=" "$FILE" | grep -v "readonly\|const\|//"
# Expected: only static readonly / const

# Rule 7: file writes must be atomic
grep -n "File.WriteAllBytes\|File.WriteAllText" "$FILE" | grep -v "\.tmp"
# Review: a direct write without temp-then-rename is a violation

# Base: every service extends MicroserviceBase<TService>
grep -n "class .*Service" "$FILE" | grep -v "MicroserviceBase<\|InMemory\|ServiceCollectionExtensions\|: I.*Service$"
# Review: a platform service that is not MicroserviceBase<T> needs investigation
```

Expected for a clean file: all sections return no violations.

---

## Quick verdicts

| Observation | Rule | Action |
|-------------|------|--------|
| `public Task<string> AnswerAsync(...)` | Rule 1 | Change to `public Task<DataProcessResult<string>> AnswerAsync(...)` |
| `if (x == null) throw new ArgumentException(...)` for expected input | Rule 1 | `return Failure<T>("CODE", "message");` |
| `_documents = new InMemoryDocumentStore();` in ctor | Rule 2 | Inject `IDocumentStore documents` via constructor |
| `private static int _counter;` mutated per call | Rule 5 | Remove shared mutable static; scope per request |
| `File.WriteAllBytes(path, data);` | Rule 7 | temp-then-rename: write `path + ".tmp"`, then `File.Move(..., overwrite: true)` |
| Endpoint method contains validation + persistence + business rules | Rule 10 | Move logic into a `MicroserviceBase<T>` service; keep the endpoint thin |
| `public class FooService` (no base, no interface) | Base | Add `: MicroserviceBase<FooService>, IFooService` |
| `OperationResult<T>` used in a platform `.cs` file | Result | Replace with `DataProcessResult<T>` |

---

## When a violation is found

1. Do NOT commit the violating file.
2. Fix the violation in the production code (not in the test).
3. Re-run `dotnet build Xiigen.Platform.slnx` — must be 0 errors AND 0 warnings
   (`TreatWarningsAsErrors=true`).
4. Re-run `dotnet test Xiigen.Platform.slnx` — must be 0 failures, count >= baseline.
5. Re-run the pre-commit gate — must show no violations.
6. Then commit.

---

## Rule 2 — Fabric First expanded

The most common Rule 2 violation is `new ConcreteFabric()` instead of accepting
an interface. Bind to the real platform contracts:

| Concrete type (never `new` inside a service) | Interface to inject | Declared in |
|----------------------------------------------|---------------------|-------------|
| `InMemoryDocumentStore` | `IDocumentStore` | `src/Xiigen.Kernel/FabricContracts.cs` |
| `InMemoryIdempotencyStore` | `IIdempotencyStore` | `src/Xiigen.Kernel/FabricContracts.cs` |
| `InMemoryOutboxStore` | `IOutboxStore` | `src/Xiigen.Kernel/FabricContracts.cs` |
| `AsyncLocalTenantContextAccessor` | `ITenantContextAccessor` | `src/Xiigen.Kernel/TenantContext.cs` |
| `InMemorySchedulerService` | `ISchedulerService` | `src/Xiigen.Fabrics.Scheduler/Scheduler.cs` |

```csharp
// VIOLATION: concrete instantiation
public SmartCodeAssistantService()
{
    _documents = new InMemoryDocumentStore();   // ← VIOLATION Rule 2
}

// CORRECT: inject interfaces, pass descriptor + context to MicroserviceBase<T>
public SmartCodeAssistantService(
    IDocumentStore documents,
    ITenantContextAccessor tenantContextAccessor,
    ICorrelationContextAccessor correlationContextAccessor,
    ILogger<SmartCodeAssistantService> logger)
    : base(new ServiceDescriptor("xiigen-smart-code", "XIIGen Smart Code Assistant", "PLATFORM"),
           tenantContextAccessor, correlationContextAccessor, logger)
{
    _documents = documents;   // injected
}
```

---

## Rule 1 — DataProcessResult<T> return pattern

```csharp
// VIOLATION: raw return types / throw for business logic
public string GetTenant(string id) { ... }
public bool IsValid(SmartWikiIndexRequest r) { ... }
if (r.Nodes.Count == 0) throw new InvalidOperationException("no nodes");

// CORRECT: always wrapped (matches SmartWikiService.cs)
public DataProcessResult<string> GetTenant(string id) { ... }
public DataProcessResult<bool> IsValid(SmartWikiIndexRequest r) { ... }
if (r.Nodes.Count == 0)
    return Failure<IReadOnlyDictionary<string, object?>>("GRAPH_NODES_REQUIRED",
        "Smart Wiki index requires linked graph nodes.");
```

`DataProcessResult<T>` exposes `IsSuccess`, `IsFailure`, `Data`, `Error`
(`DataProcessError(Code, Message, Details)`), and `FromCache`
(`src/Xiigen.Kernel/DataProcessResult.cs`).

---

## MicroserviceBase<TService> pattern

```csharp
// CORRECT: MicroserviceBase<TService> + interface; descriptor first to base
public sealed class SmartWikiService : MicroserviceBase<SmartWikiService>, ISmartWikiService
{
    public SmartWikiService(
        IDocumentStore documents,
        ITenantContextAccessor tenantContextAccessor,
        ICorrelationContextAccessor correlationContextAccessor,
        ILogger<SmartWikiService> logger)
        : base(new ServiceDescriptor("xiigen-smart-wiki", "XIIGen Smart Wiki", "PLATFORM"),
               tenantContextAccessor, correlationContextAccessor, logger)
    { _documents = documents; }
}

// VIOLATION: missing base class
public sealed class SmartWikiService : ISmartWikiService { ... }

// VIOLATION: missing interface
public sealed class SmartWikiService : MicroserviceBase<SmartWikiService> { ... }
```

`MicroserviceBase<TService>` provides `CurrentTenant`, `CurrentCorrelationId`,
and the `Success<T>` / `Failure<T>` helpers (`src/Xiigen.Kernel/MicroserviceBase.cs`).

---

## Generated output audit

Any generated artifact (e.g. code/JSON a generator emits) must pass before commit:

```bash
GENERATED_FILE="<path>"

# 1. Generated C# parses (Roslyn) or the consuming project builds clean
dotnet build Xiigen.Platform.slnx 2>&1 | grep -E "error|warning"
# Expected: 0 errors AND 0 warnings (TreatWarningsAsErrors=true)

# 2. Generator returns DataProcessResult<T> on all paths
grep -n "public .*Generate" <generator-file> | grep -v "DataProcessResult"
# Expected: no output

# 3. No hardcoded absolute paths in generated output
grep -n "C:\\\\\|/home/\|/Users/" "$GENERATED_FILE"
# Expected: no output

# 4. Generated JSON evidence parses
#    (in a [Fact]) JsonDocument.Parse(File.ReadAllText(path)) must not throw
```

### Gate B format for generation phases

```
Gate B: dotnet build Xiigen.Platform.slnx exits 0 with 0 warnings after generation;
        <GeneratorClass>.Generate(<sample>) returns IsSuccess=true;
        generated output contains the expected element/record count (was 0)
```

---

## Governance binding

Do not commit secrets (AGENTS.md "No Secrets In Repo"): no raw AWS access keys,
GitHub tokens, Vault values, JWT signing keys, Docker registry tokens, tenant
secret payloads, or license HMAC secrets — evidence records proof-of-presence
metadata and redacted identifiers only. A DNA pass is `EXECUTOR_INTERNAL`
evidence; it hands off to `REVIEW_INTERNAL` and never authorizes a final report
by itself.
