# Project Structure

The layout is intentionally small and feature-oriented. Folders provide homes for real work; they do not authorize creating every possible subsystem immediately. Game-owned folders and non-C# resource files use Godot's recommended `snake_case`; C# files match their PascalCase class names.

## Repository tree

```text
RPG-2D/
├── .github/
│   ├── ISSUE_TEMPLATE/
│   ├── workflows/
│   └── PULL_REQUEST_TEMPLATE.md
├── assets/
│   ├── audio/
│   ├── characters/
│   └── environment/
├── data/
│   └── world/
├── Docs/
│   ├── Architecture/
│   └── Design/
├── scenes/
│   └── world/
├── scripts/
│   ├── bootstrap/
│   └── features/
│       ├── combat/
│       └── exploration/
├── Tests/
├── Tools/
│   └── Tests/
├── Ninja Adventure - Asset Pack/
├── RPG2D.csproj
├── Tests/RPG2D.Tests.csproj
├── AGENTS.md
├── CONTRIBUTING.md
├── README.md
├── WORKFLOW.md
└── project.godot
```

## Responsibilities

| Folder | Responsibility |
|---|---|
| `assets/` | Game-owned source art, audio and UI assets. Preserve the supplied third-party pack and license. |
| `data/` | Authored game content grouped by domain; stable IDs are separate from player-facing labels. |
| `scenes/` | Godot scenes grouped by player-facing feature or composition role. |
| `scripts/bootstrap/` | Application entry point and top-level runtime composition. |
| `scripts/features/` | Feature-owned gameplay and presentation code. |
| `Tests/` | .NET tests for pure game/domain behavior and integration seams. |
| `Tools/` | Repository validation and development utilities. |
| `Tools/Tests/` | Python unit tests for repository tooling. |
| `Docs/Design/` | Intended player experience and accepted gameplay rules. |
| `Docs/Architecture/` | Current code ownership, boundaries and runtime/data flows. |

Keep scenes, scripts, data and assets separate by purpose. Keep feature-specific code near its feature. Shared code requires a real second consumer. Do not create empty classes, placeholder managers or generic abstractions to populate folders. The `.godot/` cache and generated build output must not be committed.

Use snake_case for scene/resource files (for example, `dormitory_room.tscn`) and PascalCase for C# files (for example, `PlayerController.cs`). Godot node names remain PascalCase. See [Coding standards](CodingStandards.md) for naming and ownership rules, and [Testing strategy](TestingStrategy.md) for test placement.
