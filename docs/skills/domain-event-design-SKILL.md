# Domain Event Design (XIIGen Enterprise Platform)

## What This Skill Prevents

Event type names that do not form a coherent vocabulary. Payload shapes that drop
fields across refactors, breaking downstream consumers silently. Consumers that
couple to internal state that should not cross boundaries.

Platform port of the core `domain-event-design` skill, re-bound to
**.NET 10 ASP.NET Core Web API**, **xUnit**, and the platform event transport
`IOutboxStore` + `CloudEventEnvelope` (`src/Xiigen.Kernel/FabricContracts.cs`).
Events are published through the durable outbox, scoped by `ITenantContextAccessor`,
and correlated by `ICorrelationContextAccessor`. Gate-B is
`dotnet build Xiigen.Platform.slnx` + `dotnet test`.

> **Naming note vs core.** Core uses the `[Entity][PastVerb]Message` convention for
> in-process `IAgentMessageBus` records. On platform the durable wire identity is the
> `CloudEventEnvelope.Type` **string constant**. Define the past-tense name as a
> `const string` and use a matching `record` only for the strongly-typed payload that
> is serialized into `Data`.

## When to load

- Before defining any new event type appended through `IOutboxStore`.
- When an event crosses a project boundary (publisher in one `Xiigen.*` project,
  consumer in another).
- When an event's subscriber count grows past 5.

---

## Rule 1 — Naming Convention

Event type identity is the string `CloudEventEnvelope.Type`, named
`[Entity][PastTenseVerb]`:

```csharp
// CORRECT — string constant is the wire identity; record carries the payload
public static class WikiEventTypes
{
    public const string CapabilityIndexed = "CapabilityIndexed";
    public const string CapabilityPromoted = "CapabilityPromoted";
}

public sealed record CapabilityIndexedPayload(
    string CapabilityId,
    string CapabilityClass,
    int NodeCount,
    DateTimeOffset IndexedAt);

// WRONG
public const string Cap = "Capability";              // no verb — what happened?
public const string CapabilityIndexedEvent = "...";  // "Event" suffix in the name
public const string CapabilityIndex = "...";         // noun, not past-tense verb
public const string CapabilityWasIndexed = "...";    // "Was" — use simple past
```

For state-machine transitions, use the specific state name:
```
CapabilityCreated → CapabilityIndexed → CapabilityPromoted
NOT: CapabilityUpdated (ambiguous — what update?)
```

---

## Rule 2 — Payload Design

The payload serialized into `CloudEventEnvelope.Data` carries only what consumers
NEED:

```csharp
// CORRECT — stable identifiers, no internal state, enough to act
public sealed record CapabilityIndexedPayload(
    string CapabilityId,      // stable reference — consumers can look up details
    string CapabilityClass,   // needed for routing decisions
    int NodeCount,            // needed without a callback lookup
    DateTimeOffset IndexedAt);// needed for ordering / timeout tracking

// WRONG — internal state, mutable objects, internal encodings
public sealed record CapabilityIndexedPayload(
    int DatabaseRowId,        // internal — unstable if storage changes
    List<GraphNode> Nodes,    // mutable reference — consumers can mutate shared state
    byte[] SerializedState);  // internal encoding — coupling to implementation
```

**Payload rules:**
1. **Stable identifiers, not internal ids.** Use `CapabilityId`, not a storage row
   key. On platform the stable scope ids come from
   `ITenantContextAccessor.Current.TenantId` and the entity's own id.
2. **Never include mutable objects.** Use `IReadOnlyList<T>` and `record`s, not
   `List<T>` or classes with setters.
3. **Enough to act without a lookup.** If most consumers need `CapabilityClass`
   alongside `CapabilityId`, include both.
4. **Additive evolution only.** Future versions add fields; they never remove or
   rename existing fields.

---

## Rule 3 — Subscriber Count Awareness

When an event type has more than 5 consumers, it is shared infrastructure and needs
careful design. On platform, consumers read from the outbox / their own handler
registrations:

```
CapabilityIndexed consumers:
  1 search-reindex handler
  2 audit/logging handler
  3 metrics handler
  ...
At 5+ consumers: consider a more specific event type
  (CapabilityIndexedForSearch vs CapabilityIndexedForAudit).
```

---

## Rule 4 — Cross-Project Event Contracts

When an event crosses a project boundary (`Xiigen.Platform.Marketplace` publishes →
another project consumes), it is a cross-project contract with stricter rules:

1. **Schema is frozen once a consumer exists.** After a consumer reads
   `CapabilityIndexed`, its `Data` shape cannot change without a version bump.
2. **Document it as a public contract** alongside the `Xiigen.Kernel` contracts /
   interfaces doc.
3. **Include the minimum viable payload for all current consumers** — do not find the
   lowest common denominator; include what all current consumers need.
4. **Version when the schema changes** — `CapabilityIndexedV2` (new `Type` constant),
   not a silent change.

---

## Step-by-Step: Design an Event

```
1. Name entity + verb: Capability + Indexed → "CapabilityIndexed" (const string Type).
2. Naming check: past tense? clear entity? no "Event"/"Was"/noun? ✅
3. List who publishes: SmartWikiService after persisting the graph.
4. List who consumes: search-reindex, audit, metrics.
5. Subscriber count check: ≤5? ✅
6. Payload: what does each consumer NEED? Minimum first; add only required fields.
7. Cross-project check: does it cross a project boundary?
   YES → document as a public contract; frozen-schema rule applies.
8. Immutability: record + IReadOnly collections? ✅
9. Correlation: envelope Extensions carry correlationid/tenantid/traceparent.
10. Verify the new Type does not duplicate an existing one's state transition.
```

---

## The Event Vocabulary

Maintain a consistent vocabulary. Before adding a new event type, verify:
- No existing event covers the same state transition.
- The name follows Entity+PastVerb (string constant).
- The granularity is consistent with similar events.

```
Current event vocabulary (example):
  CapabilityIndexed     ← xiigen-smart-wiki, graph persisted
  CapabilityPromoted    ← xiigen-smart-wiki, confidence threshold reached
  ScheduledJobCompleted ← scheduler, delayed job ran
```

---

## Test Strategy (per branch)

- **Unit (xUnit):** a naming test asserts every published `Type` matches
  `^[A-Z][A-Za-z]+[a-z]ed$`-style past-tense (or an approved vocabulary list), and the
  payload record exposes only `IReadOnly`/value members.
- **Integration:** publish through `InMemoryOutboxStore`; `ListAsync` returns the
  event; deserialize `Data` into the payload record and assert real fields.
- **Cross-project freeze:** a regression test pins the `Data` keys of an event that
  already has a consumer (adding is allowed, removing/renaming fails the test).
- **Gate-B:** `dotnet build Xiigen.Platform.slnx` (0/0) + `dotnet test` (0 failed).

---

## Anti-Patterns

1. **Generic event names** — `StatusChanged` covers nothing specifically.
2. **Mutable payloads** — `List<T>` in a payload record can be mutated by consumers.
3. **Including internal ids** — a storage row key in `Data` crosses internal
   boundaries.
4. **Cross-project events not documented** — they are public contracts; document them.
5. **Removing/renaming a field on an existing event** — always additive; version with
   a new `Type` for breaking changes.

---

## Integration

- `event-contract-design-SKILL.md` defines the full envelope schema (correlation,
  compensation, SLA, INJECTABLE/PLATFORM_ONLY) for events named here.
- `IOutboxStore` + `CloudEventEnvelope` are the transport (`Xiigen.Kernel`).
- `change-propagation-SKILL.md` keeps the event vocabulary doc in sync when a Type is
  added or versioned.
