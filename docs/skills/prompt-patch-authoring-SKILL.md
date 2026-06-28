# Prompt Patch Authoring (XIIGen Enterprise Platform)

> Ported from the `llm_mvp_core` standard, adapted for **.NET 10 ASP.NET Core Web API**,
> xUnit, Gate-B = `dotnet build` / `dotnet test`. Only the **non-ML governance facet** is
> ported: Rules 0–4 and Rule 6 — the discipline for patching a context/code-generation
> package so a single bottleneck is fixed without regressing another named check. **Rule 5
> (multi-generate verification) is note-only here:** it is bound to the core multi-generate
> topology (Pattern F) and the DPO arbiter, which by R5/R6 stay in `llm_mvp_core`; this
> platform consumes models via manifest and runs no DPO loop. WRONG/CORRECT patterns are
> written in C#/ASP.NET Core. This skill is relevant where there is generation of
> context/code for services; for pure-infrastructure phases it is note-only.
>
> **Seam with G12:** in the core ML group this file's *application* (DPO/escalation patches
> in the `meta-arbiter` / score-interpretation loop) is `na — core-specific`. Here only the
> general patch-authoring rule is ported — a different facet of the same file, not a conflict.

## Core Problem

A context package that produces a score in the 0.65–0.89 range has a fixable gap — but
applying the wrong patch type, patching the wrong bottleneck, or introducing a conflict with
an existing named check will produce a regression in a different capability class while
appearing to fix the current one. This skill defines the rules that govern when and how to
patch a context package, what the patch must contain, and how to prevent it from breaking
what was already working.

---

## When to Invoke

- When a capability class scores in DETAIL_GAP (0.65–0.84) and a targeted patch is the right
  intervention (not a full rewrite).
- When score interpretation routes to "apply PATCH".
- When the context/code-generation package for a service produces a recurring WRONG pattern.

---

## Section 1 — The Rules (non-ML facet)

### Rule 0 — One Bottleneck Per Patch

A patch targets exactly one bottleneck per cycle. Never combine two independent gap fixes:

```text
Identify the higher-impact bottleneck. Patch it. Re-score.
If still insufficient: identify the next bottleneck. Patch separately.
```

Combining two fixes makes it impossible to tell which fix resolved the gap — or which
introduced a regression. One patch = one learning signal.

### Rule 1 — NAMED CHECK + WRONG/CORRECT Pair

Every patch must contain:

```text
NAMED_CHECK: CF-[N] — [descriptive name]
WRONG:   [exact pattern that fails — the literal failure mode, not a generalization]
CORRECT: [exact pattern that passes — the replacement that satisfies the iron rule]
```

Example (platform stack):

```text
NAMED_CHECK: CF-12 — DataProcessResult return on a public service method
WRONG:   public void ProcessBatch(IReadOnlyList<string> ids) { ... }                       // throws on failure
CORRECT: public Task<DataProcessResult<BatchResult>> ProcessBatchAsync(IReadOnlyList<string> ids) { ... }
```

Another:

```text
NAMED_CHECK: CF-7 — tenant scope on every IDocumentStore query
WRONG:   await _docs.SearchAsync("users", filter, ct);                                      // no tenant scope
CORRECT: await _docs.SearchAsync("users", WithTenant(filter, _tenant.Current), ct);
```

The WRONG pattern must match the literal failure mode. A generalized description ("missing
return type") is insufficient — the validator must match the pattern against generated code.

### Rule 2 — Target Zone 0.80–0.90

```text
< 0.80:     Patch is attempting too much — narrow the scope further.
0.80–0.90:  Target zone — single focused fix.
> 0.90:     Over-specified — the patch is adding detail the iron rule does not require.
```

Over-specification at > 0.90 is a signal to loosen, not to celebrate. A patch that constrains
generated code beyond what the iron rule requires will block valid implementations that
satisfy the rule through different but correct means (e.g. forcing one specific
`DataProcessResult` construction when several are valid).

### Rule 3 — Regression Prevention Checklist

Before submitting any patch:

```text
[ ] Load all existing NAMED CHECKs for this capability class.
[ ] Does the new WRONG pattern conflict with any existing CORRECT pattern?
[ ] Does the new CORRECT pattern match any existing WRONG pattern?
[ ] If either conflict exists: the bottleneck is not a patch target — escalate to a contract revision.
```

A patch that passes the current class while conflicting with an existing check is worse than
no patch — it introduces a silent failure in another capability class.

### Rule 4 — Patch vs Rewrite Thresholds

```text
Score 0.65–0.84 (DETAIL_GAP):       Targeted patch (this skill).
Score 0.50–0.64 (PATTERN_MISSING):  Section rewrite (all checks in one sub-section).
Score < 0.50 (STRUCTURAL):          Full rewrite (reconstruct from iron rules).
```

A DETAIL_GAP means the generator understands the pattern but missed a specific detail. A
PATTERN_MISSING means the wrong mental model of one sub-section. A STRUCTURAL score means the
context package itself must be rebuilt. **Never apply a targeted patch to a STRUCTURAL
failure** — it looks helpful locally while the structural problem compounds.

### Rule 5 — Multi-Generate Verification — NOTE-ONLY (not used here)

> **Not portable to this platform.** Rule 5 is bound to the core multi-generate topology
> (Pattern F) and the DPO arbiter ("the DPO arbiter sees disagreement"), which stay in
> `llm_mvp_core` by R5/R6. This platform consumes models via manifest and runs no
> multi-generate / DPO loop, so Rule 5 is inapplicable. If a future phase introduces a
> platform-side multi-generate path, re-evaluate against the core skill before relying on it.

### Rule 6 — Patch History Record

Every applied patch is recorded:

```csharp
public sealed record PatchHistoryEntry(
    string CapabilityClass,
    string SessionId,
    string NamedCheck,      // CF-N identifier
    string WrongPattern,
    string CorrectPattern,
    double ScoreBefore,
    double ScoreAfter,
    string PatchOutcome);   // RESOLVED / PARTIAL / REGRESSED
```

A `REGRESSED` outcome is high-value evidence. Do not delete it — it indicates a check
over-fires or the contract is wrong, and feeds a later contract-revision decision. The
history record lays into the v3.3 completion-matrix / session evidence, not an external scratch file.

---

## Section 2 — Patch Format Template

```text
CONTEXT_PATCH:
  CapabilityClass: [capability class name]
  IronRuleTarget:  CF-[N]
  ScoreBefore:     [current score]
  TargetScore:     0.85 (PASS minimum)

  NAMED_CHECK: CF-[N] — [check name]
  WRONG:   [exact failure pattern — literal C# / ASP.NET Core code or description]
  CORRECT: [exact passing pattern — literal C# / ASP.NET Core code or description]

  RegressionCheck:
    - Conflicts with existing WRONG patterns: NONE / [list]
    - Conflicts with existing CORRECT patterns: NONE / [list]

  MultiGenerateVerified: N/A — platform consumes models via manifest; no multi-generate loop (see Rule 5 note)
```

---

## Anti-Patterns

1. **"Patching two bottlenecks in one cycle"** — produces two learning signals at once,
   making attribution impossible. One bottleneck per patch.
2. **"WRONG pattern is a generalization"** — "missing DataProcessResult" is not matchable.
   Write the actual C# that fails (`public void X(...)`), not a description of why.
3. **"Target score set at 1.00"** — 1.00 means only one valid implementation is allowed.
   0.85–0.90 is the target; variation above that produces valid code through different routes.
4. **"Skipping Rule 3 regression check"** — a patch that conflicts with an existing check
   creates a silent failure elsewhere. Not optional.
5. **"Applying a patch to a STRUCTURAL failure"** — a package scoring < 0.50 needs
   reconstruction, not a targeted addition.
6. **Reviving Rule 5 locally** — there is no DPO/multi-generate loop in this repo; Rule 5
   stays note-only until a platform multi-generate path is explicitly added.

## Integration

- `archetype-failure-map-SKILL.md` routes a DETAIL_GAP (0.65–0.84) bracket to this skill.
- `iron-rule-derivation-SKILL.md` supplies the iron rule a NAMED_CHECK targets.
- `STACK-MAP.md` records the R5/R6 boundary that keeps the DPO arbiter and multi-generate topology (Rule 5) in `llm_mvp_core`.
- `audit-protocol-SKILL.md` Point 3 (API shape) verifies the CORRECT pattern's `DataProcessResult<T>` return shape.
