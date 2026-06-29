# decision-reopening-SKILL.md — The gate for changing a locked decision

> Ported from the `llm_mvp_core` governance standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build` / `dotnet test`, domain return type `DataProcessResult<T>`
> (`src/Xiigen.Kernel/DataProcessResult.cs`) — **not** core `OperationResult<T>`.
> Locked-decision source = `docs/DECISIONS-LOCKED.md` (create it if missing) plus
> the decisions embedded in the v3.3 state / completion matrix
> (`docs/plans/enterprise-adaptation/v3.3/`). **The v3.3 Beacon
> (`XIIGEN-ENTERPRISE-ADAPTATION-BEACON-PROMPT-v1.md`) cannot be reopened without
> an explicit current Luba instruction** — it is supreme over skills and habits.

## Purpose
Locked decisions prevent a recurring failure mode: every session re-derives the
same conclusions from first principles, wastes time, and sometimes lands on a
different answer because the reasoning context was lost. But architecture must
evolve. This skill defines the narrow gate through which a decision legitimately changes.

---

## When to invoke
Any time a session or response challenges a locked decision:
- "I think we should reconsider D-NN."
- "This decision doesn't work for the new context."
- "The endpoint/DTO contract needs to change because…"
- Any suggestion that contradicts a `D-`series entry in `docs/DECISIONS-LOCKED.md`
  or a locked row in the v3.3 state/matrix.

---

## Four-step test

A decision may be reopened only if it passes all four steps.

### Step 1 — Is this decision actually locked?
```bash
grep -n "D-[0-9][0-9]" docs/DECISIONS-LOCKED.md | head -20
```
Also check the v3.3 state / completion matrix for a locked row on the same concern.
- NOT locked anywhere → proceed freely.
- Locked → apply steps 2–4.
- If the challenge targets the **v3.3 Beacon** → STOP; the Beacon is reopened
  only by an explicit current Luba instruction.

---

### Step 2 — What is the trigger for reopening?

**Valid triggers (any one is sufficient):**
```
□ New information that did not exist when the decision was made
  Example: "The locked decision assumes a single-tenant outbox, but the new
   master-tenant runbook requires per-tenant CloudEvents partitioning."

□ The decision causes a concrete failure in the current plan
  Example: "The locked decision says IDocumentStore is only InMemory, but the
   integration test needs a persistent store and there is no way to inject one."

□ The decision's rationale was invalidated by a later decision
  Example: "D-06 chose inline dispatch as default, but a later decision added
   IOutboxStore — inline is now the wrong default for cross-service events."
```

**Invalid triggers (do not reopen):**
```
✗ "I just think there's a better way"
✗ "This would be simpler if we changed it"
✗ "The other approach is more familiar"
✗ Re-examining without new information
```

---

### Step 3 — Write the ADR entry BEFORE changing anything

Append to the Decision Change Log in `docs/DECISIONS-LOCKED.md`:
```markdown
## Decision Change Log

### D-NN Amendment — [today's date]
**Changed:** [what is changing]
**Rationale:** [the valid trigger from step 2 — exact new information or failure]
**Impact:** [which projects/endpoints/DTOs/fabric interfaces/state rows need updating]
**Previous decision text:** [quote the original]
**New decision text:** [the replacement]
```
The ADR entry must exist as a written record **BEFORE** any file is edited or any
implementation changes. Capture it via `decision-capture-SKILL.md`.

---

### Step 4 — Scope the impact and update all affected documents
After writing the ADR entry:
1. List every document / project that references this decision.
2. Update each one — no partial updates.
3. Run Gate-B after all updates:
```bash
grep -rn "D-NN\|decisionNN" docs/ src/ tests/ --include="*.md" --include="*.cs" --include="*.json"
dotnet build Xiigen.Platform.slnx
dotnet test
```
Run the build/test only when the active session file opens that gate
(`CLAUDE.md` "Governance Startup" — broad commands open only when the session opens them).

---

## Positive example
```
Trigger: "The locked decision says ISchedulerService is the only delayed-work
  path, but the resend flow needs an idempotent reservation first, and there is
  no documented order between IIdempotencyStore.TryReserveAsync and
  ISchedulerService.ScheduleDelayedAsync."

Step 1: locked in DECISIONS-LOCKED.md → apply steps 2–4.
Step 2: valid trigger — concrete failure / missing contract in the current plan.
Step 3: write ADR: "Amended: a resend operation reserves via
  IIdempotencyStore.TryReserveAsync(ttl) before ISchedulerService.ScheduleDelayedAsync;
  the scheduler idempotencyKey reuses the reservation key. Contract unchanged;
  ordering made explicit."
Step 4: update DECISIONS-LOCKED.md, the affected flow doc, the integration test;
  run dotnet build + dotnet test if the session opened that gate.
```

---

## Negative example
```
Trigger: "I think we should use a different idempotency key format."
WRONG:
  → Re-debating the locked decision without new information
  → No concrete failure cited
  → No ADR entry written
  → Changing the format in a comment that contradicts the locked decision
```

---

## Integration
- `decision-capture-SKILL.md` writes the ADR entry into `docs/DECISIONS-LOCKED.md`.
- `document-hierarchy-SKILL.md` — higher tier wins; the Beacon is supreme.
- `solution-scope-gate-SKILL.md` — a Level-5 change that replaces an existing interface must pass this gate first (Backward-Compat reframe test).
- `authority-requirement-binding-SKILL.md` — a reopened decision becomes a ledger row, not a note.
