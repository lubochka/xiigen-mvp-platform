# REMOVED-FROM-XIIGEN.md — stack-map alias (XIIGen Enterprise Platform)

> Ported from the `llm_mvp_core` `REMOVED-FROM-XIIGEN.md` bridge, **inverted** for this
> platform repo. The canonical, full content lives in **`STACK-MAP.md`** in this same
> directory; this file is the same map under the core file's original name so that a
> reader following a `REMOVED-FROM-XIIGEN.md` reference lands on the correct content.
>
> Adapted for **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B = `dotnet build` / `dotnet test`.
> This repo has **no frontend** — UI rows are `na`.

## Why this file exists

In `llm_mvp_core`, `REMOVED-FROM-XIIGEN.md` records "nothing was thrown away from XIIGen —
every concept was translated to its .NET equivalent" (Elasticsearch → BM25/HNSW,
Redis → `ISessionStorage`, NestJS DI → .NET DI, APP_GUARD → PermissionGate, …). Here the
direction is **inverted**: it records, for the platform, *which core concept maps to which
platform class*, so porting a core skill is unambiguous.

## The one rule

`OperationResult<T>` is a **core** token and does **not** appear in this repo's `src/` (0×).
The platform domain return type is `DataProcessResult<T>`
(`src/Xiigen.Kernel/DataProcessResult.cs`). `Results.Ok` / `Results.Problem` /
`ProblemDetails` are HTTP transport only (`Xiigen.Platform.Api` / `Program.cs`).

## Quick map (full table in `STACK-MAP.md`)

| Core concept | Platform equivalent | Location |
|---|---|---|
| `OperationResult<T>` | `DataProcessResult<T>` (`.Success` / `.Failure`) | `src/Xiigen.Kernel/DataProcessResult.cs` |
| Fabric First (interface resolution) | built-in .NET DI, constructor-injected interfaces | `AddXiigen*` registrations |
| `ISessionStorage` | `IDocumentStore` | `src/Xiigen.Kernel/FabricContracts.cs` |
| outbox / idempotency / scheduler fabrics | `IOutboxStore` / `IIdempotencyStore` / `ISchedulerService` | `FabricContracts.cs`, `Scheduler.cs` |
| tenant isolation | `ITenantContextAccessor` (`Current` / `Push`) | `src/Xiigen.Kernel/TenantContext.cs` |
| FREEDOM config | `IFreedomConfigService.GetAsync(key)` → `DataProcessResult<FreedomConfigValue?>` | `src/Xiigen.Freedom/FreedomConfig.cs` |
| `ILearningMachine` / DPO / checkpoints / `DomainModelRegistry` | **stay in `llm_mvp_core`** (R5/R6), consumed via manifest/locator | note-only |

> **Canonical reference:** see `STACK-MAP.md` for the complete concept → equivalent →
> location table, the ML/trainable boundary, anti-patterns, and integration notes.

## Integration

- `STACK-MAP.md` — canonical full version of this map.
- `HOW-TO-USE-SKILLS.md` — names both `STACK-MAP.md` and `REMOVED-FROM-XIIGEN.md` for the "map a core concept" trigger.
