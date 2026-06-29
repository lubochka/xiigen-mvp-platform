# Naming Conventions Enforcer (XIIGen Enterprise Platform)

## Overview

`src/Xiigen.Platform.Marketplace/Service.cs` tells a reader nothing.
`src/Xiigen.Platform.Marketplace/SmartWikiService.cs` tells them exactly what
capability lives there. This skill enforces naming standards so files, classes,
interfaces, tests, state, and commits describe capability — not a sequence number.

Stack-bound to **.NET 10 ASP.NET Core Web API**, **xUnit** (`tests/`),
`DataProcessResult<T>`, and the platform fabric interfaces. The platform's
projects are already named by capability —
`Xiigen.Kernel`, `Xiigen.Auth`, `Xiigen.Freedom`, `Xiigen.Platform.Api`,
`Xiigen.Platform.Marketplace`, `Xiigen.Fabrics.InMemory`,
`Xiigen.Fabrics.Scheduler` — and this skill keeps file/class/test names equally
descriptive.

This skill covers:
1. C# file and class names
2. Interface and implementation names
3. Test class and method names
4. STATE / session entries
5. Commit messages

---

## Rule 1 — File names describe capability, not a number

**NEVER:**
```
src/Xiigen.Platform.Marketplace/Service.cs
src/Xiigen.Kernel/Helper.cs
src/Xiigen.Fabrics.Scheduler/Module.cs
```

**ALWAYS:**
```
src/Xiigen.Platform.Marketplace/SmartWikiService.cs
src/Xiigen.Kernel/TenantContext.cs
src/Xiigen.Fabrics.Scheduler/Scheduler.cs
```

**Rule:** the capability/area lives in the **project** name
(`Xiigen.Platform.Marketplace`); the file name names the DOMAIN CAPABILITY, never
echoes a number.

---

## Rule 2 — Interfaces follow the I-prefix convention

**NEVER:**
```csharp
interface DocumentStore     // missing I-prefix
interface Sched             // truncated
```

**ALWAYS (matches the real contracts):**
```csharp
interface IDocumentStore        // src/Xiigen.Kernel/FabricContracts.cs
interface IIdempotencyStore     // src/Xiigen.Kernel/FabricContracts.cs
interface IOutboxStore          // src/Xiigen.Kernel/FabricContracts.cs
interface ITenantContextAccessor // src/Xiigen.Kernel/TenantContext.cs
interface ISchedulerService     // src/Xiigen.Fabrics.Scheduler/Scheduler.cs
```

**Rule:** interfaces start with `I` + a capitalized noun phrase naming the
RESPONSIBILITY, not the implementing technology. First use of any abbreviation in
a comment includes its expansion (e.g. "HMAC (Hash-based Message Authentication
Code)").

> Apply the `I`-prefix to injectable service / port / fabric interfaces. Plain
> data-shape records (`DocumentRecord`, `ScheduledJobResult`, `TenantContext`,
> request/response DTOs) are records, not `I…` ports — do not force an `I`
> prefix on them.

---

## Rule 3 — Implementation names match their interface

For `IThingDoer`, the implementation is `ThingDoer` or `[Qualifier]ThingDoer` —
never an unrelated name.

```csharp
// ✅ (real)
interface IDocumentStore     → InMemoryDocumentStore
interface ISchedulerService  → InMemorySchedulerService
interface ITenantContextAccessor → AsyncLocalTenantContextAccessor

// ❌
interface IDocumentStore     → StorageProvider   // unrecognizable as the impl
```

---

## Rule 4 — Test class names include the class under test

**NEVER:**
```csharp
class TestSuite1
class MarketplaceTests   // which class?
```

**ALWAYS:**
```csharp
class SmartWikiServiceTests                  // one class under test
class InMemorySchedulerServiceTests          // one class under test
class MarketplaceServiceIntegrationTests     // integration tests for MarketplaceService
```

**Rule:** test class = `[TestedClass]Tests` or `[TestedClass]IntegrationTests`,
in `tests/Xiigen.*.Tests`. Test methods (xUnit `[Fact]` / `[Theory]`):
`[Method]_[Scenario]_[ExpectedResult]`, e.g.
`ScheduleDelayedAsync_EmptyAction_ReturnsFailure`.

---

## Rule 5 — STATE / session entries include a human-readable name

```json
// ❌ machine-only
{ "phase": "P5", "status": "DONE" }

// ✅ machine + human readable
{ "phase": "P5", "phaseName": "Smart Wiki indexing through IDocumentStore", "status": "DONE" }
```

Every STATE entry includes a `phaseName` alongside the phase number (matches the
`SESSION-XX-*` / completion-matrix convention in `AGENTS.md`).

---

## Rule 6 — Commit messages name the phase and files

**NEVER:**
```
fix: update stuff
phase 6 done
```

**ALWAYS (per phase-git-report-SKILL.md, and the existing `P{n}:` style):**
```
phase 6: Smart Wiki indexing — src/Xiigen.Platform.Marketplace/SmartWikiService.cs, tests/Xiigen.Platform.Marketplace.Tests/SmartWikiServiceTests.cs
```

Format: `phase N: short capability description — comma-separated file list`.

---

## Enforcement Gates

### At code-writing time
```bash
# Interface names follow I-prefix (injectable ports only)
grep -rn "public interface [^I]" src/ --include="*.cs"
# Review: each hit should either be an I-prefixed port or a plain data record (not an interface)

# Test classes name the class under test
grep -rn "public class .*Tests$" tests/ --include="*.cs" | head -20
# Each match should follow [ClassName]Tests / [ClassName]IntegrationTests
```

### At commit time
```bash
# File names are descriptive, not generic
grep -rln "Helper\.cs\|Processor\.cs\|Module\.cs\|Manager\.cs" src/ --include="*.cs"
# Review: "SmartWikiService" is fine; bare "Service"/"Helper"/"Module" is not
```

---

## Naming Lint Quick Reference

| Artifact type | Pattern | Example |
|---|---|---|
| Injectable interface | `I[CapabilityNoun]` | `IDocumentStore`, `ISchedulerService` |
| Data record / DTO | `[CapabilityNoun]` (no `I`) | `DocumentRecord`, `ScheduledJobResult` |
| Implementation | `[Qualifier][NounFromInterface]` | `InMemoryDocumentStore`, `AsyncLocalTenantContextAccessor` |
| Test class | `[ClassName]Tests` / `[ClassName]IntegrationTests` | `SmartWikiServiceTests` |
| Test method | `[Method]_[Scenario]_[Expected]` | `ScheduleDelayedAsync_EmptyAction_ReturnsFailure` |
| Source file | `[CapabilityNoun].cs` | `SmartWikiService.cs` |
| State/session | `phaseName` with full description | see Rule 5 |
| Commit | `phase N: description — files` | see Rule 6 |
