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
        private ShopData? _session;
        private bool _tabWasPressed;

        public bool IsOpen => _session != null && _shopUi.IsShopVisible;

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

            _session = new ShopData(shop, _playerData, _itemDatabase);
            _tabWasPressed = false;
            RefreshUI();
            return true;
        }

        public void Close()
        {
            _session = null;
            _tabWasPressed = false;
            _shopUi.HideShop();
        }

        public void HandleInput()
        {
            if (!IsOpen || _session == null)
            {
                return;
            }

            bool tabPressed = Input.IsKeyPressed(Key.Tab);
            if (tabPressed && !_tabWasPressed)
            {
                ShopMode nextMode = _session.Mode == ShopMode.Buying
                    ? ShopMode.Selling
                    : ShopMode.Buying;
                SetMode(nextMode);
                _tabWasPressed = true;
                return;
            }

            _tabWasPressed = tabPressed;

            if (Input.IsActionJustPressed("interaction_menu_up"))
            {
                _session.MoveSelection(-1);
                RefreshDetails();
                return;
            }

            if (Input.IsActionJustPressed("interaction_menu_down"))
            {
                _session.MoveSelection(1);
                RefreshDetails();
                return;
            }

            if (Input.IsActionJustPressed("move_left"))
            {
                _session.ChangeQuantity(-1);
                RefreshDetails();
                return;
            }

            if (Input.IsActionJustPressed("move_right"))
            {
                _session.ChangeQuantity(1);
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
            if (_session == null)
            {
                return;
            }

            _session.SetMode(mode);
            RefreshUI();
        }

        private void SelectItem(int index)
        {
            if (_session == null)
            {
                return;
            }

            _session.SelectItem(index);
            RefreshDetails();
        }

        private void ConfirmItem(int index)
        {
            if (_session == null)
            {
                return;
            }

            _session.SelectItem(index);
            ConfirmSelection();
        }

        private void ConfirmSelection()
        {
            if (_session == null)
            {
                return;
            }

            ShopDataOutcome outcome = _session.ConfirmSelection();
            if (outcome == ShopDataOutcome.LeaveRequested)
            {
                LeaveRequested?.Invoke();
                return;
            }

            RefreshUI();
        }

        private void AdjustCurrency(bool increase)
        {
            if (_session == null)
            {
                return;
            }

            _session.AdjustCurrency(increase);
            RefreshDetails();
        }

        private void RefreshUI()
        {
            if (_session == null)
            {
                return;
            }

            _shopUi.ShowShop(_session.CreateView());
        }

        private void RefreshDetails()
        {
            if (_session == null)
            {
                return;
            }

            _shopUi.RefreshDetails(_session.CreateView());
        }
    }
}
