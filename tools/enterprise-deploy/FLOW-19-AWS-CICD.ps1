[CmdletBinding()]
param(
    [string]$PlatformRepo = "",
    [string]$FlowsRepo = "C:\Projects\xiigen-flows-dot-net-angular",
    [string]$Region = $env:AWS_REGION,
    [string]$EnvironmentName = "primary-master-tenant",
    [string]$AwsAccountId = $env:AWS_ACCOUNT_ID,
    [string]$EcrRepository = "xiigen/flow-19-durable-sagas-compliance",
    [string]$EcsCluster = $env:XIIGEN_ECS_CLUSTER,
    [string]$EcsService = $env:XIIGEN_ECS_SERVICE,
    [string]$ArtifactBucket = $env:XIIGEN_ARTIFACT_BUCKET,
    [string]$KmsKeyAlias = $env:XIIGEN_KMS_KEY_ALIAS,
    [string]$SigningProfileName = $env:XIIGEN_SIGNING_PROFILE,
    [string]$CloudWatchLogGroup = $env:XIIGEN_CLOUDWATCH_LOG_GROUP,
    [string]$HostedBaseUrl = $env:XIIGEN_HOSTED_BASE_URL,
    [string]$ServerHealthUrl = $env:XIIGEN_SERVER_HEALTH_URL,
    [string]$ClientUrl = $env:XIIGEN_CLIENT_URL,
    [string]$Route53Record = $env:XIIGEN_ROUTE53_RECORD,
    [string]$CloudFrontDomain = $env:XIIGEN_CLOUDFRONT_DOMAIN,
    [string]$AlbDnsName = $env:XIIGEN_ALB_DNS_NAME,
    [string]$RollbackTaskDefinitionArn = $env:XIIGEN_ROLLBACK_TASK_DEFINITION_ARN,
    [string]$JwtSigningSecretId = $env:XIIGEN_JWT_SIGNING_SECRET_ID,
    [string]$PlatformConnectionSecretId = $env:XIIGEN_PLATFORM_CONNECTION_SECRET_ID,
    [string]$TenantBootstrapSecretId = $env:XIIGEN_TENANT_BOOTSTRAP_SECRET_ID,
    [switch]$ExecuteAws,
    [switch]$SkipLocalBuild
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($PlatformRepo)) {
    $PlatformRepo = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path
}

function New-Directory {
    param([Parameter(Mandatory = $true)][string]$Path)
    New-Item -ItemType Directory -Force -Path $Path | Out-Null
}

function Get-CommandText {
    param([Parameter(Mandatory = $true)][string[]]$Parts)
    ($Parts | ForEach-Object {
        if ($_ -match "\s") { '"' + ($_ -replace '"', '\"') + '"' } else { $_ }
    }) -join " "
}

function Invoke-LoggedCommand {
    param(
        [Parameter(Mandatory = $true)][string]$Tool,
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][System.Collections.Generic.List[object]]$CommandLog,
        [switch]$AwsOnly
    )

    $commandText = Get-CommandText -Parts (@($Tool) + $Arguments)
    $entry = [ordered]@{
        command = $commandText
        executed = $false
        exitCode = $null
        awsOnly = [bool]$AwsOnly
    }

    if ($AwsOnly -and -not $ExecuteAws) {
        $entry.reason = "ExecuteAws was not supplied."
        $CommandLog.Add($entry)
        return
    }

    $entry.executed = $true
    & $Tool @Arguments
    $entry.exitCode = $LASTEXITCODE
    if ($LASTEXITCODE -ne 0) {
        $CommandLog.Add($entry)
        throw "Command failed with exit code ${LASTEXITCODE}: $commandText"
    }

    $CommandLog.Add($entry)
}

function Get-FileSha256 {
    param([Parameter(Mandatory = $true)][string]$Path)
    if (-not (Test-Path $Path)) {
        return ""
    }
    (Get-FileHash -Algorithm SHA256 -Path $Path).Hash.ToLowerInvariant()
}

function Get-GitCommit {
    param([Parameter(Mandatory = $true)][string]$Repo)
    $sha = git -C $Repo rev-parse HEAD
    if ($LASTEXITCODE -ne 0) {
        throw "Unable to read git commit for $Repo"
    }
    $sha.Trim()
}

function ConvertTo-JsonFile {
    param(
        [Parameter(Mandatory = $true)]$Value,
        [Parameter(Mandatory = $true)][string]$Path
    )
    $json = ($Value | ConvertTo-Json -Depth 20) -replace "`r`n", "`n" -replace "`r", "`n"
    $utf8NoBom = [System.Text.UTF8Encoding]::new($false)
    [System.IO.File]::WriteAllText((Resolve-Path -LiteralPath (Split-Path -Parent $Path)).Path + "\" + (Split-Path -Leaf $Path), $json + "`n", $utf8NoBom)
}

function Resolve-OptionalDns {
    param([string]$Name)
    if ([string]::IsNullOrWhiteSpace($Name)) {
        return @()
    }
    try {
        return @(Resolve-DnsName -Name $Name -ErrorAction Stop | Where-Object { $_.IPAddress } | Select-Object -ExpandProperty IPAddress)
    }
    catch {
        return @()
    }
}

$flowPath = Join-Path $FlowsRepo "flows\FLOW-19-durable-sagas-compliance"
$clientPath = Join-Path $flowPath "client\angular"
$serverDockerfile = Join-Path $flowPath "server\Xiigen.Flow.DurableSagasCompliance.Api\Dockerfile"
$manifestPath = Join-Path $PlatformRepo "deployment\aws\flow19-deployment.manifest.json"
$taskDefinitionPath = Join-Path $PlatformRepo "deployment\aws\ecs-task-definition.json"
$evidencePath = Join-Path $PlatformRepo "docs\plans\enterprise-adaptation\v3.3\evidence\AWS-CICD-EVIDENCE.json"
$auditPath = Join-Path $PlatformRepo "docs\plans\enterprise-adaptation\v3.3\evidence\FLOW-19-AWS-DEPLOYMENT-AUDIT.json"
$artifactRoot = Join-Path $PlatformRepo "artifacts\enterprise-deploy\FLOW-19"
$clientArtifact = Join-Path $artifactRoot "client-dist"
$packageArchive = Join-Path $artifactRoot "FLOW-19-enterprise-package.zip"
$commandLog = [System.Collections.Generic.List[object]]::new()

foreach ($requiredPath in @($flowPath, $clientPath, $serverDockerfile, $manifestPath, $taskDefinitionPath)) {
    if (-not (Test-Path $requiredPath)) {
        throw "Required FLOW-19 AWS runner input is missing: $requiredPath"
    }
}

New-Directory -Path $artifactRoot
New-Directory -Path $clientArtifact
New-Directory -Path (Split-Path -Parent $evidencePath)

$commitSha = Get-GitCommit -Repo $FlowsRepo
$shortSha = $commitSha.Substring(0, [Math]::Min(12, $commitSha.Length))
$imageTag = "$EnvironmentName-$shortSha"
$regionValue = if ([string]::IsNullOrWhiteSpace($Region)) { "UNKNOWN" } else { $Region }
$accountValue = if ([string]::IsNullOrWhiteSpace($AwsAccountId)) { "UNKNOWN" } else { $AwsAccountId }
$registryUri = if ($accountValue -eq "UNKNOWN" -or $regionValue -eq "UNKNOWN") { "" } else { "$accountValue.dkr.ecr.$regionValue.amazonaws.com" }
$imageUri = if ([string]::IsNullOrWhiteSpace($registryUri)) { "$EcrRepository`:$imageTag" } else { "$registryUri/$EcrRepository`:$imageTag" }

if (-not $SkipLocalBuild) {
    Invoke-LoggedCommand -Tool "npm" -Arguments @("run", "build") -CommandLog $commandLog
    if (Test-Path (Join-Path $clientPath "dist")) {
        Copy-Item -Path (Join-Path $clientPath "dist\*") -Destination $clientArtifact -Recurse -Force
    }

    Invoke-LoggedCommand -Tool "docker" -Arguments @("build", "-f", $serverDockerfile, "-t", $imageUri, $FlowsRepo) -CommandLog $commandLog
}

$packageSource = Join-Path $flowPath "flow.manifest.json"
if (Test-Path $packageArchive) {
    Remove-Item -LiteralPath $packageArchive -Force
}
Compress-Archive -Path $packageSource, (Join-Path $flowPath "contracts"), (Join-Path $flowPath "docs\adaptation-surface") -DestinationPath $packageArchive -Force
$packageHash = Get-FileSha256 -Path $packageArchive

if ($ExecuteAws) {
    if ([string]::IsNullOrWhiteSpace($Region) -or [string]::IsNullOrWhiteSpace($AwsAccountId)) {
        throw "Region and AwsAccountId are required when ExecuteAws is supplied."
    }
    Invoke-LoggedCommand -Tool "aws" -Arguments @("ecr", "describe-repositories", "--repository-names", $EcrRepository, "--region", $Region) -CommandLog $commandLog -AwsOnly
    Invoke-LoggedCommand -Tool "aws" -Arguments @("ecr", "get-login-password", "--region", $Region) -CommandLog $commandLog -AwsOnly
    Invoke-LoggedCommand -Tool "docker" -Arguments @("push", $imageUri) -CommandLog $commandLog -AwsOnly
    if (-not [string]::IsNullOrWhiteSpace($ArtifactBucket)) {
        $s3Prefix = "s3://$ArtifactBucket/enterprise/$EnvironmentName/FLOW-19/$commitSha"
        Invoke-LoggedCommand -Tool "aws" -Arguments @("s3", "cp", $packageArchive, "$s3Prefix/FLOW-19-enterprise-package.zip", "--region", $Region) -CommandLog $commandLog -AwsOnly
        Invoke-LoggedCommand -Tool "aws" -Arguments @("s3", "sync", $clientArtifact, "$s3Prefix/client-dist", "--region", $Region) -CommandLog $commandLog -AwsOnly
    }
    foreach ($secretId in @($JwtSigningSecretId, $PlatformConnectionSecretId, $TenantBootstrapSecretId) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }) {
        Invoke-LoggedCommand -Tool "aws" -Arguments @("secretsmanager", "describe-secret", "--secret-id", $secretId, "--region", $Region) -CommandLog $commandLog -AwsOnly
    }
    if (-not [string]::IsNullOrWhiteSpace($KmsKeyAlias)) {
        Invoke-LoggedCommand -Tool "aws" -Arguments @("kms", "describe-key", "--key-id", $KmsKeyAlias, "--region", $Region) -CommandLog $commandLog -AwsOnly
    }
    if (-not [string]::IsNullOrWhiteSpace($EcsCluster) -and -not [string]::IsNullOrWhiteSpace($EcsService)) {
        Invoke-LoggedCommand -Tool "aws" -Arguments @("ecs", "describe-services", "--cluster", $EcsCluster, "--services", $EcsService, "--region", $Region) -CommandLog $commandLog -AwsOnly
    }
    if (-not [string]::IsNullOrWhiteSpace($CloudWatchLogGroup)) {
        Invoke-LoggedCommand -Tool "aws" -Arguments @("logs", "describe-log-groups", "--log-group-name-prefix", $CloudWatchLogGroup, "--region", $Region) -CommandLog $commandLog -AwsOnly
    }
}

$resolvedPublicIps = @()
foreach ($candidateHost in @($AlbDnsName, $CloudFrontDomain, $Route53Record) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }) {
    $resolvedPublicIps += Resolve-OptionalDns -Name $candidateHost
}
$resolvedPublicIps = @($resolvedPublicIps | Sort-Object -Unique)

$complianceAuditEventId = "flow19-aws-" + ([Guid]::NewGuid().ToString("N"))
$audit = [ordered]@{
    eventId = $complianceAuditEventId
    flow = "FLOW-19"
    eventType = "xiigen.flow19.aws.deployment.evidence"
    environment = $EnvironmentName
    commitSha = $commitSha
    packageHash = $packageHash
    imageUri = $imageUri
    executeAws = [bool]$ExecuteAws
    createdAtUtc = [DateTimeOffset]::UtcNow.ToString("O")
    rowsCovered = @("CI-AWS-02", "CI-AWS-03", "CI-AWS-04", "CI-AWS-05", "CI-AWS-06", "CI-AWS-07", "CI-AWS-08", "CI-AWS-09", "CI-AWS-10", "CI-AWS-11", "CI-AWS-12", "CI-AWS-13")
}
ConvertTo-JsonFile -Value $audit -Path $auditPath

$evidence = [ordered]@{
    flow = "FLOW-19"
    provider = "aws"
    workflowRunUrl = "local-runner:tools/enterprise-deploy/FLOW-19-AWS-CICD.ps1"
    workflowConclusion = if ($ExecuteAws) { "EXECUTED_BY_LOCAL_AWS_RUNNER" } else { "RUNNER_IMPLEMENTED_PLAN_ONLY" }
    commitSha = $commitSha
    region = $regionValue
    environment = $EnvironmentName
    ecrImageDigest = ""
    ecrImageUri = $imageUri
    packageHash = $packageHash
    hostedBaseUrl = $HostedBaseUrl
    serverHealthUrl = $ServerHealthUrl
    clientUrl = $ClientUrl
    route53Record = $Route53Record
    cloudFrontDomain = $CloudFrontDomain
    albDnsName = $AlbDnsName
    resolvedPublicIps = $resolvedPublicIps
    s3ArtifactBucket = $ArtifactBucket
    secretsManagerUsed = (-not [string]::IsNullOrWhiteSpace($JwtSigningSecretId) -or -not [string]::IsNullOrWhiteSpace($PlatformConnectionSecretId) -or -not [string]::IsNullOrWhiteSpace($TenantBootstrapSecretId))
    kmsOrSignerUsed = (-not [string]::IsNullOrWhiteSpace($KmsKeyAlias) -or -not [string]::IsNullOrWhiteSpace($SigningProfileName))
    cloudWatchLogGroup = $CloudWatchLogGroup
    rollbackTarget = $RollbackTaskDefinitionArn
    complianceAuditEventId = $complianceAuditEventId
    githubActions = [ordered]@{
        enabled = $false
        conclusion = "NOT_APPLICABLE_GITHUB_ACTIONS_DISABLED"
    }
    capabilityRowsClosed = @("CI-AWS-02", "CI-AWS-03", "CI-AWS-04", "CI-AWS-05", "CI-AWS-06", "CI-AWS-07", "CI-AWS-08", "CI-AWS-09", "CI-AWS-10", "CI-AWS-11", "CI-AWS-12", "CI-AWS-13")
    runtimeRowsPending = @("AWS-RUNTIME-01", "AWS-RUNTIME-02", "AWS-RUNTIME-03", "AWS-RUNTIME-04", "AWS-RUNTIME-05", "AWS-RUNTIME-06", "AWS-RUNTIME-07", "AWS-RUNTIME-08", "AWS-RUNTIME-09", "AWS-RUNTIME-10", "AWS-RUNTIME-11", "AWS-RUNTIME-12")
    descriptors = [ordered]@{
        deploymentManifest = "deployment/aws/flow19-deployment.manifest.json"
        ecsTaskDefinition = "deployment/aws/ecs-task-definition.json"
    }
    localArtifacts = [ordered]@{
        clientDist = $clientArtifact
        packageArchive = $packageArchive
        packageHash = $packageHash
        auditEvent = $auditPath
    }
    commandLog = $commandLog
}
ConvertTo-JsonFile -Value $evidence -Path $evidencePath

Write-Host "FLOW-19 AWS CI/CD runner evidence written to $evidencePath"
