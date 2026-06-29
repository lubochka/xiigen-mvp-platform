# Dev Safety (XIIGen Enterprise Platform)

## Overview

Build gate, git discipline, and a pre-commit DNA quick check. Platform port of
the core dev-safety skill, re-bound to this stack and to the governance prose
that currently lives only in `AGENTS.md` / `CLAUDE.md` (No-Secrets list,
executor-cannot-self-finish, role loop). This skill lifts that prose into an
enforceable skill file.

Stack-bound to **.NET 10 ASP.NET Core Web API**, **xUnit** (`tests/`),
`DataProcessResult<T>`, the platform fabric interfaces, and Roslyn for
generated-code checks. Gate-B is `dotnet build Xiigen.Platform.slnx` +
`dotnet test`. `Directory.Build.props` sets `TreatWarningsAsErrors=true`, so a
warning is a build failure.

## When to load

- At the start of every `EXECUTOR_INTERNAL` session.
- Before any commit.
- When a new `.cs` service file is created (Rule 1 + Rule 2 quick check, with
  `dna-compliance-guard-SKILL.md`).

---

## SESSION GATE — runs at start AND end of every phase

```bash
dotnet build Xiigen.Platform.slnx                     # 0 errors AND 0 warnings
dotnet test  Xiigen.Platform.slnx --logger "console;verbosity=minimal"  # 0 failed, count >= baseline
git diff --name-only                                  # only expected files changed
```

All three must pass. If any fails: fix before proceeding. Per `CLAUDE.md`
("Governance Startup"), broad build/test commands run only when the active
session file opens that gate — otherwise run only the commands the session allows.

---

## Build errors AND warnings = BLOCKED

A C# compile error is not a warning, and on this platform a **warning is a build
failure** (`TreatWarningsAsErrors=true`). Compile errors cause real runtime
failures because:
- `DataProcessResult<T>` generics break if the type argument is wrong — the wrong
  `T` silently changes which `.Data` is returned.
- Interface mismatches make the DI container resolve the wrong concrete fabric.
- A nullable-warning-as-error often marks a real null-deref path.

**Protocol on a compile error/warning:**
1. STOP all other work.
2. Fix it completely.
3. Re-run `dotnet build Xiigen.Platform.slnx` — 0 errors AND 0 warnings.
4. Then continue.

**Anti-patterns to reject immediately:**
- "I'll fix the nullable warning later, the logic is right" → REJECT (it is a build break here).
- "It's just an unused-using warning, harmless" → REJECT — fix it now.
- "Suppress the warning to get green" → REJECT unless the session explicitly approves the suppression with a justification.

---

## Test suite discipline

### Count must never decrease

Session-start count is the floor. If tests drop:
1. Do NOT proceed.
2. Identify which tests disappeared and why.
3. Accidentally deleted → restore. Helper renamed → update references.
4. Re-run until count is ≥ baseline.

### Never change an assertion to make a test pass

```csharp
// WRONG — changing the assertion to accept wrong output
Assert.Equal("CODE_Q", result.Data!.Domain);  // ← changed from "BUG" because code returned "CODE_Q"

// CORRECT — fix the service so it returns "BUG"
```

Changing an assertion to pass is accepting wrong output. Fix the service. The
only exception: the active session file / product owner explicitly says "the old
behavior was wrong, update the test" — that needs explicit written approval.

### Baseline

Always read the actual number from `dotnet test Xiigen.Platform.slnx` output —
never copy from memory. Record it in the session/STATE file as the floor.

---

## Git discipline

### Branch

Work on the current working branch (e.g. an `UpdateUniversalSkills` /
worktree-named branch), never directly on `main`/`master`. Per `CLAUDE.md`
("Enterprise Scope"), implement only in
`github.com/lubochka/xiigen-platform-dot-net-angular` (this repo) — not in
`xiigen-mvp`, customer forks, validation projects, or exported package folders.

```bash
git branch --show-current
# Must be the working branch — never main/master directly
```

### Commit hygiene

- One commit per phase — never bundle multiple phases (see `phase-git-report-SKILL.md`).
- Message format: `phase N: what changed — files changed` (or the existing `P{n}:` style).
- Never `git commit --no-verify`. Never commit with build errors/warnings or failing tests.

### Sensitive files — never commit (AGENTS.md "No Secrets In Repo")

- Raw **AWS access keys**, **GitHub tokens**, **Vault** values.
- **JWT signing keys**, **Docker registry tokens**.
- **Tenant secret payloads**, private **license HMAC** secrets.
- `.env` / `.env.*`, `*.pem`, `*.pfx`, `*.key`, `*.cert`.
- `appsettings.*.json` containing real connection strings or secrets.

Evidence may record **proof-of-presence metadata and redacted identifiers only** —
never the secret value.

---

## DNA quick check — run before every commit

For every new or modified `.cs` file in `src/` (not in `tests/`):

```
☐ Rule 1: every public service method returns DataProcessResult<T> — no void, no raw bool, no throw for business logic
☐ Rule 2: every fabric dependency injected through interface — no "new InMemoryDocumentStore()" etc.
☐ Rule 5: tenant from ITenantContextAccessor.Current; no shared mutable static in singleton services
☐ Rule 7: file/checkpoint writes are atomic (temp-then-rename); repeatable ops use IIdempotencyStore
☐ Rule 8: persist (UpsertAsync) before dispatch (AppendAsync / ScheduleDelayedAsync)
☐ Base: every platform service extends MicroserviceBase<TService> and an I… contract
☐ Result: DataProcessResult<T> only — no OperationResult<T> in platform .cs files
```

If any check fails: fix before committing. Full detector commands live in
`dna-compliance-guard-SKILL.md`.

---

## Zero failures rule

"Zero failures" means zero. Not "zero new failures." Not "pre-existing failures
excluded." Pre-existing failures are fixed in the current phase, not deferred.
There is no narrow reading where only new code must pass.

---

## Executor handoff (AGENTS.md / CLAUDE.md)

The executor may not self-finish. A green build/test, a written report, a
commit, a push, or a matrix update is `EXECUTOR_INTERNAL` evidence that hands
control to `REVIEW_INTERNAL`. Only an architect verdict authorizes final
reporting, and only when `stop_allowed` is true and the completion matrix has no
required `UNKNOWN`/`FAIL` rows. Soft stops ("commit done", "report written",
"context getting long") are invalid.
