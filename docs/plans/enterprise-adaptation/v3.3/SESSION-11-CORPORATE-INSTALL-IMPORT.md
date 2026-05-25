# SESSION-11-CORPORATE-INSTALL-IMPORT.md

## Session type

INSTALL / IMPORT PLAN

## Parent plan

`XIIGEN-ENTERPRISE-ADAPTATION-MASTER-PLAN-v3.2.md`

## Phase

11 — Corporate install/import

## Goal

Plan corporate installation and import validation of exported package and trained unit.

## Binary acceptance criterion

INSTALL-01 through INSTALL-12 define manifest/signature/hash verification, unit locator import, health, plugin locator update, no-secrets proof, rollback and evidence storage.

## Inputs

CORPORATE-INSTALL-EVIDENCE-CONTRACT.md; export package; target corporate environment assumptions.

## Outputs

Install runbook; import validator; health checks; visual evidence requirements.

## Actions

1. Define install.ps1/install.sh behavior.
2. Define import manifest/signature/hash verification.
3. Define unit locator import.
4. Define health checks.
5. Define plugin locator update.
6. Define corporate environment evidence.
7. Define rollback if install fails.

## Allowed verification

```powershell
rg -n "install.ps1|install.sh|import|verify|signature|health|rollback|locator" .
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

Installation cannot pass if unit import is missing or plugin locator cannot find unit.

## State update

Update install status and next_action to SESSION-12.

## Matrix update

INSTALL-01 through INSTALL-12

## Stop condition

This session is not a final stop. Commit/push, report writing, child gate success, or evidence creation are checkpoints only. If the parent completion matrix still has required UNKNOWN/FAIL/IN_PROGRESS rows, continue to the next session from state.next_action.
