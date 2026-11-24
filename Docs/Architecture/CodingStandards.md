# Coding Standards

These rules follow Microsoft's C# conventions and Godot 4.7's C# and project-organization guidance. Where Godot-specific guidance is relevant, prefer the engine convention unless this document records a deliberate project constraint.

## General principles
- Prefer clarity, explicit ownership and the simplest design that solves the current player-facing requirement.
- Use descriptive names; avoid unexplained abbreviations, Hungarian notation and unnecessary single-letter names.
- Use explicit access modifiers and properties instead of publicly mutable fields.
- Catch only exceptions that can be handled meaningfully. Do not swallow exceptions.
- Use async/await for genuinely asynchronous I/O, not ordinary synchronous gameplay logic.
- Do not add interfaces, factories, managers or service layers for hypothetical reuse.

## C# naming

Follow the [Godot 4.7 C# style guide](https://docs.godotengine.org/en/4.7/tutorials/scripting/c_sharp/c_sharp_style_guide.html) and [Microsoft identifier naming rules](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/identifier-names).

| Element | Convention | Example |
|---|---|---|
| Namespace, class, struct, enum, delegate | PascalCase | `RPG2D.Exploration`, `PlayerController` |
| Interface | `I` + PascalCase | `IInteractable` |
| Method, property, event | PascalCase | `MoveTo()`, `CurrentHealth` |
| Constant | PascalCase | `DefaultMoveSpeed` |
| Private field | `_camelCase` | `_moveSpeed` |
| Parameter and local variable | camelCase | `targetPosition` |
| Generic type parameter | T-prefixed PascalCase | `TResult` |

**Acronyms:** Follow Godot's two-character exception. Acronyms consisting of exactly two letters remain uppercase where PascalCase is expected and lowercase where camelCase is expected. Longer acronyms use normal PascalCase/camelCase.

| Context | Correct | Incorrect |
|---|---|---|
| Type/member | `UIManager`, `UI` | `UiManager`, `Ui` |
| Local/parameter | `uiManager`, `ui` | `uIManager`, `UIManager` |
| Private field | `_uiManager` | `_UiManager` |
| Identifier | `Id`, `PlayerId` | `ID`, `PlayerID` |
| Longer acronym | `CpuParticles2D`, `HttpClient` | `CPUParticles2D`, `HTTPClient` |

Use the primary class name as its C# file name. Keep namespaces aligned with feature/domain ownership. Do not shorten names when it reduces readability.

## Formatting and language
- UTF-8, LF, four spaces, one final newline and Allman braces.
- One statement per line; separate members with a blank line.
- Use `var` only when the assigned type is obvious from the expression.
- Keep nullable reference types enabled and fix warnings instead of suppressing them broadly.
- Use language features supported by the Godot 4.7.2 .NET target.
- Add XML documentation to public APIs when their contract is not self-evident.
- Do not rename Godot-defined callbacks or overrides.

## Godot naming

Follow [Godot's project organization guide](https://docs.godotengine.org/en/4.7/tutorials/best_practices/project_organization.html):

| Element | Convention | Example |
|---|---|---|
| Game-owned directories | snake_case | `scenes/exploration/` |
| Non-C# resource files | snake_case | `dormitory_room.tscn`, `player_portrait.png` |
| C# script files | PascalCase, matching class | `PlayerController.cs` |
| C# project files | PascalCase | `RPG2D.csproj` |
| Godot node names | PascalCase | `InteractionPrompt`, `Camera2D` |
| Input action names | snake_case | `move_left`, `interact` |
| C# signals/events | Past-tense PascalCase | `InteractionCompleted` |

Repository-maintenance areas retain their established names: `Docs/`, `Tools/`, `Tests/` and `.github/`. Imported Ninja Adventure assets are grouped under `assets/<category>/ninja_adventure/`. The project-owned category and vendor marker use snake_case; everything beneath the vendor marker preserves the upstream folder names and filenames exactly. This avoids changing asset identity or breaking any references while keeping imported files visibly separate from project-authored assets. The original CC0 license and source README are retained under `assets/licenses/ninja_adventure/` and `assets/reference/ninja_adventure/`.

Godot sidecars such as `.uid` and `.import` are engine-managed; do not rename or edit them manually. Node paths, resource UIDs, signal names and input actions are compatibility-sensitive. Rename them only in a focused migration and update all references.

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
