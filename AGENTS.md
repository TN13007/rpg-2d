# Repository Instructions for Coding Agents

This is a Godot 2D RPG. The scripting runtime and language must be confirmed before implementation; the repository currently has no gameplay code or .NET project.

## Start with task context

1. Read the active GitHub Issue and related PRs. Latest `main` is the implemented baseline; open PRs are proposals.
2. Read [WORKFLOW.md](WORKFLOW.md) for status, blockers and handoff.
3. Read relevant documents under [Docs/Design](Docs/Design/README.md) and [Docs/Architecture](Docs/Architecture/ProjectStructure.md).
4. Read [CONTRIBUTING.md](CONTRIBUTING.md) before creating Issues, branches, commits or PRs.

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

- Design around a concrete player-facing scenario. Prefer a complete, testable player journey over disconnected systems.
- Keep feature-specific behavior near its owning feature. Put code in shared folders only when genuinely used by multiple features.
- Keep scene composition, gameplay rules, content data and UI responsibilities distinguishable.
- Do not add managers, interfaces, factories, repositories, service locators or universal executors pre-emptively.
- Do not create architecture layers solely to mirror folders.
- Preserve scene paths, resource references, input actions and serialized IDs unless a focused migration requires changing them.
- Do not reorganize the third-party asset pack as unrelated cleanup.
- Avoid broad refactors and generated-file churn. Keep each PR focused on its Issue.

## Validation and reporting

Follow [CONTRIBUTING.md](CONTRIBUTING.md). State exactly which checks ran and their observed results. Separate automated tests, editor/build checks and manual playtesting. Never claim a check passed unless observed.