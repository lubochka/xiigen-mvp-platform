# Claim Verification (XIIGen Enterprise Platform)

## Core Principle

Every claim must be treated as a hypothesis until verified with a command.
A claim without a verification command is an assumption.

Platform port of the `llm_mvp_core` skill. Stack-bound to **.NET 10 ASP.NET Core
Web API**, **xUnit** (`tests/`), `DataProcessResult<T>`, and the v3.3 governance
triple. Constitutional reference: `AGENTS.md` / `CLAUDE.md` — any round-ending
claim without an `ARCHITECT_VERDICT_ALLOW_FINAL` finish verdict is a
project-survival risk.

## Claim Classification

| Class | Definition | What to Do |
|-------|-----------|-----------|
| VERIFIED | Command cited that confirms it | Spot-check one data point |
| INFERRED | Reasonable deduction, reasoning shown but not run | Run the verification command |
| ASSUMED | Stated as fact, no command shown | Treat as hypothesis, verify before using |

## Verification Protocol

For any claim a plan depends on, write and run the verification command:

```
Claim: "ISchedulerService.ScheduleDelayedAsync returns a failure on empty action"

Verification:
  Read src/Xiigen.Fabrics.Scheduler/Scheduler.cs around the guard, OR
  dotnet test --filter "FullyQualifiedName~Scheduler" 2>&1 | tail -5

Expected: a validation branch returns DataProcessResult<...>.Failure("VALIDATION_FAILED", ...)

Fallback: if not present -> claim is false -> callers cannot rely on it
```

## High-Risk Claims That Must Always Be Verified (.NET 10)

| Claim Type | Verification Command |
|-----------|---------------------|
| "Method X exists in class Y" | `grep -n "public .*MethodName" src/Xiigen.<Project>/ClassName.cs` |
| "Interface has method Z" | `grep -rn "MethodName" src/ --include="*.cs"` |
| "Class implements interface I" | `grep -rn "class .*: *IInterfaceName\|: IInterfaceName" src/ --include="*.cs"` |
| "Returns DataProcessResult" | `grep -n "DataProcessResult" src/Xiigen.<Project>/ClassName.cs` |
| "DI registration exists" | `grep -rn "AddSingleton\|AddScoped\|AddTransient" src/ --include="*.cs" \| grep InterfaceName` |
| "Endpoint is mapped" | `grep -rn "MapGet\|MapPost\|MapPut\|MapDelete\|\[Http" src/Xiigen.Platform.Api/ --include="*.cs"` |
| "Namespace is X" | `grep -n "^namespace" src/Xiigen.<Project>/ClassName.cs \| head -1` |
| "Test count is N" | `dotnet test Xiigen.Platform.slnx 2>&1 \| tail -3` (fresh run) |
| "File exists at path P" | `Test-Path "src/Xiigen.<Project>/ClassName.cs"` (PowerShell) |
| "End-to-end capability works" | Run a WebApplicationFactory/integration test that invokes the endpoint and asserts the `DataProcessResult`; reject `validation_only`, static/contract/mixed metric, or `UNKNOWN` cpu/ram |
| "Round can finish" | Verify ARCHITECT_INTERNAL produced `ARCHITECT_VERDICT_ALLOW_FINAL` and `STATE-v3.3.json` allows it; executor self-review is not sufficient |
| "State allows stopping" | Verify current state, not a future row: if `next_action` is non-empty or matrix gaps remain, state must be `goal_reached=false`, `stop_allowed=false` |
| "N review/fix passes completed" | Verify N independently inspectable before/after records with reviewer id, scope, findings, recheck; reject ledger-only evidence |
| "A sub-agent did it" | Verify sub-agent id/nickname, work order, returned packet, reviewed/changed files, and parent acceptance/rejection; parent/self work is not sub-agent work |

## Completion Claim Rejection Rules

Reject or downgrade any completion claim when the supporting artifact contains:

```text
validation_only: true
metric_type contains static
metric_type contains contract
metric_type contains mixed
cpu_ms: UNKNOWN
ram_mb: UNKNOWN
build_or_static_contract_check without an executed runtime/integration probe
compatibility_report_ref without an executed compatibility probe
rollback_ref without an executed rollback/load probe
report cannot be reproduced by current code
missing ARCHITECT_VERDICT_ALLOW_FINAL before a round-ending response
ledger rows used as evidence for completed cycles
parent/self review described as sub-agent review
count_claimed greater than independent_evidence_count
sub-agent claim without sub-agent id or returned packet
```

Correct downgrades:
- static/contract evidence -> "contract gate passed";
- validation-only evidence -> "validation artifact passed";
- `dotnet build` evidence -> "build passed";
- DI/registry field evidence -> "registration references exist";
- generated report evidence -> "report exists";
- ledger/checklist evidence -> "ledger exists; cycles not proven";
- parent/self review -> "parent reviewed; sub-agent review not proven".

Do not combine these into "done", "ready", "accepted", or `TASK_FULLY_COMPLETE`.

> Plugin / IDE-host / Dual-Path-Host high-risk rows from the core skill are
> **`na`** for this repository (no frontend, no plugin host).

## Completion Coverage Claims

Treat a completion report as a claim about the active session and
`COMPLETION-MATRIX-v3.3`, not only about the report file:

```text
1. Extract every literal requirement from the active session's gate and the matrix.
2. Extract exact sets named: endpoint names, fabric interfaces, test counts,
   source-of-truth artifacts, state fields.
3. Compare the actual artifacts to those sets.
4. Reject if any row is missing, mismatched, validation-only, static-only, or
   narrower than scope.
```

`passed: true` in a child report is a child claim. It is not a session/matrix
claim until the coverage matrix proves the child covers the requirement.

If the architect challenges a prior completion claim, treat it as a fresh
verification attempt: show the previous score from state, reset the new attempt
to 0, add discovered holes to state, continue from the first hole. Do not restate
the old report from memory.

## Renumbering / Count Claims

When a plan specifies a count or ordinal (next test count, next matrix row id):

```
Claim: "Next test baseline should be 132"

Verification:
  dotnet test Xiigen.Platform.slnx 2>&1 | tail -3   -> authoritative current count
  grep -n "testCount\|baseline" docs/plans/enterprise-adaptation/v3.3/*STATE*.json

Fallback: live count wins; state files may be stale between sessions.
```

## Anti-Patterns

1. **Accepting test counts from summaries** — run `dotnet test` fresh; counts drift.
2. **Accepting signatures from memory** — grep the file; signatures change.
3. **"It should be there, I added it last session"** — it may not be; verify.
4. **Wrong grep pattern** — anchor it: `"public .*Schedule"` not just `"Schedule"`.
5. **Verifying existence but not shape** — file + method name exists != method has
   the expected signature or returns `DataProcessResult`.
