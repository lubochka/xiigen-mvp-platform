# Blast Radius Tagger (XIIGen Enterprise Platform)

## Purpose

Categorizes every planned deliverable file before writing it. Files with higher
blast radius get additional validation gates. Prevents treating a test file and a
public fabric contract as equivalent deliverables.

Platform port of the core `blast-radius-tagger` skill, re-bound to
**.NET 10 ASP.NET Core Web API**, **xUnit**, and the real `Xiigen.*` project layout.
Gate-B is `dotnet build Xiigen.Platform.slnx` + `dotnet test`.

## When to load

- At the start of any phase — tag every planned deliverable before writing.
- At session end — verify PRODUCTION files have been validated.
- In any session that mixes file types (tests + production + docs + skills).

---

## Blast Radius Categories

| Tag | Definition | Examples (platform) | Additional Gates Required |
|-----|-----------|---------------------|---------------------------|
| TEST-ONLY | Only affects the test suite | `tests/Xiigen.*.Tests/**/*.cs`, test fakes | Standard gates only (`dotnet test`) |
| GOVERNANCE | Affects session behavior | `docs/skills/*.md`, `AGENTS.md`, `CLAUDE.md` | Commit BEFORE code files in mixed sessions |
| PRODUCTION | Affects runtime algorithms | `src/Xiigen.*/**/*.cs`, `*.csproj` | `dotnet build` + `dotnet test` after every change |
| INTERFACE | Changes a public contract | `src/Xiigen.Kernel/FabricContracts.cs`, any `I*` contract, `DataProcessResult.cs`, `MicroserviceBase.cs` | Blast radius assessment + all-caller check |
| DOCS | Documentation update | `docs/*.md`, `docs/plans/*.md` | Verify accuracy against code state |

---

## Rules

1. **Tag first** — assign a blast radius to every deliverable before writing any
   file.
2. **INTERFACE** files require a blast radius assessment
   (`blast-radius-SKILL.md`) before any edit.
3. **GOVERNANCE** files in mixed sessions MUST be committed separately BEFORE code
   files.
4. **PRODUCTION** files must pass `dotnet build Xiigen.Platform.slnx → 0 errors,
   0 warnings` (`TreatWarningsAsErrors=true`) before the session is declared done.
5. **Never mix** TEST-ONLY and INTERFACE in the same commit (different blast radii).
6. **DOCS** files go in the SAME commit as the code they describe (Rule 6 — one
   commit per phase).

---

## Tagging Example

Session: "Add `ListDueAsync` to `ISchedulerService` and implement in
`InMemorySchedulerService`".

| File | Tag | Extra Gates |
|------|-----|-------------|
| `src/Xiigen.Fabrics.Scheduler/Scheduler.cs` (interface section) | INTERFACE | Blast radius assessment first — list all implementations and callers |
| `src/Xiigen.Fabrics.Scheduler/Scheduler.cs` (impl section) | PRODUCTION | `dotnet build` after edit |
| `tests/Xiigen.Fabrics.Scheduler.Tests/SchedulerTests.cs` | TEST-ONLY | `dotnet test` after edit |
| `docs/skills/api-shape-verification-SKILL.md` (update the table) | GOVERNANCE | Must match the new signature |

Commit sequence in this session:
1. GOVERNANCE changes (separate skill/doc files) — separate commit if mixed with code.
2. INTERFACE + PRODUCTION + TEST-ONLY + DOCS — one commit (Rule 6).

---

## INTERFACE Validation Template

When writing to or changing a `Xiigen.Kernel` `I*` contract:

```bash
# 1. List all implementations before changing
grep -rn "class.*:.*IDocumentStore" src/ --include="*.cs"
# → must add the new member to EVERY implementation before committing

# 2. List all callers before changing
grep -rn "\.SearchAsync\|\.UpsertAsync\|\.GetAsync" src/ tests/ --include="*.cs"
# → must verify callers still compile after the change

# 3. Build verification after change
dotnet build Xiigen.Platform.slnx
# → 0 Error(s) AND 0 Warning(s) (TreatWarningsAsErrors=true)

# 4. Rollback note (before editing, record how to revert)
git stash   # only if a revert is needed; commit boundary is owned by the architect
```

---

## PRODUCTION Validation Template

When writing to `src/Xiigen.*/**/*.cs`:

```bash
dotnet build Xiigen.Platform.slnx
# → 0 Error(s) / 0 Warning(s)

dotnet test Xiigen.Platform.slnx --logger "console;verbosity=minimal" 2>&1 | tail -5
# → Failed: 0, count >= baseline

# DNA review: dna-compliance-guard-SKILL.md pre-commit gate on the changed file
# (DataProcessResult<T>, MicroserviceBase<T>, no concrete fabric `new`, no shared mutable static)
```

---

## Test Strategy (per branch)

- **Self-check:** the tag table for a session is reviewed before writing; any
  INTERFACE row must show a completed blast radius assessment.
- **Gate-B:** PRODUCTION/INTERFACE rows are not "done" until
  `dotnet build Xiigen.Platform.slnx` is 0/0 and `dotnet test` is 0 failed.
- **Commit ordering test:** in a mixed session, the GOVERNANCE commit precedes the
  code commit (verified by `git log --oneline`).

---

## Anti-Patterns

1. **Editing a public fabric contract alongside 5 test files without tagging** — the
   interface edit gets no blast radius check.
2. **Committing GOVERNANCE files together with code during a blocked code session** —
   skill files and code should be separable.
3. **Tagging TEST-ONLY and then asserting on call counts** — fragility not caught by
   tagging; also run a test-integrity review.
4. **Treating DOCS as a follow-up commit** — docs go in the same commit as the code
   they describe.

---

## Integration

- `blast-radius-SKILL.md` is the assessment an INTERFACE tag triggers.
- `dna-compliance-guard-SKILL.md` is the PRODUCTION pre-commit gate.
- `change-propagation-SKILL.md` enumerates which DOCS go stale with a code change.
