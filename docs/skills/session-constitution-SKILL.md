# session-constitution-SKILL.md — Supreme law of every session (start / end / critique)

> Ported from the `llm_mvp_core` governance standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build` / `dotnet test`, domain return type `DataProcessResult<T>`
> (`src/Xiigen.Kernel/DataProcessResult.cs`) — **not** core `OperationResult<T>`.
> This skill is subordinate to the v3.3 Beacon
> (`XIIGEN-ENTERPRISE-ADAPTATION-BEACON-PROMPT-v1.md`), `AGENTS.md`, `CLAUDE.md`,
> Luba's current instruction, and the active session file. It **lifts the
> `CLAUDE.md` "Executor Cannot Stop The Plan" + `AGENTS.md` "No Secrets In Repo"
> prose into a checkable constitution**, and adds the sub-agent commit/push
> boundary. Build/test run only when the active session file opens that gate.

## What this skill does
Governs session start, session end, and the critique-response protocol. Every
session follows this protocol; it cannot force a build/test or a user-facing stop
when the active session file names a different allowed verification and a known
`next_action`.

---

## Session Start Protocol (run every time)

```
Step 1 — Beacon first
  Read docs/plans/enterprise-adaptation/v3.3/XIIGEN-ENTERPRISE-ADAPTATION-BEACON-PROMPT-v1.md
  Then state json, completion matrix, and the active SESSION-XX-*.md.
  Skills/local habits/old plan versions cannot override the Beacon.

Step 2 — Branch check
  git branch --show-current
  → Record the current branch. Do not switch branches unless Luba explicitly asks.

Step 3 — Clean build baseline
  dotnet build Xiigen.Platform.slnx
  → Run only when the active session file opens that gate.

Step 4 — Test baseline
  dotnet test 2>&1 | tail -10
  → Run only when the active session file opens that gate.

Step 5 — Read session plan / matrix
  Read the active SESSION-XX-*.md completely + the matrix rows it owns.
  Record one specific fact from the end of the session file (proof of reading).
```
**Do not skip steps.** A build baseline from a previous conversation is not a baseline.

---

## Session End Protocol (before every commit)

```
Step 1 — Contract/fabric audit
  grep -rn "throw new" src/ --include="*.cs"
    → Zero matches in business logic (DataProcessResult<T> everywhere)
  grep -rn "new [A-Z][a-zA-Z]*Store\b\|new [A-Z][a-zA-Z]*Service\b" src/ --include="*.cs"
    → Zero direct instantiation of concrete fabric types in production (Fabric-First; DI only)
  grep -rni "AKIA\|BEGIN .*PRIVATE KEY\|password=\|Bearer " src/ --include="*.cs"
    → Zero secrets in source (No-Secrets — evidence records redacted identifiers only)

Step 2 — Tenant-scope audit
  Any data access path uses ITenantContextAccessor.Current; no cross-tenant read.

Step 3 — Test gate
  dotnet test 2>&1 | tail -10
  → Run only when the active session file opens that gate. Must show 0 failures when required.

Step 4 — Documentation sync
  Update DECISIONS-LOCKED.md, the matrix, and the runbook if the change made them stale.
  Update before committing, not after.

Step 5 — State / matrix update
  Update the v3.3 state json + completion matrix with deliverables, evidence, next step.

Step 6 — Commit (parent / commit-only work order only)
  git add [specific files — not "git add -A"]
  git commit -m "phase N: [description] — [list every changed file]"
  git push origin [current-branch]
```

### Sub-agent commit/push boundary
For bounded sub-agents, **Step 6 is forbidden** unless the work order is
commit/push-only or explicitly grants commit/push for the exact file set.
`STATE.json next_action`, a passed gate, or a known next phase is **not**
commit/push authority. An artifact/code/doc/data/report/plan/governance sub-agent
returns artifacts + evidence only; the parent performs the commit/push boundary
after accepting the packet, or launches a separate commit-only work order with an
exact file list and commit message.

When `commit_push_authorized=false`, git write/ref commands are forbidden as
commands, not only as outcomes: no `git add`, `git commit`, `git push`,
`git checkout`, `git reset`, `git branch`, `git tag`, `git merge`, `git rebase`,
or any staging/ref-writing/history/branch/publish command unless the work order
explicitly grants it for the exact file set. Read-only `git status`, `git diff`,
`git log`, `git show`, `git rev-parse` are allowed only when listed in the work
order. A sub-agent report that claims it committed/pushed/staged/changed-refs
without exact authority is a process violation even when the files are useful;
the artifacts go through quarantine validation and the agent is not delegated
more write work in that phase family until the loophole is recorded and repaired.

---

## Critique-Response Protocol
When a plan, approach, or implementation is in doubt:
```
Rule: Ask ONE precise question only when the Beacon, session file, state, matrix,
local files, and document set cannot derive a safe next action. If the next
action is known, continue instead of asking.
```
A precise question has: one specific decision point, the current state described,
the two concrete options, and nothing else. Wait for the answer before writing
code only when this is a real blocker.

---

## Forbidden Decisions (escalate to Luba)
Never decide these unilaterally:

| Forbidden | Why |
|-----------|-----|
| Commit a raw AWS key / GitHub token / Vault value / JWT signing key / tenant secret | `AGENTS.md` No-Secrets — MACHINE invariant |
| Change a public Web API endpoint or DTO signature | Breaks all consumers |
| Relax the tenant-scope invariant for convenience | Cross-tenant leak |
| Change the domain return type away from `DataProcessResult<T>` | Cross-assembly contract |
| Skip a required Gate-B (`dotnet test`) for any reason | No exceptions |
| Add a new NuGet dependency | Dependency policy is product scope |
| Create a new project (`*.csproj` / `.slnx` entry) | Architecture decision |
| Decrease the test baseline count | Regressions require approval |

---

## Session complexity scaling
| Session type | Required start steps | Required end steps |
|-------------|---------------------|-------------------|
| Bug fix (single file) | Steps 1–4 | Steps 1, 3, 6 |
| Feature (single module) | Steps 1–5 | Steps 1–6 |
| Phase (multiple modules) | Steps 1–5 | Steps 1–6 + doc/matrix sync |
| Architecture (interface changes) | Steps 1–5 + read all affected files | Steps 1–6 + full doc sync |

Scale up, never down. When uncertain, use the more thorough protocol.

---

## Self-verification loop (session end)
```
□ Beacon + state + matrix read at session start?
□ Build baseline recorded (if the session opened it)?
□ Test count: same or higher than start (if the session opened it)?
□ 0 failures — verified with a fresh dotnet test run when required?
□ No throw in business logic (DataProcessResult<T>)?
□ No direct fabric instantiation in production (Fabric-First / DI)?
□ No secrets in source (No-Secrets)?
□ Tenant-scope honored on every data path?
□ Gate B for every placement explicitly met?
□ Docs/matrix synced; runbook updated if stale?
□ State + matrix saved; no required row left UNKNOWN/FAIL when claiming done?
□ Commit message lists every changed file?
□ Executor did not self-finish — slice handed to REVIEW_INTERNAL?
```
**"I think I covered everything" is not a checklist result.** Run each box with evidence.

## Integration
- `session-scope-resolution-SKILL.md` dispatches the per-type document + chain set.
- `no-product-decisions-SKILL.md` lists the escalation boundary in detail.
- `decision-capture-SKILL.md` records the session's significant decisions.
- `verification-before-completion-SKILL.md` is the final gate before any done claim.
