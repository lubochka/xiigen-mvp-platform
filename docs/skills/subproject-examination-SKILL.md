# Subproject Examination

> Ported from the `llm_mvp_core` subproject-examination standard, adapted for the
> XIIGen Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build Xiigen.Platform.slnx` / `dotnet test`. The unit is a new or
> growing project `src/Xiigen.*` (or a capability within one). CHECK commands are
> `dotnet build Xiigen.Platform.slnx`, `dotnet test`, `grep "interface I"` in
> `Xiigen.Kernel/FabricContracts.cs` + `TenantContext.cs`
> (IDocumentStore/IIdempotencyStore/IOutboxStore/ITenantContextAccessor/ISchedulerService),
> and ProjectReference in `.csproj`. Capability contracts use `DataProcessResult<T>`
> (never core `OperationResult<T>`) + fabric-first (map 1:1 to existing contracts).
> **UI documents are na — platform has no frontend.**

## Core Problem

A project/capability that starts implementation before its infrastructure gaps
are known will stall mid-phase when a missing contract or dependency is
discovered. This skill installs a pre-implementation examination gate that
produces three documents before any code is written. Nothing starts until those
documents exist and are approved.

---

## When to Invoke

- When a new project is added to the v3.3 plan.
- When an existing project advances from a stub state to ACTIVE.
- When a new capability (contract + implementation + tests) is planned.
- Before any phase that creates more than one new `.cs` file.

One examination before implementation = no mid-phase infrastructure discoveries.

---

## Section 1 — The 3-Document Output

The examination produces exactly 3 documents before any `.cs` file is written. Do
not produce implementation until all 3 are reviewed and approved by Luba.

### Document 1: `docs/[CAPABILITY]-MASTER.md`

**Header:**
```
# [CAPABILITY]-MASTER.md
## Project: Xiigen.[Name] — [capability]
## Date: [date]
## Status: DRAFT — review before executing
```

**Section 1: Infrastructure Decision Tree** — a decision tree, NOT a checklist.
Every FAIL has a BUILD instruction.

```markdown
Run each check in order. If it fails, follow the BUILD instruction.
Only begin Phase 1 after ALL checks pass.

CHECK 1: Build baseline
  Command: dotnet build Xiigen.Platform.slnx → must output "0 Error(s)" and "0 Warning(s)"
  If PASS: proceed to CHECK 2
  If FAIL: Fix build errors/warnings first. Record failing files. Do not proceed.

CHECK 2: Test baseline
  Command: dotnet test Xiigen.Platform.slnx → record exact count (N passed, 0 failed)
  Record in the v3.3 state as test_baseline. Measurement only. Proceed to CHECK 3.

CHECK 3: Required fabric contracts
  For each contract this capability depends on:
  Command: grep -rn "interface [IContractName]" src/Xiigen.Kernel/
  If PASS (found): proceed to next contract
  If FAIL (not found): record as BLOCKED in MISSING-INFRASTRUCTURE document.

CHECK 4: Required project references
  Command: grep -n "[ReferencedProject]" src/Xiigen.[Name]/Xiigen.[Name].csproj
  If PASS: proceed to CHECK 5
  If FAIL: Add ProjectReference. dotnet build to verify. Then proceed.

CHECK 5: Naming convention
  New files: [ClassName].cs, [ClassName]Tests.cs.
  Verify no existing files in the project conflict. Proceed to implementation.

All checks passed → produce Document 2 and Document 3 → await Luba approval → begin Phase 1.
```

**Section 2: Capability Contract** — for each class/contract to be created:

```markdown
### [ClassName]

Contract: I[ClassName] (if public-facing)
Concrete: [ClassName] (extends MicroserviceBase<[ClassName]> if it is a service)
Test class: [ClassName]Tests

Public methods:
  [MethodName]([params]) → DataProcessResult<[T]>
  [what this method does]

DNA requirements (DNA-DOTNET-TRANSLATION.md):
  Rule 3: returns DataProcessResult<T> — never throws for business logic, never returns null
  Rule 4: depends on [IContractName] (fabric interface) — not the concrete class
  Rule 5: tenant from ITenantContextAccessor.Current; no shared mutable static

Constructor dependencies (fabric-first):
  [IContractName] [_fieldName]

Gate B:
  dotnet test --filter "FullyQualifiedName~[ClassName]Tests" → [N] passed, 0 failed
```

**Section 3: Phase Plan**

```markdown
Phase 1: [ClassName] implementation
  Files: src/Xiigen.[Name]/[ClassName].cs, tests/Xiigen.[Name].Tests/[ClassName]Tests.cs
  Gate B: dotnet test --filter "FullyQualifiedName~[ClassName]Tests" → [N] passed, 0 failed

Phase 2: DI + integration (if applicable)
  Files: src/Xiigen.Platform.Api/Program.cs (registration)
  Gate B: dotnet test --filter "[filter]" → [N] passed, 0 failed

Phase [LAST]: docs/HOW-TO-USE-SKILLS.md or v3.3 contract update (if applicable)
  Gate B: artifact exists and references the new capability
```

---

### Document 2: `docs/[CAPABILITY]-MISSING-INFRASTRUCTURE.md`

One entry per gap found in CHECK 3 (missing contracts, project references, base
classes):

```markdown
### GAP-[N]: [Feature Name]

**What it is:** one paragraph — what is missing and what it provides

**What fails without it:**
- [which Phase gate fails]
- [which tests cannot be written]

**Build spec:**
  File to create: [exact path under src/Xiigen.*]
  Contract/class: [full C# signature with namespace — DataProcessResult<T> returns]
  Verification: dotnet build → 0 Error(s), 0 Warning(s)
               dotnet test --filter "[filter]" → [N] passed, 0 failed

**Priority:** BLOCKER | CRITICAL | IMPORTANT
  BLOCKER   = Phase 1 cannot start without this
  CRITICAL  = Phase completes with wrong behavior
  IMPORTANT = quality or test coverage degraded
```

End with: (1) a summary table Gap | Name | Priority | Blocks Phase | Also needed
by; (2) implementation order (dependency chain); (3) a "What is NOT missing"
section listing what was verified present (the `Xiigen.Fabrics.*` contracts that
already ship).

---

### Document 3: `docs/[CAPABILITY]-LEARNING-SPEC.md`

```markdown
## Domain input
[Plain-English description of what this capability does for a tenant — no C# class names]

## New capabilities introduced
| Capability | What it adds to the platform | First consumer |
|---|---|---|
| [capability] | [description] | [which Program.cs endpoint / service calls it] |

## Behavioral tests required
For each capability:
  Input: [specific input value or state]
  Expected output: [specific DataProcessResult with a real Data value]
  Verified by: dotnet test --filter "[TestName]"

## Trainable-unit / manifest link (if applicable)
  Does this capability consume or produce a trainable-unit manifest under the
  TRAINABLE-UNIT-MANIFEST-CONTRACT.md / .xiigen wiki-domain? [YES + which / NO]
  (Common-model/ML internals remain owned by llm_mvp_core; platform consumes them.)

## Completion criteria
  All tests pass: dotnet test → [N] passed, 0 failed
  Build: dotnet build → 0 Error(s), 0 Warning(s)
  DNA compliance: Rule 3 (DataProcessResult<T>), Rule 4 (fabric-first), Rule 5 (tenant scope)
```

---

## Section 2 — Examination Rules

1. **No placeholder values** — every field in Document 1 Section 2 must have a real value. Unknown values become gaps in Document 2.
2. **Gaps have BUILD specs** — a gap that says "add this contract" without the full C# signature is incomplete.
3. **Decision tree, not checklist** — every CHECK failure has a BUILD instruction, not "STOP and investigate".
4. **BLOCKER means stop** — a GAP-N marked BLOCKER means Phase 1 cannot start. Resolve it first.
5. **Approval required** — do not begin writing `.cs` files until Luba has reviewed all 3 documents.
6. **Gate B per phase** — every phase in Document 1 Section 3 has one `dotnet test` command with an exact expected count.

---

## Anti-Patterns

1. **"I know what's needed, skip the examination"** — the examination is one session; discovering a missing contract mid-Phase 2 is two.
2. **"Infrastructure gap can be fixed later"** — BLOCKER gaps block Phase 1; they cannot be fixed in a later phase that depends on Phase 1.
3. **"Document 3 is optional"** — it is required when the capability consumes/produces a trainable-unit manifest.
4. **"Gap has no build spec"** — a gap without a build spec is not actionable.

---

## Integration

- `prerequisite-chain-SKILL.md` verifies the BLOCKER items in Document 2 before execution.
- `subproject-completeness-checker-SKILL.md` requires Document 1 Section 3 (phase plan) to have Gate B per phase.
- `gap-to-proposal-SKILL.md` classifies the scope level (Level 3/4) for each gap in Document 2.
- `phase-completion-packager-SKILL.md` uses Document 3 completion criteria as the evidence baseline.
