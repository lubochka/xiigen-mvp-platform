# Self-Verification (XIIGen Enterprise Platform)

## Overview

Six rules for verifying your own work before claiming it is correct. Applied
after writing code, before claiming completion.

Platform port. Stack-bound to **.NET 10 ASP.NET Core Web API**, **xUnit**
(`tests/`), `DataProcessResult<T>`, the platform fabric interfaces, and Roslyn
for any generated-code checks.

## Rule 1 — Fresh Fixtures

Tests must use real contracts, not hand-crafted fakes that happen to pass. For a
fabric interface, build the **real** implementation through a `ServiceCollection`
/ DI, not a mock of the thing under test.

```csharp
// ✅ Real fixture — exercises the real InMemorySchedulerService through DI
var services = new ServiceCollection();
services.AddSingleton<ISchedulerService, InMemorySchedulerService>();
using var provider = services.BuildServiceProvider();
var scheduler = provider.GetRequiredService<ISchedulerService>();

var result = await scheduler.ScheduleDelayedAsync(
    action: "send-invoice",
    delay: TimeSpan.FromMinutes(5),
    payload: new Dictionary<string, object?> { ["tenant"] = "t1" },
    idempotencyKey: "k1");

Assert.True(result.IsSuccess);
Assert.Equal("send-invoice", result.Data!.Action);

// ❌ Hand-crafted fake — returns whatever you tell it to, proves nothing
var mock = new Mock<ISchedulerService>();
mock.Setup(x => x.ScheduleDelayedAsync(...)).ReturnsAsync(DataProcessResult<ScheduledJobResult>.Success(fake));
```

Mocks are for testing callers, not for verifying that the thing being called
works. For ASP.NET Core endpoints, prefer `WebApplicationFactory<TEntryPoint>`
so the real request pipeline runs.

## Rule 2 — Categorize the Change

| Category | Definition | Verification Required |
|----------|-----------|----------------------|
| `MACHINE_CHANGE` | Changed a fixed interface or contract (e.g. a fabric interface signature) | All callers + integration tests |
| `FREEDOM_CHANGE` | Changed a swappable implementation (e.g. one `ISchedulerService` impl) | That implementation's tests only |
| `BACKWARD_COMPAT` | Changed without breaking the contract | Contract test + regression suite |
| `NEW_ARTIFACT` | Added a new class/interface/endpoint | Unit + integration + doc sync |

Categorize first, then choose the verification scope. Over-verifying wastes time;
under-verifying ships bugs.

## Rule 3 — Right Level

| Change Blast Radius | Test Level |
|--------------------|-----------|
| Single method, no callers changed | Unit test (`[Fact]` + direct construction) |
| Class with DI dependencies | Integration test (`ServiceCollection` + real DI) |
| Endpoint behavior | `WebApplicationFactory` integration test |
| Multiple projects affected | Full suite (`dotnet test Xiigen.Platform.slnx`) |
| Public/fabric interface changed | All callers tested + full suite |

Do not run the full suite to verify a private method change; do not run only a
unit test to verify a fabric-interface change.

## Rule 4 — Cascade

Trace the full effect chain before declaring a fix complete.

```
Changed: InMemorySchedulerService.ScheduleDelayedAsync()
  -> Used by: the API endpoint that schedules jobs
    -> Used by: any application service composing scheduling + outbox
```

Find callers:
```bash
grep -rn "ISchedulerService\|ScheduleDelayedAsync" src/ --include="*.cs"
```
Verify each caller's tests still pass after the change.

## Rule 5 — Non-Renderable

Generated code (e.g. a code generator's output) cannot be verified by inspection
alone. Inspection is not verification.

```csharp
// Verify generated C# parses without error (Roslyn)
var tree = CSharpSyntaxTree.ParseText(generatedCode);
var diagnostics = tree.GetDiagnostics();
Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
```

For generated JSON/config evidence: parse it (`JsonDocument.Parse`) and assert
the expected keys, rather than eyeballing it.

## Rule 6 — Predict

Before a change, state a prediction; after, verify it.

Pre-change:
```
Before touching src/Xiigen.Fabrics.Scheduler/Scheduler.cs:
1. Current test count: [dotnet test --filter ~Scheduler -> record N]
2. Predicted outcome: the empty-action test goes FAIL -> PASS
3. Predicted side effects: none — callers are {scheduling endpoint only}
4. Blast radius check: grep confirms ISchedulerService callers
5. Risk: validation branch may reject a previously-accepted input
   -> Mitigation: run the full Scheduler suite, not just the new test
```

Post-change:
```
1. Actual test count: [dotnet test -> N+1 or same]
2. Prediction matched? YES / NO — if NO, explain delta
3. Side effects observed: none / [describe]
```

If prediction does not match outcome: investigate before claiming complete.

## Verification Quick Checklist

```
□ Categorized the change (MACHINE/FREEDOM/BACKWARD_COMPAT/NEW_ARTIFACT)?
□ Identified blast radius — all callers in the chain?
□ Test level matches blast radius?
□ Fresh fixtures used (real impl via DI / WebApplicationFactory, not mocks of the thing tested)?
□ Generated code verified with a Roslyn/JSON parse check, not just inspection?
□ Pre-change prediction recorded?
□ Post-change outcome matches prediction?
□ DataProcessResult success comes from real data, not a canned Success()?
□ dotnet test (allowed scope) shows Failed: 0?
```
