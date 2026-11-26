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
- `TryPurchaseItem(item, unitPrice, quantity, out failureReason)`: validates funds and item addition, adds through `TryAddItem`, then deducts currency. If validation or item addition fails, neither currency nor Inventory changes.
- `TrySellItem(itemId, unitPrice, quantity, out failureReason)`: validates owned quantity and currency overflow, removes through `TryRemoveItem`, then credits the sale proceeds. If validation or removal fails, neither currency nor Inventory changes.

Currency is not an Inventory item. Shop purchases do not allow debt. The Shop debug controls can add or spend 1,000 vàng per click; spending is rejected when the balance is insufficient.

## Loading

- `ItemDatabase.Load()` reads item definitions from `res://data/items/items.json`.
- `PlayerDataDatabase.Load(itemDatabase)` reads the starting currency and Inventory from `res://data/player/player_data.json`.
- Runtime changes remain in memory. Persistence is outside this prototype.

## Feature boundaries

Shop data owns purchase stock and buyback prices. `ShopData` coordinates Shop-side selection and transaction flow, but delegates player-data mutations to `PlayerData.TryPurchaseItem` and `PlayerData.TrySellItem`. `ShopInteractionUI` formats and renders the Shop data; the UI must not edit Inventory or currency directly.

Gift and future item-reward features should use `TryRemoveItem` and `TryAddItem` rather than editing the Inventory collection.
