# CLAUDE.md - XIIGen Enterprise Platform Runtime Rules

## Beacon First

Before any execution, review, repair, report, commit, push, AWS action, or site implementation, read:

```text
docs/plans/enterprise-adaptation/v3.3/XIIGEN-ENTERPRISE-ADAPTATION-BEACON-PROMPT-v1.md
```

Then read state, matrix, and the active session file. Skills, local habits, and previous plan versions cannot override the beacon.

## Executor Cannot Stop The Plan

The executor must not decide that the plan is complete. The executor produces slice evidence and hands off to `REVIEW_INTERNAL`. If review finds a gap, control moves to `ARCHITECT_INTERNAL_REPAIR` and then back to `EXECUTOR_INTERNAL_CONTINUE` when the next action is present in the plan, state, or matrix.

Soft stops are invalid: commit done, report written, one child session passed, AWS URL produced, screenshot captured, plugin harness passed, or context getting long.

## Enterprise Scope

Target implementation repositories:

```text
github.com/lubochka/xiigen-platform-dot-net-angular
github.com/lubochka/xiigen-flows-dot-net-angular
```

Do not implement the Enterprise adaptation plan in `xiigen-mvp`, customer tenant forks, validation projects, exported package folders, or plugin-only repositories.

## Governance Startup

The master-tenant startup runbook is:

```text
docs/plans/enterprise-adaptation/v3.3/MASTER-TENANT-LAUNCH-RUNBOOK-v1.md
```

The legal documents contract is:

```text
docs/plans/enterprise-adaptation/v3.3/LEGAL-DOCUMENTS-SITE-INTEGRATION-CONTRACT.md
```

During governance acceptance, run only the commands explicitly allowed by the active session file. Broad build/test/deploy commands open only when the relevant session opens that gate.

