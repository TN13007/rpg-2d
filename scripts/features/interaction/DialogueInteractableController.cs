using Godot;
using RPG2D.Features.Exploration;

namespace RPG2D.Features.Interaction
{
    public partial class DialogueInteractableController : Node2D
    {
        [Export]
        public string DialogueId { get; set; } = string.Empty;

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

            controller?.SetCurrentInteractable(this);
        }

        private void OnBodyExited(Node2D body)
        {
            PlayerInteractionController controller =
                body.GetNodeOrNull<PlayerInteractionController>("PlayerInteractionController");

            controller?.ClearCurrentInteractable(this);
        }
    }
}
