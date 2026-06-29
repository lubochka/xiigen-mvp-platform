# Archetype Failure Map (XIIGen Enterprise Platform)

> Ported from the `llm_mvp_core` standard, adapted for **.NET 10 ASP.NET Core Web API**,
> xUnit, Gate-B = `dotnet build` / `dotnet test`. **Capability classes** are platform
> domain services. Score brackets are measured by **`dotnet test` exact pass counts**, not
> approximations. The "fix action" for a PATCH-class failure is **add an iron rule** (see
> `iron-rule-derivation-SKILL.md`). **ML archetypes** (HybridRetriever, IncrementalTrainer,
> ContinuousLearningPipeline) are **note-only** — those models stay in `llm_mvp_core` (R5/R6)
> and are consumed via manifest/locator; they are not implemented or scored here.

## Core Problem

A plan that says "implement service X and run tests" with no prediction of where it will
fail leaves the executor making ad-hoc decisions per result. When a capability passes 68%
of its tests on cycle 1, should the response be a targeted patch, a rewrite, or an
escalation? Without a per-archetype failure map, the answer varies by whoever is running
the cycle — not by the evidence. This skill produces a failure map for each capability
class **before** any implementation begins.

> **Platform scoring note.** In this repo a capability's "score" is the fraction of its
> declared `[Fact]`/`[Theory]` acceptance tests that pass. The Gate-B threshold is "all
> required tests green" (`dotnet test -> N passed, 0 failed`). The brackets below map an
> intermediate pass-fraction to a response.

---

## When to Invoke

- Before writing any phase plan for a new capability class (domain service).
- When score interpretation requires a per-bracket response.
- When a retrospective shows the predicted cycle count was wrong (recalibrate the map).

---

## Section 1 — Score Bracket Action Table

Every capability class must declare its score bracket actions before implementation:

| Score | Classification | Action |
|---|---|---|
| ≥ 85% | PASS | Gate B satisfied (all required tests green). Proceed to next phase. Record result. |
| 70–84% | NEAR-PASS | Add a targeted patch for the specific gap. One re-run. Escalate if still NEAR-PASS after the patch. |
| 60–69% | PATCH | Identify the failing iron rule / missing invariant. Add the correction. Re-run. |
| < 60% | STRUCTURAL_FAILURE | Stop. Root cause required before cycling again. Do not retry blindly. |

**Exception — inversion patterns.** A score of 0 on cycle 1 is **correct** when the
capability's iron rules require inverting a default (e.g. a guard that must *reject* what the
permissive default accepts). Verify the `[Fact]` test asserts the inversion. Proceed to
cycle 2; escalate only if cycle 2 also fails.

---

## Section 2 — Per-Capability-Class Failure Mode Templates

For each capability class built in this repo, declare its failure-map entry. Examples:

### UserRegistrationService (CapabilityClass)

```text
Expected cycle 1 score: 70–85%
Common failure: registration not idempotent on email — repeat creates a second record
Fix: add iron rule — "registration MUST be idempotent on email; repeat returns cached result"
Escalate if: < 60% on cycle 1 (STRUCTURAL_FAILURE)
```

### OutboxPublishingService (CapabilityClass)

```text
Expected cycle 1 score: 65–80%
Common failure: event appended to IOutboxStore before IDocumentStore upsert is durable
Fix: add iron rule — "persist via IDocumentStore before IOutboxStore.AppendAsync; else event lost on crash"
Escalate if: < 85% after cycle 2
```

### IdempotentCommandHandler (CapabilityClass)

```text
Expected cycle 1 score: 60–75%
Common failure: side effect runs before IIdempotencyStore.TryReserveAsync succeeds
Fix cycle 1: add iron rule — "reserve via IIdempotencyStore before any side-effecting operation"
Fix cycle 2: per-remaining-gap patch
Escalate if: < 85% after cycle 3
```

### TenantScopedQueryService (CapabilityClass)

```text
Expected cycle 1 score: 70–85%
Common failure: query omits ITenantContextAccessor.Current scope -> cross-tenant leak
Fix: add iron rule — "every IDocumentStore query MUST include the current tenant scope"
Escalate if: < 60% on cycle 1 (security-invariant STRUCTURAL_FAILURE)
```

> **ML archetypes (note-only).** `HybridRetriever`, `IncrementalTrainer`,
> `ContinuousLearningPipeline` stay in `llm_mvp_core`. Do not author a platform failure-map
> entry for them; consume the trained model via manifest/locator (see `STACK-MAP.md`).

---

## Section 3 — Cycle-Specific Escalation Triggers

**Always escalate:**
- STRUCTURAL_FAILURE on any cycle.
- Pass-fraction decreases between consecutive cycles (regression).
- The same failure mode recurs after a targeted patch was applied.
- An expected-failure capability class still failing after its full cycle budget.

**Never escalate before budget exhausted for these:**
- Multi-invariant capabilities (OutboxPublishingService, IdempotentCommandHandler): do not escalate before cycle 3.
- The first instance of any capability class in this repo: cycle 1 < 85% is expected.
- NEAR-PASS on cycle 1 for an established capability class: add one targeted patch, do not escalate.

---

## Section 4 — Failure Map Table Format

Produce this for every capability class before any implementation phase:

```markdown
## Failure Map — [CapabilityClass]

| Capability class | Archetype | Cycle 1 expected | Known failure | Fix action | Escalate trigger |
|---|---|---|---|---|---|
| UserRegistrationService | DOMAIN_SERVICE | 70–85% | Not idempotent on email | Add iron rule: idempotency | < 60% cycle 1 |
| OutboxPublishingService | EVENT_PUBLISH | 65–80% | Emit before durable store | Add iron rule: store-before-emit | < 85% cycle 2 |
| TenantScopedQueryService | TENANT_QUERY | 70–85% | Missing tenant scope | Add iron rule: tenant scope on every query | < 60% cycle 1 |
```

This table belongs in the phase plan's adaptation-map section. It is not optional even when
cycle counts are low — it determines what happens per score bracket without ad-hoc
diagnosis each cycle.

---

## Anti-Patterns

1. **"Score 68% is fine, it's close to NEAR-PASS"** — 60–69% is PATCH, not NEAR-PASS. PATCH
   requires identifying a specific failing invariant before re-running.
2. **"I'll escalate as soon as cycle 1 misses 85%"** — NEAR-PASS and PATCH are not
   escalation triggers. Only STRUCTURAL_FAILURE or budget exhaustion triggers escalation.
3. **"The failure map is for simple capabilities only"** — it is most valuable for complex
   ones (outbox, idempotency, tenant scope). Pre-declaring where they fail gives the
   executor a response plan, not just a diagnosis task.
4. **"Score 0 = failure"** — for inversion-pattern guards, score 0 on cycle 1 verifies the
   inversion is enforced. Treat as PASS-CONDITIONAL.
5. **Authoring an ML-archetype entry locally** — those models stay in `llm_mvp_core`;
   consume via manifest. Do not score them here.

## Integration

- `iron-rule-derivation-SKILL.md` produces the iron rules that this map names as "fix actions".
- `plan-review-SKILL.md` checks that every new capability class carries a failure-map entry with exact bracket thresholds.
- `audit-protocol-SKILL.md` Point 7 (gate thresholds) requires the `dotnet test -> N passed` numbers the brackets reference.
- `STACK-MAP.md` records the ML/trainable boundary that keeps ML archetypes note-only.
