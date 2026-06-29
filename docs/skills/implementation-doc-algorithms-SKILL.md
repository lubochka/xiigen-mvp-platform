# Skill: Implementation Documentation — Algorithm Descriptions (§1 / §2)

> Ported from the `llm_mvp_core` planning standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build` / `dotnet test`, domain result wrapper `DataProcessResult<T>`
> (`src/Xiigen.Kernel/DataProcessResult.cs`). Domain services inherit
> `MicroserviceBase<TSelf>` (`src/Xiigen.Kernel/MicroserviceBase.cs`) and use its
> protected `Success<T>` / `Failure<T>` helpers — they do **not** use the
> `llm_mvp_core` `OperationResult<T>` (that type stays in core and never leaks here).

## Purpose

Every component in an implementation document must have a human-language
algorithm description (§1/§2) that explains HOW and WHY, not just WHAT. A
programmer must understand the component without reading the source code. Default
language is **Russian** unless Luba asks otherwise.

For each algorithm, name the **exact class AND the implementing method**, the
**parameters** it takes, and the **actual step-by-step algorithm**. A functional
spec ("what it should do") and a class name alone are NOT sufficient.

## Required structure for each component description

### 1. Human analogy first (mandatory)
Start with an analogy a non-programmer can understand:
- ✅ "Представь регистратора маркетплейса: он принимает заявку на публикацию,
  проверяет, что у заявителя есть нужная роль и арендатор (tenant), и только
  тогда заносит запись в журнал арендатора."
- ❌ "Этот класс вызывает `IDocumentStore.UpsertAsync` с optimistic concurrency."

### 2. What happens during initialization / setup
Concrete description of what state the component holds and how it changes. Name
the internal data structures and injected dependencies (the constructor
parameters from `MicroserviceBase<TSelf>`: `ServiceDescriptor`,
`ITenantContextAccessor`, `ICorrelationContextAccessor`, `ILogger<TService>`), and
the fabric stores it uses (`IDocumentStore` / `IOutboxStore` /
`IIdempotencyStore` / `ISchedulerService`).

### 3. What happens during a request (step-by-step)
Numbered list of what the component does on a request:
1. What comes in (with a **real example value**, not just a type name — e.g.
   `PublishMarketplaceListingRequest{ flowSlug="user-registration", version="1.0.0" }`)
2. What transformation/validation happens (tenant scope check, role check,
   idempotency reservation)
3. What decision is made and why (threshold, condition — e.g. "если
   `expectedVersion` не совпал → конфликт версий")
4. What comes out (with a **real example value** — e.g.
   `DataProcessResult<DocumentRecord>.Success(record)` или
   `DataProcessResult.Failure("VALIDATION_FAILED", "...")`)

### 4. "Что метод НЕ делает" section (mandatory)
Explicitly state what the component does NOT do:
- ❌ does not bypass the tenant scope (`CurrentTenant` is required)
- ❌ does not throw for business errors — it returns `DataProcessResult.Failure(...)`
- ❌ does not call another tenant's data

### 5. Parameter signatures with examples
After the text, add a `**Сигнатуры и типы данных:**` block:
- Show the main method signature (`Task<DataProcessResult<T>> ...Async(...)`)
- Show what the return type contains with real field values
- Show an example of the most important input type

**Example of correct parameter block:**
```csharp
// MarketplaceService (src/Xiigen.Platform.Marketplace/MarketplaceService.cs),
// наследует MicroserviceBase<MarketplaceService>, IMarketplaceService.
Task<DataProcessResult<IReadOnlyDictionary<string, object?>>> PublishListingAsync(
    PublishMarketplaceListingRequest request,   // flowSlug="user-registration", version="1.0.0"
    CancellationToken cancellationToken = default);

// Возвращаемое значение при успехе:
DataProcessResult<...>.Success(new Dictionary<string, object?>
{
    ["flowSlug"] = "user-registration",   // ключ листинга
    ["version"]  = "1.0.0",               // версия пакета
    ["status"]   = "published"            // состояние записи
});
// При ошибке валидации:
DataProcessResult<...>.Failure("VALIDATION_FAILED", "flowSlug is required");
```

## Status annotation

Every component description must end with its implementation status:
- **✅ Полная** — all public methods implemented and tested (xUnit green)
- **⚠️ Частичная** — state exactly which methods work and which are stubs
- **🔶 Скелет** — state what is saved (contract fields), what returns stub values, and WHY
- **✅ Hardcoded** — returns fixed values (e.g. a static registry/list)

## Validation gate

Before submitting §1/§2, verify each component description:
- [ ] Has a human analogy (not a technical summary)
- [ ] Names exact class + implementing method + parameters + step-by-step algorithm
- [ ] Shows concrete example values (not just type names)
- [ ] Has "Что НЕ делает" section
- [ ] Has a `**Сигнатуры и типы данных:**` block with real example values and `DataProcessResult<T>` (never `OperationResult<T>`)
- [ ] States implementation status

## Integration

Load alongside: `implementation-doc-uml-SKILL.md`,
`implementation-doc-code-refs-SKILL.md`.
Required by: `planning-session-startup-SKILL.md` (pre-execution doc gate),
`plan-review-SKILL.md` (FC-15).
