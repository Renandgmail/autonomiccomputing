# The Development Loop

Every unit of work follows this loop exactly, in this order.
Do not skip steps. Do not reorder steps.

```
┌─────────────────────────────────────────────────────┐
│  1. PICK         Read action list → select next task │
│  2. IMPLEMENT    Write code                          │
│  3. COMPILE      dotnet build  (hard stop if fail)   │
│  4. REGRESS      dotnet test   (hard stop if fail)   │
│  5. INTEGRATE    Run integration tests               │
│  6. VERIFY       Check DB + logs                     │
│  7. REVIEW       Stakeholder perspective pass        │
│  8. DOCUMENT     Update action list                  │
│  9. PRIORITISE   Re-read list → pick next task       │
└─────────────────────────────────────────────────────┘
```

---

## Step 1 — Pick the Next Action

Open `04-ACTION-LIST.md`.

Find the first item that is:
- Status: `[ ]` (not started) or `[~]` (in progress)
- Not marked `BLOCKED:`
- Has the highest priority score (P1 > P2 > P3)

Read the action's **Done Condition** carefully. You must be able to state in one sentence
what observable, testable outcome means this action is complete.

Write the action ID and done condition here (mentally):
> "I am working on action **[ID]**. It is done when **[done condition]**."

If no unblocked action exists, read `07-HALT-AND-ESCALATE.md` and stop.

---

## Step 2 — Implement

Write the minimum code needed to satisfy the done condition.

Guidelines:
- Follow the architecture in `03-ARCHITECTURE.md`
- If the change requires a new interface, define it in `RepoLens.Core`
- If the change requires a new database column, add a migration — never edit existing migrations
- If the change requires a new UI component, place it in the correct component subfolder
- Do not refactor unrelated code in the same pass
- Do not add dependencies not already in the project without an explicit action item for it

When implementing a new provider:
- Implement `IGitProviderService` in `RepoLens.Infrastructure/Providers/{ProviderName}/`
- Register it in `Program.cs` via the provider factory pattern
- Add URL pattern to `RepositoryValidationService`

---

## Step 3 — Compile Gate

```bash
# Backend
dotnet build Repolense/RepoLens.sln

# Frontend (if any .tsx/.ts files changed)
cd repolens-ui && npm run build --silent
```

**Exit code must be 0 for both.**

If compile fails:
1. Read the full error output
2. Fix the error — do not suppress it with `#pragma warning disable` or `!` null-forgiving operators unless there is a documented reason
3. Re-run compile
4. Do not proceed to Step 4 until compile is clean

---

## Step 4 — Regression Gate

```bash
dotnet test Repolense/RepoLens.sln \
  --no-build \
  --logger "console;verbosity=normal" \
  --results-directory ./TestResults
```

Expected outcomes:
- All tests that passed before this iteration still pass
- New tests you wrote for this action now pass (if they were written)
- Test count may increase; failure count must not increase for pre-existing tests

If a regression appears:
1. Identify exactly which pre-existing test broke
2. Understand why (your change broke a contract, a mock expectation, a data shape)
3. Fix the code (not the test, unless the test was testing wrong behaviour)
4. Re-run from Step 3
5. If you cannot fix it after 3 attempts → Rule 7 (Three-Strike Halt)

---

## Step 5 — Integration Tests

Integration tests live in `RepoLens.Tests/Integration/`.

Run only the integration tests relevant to the feature you just implemented:

```bash
dotnet test Repolense/RepoLens.Tests \
  --no-build \
  --filter "Category=Integration" \
  --logger "console;verbosity=detailed"
```

If no integration test exists yet for this feature, **you must write one** before marking the action complete. See `05-INTEGRATION-TEST-SPEC.md` for the exact specification format.

Integration tests for a new feature must cover:
1. The happy path (success case)
2. At least one failure path (invalid input, permission denied, resource not found)
3. Idempotency where relevant (adding the same repo twice)

---

## Step 6 — Verify Database and Logs

After integration tests pass, manually (or programmatically) verify:

### Database verification

Connect to the test database and run the verification query for your scenario.
Each integration test spec in `05-INTEGRATION-TEST-SPEC.md` includes the exact SQL to run.

```sql
-- Example: verify a repository was persisted correctly
SELECT id, name, url, provider_type, status, created_at
FROM repositories
WHERE url = 'https://github.com/owner/repo'
ORDER BY created_at DESC
LIMIT 1;
```

Expected: the row exists, `provider_type` is correct, `status` is not `Failed`.

### Log verification

Confirm the key log lines were emitted. Integration tests should capture logs via
`ITestOutputHelper` or a test logger sink. Verify at minimum:

- The operation-start log (`LogInformation("Starting metrics collection for {Owner}/{Repo}", ...)`)
- No unexpected `LogError` or `LogCritical` entries
- The operation-complete log with correct counts

If logs show errors that the test did not catch via assertions, investigate — do not ignore.

---

## Step 7 — Stakeholder Review

After verification passes, perform a rapid review pass using `06-STAKEHOLDER-REVIEW-TEMPLATE.md`.

This is a **checklist scan**, not a deep analysis. Spend 5–10 minutes maximum.
The goal is to catch obvious gaps before moving on.

For each perspective (UX, Domain, Architect, DevOps):
- Go through the checklist items relevant to the feature you just built
- If a gap is found, create a new action item in `04-ACTION-LIST.md` with the label `[GAP-UX]`, `[GAP-DOMAIN]`, `[GAP-ARCH]`, or `[GAP-DEVOPS]`
- Do **not** fix the gap now — log it and move on

---

## Step 8 — Update Action List

Open `04-ACTION-LIST.md` and:

1. Mark the completed action as `[x]` with today's date: `[x] 2026-03-26`
2. Add any new action items discovered during this iteration (regressions found and fixed count as discoveries if they revealed a design gap)
3. Add any gaps found in Step 7 as new action items
4. If the action was blocked and you are now documenting the block, update the status to `BLOCKED` with the date and a reference to `07-HALT-AND-ESCALATE.md`
5. Update the "Last Updated" timestamp at the top of the file

---

## Step 9 — Prioritise and Loop

Re-read `04-ACTION-LIST.md` from top to bottom.

Ask:
- What is the highest-priority unblocked item?
- Does it depend on anything that was just completed?
- Are any blocked items now unblocked because of what was just done?

Select the next action and return to Step 1.

**Stop the loop when:**
- All P1 and P2 actions are marked complete or blocked, AND
- All blockers have been documented in `07-HALT-AND-ESCALATE.md`, AND
- A stakeholder review has been completed for the last batch of completed work

---

## Loop State Tracker

Use this section at the top of each working session to record where you are:

```
SESSION START: [date/time]
CURRENT ACTION: [action ID from 04-ACTION-LIST.md]
BASELINE TESTS: [X passing, Y failing]
COMPILE STATUS: [clean / errors]
LAST COMPLETED ACTION: [action ID]
```
