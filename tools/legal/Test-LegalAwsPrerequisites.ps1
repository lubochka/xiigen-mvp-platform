param(
    [string]$Region = "eu-west-1",
    [string]$OutputPath = "docs/plans/enterprise-adaptation/v3.3/project-assets/legal-publication-completion/legal-aws-deploy-prerequisite-evidence.json"
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
function Resolve-RepoPath {
    param([string]$Path)
    if ([System.IO.Path]::IsPathRooted($Path)) { return $Path }
    return Join-Path $repoRoot $Path
}

$awsCliPath = $null
$awsCommand = Get-Command aws -ErrorAction SilentlyContinue
if ($awsCommand) {
    $awsCliPath = $awsCommand.Source
} else {
    $defaultAwsCli = "C:\Program Files\Amazon\AWSCLIV2\aws.exe"
    if (Test-Path -LiteralPath $defaultAwsCli) {
        $awsCliPath = $defaultAwsCli
    }
}

$dockerCommand = Get-Command docker -ErrorAction SilentlyContinue
$moduleNames = @("AWS.Tools.SecurityToken", "AWS.Tools.ECR", "AWS.Tools.AppRunner")
$modules = @()
foreach ($moduleName in $moduleNames) {
    $module = Get-Module -ListAvailable -Name $moduleName | Sort-Object Version -Descending | Select-Object -First 1
    $modules += [ordered]@{
        name = $moduleName
        present = ($null -ne $module)
        version = if ($module) { [string]$module.Version } else { $null }
        path = if ($module) { [string]$module.Path } else { $null }
    }
}

$callerIdentity = $null
$credentialError = $null
$awsCliPresent = -not [string]::IsNullOrWhiteSpace($awsCliPath)
if ($awsCliPresent) {
    try {
        $identityJson = & $awsCliPath sts get-caller-identity --region $Region 2>&1
        if ($LASTEXITCODE -eq 0) {
            $parsed = ($identityJson | Out-String | ConvertFrom-Json)
            $callerIdentity = [ordered]@{
                account = [string]$parsed.Account
                arn = [string]$parsed.Arn
                userId = [string]$parsed.UserId
            }
        } else {
            $credentialError = ($identityJson | Out-String).Trim()
        }
    }
    catch {
        $credentialError = $_.Exception.Message
    }
} else {
    $credentialError = "AWS CLI not found on PATH or default install path."
}

$awsPowerShellModulesPresent = @($modules | Where-Object { -not $_.present }).Count -eq 0
$awsCredentialsPresent = $null -ne $callerIdentity
$deployCanRunFromThisHost = $awsCredentialsPresent -and ($awsCliPresent -or $awsPowerShellModulesPresent) -and ($null -ne $dockerCommand)

$result = [ordered]@{
    schema_version = "1.0"
    artifact_scope = "legal_aws_deploy_prerequisite_evidence"
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString("o")
    region = $Region
    dockerPresent = ($null -ne $dockerCommand)
    dockerPath = if ($dockerCommand) { [string]$dockerCommand.Source } else { $null }
    awsCliPresent = $awsCliPresent
    awsCliPath = $awsCliPath
    awsPowerShellModulesPresent = $awsPowerShellModulesPresent
    awsPowerShellModules = @($modules)
    awsCredentialsPresent = $awsCredentialsPresent
    callerIdentity = $callerIdentity
    credentialError = $credentialError
    deployCanRunFromThisHost = $deployCanRunFromThisHost
    noSecretValuesPrinted = $true
    nextAction = if ($deployCanRunFromThisHost) {
        "Run tools/legal/Deploy-LegalAwsBlockedShell.ps1 from the platform repository root."
    } else {
        "Authenticate AWS CLI or AWS PowerShell for account 595003292091, verify Docker is running, then rerun this prerequisite check."
    }
}

$resolvedOutputPath = Resolve-RepoPath $OutputPath
$parent = Split-Path -Parent $resolvedOutputPath
if (-not [string]::IsNullOrWhiteSpace($parent)) {
    New-Item -ItemType Directory -Force -Path $parent | Out-Null
}
$json = ($result | ConvertTo-Json -Depth 12) -replace "`r`n", "`n" -replace "`r", "`n"
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($resolvedOutputPath, ($json + "`n"), $utf8NoBom)
Write-Output $json

if (-not $deployCanRunFromThisHost) {
    exit 1
}
