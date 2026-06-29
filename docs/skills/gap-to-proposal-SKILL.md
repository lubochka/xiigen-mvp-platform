# Gap to Proposal

> Ported from the `llm_mvp_core` gap-to-proposal standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build Xiigen.Platform.slnx` / `dotnet test`. Ladder levels are bound to
> the real stack: LEVEL 1 = governance edit (`AGENTS.md`/`CLAUDE.md`/skill),
> LEVEL 2 = new implementation over an existing `Xiigen.Kernel` fabric contract,
> LEVEL 4 = new contract + class + xUnit + DI in `Xiigen.Platform.Api/Program.cs`.
> Overrides are recorded in the v3.3 beacon/decision artifact (no
> `DECISIONS-LOCKED.md` on platform). There is no UI ladder — platform has no
> frontend. Domain return type is `DataProcessResult<T>` (never core
> `OperationResult<T>`).

## The Resolution Ladder

Before proposing a new project, new fabric contract, or new algorithm, climb the
ladder. Start at Level 1. Move up only if the lower level genuinely cannot solve
the problem.

```
LEVEL 1 — CONVENTION: Can this be fixed by updating AGENTS.md, CLAUDE.md, or a skill/DNA rule?
  Example: A service was found instantiating a fabric type directly (fabric-first violation)
  Fix: Add/strengthen the fabric-first grep check in dna-compliance-guard-SKILL.md
  No new code needed. ~10-minute governance update.

LEVEL 2 — ADAPTATION: Does an existing fabric contract cover this with a new implementation?
  Example: Need a Postgres-backed document store
  Check: IDocumentStore exists (it does — Xiigen.Kernel/FabricContracts.cs)
  Fix: Add PostgresDocumentStore : IDocumentStore (new Xiigen.Fabrics.Postgres project or class)
  No new contract needed. One implementation session + DI swap.

LEVEL 3 — EXTENSION: Does an existing class/contract cover this with a new method?
  Example: ISchedulerService needs a CancelAsync(jobId) method
  Check: ISchedulerService already exists with ScheduleDelayedAsync/ListAsync
  Fix: Add CancelAsync() to ISchedulerService + implement in InMemorySchedulerService + update tests
  A contract addition, not a new project.

LEVEL 4 — NEW COMPONENT: None of the above apply.
  Example: Need a new tenant-quota fabric — no existing contract covers it
  New interface (IQuotaStore) + new implementation + new xUnit tests + DI registration
  in Xiigen.Platform.Api/Program.cs.
```

Document why each lower level is insufficient before proposing a higher level.

---

## Step 1 — Classify the Gap

```
Gap: No way to enforce per-tenant request quotas

Q1: Can CONVENTION fix it?
    No — a quota is runtime state, not a configuration/governance value.

Q2: Can ADAPTATION fix it? (existing contract covers it with a new implementation)
    No — IDocumentStore is document CRUD, IIdempotencyStore is dedup, IOutboxStore
    is events. None model a counting/limit semantic.

Q3: Can EXTENSION fix it? (add a method to an existing contract)
    Partially — IIdempotencyStore reserves keys, but overloading it with quota
    counting mixes two concerns and breaks the single-responsibility of the contract.

Q4: New component needed?
    YES — IQuotaStore with a typed limit/usage model, injected where enforcement happens.
```

---

## Step 2 — Produce the Proposal

```markdown
## GAP RESOLUTION PROPOSAL — IQuotaStore

**Gap detected:** No per-tenant request-quota enforcement
**Resolution level:** LEVEL 4 — new fabric contract + implementation

**What will be built:**
  - IQuotaStore contract: TryConsumeAsync(tenantId, key, limit) → DataProcessResult<QuotaReservation>
  - InMemoryQuotaStore : IQuotaStore (Lock-guarded counters, like InMemorySchedulerService)
  - xUnit tests: limit reached, under limit, tenant isolation, concurrent consume
  - DI registration: builder.Services.AddSingleton<IQuotaStore, InMemoryQuotaStore>()
    in Xiigen.Platform.Api/Program.cs (or an AddXiigen... extension)

**Prerequisite gaps to resolve first:**
  - Tenant identity must come from ITenantContextAccessor.Current (it does)

**Estimated effort:** 1 code session (contract + implementation + tests + DI)
**Builds on:** Pattern from ISchedulerService + InMemorySchedulerService

**Gate B:** dotnet test --filter "FullyQualifiedName~QuotaStoreTests" → [N] passed, 0 failed

**Stop required before:** wiring quota enforcement into any Program.cs endpoint

**Needs Luba decision:**
  - Should the default limit be a FREEDOM (Xiigen.Freedom) key per tenant, or a const?
    (Recommendation: FREEDOM key so it is tenant-overridable.)
```

---

## Step 3 — Dependency Order for Multiple Gaps

When multiple gaps exist, order them by dependency:

```
Gap A (IQuotaStore contract) → blocks Gap B
Gap B (quota enforcement middleware in Program.cs) → blocks Gap C
Gap C (quota-exceeded outbox event) → blocks tenant-notification feature

Order: A → B → C
Rationale: A is the prerequisite, B uses A, C uses both.
```

---

## Stop Gate

Every proposal requires Luba's approval before work begins. The proposal is the
INPUT to the approval, not the permission to start. No new-component session
begins without an explicit "yes" from Luba. When the override changes a v3.3
decision, record it in the v3.3 beacon/decision artifact.

---

## Anti-Patterns

1. **Jumping to LEVEL 4 without checking lower levels** — most gaps are Level 2 or 3 over the existing `Xiigen.Fabrics.*` contracts.
2. **"We'll need this eventually so let's build it now"** — build when the gap blocks current work, not speculatively.
3. **Proposal without a prerequisite chain** — if Gap A depends on Gap B, the proposal must say so.
4. **Starting work before Luba's approval** — the proposal gate is mandatory.
5. **Overloading an existing fabric contract to dodge a LEVEL 4** — mixing concerns into `IIdempotencyStore`/`IDocumentStore` breaks single responsibility; prefer a new contract.

---

## Integration

- `solution-scope-gate-SKILL.md` classifies the fix-class (Level) before the proposal.
- `prerequisite-chain-SKILL.md` verifies each prerequisite gap before work begins.
- `subproject-examination-SKILL.md` produces the MASTER/MISSING-INFRASTRUCTURE/LEARNING-SPEC documents for a LEVEL 4 proposal.
- The v3.3 beacon/decision artifact receives every priority/architecture override.
