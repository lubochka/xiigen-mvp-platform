param(
    [string]$PlatformRepo = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path,
    [string]$OutputPath = "docs/plans/enterprise-adaptation/v3.3/project-assets/legal-publication-completion/legal-approved-import-tool-test-evidence.json"
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
    $json = ($Value | ConvertTo-Json -Depth 30) -replace "`r`n", "`n" -replace "`r", "`n"
    $utf8NoBom = [System.Text.UTF8Encoding]::new($false)
    [System.IO.File]::WriteAllText((Join-Path (Resolve-Path -LiteralPath $parent).Path (Split-Path -Leaf $Path)), $json + "`n", $utf8NoBom)
}

function Read-Json {
    param([string]$Path)
    Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
}

$registryPath = Resolve-RepoPath "docs/plans/enterprise-adaptation/v3.3/project-assets/legal-documents/legal-document-registry.json"
$routeMapPath = Resolve-RepoPath "docs/plans/enterprise-adaptation/v3.3/project-assets/legal-documents/legal-route-map.json"
$visualIndexPath = Resolve-RepoPath "docs/plans/enterprise-adaptation/v3.3/project-assets/legal-documents/legal-visual-evidence-index.json"
$registry = Read-Json -Path $registryPath

$tempRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("xiigen-legal-import-test-" + [Guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Force -Path $tempRoot | Out-Null
$tempRegistryPath = Join-Path $tempRoot "legal-document-registry.json"
$tempRouteMapPath = Join-Path $tempRoot "legal-route-map.json"
$tempVisualIndexPath = Join-Path $tempRoot "legal-visual-evidence-index.json"
$tempValidationPath = Join-Path $tempRoot "legal-publication-record-validation.json"
$tempImportEvidencePath = Join-Path $tempRoot "post-approval-import-evidence.json"
$tempApprovedPath = Join-Path $tempRoot "approved-records.synthetic.json"
Copy-Item -LiteralPath $registryPath -Destination $tempRegistryPath
Copy-Item -LiteralPath $routeMapPath -Destination $tempRouteMapPath
Copy-Item -LiteralPath $visualIndexPath -Destination $tempVisualIndexPath

$approvedRecords = @()
foreach ($type in @($registry.requiredDocumentTypes)) {
    foreach ($locale in @($registry.requiredLocales)) {
        $slug = "$type-$locale" -replace "_", "-"
        $approvedRecords += [ordered]@{
            id = "$type`:$locale`:synthetic-v1"
            slug = $slug
            documentType = $type
            version = "synthetic-v1"
            status = "APPROVED"
            locale = $locale
            canonicalLocale = $locale
            title = "Synthetic approved $type $locale"
            body = "Synthetic final legal body for $type in $locale. This fixture exists only to validate the import pipeline."
            summary = "Synthetic import test summary."
            effectiveDate = "2026-05-26"
            approvedBy = "synthetic-test-legal-authority"
            approvedAtUtc = "2026-05-26T00:00:00Z"
            owner = "legal"
            previousVersionId = $null
            canonicalUrl = "https://example.test/legal/$locale/$slug"
            requiredAcceptanceContexts = @("syntheticImportTest")
            relatedSiteSurfaces = @("syntheticImportTest")
            evidencePath = "docs/plans/enterprise-adaptation/v3.3/evidence/aws-runtime/legal/$locale-$slug.png"
        }
    }
}
Write-JsonFile -Value $approvedRecords -Path $tempApprovedPath

$importer = Join-Path $PlatformRepo "tools/legal/Import-ApprovedLegalRecords.ps1"
& $importer `
    -ApprovedRecordsPath $tempApprovedPath `
    -RegistryPath $tempRegistryPath `
    -RouteMapPath $tempRouteMapPath `
    -VisualEvidenceIndexPath $tempVisualIndexPath `
    -PublicationEvidencePath $tempImportEvidencePath `
    -ValidationOutputPath $tempValidationPath `
    -HostedBaseUrl "https://example.test" | Out-Null
if (-not $?) { throw "Import-ApprovedLegalRecords.ps1 failed for synthetic fixture." }

$validation = Read-Json -Path $tempValidationPath
$importEvidence = Read-Json -Path $tempImportEvidencePath
$updatedRegistry = Read-Json -Path $tempRegistryPath
$updatedVisualIndex = Read-Json -Path $tempVisualIndexPath

$passed = [bool]$validation.passed -and
    [int]$validation.actualRecordCount -eq 36 -and
    [int]$importEvidence.importedRecordCount -eq 36 -and
    [int]$updatedRegistry.documentCount -eq 36 -and
    [int]$updatedVisualIndex.publicRouteScreenshotsRequiredNow -eq 36 -and
    -not [bool]$importEvidence.publicLegalRowsPass

$result = [ordered]@{
    schema_version = "1.0"
    artifact_scope = "legal_approved_import_tool_test_evidence"
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString("o")
    status = if ($passed) { "PASS_SYNTHETIC_IMPORT_PIPELINE" } else { "FAIL_SYNTHETIC_IMPORT_PIPELINE" }
    passed = $passed
    syntheticOnly = $true
    realLegalRowsPass = $false
    tempRoot = $tempRoot
    validationPassed = [bool]$validation.passed
    importedRecordCount = [int]$importEvidence.importedRecordCount
    registryDocumentCount = [int]$updatedRegistry.documentCount
    screenshotsRequired = [int]$updatedVisualIndex.publicRouteScreenshotsRequiredNow
    importer = "tools/legal/Import-ApprovedLegalRecords.ps1"
    validator = "tools/legal/Validate-LegalPublicationRecords.ps1"
}

Write-JsonFile -Value $result -Path (Resolve-RepoPath $OutputPath)
Write-Output ($result | ConvertTo-Json -Depth 12)
if (-not $passed) { exit 1 }
