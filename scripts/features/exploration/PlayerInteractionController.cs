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
        private PanelContainer _messagePanel = null!;
        private Label _titleLabel = null!;
        private Label _messageLabel = null!;
        private Label _closeHintLabel = null!;
        private bool _isMessageOpen;

        public override void _Ready()
        {
            ProcessMode = ProcessModeEnum.Always;
            _playerController = GetParent<PlayerController>();
            BuildInterface();
        }

        public override void _Process(double delta)
        {
            if (_isMessageOpen)
            {
                if (Input.IsActionJustPressed("interact") ||
                    Input.IsActionJustPressed("ui_cancel"))
                {
                    CloseMessage();
                }

                return;
            }

            if (_currentTerminal != null && Input.IsActionJustPressed("interact"))
            {
                OpenMessage();
            }
        }

        public void SetCurrentTerminal(TerminalController terminal)
        {
            _currentTerminal = terminal;

            if (!_isMessageOpen)
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

            if (!_isMessageOpen)
            {
                _promptLabel.Visible = false;
            }
        }

        private void OpenMessage()
        {
            if (_currentTerminal == null)
            {
                return;
            }

            _isMessageOpen = true;
            _titleLabel.Text = _currentTerminal.DisplayName;
            _messageLabel.Text = _currentTerminal.Response;
            _promptLabel.Visible = false;
            _messagePanel.Visible = true;
            _playerController.SetPhysicsProcess(false);
        }

        private void CloseMessage()
        {
            _isMessageOpen = false;
            _messagePanel.Visible = false;
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

            _messagePanel = new PanelContainer
            {
                Visible = false,
                AnchorLeft = 0.15f,
                AnchorTop = 1.0f,
                AnchorRight = 0.85f,
                AnchorBottom = 1.0f,
                OffsetTop = -170,
                OffsetBottom = -24
            };
            _ui.AddChild(_messagePanel);

            VBoxContainer content = new VBoxContainer();
            content.AddThemeConstantOverride("separation", 8);
            _messagePanel.AddChild(content);

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

            _closeHintLabel = new Label
            {
                Text = "E / Esc — Close",
                HorizontalAlignment = HorizontalAlignment.Right
            };
            content.AddChild(_closeHintLabel);
        }
    }
}