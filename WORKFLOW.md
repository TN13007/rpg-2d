# WORKFLOW — Project Status and Session Handoff

**Repository:** [TN13007/rpg-2d](https://github.com/TN13007/rpg-2d)  
**Engine baseline:** Godot 4.7 project configuration  
**Last reviewed:** 2026-11-24  
**Baseline:** `main` at `42b4b0aeb1e62e8284ca2cfe5570a1d0d291f916`. Open or unmerged PRs are proposals, not implemented behavior.

This is the single changing project-status and handoff tracker. Do not create separate session logs, status trackers, decision logs or learning trackers.

## Sources of truth

| Question | Source |
|---|---|
| Intended player experience | [Design](Docs/Design/README.md) |
| Code and content placement | [Project structure](Docs/Architecture/ProjectStructure.md) |
| Actual project behavior | Latest `main`, scenes, scripts, data and tests |
| Current problem and acceptance criteria | GitHub Issue |
| Proposed solution and verification | Pull Request |
| Git and collaboration rules | [CONTRIBUTING.md](CONTRIBUTING.md) |
| Current handoff | This file |

## Product direction

The working direction is a spatial 2D RPG. The player moves through locations, sees nearby characters and interacts with objects or people in context. The broader experience combines everyday student-life activities with supernatural exploration and turn-based combat.

The intended journey has four connected parts:
1. **Everyday life:** explore the player's room, dormitory, campus and nearby locations; choose activities and manage the in-game day.
2. **Discovery:** encounter people, clues, rumors and unusual situations through exploration.
3. **Adventure:** investigate supernatural events and enter turn-based encounters when appropriate.
4. **Consequences:** outcomes may affect character state, relationships and later activities.

These are product-direction notes, not implemented features. Validate one complete player journey before expanding the system set.

## Current project snapshot

The repository currently contains Godot project configuration, an icon and the third-party `Ninja Adventure - Asset Pack` with its supplied license. It does not yet contain gameplay scripts, a main scene, automated tests or a runtime verification pipeline.

The current `project.godot` does not declare the C# feature, and there is no .NET project file. Confirm the intended runtime (GDScript or Godot .NET/C#) in a dedicated task before adding language-specific code. Do not silently change project settings in unrelated work.

[Issue #2 — Establish project workflow and initial 2D RPG structure](https://github.com/TN13007/rpg-2d/issues/2) owns this setup work. Its PR remains a proposal until merged.

## Session protocol

Each session should produce two separate outcomes:
- **Product:** one usable, testable improvement to the game or its development foundation.
- **Learning:** observable evidence that the learner can explain, verify or independently change the relevant part.

AI-generated code, a completed PR or passing checks do not by themselves prove independent learning.

1. **Context check:** inspect latest `main`, active Issue, related PRs, relevant docs, source and tests. Treat unmerged PRs as proposals.
2. **Learning objective:** agree on one product outcome and one practical skill or concept.
3. **Predict and plan:** explain expected behavior before implementation. For ambiguous or cross-feature work, prepare a temporary Implementation Brief with current behavior, evidence, responsibilities, proposed change, constraints and verification.
4. **Implement:** make focused logical changes. Prefer a complete player-facing slice over disconnected infrastructure.
5. **Verify:** run checks required by the Issue and inspect the Godot runtime when scenes, input, UI or gameplay change.
6. **Review and retain:** inspect the full diff, record actual evidence and limitations in the PR, and update this handoff only when state or next action changes.

Git lifecycle and commit rules live in [CONTRIBUTING.md](CONTRIBUTING.md).

## Current handoff

**Baseline:** `main` at `42b4b0aeb1e62e8284ca2cfe5570a1d0d291f916`.

**Next product decision:** confirm the intended Godot runtime (GDScript or Godot .NET/C#) and define the first end-to-end playable slice. It should validate movement, spatial interaction and a meaningful transition between locations before broader architecture is introduced.

Do not recreate the old RPG architecture, implement generic managers/services, or add systems without a player-facing scenario.

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