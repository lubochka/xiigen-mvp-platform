# End-to-End Test Matrix (XIIGen Enterprise Platform)

## Overview

A capability that has unit tests but no end-to-end scenarios has untested
integration paths. The E2E test matrix defines the 8 mandatory scenario
categories **before** code is written, so happy-path tests do not hide untested
idempotency, compensation, and time-based behaviour.

This is the platform port of the `llm_mvp_core` skill of the same name. The
universal content is the 8 categories, the matrix-before-code discipline, and the
"never `Thread.Sleep` — always a controlled clock" rule. It is bound to **.NET 10
ASP.NET Core Web API**, **xUnit**, `DataProcessResult<T>` (`IsSuccess` /
`Error.Code`), and the platform surfaces `Xiigen.Platform.Api`, `Xiigen.Auth`,
`Xiigen.Kernel`, and `Xiigen.Platform.Marketplace`.

All 8 categories are **API-level and applicable** here. There are no frontend
exclusions — this is a backend Web API. Rate-Limit/Quota (Category 3) and
Security (Category 8) are especially load-bearing for `Xiigen.Platform.Api` and
`Xiigen.Auth`; Boundary (Category 7) is load-bearing for
`Xiigen.Platform.Marketplace` consumers of kernel records.

## When to load

- When designing tests for any capability/endpoint.
- When writing a phase plan for a new capability.
- When a capability processes external events, timers, or long-running operations.
- Before a Gate-B is declared for any pipeline-stage capability.

One test matrix before coding = no "tests pass but the time-based case is
untested" result.

## Section 1 — The 8 mandatory scenario categories

Every capability needs all applicable categories. Mark N/A only with a one-line
justification.

### Category 1 — Happy path (required, >=1 per entry point)

Verifies the capability works end-to-end in the success case.

```csharp
[Fact]
public async Task Capability_HappyPath_ReturnsExpectedResult()
{
    // Arrange: valid input
    // Act: invoke the capability
    // Assert: result.IsSuccess == true; result.Data matches expected; side effects observable
}
```

### Category 2 — Idempotency (required for any capability with writes or state changes)

Verifies that calling the same capability twice with the same input produces the
same result, not doubled effects. The platform has a real `IIdempotencyStore`
(`TryReserveAsync` / `MarkSucceededAsync` / `MarkFailedAsync`) — use it.

```csharp
[Fact]
public async Task Capability_CalledTwice_ResultNotDoubled()
{
    // Arrange: identical input with the same operation key
    // Act: invoke twice (TryReserveAsync should reflect the existing reservation on the 2nd call)
    // Assert: store shows 1 logical effect, not 2; second call returns the same result
}
```

### Category 3 — Rate limit / quota (required for entry-point services with call limits)

Verifies that limit+1 calls produce a rejection, not silent double-processing.
Especially relevant for `Xiigen.Platform.Api` endpoints and `Xiigen.Auth`.

```text
Structure: send (limit) requests -> the (limit+1)th -> assert result.IsFailure
           with result.Error!.Code == "RATE_LIMITED" (or the endpoint's documented code)
```

### Category 4 — Compensation chain (required for multi-step operations with rollback)

Verifies that if step N fails, steps 1..N-1 roll back in **reverse (LIFO)** order.

```csharp
[Fact]
public async Task Capability_FailsAtStep3_RollsBackSteps2And1()
{
    // Arrange: succeed steps 1-2, simulate step 3 failure
    // Act: run the pipeline
    // Assert: rollback fired for step 2 BEFORE step 1 (LIFO); result.IsFailure == true
}
```

**Critical:** LIFO order must be asserted explicitly — not just "rollback fired".

### Category 5 — Time-based / delayed (REQUIRED for any capability with timers or TTLs)

Verifies time-dependent behaviour fires at the correct time. Required for the
`ISchedulerService` due-selection path and any `IIdempotencyStore` TTL/expiry.

```csharp
[Fact]
public async Task Capability_ExpiresAfterTtl_EmitsExpiredResult()
{
    // Arrange: create item with TTL = 1 hour, using a controlled clock
    _clock.AdvanceBy(TimeSpan.FromHours(1) + TimeSpan.FromSeconds(1));
    // Act: check for expiry / poll due
    // Assert: result reflects the EXPIRED / due state
}
```

**Rule:** NEVER use `Thread.Sleep`, `Task.Delay`, or real-time delays in
time-based tests. Always inject a controllable clock (see Section 3).

### Category 6 — State recovery (required for long-running or multi-step operations)

Verifies that if a process is interrupted and resumed, state is correct.

```csharp
[Fact]
public async Task Capability_AfterInterruption_RestoresCorrectState()
{
    // Arrange: run through N steps, persist state (e.g. via IDocumentStore / IOutboxStore)
    // Act: simulate a new process (new service instance, same store)
    // Assert: recovered state matches expected; no step is double-executed
}
```

### Category 7 — Boundary assertions (required for capabilities with cross-component contracts)

Verifies that components depending on this capability's output get the correct
data. Load-bearing for `Xiigen.Platform.Marketplace` consuming kernel records.

```csharp
[Fact]
public async Task Capability_Output_MatchesConsumerExpectedShape()
{
    // Assert the exact field values consumers read (result.Data.<field>),
    // not just result.IsSuccess
}
```

### Category 8 — Security boundary (required for multi-tenant or access-controlled capabilities)

Verifies access control, tenant isolation, and auth enforcement. Load-bearing for
`Xiigen.Platform.Api` and `Xiigen.Auth`, using `ITenantContextAccessor`.

```csharp
[Fact]
public async Task Capability_WrongTenant_ReturnsUnauthorized()
{
    // Arrange: set tenant A context, request tenant B data
    // Assert: result.IsFailure == true; result.Error!.Code == "UNAUTHORIZED" (or the documented code)
    // Assert: no data leak across the tenant boundary
}
```

## Section 2 — Test matrix format

Document which categories apply **before** writing any tests:

```markdown
## Test Matrix — [CapabilityName]

| Scenario | Category | Controlled clock | Gate B |
|---|---|---|---|
| Happy path — valid input | 1 (Happy Path) | No | dotnet test -> 1 passed |
| Same input twice -> single effect | 2 (Idempotency) | No | dotnet test -> 1 passed |
| Limit+1 request -> RATE_LIMITED | 3 (Rate Limit) | No | dotnet test -> 1 passed |
| Reservation expires after TTL | 5 (Time-Based) | Yes — inject MockDateTimeProvider | dotnet test -> 1 passed |
| Wrong tenant context | 8 (Security) | No | dotnet test -> 1 passed |

N/A categories with justification:
- Category 4 (Compensation): capability is a single-step write — N/A
- Category 7 (Boundary): capability is a leaf node with no downstream consumer — N/A
```

## Section 3 — Time-based test protocol

Time-based scenarios require a controllable clock. Never use real time.

```csharp
// Inject IDateTimeProvider instead of using DateTimeOffset.UtcNow directly.
public interface IDateTimeProvider { DateTimeOffset UtcNow { get; } }

// Production: reads the system clock.
public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

// Tests: controllable value.
public sealed class MockDateTimeProvider : IDateTimeProvider
{
    private DateTimeOffset _current = DateTimeOffset.UtcNow;
    public DateTimeOffset UtcNow => _current;
    public void AdvanceBy(TimeSpan duration) => _current += duration;
}
```

`DateTimeOffset` is used here to match the kernel records (`DocumentRecord.UpdatedAt`,
`IdempotencyReservation.ExpiresAt`, `OutboxRecord.CreatedAt` are all
`DateTimeOffset`).

## Anti-patterns

1. **"Happy path passes, skip the rest"** — idempotency, compensation, and
   time-based failures are invisible from the happy path. Each category covers a
   distinct failure mode.
2. **"Use `Thread.Sleep`/`Task.Delay` for time-based tests"** — real-time tests
   are slow, flaky, and non-deterministic. Always inject a controllable clock.
3. **"Compensation is tested by checking the final state"** — LIFO order must be
   asserted. Compensation in the wrong order can leave inconsistent data even if
   the final state looks correct.
4. **"Category N/A without justification"** — every N/A needs a one-line reason;
   unlabeled N/A hides missing coverage.

## Integration

- `user-journey-testing-SKILL.md` provides scenario-first testing (scenarios
  before implementation) and the `SCENARIO-MANIFEST`.
- `mvp-round-trip-verification-SKILL.md` Step 5-6 verifies Category 1 (happy
  path); Step 7 verifies Category 7 (boundary assertions).
- `plan-self-validation-SKILL.md` verifies that the matrix covers all stated goal
  elements.
- The active `SESSION-XX` file governs when the broad `dotnet test` Gate-B opens.
