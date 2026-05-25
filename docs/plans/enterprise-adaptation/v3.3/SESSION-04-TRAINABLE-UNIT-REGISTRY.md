# SESSION-04-TRAINABLE-UNIT-REGISTRY.md

## Session type

ARCHITECTURE / TRAINABLE ASSET REGISTRY

## Parent plan

`XIIGEN-ENTERPRISE-ADAPTATION-MASTER-PLAN-v3.2.md`

## Phase

4 — Trainable unit registry

## Goal

Define project asset registry, manifest, locator, checkpoints, import/export and upgrade paths for every trainable unit.

## Binary acceptance criterion

TU-01 through TU-12 have exact manifest/locator/checkpoint/import/export/validation/plugin-loading requirements and no static helper can satisfy trainable-unit rows.

## Inputs

TRAINABLE-UNIT-MANIFEST-CONTRACT.md; project-local wiki requirements; plugin requirements.

## Outputs

Unit registry schema; manifest contract; import/export proof rows; no embedded weights rule.

## Actions

1. Define registry path inside project assets.
2. Define required unit ids and versions.
3. Define manifest fields and checkpoint references.
4. Define locator format used by runtime and plugins.
5. Define import/export paths and validation reports.
6. Define upgrade/migration rules.
7. Define no-embedded-weights plugin rule.

## Allowed verification

```powershell
rg -n "unit|manifest|checkpoint|locator|model|import|export|registry" .
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

TU rows updated with exact artifact/evidence expectations or new-development-required remediation.

## State update

Update trainableUnitEvidence and next_action to SESSION-05.

## Matrix update

TU-01 through TU-12

## Stop condition

This session is not a final stop. Commit/push, report writing, child gate success, or evidence creation are checkpoints only. If the parent completion matrix still has required UNKNOWN/FAIL/IN_PROGRESS rows, continue to the next session from state.next_action.
