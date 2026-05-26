param(
    [string]$SourceMapPath = "",
    [string]$SourceRoot = "C:\Users\lubao\Documents\xiigen\legal",
    [string]$OutputPath = "",
    [switch]$FailOnPublicationBlockers
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..")
$defaultEvidenceRoot = Join-Path $repoRoot "docs\plans\enterprise-adaptation\v3.3\project-assets\legal-publication-completion"
if ([string]::IsNullOrWhiteSpace($SourceMapPath)) {
    $SourceMapPath = Join-Path $defaultEvidenceRoot "legal-draft-source-map.json"
}
if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $defaultEvidenceRoot "legal-draft-source-action-inventory.json"
}

if (-not (Test-Path -LiteralPath $SourceMapPath)) {
    throw "SourceMapPath not found: $SourceMapPath"
}
if (-not (Test-Path -LiteralPath $SourceRoot)) {
    throw "SourceRoot not found: $SourceRoot"
}

$sourceMap = Get-Content -Raw -LiteralPath $SourceMapPath | ConvertFrom-Json
$candidates = @($sourceMap.candidateSources)

$markerRules = @(
    [pscustomobject]@{ id = "draft-label"; pattern = "(?i)\bdraft\b|Version\s+1\.0\s+Draft"; severity = "BLOCK_PUBLICATION"; description = "Draft marker remains in source" },
    [pscustomobject]@{ id = "insert-placeholder"; pattern = "(?i)\[\s*insert|insert\s+date|insert\s+company|insert\s+name"; severity = "BLOCK_PUBLICATION"; description = "Insert placeholder remains in source" },
    [pscustomobject]@{ id = "generic-placeholder"; pattern = "(?i)\bplaceholder\b|\bTODO\b|\bTBD\b|\[.*?\]"; severity = "REVIEW_REQUIRED"; description = "Generic placeholder marker remains in source" },
    [pscustomobject]@{ id = "approval-marker"; pattern = "(?i)approvedBy|approvedAtUtc|LEGAL_APPROVED|counsel approved"; severity = "INFO_IF_PRESENT"; description = "Approval marker detector" }
)

$items = @()
$totalPlaceholderFindings = 0
$missingSources = 0
$draftSources = 0

foreach ($candidate in $candidates) {
    $candidatePath = [string]$candidate.candidatePath
    $sourceExists = -not [string]::IsNullOrWhiteSpace($candidatePath) -and (Test-Path -LiteralPath $candidatePath)
    $lineFindings = @()
    $lineCount = 0
    $hasApprovalMarker = $false

    if ($sourceExists) {
        $lines = Get-Content -LiteralPath $candidatePath
        $lineCount = @($lines).Count
        for ($i = 0; $i -lt $lines.Count; $i++) {
            $line = [string]$lines[$i]
            foreach ($rule in $markerRules) {
                if ($line -match $rule.pattern) {
                    if ($rule.id -eq "approval-marker") {
                        $hasApprovalMarker = $true
                        continue
                    }
                    $totalPlaceholderFindings++
                    $lineFindings += [pscustomobject][ordered]@{
                        ruleId = $rule.id
                        severity = $rule.severity
                        description = $rule.description
                        lineNumber = $i + 1
                        snippet = ($line.Trim() -replace "\s+", " ")
                    }
                }
            }
        }
    } else {
        $missingSources++
    }

    $candidateLocales = @($candidate.candidateLocales | ForEach-Object { [string]$_ })
    $requiredLocales = @($candidate.requiredLocales | ForEach-Object { [string]$_ })
    $missingLocales = @($requiredLocales | Where-Object { $_ -notin $candidateLocales })
    $blockers = @($candidate.blockers | ForEach-Object { [string]$_ })
    if (($blockers -join " ") -match "(?i)draft|placeholder|No approval|Missing") {
        $draftSources++
    }

    $resolvedCandidatePath = $null
    if ($sourceExists) {
        $resolvedCandidatePath = $candidatePath
    }

    if (-not $sourceExists) {
        $nextAction = "Create source, then localize en/he/ru and approve metadata before import."
    } elseif ($missingLocales.Count -gt 0 -or -not $hasApprovalMarker -or $lineFindings.Count -gt 0) {
        $nextAction = "Resolve line findings, produce approved metadata, localize missing locales, then feed Import-ApprovedLegalRecords.ps1."
    } else {
        $nextAction = "Candidate source is clean enough for approval packaging; still requires explicit approvedBy/approvedAtUtc record."
    }

    $items += [pscustomobject][ordered]@{
        documentType = [string]$candidate.documentType
        sourceStatus = [string]$candidate.sourceStatus
        candidatePath = $resolvedCandidatePath
        sourceExists = $sourceExists
        lineCount = $lineCount
        candidateLocales = $candidateLocales
        requiredLocales = $requiredLocales
        missingLocales = $missingLocales
        hasApprovalMarker = $hasApprovalMarker
        existingBlockers = $blockers
        placeholderFindings = @($lineFindings)
        nextAction = $nextAction
    }
}

$result = [ordered]@{
    schema_version = "1.0"
    artifact_scope = "legal_draft_source_action_inventory"
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString("o")
    sourceRoot = $SourceRoot
    sourceMapPath = $SourceMapPath
    requiredDocumentTypes = @($candidates).Count
    scannedSourceCount = @($items | Where-Object { $_.sourceExists }).Count
    missingSourceCount = $missingSources
    draftOrBlockedSourceCount = $draftSources
    placeholderFindingCount = $totalPlaceholderFindings
    readyForApprovedImport = $false
    canPublishPublicLegalRoutes = $false
    reason = "This inventory is an implementation aid only. It never approves legal text; publication remains blocked until approved EN/HE/RU records or a valid locale-scope decision are imported."
    items = @($items)
}

$json = ($result | ConvertTo-Json -Depth 12) -replace "`r`n", "`n" -replace "`r", "`n"
$parent = Split-Path -Parent $OutputPath
if (-not [string]::IsNullOrWhiteSpace($parent)) {
    New-Item -ItemType Directory -Force -Path $parent | Out-Null
}
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($OutputPath, ($json + "`n"), $utf8NoBom)
Write-Output $json

if ($FailOnPublicationBlockers -and ($missingSources -gt 0 -or $totalPlaceholderFindings -gt 0)) {
    exit 1
}
