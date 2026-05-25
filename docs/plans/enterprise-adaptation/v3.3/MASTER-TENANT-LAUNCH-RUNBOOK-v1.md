# MASTER-TENANT-LAUNCH-RUNBOOK-v1

**Status:** binding startup runbook for XIIGen Enterprise adaptation package v3.3.  
**Audience:** executor, architect, reviewer, any model role.  
**Scope:** launch this plan from the main Enterprise project for the master tenant.

---

## 1. Non-negotiable launch rule

This plan is launched from the **main XIIGen Enterprise project** under the **master tenant**.

Executor must not start from:

- `xiigen-mvp`;
- a tenant fork such as `acme-corp--...`;
- an exported corporate install package;
- a plugin-only repository;
- a validation project;
- a scratch folder;
- a local temp copy outside the repo.

Customer tenant repositories and validation projects are targets. They are not the control plane for this launch.

---

## 2. Canonical target repos

The executor must first identify the correct Enterprise root from the current working directory and repo remotes.

Allowed product-line repos:

```text
github.com/lubochka/xiigen-platform-dot-net-angular
github.com/lubochka/xiigen-flows-dot-net-angular
```

If the current repo is not one of these or a documented Enterprise monorepo containing these modules, executor must stop execution actions and hand off to Architect with `UNKNOWN_REPO_SCOPE`, not claim blocker if repo evidence exists elsewhere.

---

## 3. Canonical package location

Copy the full package into:

```text
docs/plans/enterprise-adaptation/v3.3/
```

Required files:

```text
XIIGEN-ENTERPRISE-ADAPTATION-BEACON-PROMPT-v1.md
XIIGEN-ENTERPRISE-ADAPTATION-MASTER-PLAN-v3.3.md
XIIGEN-ENTERPRISE-ADAPTATION-STATE-v3.3.json
XIIGEN-ENTERPRISE-ADAPTATION-COMPLETION-MATRIX-v3.3.md
MASTER-TENANT-LAUNCH-RUNBOOK-v1.md
LEGAL-DOCUMENTS-SITE-INTEGRATION-CONTRACT.md
SESSION-00-GOVERNANCE-ACCEPTANCE.md
SESSION-01-FLOW19-AWS-CICD.md
SESSION-02-AWS-RUNTIME-VISUAL-EVIDENCE.md
SESSION-03-LINKED-SYSTEM-GRAPH.md
SESSION-04-TRAINABLE-UNIT-REGISTRY.md
SESSION-05-TENANT-ADAPTATION-STUDIO.md
SESSION-06-SMART-WIKI.md
SESSION-07-SMART-CODE-ASSISTANT.md
SESSION-08-PROMPT-BY-PROMPT-VALIDATION.md
SESSION-09-PLUGIN-RUNTIME-EVIDENCE.md
SESSION-10-EXPORT-PACKAGE.md
SESSION-11-CORPORATE-INSTALL-IMPORT.md
SESSION-12-UPDATE-REEXPORT-LIFECYCLE.md
SESSION-13-LEGAL-DOCUMENTS-SITE-INTEGRATION.md
```

No phase starts from a partial package.

---

## 4. Startup sequence

Executor must perform exactly this sequence:

1. Read `XIIGEN-ENTERPRISE-ADAPTATION-BEACON-PROMPT-v1.md`.
2. Read `PROTOCOL-ARCHITECT-PLAN-SKILL-AGENT-ROLE-REVIEW-v1.md` if present in repo.
3. Read `AGENTS.md`.
4. Read `CLAUDE.md`.
5. Read this runbook.
6. Read active state JSON.
7. Read completion matrix.
8. Read `SESSION-00-GOVERNANCE-ACCEPTANCE.md`.
9. Record current branch without switching.
10. Verify master-tenant launch scope.
11. Verify or patch governance references.
12. Validate state JSON.
13. Update state and matrix.
14. Handoff to `REVIEW_INTERNAL`.

---

## 5. Allowed startup commands

```powershell
git branch --show-current
git status --short
git diff --check
rg -n "XIIGEN-ENTERPRISE-ADAPTATION-BEACON|MASTER-TENANT-LAUNCH-RUNBOOK|LEGAL-DOCUMENTS-SITE-INTEGRATION|role loop|stop_allowed|EXECUTOR_INTERNAL_CONTINUE" AGENTS.md CLAUDE.md docs/HOW-TO-USE-SKILLS.md docs/plans
Get-Content -Raw docs/plans/enterprise-adaptation/v3.3/XIIGEN-ENTERPRISE-ADAPTATION-STATE-v3.3.json | ConvertFrom-Json | Out-Null
```

---

## 6. Forbidden startup commands unless Gate B is opened

```powershell
dotnet test
dotnet build
npm test
npm run build
npx playwright test
aws cloudformation deploy
aws ecs update-service
```

Startup is governance acceptance only. It is not implementation, test, deploy, or visual evidence capture.

---

## 7. Master tenant launch evidence artifact

Create/update:

```text
docs/plans/enterprise-adaptation/v3.3/evidence/master-tenant-launch-evidence.json
```

Required schema:

```json
{
  "repoRoot": "",
  "repoRemote": "",
  "currentBranch": "",
  "packagePath": "docs/plans/enterprise-adaptation/v3.3/",
  "masterTenantScopeConfirmed": false,
  "masterTenantEvidence": "",
  "agentsReference": "PASS|FAIL|UNKNOWN",
  "claudeReference": "PASS|FAIL|UNKNOWN",
  "skillsGuideReference": "PASS|FAIL|UNKNOWN",
  "stateJsonParses": false,
  "matrixUpdatedRows": [],
  "executorHandoffToReviewRequired": true,
  "timestampUtc": ""
}
```

Do not include secrets, tokens, raw tenant private config, AWS keys, JWTs, or customer data.

---

## 8. Handoff law

After startup slice, executor must hand off to `REVIEW_INTERNAL`.

Executor may not continue to SESSION-01 until Architect records:

```json
{
  "architect_verdict": "ARCHITECT_VERDICT_CONTINUE_EXECUTION",
  "executor_authority_reinstated": true,
  "executor_authority_scope": "SESSION-01-FLOW19-AWS-CICD only"
}
```

Commit/push may be required as checkpoint, but it is not a stop gate and not a final report permission.
