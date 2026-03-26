# Halt and Escalate

This document is used when the agent is stuck and cannot proceed after 3 attempts.
It is also the record of all past escalations and their resolutions.

---

## When to Use This Document

Trigger the halt-and-escalate process when **any of the following are true**:

1. The same error has occurred 3 times despite 3 different fix attempts
2. A compile error involves a dependency version conflict that cannot be resolved without package changes
3. An integration test consistently fails in a way that suggests a fundamental design mismatch
4. The action list has all P1 and P2 items either complete or blocked, with no clear path to unblocking them
5. A security concern is found that requires an architectural decision (e.g., token storage model is wrong)
6. Conflicting requirements are discovered (e.g., "user-scoped repos" conflicts with existing tests that assume global repos)

---

## Halt Protocol

When the halt condition is triggered:

### Step 1 — Stop all changes
Do not make any further code changes. Leave the codebase in a compilable state.
If the codebase is currently broken, make the minimum fix to restore compilation, then stop.

### Step 2 — Document the blocker

Add an entry to the **Escalation Log** section below using the template:

```markdown
### ESC-[NNN] — [Short Title]

**Date**: YYYY-MM-DD
**Triggered by**: Action ID [P1-XXX] / [Rule 7 — third failed attempt]
**Compile status at halt**: Clean / Broken (specify what is broken)
**Test status at halt**: X passing, Y failing (list failing tests by name)

**What I was trying to do**:
[One paragraph explaining the goal of the action item]

**Attempt 1**:
- Approach: [what was tried]
- Result: [exact error message or test failure]
- Why it failed: [hypothesis]

**Attempt 2**:
- Approach: [what was tried]
- Result: [exact error message or test failure]
- Why it failed: [hypothesis]

**Attempt 3**:
- Approach: [what was tried]
- Result: [exact error message or test failure]
- Why it failed: [hypothesis]

**Root cause hypothesis**:
[Best guess at the underlying problem. Be specific.]

**What information would help resolve this**:
[What does a human need to provide, decide, or explain?]

**Impact of remaining blocked**:
[Which other action items does this block? Can work continue elsewhere?]

**Suggested resolution**:
[Recommend an approach — even if you cannot implement it]
```

### Step 3 — Update the action list

In `04-ACTION-LIST.md`:
- Mark the action as `BLOCKED: ESC-[NNN] — [date]`
- Add a line below it: `→ See 07-HALT-AND-ESCALATE.md ESC-[NNN]`

### Step 4 — Resume on a different item

Identify the next highest-priority unblocked action item and resume the loop.
If all items are blocked, document this condition here and stop.

---

## Resuming After Escalation

When a human provides guidance to resolve a blocker:

1. Read the escalation entry
2. Note the suggested resolution from the human
3. Update the escalation entry with `**Resolution**: [what the human said]`
4. Remove the `BLOCKED` tag from the action item in `04-ACTION-LIST.md`
5. Begin a fresh set of 3 attempts with the new guidance

---

## Complete Stop Condition

Stop all work and await human instruction if:

- All action items are blocked
- The codebase cannot compile and you have tried 3 times to fix it
- A security vulnerability is found that would require shipping code with a known exploit
- The database schema is in an inconsistent state (failed migration, orphaned data)

Document the complete stop condition here:

```markdown
## COMPLETE STOP — [Date]

**Reason**: [Why everything stopped]
**Last clean state**: [Git commit hash or description of last working state]
**To resume**: [What a human needs to do to unblock]
```

---

## Escalation Log

*(No escalations yet — this section is populated during development)*

---

## Known Limitations (Not Escalations)

These are known gaps in the current system that are tracked as action items,
not active blockers:

| Issue | Action Item | Notes |
|-------|------------|-------|
| Swagger disabled in Program.cs | P2-001 | Commented out due to compatibility issue — root cause unknown |
| EnsureCreatedAsync disabled | P2-006 | Replaced with MigrateAsync needed |
| GitHub API rate limits not handled with retry | [to be added] | Need Polly or similar |
| Redis cache not implemented | [P3 backlog] | Mentioned in README, not wired |
| Complexity metrics are estimated | [GAP-DOMAIN] | Not from AST parsing |
