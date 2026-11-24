# Player Data Contract

This document describes how gameplay features share player currency and Inventory.

## Runtime ownership

- One `PlayerData` instance owns the player's currency and Inventory for the current session.
- `PlayerInteractionController` creates the shared `ItemDatabase` and `PlayerData` instances, then passes them to feature controllers.
- Feature controllers must not load another copy of player data or mutate Inventory stacks directly.
- Inventory stack rules and the four core Inventory APIs are documented in [Inventory Contract](inventory_contract.md).

## Currency API

- `Currency`: read-only current balance.
- `TryAddCurrency(amount)`: adds a positive amount if the balance will not overflow.
- `TrySpendCurrency(amount, allowDebt = false)`: spends a positive amount. Debt is denied by default and must be explicitly allowed by the caller.
- `TryPurchaseItem(itemDefinition, unitPrice, quantity, out failureReason)`: validates funds and item addition, adds through `TryAddItem`, then deducts currency. If validation or item addition fails, neither currency nor Inventory changes.

Currency is not an Inventory item. Shop purchases do not allow debt.

## Loading

- `ItemDatabase.Load()` reads item definitions from `res://data/items/items.json`.
- `PlayerDataDatabase.Load(itemDatabase)` reads the starting currency and Inventory from `res://data/player/player_data.json`.
- Runtime changes remain in memory. Persistence is outside this prototype.

## Feature boundaries

Shop owns shop stock, price and purchase UI. It delegates the player-side transaction to `PlayerData.TryPurchaseItem`, and decreases stock only after that method succeeds.

Gift and future item-reward features should use `TryRemoveItem` and `TryAddItem` rather than editing the Inventory collection.