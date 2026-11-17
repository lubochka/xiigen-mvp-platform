# XIIGEN-ENTERPRISE-ADAPTATION-BEACON-PROMPT-v1.md

**Status:** binding control beacon for all roles and models.\
**Applies to:** XIIGen Enterprise adaptation package v3.2 and every child session.  
**Purpose:** keep every model, role, RAG lookup, executor slice, review, repair and report aligned with one controlling objective and one source-of-truth order.

---

## 0. Non-negotiable instruction

Every role must read and obey this beacon before acting:

```text
BEACON FIRST.
STATE SECOND.
MATRIX THIRD.
ACTIVE SESSION FOURTH.
ONLY THEN EXECUTE OR REVIEW.
```

No role may replace this beacon with memory, habit, a skill shortcut, previous plan version, local convenience, or a partial report.

---

## 1. Main focus goal

XIIGen must be able to create, train, export, import, validate, update and use the full set of trainable units for project-local wiki and tenant-local Smart Code Assistant over one linked business system:

```text
Angular client
+ ASP.NET Core / .NET server
+ SQL stored procedures
+ flow manifests
+ tenant policies
+ project-local wiki
+ trainable assistant units
+ Visual Studio plugin user-facing entrypoint
+ VS Code plugin user-facing entrypoint
+ prompt-by-prompt validation suite
+ FLOW-19 AWS CI/CD deployment and runtime evidence
```

Any action that proves a smaller goal is partial evidence only. It cannot close the parent goal.

---

## 2. Product-line boundary

This is **XIIGen Enterprise**.

Allowed target repos:

```text
github.com/lubochka/xiigen-platform-dot-net-angular
github.com/lubochka/xiigen-flows-dot-net-angular
```

Forbidden target for this plan:

```text
xiigen-mvp
NestJS/React Lite implementation as execution source
```

Shared product specs may be read from either product line only as functional/business source. Implementation target stays Enterprise.

---

## 3. Role control law

The executor never owns final control.

Mandatory role loop:

```text
PLAN_AUTHOR_INTERNAL
  -> PLAN_REVIEW_INTERNAL
  -> ARCHITECT_INTERNAL
  -> EXECUTOR_INTERNAL
  -> REVIEW_INTERNAL
  -> ARCHITECT_INTERNAL_REPAIR
  -> EXECUTOR_INTERNAL_CONTINUE
```

After every executor slice, even after commit/push, the executor must hand control to `REVIEW_INTERNAL` and then to `ARCHITECT_INTERNAL` if any gap is found.

The executor may not decide that work is complete. Only architect review may allow a final report, and only when the stop permission gate passes.

---

## 4. Executor handoff rule

Executor must stop executing and transfer control to Architect when any of these happens:

1. A slice finishes.
2. A test/build/check finishes.
3. A report is written.
4. A commit or push is made.
5. Any matrix row changes.
6. Any evidence artifact is created.
7. Any runtime visual evidence is captured.
8. Any plugin scenario is claimed.
9. Any prompt-suite coverage is claimed.
10. Any failure, unknown, contradiction, or missing evidence appears.

Executor output after a slice must be shaped as:

```text
EXECUTOR_SLICE_RESULT:
  changed_files:
  commands_run:
  evidence_created:
  matrix_rows_touched:
  state_updates_required:
  unresolved_unknowns:
  handoff_to: REVIEW_INTERNAL
```

Executor may not end with:

```text
done
gотово
complete
can continue
waiting for approval
next step is
blocked
```

unless an Architect verdict immediately follows and validates whether this is a real blocker or a continuation point.

The literal `gотово` above is a mixed-script spelling of Russian "ready/done".

---

## 5. Stop permission gate

Final answer is forbidden unless all are true:

```text
goal_reached == true
completion matrix has zero failed/unknown required rows
state.stop_allowed == true
git boundary is satisfied or explicitly impossible
architect_verdict == ARCHITECT_VERDICT_ALLOW_FINAL
```

These are never stop gates:

```text
report written
review done
architecture repair done
commit done
push done
one child session passed
AWS endpoint produced
screenshots captured
plugin contract harness passed
all prompts classified by category
context/session got long
```

If a next action exists in plan/state/matrix/report, blocker is false.

---

## 6. RAG lookup order — where to look before asking Luba

### 6.1 Current instruction and governance

1. Latest direct Luba instruction in current conversation.
2. `XIIGEN-ENTERPRISE-ADAPTATION-BEACON-PROMPT-v1.md`.
3. `PROTOCOL-ARCHITECT-PLAN-SKILL-AGENT-ROLE-REVIEW-v1.md`.
4. `CONSTITUTION-EXECUTOR-ARCHITECT-ROUND-CONTROL-v1.md` if present.
5. `AGENTS.md`.
6. `CLAUDE.md`.
7. `docs/HOW-TO-USE-SKILLS.md`.
8. Active master plan.
9. Active state JSON.
5A. `MASTER-TENANT-LAUNCH-RUNBOOK-v1.md` when starting from the main Enterprise project / master tenant.
5B. `LEGAL-DOCUMENTS-SITE-INTEGRATION-CONTRACT.md` when touching public site, affiliate, checkout, export, install, privacy, cookie or legal pages.
10. Active completion matrix.
11. Active session file.
12. Evidence contracts used by that session.

If a lower source conflicts with a higher source, patch the lower source.

### 6.2 Product-line and architecture sources

Use these to decide **what** must be implemented:

```text
XIIGEN_PRODUCT_SPECS.md
docs/business-flows/{slug}.md
docs/flow-coverage/{slug}/P1-business-logic-inventory.md
docs/flow-coverage/{slug}/P3-automation-gap-analysis.md
docs/flow-coverage/{slug}/P5-ui-specs.md
docs/flow-coverage/{slug}/P9-edge-cases.md
docs/flow-coverage/{slug}/P10-server-specs.md
docs/flow-coverage/{slug}/P11-impl-readiness.md
```

Use Enterprise code repos to decide **how** it is implemented.

### 6.3 Flow lookup map

| Question | Look here first |
|---|---|
| Tenant adaptation lifecycle | master plan, SESSION-05, FLOW-30 tenant lifecycle, FLOW-15 SaaS multi-tenancy |
| AWS deployment / Amazon endpoint | SESSION-01, AWS-CICD-EVIDENCE-CONTRACT, FLOW-19 CI/CD capability |
| Real AWS visual proof | SESSION-02, REAL-VISUAL-EVIDENCE-PROTOCOL-ENTERPRISE |
| Angular + .NET + SQL business graph | SESSION-03, linked system graph artifacts, P5/P10 docs |
| Trainable unit manifest / checkpoints | SESSION-04, TRAINABLE-UNIT-MANIFEST-CONTRACT |
| Smart Wiki | SESSION-06, 42.ProjectWikiDomain, 43.LearningLocalWikiData |
| Smart Code Assistant | SESSION-07, 25.ModelServer, 15.OrchestratorModel, tenant policies |
| Prompt suite validation | SESSION-08, PROMPT-SUITE-VALIDATION-CONTRACT, `orchestrator prompts.txt` |
| Plugin runtime proof | SESSION-09, PLUGIN-RUNTIME-EVIDENCE-CONTRACT, VS/VS Code plugin repos |
| Export package | SESSION-10, EXPORT-PACKAGE-CONTRACT |
| Corporate install/import | SESSION-11, CORPORATE-INSTALL-EVIDENCE-CONTRACT |
| Update/re-export | SESSION-12, upgrade path manifests |
| Auth/tenant isolation | Enterprise auth registry, role guards, route policies, P10/P11, FLOW-01/FLOW-15/FLOW-30 |
| Mobility/cascade/package signing | FLOW-MOBILITY-AUTH-WORK-LEDGER.md, per-flow certification evidence |

### 6.4 Evidence lookup map

| Claim | Required evidence |
|---|---|
| AWS deployment works | FLOW-19 run/workflow, hostedBaseUrl, region, DNS/IP, health result, image digest/package hash |
| UI works on AWS | PNG screenshot/trace/video from AWS URL, timestamp UTC, tenant/role context |
| Plugin orchestration works | real Visual Studio command/tool-window and real VS Code command/panel/chat invocation |
| Prompt suite covered | one row per prompt item, expected output, actual output, authority source, arbiter path |
| Model/unit is project asset | manifest, locator, checkpoint reference, import path, export path, project registry row |
| No plugin embedded weights | plugin package inspection evidence and manifest reference proof |
| Export package installable | signed package, manifest, install script, import validation, no-secrets proof |
| Tenant isolation works | 200/401/403 matrix, cross-tenant denial, role policy evidence |

---

## 7. Evidence firewall

The following may never prove runtime completion by themselves:

```text
build passed
readiness report
contract harness
internal helper test
mock endpoint
localhost
Angular dev server
class-level prompt category pass
static screenshot without tested URL/IP
a report that says passed but has no artifact paths
```

Use values:

```text
PASS
FAIL
UNKNOWN
NOT_APPLICABLE
IN_PROGRESS_NEXT_STEP_KNOWN
```

If CPU/RAM/time/accuracy/reward/loss/held-out metric was not emitted by runner and persisted as artifact, write `UNKNOWN`.

---

## 8. AWS runtime beacon

FLOW-19 is the owner of Amazon deployment capability. AWS is not a manual appendix.

Executor must capture the endpoint produced by FLOW-19:

```text
hostedBaseUrl
serverHealthUrl
clientUrl
Route53 URL if any
CloudFront domain if any
ALB DNS if any
public IPv4 / DNS-resolved IPs at test time
AWS region
environment name
workflow/run URL
commit SHA
image digest or package hash
```

All visual tests must use that endpoint, not localhost.

---

## 9. Plugin runtime beacon

Two evidence levels are separate:

```text
contract_harness
interactive_ide_plugin_scenario
```

`contract_harness` is not enough.

Required plugin proof:

```text
Visual Studio Experimental Instance or equivalent user-facing host path
  -> command/tool-window/view-model invocation
  -> prompt visible to plugin entrypoint
  -> result returned through plugin UI path
  -> evidence artifact path

VS Code Extension Host or equivalent user-facing host path
  -> command/panel/chat invocation
  -> prompt visible to plugin entrypoint
  -> result returned through plugin UI path
  -> evidence artifact path
```

---

## 10. Prompt suite beacon

Every prompt item must have its own row.

Each row must include:

```text
prompt_id
prompt_text_or_pointer
expected unit A-to-B structure
expected fanout
expected arbiter path
expected reprocessing path
expected authority source
actual output artifact
PASS/FAIL/UNKNOWN
```

Prompt class/category coverage is not enough.

---

## 11. Architect review response format

Every coverage challenge from Luba must begin with Architect verdict:

```text
ARCHITECT VERDICT:
  claimed_scope:
  proven_scope:
  missing_evidence:
  reopened_holes:
  score_delta:
  executor_authority:
  next_action:
```

Only after this may executor metrics be shown.

Coverage challenge examples (original Russian phrases retained with English glosses):

```text
все ли покрыто? — is everything covered?
какие сценарии проверены? — which scenarios were checked?
план полностью выполнен? — has the plan been fully completed?
почему остановился? — why did you stop?
точность выше порога? — is accuracy above the threshold?
модель реально импортируется/экспортируется? — is the model actually imported/exported?
плагин реально оркестрирует? — does the plugin actually orchestrate?
```

---

## 12. State synchronization rule

The following must always agree:

```text
state.next_action
state.last_plan_reminder.next_action
state.execution_attempt_latest.next_action
completion_matrix.first_failed_row.next_plan_action
latest report.next_action
```

If they disagree, state is stale and completion claim is forbidden.

Commit/push cannot remain next action after a deeper first failed row exists.

---

## 13. Authority restoration rule

If executor authority was revoked, executor cannot self-restore.

Required architect restoration record:

```json
{
  "architect_verdict": "ARCHITECT_VERDICT_CONTINUE_EXECUTION",
  "executor_authority_reinstated": true,
  "executor_authority_scope": "exact next session/action only",
  "reinstatement_reason": "governing loophole patched and next action derived from plan"
}
```

Without this, executor may not run shell/edit/commit/push/final report.

---

## 14. No-secrets rule

Evidence, screenshots, packages, logs and reports must never include:

```text
raw Vault values
GitHub tokens
Docker registry tokens
JWT signing keys
AWS secret values
tenant secret payloads
private license HMAC secrets
```

Use redacted identifiers and proof-of-presence metadata only.

---

## 15. Minimal first action after installing this beacon

```text
1. Copy BEACON + master plan + state + matrix + sessions + contracts into target repo.
2. Record current branch without switching.
3. Patch AGENTS.md, CLAUDE.md and docs/HOW-TO-USE-SKILLS.md to reference the beacon.
4. Validate state JSON.
5. Update matrix rows for beacon presence and governance linkage.
6. Hand control to REVIEW_INTERNAL.
7. Architect decides whether executor authority is restored for SESSION-01.
```

No broad build/tests in this beacon installation step unless Gate B is explicitly opened.
