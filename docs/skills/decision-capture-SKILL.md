# decision-capture-SKILL.md — Record significant decisions so they are not re-derived

> Ported from the `llm_mvp_core` governance standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build` / `dotnet test`, domain return type `DataProcessResult<T>`
> (`src/Xiigen.Kernel/DataProcessResult.cs`) — **not** core `OperationResult<T>`.
> Decisions are written to `docs/DECISIONS-LOCKED.md` (create it if missing) and
> cross-referenced with the v3.3 completion matrix
> (`docs/plans/enterprise-adaptation/v3.3/XIIGEN-ENTERPRISE-ADAPTATION-COMPLETION-MATRIX-v3.3.md`).
> `Principle` references platform invariants: Fabric-First, `DataProcessResult<T>`,
> tenant-scope (`ITenantContextAccessor`), and No-Secrets (`AGENTS.md`).

## Core problem
A decision made in a planning session and not recorded is re-derived in the next
session. Re-derivation costs planning time and often produces a *different*
answer — not because the situation changed, but because the reasoning context
was lost. This skill defines what counts as a significant decision, how to record
it in the DESIGN_REASONING format, and when to write it to
`docs/DECISIONS-LOCKED.md` vs keep it a session-level note.

---

## When to invoke
- At Gate B of every phase — before declaring a phase complete.
- During any planning session when a significant decision is made (capture immediately).
- When a self-validation pass finds an undocumented design choice.
- When a future session references "a prior decision" not in `docs/DECISIONS-LOCKED.md`.

---

## Section 1 — What counts as a significant decision

A decision is significant if a future session planning similar work would need to
know it to avoid rework.

**The one-sentence test:**
> "If a future session encountered a similar capability, would knowing this
> decision save at least one phase of replanning?"

If YES → capture it.

**Decision types that are always significant:**

| Type | Description | Platform example |
|---|---|---|
| `CAPABILITY_CLASSIFICATION` | four-tier classification result with non-obvious reasoning | A routing weight classified LEARNABLE (consumed via manifest, R5) — not a FREEDOM constant |
| `INTERFACE_INTRODUCTION` | Why a new fabric interface was added instead of extending an existing one | `IOutboxStore` added instead of extending `ISchedulerService` — different contract, different lifecycle |
| `DEPENDENCY_DIRECTION` | Why a capability depends on another in this direction | Application depends on `IDocumentStore` (Kernel), not the reverse — Kernel is the contract layer |
| `IRON_RULE_DERIVATION` | A non-obvious rule traced to a specific failure mode | Idempotency-reserve-before-schedule rule derived from a duplicate-resend race |
| `ARCHITECTURE_PATTERN` | A general pattern established for future flows | "Every cross-service side effect goes through `IOutboxStore` + CloudEvents, never inline" |
| `LOCKED_DECISION_OVERRIDE` | Why a locked decision needed an ADR before changing | A v3.3-matrix-locked default overridden for the master-tenant runbook |

**What is NOT significant:**
- Decisions that follow directly from the platform invariants (Fabric-First,
  `DataProcessResult<T>`, tenant-scope, No-Secrets) — no reasoning needed.
- File/test naming choices (implementation details).
- xUnit `[Fact]`/`[Theory]` usage (already decided).

---

## Section 2 — The DESIGN_REASONING format

```csharp
// This record maps directly to a docs/DECISIONS-LOCKED.md entry
public record DesignReasoning
{
    public string DecisionId { get; init; }   // "D-NN" — next sequential number
    public string Type { get; init; }         // capability-classification, interface-introduction, ...
    public string Question { get; init; }     // what was decided? (the retrievable key — a QUESTION)
    public string Reasoning { get; init; }    // why? what was considered and rejected?
    public string Principle { get; init; }    // which platform invariant or locked decision applies
    public string Outcome { get; init; }      // what is now true as a result
}
```

**The Question field is the retrieval key.** Future sessions match against
`Question`. A vague question produces no match.
```
BAD:  Question = "Outbox design"
GOOD: Question = "Should a cross-service side effect dispatch inline or go through IOutboxStore + CloudEvents?"

BAD:  Question = "Dependency direction"
GOOD: Question = "Should the Application layer depend on IDocumentStore, or should Kernel reference Application?"
```

**The Reasoning field is what teaches future sessions.** "We decided X" teaches
nothing. "We considered A and B; A was rejected because it couples Kernel to
Application; B preserves the contract direction…" gives the classification logic.

---

## Section 3 — `docs/DECISIONS-LOCKED.md` entry format

```markdown
## D-NN: [One-sentence decision statement]

**Type:** CAPABILITY_CLASSIFICATION | INTERFACE_INTRODUCTION | DEPENDENCY_DIRECTION |
          IRON_RULE_DERIVATION | ARCHITECTURE_PATTERN | LOCKED_DECISION_OVERRIDE

**Question:** [Exact retrievable question — stated as a question]

**Reasoning:** [What was considered and rejected. Why this option. Which failure
               mode it prevents. Two to five sentences.]

**Principle:** [Fabric-First / DataProcessResult<T> / tenant-scope / No-Secrets /
               prior locked decision / v3.3 matrix row]

**Outcome:** [What is now true. What this enables or prevents.]

**NEVER reopen without Luba approval.** [Add when 3+ capabilities depend on it]
```

**Minimum 2 decisions per planning session.** A session with fewer than 2
significant decisions either had trivial scope or decisions were missed.

---

## Section 4 — When to write to `docs/DECISIONS-LOCKED.md`

| Write to DECISIONS-LOCKED.md when | Keep as session note when |
|---|---|
| Decision affects ≥ 2 capability classes / flows | Decision is specific to one implementation |
| Decision sets a pattern for future phases | Decision is about current session scope only |
| Decision would be challenged without documentation | Decision is obvious from the code |
| Decision requires "NEVER reopen without Luba" | Decision can safely be revisited |
| Decision overrides a prior locked decision / matrix row | Decision supplements but does not override |

---

## Section 5 — Retroactive capture
For phases that ran before this skill existed, capture decisions from: git commit
messages, `AGENTS.md`/`CLAUDE.md` (settled decisions), and code comments.
Retroactive capture is less valuable than real-time, but far better than nothing.

---

## Anti-patterns
1. **"The decision is obvious from the code"** — code shows *what*; the doc records *why*.
2. **"I'll capture it at session end"** — end-of-session memory is thinner. Record as you go.
3. **"Decision type = GENERAL"** — every significant decision fits one of the 6 types.
4. **"Question written as a statement"** — `Question = "the outbox design"` is not retrievable; ask it as a question.
5. **"Fewer than 2 decisions per session"** — re-read the session; decisions were missed.

---

## Integration
- `docs/DECISIONS-LOCKED.md` is the persistence layer for captured decisions.
- `decision-reopening-SKILL.md` governs when a locked decision is revisited.
- `document-hierarchy-SKILL.md` routes decisions to the correct canonical file.
- `planning-session-startup-SKILL.md` reads `docs/DECISIONS-LOCKED.md` before any plan is written.
