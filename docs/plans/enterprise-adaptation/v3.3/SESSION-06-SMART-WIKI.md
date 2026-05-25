# SESSION-06-SMART-WIKI.md

## Session type

PRODUCT / SMART WIKI PLAN

## Parent plan

`XIIGEN-ENTERPRISE-ADAPTATION-MASTER-PLAN-v3.2.md`

## Phase

6 — Smart Wiki

## Goal

Plan project-local Smart Wiki that answers advanced questions from linked Angular + .NET + SQL graph.

## Binary acceptance criterion

SW-01 through SW-10 define linked indexing, citations, advanced questions, abstention, project registry storage and update path.

## Inputs

Linked graph contract; wiki requirements; trainable unit registry.

## Outputs

Smart Wiki data contract; answer evidence format; advanced question suite.

## Actions

1. Define wiki index inputs from linked graph.
2. Define citation/source authority rules.
3. Define advanced question categories.
4. Define abstention when evidence missing.
5. Define project registry storage.
6. Define reindex/update path.

## Allowed verification

```powershell
rg -n "wiki|RAG|index|citation|source|abstain|impact" .
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

SW rows updated with evidence expectations.

## State update

Update smart_wiki status and next_action to SESSION-07.

## Matrix update

SW-01 through SW-10

## Stop condition

This session is not a final stop. Commit/push, report writing, child gate success, or evidence creation are checkpoints only. If the parent completion matrix still has required UNKNOWN/FAIL/IN_PROGRESS rows, continue to the next session from state.next_action.
