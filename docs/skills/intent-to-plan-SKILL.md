# Intent to Plan

> Ported from the `llm_mvp_core` planning standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build` / `dotnet test`. Framework/stack names (".NET 10", "minimal API",
> "ASP.NET Core") are **context qualifiers**, not clauses, and never belong in a
> REFERENCE-PLAN phase title.

## What This Skill Prevents

1. **Technology names in plan steps.** When planning "add a marketplace publish
   endpoint", the natural default is to write "create `MarketplaceService.PublishListingAsync`
   using `IDocumentStore.UpsertAsync` in C#." That is a technology-scoped step,
   not a plain-language step. The REFERENCE-PLAN must be infrastructure-neutral;
   class/method names belong in the EXECUTION work order (Part B).
2. **Scope drift from sentence to implementation.** The INTENT field must be the
   user's request **verbatim**. Every paraphrase changes scope.
3. **Steps that combine responsibilities.** A step containing "and" usually
   contains two steps. Combined responsibilities make Gate B ambiguous — a step
   covering two things has no single measurable Gate B.

## When to Invoke

Load before writing any REFERENCE-PLAN / EXECUTION phase descriptions. Use
alongside `phase-planning-SKILL.md` — this skill governs WHAT to put in each plan
step; `phase-planning-SKILL.md` governs the gate validation.

## Section 1 — Parsing the User's Request

An **intent clause** is one distinct capability the user is requesting. Parse the
user sentence to identify all intent clauses before writing anything.

| Signal | What it means |
|--------|--------------|
| Every noun phrase naming a capability | One clause |
| Technology qualifiers ("in C#", "as a minimal-API endpoint", "with xUnit") | Context — not a clause |
| Multiple verbs describing distinct actions | One clause per verb |
| "and" connecting two capabilities | Two clauses |
| Ambiguous phrase that could mean two things | Flag as ambiguous — ask |

### Clause table (fill before writing the plan)

| # | Clause | Source phrase | Ambiguous? | Question if ambiguous |
|---|--------|---------------|-----------|----------------------|
| 1 | | | | |

State the phase-count reasoning explicitly. Do not use default ranges without
justification.

## Section 2 — What a Valid Plan Phase Looks Like

### Property 1 — PLAIN LANGUAGE
The phase names a capability, not a technology implementation.
```
Good: "Add the ability to publish a flow package to the marketplace"
Bad:  "Add MarketplaceService.PublishListingAsync that calls IDocumentStore.UpsertAsync"
      — contains class and method names

Good: "Expose tenant-scoped marketplace listings to authorized callers"
Bad:  "Add app.MapGet('/api/marketplace/listings', ...) returning Results.Ok"
      — contains route and Results.* detail
```
The technology-level detail belongs in the EXECUTION file (Part B), not the
REFERENCE-PLAN.

### Property 2 — SINGLE RESPONSIBILITY
One phase = one distinct capability. "And", "also", "as well as" in the same
phase description is a candidate split point.
```
Good: "Define the marketplace publish capability"
Bad:  "Define the marketplace publish capability and wire its DI registration"
      — two capabilities

Split rule: if you can draw a line between two parts and each part makes sense
            alone — split the phase.
```

### Property 3 — GATE B DECLARED
Every phase has a measurable Gate-B condition. A phase without Gate B is not a
valid plan phase.
```
Good: "Gate B: dotnet test --filter Marketplace → 5+ tests pass, 0 fail"
Good: "Gate B: dotnet build src/Xiigen.Platform.Api → 0 Error"
Bad:  "Gate B: marketplace publishing works correctly"
      — "works correctly" is not measurable

Gate B format: [dotnet command] → [exact expected value or comparison]
```

## Section 3 — Plan Reviewer Protocol

After writing a plan, apply these checks to each phase.

### Check 1 — COVERAGE
For each intent clause: does at least one phase address it as its primary
subject? Verdict per clause: COVERED | PARTIAL | MISSING.

### Check 2 — ABSTRACTION
For each phase: does it contain a class name, method signature, or C# keyword?
```
Technology list for this plan (derive per plan):
  - Class/interface names: MarketplaceService, IDocumentStore, IMarketplaceService
  - Method/route names: PublishListingAsync(), MapPost(), AddXiigenInMemoryFabrics()
  - C# keywords: interface, class, async, Task<DataProcessResult<T>>
Verdict: CLEAN | VIOLATION
Plan fails abstraction if ANY phase has a VIOLATION in the REFERENCE-PLAN.
```

### Check 3 — SINGLE RESPONSIBILITY
Signal words: "and", "also", "as well as" → flag as compound. Plan fails if more
than 20% of phases are compound.

### Check 4 — GATE B COMPLETENESS
For each phase: is there exactly one Gate B with a measurable `dotnet` command +
expected output? Verdict: PRESENT | MISSING | VAGUE ("works correctly", "tests
pass" → must be replaced with exact command + expected output).

## Anti-Patterns
1. **Technology names in REFERENCE-PLAN** — REFERENCE-PLAN describes capabilities; EXECUTION (Part B) describes implementation with class/method/route names.
2. **100% first-run coverage** — suspicious; check whether the plan addresses the user's actual clauses, not a standard pattern.
3. **"While I'm here" phases** — adding phases not requested by the user.
4. **Vague Gate B** — "tests pass" is not Gate B. "dotnet test --filter Marketplace → 5+ pass, 0 fail" is Gate B.

## Integration
Use with `phase-planning-SKILL.md` (gate validation), `plan-self-validation-SKILL.md`
(Phase 0), and the implementation-doc skills for Part A depth.
