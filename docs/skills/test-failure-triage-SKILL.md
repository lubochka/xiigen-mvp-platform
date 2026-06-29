# Test Failure Triage (XIIGen Enterprise Platform)

## Overview

Treating all test failures identically — always "fix the production code" — is
wrong when the test itself is fragile, or when the infrastructure (DI,
constructor, interface) changed beneath correct code. This skill classifies every
failure into one of four classes **before** any fix, so the right layer is
repaired.

This is the platform port of the `llm_mvp_core` skill of the same name. The
universal content is the 4-class decision tree, "classify before fixing", and the
**Iron Rule** (changing a test assertion requires Luba's explicit approval). It is
bound to **.NET 10 / xUnit**, `DataProcessResult<T>`, and the platform projects
(`Xiigen.Kernel`, `Xiigen.Fabrics.InMemory`, `Xiigen.Fabrics.Scheduler`,
`Xiigen.Auth`, `Xiigen.Platform.Api`, `Xiigen.Platform.Marketplace`).

This platform is backend-only — there are **no `na` exclusions**; the 4-class
triage applies to every test.

## When to load

- Any time one or more `dotnet test` cases fail.
- Before committing when a pre-commit Gate-B reported a failure.
- After refactoring DI registrations in `Xiigen.Fabrics.*` / `Xiigen.Kernel`
  (Class 2 is especially frequent here).

## Step 1 — Capture the failure report

```bash
dotnet test --no-build 2>&1 | tee /tmp/test-output.txt
# Look for: test names, error messages, stack traces, and the FAILING LINE NUMBER.
grep -E "Failed|Error|   at " /tmp/test-output.txt | head -30
```

The failing **line number** is the key disambiguator between Class 1 and Class 4
(see below).

## Step 2 — Classify each failure

```text
Was this test passing at the start of this session?

  YES (regression) -> Was production code changed this session?
    YES -> CLASS 1: Implementation regression
    NO  -> Was infrastructure / DI / a constructor / an interface changed?
      YES -> CLASS 2: Infrastructure regression
      NO  -> CLASS 4: Test environment failure (investigate setup)

  NO (new test) -> Is the test code correct?
    NO  -> CLASS 3: Test fragility (the test was written wrong)
    YES -> Was the production code supposed to satisfy this test?
      YES -> CLASS 1: Implementation regression (new code broke an invariant)
      NO  -> Review: is this test for the right class?
```

## Class 1 — Implementation regression

**Definition:** production code changed in a way that breaks a previously-passing
test. A behavioural invariant that was satisfied no longer is.

**Detection:**
```bash
git diff --name-only HEAD          # what changed
# Read each changed file completely; find which change broke the invariant.
```

**Response:**
1. DO NOT modify the test — the test is correct.
2. Find what changed in the production class and why the invariant broke.
3. Fix the production class to satisfy the invariant again.
4. Re-run the specific test after the fix.

Class 1 typically fails **at the `Assert` line** (the body ran, the outcome was
wrong).

## Class 2 — Infrastructure regression

**Definition:** a dependency, DI registration, or constructor/interface signature
changed. Production code and test are both correct; the test fails because the
environment changed beneath them. **Most frequent on this platform** when
`Xiigen.Fabrics.*` or `Xiigen.Kernel` registrations are refactored.

**Detection:**
```bash
# DI / startup changes:
git diff --name-only HEAD | grep -iE "Program|Startup|ServiceCollection|ServiceCollectionExtensions"
# Interface / constructor signature changes (all implementors may need updating):
git diff HEAD -- src/ | grep -E "public .*interface |public .*class |public .*\("
```

For example, `XiigenMarketplaceServiceCollectionExtensions.cs` or a fabric
registration changing a lifetime/constructor will fail tests whose fixtures build
a `ServiceCollection` expecting the old shape.

**Response:**
1. DO NOT modify the production algorithm.
2. Fix the infrastructure: update the DI registration, update the interface
   implementation, fix the test fixture setup.
3. Re-run only after the infrastructure fix.
4. Record: "FIXED: infrastructure regression — [what changed]".

## Class 3 — Test fragility

**Definition:** the test was always fragile — it passed by accident (implicit
assumptions) or it tests implementation details instead of behaviour.

**Detection markers:**
- asserts on exact mock call counts (`Times(3)`, `Verify(..., Times.Exactly(n))`);
- asserts on internal state not observable from outside;
- depends on a specific execution order;
- exact-string match on a message that may change;
- mocks the class under test instead of its dependencies.

```bash
grep -rnE "Times\(|Times\.|Verify\(|Invocations|InSequence|MockSequence" tests/ --include="*.cs"
# Behavioural tests assert outcomes (result.Data / result.Error!.Code), not call counts.
```

**Response:**
1. Fix the test to assert behaviour, not implementation detail.
2. **Iron Rule:** changing a test assertion requires Luba's explicit written
   approval (see `bug-to-tests-SKILL.md`).
3. After rewriting, run the test to confirm it passes for the right reason.
4. Record: "FIXED: test fragility — rewritten to assert behaviour".

## Class 4 — Test environment failure

**Definition:** the test **setup** is broken; the test cannot reach the code under
test because DI setup failed, a required service is not registered, or test data
is missing.

**Detection — common xUnit patterns on this platform:**
```text
- System.InvalidOperationException: No service for type 'IDocumentStore' has been registered.
- System.NullReferenceException at the first line of the test body (before any Act).
- System.TypeInitializationException in setup / constructor / IClassFixture.
```

```bash
dotnet test --filter "FullyQualifiedName~<Class>" -- --verbosity detailed 2>&1 | head -50
# Look for setup errors that occur before the test body runs.
```

**Response:**
1. DO NOT modify the production class.
2. DO NOT patch the test body to "work around" it.
3. Fix the setup: register the missing fabric/service in the test `ServiceCollection`,
   provide the missing test data.
4. Confirm by running the test — the class under test should now be reached.

**Key distinction:** Class 4 fails **before the `Act` line** (setup); Class 1
fails **at the `Assert` line** (outcome). Check the line number in the stack
trace.

## Step 3 — Record the classification (before fixing)

```text
Failure triage: [date]

Test: IdempotencyStoreTests.TryReserve_DuplicateKey_ReflectsExisting
  Class: CLASS_3_TEST_FRAGILITY
  Evidence: asserts Verify(..., Times.Once) on an internal call — not a behavioural assertion
  Response: rewrite to assert second.Data!.Key == "op-1" (the outcome), not the call count

Test: PublishLegalDocumentTests.Publish_Valid_ReturnsRecord
  Class: CLASS_2_INFRASTRUCTURE_REGRESSION
  Evidence: IDocumentStore not registered after the DI refactor in XiigenMarketplaceServiceCollectionExtensions.cs
  Response: add the InMemoryDocumentStore registration back to the test ServiceCollection
```

## Anti-patterns

1. **Patching Class 3 by changing production code** — makes the test pass but the
   code is now wrong.
2. **Fixing Class 1 by changing the assertion** — violates the Iron Rule.
3. **Not classifying before acting** — fixing the wrong layer every time.
4. **Treating "test failed" as "code is wrong"** — Class 2/3/4 failures are not
   code bugs.
5. **Committing with unclassified failures** — classification is the first step;
   never skip it. Zero failures is a hard gate before commit.
