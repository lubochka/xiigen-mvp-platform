# Self-Questioning (XIIGen Enterprise Platform)

## Core Problem

An artifact produced without questioning its own assumptions will contain gaps
the author could have caught — but didn't, because the production process never
paused to ask. This skill defines the QUESTION YOURSELF protocol: the minimum
question set, the required output format, and the downstream checks that confirm
the protocol was applied. An artifact that reaches review without a
self-questioning record is treated as incomplete, regardless of how correct it
appears.

Platform port. The ML/EngineContract/RAG domain question sets from the
`llm_mvp_core` skill are replaced with platform-domain sets: **.NET 10 ASP.NET
Core Web API** contracts, `DataProcessResult<T>` discipline, fabric interfaces,
and the v3.3 governance triple.

## When to Invoke

- Before finalizing any new contract (a fabric interface, an endpoint contract,
  a service signature).
- Before submitting any new service/endpoint implementation for review.
- Before producing a plan/matrix artifact that feeds the v3.3 review loop.
- When a downstream review fires SELF-Q-001 (missing questions).

## Section 1 — QUESTION YOURSELF Protocol

**Minimum: 3 questions. Default: apply all 5 standard questions.**

```
Standard Question Set:

Q1 — Result-shape:
  "Does every public method here return DataProcessResult / DataProcessResult<T>,
   and does it never throw for business failures or return null?"

Q2 — Failure path:
  "Is there a test that exercises the failure path (DataProcessResult.Failure
   with a specific Error.Code), not only the success path?"

Q3 — Dependency boundary:
  "Are dependencies (fabric interfaces, stores, accessors) injected via the
   constructor / DI, or did I `new` a concrete dependency inside business logic?"

Q4 — Configuration:
  "Are any thresholds, TTLs, timeouts, or limits hardcoded? Should they come from
   IConfiguration / IOptions instead?"

Q5 — Verifiability:
  "Is there any branch in this artifact I cannot prove will fire on a real input?
   If so, I need a test for that branch before this artifact is accepted."
```

## Section 2 — Output Format

Every artifact that passes through the protocol must include three fields in its
output record:

```csharp
public sealed record SelfQuestionedArtifact<T>
{
    public required T Artifact { get; init; }
    public required IReadOnlyList<SelfQuestion> SelfQuestions { get; init; }   // minimum 3
    public required IReadOnlyList<string> Answers { get; init; }              // one per question, in order
    public required IReadOnlyList<string> ModificationsFromQuestioning { get; init; }
    // If nothing changed: include at least one entry "No modifications required — <reason>".
    // An empty list is NOT valid: it means the questions were not genuinely applied.
}

public sealed record SelfQuestion
{
    public required string QuestionId { get; init; }   // "SELF-Q-001" or custom
    public required string QuestionText { get; init; }
    public required string Domain { get; init; }       // ENDPOINT / FABRIC / SERVICE / GENERAL
}
```

`ModificationsFromQuestioning` is the most important field. If empty, the
downstream review rejects the artifact. It must contain either a concrete change
made OR an explicit statement of why no change was needed.

## Section 3 — Downstream Validation Checks

```
SELF-Q-001: selfQuestions.Count >= 3
  FAIL: fewer than 3 questions -> return with SELF_QUESTIONING_INCOMPLETE

SELF-Q-002: answers.Count == selfQuestions.Count
  FAIL: not all questions answered -> return with UNANSWERED_QUESTIONS
  (a listed-but-unanswered question is worse than none — protocol started, abandoned)

SELF-Q-003: modificationsFromQuestioning is not empty
  FAIL: ModificationsFromQuestioning.Count == 0 -> return
  "No modifications required — <reason>" PASSES; an empty list does not.
```

## Section 4 — Domain-Specific Question Sets

### Endpoint / Controller artifacts
```
EP1: "Does this endpoint translate a DataProcessResult into the right HTTP
      status (success vs typed failure), rather than throwing?"
EP2: "Is tenant context applied (ITenantContextAccessor) where this endpoint
      touches tenant-scoped data?"
EP3: "Is there an integration test (WebApplicationFactory) that hits this route
      and asserts both the success and a failure response?"
```

### Fabric interface artifacts (ISchedulerService / IOutboxStore / IIdempotencyStore / IDocumentStore)
```
FB1: "Does every method return DataProcessResult<T> and accept a
      CancellationToken, matching the existing fabric contracts in Xiigen.Kernel?"
FB2: "Is idempotency/version handling explicit where the operation can be retried
      (e.g. expectedVersion / idempotencyKey)?"
FB3: "Is there a fresh-fixture test using the real implementation through DI, not
      a mock of this interface?"
```

### Service artifacts
```
SV1: "Does this service compose fabrics through injected interfaces only?"
SV2: "Is there a Fail-path test, not just an Ok-path test?"
SV3: "Are limits/timeouts from IConfiguration, not hardcoded?"
```

## Anti-Patterns

1. **"Filling in selfQuestions[] after writing the artifact"** — the purpose is to
   change the artifact before it is finalized, not to document questions after.
2. **"ModificationsFromQuestioning: []"** — an empty list fails SELF-Q-003; write
   "No modifications required — <specific reason>".
3. **"Using Q1–Q5 verbatim when domain questions apply"** — an endpoint artifact
   that uses generic Q1 instead of EP1 misses the endpoint-specific check.
4. **"Answering questions without changing anything for 3+ sessions"** —
   consistent zero-modification records signal mechanical application; rotate the
   question set or escalate for review.
5. **"Skipping self-questioning for 'simple' artifacts"** — simple-looking
   artifacts most often hide silent errors (hardcoded thresholds, missing
   Fail-path tests, canned `DataProcessResult.Success()`).
