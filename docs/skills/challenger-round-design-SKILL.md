# Challenger Round Design

> Ported from the `llm_mvp_core` challenger-round standard — **universal part
> only** — adapted for the XIIGen Enterprise Platform: **.NET 10 ASP.NET Core Web
> API**, xUnit, Gate-B = `dotnet build Xiigen.Platform.slnx` / `dotnet test`.
> This skill is the formalization of the `PLAN_REVIEW_INTERNAL` / `REVIEW_INTERNAL`
> step in the `AGENTS.md` role loop (cross-review before `ARCHITECT` / before
> matrix acceptance). The DNA challenger references platform governance prose
> (`AGENTS.md` / `CLAUDE.md` Beacon-First, fabric-first, `DataProcessResult<T>`,
> tenant isolation) and `DNA-DOTNET-TRANSLATION.md` — **not** core Rules 1–10.
> Context packages are domain services in `src/Xiigen.*`, fabric contracts in
> `Xiigen.Kernel/FabricContracts.cs`, and tenant/session data in `Xiigen.Auth`.
> `CONTEXT_INSUFFICIENT` / `DEFERRED` are recorded in the v3.3 state/matrix;
> `STALLED` escalates to Luba. **UI challenger is na — platform has no frontend.**
>
> Forward-note (stays in `llm_mvp_core`, R5/R6): the ML/arbiter-coupled tail —
> a DNA arbiter hard-wired to `CLAUDE.md Rules 1–10` numbering, and the
> "convergence training signals feed DomainModelRegistry via
> ContinuousLearningPipeline" integration plus `arbiter-panel-design` runtime
> coupling — is NOT ported. Platform consumes any such trained signal through the
> `TRAINABLE-UNIT-MANIFEST-CONTRACT.md` / `.xiigen` manifests, it does not own it.

## Core Problem

A multi-model convergence (cross-review) session without defined challenger roles
produces inconsistent challenges — one reviewer checks intent, another checks
security, both check both, neither checks completeness. The challenge history
contains noise; the final design carries unresolved ambiguity. This skill defines
how to compose challenger roles, what each challenger receives (context
isolation), and when to declare consensus vs escalate.

---

## When to Invoke

- Before writing any multi-reviewer cross-review for a capability design.
- When `multi-reviewer-design-SKILL.md` requires challenger prompt templates.
- When a cross-review is STALLED and the escalation type needs diagnosis.
- Before Gate B of any phase that uses multi-model design review.

---

## Section 1 — Challenger Role Selection

### Minimum challenger set by capability archetype

| Archetype | Minimum challengers |
|---|---|
| ROUTING | Domain + DNA Compliance |
| DATA_PIPELINE | Domain + Security + DNA Compliance |
| VALIDATION | Domain + Completeness + DNA Compliance |
| SCORING | Domain + DNA Compliance |
| ORCHESTRATION | Domain + Security + Business + Completeness + DNA Compliance |

(Common-model/ML training archetypes are owned by `llm_mvp_core`; on platform a
capability that consumes a trained manifest is reviewed as DATA_PIPELINE or
ORCHESTRATION over the manifest contract.)

### Add optional challengers when:
- Capability emits cross-capability / cross-project events (CloudEvents via `IOutboxStore`) → add **Business**.
- Capability crosses platform-project boundaries → add **Business**.
- Capability involves tenant/session data or `Xiigen.Auth` context → add **Security**.

### Never add challengers for noise

Do not add a Security challenger to a read-only query that touches no tenant
session data. Do not add a Business challenger to a single-project capability with
no cross-system contracts. Noise challengers inflate rounds without adding signal.

---

## Section 2 — Challenger Prompt Templates

Every challenger receives a prompt that defines its role, context package, and
response format. **Context isolation is mandatory: each challenger receives ONLY
its defined subset.**

### Domain Challenger

```
ROLE:   Domain Challenger in a capability design cross-review.
EXPERTISE: Intent correctness and invariant completeness for this capability class.

RECEIVES:
- The proposed capability design (contract + service shape)
- The capability's invariants
- The domain context (what this capability does for a tenant — from system-intake)

DOES NOT RECEIVE: security patterns, DNA principle text, other challenger verdicts.

For each challenge, provide:
1. Which field in the design is incorrect or incomplete
2. Why (trace to a specific domain invariant)
3. What the correct representation should be

RESPONSE FORMAT (a C# record shape, returned as JSON):
{
  "verdict": "APPROVED" | "CHALLENGE" | "CONTEXT_INSUFFICIENT",
  "challenges": [ { "field": "...", "reason": "...", "correction": "..." } ],
  "contextRequests": [ { "type": "DOWNSTREAM_CONTRACT | INTERFACE_SPEC | ...", "target": "...", "question": "..." } ]
}

A challenge is GENUINE if removing it would change what code gets generated.
A challenge is NOISE if the correction would produce functionally identical code.
Raise only genuine challenges.
```

### Security Challenger

```
ROLE:   Security Challenger in a capability design cross-review.
EXPERTISE: Tenant isolation (ITenantContextAccessor), DataProcessResult<T> failure
           coverage, no-secrets, fabric-first.

RECEIVES:
- The proposed capability design
- AGENTS.md / CLAUDE.md governance prose on tenant isolation and no-secrets (with line refs)
- The capability's declared failure modes

DOES NOT RECEIVE: domain context, other challenger verdicts.

Check:
- Does every failure path return a DataProcessResult.Failure(code, message) rather than throw?
- Is tenant identity taken from ITenantContextAccessor.Current, never a method parameter?
- Are there missing failure modes for: cross-tenant leakage, idempotency-key reuse, outbox-before-persist?

RESPONSE FORMAT: same as Domain Challenger
```

### Business Challenger

```
ROLE:   Business Challenger in a capability design cross-review.
EXPERTISE: Cross-project consistency — contracts between this capability and other
           platform projects/services (CloudEvents, shared fabric records).

RECEIVES:
- The proposed capability design
- The CloudEvents this capability emits/consumes (Type/Subject/Data shape)
- Upstream capability designs from earlier in the same pipeline

DOES NOT RECEIVE: DNA principle text, security patterns.

Looking for:
- Field values a prior capability may transform before they reach this one
- Event schema fields downstream consumers may interpret differently
- Dependencies on an assumption about an upstream project

RESPONSE FORMAT: same as Domain Challenger
```

### DNA Compliance Challenger (context-isolated — mandatory)

```
ROLE:   DNA Compliance Arbiter in a capability design cross-review.
EXPERTISE: Alignment with platform DNA (DNA-DOTNET-TRANSLATION.md) and the
           AGENTS.md / CLAUDE.md governance prose. NOT core CLAUDE.md Rules 1–10.

RECEIVES (ONLY these — isolation is the enforcement mechanism):
- The platform DNA rules text (DNA-DOTNET-TRANSLATION.md) with concrete violation examples
- The proposed capability design

DOES NOT RECEIVE: domain context, security patterns, other challenger verdicts.

Check: does the capability design, as written, violate any platform DNA rule?
  - Rule 3: DataProcessResult<T> for business logic (never throw, never null)
  - Rule 4: fabric-first — depends on Xiigen.Kernel/Fabrics interfaces + MicroserviceBase<T>
  - Rule 5: tenant scope from ITenantContextAccessor; no shared mutable static
  - Rule 8: persist before dispatch (UpsertAsync before AppendAsync/ScheduleDelayedAsync)
  - Rule 6: thin Program.cs endpoints — no business logic in the host

BLOCK threshold: ANY platform DNA violation is an automatic BLOCK.
A CHALLENGE from this role means the design cannot be accepted until resolved.

RESPONSE FORMAT: same as Domain Challenger
```

### Completeness Challenger

```
ROLE:   Completeness Challenger in a capability design cross-review.
EXPERTISE: Verifying nothing essential is missing.

RECEIVES:
- The proposed capability design
- The capability's structure fields (inputShape, outputShape, emits)

Check:
- Are all declared outputs reachable from the declared inputs?
- Are all declared emits (CloudEvents) traceable to a condition in the invariants/failure modes?
- Is the purpose specific enough that a second implementor could build the same thing?

RESPONSE FORMAT: same as Domain Challenger
```

---

## Section 3 — Termination Conditions

### CONSENSUS_REACHED (happy path)

All challengers return APPROVED in the same round.
```
□ Every challenger returned APPROVED (not just a majority)
□ Final design was validated against the last round's challenges
□ No pending CONTEXT_INSUFFICIENT requests
□ convergenceHistory.consensusReachedAt is set (recorded in v3.3 state)
```

### DEFERRED_CONSTRAINT (acceptable provisional)

A CONTEXT_INSUFFICIENT request could not be resolved (requires human judgment or
external information). Mark the design provisional:

```markdown
## DEFERRED_CONSTRAINT
Request type: DOWNSTREAM_CONTRACT
Target: SMART-WIKI-CONTRACT.md wiki-updated event Version field
Question: Must the wiki-updated CloudEvent carry document Version?
Deferred at: [date] (recorded in v3.3 state/matrix)
Unblocks: the invariant "subscribers do optimistic concurrency" cannot be finalized
```

Implementation proceeds with the provisional design; generated code is marked with
the DEFERRED_CONSTRAINT and may need revision when resolved.

### STALLED (escalation required)

Max rounds exceeded without consensus. Escalate to Luba. Stalling usually means:
- Genuine architectural ambiguity → escalate to Luba.
- Circular challenges (challenger scope too broad) → narrow the context package.
- Missing upstream design (convergence order violated) → check the dependency graph.

`STALLED` is NOT equivalent to `CONSENSUS`. A STALLED design used for
implementation carries unresolved ambiguity into the code.

---

## Section 4 — Genuine Disagreement Requirement

A cross-review produces learning signal only when challengers genuinely disagree.

**Genuine disagreement requires:**
```
1. At least one CHALLENGE verdict
   (all APPROVED on round 1 = either the design is trivial or challengers didn't try)
2. The CHALLENGE traces to a specific field and a specific invariant
   (vague challenges like "this needs more detail" = noise)
3. The correction, if applied, changes what code gets generated
   (cosmetic challenges = noise)
```

**Pre-convergence gate — run before starting rounds:**
```
□ Design has at least one domain-specific constraint
□ At least one legitimate implementation path exists that a challenger might object to
□ The purpose is specific enough to distinguish from similar capabilities
```

---

## Anti-Patterns

1. **"Challenger prompt without context isolation"** — a DNA Compliance challenger receiving domain context is a contamination. Each challenger receives ONLY its defined package.
2. **"Adding challengers to appear thorough"** — a Security challenger on a read-only query adds noise. Minimum panel by archetype; add only when justified.
3. **"APPROVED after round 1 with no challenges"** — either the design is trivial or the challengers did not challenge hard enough. Re-run with more specific criteria.
4. **"STALLED treated as equivalent to CONSENSUS"** — STALLED means genuine disagreement exists. Escalate it.

---

## Integration

- `multi-reviewer-design-SKILL.md` uses these challenger roles for phase-plan reviews.
- `diagnostic-escalation-SKILL.md` governs escalation when STALLED is reached.
- `context-package-design-SKILL.md` governs the structure of each challenger's context package.
- `system-intake-SKILL.md` supplies the domain context the Domain challenger receives.
- `DNA-DOTNET-TRANSLATION.md` is the DNA challenger's isolated rules text.
