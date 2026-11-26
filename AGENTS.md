# Repository Instructions for Coding Agents

This project uses Godot 4.7.2 .NET / C#. Follow [Coding standards](Docs/Architecture/CodingStandards.md) and [Testing strategy](Docs/Architecture/TestingStrategy.md) for implementation rules. Follow [WORKFLOW.md](WORKFLOW.md) for the agent-owned delivery lifecycle and current handoff.

## Start with task context

1. Read the active GitHub Issue and related PRs. Latest `main` is the implemented baseline; open or unmerged PRs are proposals.
2. Read [WORKFLOW.md](WORKFLOW.md) for project status, blockers, user-intervention boundaries and handoff.
3. Read relevant documents under [Docs/Design](Docs/Design/README.md) and [Docs/Architecture](Docs/Architecture/ProjectStructure.md).
4. Read [CONTRIBUTING.md](CONTRIBUTING.md) before creating Issues, branches, commits or PRs.
5. Inspect the actual repository state: current branch, worktree, remote refs, latest base commit and PR head. Do not assume a branch is current.

Do not read every document or source file by default. Follow the task's actual dependencies.

## Required code-discovery sequence

Before editing unfamiliar or cross-feature behavior:
1. **Locate:** find the relevant scene, script, resource, signal, input action or content ID.
2. **Trace:** inspect its definition, callers/references and the path from player input to state change and presentation.
3. **Verify ownership:** identify the feature responsible and check whether an existing component already owns it.
4. **Inspect tests:** find relevant tests and fixtures; identify normal, invalid, boundary and regression cases.
5. **Plan:** for work spanning responsibilities, prepare a temporary Implementation Brief with current behavior, evidence, affected components, proposed change, constraints and verification.

If documentation and implementation disagree, report the mismatch and verify against the actual project.

## Implementation guardrails

- Own the implementation end to end. Do not stop after writing code, after the first passing test or after opening a PR.
- Design around a concrete player-facing scenario. Prefer a complete, testable player journey over disconnected systems.
- Keep feature-specific behavior near the feature that owns the player-facing behavior.
- Keep scene composition, gameplay rules, content data and UI responsibilities distinguishable.
- Do not add managers, interfaces, factories, repositories, service locators or universal executors pre-emptively.
- Do not create architecture layers solely to mirror folders.
- Preserve scene paths, resource references, input actions and serialized IDs unless a focused migration requires changing them.
- Do not reorganize the third-party asset pack as unrelated cleanup. A dedicated import task may place vendor files under the appropriate `assets/<category>/ninja_adventure/` folders, but must preserve original asset bytes, upstream internal names and license/source information.
- Avoid broad refactors and generated-file churn. Keep each PR focused on its Issue.
- Do not add an abstraction until a real second use case or clear complexity reduction justifies it.

## Agent-owned execution and user involvement

The agent is responsible for repository exploration, implementation, technical debugging, automated verification, diff review, branch/PR maintenance and CI repair.

- Before asking the user for help, inspect the relevant code/data, Issues/PRs, tests, available runtime paths and complete failure logs.
- Diagnose and fix failures that can be investigated through repository state, code, tests or logs. Do not hand routine technical debugging to the user.
- Make conservative, reversible decisions when the requirements and existing conventions are sufficient. Ask the user only for a material product/design decision, information available only in their environment, or experiential evaluation such as game feel.
- Continue independent work while waiting for an answer to a genuinely blocking question.
- Request hands-on testing only when automated checks or available runtime inspection cannot reliably establish the behavior. First finish local verification and pass the CI gate. Give the user a focused checklist and the exact PR head commit to test.
- When the user reports a defect, reproduce or trace it, implement the correction, rerun the affected checks, push and pass CI again before handing it back.
- Do not merge a PR unless the repository owner explicitly authorizes it.

## Validation and reporting

Run the repository validation entry point from the repository root:

```bash
python Tools/Validate.py --base origin/main --head HEAD
```

For PR branch naming validation, add:

```bash
--branch <branch> --check-branch
```

Also run all additional checks required by the affected feature. Match verification to the change and keep these categories distinct:
- Automated unit/integration tests and repository validation.
- Godot editor import, C# build and headless/runtime smoke checks.
- Manual playtesting for behavior that cannot be reliably verified automatically.

After a failure, inspect the failing job or log, identify the cause, fix it and rerun the relevant checks. Repeat until successful; retry transient infrastructure failures when appropriate. Do not ask the user to inspect logs that are available to the agent.

**CI is a hard handoff gate.** Open or update the PR to trigger applicable workflows, then verify that all required checks and applicable CI jobs have completed successfully on the exact latest PR head SHA. A result from an older commit does not count. If any code-related check fails, repair and push a new commit, then verify again. If CI is blocked by external infrastructure, unavailable permissions or repository configuration, identify the exact blocker and do not claim the task is ready.

Do not report implementation-ready status or ask for owner playtesting before the CI gate passes. Report exactly which checks ran and their observed results. Never claim a check passed unless observed. Separate implementation-ready status from owner merge/closure: the agent prepares a reviewable, verified PR; the repository owner reviews and merges it.
