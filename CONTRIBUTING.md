# Contributing to RPG 2D

Use [WORKFLOW.md](WORKFLOW.md) as the single project-status, handoff and Learning-first hub. Latest `main` is the implemented baseline; open or unmerged PRs are proposals.

**Issue → Branch → Focused commits → Verification → Pull Request → Review → Merge**

## 1. Start with an Issue

Search existing Issues and relevant documentation before changing code, content, design or project configuration. Create or select one Issue and define its context, expected outcome, scope, non-goals, acceptance criteria and verification. Link dependencies.

An Issue describes a problem and observable outcome, not a premature list of files. Split work only when outcomes can be implemented and verified independently. Do not create abstractions merely because they might be reusable.

## 2. Create a branch

Create each task branch from the latest `main`. Never commit task work directly to `main`.

| Work | Prefix | Example |
|---|---|---|
| Feature | `feat` | `feat/issue-12-room-exploration` |
| Bug fix | `fix` | `fix/issue-13-player-collision` |
| Design / docs | `docs` | `docs/issue-14-world-loop` |
| Tests | `test` | `test/issue-15-movement-rules` |
| Refactor | `refactor` | `refactor/issue-16-interaction-flow` |
| Maintenance | `chore` | `chore/issue-17-project-config` |

Required pattern: `<type>/issue-<number>-<kebab-case-description>`.

## 3. Make focused commits

A commit is one coherent, atomic logical change — not one file. Include all related code, tests, scenes, assets, data and docs required for that outcome; exclude unrelated cleanup; leave the repository coherent.

Use Conventional Commits: `<type>(<optional-scope>): <imperative summary>`. Allowed types: `feat`, `fix`, `refactor`, `test`, `docs`, `chore`.

Examples: `feat(exploration): add adjacent object interaction`, `fix(movement): prevent walking through room walls`, `docs(workflow): clarify runtime verification`.

## 4. Verify before opening a PR

Match verification to the change:
- **Documentation-only:** inspect Markdown rendering, internal links, terminology and consistency.
- **Project configuration or scenes:** open the project in the selected Godot editor; check import, scene loading and affected flow.
- **Gameplay code:** run relevant automated tests and build, then manually play the affected journey.
- **Input, collision, animation or UI:** manual runtime verification is required; a successful build does not prove scene wiring or usability.

This repository does not yet have a committed automated test harness or CI validation command. Do not claim tests or CI passed until they exist and actually ran. When runtime and language are established, add one predictable validation entry point and make CI call the same checks.

Record exact commands and observed results. Separate automated checks from manual playtesting and list anything not tested.

## 5. Open a Pull Request

A PR is one reviewable solution to an Issue and may contain multiple related files and commits. Explain the problem, expected outcome, solution, decisions, non-goals, limitations and verification evidence. Use the PR template.

Link Issues accurately: `Closes #N` only when completed; `Part of #N` for partial delivery; `Related to #N` for context. Open a Draft PR when implementation or verification is incomplete. State dependencies and merge order for dependent PRs. Inspect the full diff before requesting review.

## 6. Review and merge

Confirm acceptance criteria, inspect the full diff, review tests and docs for consistency, resolve or record review feedback, complete required Godot runtime checks and confirm required automation passes.

The repository owner reviews and merges the PR. Do not merge your own PR unless explicitly requested. Delete a remote branch after merge or abandonment only when no open PR or active work depends on it.

## 7. Definition of Done

A task is done when acceptance criteria are met, relevant automated checks pass, the affected player journey is manually verified when applicable, documentation matches implementation, and the PR is reviewed and merged.

## 8. Naming conventions

Use descriptive `PascalCase` for directories, C# files/types and Godot scene/resource files. Use `camelCase` for local variables and parameters, and `_camelCase` for private fields.

Use `snake_case` for stable authored content IDs (characters, locations, events, actions and similar data). Use natural language for player-facing labels. Do not rename serialized IDs or assets as drive-by cleanup.

## 9. Documentation governance

Keep one canonical source for each kind of information:
- `README.md`: project entry point.
- `WORKFLOW.md`: current status, handoff and learning evidence.
- `CONTRIBUTING.md`: stable collaboration and Git rules.
- `AGENTS.md`: agent navigation and guardrails.
- Design documents: intended player experience and accepted gameplay rules.
- Architecture documents: current technical responsibilities and boundaries.
- GitHub Issues: problems, requirements and unresolved decisions.
- Pull Requests: proposed changes, review discussion and verification evidence.

Search for an existing canonical home before creating a document. Do not create permanent session notes, temporary plans or duplicate status trackers.