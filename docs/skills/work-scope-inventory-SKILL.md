# Work Scope Inventory

> Ported from the `llm_mvp_core` work-scope standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build Xiigen.Platform.slnx` / `dotnet test`. The "subproject" unit is a
> platform project `src/Xiigen.*` (Kernel/Auth/Freedom/Fabrics.*/Platform.Api/
> Marketplace). Total = number of `src/Xiigen.*` projects. Complete =
> `dotnet test` over `tests/Xiigen.*.Tests` → 0 failed (+ DNA evidence). Blocked =
> missing fabric contract / unresolved v3.3 decision. The denominator claim is
> synchronized with the v3.3 COMPLETION-MATRIX before any "pipeline improved"
> claim. There are no UI units. Domain return type is `DataProcessResult<T>`.

## Core Problem

A session that improves 3 projects without knowing there are 7 has not made
platform-level progress — only local progress on an unknown fraction. This skill
installs the denominator before any work begins.

---

## When to Invoke

- Before any session that touches multiple projects.
- Before any platform-level / pipeline-level improvement claim.
- Before any STOP that reports a percentage reduction in errors or failures.
- At session start for any session spanning more than one project.

One run before work begins = no overclaimed platform-level improvement claims.

---

## Section 1 — The Scope Tracking Record

Before any project is touched, produce a scope tracking record:

```json
{
  "scope": {
    "totalProjects": 7,
    "completeCount": 0,
    "partialCount": 0,
    "blockedCount": 0,
    "notStartedCount": 0,
    "thisSessionCovers": [],
    "denominator": "N of 7 projects (M blocked → N of (7-M) eligible)",
    "capturedAt": "session-start"
  }
}
```

**Rule:** A session without a scope tracking record may only claim improvement at
the individual project level for projects it fully tested in that session.

---

## Section 2 — The Count Protocol

Before the first file is touched, establish all counts.

### Count 1 — Total project count

```bash
# Count platform source projects
ls -d src/Xiigen.* | wc -l
# → total count (Kernel, Auth, Freedom, Fabrics.InMemory, Fabrics.Scheduler, Platform.Api, Platform.Marketplace)
```

Or for a specific pipeline: "This session covers the Marketplace capabilities:
Smart Wiki, Smart Code (1 project, 2 capabilities)." The full platform total
remains the denominator for any platform-level claim.

### Count 2 — Complete count

A project is complete when all its tests pass AND it has DNA compliance evidence.
```bash
dotnet test --filter "FullyQualifiedName~[ProjectName]" --no-build 2>&1 | tail -3
# → per-project pass/fail
```

### Count 3 — Partial count

A project is partial when started but with failing tests, build warnings, or
missing DNA checks.

### Count 4 — Blocked count

A project is blocked when a prerequisite (fabric contract, dependency, or v3.3
decision) is not yet in place. Check `prerequisite-chain-SKILL.md` for the chain.

### Count 5 — Not-started count

```
not_started = total - complete - partial - blocked
```

### Count 6 — The denominator

```
Denominator: [sessionProjectCount] of [total - blockedCount] eligible projects
 ([sessionProjectCount / (total - blockedCount) × 100]% of eligible)
```

---

## Section 3 — Scope Tracking Record Template

```markdown
## Work Scope Inventory (work-scope-inventory-SKILL)

Platform total: [N] projects
Complete (all tests pass, DNA checked): [N] projects
Partial: [N] projects
Blocked (missing prerequisites): [N] projects — [list names]
Not started: [N] projects

This session covers: [list projects or pipeline name]
Session count: [N] of [total - blocked] eligible projects
Denominator: [N / (total - blocked) × 100]% of eligible

Claim authority:
  Platform-level: NOT ESTABLISHED (session covers < 20% of eligible)
  OR
  Platform-level: ESTABLISHED (session covers ≥ 20% of eligible)
  Project-level: ESTABLISHED for [list projects fully tested this session]
```

Synchronize the denominator with the v3.3 COMPLETION-MATRIX before any "pipeline
improved" claim — the matrix is the canonical platform completion record.

---

## Section 4 — Claim Authority Rules

| Session covers | Permitted claim |
|---|---|
| 1 project, fully tested | "ProjectName improved" |
| N projects, all fully tested | "These N projects improved" |
| ≥ 20% of eligible, all fully tested | "Platform is improving" (with coverage-completeness-gate check + v3.3 matrix sync) |
| < 20% of eligible | Project-level claims only — no platform-level claims |
| Any session without a scope tracking record | No improvement claims at any level |

**"Fully tested" means:** all tests pass for that project + DNA compliance verified
(+ 0 build warnings, since warnings are errors here).

---

## Anti-Patterns

1. **"I'll establish scope after I see what there is"** — scope is established before work begins, not discovered during work.
2. **"I only need the denominator if I'm making a pipeline claim"** — establish it regardless; sessions that don't plan to make platform claims often end up making them when work goes well.
3. **"I improved 3 of 7 projects — that's meaningful progress"** — meaningful at the project level; at the platform level state the denominator explicitly (3/7 ≈ 43%).
4. **"Partial progress counts as complete"** — complete means all tests pass with DNA compliance and 0 warnings. Partial is partial.

---

## Integration

- `coverage-completeness-gate-SKILL.md` reads the scope tracking record denominator to verify claim scope before every STOP.
- `output-skepticism-SKILL.md` Q2 (scope validity) checks "does my evidence cover my scope?" — absent the record, the answer is "unknown".
- The v3.3 COMPLETION-MATRIX is the canonical record the denominator is reconciled against.
