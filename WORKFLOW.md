# WORKFLOW — Project Status and Delivery Handoff

**Repository:** [TN13007/rpg-2d](https://github.com/TN13007/rpg-2d)  
**Engine:** Godot 4.7.2 .NET / C#  
**Last reviewed:** 2026-11-27

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
| Agent navigation and implementation guardrails | [AGENTS.md](AGENTS.md) |
| Current handoff | This file |

## Product direction

The working direction is a spatial 2D RPG. The player moves through locations, sees nearby characters and interacts with objects or people in context. The broader experience combines everyday student-life activities with supernatural exploration and turn-based combat.

Validate one complete player journey before expanding the system set. Design direction is not proof of implementation.

## Current project snapshot

- The project foundation is merged into `main`: Godot 4.7.2 .NET / C#, naming standards, tests and CI.
- Issue [#6 — asset import and controllable player scene](https://github.com/TN13007/rpg-2d/issues/6) is closed as completed.
- PR [#28 — typed item hierarchy](https://github.com/TN13007/rpg-2d/pull/28) is merged into `main`.
- PR [#24 — Shop purchase flow](https://github.com/TN13007/rpg-2d/pull/24) remains open and unmerged. The project owner has confirmed the Shop gameplay works; merge remains the owner's decision.

## Agent-owned delivery protocol

**Primary objective:** deliver the requested implementation end to end. Learning is valuable, but it is a separate, non-blocking outcome. Do not require the user to supervise routine technical steps or agree on a learning objective before each task.

1. **Inspect:** read the active Issue and acceptance criteria; inspect the latest `main`, related open/merged PRs, working branch and Git state, relevant documentation, implementation, data and tests. Treat unmerged PRs as proposals. Never assume the local or remote branch is current without checking.
2. **Resolve decisions:** distinguish missing product decisions from questions answerable through repository inspection, tests or logs. Make a conservative, reversible choice when requirements and existing conventions are sufficient. Ask the user only for decisions that materially change the intended experience, information unavailable to the agent, or experiential feedback such as game feel. Ask one focused question and continue all independent work.
3. **Plan:** define the intended outcome, scope, non-goals, affected responsibilities and verification. Create a temporary Implementation Brief for cross-feature work when it reduces ambiguity; do not create permanent duplicate planning documents.
4. **Implement:** work on the task branch, keep changes focused, preserve existing project conventions and complete the player-facing slice where practical. Update tests, fixtures, data and documentation alongside implementation. Do not stop after writing code or after the first successful test.
5. **Verify and repair:** run the repository validator and all focused tests/build/runtime checks required by the change. When a check fails, inspect its output, trace the cause, fix it and rerun the affected checks. Repeat until successful. Do not ask the user to diagnose technical failures that can be investigated from code, tests, logs or repository state.
6. **Review and push:** inspect the complete diff against the target base; check acceptance criteria, regressions, scope creep, data/scene references and documentation consistency. Commit coherent changes and push the task branch. Open or update a PR so the repository's PR-triggered CI can run.
7. **CI hard gate:** verify every required check and every applicable CI job has completed successfully on the exact latest PR head commit. A green result on an older commit is not sufficient. If CI fails, inspect the failing job/log, fix code or configuration issues, push and repeat verification. Retry transient infrastructure failures where appropriate. Never report implementation-ready status or ask for user playtesting before this gate passes. If CI cannot run because of an external service, missing permission or repository configuration, report the specific blocker and do not claim success.
8. **User validation:** request manual playtesting only for behavior that cannot be reliably established through automated checks or available runtime inspection, such as game feel, timing, visual readability or an input/scene interaction that requires hands-on play. First complete the technical checks and CI gate. Give the user a short, specific test target and identify the exact PR commit being tested. If the user reports a defect, resume the verify-and-repair loop, push the fix and pass CI again before the next handoff. If the change is adequately covered by automation, do not request a redundant manual test.
9. **Deliver:** update the PR with the actual implementation, decisions, test evidence, limitations and any remaining user-validation result. Update this handoff only when the current project state or next action changes. Report the result only after the applicable completion gates are satisfied. Do not merge unless the repository owner explicitly authorizes it.

### Implementation-ready and completion criteria

**Implementation-ready for owner review** means all of the following are true:

- Issue acceptance criteria are satisfied, or any explicitly deferred item is documented and agreed.
- Implementation, relevant tests, fixtures, data and documentation are updated.
- Local validation and all applicable feature-specific checks pass.
- The full diff has been reviewed and contains no known unintended changes.
- All required CI checks and applicable CI jobs pass on the latest PR head commit.
- Required experiential/manual validation has been completed by the project owner, or is explicitly identified as the only remaining owner decision.

A task is **not ready for handoff** while a diagnosable code, test, build or CI failure remains. A task is **merged/closed** only after the repository owner reviews and merges the PR. These are separate states: the agent is responsible for making the PR implementation-ready; the owner retains merge authority.

### User-intervention boundaries

The agent owns repository exploration, implementation, technical debugging, automated testing, diff review, branch/PR maintenance and CI repair. The user should not be asked to perform these routine steps.

Ask the user only when:
- a product or design decision cannot be determined from the Issue, accepted design or existing behavior;
- the required information or bug reproduction exists only in the user's runtime/environment;
- hands-on evaluation is necessary for game feel, visual readability, usability or other experiential qualities;
- an external permission or service constraint prevents further progress.

Before asking, inspect relevant code and data, search existing issues/PRs, read available logs and attempt the available reproduction methods. State what is already known, what cannot be established and the single decision or observation needed.

### Verification selection

| Change type | Expected verification |
|---|---|
| Pure data or domain logic | Focused unit tests, validation and regression suite |
| JSON/schema/content | Data validation and relevant loader tests |
| Dialogue, inventory or transactions | Unit/integration tests plus runtime journey when scene/input wiring is affected |
| Scene, UI, input, collision or transitions | Godot import/build/smoke tests and hands-on gameplay where automation cannot verify usability |
| Animation, camera, timing or combat feel | Automated structural checks plus owner playtest for the experiential result |
| Documentation only | Markdown/link/anchor validation and diff review |

Do not request manual testing merely to repeat assertions already proven by reliable automated checks. Do not treat a successful build as proof of correct gameplay behavior.

## Current handoff

**Current delivery:** [PR #24 — Shop purchase flow](https://github.com/TN13007/rpg-2d/pull/24) remains open and unmerged. The project owner has confirmed the gameplay test succeeds. Review and merge are still owner-controlled.

**Next action:** the repository owner reviews the PR and decides whether to merge it. Do not merge automatically.

## Learning evidence

Learning evidence is separate from delivery acceptance. Capture it only when it is demonstrated and useful; it must not block implementation or force a separate lesson into every task.

```text
YYYY-MM-DD — Issue / topic
Learning: Not assessed / Needs practice / With support / Independent
Evidence: what the learner explained, changed or verified independently
Remaining gap: one specific concept or skill
Next practice: one small exercise
```

Do not keep a chronological session log. Issues and PRs retain delivery history; this file keeps only the current handoff and meaningful learning evidence.
