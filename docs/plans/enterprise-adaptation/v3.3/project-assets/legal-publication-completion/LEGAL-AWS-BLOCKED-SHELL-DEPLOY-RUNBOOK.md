# LEGAL AWS BLOCKED-SHELL DEPLOY RUNBOOK

Status: executable deploy runbook for the technical legal shell. This does not approve or publish legal copy.

## Current machine prerequisite check

- Docker is present.
- AWS CLI is required for ECR/App Runner deployment.
- On this machine, `aws --version` was not available during SESSION-14 verification, so AWS deployment cannot run from this host until AWS CLI credentials/tooling are installed.

## Goal

Deploy the current platform API image containing `/legal/{slug}` and `/legal/{locale}/{slug}` so AWS returns the blocked legal-review shell for unapproved records instead of 404.

This closes only technical route availability evidence. It does not close `LEGAL-PUBLIC-*` rows.

## Required environment

```powershell
$env:AWS_ACCOUNT_ID = "595003292091"
$env:AWS_REGION = "eu-west-1"
$env:XIIGEN_APP_RUNNER_SERVICE_ARN = "arn:aws:apprunner:eu-west-1:595003292091:service/xiigen-platform-primary/a43b42b090ef4d66b33099863c28555d"
$env:XIIGEN_PLATFORM_ECR_REPOSITORY = "xiigen/platform-api"
```

AWS CLI must be authenticated for the account above and must have ECR push + App Runner update/start-deployment permissions.

## Deploy command outline

```powershell
$tag = "legal-shell-" + (git rev-parse --short HEAD)
$image = "$env:AWS_ACCOUNT_ID.dkr.ecr.$env:AWS_REGION.amazonaws.com/$env:XIIGEN_PLATFORM_ECR_REPOSITORY:$tag"

docker build -f src/Xiigen.Platform.Api/Dockerfile -t $image .
aws ecr get-login-password --region $env:AWS_REGION | docker login --username AWS --password-stdin "$env:AWS_ACCOUNT_ID.dkr.ecr.$env:AWS_REGION.amazonaws.com"
docker push $image
aws apprunner update-service --service-arn $env:XIIGEN_APP_RUNNER_SERVICE_ARN --source-configuration "ImageRepository={ImageIdentifier=$image,ImageRepositoryType=ECR,ImageConfiguration={Port=8080}}" --region $env:AWS_REGION
aws apprunner start-deployment --service-arn $env:XIIGEN_APP_RUNNER_SERVICE_ARN --region $env:AWS_REGION
```

## Post-deploy evidence command

```powershell
tools/legal/Test-LegalAwsBlockedShell.ps1 -BaseUrl https://gyxwjim5ea.eu-west-1.awsapprunner.com -OutputPath docs/plans/enterprise-adaptation/v3.3/project-assets/legal-publication-completion/legal-aws-blocked-shell-evidence.json
```

Expected interim result before legal approval:

- `/legal/en/terms-of-use` returns `423 Locked`.
- `/legal/he/terms-of-use` returns `423 Locked` and `dir="rtl"`.
- `/legal/ru/terms-of-use` returns `423 Locked`.
- No route exposes final approved legal body.

## Final public launch still requires

After legal approval and localization, seed approved records, redeploy if needed, then rerun AWS visual evidence for all 36 routes. Only that later evidence can move `LEGAL-PUBLIC-*` rows to PASS.
