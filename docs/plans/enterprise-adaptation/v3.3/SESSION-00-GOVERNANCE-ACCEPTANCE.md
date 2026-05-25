# SESSION-00-GOVERNANCE-ACCEPTANCE.md

## Session type

ARCHITECTURE / GOVERNANCE

## Parent plan

`XIIGEN-ENTERPRISE-ADAPTATION-MASTER-PLAN-v3.2.md`

## Phase

0 — Governance acceptance

## Goal

Accept the v3.2 package and binding beacon into the target Enterprise repository without starting product implementation.

## Binary acceptance criterion

The package and `XIIGEN-ENTERPRISE-ADAPTATION-BEACON-PROMPT-v1.md` are copied into the target repo; current branch is recorded without switching; AGENTS.md/CLAUDE.md/docs/HOW-TO-USE-SKILLS.md are verified or patched to reference the beacon, active protocol and plan; constitution linkage is checked; state JSON parses; G02/G07/G12/G21-G30 and BEACON-01 through BEACON-08 are updated from real repo evidence.

## Inputs

Current Luba instruction; active protocol; master plan v3.2; state v3.2; matrix v3.2; target repo governance files.

## Outputs

Beacon, plan/state/matrix/session files in repo; branch evidence; governance reference evidence; JSON parse evidence; diff-check evidence; review handoff evidence.

## Actions

1. Copy all v3.2 package files into the target repo, including `XIIGEN-ENTERPRISE-ADAPTATION-BEACON-PROMPT-v1.md`.
2. Run `git branch --show-current` and record result in state/matrix evidence.
3. Run governance `rg` checks against AGENTS.md, CLAUDE.md, docs/HOW-TO-USE-SKILLS.md for beacon, protocol, role loop, stop gate and executor handoff references.
4. If governance references are missing, patch those files.
5. Check whether `CONSTITUTION-EXECUTOR-ARCHITECT-ROUND-CONTROL-v1.md` exists; if yes, ensure plan/state/matrix/docs reference it; if no, mark G29 NOT_APPLICABLE with rg evidence.
6. Validate state JSON.
7. Run `git diff --check`.
8. Update matrix G02/G07/G12/G21-G30 and BEACON-01 through BEACON-08.
9. Update state.next_action to SESSION-01 only after REVIEW_INTERNAL confirms beacon installation evidence.
10. Executor hands control to REVIEW_INTERNAL; executor may not proceed to SESSION-01 until Architect reinstates authority for that exact scope.

## Allowed verification

```powershell
git branch --show-current
git status --short
git diff --check
rg -n "Plan governance|role loop|soft stop|stop_allowed|ARCHITECT_INTERNAL_REPAIR|EXECUTOR_INTERNAL_CONTINUE" AGENTS.md CLAUDE.md docs/HOW-TO-USE-SKILLS.md
rg -n "CONSTITUTION-EXECUTOR-ARCHITECT-ROUND-CONTROL|executor_authority|contract_harness_only|interactive_ide_plugin_scenario|positive_points_awarded_for_repair|first_failed_requirement_id" .
Get-Content -Raw XIIGEN-ENTERPRISE-ADAPTATION-STATE-v3.2.json | ConvertFrom-Json | Out-Null
```

## Forbidden verification

Do not run these unless this session explicitly opens Gate B or Luba directly asks:

```powershell
dotnet test
dotnet build
npm test
npm run build
```

Do not switch branch. Do not use localhost as AWS runtime proof. Do not replace real IDE host evidence with contract harness evidence.

## Gate

G02/G07/G12 and G21-G30 have repo evidence, PASS/NOT_APPLICABLE where justified, or remain UNKNOWN with exact remediation. No broad tests were run.

## State update

Update current_internal_role_state, branch, next_action, score audit, governance evidence paths and constitution_linkage.

## Matrix update

G02, G07, G12, G21-G30

## Stop condition

This session is not a final stop. Commit/push, report writing, child gate success, or evidence creation are checkpoints only. If the parent completion matrix still has required UNKNOWN/FAIL/IN_PROGRESS rows, continue to the next session from state.next_action.


## Additional v3.3 master-tenant launch actions

Before any later session starts, executor must:

1. Read `MASTER-TENANT-LAUNCH-RUNBOOK-v1.md`.
2. Confirm the active working directory is the main Enterprise project repo, not a customer tenant repo, validation target, exported package folder, or plugin-only repo.
3. Copy the full package into:

```text
/docs/plans/enterprise-adaptation/v3.3/
```

4. Confirm the launch scope is `master tenant` and record the evidence in state field `masterTenantLaunch.scopeEvidence`.
5. Add or verify references to `MASTER-TENANT-LAUNCH-RUNBOOK-v1.md` and `LEGAL-DOCUMENTS-SITE-INTEGRATION-CONTRACT.md` in AGENTS.md, CLAUDE.md, and docs/HOW-TO-USE-SKILLS.md.
6. Update matrix rows `LAUNCH-01..LAUNCH-14` and `LEGAL-01..LEGAL-18` as `PASS`, `UNKNOWN`, or `NOT_APPLICABLE` based on real repo evidence.
7. Handoff to `REVIEW_INTERNAL`; executor may not continue to SESSION-01 until Architect reinstates executor authority.
