# Verification Before Completion (XIIGen Enterprise Platform)

## Overview

Claiming work is complete without verification is dishonesty, not efficiency.

**Core principle:** Evidence before claims, always.

**Violating the letter of this rule is violating the spirit of this rule.**

This is the platform port of the `llm_mvp_core` skill of the same name. It lifts
the universal honesty axis into a reusable file and binds it to this repository's
stack: **.NET 10 ASP.NET Core Web API**, **xUnit** tests under `tests/`, the
domain result wrapper `DataProcessResult<T>` (in `Xiigen.Kernel`), and the
binding **v3.3 Enterprise Adaptation** governance triple
(`BEACON` / `STATE-v3.3.json` / `COMPLETION-MATRIX-v3.3.md`).

UI / extension / dashboard / Dual-Path-Host parts of the core skill are **`na`**
for this repository: the platform has no frontend. Only runtime / API / `dotnet`
evidence applies here.

## The Iron Law

```
NO COMPLETION CLAIMS WITHOUT FRESH VERIFICATION EVIDENCE
```

```
NO COUNT CLAIMS WITHOUT COUNT-MATCHING EVIDENCE
NO SUB-AGENT CLAIMS WITHOUT A NAMED SUB-AGENT PACKET
```

Platform constitutional law (from `AGENTS.md` / `CLAUDE.md`):

```text
The executor may not self-finish. A slice result, test result, report, commit,
push, evidence artifact, or matrix update must hand control to REVIEW_INTERNAL.
Only an architect verdict can authorize final reporting, and only when
stop_allowed is true and the COMPLETION-MATRIX-v3.3 has no required UNKNOWN or
FAIL rows.
```

This is a project-survival rule, not a formatting preference. A false completion
claim makes the project plan future work from false evidence. The role loop is
binding:

```text
PLAN_AUTHOR_INTERNAL -> PLAN_REVIEW_INTERNAL -> ARCHITECT_INTERNAL
  -> EXECUTOR_INTERNAL -> REVIEW_INTERNAL
  -> ARCHITECT_INTERNAL_REPAIR -> EXECUTOR_INTERNAL_CONTINUE
```

If you have not run the verification command allowed by the current instruction
or by the active session file's Gate-B, you cannot claim it passes.

Platform override on scope of verification:

```text
This skill does not authorize a broad `dotnet build`/`dotnet test`/deploy by
itself. During governance acceptance, run only the commands the active
SESSION-XX file explicitly allows. Broad build/test/deploy commands open only
when the relevant session opens that gate (CLAUDE.md "Governance Startup").
```

## Soft stops are invalid

These are NOT completion (lifted from `CLAUDE.md` "Executor Cannot Stop The Plan"):

```text
commit done, report written, one child session passed, AWS URL produced,
screenshot captured, plugin harness passed, or context getting long.
```

When any of these happen and `STATE-v3.3.json` still has `stop_allowed=false`
or `next_action` non-empty, the next correct action is `next_action`, not a
final report.

## The Gate Function

```
BEFORE claiming any status or expressing satisfaction:

1. IDENTIFY: What command proves this claim?
2. RUN: Execute the FULL command (fresh, complete)
3. READ: Full output, check exit code, count failures
4. VERIFY: Does output confirm the claim?
   - If NO: State actual status with evidence
   - If YES: State claim WITH evidence
5. ONLY THEN: Make the claim

Skip any step = lying, not verifying
```

Count/actor extension:

```text
6. COUNT: If claiming N cycles/reviews/passes, show N independent evidence
   records of that exact type. Ledger/checklist rows are not cycles.
7. ACTOR: If claiming sub-agent work, show sub-agent id/nickname, work order,
   returned packet, reviewed scope, and parent acceptance/rejection. Otherwise
   say the parent did it or say actor not proven.
```

## .NET 10 Platform Gate-B Commands

| Claim | Command | Passing Evidence |
|-------|---------|-----------------|
| Build succeeds | `dotnet build Xiigen.Platform.slnx` | `Build succeeded. 0 Error(s)` |
| Tests pass | `dotnet test Xiigen.Platform.slnx 2>&1 \| tail -10` | `Passed!  - Failed: 0` |
| One test project | `dotnet test tests/Xiigen.Kernel.Tests 2>&1 \| tail -5` | `Failed: 0, Passed: > 0` |
| Filtered tests | `dotnet test --filter "FullyQualifiedName~Scheduler" 2>&1 \| tail -5` | `Failed: 0` |
| No thrown business errors | `grep -rn "throw new" src/ --include="*.cs"` | Zero matches on business paths (use `DataProcessResult<T>.Failure` instead) |
| Interface present | `grep -rn "ISchedulerService\|IOutboxStore\|IIdempotencyStore\|IDocumentStore\|ITenantContextAccessor" src/ --include="*.cs"` | Matches present |
| DI registration | `grep -rn "AddSingleton\|AddScoped\|AddTransient" src/ --include="*.cs"` | Registration for the interface present |

`DataProcessResult<T>` shape (for "operation succeeded" claims): success is
`result.IsSuccess == true`; failure carries `result.Error.Code` /
`result.Error.Message`. Never claim an operation succeeded from the absence of
an exception — read `IsSuccess`.

## Common Failures in This Project

| Claim | Requires | Not Sufficient |
|-------|----------|----------------|
| Tests pass | `dotnet test` output: `Failed: 0` | Previous run, "should pass" |
| Build clean | `dotnet build Xiigen.Platform.slnx`: `0 Error(s)` | Looks compilable |
| Bug fixed | Test the original symptom: passes | Code changed, assumed fixed |
| Regression test works | Red-green cycle verified | Test passes once |
| `DataProcessResult` correct | Return type + `IsSuccess`/`Error.Code` verified in file | "I think I used it" |
| Fabric wired | `grep` shows DI registration in `Program.cs`/module | "It should be registered" |
| Phase complete | Active session Gate-B met with count/assertion + matrix row | "Tests passing" |

## Red Flags — STOP

- Using "should", "probably", "seems to".
- Expressing satisfaction before verification ("Great!", "Perfect!", "Done!").
- About to commit without running the allowed Gate-B verification.
- Relying on a build run from minutes ago.
- A required verification printed failure output before commit/push; that is a
  hard fail even if a later `git commit`/`git push` succeeds.
- A command chain can continue into stage/commit/push after failed verification.
- In PowerShell, a required verification and `git add`/`git commit`/`git push`
  are on one semicolon-separated line; run the verification as its own step,
  check `$LASTEXITCODE`, and stop before any commit/push on nonzero.
- `chained_precommit_check_bypass`: pre-commit checks (`git diff --check`,
  `git diff --cached --check`) chained with commit/push so the line can continue
  after a failed check.
- A commit group stages by broad `glob` alone instead of exact
  `required_commit_group` paths from the reviewer/validation ledger.

Required verification literals (kept verbatim for exact-file checks):
`commit is forbidden if a pre-commit gate prints warnings/errors`;
`do not chain commit/push after a gate command unless exit code is checked`;
`chained_precommit_check_bypass`.

## Runtime Evidence Firewall (platform scope)

For a request/response, fabric, persistence, scheduling, idempotency, outbox, or
end-to-end API capability claim, `passed: true` in a JSON artifact is not enough.
The verification must prove the same kind of thing as the claim.

Before any completion claim, reject the claim if the evidence contains:

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
```

Required downgrades:
- contract/static evidence proves only contract/static readiness;
- `dotnet build` evidence proves only build readiness;
- validation-only evidence proves only validation schema coverage;
- registry/DI fields prove only registration presence;
- report existence proves only artifact creation.

Runtime completion requires fresh runtime evidence: the endpoint or fabric path
invoked (e.g. through a WebApplicationFactory integration test or a real probe),
`DataProcessResult` success/failure observed, and the result persisted/asserted.

Fresh evidence must be reproducible from the current code. If a persisted report
claims a state but the current code path that produces that report would emit a
different/narrower result now, the report is stale: mark the completion claim
failed, add a hole to `STATE-v3.3.json`, regenerate from current code, continue.

> Dual-Path Host Verification / local-desktop UI / extension-host evidence from
> the core skill is **`na`** here — the platform has no UI surface to drive.

## Completion Coverage Matrix

For multi-step work, fresh verification is not only "run the command". It also
means proving the command covers every literal requirement of the active session
and the `COMPLETION-MATRIX-v3.3`.

Before any final completion claim, require a coverage matrix with:

```text
requirement_id
literal_requirement
expected_literal_set
actual_artifact
actual_value
comparison_operator
passed
gap
next_plan_action
```

Reject completion when:
- a lower-level report says `passed: true` but the matrix lacks the session's
  full literal set;
- a top-level bucket count is used instead of required underlying items;
- "at least one" is used where the requirement says "all", "every", or lists
  exact items;
- `STATE-v3.3.json` says `goal_reached: true` while a matrix row still has a gap;
- state/matrix/self-review name an old commit or stale `next_action` after
  commit/push;
- the matrix accepts a `validation_only: true` report for full runtime coverage;
- the matrix accepts a report not reproducible by current code;
- ARCHITECT_INTERNAL has not issued `ARCHITECT_VERDICT_ALLOW_FINAL` for the round.

The correct result when any row fails is not a final report. Set
`IN_PROGRESS_NEXT_STEP_KNOWN`, copy the first failed matrix row into
`next_action`, and continue.

## Final Execution Review Gate (count honesty)

When sub-agents claim a multi-step plan was fully executed, require independent
review packets before any `ready`/`done`/`complete` claim. A ledger row is not a
review. "N reviews" is valid only with N independently inspectable before/after
records, each naming reviewer id, scope, verdict, findings, repair, and recheck.
A non-PASS review does not count toward the N.

## Why This Matters

- Zero failures is a hard gate — not "green except pre-existing".
- Every placement has a measurable Gate-B before/after.
- A Gate-B is a command with a measurable output, not a vague claim.
- Committing with failures wastes reviewer time and masks regressions.
- A false "done" makes the platform plan from confidence instead of evidence.
