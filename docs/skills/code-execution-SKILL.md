# Code Execution (XIIGen Enterprise Platform)

## Overview

The test gate, the tier system for experiments, and the Gate-B format. Platform
port of the core code-execution skill: Gate-B is a runnable condition (not "works
correctly"), every placement has a metric measured before and after, and one
test asserts one metric.

Stack-bound to **.NET 10 ASP.NET Core Web API**, **xUnit** (`tests/`),
`DataProcessResult<T>`, the platform fabric interfaces, and Roslyn for
generated-code checks. Gate-B is `dotnet build Xiigen.Platform.slnx` (0 errors
AND 0 warnings, `TreatWarningsAsErrors=true`) + `dotnet test` over the platform
test projects.

## When to load

Any session that adds, modifies, or reviews code or tests.

---

## Human-readable plan/report precondition

Do not start implementation from a plan/report that fails the Human-Readable
Architecture Artifact Gate: reject table-only, shortcode-heavy, English-only
where Russian was required, schema-only, or prompt/spec inputs. Per `AGENTS.md`
and `CLAUDE.md`, also read the beacon, state, completion matrix, and active
session file before any execution; skills cannot override the beacon.

For suspicious scorer/selector/router/dictionary/counter repairs, the controlling
artifact must name exact classes/records/methods/tests, affected
sets/checkpoints/manifests, and lifecycle paths. Generic "wrap it" / "add tests"
is not a decision.

---

## Sub-Agent-Only Execution Boundary

If the parent/current session is architect-only, governance-only, or forbidden
to become executor, this skill does not grant the parent permission to build,
test, run, or repair locally. Convert work into bounded sub-agent work orders.

No code executor keeps an unbounded scope. "Do not stop" means continue the
parent-owned bounded phase chain, not one executor running until the whole plan
is closed. Each work order names the exact deliverable, allowed files/commands,
max active time ≤ 10 minutes, required evidence report, and
`must_stop_after_report=true`; remaining work returns to parent review.

While a sub-agent runs, the parent may not read governed files/code for sidecar
research. Parent read-only verification is allowed only after the packet returns.
`parent local sidecar work ban`; `read-only verification after a sub-agent packet
returns`; `sub-agent waiting is not a stop`; `no local non-overlapping work escape`.

Every code-execution packet opens with human-readable status (plain
Russian/plain language): total phases/steps, done/in-progress/remaining, what the
phase solves, what the step solves, what evidence exists, what is not proven, and
the next bounded action. Machine labels and paths follow only after that.

---

## Runtime/Tool Command Construction Gate

Executor work orders must not use huge inline shell scripts. If a command would
exceed 20 nonblank lines, 4000 characters, or include more than one
function/class, write a scoped `script-file runner` under the evidence/run folder
and execute it by path, capturing stdout/stderr and the exit code. Windows error
206 or an oversized inline command is a tooling failure, not a product blocker;
the next step is preflight-only script-file prep, not another huge inline retry.

---

## Evidence Text Formatting Gate

Write every text evidence artifact commit-ready on the first write: UTF-8 without
BOM, LF endings, no trailing whitespace; JSON/JSONL must parse before the packet
returns. For in-repo changes run `git diff --check -- <files>`; for untracked
evidence run an explicit BOM/CRLF/trailing-whitespace/JSON scan. The packet lists
files checked, the command, exit code, and result.

---

## Zero failures is a hard gate

`dotnet test Xiigen.Platform.slnx` must report `Failed: 0` before every commit.
Not "only new tests fail." Not "pre-existing failures excluded." Zero. And the
build must show 0 warnings (`TreatWarningsAsErrors=true`).

---

## One test = one metric

```csharp
// ✅ asserts one concrete metric on real result data
Assert.Equal("send-invoice", result.Data!.Action);
Assert.True(enhanced > baseline, $"expected enhanced > {baseline}, got {enhanced}");

// ❌ too vague — what succeeded?
Assert.True(result.IsSuccess);
```

For `DataProcessResult<T>`, success means `IsSuccess` AND a real `Data` field
value — never `Assert.True(result.IsSuccess)` alone.

---

## Test-before-after rule

Every placement in a plan has a metric measured before and after the change. The
test captures both numbers and asserts `enhanced >= baseline`. The metric may be
a `dotnet test` count, an endpoint assertion via `WebApplicationFactory`, a
`DataProcessResult` field value, or a document/idempotency state check.

---

## Tier system for experiments

```
Tier 1: 1-10 inputs    (unit validation — [Fact] + direct construction)
Tier 2: 11-100 inputs  (integration signal — ServiceCollection + real DI)
Tier 3: 100+ inputs    (production signal — WebApplicationFactory / full suite)
```

FAILED at Tier 2 → escalate to Tier 3. SCALE_UP at any tier → needs more data,
the algorithm is correct.

---

## Subproject / phase completion gate

Before any phase is marked done, verify (with `dev-safety-SKILL.md`):

```bash
dotnet build Xiigen.Platform.slnx                            # 0 errors AND 0 warnings
dotnet test  Xiigen.Platform.slnx                            # 0 failures, count >= baseline
git diff --name-only                                         # only expected files changed
```

Platform test projects (xUnit), all under `tests/`:

```
tests/Xiigen.Kernel.Tests
tests/Xiigen.Auth.Tests
tests/Xiigen.Freedom.Tests
tests/Xiigen.Platform.Api.Tests
tests/Xiigen.Platform.Marketplace.Tests
tests/Xiigen.Fabrics.InMemory.Tests
tests/Xiigen.Fabrics.Scheduler.Tests
```

Scope a single project with `--filter` or by project path, e.g.
`dotnet test tests/Xiigen.Fabrics.Scheduler.Tests`.

---

## MicroserviceBase<TService> rule — every new platform service

```csharp
public sealed class MyService : MicroserviceBase<MyService>, IMyService
{
    public MyService(
        IDocumentStore documents,
        ITenantContextAccessor tenantContextAccessor,
        ICorrelationContextAccessor correlationContextAccessor,
        ILogger<MyService> logger)
        : base(new ServiceDescriptor("xiigen-my-service", "XIIGen My Service", "PLATFORM"),
               tenantContextAccessor, correlationContextAccessor, logger)
    { _documents = documents; }
}
// ❌ class MyService : IMyService                       // missing MicroserviceBase<T>
// ❌ class MyService : MicroserviceBase<MyService>      // missing interface
```

---

## DataProcessResult<T> rule — every public service method

```csharp
// ✅ public DataProcessResult<string> GetTenant() => ...
// ✅ public Task<DataProcessResult<bool>> SaveAsync(...) => ...
// ❌ public string GetTenant() => ...     // no DataProcessResult wrapper
// ❌ public bool Save() => ...            // bool without context
// ❌ public OperationResult<T> ...        // wrong Result type for platform
```

---

## Final Between Bounded Slices Code-Execution Gate

Deprecated term: use **dual-path scenario coverage**, not "product readiness", as
a working gate/status. A scenario is accepted only when the same scenario and
prompt pass Path A (API/boundary: adapter/API/core request, response, trace) and
Path B (`local-agent UI path` in the real host with screenshots/logs/redaction).
If Path B cannot reach a route, record a blocked host-path blocker with exact
screen/state and next repair; do not revive the old live-host gate.

A bounded code slice returns its packet, but the parent must not convert that
return — or a later commit/push — into a final stop while coverage is incomplete
or a known safe next bounded action exists. `commit/push boundary is not a stop`;
`promise_to_continue_is_not_continuation`. Pre-final gate:
`scenario_coverage_complete?`, `explicit_user_stop?`, `next_bounded_action_exists?`,
`next_subagent_started_or_real_blocker?` — false/false/true/false means final is
forbidden.
