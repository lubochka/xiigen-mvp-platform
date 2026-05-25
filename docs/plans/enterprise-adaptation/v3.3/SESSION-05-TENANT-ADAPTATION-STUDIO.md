# SESSION-05-TENANT-ADAPTATION-STUDIO.md

## Session type

PRODUCT / TENANT UI PLAN

## Parent plan

`XIIGEN-ENTERPRISE-ADAPTATION-MASTER-PLAN-v3.2.md`

## Phase

5 — Tenant Adaptation Studio

## Goal

Plan website Studio where tenant adapts platform, policies and solution before export.

## Binary acceptance criterion

TAS-01 through TAS-10 define tenant workspace, project connection/upload, policy capture, adaptation request, plan-before-patch, reviewable diff, tenant isolation, audit and AWS UI evidence.

## Inputs

Enterprise website requirements; tenant lifecycle; auth/policy requirements.

## Outputs

Studio route/module map; API capability map; evidence requirements.

## Actions

1. Define workspace creation flow.
2. Define repo connection/snapshot upload flow.
3. Define company coding/architecture policy capture.
4. Define adaptation request fields.
5. Define plan-before-patch workflow.
6. Define reviewable diff and approval gate.
7. Define tenant isolation checks.
8. Define AWS visual evidence for UI claims.

## Allowed verification

```powershell
rg -n "tenant|workspace|policy|adaptation|diff|approval|audit|CanActivate|Authorize" .
git diff --check
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

TAS rows updated; no tenant UI runtime claim can pass without AWS visual evidence.

## State update

Update tenant_adaptation_studio status and next_action to SESSION-06.

## Matrix update

TAS-01 through TAS-10

## Stop condition

This session is not a final stop. Commit/push, report writing, child gate success, or evidence creation are checkpoints only. If the parent completion matrix still has required UNKNOWN/FAIL/IN_PROGRESS rows, continue to the next session from state.next_action.
