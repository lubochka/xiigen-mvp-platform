# Event-Driven Debugging (XIIGen Enterprise Platform)

## Overview

Debugging a consumer when the event was never appended. Checking the wrong
handler when the real break is two hops downstream. Spending time on service
internals when the outbox/scheduler state shows the break immediately. This skill
finds WHICH break class is the case before touching any code.

Stack-bound to **.NET 10 ASP.NET Core Web API**, **xUnit** (`tests/Xiigen.*.Tests`),
`DataProcessResult<T>` (`Xiigen.Kernel`), and the platform asynchronous fabrics:
`IOutboxStore` (append-then-dispatch of `CloudEventEnvelope` / `OutboxRecord`) and
`ISchedulerService` (`ScheduleDelayedAsync` of delayed jobs). **Rule 8 of the DNA
is store-before-dispatch:** the service persists state (`IDocumentStore.UpsertAsync`)
*before* it appends the event (`IOutboxStore.AppendAsync`) or schedules the job.
Every envelope carries a correlation id (the `MicroserviceBase` correlation
context / `CloudEventEnvelope.Id`/`Extensions`), which is the trace key.

If a given service has no event/outbox/scheduler path, this skill is conditionally
applicable — focus the chain analysis on `ISchedulerService` and `IOutboxStore`,
or note "no async chain in this service" and use `data-flow-integrity-SKILL.md`
instead. There is no frontend: no UI event tracing.

## When to invoke

- A final outcome (a persisted record, a scheduled job, a downstream event) is missing.
- An event was appended but nothing consumed it, or the wrong type was appended.

## Sub-Agent-Only Parent Boundary

Under a sub-agent-only / architect-only instruction, the chain definition,
bisection, and capture tests run inside the bounded sub-agent. The parent reads
the result after the packet returns. `sub-agent waiting is not a stop`.

---

## Section 1 — Define the Expected Chain (before any code/log)

Write out the expected chain first:

```
User action: a marketplace order is submitted

Expected chain:
  1. MarketplaceService.SubmitAsync persists the order: IDocumentStore.UpsertAsync("orders", id, body)
  2. THEN (Rule 8) appends the event: IOutboxStore.AppendAsync(CloudEventEnvelope "order.submitted")
  3. THEN schedules follow-up: ISchedulerService.ScheduleDelayedAsync("settle-order", delay, payload, idempKey)
  4. A read path later returns the order via IDocumentStore.GetAsync("orders", id)

Total: 1 persist, 1 outbox append, 1 scheduled job, 1 read.
```

---

## Section 2 — Find the Break Point (Binary Search)

Do not start at step 1. Start in the middle.

```csharp
// Probe the midpoint — confirm the persist landed before the append
var stored = await _documents.GetAsync("orders", id);
Logger.LogDebug("Order persist check: found={Found} corr={Corr}",
    stored.IsSuccess && stored.Data is not null, CurrentCorrelationId);
```

Run the scenario with detailed logging:

```bash
dotnet test Xiigen.Platform.slnx --filter "FullyQualifiedName~OrderFlow" \
  --logger "console;verbosity=detailed" 2>&1 | grep "persist check"
```

- Midpoint shows the record present → the break is downstream (append/schedule/read).
- Midpoint shows it absent → the break is upstream (persist never happened).

Recurse until the break is isolated to a single step.

---

## Section 3 — Classify the Break

Once isolated to one step (state N present, state N+1 absent):

### Break Class A — Event Appended, No Consumer / Subscriber

The `CloudEventEnvelope` is in `IOutboxStore.ListAsync()` but nothing consumes its
type (or the scheduler job action has no registered handler).

```bash
grep -rn "AppendAsync\|order.submitted\|ScheduleDelayedAsync\|\"settle-order\"" src/ --include="*.cs"
# A handler/consumer for the type is missing in DI or in the dispatcher.
```

Fix: register the consumer/handler for that event type or scheduled action.

### Break Class B — Handler Runs, Fails Before Appending

The persist ran, the handler was reached, but it returned `Failure` before
`AppendAsync`/`ScheduleDelayedAsync`.

```csharp
var append = await _outbox.AppendAsync(envelope);
if (!append.IsSuccess)
{
    // Rule 6 — this guard reveals the failure we are hunting
    Logger.LogDebug("append failed: {Code} {Msg}", append.Error!.Code, append.Error.Message);
    return Failure<OrderResult>(append.Error.Code, append.Error.Message);
}
```

Fix: root-cause-trace the `Failure` (`root-cause-tracing-SKILL.md`) and fix the source.

### Break Class C — Wrong Event Type / Action Appended

The handler runs and appends, but the envelope `Type` (or scheduler `action`)
string does not match what the consumer expects.

```bash
grep -rn "Type\s*=\s*\"\|new CloudEventEnvelope\|action:" src/ --include="*.cs" | grep -i "order"
# Compare the appended Type/action to the consumer's expected string (case/name).
```

Fix: align the appended `Type`/`action` with the consumer's expected value.

### Break Class D — Everything Appended, Read Returns Wrong State

The persist, append, and schedule all happened, but a later read returns wrong
data. **Most often a store/cache/tenant-scope issue, not a chain issue.**

```csharp
var read = await _documents.GetAsync("orders", id);
Logger.LogDebug("read-after-write: success={S} data={D}",
    read.IsSuccess, System.Text.Json.JsonSerializer.Serialize(read.Data));
// State correct here but the caller sees wrong data → caching / wrong collection / wrong tenant.
```

Fix: not a chain problem. Check the `collection`/`id`, the tenant context, and any
caching/`FromCache` path.

---

## Section 4 — Verify the Fix Does Not Break Adjacent Chains

After fixing the identified break, run a capture test that asserts the full chain,
plus a second scenario:

```csharp
[Fact]
public async Task OrderFlow_FullChain_AllStepsComplete()
{
    // Arrange: real in-memory fabrics (no mock of the chain itself)
    var documents = new InMemoryDocumentStore();
    var outbox = new InMemoryOutboxStore();
    var scheduler = new InMemorySchedulerService();
    var service = BuildMarketplaceService(documents, outbox, scheduler);

    using var _ = tenantAccessor.Push(TestTenant());

    // Act
    var result = await service.SubmitAsync(testOrder);

    // Assert: persisted, event appended, job scheduled (chain verification)
    Assert.True(result.IsSuccess);
    var stored = await documents.GetAsync("orders", result.Data!.OrderId);
    Assert.True(stored.IsSuccess && stored.Data is not null);
    var events = await outbox.ListAsync();
    Assert.Contains(events.Data!, e => e.Event.Type == "order.submitted");
    var jobs = await scheduler.ListAsync();
    Assert.Contains(jobs.Data!, j => j.Action == "settle-order");
}
```

---

## Section 5 — Break Class by Symptom

| Symptom | Break class | First check |
|---------|------------|-------------|
| State N+1 absent, state N present | A or B | grep for the consumer/handler registration |
| Consumer registered, N+1 still absent | B | log the `AppendAsync`/`ScheduleDelayedAsync` `IsSuccess` |
| N+1 present but wrong type/action | C | compare appended `Type`/`action` to the consumer's expected string |
| All states present but read wrong | D | log read-after-write; check collection/tenant/cache |
| State N itself absent | producer bug | confirm `UpsertAsync` ran BEFORE the append (Rule 8) |

---

## Anti-Patterns

1. **Reading service internals before checking the chain** — check the
   outbox/scheduler/store state first.
2. **Debugging a consumer when the producer never appended** — bisect before blame.
3. **Adding more tests before isolating which step fails** — classify first.
4. **Fixing the wrong service because it's "nearby"** — the bisection locates the
   exact step.
5. **Appending before persisting** — that violates Rule 8 (store-before-dispatch)
   and is itself the bug.

---

## Integration

- `mental-debug-SKILL.md` Rule 6/Rule 8 drive break classes B and A.
- `root-cause-tracing-SKILL.md` finds the source of a Break Class B `Failure`.
- `data-flow-integrity-SKILL.md` for Break Class D schema/read issues, and for services with no async chain.
- `dev-safety-SKILL.md` Rule 8 (persist before dispatch) is the contract this skill assumes.
