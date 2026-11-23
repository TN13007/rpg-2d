# WORKFLOW — Project Status and Session Handoff

**Repository:** [TN13007/rpg-2d](https://github.com/TN13007/rpg-2d)  
**Engine:** Godot 4.7.2 .NET / C#  
**Last reviewed:** 2026-11-24

This is the single changing project-status and handoff tracker. Do not create separate session logs, status trackers, decision logs or learning trackers. Latest `main` is the implemented baseline; open or unmerged PRs are proposals.

## Sources of truth

| Question | Source |
|---|---|
| Intended player experience | [Design](Docs/Design/README.md) |
| Code and content placement | [Project structure](Docs/Architecture/ProjectStructure.md) |
| Coding and naming rules | [Coding standards](Docs/Architecture/CodingStandards.md) |
| Test scope and verification | [Testing strategy](Docs/Architecture/TestingStrategy.md) |
| Actual project behavior | Latest `main`, scenes, scripts, data and tests |
| Current problem and acceptance criteria | GitHub Issue |
| Proposed solution and verification | Pull Request |
| Git and collaboration rules | [CONTRIBUTING.md](CONTRIBUTING.md) |
| Current handoff | This file |

## Product direction

The working direction is a spatial 2D RPG. The player moves through locations, sees nearby characters and interacts with objects or people in context. The broader experience combines everyday student-life activities with supernatural exploration and turn-based combat.

Validate one complete player journey before expanding the system set. Design direction is not proof of implementation.

## Current project snapshot

- The project foundation is merged into `main`: Godot 4.7.2 .NET / C#, naming standards, tests and CI.
- Issue #6 imports the Ninja Adventure pack into category-specific `assets/` folders. The vendor's original internal filenames and folder names are preserved under the `ninja_adventure` marker; its CC0 license and source notes are retained.
- Issue #6 adds a reusable Player scene and a movement test arena. The proposed behavior is four-direction movement, WASD/arrow input, directional idle/walk frames, collision and camera follow.
- The Issue #6 branch is not part of `main` until its PR is reviewed and merged. Do not describe its prototype as a released main-branch feature yet.

## Session protocol

Each session should produce two outcomes:
- **Product:** one usable, testable improvement to the game or its development foundation.
- **Learning:** observable evidence that the learner can explain, verify or independently change the relevant part.

1. **Context check:** inspect latest main, active Issue, related PRs, relevant docs, source and tests. Treat unmerged PRs as proposals.
2. **Learning objective:** agree on one product outcome and one practical skill or concept.
3. **Predict and plan:** explain expected behavior before implementation. For cross-feature work, prepare a temporary Implementation Brief with current behavior, evidence, responsibilities, proposed change, constraints and verification.
4. **Implement:** make focused logical changes. Prefer a complete player-facing slice over disconnected infrastructure.
5. **Verify:** run `python Tools/Validate.py --base origin/main --head HEAD`; add `--branch <branch> --check-branch` for PR branch validation. Also perform the Godot/runtime checks required by the change.
6. **Review and retain:** inspect the full diff, record actual evidence and limitations in the PR, and update this handoff only when state or next action changes.

Git lifecycle and commit rules live in [CONTRIBUTING.md](CONTRIBUTING.md).

## Current handoff

**Current product task:** [Issue #6 — asset import and controllable player scene](https://github.com/TN13007/rpg-2d/issues/6).  
**Branch:** `feat/issue-6-player-character-controller`.  
**Next verification:** run unified repository validation, Godot headless import/build, then manually test WASD/arrows, diagonal speed, facing/animation, wall collision and camera follow.

Do not recreate the old RPG architecture, implement generic managers, or add systems without a player-facing scenario.

## Learning evidence

Record learning only when demonstrated. Product completion and learning status are independent.

```text
YYYY-MM-DD — Issue / topic
Product: Done / Partial / Blocked — PR or evidence link
Learning: Not assessed / Needs practice / With support / Independent
Evidence: what the learner explained, changed or verified independently
Remaining gap: one specific concept or skill
Next practice: one small exercise
```

Do not keep a chronological session log. Issues and PRs retain delivery history; this file keeps only the current handoff and meaningful learning evidence.
