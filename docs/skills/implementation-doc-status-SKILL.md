# Skill: Implementation Documentation — Implementation Status Table (§4)

> Ported from the `llm_mvp_core` planning standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**. One row per minimal-API
> endpoint module and domain service (`src/Xiigen.Platform.*`, `Xiigen.Fabrics.*`).
> Keep it in sync with the v3.3 completion matrix
> (`docs/plans/enterprise-adaptation/v3.3/XIIGEN-ENTERPRISE-ADAPTATION-COMPLETION-MATRIX-v3.3.md`).
> **Note:** the core `✅ Bootstrap` symbol (deterministic Confidence=1.0 wrapper
> awaiting a trained model) is core-specific (DPO / ILearningMachine) and is **not
> ported** here. For a static wrapper use `✅ Hardcoded`.

## Purpose

Every implementation document must end with a status table showing the current
implementation state of every component in the affected area. This is the single
source of truth for "what works, what doesn't" before any plan starts.

## Required table format

| Component | File | Status | What actually works now |
|---|---|---|---|

**Columns:**
- **Component** — class / interface / endpoint name with backtick formatting
- **File** — relative path from repo root, no absolute paths (e.g.
  `src/Xiigen.Platform.Marketplace/MarketplaceService.cs`)
- **Status** — one of the status symbols below
- **What actually works now** — concrete routes/methods and their state, not
  "see above", not "Partially implemented"

## Status symbols

| Symbol | Meaning | When to use |
|---|---|---|
| ✅ Complete | All public methods implemented, tested (xUnit green) | Nothing returns stub / NOT_READY |
| ⚠️ Partial | Core path works, some methods are stubs | Name working methods AND stub methods explicitly |
| 🔶 Skeleton | Contract-only — types/fields exist, inference always returns a constant stub | State what is SAVED and what ALWAYS returns stub, and WHY |
| ✅ Hardcoded | Returns fixed values — not trained, not configurable | Registries/factories/static lists; static wrappers |

## What the "What actually works now" column must contain

- For ✅ Complete: list key working routes/methods (e.g.
  "`GET /api/marketplace/listings`, `POST /api/marketplace/listings` →
  `MarketplaceService` writes through `IDocumentStore`, tenant scope, 200/400").
- For ⚠️ Partial: explicitly name working methods AND stub methods:
  - ✅ "`ListListingsAsync`, `PublishListingAsync` work; `ApplyAdaptationAsync` → `DataProcessResult.Failure(\"NOT_READY\", ...)`"
  - ❌ "Partially implemented" (no detail)
- For 🔶 Skeleton: state what is SAVED (preserved contract fields) and what ALWAYS
  returns stub:
  - ✅ "`AnswerAsync` preserves projectId/checksum; without a SQL source, it always returns `ABSTAIN_NO_SOURCE`"
  - ❌ "Not implemented"

## Example row

| Component | File | Status | What actually works now |
|---|---|---|---|
| `MarketplaceService` | `src/Xiigen.Platform.Marketplace/MarketplaceService.cs` | ✅ Complete | `ListListingsAsync` / `GetListingAsync` / `PublishListingAsync` / `RecordPackageReadbackAsync` / `CreateTenantForkAsync` through `IDocumentStore`, tenant scope; routes `/api/marketplace/*` return 200/400 |
| `InMemorySchedulerService` | `src/Xiigen.Fabrics.Scheduler/Scheduler.cs` | ✅ Hardcoded | In-memory job dictionary; `ScheduleDelayedAsync` (idempotent by key), `ListAsync`; not persisted across restarts |

## Validation gate

Before submitting §4:
- [ ] Every component mentioned in §1-§3 has a row
- [ ] No row uses a status symbol not in the allowed list (no `Bootstrap` here)
- [ ] "What actually works now" contains specific route/method names or return values, not prose
- [ ] Partial and skeleton components explicitly list which methods work and which don't
- [ ] Paths are relative, never absolute

## Integration

Required by: `planning-session-startup-SKILL.md` (pre-execution doc gate),
`plan-review-SKILL.md` (FC-17).
