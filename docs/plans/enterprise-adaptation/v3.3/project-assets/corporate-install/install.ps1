param(
    [string]$PackageRoot = "../export-package/xiigen-primary-master-enterprise-export-v1.0.0/package-root",
    [string]$EvidenceRoot = "."
)

$ErrorActionPreference = "Stop"

function Read-Json([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path)) { throw "Missing required file: $Path" }
    return Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
}

function Get-Sha256([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path)) { throw "Missing hash target: $Path" }
    return (Get-FileHash -Algorithm SHA256 -LiteralPath $Path).Hash.ToLowerInvariant()
}

$manifest = Read-Json (Join-Path $PackageRoot "manifest.json")
$provenance = Read-Json (Join-Path $PackageRoot "provenance.json")
$hashes = Read-Json (Join-Path $PackageRoot "hashes.json")
$locator = Read-Json (Join-Path $PackageRoot "plugin-locators/plugin-unit-locator.json")
$unitRegistry = Read-Json (Join-Path $PackageRoot "trainable-units/registry/trainable-unit-registry.json")
$signature = Read-Json (Join-Path $PackageRoot "signature/signature.json")

if ($manifest.tenantId -ne "primary-master-tenant") { throw "Unexpected tenant: $($manifest.tenantId)" }
if (-not $signature.packageSha256) { throw "Signature package hash is missing" }

$hashFailures = @()
foreach ($entry in @($hashes.files)) {
    $target = Join-Path $PackageRoot $entry.path
    $actual = Get-Sha256 $target
    if ($actual -ne $entry.sha256) {
        $hashFailures += [pscustomobject]@{ path = $entry.path; expected = $entry.sha256; actual = $actual }
    }
}
if ($hashFailures.Count -gt 0) {
    throw ("Hash verification failed: " + ($hashFailures | ConvertTo-Json -Compress))
}

$secretPatterns = @(
    '-----BEGIN (RSA |EC |OPENSSH |DSA |)?PRIVATE KEY-----',
    'AKIA[0-9A-Z]{16}',
    '(?i)aws_secret_access_key\s*[:=]',
    '(?i)(github|ghp|gho|ghu|ghs|ghr)_[A-Za-z0-9_]{20,}',
    '(?i)stripe_(live|test)_[A-Za-z0-9]{20,}',
    '(?i)password\s*[:=]\s*[''"][^''"]{8,}[''"]'
)
$secretHits = @()
Get-ChildItem -LiteralPath $PackageRoot -Recurse -File | Where-Object { $_.Length -lt 5MB } | ForEach-Object {
    $text = Get-Content -Raw -LiteralPath $_.FullName -ErrorAction SilentlyContinue
    foreach ($pattern in $secretPatterns) {
        if ($text -match $pattern) {
            $secretHits += [pscustomobject]@{ path = $_.FullName; pattern = $pattern }
        }
    }
}
if ($secretHits.Count -gt 0) {
    throw ("Secret scan failed: " + ($secretHits | ConvertTo-Json -Compress))
}

$locatorEntries = @($locator.locators).Count
$unitCount = @($unitRegistry.units).Count
if ($locatorEntries -ne $unitCount) { throw "Locator count $locatorEntries does not match unit count $unitCount" }
if (@($locator.locators | Where-Object { -not $_.noEmbeddedPluginWeights }).Count -gt 0) { throw "Plugin locator embeds or permits plugin weights" }

$checkpointDir = Join-Path $PackageRoot "trainable-units/checkpoints"
$checkpointCount = @(Get-ChildItem -LiteralPath $checkpointDir -Filter "*.checkpoint-reference.json" -File).Count
if ($checkpointCount -ne $unitCount) { throw "Checkpoint count $checkpointCount does not match unit count $unitCount" }

$result = [pscustomobject]@{
    status = "PASS_CORPORATE_INSTALL_IMPORT_VERIFIED"
    tenantId = $manifest.tenantId
    projectId = $manifest.projectId
    packageVersion = $manifest.version
    provenanceId = $provenance.auditEventId
    hashEntries = @($hashes.files).Count
    locatorEntries = $locatorEntries
    unitCount = $unitCount
    checkpointCount = $checkpointCount
    noSecretsHits = 0
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString("o")
}

$result | ConvertTo-Json -Depth 10
