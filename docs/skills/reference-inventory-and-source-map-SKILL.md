# Reference Inventory And Source Map

> Ported from the `llm_mvp_core` source-map standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build Xiigen.Platform.slnx` / `dotnet test`. `source_layer` tags split
> IMPLEMENTATION (`src/Xiigen.*/*.cs`, `*.csproj`) from RECONCILIATION/DESIGN_DOC
> (v3.3 Beacon/state/matrix/contract). Every governance claim lifted from
> `AGENTS.md`/`CLAUDE.md` prose into a skill must carry a `path` + `line_range`.
> Domain return type is `DataProcessResult<T>` (never core `OperationResult<T>`).

## When to load

Load before deep planning, plan review, benchmark-depth comparison,
shallow-planning repair, architecture review, or any artifact that makes claims
about existing files, guides, code, plans, tests, or governance rules.

## Purpose

Force every planning or review claim to trace back to exact read sources. A broad
statement such as "the repo/docs say" is not evidence until it is mapped to a
`source_id`, exact `path`, `line_range`, and the plan or review claim it supports.

## Subagent Firewall Sentinels

Preserve these exact proof strings from `AGENTS.md` as authority:
- `Parent Architect Subagent-Only Firewall`
- `parent local sidecar work ban`
- `read-only verification after a sub-agent packet returns`
- `sub-agent waiting is not a stop`
- `no local non-overlapping work escape`

Plain-language rule: the source map is not just a polished table; it lets
Luba see where each conclusion comes from. Another author's example, an old plan,
a benchmark, a sub-agent packet, or a broad folder search can serve only as
supporting evidence. They do not become the current task, the current artifact,
permission to copy, or evidence of product readiness without a direct current
instruction from Luba and direct reading of the source.

## Required Source Inventory

Create a source inventory before plan prose or before approving a review:

```text
Reference Inventory:
  source_id:
  path:
  line_range:
  source_layer: current_luba_quote | work_order | plan | v3.3_contract | skill | code | test | doc | benchmark_evidence | sub_agent_packet
  claim_supported:
  plan_section:
  decision_supported:
  active_artifact_scope: exact_current_target | evidence_only | out_of_scope
  source_license_status: project_owned | third_party | unknown | not_applicable
  reuse_boundary: universal_pattern | project_specific | benchmark_only | evidence_only
  source_disjoint_evidence: independent_roots | duplicate_derived | single_source
  confidence_effect:
  held_out_roots_or_sources:
  status: used | rejected | unavailable | evidence_only
  why_used_or_rejected:
```

Rules:
- Every read source must map to a claim, plan section, review finding, or explicit rejection row.
- Broad repo, folder, or document-family references cannot satisfy a plan/review claim without exact `source_id`, `path`, and `line_range`.
- Benchmark examples, quoted transcripts, old plans, v3.3 state files, and sub-agent packets may support `benchmark_evidence` / `evidence_only` rows only; they cannot create active requirements unless Luba reactivates them in the current unquoted instruction.
- Every source row must distinguish universal reusable pattern from project-specific detail. Project-specific paths, tenant data, customer names, and licensed third-party material cannot be copied into a universal guide unless the row records the license/source boundary and the reusable pattern is restated generically.
- A source with `source_license_status=unknown` cannot support implementation, publication, dataset, or readiness claims. It may only support a recon task, an evidence-only note, or a rejected/deferred review row.
- For cross-repo (platform ↔ flows ↔ `llm_mvp_core`), multi-root, or broad architecture planning, every source row must state whether evidence is from independent roots/sources, duplicate-derived material, or a single source; how this changes confidence; and which roots remain held out. This is a general planning/review confidence gate, not a DPO/training-only rule.
- The exact current artifact must stay explicit. A map for one plan, session file, or sub-agent packet cannot silently repair, approve, stage, commit, or replace a different artifact.
- If a source was read but does not affect the plan/review, mark `status=rejected` and explain why. Silent unused reads fail the map.
- If a source is needed but unavailable, mark `status=unavailable`, name the blocked claim, and either remove the claim or queue an explicit recon task.

## Source-To-Plan / Source-To-Review Map

After the inventory, create the map:

```text
Source Map:
  source_id:
  claim_supported:
  plan_section_or_review_finding:
  decision_supported:
  exact_evidence_line_range:
  active_artifact_scope:
  source_license_status:
  reuse_boundary:
  source_disjoint_evidence:
  confidence_effect:
  held_out_roots_or_sources:
  status: mapped | rejected | missing | contradicted
  why_used_or_rejected:
```

Review must check:
- every binding requirement has at least one mapped current-authority source;
- every code/doc/governance claim has an exact source id and line range;
- every review finding has PASS/FAIL, exact line or absence evidence, impacted requirement, required fix, and re-review trigger;
- every rejected or evidence-only source stays out of active target scope;
- every license/source row and universal-vs-project-specific boundary is explicit before a detail is reused outside its original project context;
- cross-repo or multi-root claims record source disjointness, confidence impact, and held-out roots before acceptance;
- no completion/readiness claim depends on a broad folder search, memory, or a sub-agent summary without direct source read-back;
- no plan/governance review claims product evidence or implementation readiness unless the exact mapped artifact contains runtime/test evidence required by its Gate B (`dotnet test`).

If any row is `missing` or `contradicted`, the plan or review is not ready.

## Governance-Prose Lift Rule (platform-specific)

Platform governance lives largely as prose in `AGENTS.md` and `CLAUDE.md`. When a
skill, plan, or review restates a governance rule (Beacon First, executor cannot
stop, fabric-first, `DataProcessResult<T>`, enterprise scope), the source row
must give the exact `AGENTS.md`/`CLAUDE.md` `path` + `line_range`. A governance
claim with no line range is a `missing` row.

## High-Risk Architecture Recon Library Hook

When `reconnaissance-gate-SKILL.md` requires the High-Risk Architecture Recon
Library Gate, this map must link to the Knowledge Library artifact instead of
replacing it. A generic source inventory is not a substitute for the quantified
library passes. Required additional rows:

```text
Knowledge Library:
  path:
  type:
  summary:
  why_to_use:
  when_to_use:
  relevance_to_target_plan:
  document_locations:
  implementation_locations:
  evidence_line_range:
  decision_supported:
  source_disjoint_evidence: independent_roots | duplicate_derived | single_source
  confidence_effect:
  held_out_roots_or_sources:
  status:
  coverage_critic:
```

## Named Artifact / Library / Folder Population Proof

When a claim depends on a named artifact, evidence library, or folder (for
example the `.xiigen/wiki-domain` manifests consumed under the
`TRAINABLE-UNIT-MANIFEST-CONTRACT.md`), add a population proof row before
completion:

```text
Population Proof:
  exact_path:
  file_list_or_index:
  non_empty_proof:
  cutoff_snapshot:
  late_file_queue:
  evidence_depth_score: targeted | recon-library | full-pass | legal-verified
```

Reject completion when the proof is a broad folder grep, remembered path,
summary-only packet, or empty directory. Accepted late files must be merged into
the current target artifact or rejected/deferred with reason and line/section
read-back.

Review must fail when:
- high-risk architecture plan prose appears before the required pass floors;
- Knowledge Library rows lack `coverage_critic` or `decision_supported`;
- cross-repo or multi-root rows omit `source_disjoint_evidence`, `confidence_effect`, or `held_out_roots_or_sources`;
- a broad source map, folder grep, or old packet is used as the library;
- a required artifact/library/folder lacks exact path, file list/index, non-empty proof, cutoff snapshot, or evidence depth score;
- source rows do not distinguish document locations from implementation locations for decisions that depend on both.

## Integration

- `reconnaissance-gate-SKILL.md` produces the RECON REPORT this inventory cross-links to.
- `specificity-calibration-SKILL.md` scores whether the synthesis cites these source rows.
- `claim-verification-SKILL.md` confirms VERIFIED claims trace to a mapped IMPLEMENTATION/TEST source.
