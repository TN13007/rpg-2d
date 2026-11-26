using System;
using Godot;
using RPG2D.Features.PlayerData;
using PlayerDataState = RPG2D.Features.PlayerData.PlayerData;

namespace RPG2D.Features.Shop
{
    public partial class ShopController : Node
    {
        public event Action? LeaveRequested;

        private PlayerDataState? _playerData;
        private ItemDatabase? _itemDatabase;
        private ShopInteractionUI _shopUi = null!;
        private ShopData? _shopData;
        private bool _tabWasPressed;

        public bool IsOpen => _shopData != null && _shopUi.IsShopVisible;

        public override void _Ready()
        {
            _shopUi = new ShopInteractionUI();
            AddChild(_shopUi);
            _shopUi.ItemHovered += SelectItem;
            _shopUi.ItemConfirmed += ConfirmItem;
            _shopUi.BuyModeRequested += () => SetMode(ShopMode.Buying);
            _shopUi.SellModeRequested += () => SetMode(ShopMode.Selling);
            _shopUi.CurrencyIncreaseRequested += () => AdjustCurrency(true);
            _shopUi.CurrencyDecreaseRequested += () => AdjustCurrency(false);
        }

        public void Configure(PlayerDataState playerData, ItemDatabase itemDatabase)
        {
            _playerData = playerData;
            _itemDatabase = itemDatabase;
        }

        public bool Open()
        {
            if (_playerData == null || _itemDatabase == null)
            {
                GD.PushError("ShopController must be configured before opening.");
                return false;
            }

            ShopDefinition? shop = ShopDatabase.LoadDefaultShop();
            if (shop == null || (shop.Items.Count == 0 && _playerData.Inventory.Count == 0))
            {
                GD.PushWarning("The default shop has no goods and the player's Inventory is empty.");
                return false;
            }

            _shopData = new ShopData(shop, _playerData, _itemDatabase);
            _tabWasPressed = false;
            RefreshUI();
            return true;
        }

        public void Close()
        {
            _shopData = null;
            _tabWasPressed = false;
            _shopUi.HideShop();
        }

        public void HandleInput()
        {
            if (!IsOpen || _shopData == null)
            {
                return;
            }

            bool tabPressed = Input.IsKeyPressed(Key.Tab);
            if (tabPressed && !_tabWasPressed)
            {
                ShopMode nextMode = _shopData.Mode == ShopMode.Buying
                    ? ShopMode.Selling
                    : ShopMode.Buying;
                SetMode(nextMode);
                _tabWasPressed = true;
                return;
            }

            _tabWasPressed = tabPressed;

            if (Input.IsActionJustPressed("interaction_menu_up"))
            {
                _shopData.MoveSelection(-1);
                RefreshDetails();
                return;
            }

            if (Input.IsActionJustPressed("interaction_menu_down"))
            {
                _shopData.MoveSelection(1);
                RefreshDetails();
                return;
            }

            if (Input.IsActionJustPressed("move_left"))
            {
                _shopData.ChangeQuantity(-1);
                RefreshDetails();
                return;
            }

            if (Input.IsActionJustPressed("move_right"))
            {
                _shopData.ChangeQuantity(1);
                RefreshDetails();
                return;
            }

            if (Input.IsActionJustPressed("interaction_confirm"))
            {
                ConfirmSelection();
            }
        }

        private void SetMode(ShopMode mode)
        {
            if (_shopData == null)
            {
                return;
            }

            _shopData.SetMode(mode);
            RefreshUI();
        }

        private void SelectItem(int index)
        {
            if (_shopData == null)
            {
                return;
            }

            _shopData.SelectItem(index);
            RefreshDetails();
        }

        private void ConfirmItem(int index)
        {
            if (_shopData == null)
            {
                return;
            }

            _shopData.SelectItem(index);
            ConfirmSelection();
        }

        private void ConfirmSelection()
        {
            if (_shopData == null)
            {
                return;
            }

            ShopDataOutcome outcome = _shopData.ConfirmSelection();
            if (outcome == ShopDataOutcome.LeaveRequested)
            {
                LeaveRequested?.Invoke();
                return;
            }

            RefreshUI();
        }

        private void AdjustCurrency(bool increase)
        {
            if (_shopData == null)
            {
                return;
            }

            _shopData.AdjustCurrency(increase);
            RefreshDetails();
        }

        private void RefreshUI()
        {
            if (_shopData == null)
            {
                return;
            }

            _shopUi.ShowShop(_shopData.CreateView());
        }

        private void RefreshDetails()
        {
            if (_shopData == null)
            {
                return;
            }

            _shopUi.RefreshDetails(_shopData.CreateView());
        }
    }
}
