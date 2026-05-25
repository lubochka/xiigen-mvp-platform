# AGENTS.md - XIIGen Enterprise Platform Agent Instructions

## Binding Enterprise Adaptation Beacon

Every session in this repository must read and obey:

1. `docs/plans/enterprise-adaptation/v3.3/XIIGEN-ENTERPRISE-ADAPTATION-BEACON-PROMPT-v1.md`
2. `docs/plans/enterprise-adaptation/v3.3/XIIGEN-ENTERPRISE-ADAPTATION-STATE-v3.3.json`
3. `docs/plans/enterprise-adaptation/v3.3/XIIGEN-ENTERPRISE-ADAPTATION-COMPLETION-MATRIX-v3.3.md`
4. The active `SESSION-XX-*.md` file for the current phase

The role loop is binding:

```text
PLAN_AUTHOR_INTERNAL
  -> PLAN_REVIEW_INTERNAL
  -> ARCHITECT_INTERNAL
  -> EXECUTOR_INTERNAL
  -> REVIEW_INTERNAL
  -> ARCHITECT_INTERNAL_REPAIR
  -> EXECUTOR_INTERNAL_CONTINUE
```

The executor may not self-finish. A slice result, test result, report, commit, push, evidence artifact, or matrix update must hand control to `REVIEW_INTERNAL`. Only an architect verdict can authorize final reporting, and only when `stop_allowed` is true and the completion matrix has no required `UNKNOWN` or `FAIL` rows.

## Master Tenant Launch

This repository is the main Enterprise platform repo for master-tenant launch governance. The startup runbook is:

```text
docs/plans/enterprise-adaptation/v3.3/MASTER-TENANT-LAUNCH-RUNBOOK-v1.md
```

The legal-document site integration contract is:

```text
docs/plans/enterprise-adaptation/v3.3/LEGAL-DOCUMENTS-SITE-INTEGRATION-CONTRACT.md
```

Customer tenant repositories, validation projects, exported packages, and plugin-only repositories are targets. They are not the launch control plane.

## No Secrets In Repo

Do not commit raw AWS access keys, GitHub tokens, Vault values, JWT signing keys, Docker registry tokens, tenant secret payloads, or private license HMAC secrets. Evidence may record proof-of-presence metadata and redacted identifiers only.

