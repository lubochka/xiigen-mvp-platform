# Output Skepticism (XIIGen Enterprise Platform)

## What This Skill Prevents

Reporting a result without asking whether the evidence actually supports the
claim. Claiming "tests pass" from a partial run. Claiming "fixed" when only one
test was checked. Claiming "improved" when the metric could improve while the
problem stays the same. Claiming "50 cycles" when only 50 ledger rows exist.
Claiming "sub-agent did it" when the parent did it.

Platform port. Stack-bound to **.NET 10 ASP.NET Core Web API**, **xUnit**
(`tests/`), `Xiigen.Platform.slnx`, and the v3.3 governance triple.

## When to Invoke

Any response that contains a result claim. Trigger phrases:
- improvement / reduced / fixed / resolved / addressed
- percentage changes or comparisons
- completion: done / complete / finished / passes
- progress: improving / significant progress / major improvement
- count: N cycles / N reviews / N passes / N files / N checks
- actor: sub-agent did it / reviewer did it / tests ran / probe checked

If the response contains none of these: "No result claims — skepticism N/A."

## The Three Skeptic Questions

Answer all three before sending. Each answer is a specific statement with
evidence, not a reassurance. "I believe so" is not an answer.

### Question 1 — Refutation Evidence

"What observable evidence would prove this result wrong? Have I looked for it?"

For test pass claims:
```
Refutation evidence:
  Evidence that would refute: dotnet test shows Failed > 0
  Did I look: YES — [paste the last lines of dotnet test output including Failed: 0]
  OR: NO — I did not run dotnet test in this message -> cannot claim tests pass
```

For bug fix claims:
```
Refutation evidence:
  Evidence that would refute: the original symptom still reproduces
  Did I look: YES — ran the specific xUnit test that exercises the symptom: [test name]
              Output: [paste result]
  OR: NO — downgrade to "fix implemented, not verified"
```

For standards-compliance claims:
```
Refutation evidence:
  Evidence that would refute: grep finds `throw new` on a business path, or a
    canned DataProcessResult.Success() that skips work
  Did I look: YES — ran grep on the changed files -> 0 violations
  OR: NO -> cannot claim standards compliance
```

### Question 2 — Scope Validity

"Does the evidence I have cover the scope I am claiming?"

```
Scope validity:
  Scope claimed: [one file / one class / one project / full solution]
  Evidence scope: [filter used, files read]
  Coverage sufficient: YES / NO
  Count claimed: [N or N/A]
  Independent evidence records: [N or lower actual count]
  Actor claimed: [parent / sub-agent / probe / unknown]
  Actor evidence: [sub-agent id, command output, file diff, or NONE]
  If NO: downgrade claim to match actual evidence scope
```

Examples:
```
"Tests pass" without --filter on the slnx -> scope: full solution -> PASS
"Scheduler tests pass" with --filter ~Scheduler -> scope: one area -> PASS
"All tests pass" with --filter ~Scheduler -> scope mismatch -> FAIL -> revise to "Scheduler tests pass"
```

### Question 3 — Proxy Check

"Am I measuring a proxy that could improve without the underlying thing improving?"

Common proxies on this platform:

| Proxy metric | Underlying thing | Can proxy improve without underlying? |
|-------------|-----------------|--------------------------------------|
| `dotnet build`: 0 errors | Tests pass | YES — build checks compilation, not behavior |
| `grep`: 0 violations | Code is correct | YES — grep checks patterns, not logic |
| Single test pass | All related tests pass | YES — test isolation may hide regressions |
| Test count increased | Test quality improved | YES — tests may be shallow assertions |
| `DataProcessResult.IsSuccess == true` | The work actually happened | YES — a canned `Success()` returns true without doing the work |
| Ledger row count | Review/fix cycles completed | YES — rows can be generated without independent passes |
| Parent self-review | Sub-agent review completed | YES — actor is different; independence not proven |
| `passed: true` in a JSON report | Runtime capability works end-to-end | YES — report may be static/contract/mixed/validation-only |
| Endpoint mapped | Endpoint behaves correctly | YES — a mapped route can still return a stub |

```
Proxy check:
  Metric measured: [command run + what it showed]
  Underlying goal: [what was supposed to change]
  Are they the same: YES / NO — [one sentence connecting them]
  If NO: state explicitly what was and was not verified
```

For end-to-end / persistence / fabric claims, the proxy check must also answer:
```text
Does the evidence include runtime execution (e.g. WebApplicationFactory /
  integration test), not only generated reports?
Would the current code reproduce the persisted report?
Does it reject validation_only: true?
Does it reject static/contract/mixed metric types for runtime claims?
Does a DataProcessResult success come from real data, not a canned Success()?
Does persistence prove the row is observable (IOutboxStore.ListAsync /
  IDocumentStore.GetAsync), not just that AppendAsync returned success?
```

## Output Skepticism Format (Before Reporting)

```
OUTPUT SKEPTICISM CHECK:
Q1 — Refutation evidence: [what would refute + whether I looked + exact evidence]
Q2 — Scope validity: [scope claimed] vs [scope of evidence] -> PASS / FAIL
Q3 — Proxy check: [metric] -> [goal] — same / different; if different, state what is unverified
Overall: ALL PASS -> report with evidence; ANY FAIL -> revise claim first
```

For count or sub-agent claims, add:
```text
COUNT / ACTOR HONESTY CHECK:
  count_claimed:
  independent_evidence_count:
  ledger_rows_or_summary_only:
  subagent_id_or_NONE:
  allowed_wording:
```
If `independent_evidence_count < count_claimed`, or `subagent_id_or_NONE=NONE`
for a sub-agent claim, say the narrow truth before anything else.

## Worked Example — FAIL -> Revised

Claim: "The platform is now bug-free."
```
Q1: ran dotnet test --filter ~SchedulerTests; did not run the full slnx -> FAIL (scope)
Q2: scope claimed = whole platform; evidence = Scheduler only -> FAIL
Overall: FAIL -> revised:
  "Scheduler tests pass. Full solution not run in this message. Cannot claim
   platform-wide status without dotnet test Xiigen.Platform.slnx."
```

## Anti-Patterns

1. **"I answered the questions in my head"** — answers must appear in the response.
2. **Listing the questions without answering** — each needs a specific, falsifiable answer.
3. **"I believe the fix is correct"** — belief is not evidence.
4. **"The metric and goal are related"** — relatedness is not sufficiency.
5. **Skipping because "I just ran the command"** — if the output is not in the message, it cannot be verified.
