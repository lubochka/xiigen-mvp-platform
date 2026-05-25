# SESSION-02-AWS-RUNTIME-VISUAL-EVIDENCE.md

## Session type

RUNTIME EVIDENCE PROTOCOL

## Parent plan

`XIIGEN-ENTERPRISE-ADAPTATION-MASTER-PLAN-v3.2.md`

## Phase

2 — AWS runtime visual evidence

## Goal

Define and enforce testing against the real AWS URL/IP produced by FLOW-19, with real visual evidence.

## Binary acceptance criterion

AWS-RUNTIME-01 through AWS-RUNTIME-12 have exact artifact fields, evidence paths and invalid-evidence rejection rules; no runtime PASS can be claimed from localhost/mock/dev-server/build-only evidence.

## Inputs

FLOW-19 CI/CD evidence; AWS-CICD-EVIDENCE-CONTRACT.md; REAL-VISUAL-EVIDENCE-PROTOCOL-ENTERPRISE.md.

## Outputs

AWS evidence contract; visual evidence map format; Playwright target requirements; invalid evidence list.

## Actions

1. Require CI/CD to emit hostedBaseUrl, serverHealthUrl, clientUrl, workflowRunUrl, commitSha, imageDigest/packageHash.
2. Require DNS-resolved IP capture at test time.
3. Require Playwright/browser tests to target the AWS hosted endpoint.
4. Require screenshot metadata for every route.
5. Reject localhost/mock/synthetic/build-only evidence.
6. Add mapping from screenshots to matrix rows.

## Allowed verification

```powershell
Get-Content -Raw AWS-CICD-EVIDENCE.json | ConvertFrom-Json | Out-Null
Test-NetConnection <host>
Resolve-DnsName <host>
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

AWS evidence protocol exists and AWS-RUNTIME rows are ready for execution evidence or marked new development required from FLOW-19.

## State update

Update state.awsCicdEvidence required fields and next_action to SESSION-03.

## Matrix update

AWS-RUNTIME-01 through AWS-RUNTIME-12

## Stop condition

This session is not a final stop. Commit/push, report writing, child gate success, or evidence creation are checkpoints only. If the parent completion matrix still has required UNKNOWN/FAIL/IN_PROGRESS rows, continue to the next session from state.next_action.
