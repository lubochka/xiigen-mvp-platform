# Systematic Debugging (XIIGen Enterprise Platform)

## Overview

Random fixes waste time and create new bugs. Quick patches mask the underlying
issue and the symptom returns next phase. This skill installs the Iron Law and
the four-phase protocol that force root-cause investigation before any fix.

Stack-bound to **.NET 10 ASP.NET Core Web API**, **xUnit** (`tests/Xiigen.*.Tests`),
`DataProcessResult<T>` (`Xiigen.Kernel`), the platform fabric interfaces
(`IDocumentStore`, `IIdempotencyStore`, `IOutboxStore`, `ISchedulerService`,
`ITenantContextAccessor`), `MicroserviceBase<TService>`, and ASP.NET Core DI.
Gate-B is `dotnet build Xiigen.Platform.slnx` + `dotnet test` —
`Directory.Build.props` sets `TreatWarningsAsErrors=true`, so a warning is a
build failure. There is no frontend in this repo: no Playwright/Angular steps.

The rules themselves come from `llm_mvp_core`'s systematic-debugging skill (they
are language-independent). The role loop and "executor cannot self-finish" prose
that currently lives only in `AGENTS.md` / `CLAUDE.md` is bound into this file so
the skill is enforceable on its own.

## The Iron Law

```
NO FIXES WITHOUT ROOT CAUSE INVESTIGATION FIRST
```

If you have not completed Phase 1, you cannot propose fixes. A symptom fix is a
failure, not a partial success.

## When to load

- ANY technical issue: test failure, build error, unexpected behavior, wrong
  `DataProcessResult` value, DI resolving the wrong concrete fabric.
- Especially under time pressure, when "just one quick fix" seems obvious, or
  after 2+ fixes that did not work.
- Do NOT skip when the bug seems simple — simple bugs have root causes too, and
  the process is faster than guess-and-check thrashing.

## Sub-Agent-Only Parent Boundary

When the parent/current session is sub-agent-only or architect-only, this
protocol belongs inside the bounded sub-agent work order. The parent must not run
the debugging commands as local sidecar work while a sub-agent runs. Parent may
read governed files/code only after the packet returns, to verify/integrate it.
`sub-agent waiting is not a stop`.

---

## Section 1 — The Four Phases

Complete each phase before proceeding to the next.

### Phase 1 — Root Cause Investigation

1. **Read error output completely.** Read the full `dotnet build` / `dotnet test`
   output — file paths, line numbers, C# error codes (`CS0246`, `CS8602`, …),
   and the xUnit stack trace, which shows the exact call chain.

2. **Reproduce consistently.**
   ```bash
   dotnet test Xiigen.Platform.slnx --filter "FullyQualifiedName~FailingTestName" --no-build
   # Does it fail every time? Intermittent failure points at shared state / async.
   ```

3. **Check recent changes.**
   ```bash
   git diff HEAD~1 -- src/
   git log --oneline -5
   # What changed that could cause this?
   ```

4. **Gather evidence in multi-component systems.** When a request crosses
   contract → service → fabric → DI → test, find WHERE the chain breaks before
   proposing a fix:
   ```bash
   # Layer 1 — does the interface declare the method?
   grep -rn "DataProcessResult.*MethodName" src/Xiigen.Kernel/FabricContracts.cs src/**/I*.cs --include="*.cs"

   # Layer 2 — does the implementation match the interface (same signature)?
   grep -rn "public.*MethodName" src/Xiigen.Platform.Marketplace/ --include="*.cs"

   # Layer 3 — is the fabric registered in DI?
   grep -rn "AddSingleton\|AddScoped\|AddTransient\|AddXiigen" src/Xiigen.Platform.Api/ --include="*.cs"

   # Layer 4 — does the test inject the right concrete fabric?
   grep -rn "IDocumentStore\|InMemoryDocumentStore\|new .*Service(" tests/Xiigen.Platform.Marketplace.Tests/ --include="*.cs"
   ```
   Run once to find the breaking layer → THEN analyze → THEN investigate that
   layer only.

5. **Trace data flow** backward — see `root-cause-tracing-SKILL.md`.

### Phase 2 — Pattern Analysis

1. **Find a working example** — locate a sibling service in
   `src/Xiigen.Platform.Marketplace/` (e.g. `MarketplaceService`,
   `SmartWikiService`) that uses the same fabric correctly.
2. **Compare against it completely** — read the working example end to end; do
   not skim.
3. **List every difference** between working and broken, however small (a missing
   `IsSuccess` guard, a different `collection` string, a wrong `DataProcessResult<T>`
   type argument).

### Phase 3 — Hypothesis and Testing

1. **Form a single hypothesis** — "I think X is the root cause because Y." Write
   it down (`debug-session-SKILL.md` records it).
2. **Test minimally** — the SMALLEST change that tests the hypothesis; one
   variable at a time.
3. **Verify before continuing** — worked? → Phase 4. Did not? → form a NEW
   hypothesis. Do NOT stack more fixes on top.
4. **When you don't know, say so** — "I don't understand X" — do not pretend.

### Phase 4 — Implementation

1. **Create failing test(s) FIRST** — `bug-to-tests-SKILL.md` (three failing
   xUnit tests confirmed failing before the fix).
2. **Implement a single fix** at the root cause. ONE change. No "while I'm here"
   refactoring, no bundled cleanup.
3. **Verify the fix** — `dotnet test Xiigen.Platform.slnx`: target test green, no
   other test broken, count ≥ baseline.
4. **If the fix does not work — STOP and count.** How many fixes attempted?
   - `< 3`: return to Phase 1 with the new information.
   - **`>= 3`: STOP and question the architecture.** Do not attempt Fix #4
     without discussing the architectural problem.

5. **If 3+ fixes failed — question the architecture.** Pattern indicating an
   architectural problem: each fix reveals new coupling/shared state somewhere
   else; fixes require restructuring; each fix creates new symptoms. STOP and
   raise the design question (`root-cause-ladder-SKILL.md` Level 3 /
   `diagnostic-escalation-SKILL.md` escalation triggers) before more fixes.

---

## Section 2 — Red Flags (return to Phase 1)

- "Quick fix for now, investigate later."
- "Just try changing X and see if it works."
- "I don't fully understand but this might work."
- "One more fix attempt" (already tried 2+).
- Each fix reveals a new problem in a different place.

---

## Section 3 — Common Rationalizations

| Excuse | Reality |
|--------|---------|
| "Issue is simple, no process needed" | Simple issues have root causes too. The process is fast for them. |
| "Emergency, no time for process" | Systematic debugging is FASTER than guess-and-check thrashing. |
| "Just try this first, investigate later" | The first fix sets the pattern. Do it right from the start. |
| "I'll write the test after the fix works" | Untested fixes don't stick. Test-first proves it. |
| "Change the assertion so the test passes" | That is accepting wrong output. Fix the service. |
| "Suppress the nullable warning to get green" | A warning is a build break here (`TreatWarningsAsErrors`). Fix it. |
| "One more fix" (after 2+ failures) | 3+ failures = architectural problem. Question the design. |

---

## Integration

- `root-cause-tracing-SKILL.md` — backward tracing through the call chain (Phase 1.5).
- `debug-session-SKILL.md` — persists hypotheses + status across sessions (Phase 3).
- `diagnostic-escalation-SKILL.md` — 4-step Gate-B failure diagnosis when a fix fails.
- `defense-in-depth-SKILL.md` — validate at every layer after the root cause is fixed.
- `bug-to-tests-SKILL.md` — three failing tests before the fix (Phase 4.1).
- `verification-before-completion-SKILL.md` — confirm the fix before claiming success.
- `dev-safety-SKILL.md` — the Gate-B build/test discipline this skill's Phase 4 relies on.
