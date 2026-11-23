# Contributing to RPG 2D

Use [WORKFLOW.md](WORKFLOW.md) as the single project-status, handoff and Learning-first hub. Latest `main` is the implemented baseline; open PRs are proposals.

**Issue → Branch → Focused commits → Verification → Pull Request → Review → Merge**

## 1. Start with an Issue

Search existing Issues and relevant documentation before changing code, content, design or project configuration. Define context, expected outcome, scope, non-goals, acceptance criteria and verification. Split work only when outcomes can be implemented and verified independently.

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

A commit is one coherent, atomic logical change — not one file. Use Conventional Commits: `<type>(<optional-scope>): <imperative summary>`. Allowed types: `feat`, `fix`, `refactor`, `test`, `docs`, `chore`.

Examples: `feat(exploration): add adjacent object interaction`, `fix(movement): prevent walking through room walls`, `docs(workflow): clarify runtime verification`.

## 4. Verify before opening a PR

Run the single local validation entry point from the repository root:

```bash
python Tools/Validate.py --base origin/main --head HEAD --branch feat/issue-12-room-exploration --check-branch
```

Omit `--branch` and `--check-branch` for ordinary local validation if branch naming is not yet relevant. The command runs Python validator tests, Markdown link/anchor checks, changed-path/JSON naming checks and the .NET Release test project. It stops at the first failure and returns a non-zero exit code.

Match additional verification to the change:
- Documentation-only: inspect Markdown rendering, links, anchors and terminology.
- Project configuration or scenes: open the project in Godot 4.7.2 .NET; verify import and affected scenes.
- Gameplay logic: run focused automated tests and the complete suite, then manually play the affected journey.
- Input, collision, animation, UI, scene transitions or timing: manual runtime verification is required. A successful build does not prove correct scene wiring or usability.

Record exact commands and observed results. Separate automated tests, Godot editor/build checks and manual playtesting. List anything not tested.

## 5. Open a Pull Request

A PR is one reviewable solution to an Issue and may contain multiple related files and commits. Explain the problem, outcome, solution, decisions, non-goals, limitations and verification evidence. Use the PR template.

Link Issues accurately: `Closes #N` only when completed; `Part of #N` for partial delivery; `Related to #N` for context. Open a Draft PR when implementation or verification is incomplete. Inspect the full diff before requesting review.

## 6. Review and merge

Confirm acceptance criteria, inspect the full diff, review tests and docs, resolve or record review feedback, complete required Godot runtime checks and confirm required automation passes. The repository owner reviews and merges. Do not merge your own PR unless explicitly requested.

## 7. Definition of Done

A task is done when acceptance criteria are met, relevant automated checks pass, the affected player journey is manually verified when applicable, documentation matches implementation, and the PR is reviewed and merged.

## 8. Canonical engineering standards

[Coding standards](Docs/Architecture/CodingStandards.md) is the single source of truth for C#, Godot resources/scenes/nodes, data, naming, architecture boundaries and error handling. [Testing strategy](Docs/Architecture/TestingStrategy.md) owns test levels, test naming, fixtures and manual verification requirements.

## 9. Documentation governance

- `README.md`: project entry point.
- `WORKFLOW.md`: current status, handoff and learning evidence.
- `CONTRIBUTING.md`: stable collaboration and Git rules.
- `AGENTS.md`: agent navigation and guardrails.
- Design documents: intended player experience and accepted gameplay rules.
- Architecture documents: current technical responsibilities and boundaries.
- GitHub Issues: problems, requirements and unresolved decisions.
- Pull Requests: proposed changes, review discussion and verification evidence.

Search for an existing canonical home before creating a document. Do not create permanent session notes or duplicate status trackers.
