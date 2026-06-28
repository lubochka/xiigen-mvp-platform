# Feature Prioritization

> Ported from the `llm_mvp_core` prioritization standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build Xiigen.Platform.slnx` / `dotnet test`. Candidates are
> capabilities in `Xiigen.Platform.Api` / `Xiigen.Platform.Marketplace`. Axis 3
> (infrastructure cost, inverted) is measured against the readiness of the fabric
> contracts in `Xiigen.Kernel/FabricContracts.cs` and their implementations in
> `Xiigen.Fabrics.*`. Overrides are recorded in the v3.3 beacon/decision artifact
> (there is no `DECISIONS-LOCKED.md` on platform). Domain return type is
> `DataProcessResult<T>` (never core `OperationResult<T>`).

## Core Problem

When multiple capabilities are buildable at the same time, choosing the wrong one
first delays tenant-visible value and wastes infrastructure investment. This skill
defines a 3-axis score that makes the ordering decision explicit and auditable,
and records any override so priority decisions are not invisible.

---

## When to Invoke

- When a v3.3 session has 2+ candidate capabilities to sequence.
- When Luba asks "what should we build next?".
- When a new capability is proposed and dependencies are not yet mapped.
- When session-start reconnaissance finds multiple incomplete capabilities.

One score card per session = zero "we built the wrong thing first" retrospectives.

---

## Section 1 — The 3-Axis Score

### Axis 1 — Business Value Unlock (1–5)

Does completing this capability let tenants DO something new?

| Score | Meaning |
|---|---|
| 5 | Tenants can take a new action that was impossible before |
| 4 | Substantially improves a current action (speed, quality, reach) |
| 3 | Makes the platform more complete, but no new tenant action is unlocked |
| 2 | Operational improvement (monitoring, diagnostics) |
| 1 | Backend-only: infrastructure, plumbing, no tenant-visible change |

### Axis 2 — Dependency Multiplier (1–3)

How many other capabilities are blocked until this one ships?

| Score | Meaning |
|---|---|
| 3 | 3 or more capabilities cannot start until this ships |
| 2 | 1–2 capabilities are blocked |
| 1 | Nothing else is blocked by this |

### Axis 3 — Infrastructure Cost (1–3, INVERTED — lower cost = higher score)

How much new fabric must be built before this capability can be implemented?

| Score | Meaning |
|---|---|
| 3 | All required fabric contracts already exist (IDocumentStore, IIdempotencyStore, IOutboxStore, ITenantContextAccessor, ISchedulerService) with implementations in `Xiigen.Fabrics.*` |
| 2 | One new fabric contract required |
| 1 | Two or more new fabric contracts required |

**INVERTED because:** high infrastructure cost means the capability cannot be
built until that fabric exists — building the fabric first is the only correct
sequence, and scoring it low captures that friction.

### Priority Score Formula

```
Priority Score = Axis1 × Axis2 × Axis3
```

Maximum: 5 × 3 × 3 = 45. Minimum: 1 × 1 × 1 = 1.

---

## Section 2 — Scoring Example

| Capability | Axis 1 | Axis 2 | Axis 3 | Score | Rationale |
|---|---|---|---|---|---|
| Smart Wiki search (Marketplace) | 5 | 3 | 3 | 45 | New tenant action; 3 caps depend on it; all fabric contracts exist |
| Smart Code assistant | 4 | 2 | 3 | 24 | Improves a workflow; 2 caps blocked; contracts exist |
| Trainable-unit manifest registry | 3 | 1 | 2 | 6 | More complete; nothing blocked; 1 new contract needed |
| Per-tenant quota enforcement | 3 | 1 | 1 | 3 | Backend-only; nothing blocked; 2+ new fabric pieces needed |

**Build order:** Smart Wiki search → Smart Code assistant → Trainable-unit
registry → Quota enforcement.

---

## Section 3 — Score Card Format

Before executing a plan with multiple sequenced capabilities, produce a score card:

```markdown
## Feature Priority Score Card — [Session Date]

| Capability | Axis1 (value) | Axis2 (deps) | Axis3 (cost⁻¹) | Score | Verdict |
|---|---|---|---|---|---|
| [name] | [1-5] | [1-3] | [1-3] | [A1×A2×A3] | BUILD_NEXT / QUEUE / BLOCKED |

Selected build order: [ranked list]
Override: [NONE / v3.3 decision ref — reason]
```

**Verdict definitions:**
- **BUILD_NEXT** — highest score in the current set; no blocking prerequisites.
- **QUEUE** — lower score; prerequisites met; sequence after BUILD_NEXT.
- **BLOCKED** — hard dependency on an unshipped capability; score irrelevant until unblocked.

---

## Section 4 — Override Rule

A priority score can be overridden when Luba explicitly names a different
capability as the priority, or a stakeholder commitment requires a specific
delivery order.

**Override is not free.** Record every override in the v3.3 beacon/decision
artifact (the platform has no `DECISIONS-LOCKED.md`):

```markdown
Priority Override: [capability] promoted over [score-winner]
Reason: [stakeholder reason / commitment]
Score at override time: [score-winner]=N, [promoted]=M
Recorded: [date] in [v3.3 decision artifact path]
```

**What override cannot do:** override a hard dependency lock. If Capability B
requires Capability A's output, B cannot be built first regardless of score or
priority. The dependency graph is invariant. Only the ordering of non-blocked
capabilities is flexible.

---

## Section 5 — Dependency Lock Rule

Before scoring, classify each candidate:

```markdown
Dependency check: [CapabilityName]
  Requires: [fabric contracts / capabilities that must exist first]
  Status of each: [SHIPPED / IN_PROGRESS / MISSING]
  Can start now: YES / NO — blocked by [name]
```

Capabilities with any MISSING prerequisite are marked BLOCKED and removed from
the score calculation. They are re-evaluated after their prerequisites ship.

---

## Anti-Patterns

1. **"We built what was easiest"** — low infrastructure cost (Axis 3 = 3) alone does not justify building first if there is no tenant value (Axis 1 = 1). Score all three axes.
2. **"We built what felt most important"** — feeling is not a score. Produce the card before building.
3. **"Override is fine, we don't need to record it"** — an unrecorded override is an invisible decision. Record it in the v3.3 decision artifact.
4. **"Dependency locks are guidelines"** — they are code constraints. If B reads A's output from a store key, B cannot run before A writes it.

---

## Integration

- `prerequisite-chain-SKILL.md` classifies each dependency as PASS/FAIL/BLOCKING.
- `gap-to-proposal-SKILL.md` classifies each capability's fix-class (Level) before scoring.
- `wave-assignment-SKILL.md` consumes the same dependency graph for FOUNDATION/Wave classification.
- The v3.3 beacon/decision artifact receives every priority override.
