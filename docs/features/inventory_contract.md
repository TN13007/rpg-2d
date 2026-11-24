# Inventory Contract

This document defines the shared runtime Inventory contract for gameplay features such as Shop, Gift, item rewards and future inventory UI.

## Source of truth

- A single `PlayerData` instance owns the player's Inventory during a runtime session.
- Features receive that instance; they do not create their own Inventory or edit its stack collection.
- Item definitions live in `res://data/items/items.json`. Inventory stores item IDs and quantities, not display names.
- Runtime changes are in memory. Persistent save/load is outside this prototype contract.

## Item definitions

Each item defines:

| Field | Type | Rule |
|---|---|---|
| `id` | string | Required and unique, compared case-insensitively |
| `name` | string | Required |
| `description` | string | Optional display text |
| `stackable` | boolean | Whether copies can share a stack |
| `maxStackSize` | integer | Positive for stackable items; exactly 1 for non-stackable items |

A non-stackable item always occupies one stack with quantity 1. A stackable item may occupy multiple stacks when its total quantity exceeds `maxStackSize`.

## Public API

Namespace: `RPG2D.Features.PlayerData`

- `GetItemQuantity(itemId)`: returns the total quantity across all stacks with that item ID. Invalid or empty IDs return 0.
- `HasItem(itemId, quantity = 1)`: returns whether the requested positive quantity is available.
- `TryAddItem(itemDefinition, quantity)`: adds a previously defined item. Existing stacks are filled first; additional stacks are created as needed.
- `TryRemoveItem(itemId, quantity)`: removes the requested quantity across one or more stacks and removes stacks that reach zero.

All mutation quantities must be positive. Invalid definitions, invalid quantities, insufficient quantity and integer overflow fail without changing Inventory.

The stack collection is exposed as read-only data. Features must use the public API rather than adding, removing or editing stack entries directly.

## Ownership boundaries

Inventory only manages item ownership and quantities. It does not apply gameplay effects such as healing, damage, equipment bonuses or NPC reactions. Those actions belong to the gameplay feature that consumes or uses an item.

Currency is a separate PlayerData concern and is not represented as an Inventory item.

## Sample fixture

- `health_potion`: stackable, maximum 20 per stack.
- `iron_sword`: non-stackable, one item per stack.
