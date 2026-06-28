# Data Flow Integrity (XIIGen Enterprise Platform)

## Overview

The "all classes pass individually but the pipeline fails" failure mode. Each
class tests with its own fixtures. But in the live chain, if class A drops a field
from its output, every downstream class receives null/default where it expects a
value — and all of them produce wrong results while their unit tests still pass.

Stack-bound to **.NET 10 ASP.NET Core Web API**, **xUnit** (`tests/Xiigen.*.Tests`),
`DataProcessResult<T>` and the kernel records (`Xiigen.Kernel`), and ASP.NET Core
DI. Platform data contracts are `record` types: `DocumentRecord`,
`CloudEventEnvelope`, `OutboxRecord`, `IdempotencyReservation`,
`ScheduledJobResult`, plus per-service request/response records. Two of those
records carry **`IReadOnlyDictionary<string, object?>`** payloads
(`DocumentRecord.Body`, `CloudEventEnvelope.Data`) — a missing key there is a
null `object?` at runtime, never a compile error, which is exactly where this
failure mode hides. There is no frontend: the DTO contract is server-only.

## When to invoke

- Before writing any class that depends on a producer→consumer chain.
- When unit tests are green but an end-to-end path returns null/empty/wrong data.

## Sub-Agent-Only Parent Boundary

Under a sub-agent-only / architect-only instruction, the schema extraction and
the integration test run inside the bounded sub-agent. The parent reads the report
after the packet returns. `sub-agent waiting is not a stop`.

---

## Section 1 — Build the Data Flow Map

For each chain, document the data contract (producer output → consumer input):

```
Chain: MarketplaceService → IDocumentStore.UpsertAsync → IOutboxStore.AppendAsync

MarketplaceService produces (OrderRecord-shaped body):
  body["orderId"]: string        ← consumed by the read path GetAsync
  body["tenantId"]: string       ← consumed by tenant-scope checks
  body["total"]: decimal (object?) ← consumed by settlement

IOutboxStore.AppendAsync EXPECTS (CloudEventEnvelope):
  Type: string                   ✅ set by the producer
  Subject: string                ✅ set
  Data["orderId"]: object?       ⚠️ present only if the producer copied it into Data
```

---

## Section 2 — Extract Actual Schemas from Live Code

Do not trust fixtures. Read the actual `record`/class definitions and the kernel
contracts:

```bash
# The actual record definition
grep -rn "record DocumentRecord\|record CloudEventEnvelope\|record OutboxRecord" src/Xiigen.Kernel/ --include="*.cs"

# What a consuming service actually reads from its input
grep -rn "\.Body\[\|\.Data\[\|\.Data!\.\|\.Error!\." src/Xiigen.* --include="*.cs" | grep -v "Tests"
```

`DocumentRecord.Body` and `CloudEventEnvelope.Data` are
`IReadOnlyDictionary<string, object?>` — every read of a key returns `object?` and
a missing key is null. Confirm both the producer writes the key and the consumer
reads the same key name with the same expected runtime type.

---

## Section 3 — Schema Diff Per Chain Step

For each (producer, consumer) pair:

```bash
# Keys/fields the consumer uses
grep -rn "\.Body\[\"\|\.Data\[\"\|input\.\|result\.Data\.\|message\." src/Xiigen.<Consumer>/ --include="*.cs" | head -20

# Keys/fields the producer actually provides
grep -rn "\[\"\|new DocumentRecord\|new CloudEventEnvelope\|UpsertAsync(" src/Xiigen.<Producer>/ --include="*.cs" | head -20
```

Any key/field read by the consumer but not produced by the producer = DATA
INTEGRITY GAP.

---

## Section 4 — Classify Each Gap

| Gap type | Description | Fix |
|----------|-------------|-----|
| MISSING_FIELD | Producer omits a key the consumer requires | Add it to the producer's `Body`/`Data` |
| RENAMED_FIELD | Producer writes `"count"`, consumer reads `"totalCount"` | Align the key name in one place |
| TYPE_MISMATCH | Producer stores a `string`, consumer casts to `decimal` | Fix the stored type in the producer |
| NULLABLE_GAP | Key present sometimes; consumer assumes non-null | Add a null guard in the consumer + document the contract |
| NESTED_MISMATCH | Producer flattens; consumer expects nested | Align the structure |

---

## Section 5 — Produce the Integrity Report

```markdown
## Data Flow Integrity Report — [date]

Chain: [Producer] → [Fabric] → [Consumer]

| Link | Producer field/key | Consumer expects | Status |
|------|-------------------|-----------------|--------|
| Order→Upsert | body["orderId"] | body["orderId"] | ✅ MATCH |
| Upsert→Outbox | Data["orderId"] | Data["orderId"] | ❌ MISSING (producer never copied it into Data) |
| Outbox→Read | GetAsync("orders", id) | DocumentRecord.Body["total"] | ✅ MATCH |

Gaps Found: 1
Fix Required: copy orderId into CloudEventEnvelope.Data in MarketplaceService

Verified By:
  grep -n "Data\[\"orderId\"\]" src/Xiigen.Platform.Marketplace/MarketplaceService.cs → not found (gap confirmed)
  grep -n "Data\[\"orderId\"\]" src/Xiigen.Platform.<Consumer>/<Consumer>.cs → line 52 (usage confirmed)
```

---

## Section 6 — Integration Test for the Chain

After fixing any gap, write an integration test that exercises the chain with
**real DI / real in-memory fabrics — no mock of the chain itself**:

```csharp
[Fact]
public async Task OrderPipeline_EndToEnd_AllFieldsPropagate()
{
    // Arrange: real fabrics, real DI (no mocks of the chain)
    var documents = new InMemoryDocumentStore();
    var outbox = new InMemoryOutboxStore();
    var service = BuildMarketplaceService(documents, outbox);
    using var _ = tenantAccessor.Push(TestTenant());

    // Act: run the full chain
    var submit = await service.SubmitAsync(testOrder);
    var read = await documents.GetAsync("orders", submit.Data!.OrderId);
    var events = await outbox.ListAsync();

    // Assert: no null/empty propagation; the key made it through
    Assert.True(submit.IsSuccess);
    Assert.True(read.IsSuccess && read.Data is not null);
    Assert.True(read.Data!.Body.ContainsKey("total"));
    Assert.Contains(events.Data!, e => e.Event.Data.ContainsKey("orderId"));
}
```

For API-level chains, use `WebApplicationFactory<Program>` to run the real
endpoint → service → fabric path and assert the response carries the expected
fields (not a null/placeholder).

---

## Anti-Patterns

1. **Trusting unit tests as proof of chain integrity** — mocks hide the gaps.
2. **Running the integrity check only on the first class** — the gap is always downstream.
3. **Treating nullable dictionary keys as "low priority"** — a missing `Body`/`Data`
   key is null at runtime and silently corrupts the consumer.
4. **Running after the pipeline is in use** — run before any class depending on the
   chain is written.

---

## Integration

- `mental-debug-SKILL.md` Rule 4 (boundary data lifecycle) is the per-symptom version of this skill.
- `event-driven-debugging-SKILL.md` Break Class D uses this skill for read-after-write gaps.
- `root-cause-tracing-SKILL.md` traces a discovered gap back to the producer.
- `integration-testing-SKILL.md` / `e2e-test-matrix-SKILL.md` host the chain integration tests.
