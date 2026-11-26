using System;
using System.Collections.Generic;
using RPG2D.Features.PlayerData;
using PlayerDataState = RPG2D.Features.PlayerData.PlayerData;

namespace RPG2D.Features.Shop
{
    public enum ShopMode
    {
        Buying,
        Selling
    }

    public enum ShopSessionOutcome
    {
        None,
        LeaveRequested
    }

    public enum ShopFeedbackKind
    {
        None,
        Error,
        ItemNotAccepted,
        PurchaseSucceeded,
        SaleSucceeded,
        CurrencyAdded,
        CurrencyAddFailed,
        CurrencySpent,
        CurrencySpendFailed
    }

    public sealed class ShopFeedback
    {
        public ShopFeedback(
            ShopFeedbackKind kind,
            string message = "",
            string itemName = "",
            int quantity = 0,
            long amount = 0
        )
        {
            Kind = kind;
            Message = message;
            ItemName = itemName;
            Quantity = quantity;
            Amount = amount;
        }

        public ShopFeedbackKind Kind { get; }
        public string Message { get; }
        public string ItemName { get; }
        public int Quantity { get; }
        public long Amount { get; }

        public static ShopFeedback None => new(ShopFeedbackKind.None);
    }

    public sealed class ShopItemOption
    {
        public ShopItemOption(
            string itemId,
            Item? item,
            int unitPrice,
            int stock,
            int ownedQuantity,
            bool isAvailable
        )
        {
            ItemId = itemId;
            Item = item;
            UnitPrice = unitPrice;
            Stock = stock;
            OwnedQuantity = ownedQuantity;
            IsAvailable = isAvailable;
        }

        public string ItemId { get; }
        public Item? Item { get; }
        public int UnitPrice { get; }
        public int Stock { get; }
        public int OwnedQuantity { get; }
        public bool IsAvailable { get; }
    }

    public sealed class ShopSessionView
    {
        public ShopSessionView(
            string shopName,
            ShopMode mode,
            IReadOnlyList<ShopItemOption> items,
            int selectedIndex,
            int selectedQuantity,
            int currency,
            bool inventoryIsEmpty,
            ShopFeedback feedback
        )
        {
            ShopName = shopName;
            Mode = mode;
            Items = items;
            SelectedIndex = selectedIndex;
            SelectedQuantity = selectedQuantity;
            Currency = currency;
            InventoryIsEmpty = inventoryIsEmpty;
            Feedback = feedback;
        }

        public string ShopName { get; }
        public ShopMode Mode { get; }
        public IReadOnlyList<ShopItemOption> Items { get; }
        public int SelectedIndex { get; }
        public int SelectedQuantity { get; }
        public int Currency { get; }
        public bool InventoryIsEmpty { get; }
        public ShopFeedback Feedback { get; }
    }

    public sealed class ShopSession
    {
        private const int DebugCurrencyAmount = 1000;

        private readonly ShopDefinition _shop;
        private readonly PlayerDataState _playerData;
        private readonly ItemDatabase _itemDatabase;

        private ShopMode _mode = ShopMode.Buying;
        private int _selectedIndex;
        private int _selectedQuantity = 1;
        private ShopFeedback _feedback = ShopFeedback.None;

        public ShopSession(
            ShopDefinition shop,
            PlayerDataState playerData,
            ItemDatabase itemDatabase
        )
        {
            _shop = shop ?? throw new ArgumentNullException(nameof(shop));
            _playerData = playerData ?? throw new ArgumentNullException(nameof(playerData));
            _itemDatabase = itemDatabase ?? throw new ArgumentNullException(nameof(itemDatabase));
        }

        public ShopMode Mode => _mode;

        public void SetMode(ShopMode mode)
        {
            if (_mode == mode)
            {
                return;
            }

            _mode = mode;
            _selectedIndex = Math.Clamp(_selectedIndex, 0, BuildItems().Count);
            _selectedQuantity = 1;
            _feedback = ShopFeedback.None;
        }

        public void MoveSelection(int direction)
        {
            int optionCount = BuildItems().Count + 1;
            if (optionCount <= 0 || direction == 0)
            {
                return;
            }

            int nextIndex = _selectedIndex + direction;
            _selectedIndex = ((nextIndex % optionCount) + optionCount) % optionCount;
            _selectedQuantity = 1;
            _feedback = ShopFeedback.None;
        }

        public void SelectItem(int index)
        {
            int itemCount = BuildItems().Count;
            if (index < 0 || index > itemCount)
            {
                return;
            }

            _selectedIndex = index;
            _selectedQuantity = 1;
            _feedback = ShopFeedback.None;
        }

        public void ChangeQuantity(int direction)
        {
            List<ShopItemOption> items = BuildItems();
            if (direction == 0 || _selectedIndex < 0 || _selectedIndex >= items.Count)
            {
                return;
            }

            ShopItemOption option = items[_selectedIndex];
            if (!option.IsAvailable)
            {
                return;
            }

            int maximumQuantity = _mode == ShopMode.Buying
                ? option.Stock
                : option.OwnedQuantity;
            long nextQuantity = (long)_selectedQuantity + direction;

            if (nextQuantity < 1 || nextQuantity > maximumQuantity)
            {
                return;
            }

            _selectedQuantity = (int)nextQuantity;
            _feedback = ShopFeedback.None;
        }

        public ShopSessionOutcome ConfirmSelection()
        {
            List<ShopItemOption> items = BuildItems();
            if (_selectedIndex == items.Count)
            {
                return ShopSessionOutcome.LeaveRequested;
            }

            if (_selectedIndex < 0 || _selectedIndex >= items.Count)
            {
                return ShopSessionOutcome.None;
            }

            if (_mode == ShopMode.Buying)
            {
                ConfirmPurchase();
            }
            else
            {
                ConfirmSale(items[_selectedIndex]);
            }

            return ShopSessionOutcome.None;
        }

        public void AdjustCurrency(bool increase)
        {
            if (increase)
            {
                _feedback = _playerData.TryAddCurrency(DebugCurrencyAmount)
                    ? new ShopFeedback(
                        ShopFeedbackKind.CurrencyAdded,
                        amount: DebugCurrencyAmount
                    )
                    : new ShopFeedback(ShopFeedbackKind.CurrencyAddFailed);
                return;
            }

            _feedback = _playerData.TrySpendCurrency(DebugCurrencyAmount)
                ? new ShopFeedback(
                    ShopFeedbackKind.CurrencySpent,
                    amount: DebugCurrencyAmount
                )
                : new ShopFeedback(
                    ShopFeedbackKind.CurrencySpendFailed,
                    amount: DebugCurrencyAmount
                );
        }

        public ShopSessionView CreateView()
        {
            List<ShopItemOption> items = BuildItems();
            _selectedIndex = Math.Clamp(_selectedIndex, 0, items.Count);

            return new ShopSessionView(
                _shop.Name,
                _mode,
                items.AsReadOnly(),
                _selectedIndex,
                _selectedQuantity,
                _playerData.Currency,
                _mode == ShopMode.Selling && items.Count == 0,
                _feedback
            );
        }

        private List<ShopItemOption> BuildItems()
        {
            return _mode == ShopMode.Buying
                ? BuildPurchaseItems()
                : BuildOwnedItems();
        }

        private List<ShopItemOption> BuildPurchaseItems()
        {
            List<ShopItemOption> items = new();

            foreach (ShopItemDefinition shopItem in _shop.Items)
            {
                bool hasItem = _itemDatabase.TryGetItem(shopItem.ItemId, out Item item);
                items.Add(
                    new ShopItemOption(
                        shopItem.ItemId,
                        hasItem ? item : null,
                        shopItem.Price,
                        shopItem.Stock,
                        0,
                        hasItem
                    )
                );
            }

            return items;
        }

        private List<ShopItemOption> BuildOwnedItems()
        {
            List<ShopItemOption> items = new();
            Dictionary<string, int> optionIndexes = new(StringComparer.OrdinalIgnoreCase);

            foreach (PlayerItemStack stack in _playerData.Inventory)
            {
                if (optionIndexes.TryGetValue(stack.ItemId, out int existingIndex))
                {
                    ShopItemOption existingOption = items[existingIndex];
                    items[existingIndex] = new ShopItemOption(
                        existingOption.ItemId,
                        existingOption.Item,
                        existingOption.UnitPrice,
                        existingOption.Stock,
                        existingOption.OwnedQuantity + stack.Quantity,
                        existingOption.IsAvailable
                    );
                    continue;
                }

                int buybackPrice = _shop.TryGetBuybackPrice(stack.ItemId, out int configuredPrice)
                    ? configuredPrice
                    : 0;

                optionIndexes.Add(stack.ItemId, items.Count);
                items.Add(
                    new ShopItemOption(
                        stack.ItemId,
                        stack.Item,
                        buybackPrice,
                        0,
                        stack.Quantity,
                        buybackPrice > 0
                    )
                );
            }

            return items;
        }

        private void ConfirmPurchase()
        {
            ShopItemDefinition shopItem = _shop.Items[_selectedIndex];
            if (!_itemDatabase.TryGetItem(shopItem.ItemId, out Item item))
            {
                _feedback = new ShopFeedback(
                    ShopFeedbackKind.Error,
                    "Không tìm thấy dữ liệu vật phẩm."
                );
                return;
            }

            if (shopItem.Stock < _selectedQuantity)
            {
                _feedback = new ShopFeedback(
                    ShopFeedbackKind.Error,
                    "Số lượng hàng trong cửa hàng không đủ."
                );
                return;
            }

            if (!_playerData.TryPurchaseItem(
                item,
                shopItem.Price,
                _selectedQuantity,
                out string failureReason
            ))
            {
                _feedback = new ShopFeedback(ShopFeedbackKind.Error, failureReason);
                return;
            }

            shopItem.Stock -= _selectedQuantity;
            _feedback = new ShopFeedback(
                ShopFeedbackKind.PurchaseSucceeded,
                itemName: item.Name,
                quantity: _selectedQuantity
            );
        }

        private void ConfirmSale(ShopItemOption option)
        {
            if (!option.IsAvailable)
            {
                _feedback = new ShopFeedback(ShopFeedbackKind.ItemNotAccepted);
                return;
            }

            int quantity = _selectedQuantity;
            long totalRevenue = (long)option.UnitPrice * quantity;
            string soldItemId = option.ItemId;
            string soldItemName = option.Item?.Name ?? option.ItemId;

            if (!_playerData.TrySellItem(
                soldItemId,
                option.UnitPrice,
                quantity,
                out string failureReason
            ))
            {
                _feedback = new ShopFeedback(ShopFeedbackKind.Error, failureReason);
                return;
            }

            _feedback = new ShopFeedback(
                ShopFeedbackKind.SaleSucceeded,
                itemName: soldItemName,
                quantity: quantity,
                amount: totalRevenue
            );

            List<ShopItemOption> updatedItems = BuildOwnedItems();
            int preferredIndex = updatedItems.FindIndex(
                item => string.Equals(
                    item.ItemId,
                    soldItemId,
                    StringComparison.OrdinalIgnoreCase
                )
            );
            _selectedIndex = preferredIndex >= 0
                ? preferredIndex
                : Math.Min(_selectedIndex, updatedItems.Count);
            _selectedQuantity = 1;
        }
    }
}