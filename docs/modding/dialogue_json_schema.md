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
      "message": "System online.",
      "choices": [
        {
          "id": "open_status",
          "text": "Check system status",
          "nextDialogueId": "terminal_status"
        },
        {
          "id": "open_shop_buy",
          "text": "Buy items",
          "action": "openShop",
          "shopMode": "Buying"
        },
        {
          "id": "open_shop_sell",
          "text": "Sell items",
          "action": "openShop",
          "shopMode": "Selling"
        },
        {
          "id": "leave",
          "text": "Leave",
          "closesInteraction": true
        }
      ]
    },
    {
      "id": "terminal_status",
      "title": "System status",
      "message": "All systems operational. Press Space to return.",
      "choices": [],
      "nextDialogueId": "terminal_intro"
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
| `nextDialogueId` | string | No | Dialogue shown when Space is pressed on a page with no choices. |
| `choices[].id` | string | Yes | Unique choice identifier within its dialogue. |
| `choices[].text` | string | Yes | Text shown for the choice. |
| `choices[].action` | string | No | Supported action. `openShop` opens the Shop interface. |
| `choices[].shopMode` | string | Required for `openShop` | Must be `Buying` or `Selling`; selects the Shop mode before its UI opens. |
| `choices[].closesInteraction` | boolean | No | When true, selecting the choice closes the interaction. Defaults to false. |
| `choices[].nextDialogueId` | string | No | Dialogue shown after selecting this choice. |
 
A dialogue with no choices can use `nextDialogueId` to create a sequence of pages advanced with Space. A choice can use `nextDialogueId` to branch to another dialogue. A terminal page can point back to the menu dialogue to return to its choices. An `openShop` choice must provide a valid `shopMode`; invalid Shop action modes cause the containing dialogue entry to be skipped during loading.

## Loading and overrides

Built-in dialogues load first. Mod files load afterward in alphabetical filename order. A mod entry with the same dialogue `id` replaces the entire built-in dialogue entry; it does not merge individual choices. This makes it possible to translate or rewrite a built-in dialogue without changing game files.

Different mod files that define the same ID are resolved by alphabetical filename order: the later file wins. Use unique IDs to avoid accidental conflicts.

Invalid JSON files and invalid dialogue entries are skipped with a warning in the Godot output. The game does not execute code from JSON. Dialogue JSON controls display text, choice order, navigation links, the close-interaction behavior, and the supported `openShop` action with its required mode.

## Example mod

Create a file such as `my_terminal_overhaul.json` in `user://mods/dialogues/`. Use the same `schemaVersion` and dialogue structure shown above. Restart the game to load the mod. The built-in terminal uses the `terminal_intro` ID by default.
