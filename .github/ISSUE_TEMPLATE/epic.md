---
name: Epic
about: Track a multi-issue feature or player-facing milestone
title: "epic: "
assignees: []
---

## Epic goal
What larger player-facing outcome or product capability should this Epic deliver?

## Player / user outcome
Describe what the player or end user will be able to do when the Epic is complete.

## Context / Problem
Why is this work needed? What exists today, and what limitation does this Epic address?

## Roadmap
List the child Issues in the intended implementation order. Link each Issue and give it one independently verifiable outcome.

- [ ] **Step 1 — [Name]:** #  
  - Outcome:
- [ ] **Step 2 — [Name]:** #  
  - Outcome:

## Dependencies and sequencing
Describe prerequisites between child Issues, external systems, assets, or decisions. Make blocking dependencies explicit. Do not duplicate implementation tasks already owned by a child Issue.

- Depends on:
- Blocks:
- Can proceed in parallel:

## Epic scope
Summarize the capability boundaries covered by this Epic. Detailed implementation scope belongs in the child Issues.

## Out of scope
List adjacent systems or features that must not be pulled into this Epic.

## Epic-level acceptance criteria
- [ ] All required child Issues are completed and their PRs are merged.
- [ ] The end-to-end player/user journey described above works.
- [ ] Shared behavior remains consistent across all child features.
- [ ] Relevant automated validation and CI checks pass.
- [ ] Required manual runtime/playtest verification is recorded.
- [ ] Documentation and project handoff reflect the delivered behavior.

## Verification strategy
Describe the end-to-end scenario that proves the Epic works. Include important success, failure, cancel/exit, and regression paths where relevant. Child Issues should contain their own detailed test cases.

## Progress tracking
Update the Roadmap checklist as child Issues are completed. Keep implementation details, commits, test logs, and review discussion in the corresponding child Issues and PRs.

## References
Related design/architecture documents, existing Issues, PRs, assets, and external constraints.
