using Godot;
using RPG2D.Features.Interaction;

namespace RPG2D.Features.Exploration
{
    public partial class PlayerInteractionController : Node
    {
        private PlayerController _playerController = null!;
        private TerminalController? _currentTerminal;
        private CanvasLayer _ui = null!;
        private Label _promptLabel = null!;
        private PanelContainer _interactionPanel = null!;
        private Label _titleLabel = null!;
        private Label _messageLabel = null!;
        private Label _feedbackLabel = null!;
        private Label _closeHintLabel = null!;
        private readonly PanelContainer[] _choiceRows = new PanelContainer[4];
        private readonly Label[] _choiceCursorLabels = new Label[4];
        private readonly Label[] _choiceOptionLabels = new Label[4];
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
            _selectedChoiceIndex = (_selectedChoiceIndex + direction + _choiceRows.Length) % _choiceRows.Length;
            UpdateSelectionVisuals();
        }

        private void ConfirmSelection()
        {
            if (_selectedChoiceIndex == 3)
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
                AnchorLeft = 0.15f,
                AnchorTop = 1.0f,
                AnchorRight = 0.85f,
                AnchorBottom = 1.0f,
                OffsetTop = -260,
                OffsetBottom = -24
            };
            _ui.AddChild(_interactionPanel);

            VBoxContainer content = new VBoxContainer();
            content.AddThemeConstantOverride("separation", 8);
            _interactionPanel.AddChild(content);

            _titleLabel = new Label
            {
                Text = "Terminal",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            content.AddChild(_titleLabel);

            _messageLabel = new Label
            {
                Text = string.Empty,
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            content.AddChild(_messageLabel);

            VBoxContainer choicesContainer = new VBoxContainer();
            choicesContainer.AddThemeConstantOverride("separation", 4);
            content.AddChild(choicesContainer);

            CreateChoiceRow(choicesContainer, 0, "Lựa chọn 1");
            CreateChoiceRow(choicesContainer, 1, "Lựa chọn 2");
            CreateChoiceRow(choicesContainer, 2, "Lựa chọn 3");
            CreateChoiceRow(choicesContainer, 3, "Leave");

            _feedbackLabel = new Label
            {
                Text = string.Empty,
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            content.AddChild(_feedbackLabel);

            _closeHintLabel = new Label
            {
                Text = "W / S — Di chuyển     Space — Chọn     Esc — Đóng",
                HorizontalAlignment = HorizontalAlignment.Right
            };
            content.AddChild(_closeHintLabel);

            UpdateSelectionVisuals();
        }

        private void CreateChoiceRow(VBoxContainer parent, int index, string optionText)
        {
            PanelContainer row = new PanelContainer
            {
                CustomMinimumSize = new Vector2(0, 34)
            };
            parent.AddChild(row);
            _choiceRows[index] = row;

            HBoxContainer rowContent = new HBoxContainer();
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
            _choiceCursorLabels[index] = cursorLabel;

            Label optionLabel = new Label
            {
                Text = optionText,
                VerticalAlignment = VerticalAlignment.Center,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            rowContent.AddChild(optionLabel);
            _choiceOptionLabels[index] = optionLabel;
        }

        private void UpdateSelectionVisuals()
        {
            for (int i = 0; i < _choiceRows.Length; i++)
            {
                bool isSelected = i == _selectedChoiceIndex;

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

                _choiceRows[i].AddThemeStyleboxOverride("panel", style);
                _choiceCursorLabels[i].Text = isSelected ? "▶" : string.Empty;
                _choiceCursorLabels[i].AddThemeColorOverride(
                    "font_color",
                    isSelected ? new Color(0.35f, 0.95f, 1.0f) : Colors.Transparent
                );
                _choiceOptionLabels[i].AddThemeColorOverride(
                    "font_color",
                    isSelected ? Colors.White : new Color(0.72f, 0.74f, 0.78f)
                );
            }
        }
    }
}
