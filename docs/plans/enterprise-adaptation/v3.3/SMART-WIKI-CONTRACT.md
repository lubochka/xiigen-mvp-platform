# SMART-WIKI-CONTRACT.md

## Scope

Smart Wiki is the project-local knowledge surface over the linked Angular, .NET and SQL graph. It indexes only graph evidence, answers with citations, and abstains when a required source family is absent.

## Runtime Surfaces

| Surface | Path | Purpose |
|---|---|---|
| Smart Wiki UI | `/wiki` | AWS-visible evidence surface for linked knowledge, citations, abstention and reindex path |
| Index graph | `POST /api/wiki/index` | Stores linked graph nodes, edges and abstentions as tenant-local wiki data |
| Ask question | `POST /api/wiki/questions` | Answers approved advanced question kinds with citations or abstention |
| Reindex | `POST /api/wiki/reindex` | Records source graph checksum changes and reindex audit state |
| Read index | `GET /api/wiki/index/{projectId}` | Reads the tenant-scoped wiki index |

## Approved Question Kinds

| Question kind | Expected behavior |
|---|---|
| `entity-impact` | Cites matching graph nodes and edges for the named entity or collection |
| `flow-trace` | Cites route, controller, service, storage and event graph edges |
| `stored-procedure-impact` | Cites stored procedure evidence or returns `ABSTAIN_NO_SOURCE` |
| `angular-impact` | Cites Angular route/component/service graph records |
| `dotnet-impact` | Cites .NET controller/service/contract/storage records |
| `missing-evidence-abstention` | Explains abstention records instead of fabricating links |

## Authority Rule

Smart Wiki answers must use source graph authority. Unsupported model-memory answers are not valid evidence.

## Tenant Storage

The service stores wiki index, answer and reindex records through the platform document store with `tenant_id`. The index record references the project-local trainable unit registry:

`docs/plans/enterprise-adaptation/v3.3/project-assets/trainable-units/registry/trainable-unit-registry.json`

## Evidence

SESSION-06 evidence is stored in:

- `docs/plans/enterprise-adaptation/v3.3/evidence/smart-wiki-answer-evidence.json`
- `docs/plans/enterprise-adaptation/v3.3/evidence/smart-wiki-aws-runtime.json`
- `docs/plans/enterprise-adaptation/v3.3/evidence/aws-runtime/smart-wiki-aws.png`
