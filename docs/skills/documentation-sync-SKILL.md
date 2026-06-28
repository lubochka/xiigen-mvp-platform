# Documentation Sync (XIIGen Enterprise Platform)

## When to Sync

Sync documentation **before** committing, not after. The commit message lists the
changed files, and doc files are changed files.

Platform port of the core `documentation-sync` skill, re-bound to
**.NET 10 ASP.NET Core Web API**, **xUnit**, the real `Xiigen.*` projects and
`Xiigen.Platform.slnx`, and the platform's documentation surface (`docs/`,
`docs/skills/`, `AGENTS.md`, `CLAUDE.md`). Gate-B is
`dotnet build Xiigen.Platform.slnx` + `dotnet test`.

Documentation is part of the same commit as the code that made it stale. A code
change that alters a contract, lifecycle, event, DI registration, or runner output
updates the matching doc in the same slice. Do not claim done while known
documentation drift remains.

## When to load

- Before committing any phase that changed a contract, project, decision, or skill.
- At session end as the sync checklist.

---

## File → Canonical Docs Maintenance Map

| What Changed | Update These Docs |
|--------------|-------------------|
| New `.cs` class/service in `src/Xiigen.*` | project/architecture doc (status) |
| New public fabric contract (`I*` in `Xiigen.Kernel`) | interfaces/contracts doc (signature block + implementations) |
| New project (`.csproj` added) | project map (add row), architecture doc (add node), `Xiigen.Platform.slnx`, `CLAUDE.md` (Quick Reference if canonical) |
| Contract signature changed | interfaces/contracts doc (update signature), architecture doc (if in a diagram) |
| New algorithm/technique introduced | architecture doc (relevant section), FREEDOM config keys documented |
| New event type (`CloudEventEnvelope.Type`) | event/contracts doc (Type + payload + correlation + compensation) |
| New ASP.NET Core endpoint exposed | the API doc: human name, examples, parameters, outputs, evidence boundary per endpoint |
| DI registration added/changed (`Program.cs`) | interfaces/contracts doc (registration note) |
| New xUnit test class/file | project map (test-count column if tracked) |
| DNA rule applied for the first time in a new context | `docs/skills/dna-compliance-guard-SKILL.md` (examples) |
| Locked decision made | decisions surface (AGENTS.md/CLAUDE.md or a `docs/` decisions file) |
| New skill added to `docs/skills/` | `docs/HOW-TO-USE-SKILLS.md` (index + activation trigger) |
| Phase plan completed | `docs/plans/[PLAN].md` (status), STATE/matrix artifact (COMPLETE) |

---

## What "Synced" Means

A doc is synced when:
- The information matches what is currently true in the code.
- No section describes a thing that no longer exists.
- No section is missing for a thing that now exists.
- Version numbers / counts / lists are accurate.

A doc is NOT synced by:
- Adding a placeholder "TODO: update this".
- Updating a different doc and assuming cross-references are fine.
- Updating the doc in the next commit.
- Adding tests or a QA report while leaving the human API/contract doc stale.

For the ASP.NET Core Web API surface, the doc must say exactly what the evidence
proves: contract/unit test, integration test against `Xiigen.Fabrics.InMemory`, and
real HTTP endpoint behavior are separate evidence scopes — do not conflate them.

---

## Contracts Doc Sync

When a fabric contract is added or changed, add or update its block:

```markdown
### IDocumentStore

**Assembly:** `Xiigen.Kernel` (src/Xiigen.Kernel/FabricContracts.cs)
**Purpose:** Tenant-scoped document persistence and search.

```csharp
public interface IDocumentStore
{
    Task<DataProcessResult<DocumentRecord?>> GetAsync(string collection, string id, CancellationToken ct = default);
    Task<DataProcessResult<IReadOnlyList<DocumentRecord>>> SearchAsync(string collection, IReadOnlyDictionary<string, object?> filter, CancellationToken ct = default);
    Task<DataProcessResult<DocumentRecord>> UpsertAsync(string collection, string id, IReadOnlyDictionary<string, object?> body, string? expectedVersion = null, CancellationToken ct = default);
}
```

**Implementations:**
- `InMemoryDocumentStore` — src/Xiigen.Fabrics.InMemory/

**Registered in:** `Xiigen.Platform.Api/Program.cs`

**Used by:** `SmartWikiService` (`Xiigen.Platform.Marketplace`), and any read/write service
```

Return wrappers are `DataProcessResult<T>` (or non-generic `DataProcessResult`),
**not** `OperationResult<T>`.

---

## Project Map Sync

When a project's status changes, update its row:

| # | Name | Status | Tests |
|---|------|--------|-------|
| — | Xiigen.Platform.Marketplace | Active / Planned / Complete | X tests |

---

## HOW-TO-USE-SKILLS.md Sync

When a new skill file is added to `docs/skills/`, add it to the index and the
activation-trigger table:

```markdown
| <skill>-SKILL.md | When to use it | Activation trigger |
```

---

## Sync Checklist (Session End)

```
□ Contracts doc: reflects all fabric contracts added/changed this session
□ Project map: reflects all projects and status changes
□ Architecture doc: reflects structural changes (new project, new dependency edge)
□ CLAUDE.md: test baseline count updated if it changed (from a FRESH dotnet test run)
□ HOW-TO-USE-SKILLS.md: new skills added to the index + triggers
□ API doc: each exposed endpoint has a human name, examples, parameters, outputs,
  and an evidence boundary
□ Event/contracts doc: new CloudEventEnvelope.Type values documented with payload +
  correlation + compensation
□ Decisions surface: any locked decisions recorded
□ STATE/matrix artifact: phase state saved with deliverables and test count
□ .xiigen/wiki-domain: docs state whether the consumed model has trained weights or
  only runner-missing/manifest readiness (R5 — the model itself lives in llm_mvp_core)
```

Not every session triggers every row. Skip inapplicable rows — but confirm they are
inapplicable; do not assume.

---

## Test Strategy (per branch)

- **Doc-accuracy check:** every contracts-doc entry is verified against the real
  signature via `grep` in `Xiigen.Kernel`/`Xiigen.Fabrics.*`.
- **Count freshness:** any test-count number in `CLAUDE.md`/project map comes from a
  fresh `dotnet test Xiigen.Platform.slnx` run, not memory.
- **Gate-B:** `dotnet build Xiigen.Platform.slnx` (0/0) + `dotnet test` (0 failed) for
  the slice whose docs are being synced.

---

## Anti-Patterns

1. **Post-commit doc update** — docs are part of the commit, not a follow-up.
2. **Updating the `CLAUDE.md` test count without running `dotnet test`** — the number
   must come from a fresh run.
3. **"I'll sync docs next session"** — there is no next session for this doc; it rots.
4. **Updating the contract in the contracts doc without updating the implementation
   list** — partial sync is worse than no sync.

---

## Integration

- `change-propagation-SKILL.md` enumerates which derived docs each code change makes
  stale; this skill is the same-commit enforcement.
- `dna-compliance-guard-SKILL.md` is the code gate that must be green before the
  synced docs are committed.
