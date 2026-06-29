# Temporal Behavior Design (XIIGen Enterprise Platform)

## Core Problem

A service that computes different outputs at different times relative to an anchor
event is not a SCHEDULED archetype (which runs once at a set time). It is a single
service whose scoring formula is parameterized by time-delta. Without explicit
design, two failure modes appear: one creates a separate handler per phase
(over-decomposition); another polls `DateTimeOffset.UtcNow` on each request
(non-reproducible). This skill defines the anchor → phase → weights → formula chain
that lets one handler produce correct output across all time phases.

Platform port of the core `temporal-behavior` skill, re-bound to
**.NET 10 ASP.NET Core Web API**, **xUnit**, `DataProcessResult<T>`, the real
scheduler fabric `ISchedulerService` (`src/Xiigen.Fabrics.Scheduler/Scheduler.cs`),
and the config fabric `IFreedomConfigService` (`src/Xiigen.Freedom/FreedomConfig.cs`).
Gate-B is `dotnet build Xiigen.Platform.slnx` + `dotnet test`.

> **Scheduler API note (verified).** The platform scheduler method is
> `ScheduleDelayedAsync(string action, TimeSpan delay, IReadOnlyDictionary<string,object?> payload, string idempotencyKey, CancellationToken)`.
> There is **no** `ScheduleAt(DateTimeOffset, ...)`. Schedule a phase transition by
> computing the delay from the anchor and passing it as `delay`. (See
> `api-shape-verification-SKILL.md`.)

## When to load

- Before designing any scoring/ranking service whose behavior changes with time
  distance from a reference event.
- When a handler reads the current time but has no reference to an anchor event
  timestamp.
- When separate handlers are created for "early" vs "approaching" vs "active" phases
  of the same domain concept.
- When phase threshold values are hardcoded instead of read from `IFreedomConfigService`.

---

## Section 1 — Identify the Anchor Event and Phases

```
Anchor event: the fixed reference point that time is measured from
  Examples:
    CapabilityPromoted.PromotedAt
    ScheduledJobResult.DueAt
    CapabilityIndexed.IndexedAt

Phases: distinct time ranges where behavior is qualitatively different
  Phase A: [creation] → [T minus N]:  baseline behavior
  Phase B: [T minus N] → [T minus M]: approach behavior
  Phase C: [T minus M] → [event]:     imminent behavior
  Phase D: [event] → [event end]:     active behavior
  Phase E: [event end] → [T plus K]:  decay behavior
  Phase F: [T plus K] → ∞:            archive behavior
```

Define only the phases where behavior is qualitatively different — not just
quantitatively (a weight change is not a phase change).

---

## Section 2 — Classify Phase Behavior

```
Phase A (baseline):
  MACHINE constants: formula structure (which factors combine)
  FREEDOM config:    Weighting:PhaseA:MatchWeight = 0.25 (IFreedomConfigService)
  Trigger: default when timeDelta > phase boundary threshold from config

Phase B (approach):
  MACHINE constants: same formula structure as Phase A
  FREEDOM config:    Weighting:PhaseB:CapacityBoostMultiplier = 1.5
                     Weighting:PhaseBThresholdDays = 7
  Trigger: ISchedulerService.ScheduleDelayedAsync with delay = (anchor - threshold) - now
           action: "ApproachPhaseActivated"

Phase C (imminent):
  MACHINE constants: urgency formula (NEW formula, not just different weights)
  FREEDOM config:    Weighting:PhaseC:ProximityWeight = 0.40
  Trigger: ISchedulerService.ScheduleDelayedAsync with delay = (anchor - 1 day) - now
```

**Key design rule:** phase transition thresholds are FREEDOM config (per
tenant/environment, via `IFreedomConfigService`). The formula structure per phase is
a MACHINE constant (cannot change without a code deployment).

---

## Section 3 — State Machine Design

```csharp
// One handler for ALL phases — parameterized by phase
public enum TemporalPhase
{
    Baseline,   // default — no trigger needed
    Approach,   // ISchedulerService triggers the transition
    Imminent,
    Active,
    Decay,
    Archive
}

public sealed record TemporalContext(
    DateTimeOffset AnchorTimestamp,   // from the event payload, NOT DateTimeOffset.UtcNow
    DateTimeOffset EvaluatedAt,       // current evaluation time
    TemporalPhase Phase,              // derived from timeDelta + FREEDOM config
    TemporalWeights Weights);         // from FREEDOM config for this phase
```

**Do NOT create a separate handler per phase.** One handler reads the anchor
timestamp, computes the delta, determines the phase from `IFreedomConfigService`
thresholds, and applies the phase-appropriate formula. Separate handlers per phase is
over-decomposition — the domain concept is the same entity at different times.

---

## Section 4 — Three Iron Rules for Temporal Services

```
IR-TEMPORAL-1: temporal_threshold_from_config
  Phase boundary thresholds are read from IFreedomConfigService.
  Hardcoding "7 days" / "24 hours" as a literal violates this rule.
  Violation symptom: integer literals adjacent to TimeSpan or timeDelta comparisons
    in non-test code.

IR-TEMPORAL-2: anchor_event_timestamp_present
  Time-delta uses the anchor event's timestamp from the event payload, not
  DateTimeOffset.UtcNow alone. Using only current time makes the service
  non-reproducible — two calls at different times produce different results for the
  same event.
  Violation symptom: ComputeAsync(string input) where input has no anchor timestamp;
    method uses only DateTimeOffset.UtcNow.

IR-TEMPORAL-3: phase_transition_uses_scheduler
  Phase transitions are scheduled via ISchedulerService.ScheduleDelayedAsync, not
  detected by polling DateTimeOffset.UtcNow on each request.
  Violation symptom: if (DateTimeOffset.UtcNow - anchor > threshold) inside the main
    execution path instead of in a scheduler registration.
```

---

## Section 5 — C# Pattern (platform-bound)

```csharp
public sealed class TemporalScoringService : MicroserviceBase<TemporalScoringService>, ITemporalScoringService
{
    private readonly IFreedomConfigService _freedom;
    private readonly ISchedulerService _scheduler;

    public TemporalScoringService(
        IFreedomConfigService freedom,
        ISchedulerService scheduler,
        ITenantContextAccessor tenant,
        ICorrelationContextAccessor correlation,
        ILogger<TemporalScoringService> logger)
        : base(new ServiceDescriptor("xiigen-temporal-scoring", "XIIGen Temporal Scoring", "PLATFORM"),
               tenant, correlation, logger)
    { _freedom = freedom; _scheduler = scheduler; }

    public async Task<DataProcessResult<TemporalScore>> ExecuteAsync(TemporalScoreRequest request, CancellationToken ct = default)
    {
        // IR-TEMPORAL-2: anchor timestamp must come from the payload
        var anchor = request.AnchorTimestamp;
        var delta = DateTimeOffset.UtcNow - anchor;

        // IR-TEMPORAL-1: thresholds from FREEDOM config
        var approachDays = (await _freedom.GetAsync("Weighting:PhaseBThresholdDays", ct)).Data?.Value as int? ?? 7;
        var imminentHours = (await _freedom.GetAsync("Weighting:PhaseCThresholdHours", ct)).Data?.Value as int? ?? 24;

        var phase = DeterminePhase(delta, approachDays, imminentHours);   // MACHINE constant
        var weights = await GetWeightsForPhaseAsync(phase, ct);
        var score = ApplyFormula(request.Factors, weights, phase);

        return Success(new TemporalScore(phase, score, weights, DateTimeOffset.UtcNow));
    }

    private static TemporalPhase DeterminePhase(TimeSpan delta, int approachDays, int imminentHours)
    {
        // MACHINE constant: phase determination logic
        if (delta.TotalDays > approachDays) return TemporalPhase.Baseline;
        if (delta.TotalHours > imminentHours) return TemporalPhase.Approach;
        if (delta.TotalHours > 0) return TemporalPhase.Imminent;
        if (delta.TotalDays > -1) return TemporalPhase.Active;
        return TemporalPhase.Decay;
    }
}

// Phase transitions registered via the scheduler — IR-TEMPORAL-3
public sealed class TemporalPhaseScheduler
{
    private readonly ISchedulerService _scheduler;
    private readonly IFreedomConfigService _freedom;

    public async Task<DataProcessResult<ScheduledJobResult>> ScheduleApproachAsync(
        DateTimeOffset anchor, string entityId, CancellationToken ct = default)
    {
        var approachDays = (await _freedom.GetAsync("Weighting:PhaseBThresholdDays", ct)).Data?.Value as int? ?? 7;
        var fireAt = anchor.AddDays(-approachDays);
        var delay = fireAt - DateTimeOffset.UtcNow;                       // compute delay (no ScheduleAt)
        if (delay < TimeSpan.Zero) delay = TimeSpan.Zero;

        return await _scheduler.ScheduleDelayedAsync(
            action: "ApproachPhaseActivated",
            delay: delay,
            payload: new Dictionary<string, object?> { ["entityId"] = entityId, ["anchor"] = anchor },
            idempotencyKey: $"approach:{entityId}",                       // idempotent transition
            cancellationToken: ct);
    }
}
```

---

## Test Strategy (per branch)

- **Unit (xUnit) — IR-TEMPORAL-2 reproducibility:** call `ExecuteAsync` twice at
  different wall-clock times with the same anchor and assert the same phase/score
  (anchor-driven, not now-driven).
- **Unit — IR-TEMPORAL-1:** set `Weighting:PhaseBThresholdDays` to a non-default via
  `IFreedomConfigService.SetAsync` and assert the phase boundary moves.
- **Integration — IR-TEMPORAL-3:** `ScheduleApproachAsync` calls
  `ScheduleDelayedAsync` (verified through `InMemorySchedulerService.ListAsync`), and a
  repeated call with the same `idempotencyKey` returns the cached job (`fromCache`),
  not a duplicate.
- **Negative:** a request without an anchor timestamp returns `Failure<T>`
  (IR-TEMPORAL-2 guard), never a silent `UtcNow`-only result.
- **Gate-B:** `dotnet build Xiigen.Platform.slnx` (0/0) + `dotnet test` (0 failed).

---

## Anti-Patterns

1. **"I'll create a Phase A handler, a Phase B handler..."** — one handler
   parameterized by phase is correct. Separate handlers duplicate formula logic and
   turn phase transitions into a routing problem instead of a configuration problem.
2. **"`DateTimeOffset.UtcNow` is the only time I need"** — without the anchor
   timestamp, the same event evaluated at different wall-clock times produces different
   scores.
3. **"7 days is a business constant, not config"** — threshold days are FREEDOM config
   because different tenants need different windows (`IFreedomConfigService` resolves
   per tenant/environment).
4. **"Schedule at the due date"** — there is no `ScheduleAt`; compute the delay and
   call `ScheduleDelayedAsync` with an idempotency key.

---

## Integration

- `algorithm-as-service-SKILL.md` Class 2 (Temporal Decay) is the scoring side of this
  pattern; this skill adds the phase-transition machinery.
- `ISchedulerService` (`Xiigen.Fabrics.Scheduler`) owns phase transitions;
  `IFreedomConfigService` (`Xiigen.Freedom`) owns thresholds/weights.
- `api-shape-verification-SKILL.md` supplies the verified `ScheduleDelayedAsync`
  signature (no `ScheduleAt`).
