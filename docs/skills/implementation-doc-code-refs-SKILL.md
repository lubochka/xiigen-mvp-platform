# Skill: Implementation Documentation — Code References with Explanations (§3)

> Ported from the `llm_mvp_core` planning standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API (minimal API)**. Cite real
> C# at `path:line` for minimal-API delegates in
> `src/Xiigen.Platform.Api/Program.cs` (`app.MapGet` / `app.MapPost` →
> `Results.Ok` / `Results.BadRequest` / `Results.Problem`) and domain services in
> `src/Xiigen.Platform.Marketplace/*Service.cs`. Domain return type is
> `DataProcessResult<T>` (`src/Xiigen.Kernel/DataProcessResult.cs`), never core
> `OperationResult<T>`.

## Purpose

Code reference sections (§3) must explain WHY and HOW, not just WHAT. A code block
that only restates what the syntax says is useless documentation.

## Three layers every code block must have

### Layer 1 — WHY (mandatory, most important)
Explain why this condition, threshold, or decision exists:
- ✅ "> **Почему проверяем `X-Roles` до тела запроса:** конечная точка пишет в
  tenant-scope; без явной роли (`platform_admin` / `tenant_admin` / `tenant_ops`)
  возвращаем 401/403 раньше, чем дойдём до доменного сервиса."
- ❌ `// role check`

### Layer 2 — HOW (mandatory when calling another component)
When a code block calls a function, explain what happens INSIDE that function:
- ✅ "> **Что происходит внутри `IIdempotencyStore.TryReserveAsync` пошагово:**
  1. строит ключ операции 2. пытается зарезервировать на TTL 3. если ключ уже
  существует — возвращает кешированный ответ (`FromCache=true`)."
- ❌ `// reserve idempotency`

### Layer 3 — WHAT (secondary, the code shows this)
Comments inside code blocks should focus on values and outcomes:
- ✅ `// result.IsSuccess=true, result.Data.JobId="job-123" → Results.Accepted(...)`
- ❌ `// checks result`

## Format rules

1. Use `>` blockquote blocks for WHY/HOW explanations — they visually separate
   explanation from code.
2. Inside code blocks: show concrete example values in comments:
   ```csharp
   // src/Xiigen.Platform.Api/Program.cs — обёртка результата в HTTP-ответ:
   // result.IsSuccess=true  → Results.Ok(result.Data)         (HTTP 200)
   // result.IsFailure=true  → Results.BadRequest({ code, message }) (HTTP 400)
   return ToHttpResult(result);
   ```
3. When a condition blocks something, show ALL blocking conditions with reasons:
   ```csharp
   // Блокировка публикации листинга когда:
   // 1. отсутствует заголовок X-Roles            → Results.Unauthorized() (401)
   // 2. роль не в {platform_admin,tenant_admin,tenant_ops} → Results.Forbid() (403)
   // 3. flowSlug/version пустые                   → DataProcessResult.Failure("VALIDATION_FAILED", ...)
   ```

## What NOT to do

- ❌ Code block + comment that just renames the call: `// сервис публикует листинг`
- ❌ `// returns result` — what result? show it (`result.Data`, status code)
- ❌ Calling a function and explaining nothing about its non-obvious internals
- ❌ Threshold/constant/HTTP status without explaining why that value was chosen
- ❌ "Иначе — ошибка" without explaining which `DataProcessError.Code` is returned and why

## Validation gate

For every code block in §3, verify:
- [ ] Has at least one WHY explanation (blockquote or comment)
- [ ] If it calls another function → internals explained or linked to §2
- [ ] All thresholds/constants/HTTP status codes have "почему X" explanation
- [ ] All conditions show ALL branches with reasons (`Results.Ok` / `BadRequest` / `NotFound` / `Problem` / `Forbid` / `Unauthorized`)
- [ ] Example values shown in comments (not just type names)
- [ ] Citations are precise `path:line` reflecting code AS ACTUALLY IMPLEMENTED, not aspirational

## Integration

Load alongside: `implementation-doc-uml-SKILL.md`,
`implementation-doc-algorithms-SKILL.md`.
Required by: `planning-session-startup-SKILL.md` (pre-execution doc gate),
`plan-review-SKILL.md` (FC-16).
