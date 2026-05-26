param(
    [ValidateSet("Auto", "Cli", "PowerShell")]
    [string]$Toolchain = "Auto",
    [string]$AwsAccountId = $env:AWS_ACCOUNT_ID,
    [string]$Region = $env:AWS_REGION,
    [string]$AppRunnerServiceArn = $env:XIIGEN_APP_RUNNER_SERVICE_ARN,
    [string]$EcrRepository = $env:XIIGEN_PLATFORM_ECR_REPOSITORY,
    [string]$BaseUrl = $(if ([string]::IsNullOrWhiteSpace($env:XIIGEN_PLATFORM_BASE_URL)) { "https://gyxwjim5ea.eu-west-1.awsapprunner.com" } else { $env:XIIGEN_PLATFORM_BASE_URL }),
    [string]$OutputPath = "docs/plans/enterprise-adaptation/v3.3/project-assets/legal-publication-completion/legal-aws-blocked-shell-evidence.json",
    [string]$DeploymentEvidencePath = "docs/plans/enterprise-adaptation/v3.3/project-assets/legal-publication-completion/legal-aws-blocked-shell-deploy-evidence.json",
    [string]$ImageTag = "",
    [int]$VerificationAttempts = 18,
    [int]$VerificationDelaySeconds = 20,
    [switch]$SkipDockerBuild,
    [switch]$SkipDeploy,
    [switch]$SkipVerification
)

$ErrorActionPreference = "Stop"

if (-not (Get-Command "aws" -ErrorAction SilentlyContinue)) {
    $defaultAwsCli = "C:\Program Files\Amazon\AWSCLIV2"
    if (Test-Path -LiteralPath (Join-Path $defaultAwsCli "aws.exe")) {
        $env:PATH = "$defaultAwsCli;$env:PATH"
    }
}

function Require-Value {
    param([string]$Name, [AllowEmptyString()][string]$Value)
    if ([string]::IsNullOrWhiteSpace($Value)) {
        throw "$Name is required. Set the parameter or environment variable before running this deploy script."
    }
}

function Require-Command {
    param([string]$Name)
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Required command '$Name' was not found in PATH."
    }
}

function Add-LogEntry {
    param($CommandLog, [string]$Command, [bool]$Executed, [AllowNull()][int]$ExitCode)
    $CommandLog.Add([ordered]@{
        command = $Command
        executed = $Executed
        exitCode = $ExitCode
        createdAtUtc = (Get-Date).ToUniversalTime().ToString("o")
    })
}

function Invoke-LoggedNative {
    param([string]$Tool, [string[]]$Arguments, $CommandLog)
    $commandText = (@($Tool) + $Arguments) -join " "
    & $Tool @Arguments
    $exitCode = $LASTEXITCODE
    Add-LogEntry -CommandLog $CommandLog -Command $commandText -Executed $true -ExitCode $exitCode
    if ($exitCode -ne 0) {
        throw "Command failed with exit code ${exitCode}: $commandText"
    }
}

function Write-JsonFile {
    param($Value, [string]$Path)
    $parent = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        New-Item -ItemType Directory -Force -Path $parent | Out-Null
    }
    $resolvedParent = (Resolve-Path -LiteralPath $parent).Path
    $json = ($Value | ConvertTo-Json -Depth 12) -replace "`r`n", "`n" -replace "`r", "`n"
    $utf8NoBom = [System.Text.UTF8Encoding]::new($false)
    [System.IO.File]::WriteAllText((Join-Path $resolvedParent (Split-Path -Leaf $Path)), $json + "`n", $utf8NoBom)
}

function Import-AwsPowerShellModules {
    Import-Module AWS.Tools.SecurityToken -ErrorAction Stop
    Import-Module AWS.Tools.ECR -ErrorAction Stop
    Import-Module AWS.Tools.AppRunner -ErrorAction Stop
}

function Resolve-AwsToolchain {
    param([string]$RequestedToolchain)
    $awsCliPresent = [bool](Get-Command aws -ErrorAction SilentlyContinue)
    $awsPowerShellPresent = [bool](Get-Module -ListAvailable AWS.Tools.SecurityToken) -and [bool](Get-Module -ListAvailable AWS.Tools.ECR) -and [bool](Get-Module -ListAvailable AWS.Tools.AppRunner)

    if ($RequestedToolchain -eq "Cli") {
        if (-not $awsCliPresent) { throw "AWS CLI was requested but aws.exe is not on PATH." }
        return "Cli"
    }
    if ($RequestedToolchain -eq "PowerShell") {
        if (-not $awsPowerShellPresent) { throw "AWS PowerShell was requested but required AWS.Tools modules are not installed." }
        Import-AwsPowerShellModules
        return "PowerShell"
    }
    if ($awsCliPresent) { return "Cli" }
    if ($awsPowerShellPresent) {
        Import-AwsPowerShellModules
        return "PowerShell"
    }
    throw "No AWS deployment toolchain is available. Install AWS CLI v2 or AWS.Tools.SecurityToken/AWS.Tools.ECR/AWS.Tools.AppRunner."
}

Require-Value -Name "AWS account ID" -Value $AwsAccountId
Require-Value -Name "AWS region" -Value $Region
Require-Value -Name "App Runner service ARN" -Value $AppRunnerServiceArn
Require-Value -Name "ECR repository" -Value $EcrRepository
Require-Command -Name "git"
Require-Command -Name "docker"

$commandLog = [System.Collections.Generic.List[object]]::new()
$resolvedToolchain = Resolve-AwsToolchain -RequestedToolchain $Toolchain

if ($resolvedToolchain -eq "Cli") {
    Invoke-LoggedNative -Tool "aws" -Arguments @("sts", "get-caller-identity", "--region", $Region) -CommandLog $commandLog
}
else {
    $caller = Get-STSCallerIdentity -Region $Region -ErrorAction Stop
    if ($null -eq $caller) { throw "AWS PowerShell returned no caller identity. Check AWS credentials." }
    Add-LogEntry -CommandLog $commandLog -Command "Get-STSCallerIdentity -Region $Region" -Executed $true -ExitCode 0
}

if ([string]::IsNullOrWhiteSpace($ImageTag)) {
    $safeDirectory = ((Get-Location).Path -replace "\\", "/")
    $shortSha = (git -c "safe.directory=$safeDirectory" rev-parse --short HEAD).Trim()
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($shortSha)) { throw "Unable to resolve git short SHA for image tag." }
    $ImageTag = "legal-shell-$shortSha"
}

$registry = "$AwsAccountId.dkr.ecr.$Region.amazonaws.com"
$image = "$registry/$EcrRepository`:$ImageTag"

if (-not $SkipDockerBuild) {
    Invoke-LoggedNative -Tool "docker" -Arguments @("build", "-f", "src/Xiigen.Platform.Api/Dockerfile", "-t", $image, ".") -CommandLog $commandLog
    if ($resolvedToolchain -eq "Cli") {
        $password = aws ecr get-login-password --region $Region
        if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($password)) { throw "Unable to get ECR login password." }
        Add-LogEntry -CommandLog $commandLog -Command "aws ecr get-login-password --region $Region" -Executed $true -ExitCode 0
    }
    else {
        $login = Get-ECRLoginCommand -Region $Region -RegistryId $AwsAccountId -ErrorAction Stop
        $password = $login.Password
        if ([string]::IsNullOrWhiteSpace($password)) { throw "Get-ECRLoginCommand returned no password." }
        Add-LogEntry -CommandLog $commandLog -Command "Get-ECRLoginCommand -Region $Region -RegistryId $AwsAccountId" -Executed $true -ExitCode 0
    }
    $password | docker login --username AWS --password-stdin $registry
    $dockerLoginExitCode = $LASTEXITCODE
    Add-LogEntry -CommandLog $commandLog -Command "docker login --username AWS --password-stdin $registry" -Executed $true -ExitCode $dockerLoginExitCode
    if ($dockerLoginExitCode -ne 0) { throw "Docker login failed for $registry." }
    Invoke-LoggedNative -Tool "docker" -Arguments @("push", $image) -CommandLog $commandLog
}

if (-not $SkipDeploy) {
    if ($resolvedToolchain -eq "Cli") {
        Invoke-LoggedNative -Tool "aws" -Arguments @("apprunner", "update-service", "--service-arn", $AppRunnerServiceArn, "--source-configuration", "ImageRepository={ImageIdentifier=$image,ImageRepositoryType=ECR,ImageConfiguration={Port=8080}}", "--region", $Region) -CommandLog $commandLog
        Invoke-LoggedNative -Tool "aws" -Arguments @("apprunner", "start-deployment", "--service-arn", $AppRunnerServiceArn, "--region", $Region) -CommandLog $commandLog
    }
    else {
        Update-AARService -ServiceArn $AppRunnerServiceArn -ImageRepository_ImageIdentifier $image -ImageRepository_ImageRepositoryType ECR -ImageConfiguration_Port "8080" -Region $Region -Force | Out-Null
        Add-LogEntry -CommandLog $commandLog -Command "Update-AARService -ServiceArn $AppRunnerServiceArn -ImageRepository_ImageIdentifier $image" -Executed $true -ExitCode 0
        Start-AARDeployment -ServiceArn $AppRunnerServiceArn -Region $Region -Force | Out-Null
        Add-LogEntry -CommandLog $commandLog -Command "Start-AARDeployment -ServiceArn $AppRunnerServiceArn" -Executed $true -ExitCode 0
    }
}

$deployEvidence = [ordered]@{
    schema_version = "1.0"
    artifact_scope = "legal_aws_blocked_shell_deploy_evidence"
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString("o")
    image = $image
    baseUrl = $BaseUrl
    appRunnerServiceArn = $AppRunnerServiceArn
    ecrRepository = $EcrRepository
    region = $Region
    toolchain = $resolvedToolchain
    dockerBuildSkipped = [bool]$SkipDockerBuild
    deploySkipped = [bool]$SkipDeploy
    verificationSkipped = [bool]$SkipVerification
    verificationAttempts = $VerificationAttempts
    verificationDelaySeconds = $VerificationDelaySeconds
    blockedShellEvidence = $OutputPath
    commandLog = $commandLog
}
Write-JsonFile -Value $deployEvidence -Path $DeploymentEvidencePath

if (-not $SkipVerification) {
    $verified = $false
    for ($attempt = 1; $attempt -le $VerificationAttempts; $attempt++) {
        & tools/legal/Test-LegalAwsBlockedShell.ps1 -BaseUrl $BaseUrl -OutputPath $OutputPath
        if ($LASTEXITCODE -eq 0) { $verified = $true; break }
        if ($attempt -lt $VerificationAttempts) { Start-Sleep -Seconds $VerificationDelaySeconds }
    }
    if (-not $verified) { throw "Deploy completed, but blocked-shell verification did not pass after $VerificationAttempts attempts. App Runner may still be rolling out; rerun tools/legal/Test-LegalAwsBlockedShell.ps1." }
}

Write-Output "Legal blocked-shell deploy submitted for $image"
