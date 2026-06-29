# Reconnaissance Gate

> Ported from the `llm_mvp_core` reconnaissance standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build Xiigen.Platform.slnx` / `dotnet test`. Tier-0 reads are the
> platform anchors (`AGENTS.md`, `CLAUDE.md` Beacon First, the v3.3
> Beacon/state/matrix, `docs/HOW-TO-USE-SKILLS.md`). IMPLEMENTATION evidence is
> `src/Xiigen.*/*.cs`; TEST evidence is `tests/Xiigen.*.Tests/*.cs` (xUnit).
> Domain return type is `DataProcessResult<T>` (never core `OperationResult<T>`).

## Core Problem

A plan, review, or architect-level answer produced before the session read the
files is an essay, not an analysis. Without file reads, grep counts, and verbatim
excerpts from the actual code, synthesis claims are memory-retrieval dressed as
evidence. This skill installs the mandatory evidence threshold that must be met
before any synthesis output is produced.

---

## Subagent Firewall Sentinels

Preserve these exact proof strings from `AGENTS.md` as authority:
- `Parent Architect Subagent-Only Firewall`
- `parent local sidecar work ban`
- `read-only verification after a sub-agent packet returns`
- `sub-agent waiting is not a stop`
- `no local non-overlapping work escape`

---

## When to Invoke

- At session start, BEFORE any plan output or architectural synthesis.
- Before any plan-review verdict (`plan-review-SKILL.md`).
- BEFORE writing any v3.3 session plan or execution document.
- When reviewing a completion claim against actual codebase / matrix state.

---

## Section 1 — Thresholds per Session Type

No synthesis output may be produced until the reconnaissance threshold is met AND
the RECON REPORT is written.

| Session type | File reads | Grep counts | Verbatim excerpts |
|---|---|---|---|
| EXECUTOR | 5 | 2 | 3 |
| PLANNING | 10 | 3 | 5 |
| REVIEW | 15 | 5 | 8 |
| ARCHITECT | 20 | 8 | 10 |

**Wide-scope mode** (Luba says "see the whole picture" / "load the real state" /
uploads ≥5 files): double all thresholds.

**Counts are orthogonal.** Meeting the file-read count without meeting the grep
count fails the threshold. Each dimension catches a different failure class:
- File reads catch "that file doesn't exist at that path".
- Grep counts catch "how many of X are there and how many are populated".
- Verbatim excerpts catch "the code actually says Y, not what I remembered".

---

## Section 2 — Evidence Layer Tags

Every verbatim excerpt in the RECON REPORT carries one of four evidence-layer
tags. A synthesis claim that asserts something "works" or "exists as implemented"
must trace to at least one IMPLEMENTATION or TEST excerpt.

```
DESIGN_DOC     File describes intended behavior — not evidence it exists in code.
               Includes: AGENTS.md, CLAUDE.md, docs/, v3.3 *-CONTRACT.md,
               SESSION-XX files, skill files, the Beacon.
               A "works" claim backed only by DESIGN_DOC is NOT verified.

IMPLEMENTATION File is source code — evidence of what the platform actually does.
               Includes: src/Xiigen.*/*.cs, *.csproj, Program.cs, appsettings*.json.
               A "works" claim requires at least one IMPLEMENTATION excerpt.

TEST           File is a test — evidence of what has been verified.
               Includes: tests/Xiigen.*.Tests/*.cs (xUnit [Fact]/[Theory]).
               A "passes" claim requires at least one TEST excerpt.

RECONCILIATION File is a state record that may be stale.
               Includes: XIIGEN-ENTERPRISE-ADAPTATION-STATE-v3.3.json,
               COMPLETION-MATRIX-v3.3.md, session-state files.
               RECONCILIATION excerpts are hypotheses until confirmed by IMPLEMENTATION.
```

---

## Section 3 — RECON REPORT Template

Before first synthesis output, produce a RECON REPORT:

```markdown
# RECON REPORT — Session [date/ID]

Session type: [EXECUTOR | PLANNING | REVIEW | ARCHITECT]
Wide-scope: [YES | NO]
Threshold: [N] file reads / [M] grep counts / [K] excerpts
Actual: [n] file reads / [m] grep counts / [k] excerpts
Threshold met: [PASS | FAIL — short in dimension X]

## Domain Universe (ARCHITECT sessions only)
Entity type examined: [projects | services | fabric contracts | tests | other]
Total universe (N): [integer] — source: [command that established this count]
Examined this session: [n]
Coverage fraction: [n/N = X%]
Unread and material:
- [entity] — [why it could change a synthesis claim if read]
(If all material entities examined: "none — all material entities examined.")

## File Inventory
- [filepath]: [line count] lines — [one-line summary]

## Grep Findings
- Pattern "[pattern]" in [scope]: [N] hits
  - [file:line] [excerpt]

## Verbatim Excerpts
- [filepath:line]: "[exact text]"
  Evidence layer: [DESIGN_DOC | IMPLEMENTATION | TEST | RECONCILIATION]

## Hypotheses (N≥5 entity examination only)
H1: [claim] — Confirms if: [observable] — Refutes if: [observable]
(declare ALL hypotheses before examining instance 1)
```

---

## Section 4 — Gate Enforcement

### Before first synthesis output
```
□ RECON REPORT exists
□ RECON REPORT meets threshold (all three dimensions)
□ Synthesis output cites RECON REPORT entries
```

### Before any "works" or "exists" claim
```
□ At least one IMPLEMENTATION or TEST excerpt backs the claim
□ If only DESIGN_DOC evidence exists: reword to assert intent, not existence
  ("was designed to do X" not "does X")
```

### For ARCHITECT sessions: before system-level claims
```
□ Domain universe block present (coverage fraction declared)
□ Universal quantifiers ("no project", "every contract", "all tests") require
  either ≥50% coverage in the domain universe, or an explicit scope qualifier:
  "among the [n] [entities] examined..."
```

### For multi-entity sessions (N≥5 instances): before examining instance 1
```
□ Hypotheses declared with Confirms-if / Refutes-if
□ per-entity-examination-SKILL.md activated
```

---

## Section 5 — Platform Tier-0 Reads

At the start of every ARCHITECT session in this repository, read these files
before any other. Tier-0 reads count toward the Section 1 threshold.

```
1. AGENTS.md                  — binding Beacon, role loop, executor-cannot-stop
2. CLAUDE.md                  — Beacon First, soft-stop ban, enterprise scope
3. docs/plans/enterprise-adaptation/v3.3/XIIGEN-ENTERPRISE-ADAPTATION-BEACON-PROMPT-v1.md
4. docs/plans/enterprise-adaptation/v3.3/XIIGEN-ENTERPRISE-ADAPTATION-STATE-v3.3.json
5. docs/plans/enterprise-adaptation/v3.3/XIIGEN-ENTERPRISE-ADAPTATION-COMPLETION-MATRIX-v3.3.md
```

Plus `docs/HOW-TO-USE-SKILLS.md` for the skills catalogue. Suggested grep counts:
`public.*DataProcessResult` across `src/`, and `interface I` across
`src/Xiigen.Kernel/`. Tier-0 covers governance anchors; capability-specific reads
follow it.

---

## Section 5A — High-Risk Architecture Recon Library Gate

For high-risk architecture plans, cross-project designs (platform ↔ flows ↔
core), source-corpus learning plans, or any plan where a wrong foundation causes
broad downstream rework, the normal RECON REPORT is only the startup layer.
Before architecture plan prose, build a Knowledge Library from quantified
source-corpus passes.

A focused pass means one bounded question, named roots, exact files/line ranges,
and a decision the pass can support or reject. Generic source maps, folder
summaries, or "read many docs" claims do not satisfy this gate.

Knowledge Library artifact schema:

```text
path:
type:
summary:
why_to_use:
when_to_use:
relevance_to_target_plan:
document_locations:
implementation_locations:
evidence_line_range:
decision_supported:
source_disjoint_evidence: independent_roots | duplicate_derived | single_source
confidence_effect:
held_out_roots_or_sources:
status: accepted | rejected | deferred | evidence_only
coverage_critic:
```

Population proof is required when the task names a library, folder, corpus, or
artifact as evidence: exact path, file list or index, non-empty proof, cutoff
snapshot, late-file queue. Broad folder grep or row presence alone is not
completion evidence. If the floor is not met, the output may be a recon plan or
blocker, not architecture plan prose.

> Note: cross-repo common-model/ML internals (ILearningMachine / DPO / trainable
> registry) remain owned by `llm_mvp_core` (R5/R6). Platform consumes them via
> the `TRAINABLE-UNIT-MANIFEST-CONTRACT.md` / `.xiigen` manifests — recon them as
> third-party-derived evidence, do not re-implement them here.

---

## Anti-Patterns

1. **"I already know this codebase"** — state changes between sessions; the RECON REPORT is a current-session snapshot.
2. **"Project knowledge gives me the counts"** — the report requires actual reads and grep output, not recalled summaries.
3. **"DESIGN_DOC evidence proves implementation"** — a v3.3 contract describes intent; a `.cs` file shows what exists.
4. **"Threshold met means the picture is complete"** — threshold confirms minimum evidence; coverage fraction is declared separately.

---

## Integration

- `per-entity-examination-SKILL.md` activates when the domain universe shows N≥5 unread material entities.
- `planning-session-startup-SKILL.md` Step 0 (Beacon First) populates the Tier-0 reads.
- `claim-verification-SKILL.md`: user claims about existing state require IMPLEMENTATION verification; those actions count toward the threshold.
- `code-examination-SKILL.md`: single-file 5-step protocol; reconnaissance applies at session level.
- `specificity-calibration-SKILL.md` scores the synthesis output the RECON REPORT feeds.
