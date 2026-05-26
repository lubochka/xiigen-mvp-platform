param(
    [Parameter(Mandatory = $true)]
    [string]$RecordsPath,

    [string]$OutputPath = ""
)

$ErrorActionPreference = "Stop"

$requiredTypes = @(
    "terms_of_service",
    "privacy_policy",
    "cookie_policy",
    "eula_license",
    "enterprise_terms_msa",
    "dpa",
    "acceptable_use",
    "affiliate_terms",
    "refund_cancellation",
    "security_responsible_disclosure",
    "subprocessors",
    "open_source_notices"
)
$requiredLocales = @("en", "he", "ru")

if (-not (Test-Path -LiteralPath $RecordsPath)) {
    throw "RecordsPath not found: $RecordsPath"
}

$records = Get-Content -Raw -LiteralPath $RecordsPath | ConvertFrom-Json
if ($null -eq $records) {
    throw "RecordsPath did not parse as JSON: $RecordsPath"
}
if ($records -isnot [System.Array]) {
    $records = @($records)
}

$errors = New-Object System.Collections.Generic.List[string]
$seen = @{}

foreach ($record in $records) {
    $key = "$($record.documentType):$($record.locale)"
    $seen[$key] = $true

    foreach ($field in @("id", "slug", "documentType", "version", "status", "locale", "title", "body", "effectiveDate", "approvedBy", "approvedAtUtc", "canonicalUrl")) {
        if ([string]::IsNullOrWhiteSpace([string]$record.$field)) {
            $errors.Add("$key missing required field $field")
        }
    }

    if ($record.status -notin @("APPROVED", "PUBLISHED")) {
        $errors.Add("$key has non-publication status $($record.status)")
    }

    if ($record.locale -notin $requiredLocales) {
        $errors.Add("$key has unsupported locale $($record.locale)")
    }

    if ($record.documentType -notin $requiredTypes) {
        $errors.Add("$key has unsupported documentType $($record.documentType)")
    }

    $body = [string]$record.body
    if ([string]::IsNullOrWhiteSpace($body) -or $body -match "\[Insert|Insert Date|TODO|DRAFT|placeholder") {
        $errors.Add("$key body is empty or still contains draft/placeholder markers")
    }

    if ([string]$record.effectiveDate -notmatch "^\d{4}-\d{2}-\d{2}$") {
        $errors.Add("$key effectiveDate must match yyyy-MM-dd: $($record.effectiveDate)")
    }

    $approvedAt = [datetimeoffset]::MinValue
    if (-not [datetimeoffset]::TryParse([string]$record.approvedAtUtc, [ref]$approvedAt) -or [string]$record.approvedAtUtc -notmatch "Z$") {
        $errors.Add("$key approvedAtUtc must be ISO-8601 UTC ending Z: $($record.approvedAtUtc)")
    }

    if ([string]$record.canonicalUrl -notmatch "^https://[^/]+/legal/$($record.locale)/[a-z0-9-]+$") {
        $errors.Add("$key canonicalUrl must match https://<host>/legal/<locale>/<slug>: $($record.canonicalUrl)")
    }
}

foreach ($type in $requiredTypes) {
    foreach ($locale in $requiredLocales) {
        $key = "${type}:$locale"
        if (-not $seen.ContainsKey($key)) {
            $errors.Add("missing required record $key")
        }
    }
}

$result = [ordered]@{
    schema_version = "1.0"
    artifact_scope = "legal_publication_record_validation"
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString("o")
    recordsPath = $RecordsPath
    requiredRecordCount = 36
    actualRecordCount = @($records).Count
    passed = ($errors.Count -eq 0)
    errors = @($errors)
}

$json = $result | ConvertTo-Json -Depth 8
if (-not [string]::IsNullOrWhiteSpace($OutputPath)) {
    $parent = Split-Path -Parent $OutputPath
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        New-Item -ItemType Directory -Force -Path $parent | Out-Null
    }
    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($OutputPath, ($json + "`n"), $utf8NoBom)
}

Write-Output $json
if ($errors.Count -gt 0) {
    exit 1
}
