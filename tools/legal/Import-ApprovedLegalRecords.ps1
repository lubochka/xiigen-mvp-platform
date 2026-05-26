param(
    [Parameter(Mandatory = $true)]
    [string]$ApprovedRecordsPath,

    [string]$RegistryPath = "docs/plans/enterprise-adaptation/v3.3/project-assets/legal-documents/legal-document-registry.json",
    [string]$RouteMapPath = "docs/plans/enterprise-adaptation/v3.3/project-assets/legal-documents/legal-route-map.json",
    [string]$VisualEvidenceIndexPath = "docs/plans/enterprise-adaptation/v3.3/project-assets/legal-documents/legal-visual-evidence-index.json",
    [string]$PublicationEvidencePath = "docs/plans/enterprise-adaptation/v3.3/project-assets/legal-publication-completion/post-approval-import-evidence.json",
    [string]$ValidationOutputPath = "docs/plans/enterprise-adaptation/v3.3/project-assets/legal-publication-completion/legal-publication-record-validation.json",
    [string]$HostedBaseUrl = "https://gyxwjim5ea.eu-west-1.awsapprunner.com"
)

$ErrorActionPreference = "Stop"
$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path

function Resolve-RepoPath {
    param([Parameter(Mandatory = $true)][string]$Path)
    if ([System.IO.Path]::IsPathRooted($Path)) {
        return $Path
    }
    return (Join-Path $RepoRoot $Path)
}

function ConvertTo-JsonFile {
    param(
        [Parameter(Mandatory = $true)]$Value,
        [Parameter(Mandatory = $true)][string]$Path
    )

    $parent = Split-Path -Parent $Path
    if (-not [string]::IsNullOrWhiteSpace($parent)) {
        New-Item -ItemType Directory -Force -Path $parent | Out-Null
    }

    $json = ($Value | ConvertTo-Json -Depth 30) -replace "`r`n", "`n" -replace "`r", "`n"
    $utf8NoBom = [System.Text.UTF8Encoding]::new($false)
    [System.IO.File]::WriteAllText((Resolve-Path -LiteralPath $parent).Path + "\" + (Split-Path -Leaf $Path), $json + "`n", $utf8NoBom)
}

function Read-Json {
    param([Parameter(Mandatory = $true)][string]$Path)
    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Required JSON file not found: $Path"
    }
    Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
}

$validator = Join-Path $PSScriptRoot "Validate-LegalPublicationRecords.ps1"
$ApprovedRecordsPath = Resolve-RepoPath -Path $ApprovedRecordsPath
$RegistryPath = Resolve-RepoPath -Path $RegistryPath
$RouteMapPath = Resolve-RepoPath -Path $RouteMapPath
$VisualEvidenceIndexPath = Resolve-RepoPath -Path $VisualEvidenceIndexPath
$PublicationEvidencePath = Resolve-RepoPath -Path $PublicationEvidencePath
$ValidationOutputPath = Resolve-RepoPath -Path $ValidationOutputPath

& $validator -RecordsPath $ApprovedRecordsPath -OutputPath $ValidationOutputPath | Out-Null
if (-not $?) {
    throw "Approved legal records failed validation. See $ValidationOutputPath"
}
$validationResult = Read-Json -Path $ValidationOutputPath
if (-not [bool]$validationResult.passed) {
    throw "Approved legal records failed validation. See $ValidationOutputPath"
}

$approvedRecords = Read-Json -Path $ApprovedRecordsPath
if ($approvedRecords -isnot [System.Array]) {
    $approvedRecords = @($approvedRecords)
}
$registry = Read-Json -Path $RegistryPath
$routeMap = Read-Json -Path $RouteMapPath
$visualIndex = Read-Json -Path $VisualEvidenceIndexPath

$approvedByKey = @{}
foreach ($record in $approvedRecords) {
    $approvedByKey["$($record.documentType):$($record.locale)"] = $record
}

$updatedDocuments = @()
foreach ($document in @($registry.documents)) {
    $key = "$($document.documentType):$($document.locale)"
    if (-not $approvedByKey.ContainsKey($key)) {
        throw "Approved record set passed validation but does not contain key used by registry: $key"
    }

    $approved = $approvedByKey[$key]
    foreach ($property in $approved.PSObject.Properties) {
        $document | Add-Member -NotePropertyName $property.Name -NotePropertyValue $property.Value -Force
    }
    $updatedDocuments += $document
}

$registry.documents = $updatedDocuments
$registry.documentCount = @($updatedDocuments).Count
$registry.publicLaunchLegalGate = "APPROVED_RECORDS_IMPORTED_AWS_PUBLICATION_EVIDENCE_REQUIRED"
$registry.generatedAtUtc = [DateTimeOffset]::UtcNow.ToString("O")

$publishableByType = @{}
foreach ($document in $updatedDocuments | Where-Object { $_.locale -eq "en" }) {
    $publishableByType[$document.documentType] = $document
}

foreach ($route in @($routeMap.footerRoutes)) {
    if ($publishableByType.ContainsKey($route.documentType)) {
        $record = $publishableByType[$route.documentType]
        $route.route = "/legal/en/$($record.slug)"
        $route.canonicalUrl = "$HostedBaseUrl/legal/en/$($record.slug)"
        $route.published = $true
        $route.reason = "APPROVED_RECORD_IMPORTED_AWS_EVIDENCE_PENDING"
    }
}
$routeMap.publicationMode = "APPROVED_RECORDS_IMPORTED_AWS_EVIDENCE_PENDING"
$routeMap.generatedAtUtc = [DateTimeOffset]::UtcNow.ToString("O")

$localeStatus = @()
$screenshots = @()
foreach ($type in @($registry.requiredDocumentTypes)) {
    $byLocale = @{}
    foreach ($locale in @($registry.requiredLocales)) {
        $record = $approvedByKey["${type}:$locale"]
        $byLocale[$locale] = $record.status
        $screenshots += [ordered]@{
            documentType = $type
            locale = $locale
            url = "$HostedBaseUrl/legal/$locale/$($record.slug)"
            evidencePath = "docs/plans/enterprise-adaptation/v3.3/evidence/aws-runtime/legal/$locale-$($record.slug).png"
            requiredBeforePass = $true
            captured = $false
        }
    }
    $localeStatus += [ordered]@{
        documentType = $type
        en = $byLocale["en"]
        he = $byLocale["he"]
        ru = $byLocale["ru"]
        publicPublished = $true
    }
}

$visualIndex.generatedAtUtc = [DateTimeOffset]::UtcNow.ToString("O")
$visualIndex.publicRouteScreenshotsRequiredNow = @($screenshots).Count
$visualIndex.reason = "Approved records imported; AWS route screenshots are now required before public legal rows can pass."
$visualIndex.hebrewRtlGate = "REQUIRED_FOR_IMPORTED_HEBREW_LEGAL_ROUTES"
$visualIndex.localeStatus = $localeStatus
$visualIndex.screenshots = $screenshots

$evidence = [ordered]@{
    schema_version = "1.0"
    artifact_scope = "post_approval_legal_record_import"
    generatedAtUtc = [DateTimeOffset]::UtcNow.ToString("O")
    status = "PASS_APPROVED_RECORDS_IMPORTED_AWS_EVIDENCE_PENDING"
    approvedRecordsPath = $ApprovedRecordsPath
    validationOutputPath = $ValidationOutputPath
    registryPath = $RegistryPath
    routeMapPath = $RouteMapPath
    visualEvidenceIndexPath = $VisualEvidenceIndexPath
    importedRecordCount = @($approvedRecords).Count
    requiredRecordCount = 36
    publicLegalRowsPass = $false
    nextAction = "Deploy current platform API if needed, capture AWS route screenshots for all visualIndex.screenshots, then update LEGAL-PUBLIC rows."
}

ConvertTo-JsonFile -Value $registry -Path $RegistryPath
ConvertTo-JsonFile -Value $routeMap -Path $RouteMapPath
ConvertTo-JsonFile -Value $visualIndex -Path $VisualEvidenceIndexPath
ConvertTo-JsonFile -Value $evidence -Path $PublicationEvidencePath

Write-Output ($evidence | ConvertTo-Json -Depth 12)
