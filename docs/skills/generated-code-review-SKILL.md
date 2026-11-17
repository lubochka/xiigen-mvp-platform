# Generated Code Review (XIIGen Enterprise Platform)

## Overview

The 5-layer review that replaces visual inspection for generated/produced code,
plus Layer 5 (trainable-vs-stub) and the Deterministic-Boundary gate. Platform
port of the core generated-code-review skill.

Stack-bound to **.NET 10 ASP.NET Core Web API**, **xUnit** (`tests/`),
`DataProcessResult<T>`, the platform fabric interfaces, and **Roslyn**
(`CSharpSyntaxTree.ParseText`) for parse checks. Gate-B is
`dotnet build Xiigen.Platform.slnx` (0 errors AND 0 warnings) + `dotnet test`.

## When to apply

Apply to code produced by any generator, and to any Web API service in
`src/Xiigen.Platform.Marketplace/` claimed to be trainable —
`SmartWikiService.cs`, `SmartCodeAssistantService.cs`, `MarketplaceService.cs`,
`TenantAdaptationStudioService.cs` (tests: `tests/Xiigen.Platform.Marketplace.Tests`).
Any deliverable a plan calls "trainable", "learned", "model", or "DPO" → Layer 5
is mandatory. Contract/build/DNA evidence is NOT sufficient for a trainable claim.

---

## Layer 1 — Semantic Review

Names and comments reflect intent, not implementation accidents.

```
1. Does the class name describe what it represents (not what generated it)?
2. Does every method name describe the action, not the algorithm?
3. Any placeholder names? ("Item1", "Temp", "Result1")
4. Comments present where logic is non-obvious?
```

**Gate B:**
```bash
grep -n "Item[0-9]\|Temp[0-9]\|Result[0-9]\|TODO\|FIXME\|PLACEHOLDER" <file.cs>
# → 0 matches
```

---

## Layer 2 — DNA Compliance

```bash
# Rule 1: no throw for business logic
grep -n "throw new" <file.cs>
# → only programmer-error throws, with a comment

# Rule 2: no direct fabric instantiation
grep -n "new .*\(Store\|Service\|Accessor\|Scheduler\|Client\)(" <file.cs>
# → 0 matches in business logic

# Rule 1 continued: DataProcessResult<T> returns
grep -n "DataProcessResult" <file.cs>
# → every public business method has at least one match

# No external ML framework imports added on platform (models live in llm_mvp_core)
grep -n "using.*TorchSharp\|using.*Microsoft.ML\|using.*TensorFlow" <file.cs>
# → 0 matches
```

**Gate B:** all grep checks pass; Rule 3 (comment density) checked manually.

---

## Layer 3 — Silent Failure (MACHINE Constant Test)

Generated/produced code can output something that looks correct but silently
fails at runtime. Verify the logic with fixed, pre-calculated constants.

```csharp
[Fact]
public async Task SmartWiki_IndexGraph_WithKnownGraph_StoresExpectedShape()
{
    // Arrange — real service through DI, known input
    var services = new ServiceCollection();
    services.AddXiigenInMemoryFabrics();      // real IDocumentStore etc.
    services.AddSingleton<ISmartWikiService, SmartWikiService>();
    using var provider = services.BuildServiceProvider();
    using var _ = provider.GetRequiredService<ITenantContextAccessor>()
        .Push(new TenantContext("t1", "u1", new HashSet<string>(), "corr-1", "test", DateTimeOffset.UtcNow));
    var wiki = provider.GetRequiredService<ISmartWikiService>();

    // Act
    var result = await wiki.IndexGraphAsync(KnownRequestWith(nodes: 3, edges: 2));

    // Assert — MACHINE constants (fixed, not derived from the generator)
    Assert.True(result.IsSuccess);
    Assert.Equal(3, result.Data!["nodeCount"]);
    Assert.Equal(2, result.Data!["edgeCount"]);
}

// "Security break" variant — it MUST fail when it should fail
[Fact]
public async Task SmartWiki_IndexGraph_WithNoNodes_ReturnsFailure()
{
    var wiki = /* same DI setup */ BuildWiki();
    var result = await wiki.IndexGraphAsync(RequestWith(nodes: 0));
    Assert.True(result.IsFailure);
    Assert.Equal("GRAPH_NODES_REQUIRED", result.Error!.Code);
}
```

**Gate B:** both the match test AND the non-match/failure test pass.

---

## Layer 4 — Generalization

```
1. On 100 different inputs, will it produce 100 correct outputs, or work by accident?
2. Does it rely on a specific hardcoded value?
   grep -n "[\"'][a-zA-Z0-9_]\{10,\}[\"']" <file.cs>   # hardcoded strings in logic are a red flag
3. Edge cases handled? empty / single-element / max-length / special characters.
4. Would the output be valid training data for future improvement?
```

**Gate B:** at least 3 different inputs tested, all producing correct output.

---

## Layer 5 — Trainable vs Stub (MACHINE numeric-metric test)

A trainable unit can compile, pass contract tests, and satisfy DNA while being a
pure stub that never learns. FAIL the review when the plan requires a trained
algorithm but the code shows any of:
- a status string such as `not_ready_*`, `*_no_inference_implemented`, `not_trained_*`;
- a method returning a fixed descriptor object instead of computing from learned state;
- an empty/no-op training loop;
- `NotImplementedException`, `throw`, `TODO`, `FIXME`, `PLACEHOLDER` on the inference/training path;
- inference that is exact-source/exact-prompt lookup presented as generalization.

Required numeric evidence (reproducible from the runner; UNKNOWN not allowed for
a done claim): accuracy / exact / loss / reward-margin on a held-out set;
`cpu_ms`, `ram_mb`, row counts; train vs test identity sets (no replay leakage).

**Gate B:** numeric metrics present AND reproducible AND the unit changes its
decision after training on new pairs. UNKNOWN / validation_only / static /
contract-only → Layer 5 = FAIL.

> R5 boundary: trainable/common models are owned by `llm_mvp_core` and consumed
> on platform via manifests/locators (`.xiigen/wiki-domain`). Apply Layer 5 only
> when a path is actually claimed trainable; otherwise label it an
> honest-untrained scaffold and say so.

### Layer 5A — Allowed Trainable Architecture Pattern

For any learned/semantic/trainable/adaptive/DPO/selector/router/classifier/
scorer/retrieval/KNN/dictionary/counter/pattern-matching claim, classify the
actual runtime mechanism. Allowed levels only: `ILearningMachine<TDomain>`,
`ILogicalOrchestrator`, `ILogicalFlow<TInput>`, `ILogicalUnit<TInput,TOutput>`,
`ILearningMachinePair<TPersonal,TGlobal>`. Scorers/selectors/routers/retrievers/
KNN/Jaccard/BM25/dictionaries/regex/pattern matchers and private `TryDecide*` /
`ChooseBestPattern` methods are not architecture patterns — they pass only as
internal details inside an allowed level, with honest scope labels.

```text
Allowed hierarchy: <one of the five>
Non-level classification: <internal detail only / none>
Actual mechanism: <short human explanation + code quote>
Runtime authority: yes/no
Semantic claim: full/baseline/scaffold/eval-only/too strong
Verdict: OK_ALLOWED_LEVEL / OK_INTERNAL_DETAIL / BASELINE_MUST_BE_LABELLED /
  NEEDS_ILOGICALUNIT_WRAPPER / FORBIDDEN_RUNTIME_SHORTCUT / REPORT_CLAIM_TOO_STRONG
Concrete repair: current -> target; replacement level; replacement class/unit/flow;
  replacement algorithm; input/output contracts; typed trained state;
  KNN/nearest methods and role; DPO/source roots + license gate; pair-building rule;
  load user/cache first then repo/common fallback; backup/export/import/share;
  continue-training from checkpoint; affected sets/assets; affected tests.
```

Layer 5A fails if `Concrete repair` is generic, or if a learned
dictionary/counter/map is replaced only by a class name without showing the typed
state record and learning lifecycle.

### Deterministic-Boundary Code Review Gate

Every code slice review must explicitly search for hidden deterministic
mechanisms posing as trainable behavior:

```text
deterministic_boundary_review = PASS | FAIL
searched_mechanisms = scorer/router/lookup/counter/exact-replay/static-fixture
hidden_deterministic_mechanism_found = true/false
actual_runtime_authority:
allowed_level = ILogicalUnit | ILogicalFlow | ILogicalOrchestrator | ILearningMachine | ILearningMachinePair | internal detail only
claim_matches_mechanism = true/false
project_coding_standards_checked = true/false
allowed_tests_or_gate_b_checked = true/false
core_logic_boundary_checked = true/false   # core algorithm logic stays in llm_mvp_core, not the platform host/API
host_adapters_thin_checked = true/false    # Xiigen.Platform.Api endpoints stay thin
deterministic_helpers_labelled_eval_only_helper_internal_or_baseline = true/false
deterministic_helpers_not_runtime_trainable_authority = true/false
docs_qa_impact_checked_when_applicable = true/false
required_fix:
```

The review also fails if coding standards, allowed tests, the
`llm_mvp_core` core boundary, thin API endpoints, deterministic-helper labels, or
docs/QA impact were not explicitly checked. A passing test result or machine
label alone is not a complete code review.

### Human-readable code review report gate

For trainable/architecture findings the review must include an English human
summary unless the user requests another language; a Mermaid/PlantUML diagram when architecture/data flow is explained; a
no-code algorithm explanation; an exact `file:line` quote plus
`current behavior` / `why this is wrong/acceptable` / `required behavior`; and the Concrete Repair Blueprint
fields. "wrap in ILogicalUnit", "add KNN", "add tests" is not a decision unless
exact classes/records/methods/tests/sets/manifests and lifecycle are named.

---

## Review Output Record

```
GENERATED CODE REVIEW — [date]
File: [file.cs]
Producer: [generator / SmartWikiService / SmartCodeAssistantService / ...]

Layer 1 (Semantic): PASS/FAIL — [notes]
Layer 2 (DNA):      PASS/FAIL — [grep results]
Layer 3 (Silent):   PASS/FAIL — [match + failure test]
Layer 4 (Generalization): PASS/FAIL — [inputs tested]
Layer 5 (Trainable vs Stub): PASS/FAIL — [numeric metrics or stub-evidence]
Layer 5A (Allowed Architecture): PASS/FAIL — [allowed level, mechanism, verdict]
Code review completeness: PASS/FAIL — [standards/tests/core boundary/thin API/labels/docs-QA]

Overall: PASS/FAIL
Action: [commit as-is / regenerate with fixed generator / escalate to REVIEW_INTERNAL]
```

---

## Multi-cycle code review floor

When the plan requires at least four review/fix cycles before commit/push, one
review record is one cycle:

```text
generated_code_review_cycle_1..4 = PASS after fixes/recheck
cycle_focuses_are_distinct = true
unfixed_findings_open = false
```

If any cycle finds a defect, apply the scoped fix and rerun the next cycle after
the fix. Missing cycle evidence keeps commit/push closed.

---

## Bounded sub-agent evidence boundary

When the evidence comes from a bounded sub-agent, verify the work-order boundary
before judging quality: `work_order_scope`, `exact_allowed_files`,
`actual_files_changed`, `commit_push_authorized`, `allowed_git_commands`,
`forbidden_commands`, `commit_push_performed`,
`parent_will_perform_commit_push_after_review`, `may_derive_next_phase`,
`must_stop_after_report`, `parent_local_sidecar_work_while_waiting`,
`read_only_verification_after_packet_return_only`. Reject the packet if the
sub-agent created later-phase artifacts, committed/pushed without commit-only
authority, derived the next phase from `STATE next_action`, or ran git write/ref
commands without authority. A sub-agent `parent-review` file is evidence only.

---

## Anti-Patterns

1. **Visual inspection as verification** — "looks right" is not Layer 3.
2. **One-input testing** — one passing case does not prove generalization.
3. **Skipping Layer 2 because "the generator would never do that"** — verify.
4. **Committing produced files without a review record.**
5. **Reviewing a trainable unit by contract/build test only** — a stub returning
   `not_ready` passes contract tests, builds clean, and satisfies DNA. Layer 5
   numeric metrics are mandatory for trainable deliverables (the V20 failure).
