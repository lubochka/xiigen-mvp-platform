# freedom-machine-SKILL.md — Freedom vs Machine classification

> Ported from the `llm_mvp_core` governance standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build` / `dotnet test`, domain return type `DataProcessResult<T>`
> (`src/Xiigen.Kernel/DataProcessResult.cs`) — **not** core `OperationResult<T>`.
> FREEDOM values in this repo are configured through the real
> `IFreedomConfigService` (`src/Xiigen.Freedom/FreedomConfig.cs`), not invented
> per-interface knobs. MACHINE invariants are the real fabric contracts in
> `src/Xiigen.Kernel/FabricContracts.cs`, `src/Xiigen.Kernel/TenantContext.cs`,
> and `src/Xiigen.Fabrics.Scheduler/Scheduler.cs`.

## When to apply
- Before writing any constant, threshold, string, or configuration value into code.
- Before deciding whether a value should be a seeded FREEDOM key, an
  `IConfiguration` value, or a hardcoded named constant.
- When a Fabric-First (interface-resolution) check finds a hardcoded value.

---

## The classification test

One question:

> **If this value changed, would the system's behavioral guarantees break?**

```
YES → MACHINE. This value enforces a system invariant.
      Hardcode it as a named const. Add an xUnit presence/contract test.
      Never make it config — changing it changes what the system guarantees.

NO  → FREEDOM. This value is a configurable parameter.
      Seed it in IFreedomConfigService (or read IConfiguration) with a safe default.
      Different tenants / deployments may reasonably use different values.
```

---

## Canonical MACHINE values in this repo

| Value | Why MACHINE |
|-------|-------------|
| `DataProcessResult<T>` return-type requirement (`.Success` / `.Failure`, never throw) | Violating it breaks error propagation for every caller |
| Fabric interface resolution (constructor-injected `IDocumentStore` / `IOutboxStore` / `IIdempotencyStore` / `ISchedulerService`) | Bypassing the interface breaks testability and swappability |
| Tenant-scope via `ITenantContextAccessor.Current` / `Push` (`src/Xiigen.Kernel/TenantContext.cs`) | A tenant-id leak is a security-invariant break, not a tuning knob |
| The shape of `IDocumentStore` / `IOutboxStore` / `IIdempotencyStore` / `ISchedulerService` contracts | Changing the contract breaks all callers |
| CloudEvents envelope shape (`CloudEventEnvelope`) | Cross-service event contract — fixed by integration |
| No-Secrets-in-repo (`AGENTS.md`) | A MACHINE invariant: the secret value lives in Vault/config, never in source |
| xUnit zero-failures Gate-B | The hard gate — cannot be relaxed |

---

## Canonical FREEDOM values in this repo

These are configured through `IFreedomConfigService`
(`InMemoryFreedomConfigService` seeds them) — **not** as new interface knobs:

| Seeded FREEDOM key | Default | Why FREEDOM |
|--------------------|---------|-------------|
| `flow01_email_verification_ttl_seconds` | `86400` | Deployment/tenant policy — not a correctness invariant |
| `flow01_resend_rate_limit_minutes` | `60` | Operational policy — tunable per tenant |
| `flow01_invitation_inviter_name` | `"The Team"` | Tenant branding string |
| `flow01_invitation_community_name` | `"Community"` | Tenant branding string |
| `flow_module_name` | `"user-registration"` | Module label, configurable |

Real per-call FREEDOM parameter on a MACHINE interface: the `TimeSpan ttl` of
`IIdempotencyStore.TryReserveAsync(operationKey, ttl)` — the rule "reservations
expire" is MACHINE; the specific TTL is FREEDOM.

> **Do not invent config knobs that the interfaces do not expose.**
> `ISchedulerService` is `ScheduleDelayedAsync(action, delay, payload, idempotencyKey)`
> + `ListAsync` — there is **no** poll-interval / lease-duration / retry-count /
> batch-size surface. `IOutboxStore` is `AppendAsync` + `ListAsync`;
> `OutboxRecord.Attempts` is record **data**, not a setting. Tunables live in
> `FreedomConfig`; reference that, do not fabricate interface parameters.

---

## The borderline cases

### Thresholds that enforce invariants vs configure behaviour
```
MACHINE: the rule "an idempotency reservation must expire" — removing expiry
  changes the de-dup guarantee. Hardcode the rule; test it.

FREEDOM: the specific TTL passed to TryReserveAsync — 5 min vs 60 min changes
  policy, not correctness. Seed it in IFreedomConfigService.
```
The rule is MACHINE. The parameters of the rule may be FREEDOM.

### Fabric interface vs concrete type
```
MACHINE: the public interface (IDocumentStore, ISchedulerService, IOutboxStore)
  → cannot change without breaking all callers; lives in an interface.

FREEDOM: the implementation chosen at runtime
  → InMemoryDocumentStore now, a persistent store later — both implement
    IDocumentStore. Selected via DI registration, not hardcoded `new`.
```

---

## The annotation format

Annotate the classification explicitly in code:

```csharp
// MACHINE: idempotency reservations must expire — the de-dup guarantee depends on it
private static readonly TimeSpan ReservationFloor = TimeSpan.FromSeconds(1);

// FREEDOM: resend rate limit — seeded in IFreedomConfigService, tunable per tenant
var rate = await _freedom.GetAsync("flow01_resend_rate_limit_minutes", ct);
```

---

## Hardcoded value detection

Run periodically to find FREEDOM values incorrectly hardcoded:

```bash
# Magic numbers in production code (potential FREEDOM values that should be seeded/config)
grep -rn "= 0\.[0-9]\|= [0-9][0-9][0-9]" src/ --include="*.cs" | grep -v "const\|//" | grep -v -i "test"

# Connection strings / secret-looking literals that must NOT be in source (No-Secrets MACHINE)
grep -rni "AKIA\|BEGIN .*PRIVATE KEY\|password=\|Bearer " src/ --include="*.cs" | grep -v "//"
# Expected: 0 hits — secrets live in Vault/config, never in the repo
```

Every hit is a potential MACHINE/FREEDOM classification error, or a No-Secrets violation.

---

## Anti-patterns
1. **"It's just a default, change it later"** — defaults become dependencies; if
   it's FREEDOM, seed it in `IFreedomConfigService` now.
2. **"The threshold is internal, no caller will change it"** — that is the
   definition of MACHINE; add an xUnit presence/contract test.
3. **Inventing `ISchedulerService` poll-interval / `IOutboxStore` batch-size
   config** — the interfaces do not expose them; reference `FreedomConfig`.
4. **Putting fabric interface methods on concrete types** — interface = MACHINE
   (stable); implementation = FREEDOM (swappable via DI).
5. **Reading a MACHINE invariant from config** — a "constant" changeable by
   config is not a constant; and a secret read from source is a No-Secrets break.

---

## Connection to platform rules

| Rule | MACHINE/FREEDOM application |
|------|-----------------------------|
| `DataProcessResult<T>` everywhere | The return-type pattern is MACHINE — never bypass with nullable/raw bool |
| Fabric-First | The interface is MACHINE; the concrete class is FREEDOM (DI-selected) |
| Tenant scope (`ITenantContextAccessor`) | MACHINE — never relax per call |
| No-Secrets-in-repo | MACHINE invariant "not in source"; the value itself comes from Vault/config |
| Idempotency TTL, resend rate, branding strings | FREEDOM — seeded in `IFreedomConfigService` with safe defaults |

## Integration
- `four-tier-decision-classification-SKILL.md` extends this with LEARNABLE / AI-PIPELINE tiers for NEW components.
- `solution-scope-gate-SKILL.md` Level 2 (ADAPTATION) wraps an external provider behind `IConfiguration`/a fabric interface.
- `no-product-decisions-SKILL.md` escalates the *choice of default* when it is product scope.
