# four-tier-decision-classification-SKILL.md — Boundary / Freedom / Learnable / AI

> Ported from the `llm_mvp_core` governance standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build` / `dotnet test`, domain return type `DataProcessResult<T>`
> (`src/Xiigen.Kernel/DataProcessResult.cs`) — **not** core `OperationResult<T>`.
> Q1 BOUNDARY uses the real `DataProcessResult<T>` contract + the fabric
> interfaces in `src/Xiigen.Kernel`; Q2 FREEDOM uses `IFreedomConfigService`
> (`src/Xiigen.Freedom/FreedomConfig.cs`) / `IConfiguration`. **R5 split:** Q3
> LEARNABLE and Q4 AI-PIPELINE remain valid *classifications*, but common
> trainable models are **not** built or stored in this repo — they are consumed
> through a manifest/locator; only an adaptive leg may be local.

## When to invoke
Before designing any class, service, or component that makes a decision: any
`if/else`, `switch`, lookup table, threshold comparison, enum-to-value mapping,
or conditional branch. Supersedes `freedom-machine-SKILL.md` for NEW components
by adding the LEARNABLE and AI-PIPELINE tiers.

---

## What this skill prevents
- Hardcoding a value that should be tenant/deployment-tunable (it belongs in
  `IFreedomConfigService`, not a constant).
- Putting a LEARNABLE decision in `IConfiguration` (it gets a value but never
  improves with more data).
- Building an inference pipeline for a decision that is genuinely static.
- Building/storing a **common trainable model in this repo** when R5 requires it
  to live upstream and be consumed via manifest.

---

## The four-tier test

For every decision point, run these four questions IN ORDER. Stop at first YES.

```
Q1: Does changing this value break a system guarantee AND require a code change?
    YES → BOUNDARY CODE (Layer 1)
    Hardcode as a named const / fabric invariant. Abstract interface + DI wiring.
    Platform examples:
      - DataProcessResult<T> contract (.Success / .Failure — never throw)
      - IDocumentStore / IOutboxStore / IIdempotencyStore / ISchedulerService contract shape
      - Tenant-scope invariant via ITenantContextAccessor.Current
      - CloudEventEnvelope shape
    → STOP. Fabric-First + DataProcessResult<T> everywhere.

Q2: Might different tenants or deployments reasonably want different values?
    YES → FREEDOM CONFIG (Layer 2)
    Seed in IFreedomConfigService (or read IConfiguration) with a safe default.
    Platform examples:
      - flow01_email_verification_ttl_seconds (default 86400)
      - flow01_resend_rate_limit_minutes (default 60)
      - invitation branding strings (flow01_invitation_*)
      - IIdempotencyStore TryReserveAsync ttl (per-call FREEDOM)
    → STOP. Seed/read via IFreedomConfigService + FREEDOM annotation in code.

Q3: Could the correct answer improve if the system observed more outcomes?
    YES → LEARNABLE (Layer 3) — CLASSIFY ONLY in this repo (R5).
    The common trainable model is NOT built/stored here. It is consumed through
    a model manifest/locator; only a tenant-specific adaptive leg may be local.
    Platform examples (classified, consumed upstream):
      - routing/ranking weights that improve from feedback
      - confidence thresholds tuned from observed outcomes
    → Record the classification + the manifest/locator consumption point.
       Do NOT add a local common-training pipeline without explicit Luba approval.

Q4: Does answering this require contextual reasoning beyond pattern matching?
    YES → AI PIPELINE (Layer 4) — CLASSIFY ONLY in this repo (R5).
    Route through the consumed inference model (manifest), behind a fabric
    interface; no in-repo common model build.
    Platform examples:
      - novel input not covered by any existing example
      - ambiguous intent needing multi-step clarification
    → Record the classification + the inference consumption boundary.
```

For any Q3/Q4 decision, the **learning signal** is captured upstream (where the
common model lives), not in this platform repo. If a plan claims a local common
trainable unit here, it contradicts R5 — escalate via `no-product-decisions-SKILL.md`.

---

## The five learnable indicators (for Q3)

When Q1 and Q2 are both NO, apply these to decide LEARNABLE vs an unclassified
FREEDOM value:

1. The decision has been wrong before (corrections in git history).
2. The answer differs by tenant / domain / corpus.
3. The right answer needs evidence from multiple runs, not one observation.
4. Reasonable engineers disagree on the default (uncertainty = learning opportunity).
5. A human has had to override the default more than once.

If 2+ indicators are YES → LEARNABLE (classify; consume the model via manifest).
If 0–1 → likely an unclassified FREEDOM value → seed it in `IFreedomConfigService`.

---

## Classification table

Produce this table before writing any new service or component:

```
| Decision point                         | Q1  | Q2  | Q3  | Q4  | Tier            |
|----------------------------------------|-----|-----|-----|-----|-----------------|
| DataProcessResult return convention    | YES | —   | —   | —   | BOUNDARY CODE   |
| email verification TTL                 | NO  | YES | —   | —   | FREEDOM CONFIG  |
| resend rate limit                      | NO  | YES | —   | —   | FREEDOM CONFIG  |
| feedback-tuned routing weight          | NO  | NO  | YES | —   | LEARNABLE (consume) |
| ambiguous-intent resolution            | NO  | NO  | NO  | YES | AI PIPELINE (consume) |
```

---

## Anti-patterns
1. **"Every tunable should be config"** — a value that improves with observed
   outcomes is LEARNABLE, not `IFreedomConfigService`. Config gives a value but
   never improves it.
2. **"I'll make it learnable later"** — classify before implementing; record the
   manifest consumption point now.
3. **"Simple enough to hardcode"** — simplicity is not the test; a simple
   threshold that should improve with data IS a learning target (classified, consumed).
4. **Building a common trainable model in this repo** — R5: common models live
   upstream; this platform consumes them via manifest/locator. A local common
   model build needs explicit Luba approval.

---

## Connection to downstream skills
```
Q1 → BOUNDARY CODE   → DataProcessResult<T> everywhere + Fabric-First
Q2 → FREEDOM CONFIG  → freedom-machine-SKILL.md annotation + IFreedomConfigService/IConfiguration
Q3 → LEARNABLE       → classify only; consume via model manifest/locator (R5)
Q4 → AI PIPELINE     → classify only; consume inference via manifest behind a fabric interface (R5)
```
