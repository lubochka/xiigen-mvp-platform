# Context Package Design (XIIGen Enterprise Platform)

## Core Problem

A document that tells an AI agent what steps to execute is an instruction file. A
document that tells an AI agent what it needs to know to make a decision is a
context package. Confusing the two produces agent decisions that either have too
little context (wrong decisions) or are over-constrained by instructions (no
genuine decision-making). This skill defines the 5-field structure of a valid
context package and how to verify it before use.

Platform port of the `llm_mvp_core` skill. Stack-bound to **.NET 10 ASP.NET Core
Web API**, **xUnit** (`tests/`), `Xiigen.Platform.slnx`, `DataProcessResult<T>`,
and the v3.3 governance triple. The core skill's machine-learning examples
(ContinuousLearningPipeline / IncrementalTrainer / DomainModelRegistry) are **not
ported** — the platform consumes no local models and trains nothing here. On this
platform the actor is the AI agent or bounded sub-agent making a Web API or
domain-logic decision (for example: how a fabric provider should behave, what a
service method's failure path returns), not a training pipeline.

---

## When to Invoke

- Before writing any context input passed to an AI agent / bounded sub-agent that
  must make a Web API or domain-logic decision
- Before authoring a decision package for a fabric provider behavior or a service
  contract
- When reviewing another agent's context input and verifying it is not an
  instruction file

---

## Section 1 — The Fundamental Distinction

| Instruction file | Context package |
|---|---|
| Tells the agent: do step 1, then step 2 | Tells the agent: here is the intent, constraints, context, and success criteria |
| Output is prescribed — agent executes it | Output is decided by the agent — context enables the decision |
| Fails if a step is ambiguous | Fails if a field is missing or mis-filled |
| The agent is a transcriber | The agent is the decision-maker |
| Self-containment check: can the agent run this alone? | Self-containment check: can the agent make a defensible decision from this alone? |

**The one-sentence test:**
If the document tells the agent WHAT to produce — it is an instruction file. If
the document tells the agent WHAT IT NEEDS TO KNOW to decide — it is a context
package.

Practical test: remove all field labels. Read the document. Does it sound like a
recipe (do this, then do that) or a briefing (here is the situation, constraints,
what success looks like)? A recipe is an instruction file. A briefing is a context
package.

---

## Section 2 — The 5-Field Structure

Every context package has exactly these 5 fields. No more. No fewer. If a field is
absent, the package is incomplete and must not be used.

### INTENT

| | |
|--|--|
| **Definition** | The exact thing this agent decision is trying to understand or decide |
| **What belongs** | The user's request or system goal verbatim — no rephrasing |
| **What does NOT belong** | Interpretation, clarification, scope expansion |
| **Empty fails because** | The agent solves what is written. Empty = invented problem. Rephrased = different problem. |

### DOMAIN

| | |
|--|--|
| **Definition** | What this part of the platform does, in 2-3 sentences |
| **What belongs** | Capability behaviors, tenant/idempotency/outbox rules, user-facing API behavior |
| **What does NOT belong** | Technology names (.NET, ASP.NET Core, xUnit), class names, method signatures |
| **Empty fails because** | Without domain context, the agent produces generic output unconstrained by what the platform actually does. |

### CONSTRAINTS

| | |
|--|--|
| **Definition** | Machine-enforced invariants — things that must hold regardless of what the agent decides |
| **What belongs** | Every public method returns `DataProcessResult<T>` and never throws for business logic; fabric-first (providers resolved through DI, not instantiated); locked decisions for the platform |
| **What does NOT belong** | Preferences, quality goals, technology choices |
| **Empty fails because** | The agent produces unconstrained output that may violate rules the platform must always enforce. |

### PRIOR_ART

| | |
|--|--|
| **Definition** | A retrieval query that fetches relevant prior work — NOT the prior work itself |
| **What belongs** | A query string: "prior implementations of [capability] in [project area]" |
| **What does NOT belong** | Copied documents, pasted prior implementations, inline examples |
| **Empty is acceptable ONLY when** | Explicitly stated as `NO_PRIOR_ART` with reasoning. Silent empty field is a failure. |
| **Empty fails because** | The agent cannot reuse prior work if it cannot retrieve it. |

### SUCCESS

| | |
|--|--|
| **Definition** | What the output must look like — format and structure, not content |
| **What belongs** | Output shape, required xUnit test categories, Gate B condition format |
| **What does NOT belong** | Expected content, prescribed answers, example outputs that bound what the agent can produce |
| **Empty fails because** | Without a success definition, the result cannot be self-evaluated. Gate B cannot be verified. |

---

## Section 3 — Verification Checklist

Run before using any context package. All 8 must be TRUE:

```
□ INTENT contains the exact source text — no paraphrasing, no added scope

□ DOMAIN has no technology names (.NET, ASP.NET Core, xUnit, IConfiguration, etc.)

□ CONSTRAINTS lists only machine-enforced invariants — not preferences or goals

□ Each constraint is stated as a verifiable condition — not as a description

□ PRIOR_ART is a query string OR explicitly states NO_PRIOR_ART with reasoning
  (copying prior implementations inline fails this check)

□ SUCCESS defines output FORMAT — not output content or expected answers

□ No field references another document ("see X", "per Y", "as in Z")

□ No field contains a step or instruction ("first do X", "then check Y")
```

If any check returns FALSE: fix the field before using the package. A failing
context package produces unverifiable output.

---

## Section 4 — Runtime Failure Modes

### INTENT_REPHRASED

```
In package INTENT: "The system should handle duplicate requests."
User said:         "I need idempotency for the order-submit endpoint."
Effect:            Agent produces a generic dedup solution, not the idempotency contract.
Fix:               Copy the request verbatim into INTENT. Never paraphrase.
```

### DOMAIN_SCOPES_THE_STACK

```
In package DOMAIN: "An ASP.NET Core Web API targeting .NET 10."
Effect:            Agent optimizes for framework specifics, not domain behavior.
Fix:               Describe what this part of the platform does for tenants/users.
                   Remove all technology names from DOMAIN.
```

### CONSTRAINTS_CONTAIN_PREFERENCES

```
In package CONSTRAINTS: "Code should be clean and maintainable."
Effect:            "Clean" is unverifiable. Any output can claim to be clean.
Fix:               Replace with: "Every public method returns DataProcessResult<T>
                   and never throws for business logic." — a failing xUnit test
                   exists if this is violated.
```

### PRIOR_ART_INLINE

```
In package PRIOR_ART: [copied 50 lines from InMemoryOutboxStore.cs implementation]
Effect:            Agent reproduces the prior implementation instead of deciding.
Fix:               Replace with query: "prior outbox-store implementations in Xiigen.Fabrics.InMemory"
                   Or: "NO_PRIOR_ART — first idempotency store in this fabric"
```

### SUCCESS_PRESCRIBES_CONTENT

```
In package SUCCESS: "The implementation should match InMemoryOutboxStore.cs structure."
Effect:            Agent copies the reference instead of deciding independently.
Fix:               Replace with: "Each public method returns DataProcessResult<T>.
                   One xUnit test per invariant. Gate B: dotnet test -> N passing, Failed: 0."
```

---

## Anti-Patterns

1. **"I'll fill in PRIOR_ART later"** — an empty PRIOR_ART field cannot be
   distinguished from a forgotten one. Fill it now or mark `NO_PRIOR_ART: first
   instance in this area`.
2. **"CONSTRAINTS repeats what's in the governance docs"** — yes, it should. The
   package makes constraints explicit so the agent does not need to load external
   documents. If this feels redundant, the constraints section is correctly authored.
3. **"I added instructions to SUCCESS to be helpful"** — SUCCESS defines what the
   output looks like, not how to produce it. Remove all procedural language.
4. **"One context package covers all decisions of this phase"** — each decision
   is a separate decision point. A second-decision package is NOT the same as the
   first. The INTENT field changes per decision.

---

## Integration

- An AI agent / bounded sub-agent consumes context packages for each Web API or domain-logic decision
- `authority-requirement-binding-SKILL.md` feeds CONSTRAINTS from binding requirements
- `context-package-design` verification checklist runs before any decision package is used
- `output-readability-gate-SKILL.md` governs the prose sections of the package
