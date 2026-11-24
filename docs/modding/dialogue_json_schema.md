# Dialogue JSON Modding

Dialogue content is loaded from two locations:

- Built-in content: `res://data/dialogues/dialogues.json`
- User mods: `user://mods/dialogues/*.json`

The game creates the user mod directory on startup. On Windows, `user://` resolves to the game's Godot user data directory. Put mod JSON files in its `mods/dialogues` subdirectory.

## File format

Each file must contain a manifest with `schemaVersion` set to `1` and a `dialogues` array:

```json
{
  "schemaVersion": 1,
  "dialogues": [
    {
      "id": "terminal_intro",
      "title": "Terminal",
      "message": "System online. No new messages.",
      "choices": [
        {
          "id": "status",
          "text": "Check system status"
        },
        {
          "id": "leave",
          "text": "Leave",
          "closesInteraction": true
        }
      ]
    }
  ]
}
```

## Fields

| Field | Type | Required | Description |
|---|---|---:|---|
| `schemaVersion` | integer | Yes | Must be `1`. |
| `dialogues` | array | Yes | Dialogue entries contained in this file. |
| `id` | string | Yes | Stable dialogue identifier. IDs are case-insensitive. |
| `title` | string | Yes | Title shown by the interaction UI. |
| `message` | string | Yes | Main dialogue text. |
| `choices` | array | Yes | Choices shown to the player; an empty array is allowed. |
| `choices[].id` | string | Yes | Unique choice identifier within its dialogue. |
| `choices[].text` | string | Yes | Text shown for the choice. |
| `choices[].closesInteraction` | boolean | No | When true, selecting the choice closes the interaction. Defaults to false. |

## Loading and overrides

Built-in dialogues load first. Mod files load afterward in alphabetical filename order. A mod entry with the same dialogue `id` replaces the entire built-in dialogue entry; it does not merge individual choices. This makes it possible to translate or rewrite a built-in dialogue without changing game files.

Different mod files that define the same ID are resolved by alphabetical filename order: the later file wins. Use unique IDs to avoid accidental conflicts.

Invalid JSON files and invalid dialogue entries are skipped with a warning in the Godot output. The game does not execute code from JSON. Dialogue JSON currently controls display text, choice order, and the close-interaction behavior only.

## Example mod

Create a file such as `my_terminal_overhaul.json` in `user://mods/dialogues/`:

```json
{
  "schemaVersion": 1,
  "dialogues": [
    {
      "id": "terminal_intro",
      "title": "Maintenance Console",
      "message": "Choose a maintenance operation.",
      "choices": [
        { "id": "diagnostics", "text": "Run diagnostics" },
        { "id": "shutdown", "text": "Shut down", "closesInteraction": true }
      ]
    }
  ]
}
```

Restart the game to load the mod. The built-in terminal uses the `terminal_intro` ID by default.
