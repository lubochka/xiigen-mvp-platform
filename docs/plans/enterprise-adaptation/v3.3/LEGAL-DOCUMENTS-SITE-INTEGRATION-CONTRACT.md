# LEGAL-DOCUMENTS-SITE-INTEGRATION-CONTRACT

**Status:** binding contract for legal document integration into XIIGen Enterprise official site.  
**Scope:** master tenant website, affiliate flow, purchase flow, export package flow, corporate install flow.

---

## 1. Purpose

Legal documents must be integrated as governed site content, not as loose files.

A legal document is publishable only when:

1. it has a version;
2. it has an effective date;
3. it has an owner/reviewer;
4. it has approval status;
5. it has locale metadata;
6. it has canonical URL;
7. prior version reference exists when updated;
8. site surfaces link to the correct version;
9. acceptance records point to the exact version accepted;
10. AWS visual evidence proves the live URL and related UI surfaces.

---

## 2. Required first legal set

| ID | Document | Required before |
|---|---|---|
| LEGAL-DOC-01 | Terms of Service / Terms of Use | public signup, account creation |
| LEGAL-DOC-02 | Privacy Policy | public site launch, signup, analytics/cookies |
| LEGAL-DOC-03 | Cookie Policy / Cookie Notice | public website with cookies/analytics |
| LEGAL-DOC-04 | EULA / License Agreement | license purchase, download/install |
| LEGAL-DOC-05 | Enterprise Terms / MSA placeholder | corporate export/install or sales handoff |
| LEGAL-DOC-06 | Data Processing Agreement / DPA | corporate tenant data processing |
| LEGAL-DOC-07 | Acceptable Use Policy | signup, affiliate, tenant workspace |
| LEGAL-DOC-08 | Affiliate Program Terms | affiliate application/dashboard |
| LEGAL-DOC-09 | Refund / Cancellation Policy | pricing/checkout |
| LEGAL-DOC-10 | Security / Responsible Disclosure Policy | public footer/security page |
| LEGAL-DOC-11 | Subprocessors / Infrastructure Disclosure | privacy/DPA/corporate customers |
| LEGAL-DOC-12 | Open Source / Third-Party Notices | download/install/export package |

If a final lawyer-approved document is not available, publish state must remain `DRAFT` or `LEGAL_REVIEW_REQUIRED`; public routes must not pretend final approval.

---

## 3. Content model

```text
LegalDocument
  id
  slug
  documentType
  version
  status: DRAFT | LEGAL_REVIEW_REQUIRED | APPROVED | PUBLISHED | SUPERSEDED
  locale
  canonicalLocale
  title
  body
  summary
  effectiveDate
  approvedBy
  approvedAtUtc
  owner
  previousVersionId
  canonicalUrl
  requiredAcceptanceContexts[]
  relatedSiteSurfaces[]
  evidencePath
```

---

## 4. Required site surfaces

Legal links and acceptance must be wired into:

1. footer legal navigation;
2. signup/register page;
3. login/account creation acceptance history where applicable;
4. pricing/checkout;
5. license activation/download/install guide;
6. affiliate application;
7. affiliate dashboard/training if affiliate terms apply;
8. tenant adaptation workspace creation;
9. export package download;
10. corporate install/import screen;
11. cookie banner/preferences if cookies/analytics are used.

---

## 5. Acceptance records

When user acceptance is required, store:

```json
{
  "userId": "",
  "tenantId": "",
  "documentId": "",
  "documentType": "",
  "version": "",
  "locale": "",
  "acceptedAtUtc": "",
  "acceptanceContext": "signup|checkout|affiliateApply|exportDownload|corporateInstall",
  "ipHashOrAuditReference": "",
  "userAgentHashOrAuditReference": ""
}
```

Never store raw secrets or unnecessary personal data in public evidence.

---

## 6. Localization rule

Minimum public launch locales:

```text
en
he
ru
```

If a translation is missing:

- public page may fallback to English only if fallback is explicitly allowed;
- acceptance record must identify the actual locale shown;
- matrix row remains `UNKNOWN` until required locale is approved/published.

Hebrew pages must support RTL visual evidence.

---

## 7. AWS visual evidence rule

For every public legal route claim, evidence must include:

1. tested AWS `clientUrl` or `hostedBaseUrl`;
2. DNS/IP metadata at test time;
3. screenshot path;
4. timestamp UTC;
5. route path;
6. locale;
7. document type/version;
8. matrix row id.

Localhost screenshots do not satisfy public legal launch evidence.

---

## 8. Forbidden legal shortcuts

```text
Legal document in repo only = not published.
PDF in folder only = not integrated.
Footer link only = not acceptance flow.
Checkbox without version record = invalid acceptance.
Checkout without EULA/Refund link = incomplete.
Affiliate page without Affiliate Terms = incomplete.
Install/export without license/third-party notices = incomplete.
Draft document published as final = violation.
```
