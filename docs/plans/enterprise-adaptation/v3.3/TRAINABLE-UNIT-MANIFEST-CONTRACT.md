# Trainable Unit Manifest Contract — Enterprise v3.2

Every trainable unit is a project asset, not an embedded plugin helper.

Required fields:

```json
{
  "unitId": "",
  "unitType": "project-local-wiki | smart-code-assistant | routing | arbiter | reprocessor",
  "tenantId": "",
  "projectId": "",
  "version": "",
  "sourceGraphId": "",
  "checkpointReference": "",
  "checkpointChecksum": "",
  "locatorEntry": "",
  "exportPath": "",
  "importPath": "",
  "validationReportPath": "",
  "upgradeFrom": "",
  "upgradeTo": "",
  "noEmbeddedPluginWeights": true
}
```

A plugin can reference or load a unit manifest/checkpoint; it must not embed model weights.

## SESSION-04 registry paths

The project-local registry lives inside the master-tenant project assets:

```text
docs/plans/enterprise-adaptation/v3.3/project-assets/trainable-units/registry/trainable-unit-registry.json
```

The plugin/runtime locator file is:

```text
docs/plans/enterprise-adaptation/v3.3/project-assets/trainable-units/locators/plugin-unit-locator.json
```

The registry owns five first-version units:

| Unit suffix | Unit type | Purpose |
|---|---|---|
| `project-local-wiki` | `project-local-wiki` | Cited wiki answers over the linked Angular + .NET + SQL graph. |
| `smart-code-assistant` | `smart-code-assistant` | Tenant-scoped coding assistant over graph impact, policy and patch validation. |
| `routing` | `routing` | Prompt-to-unit routing without static helper decisions. |
| `arbiter` | `arbiter` | Evidence/policy review over answers and patches. |
| `reprocessor` | `reprocessor` | Graph-change reprocessing into wiki and assistant refresh tasks. |

Every unit has:

```text
manifests/{unit-suffix}.manifest.json
checkpoints/{unit-suffix}.checkpoint-reference.json
validation/{unit-suffix}.validation-report.json
export/{unit-suffix}.export.json
import/{unit-suffix}.import.json
```

Checkpoint files are references/checksums only. They are not embedded model weights. The checksum is computed over the checkpoint-reference metadata and the linked-system graph checksum. Runtime training metrics remain governed by the later Smart Wiki, Smart Code Assistant, prompt-suite and plugin-host sessions.

## No static helper rule

Semantic routing, code-assistant decisions, arbitration and reprocessing decisions must resolve through registry units and locator entries. A static helper may load a manifest or perform deterministic IO, but it must not replace a trainable/exportable unit for semantic decisions.
