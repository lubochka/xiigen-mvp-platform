# STACK-MAP.md — core concept → XIIGen Enterprise Platform equivalent

> Ported from the `llm_mvp_core` `REMOVED-FROM-XIIGEN.md` bridge, **inverted** for the
> XIIGen Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build` / `dotnet test`. In `llm_mvp_core` the bridge maps *xiigen concept →
> its .NET equivalent in core*. Here the bridge maps *core concept/skill → its
> equivalent in this platform repo*, so that when a core skill is ported the reader
> can see exactly which abstraction replaces which — and that **nothing was thrown
> away, everything was translated**.
>
> Aliases: this file is also published as `REMOVED-FROM-XIIGEN.md` (same content).
> This repo has **no frontend** — UI rows are `na`.

---

## The one rule

`OperationResult<T>` is a **core** concept. It does **not** appear in this repo's `src/`
(0×). It stays in `llm_mvp_core`. In every .NET line here the domain return token is
`DataProcessResult<T>` (`src/Xiigen.Kernel/DataProcessResult.cs`, ~160 token occurrences
in `src/` at audit time — the count drifts as code grows). `Results.Ok` / `Results.Problem`
/ `ProblemDetails` are the **HTTP layer only** (minimal-API host in `Xiigen.Platform.Api` /
`Program.cs`), never the domain wrapper.

---

## Concept → equivalent → location

| Core concept / skill | Platform equivalent | Location (verified) |
|---|---|---|
| `OperationResult<T>` (domain return wrapper) | `DataProcessResult<T>` — `.Success(value)` / `.Failure("CODE","msg")`, never throw for business logic | `src/Xiigen.Kernel/DataProcessResult.cs` |
| `OperationResult.Fail("X","CODE")` | `DataProcessResult.Failure("CODE","message")` | `src/Xiigen.Kernel/DataProcessResult.cs` |
| HTTP success/error shaping | `Results.Ok(...)` / `Results.Problem(...)` / `ProblemDetails` — **HTTP transport only**, not the domain wrapper | `Xiigen.Platform.Api` / `Program.cs` (minimal-API host) |
| Fabric First (resolve through interface, not concrete) | Built-in .NET DI — constructor injection through interfaces; concrete chosen at `AddXiigen*` registration | `src/*/...ServiceCollectionExtensions` (e.g. `AddXiigenFreedomConfig`) |
| `ISessionStorage` (core persistence fabric) | `IDocumentStore` (collection/id/version/body document fabric) | `src/Xiigen.Kernel/FabricContracts.cs` |
| Outbox / store-before-emit fabric | `IOutboxStore` (`AppendAsync` / `ListAsync`, `CloudEventEnvelope`) | `src/Xiigen.Kernel/FabricContracts.cs` |
| Idempotency / de-dup fabric | `IIdempotencyStore` (`TryReserveAsync(key, ttl)` / `MarkSucceeded` / `MarkFailed`) | `src/Xiigen.Kernel/FabricContracts.cs` |
| Delayed-work / scheduler fabric | `ISchedulerService` (`ScheduleDelayedAsync(action, delay, payload, idempotencyKey)` / `ListAsync`) | `src/Xiigen.Fabrics.Scheduler/Scheduler.cs` |
| Tenant scope / multi-tenant isolation | `ITenantContextAccessor` (`Current` / `Push`), `TenantContext` record | `src/Xiigen.Kernel/TenantContext.cs` |
| FREEDOM config (runtime-tunable values) | `IFreedomConfigService.GetAsync(key)` → `DataProcessResult<FreedomConfigValue?>`; **not** raw `IConfiguration`/`appsettings.json` (this repo holds no appsettings) | `src/Xiigen.Freedom/FreedomConfig.cs` |
| `FreedomConfigValue` typed config record | `FreedomConfigValue(Key, Value, ValueType, Scope, TenantId, Environment, Version)` | `src/Xiigen.Freedom/FreedomConfig.cs` |
| Gate B (`dotnet test` zero failures, from core) | Same — Gate-B = `dotnet build` / `dotnet test` (xUnit), exact `N passed` | repo-wide xUnit projects |
| DNA "never throw for business logic" | The `DataProcessResult<T>` return-type requirement is MACHINE | `src/Xiigen.Kernel/DataProcessResult.cs` |
| No-Secrets-in-repo | MACHINE invariant — secret value lives in Vault/config, never in source | `AGENTS.md` "No Secrets In Repo" |
| `ILearningMachine` / `ILogicalUnit` / DPO / checkpoints / model assets | **Stays in `llm_mvp_core`** (R5/R6) — this platform **consumes** trained models via manifest/locator; it does not train or store common models | `llm_mvp_core` (note-only here) |
| `DomainModelRegistry` / `DynamicGraph` / skill-graph drift engine | **Core-ML, group G12** — not replicated here | `llm_mvp_core` (note-only here) |

---

## ML / trainable boundary (R5/R6)

```text
Common model + creation logic + training sets  -> live in llm_mvp_core (saved/imported/exported/upgradeable)
This platform                                  -> consumes trained models via manifest/locator only
DPO / checkpoints / ablation / continue-training -> NOT run here; note-only
```

A core skill that names `ILearningMachine`, `DPO`, a checkpoint, a manifest, or a model
asset is ported as **note-only**: record that the capability stays in `llm_mvp_core` and is
consumed through the `.xiigen` manifest/locator. It is never re-implemented as a platform
service.

---

## Anti-patterns

1. **Using `OperationResult<T>` in platform code** — it is a core token (0× here). Use
   `DataProcessResult<T>`.
2. **Treating `Results.Ok` / `ProblemDetails` as the domain wrapper** — they are HTTP
   transport in the minimal-API host only. The domain returns `DataProcessResult<T>`.
3. **Reading FREEDOM values from raw `IConfiguration`/`appsettings.json`** — this repo
   has no appsettings; tunables come from `IFreedomConfigService.GetAsync(key)`.
4. **Re-implementing a core ML/graph capability locally** — it stays in `llm_mvp_core`;
   consume via manifest/locator.

## Integration

- `HOW-TO-USE-SKILLS.md` activation-trigger row "Mapping a core concept to its platform equivalent" loads this file.
- `freedom-machine-SKILL.md` and `dna-compliance-guard-SKILL.md` use the same `DataProcessResult<T>` + fabric-interface bindings recorded here.
- `iron-rule-derivation-SKILL.md` cites `DataProcessResult<T>` and `IFreedomConfigService` from this map.
