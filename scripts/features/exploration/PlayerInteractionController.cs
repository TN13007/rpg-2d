using System.Collections.Generic;
using Godot;
using RPG2D.Features.Interaction;

namespace RPG2D.Features.Exploration
{
    public partial class PlayerInteractionController : Node
    {
        private sealed class ChoiceDefinition
        {
            public string Text { get; }
            public bool ClosesInteraction { get; }

            public ChoiceDefinition(string text, bool closesInteraction = false)
            {
                Text = text;
                ClosesInteraction = closesInteraction;
            }
        }

        private sealed class ChoiceRow
        {
            public PanelContainer Row { get; }
            public Label CursorLabel { get; }
            public Label OptionLabel { get; }
            public bool ClosesInteraction { get; }

            public ChoiceRow(
                PanelContainer row,
                Label cursorLabel,
                Label optionLabel,
                bool closesInteraction)
            {
                Row = row;
                CursorLabel = cursorLabel;
                OptionLabel = optionLabel;
                ClosesInteraction = closesInteraction;
            }
        }

        private PlayerController _playerController = null!;
        private TerminalController? _currentTerminal;
        private CanvasLayer _ui = null!;
        private Label _promptLabel = null!;
        private PanelContainer _interactionPanel = null!;
        private Label _titleLabel = null!;
        private Label _messageLabel = null!;
        private Label _feedbackLabel = null!;
        private ScrollContainer _choicesScrollContainer = null!;

        private readonly List<ChoiceDefinition> _choiceOptions = new()
        {
            new ChoiceDefinition("Lựa chọn 1"),
            new ChoiceDefinition("Lựa chọn 2"),
            new ChoiceDefinition("Lựa chọn 3"),
            new ChoiceDefinition("Lựa chọn 4"),
            new ChoiceDefinition("Lựa chọn 5"),
            new ChoiceDefinition("Lựa chọn 6"),
            new ChoiceDefinition("Leave", true)
        };

        private readonly List<ChoiceRow> _choiceRows = new();
        private int _selectedChoiceIndex;
        private bool _isInteractionOpen;

        public override void _Ready()
        {
            ProcessMode = ProcessModeEnum.Always;
            _playerController = GetParent<PlayerController>();
            BuildInterface();
        }

        public override void _Process(double delta)
        {
            if (_isInteractionOpen)
            {
                HandleInteractionInput();
                return;
            }

            if (_currentTerminal != null && Input.IsActionJustPressed("interact"))
            {
                OpenInteraction();
            }
        }

        public void SetCurrentTerminal(TerminalController terminal)
        {
            _currentTerminal = terminal;

            if (!_isInteractionOpen)
            {
                _promptLabel.Visible = true;
            }
        }

        public void ClearCurrentTerminal(TerminalController terminal)
        {
            if (_currentTerminal != terminal)
            {
                return;
            }

            _currentTerminal = null;

            if (!_isInteractionOpen)
            {
                _promptLabel.Visible = false;
            }
        }

        private void HandleInteractionInput()
        {
            if (Input.IsActionJustPressed("ui_cancel"))
            {
                CloseInteraction();
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

            if (Input.IsActionJustPressed("interaction_confirm"))
            {
                ConfirmSelection();
            }
        }

        private void MoveSelection(int direction)
        {
            if (_choiceRows.Count == 0)
            {
                return;
            }

            _selectedChoiceIndex =
                (_selectedChoiceIndex + direction + _choiceRows.Count) % _choiceRows.Count;

            UpdateSelectionVisuals();
            _choicesScrollContainer.EnsureControlVisible(
                _choiceRows[_selectedChoiceIndex].Row
            );
        }

        private void SelectChoice(int index)
        {
            if (!_isInteractionOpen ||
                index < 0 ||
                index >= _choiceRows.Count ||
                _selectedChoiceIndex == index)
            {
                return;
            }

            _selectedChoiceIndex = index;
            UpdateSelectionVisuals();
        }

        private void ConfirmSelection()
        {
            if (_selectedChoiceIndex < 0 ||
                _selectedChoiceIndex >= _choiceRows.Count)
            {
                return;
            }

            if (_choiceRows[_selectedChoiceIndex].ClosesInteraction)
            {
                CloseInteraction();
                return;
            }

            _feedbackLabel.Text = $"Bạn chọn số {_selectedChoiceIndex + 1}";
        }

        private void OpenInteraction()
        {
            if (_currentTerminal == null)
            {
                return;
            }

            _isInteractionOpen = true;
            _titleLabel.Text = _currentTerminal.DisplayName;
            _messageLabel.Text = _currentTerminal.Response;
            _feedbackLabel.Text = string.Empty;
            _selectedChoiceIndex = 0;
            UpdateSelectionVisuals();
            _promptLabel.Visible = false;
            _interactionPanel.Visible = true;
            _playerController.SetPhysicsProcess(false);
        }

        private void CloseInteraction()
        {
            _isInteractionOpen = false;
            _interactionPanel.Visible = false;
            _playerController.SetPhysicsProcess(true);
            _promptLabel.Visible = _currentTerminal != null;
        }

        private void BuildInterface()
        {
            _ui = new CanvasLayer();
            AddChild(_ui);

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
            _ui.AddChild(_promptLabel);

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
            _ui.AddChild(_interactionPanel);

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

            VBoxContainer choicesContainer = new VBoxContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            choicesContainer.AddThemeConstantOverride("separation", 4);
            _choicesScrollContainer.AddChild(choicesContainer);

            _choiceRows.Clear();

            for (int i = 0; i < _choiceOptions.Count; i++)
            {
                CreateChoiceRow(choicesContainer, i, _choiceOptions[i]);
            }

            _feedbackLabel = new Label
            {
                Text = string.Empty,
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            content.AddChild(_feedbackLabel);

            Label closeHintLabel = new Label
            {
                Text = "W / S — Di chuyển     Space — Chọn     Esc — Đóng",
                HorizontalAlignment = HorizontalAlignment.Right,
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            content.AddChild(closeHintLabel);

            UpdateSelectionVisuals();
        }

        private void CreateChoiceRow(
            VBoxContainer parent,
            int index,
            ChoiceDefinition option)
        {
            PanelContainer row = new PanelContainer
            {
                CustomMinimumSize = new Vector2(0, 34),
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                MouseFilter = Control.MouseFilterEnum.Stop
            };
            parent.AddChild(row);

            row.MouseEntered += () => SelectChoice(index);
            row.GuiInput += input =>
            {
                if (input is InputEventMouseButton mouseButton &&
                    mouseButton.Pressed &&
                    mouseButton.ButtonIndex == MouseButton.Left)
                {
                    SelectChoice(index);
                    ConfirmSelection();
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
                Text = option.Text,
                VerticalAlignment = VerticalAlignment.Center,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            rowContent.AddChild(optionLabel);

            _choiceRows.Add(new ChoiceRow(
                row,
                cursorLabel,
                optionLabel,
                option.ClosesInteraction
            ));
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
