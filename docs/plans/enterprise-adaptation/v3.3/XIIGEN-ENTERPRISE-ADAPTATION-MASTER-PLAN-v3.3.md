# XIIGEN Enterprise Tenant Adaptation, AWS Runtime Evidence, Export/Import, and Trained Unit Master Plan — v3.3

**Date:** 2026-05-25  
**Session type:** ARCHITECTURE / PLANNING / GOVERNANCE  
**Status:** Protocol-repaired execution package with master-tenant launch runbook and legal-document site integration gates.  
**Target product line:** XIIGen Enterprise  
**Target stack:** ASP.NET Core / .NET 10 backend + Angular 21+ frontend + SQL stored procedures  
**Target repos:**

- `github.com/lubochka/xiigen-platform-dot-net-angular`
- `github.com/lubochka/xiigen-flows-dot-net-angular`

---

## 0. Why v3.3 exists

v3.0 fixed the monolithic-plan problem by creating a package with a master plan, session plans, contracts, state and matrix. The protocol review still found that several completion matrix rows were generic placeholders such as `CI-AWS capability row 1` or `TU requirement 1`.

v3.3 preserves the v3.2 governance repair and adds master-tenant launch/runbook and legal-document integration gates:

1. every completion row is now a literal user/system expectation;
2. `G20` is split into rule-vs-runtime-evidence rows;
3. session acceptance criteria are evidence-ready, not “executor knows” language;
4. FLOW-19 AWS CI/CD capabilities are listed by concrete AWS function;
5. AWS runtime visual evidence rows require real hosted URL/IP, screenshots/traces and metadata;
6. Visual Studio / VS Code runtime rows explicitly require real host invocation, not contract harness;
7. state limits executor authority to `SESSION-00-GOVERNANCE-ACCEPTANCE` only.

7. launch starts from the main Enterprise project under the master tenant, not from a tenant fork or scratch folder;
8. a dedicated runbook defines exactly how executor starts, records branch, installs the package, updates state/matrix and hands off to review;
9. legal documents are first-class site content with versioning, approval, locale, footer/signup/checkout/affiliate/install integration and AWS visual evidence;
10. no legal page or legal acceptance checkbox may be published from draft or unreviewed content.

---

## 0A. Binding beacon prompt

This package is controlled by:

```text
XIIGEN-ENTERPRISE-ADAPTATION-BEACON-PROMPT-v1.md
```

Every role and every model must read the beacon before acting. The beacon defines the common focus goal, RAG lookup order, role handoff law, executor-to-architect transfer rule, evidence firewall, AWS endpoint/IP visual proof rules, and stop-permission gate.

The executor cannot finish by personal judgment. After every slice, report, commit, push, evidence capture, test, AWS endpoint check, plugin scenario or matrix update, executor must hand control to `REVIEW_INTERNAL`; if any gap exists, control moves to `ARCHITECT_INTERNAL_REPAIR`, then back to `EXECUTOR_INTERNAL_CONTINUE`.

Beacon installation is part of `SESSION-00-GOVERNANCE-ACCEPTANCE.md` and must update AGENTS.md, CLAUDE.md and docs/HOW-TO-USE-SKILLS.md references.

## 0B. Master tenant launch rule

This package is launched from the main Enterprise project for the **master tenant**. The executor does not start from a customer tenant repository, scratch folder, validation project, exported package, or plugin repo.

Canonical launch root in the target repo:

```text
/docs/plans/enterprise-adaptation/v3.3/
```

Required package files must be copied there as one governance acceptance slice. The master tenant is the control tenant that owns:

1. the official XIIGen website;
2. Tenant Adaptation Studio;
3. affiliate and legal site content;
4. trainable-unit registry administration;
5. FLOW-19 deployment orchestration;
6. export package generation;
7. legal document publication and acceptance records.

Customer tenants created later are adaptation targets. They do not own the launch plan or governance package.

The concrete startup procedure is defined in:

```text
MASTER-TENANT-LAUNCH-RUNBOOK-v1.md
```

Executor must follow that runbook before opening any later session file.

## 0C. Legal documents are part of the launch scope

Legal documents are not side PDFs and are not optional marketing pages. They are governed website content and must be integrated into the master-tenant site before public launch, affiliate launch, purchase flow, corporate export download, or customer install flow.

Legal content is controlled by:

```text
SESSION-13-LEGAL-DOCUMENTS-SITE-INTEGRATION.md
LEGAL-DOCUMENTS-SITE-INTEGRATION-CONTRACT.md
```

Required first legal set:

1. Terms of Service / Terms of Use.
2. Privacy Policy.
3. Cookie Policy / cookie notice.
4. EULA / License Agreement.
5. Enterprise Terms or Master Services Agreement placeholder.
6. Data Processing Agreement / DPA.
7. Acceptable Use Policy.
8. Affiliate Program Terms.
9. Refund / Cancellation Policy.
10. Security / Responsible Disclosure Policy.
11. Subprocessors / infrastructure disclosure.
12. Open-source notices / third-party notices.

Every legal document requires version, locale, effective date, approval status, canonical URL, previous-version reference, and evidence that the right site flow links to the right legal version.

---

## 1. Main focus goal

XIIGen Enterprise must be able to create, train, export, import, validate, update, and use the full set of trainable units for project-local wiki and tenant-local Smart Code Assistant over a single linked business system:

```text
Angular client
  + ASP.NET Core / .NET server
  + SQL stored procedures
  + flow manifests
  + tenant policies
  + project-local wiki
  + trainable assistant units
```

Visual Studio and VS Code plugins must be able to invoke this orchestration through real user-facing entrypoints on real projects and on every prompt task from the prompt suite.

The official XIIGen site is not only a marketing site. It is also the Tenant Adaptation Studio where a corporate tenant can:

1. create a corporate workspace;
2. connect or upload a project;
3. let XIIGen build a linked Angular + .NET + SQL graph;
4. create project-local wiki knowledge;
5. train/export/import tenant-specific units;
6. adapt the platform to corporate needs;
7. export a signed corporate install package;
8. install the adapted package inside the corporation;
9. use a specially trained XIIGen Smart Code Assistant Unit for that solution;
10. update, revalidate, and re-export the package through governed CI/CD.

---

## 2. Product line decision

This plan is for **XIIGen Enterprise** only.

| Area | Decision |
|---|---|
| Backend | ASP.NET Core / .NET 10 |
| Frontend | Angular 21+ |
| Data | SQL stored procedures are first-class source nodes |
| License | Commercial / corporate deployment |
| Target repos | `xiigen-platform-dot-net-angular`, `xiigen-flows-dot-net-angular` |
| CI/CD owner | FLOW-19 |
| AWS deployment | FLOW-19 capability, not manual appendix |
| Plugins | Visual Studio and VS Code user-facing entrypoints |
| Lite repo | Not target |

Forbidden:

```text
Do not implement this plan in xiigen-mvp.
Do not treat NestJS/React Lite architecture as the Enterprise execution target.
Do not replace Enterprise plugin/runtime proof with Lite source/build/readiness proof.
```

---

## 3. Required governance block

### main_focus_goal

Deliver the full Enterprise tenant adaptation lifecycle, including trainable/importable/exportable units, project-local wiki, Smart Code Assistant unit, AWS-hosted CI/CD verification, real visual evidence from the AWS endpoint/IP, and Visual Studio / VS Code runtime plugin entrypoint evidence.

### branch_rule

Use the current branch. Record it with:

```powershell
git branch --show-current
```

Do not switch branch without Luba's explicit current instruction.

### allowed_verification

During governance phases:

```powershell
git branch --show-current
git status --short
git diff --check
rg -n "Plan governance|role loop|soft stop|stop_allowed|ARCHITECT_INTERNAL_REPAIR|EXECUTOR_INTERNAL_CONTINUE" AGENTS.md CLAUDE.md docs/HOW-TO-USE-SKILLS.md
rg -n "CONSTITUTION-EXECUTOR-ARCHITECT-ROUND-CONTROL|executor_authority|contract_harness_only|interactive_ide_plugin_scenario|positive_points_awarded_for_repair|first_failed_requirement_id" .
Get-Content -Raw <state-or-report>.json | ConvertFrom-Json | Out-Null
```

During implementation gates, only the specific commands named in the relevant session file are allowed.

### forbidden_verification

Do not run broad tests during governance/doc/registry phases unless Gate B is explicitly opened by the plan or by Luba:

```powershell
dotnet test
dotnet build
npm test
npm run build
```

### role_loop

```text
PLAN_AUTHOR_INTERNAL
  -> PLAN_REVIEW_INTERNAL
  -> ARCHITECT_INTERNAL
  -> EXECUTOR_INTERNAL
  -> REVIEW_INTERNAL
  -> ARCHITECT_INTERNAL_REPAIR
  -> EXECUTOR_INTERNAL_CONTINUE
```

Review/repair is not a stop. It returns to executor when next action exists.

### skills_by_lifecycle_stage

| Stage | Skills / protocols |
|---|---|
| Plan creation | architect planning, requirement-to-flow, product variant router |
| Plan review | architect protocol, plan review, no-invention review |
| Execution | executor runbook, flow-specific session file |
| Code review | DNA/fabric/provider-boundary checks |
| Documentation sync | state/matrix/update artifacts |
| Completion verification | completion matrix, evidence-scope review |
| Loophole repair | protocol Section 22, architect repair loop |

### document_rag_lookup_order

1. Luba's current instruction.
2. `AGENTS.md`.
3. `CLAUDE.md`.
4. This master plan.
5. Active state JSON.
6. Completion matrix.
7. `docs/HOW-TO-USE-SKILLS.md`.
8. `MASTER-TENANT-LAUNCH-RUNBOOK-v1.md` for startup procedure.
9. Session file for current phase.
10. Legal integration contract when site/public/affiliate/checkout/install pages are touched.
11. Flow docs / P1-P11 docs.
12. Runtime reports, manifests, plugin evidence, AWS evidence, git status.

### anti_loophole_register

Forbidden completion patterns:

```text
single file done
commit/push done
report written done
build passed therefore runtime passed
contract harness passed therefore plugin passed
localhost screenshot as AWS visual evidence
prompt class passed instead of prompt-by-prompt
static helper as trainable unit
missing metrics reported as PASS
repair awarded positive score
stale next_action left after deeper gap opens
```

### state_update_points

Update state:

```text
before execution
after every executor slice
after review
after repair
after commit/push checkpoint
after AWS deployment evidence
after visual evidence capture
after plugin host evidence
after prompt-suite validation
```

### completion_matrix_rows

Completion matrix rows are in `XIIGEN-ENTERPRISE-ADAPTATION-COMPLETION-MATRIX-v3.2.md`. Each row must name a literal expectation and expected evidence. Generic placeholders such as `requirement 1` are forbidden.

### score_rules

```text
+1 only for real progress without stopping and not for governance repair after a violation.
-100 premature incomplete stop.
-1000 severe violation, false completion, wrong role, or soft stop.
0 for protocol repair artifacts that merely remove previous governance holes.
```

### commit_push_boundary

Commit/push is required when a phase gate requires repository persistence, but commit/push is not a stop gate and cannot be used as completion evidence for runtime/product claims.

### repair_returns_to_executor

After governance repair, derive the next action from state/matrix/first failed row and continue. Do not ask Luba to decide if the safe next action is already present.

---

## 4. Canonical phase map

### SESSION-00A — Master tenant launch runbook

**File:** `MASTER-TENANT-LAUNCH-RUNBOOK-v1.md`

**Purpose:** gives the exact starting procedure for the executor inside the main Enterprise project and master tenant scope.

**Gate:** executor has copied BEACON + plan package to the canonical repo path, recorded current branch, verified master-tenant launch scope, updated state/matrix with launch evidence, and handed off to REVIEW_INTERNAL.

### SESSION-13 — Legal documents site integration

**File:** `SESSION-13-LEGAL-DOCUMENTS-SITE-INTEGRATION.md`

**Purpose:** integrate legal documents into the official website, affiliate flow, purchase flow, export/download flow, and corporate install flow.

**Gate:** legal docs are versioned, approved, localized as required, linked from the correct site surfaces, acceptance records are stored, and AWS visual evidence proves public access to the correct legal pages without exposing secrets.


| Session | File | Purpose | Primary matrix rows |
|---|---|---|---|
| 00 | `SESSION-00-GOVERNANCE-ACCEPTANCE.md` | Accept package into target repo, patch governance references, validate state/matrix | G02/G07/G12/G21-G30 |
| 01 | `SESSION-01-FLOW19-AWS-CICD.md` | Verify FLOW-19 AWS CI/CD capability | CI-AWS-01..12 |
| 02 | `SESSION-02-AWS-RUNTIME-VISUAL-EVIDENCE.md` | Enforce real AWS endpoint/IP and visual evidence | AWS-RUNTIME-01..12 |
| 03 | `SESSION-03-LINKED-SYSTEM-GRAPH.md` | Angular + .NET + SQL linked graph | LGS-01..10 |
| 04 | `SESSION-04-TRAINABLE-UNIT-REGISTRY.md` | Trainable units as import/export assets | TU-01..12 |
| 05 | `SESSION-05-TENANT-ADAPTATION-STUDIO.md` | Website adaptation studio | TAS-01..10 |
| 06 | `SESSION-06-SMART-WIKI.md` | Project-local Smart Wiki | SW-01..10 |
| 07 | `SESSION-07-SMART-CODE-ASSISTANT.md` | Tenant Smart Code Assistant unit | SCA-01..12 |
| 08 | `SESSION-08-PROMPT-BY-PROMPT-VALIDATION.md` | Prompt-by-prompt validation | PROMPT-01..10 plus generated prompt rows |
| 09 | `SESSION-09-PLUGIN-RUNTIME-EVIDENCE.md` | Real VS/VS Code host proof | PLUGIN-VS-01..06, PLUGIN-VSCODE-01..06 |
| 10 | `SESSION-10-EXPORT-PACKAGE.md` | Signed corporate export package | EXPORT-01..12 |
| 11 | `SESSION-11-CORPORATE-INSTALL-IMPORT.md` | Corporate import/install validation | INSTALL-01..12 |
| 12 | `SESSION-12-UPDATE-REEXPORT-LIFECYCLE.md` | Update/re-export/revalidation lifecycle | UPDATE-01..12 |

---

## 5. FLOW-19 AWS rule

AWS deployment is a FLOW-19 capability. The plan must not instruct the executor to manually invent an AWS topology outside the existing CI/CD flow. The executor first locates FLOW-19 implementation, CI/CD pipeline files and AWS deployment evidence. Missing AWS capabilities become explicit `new development required` rows, not hand-waved manual steps.

---

## 6. Real visual evidence rule

Runtime evidence must target the actual AWS-hosted endpoint emitted by FLOW-19.

Invalid evidence:

```text
localhost
Angular dev server
mock endpoint
internal helper
contract harness
build-only report
static screenshot without URL/IP metadata
```

Valid evidence must include:

```text
hostedBaseUrl
serverHealthUrl
clientUrl
resolved public IPs at test time
region/environment
workflow/run URL
commit SHA
image digest or package hash
UTC timestamp
PNG screenshot paths
Playwright trace/video paths where applicable
tenant and role context with secrets redacted
matrix row mapping
```

---

## 7. Execution authority after this package

The executor may only start `SESSION-00-GOVERNANCE-ACCEPTANCE.md`. No production code, AWS deployment, broad build/test command or plugin runtime claim is allowed until the relevant session opens that gate.
