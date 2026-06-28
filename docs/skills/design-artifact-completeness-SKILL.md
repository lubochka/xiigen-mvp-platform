# Design Artifact Completeness

> Ported from the `llm_mvp_core` planning standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build` / `dotnet test`. Detection commands target `.cs` files in
> `src/Xiigen.Platform.*` / `src/Xiigen.Kernel/*` / `src/Xiigen.Fabrics.*` and
> xUnit tests under `tests/*.Tests/`. C5 "reachable" = DI registration in
> `Program.cs` (`builder.Services.Add...`, `AddXiigenInMemoryFabrics()`,
> `AddXiigenMarketplace()`) plus the route registered (`MapGet`/`MapPost`/`MapGroup`)
> — **not** an MVC controller. Domain return type is `DataProcessResult<T>`
> (never core `OperationResult<T>`).

## Core Problem

A C# file that exists on disk is not automatically an implemented capability. An
interface that is defined is not automatically wired. A test class that exists is
not automatically a test class with assertions. This skill installs **5 orthogonal
checks** that distinguish "file exists" from "file is a valid input to downstream
processes." Each check is independent — an artifact can pass Check 1 and fail
Check 2 (the classic stub pattern).

## When to Invoke

- During reconnaissance, for every project the session's scope touches.
- Before any plan step that depends on "X is implemented" / "X is designed".
- When a claim about "X is done / complete / implemented" is captured.
- Before writing a phase plan that builds on existing artifacts.

One completeness check before planning = zero "built on empty stub" execution
failures.

## The 5 Checks

### Check 1 — Files Exist
```bash
ls src/Xiigen.Platform.Marketplace/[Name]Service.cs
ls src/Xiigen.Kernel/[Contract].cs
ls tests/Xiigen.Platform.Marketplace.Tests/[Name]ServiceTests.cs
```
PASS = file exists at expected path. FAIL = missing.

### Check 2 — Fields/Methods Populated
```bash
# Throw-only / placeholder bodies:
grep -n "NotImplementedException\|throw new\|TODO\|FIXME\|NOT_READY" src/Xiigen.Platform.*/[Name].cs
# Empty bodies:
grep -n "{ }" src/Xiigen.Platform.*/[Name].cs
# Test class has at least one fact/theory:
grep -c "\[Fact\]\|\[Theory\]" tests/Xiigen.Platform.*.Tests/[Name]Tests.cs
```
PASS = 0 `NotImplementedException`/placeholder hits; ≥1 `[Fact]`/`[Theory]` in the
test class. FAIL = any `NotImplementedException`; 0 test facts. PARTIAL = some
methods implemented, some return `DataProcessResult.Failure("NOT_READY", ...)`;
tests exist but few.
Caught: the stub-as-implementation pattern — file exists, builds (even with
`TreatWarningsAsErrors=true`), but has no real behavior.

### Check 3 — Content Specific
```bash
grep -n "public.*Task<DataProcessResult" src/Xiigen.Platform.*/[Name].cs
# Verify method names match this capability's contract, not a copy-pasted sibling.
```
PASS = method names match the capability spec; content is capability-specific.
PARTIAL = signatures match a template; body is generic. FAIL = identical method
names to another class with no distinction.

### Check 4 — Matches Specification
```bash
# Every interface method has a concrete implementation:
grep -n "interface I[Name]" src/Xiigen.Platform.Marketplace/[Contracts].cs
grep -n "[MethodName]" src/Xiigen.Platform.Marketplace/[Name]Service.cs
# Concrete class is registered:
grep -n "AddXiigenMarketplace\|AddSingleton<I[Name]" src/Xiigen.Platform.Api/Program.cs
```
PASS = interface ↔ concrete bidirectional match; class registered. PARTIAL = some
methods match; registration missing. FAIL = mismatch without explanation.
Caught: interface-implementation drift.

### Check 5 — Reachable by Consumers
```bash
# Project reference exists:
grep -n "[ProjectName]" src/Xiigen.Platform.Api/Xiigen.Platform.Api.csproj
# Builds:
dotnet build src/Xiigen.Platform.Api → 0 Error(s)
# The route/handler reaches the service, and a test exercises it through HTTP/interface:
grep -n "Map\(Get\|Post\|Group\)\|I[Name]Service" src/Xiigen.Platform.Api/Program.cs
grep -n "I[Name]Service\|WebApplicationFactory" tests/Xiigen.Platform.Api.Tests/[Consumer]Tests.cs
```
PASS = project reference present; build succeeds; route registered; a test calls
the capability through its interface or via an in-process HTTP request
(`WebApplicationFactory<Program>` — no UI in this repo). PARTIAL = reference +
build OK; no consumer test yet. FAIL = reference missing; build fails; type not
resolvable (constructor-injected but not registered → `InvalidOperationException`
at resolve time).

## Per-Artifact Output Format
```markdown
## Design Artifact Completeness (design-artifact-completeness-SKILL)

Capability: [CapabilityName]

| Artifact | C1 (exists) | C2 (populated) | C3 (specific) | C4 (matches spec) | C5 (reachable) | Status |
|---|---|---|---|---|---|---|
| I[Name].cs | PASS | PASS | PASS | — | — | PASS |
| [Name]Service.cs | PASS | PARTIAL | PASS | PARTIAL | FAIL | INCOMPLETE |
| [Name]ServiceTests.cs | PASS | FAIL (0 facts) | — | — | — | INCOMPLETE |

Overall: INCOMPLETE
Blocking: C2 on [Name]Service.cs (3 methods return NOT_READY), C2 on tests (0 [Fact])
Non-blocking: C4 PARTIAL (registration missing — add AddXiigen* / AddSingleton in Program.cs)
```

### Overall Status Categories
| Status | Meaning |
|---|---|
| COMPLETE | All 5 checks PASS across all artifacts |
| DESIGN_COMPLETE_NOT_WIRED | Checks 1–4 PASS, Check 5 FAIL (done, not registered/routed) |
| PARTIAL | Some PASS, some FAIL, no structural blockers |
| STUB | Check 2 FAIL — file exists but throws/returns placeholder |
| MISSING | Check 1 FAIL on any required artifact |

## Gate Enforcement
**Blocking checks:** Check 1 (missing file) or Check 2 (stub/empty) blocks treating
the artifact as a complete input to plans. Options when a referenced artifact
fails a blocking check:
1. **Implement first** — add a task to implement the stub before proceeding.
2. **Scope around the gap** — acknowledge: "this phase proceeds without X; consequence is Y".
3. **Mark DEFERRED** — Luba approves with an explicit reason.
Silent assumption that a stub is usable is not an option.

**Non-blocking checks:** Check 3 / 4 / 5 are surfaced as CONCERNs but do not block
the plan from proceeding.

## Anti-Patterns
1. **"The file exists, so the design exists"** — C1 alone is not completeness; C2 is the difference between existence and usability.
2. **"The stub will be implemented later"** — "later" is a task; if the stub must work before the plan works, the plan has a task to implement it first.
3. **"NotImplementedException is fine while prototyping"** — a STUB blocks treatment as a complete input.
4. **"Tests exist"** — a test class with 0 `[Fact]` is not a test class; C2 on tests requires ≥1 `[Fact]`.
5. **"Wiring is trivial, skip C5"** — constructor injection that is not registered throws at resolve time. C5 is not optional.

## Integration
- `plan-self-validation-SKILL.md` Check 2 (phantom skills) is a specific instance of this skill's Check 1 applied to skill files.
- `phase-preflight-SKILL.md` uses C1–C2 results to classify prerequisites.
- `plan-review-SKILL.md` FC-25 (anti-stub completion) consumes this skill's C2 verdict.
