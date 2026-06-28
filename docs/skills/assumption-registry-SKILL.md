# Assumption Registry

> Ported from the `llm_mvp_core` assumption-registry standard, adapted for the
> XIIGen Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build Xiigen.Platform.slnx` / `dotnet test`. INFRASTRUCTURE assumptions
> verify against the real fabric contracts (`grep "interface I" src/Xiigen.Kernel/FabricContracts.cs`)
> and their implementations in `Xiigen.Fabrics.InMemory`/`Xiigen.Fabrics.Scheduler`.
> OPERATIONAL baseline is `dotnet build` 0 Error(s) + `dotnet test` Failed: 0.
> Domain return type is `DataProcessResult<T>` (never core `OperationResult<T>`).

## When to Invoke

After a plan is produced, before session / EXECUTION files are written. The
assumption registry feeds `phase-preflight-SKILL.md`, which executes the
verification commands at the start of the session. It belongs in STEP 0 of the
EXECUTION file.

---

## Why This Exists

The classic blocking issues are all discoverable in 30 seconds with a grep, but
none were checked at plan-authoring time:
1. A method assumed to exist on a fabric contract — it did not (signature changed).
2. A DI registration assumed to be in `Program.cs` — it was not.
3. A field assumed to be a property — it was a method (wrong access pattern).

The cost of a wrong assumption found in review: 1 review cycle. Found at
execution: blocked session + re-plan + re-implementation.

---

## The Three Assumption Classes

### Class 1 — INFRASTRUCTURE (verifiable by grep or file existence)

Something must exist in the codebase that the plan depends on.

```
Example:  "IDocumentStore.SearchAsync exists in the contract"
Verify:   grep -n "SearchAsync" src/Xiigen.Kernel/FabricContracts.cs
Expected: 1+ hits
Fallback: If 0 hits — BLOCKING: add SearchAsync to IDocumentStore before this phase
```

```
Example:  "InMemoryIdempotencyStore implements IIdempotencyStore"
Verify:   grep -rn "class InMemoryIdempotencyStore.*IIdempotencyStore" src/Xiigen.Fabrics.InMemory/ --include="*.cs"
Expected: 1 hit
Fallback: If 0 hits — BLOCKING: implement the interface first
```

### Class 2 — BEHAVIORAL (verifiable by reading the code)

Something must behave a certain way — a method signature, a field name, return
type.

```
Example:  "ISchedulerService.ScheduleDelayedAsync returns DataProcessResult<ScheduledJobResult>"
Verify:   grep -n "ScheduleDelayedAsync" src/Xiigen.Fabrics.Scheduler/Scheduler.cs
Expected: signature wraps DataProcessResult<ScheduledJobResult>
Fallback: If the return shape differs — update callers, or change the method
```

### Class 3 — OPERATIONAL (verifiable by running a command)

Something must build or compile for the plan to be executable.

```
Example:  "All existing tests still pass before adding new tests"
Verify:   dotnet test Xiigen.Platform.slnx --no-build 2>&1 | tail -3
Expected: Failed: 0
Fallback: If failures exist — BLOCKING: fix pre-existing failures before this phase
```

```
Example:  "Solution builds clean (warnings are errors here)"
Verify:   dotnet build Xiigen.Platform.slnx 2>&1 | tail -5
Expected: 0 Error(s), 0 Warning(s)
Fallback: If any error/warning — BLOCKING: fix before this phase
```

---

## The Assumption Registry Format

Produce an `ASSUMPTIONS` block for every EXECUTION file, in STEP 0 (PRE-FLIGHT):

```markdown
## STEP 0 — PRE-FLIGHT: ASSUMPTION REGISTRY

Before executing any step, verify all assumptions. A failed assumption is BLOCKING
unless a fallback is specified. If BLOCKING and no fallback: STOP. Escalate to Luba.

| # | Assumption | Class | Verification | Expected | Fallback |
|---|-----------|-------|--------------|----------|---------|
| A1 | IDocumentStore.SearchAsync exists | INFRA | `grep -n "SearchAsync" src/Xiigen.Kernel/FabricContracts.cs` | 1+ hits | BLOCKING — add to contract first |
| A2 | InMemoryDocumentStore implements IDocumentStore | INFRA | `grep -rn "class InMemoryDocumentStore.*IDocumentStore" src/Xiigen.Fabrics.InMemory/ --include="*.cs"` | 1 hit | BLOCKING — implement interface first |
| A3 | ISchedulerService.ScheduleDelayedAsync returns DataProcessResult<T> | BEHAVIORAL | `grep -n "ScheduleDelayedAsync" src/Xiigen.Fabrics.Scheduler/Scheduler.cs` | wraps DataProcessResult<ScheduledJobResult> | BLOCKING — update all callers |
| A4 | Build/test baseline clean | OPERATIONAL | `dotnet build Xiigen.Platform.slnx 2>&1 \| tail -5` then `dotnet test --no-build 2>&1 \| tail -3` | 0 Error(s)/0 Warning(s); Failed: 0 | BLOCKING — fix pre-existing failures |
```

**Each row must have:** a single testable claim (not "everything is set up"), a
literal shell command, the expected output, and a fallback OR "BLOCKING".

---

## How to Identify Assumptions From a Plan

Read each step in the plan. For every step, ask:

```
1. What file/contract does this step touch?
   → Does that file/contract exist? (Class 1: INFRASTRUCTURE)

2. What method or field does this step call or read?
   → Does it have the expected signature/return shape? (Class 2: BEHAVIORAL)
   → Will it still be there when this session runs?

3. What state must the codebase be in for this step to succeed?
   → Are all prior phases complete? (Class 1)
   → Does the build (0 warnings) and test gate pass? (Class 3: OPERATIONAL)
```

For every YES answer: register the assumption and write its verification command.

---

## Assumption Registry vs. Simple File Check

Weak:
```bash
ls src/Xiigen.Kernel/FabricContracts.cs   # confirms the file exists
```

Strong:
```bash
# A1: IDocumentStore.SearchAsync accepts a filter dictionary
grep -n "SearchAsync(.*string collection,.*IReadOnlyDictionary<string, object?>" src/Xiigen.Kernel/FabricContracts.cs
# Expected: 1+ hits — method present with the correct parameters
# Fallback: If 0 hits — signature changed; update callers before this phase
```

The `ls` confirms the file exists. The registry confirms the thing the plan
depends on exists INSIDE the file and has the right shape.

---

## Anti-Patterns

1. **Assumptions stated in prose** ("we assume the document store works") — every assumption needs a verification command.
2. **Registering only obvious assumptions** — the non-obvious ones (signature changed, field renamed, namespace conflict) cause blocked sessions.
3. **Fallback of "check manually"** — not a fallback. Write the specific command, or mark BLOCKING and escalate at planning time.
4. **Assumption registry at the end of the plan** — it is produced BEFORE session files are written; a failed assumption may change session-file content.

---

## Integration With Phase-Preflight

This skill produces the registry at planning time. `phase-preflight-SKILL.md`
executes it at session start.

```
Plan authoring:
  assumption-registry-SKILL → ASSUMPTIONS registered → EXECUTION STEP 0 populated

Session start:
  phase-preflight-SKILL → reads STEP 0 → runs each command → PASS/FAIL/FALLBACK
  → If any BLOCKING fails: STOP, escalate to Luba
  → If fallback available: apply fallback, note in session, continue
```

The two skills are a pair. `prerequisite-chain-SKILL.md` supplies the
contract/DI prerequisites that become INFRASTRUCTURE rows here.
