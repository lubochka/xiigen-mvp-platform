# UML Simulation To Test Case Skill

> Ported from the `llm_mvp_core` planning standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit. `test_layer`
> map: `unit` / `integration` = xUnit on domain services and fabrics;
> `e2e_host` = HTTP endpoint test (this repo has **no UI**, so the "host" e2e
> layer is an in-process HTTP scenario via `WebApplicationFactory<Program>` —
> `Program.cs` exposes `public partial class Program {}` for exactly this);
> `qa_automation` / `negative` as in core. `responsible_class` / `responsible_method`
> are real C# in `src/Xiigen.Platform.*` / `src/Xiigen.Kernel/*` / `src/Xiigen.Fabrics.*`.

## Purpose

Use this skill for Luba-readable implementation plans (Part A) that include UML,
data flow, algorithm branches, or request→handler→response diagrams.

The goal is to turn every diagram connection and algorithm branch into a testable
xUnit scenario, and to prove Part A ↔ Part B parity (every Part A branch maps to a
Part B work order/test, and back).

## Required Per-Connection Row

For every UML arrow (e.g. `MapPost` delegate → `MarketplaceService.PublishListingAsync`
→ `IDocumentStore.UpsertAsync`):

```text
uml_connection_case:
  connection_id:
  from_component:
  to_component:
  data_sent:
  example_input:                 # real value, e.g. PublishMarketplaceListingRequest{...}
  expected_output:               # Results.Ok / DataProcessResult<T>.Success(...) / HTTP code
  failure_case:                  # e.g. DataProcessResult.Failure("VALIDATION_FAILED", ...) → 400
  responsible_class:             # e.g. MarketplaceService
  responsible_method:            # e.g. PublishListingAsync
  test_case_id:                  # e.g. MarketplaceService_PublishListing_PersistsInTenantScope
  test_layer: unit | integration | e2e_host | qa_automation | negative
  evidence_path:                 # e.g. tests/Xiigen.Platform.Marketplace.Tests/MarketplaceServiceTests.cs
```

## Required Per-Branch Row

For every algorithm/decision branch (each `request → handler → response` outcome):

```text
algorithm_branch_case:
  branch_id:
  human_condition:               # e.g. "The X-Roles header is missing"
  input_example:
  expected_result:               # e.g. Results.Unauthorized() (401)
  not_expected_result:           # e.g. Results.Ok (must NOT reach the service)
  responsible_class:
  responsible_method:
  test_case_id:
```

## Gate

- A diagram with boxes and arrows but no simulation table is incomplete.
- A branch with no test row is incomplete.
- Part A and Part B must match: every Part A row maps to a Part B work order or
  xUnit test, and every Part B test maps back to a Part A branch.
- `e2e_host` here is an in-process HTTP test (`WebApplicationFactory<Program>` with
  headers `X-Tenant-Id` / `X-Roles`), since the platform has no browser UI; it
  exercises the real minimal-API route, not a synthetic helper.

## Integration

Load alongside: `implementation-doc-uml-SKILL.md` (the §0/§1 diagrams this skill
converts), `plan-review-SKILL.md` (FC-14 per-connection simulation, FC-19 Part A↔B
parity, FC-26 test strategy).
