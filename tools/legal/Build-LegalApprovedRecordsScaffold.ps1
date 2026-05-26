param(
    [string]$WorkPackagePath = "",
    [string]$SourceMapPath = "",
    [string]$OutputPath = ""
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$completionRoot = Join-Path $repoRoot "docs\plans\enterprise-adaptation\v3.3\project-assets\legal-publication-completion"
if ([string]::IsNullOrWhiteSpace($WorkPackagePath)) {
    $WorkPackagePath = Join-Path $completionRoot "legal-approval-work-package.json"
}
if ([string]::IsNullOrWhiteSpace($SourceMapPath)) {
    $SourceMapPath = Join-Path $completionRoot "legal-draft-source-map.json"
}
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $completionRoot "approved-legal-records.scaffold.json"
}

if (-not (Test-Path -LiteralPath $WorkPackagePath)) {
    throw "WorkPackagePath not found: $WorkPackagePath"
}
if (-not (Test-Path -LiteralPath $SourceMapPath)) {
    throw "SourceMapPath not found: $SourceMapPath"
}

$workPackage = Get-Content -Raw -LiteralPath $WorkPackagePath | ConvertFrom-Json
$sourceMap = Get-Content -Raw -LiteralPath $SourceMapPath | ConvertFrom-Json
$sourceByType = @{}
foreach ($source in @($sourceMap.candidateSources)) {
    $sourceByType[[string]$source.documentType] = $source
}

function ConvertTo-Title {
    param([string]$DocumentType)
    return (($DocumentType -split "_" | ForEach-Object {
        if ($_.Length -eq 0) { $_ } else { $_.Substring(0,1).ToUpperInvariant() + $_.Substring(1) }
    }) -join " ")
}

function ConvertTo-Slug {
    param([string]$DocumentType)
    return ($DocumentType -replace "_","-").ToLowerInvariant()
}

$records = @()
foreach ($item in @($workPackage.workItems)) {
    $documentType = [string]$item.documentType
    $locale = [string]$item.locale
    $slug = ConvertTo-Slug $documentType
    $title = ConvertTo-Title $documentType
    $source = $sourceByType[$documentType]
    $candidatePath = $null
    if ($null -ne $source) {
        $candidatePath = [string]$source.candidatePath
    }

    $body = "DRAFT SCAFFOLD ONLY. Replace this body with approved final $locale legal copy for $documentType before import. Draft source path: $candidatePath"

    $records += [pscustomobject][ordered]@{
        id = "${documentType}:${locale}:DRAFT-SCAFFOLD"
        slug = $slug
        documentType = $documentType
        version = "DRAFT-SCAFFOLD"
        status = "LEGAL_REVIEW_REQUIRED"
        locale = $locale
        canonicalLocale = $locale
        title = $title
        body = $body
        summary = "DRAFT SCAFFOLD ONLY. Legal approval, localization, metadata, AWS screenshot, and import validation are still required."
        effectiveDate = "<yyyy-MM-dd>"
        approvedBy = ""
        approvedAtUtc = ""
        owner = "legal"
        previousVersionId = $null
        canonicalUrl = "https://gyxwjim5ea.eu-west-1.awsapprunner.com/legal/$locale/$slug"
        requiredAcceptanceContexts = @("signup", "checkout", "downloadInstall", "corporateInstall", "affiliateApply")
        relatedSiteSurfaces = @("footer", "signup", "checkout", "affiliate", "exportPackage", "corporateInstall")
        evidencePath = "docs/plans/enterprise-adaptation/v3.3/evidence/aws-runtime/legal/$locale-$slug.png"
        scaffoldMetadata = [ordered]@{
            sourcePath = $candidatePath
            sourceStatus = [string]$item.currentStatus
            requiredAction = [string]$item.requiredAction
            draftSourceUsableForLocale = [bool]$item.draftSourceUsableForLocale
            publicRowCanPass = $false
        }
    }
}

$parent = Split-Path -Parent $OutputPath
if (-not [string]::IsNullOrWhiteSpace($parent)) {
    New-Item -ItemType Directory -Force -Path $parent | Out-Null
}
$json = ($records | ConvertTo-Json -Depth 20) -replace "`r`n", "`n" -replace "`r", "`n"
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($OutputPath, ($json + "`n"), $utf8NoBom)

$summary = [ordered]@{
    schema_version = "1.0"
    artifact_scope = "approved_legal_records_scaffold_generation"
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString("o")
    outputPath = $OutputPath
    recordCount = @($records).Count
    readyForImport = $false
    canPublishPublicLegalRoutes = $false
    reason = "Scaffold records intentionally fail approved-record validation until legal approval metadata, final copy, and localization are supplied."
}
Write-Output (($summary | ConvertTo-Json -Depth 10) -replace "`r`n", "`n" -replace "`r", "`n")
