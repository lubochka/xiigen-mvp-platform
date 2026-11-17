# Skill: Implementation Documentation — UML Diagrams (§0 + §1)

> Ported from the `llm_mvp_core` planning standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API (minimal API, no MVC
> controllers)**, xUnit, Gate-B = `dotnet build` / `dotnet test`, domain result
> wrapper `DataProcessResult<T>` (`src/Xiigen.Kernel/DataProcessResult.cs`).
> This repository has **no Angular frontend** (despite the repo name); there are
> server-rendered HTML evidence surfaces in `Program.cs` (`/marketplace`,
> `/studio`, `/wiki`, `/assistant`, `/legal/*`) but no UI-only components. §0-§1
> therefore describe `request → handler → fabric/service → response` branches.

## Purpose

Every implementation documentation section (Part A) must begin with two UML
subsections:
- **§0 Class diagram** — technical reference for programmers (draw.io importable)
- **§1 Human-language flowcharts** — explain what happens in plain English unless the user requests another language, not
  method names

## §0 Class Diagram — required elements

1. **Mermaid `classDiagram` block** importable via draw.io Extras → Edit Diagram.
   PlantUML is also acceptable when it is importable to draw.io and carries the
   same class responsibilities, relationship legend, and simulations.
2. Every class/record/interface must have a `note for ClassName "..."`
   annotation **in English** (unless the user requests another language) explaining in 1-3 lines:
   - What this class does (not its method names)
   - Its status: FULLY IMPLEMENTED / PARTIAL / STUB / SKELETON
3. All relationship arrows must be present: inheritance (`--|>`), implementation
   (`<|..`), composition (`-->`), dependency (`..>`). For this platform that
   means the minimal-API endpoint delegate → domain service (e.g.
   `MarketplaceService`) → fabric store (`IDocumentStore` / `IOutboxStore` /
   `IIdempotencyStore`) → `DataProcessResult<T>`, plus
   `MicroserviceBase<TSelf>` inheritance and `ITenantContextAccessor` /
   `ICorrelationContextAccessor` dependencies.
4. **Legend table** immediately after the mermaid block with columns:
   Class | What it does | Status

**What the `note for` must say (example):**
```
note for MarketplaceService "FULLY IMPLEMENTED\nReads/publishes marketplace listings →\nwrites through IDocumentStore within the tenant scope →\nreturns DataProcessResult<T>"
```

5. **Per-connection simulation table (REQUIRED).** For EVERY relationship arrow
   (connection) in the diagram, give at least one example simulation: a concrete
   sample input that flows across that connection AND the expected result/output
   on that path. Use this table immediately after the legend:

| Connection (A → B) | Example input (simulation) | Expected result on this branch |
|---|---|---|
| `MapPost /api/marketplace/listings` → `MarketplaceService.PublishListingAsync` | `PublishMarketplaceListingRequest{ flowSlug="user-registration", version="1.0.0" }`, header `X-Tenant-Id: acme` | `DataProcessResult<...>.Success` → `Results.Ok(data)` (HTTP 200) |
| `MarketplaceService` → `IDocumentStore.UpsertAsync` | collection="marketplace-listings", id="user-registration:1.0.0" | `DataProcessResult<DocumentRecord>.Success`, record within the tenant scope |

A connection without an example simulation and expected result is an incomplete §0.

**What NOT to do:**
- Class diagram without `note for` annotations (programmer sees method names, not meaning)
- `note for` that just repeats the class name or lists methods
- Omitting the legend table
- Connection/arrow without an example simulation and expected result
- PlantUML/draw.io diagram that contains only boxes and arrows but no concrete
  data-flow example or expected branch outcome

## §1 Human-language flowcharts — required elements

1. Use `flowchart TD` (not `sequenceDiagram` for the primary human diagram)
2. Every decision node `{...}` must state the human question, not a method name:
   - ✅ `{Does the X-Roles header contain platform_admin / tenant_admin / tenant_ops?}`
   - ❌ `{RequireStudioRole(context) == null}`
3. Every action node must explain the business meaning:
   - ✅ `["🎯 Publish the marketplace listing\nThe record enters the tenant scope through IDocumentStore"]`
   - ❌ `["MarketplaceService.PublishListingAsync(request, ct)"]`
4. Label arrows with the condition in human language:
   - ✅ `-->|"YES\nRole is allowed"|`
   - ❌ `-->|true|`
5. Include emoji status indicators: ✅⚠️🔶 on skeleton/partial components
6. **Per-branch simulation (REQUIRED).** For EVERY decision branch (each outcome
   of each `{...}` node), give a concrete simulation input + the expected result
   for that branch. Example:
   `Input: request without the X-Roles header → NO branch → result: Results.Unauthorized() (HTTP 401)`;
   `Input: X-Roles=platform_admin, valid body → YES branch → result: Results.Ok(data) (HTTP 200)`.
   Every branch of every decision node must show one simulated input and its
   expected output (use the actual `request → handler → response` branches:
   `Results.Ok`, `Results.BadRequest`, `Results.NotFound`, `Results.Problem`,
   `Results.Forbid`, `Results.Unauthorized`).

**What NOT to do (branches):** a decision branch without a simulated example input and
expected result violates the §0/§1 contract (cross-reference FC-14 in
`plan-review-SKILL.md`).

**One flowchart per major decision path.** The request-validation / authorization
path (`RequireStudioRole`, tier policy, idempotency reservation) must have its own
flowchart showing all branches.

## Validation gate

Before submitting a UML section, verify:
- [ ] Class diagram has `note for` (English unless the user requests another language) on EVERY class/record/interface
- [ ] Legend table present after class diagram
- [ ] Flowcharts use human-language labels (no raw method names as primary labels)
- [ ] Decision nodes ask human questions
- [ ] All diagrams open in draw.io without parse errors (test by reading the mermaid syntax)
- [ ] PlantUML audit diagrams, when used, are importable to draw.io with the same simulation coverage
- [ ] Every connection (relationship arrow) in §0 has ≥1 example simulation + expected result (per-connection table present)
- [ ] Every flowchart branch in §1 has a simulation input + expected `Results.*` / `DataProcessResult<T>` result

## Integration

Load alongside: `implementation-doc-algorithms-SKILL.md`,
`implementation-doc-code-refs-SKILL.md`, `uml-simulation-to-testcase-SKILL.md`.
Required by: `planning-session-startup-SKILL.md` (pre-execution doc gate),
`plan-review-SKILL.md` (FC-14, per-connection simulation), and the platform
governance loop in `AGENTS.md` / `CLAUDE.md` (v3.3 Beacon; PLAN_AUTHOR_INTERNAL →
PLAN_REVIEW_INTERNAL → ARCHITECT_INTERNAL → EXECUTOR_INTERNAL).
