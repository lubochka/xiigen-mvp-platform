# solution-scope-gate-SKILL.md — Pick the smallest sufficient scope level

> Ported from the `llm_mvp_core` governance standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build` / `dotnet test`, domain return type `DataProcessResult<T>`
> (`src/Xiigen.Kernel/DataProcessResult.cs`) — **not** core `OperationResult<T>`.
> Level 5 (NEW INTERFACE) = a new C# `I`-prefixed interface + concrete class + DI
> registration in `Program.cs` (`src/Xiigen.Platform.Api`) + xUnit tests. Level 2
> (ADAPTATION) = a wrapper around a .NET BCL type or NuGet package behind
> `IConfiguration` / a fabric interface.

## Core problem
A session that proposes a new infrastructure component when a convention change
would suffice has over-engineered the solution. A session that proposes a
convention when new infrastructure is genuinely required has under-specified it.
This skill installs the correct scope level before any proposal is written.

---

## When to invoke
- Before proposing any solution to a gap or missing capability.
- Before writing a new plan that adds components to the project.
- When `self-extension-session-SKILL.md` / gap analysis identifies a gap needing resolution.
- Before classifying any change as "new interface required".

One run of this skill before a proposal = no over-engineered solutions, no
under-specified ones.

---

## Section 1 — The 5-level scope ladder

Every solution proposal occupies exactly one level. Start at Level 1 and move up
only when the lower level is genuinely insufficient.

| Level | Name | What it produces | When it applies |
|-------|------|------------------|-----------------|
| **1** | CONVENTION | Entry in `AGENTS.md`, `CLAUDE.md`, or a skill file | Behaviour that must be consistent but needs no code |
| **2** | ADAPTATION | Wrapper around a .NET BCL type or NuGet package behind `IConfiguration`/a fabric interface | External tool exists; it needs a project-conformant interface |
| **3** | EXTENSION | New method/capability on an existing class or interface | The class already exists; it is missing one thing |
| **4** | NEW COMPONENT | New class, new pipeline step, new xUnit test fixture | Net-new behaviour with no existing attachment point |
| **5** | NEW INTERFACE | New `I`-prefixed interface + concrete class + `Program.cs` DI registration + xUnit + all downstream wiring | Net-new abstraction boundary required; Level 4 is insufficient |

**Level 5 is the last resort.** Every interface addition requires:
- A new `I`-prefixed interface file (under the appropriate project, e.g. `Xiigen.Kernel` for a fabric contract).
- A concrete implementation (e.g. an `InMemory*` for the contract harness).
- DI registration in `Program.cs` (or a `ServiceCollection` extension like `AddXiigenInMemoryScheduler`).
- xUnit tests for the concrete implementation (project `*.Tests`).
- A Fabric-First / `DataProcessResult<T>` compliance check for all touched files.

---

## Section 2 — Gap classification table

When a gap is identified, classify it on two axes before assigning a scope level.

### Axis 1 — Gap category
| Category | Meaning |
|----------|---------|
| **TOOL** | A utility is missing (formatter, parser, helper) |
| **SKILL** | A skill document is missing or incomplete |
| **ORCHESTRATION** | A coordination step between existing components is missing |
| **ARCHITECTURE** | A structural boundary or abstraction is missing |

### Axis 2 — Fix class
| Fix class | Meaning | Max scope level |
|-----------|---------|-----------------|
| **CONTENT** | Missing documentation, comment, or convention entry | Level 1 |
| **INTERFACE** | Missing `I`-prefix interface definition | Level 5 |
| **EXTENSION** | Existing class needs a new method | Level 3 |
| **NEW_HANDLER** | New class needed, but no new interface boundary | Level 4 |
| **INFRASTRUCTURE** | New external dependency or storage layer (NuGet, persistence) | Level 5 |

Classification record:
```
Gap: [description]
Category:  [TOOL | SKILL | ORCHESTRATION | ARCHITECTURE]
Fix class: [CONTENT | INTERFACE | EXTENSION | NEW_HANDLER | INFRASTRUCTURE]
Assigned scope level: [1-5]
Justification: [one sentence — why this level and not one lower]
```

---

## Section 3 — The 3 reframe tests

Before finalizing a scope level, run all 3. Each test that triggers a downgrade
must be recorded.

### Reframe Test 1 — Next Instance Test
> "What happens at the next instance of this same need?"

- "Same convention applies" → Level 1.
- "Same wrapper applies" → Level 2.
- "Each instance needs its own class" → Level 4 or 5.

A solution that scales to next instances without code change belongs at Level 1 or 2.

### Reframe Test 2 — Provider vs Capability Test
> "Is this a provider (external tool, service, data source) or a capability
> (algorithm, decision, transformation)?"

- Provider → Level 2 (ADAPTATION: wrap the provider behind `IConfiguration`/a fabric interface).
- Capability → Level 3, 4, or 5 depending on whether an attachment point exists.

### Reframe Test 3 — Backward Compatibility Test
> "Does the existing design survive intact inside the new design?"

- Yes → the new design is an extension → maximum Level 3.
- No → the new design replaces something → validate with
  `decision-reopening-SKILL.md` before proceeding to Level 4 or 5.

---

## Section 4 — Scope Gate Record

Produce this record before writing any proposal:
```markdown
## Solution Scope Gate (solution-scope-gate-SKILL)

Gap statement: [one sentence — what is missing]
Category: [TOOL | SKILL | ORCHESTRATION | ARCHITECTURE]
Fix class: [CONTENT | INTERFACE | EXTENSION | NEW_HANDLER | INFRASTRUCTURE]

Reframe tests:
  Test 1 (next instance): [result]
  Test 2 (provider vs capability): [result]
  Test 3 (backward compatibility): [result]

Assigned scope level: [1 | 2 | 3 | 4 | 5]
Level name: [CONVENTION | ADAPTATION | EXTENSION | NEW COMPONENT | NEW INTERFACE]
Justification: [one sentence — why this level and not one lower]

Proposal permitted: YES
```

If any reframe test indicates a lower level is sufficient, downgrade the scope
before finalizing the record. Do not propose at a higher level than the record permits.

---

## Anti-patterns
1. **"Start at Level 5 to be safe"** — Level 5 creates DI/wiring debt that
   outlasts the feature. Justify each level from Level 1 up.
2. **"Skip classification, just write the code"** — unclassified gaps land at the wrong scope. Classification takes 2 minutes; rework takes hours.
3. **"Reframe tests don't apply here"** — they always apply. "Not applicable" means the gap was not stated precisely.
4. **"Convention is too weak for this"** — if an `AGENTS.md`/`CLAUDE.md`/skill entry solves it, the convention IS the right answer. Code is not stronger than a rule — it is more expensive.

---

## Integration
- `level-correction-response-SKILL.md` detects when a proposal sits at the wrong level on this ladder.
- `decision-reopening-SKILL.md` is required before any Level-5 proposal that replaces an existing interface (Backward-Compat Test 3 failure).
- `self-extension-session-SKILL.md` calls this skill to assign the proposal level (Step 2).
