# Remediation Session Design (XIIGen Enterprise Platform)

## Overview

Raw gaps do not map to sessions. A list of 12 gaps may collapse to 3 root causes
(via `root-cause-ladder-SKILL.md`), and those 3 may group into 2 sessions with a
clear dependency. Designing sessions from raw gaps produces 12 sessions instead of
2. This skill takes root-cause-ladder output and produces an ordered session plan:
groups of fixes, each with a session type, blast-radius classification, dependency
chain, pre-flight assumptions, and **an explicit Gate B**.

Stack-bound to **.NET 10 ASP.NET Core Web API**, **xUnit** (`tests/Xiigen.*.Tests`),
`DataProcessResult<T>` (`Xiigen.Kernel` — the platform's native result wrapper,
**not** `OperationResult<T>`), the fabric interfaces, and ASP.NET Core DI in
`Xiigen.Platform.Api/Program.cs`. Gate B per group is
`dotnet test Xiigen.Platform.slnx --filter "..."` returning "N passed, 0 failed"
(plus `dotnet build Xiigen.Platform.slnx` → 0 Error(s)/0 Warning(s) for
INFRASTRUCTURE groups, since `TreatWarningsAsErrors=true`). Group ordering and
the closure of each Level-3 root are tracked as required rows of
`docs/plans/enterprise-adaptation/v3.3/XIIGEN-ENTERPRISE-ADAPTATION-COMPLETION-MATRIX-v3.3.md`,
and the group sequence respects the binding role loop from `AGENTS.md`
(`PLAN_AUTHOR → PLAN_REVIEW → ARCHITECT → EXECUTOR → REVIEW`). There is no
frontend: no Playwright/Angular groups.

## When to invoke

- After `root-cause-ladder-SKILL.md` produces K root causes from N gaps.
- Before scheduling remediation work (to avoid 12 sessions when 2 suffice).
- When a prior remediation session failed to close the gap (re-plan from roots).
- As the planning output when a gate review produces multiple BLOCK verdicts.

## Sub-Agent-Only Parent Boundary

Under a sub-agent-only / architect-only instruction, the architect authors the
session plan and issues one bounded work order per group; each group's fix is
executed by a bounded sub-agent and returns to review. The parent does not
implement the fixes directly. `sub-agent waiting is not a stop`.

---

## Prerequisite

Run `root-cause-ladder-SKILL.md` first. This skill starts from its output:

```
Root cause 1: [description] — covers gaps [N, M, P]
Root cause 2: [description] — covers gaps [Q, R]
Dependency: Root cause 1 (Level 3) before root cause 2 (Level 1)
```

If you have raw gaps but have not run root-cause-ladder: run it first. Do not
group raw gaps into sessions — collapse to root causes first.

---

## Section 1 — Fix Class Taxonomy

| Fix class | Definition (platform) |
|---|---|
| CONTENT | Change to existing logic — no new interfaces, no new DI registrations |
| EXTENSION | Add to existing structure — new method on an existing service, new ctor parameter |
| INFRASTRUCTURE | New fabric interface (`Xiigen.Kernel`/`Xiigen.Fabrics.*`) or new DI registration |
| NEW_CAPABILITY | New service + interface + tests (full capability) |

**Blast radius by fix class:**

```
CONTENT:        ≤ 3 files, ≤ 50 lines  → LOW    → may batch with other CONTENT fixes
CONTENT:        ≤ 3 files, 50–150 lines → MEDIUM → own session if functionally unrelated
EXTENSION:      ≤ 5 files, ≤ 150 lines, no new interface → MEDIUM → may batch if contained
EXTENSION:      > 5 files OR blast radius HIGH → own session
INFRASTRUCTURE: always own session (new fabric interface = blast radius always HIGH)
NEW_CAPABILITY: always own session + plan-review-SKILL.md gate before execution
```

**Hard rule:** > 10 files changed in one session → always its own isolated
session. A 10-file grep-and-replace in a maintenance session is a test-failure
risk.

Estimate file/line counts from the real layout:

```bash
grep -rln "SymbolToChange" src/Xiigen.* --include="*.cs"        # files touched
wc -l src/Xiigen.<Project>/<File>.cs                            # line scale
```

---

## Section 2 — Session Type Assignment

| Fix class | Session type | Rule |
|---|---|---|
| CONTENT (LOW) | MAINTENANCE (batched) | ≤ 3 files, ≤ 50 lines → combine CONTENT fixes |
| CONTENT (MEDIUM) | MAINTENANCE (isolated) | functionally unrelated → own session |
| EXTENSION (MEDIUM) | MAINTENANCE (isolated) | contained blast radius → may batch |
| EXTENSION (HIGH) | MAINTENANCE (isolated) | > 5 files → own session |
| INFRASTRUCTURE | MAINTENANCE + gate review | `plan-review-SKILL.md` FC check required |
| NEW_CAPABILITY | EXECUTOR session | own session + Gate B declaration |

---

## Section 3 — Session Group Format

Every group carries an explicit `Gate B:` line — a group without one is a failed
plan.

```markdown
## Remediation Session Plan

### Group A — [session type]: [one-line description]
Fix class:    [CONTENT / EXTENSION / INFRASTRUCTURE / NEW_CAPABILITY]
Ladder level: [Level 1 / Level 2 / Level 3]
Files:        [list of src/Xiigen.* + tests/Xiigen.*.Tests files]
Lines:        ~[N] estimated
Blast radius: LOW / MEDIUM / HIGH
Depends on:   [nothing / Group X]
Blocks:       [Group Y / nothing]
Matrix row:   [completion-matrix row this group closes]

Fixes:
  A1: [root cause 1] — [file:what to change]
  A2: [root cause 2] — [file:what to change]

Pre-flight assumptions:
  INFRA-1: [assumption] | verify: [bash command] | expected: [result]
  BEHAV-1: [assumption] | verify: [bash command] | expected: [result]

Gate B:
  dotnet test Xiigen.Platform.slnx --filter "FullyQualifiedName~[Test]" → [N] passed, 0 failed
  (+ for INFRASTRUCTURE: dotnet build Xiigen.Platform.slnx → 0 Error(s), 0 Warning(s))

### Group B — [session type]: [description]
Depends on:   Group A complete
Blocks:       Group C
...
```

---

## Section 4 — Dependency Ordering Rules

Order groups by:

1. **Ladder level (from `root-cause-ladder-SKILL.md`):** Level 3 (architectural
   decisions) before Level 2 (skill/structural updates) before Level 1 (immediate
   code changes).
2. **Within the same level:** INFRASTRUCTURE before EXTENSION before CONTENT.
3. **Infrastructure before capability:** a capability injects what infrastructure
   registers; the registration must exist before the injection.

The whole sequence respects the role loop (`AGENTS.md`): each group runs
`ARCHITECT → EXECUTOR → REVIEW`, and the executor cannot self-finish a group —
control returns to review before the next group starts.

```
Example ordering:
  Group A: INFRASTRUCTURE — registers INewFabric in Xiigen.Kernel + Program.cs (Level 3, first)
  Group B: EXTENSION — wires INewFabric into an existing service (Level 2, depends on A)
  Group C: CONTENT — fixes logic in the wired service (Level 1, depends on B)
```

---

## Section 5 — Worked Example

**Input from `root-cause-ladder-SKILL.md` (3 root causes):**

```
Root cause 1: MarketplaceService appends the outbox event BEFORE persisting (violates Rule 8)
  → covers: gaps 1, 3 (lost order on append failure, duplicate event)
  Level 1: CONTENT fix
Root cause 2: IIdempotencyStore not injected into SettlementService
  → covers: gap 2 (double settlement)
  Level 2: EXTENSION (adds a ctor parameter + DI registration)
Root cause 3: No explicit decision for store-before-dispatch (Rule 8) — it is prose only
  → covers: gap 4 (the same ordering question recurs)
  Level 3: ARCHITECTURAL (promote Rule 8 to an explicit decision + matrix row)
Dependency: Root cause 3 must be recorded before 1 and 2 are fixed.
```

**Output:**

```markdown
### Group A — MAINTENANCE: Record the store-before-dispatch decision (Rule 8)
Fix class:    CONTENT (documentation/governance)
Ladder level: Level 3
Files:        AGENTS.md prose → explicit decision entry; completion-matrix row
Lines:        ~10
Blast radius: LOW
Depends on:   nothing
Blocks:       Group B, Group C
Matrix row:   RULE8-STORE-BEFORE-DISPATCH

Fixes:
  A1: Promote "persist (UpsertAsync) before dispatch (AppendAsync/ScheduleDelayedAsync)"
      from prose to an explicit, testable decision and add the matrix row.

Pre-flight:
  GOV-1: the matrix has no row for Rule 8 yet | grep -c "STORE-BEFORE-DISPATCH" <matrix> | expected: 0

Gate B:
  grep -n "STORE-BEFORE-DISPATCH" <completion-matrix> → 1 hit

### Group B — MAINTENANCE: Fix MarketplaceService ordering (persist before append)
Fix class:    CONTENT
Ladder level: Level 1
Files:        src/Xiigen.Platform.Marketplace/MarketplaceService.cs
Lines:        ~15
Blast radius: LOW
Depends on:   Group A (decision recorded)
Blocks:       nothing
Matrix row:   MARKETPLACE-ORDER-PERSIST-FIRST

Fixes:
  B1: Move IDocumentStore.UpsertAsync above IOutboxStore.AppendAsync; return Failure
      and skip the append when the persist fails.

Gate B:
  dotnet test Xiigen.Platform.slnx --filter "FullyQualifiedName~MarketplaceService_PersistsBeforeAppend" → 1 passed, 0 failed

### Group C — MAINTENANCE: Inject IIdempotencyStore into SettlementService
Fix class:    EXTENSION
Ladder level: Level 2
Files:        src/Xiigen.Platform.Marketplace/SettlementService.cs; src/Xiigen.Platform.Api/Program.cs
Lines:        ~20
Blast radius: LOW (one service + one registration)
Depends on:   Group A (decision recorded); independent of B
Matrix row:   SETTLEMENT-IDEMPOTENT

Fixes:
  C1: Add IIdempotencyStore to the ctor; reserve via TryReserveAsync, MarkSucceeded/MarkFailed;
      register it in Program.cs.

Gate B:
  dotnet test Xiigen.Platform.slnx --filter "FullyQualifiedName~SettlementService" → ≥ N passed, 0 failed
  + dotnet build Xiigen.Platform.slnx → 0 Error(s), 0 Warning(s)
```

---

## Anti-Patterns

1. **Grouping raw gaps without running `root-cause-ladder-SKILL.md` first** — 12
   gaps may collapse to 3 roots; grouping 12 gaps = 12 sessions.
2. **Assigning the session type before estimating the file count** — file count
   determines blast radius, which determines session type.
3. **Batching high-blast-radius fixes "for efficiency"** — a 15-file change in one
   session is a test-failure risk; the extra session is cheaper.
4. **Starting Group B before Group A completes** — dependency chains are not
   optional; Group A locked the decision Group B encodes.
5. **Gate B omitted from a group** — every group must have an explicit, runnable
   Gate B. "Works correctly" is not a Gate B (`plan-review-SKILL.md`).

---

## Integration

- `root-cause-ladder-SKILL.md` provides the K root causes (and their levels) this skill starts from.
- `solution-scope-gate-SKILL.md` classifies the fix class per gap.
- `phase-preflight-SKILL.md` verifies each group's pre-flight assumptions before it starts.
- Blast radius (HIGH/MEDIUM/LOW per group) uses this skill's own simplified scale in Section 1.
- `dev-safety-SKILL.md` defines the Gate-B build/test discipline each group's Gate B uses.
