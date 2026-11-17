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
- ✅ "> **Why we check `X-Roles` before the request body:** the endpoint writes within
  the tenant scope; without an explicit role (`platform_admin` / `tenant_admin` / `tenant_ops`),
  we return 401/403 before reaching the domain service."
- ❌ `// role check`

### Layer 2 — HOW (mandatory when calling another component)
When a code block calls a function, explain what happens INSIDE that function:
- ✅ "> **What happens inside `IIdempotencyStore.TryReserveAsync`, step by step:**
  1. Build the operation key. 2. Try to reserve it for the TTL. 3. If the key already
  exists, return the cached response (`FromCache=true`)."
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
   // src/Xiigen.Platform.Api/Program.cs — wrap the result in an HTTP response:
   // result.IsSuccess=true  → Results.Ok(result.Data)         (HTTP 200)
   // result.IsFailure=true  → Results.BadRequest({ code, message }) (HTTP 400)
   return ToHttpResult(result);
   ```
3. When a condition blocks something, show ALL blocking conditions with reasons:
   ```csharp
   // Block listing publication when:
   // 1. the X-Roles header is missing            → Results.Unauthorized() (401)
   // 2. the role is not in {platform_admin,tenant_admin,tenant_ops} → Results.Forbid() (403)
   // 3. flowSlug/version are empty                   → DataProcessResult.Failure("VALIDATION_FAILED", ...)
   ```

## What NOT to do

- ❌ Code block + comment that just renames the call: `// the service publishes the listing`
- ❌ `// returns result` — what result? show it (`result.Data`, status code)
- ❌ Calling a function and explaining nothing about its non-obvious internals
- ❌ Threshold/constant/HTTP status without explaining why that value was chosen
- ❌ "Otherwise — an error" without explaining which `DataProcessError.Code` is returned and why

## Validation gate

For every code block in §3, verify:
- [ ] Has at least one WHY explanation (blockquote or comment)
- [ ] If it calls another function → internals explained or linked to §2
- [ ] All thresholds/constants/HTTP status codes have "why X" explanation
- [ ] All conditions show ALL branches with reasons (`Results.Ok` / `BadRequest` / `NotFound` / `Problem` / `Forbid` / `Unauthorized`)
- [ ] Example values shown in comments (not just type names)
- [ ] Citations are precise `path:line` reflecting code AS ACTUALLY IMPLEMENTED, not aspirational

## Integration

Load alongside: `implementation-doc-uml-SKILL.md`,
`implementation-doc-algorithms-SKILL.md`.
Required by: `planning-session-startup-SKILL.md` (pre-execution doc gate),
`plan-review-SKILL.md` (FC-16).
