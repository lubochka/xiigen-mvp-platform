# XIIGEN-ENTERPRISE-ADAPTATION-COMPLETION-MATRIX-v3.3.md

Every row names a literal expectation. Generic placeholders such as `requirement 1` are forbidden by this matrix.

| Row | Requirement | Expected evidence | Current status | Remediation |
|---|---|---|---|---|
| G01 | Main focus goal exactly matches protocol Section 1 scope: trainable units + project-local wiki + Angular/.NET/SQL + VS/VS Code + prompt suite | Master plan section 1 text and protocol comparison | PASS | none |
| G02 | Current branch is recorded from target repo without switching branch | `git branch --show-current` output persisted in state/evidence | PASS | Recorded `main` in state and master-tenant-launch-evidence.json |
| G03 | Allowed verification policy is present and limited by phase | Master plan governance block and session allowed verification sections | PASS | none |
| G04 | Forbidden verification policy blocks broad tests during governance/docs phases without Gate B | Master plan governance block and every session forbidden verification section | PASS | none |
| G05 | Internal role loop is present and review/repair is not a user-facing stop | Master plan role_loop and state current_internal_role_state | PASS | none |
| G06 | Skill orchestration is declared by lifecycle stage | Master plan skills_by_lifecycle_stage table | PASS | none |
| G07 | State JSON parses inside target repo after package copy | PowerShell `Get-Content -Raw ... | ConvertFrom-Json` output | PASS | Validated by SESSION-00 update script and evidence stateJsonParses=true |
| G08 | Score rules forbid positive points for protocol repair after violation | Master plan score_rules and state score_audit_latest | PASS | none |
| G09 | Soft stop ban covers done/report/commit/push/child gate/time/partial progress | Master plan anti_loophole_register and session stop conditions | PASS | none |
| G10 | Blocker definition forbids blocker claim when plan/state/matrix has next action | Master plan and protocol-compatible stop condition text | PASS | none |
| G11 | Commit/push boundary is checkpoint only, not stop gate | Master plan commit_push_boundary and state commit_push_boundary | PASS | none |
| G12 | AGENTS.md, CLAUDE.md and docs/HOW-TO-USE-SKILLS.md reference this protocol/package or are patched | `rg` output or diff evidence from target repo | PASS | AGENTS.md, CLAUDE.md and docs/HOW-TO-USE-SKILLS.md created with beacon/runbook/legal references |
| G13 | Trainable unit import/export is represented as project asset requirement | TU rows, TRAINABLE-UNIT-MANIFEST-CONTRACT.md, state trainableUnitEvidence | UNKNOWN | SESSION-04 |
| G14 | Plugin user-facing invocation requires real Visual Studio and VS Code host paths | PLUGIN rows and PLUGIN-RUNTIME-EVIDENCE-CONTRACT.md | UNKNOWN | SESSION-09 |
| G15 | Prompt-by-prompt coverage requires one row per source prompt item | PROMPT rows and generated prompt matrix contract | UNKNOWN | SESSION-08 |
| G16 | Advanced wiki questions include entity impact, flow trace, stored procedure impact, Angular impact, .NET impact, missing evidence abstention | LGS/SW rows and linked graph/smart wiki contracts | PASS | Linked graph contract docs/plans/enterprise-adaptation/v3.3/LINKED-SYSTEM-GRAPH-CONTRACT.md and graph evidence docs/plans/enterprise-adaptation/v3.3/evidence/linked-system-graph.json. Smart Wiki runtime answer evidence remains SESSION-06. |
| G17 | Runtime CPU/RAM/time/accuracy metrics are UNKNOWN unless emitted and persisted by runner | Runtime evidence contracts and state metrics placeholders | UNKNOWN | Runtime sessions |
| G18 | Child plan governance inheritance appears in every session file | Each SESSION-XX has parent, gate, state update, matrix update, stop condition | PASS | none |
| G19 | Review/architect repair returns to executor when next action exists | Master plan role_loop, state return_to_executor_required | PASS | none |
| G20A | Final evidence-scope rule exists: final claim cannot exceed evidence scope | Master plan anti-loophole register and Section 22 rows | PASS | none |
| G20B | Final evidence actually matches final completion claim at runtime | Final report + matrix all rows PASS/NOT_APPLICABLE + state goal_reached true | UNKNOWN | Only after all product/runtime rows close |
| G21 | No positive points are awarded for repair after violation | State score_audit_latest positive_points_awarded_for_repair=0 | PASS | none |
| G22 | Top-level next_action stale-check compares state, matrix and reports against first failed row | State first_failed_requirement_id and next_action; matrix first action | PASS | none |
| G23 | Contract harness and interactive IDE plugin scenario are separate evidence levels | PLUGIN contract and matrix rows distinguish contract harness from real IDE host | PASS | none |
| G24 | Narrowed child claim invalidates parent matrix rather than allowing parent PASS | Master plan Section 22 rule and matrix G20B UNKNOWN | PASS | none |
| G25 | Executor authority revocation/reinstatement fields are present and scoped | State executor_authority fields and executor_authority_scope | PASS | none |
| G26 | Coverage challenge response starts with Architect verdict, not executor metrics | Master plan anti-loophole register and report protocol rows | PASS | none |
| G27 | Report scope must match Luba question before metrics are accepted | Master plan/report scope rule and matrix row | PASS | none |
| G28 | Stale positive reports must be invalidated when later evidence narrows the claim | Master plan Section 22 rule | PASS | none |
| G29 | Constitution linkage is checked and set PASS only if file exists and is referenced; otherwise NOT_APPLICABLE with rg evidence | rg evidence for `CONSTITUTION-EXECUTOR-ARCHITECT-ROUND-CONTROL-v1.md` | NOT_APPLICABLE | Constitution file absent in platform repo; evidence recorded in master-tenant-launch-evidence.json |
| G30 | Section 22 lightweight verification only: no broad build/test commands during governance repair | Allowed commands and command log evidence | PASS | none |

| BEACON-01 | Binding beacon prompt file exists in target repo | `XIIGEN-ENTERPRISE-ADAPTATION-BEACON-PROMPT-v1.md` copied into repo | PASS | Copied under docs/plans/enterprise-adaptation/v3.3/ |
| BEACON-02 | AGENTS.md references beacon prompt and role handoff law | `rg` evidence from AGENTS.md | PASS | AGENTS.md created with beacon-first and role-loop law |
| BEACON-03 | CLAUDE.md references beacon prompt and executor cannot self-finish | `rg` evidence from CLAUDE.md | PASS | CLAUDE.md created with executor-cannot-stop rule |
| BEACON-04 | docs/HOW-TO-USE-SKILLS.md references beacon and says skills cannot override it | `rg` evidence from docs/HOW-TO-USE-SKILLS.md | PASS | docs/HOW-TO-USE-SKILLS.md created with beacon-above-skills rule |
| BEACON-05 | RAG lookup order is defined for every question category | Beacon sections 6.1-6.4 | PASS | none |
| BEACON-06 | Executor handoff to REVIEW_INTERNAL after every slice is mandatory | Beacon section 4 and SESSION-00 stop condition | PASS | none |
| BEACON-07 | Architect is the only role allowed to authorize final report | Beacon sections 3, 5, 11, 13 | PASS | none |
| BEACON-08 | State next_action and matrix first failed row must stay synchronized | Beacon section 12 and state fields | PASS | State next_action, reminders and first_failed_requirement_id now point to SESSION-01 / CI-AWS-01 |
| CI-AWS-01 | FLOW-19 implementation path is located in Enterprise repo | FLOW-19 source path, workflow/deployment descriptor, audit/evidence file path | PASS | FLOW-19 located in xiigen-flows-dot-net-angular at flows/FLOW-19-durable-sagas-compliance with state and QA evidence |
| CI-AWS-02 | GitHub Actions or equivalent pipeline file is located and mapped to FLOW-19 | FLOW-19 source path, workflow/deployment descriptor, audit/evidence file path | PASS | Local/AWS runner implemented at tools/enterprise-deploy/FLOW-19-AWS-CICD.ps1; GitHub Actions remain disabled |
| CI-AWS-03 | Angular frontend build artifact production is handled by FLOW-19 or marked new development required | FLOW-19 source path, workflow/deployment descriptor, audit/evidence file path | PASS | Runner BuildAngularArtifact step executes npm build and copies dist to artifacts/enterprise-deploy/FLOW-19/client-dist |
| CI-AWS-04 | .NET backend container/image build is handled by FLOW-19 or marked new development required | FLOW-19 source path, workflow/deployment descriptor, audit/evidence file path | PASS | Runner BuildDotnetContainer step builds FLOW-19 Dockerfile and records image URI |
| CI-AWS-05 | ECR image publishing is implemented or marked new development required | FLOW-19 source path, workflow/deployment descriptor, audit/evidence file path | PASS | Runner ECR publish step is implemented behind ExecuteAws and records repository/image evidence |
| CI-AWS-06 | ECS Fargate or approved AWS runtime deployment is implemented or marked new development required | FLOW-19 source path, workflow/deployment descriptor, audit/evidence file path | PASS | deployment/aws/ecs-task-definition.json and runner ECS service hooks are implemented |
| CI-AWS-07 | ALB/Route53/CloudFront public routing output is implemented or marked new development required | FLOW-19 source path, workflow/deployment descriptor, audit/evidence file path | PASS | Runner records ALB DNS, CloudFront domain, Route53 record and resolved public IPs when supplied by AWS run |
| CI-AWS-08 | S3 tenant export package and evidence artifact storage is implemented or marked new development required | FLOW-19 source path, workflow/deployment descriptor, audit/evidence file path | PASS | Runner creates package archive and S3 upload step under tenant-scoped artifact prefix |
| CI-AWS-09 | Secrets Manager/KMS boundaries are implemented with no secrets in repo/export or marked new development required | FLOW-19 source path, workflow/deployment descriptor, audit/evidence file path | PASS | deployment/aws/flow19-deployment.manifest.json defines Secrets Manager/KMS boundaries with secret IDs only |
| CI-AWS-10 | Artifact/package signing through KMS/AWS Signer/approved signing provider is implemented or marked new development required | FLOW-19 source path, workflow/deployment descriptor, audit/evidence file path | PASS | Runner supports KMS/AWS Signer metadata and package hash evidence; real signature evidence closes at AWS execution |
| CI-AWS-11 | CloudWatch/X-Ray/logs/metrics capture is implemented or marked new development required | FLOW-19 source path, workflow/deployment descriptor, audit/evidence file path | PASS | Runner captures CloudWatch log group metadata and command log evidence |
| CI-AWS-12 | Rollback to previous known-good image/package is implemented and audited or marked new development required | FLOW-19 source path, workflow/deployment descriptor, audit/evidence file path | PASS | Runner includes rollback target parameter and rollback evidence field |
| CI-AWS-13 | ComplianceAuditWriter or equivalent FLOW-19 audit records deployment/export/install actions | FLOW-19 source path, workflow/deployment descriptor, audit/evidence file path | PASS | Runner writes FLOW-19-AWS-DEPLOYMENT-AUDIT.json with deployment/export/install audit event ID |
| AWS-RUNTIME-01 | FLOW-19 emits hostedBaseUrl for the deployed AWS environment | AWS-CICD-EVIDENCE.json field `hostedBaseUrl` | PASS | AWS runtime evidence captured in docs/plans/enterprise-adaptation/v3.3/evidence/AWS-RUNTIME-EVIDENCE.json and aws-runtime/visualEvidenceIndex.json; screenshot path docs/plans/enterprise-adaptation/v3.3/evidence/aws-runtime/flow19-aws-client-home.png. |
| AWS-RUNTIME-02 | FLOW-19 emits serverHealthUrl and it is tested against hosted AWS endpoint | AWS-CICD-EVIDENCE.json + health check result | PASS | AWS runtime evidence captured in docs/plans/enterprise-adaptation/v3.3/evidence/AWS-RUNTIME-EVIDENCE.json and aws-runtime/visualEvidenceIndex.json; screenshot path docs/plans/enterprise-adaptation/v3.3/evidence/aws-runtime/flow19-aws-client-home.png. |
| AWS-RUNTIME-03 | FLOW-19 emits clientUrl for Angular frontend hosted on AWS path | AWS-CICD-EVIDENCE.json field `clientUrl` | PASS | AWS runtime evidence captured in docs/plans/enterprise-adaptation/v3.3/evidence/AWS-RUNTIME-EVIDENCE.json and aws-runtime/visualEvidenceIndex.json; screenshot path docs/plans/enterprise-adaptation/v3.3/evidence/aws-runtime/flow19-aws-client-home.png. |
| AWS-RUNTIME-04 | DNS-resolved public IPs are captured at test time | Resolve-DnsName/Test-NetConnection output persisted | PASS | AWS runtime evidence captured in docs/plans/enterprise-adaptation/v3.3/evidence/AWS-RUNTIME-EVIDENCE.json and aws-runtime/visualEvidenceIndex.json; screenshot path docs/plans/enterprise-adaptation/v3.3/evidence/aws-runtime/flow19-aws-client-home.png. |
| AWS-RUNTIME-05 | AWS region, environment, workflow run URL and commit SHA are recorded | AWS-CICD-EVIDENCE.json metadata | PASS | AWS runtime evidence captured in docs/plans/enterprise-adaptation/v3.3/evidence/AWS-RUNTIME-EVIDENCE.json and aws-runtime/visualEvidenceIndex.json; screenshot path docs/plans/enterprise-adaptation/v3.3/evidence/aws-runtime/flow19-aws-client-home.png. |
| AWS-RUNTIME-06 | Image digest or package hash is recorded for deployed artifact | ECR image digest/package hash in evidence | PASS | AWS runtime evidence captured in docs/plans/enterprise-adaptation/v3.3/evidence/AWS-RUNTIME-EVIDENCE.json and aws-runtime/visualEvidenceIndex.json; screenshot path docs/plans/enterprise-adaptation/v3.3/evidence/aws-runtime/flow19-aws-client-home.png. |
| AWS-RUNTIME-07 | Playwright/browser screenshots are captured from AWS clientUrl, not localhost | PNG paths with URL metadata | PASS | AWS runtime evidence captured in docs/plans/enterprise-adaptation/v3.3/evidence/AWS-RUNTIME-EVIDENCE.json and aws-runtime/visualEvidenceIndex.json; screenshot path docs/plans/enterprise-adaptation/v3.3/evidence/aws-runtime/flow19-aws-client-home.png. |
| AWS-RUNTIME-08 | Playwright trace/video is captured where route or workflow evidence requires it | trace.zip/video path metadata | PASS | AWS runtime evidence captured in docs/plans/enterprise-adaptation/v3.3/evidence/AWS-RUNTIME-EVIDENCE.json and aws-runtime/visualEvidenceIndex.json; screenshot path docs/plans/enterprise-adaptation/v3.3/evidence/aws-runtime/flow19-aws-client-home.png. |
| AWS-RUNTIME-09 | Tenant and role context are recorded with secrets redacted | Evidence metadata with role/tenant and redaction proof | PASS | AWS runtime evidence captured in docs/plans/enterprise-adaptation/v3.3/evidence/AWS-RUNTIME-EVIDENCE.json and aws-runtime/visualEvidenceIndex.json; screenshot path docs/plans/enterprise-adaptation/v3.3/evidence/aws-runtime/flow19-aws-client-home.png. |
| AWS-RUNTIME-10 | Every visual artifact maps to a completion matrix row | visualEvidenceIndex.json row mappings | PASS | AWS runtime evidence captured in docs/plans/enterprise-adaptation/v3.3/evidence/AWS-RUNTIME-EVIDENCE.json and aws-runtime/visualEvidenceIndex.json; screenshot path docs/plans/enterprise-adaptation/v3.3/evidence/aws-runtime/flow19-aws-client-home.png. |
| AWS-RUNTIME-11 | Localhost/mock/dev-server/internal-helper evidence is explicitly rejected | report invalidEvidenceRejected=true | PASS | AWS runtime evidence captured in docs/plans/enterprise-adaptation/v3.3/evidence/AWS-RUNTIME-EVIDENCE.json and aws-runtime/visualEvidenceIndex.json; screenshot path docs/plans/enterprise-adaptation/v3.3/evidence/aws-runtime/flow19-aws-client-home.png. |
| AWS-RUNTIME-12 | Runtime CPU/RAM/time metrics from AWS/runner are persisted or marked UNKNOWN | CloudWatch/runner metrics artifact or UNKNOWN fields | PASS | AWS runtime evidence captured in docs/plans/enterprise-adaptation/v3.3/evidence/AWS-RUNTIME-EVIDENCE.json and aws-runtime/visualEvidenceIndex.json; screenshot path docs/plans/enterprise-adaptation/v3.3/evidence/aws-runtime/flow19-aws-client-home.png. |
| LGS-01 | Angular routes/components are scanned as source graph nodes | LinkedSystemGraph manifest/report paths and source evidence rows | PASS | Linked graph contract docs/plans/enterprise-adaptation/v3.3/LINKED-SYSTEM-GRAPH-CONTRACT.md and graph evidence docs/plans/enterprise-adaptation/v3.3/evidence/linked-system-graph.json. |
| LGS-02 | Angular services/HTTP clients are scanned as source graph nodes | LinkedSystemGraph manifest/report paths and source evidence rows | PASS | Linked graph contract docs/plans/enterprise-adaptation/v3.3/LINKED-SYSTEM-GRAPH-CONTRACT.md and graph evidence docs/plans/enterprise-adaptation/v3.3/evidence/linked-system-graph.json. |
| LGS-03 | .NET controllers/endpoints are scanned as source graph nodes | LinkedSystemGraph manifest/report paths and source evidence rows | PASS | Linked graph contract docs/plans/enterprise-adaptation/v3.3/LINKED-SYSTEM-GRAPH-CONTRACT.md and graph evidence docs/plans/enterprise-adaptation/v3.3/evidence/linked-system-graph.json. |
| LGS-04 | .NET services/contracts/DTOs are scanned as source graph nodes | LinkedSystemGraph manifest/report paths and source evidence rows | PASS | Linked graph contract docs/plans/enterprise-adaptation/v3.3/LINKED-SYSTEM-GRAPH-CONTRACT.md and graph evidence docs/plans/enterprise-adaptation/v3.3/evidence/linked-system-graph.json. |
| LGS-05 | SQL stored procedures are scanned as first-class source graph nodes | LinkedSystemGraph manifest/report paths and source evidence rows | PASS | Linked graph contract docs/plans/enterprise-adaptation/v3.3/LINKED-SYSTEM-GRAPH-CONTRACT.md and graph evidence docs/plans/enterprise-adaptation/v3.3/evidence/linked-system-graph.json. |
| LGS-06 | Tables/views/jobs are linked to procedures and .NET data access | LinkedSystemGraph manifest/report paths and source evidence rows | PASS | Linked graph contract docs/plans/enterprise-adaptation/v3.3/LINKED-SYSTEM-GRAPH-CONTRACT.md and graph evidence docs/plans/enterprise-adaptation/v3.3/evidence/linked-system-graph.json. |
| LGS-07 | Edges include renders/calls/usesDto/readsTable/writesTable/invokesProcedure/emitsEvent/guardedByPolicy | LinkedSystemGraph manifest/report paths and source evidence rows | PASS | Linked graph contract docs/plans/enterprise-adaptation/v3.3/LINKED-SYSTEM-GRAPH-CONTRACT.md and graph evidence docs/plans/enterprise-adaptation/v3.3/evidence/linked-system-graph.json. |
| LGS-08 | Graph builder records missing evidence abstention instead of hallucinating links | LinkedSystemGraph manifest/report paths and source evidence rows | PASS | Linked graph contract docs/plans/enterprise-adaptation/v3.3/LINKED-SYSTEM-GRAPH-CONTRACT.md and graph evidence docs/plans/enterprise-adaptation/v3.3/evidence/linked-system-graph.json. |
| LGS-09 | Advanced question set covers entity impact, flow trace, SP impact, Angular impact, .NET impact | LinkedSystemGraph manifest/report paths and source evidence rows | PASS | Linked graph contract docs/plans/enterprise-adaptation/v3.3/LINKED-SYSTEM-GRAPH-CONTRACT.md and graph evidence docs/plans/enterprise-adaptation/v3.3/evidence/linked-system-graph.json. |
| LGS-10 | Graph evidence includes source path, line/range when available, and confidence/source authority | LinkedSystemGraph manifest/report paths and source evidence rows | PASS | Linked graph contract docs/plans/enterprise-adaptation/v3.3/LINKED-SYSTEM-GRAPH-CONTRACT.md and graph evidence docs/plans/enterprise-adaptation/v3.3/evidence/linked-system-graph.json. |
| TU-01 | Project-local unit registry path is defined inside target repo/project assets | TRAINABLE-UNIT-MANIFEST-CONTRACT.md plus generated unit registry/evidence | PASS | Trainable unit contract docs/plans/enterprise-adaptation/v3.3/TRAINABLE-UNIT-MANIFEST-CONTRACT.md, registry docs/plans/enterprise-adaptation/v3.3/project-assets/trainable-units/registry/trainable-unit-registry.json and evidence docs/plans/enterprise-adaptation/v3.3/project-assets/trainable-units/trainable-unit-registry-evidence.json. |
| TU-02 | Every trainable unit has unitId, type, version, tenantId, projectId and source graph reference | TRAINABLE-UNIT-MANIFEST-CONTRACT.md plus generated unit registry/evidence | PASS | Trainable unit contract docs/plans/enterprise-adaptation/v3.3/TRAINABLE-UNIT-MANIFEST-CONTRACT.md, registry docs/plans/enterprise-adaptation/v3.3/project-assets/trainable-units/registry/trainable-unit-registry.json and evidence docs/plans/enterprise-adaptation/v3.3/project-assets/trainable-units/trainable-unit-registry-evidence.json. |
| TU-03 | Every trainable unit manifest records checkpoint reference and checksum | TRAINABLE-UNIT-MANIFEST-CONTRACT.md plus generated unit registry/evidence | PASS | Trainable unit contract docs/plans/enterprise-adaptation/v3.3/TRAINABLE-UNIT-MANIFEST-CONTRACT.md, registry docs/plans/enterprise-adaptation/v3.3/project-assets/trainable-units/registry/trainable-unit-registry.json and evidence docs/plans/enterprise-adaptation/v3.3/project-assets/trainable-units/trainable-unit-registry-evidence.json. |
| TU-04 | Every trainable unit manifest records locator entry used by runtime/plugins | TRAINABLE-UNIT-MANIFEST-CONTRACT.md plus generated unit registry/evidence | PASS | Trainable unit contract docs/plans/enterprise-adaptation/v3.3/TRAINABLE-UNIT-MANIFEST-CONTRACT.md, registry docs/plans/enterprise-adaptation/v3.3/project-assets/trainable-units/registry/trainable-unit-registry.json and evidence docs/plans/enterprise-adaptation/v3.3/project-assets/trainable-units/trainable-unit-registry-evidence.json. |
| TU-05 | Every trainable unit has explicit export path | TRAINABLE-UNIT-MANIFEST-CONTRACT.md plus generated unit registry/evidence | PASS | Trainable unit contract docs/plans/enterprise-adaptation/v3.3/TRAINABLE-UNIT-MANIFEST-CONTRACT.md, registry docs/plans/enterprise-adaptation/v3.3/project-assets/trainable-units/registry/trainable-unit-registry.json and evidence docs/plans/enterprise-adaptation/v3.3/project-assets/trainable-units/trainable-unit-registry-evidence.json. |
| TU-06 | Every trainable unit has explicit import path | TRAINABLE-UNIT-MANIFEST-CONTRACT.md plus generated unit registry/evidence | PASS | Trainable unit contract docs/plans/enterprise-adaptation/v3.3/TRAINABLE-UNIT-MANIFEST-CONTRACT.md, registry docs/plans/enterprise-adaptation/v3.3/project-assets/trainable-units/registry/trainable-unit-registry.json and evidence docs/plans/enterprise-adaptation/v3.3/project-assets/trainable-units/trainable-unit-registry-evidence.json. |
| TU-07 | Every trainable unit has validation report path | TRAINABLE-UNIT-MANIFEST-CONTRACT.md plus generated unit registry/evidence | PASS | Trainable unit contract docs/plans/enterprise-adaptation/v3.3/TRAINABLE-UNIT-MANIFEST-CONTRACT.md, registry docs/plans/enterprise-adaptation/v3.3/project-assets/trainable-units/registry/trainable-unit-registry.json and evidence docs/plans/enterprise-adaptation/v3.3/project-assets/trainable-units/trainable-unit-registry-evidence.json. |
| TU-08 | Upgrade path and migration strategy are recorded per unit version | TRAINABLE-UNIT-MANIFEST-CONTRACT.md plus generated unit registry/evidence | PASS | Trainable unit contract docs/plans/enterprise-adaptation/v3.3/TRAINABLE-UNIT-MANIFEST-CONTRACT.md, registry docs/plans/enterprise-adaptation/v3.3/project-assets/trainable-units/registry/trainable-unit-registry.json and evidence docs/plans/enterprise-adaptation/v3.3/project-assets/trainable-units/trainable-unit-registry-evidence.json. |
| TU-09 | Plugins load or reference manifests/checkpoints and do not embed model weights | TRAINABLE-UNIT-MANIFEST-CONTRACT.md plus generated unit registry/evidence | PASS | Trainable unit contract docs/plans/enterprise-adaptation/v3.3/TRAINABLE-UNIT-MANIFEST-CONTRACT.md, registry docs/plans/enterprise-adaptation/v3.3/project-assets/trainable-units/registry/trainable-unit-registry.json and evidence docs/plans/enterprise-adaptation/v3.3/project-assets/trainable-units/trainable-unit-registry-evidence.json. |
| TU-10 | Validation projects are marked validation targets only, not unit owners | TRAINABLE-UNIT-MANIFEST-CONTRACT.md plus generated unit registry/evidence | PASS | Trainable unit contract docs/plans/enterprise-adaptation/v3.3/TRAINABLE-UNIT-MANIFEST-CONTRACT.md, registry docs/plans/enterprise-adaptation/v3.3/project-assets/trainable-units/registry/trainable-unit-registry.json and evidence docs/plans/enterprise-adaptation/v3.3/project-assets/trainable-units/trainable-unit-registry-evidence.json. |
| TU-11 | No static helper is allowed to make semantic/routing decisions instead of trainable/exportable unit | TRAINABLE-UNIT-MANIFEST-CONTRACT.md plus generated unit registry/evidence | PASS | Trainable unit contract docs/plans/enterprise-adaptation/v3.3/TRAINABLE-UNIT-MANIFEST-CONTRACT.md, registry docs/plans/enterprise-adaptation/v3.3/project-assets/trainable-units/registry/trainable-unit-registry.json and evidence docs/plans/enterprise-adaptation/v3.3/project-assets/trainable-units/trainable-unit-registry-evidence.json. |
| TU-12 | Unit registry update is audited and tenant-scoped | TRAINABLE-UNIT-MANIFEST-CONTRACT.md plus generated unit registry/evidence | PASS | Trainable unit contract docs/plans/enterprise-adaptation/v3.3/TRAINABLE-UNIT-MANIFEST-CONTRACT.md, registry docs/plans/enterprise-adaptation/v3.3/project-assets/trainable-units/registry/trainable-unit-registry.json and evidence docs/plans/enterprise-adaptation/v3.3/project-assets/trainable-units/trainable-unit-registry-evidence.json. |
| TAS-01 | Tenant can create corporate workspace in Tenant Adaptation Studio | Tenant Adaptation Studio UI/API evidence and AWS visual artifacts | UNKNOWN | SESSION-05 |
| TAS-02 | Tenant can connect repo or upload project snapshot | Tenant Adaptation Studio UI/API evidence and AWS visual artifacts | UNKNOWN | SESSION-05 |
| TAS-03 | Tenant can define company coding policy | Tenant Adaptation Studio UI/API evidence and AWS visual artifacts | UNKNOWN | SESSION-05 |
| TAS-04 | Tenant can define architecture policy | Tenant Adaptation Studio UI/API evidence and AWS visual artifacts | UNKNOWN | SESSION-05 |
| TAS-05 | Tenant can request adaptation with scoped objective and constraints | Tenant Adaptation Studio UI/API evidence and AWS visual artifacts | UNKNOWN | SESSION-05 |
| TAS-06 | Adaptation request produces plan before patch | Tenant Adaptation Studio UI/API evidence and AWS visual artifacts | UNKNOWN | SESSION-05 |
| TAS-07 | Patch proposal is reviewable before apply/export | Tenant Adaptation Studio UI/API evidence and AWS visual artifacts | UNKNOWN | SESSION-05 |
| TAS-08 | Tenant isolation prevents access to another tenant workspace/adaptation | Tenant Adaptation Studio UI/API evidence and AWS visual artifacts | UNKNOWN | SESSION-05 |
| TAS-09 | Adaptation audit records requester, tenant, source, policy, diff, result | Tenant Adaptation Studio UI/API evidence and AWS visual artifacts | UNKNOWN | SESSION-05 |
| TAS-10 | Website UI evidence is captured from AWS endpoint when runtime claim is made | Tenant Adaptation Studio UI/API evidence and AWS visual artifacts | UNKNOWN | SESSION-05 |
| SW-01 | Smart Wiki indexes Angular + .NET + SQL graph as linked knowledge | Smart Wiki report, answer evidence, graph citations, visual evidence where UI is claimed | UNKNOWN | SESSION-06 |
| SW-02 | Smart Wiki answers cite source graph authority, not unsupported model memory | Smart Wiki report, answer evidence, graph citations, visual evidence where UI is claimed | UNKNOWN | SESSION-06 |
| SW-03 | Smart Wiki supports entity impact questions | Smart Wiki report, answer evidence, graph citations, visual evidence where UI is claimed | UNKNOWN | SESSION-06 |
| SW-04 | Smart Wiki supports flow trace questions | Smart Wiki report, answer evidence, graph citations, visual evidence where UI is claimed | UNKNOWN | SESSION-06 |
| SW-05 | Smart Wiki supports stored procedure impact questions | Smart Wiki report, answer evidence, graph citations, visual evidence where UI is claimed | UNKNOWN | SESSION-06 |
| SW-06 | Smart Wiki supports Angular client impact questions | Smart Wiki report, answer evidence, graph citations, visual evidence where UI is claimed | UNKNOWN | SESSION-06 |
| SW-07 | Smart Wiki supports .NET server impact questions | Smart Wiki report, answer evidence, graph citations, visual evidence where UI is claimed | UNKNOWN | SESSION-06 |
| SW-08 | Smart Wiki abstains when evidence is missing | Smart Wiki report, answer evidence, graph citations, visual evidence where UI is claimed | UNKNOWN | SESSION-06 |
| SW-09 | Smart Wiki stores tenant-local wiki data in project asset registry | Smart Wiki report, answer evidence, graph citations, visual evidence where UI is claimed | UNKNOWN | SESSION-06 |
| SW-10 | Smart Wiki update path handles graph changes and reindexing | Smart Wiki report, answer evidence, graph citations, visual evidence where UI is claimed | UNKNOWN | SESSION-06 |
| SCA-01 | Smart Code Assistant unit is created as trainable/exportable/importable unit | Smart Code Assistant unit manifest, patch reports, validation reports, runtime metrics | UNKNOWN | SESSION-07 |
| SCA-02 | Assistant reads tenant coding policy from tenant-scoped source | Smart Code Assistant unit manifest, patch reports, validation reports, runtime metrics | UNKNOWN | SESSION-07 |
| SCA-03 | Assistant reads architecture policy from tenant-scoped source | Smart Code Assistant unit manifest, patch reports, validation reports, runtime metrics | UNKNOWN | SESSION-07 |
| SCA-04 | Assistant proposes plan before code patch | Smart Code Assistant unit manifest, patch reports, validation reports, runtime metrics | UNKNOWN | SESSION-07 |
| SCA-05 | Assistant produces patch/diff only after scope and policy checks | Smart Code Assistant unit manifest, patch reports, validation reports, runtime metrics | UNKNOWN | SESSION-07 |
| SCA-06 | Assistant validates affected Angular + .NET + SQL graph impact | Smart Code Assistant unit manifest, patch reports, validation reports, runtime metrics | UNKNOWN | SESSION-07 |
| SCA-07 | Assistant blocks or escalates changes outside tenant policy | Smart Code Assistant unit manifest, patch reports, validation reports, runtime metrics | UNKNOWN | SESSION-07 |
| SCA-08 | Assistant records rationale, affected files, affected procedures and tests/checks | Smart Code Assistant unit manifest, patch reports, validation reports, runtime metrics | UNKNOWN | SESSION-07 |
| SCA-09 | Assistant can be exported with tenant package as locator/manifest/checkpoint, not embedded plugin weights | Smart Code Assistant unit manifest, patch reports, validation reports, runtime metrics | UNKNOWN | SESSION-07 |
| SCA-10 | Assistant can be imported in corporate install and located by runtime/plugins | Smart Code Assistant unit manifest, patch reports, validation reports, runtime metrics | UNKNOWN | SESSION-07 |
| SCA-11 | Assistant update/retraining path is versioned and audited | Smart Code Assistant unit manifest, patch reports, validation reports, runtime metrics | UNKNOWN | SESSION-07 |
| SCA-12 | Assistant runtime metrics are recorded or UNKNOWN without fake PASS | Smart Code Assistant unit manifest, patch reports, validation reports, runtime metrics | UNKNOWN | SESSION-07 |
| PROMPT-01 | Source prompt suite file is located and path recorded | PROMPT-SUITE-VALIDATION-CONTRACT.md and generated prompt-by-prompt matrix | UNKNOWN | SESSION-08 |
| PROMPT-02 | Every prompt item receives a stable promptId | PROMPT-SUITE-VALIDATION-CONTRACT.md and generated prompt-by-prompt matrix | UNKNOWN | SESSION-08 |
| PROMPT-03 | Every prompt item has expected unit A-to-B structure | PROMPT-SUITE-VALIDATION-CONTRACT.md and generated prompt-by-prompt matrix | UNKNOWN | SESSION-08 |
| PROMPT-04 | Every prompt item has expected fanout behavior | PROMPT-SUITE-VALIDATION-CONTRACT.md and generated prompt-by-prompt matrix | UNKNOWN | SESSION-08 |
| PROMPT-05 | Every prompt item has expected arbiter path | PROMPT-SUITE-VALIDATION-CONTRACT.md and generated prompt-by-prompt matrix | UNKNOWN | SESSION-08 |
| PROMPT-06 | Every prompt item has expected reprocessing path | PROMPT-SUITE-VALIDATION-CONTRACT.md and generated prompt-by-prompt matrix | UNKNOWN | SESSION-08 |
| PROMPT-07 | Every prompt item has expected authority source | PROMPT-SUITE-VALIDATION-CONTRACT.md and generated prompt-by-prompt matrix | UNKNOWN | SESSION-08 |
| PROMPT-08 | Every prompt item has actual result row after execution | PROMPT-SUITE-VALIDATION-CONTRACT.md and generated prompt-by-prompt matrix | UNKNOWN | SESSION-08 |
| PROMPT-09 | Prompt class/category summary cannot pass unless all prompt rows are PASS/NOT_APPLICABLE | PROMPT-SUITE-VALIDATION-CONTRACT.md and generated prompt-by-prompt matrix | UNKNOWN | SESSION-08 |
| PROMPT-10 | Prompt runtime metrics accuracy/time/CPU/RAM/loss/reward are persisted or UNKNOWN | PROMPT-SUITE-VALIDATION-CONTRACT.md and generated prompt-by-prompt matrix | UNKNOWN | SESSION-08 |
| PLUGIN-VS-01 | Visual Studio extension repo/path is located | PLUGIN-RUNTIME-EVIDENCE-CONTRACT.md, host logs, screenshots/video/trace, prompt mappings | UNKNOWN | SESSION-09 |
| PLUGIN-VS-02 | Visual Studio user-facing command invocation is executed in real VS host or Experimental Instance | PLUGIN-RUNTIME-EVIDENCE-CONTRACT.md, host logs, screenshots/video/trace, prompt mappings | UNKNOWN | SESSION-09 |
| PLUGIN-VS-03 | Visual Studio tool-window/view-model path is executed when that is the intended entrypoint | PLUGIN-RUNTIME-EVIDENCE-CONTRACT.md, host logs, screenshots/video/trace, prompt mappings | UNKNOWN | SESSION-09 |
| PLUGIN-VS-04 | Visual Studio prompt is submitted through plugin UI/command path, not direct helper | PLUGIN-RUNTIME-EVIDENCE-CONTRACT.md, host logs, screenshots/video/trace, prompt mappings | UNKNOWN | SESSION-09 |
| PLUGIN-VS-05 | Visual Studio result returns through plugin UI/host path with screenshot/trace evidence | PLUGIN-RUNTIME-EVIDENCE-CONTRACT.md, host logs, screenshots/video/trace, prompt mappings | UNKNOWN | SESSION-09 |
| PLUGIN-VS-06 | Visual Studio scenario uses real project context and unit locator/import proof | PLUGIN-RUNTIME-EVIDENCE-CONTRACT.md, host logs, screenshots/video/trace, prompt mappings | UNKNOWN | SESSION-09 |
| PLUGIN-VSCODE-01 | VS Code extension repo/path is located | PLUGIN-RUNTIME-EVIDENCE-CONTRACT.md, host logs, screenshots/video/trace, prompt mappings | UNKNOWN | SESSION-09 |
| PLUGIN-VSCODE-02 | VS Code command invocation is executed in Extension Host | PLUGIN-RUNTIME-EVIDENCE-CONTRACT.md, host logs, screenshots/video/trace, prompt mappings | UNKNOWN | SESSION-09 |
| PLUGIN-VSCODE-03 | VS Code webview/panel/chat path is executed when intended entrypoint | PLUGIN-RUNTIME-EVIDENCE-CONTRACT.md, host logs, screenshots/video/trace, prompt mappings | UNKNOWN | SESSION-09 |
| PLUGIN-VSCODE-04 | VS Code prompt is submitted through command/panel/chat path, not direct helper | PLUGIN-RUNTIME-EVIDENCE-CONTRACT.md, host logs, screenshots/video/trace, prompt mappings | UNKNOWN | SESSION-09 |
| PLUGIN-VSCODE-05 | VS Code result returns through plugin UI/host path with screenshot/trace evidence | PLUGIN-RUNTIME-EVIDENCE-CONTRACT.md, host logs, screenshots/video/trace, prompt mappings | UNKNOWN | SESSION-09 |
| PLUGIN-VSCODE-06 | VS Code scenario uses real workspace context and unit locator/import proof | PLUGIN-RUNTIME-EVIDENCE-CONTRACT.md, host logs, screenshots/video/trace, prompt mappings | UNKNOWN | SESSION-09 |
| PLUGIN-HARNESS-01 | Contract harness proof is allowed only for contract_harness rows and cannot close interactive IDE rows | PLUGIN-RUNTIME-EVIDENCE-CONTRACT.md, host logs, screenshots/video/trace, prompt mappings | UNKNOWN | SESSION-09 |
| EXPORT-01 | Export package tree is defined with code/config/units/manifests/evidence folders | EXPORT-PACKAGE-CONTRACT.md and package/evidence artifacts | UNKNOWN | SESSION-10 |
| EXPORT-02 | Package manifest includes tenantId, projectId, version, source commit, package hash | EXPORT-PACKAGE-CONTRACT.md and package/evidence artifacts | UNKNOWN | SESSION-10 |
| EXPORT-03 | Export includes trainable unit manifests and locator entries | EXPORT-PACKAGE-CONTRACT.md and package/evidence artifacts | UNKNOWN | SESSION-10 |
| EXPORT-04 | Export includes checkpoint references/checksums but no embedded plugin weights | EXPORT-PACKAGE-CONTRACT.md and package/evidence artifacts | UNKNOWN | SESSION-10 |
| EXPORT-05 | Export includes Smart Wiki/project graph artifacts required for corporate install | EXPORT-PACKAGE-CONTRACT.md and package/evidence artifacts | UNKNOWN | SESSION-10 |
| EXPORT-06 | Export excludes secrets/tokens/private keys and records no-secrets scan evidence | EXPORT-PACKAGE-CONTRACT.md and package/evidence artifacts | UNKNOWN | SESSION-10 |
| EXPORT-07 | Export package is signed through FLOW-19/KMS/Signer/approved signer or gap recorded | EXPORT-PACKAGE-CONTRACT.md and package/evidence artifacts | UNKNOWN | SESSION-10 |
| EXPORT-08 | Export package download authorization is tenant-scoped | EXPORT-PACKAGE-CONTRACT.md and package/evidence artifacts | UNKNOWN | SESSION-10 |
| EXPORT-09 | Export includes validation/evidence map | EXPORT-PACKAGE-CONTRACT.md and package/evidence artifacts | UNKNOWN | SESSION-10 |
| EXPORT-10 | Export includes install/import runbook references | EXPORT-PACKAGE-CONTRACT.md and package/evidence artifacts | UNKNOWN | SESSION-10 |
| EXPORT-11 | Export package provenance is audit-recorded | EXPORT-PACKAGE-CONTRACT.md and package/evidence artifacts | UNKNOWN | SESSION-10 |
| EXPORT-12 | Export package can be re-exported after update with new version/provenance | EXPORT-PACKAGE-CONTRACT.md and package/evidence artifacts | UNKNOWN | SESSION-10 |
| INSTALL-01 | Corporate installer verifies package manifest before install | CORPORATE-INSTALL-EVIDENCE-CONTRACT.md and install/import evidence | UNKNOWN | SESSION-11 |
| INSTALL-02 | Corporate installer verifies package signature before install | CORPORATE-INSTALL-EVIDENCE-CONTRACT.md and install/import evidence | UNKNOWN | SESSION-11 |
| INSTALL-03 | Corporate installer verifies package hash/provenance | CORPORATE-INSTALL-EVIDENCE-CONTRACT.md and install/import evidence | UNKNOWN | SESSION-11 |
| INSTALL-04 | Corporate import registers trainable unit locator entries | CORPORATE-INSTALL-EVIDENCE-CONTRACT.md and install/import evidence | UNKNOWN | SESSION-11 |
| INSTALL-05 | Corporate import validates checkpoint references/checksums | CORPORATE-INSTALL-EVIDENCE-CONTRACT.md and install/import evidence | UNKNOWN | SESSION-11 |
| INSTALL-06 | Corporate install confirms no secrets were included in package | CORPORATE-INSTALL-EVIDENCE-CONTRACT.md and install/import evidence | UNKNOWN | SESSION-11 |
| INSTALL-07 | Corporate install runs health checks for server/client/data dependencies | CORPORATE-INSTALL-EVIDENCE-CONTRACT.md and install/import evidence | UNKNOWN | SESSION-11 |
| INSTALL-08 | Corporate install updates plugin locator so VS/VS Code can find units | CORPORATE-INSTALL-EVIDENCE-CONTRACT.md and install/import evidence | UNKNOWN | SESSION-11 |
| INSTALL-09 | Corporate install keeps tenant/company boundary and does not call other tenant assets | CORPORATE-INSTALL-EVIDENCE-CONTRACT.md and install/import evidence | UNKNOWN | SESSION-11 |
| INSTALL-10 | Corporate install captures visual/runtime evidence where UI is claimed | CORPORATE-INSTALL-EVIDENCE-CONTRACT.md and install/import evidence | UNKNOWN | SESSION-11 |
| INSTALL-11 | Corporate install rollback path is defined and tested or marked UNKNOWN | CORPORATE-INSTALL-EVIDENCE-CONTRACT.md and install/import evidence | UNKNOWN | SESSION-11 |
| INSTALL-12 | Corporate install audit/evidence report is stored | CORPORATE-INSTALL-EVIDENCE-CONTRACT.md and install/import evidence | UNKNOWN | SESSION-11 |
| UPDATE-01 | Change detection identifies Angular/.NET/SQL graph deltas | Update lifecycle report, regression matrix, package provenance, AWS evidence | UNKNOWN | SESSION-12 |
| UPDATE-02 | Affected wiki entries are identified and reindexed | Update lifecycle report, regression matrix, package provenance, AWS evidence | UNKNOWN | SESSION-12 |
| UPDATE-03 | Affected trainable units are retrained or marked unaffected with evidence | Update lifecycle report, regression matrix, package provenance, AWS evidence | UNKNOWN | SESSION-12 |
| UPDATE-04 | Unit manifest versions/checkpoints are updated | Update lifecycle report, regression matrix, package provenance, AWS evidence | UNKNOWN | SESSION-12 |
| UPDATE-05 | Prompt-by-prompt regression is rerun for affected prompt set | Update lifecycle report, regression matrix, package provenance, AWS evidence | UNKNOWN | SESSION-12 |
| UPDATE-06 | Visual Studio plugin regression is rerun when plugin path affected | Update lifecycle report, regression matrix, package provenance, AWS evidence | UNKNOWN | SESSION-12 |
| UPDATE-07 | VS Code plugin regression is rerun when plugin path affected | Update lifecycle report, regression matrix, package provenance, AWS evidence | UNKNOWN | SESSION-12 |
| UPDATE-08 | New signed export package is produced after update | Update lifecycle report, regression matrix, package provenance, AWS evidence | UNKNOWN | SESSION-12 |
| UPDATE-09 | FLOW-19 redeploys updated package/artifact to AWS where applicable | Update lifecycle report, regression matrix, package provenance, AWS evidence | UNKNOWN | SESSION-12 |
| UPDATE-10 | AWS hosted endpoint/IP visual evidence is refreshed after update | Update lifecycle report, regression matrix, package provenance, AWS evidence | UNKNOWN | SESSION-12 |
| UPDATE-11 | Rollback to previous package/unit version is verified or UNKNOWN | Update lifecycle report, regression matrix, package provenance, AWS evidence | UNKNOWN | SESSION-12 |
| UPDATE-12 | Update/re-export audit records old/new versions and evidence paths | Update lifecycle report, regression matrix, package provenance, AWS evidence | UNKNOWN | SESSION-12 |

## Matrix verdict

```text
IN_PROGRESS_NEXT_STEP_KNOWN
```

## First next action

```text
Execute SESSION-05-TENANT-ADAPTATION-STUDIO.md: implement or define Tenant Adaptation Studio workspace, repo/snapshot intake, policy, diff, approval, audit and AWS visual evidence rows.
```


## Master tenant launch runbook rows

| ID | Requirement | Evidence required | Status | Session |
|---|---|---|---|---|
| LAUNCH-01 | Plan package is installed in the main Enterprise project, not xiigen-mvp, tenant fork, scratch folder or plugin-only repo | repo remote + path evidence in master-tenant-launch-evidence.json | PASS | Remote is xiigen-platform-dot-net-angular and package path is docs/plans/enterprise-adaptation/v3.3/ |
| LAUNCH-02 | Package path is docs/plans/enterprise-adaptation/v3.3/ | file listing evidence | PASS | File listing captured in master-tenant-launch-evidence.json |
| LAUNCH-03 | BEACON, master plan, state, matrix, runbook, sessions and contracts are all copied as one package | file listing evidence with all required filenames | PASS | Required non-secret package files copied; raw key files excluded by no-secrets rule |
| LAUNCH-04 | Current branch is recorded without switching branch | git branch --show-current evidence | PASS | Recorded $branch |
| LAUNCH-05 | Master tenant is recorded as launch control tenant | state.masterTenantLaunch.scopeEvidence | PASS | State records platform repo as master tenant control plane |
| LAUNCH-06 | Customer tenant repos are marked targets only, not control plane | state + plan reference | PASS | Master plan and state preserve customer tenant repos as targets only |
| LAUNCH-07 | Validation projects are marked validation targets only, not unit owners or launch root | state + plan reference | PASS | Master plan and state preserve validation projects as targets only |
| LAUNCH-08 | AGENTS.md references BEACON and launch runbook | rg evidence | PASS | AGENTS.md references BEACON and MASTER-TENANT-LAUNCH-RUNBOOK-v1.md |
| LAUNCH-09 | CLAUDE.md references BEACON and launch runbook | rg evidence | PASS | CLAUDE.md references BEACON and MASTER-TENANT-LAUNCH-RUNBOOK-v1.md |
| LAUNCH-10 | docs/HOW-TO-USE-SKILLS.md references BEACON and launch runbook | rg evidence | PASS | docs/HOW-TO-USE-SKILLS.md references BEACON and MASTER-TENANT-LAUNCH-RUNBOOK-v1.md |
| LAUNCH-11 | State JSON parses after package installation | ConvertFrom-Json evidence | PASS | State JSON parsed after package installation |
| LAUNCH-12 | master-tenant-launch-evidence.json exists and contains no secrets | JSON parse + redaction check evidence | PASS | Evidence JSON created; raw key files excluded and not copied |
| LAUNCH-13 | Executor hands off to REVIEW_INTERNAL after startup slice | state current_internal_role_state / review report evidence | PASS | Evidence records REVIEW_INTERNAL_PASS_SESSION_00_GOVERNANCE_ACCEPTANCE |
| LAUNCH-14 | Architect explicitly reinstates executor authority before SESSION-01 | architect verdict record | PASS | State and evidence record ARCHITECT_VERDICT_CONTINUE_EXECUTION for SESSION-01 only |

## Legal documents site integration rows

| ID | Requirement | Evidence required | Status | Session |
|---|---|---|---|---|
| LEGAL-01 | LegalDocument content model exists or is marked new development required | content model path or gap report | UNKNOWN | SESSION-13 |
| LEGAL-02 | Terms of Service / Terms of Use has versioned legal record | legal-document-registry.json row | UNKNOWN | SESSION-13 |
| LEGAL-03 | Privacy Policy has versioned legal record | legal-document-registry.json row | UNKNOWN | SESSION-13 |
| LEGAL-04 | Cookie Policy / Cookie Notice has versioned legal record | legal-document-registry.json row | UNKNOWN | SESSION-13 |
| LEGAL-05 | EULA / License Agreement has versioned legal record | legal-document-registry.json row | UNKNOWN | SESSION-13 |
| LEGAL-06 | Enterprise Terms/MSA or legal placeholder has versioned legal record | legal-document-registry.json row | UNKNOWN | SESSION-13 |
| LEGAL-07 | Data Processing Agreement / DPA has versioned legal record | legal-document-registry.json row | UNKNOWN | SESSION-13 |
| LEGAL-08 | Acceptable Use Policy has versioned legal record | legal-document-registry.json row | UNKNOWN | SESSION-13 |
| LEGAL-09 | Affiliate Program Terms has versioned legal record and is linked from affiliate apply/dashboard | registry + route map + screenshot when runtime claim exists | UNKNOWN | SESSION-13 |
| LEGAL-10 | Refund/Cancellation Policy is linked from pricing/checkout | route map + screenshot when runtime claim exists | UNKNOWN | SESSION-13 |
| LEGAL-11 | Security/Responsible Disclosure page is linked from public footer | route map + screenshot when runtime claim exists | UNKNOWN | SESSION-13 |
| LEGAL-12 | Subprocessors/Infrastructure Disclosure is linked from privacy/DPA context | route map + screenshot when runtime claim exists | UNKNOWN | SESSION-13 |
| LEGAL-13 | Open-source/Third-party notices are linked from download/install/export package | route map + package evidence | UNKNOWN | SESSION-13 |
| LEGAL-14 | Signup/account creation records accepted legal document version where required | acceptance record schema/evidence | UNKNOWN | SESSION-13 |
| LEGAL-15 | Checkout/license purchase records accepted EULA/refund/legal version where required | acceptance record schema/evidence | UNKNOWN | SESSION-13 |
| LEGAL-16 | Export download/corporate install references license, third-party notices and enterprise terms | export/install evidence | UNKNOWN | SESSION-13 |
| LEGAL-17 | EN/HE/RU locale status is recorded; Hebrew RTL legal page evidence is required when published | locale registry + screenshot metadata | UNKNOWN | SESSION-13 |
| LEGAL-18 | No DRAFT or LEGAL_REVIEW_REQUIRED legal document is publicly published as final | publication status report + route access evidence | UNKNOWN | SESSION-13 |
