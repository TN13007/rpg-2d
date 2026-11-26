# Inventory Contract

This document defines the shared runtime Inventory contract for gameplay features such as Shop, Gift, item rewards and future inventory UI.

## Source of truth

- A single \`PlayerData\` instance owns the player's Inventory during a runtime session.
- Features receive that instance; they do not create their own Inventory or edit its stack collection.
- Item definitions live in \`res://data/items/items.json\`. Inventory stacks reference the corresponding \`Item\` object from \`ItemDatabase\` and store a quantity.
- Runtime changes are in memory. Persistent save/load is outside this prototype contract.

## Item hierarchy

All item definitions inherit from the abstract \`Item\` class.

| Class | Purpose | Additional data |
|---|---|---|
| \`ConsumableItem\` | An item intended to be used by the player | None yet; effect execution is outside Inventory |
| \`EquipmentItem\` | An item that can occupy an equipment slot | \`equipmentSlot\` |
| \`QuestItem\` | An item associated with a quest | None yet; quest progression is outside Inventory |

Every item contains:

| Field | Type | Rule |
|---|---|---|
| \`id\` | string | Required and unique, compared case-insensitively |
| \`type\` | string | \`Consumable\`, \`Equipment\`, or \`Quest\` |
| \`name\` | string | Required |
| \`description\` | string | Optional display text |
| \`stackable\` | boolean | Whether copies can share a stack |
| \`maxStackSize\` | integer | Positive for stackable items; exactly 1 for non-stackable items |

Equipment additionally requires \`equipmentSlot\`, one of \`main_hand\`, \`off_hand\`, \`head\`, \`body\`, \`legs\`, \`feet\`, or \`accessory\`. Equipment is non-stackable and always has \`maxStackSize: 1\`.

\`ItemDatabase\` reads the \`type\` discriminator and creates the corresponding derived class. Unknown types, malformed entries, invalid equipment slots and duplicate IDs are skipped. A malformed manifest or unsupported schema version causes the manifest load to fail.

## Inventory stacks and API

A \`PlayerItemStack\` holds a reference to its canonical \`Item\` definition and a quantity. The player data JSON continues to store only \`itemId\` and \`quantity\`; \`PlayerDataDatabase\` resolves the ID through \`ItemDatabase\` when loading.

Namespace: \`RPG2D.Features.PlayerData\`

- \`GetItemQuantity(itemId)\`: returns the total quantity across all stacks with that item ID. Invalid or empty IDs return 0.
- \`HasItem(itemId, quantity = 1)\`: returns whether the requested positive quantity is available.
- \`TryAddItem(item, quantity)\`: adds a previously defined item. Existing stacks are filled first; additional stacks are created as needed.
- \`TryRemoveItem(itemId, quantity)\`: removes the requested quantity across one or more stacks and removes stacks that reach zero.

All mutation quantities must be positive. Invalid definitions, invalid quantities, insufficient quantity and integer overflow fail without changing Inventory.

The stack collection is exposed as read-only data. Features must use the public API rather than adding, removing or editing stack entries directly.

## Ownership boundaries

Inventory only manages item ownership and quantities. It does not apply gameplay effects such as healing, damage, equipment bonuses or NPC reactions. Those actions belong to the gameplay feature that consumes or uses an item.

Currency is a separate PlayerData concern and is not represented as an Inventory item.

The current contract defines equipment item types and their allowed slots only. It does not yet track which item is equipped, or implement equip/unequip transactions.

## Sample fixture

- \`health_potion\`: ConsumableItem, stackable, maximum 20 per stack.
- \`iron_sword\`: EquipmentItem, \`main_hand\`, non-stackable.
- \`ancient_key\`: QuestItem, stackable, maximum 10 per stack.
