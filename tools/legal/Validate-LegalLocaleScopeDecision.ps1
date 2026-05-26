param(
    [Parameter(Mandatory = $true)]
    [string]$DecisionPath,

    [string]$OutputPath = ""
)

$ErrorActionPreference = "Stop"
$allLocales = @("en", "he", "ru")

if (-not (Test-Path -LiteralPath $DecisionPath)) {
    throw "DecisionPath not found: $DecisionPath"
}

$decisionRoot = Get-Content -Raw -LiteralPath $DecisionPath | ConvertFrom-Json
$decision = if ($decisionRoot.PSObject.Properties.Name -contains "decision") { $decisionRoot.decision } else { $decisionRoot }
$errors = [System.Collections.Generic.List[string]]::new()

foreach ($field in @("decisionId", "status", "includedLocales", "excludedLocales", "decidedBy", "decidedAtUtc", "effectiveDate", "rationale", "sourceEvidencePath")) {
    if ($null -eq $decision.$field) {
        $errors.Add("missing required field $field")
        continue
    }
    if ($field -notin @("includedLocales", "excludedLocales") -and [string]::IsNullOrWhiteSpace([string]$decision.$field)) {
        $errors.Add("missing required field $field")
    }
}

if ($decision.status -ne "APPROVED") {
    $errors.Add("status must be APPROVED")
}

$included = @($decision.includedLocales)
if ($included.Count -lt 1) {
    $errors.Add("includedLocales must include at least one locale")
}
foreach ($locale in $included) {
    if ($locale -notin $allLocales) { $errors.Add("included locale is unsupported: $locale") }
}

$excluded = @($decision.excludedLocales)
$excludedLocales = @($excluded | ForEach-Object { $_.locale })
foreach ($locale in $excludedLocales) {
    if ($locale -notin $allLocales) { $errors.Add("excluded locale is unsupported: $locale") }
    if ($locale -in $included) { $errors.Add("locale cannot be both included and excluded: $locale") }
}
foreach ($locale in $allLocales) {
    if ($locale -notin $included -and $locale -notin $excludedLocales) {
        $errors.Add("locale must be included or explicitly excluded: $locale")
    }
}
foreach ($row in $excluded) {
    if ([string]::IsNullOrWhiteSpace([string]$row.reason)) { $errors.Add("excluded locale $($row.locale) missing reason") }
    if ($null -eq $row.requiredBeforeLocaleLaunch) { $errors.Add("excluded locale $($row.locale) missing requiredBeforeLocaleLaunch") }
}

if ([string]$decision.effectiveDate -notmatch "^\d{4}-\d{2}-\d{2}$") {
    $errors.Add("effectiveDate must match yyyy-MM-dd")
}
$decidedAt = [datetimeoffset]::MinValue
if (-not [datetimeoffset]::TryParse([string]$decision.decidedAtUtc, [ref]$decidedAt) -or [string]$decision.decidedAtUtc -notmatch "Z$") {
    $errors.Add("decidedAtUtc must be ISO-8601 UTC ending Z")
}
foreach ($field in @("decidedBy", "rationale", "sourceEvidencePath")) {
    $value = [string]$decision.$field
    if ($value -match "<|>|TODO|DRAFT|placeholder") { $errors.Add("$field contains placeholder/draft marker") }
}

$result = [ordered]@{
    schema_version = "1.0"
    artifact_scope = "legal_locale_scope_decision_validation"
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString("o")
    decisionPath = $DecisionPath
    passed = ($errors.Count -eq 0)
    includedLocales = $included
    excludedLocales = $excludedLocales
    publicLaunchAllowed = [bool]$decision.publicLaunchAllowed
    errors = @($errors)
}
$json = $result | ConvertTo-Json -Depth 10
if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
    $parent = Split-Path -Parent $OutputPath
    if (-not [string]::IsNullOrWhiteSpace($parent)) { New-Item -ItemType Directory -Force -Path $parent | Out-Null }
    $utf8NoBom = [System.Text.UTF8Encoding]::new($false)
    [System.IO.File]::WriteAllText($OutputPath, $json + "`n", $utf8NoBom)
}
Write-Output $json
if ($errors.Count -gt 0) { exit 1 }
