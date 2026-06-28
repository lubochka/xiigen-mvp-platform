# Defense-in-Depth Validation (XIIGen Enterprise Platform)

## Overview

Adding validation at one place feels sufficient, but a single check can be
bypassed by a different code path, a refactor, or a test mock. Several echelons
make a bug **structurally impossible**.

```text
Single validation: "We fixed the bug."
Multiple layers:   "We made the bug structurally impossible."
```

This is the platform port of the `llm_mvp_core` skill of the same name. The
universal content is the **four validation layers**, the rule that every layer
**returns a typed failure — never throws** for a business condition, and the rule
that each layer is **one test case** (paired with `bug-to-tests-SKILL.md`). It is
bound to **.NET 10 ASP.NET Core Web API**, `DataProcessResult<T>` in
`Xiigen.Kernel` (`Failure(code, message, details)`), and the platform boundaries
`Xiigen.Platform.Api` / `Xiigen.Auth`. Backend-only — no `na` exclusions.

> The result wrapper is the platform's `DataProcessResult<T>`, **not** the
> `llm_mvp_core` `OperationResult<T>` (which stays in core and does not leak here).

## When to invoke

- After `root-cause-tracing` identifies the source of invalid data.
- When a bug was caught only at a deep call site (the symptom was far from the
  cause).
- After any fix to a method that receives untrusted or externally-provided input.
- When adding a service that crosses a system boundary (HTTP input, file I/O, IPC).

## The four layers

### Layer 1 — Entry-point validation (system boundary)

Reject obviously invalid input at the first point it enters the system — at the
`Xiigen.Platform.Api` endpoint / DTO boundary or the `Xiigen.Auth` boundary.

```csharp
// V Layer 1: reject at the system boundary, return a typed failure
public DataProcessResult<PublishedDocument> Publish(string tenantId, string documentId, string body)
{
    if (string.IsNullOrWhiteSpace(tenantId))
        return DataProcessResult<PublishedDocument>.Failure("INVALID_TENANT_ID", "tenantId cannot be empty");

    if (string.IsNullOrWhiteSpace(documentId))
        return DataProcessResult<PublishedDocument>.Failure("INVALID_DOCUMENT_ID", "documentId cannot be empty");
    // ... proceed
}
```

### Layer 2 — Business-logic validation

Ensure the data makes sense for the specific operation, inside the `.Application`
/ service layer.

```csharp
// V Layer 2: validate invariants at the business-logic level
public DataProcessResult<PublishedDocument> PublishVersion(DocumentRecord current, string newBody)
{
    if (string.IsNullOrEmpty(newBody))
        return DataProcessResult<PublishedDocument>.Failure("EMPTY_BODY", "document body cannot be empty");

    if (current.Version is null)
        return DataProcessResult<PublishedDocument>.Failure("MISSING_VERSION", "current record has no version");
    // ... proceed
}
```

### Layer 3 — Environment guards

Prevent dangerous operations in specific contexts (test vs production) — important
for destructive fabric operations.

```csharp
// V Layer 3: refuse destructive operations outside a safe context
public DataProcessResult<bool> ClearCollection(string collection, string storagePath)
{
    // In test environments, refuse to clear paths outside the test temp directory.
    var testRoot = Environment.GetEnvironmentVariable("TEST_TEMP_ROOT");
    if (testRoot is not null && !storagePath.StartsWith(testRoot, StringComparison.Ordinal))
        return DataProcessResult<bool>.Failure(
            "UNSAFE_TEST_OPERATION",
            $"refusing to clear a collection outside the test temp root during testing: {storagePath}");
    // ... proceed
}
```

### Layer 4 — Debug instrumentation

Capture context for forensics when other layers fail.

```csharp
// V Layer 4: log context at the call site for forensic analysis
public async Task<DataProcessResult<DocumentRecord>> UpsertAsync(
    string collection, string id, IReadOnlyDictionary<string, object?> body,
    string? expectedVersion = null, CancellationToken ct = default)
{
    _logger.LogDebug(
        "UpsertAsync called: collection={Collection}, id={Id}, expectedVersion={Version}, bodyKeys={Keys}",
        collection, id, expectedVersion, string.Join(",", body.Keys));
    // ... proceed
}
```

## Applying the pattern

When you find a bug that reached a deep call site:

1. **Trace the data flow** — where did the bad value originate? Where was it used?
2. **Map all checkpoints** — list every point the data passes through.
3. **Add validation at each layer** — entry, business logic, environment, debug.
4. **Test each layer independently** — verify that bypassing Layer 1 is caught by
   Layer 2.

## .NET platform example

Bug: a `null` document id caused a `NullReferenceException` deep in the document
store path.

**Data flow:**
1. API request -> `documentId = null`
2. `LegalPublicationService.Publish(tenantId, documentId, body)`
3. `IDocumentStore.UpsertAsync(collection, documentId, body)`
4. internal key composition `collection + ":" + documentId` -> throws

**Four layers added:**
- Layer 1: the API/service boundary validates `documentId` is not null/empty and
  returns `DataProcessResult.Failure("INVALID_DOCUMENT_ID", ...)`.
- Layer 2: `PublishVersion` validates the record invariants before calling the
  store.
- Layer 3: if `TEST_TEMP_ROOT` is set, refuse destructive paths outside it
  (prevents accidental production writes from test code).
- Layer 4: log parameters at `UpsertAsync` entry, including the body keys.

**Result:** the bug is structurally impossible — it is caught at Layer 1 before
reaching Layer 4.

## DataProcessResult enforcement (kernel Rule 1)

Every layer returns `DataProcessResult<T>` — it **never throws** for a business
condition.

```csharp
// V validation returns a typed failure, never throws
if (invalidCondition)
    return DataProcessResult<T>.Failure("ERROR_CODE", "reason");
```

A validation layer that throws instead of returning `Failure` is a kernel Rule 1
violation — and a `test-integrity` defect if a test accepts the throw as the
"expected failure".

## Each layer is one test case

Pair this skill with `bug-to-tests-SKILL.md`: **one failing test per layer** before
the check is added. For example:

```csharp
[Fact] public void Publish_NullDocumentId_ReturnsInvalidDocumentId() { /* Layer 1 */ }
[Fact] public void PublishVersion_EmptyBody_ReturnsEmptyBody()       { /* Layer 2 */ }
[Fact] public void ClearCollection_OutsideTestRoot_ReturnsUnsafe()   { /* Layer 3 */ }
```

Assert the specific `result.Error!.Code` at each layer, not just `result.IsFailure`.

## Integration

- Load after `root-cause-tracing` identifies the bug source.
- Use `blast-radius` to find all callers before adding Layer 1 validation.
- Each validation layer produces one test case (`bug-to-tests-SKILL.md`: one
  failing test per layer before the check).
- `test-integrity-SKILL.md` enforces the return-Failure-not-throw rule and the
  behavioral-assertion-gate.
