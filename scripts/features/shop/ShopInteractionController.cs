using System;
using System.Collections.Generic;
using Godot;
using RPG2D.Features.PlayerData;
using PlayerDataState = RPG2D.Features.PlayerData.PlayerData;

namespace RPG2D.Features.Shop
{
    public partial class ShopInteractionController : Node
    {
        private enum ShopMode
        {
            Buying,
            Selling
        }

        private sealed class OwnedItemOption
        {
            public OwnedItemOption(Item item, int quantity, int buybackPrice)
            {
                Item = item;
                Quantity = quantity;
                BuybackPrice = buybackPrice;
            }

            public Item Item { get; }
            public int Quantity { get; set; }
            public int BuybackPrice { get; }
        }

        public event Action? LeaveRequested;

        private PlayerDataState _playerData = null!;
        private ItemDatabase _itemDatabase = null!;
        private ShopInteractionUI _shopUi = null!;
        private ShopDefinition? _shop;
        private readonly List<OwnedItemOption> _ownedItems = new();
        private ShopMode _mode = ShopMode.Buying;
        private int _selectedIndex;
        private int _selectedQuantity = 1;
        private bool _tabWasPressed;

        public bool IsOpen => _shop != null && _shopUi.IsShopVisible;

        public override void _Ready()
        {
            _shopUi = new ShopInteractionUI();
            AddChild(_shopUi);
            _shopUi.ItemHovered += SelectItem;
            _shopUi.ItemConfirmed += OnItemConfirmed;
            _shopUi.BuyModeRequested += ShowBuyMode;
            _shopUi.SellModeRequested += ShowSellMode;
            _shopUi.CurrencyIncreaseRequested += IncreaseCurrency;
            _shopUi.CurrencyDecreaseRequested += DecreaseCurrency;
        }

        public void Configure(PlayerDataState playerData, ItemDatabase itemDatabase)
        {
            _playerData = playerData;
            _itemDatabase = itemDatabase;
        }

        public bool Open()
        {
            _shop = ShopDatabase.LoadDefaultShop();
            if (_shop == null || (_shop.Items.Count == 0 && _playerData.Inventory.Count == 0))
            {
                GD.PushWarning("The default shop has no goods and the player's Inventory is empty.");
                return false;
            }

            _mode = ShopMode.Buying;
            _selectedIndex = 0;
            _selectedQuantity = 1;
            _tabWasPressed = false;
            ShowBuyMode(string.Empty);
            return true;
        }

        public void Close()
        {
            _shop = null;
            _ownedItems.Clear();
            _tabWasPressed = false;
            _shopUi.HideShop();
        }

        public void HandleInput()
        {
            if (!IsOpen || _shop == null)
            {
                return;
            }

            bool tabPressed = Input.IsKeyPressed(Key.Tab);
            if (tabPressed && !_tabWasPressed)
            {
                if (_mode == ShopMode.Buying)
                {
                    ShowSellMode(string.Empty);
                }
                else
                {
                    ShowBuyMode(string.Empty);
                }

                _tabWasPressed = true;
                return;
            }

            _tabWasPressed = tabPressed;

            if (Input.IsActionJustPressed("interaction_menu_up"))
            {
                MoveSelection(-1);
                return;
            }

            if (Input.IsActionJustPressed("interaction_menu_down"))
            {
                MoveSelection(1);
                return;
            }

            if (Input.IsActionJustPressed("move_left"))
            {
                ChangeQuantity(-1);
                return;
            }

            if (Input.IsActionJustPressed("move_right"))
            {
                ChangeQuantity(1);
                return;
            }

            if (Input.IsActionJustPressed("interaction_confirm"))
            {
                ConfirmSelection();
            }
        }

        private void ShowBuyMode()
        {
            ShowBuyMode(string.Empty);
        }

        private void ShowBuyMode(string feedback)
        {
            if (_shop == null)
            {
                return;
            }

            _mode = ShopMode.Buying;
            _selectedIndex = Math.Clamp(_selectedIndex, 0, _shop.Items.Count);
            _selectedQuantity = 1;

            List<string> itemNames = new();
            foreach (ShopItemDefinition shopItem in _shop.Items)
            {
                itemNames.Add(
                    _itemDatabase.TryGetItem(shopItem.ItemId, out Item item)
                        ? item.Name
                        : $"Không rõ: {shopItem.ItemId}"
                );
            }

            itemNames.Add("Rời cửa hàng");
            _shopUi.ShowShop(_shop.Name, "Hàng hóa", itemNames, _selectedIndex, true);
            UpdateBuyDetails(feedback);
        }

        private void ShowSellMode()
        {
            ShowSellMode(string.Empty);
        }

        private void ShowSellMode(string feedback, string preferredItemId = "")
        {
            if (_shop == null)
            {
                return;
            }

            _mode = ShopMode.Selling;
            RebuildOwnedItems();

            if (!string.IsNullOrWhiteSpace(preferredItemId))
            {
                int preferredIndex = _ownedItems.FindIndex(
                    option => string.Equals(
                        option.Item.Id,
                        preferredItemId,
                        StringComparison.OrdinalIgnoreCase
                    )
                );
                _selectedIndex = preferredIndex >= 0
                    ? preferredIndex
                    : Math.Min(_selectedIndex, _ownedItems.Count);
            }
            else
            {
                _selectedIndex = Math.Clamp(_selectedIndex, 0, _ownedItems.Count);
            }

            _selectedQuantity = 1;
            List<string> itemNames = new();
            foreach (OwnedItemOption option in _ownedItems)
            {
                string unavailable = option.BuybackPrice > 0 ? string.Empty : " (không thu mua)";
                itemNames.Add($"{option.Item.Name} ×{option.Quantity}{unavailable}");
            }

            itemNames.Add("Rời cửa hàng");
            _shopUi.ShowShop(_shop.Name, "Vật phẩm đang sở hữu", itemNames, _selectedIndex, false);
            UpdateSellDetails(feedback);
        }

        private void RebuildOwnedItems()
        {
            _ownedItems.Clear();
            Dictionary<string, int> optionIndexes = new(StringComparer.OrdinalIgnoreCase);

            foreach (PlayerItemStack stack in _playerData.Inventory)
            {
                if (optionIndexes.TryGetValue(stack.ItemId, out int existingIndex))
                {
                    _ownedItems[existingIndex].Quantity += stack.Quantity;
                    continue;
                }

                int buybackPrice = _shop != null &&
                    _shop.TryGetBuybackPrice(stack.ItemId, out int configuredPrice)
                        ? configuredPrice
                        : 0;

                optionIndexes.Add(stack.ItemId, _ownedItems.Count);
                _ownedItems.Add(new OwnedItemOption(stack.Item, stack.Quantity, buybackPrice));
            }
        }

        private void MoveSelection(int direction)
        {
            int itemCount = GetCurrentItemCount();
            int optionCount = itemCount + 1;
            _selectedIndex = (_selectedIndex + direction + optionCount) % optionCount;
            _selectedQuantity = 1;
            _shopUi.SetSelectedItem(_selectedIndex);
            UpdateDetails(string.Empty);
        }

        private void SelectItem(int index)
        {
            int itemCount = GetCurrentItemCount();
            if (_shop == null || index < 0 || index > itemCount)
            {
                return;
            }

            _selectedIndex = index;
            _selectedQuantity = 1;
            _shopUi.SetSelectedItem(index);
            UpdateDetails(string.Empty);
        }

        private void OnItemConfirmed(int index)
        {
            SelectItem(index);
            ConfirmSelection();
        }

        private void ChangeQuantity(int direction)
        {
            int itemCount = GetCurrentItemCount();
            if (_selectedIndex >= itemCount)
            {
                return;
            }

            int maximumQuantity;
            if (_mode == ShopMode.Buying)
            {
                maximumQuantity = _shop!.Items[_selectedIndex].Stock;
            }
            else
            {
                OwnedItemOption option = _ownedItems[_selectedIndex];
                if (option.BuybackPrice <= 0)
                {
                    return;
                }

                maximumQuantity = option.Quantity;
            }

            int nextQuantity = _selectedQuantity + direction;
            if (nextQuantity < 1 || nextQuantity > maximumQuantity)
            {
                return;
            }

            _selectedQuantity = nextQuantity;
            UpdateDetails(string.Empty);
        }

        private void ConfirmSelection()
        {
            if (_shop == null)
            {
                return;
            }

            if (_selectedIndex == GetCurrentItemCount())
            {
                LeaveRequested?.Invoke();
                return;
            }

            if (_mode == ShopMode.Buying)
            {
                ConfirmPurchase();
            }
            else
            {
                ConfirmSale();
            }
        }

        private void ConfirmPurchase()
        {
            if (_shop == null)
            {
                return;
            }

            ShopItemDefinition shopItem = _shop.Items[_selectedIndex];
            if (!_itemDatabase.TryGetItem(shopItem.ItemId, out Item item))
            {
                UpdateBuyDetails("Không tìm thấy dữ liệu vật phẩm.");
                return;
            }

            if (shopItem.Stock < _selectedQuantity)
            {
                UpdateBuyDetails("Số lượng hàng trong cửa hàng không đủ.");
                return;
            }

            if (!_playerData.TryPurchaseItem(
                item,
                shopItem.Price,
                _selectedQuantity,
                out string failureReason
            ))
            {
                UpdateBuyDetails(failureReason);
                return;
            }

            shopItem.Stock -= _selectedQuantity;
            UpdateBuyDetails($"Đã mua {item.Name} × {_selectedQuantity}.");
        }

        private void ConfirmSale()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _ownedItems.Count)
            {
                return;
            }

            OwnedItemOption option = _ownedItems[_selectedIndex];
            if (option.BuybackPrice <= 0)
            {
                UpdateSellDetails("Cửa hàng không thu mua vật phẩm này.");
                return;
            }

            int quantity = _selectedQuantity;
            long totalRevenue = (long)option.BuybackPrice * quantity;
            string soldItemId = option.Item.Id;
            string soldItemName = option.Item.Name;

            if (!_playerData.TrySellItem(
                soldItemId,
                option.BuybackPrice,
                quantity,
                out string failureReason
            ))
            {
                UpdateSellDetails(failureReason);
                return;
            }

            ShowSellMode(
                $"Đã bán {soldItemName} × {quantity}, nhận {totalRevenue:N0} vàng.",
                soldItemId
            );
        }

        private void IncreaseCurrency()
        {
            if (_playerData.TryAddCurrency(1000))
            {
                UpdateDetails("Đã cộng 1.000 vàng để kiểm thử.");
                return;
            }

            UpdateDetails("Không thể cộng thêm vàng (vượt giới hạn).");
        }

        private void DecreaseCurrency()
        {
            if (_playerData.TrySpendCurrency(1000))
            {
                UpdateDetails("Đã trừ 1.000 vàng để kiểm thử.");
                return;
            }

            UpdateDetails("Không đủ vàng để trừ 1.000.");
        }

        private int GetCurrentItemCount()
        {
            return _mode == ShopMode.Buying
                ? _shop?.Items.Count ?? 0
                : _ownedItems.Count;
        }

        private void UpdateDetails(string feedback)
        {
            if (_mode == ShopMode.Buying)
            {
                UpdateBuyDetails(feedback);
            }
            else
            {
                UpdateSellDetails(feedback);
            }
        }

        private void UpdateBuyDetails(string feedback)
        {
            if (_shop == null)
            {
                return;
            }

            if (_selectedIndex == _shop.Items.Count)
            {
                _shopUi.ShowDetails(
                    "Rời cửa hàng",
                    "Quay lại danh sách lựa chọn ban đầu.",
                    string.Empty,
                    string.Empty,
                    $"Tiền hiện có: {_playerData.Currency:N0} vàng",
                    string.Empty,
                    feedback
                );
                return;
            }

            ShopItemDefinition shopItem = _shop.Items[_selectedIndex];
            if (!_itemDatabase.TryGetItem(shopItem.ItemId, out Item item))
            {
                _shopUi.ShowDetails(
                    "Không rõ vật phẩm",
                    shopItem.ItemId,
                    $"Giá mua: {shopItem.Price:N0}",
                    $"Tồn kho: {shopItem.Stock}",
                    $"Tiền hiện có: {_playerData.Currency:N0} vàng",
                    $"Số lượng mua: {_selectedQuantity}",
                    feedback
                );
                return;
            }

            _shopUi.ShowDetails(
                item.Name,
                item.Description,
                $"Giá mua: {shopItem.Price:N0} vàng / món",
                $"Còn lại: {shopItem.Stock}",
                $"Tiền hiện có: {_playerData.Currency:N0} vàng",
                $"Số lượng mua: {_selectedQuantity}",
                feedback
            );
        }

        private void UpdateSellDetails(string feedback)
        {
            if (_shop == null)
            {
                return;
            }

            if (_ownedItems.Count == 0)
            {
                _shopUi.ShowDetails(
                    "Inventory trống",
                    "Bạn chưa có vật phẩm nào để bán.",
                    string.Empty,
                    string.Empty,
                    $"Tiền hiện có: {_playerData.Currency:N0} vàng",
                    string.Empty,
                    feedback
                );
                return;
            }

            if (_selectedIndex == _ownedItems.Count)
            {
                _shopUi.ShowDetails(
                    "Rời cửa hàng",
                    "Quay lại danh sách lựa chọn ban đầu.",
                    string.Empty,
                    $"Đang sở hữu: {_ownedItems.Count} loại vật phẩm",
                    $"Tiền hiện có: {_playerData.Currency:N0} vàng",
                    string.Empty,
                    feedback
                );
                return;
            }

            OwnedItemOption option = _ownedItems[_selectedIndex];
            string price = option.BuybackPrice > 0
                ? $"Giá thu mua: {option.BuybackPrice:N0} vàng / món"
                : "Cửa hàng không thu mua vật phẩm này.";

            _shopUi.ShowDetails(
                option.Item.Name,
                option.Item.Description,
                price,
                $"Sở hữu: {option.Quantity}",
                $"Tiền hiện có: {_playerData.Currency:N0} vàng",
                option.BuybackPrice > 0 ? $"Số lượng bán: {_selectedQuantity}" : string.Empty,
                feedback
            );
        }
    }
}
