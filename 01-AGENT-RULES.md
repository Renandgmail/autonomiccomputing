# Agent Operating Rules

> These rules are **non-negotiable**. They apply to every loop iteration without exception.
> If a rule conflicts with a user instruction, apply the rule and record the conflict
> in `04-ACTION-LIST.md` under a new `CONFLICT:` note before proceeding.

---

## Rule 1 — Compile Gate (Hard Stop)

After every code change, run:

```bash
dotnet build Repolense/RepoLens.sln
```

**If exit code is not 0, stop immediately.**
Do not run tests. Do not move to the next step.
Fix the compile error before doing anything else.
This is not optional. A broken build invalidates all subsequent steps.

For frontend changes, also run:
```bash
cd repolens-ui && npm run build
```

---

## Rule 2 — Regression Gate (Hard Stop)

After a clean compile, run the full test suite:

```bash
dotnet test Repolense/RepoLens.sln --no-build --logger "trx;LogFileName=test-results.trx"
```

Compare the test results against the baseline recorded at the start of the iteration.

**If any previously-passing test now fails, stop immediately.**
Do not proceed to integration tests. Do not mark the action as complete.
Investigate the regression, fix it, re-compile, re-run tests.
Only proceed when the failing count is ≤ the count at the start of this iteration.

New failures on new tests you just wrote are acceptable — fix them.
Failures on *pre-existing* tests are regressions — fix them unconditionally.

---

## Rule 3 — Atomic Changes

Each loop iteration works on **exactly one action item** from `04-ACTION-LIST.md`.
Do not bundle unrelated changes. If you spot something broken while working on item A,
add it to the action list as item B. Do not fix it silently in the same commit.

Exception: if item B is a compile error blocking item A, fix it as part of item A
and note it in the action list.

---

## Rule 4 — Integration Test Verification

Integration tests must verify **three things**, not just HTTP status codes:

1. **Database state** — query the database directly (not via API) and confirm the expected rows/values exist
2. **Log presence** — confirm the expected log entries were written (use test log capture or check log output)
3. **Business invariants** — confirm the result is semantically correct (e.g., a health score of 0 for an empty repo is wrong even if the HTTP call returned 200)

If an integration test cannot verify database state, it is not an integration test — it is an API smoke test. Label it correctly.

---

## Rule 5 — No Silent Workarounds

If something is currently disabled (e.g., Swagger is commented out in `Program.cs`, or `EnsureCreatedAsync` is commented out), you must:

1. Understand **why** it was disabled (read the comment, check git blame if available)
2. Either fix the root cause and re-enable it, or add an action item explaining what is needed
3. Never leave a disabled feature in place without a corresponding action item explaining the path to re-enabling it

---

## Rule 6 — Provider-Agnostic by Default

The original codebase assumed GitHub. Every new piece of code you write must treat
the repository source as a pluggable provider. The current providers to support:

- **GitHub** (API-based, existing code to keep and refactor)
- **GitLab** (API-based, new)
- **Bitbucket** (API-based, new)
- **Azure DevOps** (API-based, new)
- **Local** (file-system path, LibGit2Sharp, already partially wired)

Concretely: no class, method, or variable name should contain `GitHub` unless it is specifically and exclusively about GitHub. Use `RemoteProvider`, `ProviderType`, `IGitProviderService`, etc.

---

## Rule 7 — Three-Strike Halt

If you attempt to fix a problem and fail 3 times in a row:

1. Stop immediately
2. Document the problem in `07-HALT-AND-ESCALATE.md` with full context:
   - What you were trying to do
   - All three approaches tried
   - The exact error message on the third attempt
   - What you believe the root cause is
3. Mark the action item as `BLOCKED` in `04-ACTION-LIST.md`
4. Move to the next highest-priority unblocked action item

Do not spend a 4th attempt without explicit human instruction to resume.

---

## Rule 8 — Stakeholder Review Does Not Block Progress

After every implementation cycle, conduct the stakeholder review (see `06-STAKEHOLDER-REVIEW-TEMPLATE.md`).
Gaps found during review are recorded as new action items in `04-ACTION-LIST.md` with the
appropriate stakeholder label and priority. They are **not fixed in the same iteration**.
This is intentional — it prevents scope creep and keeps the loop tight.

---

## Rule 9 — Documentation Is Code

Every public API method, interface, and service class must have XML doc comments.
Every new integration test must have a one-line comment explaining what user journey it represents.
Every migration must have a comment explaining what schema change it introduces and why.

If you add code without documentation, the action item for that code is not complete.

---

## Rule 10 — Secrets Are Never Hardcoded

No API tokens, passwords, connection strings, or JWT keys may appear in source code.
All secrets come from:
- `appsettings.Development.json` (git-ignored, local only)
- Environment variables
- A secrets manager (for production)

If a test requires a real GitHub token, it must be skipped when the token is absent,
not failed. Use `[Fact(Skip = "Requires GITHUB_TOKEN env var")]` or the equivalent
`Theory` with `Skip` when the env var is null.

---

## Pre-Flight Checklist (run mentally before every loop)

- [ ] I have read `04-ACTION-LIST.md` and know exactly which action I am working on
- [ ] I know the **done condition** for this action
- [ ] I know which tests currently pass (baseline)
- [ ] I know what database state to expect after this action succeeds
- [ ] I have not started changing more than one action item at once
