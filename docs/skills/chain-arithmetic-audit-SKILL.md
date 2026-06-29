# Chain Arithmetic Audit (XIIGen Enterprise Platform)

Verifies that `baseline + Σ(phase deltas) = final` for every tracked metric
across all sessions and phases. `claim-verification-SKILL.md` checks
single-document consistency; this skill checks end-to-end chain math.

Platform port (an extension of the count-drift / FC-1 check — NOT an AI pipeline
engine). Stack-bound to **.NET 10 ASP.NET Core Web API**, **xUnit** (`tests/`),
`Xiigen.Platform.slnx`, and the v3.3 governance triple. The authoritative number
source is `dotnet test` output; the cross-check source is `STATE-v3.3.json` and
`COMPLETION-MATRIX-v3.3`.

## When to Invoke

- At session end when writing a summary — verify "Before" equals the previous
  session's "After".
- Any time a plan spans multiple sessions or phases with incremental counts.
- Whenever a test count or capability metric has been reported across sessions.
- Before claiming "N tests passing" — verify the number chains back to baseline.

## Rules

1. Extract all count references from `STATE-v3.3.json`, the
   `COMPLETION-MATRIX-v3.3`, and every `SESSION-XX` file.
2. Build a directed chain: `baseline -> phase1 delta -> phase2 delta -> ... -> final`.
3. Verify every edge: `node[n].after == node[n+1].before`.
4. Verify the terminal: `baseline + Σ(all deltas) == final_expected`.
5. Any gap or mismatch = **CHAIN ARITHMETIC FAIL** with exact location and value.

Every claimed increase/decrease must have a numeric source (a `dotnet test`
run); the final total is not estimated "by eye".

## Detection Commands

```bash
# Authoritative current test count to anchor the chain
dotnet test Xiigen.Platform.slnx 2>&1 | tail -3
# This is the authoritative "after" for the current session

# Cross-check against what the state file records
grep -n "testCount\|passed\|baseline\|before\|after\|delta" \
  docs/plans/enterprise-adaptation/v3.3/*STATE*.json | head -20
```

(Run the broad `dotnet test` only when the active SESSION-XX file opens that
gate; otherwise anchor on the session-allowed filtered count.)

## Verification Table Format

| Transition | Before | Delta | After | Verified |
|-----------|--------|-------|-------|---------|
| Start of phase 4 | 120 | +6 | 126 | ✅ |
| Phase 4 -> phase 5 | 126 | +5 | 131 | ✅ |
| Phase 5 -> current | 131 | +N | 131+N | run dotnet test |

Every row's After must equal the next row's Before.

## STATE Chain Fields

Record these in the state file at session end:

```json
{
  "testChain": {
    "sessionStart": 126,
    "sessionEnd": 131,
    "delta": 5,
    "previousSessionEnd": 126,
    "chainVerified": true
  }
}
```

If `sessionStart != previousSessionEnd` -> chain is broken -> investigate before
claiming test counts.

## Anti-Pattern

Plan said `expected_test_count: 140` but phase deltas summed to 138.
`claim-verification-SKILL.md` passed (each document was internally consistent).
Chain arithmetic failed because one phase's "after" was recorded as 132 but the
next phase started at 134. This skill catches what single-document verification
misses: the edges between documents.

## Integration

- Extends the count-drift check in the platform plan-review path.
- Invoked at session start (verify the test baseline matches the previous
  session's end count).
- Results feed the session-end "zero-regression delta" summary and any
  `COMPLETION-MATRIX-v3.3` row that tracks a count.
