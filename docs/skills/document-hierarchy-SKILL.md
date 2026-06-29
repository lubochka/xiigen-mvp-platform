# document-hierarchy-SKILL.md — One canonical source per fact; higher tier wins

> Ported from the `llm_mvp_core` governance standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build` / `dotnet test`, domain return type `DataProcessResult<T>`
> (`src/Xiigen.Kernel/DataProcessResult.cs`) — **not** core `OperationResult<T>`.
> The canonical document set is platform-specific: the v3.3 Beacon is supreme;
> the v3.3 state + completion matrix are Tier 1; the active `SESSION-XX-*.md` is
> Tier 4. The live test baseline comes from `dotnet test --list-tests`, never memory.

## Core problem
Two documents with conflicting information about the same fact cause planning
errors. One says the test baseline is N; another says M. One plan says a
capability lives in `Xiigen.Platform.Api`; another says `Xiigen.Application`.
This skill prevents new documents from being produced when an authoritative one
already exists, and routes every question to the single correct source.

---

## When to invoke
- Before producing any new `.md` or `.json` document.
- Before answering an architectural question that could reference stale data.
- When two documents appear to conflict.
- When deciding "where does this information live?".

---

## Section 1 — Three checks before producing any document

### Check 1: Does a canonical document already exist for this concern?
```
Read: docs/HOW-TO-USE-SKILLS.md + the v3.3 plan folder index

If a Tier 0–3 document already governs this concern:
  → Do NOT produce a new document
  → Update the existing canonical document (add a section, extend, version it)
  → Never create a competing parallel document

Only create a new document if:
  □ No existing document covers this concern
  □ It is a new session file (SESSION-XX-*.md)
  □ It is a session execution log
  □ It is a session state checkpoint (the v3.3 STATE json equivalent)
```

### Check 2: Is the information consistent with existing documents?
```
A new document must not contradict:
  □ The v3.3 Beacon (XIIGEN-ENTERPRISE-ADAPTATION-BEACON-PROMPT-v1.md) — supreme
  □ AGENTS.md / CLAUDE.md (Tier 0)
  □ The v3.3 state + completion matrix (Tier 1)
  □ docs/DECISIONS-LOCKED.md (locked decisions)

If a conflict is found: the higher-tier document is right.
Investigate why the new information differs before writing anything.
```

### Check 3: Is a superseded document being accidentally referenced?
```
Superseded documents (do NOT use for new planning):
  Any file with -v1, -v2, -v3 where a higher version exists (the active line is v3.3)
  → Use the highest version. Do not propagate superseded content.
```

---

## Section 2 — Routing table

"Where is the answer to this question?"
```
Supreme session law / what to obey first  → docs/plans/enterprise-adaptation/v3.3/XIIGEN-ENTERPRISE-ADAPTATION-BEACON-PROMPT-v1.md
Agent execution rules / role loop         → AGENTS.md
Runtime rules / soft-stop ban             → CLAUDE.md
Current phase state + next_action         → docs/plans/enterprise-adaptation/v3.3/XIIGEN-ENTERPRISE-ADAPTATION-STATE-v3.3.json
Requirement→evidence completion status    → docs/plans/enterprise-adaptation/v3.3/XIIGEN-ENTERPRISE-ADAPTATION-COMPLETION-MATRIX-v3.3.md
What this session must do                  → the active SESSION-XX-*.md
Locked architectural decisions (D-NN)     → docs/DECISIONS-LOCKED.md
Master-tenant launch steps                 → docs/plans/enterprise-adaptation/v3.3/MASTER-TENANT-LAUNCH-RUNBOOK-v1.md
Current test baseline                       → dotnet test --list-tests (live run, not memory)
Which skill governs this work              → docs/HOW-TO-USE-SKILLS.md
Fabric interface contracts                  → src/Xiigen.Kernel/FabricContracts.cs, TenantContext.cs, Scheduler.cs
```

---

## Section 3 — Document tier hierarchy

When two documents conflict, the higher tier wins:
```
Tier 0 (supreme):   The v3.3 Beacon  (XIIGEN-ENTERPRISE-ADAPTATION-BEACON-PROMPT-v1.md)
                    AGENTS.md + CLAUDE.md
                    → Override everything else. Always current.

Tier 1 (primary):   v3.3 STATE json
                    v3.3 COMPLETION MATRIX
                    docs/DECISIONS-LOCKED.md
                    → Maintained after every session.

Tier 2 (reference): Master-tenant launch runbook, legal-documents contract,
                    fabric interface source (src/Xiigen.Kernel/*)
                    → Updated when interfaces or runbook change.

Tier 3 (results):   docs/HOW-TO-USE-SKILLS.md, docs/skills/*
                    → Updated when skills are added/changed.

Tier 4 (session):   SESSION-XX-*.md, execution logs
                    → Superseded by the corresponding Tier 1 updates.
```

**Resolution protocol when documents conflict:**
```
Step 1: Identify which documents conflict on which specific value
Step 2: Apply tier hierarchy — higher tier wins
Step 3: Update the lower-tier document to match the higher-tier document
Step 4: Note the reconciliation in docs/DECISIONS-LOCKED.md if architectural
```

---

## Section 4 — What goes in which document

| Information type | Goes in |
|---|---|
| Locked architectural decisions (D-NN) | docs/DECISIONS-LOCKED.md |
| Current phase state + next_action | v3.3 STATE json |
| Requirement→evidence status | v3.3 completion matrix |
| Fabric interface contracts | src/Xiigen.Kernel/* |
| Master-tenant launch steps | MASTER-TENANT-LAUNCH-RUNBOOK-v1.md |
| Skill activation triggers | docs/HOW-TO-USE-SKILLS.md |
| Agent execution / role-loop rules | AGENTS.md |
| Runtime rules / soft-stop ban | CLAUDE.md |
| What was done this session | SESSION-XX-*.md (session-level) |

---

## Anti-patterns
1. **"I'll create PHASE-N-ARCHITECTURE.md for this phase's decisions"** —
   architecture decisions belong in `docs/DECISIONS-LOCKED.md`. Create a D-NN entry, not a new file.
2. **"The baseline was N last session"** — session state is ephemeral. The baseline
   comes from `dotnet test --list-tests`. Memory is not a baseline.
3. **"According to the SESSION-03 plan baseline…"** — session files are Tier 4.
   The live test run is authoritative, not a session file.
4. **"Let me document this decision in the session file"** — decisions that govern
   future sessions belong in `docs/DECISIONS-LOCKED.md`; session files are superseded.

---

## Integration
- `planning-session-startup-SKILL.md` uses this tier hierarchy for governing documents.
- `decision-capture-SKILL.md` routes decisions to `docs/DECISIONS-LOCKED.md`.
- `decision-reopening-SKILL.md` treats the Beacon as supreme/never-auto-reopened.
- `session-scope-resolution-SKILL.md` uses this routing to pick minimum documents per session type.
