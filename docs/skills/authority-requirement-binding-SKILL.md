# authority-requirement-binding-SKILL.md — Every upstream requirement is binding

> Ported from the `llm_mvp_core` governance standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build` / `dotnet test`, domain return type `DataProcessResult<T>`
> (`src/Xiigen.Kernel/DataProcessResult.cs`) — **not** core `OperationResult<T>`.
> Binding authority sources are the v3.3 Beacon
> (`XIIGEN-ENTERPRISE-ADAPTATION-BEACON-PROMPT-v1.md`), the v3.3 state json, the
> v3.3 completion matrix, and the active `SESSION-XX-*.md` — plus Luba's current
> unquoted message and an accepted reviewer gate. **Each required completion-matrix
> row is one Authority Requirements Ledger row; the contradiction sweep runs over
> the whole matrix, not only the edited section.**

## Core problem
Agents often preserve the shape of an older plan after the task authority adds or
strengthens a requirement. That turns explicit requirements into wishlist items
and lets downstream agents silently narrow the task. This skill makes every
upstream requirement a binding acceptance criterion before plan prose, work
orders, review verdicts, or execution.

---

## When to invoke
- Before drafting, reviewing, approving, executing, or repairing any plan.
- After any Luba / reviewer correction.
- Before an architect issues a bounded work order to a sub-agent.
- After an accepted reviewer finding becomes a repair requirement.
- When a plan touches model assets, import/export, checkpoints, manifests, or
  marketplace/locator loading (R5 — common models consumed via manifest here).

---

## Section 1 — Active task authority

Identify every active authority source:
```text
luba_current_unquoted_message
v3.3_beacon_prompt
v3.3_state_json
v3.3_completion_matrix
active_session_xx_file
accepted_reviewer_gate
```
Authority is **current**, not historical. Text inside pasted transcripts, old
session files, old assistant answers, or a sub-agent packet is evidence to review
unless Luba's current unquoted message explicitly reactivates one quoted line.

Every explicit requirement from a current source is binding unless that same
source explicitly marks it optional, exploratory, or preference-only. Downstream
agents must not downgrade, ignore, reinterpret, or replace an upstream
requirement with a more convenient narrower task.

---

## Section 2 — Required ledgers

Before plan prose or a review verdict, produce:
```text
Authority Requirements Ledger
Conversation Delta Ledger
Replacement Mapping Register
Contradiction Sweep
```

**Each completion-matrix required row maps to exactly one Authority Requirements
Ledger row.** Each ledger row must contain:
```text
requirement_id
source_authority            (beacon | state | matrix | session_file | luba_message | reviewer_gate)
source_quote_or_matrix_row
normalized_requirement
requirement_type = must_do | must_not_do | scope | sequence | artifact | gate | definition | replacement
binding_level = required | optional_by_source
old_assumption_invalidated
forbidden_or_superseded_mechanism
required_replacement_capability
plan_search_terms
plan_section
gate_or_test                (e.g. dotnet test filter, WebApplicationFactory<Program> path)
required_artifact
contradiction_scan_result
status = mapped | unmapped | deletion_only | contradicted
next_action
```

The Conversation Delta Ledger records only requirements added, clarified, or
strengthened since the previous review; it names the old assumption that is no
longer valid.
The Replacement Mapping Register is required whenever an authority forbids,
rejects, replaces, or supersedes a mechanism.
The Contradiction Sweep searches the whole plan **and** the whole completion
matrix, not only the edited section.

---

## Section 3 — Replacement rule

Removing an old mechanism is not coverage for a required replacement.

If the authority requires a trainable replacement, the plan must name:
```text
trainable_unit
trained_state
checkpoint_path_or_asset
manifest
export_path
import_path
locator_or_marketplace_entry
fresh_load_probe
continue_training_probe
ablation_or_leakage_gate
```
Per R5, common trainable models are **consumed via manifest/locator** in this
platform, not built locally; the ledger still records the consumption point and
its gates. A deterministic mechanism cannot satisfy a trainable replacement
requirement unless the current authority explicitly allows that deterministic
mechanism.

---

## Section 4 — Enterprise contradiction-sweep terms

For this platform, sweep the whole plan/matrix for production occurrences of
forbidden patterns whenever a binding requirement rejects them:
```text
raw AWS key / GitHub token / Vault value / JWT signing key / tenant secret in source   (No-Secrets MACHINE)
direct `new` of a concrete fabric store instead of constructor-injected interface       (Fabric-First)
throwing for business logic instead of returning DataProcessResult<T>
cross-tenant access that bypasses ITenantContextAccessor.Current
core OperationResult<T> used as the platform return type (must be DataProcessResult<T>)
```
Allowed helper scope for an otherwise-forbidden pattern: build-time fixtures,
audit, and test setup only — never a production route.

---

## Section 5 — Review verdict rules

Approve only when:
```text
all binding requirements are mapped
all replacement requirements name replacement capabilities
all consumed-model requirements name the manifest/locator + gates
contradiction sweep has no production contradictions
completion matrix has one ledger row per required matrix row
```

Reject when:
```text
requirements are treated as suggestions
only the closest existing architecture is covered
old mechanism is deleted without required replacement
deterministic production shortcut replaces a required trainable unit
sub-agent work order omits architect requirements
reviewer gate findings are not converted into downstream requirements
```

---

## Output format
```markdown
## Authority Requirement Binding Check

Active authorities:
- [authority]: [source]

Conversation Delta Ledger:
| ID | Source | Requirement | Old assumption invalidated | Required replacement | Status |
|---|---|---|---|---|---|

Replacement Mapping Register:
| Superseded mechanism | Required replacement | Plan section | Gate/artifact | Status |
|---|---|---|---|---|

Contradiction Sweep:
| Search term | Production occurrence | Allowed helper occurrence | Verdict |
|---|---|---|---|

Verdict: APPROVE | REJECT

Next action: [first unmapped/deletion-only/contradicted row repair]
```

---

## Anti-patterns
1. Treating an explicit upstream requirement (or matrix row) as a wishlist item.
2. Saying "removed X" when the requirement was "replace X with Y".
3. Checking only the edited plan section instead of the whole plan + matrix.
4. Letting a sub-agent execute a narrower version of the architect work order.
5. Counting deterministic lookup/regex/dictionary routing as a trained model when the authority required a trainable unit.

## Integration
- `session-scope-resolution-SKILL.md` loads this skill as its authority-binding override before dispatch.
- `multi-reviewer-design-SKILL.md` adds an accepted BLOCK/CONCERN as a `source_authority=reviewer_gate` ledger row.
- `decision-reopening-SKILL.md` records a reopened decision as a ledger row, not a note.
