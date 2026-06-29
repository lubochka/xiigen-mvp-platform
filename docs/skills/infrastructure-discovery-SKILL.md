# Infrastructure Discovery

> Ported from the `llm_mvp_core` discovery standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build Xiigen.Platform.slnx` / `dotnet test`. This is **Gate 0** — a
> fresh (not cached) reconnaissance pass before any planning. The first reads are
> the platform Tier-0 anchors: `AGENTS.md`, `CLAUDE.md`,
> `docs/HOW-TO-USE-SKILLS.md` and the v3.3 Beacon. Fabric-first is checked against
> the real `Xiigen.Fabrics.*` contracts. Domain return type is
> `DataProcessResult<T>` (never core `OperationResult<T>`).

## When to Run

Run before Gate 1 of `phase-planning-SKILL.md`. This is Gate 0. Discovery must be
**fresh** — not cached from a previous session. Code and the v3.3 state change
between sessions; the snapshot is current-session only.

---

## The 10-Step Protocol

### Step 0 — Pre-Existing Failure Audit

Run the build and full test suite fresh and record the baseline:

```bash
dotnet build Xiigen.Platform.slnx 2>&1 | tail -10
# Directory.Build.props sets TreatWarningsAsErrors=true → expect 0 errors AND 0 warnings

dotnet test Xiigen.Platform.slnx --logger "console;verbosity=minimal" 2>&1 | tail -10
```

Record:
```
Pre-existing baseline: [timestamp]
  Build: [0 Error(s) / N errors] [0 Warning(s) / N warnings]
  Total: [N] tests
  Passed: [N]
  Failed: [N]  ← This number is the allowed floor. Any NEW failure = blocker.
  Skipped: [N]
```

Any pre-existing failure is listed explicitly. It is NOT a regression for this
phase — but any new failure introduced by this phase IS a regression. Because
warnings are errors here, a new warning is also a blocker.

### Step 1 — Tier-0 / Live Artifact Boundaries

Read the following files completely (record proof of reading for each — one fact
from the end of each file):

```
AGENTS.md                  → binding Beacon, role loop, executor-cannot-stop rule
CLAUDE.md                  → Beacon First, soft-stop ban, enterprise scope
docs/HOW-TO-USE-SKILLS.md  → skills catalogue and load order
docs/plans/enterprise-adaptation/v3.3/XIIGEN-ENTERPRISE-ADAPTATION-BEACON-PROMPT-v1.md
docs/plans/enterprise-adaptation/v3.3/XIIGEN-ENTERPRISE-ADAPTATION-STATE-v3.3.json
docs/plans/enterprise-adaptation/v3.3/XIIGEN-ENTERPRISE-ADAPTATION-COMPLETION-MATRIX-v3.3.md
```

Skills, local habits, and previous plan versions cannot override the Beacon. Do
not rely on memory — these files change between sessions.

### Step 2 — Code Structure Search

For each project the plan touches, verify directory structure and contracts:

```bash
# List all .csproj projects (the unit of "subproject" on platform)
find src/ tests/ -name "*.csproj" | sort

# List the fabric and domain interfaces
grep -rn "public interface I" src/ --include="*.cs" | sort

# List service classes that extend the platform base
grep -rn "class .*: MicroserviceBase" src/ --include="*.cs"
```

The canonical project set is:
`src/Xiigen.{Kernel,Auth,Freedom,Fabrics.InMemory,Fabrics.Scheduler,Platform.Api,Platform.Marketplace}`
with matching `tests/Xiigen.*.Tests`.

### Step 3 — History / Decision Docs

Read the active v3.3 session file and the COMPLETION-MATRIX. There is **no**
`docs/DECISIONS-LOCKED.md` on platform — locked decisions and overrides live in
the v3.3 beacon/state/matrix artifacts. Check whether the plan's approach
conflicts with any matrix row or recorded decision.

### Step 4 — Architecture Map

Identify every dependency edge the plan will create or modify, in terms of the
real fabric contracts:

```
New edge: [Service in Xiigen.X] → [IDocumentStore in Xiigen.Kernel/FabricContracts.cs]
Existing edge (unchanged): [Service] → [ITenantContextAccessor in Xiigen.Kernel/TenantContext.cs]
Removed edge: [Service] no longer depends on [ISchedulerService]
```

Fabric contracts available now (do not invent new ones without `gap-to-proposal`):
- `IDocumentStore`, `IIdempotencyStore`, `IOutboxStore` — `src/Xiigen.Kernel/FabricContracts.cs`
- `ITenantContextAccessor`, `ITenantContextRunner` — `src/Xiigen.Kernel/TenantContext.cs`
- `ICorrelationContextAccessor` — `src/Xiigen.Kernel/CorrelationContext.cs`
- `ISchedulerService` — `src/Xiigen.Fabrics.Scheduler/Scheduler.cs`

### Step 5 — Read Key Files in Full

Read every file the plan will change, completely. For each, record proof — one
fact from the end of the file:

```
Read: src/Xiigen.Platform.Marketplace/SmartWikiService.cs
Proof: Last method is ... returning DataProcessResult<IReadOnlyDictionary<string, object?>>
```

Do not skim. The end of the file often holds the facts that matter most.

### Step 6 — Completeness Map

List every deliverable in the plan and verify it can be found or placed:

```
DELIVERABLE-1: src/Xiigen.Platform.Marketplace/SmartCodeService.cs
  Status: Does not exist yet (NEW)
  Project exists: YES (src/Xiigen.Platform.Marketplace/)
  Contract to depend on: IDocumentStore (confirmed Xiigen.Kernel/FabricContracts.cs:9)

DELIVERABLE-2: src/Xiigen.Kernel/FabricContracts.cs (modify)
  Status: EXISTS
  Current signature: [recorded from Step 5 reading]
  Plan change: [exact method add]
```

### Step 7 — Data Format Audit

For any data structures the plan creates or modifies:

```
Verify:
- DataProcessResult<T> wraps every service return type (DNA Rule 3) — NOT OperationResult<T>
- Request/response records are concrete C# records (e.g. ScheduledJobResult), not anonymous types
- Nullable reference types are handled explicitly (Data is T? on DataProcessResult<T>)
```

### Step 8 — Fabric-First Dependency Check

For every class in the plan that has dependencies:

```bash
# Find classes that instantiate concrete fabric/domain types directly
grep -rn "= new [A-Z]" src/ --include="*.cs" | grep -v "Tests\|// Fabric:\|record\|Exception\|StringBuilder\|Dictionary\|List<\|Uri("
```

Any direct instantiation of a fabric type in a non-test file is a fabric-first
violation (DNA Rule 4). Record it in the Out-of-Scope Issues list for the
preflight. The correct pattern is constructor injection of the interface plus DI
registration in `src/Xiigen.Platform.Api/Program.cs`.

### Step 9 — Rewrite Plan Elements with Real References

After Steps 0–8, update every plan reference to include `file:line`:

```
Before: "Add a search method to SmartWikiService"
After:  "Add SearchAsync() to src/Xiigen.Platform.Marketplace/SmartWikiService.cs:NN"

Before: "Update the document store contract"
After:  "Add DeleteAsync() to src/Xiigen.Kernel/FabricContracts.cs:27"
```

No plan element may reference a file without a verified path.

---

## Discovery Completion Checklist

```
□ Step 0: Pre-existing build/warning/test counts recorded with timestamp
□ Step 1: AGENTS, CLAUDE, HOW-TO-USE-SKILLS, v3.3 Beacon/state/matrix all read
□ Step 2: Project structure verified — all referenced src/tests paths exist
□ Step 3: v3.3 matrix/session checked — no conflicts (no DECISIONS-LOCKED.md on platform)
□ Step 4: New dependency edges documented against real Xiigen.Fabrics.* contracts
□ Step 5: Every file to be changed has been read completely with proof
□ Step 6: Every deliverable verified as placeable (NEW) or locatable (MODIFY)
□ Step 7: Data formats use DataProcessResult<T> and concrete records
□ Step 8: No unrecorded fabric-first violations in scope
□ Step 9: All plan references updated with real file:line citations
```

Only after this checklist is complete: proceed to Gate 1 (DNA check via
`dna-compliance-guard-SKILL.md` + `DNA-DOTNET-TRANSLATION.md`).

---

## Anti-Patterns

1. **Reading only the relevant section** — read the whole file; record end-of-file proof.
2. **Trusting the plan's file paths** — verify every path with a directory/grep check.
3. **Skipping Step 0** — pre-existing failures/warnings must be recorded to distinguish new regressions.
4. **Reusing discovery from last session** — code and v3.3 state change between sessions; discover fresh.
5. **Treating the v3.3 matrix as ground truth without reading the code** — the matrix is intent; the `.cs` file is reality.

---

## Integration

- `phase-planning-SKILL.md` Gate 1 follows this Gate 0.
- `reconnaissance-gate-SKILL.md` Tier-0 reads overlap with Step 1.
- `prerequisite-chain-SKILL.md` consumes the fabric-contract verification from Step 4/8.
- `DNA-DOTNET-TRANSLATION.md` provides the fabric-first detection commands used in Step 8.
