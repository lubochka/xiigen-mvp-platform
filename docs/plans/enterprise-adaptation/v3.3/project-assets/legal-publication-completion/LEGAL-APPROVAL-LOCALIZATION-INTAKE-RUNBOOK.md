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

## Import sequence after approval exists

1. Copy each approved record from `approved-legal-record-template.json` into the legal registry import set.
2. Validate the records against `legal-approval-localization-checklist.json`.
3. Seed records through `ILegalPublicationService.UpsertDocumentAsync`.
4. Deploy the platform API to AWS.
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
