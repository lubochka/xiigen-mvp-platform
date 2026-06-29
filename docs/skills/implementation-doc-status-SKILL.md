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

| Компонент | Файл | Статус | Что реально работает сейчас |
|---|---|---|---|

**Columns:**
- **Компонент** — class / interface / endpoint name with backtick formatting
- **Файл** — relative path from repo root, no absolute paths (e.g.
  `src/Xiigen.Platform.Marketplace/MarketplaceService.cs`)
- **Статус** — one of the status symbols below
- **Что реально работает сейчас** — concrete routes/methods and their state, not
  "see above", not "Частично реализована"

## Status symbols

| Символ | Значение | Когда использовать |
|---|---|---|
| ✅ Полная | All public methods implemented, tested (xUnit green) | Nothing returns stub / NOT_READY |
| ⚠️ Частичная | Core path works, some methods are stubs | Name working methods AND stub methods explicitly |
| 🔶 Скелет | Contract-only — types/fields exist, inference always returns a constant stub | State what is SAVED and what ALWAYS returns stub, and WHY |
| ✅ Hardcoded | Returns fixed values — not trained, not configurable | Registries/factories/static lists; static wrappers |

## What the "Что реально работает сейчас" column must contain

- For ✅ Полная: list key working routes/methods (e.g.
  "`GET /api/marketplace/listings`, `POST /api/marketplace/listings` →
  `MarketplaceService` пишет через `IDocumentStore`, tenant-scope, 200/400").
- For ⚠️ Частичная: explicitly name working methods AND stub methods:
  - ✅ "`ListListingsAsync`, `PublishListingAsync` рабочие; `ApplyAdaptationAsync` → `DataProcessResult.Failure(\"NOT_READY\", ...)`"
  - ❌ "Частично реализована" (no detail)
- For 🔶 Скелет: state what is SAVED (preserved contract fields) and what ALWAYS
  returns stub:
  - ✅ "`AnswerAsync` сохраняет projectId/checksum; при отсутствии SQL-источника всегда `ABSTAIN_NO_SOURCE`"
  - ❌ "Не реализована"

## Example row

| Компонент | Файл | Статус | Что реально работает сейчас |
|---|---|---|---|
| `MarketplaceService` | `src/Xiigen.Platform.Marketplace/MarketplaceService.cs` | ✅ Полная | `ListListingsAsync` / `GetListingAsync` / `PublishListingAsync` / `RecordPackageReadbackAsync` / `CreateTenantForkAsync` через `IDocumentStore`, tenant-scope; маршруты `/api/marketplace/*` возвращают 200/400 |
| `InMemorySchedulerService` | `src/Xiigen.Fabrics.Scheduler/Scheduler.cs` | ✅ Hardcoded | In-memory словарь заданий; `ScheduleDelayedAsync` (idempotent по ключу), `ListAsync`; не персистится между перезапусками |

## Validation gate

Before submitting §4:
- [ ] Every component mentioned in §1-§3 has a row
- [ ] No row uses a status symbol not in the allowed list (no `Bootstrap` here)
- [ ] "Что реально работает сейчас" contains specific route/method names or return values, not prose
- [ ] Partial and skeleton components explicitly list which methods work and which don't
- [ ] Paths are relative, never absolute

## Integration

Required by: `planning-session-startup-SKILL.md` (pre-execution doc gate),
`plan-review-SKILL.md` (FC-17).
