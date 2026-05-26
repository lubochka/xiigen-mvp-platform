param(
    [string]$BaseUrl = "https://gyxwjim5ea.eu-west-1.awsapprunner.com",
    [string]$OutputPath = "docs/plans/enterprise-adaptation/v3.3/project-assets/legal-publication-completion/legal-aws-blocked-shell-evidence.json"
)

$ErrorActionPreference = "Stop"

$routes = @(
    @{ locale = "en"; slug = "terms-of-use"; requiresRtl = $false },
    @{ locale = "he"; slug = "terms-of-use"; requiresRtl = $true },
    @{ locale = "ru"; slug = "terms-of-use"; requiresRtl = $false }
)

$results = @()
foreach ($route in $routes) {
    $url = "$BaseUrl/legal/$($route.locale)/$($route.slug)"
    $statusCode = $null
    $body = ""
    $errorMessage = $null
    try {
        $response = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 20
        $statusCode = [int]$response.StatusCode
        $body = [string]$response.Content
    }
    catch {
        $errorMessage = $_.Exception.Message
        if ($_.Exception.Response) {
            $statusCode = [int]$_.Exception.Response.StatusCode
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            try { $body = $reader.ReadToEnd() } finally { $reader.Dispose() }
        }
    }

    $results += [ordered]@{
        url = $url
        locale = $route.locale
        slug = $route.slug
        statusCode = $statusCode
        bodyLength = $body.Length
        containsLegalReviewRequired = ($body -like "*LegalReviewRequired*")
        containsNotYetPublished = ($body -like "*not yet published*")
        containsFinalApprovedBody = ($body -like "*approved terms body*")
        containsRtl = ($body -like "*dir=`"rtl`"*")
        requiresRtl = [bool]$route.requiresRtl
        errorMessage = $errorMessage
    }
}

$blockedShellDeployed = @($results | Where-Object {
    $_.statusCode -eq 423 -and
    $_.containsLegalReviewRequired -eq $true -and
    $_.containsNotYetPublished -eq $true -and
    $_.containsFinalApprovedBody -eq $false
}).Count -eq $routes.Count

$rtlPass = @($results | Where-Object { $_.requiresRtl -eq $true -and $_.containsRtl -eq $true }).Count -eq @($results | Where-Object { $_.requiresRtl -eq $true }).Count

$result = [ordered]@{
    schema_version = "1.0"
    artifact_scope = "legal_aws_blocked_shell_evidence"
    generatedAtUtc = (Get-Date).ToUniversalTime().ToString("o")
    baseUrl = $BaseUrl
    status = if ($blockedShellDeployed -and $rtlPass) { "PASS_AWS_BLOCKED_SHELL_DEPLOYED_PUBLIC_LEGAL_STILL_BLOCKED" } else { "IN_PROGRESS_AWS_BLOCKED_SHELL_NOT_VERIFIED" }
    blockedShellDeployed = $blockedShellDeployed
    rtlEvidencePass = $rtlPass
    publicLegalRowsPass = $false
    reasonRowsDoNotPass = "Blocked-shell route evidence proves drafts are not exposed, but LEGAL-PUBLIC rows require approved legal content and final AWS visual evidence."
    routes = $results
}

$json = $result | ConvertTo-Json -Depth 8
$parent = Split-Path -Parent $OutputPath
if (-not [string]::IsNullOrWhiteSpace($parent)) {
    New-Item -ItemType Directory -Force -Path $parent | Out-Null
}
$json | Set-Content -LiteralPath $OutputPath -Encoding utf8
Write-Output $json
if (-not $blockedShellDeployed) {
    exit 1
}
