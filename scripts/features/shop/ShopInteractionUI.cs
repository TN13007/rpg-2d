using System;
using System.Collections.Generic;
using Godot;

namespace RPG2D.Features.Shop
{
    public partial class ShopInteractionUI : Node
    {
        public event Action<int>? ItemHovered;
        public event Action<int>? ItemConfirmed;
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

        public void ShowShop(string title, IReadOnlyList<string> itemNames, int selectedIndex)
        {
            _shopTitleLabel.Text = title;
            SetItems(itemNames);
            _shopPanel.Visible = true;
            SetSelectedItem(selectedIndex);
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

        public void ShowDetails(
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

            Label itemHeading = new()
            {
                Text = "Hàng hóa",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            itemContent.AddChild(itemHeading);

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

            Label controlsLabel = new()
            {
                Text = "W / S — Chọn hàng     A — Giảm số lượng     D — Tăng số lượng     Space — Mua / Leave     Esc — Đóng",
                HorizontalAlignment = HorizontalAlignment.Right,
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            content.AddChild(controlsLabel);
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
