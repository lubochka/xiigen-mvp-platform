# TENANT-ADAPTATION-STUDIO-CONTRACT.md

## Scope

Tenant Adaptation Studio is the primary tenant workspace where a company adapts the platform, its policies and its solution before export. The studio is implemented in the platform control plane and is tenant-scoped by `TenantContext`.

## Runtime Surfaces

| Surface | Path | Purpose |
|---|---|---|
| Studio UI | `/studio` | Tenant-facing route map and evidence surface for workspace, project source, policy and adaptation review |
| Create workspace | `POST /api/studio/workspaces` | Creates a corporate tenant workspace |
| Read workspace | `GET /api/studio/workspaces/{workspaceId}` | Reads workspace only inside the current tenant |
| Connect project source | `POST /api/studio/workspaces/{workspaceId}/sources` | Connects a repository URL or project snapshot |
| Capture policy | `POST /api/studio/workspaces/{workspaceId}/policies` | Captures company coding and architecture policies |
| Request adaptation | `POST /api/studio/workspaces/{workspaceId}/adaptations` | Creates an adaptation request with plan-before-patch |
| Review diff | `POST /api/studio/workspaces/{workspaceId}/adaptations/{adaptationId}/reviews` | Approves or rejects the reviewable diff before apply/export |
| Audit | `GET /api/studio/workspaces/{workspaceId}/audit` | Lists tenant-scoped audit events for the workspace |

## Authorization

Every Studio API endpoint requires an explicit `X-Roles` header. Accepted roles are `platform_admin`, `tenant_admin` and `tenant_ops`. Missing role evidence returns `401`; an explicit role outside the accepted set returns `403`.

The full JWT policy layer remains owned by the platform auth package, but this implementation does not silently grant Studio access from the local default context.

## Tenant Isolation

All workspace, source, policy, adaptation and audit records include `tenant_id`. Reads and writes validate the current `TenantContext.TenantId`; a workspace created under one tenant is returned as not found to another tenant.

## Plan-Before-Patch

An adaptation request creates:

- `status = PLAN_READY_REVIEW_REQUIRED`
- `planBeforePatch`
- `reviewableDiff`

No patch is applied by the request endpoint. Approval is recorded by the review endpoint and produces `APPROVED_FOR_APPLY_OR_EXPORT`; rejection produces `REJECTED_BY_TENANT`.

## Evidence

SESSION-05 evidence is stored in:

- `docs/plans/enterprise-adaptation/v3.3/evidence/tenant-adaptation-studio-capability-map.json`
- `docs/plans/enterprise-adaptation/v3.3/evidence/tenant-adaptation-studio-aws-runtime.json`
- `docs/plans/enterprise-adaptation/v3.3/evidence/aws-runtime/tenant-adaptation-studio-aws.png`
