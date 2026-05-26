param(
    [string]$PlatformRepo = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path,
    [string]$OutputPath = "docs/plans/enterprise-adaptation/v3.3/project-assets/legal-publication-completion/legal-locale-scope-decision-validator-test-evidence.json"
)

$ErrorActionPreference = "Stop"

function Resolve-RepoPath {
    param([string]$Path)
    if ([System.IO.Path]::IsPathRooted($Path)) { return $Path }
    return (Join-Path $PlatformRepo $Path)
}

function Write-JsonFile {
    param($Value, [string]$Path)
    $parent = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($parent)) { New-Item -ItemType Directory -Force -Path $parent | Out-Null }
    $json = ($Value | ConvertTo-Json -Depth 20) -replace "`r`n", "`n" -replace "`r", "`n"
    $utf8NoBom = [System.Text.UTF8Encoding]::new($false)
    [System.IO.File]::WriteAllText((Join-Path (Resolve-Path -LiteralPath $parent).Path (Split-Path -Leaf $Path)), $json + "`n", $utf8NoBom)
}

function Read-Json { param([string]$Path) Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json }

$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("xiigen-locale-scope-test-" + [Guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Force -Path $tempRoot | Out-Null
$validDecisionPath = Join-Path $tempRoot "locale-scope-decision.valid.json"
$validOutputPath = Join-Path $tempRoot "locale-scope-decision.valid.validation.json"
$validator = Join-Path $PlatformRepo "tools/legal/Validate-LegalLocaleScopeDecision.ps1"
$validDecision = [ordered]@{
    schema_version = "1.0"
    artifact_scope = "legal_locale_scope_decision"
    decisionId = "synthetic-locale-scope-decision"
    status = "APPROVED"
    includedLocales = @("en")
    excludedLocales = @(
        [ordered]@{ locale = "he"; reason = "Synthetic validator test exclusion reason."; requiredBeforeLocaleLaunch = $true },
        [ordered]@{ locale = "ru"; reason = "Synthetic validator test exclusion reason."; requiredBeforeLocaleLaunch = $true }
    )
    decidedBy = "synthetic-test-approval-authority"
    decidedAtUtc = "2026-05-26T00:00:00Z"
    effectiveDate = "2026-05-26"
    rationale = "Synthetic validator test rationale for EN-only launch scope."
    sourceEvidencePath = "docs/plans/enterprise-adaptation/v3.3/project-assets/legal-publication-completion/synthetic-locale-scope-evidence.md"
    publicLaunchAllowed = $true
}
Write-JsonFile -Value $validDecision -Path $validDecisionPath
& $validator -DecisionPath $validDecisionPath -OutputPath $validOutputPath | Out-Null
if (-not $?) { throw "Valid synthetic locale-scope decision did not pass validation." }
$validResult = Read-Json -Path $validOutputPath
$passed = [bool]$validResult.passed -and @($validResult.includedLocales).Count -eq 1 -and @($validResult.excludedLocales).Count -eq 2
$result = [ordered]@{
    schema_version = "1.0"
    artifact_scope = "legal_locale_scope_decision_validator_test_evidence"
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString("o")
    status = if ($passed) { "PASS_SYNTHETIC_LOCALE_SCOPE_VALIDATOR" } else { "FAIL_SYNTHETIC_LOCALE_SCOPE_VALIDATOR" }
    passed = $passed
    syntheticOnly = $true
    realLegalRowsPass = $false
    tempRoot = $tempRoot
    validator = "tools/legal/Validate-LegalLocaleScopeDecision.ps1"
    validDecisionPassed = [bool]$validResult.passed
    includedLocales = @($validResult.includedLocales)
    excludedLocales = @($validResult.excludedLocales)
}
Write-JsonFile -Value $result -Path (Resolve-RepoPath $OutputPath)
Write-Output ($result | ConvertTo-Json -Depth 12)
if (-not $passed) { exit 1 }
