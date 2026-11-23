# Coding Standards

These rules combine Microsoft's C# conventions, Godot 4.7's C# and project-organization guidance, and the naming rules already used in TN13007/rpg. Where general guidance conflicts with the existing project convention, the project-specific rule below wins. Change a rule through an Issue.

## General principles
- Prefer clarity, explicit ownership and the simplest design that solves the current player-facing requirement.
- Use descriptive names; avoid unexplained abbreviations, Hungarian notation and unnecessary single-letter names.
- Use explicit access modifiers and properties instead of publicly mutable fields.
- Catch only exceptions that can be handled meaningfully. Do not swallow exceptions.
- Use async/await for genuinely asynchronous I/O, not ordinary synchronous gameplay logic.
- Do not add interfaces, factories, managers or service layers for hypothetical reuse.

## C# naming

Follow the [Godot 4.7 C# style guide](https://docs.godotengine.org/en/4.7/tutorials/scripting/c_sharp/c_sharp_style_guide.html) and [Microsoft identifier naming rules](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names), with these project conventions:

| Element | Convention | Example |
|---|---|---|
| Namespace, class, struct, enum, delegate | PascalCase | `RPG2D.Exploration`, `PlayerController` |
| Interface | I + PascalCase | `IInteractable` |
| Method, property, event | PascalCase | `MoveTo()`, `CurrentHealth` |
| Constant | PascalCase | `DefaultMoveSpeed` |
| Private/protected/internal field | _camelCase | `_moveSpeed` |
| Parameter and local variable | camelCase | `targetPosition` |
| Generic type parameter | T-prefixed PascalCase | `TResult` |

Use the primary class name as its C# file name. Keep namespaces aligned with feature/domain ownership. Avoid abbreviations unless widely understood.

## Formatting and language
- UTF-8, LF, four spaces, one final newline and Allman braces.
- One statement per line; separate members with a blank line.
- Use `var` only when the assigned type is obvious from the expression.
- Keep nullable reference types enabled and fix warnings instead of suppressing them broadly.
- Use language features supported by the Godot 4.7.2 .NET target.
- Add XML documentation to public APIs when their contract is not self-evident.
- Do not rename Godot-defined callbacks or overrides.

## Godot naming

Godot's general guide recommends snake_case for folders and non-C# resources, PascalCase for nodes, and PascalCase C# files. This repository deliberately retains the existing RPG project's PascalCase convention for game-owned directories and file stems, to keep scenes, resources, scripts and tests consistent.

| Element | Convention | Example |
|---|---|---|
| Game-owned directory | PascalCase | `Scenes/Exploration/` |
| C# script | PascalCase | `PlayerController.cs` |
| Scene/resource file stem | PascalCase | `DormitoryRoom.tscn` |
| Node name | PascalCase | `InteractionPrompt` |
| Input action | snake_case | `move_left`, `interact` |
| Signal concept | Past-tense phrase | `interaction_completed` |
| Godot sidecar | Engine-generated; do not rename manually | `.uid`, `.import` |

Node paths, resource UIDs, signal names and input actions are compatibility-sensitive. Rename them only in a focused migration and update all references. Never rename third-party asset files as unrelated cleanup.

## Authored data
- JSON property names use camelCase.
- Stable content IDs/references use snake_case, e.g. `dormitory_room`, `player_ari`.
- Serialized enum-like values use PascalCase, e.g. `RestoreHealth`.
- Player-facing labels are natural language, not identifiers.
- Keep IDs independent from display names so copy/localization changes do not break references.
- Validate required fields, uniqueness and references at the content-loading boundary. Report malformed data; do not silently repair it.

The initial validator checks changed JSON documents and a small explicit set of enum-like property names. Extend that list when the real schema requires it; do not guess that every string is an enum.

## Architecture and ownership
- A scene owns its composition and node wiring.
- A feature owns its rules and presentation behavior. Keep feature code together until real shared use is demonstrated.
- Keep gameplay rules testable without rendering where practical.
- UI presents state and forwards player intent; it is not the source of truth for game rules.
- Resources/data describe authored content; avoid large content tables embedded in scripts.
- Use signals for meaningful events; avoid signal networks that obscure state ownership.
- Do not introduce a global service locator, generic EventBus, Manager or Repository without a concrete requirement.
- Do not create architecture layers merely to mirror folders.

## Godot lifecycle
- Use the appropriate Godot lifecycle callback; separate initialization from per-frame work.
- Cache node references when needed rather than repeatedly searching the tree in hot paths.
- Prefer signals or explicit methods over polling for infrequent state changes.
- Scope/disconnect signal subscriptions appropriately when nodes are freed.
- Avoid expensive work in `_Process` or `_PhysicsProcess` when an event is more appropriate.
- Keep physics interactions in the physics lifecycle and use Godot physics APIs safely.
- Preserve scene ownership and resource references; inspect affected scenes after changing node names or paths.

## Comments and documentation
Explain why a non-obvious decision exists, not what syntax already says. Update canonical design/architecture docs only for durable decisions. Do not add permanent documents for temporary implementation plans.
