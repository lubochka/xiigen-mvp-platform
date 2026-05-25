# SESSION-09-PLUGIN-RUNTIME-EVIDENCE.md

## Session type

RUNTIME / PLUGIN EVIDENCE PLAN

## Parent plan

`XIIGEN-ENTERPRISE-ADAPTATION-MASTER-PLAN-v3.2.md`

## Phase

9 — Plugin runtime evidence

## Goal

Plan real Visual Studio and VS Code user-facing host invocation evidence.

## Binary acceptance criterion

PLUGIN-VS-01..06 and PLUGIN-VSCODE-01..06 define real host command/tool-window/panel/chat scenarios; PLUGIN-HARNESS-01 prevents contract harness from closing interactive rows.

## Inputs

Plugin repos; PLUGIN-RUNTIME-EVIDENCE-CONTRACT.md; AWS evidence; prompt matrix.

## Outputs

VS scenario map; VS Code scenario map; contract harness distinction; screenshot/video/evidence map.

## Actions

1. Define Visual Studio command/tool-window scenarios.
2. Define VS Code command/panel/chat scenarios.
3. Require evidence that host invoked entrypoint.
4. Reject direct helper invocation.
5. Map every prompt scenario to host evidence or UNKNOWN.

## Allowed verification

```powershell
rg -n "VSIX|VisualStudio|ToolWindow|ICommand|vscode.commands|WebviewPanel|chat|ExtensionContext" .
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

Contract harness may pass only contract row; interactive plugin rows stay UNKNOWN/FAIL until real host proof.

## State update

Update interactiveIdeEvidence and next_action to SESSION-10.

## Matrix update

PLUGIN-VS-01..06, PLUGIN-VSCODE-01..06, PLUGIN-HARNESS-01

## Stop condition

This session is not a final stop. Commit/push, report writing, child gate success, or evidence creation are checkpoints only. If the parent completion matrix still has required UNKNOWN/FAIL/IN_PROGRESS rows, continue to the next session from state.next_action.
