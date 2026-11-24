using Godot;
using RPG2D.Features.Exploration;

namespace RPG2D.Features.Interaction
{
    public partial class TerminalController : Node2D
    {
        [Export]
        public string DisplayName { get; set; } = "Terminal";

        [Export(PropertyHint.MultilineText)]
        public string Response { get; set; } = "System online. No new messages.";

        private Area2D _interactionArea = null!;

        public override void _Ready()
        {
            _interactionArea = GetNode<Area2D>("InteractionArea");
            _interactionArea.BodyEntered += OnBodyEntered;
            _interactionArea.BodyExited += OnBodyExited;
        }

        private void OnBodyEntered(Node2D body)
        {
            PlayerInteractionController controller =
                body.GetNodeOrNull<PlayerInteractionController>("PlayerInteractionController");

            controller?.SetCurrentTerminal(this);
        }

        private void OnBodyExited(Node2D body)
        {
            PlayerInteractionController controller =
                body.GetNodeOrNull<PlayerInteractionController>("PlayerInteractionController");

            controller?.ClearCurrentTerminal(this);
        }
    }
}
