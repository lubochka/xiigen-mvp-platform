# Algorithm-as-Service Pattern (XIIGen Enterprise Platform)

## Core Problem

Algorithms that hardcode scoring weights as constants are not configurable by
operators. Algorithms that bypass the fabric (writing directly to storage, calling
external ML libraries) violate Fabric First (Rule 2) and thin-host (Rule 10). This
skill defines the 5 algorithm classes and specifies the required iron rules, fabric
interfaces, and test strategy for each.

Platform port of the core `algorithm-as-service` skill, re-bound to
**.NET 10 ASP.NET Core Web API**, **xUnit**, `DataProcessResult<T>`, and the real
platform config fabric `IFreedomConfigService` (`src/Xiigen.Freedom/FreedomConfig.cs`).
Gate-B is `dotnet build Xiigen.Platform.slnx` + `dotnet test`.

> **Configuration source on platform.** Tunable weights/thresholds come from the
> FREEDOM config fabric `IFreedomConfigService.GetAsync(key)` — this IS the
> platform's "FREEDOM config". A literal decimal weight in a service is a score-0
> violation. (`IConfiguration`/`IOptions<T>` from `appsettings.json` is acceptable
> for static host configuration, but operator-tunable algorithm weights belong in
> `IFreedomConfigService` so they can change per tenant/environment without a
> redeploy.)

## When to load

- When designing any capability that computes a composite score, ranking, or
  recommendation.
- When a capability has `if/else` logic over numeric thresholds.
- Before writing any class with weights, decay functions, similarity scores, or
  point values.

---

## Section 1 — The 5 Algorithm Classes

### Class 1 — Weighted Scoring (most common)

**Pattern:** N input signals × configurable weights → composite score in [0.0, 1.0].

**Fabric interface:** none for the math; reads weights from `IFreedomConfigService`.

**Required iron rules (violations score 0):**
1. `weights_from_config`: every weight reads from `IFreedomConfigService`, never a
   literal decimal.
2. `weights_sum_to_one`: verify sum ≈ 1.0 at construction (±0.05 tolerance).
3. `score_range_zero_to_one`: output clamped to [0.0, 1.0].
4. `score_denominator_not_zero`: guard division by zero when all signals are 0.

```csharp
// CORRECT: weight from FREEDOM config fabric
var matchWeight = (await _freedom.GetAsync("Scoring:Search:MatchWeight", ct))
    .Data?.Value as double? ?? 0.25;   // default only as fallback

// WRONG: hardcoded weight (score-0 violation)
const double matchWeight = 0.25;
```

**Test strategy (MACHINE constant test, xUnit):**
```csharp
[Fact]
public async Task WeightedScorer_UsesFreedomConfig_NotHardcodedValue()
{
    await _freedom.SetAsync("Scoring:Search:MatchWeight", 0.40, "double"); // default would be 0.25
    var result = await _scorer.ComputeAsync(signals);
    Assert.True(result.IsSuccess);
    // If hardcoded: score uses 0.25 regardless; if config-driven: score uses 0.40
    Assert.NotEqual(_expectedWithDefault025, result.Data!.Score);
}
```

### Class 2 — Temporal Decay

**Pattern:** score decreases as time since the anchor event increases.

**Fabric interface:** none for the math; decay rate/floor from `IFreedomConfigService`;
phase transitions via `ISchedulerService` (see `temporal-behavior-SKILL.md`).

**Required iron rules:**
1. `decay_rate_from_config`: half-life / decay rate from `IFreedomConfigService`.
2. `decay_never_below_floor`: configurable floor enforced.
3. `decay_idempotent`: same inputs → same result.
4. `timestamp_not_datetime_now`: uses the event timestamp from input, never
   `DateTimeOffset.UtcNow` alone inside the decay formula.

### Class 3 — Recommendation (adaptive ML)

**Pattern:** learns from feedback to improve future outputs; weights UPDATE based on
observed behavior.

**Platform rule (R5):** the learned/common model is NOT trained on platform. The
adaptive model and training live in `llm_mvp_core`; platform CONSUMES it through a
manifest/locator (`.xiigen/wiki-domain`). On platform, a Class 3 capability is a
**consumer boundary**, not a trainer.

**Required iron rules (consumer side):**
1. `model_loaded_via_manifest`: model state is loaded through the locator/manifest,
   not trained in-process.
2. `no_ml_framework_added`: no PyTorch/TensorFlow/ML.NET-training dependency is
   added to any `Xiigen.*` project.
3. `recommendation_per_context`: output is context-specific (per tenant/session),
   not global.

### Class 4 — Matching (multi-criteria similarity)

**Pattern:** two entities → similarity across N dimensions (K-NN
(K-Nearest Neighbours), semantic similarity, document comparison).

**Fabric interface:** `IDocumentStore.SearchAsync(filter)` for candidate retrieval;
dimension weights from `IFreedomConfigService`.

**Required iron rules:**
1. `match_score_symmetric`: `score(A,B) == score(B,A)`.
2. `match_score_bounded`: score ∈ [0.0, 1.0].
3. `match_weights_from_config`: dimension weights from `IFreedomConfigService`.
4. `match_never_self`: if `A.Id == B.Id`, return 0.0.

```csharp
[Fact]
public async Task MatchingAlgorithm_IsSymmetric()
{
    var ab = (await _matcher.ComputeAsync(entityA, entityB)).Data!.Score;
    var ba = (await _matcher.ComputeAsync(entityB, entityA)).Data!.Score;
    Assert.InRange(Math.Abs(ab - ba), 0.0, 0.001);
}
```

### Class 5 — Gamification (points, levels, achievements)

**Pattern:** award points for events, compute levels from totals, unlock achievements
at milestones.

**Fabric interface:** behind an `I*` interface; persist through `IDocumentStore`,
dedup through `IIdempotencyStore` — never a direct storage write from the algorithm.

**Required iron rules:**
1. `points_via_fabric`: points persisted through a fabric interface, not a direct
   write (Rule 2).
2. `points_idempotent`: same event + idempotency key (`IIdempotencyStore`) = same
   points, never doubled.
3. `achievement_check_after_award`: achievement check after every award.
4. `level_derived_not_stored`: level computed from total points, never stored
   independently.

---

## Section 2 — Design Checklist for Any Algorithm Capability

```
1. Classify the algorithm (Class 1–5).
2. Register all weight/rate keys in FREEDOM config (IFreedomConfigService), seeded
   in Program.cs or appsettings FREEDOM section.
3. Add class-appropriate iron rules (Section 1).
4. Write the MACHINE constant test BEFORE the implementation (config-driven weight
   differs from hardcoded default).
5. For Class 3: confirm the model is consumed via manifest/locator, NOT trained here.
6. Verify weights sum to 1.0 for Class 1 at construction.
7. Every compute method returns DataProcessResult<T> (Rule 1).
```

---

## Section 3 — FREEDOM Config Key Convention

Operator-tunable algorithm values use colon-delimited keys read through
`IFreedomConfigService.GetAsync`:

```
Scoring:<Domain>:<Signal>Weight        e.g. Scoring:Search:MatchWeight = 0.25
Decay:<Domain>:HalfLifeHours           e.g. Decay:Search:HalfLifeHours = 24
Decay:<Domain>:Floor                   e.g. Decay:Search:Floor = 0.05
Gamification:Points:<Action>           e.g. Gamification:Points:Submit = 10
```

`FreedomConfigValue` carries `Key`, `Value`, `ValueType`, `Scope`, `TenantId`,
`Environment`, `Version` (`src/Xiigen.Freedom/FreedomConfig.cs`) — so the same key
can resolve differently per tenant/environment.

---

## Test Strategy (per branch)

- **Unit (xUnit):** MACHINE constant test per Class 1/2/4; symmetry + self-match for
  Class 4; idempotency for Class 5.
- **Integration:** algorithm reads from the real `InMemoryFreedomConfigService` and a
  changed value changes the output.
- **Negative:** all-zero signals return a guarded `Failure<T>` or clamped 0.0, never
  a thrown `DivideByZeroException`.
- **Untrained (Class 3):** the consumer honestly reports model-not-loaded
  (runner-missing) rather than faking a score.
- **Gate-B:** `dotnet build Xiigen.Platform.slnx` (0/0) + `dotnet test` (0 failed).

---

## Anti-Patterns

1. **"Weight is a constant — it's the right value"** — today's right value is
   tomorrow's tuning request. All weights are FREEDOM config.
2. **"Recommendation can train here"** — R5 violation. Common/adaptive models live in
   `llm_mvp_core`; platform consumes via manifest. Adding an ML-training dependency
   to `Xiigen.*` is forbidden.
3. **"Matching is deterministic, no test needed"** — symmetry and self-match tests are
   structural, not probabilistic. Required.
4. **"The algorithm outputs the right value, DNA is fine"** — Rule 1
   (`DataProcessResult<T>`) applies to every compute method, including pure math.

---

## Integration

- `temporal-behavior-SKILL.md` specializes Class 2 (anchor → phase → weights →
  formula) on `ISchedulerService`.
- `dna-compliance-guard-SKILL.md` / `DNA-DOTNET-TRANSLATION.md` enforce
  `DataProcessResult<T>`, Fabric First, and "no business logic in the host layer".
- `event-contract-design-SKILL.md` governs any event a scoring/gamification service
  emits.
