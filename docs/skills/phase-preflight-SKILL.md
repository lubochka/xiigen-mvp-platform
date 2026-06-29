# Phase Preflight

> Ported from the `llm_mvp_core` planning standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit. Verify-steps read
> ASP.NET Core DI in `src/Xiigen.Platform.Api/Program.cs`
> (`builder.Services.Add...`, `AddXiigenInMemoryFabrics()`,
> `AddXiigenMarketplace()`), minimal-API routes (`MapGet` / `MapPost` /
> `MapGroup` → `Results.*`, **not** MVC controllers), fabric contracts
> (`IDocumentStore` / `IOutboxStore` / `IIdempotencyStore` /
> `ITenantContextAccessor` / `ISchedulerService`), and xUnit fixtures **before**
> any edit. Domain return type is `DataProcessResult<T>` (never `OperationResult<T>`).

## When to Run

Run this skill **before writing any code in a phase**. It catches wrong
assumptions before they are baked into implementation. (Also enforced by
`Directory.Build.props`: `TreatWarningsAsErrors=true` — a wrong assumption that
compiles with a warning still breaks the build.)

## Assumption Registry

Before writing the first line, record every assumption the plan makes about the
codebase, each with a verify command and a PENDING status:

```
ASSUMPTION-1: MarketplaceService implements IMarketplaceService and inherits MicroserviceBase
  Verify: grep -rn "class MarketplaceService" src/Xiigen.Platform.Marketplace/ --include="*.cs"
  Expected: "public sealed class MarketplaceService : MicroserviceBase<MarketplaceService>, IMarketplaceService"
  Status: [ ] PENDING

ASSUMPTION-2: IDocumentStore.UpsertAsync exists with optimistic concurrency
  Verify: Read src/Xiigen.Kernel/FabricContracts.cs (IDocumentStore)
  Expected: Task<DataProcessResult<DocumentRecord>> UpsertAsync(string collection, string id, IReadOnlyDictionary<string,object?> body, string? expectedVersion = null, CancellationToken ct = default)
  Status: [ ] PENDING

ASSUMPTION-3: Fabrics are registered in Program.cs
  Verify: grep -n "AddXiigenInMemoryFabrics\|AddXiigenMarketplace\|AddSingleton<ITenantContextAccessor" src/Xiigen.Platform.Api/Program.cs
  Expected: those registrations present in builder.Services
  Status: [ ] PENDING
```

Verify every assumption against live code (not from memory). Change the plan if
any assumption is wrong.

## Fix Patterns by Type

### Pattern A — Service Dependency Injection (adding a dependency to an existing service)
```
Verify before touching the file:
1. Read the service constructor completely (it passes a ServiceDescriptor and the
   MicroserviceBase<TSelf> deps to base)
2. Record the current constructor signature
3. Find all DI registrations of the service:
   grep -rn "AddSingleton<I.*,.*Service>\|AddScoped<.*Service>\|AddXiigenMarketplace" src/ --include="*.cs"
4. Confirm the registration location (Program.cs and/or an AddXiigen* extension method)
5. Plan: constructor change + DI registration change + any direct `new` callers
```

### Pattern B — Fabric Interface Change (adding a method to a Kernel contract)
```
Verify before changing the interface in src/Xiigen.Kernel/FabricContracts.cs (or Scheduler.cs / TenantContext.cs):
1. Read the interface completely
2. Find all implementations:
   grep -rn "class.*: IDocumentStore\|class.*: IOutboxStore\|class.*: IIdempotencyStore\|class.*: ISchedulerService" src/ --include="*.cs"
3. Find all callers:
   grep -rn "IDocumentStore\.\|IOutboxStore\.\|IIdempotencyStore\.\|ISchedulerService\." src/ --include="*.cs"
4. Plan: new method signature + implementation in each concrete class + callers
```
Fabric-contract changes have wide blast radius (in-memory + any future real
provider). If there is more than one implementation, escalate to Luba before
proceeding.

### Pattern C — New Endpoint / Route (adding a minimal-API route)
```
Verify before writing the route:
1. Locate the route group / map calls in Program.cs
   grep -n "MapGet\|MapPost\|MapGroup" src/Xiigen.Platform.Api/Program.cs
2. Confirm the domain service interface is already defined and registered
3. Confirm the route path does not already exist
4. Confirm authorization shape (RequireStudioRole / tier policy / idempotency) for the new route
5. Plan: new Map* delegate + ToHttpResult wrapping + any new request record + DI for the service
```

## Escalation Format
```
PREFLIGHT BLOCKER: [one-line description]
Assumption: [what the plan assumed]
Reality: [what the code actually shows]
Evidence: [file:line where reality is visible]
Options:
A. [option A — impact]
B. [option B — impact]
Recommended: [A or B and brief why]
Awaiting direction before proceeding.
```
Do not choose an option unilaterally unless it is clearly non-architectural.

## Out-of-Scope Issue Inventory
As you run preflight, record issues discovered that are out of scope for the
current phase (e.g. a direct `new` instead of fabric resolution), with file:line.
Do not fix them in this phase unless required to compile and test.

## Preflight Completion Criteria
```
□ All assumptions verified against live code (not from memory)
□ All failed assumptions have plan amendments
□ Fix pattern identified (Service DI / Fabric Interface / New Route)
□ All callers of any changed component identified
□ DI registration location confirmed (Program.cs / AddXiigen* extension)
□ No undocumented blockers remain
□ Out-of-scope issues recorded for future
```
Only then: begin writing code.

## Integration
Feeds Check 4 of `plan-self-validation-SKILL.md`; pairs with
`phase-planning-SKILL.md` Gate 0 (Infrastructure Discovery).
