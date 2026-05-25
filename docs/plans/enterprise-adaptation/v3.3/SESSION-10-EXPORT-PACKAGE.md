# SESSION-10-EXPORT-PACKAGE.md

## Session type

PACKAGE / RELEASE PLAN

## Parent plan

`XIIGEN-ENTERPRISE-ADAPTATION-MASTER-PLAN-v3.2.md`

## Phase

10 — Export package

## Goal

Plan signed tenant corporate export package with code, config, units, manifests, evidence, and no secrets.

## Binary acceptance criterion

EXPORT-01 through EXPORT-12 define exact package tree, manifests, signing, hash/provenance, no-secrets scan, authorization and evidence map.

## Inputs

EXPORT-PACKAGE-CONTRACT.md; unit registry; tenant adaptation output; FLOW-19 package signing capability.

## Outputs

Export package schema; signing evidence; no-secrets evidence; package matrix.

## Actions

1. Define package tree.
2. Define package manifests and provenance.
3. Define signing mechanism through FLOW-19/KMS/Signer if available.
4. Define secret exclusion rules and scan evidence.
5. Define tenant authorization to download package.
6. Define included evidence map.

## Allowed verification

```powershell
rg -n "export|package|manifest|sign|KMS|Signer|secret|license|evidence" .
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

No unsigned package can be release-ready unless signing is explicitly impossible and recorded as gap.

## State update

Update export status and next_action to SESSION-11.

## Matrix update

EXPORT-01 through EXPORT-12

## Stop condition

This session is not a final stop. Commit/push, report writing, child gate success, or evidence creation are checkpoints only. If the parent completion matrix still has required UNKNOWN/FAIL/IN_PROGRESS rows, continue to the next session from state.next_action.
