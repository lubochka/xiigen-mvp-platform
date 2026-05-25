# LINKED-SYSTEM-GRAPH-CONTRACT.md

## Purpose

The linked system graph is the source-of-truth map used by the Enterprise Smart Wiki and Smart Code Assistant to answer impact questions across Angular, .NET and SQL surfaces. It records only links that can be traced to source evidence.

## Source Roots

| Source family | Root |
|---|---|
| Angular | `C:\Projects\xiigen-flows-dot-net-angular\flows\FLOW-19-durable-sagas-compliance\client\angular` |
| .NET API | `C:\Projects\xiigen-flows-dot-net-angular\flows\FLOW-19-durable-sagas-compliance\server\Xiigen.Flow.DurableSagasCompliance.Api` |
| .NET application | `C:\Projects\xiigen-flows-dot-net-angular\flows\FLOW-19-durable-sagas-compliance\server\Xiigen.Flow.DurableSagasCompliance.Application` |
| .NET infrastructure | `C:\Projects\xiigen-flows-dot-net-angular\flows\FLOW-19-durable-sagas-compliance\server\Xiigen.Flow.DurableSagasCompliance.Infrastructure` |
| Contracts | `C:\Projects\xiigen-flows-dot-net-angular\flows\FLOW-19-durable-sagas-compliance\contracts` |
| Flow manifest | `C:\Projects\xiigen-flows-dot-net-angular\flows\FLOW-19-durable-sagas-compliance\flow.manifest.json` |

## Node Types

The graph builder must support these node types:

| Node type | Required fields |
|---|---|
| `AngularRoute` | `nodeId`, `path`, `componentId`, `sourcePath`, `line` |
| `AngularComponent` | `nodeId`, `selector`, `sourcePath`, `templatePath`, `line` |
| `AngularService` | `nodeId`, `sourcePath`, `line`, `abstentionReason` when absent |
| `DotNetController` | `nodeId`, `route`, `sourcePath`, `line`, `controllerKind` |
| `DotNetService` | `nodeId`, `serviceClass`, `sourcePath`, `line` |
| `Contract` | `nodeId`, `contractKind`, `sourcePath`, `line` |
| `StoredProcedure` | `nodeId`, `sourcePath`, `line`, `abstentionReason` when absent |
| `Table` | `nodeId`, `logicalName`, `sourcePath`, `line`, `storageKind` |
| `View` | `nodeId`, `sourcePath`, `line`, `abstentionReason` when absent |
| `Job` | `nodeId`, `logicalName`, `sourcePath`, `line`, `jobKind` |
| `FlowManifest` | `nodeId`, `flowId`, `flowSlug`, `sourcePath`, `line` |

## Edge Types

The graph builder must support these edge types:

| Edge type | Meaning |
|---|---|
| `renders` | An Angular route renders an Angular component or a component renders template blocks. |
| `calls` | A handler, service or component calls another service/function. |
| `usesDto` | An endpoint or service depends on a request/response schema or result record. |
| `readsTable` | A service reads a logical collection/table/index. |
| `writesTable` | A service writes a logical collection/table/index. |
| `invokesProcedure` | A service invokes a SQL stored procedure. |
| `emitsEvent` | A service appends an outbox/CloudEvent. |
| `guardedByPolicy` | An endpoint or UI action is guarded by role/policy evidence. |

## Authority Priority

When two sources disagree, use this priority order:

1. Executable source code with line evidence.
2. Contract files under `contracts/`.
3. `flow.manifest.json`.
4. Runtime evidence captured from AWS.
5. Human-authored planning text.

## Abstention Rule

If a source family is required by the matrix but no matching source exists, the graph must emit an abstention node or evidence row with:

```json
{
  "status": "ABSTAIN_NO_SOURCE",
  "sourceFamily": "SQL stored procedures",
  "scanCommand": "rg -n \"CREATE PROCEDURE|CREATE PROC|stored procedure|DbContext\" <source-root>",
  "reason": "No source evidence found; graph must not invent a stored procedure link."
}
```

An abstention is a valid LGS evidence outcome. It is not a PASS for implementation of the absent source family; it is a PASS for graph honesty.

## Advanced Question Checklist

The graph must be able to answer or abstain with citations for:

1. Entity impact: which Angular screens, .NET endpoints, service methods, logical tables and events are affected by a named entity or collection.
2. Flow trace: how a user action travels from Angular route/component to .NET endpoint, service method, storage write and emitted event.
3. Stored procedure impact: which endpoints/services would be affected by a stored procedure change, or why no stored procedure exists.
4. Angular impact: which routes/components are affected by a service, endpoint, contract or role-policy change.
5. .NET impact: which services/endpoints/contracts/storage/event paths are affected by a UI or contract change.
6. Missing-evidence abstention: the answer must say `ABSTAIN_NO_SOURCE` when source evidence is absent.

## Evidence Output

`docs/plans/enterprise-adaptation/v3.3/evidence/linked-system-graph.json` is the canonical graph evidence for SESSION-03. It must include:

- `nodes`
- `edges`
- `sourceEvidence`
- `abstentions`
- `advancedQuestions`
- `matrixRowsClosed`

