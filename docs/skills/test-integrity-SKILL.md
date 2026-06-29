# Test Integrity (XIIGen Enterprise Platform)

## Overview

A passing test confirms an assertion holds. It does **not** confirm that the code
path you care about actually executed. Green tests can hide a codebase that got
worse: a stub weakened to skip a guard, two guards stacked so a branch is dead,
new tests that bypass the real interface, or a guard present in one fabric and
missing in its sibling.

This is the platform port of the `llm_mvp_core` skill of the same name. It lifts
the **universal** failure-mode catalogue (FM-1..FM-4) and the four honesty rules
into a reusable file and binds them to this repository's stack:

- **.NET 10 ASP.NET Core Web API**, **xUnit** tests under `tests/`;
- the domain result wrapper **`DataProcessResult<T>`** in `Xiigen.Kernel`
  (`IsSuccess` / `IsFailure` / `Data` / `Error`, where `Error` is a
  `DataProcessError(Code, Message, Details)`);
- the real fabric interfaces `IDocumentStore`, `IIdempotencyStore`,
  `IOutboxStore` (`src/Xiigen.Kernel/FabricContracts.cs`), `ISchedulerService`
  (`src/Xiigen.Fabrics.Scheduler/Scheduler.cs`), and `ITenantContextAccessor`
  (`src/Xiigen.Kernel/TenantContext.cs`);
- the binding **v3.3 Enterprise Adaptation** governance triple
  (`BEACON` / `STATE-v3.3.json` / `COMPLETION-MATRIX-v3.3.md`).

> **`na` for this repository — there is no frontend.** The core skill's UI/UX
> evidence rules (`ui-evidence-scope`, `ux-evidence-scope`) and the
> extension/host failure modes are not portable to a backend-only Web API. They
> are listed once under "Core-specific (note-only)" so a reader knows they were
> considered and deliberately excluded, not forgotten.

## When to load

- A test fails after a guard condition (an `IsSuccess` / `Error` check or any
  branch) is changed.
- A stub is **modified** (not just created) to make a test pass.
- A phase ends with "all tests pass" and at least one test was modified.
- Test count increases by more than the number of new behaviours introduced.
- A new project, test project, or fabric implementation is added and you must
  prove the test exercises the real project boundary.

## Core principle

`A test passing` and `the production path executing` come apart whenever:

- a stub is changed to skip a `DataProcessResult` failure check, so the guard is
  never reached;
- two conditions check the same input, making a downstream branch unreachable;
- a test adds assertions but the stubs bypass the real interface contract;
- an `IsSuccess` guard exists in one service but is missing in a sibling that
  consumes the same input.

When they come apart, test count goes up and everything looks green — but a code
path is silently unguarded. Count **executed** paths, not **passing** tests.

## The four universal failure modes

| Mode | Trigger | Symptom | Rule |
|------|---------|---------|------|
| **FM-1: Stub-Fix Masking** | Stub changed to skip an `IsSuccess` / `Error.Code` check | Test passes, guard never runs | `test-fix-or-code-fix` |
| **FM-2: Guard Stacking** | Two conditions check the same input | Downstream branch unreachable | `branch-reachability` |
| **FM-3: Coverage Inflation** | Tests added but stubs bypass real contracts | Test count up, real path untested | `coverage-vs-execution` |
| **FM-4: Sibling Guard Gap** | Guard added to one fabric/service, missing in its sibling | Same input, inconsistent protection | `sibling-guard-parity` |
| **FM-5: Test-Only Boundary Substitute** | Test project links production files via `<Compile Include>` / `<Link>` instead of a real `<ProjectReference>` | Tests pass, but the requested project boundary is missing | `project-boundary-evidence` |

FM-1..FM-5 are the portable platform set. FM-6 onward in the core skill are
core-specific (UI/UX, extensions, trainable models, host sessions); see the
note-only section below.

## Quick routing table

| What just happened | Apply first | Then |
|-------------------|-------------|------|
| Test fails after a guard was tightened | `test-fix-or-code-fix` | `branch-reachability` |
| Stub changed to skip a `DataProcessResult` failure | `test-fix-or-code-fix` | `coverage-vs-execution` |
| Two guards check the same input path | `branch-reachability` | `coverage-vs-execution` |
| Test count up but stubs bypass the interface | `coverage-vs-execution` | `branch-reachability` |
| New project / test project passes through linked source files | `project-boundary-evidence` | `coverage-vs-execution` |
| Guard in one fabric, missing in sibling | `branch-reachability` (sibling audit) | — |
| "All tests pass" but a guard interaction exists | `branch-reachability` | `coverage-vs-execution` |
| Test uses a hand-crafted synthetic fixture | `contract-driven-testing` | — |

## Rule: test-fix-or-code-fix

Before changing any stub or assertion to make a test pass, answer:

```text
Diagnosis question: WHY is this path failing?

Option A: The code is wrong — the stub is strict and correctly rejecting bad output.
  -> Fix the production code. Keep the stub strict.

Option B: The stub is too strict for a valid code path.
  -> Update the stub to match the real interface contract. Do not weaken it.

Option C: The assertion tests the wrong thing.
  -> Only change the assertion if Luba explicitly approves the behaviour change
     (Iron Rule, see bug-to-tests-SKILL.md).
```

```text
Anti-pattern:
  Test fails because DataProcessResult.IsSuccess == false
  -> Stub changed to always return DataProcessResult<T>.Success(data)
  -> Test passes
  -> the IsSuccess / Error.Code guard in production code is now unreachable

Correct:
  Diagnose WHY IsSuccess == false (read Error.Code / Error.Message)
  Fix the production code to succeed on the happy path
  Keep the stub strict — it should still return DataProcessResult<T>.Failure(...)
  for the error path
```

## Rule: branch-reachability

After adding or changing a guard, verify that **every** branch has at least one
test that reaches it.

```csharp
// Example: stacked guards make the second branch unreachable
if (result.IsFailure)                           // guard 1
    return DataProcessResult<T>.Failure(result.Error!.Code, result.Error.Message);
if (result.Data!.Items.Count == 0)              // guard 2 — can this be reached?
    return DataProcessResult<T>.Failure("EMPTY", "no items");

// Checklist:
// [ ] Is there a test that provides a successful result with an empty Items list?
// [ ] If guard 1 rejects everything that reaches guard 2, guard 2 is dead code.
```

**Sibling audit (FM-4).** After adding a guard to any fabric or kernel service,
check every sibling that processes the same input type:

```text
Platform sibling pairs to audit:
  IDocumentStore implementations (InMemory) <-> any future provider implementation
  IIdempotencyStore.TryReserveAsync          <-> MarkSucceededAsync / MarkFailedAsync
  IOutboxStore.AppendAsync                   <-> ListAsync
  ISchedulerService enqueue path             <-> due/poll path
  Xiigen.Auth service guards                 <-> Xiigen.Platform.Marketplace consumers of the same record
```

If `Xiigen.Fabrics.InMemory` validates an input but a kernel service consuming
the same `DocumentRecord` / `OutboxRecord` does not, that is a sibling guard gap.

## Rule: coverage-vs-execution

Count tests that execute real code paths, not tests that merely pass.

```text
Checklist:
[ ] Does the test exercise the real IDocumentStore.UpsertAsync, or a stub returning Success(default)?
[ ] Does the test exercise the real IIdempotencyStore.TryReserveAsync, or a stub with a fixed reservation?
[ ] Is there at least one test per fabric that uses a non-trivial stub (returns real-shaped records)?
[ ] Does the test assert on the specific output field the behaviour change affects (Data.<field>), not only IsSuccess?
[ ] When FM-3 is suspected: count tests that set up real-shaped input vs tests that use empty stubs.
```

## Rule: contract-driven-testing

Use the real domain records as test inputs — not synthetic hand-crafted
dictionaries that do not match the contract.

```csharp
// Wrong — synthetic fixture that does not match the real record shape
var doc = new DocumentRecord("", "", new Dictionary<string, object?>(), default);

// Correct — real-shaped fixture matching the actual Xiigen.Kernel contract
var doc = new DocumentRecord(
    Id: "tenant-42:legal:tos",                         // real composite id
    Version: "1",                                       // real optimistic-concurrency version
    Body: new Dictionary<string, object?>               // real body shape
    {
        ["title"] = "Terms of Service",
        ["status"] = "PUBLISHED",
    },
    UpdatedAt: DateTimeOffset.UtcNow);                  // real timestamp
```

## Rule: project-boundary-evidence (FM-5)

For a new project, test project, or fabric implementation, the test must
exercise the artifact boundary the work order requested — through a real
`<ProjectReference>`, not by pulling production source files into the test
project.

```text
Checklist:
[ ] The implementation project file exists in the requested folder (e.g. src/Xiigen.Fabrics.InMemory/).
[ ] The test project uses a real <ProjectReference> to the .csproj under test, not <Compile Include> / <Link>.
[ ] `dotnet build <ConsumerProject>` proves the real project boundary compiles.
[ ] The report states whether this is unit, integration, or end-to-end (WebApplicationFactory) evidence.
```

Verify the boundary with the build, not by reading the test names:

```bash
# The test project must reference the project under test, not link its sources:
grep -n "ProjectReference\|Compile Include\|Link" tests/Xiigen.Kernel.Tests/Xiigen.Kernel.Tests.csproj
# Expected: a <ProjectReference> to ../../src/Xiigen.Kernel/Xiigen.Kernel.csproj; no source-linking of production .cs files.
```

If the real runtime path is unavailable in the current slice, write that as an
honest gap and keep the fallback evidence narrow. Do not rename a unit/contract
test as end-to-end runtime proof.

## DataProcessResult error-read idiom (read this once)

`DataProcessResult<T>` carries failure detail on `Error`, a
`DataProcessError(Code, Message, Details)`. The type has **no** `.ErrorCode` or
`.ErrorMessage` members. Read failures exactly the way the real services do
(see `src/Xiigen.Platform.Marketplace/SmartWikiService.cs`):

```csharp
// Read a failure (non-null on the failure path):
return DataProcessResult<T>.Failure(result.Error!.Code, result.Error.Message, result.Error.Details);

// Or with a fallback code when the inner error may be null:
var code = stored.Error?.Code ?? "SMART_WIKI_INDEX_FAILED";
```

A test that asserts an operation "succeeded" by the **absence of an exception**
is wrong: assert `result.IsSuccess == true`, and for failures assert the
specific `result.Error!.Code`.

## .NET 10 platform Gate-B commands

| Claim | Command | Passing evidence |
|-------|---------|------------------|
| Build succeeds | `dotnet build Xiigen.Platform.slnx` | `Build succeeded. 0 Error(s)` |
| One test project | `dotnet test tests/Xiigen.Kernel.Tests 2>&1 \| tail -5` | `Failed: 0, Passed: > 0` |
| Filtered tests reach the fixed path | `dotnet test --filter "FullyQualifiedName~<Class>" 2>&1 \| tail -5` | `Failed: 0` |
| No thrown business errors | `grep -rn "throw new" src/ --include="*.cs"` | Zero matches on business paths (use `DataProcessResult<T>.Failure`) |
| Test references the real project | `grep -n "ProjectReference" tests/<Project>.Tests/<Project>.Tests.csproj` | `<ProjectReference>` present; no `<Compile Include>` of production sources |

> The active `SESSION-XX` file governs which Gate-B commands open. Broad
> `dotnet test`/`dotnet build` runs open only when the session opens that gate
> (CLAUDE.md "Governance Startup"). This skill describes the gate; it does not
> grant the run.

## Anti-patterns

### Anti-pattern 1: Stub-fix masking
```text
X Test fails because IIdempotencyStore returns DataProcessResult.Failure("ALREADY_RESERVED", ...)
  -> stub changed to always return Success(new IdempotencyReservation(...))
  -> the ALREADY_RESERVED guard is now never reached in production

V Diagnose WHY the failure fires; provide a fresh operation key in the test
  Keep the stub strict — it should still return Failure on a duplicate key
```

### Anti-pattern 2: Guard stacking
```text
X A service checks result.IsFailure, then checks result.Data.Items.Count == 0
  on a path where IsFailure already returned — the second guard is dead code

V Run branch-reachability for the combined guard chain; ensure each branch has
  a test input that reaches it
```

### Anti-pattern 3: Coverage inflation
```text
X 5 new tests for the outbox path all use a stub IOutboxStore that always
  returns Success — the AppendAsync failure path is never tested

V At least one test must provide a stub that returns DataProcessResult.Failure(...)
  Count tests that exercise error paths, not just happy-path tests
```

### Anti-pattern 4: Sibling guard gap
```text
X An IsSuccess guard is added after IDocumentStore.UpsertAsync in one service,
  but a sibling consuming the same DocumentRecord has no guard and silently
  processes a failed result's null Data

V After adding a guard, audit every sibling that consumes the same record type
  and apply the guard consistently
```

### Anti-pattern 5: Test-only boundary substitute
```text
X tests/Xiigen.Platform.Api.Tests links Program.cs sources via <Compile Include>
  -> tests pass, but the real Xiigen.Platform.Api project boundary is never built
     as a referenced assembly

V Reference the project under test with <ProjectReference> and prove it builds;
  for API end-to-end use a WebApplicationFactory<Program> integration test
```

## Core-specific failure modes (note-only — not portable here)

These core failure modes target UI surfaces, host extensions, and trainable
models that **do not exist** in this backend-only platform. They are recorded so
a reader knows they were considered and deliberately excluded (per the G05
mapping and `CLAUDE.md` scope). Do not invent UI/extension/model evidence to
satisfy them.

```text
ui-evidence-scope / ux-evidence-scope                  -> na: no frontend in this repo.
dual-path-same-scenario-ui-proof                        -> na: no host UI to drive.
host-session-auth-preflight                             -> na: no Cursor/Claude/Codex host here.
multi-monitor-target-window-control-protocol            -> na: no desktop UI control.
no-blind-spot-ui-scenario-script / 50-pass clarity      -> na: no UI scenarios.
trainable-architecture-evidence (pseudo-trainable)      -> na: common models live in llm_mvp_core (R5).
full-extension-evidence / model-reasoning-evidence      -> na: extensions/model-tier live in llm_mvp_core.
subagent-firewall-evidence / human-readable-visibility  -> governance-wide; enforced via AGENTS.md/CLAUDE.md
                                                            and verification-before-completion-SKILL.md, not here.
```

The platform keeps one stricter-than-core behaviour worth stating explicitly:
**every validation returns `DataProcessResult<T>.Failure(...)`, never throws**
for a business condition (kernel Rule 1). A test that accepts a thrown exception
as the "expected failure" for a business path is a test-integrity defect — assert
the `DataProcessResult` failure and its `Error.Code` instead.
