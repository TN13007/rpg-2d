using System;
using System.Collections.Generic;
using Godot;

namespace RPG2D.Features.Interaction
{
    public partial class TerminalInteractionUI : Node
    {
        public event Action<int>? ChoiceHovered;
        public event Action<int>? ChoiceConfirmed;

        private sealed class ChoiceRow
        {
            public PanelContainer Row { get; }
            public Label CursorLabel { get; }
            public Label OptionLabel { get; }

            public ChoiceRow(PanelContainer row, Label cursorLabel, Label optionLabel)
            {
                Row = row;
                CursorLabel = cursorLabel;
                OptionLabel = optionLabel;
            }
        }

        private CanvasLayer _canvasLayer = null!;
        private Label _promptLabel = null!;
        private PanelContainer _interactionPanel = null!;
        private Label _titleLabel = null!;
        private Label _messageLabel = null!;
        private Label _feedbackLabel = null!;
        private ScrollContainer _choicesScrollContainer = null!;
        private VBoxContainer _choicesContainer = null!;

        private readonly List<ChoiceRow> _choiceRows = new();
        private int _selectedChoiceIndex;

        public override void _Ready()
        {
            BuildInterface();
        }

        public void SetPromptVisible(bool visible)
        {
            _promptLabel.Visible = visible;
        }

        public void ShowInteraction(
            string title,
            string message,
            IReadOnlyList<string> choices,
            int selectedChoiceIndex)
        {
            _titleLabel.Text = title;
            _messageLabel.Text = message;
            _feedbackLabel.Text = string.Empty;

            SetChoices(choices);
            _interactionPanel.Visible = true;
            SetSelectedChoice(selectedChoiceIndex);
        }

        public void HideInteraction()
        {
            _interactionPanel.Visible = false;
        }

        public void SetFeedback(string message)
        {
            _feedbackLabel.Text = message;
        }

        public void SetSelectedChoice(int index)
        {
            if (index < 0 || index >= _choiceRows.Count)
            {
                return;
            }

            _selectedChoiceIndex = index;
            UpdateSelectionVisuals();
            _choicesScrollContainer.EnsureControlVisible(_choiceRows[index].Row);
        }

        private void BuildInterface()
        {
            _canvasLayer = new CanvasLayer();
            AddChild(_canvasLayer);

            _promptLabel = new Label
            {
                Text = "E — Interact",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Visible = false
            };
            _promptLabel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.CenterBottom);
            _promptLabel.Position = new Vector2(-70, -90);
            _promptLabel.Size = new Vector2(140, 32);
            _canvasLayer.AddChild(_promptLabel);

            _interactionPanel = new PanelContainer
            {
                Visible = false,
                AnchorLeft = 0.04f,
                AnchorTop = 0.48f,
                AnchorRight = 0.96f,
                AnchorBottom = 0.96f,
                OffsetLeft = 8,
                OffsetTop = 0,
                OffsetRight = -8,
                OffsetBottom = -8
            };
            _canvasLayer.AddChild(_interactionPanel);

            VBoxContainer content = new VBoxContainer();
            content.AddThemeConstantOverride("separation", 8);
            _interactionPanel.AddChild(content);

            _titleLabel = new Label
            {
                Text = "Terminal",
                HorizontalAlignment = HorizontalAlignment.Center,
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            content.AddChild(_titleLabel);

            _messageLabel = new Label
            {
                Text = string.Empty,
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            content.AddChild(_messageLabel);

            _choicesScrollContainer = new ScrollContainer
            {
                CustomMinimumSize = new Vector2(0, 112),
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
                VerticalScrollMode = ScrollContainer.ScrollMode.Auto
            };
            content.AddChild(_choicesScrollContainer);

            _choicesContainer = new VBoxContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            _choicesContainer.AddThemeConstantOverride("separation", 4);
            _choicesScrollContainer.AddChild(_choicesContainer);

            _feedbackLabel = new Label
            {
                Text = string.Empty,
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            content.AddChild(_feedbackLabel);

            Label closeHintLabel = new Label
            {
                Text = "W / S — Di chuyển     Space — Chọn / Tiếp tục     Esc — Đóng",
                HorizontalAlignment = HorizontalAlignment.Right,
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            content.AddChild(closeHintLabel);
        }

        private void SetChoices(IReadOnlyList<string> choices)
        {
            foreach (Node child in _choicesContainer.GetChildren())
            {
                child.QueueFree();
            }

            _choiceRows.Clear();

            for (int i = 0; i < choices.Count; i++)
            {
                CreateChoiceRow(i, choices[i]);
            }
        }

        private void CreateChoiceRow(int index, string text)
        {
            PanelContainer row = new PanelContainer
            {
                CustomMinimumSize = new Vector2(0, 34),
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                MouseFilter = Control.MouseFilterEnum.Stop
            };
            _choicesContainer.AddChild(row);

            row.MouseEntered += () => ChoiceHovered?.Invoke(index);
            row.GuiInput += input =>
            {
                if (input is InputEventMouseButton mouseButton &&
                    mouseButton.Pressed &&
                    mouseButton.ButtonIndex == MouseButton.Left)
                {
                    ChoiceConfirmed?.Invoke(index);
                }
            };

            HBoxContainer rowContent = new HBoxContainer
            {
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            rowContent.AddThemeConstantOverride("separation", 8);
            row.AddChild(rowContent);

            Label cursorLabel = new Label
            {
                Text = string.Empty,
                CustomMinimumSize = new Vector2(24, 0),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            rowContent.AddChild(cursorLabel);

            Label optionLabel = new Label
            {
                Text = text,
                VerticalAlignment = VerticalAlignment.Center,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            rowContent.AddChild(optionLabel);

            _choiceRows.Add(new ChoiceRow(row, cursorLabel, optionLabel));
        }

        private void UpdateSelectionVisuals()
        {
            for (int i = 0; i < _choiceRows.Count; i++)
            {
                bool isSelected = i == _selectedChoiceIndex;
                ChoiceRow choice = _choiceRows[i];

                StyleBoxFlat style = new StyleBoxFlat
                {
                    BgColor = isSelected
                        ? new Color(0.08f, 0.25f, 0.30f, 1.0f)
                        : new Color(0.06f, 0.07f, 0.09f, 0.65f),
                    BorderColor = isSelected
                        ? new Color(0.25f, 0.90f, 1.0f, 1.0f)
                        : new Color(0.20f, 0.22f, 0.26f, 0.55f)
                };
                style.SetBorderWidthAll(isSelected ? 2 : 1);
                style.SetCornerRadiusAll(4);

                choice.Row.AddThemeStyleboxOverride("panel", style);
                choice.CursorLabel.Text = isSelected ? "▶" : string.Empty;
                choice.CursorLabel.AddThemeColorOverride(
                    "font_color",
                    isSelected ? new Color(0.35f, 0.95f, 1.0f) : Colors.Transparent
                );
                choice.OptionLabel.AddThemeColorOverride(
                    "font_color",
                    isSelected ? Colors.White : new Color(0.72f, 0.74f, 0.78f)
                );
            }
        }
    }
}
