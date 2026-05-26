param(
    [string]$PlatformRepo = (Resolve-Path (Join-Path $PSScriptRoot "..\..")).Path,
    [string]$OutputPath = "docs/plans/enterprise-adaptation/v3.3/project-assets/legal-publication-completion/legal-publication-toolchain-test-evidence.json"
)

$ErrorActionPreference = "Stop"

function Resolve-RepoPath { param([string]$Path) if ([System.IO.Path]::IsPathRooted($Path)) { return $Path } return (Join-Path $PlatformRepo $Path) }
function Write-JsonFile { param($Value, [string]$Path) $parent=Split-Path -Parent $Path; if($parent){New-Item -ItemType Directory -Force -Path $parent|Out-Null}; $json=($Value|ConvertTo-Json -Depth 20) -replace "`r`n","`n" -replace "`r","`n"; $utf8NoBom=[System.Text.UTF8Encoding]::new($false); [System.IO.File]::WriteAllText((Join-Path (Resolve-Path -LiteralPath $parent).Path (Split-Path -Leaf $Path)), $json+"`n", $utf8NoBom) }
function Add-Result { param($Rows,[string]$Name,[bool]$Passed,[string]$Detail) $Rows.Add([ordered]@{ name=$Name; passed=$Passed; detail=$Detail; checkedAtUtc=(Get-Date).ToUniversalTime().ToString("o") }) }

$rows=[System.Collections.Generic.List[object]]::new()
$completionDir=Resolve-RepoPath "docs/plans/enterprise-adaptation/v3.3/project-assets/legal-publication-completion"

$draftInventoryOutput = Join-Path $completionDir "legal-draft-source-action-inventory.json"
& (Join-Path $PlatformRepo "tools/legal/Build-LegalDraftSourceInventory.ps1") -OutputPath $draftInventoryOutput | Out-Null
$draftInventoryData = Get-Content -Raw -LiteralPath $draftInventoryOutput | ConvertFrom-Json
Add-Result -Rows $rows -Name "draft-source-action-inventory" -Passed ($draftInventoryData.readyForApprovedImport -eq $false -and $draftInventoryData.canPublishPublicLegalRoutes -eq $false -and $draftInventoryData.requiredDocumentTypes -eq 12) -Detail "Line-level draft source inventory generated; it remains non-approving and publication-blocking by design."
$scaffoldOutput = Join-Path $completionDir "approved-legal-records.scaffold.json"
$scaffoldValidation = Join-Path $completionDir "legal-approved-records-scaffold-validation.negative.json"
& (Join-Path $PlatformRepo "tools/legal/Build-LegalApprovedRecordsScaffold.ps1") -OutputPath $scaffoldOutput | Out-Null
& (Join-Path $PlatformRepo "tools/legal/Validate-LegalPublicationRecords.ps1") -RecordsPath $scaffoldOutput -OutputPath $scaffoldValidation | Out-Null
$scaffoldValidationPassed = ($LASTEXITCODE -eq 0)
$scaffoldRecords = Get-Content -Raw -LiteralPath $scaffoldOutput | ConvertFrom-Json
$scaffoldRecordCount = $scaffoldRecords.Count
Add-Result -Rows $rows -Name "approved-records-scaffold-fails-closed" -Passed (-not $scaffoldValidationPassed -and $scaffoldRecordCount -eq 36) -Detail "36-record legal import scaffold generated and intentionally rejected until approval metadata/final localized copy are supplied."
$approvedTemplate=Join-Path $completionDir "approved-legal-record-template.json"
$approvedTemplateValidation=Join-Path $completionDir "legal-publication-record-validation.template-negative.json"
& (Join-Path $PlatformRepo "tools/legal/Validate-LegalPublicationRecords.ps1") -RecordsPath $approvedTemplate -OutputPath $approvedTemplateValidation | Out-Null
Add-Result -Rows $rows -Name "approved-template-fails-closed" -Passed (-not $?) -Detail "Template must fail validation because it is not approved content."

& (Join-Path $PlatformRepo "tools/legal/Test-LegalApprovedImportTool.ps1") | Out-Null
Add-Result -Rows $rows -Name "synthetic-approved-import-pipeline" -Passed ([bool]$?) -Detail "Synthetic 36-record import validates temp-only registry/route/visual updates."

$localeTemplate=Join-Path $completionDir "legal-locale-scope-decision-template.json"
$localeTemplateValidation=Join-Path $completionDir "legal-locale-scope-decision-validation.template-negative.json"
& (Join-Path $PlatformRepo "tools/legal/Validate-LegalLocaleScopeDecision.ps1") -DecisionPath $localeTemplate -OutputPath $localeTemplateValidation | Out-Null
Add-Result -Rows $rows -Name "locale-scope-template-fails-closed" -Passed (-not $?) -Detail "Template must fail validation because it lacks real approval metadata."

& (Join-Path $PlatformRepo "tools/legal/Test-LegalLocaleScopeDecisionValidator.ps1") | Out-Null
Add-Result -Rows $rows -Name "synthetic-locale-scope-validator" -Passed ([bool]$?) -Detail "Synthetic locale-scope decision validates positive path."

$deployPassed=$false
$deployDetail=""
try {
    & (Join-Path $PlatformRepo "tools/legal/Deploy-LegalAwsBlockedShell.ps1") -AwsAccountId "595003292091" -Region "eu-west-1" -AppRunnerServiceArn "arn:aws:apprunner:eu-west-1:595003292091:service/xiigen-platform-primary/a43b42b090ef4d66b33099863c28555d" -EcrRepository "xiigen/platform-api" -SkipDockerBuild -SkipDeploy -SkipVerification | Out-Null
    $deployDetail="Deploy dry-run unexpectedly reached AWS identity success."
}
catch {
    $deployDetail=$_.Exception.Message
    $deployPassed=($deployDetail -match "NoCredentials|Unable to locate credentials|Failed to resolve AWS credentials|no caller identity|exit code 253")
}
Add-Result -Rows $rows -Name "aws-deploy-fails-closed-without-credentials" -Passed $deployPassed -Detail $deployDetail

$passed=@($rows | Where-Object { -not $_.passed }).Count -eq 0
$result=[ordered]@{
    schema_version="1.0"
    artifact_scope="legal_publication_toolchain_test_evidence"
    generatedAtUtc=(Get-Date).ToUniversalTime().ToString("o")
    status=if($passed){"PASS_LEGAL_PUBLICATION_TOOLCHAIN"}else{"FAIL_LEGAL_PUBLICATION_TOOLCHAIN"}
    passed=$passed
    realLegalRowsPass=$false
    rows=@($rows)
}
Write-JsonFile -Value $result -Path (Resolve-RepoPath $OutputPath)
Write-Output ($result | ConvertTo-Json -Depth 20)
if(-not $passed){exit 1}
