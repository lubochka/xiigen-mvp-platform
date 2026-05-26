param(
    [string]$AwsAccountId = $env:AWS_ACCOUNT_ID,
    [string]$Region = $env:AWS_REGION,
    [string]$AppRunnerServiceArn = $env:XIIGEN_APP_RUNNER_SERVICE_ARN,
    [string]$EcrRepository = $env:XIIGEN_PLATFORM_ECR_REPOSITORY,
    [string]$BaseUrl = "https://gyxwjim5ea.eu-west-1.awsapprunner.com",
    [string]$OutputPath = "docs/plans/enterprise-adaptation/v3.3/project-assets/legal-publication-completion/legal-aws-blocked-shell-evidence.json",
    [string]$DeploymentEvidencePath = "docs/plans/enterprise-adaptation/v3.3/project-assets/legal-publication-completion/legal-aws-blocked-shell-deploy-evidence.json",
    [string]$ImageTag = "",
    [string]$AwsCliPython = $env:AWS_CLI_PYTHON,
    [switch]$SkipVerification
)

$ErrorActionPreference = "Stop"

function Require-Value {
    param(
        [Parameter(Mandatory = $true)][string]$Name,
        [AllowEmptyString()][string]$Value
    )

    if ([string]::IsNullOrWhiteSpace($Value)) {
        throw "$Name is required. Set the parameter or environment variable before running this deploy script."
    }
}

function Require-Command {
    param([Parameter(Mandatory = $true)][string]$Name)

    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Required command '$Name' was not found in PATH."
    }
}

function Add-LogEntry {
    param(
        [Parameter(Mandatory = $true)][System.Collections.Generic.List[object]]$CommandLog,
        [Parameter(Mandatory = $true)][string]$Command,
        [Parameter(Mandatory = $true)][bool]$Executed,
        [AllowNull()][int]$ExitCode
    )

    $CommandLog.Add([ordered]@{
        command = $Command
        executed = $Executed
        exitCode = $ExitCode
        createdAtUtc = (Get-Date).ToUniversalTime().ToString("o")
    })
}

function Invoke-LoggedNative {
    param(
        [Parameter(Mandatory = $true)][string]$Tool,
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][System.Collections.Generic.List[object]]$CommandLog
    )

    $commandText = (@($Tool) + $Arguments) -join " "
    & $Tool @Arguments
    $exitCode = $LASTEXITCODE
    Add-LogEntry -CommandLog $CommandLog -Command $commandText -Executed $true -ExitCode $exitCode
    if ($exitCode -ne 0) {
        throw "Command failed with exit code ${exitCode}: $commandText"
    }
}

function Invoke-AwsLogged {
    param(
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][System.Collections.Generic.List[object]]$CommandLog,
        [switch]$CaptureOutput
    )

    if (-not [string]::IsNullOrWhiteSpace($AwsCliPython)) {
        if (-not (Test-Path -LiteralPath $AwsCliPython)) {
            throw "AwsCliPython not found: $AwsCliPython"
        }
        $commandText = @($AwsCliPython, "-m", "awscli") + $Arguments -join " "
        $output = & $AwsCliPython -m awscli @Arguments
    }
    else {
        Require-Command -Name "aws"
        $commandText = @("aws") + $Arguments -join " "
        $output = & aws @Arguments
    }

    $exitCode = $LASTEXITCODE
    Add-LogEntry -CommandLog $CommandLog -Command $commandText -Executed $true -ExitCode $exitCode
    if ($exitCode -ne 0) {
        throw "AWS command failed with exit code ${exitCode}: $commandText"
    }
    if ($CaptureOutput) {
        return $output
    }
}

function Write-JsonFile {
    param(
        [Parameter(Mandatory = $true)]$Value,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $parent = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        New-Item -ItemType Directory -Force -Path $parent | Out-Null
    }

    $json = ($Value | ConvertTo-Json -Depth 12) -replace "`r`n", "`n" -replace "`r", "`n"
    $utf8NoBom = [System.Text.UTF8Encoding]::new($false)
    [System.IO.File]::WriteAllText((Resolve-Path -LiteralPath $parent).Path + "\" + (Split-Path -Leaf $Path), $json + "`n", $utf8NoBom)
}

Require-Value -Name "AWS account ID" -Value $AwsAccountId
Require-Value -Name "AWS region" -Value $Region
Require-Value -Name "App Runner service ARN" -Value $AppRunnerServiceArn
Require-Value -Name "ECR repository" -Value $EcrRepository
Require-Command -Name "git"
Require-Command -Name "docker"

if ([string]::IsNullOrWhiteSpace($ImageTag)) {
    $safeDirectory = ((Get-Location).Path -replace "\\", "/")
    $shortSha = (git -c "safe.directory=$safeDirectory" rev-parse --short HEAD).Trim()
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($shortSha)) {
        throw "Unable to resolve git short SHA for image tag."
    }
    $ImageTag = "legal-shell-$shortSha"
}

$registry = "$AwsAccountId.dkr.ecr.$Region.amazonaws.com"
$image = "$registry/$EcrRepository`:$ImageTag"
$commandLog = [System.Collections.Generic.List[object]]::new()

Invoke-LoggedNative -Tool "docker" -Arguments @("build", "-f", "src/Xiigen.Platform.Api/Dockerfile", "-t", $image, ".") -CommandLog $commandLog

$password = Invoke-AwsLogged -Arguments @("ecr", "get-login-password", "--region", $Region) -CommandLog $commandLog -CaptureOutput
if ([string]::IsNullOrWhiteSpace($password)) {
    throw "Unable to get ECR login password."
}

$password | docker login --username AWS --password-stdin $registry
$dockerLoginExitCode = $LASTEXITCODE
Add-LogEntry -CommandLog $commandLog -Command "docker login --username AWS --password-stdin $registry" -Executed $true -ExitCode $dockerLoginExitCode
if ($dockerLoginExitCode -ne 0) {
    throw "Docker login failed for $registry."
}

Invoke-LoggedNative -Tool "docker" -Arguments @("push", $image) -CommandLog $commandLog
Invoke-AwsLogged -Arguments @(
    "apprunner",
    "update-service",
    "--service-arn",
    $AppRunnerServiceArn,
    "--source-configuration",
    "ImageRepository={ImageIdentifier=$image,ImageRepositoryType=ECR,ImageConfiguration={Port=8080}}",
    "--region",
    $Region
) -CommandLog $commandLog
Invoke-AwsLogged -Arguments @(
    "apprunner",
    "start-deployment",
    "--service-arn",
    $AppRunnerServiceArn,
    "--region",
    $Region
) -CommandLog $commandLog

$deployEvidence = [ordered]@{
    schema_version = "1.0"
    artifact_scope = "legal_aws_blocked_shell_deploy_evidence"
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString("o")
    image = $image
    baseUrl = $BaseUrl
    appRunnerServiceArn = $AppRunnerServiceArn
    ecrRepository = $EcrRepository
    region = $Region
    awsCliPython = $AwsCliPython
    verificationSkipped = [bool]$SkipVerification
    blockedShellEvidence = $OutputPath
    commandLog = $commandLog
}
Write-JsonFile -Value $deployEvidence -Path $DeploymentEvidencePath

if (-not $SkipVerification) {
    & tools/legal/Test-LegalAwsBlockedShell.ps1 -BaseUrl $BaseUrl -OutputPath $OutputPath
    if ($LASTEXITCODE -ne 0) {
        throw "Deploy completed, but blocked-shell verification did not pass yet. App Runner may still be rolling out; rerun tools/legal/Test-LegalAwsBlockedShell.ps1."
    }
}

Write-Output "Legal blocked-shell deploy submitted for $image"
