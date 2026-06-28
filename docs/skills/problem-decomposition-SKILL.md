# Problem Decomposition

> Ported from the `llm_mvp_core` decomposition standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build Xiigen.Platform.slnx` / `dotnet test`. Verify commands are
> `dotnet test --filter "FullyQualifiedName~..."` and `dotnet build`. The
> architectural level records into the v3.3 beacon/decision artifact (there is no
> `DECISIONS-LOCKED.md` on platform). "Minimum information" is the failing xUnit
> test + the class under test + its fabric contract in `Xiigen.Kernel`. Domain
> return type is `DataProcessResult<T>` (never core `OperationResult<T>`).

## When to Invoke

At the start of any session where the problem is complex or ambiguous. Before
reading any file, before proposing any fix, before writing any plan.

---

## The Five Steps

### Step 1 — Symptom or Root Cause?

Ask "why does this problem exist?" three times.

```
First answer  = immediate cause    → fix with a targeted code change
Second answer = structural cause   → fix with a pattern or design update
Third answer  = architectural cause → requires a design decision or new component
```

**Rule:** Identify the level before proposing the solution. A Level 1 solution
for a Level 3 problem fixes today and breaks tomorrow.

**Example on platform:**
```
Problem: SmartWikiService.SearchAsync returns duplicate documents for some tenants

Level 1: De-dup the result list in SearchAsync()                → fixes this instance
Level 2: IDocumentStore.SearchAsync filter is not tenant-scoped → add tenant key to
         every filter built in the service layer
Level 3: Tenant scoping is enforced ad-hoc per service rather than centrally
         Architectural: tenant scope must be applied by the store using
         ITenantContextAccessor.Current, not by each caller.
```

The right level depends on whether this is a one-time issue or a systematic pattern.

### Step 2 — Minimum Information Needed

Do NOT read everything. Answer: which 3–5 files tell the whole story?

```
For test failures:         failing xUnit test + class under test + its fabric contract
For service bugs:          the specific method + its callers + its xUnit coverage
For contract questions:    Xiigen.Kernel/FabricContracts.cs + the impl + one test
For session planning:      v3.3 STATE.json + the SESSION-XX section + test baseline
For "where does X live":   grep across src/ — not reading all projects
```

**Rule:** Start narrow. Widen only when the narrow read is insufficient. Every
extra file read without a specific question is noise.

### Step 3 — Classify Incoming Claims

Before accepting any factual claim (from a review, a prior session summary, or
another model):

```
VERIFIED = a command that proves it is cited → spot-check the command yourself
INFERRED = reasonable deduction, command not run → run the verification yourself
ASSUMED  = stated as fact, no command shown → treat as hypothesis until confirmed
```

**Rule:** Test counts, interface signatures, file existence, line numbers —
ALWAYS verify before using in a plan. One grep costs 10 seconds; a wrong number
costs a blocked session.

### Step 4 — Scope the Response

Start at the lowest scope that genuinely solves the problem:

```
METHOD CHANGE → CLASS CHANGE → INTERFACE/CONTRACT CHANGE → PROJECT CHANGE → ARCHITECTURE CHANGE
```

Document why each lower scope is insufficient before claiming the higher one
needs to change.

```
Problem: Need to expose a "list scheduled jobs since T" capability

Method change: Add a `since` parameter overload to ISchedulerService.ListAsync()?
  Decision: Adding the parameter changes the contract → this is a CONTRACT change,
  not a method change. Every ISchedulerService implementation + test must update.
  Why not project change: no new Xiigen.* project is needed.
```

### Step 5 — Produce Executable Output

Output is commands and facts, not prose. The next session must run the plan
without interpretation. Include:
- Exact file paths (not descriptions of where to look).
- Exact grep commands with expected output.
- Exact `dotnet test --filter` commands with expected pass counts.
- Discoveries and rejected claims (see `investigation-handoff-SKILL.md`).

---

## Anti-Patterns

1. **Starting to fix before identifying the level** — a null check may be correct for Level 1 but wrong if the structural cause is in an upstream class.
2. **Reading all files before identifying which 3 answer the question** — information overload buries the actual signal.
3. **Accepting summary numbers without running verification** — verify test counts, line numbers, class names with grep before using in a plan.
4. **Proposing infrastructure for a problem a one-line fix solves** — over-engineering.
5. **Treating every problem as architectural** — some problems are just wrong values in the right place.

---

## Level Decision Reference

| If the third "why" answer is... | Fix level |
|--------------------------------|-----------|
| A missing null/guard check | Method-level fix |
| A wrong return shape on a service method | Class-level fix |
| A fabric contract that does not enforce a needed invariant | Contract-level fix (`Xiigen.Kernel/FabricContracts.cs`) + update all impls/tests |
| A design decision never made explicit | Architecture-level decision recorded in the v3.3 beacon/decision artifact |
| A DNA rule that was not applied | Governance fix (update `CLAUDE.md` / `dna-compliance-guard-SKILL.md` / `DNA-DOTNET-TRANSLATION.md`) |

---

## Integration

- `infrastructure-discovery-SKILL.md` Gate 0 supplies the verified file:line references Step 5 needs.
- `prerequisite-chain-SKILL.md` classifies each dependency surfaced in Step 1 as PASS/FAIL/BLOCKING.
- `root-cause-ladder-SKILL.md` / `root-cause-tracing-SKILL.md` deepen the three-why analysis in Step 1.
- `solution-scope-gate-SKILL.md` formalizes the scope ladder in Step 4.
