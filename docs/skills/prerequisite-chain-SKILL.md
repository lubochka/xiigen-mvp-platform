# Prerequisite Chain

> Ported from the `llm_mvp_core` prerequisite standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build Xiigen.Platform.slnx` / `dotnet test`. Prerequisites are real
> fabric contracts (`IDocumentStore`/`IIdempotencyStore`/`IOutboxStore` in
> `Xiigen.Kernel/FabricContracts.cs`, `ITenantContextAccessor` in
> `Xiigen.Kernel/TenantContext.cs`, `ISchedulerService` in
> `Xiigen.Fabrics.Scheduler/Scheduler.cs`), their implementations in
> `Xiigen.Fabrics.InMemory`/`Xiigen.Fabrics.Scheduler`, and DI registration in
> `Xiigen.Platform.Api/Program.cs`. No mocking a missing fabric — the contracts
> map 1:1 to existing ones. Domain return type is `DataProcessResult<T>`.

## Purpose

Before starting any phase or session, verify that all prerequisites are met. A
phase with unmet prerequisites will block mid-session and waste the setup time.

---

## The Prerequisite Check Protocol

### Step 1 — List All Prerequisites

For each planned deliverable, ask:
- What fabric contract or class does it depend on?
- Does that contract/class currently exist?
- Does it have the methods the deliverable needs?
- Is it registered in DI?

```
Deliverable: SmartCodeService integration test

Prerequisites:
  1. IDocumentStore contract exists with SearchAsync/UpsertAsync
  2. InMemoryDocumentStore implements IDocumentStore
  3. ITenantContextAccessor exists with Current/Push
  4. SmartCodeService exists and extends MicroserviceBase<SmartCodeService>
  5. All of the above registered in Program.cs (AddXiigenInMemoryFabrics)
```

### Step 2 — Write Verification Commands

For each prerequisite, write the verification command:

```bash
# Prereq 1: IDocumentStore.SearchAsync exists
grep -n "interface IDocumentStore" src/Xiigen.Kernel/FabricContracts.cs
grep -n "SearchAsync\|UpsertAsync" src/Xiigen.Kernel/FabricContracts.cs
# Expected: 1 interface match + the method signatures

# Prereq 2: InMemoryDocumentStore implements IDocumentStore
grep -rn "class InMemoryDocumentStore.*IDocumentStore" src/Xiigen.Fabrics.InMemory/ --include="*.cs"
# Expected: 1 match

# Prereq 3: ITenantContextAccessor exists
grep -n "interface ITenantContextAccessor" src/Xiigen.Kernel/TenantContext.cs
# Expected: 1 match

# Prereq 5: DI registration in the API host
grep -rn "AddXiigenInMemoryFabrics\|AddSingleton<I" src/Xiigen.Platform.Api/Program.cs
# Expected: 1+ matches that wire the fabric implementations
```

### Step 3 — Classify Each Prerequisite

| Prerequisite | Verification | Status | Action if Unmet |
|-------------|--------------|--------|-----------------|
| IDocumentStore contract | grep returns 1 match | PASS / FAIL | BLOCKING — add to FabricContracts.cs first |
| InMemoryDocumentStore impl | grep returns 1 match | PASS / FAIL | BLOCKING — implement before this phase |
| ITenantContextAccessor | grep returns 1 match | PASS / FAIL | BLOCKING — present in Xiigen.Kernel |
| DI registration | grep returns 1+ | PASS / FAIL | BLOCKING — wire in Program.cs |

---

## Chain Ordering

When multiple phases have prerequisites, map the full chain:

```
Phase 3 (SmartCode integration)
  ← requires Phase 2 (SmartCodeService implementation)
     ← requires Phase 1 (IDocumentStore search filter contract)
        ← requires: NONE (starts here — contract already ships)

Execution order: Phase 1 → Phase 2 → Phase 3
```

Do not start Phase 3 until Phase 2 tests show `Failed: 0`.

---

## Prerequisite Block Protocol

If any BLOCKING prerequisite fails at session start:

```
PREREQUISITE BLOCK: [what failed]

Verification run: [command that showed the gap]
Output: [actual output vs expected]

Cannot proceed with: [which deliverable is blocked]

Options:
A. Build the missing prerequisite now (estimated: N steps)
B. Stop and wait for Luba to assign the prerequisite phase

Recommendation: [A or B with reasoning]
```

Do not work around a missing prerequisite by mocking it in tests or adding `TODO`
comments. The fabric contracts map 1:1 to existing ones — either implement the
real contract/registration or stop.

---

## Session Start Prerequisite Checklist

Before every code session:

```
□ Build: dotnet build Xiigen.Platform.slnx → 0 Error(s), 0 Warning(s)
□ Test baseline: dotnet test --no-build → record N passed, 0 failed
□ Read the active v3.3 STATE.json / SESSION-XX → confirm this phase can start
□ Verify top-3 prerequisites for this phase with grep commands
□ No unmet prerequisites → begin work
□ Any unmet prerequisite → apply block protocol above
```

(Warnings are errors here via `Directory.Build.props` `TreatWarningsAsErrors=true`,
so a new warning fails the build gate.)

---

## Anti-Patterns

1. **"I'll discover prerequisites as I go"** — blocked sessions have high overhead; verify prerequisites first.
2. **Mocking a missing fabric to unblock a test** — the mock hides the gap; the real DI wiring will fail. Implement the real contract.
3. **Assuming a prior phase is complete without verifying** — check the v3.3 STATE/matrix and run the prior phase's Gate B verification.
4. **Not listing prerequisites at all** — implicit prerequisites cause blocked sessions.

---

## Integration

- `infrastructure-discovery-SKILL.md` Step 4/8 supplies the fabric-edge inventory this skill verifies.
- `assumption-registry-SKILL.md` turns each prerequisite into a testable assumption row with a literal command.
- `wave-assignment-SKILL.md` declares hard prerequisites that must be ACTIVE before a project starts.
- `phase-preflight-SKILL.md` runs the Session-Start checklist at execution time.
