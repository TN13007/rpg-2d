# Repository Instructions for Coding Agents

This project uses Godot 4.7.2 .NET / C#. Follow [Coding standards](Docs/Architecture/CodingStandards.md) and [Testing strategy](Docs/Architecture/TestingStrategy.md) for implementation rules.

## Start with task context

1. Read the active GitHub Issue and related PRs. Latest `main` is the implemented baseline; open or unmerged PRs are proposals.
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
- Keep feature-specific behavior near the feature that owns the player-facing behavior.
- Keep scene composition, gameplay rules, content data and UI responsibilities distinguishable.
- Do not add managers, interfaces, factories, repositories, service locators or universal executors pre-emptively.
- Do not create architecture layers solely to mirror folders.
- Preserve scene paths, resource references, input actions and serialized IDs unless a focused migration requires changing them.
- Do not reorganize the third-party asset pack as unrelated cleanup. A dedicated import task may place vendor files under the appropriate `assets/<category>/ninja_adventure/` folders, but must preserve original asset bytes, upstream internal names and license/source information.
- Avoid broad refactors and generated-file churn. Keep each PR focused on its Issue.
- Do not add an abstraction until a real second use case or clear complexity reduction justifies it.

## Validation and reporting

Run `python Tools/Validate.py --base origin/main --head HEAD` and any additional checks required by the affected feature. State exactly which checks ran and their observed results. Separate automated tests, Godot editor/build checks and manual playtesting. Never claim a check passed unless observed.
