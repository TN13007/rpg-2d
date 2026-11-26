# Contributing to RPG 2D

Use [WORKFLOW.md](WORKFLOW.md) as the single project-status, handoff and learning-evidence hub. Latest `main` is the implemented baseline; open PRs are proposals.

**Issue → Branch → Focused commits → Local verification → Pull Request / CI → Owner review → Merge**

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

## 4. Verify locally before requesting review

Run the single local validation entry point from the repository root:

```bash
python Tools/Validate.py --base origin/main --head HEAD --branch feat/issue-12-room-exploration --check-branch
```

Omit `--branch` and `--check-branch` for ordinary local validation if branch naming is not yet relevant. The command runs Python validator tests, Markdown link/anchor checks, changed-path/JSON naming checks and the .NET Release test project. It stops at the first failure and returns a non-zero exit code.

Match additional verification to the change:
- Documentation-only: inspect Markdown rendering, links, anchors and terminology.
- Project configuration or scenes: open the project in Godot 4.7.2 .NET; verify import and affected scenes.
- Gameplay logic: run focused automated tests and the complete suite, then manually play the affected journey when automation cannot establish correct behavior.
- Input, collision, animation, UI, scene transitions or timing: perform manual runtime verification when automated checks cannot establish usability or the experiential result. A successful build does not prove correct scene wiring or usability.

Record exact commands and observed results. Separate automated tests, Godot editor/build checks and manual playtesting. List anything not tested. Diagnose and fix failures from code and logs before requesting user help; do not delegate routine technical debugging to the project owner.

## 5. Open and maintain a Pull Request

A PR is one reviewable solution to an Issue and may contain multiple related files and commits. Explain the problem, outcome, solution, decisions, non-goals, limitations and verification evidence. Use the PR template.

Link Issues accurately: `Closes #N` only when completed; `Part of #N` for partial delivery; `Related to #N` for context. Open a Draft PR when implementation or local verification is incomplete, or when a PR is needed to trigger CI while the work is still being stabilized. Inspect the full diff before requesting review.

Update the PR as fixes are pushed. Record the exact commit and verification evidence; do not leave stale test results presented as current.

## 6. CI, review and merge

CI is a hard gate before an implementation is reported as ready or handed to the project owner for experiential playtesting.

- Confirm every required check and applicable CI job has completed successfully on the exact latest PR head commit.
- A green run on an older commit does not count after another commit is pushed.
- If a check fails, inspect its job/log, fix the underlying code or configuration, push and rerun the affected checks. Retry transient infrastructure failures when appropriate.
- If CI is blocked by an external service or unavailable permission, document the specific blocker; do not claim the implementation is ready.
- Request manual playtesting only for behavior that cannot be reliably verified automatically, and only after the CI gate passes. When the owner reports a defect, resume implementation and verification; pass CI again before the next handoff.
- Mark the PR ready for review only when implementation and required verification are complete. The repository owner reviews and merges. Do not merge your own PR unless explicitly requested.

## 7. Implementation-ready and Definition of Done

**Implementation-ready for owner review** means acceptance criteria are met, relevant code/tests/data/docs are updated, local and feature-specific checks pass, the full diff has been reviewed, and all required CI checks pass on the latest PR head commit. Any required experiential validation must also be completed by the owner, or clearly identified as the only remaining owner decision.

A task is **closed/done in the repository** only when the owner has reviewed and merged the PR and the linked Issue is resolved. Implementation-ready and merged are distinct states: the agent is responsible for preparing a complete, verified PR; merge authority stays with the repository owner.

## 8. Canonical engineering standards

[Coding standards](Docs/Architecture/CodingStandards.md) is the single source of truth for C#, Godot resources/scenes/nodes, data, naming, architecture boundaries and error handling. [Testing strategy](Docs/Architecture/TestingStrategy.md) owns test levels, test naming, fixtures and manual verification requirements.

## 9. Documentation governance

- `README.md`: project entry point.
- `WORKFLOW.md`: current status, delivery handoff and learning evidence.
- `CONTRIBUTING.md`: stable collaboration and Git rules.
- `AGENTS.md`: agent navigation, implementation guardrails and autonomous delivery requirements.
- Design documents: intended player experience and accepted gameplay rules.
- Architecture documents: current technical responsibilities and boundaries.
- GitHub Issues: problems, requirements and unresolved decisions.
- Pull Requests: proposed changes, review discussion and verification evidence.

Search for an existing canonical home before creating a document. Do not create permanent session notes or duplicate status trackers.
