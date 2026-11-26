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
            UpdateModeSelection(shopData.Mode);
            _controlsLabel.Text =
                "W / S — Chọn vật phẩm     A / D — Số lượng     Space — Xác nhận / Rời cửa hàng     Esc — Đóng";
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

            if (shopData.Mode == ShopMode.Buying)
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
                    option.Stock > 0
                        ? $"Số lượng mua: {shopData.SelectedQuantity}"
                        : "Hết hàng",
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

            _shopPanel.AddThemeStyleboxOverride(
                "panel",
                CreatePanelStyle(
                    new Color(0.055f, 0.065f, 0.085f, 0.98f),
                    new Color(0.62f, 0.43f, 0.19f),
                    2,
                    12,
                    18
                )
            );

            MarginContainer outerMargin = new();
            outerMargin.AddThemeConstantOverride("margin_left", 4);
            outerMargin.AddThemeConstantOverride("margin_top", 4);
            outerMargin.AddThemeConstantOverride("margin_right", 4);
            outerMargin.AddThemeConstantOverride("margin_bottom", 4);
            _shopPanel.AddChild(outerMargin);

            VBoxContainer content = new();
            content.AddThemeConstantOverride("separation", 14);
            outerMargin.AddChild(content);

            HBoxContainer header = new()
            {
                Alignment = BoxContainer.AlignmentMode.Center
            };
            content.AddChild(header);

            _shopTitleLabel = new Label
            {
                Text = "CỬA HÀNG",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                VerticalAlignment = VerticalAlignment.Center
            };
            _shopTitleLabel.AddThemeFontSizeOverride("font_size", 26);
            _shopTitleLabel.AddThemeColorOverride("font_color", new Color(0.96f, 0.88f, 0.70f));
            header.AddChild(_shopTitleLabel);

            _currencyLabel = new Label
            {
                Text = "0 vàng",
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            _currencyLabel.AddThemeFontSizeOverride("font_size", 17);
            _currencyLabel.AddThemeColorOverride("font_color", new Color(1.0f, 0.78f, 0.34f));
            header.AddChild(_currencyLabel);

            HBoxContainer modeChoices = new()
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            modeChoices.AddThemeConstantOverride("separation", 12);
            content.AddChild(modeChoices);

            _buyModeButton = CreateModeButton(
                "MUA HÀNG\nChọn vật phẩm từ cửa hàng"
            );
            _buyModeButton.Pressed += () => BuyModeRequested?.Invoke();
            modeChoices.AddChild(_buyModeButton);

            _sellModeButton = CreateModeButton(
                "BÁN VẬT PHẨM\nThanh lý đồ trong túi"
            );
            _sellModeButton.Pressed += () => SellModeRequested?.Invoke();
            modeChoices.AddChild(_sellModeButton);

            HBoxContainer columns = new()
            {
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            columns.AddThemeConstantOverride("separation", 14);
            content.AddChild(columns);

            PanelContainer itemPanel = new()
            {
                CustomMinimumSize = new Vector2(280, 0),
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            itemPanel.AddThemeStyleboxOverride(
                "panel",
                CreatePanelStyle(
                    new Color(0.085f, 0.095f, 0.12f, 0.98f),
                    new Color(0.25f, 0.27f, 0.31f),
                    1,
                    8,
                    12
                )
            );
            columns.AddChild(itemPanel);

            VBoxContainer itemContent = new();
            itemContent.AddThemeConstantOverride("separation", 10);
            itemPanel.AddChild(itemContent);

            _itemsHeadingLabel = new Label
            {
                Text = "HÀNG HÓA",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            _itemsHeadingLabel.AddThemeFontSizeOverride("font_size", 17);
            _itemsHeadingLabel.AddThemeColorOverride("font_color", new Color(0.91f, 0.78f, 0.54f));
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
            detailPanel.AddThemeStyleboxOverride(
                "panel",
                CreatePanelStyle(
                    new Color(0.085f, 0.095f, 0.12f, 0.98f),
                    new Color(0.25f, 0.27f, 0.31f),
                    1,
                    8,
                    18
                )
            );
            columns.AddChild(detailPanel);

            VBoxContainer detailContent = new();
            detailContent.AddThemeConstantOverride("separation", 14);
            detailPanel.AddChild(detailContent);

            _detailNameLabel = new Label
            {
                Text = string.Empty,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            _detailNameLabel.AddThemeFontSizeOverride("font_size", 23);
            _detailNameLabel.AddThemeColorOverride("font_color", new Color(0.96f, 0.88f, 0.70f));
            detailContent.AddChild(_detailNameLabel);

            _descriptionLabel = new Label
            {
                Text = string.Empty,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            _descriptionLabel.AddThemeColorOverride("font_color", new Color(0.76f, 0.77f, 0.79f));
            detailContent.AddChild(_descriptionLabel);

            HSeparator detailSeparator = new();
            detailContent.AddChild(detailSeparator);

            _priceLabel = CreateDetailLabel();
            _priceLabel.AddThemeColorOverride("font_color", new Color(1.0f, 0.78f, 0.34f));
            detailContent.AddChild(_priceLabel);
            _stockLabel = CreateDetailLabel();
            detailContent.AddChild(_stockLabel);
            _quantityLabel = CreateDetailLabel();
            detailContent.AddChild(_quantityLabel);

            _feedbackLabel = new Label
            {
                Text = string.Empty,
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            _feedbackLabel.AddThemeFontSizeOverride("font_size", 15);
            _feedbackLabel.AddThemeColorOverride("font_color", new Color(0.54f, 0.88f, 0.65f));
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
            _controlsLabel.AddThemeFontSizeOverride("font_size", 13);
            _controlsLabel.AddThemeColorOverride("font_color", new Color(0.65f, 0.67f, 0.71f));
            content.AddChild(_controlsLabel);
        }

        private static Button CreateModeButton(string text)
        {
            Button button = new()
            {
                Text = text,
                ToggleMode = true,
                CustomMinimumSize = new Vector2(0, 78),
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                Alignment = HorizontalAlignment.Left
            };
            button.AddThemeFontSizeOverride("font_size", 16);
            button.AddThemeColorOverride("font_color", new Color(0.83f, 0.84f, 0.86f));
            button.AddThemeColorOverride("font_hover_color", Colors.White);
            button.AddThemeColorOverride("font_pressed_color", new Color(1.0f, 0.91f, 0.70f));
            button.AddThemeStyleboxOverride(
                "normal",
                CreatePanelStyle(new Color(0.11f, 0.12f, 0.15f), new Color(0.32f, 0.33f, 0.36f), 1, 8, 14)
            );
            button.AddThemeStyleboxOverride(
                "hover",
                CreatePanelStyle(new Color(0.16f, 0.15f, 0.13f), new Color(0.67f, 0.49f, 0.24f), 2, 8, 14)
            );
            button.AddThemeStyleboxOverride(
                "pressed",
                CreatePanelStyle(new Color(0.25f, 0.19f, 0.11f), new Color(1.0f, 0.72f, 0.27f), 2, 8, 14)
            );
            button.AddThemeStyleboxOverride(
                "hover_pressed",
                CreatePanelStyle(new Color(0.29f, 0.22f, 0.12f), new Color(1.0f, 0.82f, 0.42f), 2, 8, 14)
            );
            return button;
        }

        private void UpdateModeSelection(ShopMode mode)
        {
            bool buying = mode == ShopMode.Buying;
            _buyModeButton.SetPressedNoSignal(buying);
            _sellModeButton.SetPressedNoSignal(!buying);
        }

        private static StyleBoxFlat CreatePanelStyle(
            Color background,
            Color border,
            int borderWidth,
            int cornerRadius,
            int contentMargin
        )
        {
            StyleBoxFlat style = new()
            {
                BgColor = background,
                BorderColor = border,
                ContentMarginLeft = contentMargin,
                ContentMarginTop = contentMargin,
                ContentMarginRight = contentMargin,
                ContentMarginBottom = contentMargin
            };
            style.SetBorderWidthAll(borderWidth);
            style.SetCornerRadiusAll(cornerRadius);
            return style;
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
                CustomMinimumSize = new Vector2(0, 46),
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
                StyleBoxFlat style = CreatePanelStyle(
                    selected
                        ? new Color(0.24f, 0.18f, 0.10f, 1.0f)
                        : new Color(0.075f, 0.085f, 0.105f, 0.85f),
                    selected
                        ? new Color(1.0f, 0.72f, 0.27f, 1.0f)
                        : new Color(0.20f, 0.22f, 0.26f, 0.55f),
                    selected ? 2 : 1,
                    6,
                    8
                );
                row.Row.AddThemeStyleboxOverride("panel", style);
                row.CursorLabel.Text = selected ? "◆" : string.Empty;
                row.CursorLabel.AddThemeColorOverride(
                    "font_color",
                    selected ? new Color(1.0f, 0.78f, 0.34f) : Colors.Transparent
                );
                row.ItemLabel.AddThemeColorOverride(
                    "font_color",
                    selected ? Colors.White : new Color(0.72f, 0.74f, 0.78f)
                );
            }
        }
    }
}
