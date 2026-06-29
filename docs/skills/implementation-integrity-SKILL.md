# Implementation Integrity (XIIGen Enterprise Platform)

## The Golden Rule for Closing Gaps

**Fix the instance AND add the structural guard that prevents regression.**

Platform port of the `llm_mvp_core` skill. Stack-bound to **.NET 10 ASP.NET Core
Web API**, **xUnit** (`tests/`), `DataProcessResult<T>` (`Xiigen.Kernel`),
`ProjectReference` boundaries, and the v3.3 governance triple.

A session that adds a new fabric interface or service but does not:
- update the `docs/` documentation that describes that contract,
- add a DI registration guard / startup test for the new wiring,
- add xUnit coverage for the new behavior,

...has fixed the instance without adding the guard. The same gap reappears next
session.

UI/Dashboard/UX firewall checks from the core skill (its Check 7 / Check 8) are
**`na`** here — the platform has no frontend.

## Check 1 — Documentation Updated

After adding or changing any interface or class, update the matching `docs/`
description in the same slice (`Documentation follows code`). Verify:

```bash
# The new contract is described where the platform documents fabric/API contracts
grep -rn "INewInterface\|NewClass" docs/
# Expected: signature / behavior block present, status not "Planned" if it ships
```

Do not claim done while known documentation drift remains.

## Check 2 — Tests Cover the New Pattern

```bash
# A test class exists for the new behavior
grep -rn "class .*NewClass.*Tests\|NewClassTests" tests/ --include="*.cs"
# Tests run and pass
dotnet test --filter "FullyQualifiedName~NewClass" 2>&1 | tail -5
# Expected: Failed: 0, Passed: > 0
```

For a fabric interface, the test must use the **real** implementation through DI
(see `self-verification-SKILL.md` Rule 1), not a mock of the thing under test.

## Check 3 — Platform Coding Standards Applied

```bash
# No thrown errors on business paths — use DataProcessResult<T>.Failure(code,msg)
grep -n "throw new" src/Xiigen.<Project>/NewClass.cs | grep -v "ArgumentNullException"
# Expected: 0 matches on the success/failure-return path

# No direct concrete instantiation of a fabric dependency inside business logic
grep -n "new .*Store\|new .*Service\|new .*Accessor" src/Xiigen.<Project>/NewClass.cs \
  | grep -v "new DataProcessError\|new DataProcessResult"
# Expected: dependencies arrive via constructor injection, not `new`
```

`DataProcessResult` discipline: every public service method returns
`DataProcessResult` or `DataProcessResult<T>`; failure is
`DataProcessResult<T>.Failure("CODE", "message")`, never a thrown business
exception and never `null`.

## Check 4 — No New Gaps Introduced

```bash
# Callers still compile
dotnet build Xiigen.Platform.slnx 2>&1 | tail -3
# Expected: 0 Error(s)

# Test baseline unchanged or improved
dotnet test Xiigen.Platform.slnx 2>&1 | tail -5
# Expected: Failed: 0, Total >= session-start baseline
```

(Run the broad build/test only when the active SESSION-XX file opens that gate;
otherwise use the narrow filtered form the session allows.)

## Check 5 — Implementation Reality / Anti-Stub Gate

After any implementation the plan calls done, prove the delivered code is not
only a stub/scaffold/report bundle.

Required evidence:

```text
1. Actual code path:
   - API endpoint/controller -> application service -> fabric interface
     (ISchedulerService / IOutboxStore / IIdempotencyStore / IDocumentStore /
      ITenantContextAccessor) -> persisted/returned DataProcessResult
   - file:line refs for the entrypoint, the called method, and the state
     read/write

2. Not a JSON/report substitute:
   - reports may be evidence, but the phase artifact must include production
     code when the plan asked for it; JSON/PowerShell/gate output alone is
     static evidence, not implementation

3. Not a stub:
   - no `TODO`, `PLACEHOLDER`, `NotImplementedException`, `throw new
     NotSupportedException("not implemented")`, fixed canned return, or no-op
     body on the claimed done path
   - a method that returns `DataProcessResult.Success()` without doing the work
     is a stub on the done path

4. Skill/guide load evidence:
   - the executor packet lists loaded skills, required guides, and injection
     blocks used for this slice
```

If any row is missing, integrity is NOT confirmed. Required state is
`IN_PROGRESS_NEXT_STEP_KNOWN`, with `next_action` = the first missing row.

`JSON/PowerShell/report != implementation reality`.

## Check 6 — Real Project Boundary, Not Test-Only Source Linking

When a slice adds a new project or runtime surface, tests must reference the real
project artifact.

```text
1. The .csproj exists in the requested project folder under src/.
2. Tests reference it via <ProjectReference>, not <Compile Include>/<Link> of
   production source as the main path.
3. A build or equivalent project-boundary command runs against the real project.
4. The state/report says what the evidence proves and what it does not.
```

After editing any state/report JSON, rerun a JSON parse and `git diff --check`
on the changed files before claiming the gate passed. Evidence text files must
be UTF-8 without BOM, LF line endings, no trailing whitespace; JSON must parse.
If `git diff --check`, tests, JSON parse, schema checks, or builds print failure
output or exit nonzero before commit/push, integrity is failed even if a later
commit/push succeeds. Do not chain a check with commit/push so the line can
continue after failure.

## Check 7 — Commit-Group Integrity

Before any phase commit, derive `required_commit_group` from the
reviewer/validation ledger and stage those exact paths; only then use optional
`glob` for same-group discovery. After staging, compare `required_commit_group`
against `git diff --cached --name-only`; a missing required file is a hard fail
before commit. A follow-up commit for a missed required artifact is a
commit-boundary defect, not normal cleanup.

## Implementation Integrity Report Format

```text
change_category: NEW_FABRIC | NEW_ENDPOINT | BACKWARD_COMPAT | CONTRACT_CHANGE
docs_updated: <files or "none needed — reason">
tests_added: <test class + dotnet test result>
standards: throw-on-business=0, DI-not-new=ok, DataProcessResult=ok
build: dotnet build Xiigen.Platform.slnx -> 0 Error(s)
anti_stub: real code path file:line refs; no stub on done path
boundary: ProjectReference ok; no Compile Include of production source
verdict: CONFIRMED | IN_PROGRESS_NEXT_STEP_KNOWN (next_action=...)
```

## Anti-Patterns

1. **"It compiles, so it works"** — build proves syntax, not behavior. Run tests.
2. **"I returned `DataProcessResult.Success()`"** — success without the work is a
   stub; Check 5 must show the real path.
3. **"I'll add the test later"** — tests-later is a fail; add coverage in the slice.
4. **"`throw` is fine here"** — business failures use `DataProcessResult.Failure`,
   not exceptions.
5. **Counting "phase complete" from JSON/report/scaffold evidence** — a report
   proves only the report. If the phase required code, Check 5 must prove the
   real code path and anti-stub evidence first.
