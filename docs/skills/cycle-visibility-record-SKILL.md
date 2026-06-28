# Cycle Visibility Record (XIIGen Enterprise Platform)

## Scope of this port

Platform port of the `llm_mvp_core` skill, restricted to its **portable core**.

The core skill's full 4-field form — SENT (context package) / RECEIVED (A/B/C
shuffled model candidates) / DECIDED (arbiter verdict) / CHANGED
(DomainModelRegistry / training-index) — is **multi-model-arbiter ML machinery**
and is **NOT ported** here (ML stays in `llm_mvp_core`; this repository has no
multi-model pipeline). What is ported is the universal anti-stop honesty core:

1. **count honesty** — N ledger rows != N cycles;
2. **"a cycle with no visibility record is a cycle that never ran"**;
3. every review/fix cycle leaves an inspectable before/after record with a
   **non-empty result/delta**.

Main value for the platform: close the lazejka "N cycles" in ledger-style
counting, without importing the ML arbiter structure. Stack-bound to **.NET 10
ASP.NET Core Web API**, **xUnit** (`tests/`), the domain wrapper
`DataProcessResult` (`Xiigen.Kernel`), and the v3.3 role loop
`PLAN_AUTHOR -> PLAN_REVIEW -> ARCHITECT -> EXECUTOR -> REVIEW -> ...`.

## Section 1 — The Governing Rule

```
A cycle with no visibility record is a cycle that never ran.
Its output cannot be trusted and its learning cannot be captured.
```

**Count honesty rule:** a row in a ledger, checklist, generated table, or summary
is not a cycle. "50 cycles completed" / "10 review passes completed" is valid
only when there are that many independently inspectable before/after review/fix
records. If they do not exist, the honest claim is "50 ledger rows exist; 50
cycles are not proven."

This binds to the v3.3 review loop: every `REVIEW_INTERNAL` /
`ARCHITECT_INTERNAL_REPAIR` pass that changes (or consciously does not change) an
artifact must leave one record.

## Section 2 — Platform Review/Fix Cycle Record (4 fields, non-ML)

Each review/fix cycle produces one record with exactly these fields:

```text
CYCLE_ID        — stable id for this review/fix pass
WHAT_WAS_SENT   — the exact artifact/section/slice submitted for review
                  (file path + section/range), not "the plan"
VERDICT         — the reviewer verdict + the criterion it referenced
                  (e.g. "NEEDS_FIX — endpoint returns canned success, violates
                   implementation-integrity Check 5")
WHAT_CHANGED    — what the fix changed (file:line / matrix row), OR an explicit
                  "Nothing changed — <reason>". An empty WHAT_CHANGED is a failed
                  record; silence cannot distinguish "no change" from "forgot the
                  field".
```

`VERDICT` reasoning must cite a concrete platform criterion: a coding standard
(`DataProcessResult` discipline, DI-not-`new`), a Gate-B (`dotnet test` filter +
count), or a `COMPLETION-MATRIX-v3.3` row — not "looks good".

## Section 3 — C# Record (platform shape)

```csharp
namespace Xiigen.Platform.Governance;

public sealed record ReviewFixCycleRecord
{
    public required string CycleId { get; init; }
    public required string WhatWasSent { get; init; }   // file path + section/range
    public required string Verdict { get; init; }        // verdict + criterion cited
    public required string WhatChanged { get; init; }    // file:line / matrix row, OR "Nothing changed — <reason>"
    public DateTimeOffset CapturedAt { get; init; } = DateTimeOffset.UtcNow;
}

public static class ReviewFixCycleValidator
{
    public static DataProcessResult Validate(ReviewFixCycleRecord r)
    {
        if (string.IsNullOrWhiteSpace(r.WhatWasSent))
            return DataProcessResult.Failure("CYCLE_SENT_MISSING", "WhatWasSent is required");

        if (string.IsNullOrWhiteSpace(r.Verdict))
            return DataProcessResult.Failure("CYCLE_VERDICT_MISSING", "Verdict with criterion is required");

        if (string.IsNullOrWhiteSpace(r.WhatChanged))
            return DataProcessResult.Failure(
                "CYCLE_CHANGED_MISSING",
                "WhatChanged is required (use 'Nothing changed — <reason>' if no change)");

        return DataProcessResult.Success();
    }
}
```

Persist records through a platform service (the same way other state is
persisted) and surface them as numbered records `1..N`, not as a single ledger
table that claims N cycles.

## Section 4 — Count-Honesty Gate (use before any "N cycles" claim)

```text
COUNT HONESTY CHECK:
  count_claimed:                <N>
  inspectable_records_present:  <number of ReviewFixCycleRecord with non-empty WhatChanged>
  ledger_rows_only:             <true/false>
If inspectable_records_present < count_claimed -> say the narrow truth first:
  "<inspectable_records_present> review/fix cycles proven; <N> claimed."
```

## Anti-Patterns

1. **"VERDICT = pass, no criterion"** — a verdict without a cited platform
   criterion cannot be audited.
2. **"WhatChanged empty when nothing changed"** — empty differs from "Nothing
   changed — <reason>"; the field must contain one or the other.
3. **"The ledger table is the proof"** — a table of N rows is not N cycles unless
   each row links to a real before/after record.
4. **Porting the ML arbiter form (A/B/C candidates, DomainModelRegistry)** — out
   of scope here; that machinery lives in `llm_mvp_core`.
