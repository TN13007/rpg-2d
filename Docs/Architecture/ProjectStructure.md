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
│   ├── audio/ninja_adventure/
│   ├── characters/ninja_adventure/
│   │   ├── animals/
│   │   ├── animated/
│   │   ├── bosses/
│   │   ├── characters/
│   │   └── monsters/
│   ├── effects/ninja_adventure/
│   ├── environment/ninja_adventure/
│   ├── items/ninja_adventure/
│   ├── licenses/ninja_adventure/
│   ├── reference/ninja_adventure/
│   └── ui/ninja_adventure/
├── data/
│   ├── dialogues/
│   ├── items/
│   ├── player/
│   ├── shops/
│   └── world/
├── Docs/
│   ├── Architecture/
│   └── Design/
├── scenes/
│   ├── characters/
│   ├── prototypes/
│   └── world/
│       ├── characters/
│       └── interactables/
├── scripts/
│   ├── bootstrap/
│   └── features/
│       ├── combat/
│       ├── exploration/
│       ├── interaction/
│       ├── playerdata/
│       └── shop/
├── Tests/
├── Tools/
│   └── Tests/
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
| `assets/audio/` | Imported music, sound effects and jingles. |
| `assets/characters/` | Player, NPC, animal, boss and monster artwork, grouped by source. |
| `assets/environment/` | Backgrounds, tilesets, props and environmental animation. |
| `assets/effects/` | Visual effects and particles. |
| `assets/items/` | Item and equipment artwork. |
| `assets/ui/` | Interface icons, frames and visual elements. |
| `assets/licenses/` | Third-party licenses retained with the imported source. |
| `assets/reference/` | Vendor readmes, previews and source reference images; not runtime content. |
| `data/` | Authored game content grouped by domain; stable IDs are separate from player-facing labels. |
| `scenes/` | Godot scenes grouped by player-facing feature or composition role. |
| `scripts/bootstrap/` | Application entry point and top-level runtime composition. |
| `scripts/features/` | Feature-owned gameplay and presentation code, grouped by domain. |
| `scripts/features/exploration/` | Player movement, animation and player-side interaction orchestration. |
| `scripts/features/interaction/` | Dialogue interactable configuration, dialogue UI and dialogue definitions/loading. |
| `scripts/features/playerdata/` | Typed item definitions, Inventory/currency state and player-data loading. |
| `scripts/features/shop/` | Shop state and transaction rules, authored catalogue loading, and interaction UI. |
| `Tests/` | .NET tests for pure game/domain behavior and integration seams. |
| `Tools/` | Repository validation and development utilities. |
| `Tools/Tests/` | Python unit tests for repository tooling. |
| `Docs/Design/` | Intended player experience and accepted gameplay rules. |
| `Docs/Architecture/` | Current code ownership, boundaries and runtime/data flows. |

`PlayerInteractionController` uses the Player's `InteractionRayCast2D` to detect the first physics body directly in front of the facing direction. World collision bodies block the ray; interactable collision bodies resolve to their parent `DialogueInteractableController`. Each interactable scene supplies its own `DialogueId` and `CanCancelInteraction`; NPC dialogue cannot be cancelled with Escape, while Terminal interaction can. The player controller owns the prompt, dialogue navigation, cancellation check and movement lock. Visual identity and dialogue content stay in the NPC scene and dialogue JSON rather than in the shared controller.

Keep scenes, scripts, data and assets separate by purpose. Keep feature-specific code near its feature. Shared code requires a real second consumer. Do not create empty classes, placeholder managers or generic abstractions to populate folders. The `.godot/` cache and generated build output must not be committed.

Use snake_case for scene/resource files (for example, `player_movement_test.tscn`) and PascalCase for C# files (for example, `PlayerController.cs`). Godot node names remain PascalCase. Imported vendor content below the `ninja_adventure` marker preserves upstream names. See [Coding standards](CodingStandards.md) for naming and ownership rules, and [Testing strategy](TestingStrategy.md) for test placement.
