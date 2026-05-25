# SESSION-08-PROMPT-BY-PROMPT-VALIDATION.md

## Session type

VALIDATION PLAN

## Parent plan

`XIIGEN-ENTERPRISE-ADAPTATION-MASTER-PLAN-v3.2.md`

## Phase

8 — Prompt-by-prompt validation

## Goal

Plan validation of every prompt item from prompt suite, not only prompt classes.

## Binary acceptance criterion

PROMPT-01 through PROMPT-10 and generated per-prompt rows define promptId, expected unit routing, fanout, arbiter, reprocessing, authority, actual result and metrics.

## Inputs

orchestrator prompts.txt; PROMPT-SUITE-VALIDATION-CONTRACT.md; unit registry.

## Outputs

Prompt matrix schema; expected output row format; validation evidence folder.

## Actions

1. Locate prompt suite.
2. Create one generated row per prompt item.
3. Define expected unit A-to-B, fanout, arbiter, reprocessing, authority source.
4. Define actual result and metrics fields.
5. Mark missing metrics UNKNOWN, not PASS.

## Allowed verification

```powershell
rg -n "orchestrator prompts|prompt suite|fanout|arbiter|reprocess|authority" .
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

No class-level-only validation can pass.

## State update

Update promptSuiteEvidence status and next_action to SESSION-09.

## Matrix update

PROMPT-01 through PROMPT-10 plus generated prompt rows

## Stop condition

This session is not a final stop. Commit/push, report writing, child gate success, or evidence creation are checkpoints only. If the parent completion matrix still has required UNKNOWN/FAIL/IN_PROGRESS rows, continue to the next session from state.next_action.
