# SMART-CODE-ASSISTANT-CONTRACT.md

## Scope

Smart Code Assistant is the tenant-scoped trainable unit that plans and validates code changes under company coding and architecture policies before patch apply/export.

## Runtime Surfaces

| Surface | Path | Purpose |
|---|---|---|
| Assistant UI | `/assistant` | AWS-visible evidence surface for trainable unit, policy plan, patch validation, package locator and metrics |
| Unit creation | `POST /api/assistant/units` | Creates trainable/exportable/importable assistant unit evidence |
| Plan before patch | `POST /api/assistant/plans` | Requires coding and architecture policy before a patch can be planned |
| Patch validation | `POST /api/assistant/patch-validations` | Records affected files, procedures, tests, rationale and policy status |
| Package evidence | `POST /api/assistant/package-evidence` | Records export/import locator proof without embedded plugin weights |
| Metrics | `POST /api/assistant/metrics` | Records runtime metrics for the assistant unit |

## Policy Gate

The assistant cannot create a patch plan unless both tenant coding policy and architecture policy are present. Policy violations are escalated as `ESCALATED_POLICY_VIOLATION` and are not silently converted into patch-ready output.

## Evidence

SESSION-07 evidence is stored in:

- `docs/plans/enterprise-adaptation/v3.3/evidence/smart-code-assistant-evidence.json`
- `docs/plans/enterprise-adaptation/v3.3/evidence/smart-code-assistant-aws-runtime.json`
- `docs/plans/enterprise-adaptation/v3.3/evidence/aws-runtime/smart-code-assistant-aws.png`
