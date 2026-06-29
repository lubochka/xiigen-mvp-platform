# Audit Protocol (XIIGen Enterprise Platform)

> Ported from the `llm_mvp_core` standard, adapted for **.NET 10 ASP.NET Core Web API**,
> xUnit, Gate-B = `dotnet build` / `dotnet test`. **Gate thresholds** are expressed as exact
> `dotnet test -> N passed` counts. **Blast-radius INTERFACE changes** (the fabric contracts
> `IDocumentStore` / `IOutboxStore` / `IIdempotencyStore` / `ISchedulerService` /
> `IFreedomConfigService` / `ITenantContextAccessor`, or `DataProcessResult<T>`) require a
> rollback plan. This audit lays directly onto the existing v3.3
> `XIIGEN-ENTERPRISE-ADAPTATION-COMPLETION-MATRIX-v3.3.md`: a phase cannot complete while
> the matrix has a required `UNKNOWN` or `FAIL` row.

## Core Problem

An ad-hoc audit varies by reviewer and time available. One reviewer checks chain arithmetic
and skips blast radius; another checks DNA compliance but skips API shapes. The result is
findings that depend on who ran the audit, not on what the plan contains. This skill defines
a 10-point checklist that every audit runs against, with severity tags that are consistent
and non-negotiable.

---

## When to Invoke

- During any plan review (`plan-review-SKILL.md` Gate B).
- Before declaring any plan "ready for execution".
- When reviewing another agent's output or plan.
- Before a phase is marked complete against the v3.3 completion matrix.

---

## Section 1 — 10-Point Audit Checklist

Every audit must explicitly address all 10 points. Points not checked must be listed in a
"NOT CHECKED" section with a reason.

| # | Check | Governing Skill | Pass Condition |
|---|---|---|---|
| 1 | **Chain arithmetic** | `chain-arithmetic-audit-SKILL.md` | baseline + Σ(deltas) = final for every metric; no magic-number jumps |
| 2 | **Blast radius tagging** | `blast-radius-tagger-SKILL.md` | every deliverable tagged TEST / GOVERNANCE / PRODUCTION / INTERFACE; INTERFACE changes (fabric contracts, `DataProcessResult<T>`) have a rollback plan |
| 3 | **API shape verification** | `api-shape-verification-SKILL.md` | every call has a verified method signature, parameter types, and return type (`DataProcessResult<T>` vs raw) |
| 4 | **Commit atomicity** | `blast-radius-tagger-SKILL.md` | GOVERNANCE and PRODUCTION files committed before TEST-ONLY; no orphaned partial commits |
| 5 | **Write-time fix verifiability** | — | every write-time fix has a specific `grep` / `dotnet test` command that confirms it landed |
| 6 | **Path verification** | — | every file path verified against the actual codebase (`src/...`, per-flow `*/Api`/`*/Application`/`*/Infrastructure`/`*/Tests`), not assumed from plan context |
| 7 | **Gate thresholds** | `plan-review-SKILL.md` | every pass/fail threshold is explicit — `dotnet test -> 12 passed`, never "~approximately passes" |
| 8 | **Severity assignment** | — | every finding tagged BLOCKING / WRITE-TIME / INFORMATIONAL |
| 9 | **Severity consistency** | — | no finding tagged BLOCKING may resolve as "minor" in the same review |
| 10 | **Coverage declaration** | — | audit output ends with an explicit list of what was NOT checked and why |

### Mandatory human-readable architecture audit add-on

When the audit touches architecture, trainable behavior, code review, documentation, or
service-boundary design, also run the Human-Readable Architecture Artifact Gate from
`human-readable-architecture-audit-SKILL.md`. Required evidence:

```text
Russian (or human-language) summary present:
PlantUML or Mermaid UML importable to draw.io:
data-flow simulations and expected branch outcomes present:
no-code algorithm explanation present:
exact code quote + adjacent concrete solution present:
schema-only / table-only / prompt-spec solution absent:
```

Generic rows such as "create schema", "chosen/rejected/quarantine", "wrap in a service", or
"make typed state" are not a solution.

### Review/Fix Pass Evidence add-on

If the audit or work order requires at least N review/fix passes, verify N distinct evidence
packets. A Review-Fix Ledger is only an index:

```text
requested_pass_floor:
real_pass_count:
evidence_packet_count:
ledger_row_count:
ledger_rows_are_evidence=false:
pass_1_packet_ref: ... pass_N_packet_ref:
```

Each packet must prove order (run after the previous fix/no-fix recheck) and include
before/after refs, reviewer id, loaded skills/gates, findings, fix or no-fix justification,
and recheck. N ledger rows with fewer than N valid packets => verdict
`IN_PROGRESS_NEXT_STEP_KNOWN`, not pass.

### Audit Sufficiency Boundary Gate

An audit must not claim broader completion than it checked. "v1 scope completed" is allowed
only with an explicit bounded scope. "Audit sufficient / complete / closed" is forbidden
unless the breadth challenge is green:

```text
service-boundary / contract leakage checked:
tenant-scope leakage checked:
DataProcessResult/never-throw compliance checked:
fabric-interface (Fabric-First) bypass checked:
FREEDOM-vs-hardcoded values checked:
No-Secrets-in-repo checked:
fallback-to-default / first-entry shortcuts checked:
bounded completion wording used:
```

Parent review must ask and answer: **"What would a broader audit likely find that this
artifact missed?"** — naming likely-missed zones, whether they were checked, and which
claims remain explicitly out-of-scope.

### Delta-Severity Feedback Gate

If the next audit in the same area finds BLOCKING/HIGH issues that should have been
discoverable, the previous audit has a retroactive review-skill failure (unless the missed
area was explicitly out-of-scope). Record:

```text
previous audit claim:
later BLOCKING/HIGH issue:
discoverable evidence path:
why previous review missed it:
skill/guide/index/agent rule patched:
new breadth-challenge row:
corrected completion wording:
```

Run governance repair before accepting or polishing the next audit artifact.

---

## Section 2 — Severity Definitions

| Tag | Meaning | Required Action |
|---|---|---|
| **BLOCKING** | Execution cannot proceed without fixing this | Stop, fix, re-audit before any further work |
| **WRITE-TIME** | Fix during execution, pre-approved | Document as a WF-N entry with a verification command |
| **INFORMATIONAL** | Noted, no action required | Log only, no gate impact |

### Severity Rules (non-negotiable)

1. Every finding has exactly ONE severity tag.
2. A BLOCKING finding that resolves as "minor" is a protocol violation — reclassify or escalate.
3. No finding may downgrade from BLOCKING to WRITE-TIME without explicit approval.
4. A plan with any BLOCKING finding cannot proceed; a matrix row stays `FAIL`/`UNKNOWN`.

---

## Section 3 — Audit Output Format

```markdown
## Audit Report — [Plan Name or Session ID]
Date: [date]   Auditor: [session]   Plan version: [N]

### Section A — Findings
| # | Point | Status | Severity | Finding |
|---|---|---|---|---|
| 1 | Chain arithmetic | PASS | — | baseline 40 + 5 = 45; final declared 45 |
| 2 | Blast radius | PASS | — | 4 deliverables tagged; INTERFACE change to IOutboxStore has rollback plan |
| 3 | API shape | FAIL | BLOCKING | service returns `Result<T>` in plan, but `IDocumentStore.UpsertAsync` returns `DataProcessResult<DocumentRecord>` — mismatch |
| 4 | Commit atomicity | PASS | — | GOVERNANCE commit before TEST-ONLY |
| 5 | WF verifiability | PASS | — | all write-time fixes have `dotnet test` / `grep` commands |
| 6 | Path verification | FAIL | WRITE-TIME | `src/Xiigen.Flows/...Service.cs` listed but not confirmed to exist |
| 7 | Gate thresholds | PASS | — | all Gate B conditions have exact `dotnet test -> N passed` counts |
| 8 | Severity assignment | PASS | — | 1 BLOCKING, 1 WRITE-TIME, 0 INFORMATIONAL |
| 9 | Severity consistency | PASS | — | no downgrade instances |
| 10 | Coverage declaration | see below | — | |

### Section B — NOT CHECKED (with reason)
| Point | What was not checked | Reason |
|---|---|---|
| 3 (partial) | constructor-injection test for the new service | test file does not exist yet |

### Final Gate
BLOCKING findings: 1 -> plan cannot proceed until the `DataProcessResult<T>` return-type mismatch (Point 3) is resolved.
WRITE-TIME findings: 1 -> WF-1: verify `src/.../Service.cs` exists (cmd: `ls src/.../Service.cs`).
Verdict: NOT READY — 1 BLOCKING finding requires resolution.
```

---

## Anti-Patterns

1. **"The audit found no issues"** — an audit with no findings either genuinely passed all
   10 points or skipped some. If points were skipped, declare them in NOT CHECKED.
2. **"This issue is minor but I tagged it BLOCKING"** — if minor, tag INFORMATIONAL.
   BLOCKING means execution stops; do not inflate severity.
3. **"The NOT CHECKED section is embarrassing so I skip it"** — omitting it implies all 10
   points were checked. Declare omissions.
4. **"Gate thresholds don't need counts — 'tests pass' is clear enough"** — Point 7 requires
   exact counts. A Gate B a third party cannot verify (`dotnet test -> N passed`) is not a Gate B.
5. **"BLOCKING finding resolved itself after I re-read"** — a retracted BLOCKING finding
   without a specific code change is a consistency violation. Either downgrade with reason or remain BLOCKING.

## Integration

- `plan-review-SKILL.md` Gate B invokes this audit protocol.
- `chain-arithmetic-audit-SKILL.md` governs Point 1; `blast-radius-tagger-SKILL.md` governs Points 2 and 4.
- `api-shape-verification-SKILL.md` governs Point 3 (`DataProcessResult<T>` vs raw return shapes).
- `human-readable-architecture-audit-SKILL.md` governs the human-readable, sufficiency-boundary, and delta-severity add-ons.
- The v3.3 `COMPLETION-MATRIX` is the source of truth this audit feeds: no completion while a required row is `UNKNOWN`/`FAIL`.
