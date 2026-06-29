# multi-reviewer-design-SKILL.md — Multi-perspective review with independent verdicts

> Ported from the `llm_mvp_core` governance standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build` / `dotnet test`, domain return type `DataProcessResult<T>`
> (`src/Xiigen.Kernel/DataProcessResult.cs`) — **not** core `OperationResult<T>`.
> This raises the single-step prose `REVIEW_INTERNAL` from `AGENTS.md`/`CLAUDE.md`
> into a formal multi-perspective panel. Evidence = `file:line` over `.cs` in
> `src/`/`tests/` + `dotnet build`/`dotnet test`. "Public interface" = an ASP.NET
> Core Web API endpoint / DTO contract. **No UI** — the Completeness/Business
> reviewers work through API scenarios. **NOT ported (note-only, R5):** the core
> "10 Review Cycle Gate" for Claude/Cursor/Codex three-extensions and the
> trainable/arbiter-gate hook are out of scope for this platform.

Accepted reviewer gates are active task authority for downstream repair. Once a
BLOCK or accepted CONCERN is adopted by the governance loop, it must be entered
into the Authority Requirements Ledger with `source_authority=reviewer_gate`
(see `authority-requirement-binding-SKILL.md`). A downstream architect or
executor must not treat it as a note, preference, or optional wishlist item.

## When to invoke
- Before any new Web API endpoint/DTO, fabric interface, or architecture decision is finalized.
- When a design touches multiple domains (security + business + principles).
- When a plan has more than 3 phases and needs multi-stakeholder validation.
- When Luba asks for a thorough review before a major implementation starts.

---

## Section 1 — The 5 reviewer roles

Select reviewers by the capability's domain and risk level.

| Role | Context package (what they see) | What they check |
|---|---|---|
| **Domain Reviewer** | Capability spec, flow invariants, domain context | Intent correctness, domain-invariant completeness |
| **Security Reviewer** | Capability spec, No-Secrets rule, tenant-scope, failure modes | No-Secrets compliance, tenant isolation (`ITenantContextAccessor`), Fabric-First |
| **Business Reviewer** | Capability spec, downstream contracts, CloudEvents schemas | Cross-module consistency, consumer/event impact |
| **Principles Reviewer** | Capability spec, v3.3 Beacon + state/matrix, DECISIONS-LOCKED.md | `DataProcessResult<T>` everywhere, locked-decision/matrix adherence |
| **Completeness Reviewer** | Capability spec, endpoint/DTO fields | All outputs reachable from inputs; no missing API path |

**Mandatory by risk level:**
| Capability type | Minimum reviewer set |
|---|---|
| Internal utility | Domain + Principles |
| Public interface (Web API endpoint / DTO / `IXxx`) | Domain + Security + Principles |
| Cross-module contract / CloudEvents | Domain + Security + Business + Principles |
| Architecture decision | All 5 reviewers |

**Never add reviewers for appearance.** A Security reviewer on a pure-compute
helper that touches no storage and no tenant data adds noise without signal.

---

## Section 2 — Context Isolation Rule
Each reviewer receives ONLY their defined context package.
- Domain Reviewer: does NOT see security patterns or principles text.
- Security Reviewer: does NOT see domain invariants — only the spec + No-Secrets/tenant/Fabric rules + failure modes.
- Principles Reviewer: does NOT see domain context or security patterns — only the spec + `DataProcessResult<T>`/DECISIONS/matrix.

**Context isolation ensures independent verdicts.** A reviewer who has seen
another reviewer's verdict anchors to it instead of finding independent issues.

---

## Section 3 — Verdict format
```markdown
## [Role] Reviewer Verdict

Verdict: APPROVED | CONCERN | BLOCK

BLOCK items (plan cannot proceed without resolving these):
- [specific issue] — [which rule/principle is violated] — [what must change]

CONCERN items (plan may proceed but author must respond in writing):
- [issue] — [what this risks if unaddressed]

Evidence:
- [exact file:line in src/ or tests/, or grep / dotnet test result, per BLOCK/CONCERN]
```

**BLOCK class:** Business-logic violations; Security violations (secret in source,
cross-tenant access, direct `new` of a concrete fabric store); Principles
violations (`throw` for business logic instead of `DataProcessResult<T>`, locked
decision/matrix breach); flow-invariant violations.

**A single BLOCK overrides all APPROVED verdicts.** BLOCK cannot be diluted by averaging.

---

## Section 4 — The Convergence Protocol

### Round 1: collect verdicts
All reviewers produce verdicts independently. Record:
```
Domain Reviewer:      [APPROVED | CONCERN | BLOCK]
Security Reviewer:    [APPROVED | CONCERN | BLOCK]
Business Reviewer:    [APPROVED | CONCERN | BLOCK]
Principles Reviewer:  [APPROVED | CONCERN | BLOCK]
Completeness Reviewer:[APPROVED | CONCERN | BLOCK]
```

### Decision rules (applied in order — stop at first match)
**Rule 1: Any BLOCK → Reject.** The blocked item is identified and must be
resolved before proceeding. The author addresses the BLOCK specifically (generic
rewrites are not sufficient). The BLOCK becomes a binding Authority Requirements
Ledger row for repair. After addressing: only the blocking reviewer re-evaluates
(not full restart).
**Rule 2: All APPROVED (0 BLOCKs) with 0–2 CONCERNs → ACCEPTED.** Plan may
proceed; author responds to CONCERNs in writing before Phase 1.
**Rule 3: All APPROVED with 3+ CONCERNs → Targeted cycle.** Author addresses
specific CONCERNs; affected reviewers re-evaluate targeted sections only.
**Rule 4: Same BLOCK persists after 2 address cycles → Escalate to Luba.** This
is a genuine architectural disagreement; escalate with full context.
**Rule 5: All outputs blocked (no path forward) → Redesign required.** Produce an
escalation report: what specifically blocks each path; redesign before a new cycle.

### Maximum 3 cycles
After 3 cycles without full approval: escalate. Three cycles of BLOCK without
resolution = the problem is architectural, not editorial.

---

## Section 5 — Review output format
```markdown
## Multi-Reviewer Design Review (multi-reviewer-design-SKILL)

Design: [endpoint / interface / capability name]
Reviewers used: [list roles]

Round [N] results:
| Reviewer | Verdict | BLOCK count | CONCERN count |
|---|---|---|---|
| Domain | APPROVED | 0 | 1 |
| Security | CONCERN | 0 | 2 |
| Principles | APPROVED | 0 | 0 |

Overall verdict: APPROVED_WITH_CONCERNS
Concerns to address: [list]
Next step: author responds to concerns → plan proceeds to Phase 1
```

---

## Section 6 — Escalation report format
```markdown
## Design Review Escalation

Capability: [name]
Cycle count: [N]
Still blocked by: [role] — [exact BLOCK text]

Address attempts:
  Attempt 1: [what was changed]
  Reviewer response: [still blocked — specific remaining issue]
  Attempt 2: [what was changed]
  Reviewer response: [still blocked — specific remaining issue]

Root cause assessment: [architectural disagreement / specification ambiguity / missing decision]
Question for Luba: [one specific decision needed to unblock]
```

---

## Anti-patterns
1. **"One thorough review is enough"** — a single reviewer's blind spots are invisible to them.
2. **"Show all reviewers each other's verdicts before they finish"** — anchoring eliminates the independence that makes the panel valuable.
3. **"Average the BLOCKs against the APPROVEDs"** — a BLOCK means a genuine problem; averaging hides it.
4. **"Skip the CONCERNs because the plan is proceeding"** — unaddressed CONCERNs become implicit tech debt; the author responds in writing.

## Integration
- `plan-review-SKILL.md` is the detailed FC gate list; this skill provides the multi-perspective structure.
- `authority-requirement-binding-SKILL.md` records accepted BLOCK/CONCERN as a `reviewer_gate` ledger row.
- `decision-reopening-SKILL.md` handles BLOCK items that conflict with locked decisions.
- `architect-behavior-classifier-SKILL.md` FAIL-5 (claims without evidence) applies to every reviewer verdict.
