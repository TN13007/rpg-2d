using System;
using System.Collections.Generic;
using Godot;

namespace RPG2D.Features.Shop
{
    public partial class ShopInteractionUI : Node
    {
        public event Action<int>? ItemHovered;
        public event Action<int>? ItemConfirmed;
        public event Action? BuyModeRequested;
        public event Action? SellModeRequested;
        public event Action? CurrencyIncreaseRequested;
        public event Action? CurrencyDecreaseRequested;
        public bool IsShopVisible => _shopPanel != null && _shopPanel.Visible;

        private sealed class ShopRow
        {
            public PanelContainer Row { get; }
            public Label CursorLabel { get; }
            public Label ItemLabel { get; }

            public ShopRow(PanelContainer row, Label cursorLabel, Label itemLabel)
            {
                Row = row;
                CursorLabel = cursorLabel;
                ItemLabel = itemLabel;
            }
        }

        private CanvasLayer _canvasLayer = null!;
        private PanelContainer _shopPanel = null!;
        private Label _shopTitleLabel = null!;
        private Label _itemsHeadingLabel = null!;
        private Label _controlsLabel = null!;
        private Button _buyModeButton = null!;
        private Button _sellModeButton = null!;
        private VBoxContainer _itemsContainer = null!;
        private ScrollContainer _itemsScrollContainer = null!;
        private Label _detailNameLabel = null!;
        private Label _descriptionLabel = null!;
        private Label _priceLabel = null!;
        private Label _stockLabel = null!;
        private Label _currencyLabel = null!;
        private Label _quantityLabel = null!;
        private Label _feedbackLabel = null!;
        private readonly List<ShopRow> _rows = new();
        private int _selectedIndex;

        public override void _Ready()
        {
            BuildInterface();
        }

        public void ShowShop(ShopData shopData)
        {
            IReadOnlyList<ShopItemOption> items = shopData.Items;
            _shopTitleLabel.Text = shopData.ShopName;
            _itemsHeadingLabel.Text = shopData.Mode == ShopMode.Buying
                ? "Hàng hóa"
                : "Vật phẩm đang sở hữu";
            _buyModeButton.Disabled = shopData.Mode == ShopMode.Buying;
            _sellModeButton.Disabled = shopData.Mode != ShopMode.Buying;
            _controlsLabel.Text =
                "W / S — Chọn vật phẩm     A / D — Số lượng     Tab — Chuyển Mua / Bán     Space — Xác nhận / Rời cửa hàng     Esc — Đóng";
            SetItems(BuildItemNames(shopData.Mode, items));
            _shopPanel.Visible = true;
            SetSelectedItem(shopData.SelectedIndex);
            UpdateDetails(shopData, items);
        }

        public void RefreshDetails(ShopData shopData)
        {
            IReadOnlyList<ShopItemOption> items = shopData.Items;
            SetSelectedItem(shopData.SelectedIndex);
            UpdateDetails(shopData, items);
        }

        private static List<string> BuildItemNames(ShopMode mode, IReadOnlyList<ShopItemOption> items)
        {
            List<string> itemNames = new();

            foreach (ShopItemOption option in items)
            {
                if (mode == ShopMode.Buying)
                {
                    itemNames.Add(option.Item?.Name ?? $"Không rõ: {option.ItemId}");
                    continue;
                }

                string unavailable = option.IsAvailable ? string.Empty : " (không thu mua)";
                string itemName = option.Item?.Name ?? option.ItemId;
                itemNames.Add($"{itemName} ×{option.OwnedQuantity}{unavailable}");
            }

            itemNames.Add("Rời cửa hàng");
            return itemNames;
        }

        public void HideShop()
        {
            _shopPanel.Visible = false;
        }

        public void SetSelectedItem(int index)
        {
            if (index < 0 || index >= _rows.Count)
            {
                return;
            }

            _selectedIndex = index;
            UpdateSelectionVisuals();
            _itemsScrollContainer.EnsureControlVisible(_rows[index].Row);
        }

        private void UpdateDetails(ShopData shopData, IReadOnlyList<ShopItemOption> items)
        {
            string currency = $"Tiền hiện có: {shopData.Currency:N0} vàng";
            string feedback = FormatFeedback(shopData.Feedback);

            if (mode == ShopMode.Buying)
            {
                if (shopData.SelectedIndex == items.Count)
                {
                    SetDetails(
                        "Rời cửa hàng",
                        "Quay lại danh sách lựa chọn ban đầu.",
                        string.Empty,
                        string.Empty,
                        currency,
                        string.Empty,
                        feedback
                    );
                    return;
                }

                ShopItemOption? option = shopData.SelectedIndex >= 0 &&
                    shopData.SelectedIndex < items.Count
                        ? items[shopData.SelectedIndex]
                        : null;

                if (option?.Item == null)
                {
                    SetDetails(
                        "Không rõ vật phẩm",
                        option?.ItemId ?? string.Empty,
                        option == null ? string.Empty : $"Giá mua: {option.UnitPrice:N0}",
                        option == null ? string.Empty : $"Tồn kho: {option.Stock}",
                        currency,
                        option == null ? string.Empty : $"Số lượng mua: {shopData.SelectedQuantity}",
                        feedback
                    );
                    return;
                }

                SetDetails(
                    option.Item.Name,
                    option.Item.Description,
                    $"Giá mua: {option.UnitPrice:N0} vàng / món",
                    $"Còn lại: {option.Stock}",
                    currency,
                    $"Số lượng mua: {shopData.SelectedQuantity}",
                    feedback
                );
                return;
            }

            if ((shopData.Mode == ShopMode.Selling && items.Count == 0))
            {
                SetDetails(
                    "Inventory trống",
                    "Bạn chưa có vật phẩm nào để bán.",
                    string.Empty,
                    string.Empty,
                    currency,
                    string.Empty,
                    feedback
                );
                return;
            }

            if (shopData.SelectedIndex == items.Count)
            {
                SetDetails(
                    "Rời cửa hàng",
                    "Quay lại danh sách lựa chọn ban đầu.",
                    string.Empty,
                    $"Đang sở hữu: {items.Count} loại vật phẩm",
                    currency,
                    string.Empty,
                    feedback
                );
                return;
            }

            ShopItemOption selectedOption = items[shopData.SelectedIndex];
            string price = selectedOption.IsAvailable
                ? $"Giá thu mua: {selectedOption.UnitPrice:N0} vàng / món"
                : "Cửa hàng không thu mua vật phẩm này.";

            SetDetails(
                selectedOption.Item?.Name ?? selectedOption.ItemId,
                selectedOption.Item?.Description ?? string.Empty,
                price,
                $"Sở hữu: {selectedOption.OwnedQuantity}",
                currency,
                selectedOption.IsAvailable
                    ? $"Số lượng bán: {shopData.SelectedQuantity}"
                    : string.Empty,
                feedback
            );
        }

        private void SetDetails(
            string name,
            string description,
            string price,
            string stock,
            string currency,
            string quantity,
            string feedback
        )
        {
            _detailNameLabel.Text = name;
            _descriptionLabel.Text = description;
            _priceLabel.Text = price;
            _stockLabel.Text = stock;
            _currencyLabel.Text = currency;
            _quantityLabel.Text = quantity;
            _feedbackLabel.Text = feedback;
        }

        private static string FormatFeedback(ShopFeedback feedback)
        {
            return feedback.Kind switch
            {
                ShopFeedbackKind.Error => feedback.Message,
                ShopFeedbackKind.ItemNotAccepted => "Cửa hàng không thu mua vật phẩm này.",
                ShopFeedbackKind.PurchaseSucceeded =>
                    $"Đã mua {feedback.ItemName} × {feedback.Quantity}.",
                ShopFeedbackKind.SaleSucceeded =>
                    $"Đã bán {feedback.ItemName} × {feedback.Quantity}, nhận {feedback.Amount:N0} vàng.",
                ShopFeedbackKind.CurrencyAdded =>
                    $"Đã cộng {feedback.Amount:N0} vàng để kiểm thử.",
                ShopFeedbackKind.CurrencyAddFailed =>
                    "Không thể cộng thêm vàng (vượt giới hạn).",
                ShopFeedbackKind.CurrencySpent =>
                    $"Đã trừ {feedback.Amount:N0} vàng để kiểm thử.",
                ShopFeedbackKind.CurrencySpendFailed =>
                    $"Không đủ vàng để trừ {feedback.Amount:N0}.",
                _ => string.Empty
            };
        }

        private void BuildInterface()
        {
            _canvasLayer = new CanvasLayer();
            AddChild(_canvasLayer);

            _shopPanel = new PanelContainer
            {
                Visible = false,
                AnchorLeft = 0.04f,
                AnchorTop = 0.08f,
                AnchorRight = 0.96f,
                AnchorBottom = 0.92f,
                OffsetLeft = 8,
                OffsetTop = 8,
                OffsetRight = -8,
                OffsetBottom = -8
            };
            _canvasLayer.AddChild(_shopPanel);

            VBoxContainer content = new();
            content.AddThemeConstantOverride("separation", 10);
            _shopPanel.AddChild(content);

            _shopTitleLabel = new Label
            {
                Text = "Cửa hàng",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            content.AddChild(_shopTitleLabel);

            HBoxContainer modeTabs = new()
            {
                Alignment = BoxContainer.AlignmentMode.Center
            };
            modeTabs.AddThemeConstantOverride("separation", 8);
            content.AddChild(modeTabs);

            _buyModeButton = new Button { Text = "Mua hàng" };
            _buyModeButton.Pressed += () => BuyModeRequested?.Invoke();
            modeTabs.AddChild(_buyModeButton);

            _sellModeButton = new Button { Text = "Bán đồ" };
            _sellModeButton.Pressed += () => SellModeRequested?.Invoke();
            modeTabs.AddChild(_sellModeButton);

            HBoxContainer columns = new()
            {
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            columns.AddThemeConstantOverride("separation", 12);
            content.AddChild(columns);

            PanelContainer itemPanel = new()
            {
                CustomMinimumSize = new Vector2(240, 0),
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            columns.AddChild(itemPanel);

            VBoxContainer itemContent = new();
            itemContent.AddThemeConstantOverride("separation", 6);
            itemPanel.AddChild(itemContent);

            _itemsHeadingLabel = new Label
            {
                Text = "Hàng hóa",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            itemContent.AddChild(_itemsHeadingLabel);

            _itemsScrollContainer = new ScrollContainer
            {
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
                VerticalScrollMode = ScrollContainer.ScrollMode.Auto
            };
            itemContent.AddChild(_itemsScrollContainer);

            _itemsContainer = new VBoxContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            _itemsContainer.AddThemeConstantOverride("separation", 4);
            _itemsScrollContainer.AddChild(_itemsContainer);

            PanelContainer detailPanel = new()
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            columns.AddChild(detailPanel);

            VBoxContainer detailContent = new();
            detailContent.AddThemeConstantOverride("separation", 12);
            detailPanel.AddChild(detailContent);

            _detailNameLabel = new Label
            {
                Text = string.Empty,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            detailContent.AddChild(_detailNameLabel);

            _descriptionLabel = new Label
            {
                Text = string.Empty,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            detailContent.AddChild(_descriptionLabel);

            _priceLabel = CreateDetailLabel();
            detailContent.AddChild(_priceLabel);
            _stockLabel = CreateDetailLabel();
            detailContent.AddChild(_stockLabel);
            _currencyLabel = CreateDetailLabel();
            detailContent.AddChild(_currencyLabel);
            _quantityLabel = CreateDetailLabel();
            detailContent.AddChild(_quantityLabel);

            _feedbackLabel = new Label
            {
                Text = string.Empty,
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            detailContent.AddChild(_feedbackLabel);

            HBoxContainer debugCurrencyControls = new()
            {
                Alignment = BoxContainer.AlignmentMode.End
            };
            debugCurrencyControls.AddThemeConstantOverride("separation", 8);
            content.AddChild(debugCurrencyControls);

            Button increaseCurrencyButton = new()
            {
                Text = "+1.000 vàng",
                TooltipText = "Cộng 1.000 vàng để kiểm thử Shop."
            };
            increaseCurrencyButton.Pressed += () => CurrencyIncreaseRequested?.Invoke();
            debugCurrencyControls.AddChild(increaseCurrencyButton);

            Button decreaseCurrencyButton = new()
            {
                Text = "-1.000 vàng",
                TooltipText = "Trừ 1.000 vàng để kiểm thử Shop."
            };
            decreaseCurrencyButton.Pressed += () => CurrencyDecreaseRequested?.Invoke();
            debugCurrencyControls.AddChild(decreaseCurrencyButton);

            _controlsLabel = new Label
            {
                Text = string.Empty,
                HorizontalAlignment = HorizontalAlignment.Right,
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            content.AddChild(_controlsLabel);
        }

        private static Label CreateDetailLabel()
        {
            return new Label
            {
                Text = string.Empty,
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
        }

        private void SetItems(IReadOnlyList<string> itemNames)
        {
            foreach (Node child in _itemsContainer.GetChildren())
            {
                child.QueueFree();
            }

            _rows.Clear();
            for (int index = 0; index < itemNames.Count; index++)
            {
                CreateItemRow(index, itemNames[index]);
            }
        }

        private void CreateItemRow(int index, string text)
        {
            PanelContainer row = new()
            {
                CustomMinimumSize = new Vector2(0, 38),
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                MouseFilter = Control.MouseFilterEnum.Stop
            };
            _itemsContainer.AddChild(row);
            row.MouseEntered += () => ItemHovered?.Invoke(index);
            row.GuiInput += input =>
            {
                if (input is InputEventMouseButton mouseButton &&
                    mouseButton.Pressed &&
                    mouseButton.ButtonIndex == MouseButton.Left)
                {
                    ItemConfirmed?.Invoke(index);
                }
            };

            HBoxContainer rowContent = new()
            {
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            rowContent.AddThemeConstantOverride("separation", 8);
            row.AddChild(rowContent);

            Label cursorLabel = new()
            {
                Text = string.Empty,
                CustomMinimumSize = new Vector2(24, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            rowContent.AddChild(cursorLabel);

            Label itemLabel = new()
            {
                Text = text,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                VerticalAlignment = VerticalAlignment.Center
            };
            rowContent.AddChild(itemLabel);
            _rows.Add(new ShopRow(row, cursorLabel, itemLabel));
        }

        private void UpdateSelectionVisuals()
        {
            for (int index = 0; index < _rows.Count; index++)
            {
                bool selected = index == _selectedIndex;
                ShopRow row = _rows[index];
                StyleBoxFlat style = new()
                {
                    BgColor = selected
                        ? new Color(0.08f, 0.25f, 0.30f, 1.0f)
                        : new Color(0.06f, 0.07f, 0.09f, 0.65f),
                    BorderColor = selected
                        ? new Color(0.25f, 0.90f, 1.0f, 1.0f)
                        : new Color(0.20f, 0.22f, 0.26f, 0.55f)
                };
                style.SetBorderWidthAll(selected ? 2 : 1);
                style.SetCornerRadiusAll(4);
                row.Row.AddThemeStyleboxOverride("panel", style);
                row.CursorLabel.Text = selected ? "▶" : string.Empty;
                row.CursorLabel.AddThemeColorOverride(
                    "font_color",
                    selected ? new Color(0.35f, 0.95f, 1.0f) : Colors.Transparent
                );
                row.ItemLabel.AddThemeColorOverride(
                    "font_color",
                    selected ? Colors.White : new Color(0.72f, 0.74f, 0.78f)
                );
            }
        }
    }
}
