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
        private Label _choicesLabel = null!;
        private Label _feedbackLabel = null!;
        private Label _closeHintLabel = null!;
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

            if (Input.IsActionJustPressed("interaction_choice_1"))
            {
                ShowChoiceResult(1);
                return;
            }

            if (Input.IsActionJustPressed("interaction_choice_2"))
            {
                ShowChoiceResult(2);
                return;
            }

            if (Input.IsActionJustPressed("interaction_choice_3"))
            {
                ShowChoiceResult(3);
                return;
            }

            if (Input.IsActionJustPressed("interaction_leave"))
            {
                CloseInteraction();
            }
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
            _promptLabel.Visible = false;
            _interactionPanel.Visible = true;
            _playerController.SetPhysicsProcess(false);
        }

        private void ShowChoiceResult(int choice)
        {
            _feedbackLabel.Text = $"Bạn chọn số {choice}";
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
                OffsetTop = -220,
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

            _choicesLabel = new Label
            {
                Text = "1 — Lựa chọn 1\n2 — Lựa chọn 2\n3 — Lựa chọn 3\n4 — Leave"
            };
            content.AddChild(_choicesLabel);

            _feedbackLabel = new Label
            {
                Text = string.Empty,
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            content.AddChild(_feedbackLabel);

            _closeHintLabel = new Label
            {
                Text = "4 — Leave     Esc — Close",
                HorizontalAlignment = HorizontalAlignment.Right
            };
            content.AddChild(_closeHintLabel);
        }
    }
}
