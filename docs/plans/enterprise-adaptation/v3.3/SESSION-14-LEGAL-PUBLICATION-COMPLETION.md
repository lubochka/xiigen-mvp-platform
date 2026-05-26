# SESSION-14 - Legal Publication Completion

## Session type

LEGAL PUBLICATION / PUBLIC LAUNCH COMPLETION

## Parent plan

`XIIGEN-ENTERPRISE-ADAPTATION-MASTER-PLAN-v3.3.md`

## Goal

Complete the public legal launch lane by importing approved legal documents, publishing AWS legal routes, capturing visual evidence, and removing the public legal publication block.


## Current architecturally correct status

`C:/Users/lubao/Documents/xiigen/legal` is a useful draft intake source set, not an approved publication source. It is English-only, marked draft, contains placeholders, and carries no approval metadata. The exact Affiliate Program Terms source is missing. Hebrew and Russian approved localizations are missing.

The executor may use these files only to prepare the legal approval/localization lane. The executor must not mark any document `APPROVED` or `PUBLISHED`, must not set `goal_reached=true`, and must not close `LEGAL-PUBLIC-01` until approved final content and approval metadata exist.
## Binary acceptance criterion

LEGAL-PUBLIC-01 through LEGAL-PUBLIC-12 pass only with approved legal content and AWS route evidence. Governed placeholders do not pass these rows.

## Inputs

- Approved legal document source set for EN/HE/RU or explicit locale-scope decision.
- Current `legal-document-registry.json` and legal content model.
- AWS hosted endpoint from SESSION-02.

## Outputs

- `legal-approved-source-inventory.json`
- `legal-draft-source-inventory.json`
- updated `legal-document-registry.json`
- updated `legal-route-map.json`
- updated `legal-visual-evidence-index.json`
- `legal-publication-evidence.json`
- AWS screenshots/traces for public legal routes.

## Allowed verification

```powershell
rg -n "APPROVED|PUBLISHED|LEGAL_REVIEW_REQUIRED|legal|Terms|Privacy|DPA|EULA" docs/plans/enterprise-adaptation/v3.3/project-assets/legal-documents
Get-Content -Raw docs/plans/enterprise-adaptation/v3.3/project-assets/legal-documents/legal-document-registry.json | ConvertFrom-Json | Out-Null
npx playwright test <specific legal route spec>
```

## Forbidden verification

Do not run broad build/test commands unless Gate B opens them.
Do not invent legal approval.
Do not publish draft or review-required text as final.

## Gate

This session is complete only when public legal launch is no longer blocked and every LEGAL-PUBLIC row is PASS.

## Matrix update

LEGAL-PUBLIC-01 through LEGAL-PUBLIC-12.

## Approved source absent branch

If no approved legal source is found, the executor does not stop and does not fabricate approval. The executor must:

1. write `legal-draft-source-map.json` for all discovered draft sources;
2. update `legal-approved-source-inventory.json` with `approvedLegalDocumentsFound=false`;
3. keep `LEGAL-PUBLIC-01` through `LEGAL-PUBLIC-12` open;
4. set the next action to legal review/localization intake;
5. hand off to REVIEW_INTERNAL with evidence that no public publication claim was made.

This branch is progress, not completion. Public AWS legal publication still requires approved EN/HE/RU records or an explicit locale-scope decision.
