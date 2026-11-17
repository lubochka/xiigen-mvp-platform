# Human-Readable Architecture Audit (XIIGen Enterprise Platform)

## When to load

Load this skill for architect, planning, plan review, code review, documentation,
and audit artifacts that explain architecture, the fabric layer, service
contracts, persistence/concurrency behavior, tenant scoping, or code risks. Also
load it when Luba asks for a human-readable architecture explanation,
UML, no-code algorithms, code quotes, or iterative audit readability improvement.

Also load it when an audit claims sufficiency/completion/closure, when "v1 scope
completed" needs bounded wording, or when a later broader audit in the same
architecture area finds BLOCKING/HIGH issues.

Platform port of the `llm_mvp_core` skill. Stack-bound to **.NET 10 ASP.NET Core
Web API**, **xUnit** (`tests/`), `Xiigen.Platform.slnx`, `DataProcessResult<T>`,
and the v3.3 governance triple. This skill reinforces the binding beacon
governance with human readability. The core skill's machine-learning material —
the `ILearningMachine`/`ILogicalOrchestrator`/`ILogicalFlow`/`ILogicalUnit`/
`ILearningMachinePair` levels, DPO triples, KNN (K-Nearest Neighbours), trained
checkpoints/manifests, and the Claude/Cursor/Codex host-extension dual-path
host-verification block — is **`na`** for this repository. The platform has no
local trainable models (it consumes the core wiki-domain manifests) and no host
extension. The deterministic-vs-learned boundary is therefore explicitly **N/A**
on this platform and must be written as `deterministic-vs-learned: N/A (no local
learned component; platform consumes core)` rather than omitted.

## Subagent-Only Firewall Compatibility

Human-readable audit and architecture repairs must respect the parent
architect/sub-agent-only role boundary:

```text
parent local sidecar work ban
read-only verification after a sub-agent packet returns
sub-agent waiting is not a stop
no local non-overlapping work escape
```

When the current instruction is sub-agent-only, architect-only, governance-only,
or forbids the parent/current session from becoming executor, the parent must not
directly author governed audit, plan, protocol, skill, report, data, or code
repairs. Parent review may inspect and integrate only after the sub-agent packet
returns. Waiting for the packet is not a stop gate while a safe bounded next
action exists.

## Human-Readable Architecture Artifact Gate

The artifact must be readable by Luba as a human architect, in English unless the user
explicitly requests another language. Machine labels may exist, but they are
secondary anchors. They cannot be the main explanation.

Hard fail:
- table-only, shortcode-heavy, schema-first, or schema-only text, or text that ignores the requested language;
- prompt/spec text that tells a future executor what to invent;
- code dumps without no-code explanation;
- generic rows such as "wrap in a service", "add tests", "create schema", or
  "make typed state".

## Required Human Format

Every architecture, code-review, documentation, or audit artifact that touches
architecture must include:

1. Clear English summary in human terms unless the user requests another language.
2. A solved architecture block for every major architecture area: problem,
   selected design, rejected alternatives, UML, data flow, current code refs,
   future files/classes/methods/signatures/dependencies,
   `deterministic-vs-learned: N/A (no local learned component on this platform)`,
   gates/tests, failure modes, and docs.
3. UML importable to draw.io: PlantUML or Mermaid `classDiagram`, plus concrete
   data-flow simulations and expected branch outcomes. On this platform the UML
   describes ASP.NET Core Web API layers — controller/endpoint -> service ->
   fabric interface (`ISchedulerService` / `IOutboxStore` / `IIdempotencyStore` /
   `IDocumentStore` / `ITenantContextAccessor`) -> returned/persisted
   `DataProcessResult`.
4. No-code algorithm explanation: what happens, where, in what order, with
   class/function references (`src/**/*.cs`), but not a code dump.
5. Code quote section. For each finding, give exact `file:line` and a short quote,
   then:
   - current behavior;
   - why this is wrong/acceptable;
   - required behavior;
   - affected files/classes/records/methods/data rows;
   - how it works (method/fabric behavior);
   - affected tests/integration/e2e/qa;
   - lifecycle (DI registration, tenant scope, idempotency/outbox/document store interaction).
6. A concrete solution immediately under each problem quote. A later appendix is
   not enough.

The solved architecture block is required even when the artifact is "only an
audit" or "only a review". Findings without adjacent solved design are not enough
to authorize downstream planning or implementation.

## Concrete Repair Blueprint Gate (platform fabric/service form)

The core skill's blueprint is written for trainable units (typed trained state,
DPO examples, KNN signatures, checkpoints). That form is **`na`** here. On this
platform, for every suspicious service/fabric quote — a method that throws for
business logic instead of returning `DataProcessResult<T>`, a directly
instantiated dependency that should be resolved through DI, a tenant-scope leak, a
non-idempotent write that should reserve through `IIdempotencyStore`, or a
swallowed failure path — write one concrete per-method blueprint:

```text
current method/quote (file:line):
current runtime behavior:
why it is wrong (rule / contract / tenant-scope / concurrency):
should become (concrete behavior):
replacement class/record/method and signature:
fabric interface involved (ISchedulerService / IOutboxStore / IIdempotencyStore / IDocumentStore / ITenantContextAccessor) or N/A:
returned DataProcessResult success/failure shape and error code:
DI registration change (Program.cs / XiigenInMemoryFabricServiceCollectionExtensions) if any:
tenant-scope handling:
tests/integration/e2e/qa (xUnit, WebApplicationFactory where relevant):
docs/matrix rows affected:
```

If the row contains only labels or a prompt/spec for later work, it is not a
solution. The review must fail until the blueprint names concrete classes,
records, methods, tests, the fabric boundary, and the `DataProcessResult` shape.

## Audit Sufficiency Boundary Gate

A readable audit may still be too narrow. Do not claim "достаточно" ("sufficient"), "готово" ("ready"),
"закрыто" ("closed"), "architecture audit sufficient", or "architecture audit complete"
unless the audit scope was broad enough and the breadth challenge pass is green.

Allowed bounded wording:
- "v1 scope completed" only when the exact checked scope is named;
- "focused audit found X" only when broader architecture sufficiency is not implied.

Minimum breadth challenge for a platform architecture audit — after focused
findings, run a second pass over adjacent runtime authority zones:
- `DataProcessResult` failure-path coverage and error-code consistency;
- tenant-scope isolation across every fabric call;
- idempotency/replay correctness on write paths;
- outbox ordering and at-least-once delivery semantics;
- optimistic-concurrency (`expectedVersion`) handling on `IDocumentStore`;
- direct-instantiation / fabric-bypass shortcuts.

Parent review must ask and answer:

```text
What would a broader audit likely find that this artifact missed?
```

The answer must list likely missed zones, what was checked now, what remains
explicitly out-of-scope, and whether the final wording is bounded.

## Delta-Severity Feedback Gate

If the next audit in the same architecture area finds BLOCKING/HIGH issues that
should have been discoverable, treat the previous "sufficient/ready/closed" review
as a retroactive review-skill failure.

```text
previous audit/review claim:
later BLOCKING/HIGH finding:
why it should have been discoverable:
why the previous review missed it:
skill/guide/index/agent rule patched:
new breadth challenge row:
completion wording corrected:
```

Return to governance repair before polishing the artifact when this gate fails.

## Review/Fix Cycle Evidence Gate

When Luba asks for iterative audit readability improvement with "minimum N", "at
least N", "preferably 10", or equivalent wording, a cycle is a real review/fix
action, not a ledger row.

Each cycle must have an inspectable `cycle_evidence` packet:

```text
cycle_number:
requested_cycle_floor:
reviewer_or_subagent_id:
artifact_before_ref:
artifact_after_ref:
loaded_skills:
loaded_gates:
findings:
fix_refs_or_no_fix_justification:
recheck_evidence:
readability_delta:
parent_review_disposition:
```

The cycle ledger may summarize and link these packets, but it cannot replace them.
`real_cycle_count` counts only valid packets created in order after prior fixes.
N ledger rows after one review is a failed audit.

When Luba explicitly asks for iterative audit readability improvement, the review
floor is minimum 6 cycles, preferably 10. If a cycle finds a missing skill/gate,
stop polishing the artifact and return to governance repair first.

## Human-Readable Plan/Execution Log Gate

Execution-stage artifacts must be readable to Luba as the primary status. Every
phase, review, execution, or completion packet must include `stage`,
`current_plan_step`, `what_changed`, `what_remains`, `claims_opened`,
`claims_closed`, and `why_this_status_is_true`. Machine-only logs, JSON, status
codes, or shortcode-heavy tables are secondary evidence only. The full board
fields live in `output-readability-gate-SKILL.md` Section 6.

## Review Verdict

Reject the artifact when:
- the human summary is missing or does not use the required language (English by default, or the language requested by the user);
- UML has no concrete simulation examples;
- algorithm text is code-first rather than no-code;
- code quotes lack adjacent concrete solutions;
- solved architecture blocks are missing for architecture areas;
- the deterministic-vs-learned boundary is missing instead of explicitly N/A;
- a solution is generic, schema-only, table-only, or prompt/spec;
- a fabric/service repair omits the concrete method, the `DataProcessResult` shape, the fabric boundary, tests, or lifecycle;
- audit completion wording overclaims sufficiency without a green breadth challenge;
- a later broader audit found discoverable BLOCKING/HIGH issues and the previous review did not record a Delta-Severity Feedback Gate repair.

## Integration

Cross-reference with:
- `output-readability-gate-SKILL.md` (prose readability + Execution Board);
- `agent-output-format-SKILL.md` (FC-31/FC-32 reference this gate);
- `implementation-doc-uml-SKILL.md`;
- `implementation-doc-algorithms-SKILL.md`;
- `implementation-doc-code-refs-SKILL.md`;
- `audit-protocol` / `audit-protocol-SKILL.md`.
