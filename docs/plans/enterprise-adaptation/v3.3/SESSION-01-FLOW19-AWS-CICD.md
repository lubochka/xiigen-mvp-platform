# SESSION-01-FLOW19-AWS-CICD.md

## Session type

ARCHITECTURE / CI-CD CAPABILITY REVIEW

## Parent plan

`XIIGEN-ENTERPRISE-ADAPTATION-MASTER-PLAN-v3.2.md`

## Phase

1 — FLOW-19 AWS CI/CD capability

## Goal

Locate and assess Enterprise FLOW-19 AWS CI/CD capability. Amazon deployment must be part of FLOW-19, not a manual appendix.

## Binary acceptance criterion

CI-AWS-01 through CI-AWS-13 have evidence paths or explicit `new development required` remediation; no AWS manual deployment appendix is introduced as substitute for FLOW-19 capability.

## Inputs

Enterprise repos; FLOW-19 docs/code; CI workflows; deployment descriptors; package signing evidence; ledger evidence.

## Outputs

FLOW-19 AWS capability map; missing capability list; matrix updates; next implementation slices.

## Actions

1. Locate FLOW-19 implementation and docs in Enterprise repos.
2. Locate CI pipeline files.
3. Map Angular build, .NET container build, ECR publish, AWS runtime deploy, public routing, S3 artifact storage, Secrets/KMS, signing, CloudWatch/X-Ray, rollback and audit capabilities.
4. For each missing capability, mark `new development required` with target file/module candidate.
5. Do not invent services; derive from repo evidence.

## Allowed verification

```powershell
rg -n "FLOW-19|durable-sagas|compliance|CI/CD|GitHub Actions|ECR|ECS|Fargate|ALB|Route53|CloudFront|S3|Secrets Manager|KMS|CloudWatch|rollback|ComplianceAuditWriter" .
git status --short
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

CI-AWS rows updated with literal evidence paths or remediation; no broad tests were run.

## State update

Update awsCicdEvidence capability map and next_action to SESSION-02 only after evidence map is written.

## Matrix update

CI-AWS-01 through CI-AWS-13

## Stop condition

This session is not a final stop. Commit/push, report writing, child gate success, or evidence creation are checkpoints only. If the parent completion matrix still has required UNKNOWN/FAIL/IN_PROGRESS rows, continue to the next session from state.next_action.
