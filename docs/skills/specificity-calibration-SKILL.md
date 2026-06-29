# Specificity Calibration

> Ported from the `llm_mvp_core` specificity standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build Xiigen.Platform.slnx` / `dotnet test`. `file:line` references
> point at `src/Xiigen.*/*.cs` and `tests/Xiigen.*.Tests/*.cs`; verbatim excerpts
> come from `.cs` files, `dotnet build` output (e.g. `CS0246`), or `dotnet test`
> output read this session. Thresholds vary by session type (ARCHITECT, REVIEW).
> Domain return type is `DataProcessResult<T>` (never core `OperationResult<T>`).

## Core Problem

An architect answer with no `file:line` references, no integer counts, and no
verbatim excerpts is an essay. It frames thinking but leaves the reader to do the
reconnaissance the architect already has access to. This skill makes concreteness
a mechanically scored property and rejects output below threshold before any STOP
gate.

---

## When to Invoke

- Before every STOP gate — self-score the output before presenting it.
- Before emitting any architect-level synthesis or plan review.
- Before handing any plan to `plan-review-SKILL.md` (its evidence base is checked).
- When `reconnaissance-gate-SKILL.md` wide-scope mode applies (doubles thresholds).

One self-score before STOP = zero "your answer is too abstract" correction cycles.

---

## Section 1 — What "Specific" Means

Three senses of specificity are often conflated:
1. **Word count** — more words. Not specificity; often the opposite.
2. **Narrow scope** — answering a smaller question. Orthogonal to specificity.
3. **Concreteness** — pointing at specific existing things. This is specificity.

This skill measures #3: the output points at a file at a path, a count with a
numerator and denominator, a verbatim string from somewhere.

**Abstract:** "the fabric stores appear to be partially wired"
**Concrete:** "2 of 3 fabric stores are registered in
`src/Xiigen.Platform.Api/Program.cs:11` via `AddXiigenInMemoryFabrics`;
`ISchedulerService` is not (grep: 0 hits for `AddXiigenInMemoryScheduler` in
Program.cs)"

---

## Section 2 — The Scoring Formula

```
Score = count(file:line refs) + count(N-of-M claims) + count(verbatim excerpts)
```

Each reference counts once regardless of how many times restated.

### Dimension 1 — file:line References

Counts as ONE:
- Exact path + line: `src/Xiigen.Kernel/FabricContracts.cs:9`
- Exact path + member: `Scheduler.cs::ScheduleDelayedAsync()`
- Exact path + range: `DataProcessResult.cs:26-50`

Does NOT count: directory only (`src/Xiigen.Kernel/`), category ("the fabric
contracts"), hypothetical path ("something like `QuotaStore.cs`").

### Dimension 2 — N-of-M Integer Claims

Counts as ONE:
- Concrete count with denominator: "2 of 3 fabric stores registered"
- Count with scope: "81 lines in DataProcessResult.cs"
- Before/after: "was 12 passed, is now 18 passed"
- Zero-result grep: "0 hits for `AddXiigenInMemoryScheduler` in Program.cs"

Does NOT count: "many" / "several" / "most"; "roughly half"; ratios without a base.

### Dimension 3 — Verbatim Quoted Excerpts

Counts as ONE:
- Direct quote from file content: `"return Failure<...>("PROJECT_ID_REQUIRED", ...);"`
- Direct quote from build output: `"CS0246: The type or namespace 'IQuotaStore' could not be found"`
- Exact symbol from a grep result: `"fromCache: true"`

Does NOT count: paraphrase, summary, generic code not from a specific source file.

---

## Section 3 — Thresholds per Session Type

| Session type | file:line | N-of-M | verbatim | Total minimum |
|-------------|----------:|-------:|---------:|--------------:|
| EXECUTOR turn output | 5 | 2 | 3 | 10 |
| ARCHITECT answer | 5 | 3 | 3 | 11 |
| PLAN-REVIEW report | 10 | 5 | 5 | 20 |
| DESIGN-REVIEW output | 15 | 8 | 5 | 28 |

Total minimum is the primary gate. Each dimension also requires ≥1 reference (11
file:line refs and zero N-of-M is unbalanced — it passes total but fails
dimension diversity). **Wide-scope mode doubles all thresholds.**

---

## Section 4 — Self-Check Protocol

Before any STOP gate, run the count on the output being presented:

```markdown
## Specificity self-check (specificity-calibration-SKILL.md)

Output scored: [session output from this response]

file:line references: [N]
  [list each: "path:line — what this reference shows"]
N-of-M integer claims: [M]
  [list each: "N of M — what is being counted"]
verbatim quoted excerpts: [K]
  [list each: `"quoted string"` — source file or command output]

Total: [N + M + K]
Threshold for [session type]: [threshold]
Dimension diversity: [≥1 in each dimension? YES | NO — which is missing]

Verdict: PASS | FAIL — [reason if FAIL]
```

**If FAIL:** output rejected. Run additional reconnaissance, re-count, retry. Do
not present a FAIL output. **If PASS:** include the self-check block (or log it).

---

## Section 5 — Anti-Gaming Rules

1. **Same file:line multiple times = 1 reference.**
2. **Same count, multiple phrasings = 1 claim.**
3. **Verbatim requires exact characters.** Removing the quotes from
   `"fromCache: true"` breaks verbatim status. Copy exactly.
4. **File paths must be real paths** in this codebase. A hallucinated path is
   worse than no reference — verify by reading or globbing.
5. **Counts must be from reconnaissance, not memory.** Count from a Grep/Glob
   result this session, not "I recall there were about 3 stores".
6. **Verbatim excerpts must come from files read this session.** Quotes from
   training-data memory are plausible strings, not verbatim excerpts.
7. **Placeholder refs score 0.** `[file]:123` signals the session never looked.

---

## Section 6 — Worked Example: FAIL vs PASS

**FAIL:**
```
"The fabric layer appears to be partially wired. Some stores are missing from DI.
The session identified gaps in the registration."
```
Score: 0 + 0 + 0 = 0 of 11 for ARCHITECT. FAIL — labels abstractions, points at no artifact.

**PASS:**
```
"2 of 3 fabric stores are registered in src/Xiigen.Platform.Api/Program.cs:11
via `AddXiigenInMemoryFabrics`. ISchedulerService is NOT registered there
(grep: 0 hits for `AddXiigenInMemoryScheduler`). InMemorySchedulerService.cs:44
returns `DataProcessResult<ScheduledJobResult>.Success(existing, fromCache: true)`
for a repeated idempotency key. 0 hits for `ScheduleDelayedAsync` across
tests/Xiigen.Platform.Api.Tests/."
```
Score: 3 file:line + 3 N-of-M + 3 verbatim = 9 of 11. One more grounded reference → PASS.

---

## Integration

- `reconnaissance-gate-SKILL.md` produces the evidence base; this skill scores the output. Both are required in ARCHITECT sessions.
- `session-mode-declaration-SKILL.md` mode determines the threshold (ARCHITECT = 11, PLAN-REVIEW = 20).
- `coverage-completeness-gate-SKILL.md` fires at STOP; a specificity FAIL means coverage claims are unsupported.
- `claim-verification-SKILL.md` verdicts must themselves meet the threshold; abstract verdicts fail.
