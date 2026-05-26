# LEGAL-PUBLICATION-COMPLETION-CONTRACT

Status: binding completion contract for public legal launch.

Public legal launch cannot pass from governed placeholders. It passes only when evidence proves:

1. all 12 required legal document types have lawyer-approved content;
2. EN, HE and RU records are approved or an explicit locale-scope decision exists;
3. each approved record has version, effective date, approvedBy, approvedAtUtc, canonical URL and final body;
4. legal routes are published on the AWS endpoint;
5. footer, signup, checkout, affiliate, export and corporate install surfaces link to the exact approved versions;
6. acceptance contexts persist document ID, version, locale and context;
7. Hebrew legal routes have RTL visual evidence;
8. no LEGAL_REVIEW_REQUIRED or DRAFT record is exposed as final;
9. screenshots/traces from the AWS endpoint prove the public legal route claims.

If approved legal copy is absent, executor must not fabricate approval. The next action remains legal content intake and publication evidence.

## Draft-source intake rule

Draft legal sources may be inventoried as `DRAFT_SOURCE_FOUND`, but they do not satisfy public launch requirements. A draft source record must keep public legal rows `IN_PROGRESS` until final lawyer approval, EN/HE/RU localization coverage or an explicit locale-scope decision, publication route evidence, and visual evidence are present.

Known SESSION-14 draft source intake: `C:/Users/lubao/Documents/xiigen/legal` contains English draft-only legal files with placeholders and no approval metadata. It is not a publication source.
