using System;
using System.Collections.Generic;
using Godot;
using RPG2D.Features.PlayerData;
using PlayerDataState = RPG2D.Features.PlayerData.PlayerData;

namespace RPG2D.Features.Shop
{
    public partial class ShopInteractionController : Node
    {
        public event Action? LeaveRequested;

        private PlayerDataState _playerData = null!;
        private ItemDatabase _itemDatabase = null!;
        private ShopInteractionUI _shopUi = null!;
        private ShopDefinition? _shop;
        private int _selectedIndex;
        private int _selectedQuantity = 1;

        public bool IsOpen => _shop != null && _shopUi.IsShopVisible;

        public override void _Ready()
        {
            _shopUi = new ShopInteractionUI();
            AddChild(_shopUi);
            _shopUi.ItemHovered += SelectItem;
            _shopUi.ItemConfirmed += OnItemConfirmed;
        }

        public void Configure(PlayerDataState playerData, ItemDatabase itemDatabase)
        {
            _playerData = playerData;
            _itemDatabase = itemDatabase;
        }

        public bool Open()
        {
            _shop = ShopDatabase.LoadDefaultShop();
            if (_shop == null || _shop.Items.Count == 0)
            {
                GD.PushWarning("The default shop has no valid items.");
                return false;
            }

            _selectedIndex = 0;
            _selectedQuantity = 1;
            List<string> itemNames = new();

            foreach (ShopItemDefinition shopItem in _shop.Items)
            {
                itemNames.Add(
                    _itemDatabase.TryGetItem(shopItem.ItemId, out ItemDefinition item)
                        ? item.Name
                        : $"Không rõ: {shopItem.ItemId}"
                );
            }

            itemNames.Add("Leave");
            _shopUi.ShowShop(_shop.Name, itemNames, _selectedIndex);
            UpdateDetails(string.Empty);
            return true;
        }

        public void Close()
        {
            _shop = null;
            _shopUi.HideShop();
        }

        public void HandleInput()
        {
            if (!IsOpen || _shop == null)
            {
                return;
            }

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

        private void MoveSelection(int direction)
        {
            if (_shop == null)
            {
                return;
            }

            int optionCount = _shop.Items.Count + 1;
            _selectedIndex = (_selectedIndex + direction + optionCount) % optionCount;
            _selectedQuantity = 1;
            _shopUi.SetSelectedItem(_selectedIndex);
            UpdateDetails(string.Empty);
        }

        private void SelectItem(int index)
        {
            if (_shop == null || index < 0 || index > _shop.Items.Count)
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
            if (_shop == null || _selectedIndex >= _shop.Items.Count)
            {
                return;
            }

            ShopItemDefinition item = _shop.Items[_selectedIndex];
            int nextQuantity = _selectedQuantity + direction;
            if (nextQuantity < 1 || nextQuantity > item.Stock)
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

            if (_selectedIndex == _shop.Items.Count)
            {
                LeaveRequested?.Invoke();
                return;
            }

            ShopItemDefinition shopItem = _shop.Items[_selectedIndex];
            if (!_itemDatabase.TryGetItem(shopItem.ItemId, out ItemDefinition item))
            {
                UpdateDetails("Không tìm thấy dữ liệu vật phẩm.");
                return;
            }

            if (shopItem.Stock < _selectedQuantity)
            {
                UpdateDetails("Số lượng hàng trong cửa hàng không đủ.");
                return;
            }

            if (!_playerData.TryPurchaseItem(
                shopItem.ItemId,
                shopItem.Price,
                _selectedQuantity,
                out string failureReason
            ))
            {
                UpdateDetails(failureReason);
                return;
            }

            shopItem.Stock -= _selectedQuantity;
            UpdateDetails($"Đã mua {item.Name} × {_selectedQuantity}.");
        }

        private void UpdateDetails(string feedback)
        {
            if (_shop == null)
            {
                return;
            }

            if (_selectedIndex == _shop.Items.Count)
            {
                _shopUi.ShowDetails(
                    "Leave",
                    "Rời cửa hàng và quay lại danh sách lựa chọn ban đầu.",
                    string.Empty,
                    string.Empty,
                    $"Tiền hiện có: {_playerData.Currency}",
                    string.Empty,
                    feedback
                );
                return;
            }

            ShopItemDefinition shopItem = _shop.Items[_selectedIndex];
            if (!_itemDatabase.TryGetItem(shopItem.ItemId, out ItemDefinition item))
            {
                _shopUi.ShowDetails(
                    "Không rõ vật phẩm",
                    shopItem.ItemId,
                    $"Giá: {shopItem.Price}",
                    $"Còn lại: {shopItem.Stock}",
                    $"Tiền hiện có: {_playerData.Currency}",
                    $"Số lượng: {_selectedQuantity}",
                    feedback
                );
                return;
            }

            _shopUi.ShowDetails(
                item.Name,
                item.Description,
                $"Giá: {shopItem.Price} vàng / món",
                $"Còn lại: {shopItem.Stock}",
                $"Tiền hiện có: {_playerData.Currency} vàng",
                $"Số lượng mua: {_selectedQuantity}",
                feedback
            );
        }
    }
}
