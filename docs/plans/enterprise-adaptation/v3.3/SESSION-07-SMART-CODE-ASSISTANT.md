# SESSION-07-SMART-CODE-ASSISTANT.md

## Session type

PRODUCT / TRAINABLE ASSISTANT PLAN

## Parent plan

`XIIGEN-ENTERPRISE-ADAPTATION-MASTER-PLAN-v3.2.md`

## Phase

7 — Smart Code Assistant

## Goal

Plan tenant Smart Code Assistant unit that adapts code under company policy and can be exported/imported.

## Binary acceptance criterion

SCA-01 through SCA-12 define trainable unit creation, policy use, plan-before-patch, patch validation, export/import, locator, auditing and metrics.

## Inputs

Trainable unit registry; tenant policies; linked graph; export package requirements.

## Outputs

Smart Code Assistant unit contract; patch workflow; validation evidence.

## Actions

1. Define assistant unit manifest.
2. Define policy inputs.
3. Define plan-before-patch workflow.
4. Define diff/patch evidence.
5. Define Angular/.NET/SQL impact validation.
6. Define policy violation escalation.
7. Define export/import locator proof.

## Allowed verification

```powershell
rg -n "assistant|patch|diff|policy|validation|impact|unit" .
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

SCA rows updated; static helper cannot pass.

## State update

Update smart_code_assistant status and next_action to SESSION-08.

## Matrix update

SCA-01 through SCA-12

## Stop condition

This session is not a final stop. Commit/push, report writing, child gate success, or evidence creation are checkpoints only. If the parent completion matrix still has required UNKNOWN/FAIL/IN_PROGRESS rows, continue to the next session from state.next_action.
