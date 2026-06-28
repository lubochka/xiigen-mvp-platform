# Change Propagation (XIIGen Enterprise Platform)

## The Prior Question

Before any task begins — not after it appears done — ask:

**"What are all the files that will be stale the moment this change is made?"**

That list is the blast radius. Writing it down before starting means nothing is
discovered reactively at the end. The task is not done until every item on the list
is checked off.

Platform port of the core `change-propagation` skill, re-bound to
**.NET 10 ASP.NET Core Web API**, **xUnit**, the real `Xiigen.*` projects and
`Xiigen.Platform.slnx`, and the platform governance surface (AGENTS.md / CLAUDE.md +
`docs/`). Gate-B is `dotnet build Xiigen.Platform.slnx` + `dotnet test`.

## When to load

- At the START of any task that adds a skill, changes a contract, adds a project, or
  locks a decision.
- Whenever a source-of-truth file changes and derived views may go stale.

---

## The Dependency Map

Six change types, each with a complete blast radius.

### Change Type 1: SKILL ADDED (new `docs/skills/*.md`)

```
docs/HOW-TO-USE-SKILLS.md
  - Add to the skill index / activation-trigger list
  - Update any version/count field in the header
AGENTS.md / CLAUDE.md (if the skill changes agent behavior rules)
  - Reference the skill in the relevant governance section
```

Before starting: confirm `docs/HOW-TO-USE-SKILLS.md` exists and note the fields to
update.

### Change Type 2: SKILL UPDATED (content change, no new file)

```
docs/HOW-TO-USE-SKILLS.md
  - Only if the update changes WHEN the skill fires or WHAT it prevents
```
Verify: `grep -n "<skill>-SKILL.md" docs/HOW-TO-USE-SKILLS.md`.

### Change Type 3: INTERFACE ADDED OR CHANGED (a `Xiigen.Kernel` `I*` contract)

```
docs/ (interfaces/contracts doc)
  - Add or update the interface signature block + implementing classes
docs/ (project/architecture doc)
  - Update if the contract changes a structural relationship
src/Xiigen.Platform.Api/Program.cs
  - Add/adjust DI registration (services.AddSingleton<INew, Impl>())
AGENTS.md / CLAUDE.md
  - If it is a primary fabric contract, reference it in the fabric/DNA section
```
Verify:
```bash
grep -rn "INewContract" src/ --include="*.cs" | grep -i "AddSingleton\|AddScoped\|AddTransient"
grep -rn "class.*:.*INewContract" src/ --include="*.cs"   # implementations exist
```

### Change Type 4: PROJECT (`.csproj`) ADDED

```
Xiigen.Platform.slnx — add the project node
docs/ (project map / architecture) — add a row/node
src/Xiigen.Platform.Api/Program.cs — register the new project's services if hosted
CLAUDE.md — update the test baseline count if the project adds tests
The new .csproj + matching tests/Xiigen.<Name>.Tests — created at project start
```

### Change Type 5: PLAN PRODUCED OR AMENDED

```
The plan/execution file itself (docs/plans/*.md)
AGENTS.md / CLAUDE.md / docs decisions — any newly locked decision from the plan
Any STATE/matrix artifact — update if phase ordering changed
Other plan files that reference the amended plan's phase count — update cross-refs
```

### Change Type 6: DECISION LOCKED

```
docs/ decisions surface (or AGENTS.md/CLAUDE.md governance) — add the D-NN entry
docs/ (interfaces/contracts doc) — add "See D-NN" if it governs a contract
CLAUDE.md — reference it if it affects the DNA rules or MicroserviceBase<T> pattern
All plan files drafted before the decision — grep the affected component; update
```

---

## How to Use This Skill

### At task start (not end):
```
1. Identify the change type(s) above.
2. Write out the blast radius for that type.
3. Cross off files not applicable to this specific change.
4. What remains is the definition of "done".
```

### At task end:
```
For each file in the blast radius:
  □ Was it updated?
  □ Can the update be verified? (grep, dotnet build, dotnet test)
  □ If not updated: explicit reason why not needed for this change.
```

---

## Sources of Truth vs Derived Views

**Sources of truth** (one owner, one writer):
- Each `src/Xiigen.*/**/*.cs` implementation file (owns its own code).
- Each `docs/skills/*-SKILL.md` (owns its own content).
- The decisions surface (AGENTS.md/CLAUDE.md governance or a `docs/` decisions file).

**Derived views** (must be kept in sync):
- `docs/HOW-TO-USE-SKILLS.md` — aggregates from skill files.
- The interfaces/contracts doc — aggregates from `Xiigen.Kernel` `I*` contracts.
- The project map / architecture doc — aggregates from project status.
- `CLAUDE.md` / `AGENTS.md` — aggregate conventions from all of the above.

**The rule:** when a source changes, every derived view that references it updates in
the **same commit**. Not the next session. Not when noticed. Same commit. (See
`documentation-sync-SKILL.md`.)

---

## Test Strategy (per branch)

- **Verification commands** (above) are the test: each blast-radius item has a
  `grep`/`dotnet build`/`dotnet test` check that proves the derived view matches the
  source.
- **Gate-B:** for INTERFACE/PROJECT changes, `dotnet build Xiigen.Platform.slnx` (0/0)
  + `dotnet test` (0 failed) proves the DI/registration propagation compiled and ran.
- **Doc-accuracy check:** a contracts-doc entry is verified against the real signature
  via `grep` in `Xiigen.Kernel`.

---

## Anti-Patterns

1. **Closing a "skill added" task after updating only the skill file** —
   `HOW-TO-USE-SKILLS.md` is stale; the next session misses the activation trigger.
2. **"I'll update the other files next session"** — partial updates are the root cause
   of documentation drift.
3. **Discovering the blast radius at the end** — it is identified at the START; it
   defines "done".
4. **Updating a derived view instead of the source** — if the contracts doc is stale
   because the `Xiigen.Kernel` `I*` changed, update the interface (source) and
   regenerate the doc entry (derived). Do not paper over a stale source.

---

## Integration

- `blast-radius-SKILL.md` classifies the severity of the change whose propagation this
  skill enumerates.
- `documentation-sync-SKILL.md` is the same-commit enforcement of the derived-view
  updates listed here.
