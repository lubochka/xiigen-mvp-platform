# Tracker — IProjectTrackerService (XIIGen Enterprise Platform)

## What This Skill Does

Provides session observability through a swappable tracker adapter. Follows the
platform's fabric-first model: the interface is fixed, the provider is swappable.

Default is always local-file — no configuration needed.

Platform port of the `llm_mvp_core` skill. Stack-bound to **.NET 10 ASP.NET Core
Web API**, **xUnit** (`tests/`), `Xiigen.Platform.slnx`, `DataProcessResult<T>`,
and the v3.3 governance triple. The interface lands NATIVELY on the existing
fabric model: it sits beside `ISchedulerService`, `IOutboxStore`,
`IIdempotencyStore`, `IDocumentStore`, and `ITenantContextAccessor`
(`src/Xiigen.Kernel/FabricContracts.cs`), returns `Task<DataProcessResult<T>>`
(not the core `OperationResult<T>`), and is registered through DI the same way as
the other fabrics (`services.AddSingleton` /
`src/Xiigen.Fabrics.InMemory/XiigenInMemoryFabricServiceCollectionExtensions.cs`).
The local file lives next to `tools/`.

---

## Interface (fixed — provider is swappable)

```csharp
namespace Xiigen.Kernel;

/// <summary>Project tracker adapter — fabric-first, the provider is swappable.</summary>
public interface IProjectTrackerService
{
    /// <summary>Create a new tracker card for a session.</summary>
    Task<DataProcessResult<CardCreated>> CreateCardAsync(
        string phase,       // "SESSION-05", "SESSION-06", etc.
        string session,     // "session-1", "session-2", etc.
        string title,
        string? description = null,
        CancellationToken cancellationToken = default);

    /// <summary>Update the status of an existing card.</summary>
    Task<DataProcessResult> UpdateStatusAsync(
        string cardId,
        CardStatus status,  // InProgress, Complete, Blocked
        string? note = null,
        CancellationToken cancellationToken = default);

    /// <summary>Add a comment to an existing card.</summary>
    Task<DataProcessResult> AddCommentAsync(
        string cardId,
        string comment,
        CancellationToken cancellationToken = default);
}

public sealed record CardCreated(string CardId);
public enum CardStatus { InProgress, Complete, Blocked }
```

All methods return `DataProcessResult` / `DataProcessResult<T>` — never throw for
business logic. The success/failure shape matches `src/Xiigen.Kernel/DataProcessResult.cs`:
`IsSuccess`/`IsFailure`, `Data`, and `Error` (a `DataProcessError(Code, Message, Details)`),
built through `DataProcessResult<T>.Success(data)` and
`DataProcessResult.Failure(code, message)`.

---

## Provider Configuration (swappable)

| Provider | Config | When to Use |
|----------|--------|-------------|
| `local-file` (default) | No config needed | Always — zero-config start |
| `linear` | `TRACKER_PROVIDER=linear` + `LINEAR_API_KEY` | When Linear is configured |
| `jira` | `TRACKER_PROVIDER=jira` + `JIRA_URL` + `JIRA_TOKEN` | When Jira is configured |
| `github` | `TRACKER_PROVIDER=github` + `GH_TOKEN` + `GH_REPO` | When GitHub Issues is configured |

Default is `local-file`. No environment variable = no external service = sessions
still track locally.

---

## Local File Provider

Writes to `tools/tracker/cards.json`.

```json
{
  "cards": [
    {
      "cardId": "SESSION-06-session-1-001",
      "phase": "SESSION-06",
      "session": "session-1",
      "title": "SESSION-06: Idempotency fabric",
      "status": "Complete",
      "createdAt": "2026-06-01T10:00:00Z",
      "updatedAt": "2026-06-01T14:00:00Z",
      "comments": [
        "Starting InMemoryIdempotencyStore implementation",
        "Gate: 205 tests passing, Failed: 0"
      ]
    }
  ]
}
```

Human-readable and durable. If the tracker code fails, Luba can read the JSON
directly.

---

## Session Usage Protocol

**At session start:**
```csharp
// Create a card for this session's work
var result = await tracker.CreateCardAsync("SESSION-06", "session-1", "SESSION-06: Idempotency fabric");
var cardId = result.Data!.CardId; // "SESSION-06-session-1-001"
```

**During the session (at step start):**
```csharp
// Record which step is active
await tracker.AddCommentAsync(cardId, "Starting InMemoryIdempotencyStore — STEP 2");
```

**At session end (gate passed):**
```csharp
// Mark complete with test-gate evidence
await tracker.UpdateStatusAsync(cardId, CardStatus.Complete, "Gate: 205 passed, Failed: 0");
```

**At session end (blocked):**
```csharp
// Record the blocker
await tracker.UpdateStatusAsync(cardId, CardStatus.Blocked, "BLOCKED: idempotency TTL semantics undecided");
```

---

## DI Registration

Register beside the other fabrics, the same way the in-memory fabric registers its
stores in `XiigenInMemoryFabricServiceCollectionExtensions`:

```csharp
// In Program.cs or a fabric service-collection extension
services.AddSingleton<IProjectTrackerService>(provider =>
{
    // Read TRACKER_PROVIDER from environment — defaults to local-file
    var trackerProvider = Environment.GetEnvironmentVariable("TRACKER_PROVIDER") ?? "local-file";

    return trackerProvider switch
    {
        "linear"  => new LinearTrackerService(Environment.GetEnvironmentVariable("LINEAR_API_KEY")),
        "jira"    => new JiraTrackerService(/* ... */),
        "github"  => new GitHubTrackerService(/* ... */),
        _         => new LocalFileTrackerService("tools/tracker/cards.json")
    };
});
```

---

## Gate Evidence

A card is marked `Complete` only after the native Gate-B passes:
```text
dotnet test Xiigen.Platform.slnx -> Failed: 0
```
Record the exact count in the card note. The tracker is observability, not the
source of truth — the source of truth is
`XIIGEN-ENTERPRISE-ADAPTATION-STATE-v3.3.json` and the completion matrix.

---

## Anti-Patterns

1. **Update status before the gate passes** — mark Complete only after `dotnet test Xiigen.Platform.slnx` shows Failed: 0
2. **Use the tracker as the source of truth** — the state file and completion matrix are the source of truth; the tracker is observability only
3. **Hardcode the provider** — always read from the environment variable so the provider is swappable
4. **Skip the tracker when blocked** — a Blocked status with reason is more useful than silence
5. **Return the core `OperationResult<T>`** — on this platform the tracker returns `DataProcessResult<T>` like every other fabric; `OperationResult<T>` stays in `llm_mvp_core` and must not leak here
