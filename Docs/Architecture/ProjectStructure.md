# Project Structure

This is the initial folder layout for the 2D RPG. It is intentionally small and feature-oriented. Folders provide clear homes for work; they do not authorize creating every possible subsystem immediately.

## Initial repository tree

```text
RPG-2D/
├── .github/
│   ├── ISSUE_TEMPLATE/
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
├── Ninja Adventure - Asset Pack/
├── AGENTS.md
├── CONTRIBUTING.md
├── README.md
├── WORKFLOW.md
└── project.godot
```

The empty folders are tracked with `.gitkeep`. Add deeper folders only when a concrete feature needs them.

## Responsibilities

| Folder | Responsibility |
|---|---|
| `Assets/` | Game-owned source art, audio and UI assets. Preserve the supplied third-party pack and license in its existing root folder. |
| `Data/` | Authored game content grouped by domain; stable IDs remain separate from player-facing labels. |
| `Scenes/` | Godot scenes grouped by player-facing feature or composition role. |
| `Scripts/Bootstrap/` | Application entry point and top-level runtime composition. |
| `Scripts/Features/` | Feature-owned gameplay and presentation code, such as exploration and combat. |
| `Tests/` | Unit and integration tests once a test framework is established. |
| `Tools/` | Repository validation and development utilities. |
| `Docs/Design/` | Intended player experience and accepted gameplay rules. |
| `Docs/Architecture/` | Current code ownership, boundaries and runtime/data flows. |

## Placement rules

- Keep scenes, scripts, data and assets separate by purpose; do not store gameplay rules in UI scenes.
- Keep feature-specific code near the feature that owns the player-facing behavior.
- Use shared code only after a second real consumer demonstrates the need.
- A folder name does not imply that its corresponding system already exists.
- Do not create empty classes, placeholder managers or generic abstractions to populate the tree.
- `.godot/` is generated and must not be committed.

Start with the smallest set of folders required by the first vertical slice. Add subdivisions only when they improve navigation or clarify independent responsibilities. Update this document when the actual structure changes materially.