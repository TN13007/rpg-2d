# Player Data Contract

This document defines the shared runtime data contract for features that read or change the player's currency and inventory, including Shop (#19) and Gift (#20).

## Source data

The prototype loads its starting state from `res://data/player/player_data.json`:

- `currency`: the player's starting currency.
- `inventory`: an array of item stacks. Each stack contains `itemId` and `quantity`.

Item names and descriptions are defined once in `res://data/items/items.json`. Features should store item IDs in inventory and resolve presentation through `ItemDatabase`.

The current prototype loads these JSON files at startup. Changes are held in memory for the running session; persistent saving and loading are not part of this contract yet.

## Runtime API

Namespace: `RPG2D.Features.PlayerData`

- `PlayerData.Currency`: current currency.
- `PlayerData.Inventory`: current item stacks.
- `PlayerData.GetItemQuantity(itemId)`: returns the owned quantity.
- `PlayerData.TryPurchaseItem(itemId, unitPrice, quantity, out failureReason)`: atomically checks funds and adds the purchased quantity while deducting currency.
- `PlayerData.TryRemoveItem(itemId, quantity)`: removes an available quantity; returns false without changing inventory when the requested quantity is unavailable.
- `ItemDatabase.TryGetItem(itemId, out item)`: resolves the shared item name and description.
- `PlayerDataDatabase.Load()`: loads the starting player data.

## Sharing the state between features

`PlayerInteractionController` creates one `PlayerData` instance and passes that same instance to interaction feature controllers. New features must receive and use that instance; they must not load a second copy or create their own inventory/currency collections.

Shop and Gift may own separate UI and interaction controllers. They must share `PlayerData` and `ItemDatabase`, and should keep their feature-specific rules in their own files.

## Prototype fixture

- Starting currency: 1000 vàng.
- Starting inventory: 1 × `item_a`.
- `item_a`: Đồ A — Mô tả A.
- `item_b`: Đồ B — Mô tả B.

These values are test fixtures, not a final economy balance.
