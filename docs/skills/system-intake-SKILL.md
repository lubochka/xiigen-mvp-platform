# System Intake

> Ported from the `llm_mvp_core` system-intake standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build Xiigen.Platform.slnx` / `dotnet test`. Entry points are
> `Xiigen.Platform.Api` minimal-API delegates in `Program.cs` (`MapGet`/`MapPost`),
> **not** MVC controllers. Constraints are `DataProcessResult<T>` boundaries plus
> guard clauses in `Xiigen.Kernel`/`Xiigen.Auth`. FREEDOM config
> (`Xiigen.Freedom`) is a separate intent source. Intent is pulled from the v3.3
> Beacon that `CLAUDE.md` references. Domain return type is `DataProcessResult<T>`
> (never core `OperationResult<T>`).

## Core Problem

A design session that starts without extracting the existing system's structure,
intent, constraints, and quality gaps will propose a new capability that violates
existing invariants. This is the most common source of re-work: the design was
locally coherent but globally wrong because the designer did not know what the
system already enforces. This skill defines the four properties to extract and
the extraction protocol per source type, producing a domain context package that
feeds capability design.

---

## When to Invoke

- Before designing any new capability for an existing platform project (`src/Xiigen.*`).
- Before writing a new v3.3 session plan that extends an existing service.
- Before a challenger round (`challenger-round-design-SKILL.md`) on a new capability.
- Before `problem-decomposition-SKILL.md` on a problem that touches existing code.

---

## Section 1 — Four Properties to Extract

Every intake extracts the same four properties, regardless of source type:

```
structure:   How the system is organized.
             Service class boundaries (MicroserviceBase<T>), the project dependency
             graph (src/Xiigen.* + ProjectReference), API entry points
             (Program.cs MapGet/MapPost), fabric interfaces injected.
             Not file names — the conceptual architecture.

intent:      What the system does for tenants / the master tenant.
             Business rules, invariants, domain concepts. Pulled from the v3.3
             Beacon and the active SESSION-XX file, not only from the code.

constraints: What must always be true.
             DataProcessResult<T> failure boundaries, tenant isolation
             (ITenantContextAccessor), idempotency/outbox ordering, FREEDOM limits.
             Explicit (AGENTS.md / CLAUDE.md / v3.3 contracts) + implicit
             (inferred from guard clauses in existing code).

quality:     How well the system currently serves its intent.
             Known gaps, divergence from intent, brittle spots, missing xUnit
             coverage.
```

---

## Section 2 — Extraction Protocol by Source Type

### C# Source Code (platform primary source type)

```bash
# Step 1: Find entry points (minimal-API endpoints + public service methods)
grep -rn "MapGet\|MapPost\|MapPut\|MapDelete" src/Xiigen.Platform.Api/ --include="*.cs"
grep -rn "public.*DataProcessResult\|public.*Async" src/ --include="*.cs" | grep -v Tests
# These tell you what the platform responds to

# Step 2: Map service boundaries
grep -rn "class .*: MicroserviceBase\|interface I\|: I[A-Z]" src/ --include="*.cs"
# Look for: MicroserviceBase<T> inheritors, fabric interfaces injected

# Step 3: Extract business rules (guard clauses)
grep -rn "DataProcessResult.*Failure\|return Failure\|throw new\|if.*Failure" src/ --include="*.cs" | grep -v Tests
# Each DataProcessResult.Failure(...) is a constraint

# Step 4: Identify event / outbox contracts
grep -rn "IOutboxStore\|CloudEventEnvelope\|AppendAsync\|ScheduleDelayedAsync" src/ --include="*.cs" | grep -v Tests
# These are the event/message and scheduled-work contracts

# Step 5: Find quality signals
grep -rn "TODO\|FIXME\|HACK\|NotImplementedException\|// stub" src/ --include="*.cs"
# Signals of known gaps or incomplete implementation
```

### v3.3 Contracts and Beacon (intent source)

```bash
# Intent for the capability under design lives in the v3.3 session/contract files
ls docs/plans/enterprise-adaptation/v3.3/ | grep -i "[capability]"
# e.g. SMART-WIKI-CONTRACT.md, SMART-CODE-ASSISTANT-CONTRACT.md, TRAINABLE-UNIT-MANIFEST-CONTRACT.md
```

Read the matching `SESSION-XX-*.md` and `*-CONTRACT.md` for the capability. The
contract describes intended behavior; the code describes actual behavior. When
they conflict, the code is truth — record the conflict in `Constraints.Unknowns`.

### FREEDOM Configuration (separate intent source)

`Xiigen.Freedom` carries tunable, tenant-overridable values. Treat each FREEDOM
key as an intent statement ("this value is meant to be configurable per tenant").
A value buried as a `const` in business logic that should be a FREEDOM key is a
quality gap.

---

## Section 3 — Domain Context Package Format

After extraction, produce a domain context package as C# records:

```csharp
public sealed record DomainContextPackage(
    string SourceType,                  // "csharp" | "v3.3_contract" | "freedom" | "mixed"
    IReadOnlyList<string> SourceArtifacts,   // file paths read (with line ranges where relevant)
    StructureRecord Structure,
    IntentRecord Intent,
    ConstraintsRecord Constraints,
    QualityRecord Quality);

public sealed record StructureRecord(
    IReadOnlyList<string> ServiceBoundaries,   // "SmartWikiService owns wiki indexing only"
    IReadOnlyList<string> Dependencies,        // "SmartWikiService depends on IDocumentStore"
    IReadOnlyList<string> EntryPoints,         // Program.cs MapPost routes + public method signatures
    IReadOnlyList<string> FabricInterfaces);   // which Xiigen.Kernel/Fabrics interfaces are injected

public sealed record IntentRecord(
    string Summary,                            // one sentence: what does this do for a tenant?
    IReadOnlyList<string> BusinessRules,       // invariants as plain-language sentences
    IReadOnlyList<string> DomainConcepts,      // abstract concepts implemented
    IReadOnlyList<string> Invariants);         // what must always be true about output

public sealed record ConstraintsRecord(
    IReadOnlyList<string> Explicit,            // AGENTS.md / CLAUDE.md / v3.3 contract
    IReadOnlyList<string> Implicit,            // inferred from guard clauses
    IReadOnlyList<string> Unknowns);           // questions requiring human judgment

public sealed record QualityRecord(
    IReadOnlyList<string> Gaps,                // known functionality gaps
    IReadOnlyList<string> KnownFailures,       // things that break silently
    IReadOnlyList<string> BrittleAreas,        // areas likely to break on change
    IReadOnlyList<string> MissingCoverage);    // untested paths (xUnit)
```

---

## Section 4 — Minimum Viable Intake

When time is constrained, produce at minimum:

```
□ Intent.Summary (one sentence — what does this capability do for a tenant?)
□ Intent.Invariants (3-5 most important things that must always be true)
□ Constraints.Implicit (what the code enforces that the v3.3 contract does not state)
□ Quality.KnownFailures (what breaks silently today)
```

A minimal intake is better than no intake. It prevents the most common mistake:
proposing a capability architecture that violates an existing invariant.

---

## Section 5 — CONTEXT_INSUFFICIENT During Intake

If extraction cannot answer a question required for capability design:

```
Request type table:
  Missing contract from another platform project  → DOWNSTREAM_CONTRACT
  Missing API contract from an external dependency → EXTERNAL_CONTRACT
  Ambiguous business rule requiring a decision     → HUMAN_JUDGMENT (route to Luba)
  Conflicting v3.3 contract vs code behavior       → SPEC_CONFLICT (document both, flag)
```

Do not guess. Emit the CONTEXT_INSUFFICIENT signal from
`pipeline-position-check-SKILL.md` Section 2 and wait.

---

## Anti-Patterns

1. **Starting to design before completing the intake** — "I'll figure out the constraints as I go" produces re-work when a violation surfaces at Gate B.
2. **Intake reads only the happy path** — the most important constraints are in error handling. Every `DataProcessResult.Failure(...)` is a constraint. Read the failure paths first.
3. **Intent.Summary describes the implementation** — "the service uses IDocumentStore and IOutboxStore" is structure, not intent. "The service indexes a tenant's documents so they are searchable" is intent.
4. **Skipping the Quality section** — known failures inform DNA/iron rules; brittle areas become FREEDOM targets.
5. **Treating a v3.3 contract as ground truth without reading the code** — the contract is intended behavior; the code is actual behavior. When they conflict, the code is truth; note it in `Constraints.Unknowns`.

---

## Integration

- `context-package-design-SKILL.md` uses the domain context package as its DOMAIN field input.
- `challenger-round-design-SKILL.md` Domain challenger builds on the intake's intent.
- `claim-as-hypothesis-SKILL.md` / `claim-verification-SKILL.md` verify claims extracted during intake before acting.
- `pipeline-position-check-SKILL.md` CONTEXT_INSUFFICIENT signal format applies to intake gaps.
