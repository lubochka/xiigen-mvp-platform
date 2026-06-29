# no-product-decisions-SKILL.md — When to stop vs self-resolve

> Ported from the `llm_mvp_core` governance standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build` / `dotnet test`, domain return type `DataProcessResult<T>`
> (`src/Xiigen.Kernel/DataProcessResult.cs`) — **not** core `OperationResult<T>`.
> Bound to the `AGENTS.md` v3.3 Enterprise Adaptation Beacon and the role loop
> `PLAN_AUTHOR_INTERNAL → PLAN_REVIEW_INTERNAL → ARCHITECT_INTERNAL →
> EXECUTOR_INTERNAL → REVIEW_INTERNAL → ARCHITECT_INTERNAL_REPAIR →
> EXECUTOR_INTERNAL_CONTINUE`. This skill **lifts the `CLAUDE.md` "Executor
> Cannot Stop The Plan" / `AGENTS.md` "No Secrets In Repo" prose into a checkable
> escalation gate.** This repo has **no frontend** (ASP.NET Core Web API + xUnit,
> despite "angular" in the repo name), so "user-facing" means the public HTTP
> API contract, not a screen.

## When to load
- Load for ARCHITECT / PLAN_AUTHOR sessions.
- Load when a plan contains two design alternatives, or a default value for a
  caller-configurable parameter must be picked.

## The rule
If a plan requires choosing between two approaches that affect:
- public HTTP API behaviour (endpoint shape, status code, contract a consumer sees);
- public DTO / contract shape returned as `DataProcessResult<T>`;
- architectural direction (which fabric, dependency direction, module boundary);
- a locked decision in `docs/DECISIONS-LOCKED.md` (create the file if missing);
- a tenant-isolation, security, or No-Secrets boundary;

→ STOP. Present both options with pros/cons. Wait for Luba's decision.
→ Do not self-select. Do not pick the "obviously better" one.

A STOP here is an internal escalation, not a final report: per `CLAUDE.md`
"Executor Cannot Stop The Plan", record the open decision and continue any other
authorized bounded work that does not depend on it. A product decision that
blocks the whole plan is an `OPERATOR_INPUT_REQUIRED` row, surfaced with the
human-readable status block, not a silent self-resolution.

## What counts as a product decision
- Choosing between two architectural approaches (e.g. outbox-then-scheduler vs
  inline dispatch).
- Adding or removing a feature / endpoint.
- Choosing a default for a caller-configurable parameter (a FREEDOM value —
  see `freedom-machine-SKILL.md`; the seed default belongs in
  `IFreedomConfigService`, the choice of *which* default is the product decision).
- Deciding which module / flow to work on next.
- Any choice that would require updating `docs/DECISIONS-LOCKED.md`.

## Enterprise Forbidden Decisions (escalate to Luba — never self-decide)

| Forbidden | Why |
|-----------|-----|
| Commit a raw AWS key / GitHub token / Vault value / JWT signing key / tenant secret | `AGENTS.md` "No Secrets In Repo" — MACHINE invariant, evidence records redacted identifiers only |
| Change a public Web API endpoint or DTO signature | Breaks all consumers |
| Relax the tenant-scope invariant (`ITenantContextAccessor`) for convenience | Cross-tenant leak is a security violation |
| Skip a required Gate-B (`dotnet build`/`dotnet test`) for any reason | No exceptions |
| Add a new NuGet dependency | Dependency policy is product scope |
| Change the domain return type away from `DataProcessResult<T>` | Cross-assembly contract |
| Create a new project (`*.csproj` / `.slnx` entry) | Architecture decision |

## Exception: purely implementation details
Minor internal choices (private variable names, internal helper structure, test
helper naming) may be self-resolved without stopping.
The test is: **"would changing this require updating `docs/DECISIONS-LOCKED.md`,
a public endpoint/DTO, or a security/tenant boundary?"** If yes → stop. If no → proceed.

## Tool approval boundary (external-AI ban, R7)
Architect must not self-select an external AI tool or unrelated external service
as an implementation shortcut. If the current work order or approved plan does
not explicitly require that exact tool family, **explicit Luba approval is
required before use.** Cross-reference `agent-constitution` / R7.

## What to do when a product decision is encountered
1. Stop at that point in the plan (internal — keep other authorized work moving).
2. Present the two (or more) options with pros/cons and the exact contract/security impact.
3. Wait for Luba's decision.
4. Proceed after explicit approval; capture it via `decision-capture-SKILL.md`.

## Three anti-patterns (from behavioral corrections)
1. Declaring an API contract change "obviously correct" without asking Luba.
2. Marking a phase done when its Gate-B (`dotnet test`) is not green.
3. Processing a mid-session file/endpoint without asking whether to switch tasks.

## Integration
- `decision-capture-SKILL.md` records the approved decision into `docs/DECISIONS-LOCKED.md`.
- `decision-reopening-SKILL.md` governs reopening an already-locked decision.
- `solution-scope-gate-SKILL.md` distinguishes a product decision from a scope-level choice.
