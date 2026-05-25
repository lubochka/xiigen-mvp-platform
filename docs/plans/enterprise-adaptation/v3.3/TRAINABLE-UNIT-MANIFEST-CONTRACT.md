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
