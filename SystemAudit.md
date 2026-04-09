# AGENT STANDING INSTRUCTIONS FOR SYSTEM AUDIT, DOCUMENTATION, CLEANUP, AND VALIDATION

## Purpose

This document defines the standing instructions that must be followed by the agent in every session and for every prompt related to this repository.

The objective is to keep the system clean, consistent, traceable, and production-ready while avoiding unnecessary code or document growth.

The agent must focus on:
- completing existing functionality,
- removing redundant or unused artifacts,
- improving integration and UX flow,
- aligning code, UI, and database,
- maintaining strong build and test discipline,
- and keeping documentation organized from different stakeholder viewpoints.

No new business feature should be introduced unless explicitly asked.

---

## Standing Instruction for Cline or Any Agent Tool

These instructions must be treated as permanent and session-wide.

The agent must obey the following in every session:
- follow this document before taking any action,
- read existing code and documents before making changes,
- avoid unnecessary assumptions,
- avoid hardcoded values,
- avoid fallback logic unless absolutely required,
- log every fallback if used,
- validate build success after major code changes,
- run integration tests after build success,
- analyze failures and document them in markdown,
- keep changes traceable through IDs and references,
- remove unused files only after checking whether they still serve as source-of-truth inputs,
- think from the correct stakeholder perspective for each task,
- keep context lean and avoid repeating content unnecessarily,
- and preserve or improve existing behavior at all times.

The agent must not treat a new prompt as isolated. Every prompt is part of the same ongoing system audit and cleanup discipline.

---

## Working Principles

### Core Constraints
- Do not add new business functionality.
- Only do cleanup, refactoring, alignment, integration, validation, or documentation rationalization.
- No hardcoding.
- No silent fallback mechanism.
- If fallback is required, it must be explicitly logged.
- Preserve or improve current behavior.
- Prefer reuse over duplication.
- Prefer enrichment over deletion where useful logic exists.
- Prefer clarity over cleverness.

### General Decision Rule
For every item found in code or documentation:
1. Check whether it is used in a real user or system flow.
2. Check whether it contains unique value or logic.
3. Check whether it is redundant or obsolete.
4. If it still adds value, integrate or refactor it.
5. If it does not add value, remove it safely.

---

## Required Stakeholder Thinking

The agent must think from different stakeholder viewpoints depending on the task.

### Architect View
Use this when assessing:
- module boundaries,
- coupling,
- maintainability,
- layering,
- ownership,
- long-term consistency.

### UX Expert View
Use this when assessing:
- screen placement,
- discoverability,
- user flow,
- naming,
- feedback states,
- interaction simplicity,
- whether the feature is visible where it should be.

### Backend Developer View
Use this when assessing:
- API correctness,
- reuse,
- service boundaries,
- validation,
- DTO flow,
- mapping,
- performance,
- dead code.

### QA / Test Engineer View
Use this when assessing:
- integration coverage,
- negative paths,
- error handling,
- contract stability,
- regressions,
- build and runtime confidence.

### DevOps View
Use this when assessing:
- build and compilation,
- execution scripts,
- service startup,
- environment assumptions,
- test run stability.

### Documentation Owner View
Use this when assessing:
- whether documents are still source-of-truth,
- whether documents reflect current implementation,
- whether duplicate or stale markdown files should be removed,
- whether documentation should be split by stakeholder viewpoint.

---

## Required Document Organization

The repository must maintain a clean and minimal documentation structure.

The documentation must be split by purpose and stakeholder viewpoint.

### Recommended Root Structure

/system-audit/
  /analysis/
  /actions/
  /decisions/
  /failures/
  /docs/
  /cleanup/

### Recommended Documentation Structure

/docs/
  /requirements/
  /ux/
  /solution/
  /features/
  /stories/
  /pending/
/cleanup/

### Purpose of Each Folder

#### /docs/requirements/
Contains requirement-level documents written from a business or analyst perspective.

These documents explain:
- what the system should do,
- why the capability exists,
- what is in scope,
- what is out of scope,
- what should remain stable.

Examples:
- requirements overview
- requirement gaps
- requirement mapping to implementation

#### /docs/ux/
Contains UX-focused documents written from the user experience perspective.

These documents explain:
- user journeys,
- screen placement,
- interactions,
- visibility,
- discoverability,
- feedback states,
- workflow issues.

Examples:
- UX review
- screen placement notes
- flow improvement suggestions
- UX gap list

#### /docs/solution/
Contains high-level solution design documents.

These documents explain:
- architecture intent,
- component boundaries,
- integration approach,
- data flow,
- API flow,
- system interactions.

Examples:
- high-level architecture
- system flow
- integration design
- service interaction overview

#### /docs/features/
Contains high-level feature documents.

These documents describe:
- existing features,
- implemented features,
- feature grouping,
- feature status,
- feature ownership.

Examples:
- feature catalog
- feature-to-API mapping
- feature coverage summary

#### /docs/stories/
Contains story-title-only documents.

These documents are intentionally lightweight and should list only:
- story titles,
- story IDs,
- brief status,
- source references.

No deep narrative should be stored here.

#### /docs/pending/
Contains pending tasks and unresolved items based on:
- existing markdown files,
- current codebase,
- unresolved audit findings,
- test failures,
- missing integrations,
- partial implementations.

This folder is the current backlog view.

#### /cleanup/
Contains documentation and code cleanup findings.

Examples:
- unused markdown files
- obsolete code files
- duplicate artifacts
- stale references
- files proposed for deletion

---

## Required Document Types and Their Structure

The agent must maintain the following document types and keep them updated from existing markdown files and from code reality.

---

### 1) Requirement Document

#### File Example
/docs/requirements/REQUIREMENTS-OVERVIEW.md

#### Structure
# Requirements Overview

## Purpose
## Scope
## Business Objective
## Existing Capability Summary
## Gaps
## Risks
## Decisions
## Traceability to Code
## Traceability to UI
## Traceability to Tests
## Pending Requirement Items

#### Rules
- Must remain at a high business level.
- Must not become implementation-heavy.
- Must reflect what the system should achieve.
- Must be updated based on current code and existing markdown files.

---

### 2) UX Document

#### File Example
/docs/ux/UX-REVIEW.md

#### Structure
# UX Review

## Purpose
## Target Users
## Current User Flow
## Screen-by-Screen Observations
## Visibility Issues
## Placement Issues
## Naming Issues
## Interaction Issues
## Feedback State Issues
## UX Recommendations
## Pending UX Tasks

#### Rules
- Must be written from a UX expert standpoint.
- Must mention where a feature should be shown and why.
- Must highlight friction and inconsistency.
- Must not include low-level code details unless needed to explain UX behavior.

---

### 3) High-Level Solution Document

#### File Example
/docs/solution/HIGH-LEVEL-SOLUTION.md

#### Structure
# High-Level Solution

## Purpose
## Problem Statement
## Current System View
## Major Components
## API Flow
## UI Flow
## Data Flow
## Integration Points
## Constraints
## Assumptions
## Risks
## Decision Summary
## Open Items

#### Rules
- Must remain architecture-focused.
- Must explain system shape, not implementation detail.
- Must be aligned with actual code behavior and actual system structure.

---

### 4) High-Level Features Document

#### File Example
/docs/features/FEATURE-CATALOG.md

#### Structure
# Feature Catalog

## Purpose
## Feature List
## Feature Status
## Feature Description
## Relevant API(s)
## Relevant UI Area(s)
## Relevant Data(s)
## Implementation Status
## Pending Work

#### Rules
- Must identify what exists already.
- Must identify what is partially implemented.
- Must identify what is missing.
- Must avoid duplicating requirement-level narrative.

---

### 5) Story Titles Only Document

#### File Example
/docs/stories/STORY-TITLES.md

#### Structure
# Story Titles

## Format
- Story ID
- Story Title
- Status
- Source

## Example
- ST-001 | Patient demographics update flow | Pending | Existing requirement doc
- ST-002 | Medical history UI linkage | In progress | API audit
- ST-003 | Integration test stabilization | Pending | Failure analysis

#### Rules
- Titles only.
- No long explanation.
- No solution detail.
- No duplicate narrative.

---

### 6) Pending Tasks Document

#### File Example
/docs/pending/PENDING-TASKS.md

#### Structure
# Pending Tasks

## Purpose
List all unresolved tasks derived from:
- existing markdown files,
- current implementation,
- current gaps,
- failing tests,
- incomplete integration,
- code cleanup findings.

## Format
- Task ID
- Title
- Source
- Priority
- Status
- Owner Viewpoint
- Notes

## Example
- PT-001 | Connect missing API to UI | API audit | High | Open | Backend/UX | Enrich existing flow if possible
- PT-002 | Split oversized service file | Code cleanup | Medium | Open | Architect/Backend | Preserve behavior
- PT-003 | Remove stale markdown file | Doc cleanup | Low | Open | Documentation Owner | Confirm no active references first

#### Rules
- Must reflect current state only.
- Must be updated from existing docs and code.
- Must remain the backlog source of truth.

---

## Required Audit and Cleanup Areas

The agent must continuously inspect these areas.

### API to UI Integration
- identify APIs not linked to UI,
- identify UI sections missing APIs,
- enrich existing flows if a partially implemented feature is already present,
- remove APIs only if they are redundant and not adding unique value.

### Code Usage
- identify unused methods,
- identify unused classes,
- identify unused services,
- identify unused modules,
- identify methods/classes consumed nowhere,
- determine whether logic should be reused before removing it.

### Database Alignment
- compare code models and mappings with actual DB schema,
- identify missing tables or columns,
- identify schema elements not used by code,
- align or remove as appropriate.

### Large File Cleanup
- identify files that are too large,
- identify files with multiple responsibilities,
- split them into smaller focused files,
- preserve behavior,
- avoid superficial splitting.

### Markdown File Cleanup
- identify unused markdown files,
- identify duplicate markdown files,
- identify stale markdown files,
- identify markdown files that no longer match code or current requirements,
- remove or consolidate them only after verifying they are not active source-of-truth documents.

### Code File Cleanup
- identify unused code files,
- identify duplicate or obsolete code files,
- identify files referenced nowhere,
- remove only after checking whether they are still needed indirectly.

---

## Build and Compilation Discipline

After every major code change:
- compile the solution,
- record the compilation result in the action document,
- do not continue blindly if compilation fails.

### Required Rule
Compilation status must be explicitly stated in the action file.

### Example Action Entry
ACT-BUILD-001
Step: Compile after API integration changes
Result: Success
Notes: No build errors

If failure occurs:
- fix the issue immediately,
- recompile,
- record the failure and fix.

---

## Integration Testing Discipline

After build success:
- run integration tests,
- record results,
- investigate failures immediately.

### Integration Test Expectations
Test:
- API flow,
- UI-to-API linkage where applicable,
- database persistence,
- validation,
- error handling,
- contract behavior.

### If Tests Fail
Create or update a markdown file under:
- /failures/

### Failure Analysis File
#### File Example
/failures/INTEGRATION-FAILURE-ANALYSIS.md

#### Structure
# Integration Failure Analysis

## Test Name
## Failure Summary
## Root Cause
## Impact
## Reproduction Notes
## Fix Recommendation
## Related Files
## Related Actions
## Status

### Rule
The failure analysis file must be created or updated every time a test fails.

---

## Service Startup Rule

After integration tests pass successfully:
- start the service using the existing batch file,
- verify runtime startup,
- confirm that the current build and test state can execute.

Do not introduce a new startup mechanism if an existing batch file already exists.

---

## Logging and Traceability Rules

Every important decision must be traceable.

### Required ID Prefixes
- ANA- for analysis items
- ACT- for action items
- DEC- for decisions
- TST- for test actions
- PT- for pending tasks
- ST- for story titles
- RF- for refactoring or file split items
- CLN- for cleanup items

### Example Traceability Links
- analysis item → action item
- action item → decision item
- action item → build result
- action item → test result
- failure analysis → related test item
- pending task → source document or code location

---

## Action File Expectations

Action files must explicitly state:
- what to do,
- why to do it,
- what it came from,
- what file or code location is affected,
- whether compilation was checked,
- whether integration tests were run,
- whether the item is complete or pending.

### Each Action Item Must Include
- ID
- title
- source reference
- decision type
- priority
- current status
- compile status
- test status
- notes

---

## Decision Log Expectations

The decision log must capture significant final decisions.

### Each Decision Entry Must Include
- decision ID
- item name
- final decision
- reason
- related action IDs
- impact

This log is especially important for:
- removals,
- merges,
- refactors,
- schema changes,
- file deletions,
- and test-related corrective actions.

---

## Cleanup Rules for Unused Markdown and Code Files

The agent must actively identify and propose cleanup of:
- unused markdown files,
- stale markdown files,
- duplicated documentation,
- unused code files,
- obsolete helper classes,
- dead services,
- redundant DTOs,
- orphaned mapping files,
- duplicate test files.

### Cleanup Decision Rule
Before removing any file:
1. check whether it is referenced from code,
2. check whether it is referenced from other documents,
3. check whether it still has source-of-truth value,
4. check whether it still supports active work,
5. then either:
   - keep it,
   - consolidate it,
   - archive it,
   - or remove it.

---

## Token Efficiency Rules for the Agent

The agent should keep the context lightweight and avoid unnecessary verbosity.

### Use This Style
- prefer IDs over repeated descriptions,
- prefer concise findings,
- avoid repeating the same reasoning in multiple documents,
- write only what is necessary for execution,
- split documents by purpose,
- do not duplicate the same content in multiple places unless required for traceability.

### Good Practice
- one fact once,
- one decision once,
- one action once,
- one test record once.

---

## Execution Order

The agent must follow this sequence:

1. Review existing markdown files
2. Review codebase and identify implemented features
3. Update requirement-level documents
4. Update UX documents
5. Update solution-level documents
6. Update feature-level documents
7. Update story-title-only document
8. Update pending tasks document
9. Identify and clean unused markdown files
10. Identify and clean unused code files
11. Perform code cleanup and refactoring
12. Check compilation after major changes
13. Run integration tests
14. Analyze failures if any
15. Start the service using the existing batch file after tests pass

Do not skip ahead.

---

## Completion Criteria

The work is complete only when:
- documentation is split by stakeholder viewpoint,
- existing markdown files are rationalized and updated,
- unused markdown files are identified and cleaned,
- unused code files are identified and cleaned,
- APIs and UI are aligned,
- code and database are aligned,
- oversized files are split,
- hardcoding is removed,
- fallback logic is avoided or logged,
- compilation succeeds after major changes,
- integration tests pass,
- failure analysis is documented when tests fail,
- and the service starts successfully using the existing batch file.

---

## Final Instruction

Work with discipline and traceability.

At every decision point, ask:
Can this improve an existing feature or keep the system more accurate before we remove it?

If yes, integrate or refactor it.  
If no, remove it safely.  
If a fallback is unavoidable, log it explicitly.  
If a major code change is made, compile immediately after.  
If tests fail, document the failure in markdown before moving on.