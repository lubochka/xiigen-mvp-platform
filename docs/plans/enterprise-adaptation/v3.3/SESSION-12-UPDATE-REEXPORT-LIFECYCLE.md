# SESSION-12-UPDATE-REEXPORT-LIFECYCLE.md

## Session type

LIFECYCLE / UPDATE PLAN

## Parent plan

`XIIGEN-ENTERPRISE-ADAPTATION-MASTER-PLAN-v3.2.md`

## Phase

12 — Update/re-export lifecycle

## Goal

Plan how tenant solution, wiki, units and package are updated, revalidated, re-exported and rolled back.

## Binary acceptance criterion

UPDATE-01 through UPDATE-12 define graph change detection, wiki/unit updates, prompt/plugin regression, signing, AWS redeploy/visual evidence and rollback audit.

## Inputs

Previous package; current project graph; unit registry; prompt matrix; plugin evidence.

## Outputs

Update lifecycle runbook; regression matrix; re-export evidence contract.

## Actions

1. Detect changes in Angular/.NET/SQL graph.
2. Determine affected wiki entries and trainable units.
3. Retrain/update units if required.
4. Run prompt regression.
5. Run plugin regression.
6. Re-export signed package.
7. Deploy via FLOW-19 if applicable.
8. Capture AWS endpoint visual evidence.
9. Record rollback path.

## Allowed verification

```powershell
rg -n "migration|version|upgrade|rollback|re-export|regression|checkpoint|manifest" .
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

Update is not complete without regression evidence and new package provenance.

## State update

Update lifecycle status and parent next_action from matrix first failed row.

## Matrix update

UPDATE-01 through UPDATE-12

## Stop condition

This session is not a final stop. Commit/push, report writing, child gate success, or evidence creation are checkpoints only. If the parent completion matrix still has required UNKNOWN/FAIL/IN_PROGRESS rows, continue to the next session from state.next_action.
