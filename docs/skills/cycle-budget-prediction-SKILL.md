# Cycle Budget Prediction (XIIGen Enterprise Platform)

## Core Problem

A phase plan that under-estimates cycles runs out of budget mid-implementation
and ships a partial result. A phase plan that over-estimates wastes allocation
and delays downstream phases. Both errors compound across batches because the
retrospective uses the predicted budget to compute MAE (Mean Absolute Error —
the average gap between predicted and actual cycles). This skill produces a
deterministic, auditable cycle budget before any implementation begins.

Platform port. Stack-bound to **.NET 10 ASP.NET Core Web API**, **xUnit**
(`tests/`), `DataProcessResult<T>`, the platform fabric interfaces, and the v3.3
governance triple. A "cycle" is one complete implementation attempt with a
Gate-B result (`dotnet build`/`dotnet test`).

## Section 1 — Cycle Budget Formula

```
cycleBudget = archetypeBaseline
            + firstOccurrencePenalty
            + noveltySum
            − clarityDiscount
            + inversionBonus
```

| Component | Value | When Applied |
|---|---|---|
| `archetypeBaseline` | 1 | Always — every capability class starts here |
| `firstOccurrencePenalty` | +1 | This capability class has never been built in this repo before |
| `noveltySum` | +0.5 per new pattern | Each new fabric interface, behavioral invariant, or DI pattern introduced for the first time |
| `clarityDiscount` | −1 | Implementable correctly from existing rules alone — no novel judgment |
| `inversionBonus` | +1 | Correct behavior inverts a platform default (rare — e.g. a boundary that must NOT return `DataProcessResult<T>` where one is normally required) |

**Maximum budget: 4** (all penalties, zero discount). **Minimum budget: 1**
(clarity discount, no penalties).

## Section 2 — Component Derivation Rules (platform)

### archetypeBaseline = 1
Every capability class gets one cycle regardless of complexity. A cycle is one
complete implementation attempt with a Gate-B result.

### firstOccurrencePenalty (+1)
Add when:
- the capability class does not appear yet in `src/Xiigen.*` (status STUB or MISSING);
- no prior phase in the current project has built this class before.

Do NOT add when a prior phase built the same class (even if this phase adds new
methods) or the capability is being extended, not created.

### noveltySum (+0.5 per new named pattern)
A "new named pattern" is a specific, named design artifact that does not yet
exist. Count each if new:
- new fabric interface (e.g. `INewFabric`) — not yet in `src/`;
- new behavioral invariant (an iron rule not yet recorded in `docs/`);
- new DI registration pattern (different from all existing `Program.cs`/module registrations);
- new cross-project `DataProcessResult` boundary (e.g. a new `Xiigen.*` project consuming `Xiigen.Kernel`).

Count 0.5 per item; round total to nearest 0.5.

### clarityDiscount (−1)
Subtract 1 when ALL are true:
1. every method signature is already defined in a verified interface;
2. the algorithm is derivable from existing rules without novel judgment;
3. no new behavioral invariants are required;
4. at least one structurally identical capability already exists (e.g. another
   `ISchedulerService`-shaped fabric with the same `DataProcessResult` pattern).

If any condition is false: no discount.

### inversionBonus (+1)
Add only when correct behavior requires explicitly overriding a platform rule at
a specific boundary. Rare. Document the override in `docs/` before adding it.

## Section 3 — Budget Card Format

```markdown
## Cycle Budget — [CapabilityClass]
archetypeBaseline:        1
firstOccurrencePenalty:   +N  [reason]
noveltySum:               +N  [items: INewFabric (+0.5), new invariant (+0.5), ...]
clarityDiscount:          −N  [reason or "not applicable"]
inversionBonus:           +N  [reason or "not applicable"]

Predicted cycle budget:   N

Gate B for this budget:
  "Phase [X] completes in <= N cycles with >= 1 passing xUnit [Fact] per cycle:
   dotnet test --filter \"FullyQualifiedName~[X]\" -> Failed: 0"
```

## Section 4 — Budget-to-Phase Wiring

```markdown
Phase [N] — [CapabilityClass]
Cycle budget: [predicted] cycles (cycle-budget-prediction-SKILL.md)
Gate B: dotnet test --filter "FullyQualifiedName~[X]" -> M passed, Failed: 0
Retrospective feed: actual vs predicted recorded in STATE-v3.3.json
```

## Section 5 — MAE Recalibration Trigger

If the retrospective shows MAE > 1.0 for the last batch:
1. list all phases where `|predicted − actual| > 1`;
2. identify which component was wrong (baseline over-estimated? noveltySum missed a new pattern?);
3. adjust component weights for the next batch (record durable changes in `docs/`);
4. do NOT adjust the baseline below 1.

There is no "close enough": MAE > 1.0 is a hard recalibration trigger.

## Anti-Patterns

1. **"Simple capability, budget = 0"** — `archetypeBaseline` is always 1.
2. **"I'll set budget = 5 to be safe"** — budget caps at 4; a >4 capability must
   be split before budgeting.
3. **"No Gate B needed for this phase"** — the budget card requires a Gate-B
   (`dotnet test` filter + count), or the retrospective cannot measure it.
4. **"noveltySum = 0 because all interfaces exist"** — check behavioral
   invariants and DI patterns too; a new iron rule is a new named pattern.
5. **"MAE > 1.0 but predictions were close enough"** — no "close enough" for
   budget accuracy.
