# Integration Testing (XIIGen Enterprise Platform)

## Overview

An "integration test" here means a test that wires **two or more real
implementations** together and exercises a cross-class code path, with **no
stubs on the interfaces directly under test**. The universal rules are: a fabric
coverage matrix (every implementation has at least one integration test with real
inputs and a real-output assertion), no shared/static state between tests, and a
fresh instance per test.

This is the platform port of the `llm_mvp_core` skill of the same name (itself a
translation of xiigen `docker-local-testing`). The transport — containers vs
in-process — is an adaptation detail, **not** part of the universal rule. On this
platform there is no container layer for the fabrics under test: every
implementation is pure in-process .NET, so integration tests create real
instances directly in test code.

Stack binding:

- **.NET 10**, **xUnit** test projects under `tests/`;
- `DataProcessResult<T>` in `Xiigen.Kernel` (`IsSuccess` / `Data` / `Error`);
- real fabric interfaces `IDocumentStore`, `IIdempotencyStore`, `IOutboxStore`
  (`src/Xiigen.Kernel/FabricContracts.cs`), `ISchedulerService`
  (`src/Xiigen.Fabrics.Scheduler/Scheduler.cs`), `ITenantContextAccessor`
  (`src/Xiigen.Kernel/TenantContext.cs`);
- real implementations in `Xiigen.Fabrics.InMemory`, `Xiigen.Fabrics.Scheduler`,
  `Xiigen.Auth`, `Xiigen.Platform.Marketplace`.

## When to load

- Before writing any test that exercises more than one class together.
- When a unit test uses a stub for a fabric that should be the thing under test.
- When a phase gate requires fabric coverage.
- When an equivalent xiigen test used Docker/Compose (translate it to in-process).

## What "integration test" means in this project

A `[Fact]` or `[Theory]` that constructs real implementations and exercises a
path that crosses class boundaries. No stubs on the interfaces involved in the
behaviour under test. Each test creates its own fresh instances — no containers,
no sidecars, no shared state.

## Fabric coverage matrix

Every fabric/kernel implementation must have at least one integration test that
exercises it with real inputs and verifies real output. This replaces the
Docker-Compose "container up" coverage check.

### Kernel fabric (`Xiigen.Fabrics.InMemory` implementing `Xiigen.Kernel` contracts)

| Implementation | Must test | Assertion |
|----------------|-----------|-----------|
| `IDocumentStore` upsert + get | Upsert a `DocumentRecord`, then get it back | `Assert.Equal(savedBody["title"], got.Data!.Body["title"])` |
| `IDocumentStore` search | Upsert two records, search by filter; only the match returns | `Assert.Single(results.Data!)` |
| `IDocumentStore` version conflict | Upsert with a stale `expectedVersion` | `Assert.True(result.IsFailure)` + assert `result.Error!.Code` |
| `IIdempotencyStore` reserve/replay | Reserve the same key twice | `Assert.Equal("op-1", second.Data!.Key)` |
| `IIdempotencyStore` mark succeeded | Reserve, then `MarkSucceededAsync`; re-reserve sees the cached response | assert status/cached response on the reservation |
| `IOutboxStore` append + list | Append a `CloudEventEnvelope`, then list | `Assert.Single(list.Data!)` + assert the event id |

### Scheduler fabric (`Xiigen.Fabrics.Scheduler` implementing `ISchedulerService`)

| Implementation | Must test | Assertion |
|----------------|-----------|-----------|
| Enqueue + due selection | Schedule an item, advance a controlled clock, poll due | `Assert.Single(due.Data!)` (see e2e-test-matrix Category 5 for the clock) |
| Not-yet-due exclusion | Schedule for the future, poll now | `Assert.Empty(due.Data!)` |

### Auth + tenant (`Xiigen.Auth`, `ITenantContextAccessor`)

| Implementation | Must test | Assertion |
|----------------|-----------|-----------|
| Tenant-scoped read | Set tenant context, read a tenant-owned record | data returned for the right tenant only |
| Cross-tenant denial | Set tenant A context, request tenant B data | `Assert.True(result.IsFailure)` + assert `result.Error!.Code` |

## Test structure pattern

```csharp
// Integration test — no stubs on the components being tested
[Fact]
public async Task DocumentStore_UpsertThenGet_ReturnsSameBody()
{
    // Arrange: real InMemory fabric (the thing under test)
    IDocumentStore store = new InMemoryDocumentStore();   // real, fresh per test

    var body = new Dictionary<string, object?> { ["title"] = "Terms of Service" };

    // Act: upsert, then read back
    var upsert = await store.UpsertAsync("legal", "tos", body);
    var got    = await store.GetAsync("legal", "tos");

    // Assert: real fabric produced the real record (read DataProcessResult, not exceptions)
    Assert.True(upsert.IsSuccess);
    Assert.True(got.IsSuccess);
    Assert.Equal("Terms of Service", got.Data!.Body["title"]);
}
```

## What replaces Docker Compose setup/teardown

| Docker concept | This project equivalent |
|----------------|------------------------|
| `docker compose up -d` before the run | Each `[Fact]` constructs its own instance in the test body |
| Container isolation (clean state per test) | `new InMemoryDocumentStore()` / `new InMemoryIdempotencyStore()` per test — no shared state |
| `docker compose down` after the run | .NET GC — no explicit teardown |
| `docker logs` for debugging | `result.Error!.Code` / `result.Error.Message` from `DataProcessResult` |
| Health check before tests | Build step: `dotnet build Xiigen.Platform.slnx` must succeed before any test runs |

## Stubs that ARE allowed in integration tests

Stubs are allowed only for components that are **not** part of the behaviour
under test.

| Interface | Allowed stub | When |
|-----------|-------------|------|
| `ITenantContextAccessor` | a fixed-tenant fake | when testing a fabric whose tenant scope is fixed and not under test |
| an outbound notifier / external client | a mock | when testing orchestration, not the external dependency |
| `ISchedulerService` | a no-op fake | when testing a consumer that only enqueues and the scheduler itself is not under test |

Stubs are **NOT** allowed for the fabric under test:

- `IDocumentStore` / `InMemoryDocumentStore` when document persistence is the behaviour;
- `IIdempotencyStore` when idempotency semantics are the behaviour;
- `IOutboxStore` when outbox append/list is the behaviour;
- `ISchedulerService` / the scheduler implementation when scheduling is the behaviour;
- the `Xiigen.Auth` service when auth/tenant isolation is the behaviour.

## Error reading (idiom used by the real services)

`DataProcessResult<T>` exposes failure detail on `Error`, a
`DataProcessError(Code, Message, Details)`. There is **no** `.ErrorMessage` or
`.ErrorCode` member on the type. Read errors the way `SmartWikiService.cs` does:

```csharp
// non-null on the failure path:
var code = result.Error!.Code;
var message = result.Error.Message;
var details = result.Error.Details;

// with a fallback when the inner error may be null:
var code2 = stored.Error?.Code ?? "DOCUMENT_UPSERT_FAILED";
```

## Gate-B format for integration tests

Every plan phase that includes integration tests must state a Gate-B in the form:

```text
Gate B: dotnet test --filter "FullyQualifiedName~<ClassName>" exits 0;
        <ClassName>.cs contains a [Fact] covering <specific behaviour>;
        test count >= <N> (was <M>)
```

> The active `SESSION-XX` file governs when the broad `dotnet test` gate opens.

## Anti-patterns

### Anti-pattern 1: Stub replacing the thing under test
```text
X Testing the idempotency consumer with a stub IIdempotencyStore that always
  returns Success — the duplicate-key path is never exercised; the test is vacuous

V Provide the real InMemoryIdempotencyStore; only stub the genuinely-out-of-scope
  dependency (e.g. an external notifier)
```

### Anti-pattern 2: Shared state between tests
```text
X static InMemoryDocumentStore _shared = new();  // polluted by earlier tests
  results depend on run order — flaky

V Create a new InMemoryDocumentStore() per test; no static fields in test classes
```

### Anti-pattern 3: Testing only the happy path
```text
X Only test IDocumentStore.UpsertAsync succeeds — never test the version-conflict
  failure or a search that returns nothing

V At least one test per implementation exercises the error/empty path, asserting
  result.IsFailure and the specific result.Error!.Code
```
