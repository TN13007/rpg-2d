# Shop Contract

This document defines the prototype Shop's purchase and sell behavior and its code ownership boundaries.

## Runtime ownership and responsibilities

- `PlayerInteractionController` creates one shared `ItemDatabase` and one shared `PlayerData` instance.
- `ShopController` is the Godot controller. It receives shared player data, translates keyboard and UI events into Shop data commands, refreshes the screen and reports when the player leaves the Shop.
- `ShopData` owns runtime Shop state and behavior: buy/sell mode, selection, quantity, purchase stock, sellable Inventory options and purchase/sale actions. It contains no Godot UI nodes.
- `ShopDatabase` loads the authored Shop definition from `res://data/shops/default_shop.json`.
- `ShopInteractionUI` builds and renders Godot controls, formats Shop data into player-facing labels/details, and raises events for player intent. It does not perform purchase or sale transactions.
- `PlayerData` remains the source of truth for owned item quantities and currency. `ShopData` delegates player-side changes to `PlayerData.TryPurchaseItem` and `PlayerData.TrySellItem`.
- NPC dialogue content remains in `data/dialogues/dialogues.json`. Choosing **Cửa hàng** navigates to a dedicated dialogue node with **Mua hàng** and **Bán vật phẩm** choices. Each choice invokes the `openShop` action with a required `shopMode` value (`Buying` or `Selling`).
- `PlayerInteractionController` validates the dialogue action mode and passes it to `ShopController.Open(mode)`. `ShopData` receives an immutable mode when created; the Shop UI does not switch modes.
- Leaving the Shop returns to the dialogue node that launched that mode (the Shop menu), allowing the player to choose the other action. Esc closes the entire interaction.
- The current Shop has no VFX behavior. Item selection styling remains in `ShopInteractionUI`; mode selection belongs to Dialogue.

## Purchase catalogue

The `items` array defines goods the Shop sells to the player:

| Field | Meaning |
|---|---|
| `itemId` | Existing ID from `data/items/items.json` |
| `price` | Player's purchase price per item |
| `stock` | Current Shop stock for this runtime session |

A purchase uses `PlayerData.TryPurchaseItem`. Shop stock decreases only after the player-side transaction succeeds. Failed purchases leave currency, Inventory and stock unchanged. After a successful purchase, the selected quantity is clamped to the remaining stock so the player can continue buying the rest. When stock reaches zero, the UI shows `Hết hàng`.

`ShopDatabase` rejects malformed catalogue entries and duplicate item IDs instead of silently removing them. A buyback entry with price `0` is allowed as a disabled/not-accepted item; negative prices are invalid.

## Selling from the player's Inventory

The sell list is generated from `PlayerData.Inventory` whenever `ShopData.Items` is read.

- Each distinct `ItemId` appears once, with quantities from all of its Inventory stacks combined.
- The displayed quantity is the amount currently owned, not Shop stock.
- The selected sale quantity must be between 1 and the owned quantity.
- The Shop's `buybackPrices` array defines which item IDs it accepts and the amount it pays per item.
- An owned item without a configured buyback price remains visible as not accepted; confirmation does not change player data.
- After a successful sale, the sell list is rebuilt from the updated Inventory, so fully sold items disappear and remaining quantities refresh.

Example buyback policy:

```json
"buybackPrices": [
  { "itemId": "item_a", "price": 50 },
  { "itemId": "health_potion", "price": 40 },
  { "itemId": "iron_sword", "price": 150 }
]
```

The prototype uses fixed prices. It does not calculate sell prices automatically from purchase prices.

## Sale transaction

`PlayerData.TrySellItem(itemId, unitPrice, quantity, out failureReason)` owns the player-side sale transaction:

1. Validate item ID, positive unit price and positive quantity.
2. Verify that the player owns the requested quantity.
3. Calculate revenue using a wide integer and reject values outside the supported currency range.
4. Verify that crediting the revenue will not overflow the currency balance.
5. Remove the requested quantity through `TryRemoveItem`.
6. Credit the currency only after the removal succeeds.

Every rejected sale leaves both Inventory and currency unchanged. Removing the final quantity removes the corresponding stack. If the item exists in multiple stacks, removal spans those stacks.

## Controls

| Input | Action |
|---|---|
| W / S | Select a catalogue or owned Inventory item |
| A / D | Decrease / increase transaction quantity |
| Space | Confirm purchase/sale or leave the Shop |
| Esc | Close the interaction and restore player control |

The currency adjustment buttons are prototype test controls and are not part of the intended finished economy.

## Current test fixtures

- Starting currency: 1,000 vàng.
- Starting Inventory: 1 × Đồ A, 2 × Bình máu, 1 × Kiếm sắt, 1 × Chìa khóa cổ.
- Shop buys from the player: Đồ A (50), Đồ B (125), Bình máu (40), Kiếm sắt (150).
- Chìa khóa cổ is intentionally not accepted, allowing the UI's unavailable-item behavior to be tested.
- Shop sells Đồ A for 100 (stock 20) and Đồ B for 250 (stock 10).

## Verification scenarios

- Buy an item and confirm currency, Inventory and Shop stock update once.
- Sell one unit from a stack and confirm only that quantity is removed.
- Sell the remaining quantity and confirm the row disappears from the Shop view.
- Sell an item represented by multiple Inventory stacks and confirm removal spans stacks.
- Attempt to sell more than owned, an unaccepted item or an invalid quantity; confirm no player data changes.
- Attempt a sale that would overflow the currency balance; confirm Inventory and currency remain unchanged.
- From the NPC menu, choose Cửa hàng, then Mua hàng; leave and verify the Shop menu reappears.
- Choose Bán vật phẩm from the Shop menu; leave and verify the same menu reappears.
- Press Esc from the Shop menu and from either Shop mode; confirm the interaction closes and player control returns.

Runtime changes are in memory. Save/load persistence, dynamic prices, restocking and a general player Inventory screen are outside this prototype contract.
