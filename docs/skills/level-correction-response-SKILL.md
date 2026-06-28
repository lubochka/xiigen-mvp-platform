# level-correction-response-SKILL.md — Abandon the frame, rebuild one level up

> Ported from the `llm_mvp_core` governance standard, adapted for the XIIGen
> Enterprise Platform: **.NET 10 ASP.NET Core Web API**, xUnit, Gate-B =
> `dotnet build` / `dotnet test`, domain return type `DataProcessResult<T>`
> (`src/Xiigen.Kernel/DataProcessResult.cs`) — **not** core `OperationResult<T>`.
> Capability-over-provider naming applies to ASP.NET Core services and their DI
> registrations in `Program.cs`. Real capability names in this repo:
> `IDocumentStore`, `IOutboxStore`, `IIdempotencyStore`, `ISchedulerService`,
> `ITenantContextAccessor`, `IFreedomConfigService`.

## Core problem
When a proposal is built at the wrong abstraction level — "new C# service" when
the answer is "extend an existing interface" — adding more detail to the wrong
proposal makes it worse, not better. The correct response to a level challenge is
to abandon the current frame entirely and rebuild from one level higher. This
skill defines the three proactive tests that catch wrong abstraction levels
before they are embedded in a plan, and the single rule for responding when a
challenge fires.

---

## When to invoke
- BEFORE drafting any plan, phase description, or component design (proactive scan).
- When Luba says "you're thinking as X, not as Y" (reactive correction).
- When "add another layer" or "try again" is the response to a proposal.
- When the same problem is solved twice instead of once.

Load order: before all other planning skills. A session where a level challenge
is met with elaboration rather than frame abandonment has failed before any
checklist fires.

---

## Section 1 — Three proactive tests (run before planning starts)

### Test 1 — Next Instance Test
**Trigger words:** "for each flow," "per-endpoint," "maintain a list of,"
"pre-authored," "one per tenant," "per provider."
**Ask:** if one more instance of this case arrives tomorrow, what happens?
- Manual work required again → **WRONG ABSTRACTION LEVEL** — find the general capability.
- Solution runs automatically → proceed.
```
FAILS: hand-write a scheduler entry for each flow
       → new flow added → manual work again → unbounded

PASSES: a flow schedules through ISchedulerService.ScheduleDelayedAsync
       → new flow added → it just calls the interface → bounded
```

### Test 2 — Provider vs Capability Test
Apply to every interface name, class name, configuration key, and DI registration.
**The test:** does this name identify a CAPABILITY (what it does) or a PROVIDER (what it uses)?
```
FAILS (provider names):        PASSES (capability names):
IRedisService                  ISessionStorage / IIdempotencyStore (scoped, expiring)
IElasticsearchService          IDocumentStore (document persistence capability)
IRabbitMqService               IOutboxStore (reliable event-emission capability)
IQuartzService                 ISchedulerService (delayed-work capability)
AwsSecretsManagerService       (no provider service — secrets come from config; No-Secrets MACHINE)
```
A class named for its provider cannot be swapped without renaming every caller.
A class named for its capability can swap providers (the DI registration in
`Program.cs`) without touching callers.

### Test 3 — Old Model Survival Test
**Trigger:** any time a reframe was established in this session ("the correct model is X instead of Y").
After any architectural reframe, scan the entire plan for surviving remnants:
- Provider names where capability names should be.
- Concrete class names where interface names should be.
- Per-instance solutions where a general mechanism should be.
- Direct `new ConcreteStore()` where constructor injection should be.

Even after a reframe is established, the old model has at least one hiding place
in the remaining plan text. Find it before writing session files.

**Rule:** do not begin writing any file until all three tests return clean.

---

## Section 2 — The one rule for responding to a challenge

**When Luba challenges the LEVEL of a proposal — not the content — abandon the
current frame entirely and rebuild from one level higher.**

Do NOT:
- Revise the current proposal with additions.
- Add a peer proposal alongside the current one.
- Explain why the current level was reasonable.
- Run the current proposal through more checks.
- Defend by citing the skill that produced it.

DO:
- Stop. Go silent on the current frame.
- Ask: what is one level above what I just proposed?
- Build from there.

---

## Section 3 — Distinguishing level correction from detail correction
**Detail correction:** "That's wrong — the method is
`TryReserveAsync(operationKey, ttl)` not `Reserve(key)`."
→ Revise the specific fact. Stay at the same level.

**Level correction:** "You're still thinking as a utility writer, not as an
interface designer."
→ Abandon the entire frame. The problem is the perspective, not a fact.

**The signal:** Luba says what you ARE, not what you got wrong.
"You're thinking as X" = level correction. Go up.

Other level-correction signals:
- "Another layer" — you proposed a new class; the answer is a convention.
- "You don't see the abstraction" — the solution exists at a higher generalization.
- "Try again" after you already added detail — detail is not the answer.
- "That's still the same thing" after you added an alternative.

---

## Section 4 — "Going up" in practice

```
Proposed: new ILoggingService fabric interface + concrete LoggingService
Challenge: "you're designing a new dependency, but the issue is a cross-cutting concern"

Wrong response: improve the interface design
Wrong response: add a logging extension method alongside the interface

Correct response:
  What is one level above "how do I add logging to this service?"
  → "How do cross-cutting concerns get added to services in this platform?"
  → The answer is ASP.NET Core middleware / DI decorators + structured logging,
    a convention, not a new fabric interface.
```

---

## Section 5 — Adding a layer is defending the frame
```
❌ "You're right that IConfiguration works — and we could also have a
   FreedomConfigService wrapper for cases where we need typed access..."
   → This keeps the wrapper. It adds a condition that makes it seem reasonable.
     The frame has not changed; a layer has been added.

✅ "IFreedomConfigService is the right abstraction for tenant-tunable values.
   No extra wrapper. four-tier-decision-classification governs whether each value
   is MACHINE, FREEDOM, or LEARNABLE."
   → The frame is gone. The answer is at the correct level.
```

---

## Anti-patterns
1. **"I added the provider name in the implementation, not the interface"** — if
   the provider name lives anywhere in a plan, the old model survived. Test 3 catches it.
2. **"The reframe was about Y, not Z"** — every reframe propagates; it touches every class name.
3. **"I'll apply the abstraction next phase"** — a plan that starts at the wrong
   level embeds it in Phase 1 artifacts. Correct before drafting.
4. **"Explaining why the previous frame was reasonable"** — when the level is wrong, the explanation is wasted. Abandon and rebuild.

---

## Integration
- `solution-scope-gate-SKILL.md` defines the 5-level scope ladder; this skill detects when a proposal is at the wrong level.
- `four-tier-decision-classification-SKILL.md` Q1–Q4 resolves Test 1 for new capability proposals.
- `freedom-machine-SKILL.md` reinforces interface=MACHINE / implementation=FREEDOM naming.
