# LEGAL APPROVAL AND LOCALIZATION INTAKE RUNBOOK

Status: executable intake runbook. This file does not approve legal content.

## Purpose

Convert draft legal source material into importable public legal records only after legal approval and localization evidence exists.

## Source material

Draft source root: `C:/Users/lubao/Documents/xiigen/legal`.

The source root is draft-only. It may be used as review input, but it cannot be published as final content.

## Required intake package

1. 36 approved records: 12 document types x 3 locales.
2. Or an explicit locale-scope decision artifact reducing the launch locale scope.
3. `approvedBy`, `approvedAtUtc`, `effectiveDate`, `version`, `canonicalUrl`, and final `body` on every approved record.
4. Affiliate Program Terms source must exist before affiliate public launch rows can pass.
5. No `[Insert ...]` placeholders may remain.


## Validation artifacts

- `legal-approval-localization-checklist.json` is the 36-record checklist: 12 document types x EN/HE/RU.
- `approved-legal-record-validation-rules.json` is the fail-closed validation rule set for approved records.
- `approved-legal-record-template.json` is the per-record copy template used only after approval exists.
- `post-approval-import-evidence-template.json` is the evidence skeleton for the AWS publication run.

The first open requirement remains `LEGAL-PUBLIC-01` until all approved records, route maps, AWS responses, and visual evidence exist.
## Machine-readable approval work package

`legal-approval-work-package.json` is the canonical 36-item queue for legal approval/localization. It maps each required document type and locale to its draft source, missing source status, required action, and output record fields. Do not import or publish until every required work item has a corresponding approved record or an explicit locale-scope decision artifact.

## Locale-scope decision alternative

If legal/business approval narrows public launch to a subset of EN/HE/RU, create a concrete decision from `legal-locale-scope-decision-template.json` and validate it with `tools/legal/Validate-LegalLocaleScopeDecision.ps1 -DecisionPath <locale-scope-decision.json>`. The template itself fails validation by design; only an approval-backed decision may change launch locale scope.
## Import sequence after approval exists

1. Copy each approved record from `approved-legal-record-template.json` into the legal registry import set.
2. Validate the records against `legal-approval-localization-checklist.json` with `tools/legal/Validate-LegalPublicationRecords.ps1 -RecordsPath <approved-records.json> -OutputPath docs/plans/enterprise-adaptation/v3.3/project-assets/legal-publication-completion/legal-publication-record-validation.json`.
3. Import the approved set with `tools/legal/Import-ApprovedLegalRecords.ps1 -ApprovedRecordsPath <approved-records.json>`; the tool validates all 36 records before updating registry, route map, and visual evidence index.
4. Deploy the platform API to AWS with `tools/legal/Deploy-LegalAwsBlockedShell.ps1` or the existing enterprise deploy path.
5. Capture screenshots from AWS routes:
   - `/legal/en/<slug>`
   - `/legal/he/<slug>` with `dir="rtl"`
   - `/legal/ru/<slug>`
6. Update `legal-document-registry.json`, `legal-route-map.json`, `legal-visual-evidence-index.json`, and `legal-publication-completion-evidence.json`.
7. Only then move `LEGAL-PUBLIC-*` rows to PASS.

## Forbidden

- Do not mark draft text as approved.
- Do not infer lawyer approval from file presence.
- Do not publish `LEGAL_REVIEW_REQUIRED` or `DRAFT` records as final.
- Do not claim public legal launch from local-only smoke evidence.

## AWS blocked-shell verification before approval

After deploying the technical shell, run:

```powershell
tools/legal/Test-LegalAwsBlockedShell.ps1 -BaseUrl https://gyxwjim5ea.eu-west-1.awsapprunner.com -OutputPath docs/plans/enterprise-adaptation/v3.3/project-assets/legal-publication-completion/legal-aws-blocked-shell-evidence.json
```

A pass here proves only that unapproved drafts are not exposed. It does not close `LEGAL-PUBLIC-*` rows.
