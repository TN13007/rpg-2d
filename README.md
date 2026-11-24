# RPG 2D

A Godot 4.7.2 .NET / C# 2D RPG focused on spatial exploration, everyday student-life activities, supernatural mysteries and turn-based encounters.

## Run the current prototype

Open the project in Godot 4.7.2 .NET and run the project (F6 runs the current scene; F5 runs the configured main scene).

- **Move:** WASD or arrow keys.
- **Movement:** four directions with normalized diagonal speed.
- **Character:** the Ninja Adventure Boy sprite, with directional idle and walk frames.
- **Camera:** follows the player.
- **Collision:** the player is blocked by the test arena walls.

The current main scene is `scenes/prototypes/player_movement_test.tscn`. The reusable player scene is `scenes/characters/player.tscn`.

## Project navigation

- [Design](Docs/Design/README.md) — intended player experience and accepted gameplay decisions.
- [Project structure](Docs/Architecture/ProjectStructure.md) — placement rules for scenes, scripts, data, assets and tests.
- [Coding standards](Docs/Architecture/CodingStandards.md) — naming, C#, Godot and data conventions.
- [Testing strategy](Docs/Architecture/TestingStrategy.md) — automated and manual verification.
- [Workflow and handoff](WORKFLOW.md) — current state, session protocol and next task.
- [Contributing](CONTRIBUTING.md) — Issue, branch, commit, PR, review and verification rules.
- [Agent instructions](AGENTS.md) — repository navigation and implementation guardrails.

A planned folder, design proposal, open Issue or unmerged PR is not implemented gameplay.
