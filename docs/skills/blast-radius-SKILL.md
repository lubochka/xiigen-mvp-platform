# Blast Radius Assessor (XIIGen Enterprise Platform)

## Purpose

Before touching any file, assess how far the change propagates. A change that seems
isolated — "just update this parameter" — may break every caller of a public
contract. The blast radius assessment makes this visible before any file is modified.

Platform port of the core `blast-radius` (Assessor) skill, re-bound to
**.NET 10 ASP.NET Core Web API**, **xUnit**, the real `Xiigen.Kernel` fabric
contracts (`ISchedulerService`, `IDocumentStore`, `IOutboxStore`,
`IIdempotencyStore`, `IFreedomConfigService`, `ITenantContextAccessor`,
`ICorrelationContextAccessor`), and `DataProcessResult<T>`. Gate-B is
`dotnet build Xiigen.Platform.slnx` + `dotnet test`.

## When to load

- Before editing any `Xiigen.Kernel` `I*` contract or `DataProcessResult<T>`.
- Before changing a `MicroserviceBase<T>` constructor or a DI registration in
  `Xiigen.Platform.Api/Program.cs`.
- Whenever `blast-radius-tagger-SKILL.md` tags a file as INTERFACE.

---

## Classification Levels

```
CRITICAL — change affects callers across the entire solution
  Triggers: public fabric interface signature change (ISchedulerService, IDocumentStore,
            IOutboxStore, IIdempotencyStore, IFreedomConfigService), DataProcessResult<T>
            change, MicroserviceBase<T> change, DNA rule change, AGENTS.md/CLAUDE.md
            fundamental change
  Impact: every implementation in Xiigen.Fabrics.* and every caller must be checked
  Protocol: full baseline before/after; read every caller; human review before commit

HIGH — change affects callers in one project/assembly
  Triggers: MicroserviceBase<T> service constructor change, new required DI dependency,
            shared base/record change, cross-assembly type change, .csproj change
  Impact: all callers in the affected project must be checked
  Protocol: grep all callers; run that project's tests before/after

MEDIUM — change affects one class or one method
  Triggers: private method change within a service, new private helper,
            implementation change (not contract change)
  Impact: only the changed class and its direct tests
  Protocol: run the changed class's tests; check its callers

LOW — change affects only future code, not existing callers
  Triggers: new file added, new method added to a class, new test, doc/skill update
  Impact: no existing code affected
  Protocol: standard gate (dotnet build + dotnet test)
```

---

## Assessment Procedure

### Step 1 — Identify Change Type and Location

| Location | Default Blast Radius |
|----------|----------------------|
| `docs/skills/` | LOW |
| `docs/` (other docs) | MEDIUM (doc change) |
| `tests/` only | LOW |
| `src/Xiigen.Kernel/FabricContracts.cs` / any `I*` contract | CRITICAL |
| `src/Xiigen.Kernel/DataProcessResult.cs` / `MicroserviceBase.cs` | CRITICAL |
| `src/Xiigen.Fabrics.*/*.cs` (concrete implementation) | MEDIUM–HIGH |
| `src/Xiigen.Platform.Marketplace/*Service.cs` (feature service) | MEDIUM |
| `src/Xiigen.Platform.Api/Program.cs` (DI composition) | HIGH |
| `AGENTS.md` / `CLAUDE.md` | HIGH (governs all sessions) |
| `*.csproj` | HIGH (assembly changes) |

### Step 2 — Find All Dependents

```bash
# For CRITICAL/HIGH, find every caller of the changed identifier
grep -rn "ISchedulerService\|IDocumentStore\|IOutboxStore" src/ tests/ --include="*.cs" | sort

# For an interface change, find all implementations
grep -rn "class.*:.*IDocumentStore" src/ --include="*.cs"

# For a base-class change, find all services that extend it
grep -rn "MicroserviceBase<" src/ --include="*.cs"

# For a DI/constructor change, find the registrations
grep -rn "AddSingleton\|AddScoped\|AddTransient" src/Xiigen.Platform.Api/ --include="*.cs"
```

Never claim "this is isolated" without running the grep.

### Step 3 — Identify Protection Gates

```
CRITICAL:
  □ dotnet test Xiigen.Platform.slnx before any change (record count)
  □ Read every caller and every Xiigen.Fabrics.* implementation in full
  □ dotnet build Xiigen.Platform.slnx (0 errors, 0 warnings — TreatWarningsAsErrors)
  □ dotnet test after change — total count must be ≥ before count

HIGH:
  □ Record the affected project's test count before change
  □ Read all callers in the affected project (and Program.cs if DI changed)
  □ Run the affected project's tests after change

MEDIUM:
  □ Run the changed class's tests before/after
  □ Check direct callers of the changed method

LOW:
  □ dotnet build: 0 errors / 0 warnings
  □ dotnet test: Failed 0
```

### Step 4 — Write Impact Summary

Before touching any file:

```
Change: Add ListDueAsync(DateTimeOffset before) to ISchedulerService

Blast radius: CRITICAL
  Reason: public fabric interface change — the implementation and all callers affected

Dependents found:
  Implementations: InMemorySchedulerService (src/Xiigen.Fabrics.Scheduler/Scheduler.cs:21)
  Callers: <grep result list with file:line>
  DI registration: AddXiigenInMemoryScheduler (Program.cs)

Protection gates:
  □ Test count before: [dotnet test, record N]
  □ Read InMemorySchedulerService + all callers in full
  □ Add ListDueAsync to the implementation, returning DataProcessResult<...>
  □ Test count after: must be ≥ N (+ new tests for ListDueAsync)

Estimated impact: 1 implementation + callers + their test files
```

---

## Special Cases

### Public Fabric Interface Change (CRITICAL)

```
Adding a method to an existing Xiigen.Kernel I* contract:
  → InMemorySchedulerService / InMemoryDocumentStore / etc. must add the method
  → All callers may need updating
  → Never add a member to a fabric interface without listing all implementations first

Removing a method:
  → Even higher blast radius — ALL callers fail to compile
  → Requires explicit approval before proceeding
```

### Constructor Signature Change (HIGH)

```
Adding a required parameter to a MicroserviceBase<T> service constructor:
  → The DI registration in Program.cs must be updated
  → All "new FooService(...)" calls in tests must be updated
  → grep all instantiation sites before changing
```

### DNA Rule Change (CRITICAL)

```
Changing which DNA rules apply or how they are enforced:
  → Affects every code review and commit in future sessions
  → Update CLAUDE.md/AGENTS.md and dna-compliance-guard-SKILL.md together
  → Requires explicit approval
```

---

## Anti-Patterns

1. **"This is just a small change"** — run the grep before assuming small.
2. **Changing an `I*` contract without listing all `Xiigen.Fabrics.*`
   implementations first** — always list before touching.
3. **Claiming LOW on a fabric interface change** — fabric interface changes are
   always CRITICAL.
4. **Not running the test suite before the change** — no baseline, no delta.

---

## Integration

- `blast-radius-tagger-SKILL.md` tags files; an INTERFACE tag triggers this Assessor.
- `change-propagation-SKILL.md` lists the derived docs that go stale when a contract
  changes.
- `api-shape-verification-SKILL.md` supplies the real signatures to compare against.
