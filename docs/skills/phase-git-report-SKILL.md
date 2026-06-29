# Phase Git Report (XIIGen Enterprise Platform)

## Overview

After a phase passes Gate-B and before push, create a traceable record linking
the plan, the commits, the changed files, and the before/after test counts. This
creates a bidirectional trace: plan → commits → files, so any `git log` /
`git blame` / `git bisect` reveals which plan and phase produced a change. On
this platform the report closes the loop to the completion matrix:
`session → commits → matrix row`.

Stack-bound to **.NET 10 ASP.NET Core Web API**, **xUnit** (`tests/`), and
Gate-B = `dotnet build Xiigen.Platform.slnx` (0 errors AND 0 warnings) +
`dotnet test Xiigen.Platform.slnx`.

> Reference only: this skill describes the git-trace report. Whether git runs is
> governed by the active session file and the role loop (`AGENTS.md`); the
> executor produces evidence and hands off to `REVIEW_INTERNAL`.

## When to invoke

After the phase gate passes (`dotnet test` shows `Failed: 0` and total ≥
baseline), as the last step before pushing. Append to the session file,
`STATE`, or the completion-matrix row.

---

## Procedure

```bash
# 1. Commits since phase/session start
git log --oneline --since="[phase start timestamp]" HEAD

# 2. Summary of changed files
git diff --stat HEAD~[n_commits]..HEAD

# 3. Check commit message convention
#    Expected: "phase N: [description] — [changed files]"
#    Example: "phase 3: Smart Wiki indexing — src/Xiigen.Platform.Marketplace/SmartWikiService.cs, tests/Xiigen.Platform.Marketplace.Tests/SmartWikiServiceTests.cs"
```

---

## Output Format

Append to the session file / STATE / completion-matrix row:

```
## Git Report — Phase [N]

Plan / session: docs/plans/enterprise-adaptation/v3.3/SESSION-XX-*.md
Matrix row: [completion-matrix row id]
Branch: [working branch]
Commits this phase: [N]

| Hash | Message |
|------|---------|
| [hash7] | phase 3: Smart Wiki indexing — src/Xiigen.Platform.Marketplace/SmartWikiService.cs |
| [hash7] | phase 3: Smart Wiki tests — tests/Xiigen.Platform.Marketplace.Tests/SmartWikiServiceTests.cs |

Changed Files:
  src/Xiigen.Platform.Marketplace/SmartWikiService.cs              | 120 +++++
  tests/Xiigen.Platform.Marketplace.Tests/SmartWikiServiceTests.cs |  60 ++++
  docs/skills/...                                                  |  12 +
  3 files changed, 192 insertions(+)

Commit Convention Check:
  ✅ All commits follow "phase N: description — file list"
  OR ⚠️ [N] commits have non-standard messages: [list]

Build/Test Gate:
  Build:  0 errors, 0 warnings (TreatWarningsAsErrors=true)
  Before: [N] passed, 0 failed
  After:  [N+M] passed, 0 failed
  Delta:  +[M] tests this phase
```

---

## Commit Message Convention

```
phase N: [what was done] — [every file changed]

Examples:
  phase 3: Smart Wiki indexing — src/Xiigen.Platform.Marketplace/SmartWikiService.cs, tests/.../SmartWikiServiceTests.cs
  phase 4: scheduler idempotency — src/Xiigen.Fabrics.Scheduler/Scheduler.cs
  phase 4: docs sync — docs/skills/dna-compliance-guard-SKILL.md
```

The existing `P{n}:` prefix style from `AGENTS.md` is acceptable as long as the
description and the file list are present. **Why this matters:** without the
convention you cannot trace a regression back to the plan/phase that introduced it.

---

## Commit Discipline Rules

```
□ One phase = one commit
□ List every changed file in the commit message
□ Never commit with Failed > 0
□ Never commit with build errors OR warnings (0 Error(s), 0 Warning(s) required)
□ Never `git commit --no-verify`
□ Include documentation in the same commit as the code it describes — not a follow-up
□ Stage only the files for this phase (e.g. `git add docs/skills`); verify with
  `git diff --cached --name-only` before commit
```

---

## Push range discipline

Before push, record the exact range and confirm it publishes only accepted
commits for this phase:

```
expected_push_range = <upstream>..HEAD
other_actor_unpushed_commits_present = false
commit_push_would_publish_only_accepted_commits = true
```

If the range contains any foreign/old/unreviewed commit, push is forbidden until
the range is repaired or the product owner gives explicit current authority.

---

## Anti-Patterns

1. **Non-standard commit messages** — "wip", "fix", "update files" cannot be traced to a plan.
2. **Giant single commit** mixing unrelated logical changes.
3. **Skipping the git report** — it closes the traceability loop; mandatory before declaring a phase complete.
4. **Committing doc sync in a follow-up commit** — docs are part of the phase; commit them together.
5. **Pushing a range wider than the accepted phase commits.**
