# Shared Service Design (XIIGen Enterprise Platform)

## Core Problem

Building two implementations of the same capability — one inside
`Xiigen.Platform.Marketplace` and another inside a future feature project — that
diverge over time and require double maintenance. The same capability written
twice is the most expensive class of architectural mistake in a multi-project
system. This skill defines when a capability is **shared infrastructure**
(requiring a fabric `I*` contract) vs per-project, and how to build the Shared
Services Map before any multi-project design session begins.

Platform port of the core `shared-service-design` skill, re-bound to
**.NET 10 ASP.NET Core Web API**, **xUnit**, `DataProcessResult<T>`, and the real
`Xiigen.Kernel`/`Xiigen.Fabrics.*` contracts. Gate-B is
`dotnet build Xiigen.Platform.slnx` + `dotnet test`.

## When to load

- Before any multi-project planning session.
- When a capability name appears in 2+ project plans.
- Before `service-boundary-design-SKILL.md` when the boundary spans projects.
- Before adding a second concrete implementation of an existing capability.

---

## Section 1 — Count Service Appearances Across Projects

For each capability mentioned in any project plan, count how many projects
reference it:

```
IDocumentStore:            ████████  used by every read/write service   ← CRITICAL: shared infrastructure
ITenantContextAccessor:    ████████  every MicroserviceBase<T> service   ← shared infrastructure
ICorrelationContextAccessor:███████  every service for tracing           ← shared infrastructure
IOutboxStore:              █████     write-side services that dispatch    ← shared infrastructure
ISchedulerService:         ████      services needing delayed work        ← shared
IIdempotencyStore:         ████      repeatable operations                ← shared
IFreedomConfigService:     ████      tunable algorithms                   ← shared
<feature-specific scorer>: █         one feature                          ← per-project
```

**Classification threshold:**
- Appears in ≥ 3 projects → SHARED INFRASTRUCTURE: fabric `I*` contract required,
  declared in `Xiigen.Kernel`, registered once in `Xiigen.Platform.Api/Program.cs`.
- Appears in 2 projects → REVIEW: may be shared depending on usage type.
- Appears in 1 project → PER-PROJECT: build inside that project.

Build the contract **before the first consumer**, never after.

---

## Section 2 — Classify Each Shared Service

**Pattern A — Shared Write + Shared Read (most complex).**
Multiple projects write AND read.
Example: `IDocumentStore` — services upsert documents and search them.
Resolution: fabric `I*` contract in `Xiigen.Kernel` with write AND read methods;
implementation in `Xiigen.Fabrics.InMemory`; registered once in DI before the
first writer.

**Pattern B — Shared Read Only.**
One project builds it; later projects only read.
Example: `IFreedomConfigService` — config values are produced/seeded once and read
by every tunable algorithm.
Resolution: build the contract once; document the read interface as a
cross-project dependency; later projects use the existing interface — never
rebuild it.

**Pattern C — Shared Notification / Event Delivery.**
Multiple projects publish events; the platform delivers them exactly-once.
Example: `IOutboxStore` — write-side services `AppendAsync` a `CloudEventEnvelope`;
the outbox is the single delivery transport (Rule 8/9). `ICorrelationContextAccessor`
supplies the correlation id carried on every envelope.
Resolution: one `IOutboxStore` contract in `Xiigen.Kernel`; each project appends
its own event types. Never build a per-project event bus.

---

## Section 3 — Order the Build Sequence

Shared infrastructure must be built (or at least stubbed as fabric contracts)
BEFORE the first project that uses it:

```
Pre-feature (e.g. Smart Wiki) infrastructure needed:
  IDocumentStore            ← persist the wiki graph (Pattern A)
  ITenantContextAccessor    ← scope every read/write to tenant
  ICorrelationContextAccessor ← correlation id on every event
  IOutboxStore              ← dispatch CapabilityIndexed event (Pattern C)
  IIdempotencyStore         ← dedup repeated index requests

Pre-scheduling feature infrastructure needed:
  ISchedulerService         ← ScheduleDelayedAsync for follow-up work
  IFreedomConfigService     ← thresholds/weights for the scheduled computation
```

---

## Section 4 — Shared Services Map Format

Before any multi-project design session, produce:

```markdown
## Shared Services Map

| Service | Appearances | Pattern | Contract in | Implementation in | Registered in |
|---|---|---|---|---|---|
| IDocumentStore | every service | A: write+read | Xiigen.Kernel/FabricContracts.cs | Xiigen.Fabrics.InMemory | Xiigen.Platform.Api/Program.cs |
| IOutboxStore | write-side | C: event delivery | Xiigen.Kernel/FabricContracts.cs | Xiigen.Fabrics.InMemory | Xiigen.Platform.Api/Program.cs |
| IIdempotencyStore | repeatable ops | A: reserve+mark | Xiigen.Kernel/FabricContracts.cs | Xiigen.Fabrics.InMemory | Xiigen.Platform.Api/Program.cs |
| ISchedulerService | delayed work | C: schedule | Xiigen.Fabrics.Scheduler/Scheduler.cs | Xiigen.Fabrics.Scheduler | Xiigen.Platform.Api/Program.cs |
| IFreedomConfigService | tunables | B: shared read | Xiigen.Freedom/FreedomConfig.cs | Xiigen.Freedom | Xiigen.Platform.Api/Program.cs |
| ITenantContextAccessor | every service | A: read+push | Xiigen.Kernel/TenantContext.cs | Xiigen.Kernel | Xiigen.Platform.Api/Program.cs |
| ICorrelationContextAccessor | every service | A: read+push | Xiigen.Kernel/CorrelationContext.cs | Xiigen.Kernel | Xiigen.Platform.Api/Program.cs |

## Per-Project Services (build inside the project, share only read)

| Service | Project | Not shared because |
|---|---|---|
| <feature scorer> | Xiigen.Platform.Marketplace | Only that feature computes it; others read results |
```

This map already exists in living form: the platform's fabric contracts are the
shared services. The skill formalizes the map and the "build the contract before
the first consumer" rule.

---

## Test Strategy (per branch)

- **Unit (xUnit):** each shared service's contract tested against its
  `Xiigen.Fabrics.InMemory` implementation; assert real `Data` fields.
- **Integration:** two consumers exercise the same registered singleton and observe
  consistent state (proves it is genuinely shared, not duplicated).
- **Negative:** an idempotent operation re-run with the same key returns the cached
  result (`IIdempotencyStore` / scheduler `fromCache: true`), proving Pattern A/C
  dedup.
- **Gate-B:** `dotnet build Xiigen.Platform.slnx` (0/0) + `dotnet test` (0 failed).

---

## Anti-Patterns

1. **"Build DocumentWriter in feature A and DocumentStore in feature B — same
   domain, different names"** — one `IDocumentStore` contract covers both.
2. **"Build an event publisher per feature"** — both need delivery. Use the single
   `IOutboxStore` + `CloudEventEnvelope`.
3. **"Discover in feature B that `IDocumentStore` needs a new method, forcing
   retroactive changes to feature A"** — the canonical cross-cutting mistake. The
   Shared Services Map declares the full contract before any consumer builds
   against it; an interface change is then a `blast-radius-SKILL.md` CRITICAL.
4. **"The contract can be built when the first feature needs it"** — too late.
   Build the stub `I*` + DI registration before the first dependent project starts.
5. **Putting the shared implementation in a feature project** — contracts live in
   `Xiigen.Kernel`, implementations in `Xiigen.Fabrics.*`.

---

## Integration

- `service-boundary-design-SKILL.md` uses this classification to decide whether a
  boundary is an assembly boundary or a namespace boundary.
- AGENTS.md / CLAUDE.md governance + `Xiigen.Kernel/FabricContracts.cs` are the
  persistence layer for the Shared Services Map.
- `change-propagation-SKILL.md` keeps the map in sync when a shared contract
  changes.
