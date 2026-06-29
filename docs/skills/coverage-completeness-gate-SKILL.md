# Coverage Completeness Gate (XIIGen Enterprise Platform)

A session that improves 3 projects and claims "the platform pipeline is complete"
has not lied — 3 projects did improve. It has overclaimed — the pipeline result
does not follow from 3 data points if the pipeline has 7 projects. This skill
gates improvement claims before every STOP so the evidence scope never falls
short of the claimed scope.

Platform port. Stack-bound to **.NET 10 ASP.NET Core Web API**, **xUnit**
(`tests/`), `Xiigen.Platform.slnx`, and the v3.3 `COMPLETION-MATRIX-v3.3` as the
source of "what counts as complete".

## When to Invoke

Fires before every STOP that contains a claim of improvement, completion, or
progress. Trigger phrases (any activates the gate):
- "improved", "fixed", "resolved", "complete", "done"
- percentages: "-X%", "X% reduction", "X% improvement"
- completion: "all tests pass", "all projects updated", "pipeline complete"
- progress: "significantly improved", "major progress"

One gate check before STOP = no overclaimed scope in the session output.

## Section 1 — The Coverage Gate Table

| Claim type | Required coverage before claim is valid |
|------------|----------------------------------------|
| "This project improved" | All affected classes in that project tested (xUnit) |
| "These N projects improved" | All N projects have passing `dotnet test` deltas |
| "Pipeline is improving" | >= 2 of N pipeline components have test evidence |
| "Pipeline is complete" | All N components pass; `dotnet test Xiigen.Platform.slnx` -> 0 failed; matrix rows green |
| "This interface is complete" | Implementation class + all integration tests pass |
| "X% improvement in [metric]" | Metric is from `dotnet test` output, not estimated |

The platform "subproject" unit is a `src/Xiigen.*` project (e.g.
`Xiigen.Kernel`, `Xiigen.Fabrics.Scheduler`, `Xiigen.Platform.Api`,
`Xiigen.Platform.Marketplace`, `Xiigen.Auth`, `Xiigen.Freedom`).

## Section 2 — Coverage Check Before STOP

```
COVERAGE CHECK (coverage-completeness-gate):

Claim detected: "[exact quote]"
Claim type: [single project | N projects | pipeline | interface | metric]

Coverage required: [per table in Section 1]
Coverage actual:
  This session touched: [list projects or classes]
  Each fully tested (dotnet test evidence): [YES/NO per item]
  Test count delta: [+N from baseline]

Coverage verdict:
  SUFFICIENT — claim matches evidence scope -> STOP may fire
  INSUFFICIENT — [actual] < [required] -> revise claim (Section 3)
```

If INSUFFICIENT: do not suppress the response. Downgrade the claim.

## Section 3 — Claim Downgrade Protocol

The work is preserved; the claim is revised to match evidence.

| Original claim | Coverage gap | Downgraded claim |
|---|---|---|
| "Platform pipeline is complete" | 3 of 7 projects tested | "Kernel, Scheduler, Auth pass. Marketplace, Freedom, Platform.Api, InMemory not yet verified." |
| "All integration tests pass" | 2 of 5 scenarios tested | "Scheduler + Outbox integration pass. Idempotency + DocumentStore + TenantContext: not yet tested." |
| "50% improvement in throughput" | Counted grep matches, not test assertions | "50% reduction in failing assertion count in SchedulerTests. End-to-end throughput unverified." |
| "Phase 6 complete" | Matrix not reconciled | "Phase 6 code complete. COMPLETION-MATRIX-v3.3 rows not reconciled — reconcile before claiming done." |

**Rule:** a downgraded claim is not a failure; it is an honest report. A session
that correctly identifies its own coverage limit is more trustworthy than one
that ships an unfalsifiable result.

## Section 4 — Threshold Reasoning

- **Why 100% for "this project improved":** a `src/Xiigen.*` project is a small
  scope (one assembly). If `Xiigen.Kernel` was not tested, its behavior is
  unknown. No partial "improved" for a specific class.
- **Why `dotnet test` output for "X% improvement":** internal code change counts
  (lines, renames) do not measure behavioral improvement; only `dotnet test`
  delta from baseline counts.
- **Why the pipeline claim requires component evidence:** claiming the pipeline
  improved without testing each component is claiming a train arrived on time
  because the departure station looked busy.

## Section 5 — Integration With Other Skills

- `output-skepticism-SKILL.md` Q2 (scope validity) is the upstream version of
  this gate. If it ran correctly, this gate has little to catch.
- `verification-before-completion-SKILL.md` gates FRESHNESS of evidence; this
  skill gates SCOPE. Both must pass before STOP fires.
- `chain-arithmetic-audit-SKILL.md` provides the numeric `dotnet test` deltas
  this gate reads for improvement claims.

## Section 6 — Anti-Patterns

1. **"I improved what I worked on — that's valid"** — valid AT THE SCOPE worked
   on. The gate catches scope inflation: pipeline claim from component-level work.
2. **"Downgrading makes the session look bad"** — the opposite; explicit
   coverage limits are more trustworthy.
3. **"The gate is too strict for short sessions"** — short sessions make
   short-scope claims; the gate is proportional to the claim.
4. **"I'll note the caveat in the notes section"** — a caveat under an
   overclaimed headline is still an overclaimed headline; downgrade the claim.
