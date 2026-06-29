# Service Boundary Design (XIIGen Enterprise Platform)

## What This Skill Prevents

**Over-splitting:** one algorithm per class where a cohesive service is correct.
Tests are shallow; each class is trivially simple with no meaningful behavior to
test.

**Under-splitting:** one class that does everything. Untestable in isolation;
changes to one concern break unrelated concerns.

Platform port of the core `service-boundary-design` skill, re-bound to
**.NET 10 ASP.NET Core Web API**, **xUnit** (`tests/`),
`DataProcessResult<T>` (`src/Xiigen.Kernel/DataProcessResult.cs`),
`MicroserviceBase<TService>` (`src/Xiigen.Kernel/MicroserviceBase.cs`), and the
real fabric contracts in `src/Xiigen.Kernel/` and `src/Xiigen.Fabrics.*`.
Gate-B is `dotnet build Xiigen.Platform.slnx` + `dotnet test`.

## When to load

- Before deciding whether a new capability is its own class/project or a method.
- During any multi-project planning session that crosses `Xiigen.*` boundaries.
- Before adding a new `I*` contract to `Xiigen.Kernel`.
- When `shared-service-design-SKILL.md` flags a boundary that spans projects.

---

## The Four Boundary Tests

Apply all four. The boundary that passes the most tests is correct.

### Test 1 — Transaction Boundary Test

Classes that must succeed or fail atomically belong in the same class.

```
SmartWikiService.BuildIndex() + SmartWikiService.PersistGraph()
→ If persist fails after build, the wiki index is inconsistent
→ They share a consistency boundary
→ MERGE into one class

SmartWikiService.BuildIndex() + IOutboxStore.AppendAsync()
→ The dispatched event is independent of the in-memory build step
→ But Rule 8 (persist-before-dispatch): UpsertAsync THEN AppendAsync, both inside
  the same service method's success path — they share the same transaction boundary
  even though they are different fabrics
```

### Test 2 — Lifecycle Test

Classes with different lifetimes, caching policies, or update frequencies belong
in different classes.

```
IFreedomConfigService — config read per request, cached by version → its own fabric
ISchedulerService — holds scheduled jobs across requests → its own fabric
A stateless per-request scorer that READS config → separate class from the config store
```

On platform, services are registered as **singletons** (DNA Rule 5), so a class
that needs per-request mutable state is a design smell, not a lifecycle reason to
split — scope per request instead (`ITenantContextAccessor.Current`).

### Test 3 — Consumer Independence Test

If component A's output is consumed by code that does not need component B's
output, A and B belong in different classes — and usually different `I*`
contracts in `Xiigen.Kernel`.

```
IDocumentStore.SearchAsync() → consumed by every read-side service
IOutboxStore.AppendAsync()   → consumed by write-side services that dispatch events
IIdempotencyStore.TryReserveAsync() → consumed by repeatable operations only

Different consumers → different interfaces → separate fabric contracts.
```

### Test 4 — Domain Ownership Test

Classes in different conceptual domains belong in different classes even if they
run sequentially.

```
Tenant resolution (ITenantContextAccessor) → owns: tenant identity, scope
Scheduling (ISchedulerService)             → owns: delayed jobs, due times
Document persistence (IDocumentStore)      → owns: collections, versions

Different domains → different classes? YES — and on platform they live in
different assemblies: Kernel contracts vs Fabrics.* implementations vs
Platform.Api entry. Project boundary = domain; class boundary = single coherent
responsibility.
```

---

## The Decision Table

| Transaction shared? | Lifecycle shared? | Consumers shared? | Decision |
|---------------------|-------------------|-------------------|----------|
| YES | YES | YES | MERGE — one class |
| YES | YES | NO | MERGE — same class, callers call it separately |
| YES | NO | any | SPLIT — different classes, same assembly |
| NO | YES | YES | MERGE if domains are compatible |
| NO | YES | NO | SPLIT |
| NO | NO | any | SPLIT — definitely separate |

This Decision Table is binding: a boundary decision in a plan must cite the row
it lands on, not "it felt cleaner".

---

## Method vs Class Boundary

Not everything needs its own class. Some things are methods within a class.

**Becomes its own CLASS when:**
- It owns state (fields) that must be managed independently.
- It has its own lifecycle (constructed once vs per-call).
- It can be swapped at runtime (Fabric First — behind an `I*` interface).
- It has 3+ callers across different assemblies/projects.

**Stays a METHOD within a class when:**
- It is stateless and called synchronously.
- Its failure does not change the calling class's state.
- No other class needs it independently.

```
// METHOD inside SmartWikiService — stateless, single caller, no swap need
private static double ScoreGraphNode(GraphNode node) { ... }

// SEPARATE CLASS behind ISchedulerService — swappable at DI registration time
InMemorySchedulerService : ISchedulerService
  - Different implementation could replace it (Hangfire/queue-backed) via DI
```

---

## Project (Assembly) Boundary Design

The platform's coarsest boundary is the `.csproj`. The boundary is already
established:

| Layer | Project | Owns |
|-------|---------|------|
| Domain / contracts | `Xiigen.Kernel` | `I*` fabric contracts, `DataProcessResult<T>`, `MicroserviceBase<T>`, tenant/correlation context |
| I/O implementations | `Xiigen.Fabrics.InMemory`, `Xiigen.Fabrics.Scheduler` | concrete `IDocumentStore`/`IOutboxStore`/`IIdempotencyStore`/`ISchedulerService` |
| Config | `Xiigen.Freedom` | `IFreedomConfigService` |
| Entry | `Xiigen.Platform.Api` | thin HTTP endpoints + DI composition (`Program.cs`) |
| Feature services | `Xiigen.Platform.Marketplace` | `MicroserviceBase<T>` services (e.g. `SmartWikiService`) |

**New project when:** genuinely different domain with its own fabric contracts,
or a clear new layer. **Same project when:** classes implement variants of the
same contract or are closely related.

Consumer-Independence example bound to platform: `IDocumentStore` and
`IOutboxStore` live as contracts in `Xiigen.Kernel`; their implementations live
in `Xiigen.Fabrics.InMemory`. Never put a fabric implementation in
`Xiigen.Kernel`, and never put a contract only in the implementation project.

---

## Recording Boundary Decisions

For each significant boundary decision, record it where the platform keeps locked
decisions (AGENTS.md / CLAUDE.md governance, or a `docs/` decisions file):

```
D-NN: SmartWikiService owns both index build and graph persistence in one class
Decision: single MicroserviceBase<T> service, not a pipeline of separate classes
Alternatives: separate IndexBuilder + GraphPersister + DispatchStep
Rationale: build+persist share a transaction boundary (Test 1); splitting yields
           three trivial classes where one meaningful service is correct
Affected: Xiigen.Platform.Marketplace, any caller of ISmartWikiService
```

---

## Test Strategy (per branch)

- **Unit (xUnit):** each boundary class tested in isolation with fakes for the
  fabrics it depends on; assert a real `DataProcessResult<T>.Data` field, not only
  `IsSuccess` (see `DNA-DOTNET-TRANSLATION.md` Test Integrity).
- **Integration:** the merged class exercised against `Xiigen.Fabrics.InMemory`
  implementations to prove the transaction boundary holds end to end.
- **Negative:** a failing fabric call returns `Failure<T>` (not a thrown
  exception), proving Rule 1 across the boundary.
- **Gate-B:** `dotnet build Xiigen.Platform.slnx` (0 errors, 0 warnings —
  `TreatWarningsAsErrors=true`) and `dotnet test` (0 failed, count ≥ baseline).

---

## Anti-Patterns

1. **One UML/endpoint box = one class** — boxes describe interactions, not
   implementation units.
2. **Splitting purely on technical lines** — a `DocumentWriter` and
   `DocumentReader` as separate classes when one `IDocumentStore` covers both.
3. **Making every stateless helper its own class** — a scoring helper is a static
   method, not a class.
4. **Under-splitting for simplicity** — a 500-line service with 8 unrelated
   responsibilities is not simple.
5. **Putting a fabric implementation in `Xiigen.Kernel`** — contracts in Kernel,
   implementations in `Xiigen.Fabrics.*` (Consumer-Independence violation).

---

## Integration

- `shared-service-design-SKILL.md` decides whether a boundary should be an
  assembly boundary or a namespace boundary (≥3-consumer threshold).
- `blast-radius-SKILL.md` assesses the impact of changing an `I*` contract before
  the split/merge is committed.
- `dna-compliance-guard-SKILL.md` enforces `MicroserviceBase<T>` + interface +
  `DataProcessResult<T>` on every boundary class.
