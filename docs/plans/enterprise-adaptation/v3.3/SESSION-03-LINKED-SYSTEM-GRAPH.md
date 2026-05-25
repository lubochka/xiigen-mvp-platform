# SESSION-03-LINKED-SYSTEM-GRAPH.md

## Session type

ARCHITECTURE / SOURCE GRAPH

## Parent plan

`XIIGEN-ENTERPRISE-ADAPTATION-MASTER-PLAN-v3.2.md`

## Phase

3 — Linked system graph

## Goal

Create the linked business-system graph for Angular routes/components/services, .NET controllers/services/contracts and SQL stored procedures.

## Binary acceptance criterion

LGS-01 through LGS-10 have exact graph node/edge/evidence/abstention requirements and no source category is generic or omitted.

## Inputs

Enterprise source layout; Angular files; .NET files; SQL stored procedures; flow manifests.

## Outputs

LinkedSystemGraph contract; source-to-graph mapping; advanced wiki question checklist.

## Actions

1. Define source nodes: AngularComponent, AngularRoute, AngularService, DotNetController, DotNetService, Contract, StoredProcedure, Table, View, Job, FlowManifest.
2. Define edge types: calls, renders, usesDto, readsTable, writesTable, invokesProcedure, emitsEvent, guardedByPolicy.
3. Define graph build order and authority source priority.
4. Define missing-evidence abstention behavior.
5. Define advanced questions for entity impact, flow trace, SP impact, Angular impact, server impact.

## Allowed verification

```powershell
rg -n "CREATE PROCEDURE|CREATE PROC|stored procedure|DbContext|Controller|CanActivate|Route|HttpClient" .
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

LGS rows updated with source categories, evidence paths or new-development-required remediation.

## State update

Update graph status and next_action to SESSION-04.

## Matrix update

LGS-01 through LGS-10

## Stop condition

This session is not a final stop. Commit/push, report writing, child gate success, or evidence creation are checkpoints only. If the parent completion matrix still has required UNKNOWN/FAIL/IN_PROGRESS rows, continue to the next session from state.next_action.
