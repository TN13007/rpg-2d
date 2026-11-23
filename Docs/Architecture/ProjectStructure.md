# Project Structure

The layout is intentionally small and feature-oriented. Folders provide homes for real work; they do not authorize creating every possible subsystem immediately.

## Repository tree

```text
RPG-2D/
├── .github/
│   ├── ISSUE_TEMPLATE/
│   ├── workflows/
│   └── PULL_REQUEST_TEMPLATE.md
├── Assets/
│   ├── Audio/
│   ├── Characters/
│   └── Environment/
├── Data/
│   └── World/
├── Docs/
│   ├── Architecture/
│   └── Design/
├── Scenes/
│   └── World/
├── Scripts/
│   ├── Bootstrap/
│   └── Features/
│       ├── Combat/
│       └── Exploration/
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
| `Assets/` | Game-owned source art, audio and UI assets. Preserve the supplied third-party pack and license. |
| `Data/` | Authored game content grouped by domain; stable IDs are separate from player-facing labels. |
| `Scenes/` | Godot scenes grouped by player-facing feature or composition role. |
| `Scripts/Bootstrap/` | Application entry point and top-level runtime composition. |
| `Scripts/Features/` | Feature-owned gameplay and presentation code. |
| `Tests/` | .NET tests for pure game/domain behavior and integration seams. |
| `Tools/` | Repository validation and development utilities. |
| `Tools/Tests/` | Python unit tests for repository tooling. |
| `Docs/Design/` | Intended player experience and accepted gameplay rules. |
| `Docs/Architecture/` | Current code ownership, boundaries and runtime/data flows. |

Keep scenes, scripts, data and assets separate by purpose. Keep feature-specific code near its feature. Shared code requires a real second consumer. Do not create empty classes, placeholder managers or generic abstractions to populate folders. The `.godot/` cache and generated build output must not be committed.

See [Coding standards](CodingStandards.md) for naming and ownership rules, and [Testing strategy](TestingStrategy.md) for test placement.
