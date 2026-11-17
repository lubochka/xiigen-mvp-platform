# Output Readability Gate (XIIGen Enterprise Platform)

## Core Problem

Generated output that contains shortcodes, status strings, or code-style
identifiers written as plain prose is unreadable to a human reviewer. The same
token that belongs in a code fence — `CF-12`, `NOT_READY`, `Rule 1` — becomes
noise when scattered through a paragraph. This skill defines the four prohibited
patterns, the exception for code fences, the dashboard/API readability gate, and
the English-first Execution Board gate that must pass before any generated output
or status is accepted.

Platform port of the `llm_mvp_core` skill. Stack-bound to **.NET 10 ASP.NET Core
Web API**, **xUnit** (`tests/`), `Xiigen.Platform.slnx`, `DataProcessResult<T>`,
and the v3.3 governance triple. The host-UI proof sections of the core skill
(Dual-Path Same-Scenario UI proof, Host Session Auth Preflight, Multi-Monitor
Target Window Control, No-Blind-Spot UI Scenario Script) are **`na`** here — the
platform has no frontend and no host UI. The machine-learning pipeline-signal
example (`OUTCOME` / `DPO_TRIPLE`) is also not ported (no learning pipeline here).
The English-first Execution Board below is the formal form of the human-first
status rule that `AGENTS.md`/`CLAUDE.md` state only as prose.

---

## When to Invoke

- Before accepting any prose output from a generation step
- When writing plan documents, skill files, or decision records
- When a human reviewer reports that output is "hard to follow" or "reads like code"
- When writing project documentation, API docs, or user-facing examples
- Before sending any execution status to Luba (Execution Board gate, Section 6)

---

## Section 1 — The Core Rule

```
No shortcodes, status strings, or code-style identifiers may appear in prose
outside of triple-backtick code fences.

Exception: inside ``` code fences, any identifier is allowed.
```

This rule applies to ALL prose — plan text, skill descriptions, decision records,
comments, and AI-generated explanations. It does NOT apply to code blocks, inline
code spans with backticks, or table cells that display code values.

---

## Section 2 — Four Prohibited Patterns

### Pattern 1 — LETTER(S)-DASH-NUMBER(S) identifiers in prose

```
Prohibited: CF-12, DR-5, FC-7, BUG-001, D-01
Reason: Shortcode syntax belongs in reference tables and code, not sentences.
Allowed form: "Capability Framework Rule 12 (CF-12)" on first use; "CF-12" after.
```

### Pattern 2 — Bare letter+number tokens in prose

```
Prohibited: R1, G3, Q2, T4 (when used as identifier shortcuts in prose)
Reason: Ambiguous without expansion — R1 could mean Rule 1, Requirement 1, or Round 1.
Allowed form: write the full term first: "Round 1 (R1)" or "Rule 1" without abbreviation.
```

### Pattern 3 — ALL_CAPS_UNDERSCORE status strings in prose sentences

```
Prohibited: "The store returned NOT_READY so we escalated."
            "When the result is TENANT_SCOPE_VIOLATION the request is rejected."
Reason: Status strings written as prose identifiers are not English sentences.
Allowed form: "The store returned a not-ready result, so we escalated."
              Or: the `NOT_READY` signal (inline backtick is allowed).
```

### Pattern 4 — Version suffix codes in prose

```
Prohibited: "The fix was applied in v3.3+hotfix-2 of the platform."
Reason: Version identifiers belong in changelogs and metadata, not description prose.
Allowed form: "The fix was applied in version 3.3 (hotfix 2)."
```

---

## Section 3 — Platform Identifier Equivalents

The same four patterns apply specifically to this platform's identifiers:

```
Ban "Rule N" alone without a subject
  -> Write "the platform standard: every public method returns DataProcessResult<T>"
     or use inline backtick `Rule N`.

Ban DataProcessResult status/error-code strings as prose
  -> Not: "The method returned Failure with TENANT_NOT_FOUND."
  -> Yes: "The method returned a failure result with error code `TENANT_NOT_FOUND`."

Ban fabric/DI token names as prose adjectives
  -> Not: "This is an IOutboxStore violation in the append path."
  -> Yes: "This violates the IOutboxStore contract — AppendAsync returns void instead of a DataProcessResult."
     (or use inline backtick / code fence for the interface and error-code names)
```

(The core skill's machine-learning pipeline-signal example is not ported — the
platform emits no learning-pipeline signals.)

---

## Section 4 — Gate Check

Before accepting any prose output:

```
□ No LETTER(S)-DASH-NUMBER(S) identifiers outside backticks or code fences
□ No bare letter+number tokens (R1, G3, Q2) used as identifier shortcuts in prose
□ No ALL_CAPS_UNDERSCORE status strings written as prose words
□ No version suffix codes in descriptive sentences
```

If any checkbox fails: return the output to the generation step with the specific
pattern identified. Do not rewrite the prose manually if a generator produced it —
instruct the generator to rewrite the failing sentence using the allowed form.

---

## Section 5 — Dashboard And API Documentation Readability

API documentation is accepted only when a non-implementer can answer: "what API is
this?", "what do I type?", "what will I see?", and "what does this evidence prove?"
without reading source code first.

Required for each exposed API endpoint/method:

```
□ Human name, not only route/method id
□ One realistic example request
□ Main parameters in plain language
□ Expected response in plain language (including the DataProcessResult success/failure shape)
□ Evidence boundary: what the result proves and what it does not prove
```

Required for project docs:

```
□ UML or flow diagram is accompanied by examples/simulations for the edges
□ No-code algorithms name the actual class and method/function
□ The text explains parameters and outputs before showing code quotations
□ Code quotations are short and tied to a "why this matters" explanation
□ Test evidence is separated by scope: unit, integration (e.g. WebApplicationFactory), and full end-to-end
□ No mojibake or broken encoding markers such as "Ð", "Ñ", "Â", or "â€™" (relevant for multilingual board strings)
□ No code-style status noise replaces a human explanation
```

This gate must reject any document that lists only technical route/method ids
without human names, shows examples without explaining the expected response, or
hides the failure-path / error-code boundary.

---

## Section 6 — Human-Readable Execution Board Gate

Every long plan execution status sent to Luba must include a progress board in plain
English unless the user requests another language (the human-first status rule is binding from
`AGENTS.md`/`CLAUDE.md`; this gate makes it enforceable):

```text
Total: <total major plan blocks and/or current-phase tasks>
Done: <completed count>/<total>; <completed names>
In progress: <current task and owner/sub-agent>
Remaining: <remaining count>; <remaining names>
Current result type: <code | artifact | review | policy | evidence-only | commit boundary>
Accepted evidence: <accepted evidence in human words>
Claims that remain closed and why: <claim names + human reason they remain closed>
Open gaps: <open gaps>
Next action: <next bounded action>
Execution paused for plan-review visibility: yes/no
```

If the board is missing, the status is not readable enough even when STATE, JSON,
or validator output exists. The board must appear in the user-visible message
before or alongside machine evidence.

The visible board is execution control, not a pause surface. If the board says
`remaining_count > 0` and a next bounded action exists, the readable output must
say that this is interim status and the parent must immediately continue by
starting, waiting for, reviewing, or repairing the next bounded sub-agent work
order. A final-style summary is readable only when Luba explicitly
stops/pauses/asks for answer-only or all active requirements are complete. Commit
or push of an evidence packet or status board is not a completion boundary while
the board says remaining groups exist.

### No Machine Log To User Rule

User-facing progress cannot be only a STATE flag such as `goal_reached=true`, a
JSON object, or an internal status label. Internal labels may appear only with a
human translation:

```text
Check: what was checked
Result: what passed or failed
What was fixed: repair made or NONE
What remains: remaining gap or NONE
```

If a status cannot be understood without opening raw logs, return it to
readability repair.

---

## Section 7 — Architecture Audit Readability

Architecture, code-review, documentation, and audit artifacts must also pass the
Human-Readable Architecture Artifact Gate (full detail in
`human-readable-architecture-audit-SKILL.md`):

```
□ Primary explanation is human-readable (English unless the user requests another language)
□ Machine labels are secondary, not the explanation
□ UML is PlantUML or Mermaid classDiagram importable to draw.io
□ Data-flow simulations and expected branch outcomes are present
□ Algorithms are explained in no-code human language
□ Code quotes have exact file:line refs and short quotes
□ Every problem quote has an adjacent concrete solution
□ No table-only, shortcode-heavy, schema-first, schema-only, or prompt/spec artifact
```

"Create schema", "wrap in a service", or "add tests" without concrete
classes/records/methods/tests/data rows is not a solution. Return the artifact to
governance/authoring repair before accepting it.

---

## Section 8 — Sub-Agent Firewall Report Readability

Reports about sub-agent-only repair must make the role boundary visible in plain
language. A readable report cannot hide parent sidecar work behind phrases such as
"I verified locally", "while the agent worked", or "read-only inspection" without
saying when the inspection happened.

```
□ The report names the bounded sub-agent id that authored the changed files
□ The report lists exact changed files
□ The report says the parent did not read/analyze governed files before the packet returned
□ Read-only verification is described as after-return verification
□ Exact files, not broad folder grep, prove the firewall repair
□ Sub-agent waiting is not described as a pause or completion boundary
```

Required exact phrases: `parent local sidecar work ban`;
`read-only verification after a sub-agent packet returns`;
`sub-agent waiting is not a stop`; `no local non-overlapping work escape`.

---

## Section 9 — Audit Sufficiency Wording

Readable architecture/audit prose must not overclaim completion.

```
□ "v1 scope completed" names the exact bounded scope
□ "audit sufficient/complete" appears only after a green breadth challenge
□ "достаточно" ("sufficient"), "готово" ("ready"), or "закрыто" ("closed") does not describe a narrow audit as broad
□ Parent review asks: "What would a broader audit likely find that this artifact missed?"
□ Later same-domain BLOCKING/HIGH findings create a Delta-Severity Feedback Gate record
```

If any row fails, the confidence boundary is false or missing. Return it to
governance/review repair before accepting the artifact.

---

## Anti-Patterns

1. **"Shortcodes are faster to write"** — shortcodes are faster for the generator;
   they are slower for the reader. The gate protects review efficiency.
2. **"It's obvious what CF-12 means in context"** — shortcodes are only obvious
   after reading the reference table. Prose must be self-explanatory.
3. **"I'll expand it later"** — the gate runs before output is accepted. Expand at
   generation time.
4. **"The code fence exception covers this"** — inline single backticks are allowed
   for identifiers within a sentence; ALL_CAPS_UNDERSCORE as a bare prose word is not.
5. **Machine board only** — a STATE/JSON status without the English-first Execution
   Board and human translation fails Section 6.

---

## Integration

- `context-package-design-SKILL.md` prose sections must pass this gate
- `agent-output-format-SKILL.md` Human-Readable Execution Board gate uses Section 6
- `human-readable-architecture-audit-SKILL.md` governs the architecture-audit readability, sufficiency boundary, breadth challenge, and delta-severity gates referenced in Sections 7 and 9
