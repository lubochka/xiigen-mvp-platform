# SESSION-13 — Legal Documents Site Integration

**Status:** required before public launch, affiliate launch, purchase launch, export download, or corporate install claim.  
**Depends on:** SESSION-00 governance acceptance and master-tenant launch runbook.  
**Contract:** `LEGAL-DOCUMENTS-SITE-INTEGRATION-CONTRACT.md`.

---

## Goal

Integrate legal documents into the official XIIGen Enterprise master-tenant website as versioned, approved, localized, linked and auditable site content.

Legal content must be part of the site and user flows, not external notes.

---

## Binary acceptance criterion

This session passes only when:

1. all required legal document types have content records;
2. each record has version/effective date/status/locale/canonical URL;
3. public legal pages are available from the AWS endpoint when published;
4. footer links point to the correct legal version;
5. signup/checkout/affiliate/export/install flows reference the correct legal documents;
6. required acceptance records persist document version and context;
7. screenshots/traces from the AWS endpoint prove legal routes and acceptance UI;
8. unapproved draft legal documents are not publicly published;
9. state and matrix rows `LEGAL-01..LEGAL-18` are updated from evidence.

---

## Inputs

- Legal document drafts or approved final texts.
- CMS/legal content model.
- Master tenant site routes.
- Localization requirements: EN / HE / RU minimum for public launch unless legal review explicitly scopes a locale later.
- AWS runtime endpoint evidence from SESSION-02 when runtime claim is made.

---

## Outputs

```text
legal-document-registry.json
legal-acceptance-context-map.json
legal-route-map.json
legal-visual-evidence-index.json
legal-publication-report.md
```

---

## Required implementation mapping

| Legal area | Site integration |
|---|---|
| Terms of Service | footer, signup, account creation |
| Privacy Policy | footer, signup, cookie banner, analytics notice |
| Cookie Policy | footer, cookie banner/preferences |
| EULA / License Agreement | pricing, checkout, download/install, activation |
| Enterprise Terms / MSA | enterprise contact/export/corporate install |
| DPA | privacy, enterprise/corporate tenant flow |
| Acceptable Use Policy | signup, workspace creation, affiliate |
| Affiliate Terms | affiliate apply/dashboard/training |
| Refund/Cancellation | pricing/checkout/account billing |
| Security/Responsible Disclosure | footer/security page |
| Subprocessors | privacy/DPA/infrastructure disclosure |
| Third-party/Open-source notices | download/install/export package |

---

## Allowed verification

Governance/content checks:

```powershell
rg -n "Terms of Service|Privacy Policy|Cookie Policy|Affiliate Terms|EULA|DPA|Refund|Subprocessors|Open Source" .
Get-Content -Raw <legal-registry>.json | ConvertFrom-Json | Out-Null
```

Runtime/visual checks only when SESSION-02 AWS endpoint exists and Gate for visual evidence is open:

```powershell
npx playwright test <specific legal route spec>
```

---

## Forbidden verification

No broad test/build commands in this session unless Gate B explicitly opens them:

```powershell
dotnet test
dotnet build
npm test
npm run build
```

No local-only screenshots for public legal launch claims.

---

## Gate

- `LEGAL-01..LEGAL-18` updated.
- Draft/unapproved docs cannot be published.
- Acceptance records include document version.
- AWS visual evidence proves public legal routes when public launch claim is made.
- Handoff to REVIEW_INTERNAL after slice.

---

## State update

Update:

```json
{
  "legalDocuments": {
    "registryPath": "",
    "routeMapPath": "",
    "acceptanceContextMapPath": "",
    "visualEvidenceIndexPath": "",
    "allRequiredDocsApproved": false,
    "publicLaunchLegalGate": "UNKNOWN"
  }
}
```

---

## Matrix update

Update rows:

```text
LEGAL-01..LEGAL-18
AWS-RUNTIME rows touched by legal visual evidence
G20A/G20B evidence scope rows
```
