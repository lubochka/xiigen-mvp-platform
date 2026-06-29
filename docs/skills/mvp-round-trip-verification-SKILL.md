# Round-Trip Verification (XIIGen Enterprise Platform)

## Overview

A feature that passes unit tests but cannot be observed end-to-end has not been
delivered. Unit tests verify internal correctness; they do not verify that a
capability flows from its author through every integration point to a consumer who
can observe a result. This skill installs the full 8-step round-trip that closes
the gap between "tests pass" and "feature delivered".

This is the platform port of the `llm_mvp_core` skill of the same name. The
universal content is: the 8 steps, "minimum delivery = Steps 1-6", and the
partial-delivery protocol ("component delivered, pipeline integration pending").
It is bound to **.NET 10 / xUnit**, `DataProcessResult<T>`, DI registration in
`Program.cs`, and the platform projects.

> **`na` here — no frontend.** On this platform **all observable outputs are at
> the API / test level** (`dotnet build`, `dotnet test`, a
> `WebApplicationFactory<Program>` request). There is no UI observation. The
> core's UI observation of Steps 6/8 is **`na`**; it is closed by an API/test
> assertion instead. Step 8 (result observable end-to-end) is an e2e API test
> against `Xiigen.Platform.Api` / `Xiigen.Auth`, not a screen.

## When to invoke

- Before declaring any phase complete that introduces a new public capability.
- Before writing a handoff that claims a feature is delivered.
- When `phase-completion-packager-SKILL.md` asks for delivery evidence.
- After any integration point is wired (new interface implementation, new DI
  registration, new endpoint).

One round-trip check before stopping = no "works in unit test, broken in the
pipeline" claim.

## Section 1 — The 8-step round-trip

Each step has a responsible actor, an observable output, and a verification
command. **All outputs are API/test-level here.**

| Step | Name | Actor | Observable output | Verification |
|------|------|-------|------------------|--------------|
| 1 | **Capability authored** | Developer | Class + interface file on disk | `ls src/<Project>/<ClassName>.cs` |
| 2 | **Visible in project** | .NET build | Assembly references resolve | `dotnet build src/<Project>/<Project>.csproj -> 0 Error(s)` |
| 3 | **Registered** | DI wiring | Service registration configured | `grep -n "AddSingleton\|AddScoped\|AddTransient" src/<Project>/Program.cs` (or the `ServiceCollectionExtensions`) |
| 4 | **Discoverable by consumer** | Project reference | Consumer project resolves the type | `dotnet build src/<ConsumerProject>/<ConsumerProject>.csproj -> 0 Error(s)` |
| 5 | **Callable** | Test fixture | Test can instantiate and call | `dotnet test --filter "FullyQualifiedName~<TestClass>" -> >=1 passed` |
| 6 | **Result visible** | Test assertion | Return value observable in output | `dotnet test --filter "FullyQualifiedName~<SpecificTest>" -> PASSED` (assert `result.Data` / `result.Error!.Code`) |
| 7 | **Consumer runs to completion** | Pipeline test | No unhandled `DataProcessResult.Failure` | `dotnet test --filter "FullyQualifiedName~<PipelineTest>" -> passed, 0 failed` |
| 8 | **Result observable end-to-end** | e2e API test | Specific output matches expected | `dotnet test --filter "FullyQualifiedName~<E2ETest>" -> PASSED` (e.g. `WebApplicationFactory<Program>` request -> response asserted) |

**Minimum delivery evidence: Steps 1-6 must all pass.** Steps 7-8 are required for
pipeline-level / endpoint-level delivery claims.

> Step 6/8 UI observation from the core skill is **`na`**: assert the
> `DataProcessResult` value (Step 6) and the HTTP response (Step 8) instead of a
> screen.

## Section 2 — Nomination format

```markdown
## Round-Trip Nomination

Capability: [ClassName implementing IInterfaceName]
Phase: [N] — [phase name]
Delivery claim: [one sentence — what the consumer can now do]

Step 1 — Authored:
  Command: ls src/Xiigen.Fabrics.InMemory/InMemoryDocumentStore.cs
  Expected: file exists
  Actual: [result]

Step 2 — Visible in project:
  Command: dotnet build src/Xiigen.Fabrics.InMemory/Xiigen.Fabrics.InMemory.csproj
  Expected: 0 Error(s)
  Actual: [result]

Step 3 — Registered:
  Command: grep -n "AddSingleton" src/Xiigen.Platform.Api/Program.cs
           OR grep -n "IDocumentStore" src/Xiigen.Platform.Marketplace/XiigenMarketplaceServiceCollectionExtensions.cs
  Expected: >=1 match
  Actual: [result]

Step 4 — Discoverable:
  Command: dotnet build src/Xiigen.Platform.Api/Xiigen.Platform.Api.csproj
  Expected: 0 Error(s)
  Actual: [result]

Step 5 — Callable:
  Command: dotnet test --filter "FullyQualifiedName~DocumentStoreTests"
  Expected: >=1 passed, 0 failed
  Actual: [result]

Step 6 — Result visible:
  Command: dotnet test --filter "FullyQualifiedName~DocumentStore_UpsertThenGet"
  Expected: PASSED; assertion verifies result.Data!.Body["title"]
  Actual: [result]

Step 7 — Pipeline complete (if pipeline claim):
  Command: dotnet test --filter "FullyQualifiedName~PublishLegalDocumentPipeline"
  Expected: [N] passed, 0 failed
  Actual: [result]

Step 8 — Result observable end-to-end (if endpoint claim):
  Command: dotnet test --filter "FullyQualifiedName~PublishLegalDocumentEndpointE2E"
  Expected: PASSED; WebApplicationFactory<Program> response asserted (status + body)
  Actual: [result]

Round-trip verdict: PASS | PARTIAL ([steps that failed]) | FAIL
```

## Section 3 — Partial round-trip protocol

When Steps 1-6 all pass but Steps 7-8 fail:
- the delivery claim is scoped to the **component**, not the pipeline/endpoint;
- the handoff states: "Component delivered; pipeline/endpoint integration pending";
- the next phase must include Steps 7-8 as mandatory Gate-B conditions.

When any of Steps 1-6 fail:
- the phase is **NOT** complete;
- the failing step is the next task — do not move to a different step;
- identify the root cause (`systematic-debugging` / `test-failure-triage`) before
  fixing.

## Anti-patterns

1. **"Tests pass, so it's delivered"** — passing unit tests satisfy Steps 5/6
   only. Steps 2-4 (build visibility, DI registration, consumer discoverability)
   can fail independently of test results.
2. **"Registration is obvious, skip Step 3"** — a missing DI registration produces
   a runtime resolution error, not a build error. Step 3 is not optional.
3. **"I'll verify Steps 7-8 later"** — "later" becomes "never" at session end.
   Steps 7-8 are required before any pipeline/endpoint delivery claim.
4. **"Partial delivery counts as delivered"** — a handoff that says "delivered"
   for a partial round-trip produces incorrect project state. Use "component
   delivered, pipeline integration pending".

## Integration

- `phase-completion-packager-SKILL.md` requires a round-trip nomination as
  delivery evidence.
- `e2e-test-matrix-SKILL.md` Step mapping: Step 5/6 covers Category 1 (happy
  path); Step 7 covers Category 7 (boundary).
- `coverage-completeness-gate-SKILL.md` reads the round-trip verdict to decide
  whether a pipeline/endpoint delivery claim is authorized.
- The active `SESSION-XX` file governs when the broad `dotnet build`/`dotnet test`
  gate opens.
