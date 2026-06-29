# Iron Rule Derivation (XIIGen Enterprise Platform)

> Ported from the `llm_mvp_core` standard, adapted for **.NET 10 ASP.NET Core Web API**,
> xUnit, Gate-B = `dotnet build` / `dotnet test`. **Subjects** are ASP.NET Core Web API
> services. **FREEDOM** values are read through the real
> `IFreedomConfigService.GetAsync(key)` → `DataProcessResult<FreedomConfigValue?>`
> (`src/Xiigen.Freedom/FreedomConfig.cs`), **not** raw `IConfiguration`/`appsettings.json`
> (this repo holds no appsettings). The DNA "never throw for business logic" principle is
> cited in the live stack as the `DataProcessResult<T>` return-type requirement
> (`src/Xiigen.Kernel/DataProcessResult.cs`) — **not** rewritten, and **not** core
> `OperationResult<T>` (0× in this repo's `src/`). Every rule yields a `[Fact]` xUnit test.

## Core Problem

A test that catches the wrong violation wastes cycles. A test that passes because the rule
it checks is vague produces false confidence. Iron rules — domain-specific behavioral
invariants — are only valuable if they are derived from specific failure modes, stated
without implementation detail, and carry a testable failure consequence. This skill defines
the derivation sequence that produces rules meeting that standard.

---

## When to Invoke

- Before writing any new `[Fact]`/`[Theory]` test class for a service capability.
- After a capability's failure modes have been identified.
- When a test is failing for "obvious reasons" that have never been written down.
- When a new service interface is designed in the Application layer.

Iron rules derived before implementation = tests that catch the actual failure mode, not
incidental surface behaviors.

---

## Section 1 — The Derivation Sequence

### Step 1: List failure modes

Start from the capability's stated contract. Then ask the platform failure-mode set:

```text
- What happens if this operation runs twice?                         (idempotency)
- What if the document upsert succeeds but the outbox append does not? (atomicity / outbox)
- What if a scheduled job is replaced without releasing the old one?  (lifecycle race)
- What if a caller gets a document from the wrong tenant?             (isolation)
- What if a FREEDOM value changes at runtime?                         (MACHINE vs FREEDOM)
- What if the operation completes but no CloudEvent is emitted?       (observability gap)
- What if two callers reserve the same idempotency key at once?       (concurrency)
```

Write each failure mode as a sentence, e.g.:
"Caller invokes `RegisterUserAsync` twice with the same email, creating two records."

### Step 2: For each failure mode, find the invariant

Ask: "What condition, if always true, would prevent this failure?"

```text
Failure:    RegisterUserAsync called twice with same email -> duplicate user records
Invariant:  Registration is idempotent on email — a repeat returns the cached result, not a new record
```

The invariant must be implementation-neutral. No C# class names. It describes what must be
true, not how to make it true.

### Step 3: Express as an iron rule

```text
Rule N: [subject] MUST [verb] [condition] — [failure consequence if violated]
```

The consequence clause is **mandatory** — a rule without a stated consequence cannot be
tested. Examples in the platform stack:

```text
Rule 1: Every write that emits a domain event MUST persist via IDocumentStore before
        appending to IOutboxStore — violation loses the event on partial failure.
Rule 2: Every IDocumentStore query MUST be scoped by the current tenant
        (ITenantContextAccessor.Current) — violation returns cross-tenant data to callers.
Rule 3: Business-logic failures MUST return DataProcessResult.Failure("CODE","msg"), never throw
        — violation breaks DataProcessResult propagation silently at every call site.
Rule 4: An idempotency reservation MUST be obtained before a side-effecting operation runs
        — violation duplicates the side effect on retry.
Rule 5: Tunable thresholds MUST come from IFreedomConfigService.GetAsync(key) with a safe default
        — violation hardcodes a value operators cannot change without redeployment.
```

### Step 4: Classify as MACHINE or FREEDOM

**MACHINE:** the invariant must always hold; no configuration can relax it. Hardcode the
rule; add an xUnit presence/contract test. Never in config.

> Example: "store via `IDocumentStore` before `IOutboxStore.AppendAsync` on every event"
> is MACHINE — partial-failure recovery depends on the outbox pattern.

**FREEDOM:** the mechanism is fixed but a *parameter* of it is tunable. Read it from
`IFreedomConfigService` with a safe default.

> Example: the rule "an idempotency reservation must expire" is MACHINE, but the specific
> `TimeSpan ttl` passed to `IIdempotencyStore.TryReserveAsync(operationKey, ttl)` is FREEDOM:

```csharp
// FREEDOM: reservation TTL — seeded in IFreedomConfigService, tunable per tenant
var ttlResult = await _freedom.GetAsync("idempotency_reservation_ttl_seconds", ct);
var ttl = TimeSpan.FromSeconds(ttlResult.IsSuccess && ttlResult.Value?.Value is int s ? s : 300);
```

Annotate each rule:

```text
Rule 5: Threshold MUST be read from IFreedomConfigService            [MACHINE: rule]
        Key: "idempotency_reservation_ttl_seconds" default 300       [FREEDOM: value]
```

---

## Section 2 — The Quality Test

A well-derived iron rule passes all four checks:

```text
[ ] Can this rule be violated WITHOUT the system throwing?  YES -> it matters (silent violations are dangerous).
    If a violation is structurally impossible -> it does not need an iron rule.
[ ] Is the rule expressed WITHOUT C# class names / impl detail?
    NO -> rewrite: "persist before emit", not "await _docs.UpsertAsync() before _outbox.AppendAsync()".
[ ] Does the rule have a testable failure consequence?
    NO -> add the consequence: what breaks if violated?
[ ] Does the rule derive from a SPECIFIC failure mode?
    NO -> trace it back; if unanswerable, it may not apply to this domain.
```

---

## Section 3 — Rules That Come From DNA (do not repeat them)

The platform DNA rules apply universally. Reference them; do not re-derive them per capability:

```text
DNA: return DataProcessResult.Failure(...), never throw for business logic
  -> in the test, state: "verifies DataProcessResult / never-throw compliance"
  -> do NOT write a new capability-specific iron rule for this — it is universal.

DNA: resolve external dependencies through an interface (Fabric First), not a concrete `new`
  -> verify via a test that the injected dependency type is one of
     IDocumentStore / IOutboxStore / IIdempotencyStore / ISchedulerService / IFreedomConfigService / ITenantContextAccessor.
```

Domain-specific rules are the ones that require derivation. Universal rules are DNA.

---

## Section 4 — Example Derivation

**Capability:** a user-registration service (Application layer).

| Failure mode | Invariant | Iron rule | Class |
|---|---|---|---|
| Same email registers twice | Idempotent on email | Rule 1: Registration MUST be idempotent on email — repeat returns cached result | MACHINE |
| Event emitted but record not stored on crash | Store before emit | Rule 2: MUST upsert via `IDocumentStore` before `IOutboxStore.AppendAsync` — else event lost | MACHINE |
| Cross-tenant user returned | Tenant scope on every query | Rule 3: Every query MUST include tenant scope — violation returns cross-tenant data | MACHINE |
| Empty/invalid input throws | Graceful failure | Rule 4: Invalid input MUST return `DataProcessResult.Failure("VALIDATION_FAILED",...)` — never throw | MACHINE |
| Resend rate hardcoded | Rate tunable | Rule 5: Resend window MUST come from `IFreedomConfigService` (`flow01_resend_rate_limit_minutes`) | MACHINE: rule, FREEDOM: value |

**Result: 5 iron rules from 5 distinct failure modes.** None copied from a template.

---

## Section 5 — Iron Rule Test Pattern (xUnit)

```csharp
[Fact]
public async Task Register_SameEmailTwice_IsIdempotent()
{
    // Verifies Rule 1: idempotency on email
    var first  = await _service.RegisterAsync("a@x.io");
    var second = await _service.RegisterAsync("a@x.io");

    Assert.True(first.IsSuccess);
    Assert.True(second.IsSuccess);
    Assert.Equal(first.Value!.UserId, second.Value!.UserId); // same record, not a new one
}

[Fact]
public async Task Register_InvalidInput_ReturnsFailure_NeverThrows()
{
    // Verifies Rule 4: DataProcessResult.Failure, never throw
    var result = await _service.RegisterAsync("");          // no exception expected
    Assert.False(result.IsSuccess);
    Assert.Equal("VALIDATION_FAILED", result.ErrorCode);
}

[Fact]
public async Task ResendWindow_ComesFromFreedomConfig_NotHardcoded()
{
    // Verifies Rule 5: FREEDOM config via IFreedomConfigService
    var cfg = await _freedom.GetAsync("flow01_resend_rate_limit_minutes");
    Assert.True(cfg.IsSuccess);                              // value is seeded/configurable, not inline
}
```

---

## Anti-Patterns

1. **"Must handle errors correctly"** — not a rule. Which error? What handling?
   Correct: "MUST return `DataProcessResult.Failure('EMPTY_INPUT')` when input is empty."
2. **Copying iron rules from another capability** — different capabilities have different
   failure modes. Derive from this one's failures.
3. **Listing DNA rules as iron rules** — `DataProcessResult`/never-throw and Fabric-First are
   universal. Reference them; do not re-derive per capability.
4. **Iron rules without a failure consequence** — every rule states what breaks if violated.
5. **Implementation-specific rules** — "MUST call `_docs.UpsertAsync()` before
   `_outbox.AppendAsync()`" is a C# detail. Correct: "MUST persist state before emitting events."

## Integration

- `archetype-failure-map-SKILL.md` produces the iron rules that fix PATCH-class failures (its "Fix action" = add an iron rule).
- `freedom-machine-SKILL.md` governs the MACHINE/FREEDOM classification in Step 4.
- `dna-compliance-guard-SKILL.md` enforces `DataProcessResult` / Fabric-First / tenant-scope at code-review time.
- `test-integrity-SKILL.md` ensures the derived `[Fact]` tests assert real outcomes, not `result.IsSuccess`-only stubs.
