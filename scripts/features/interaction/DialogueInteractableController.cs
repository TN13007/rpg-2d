using Godot;

namespace RPG2D.Features.Interaction
{
    public partial class DialogueInteractableController : Node2D
    {
        [Export]
        public string DialogueId { get; set; } = string.Empty;

        [Export]
        public bool CanCancelInteraction { get; set; } = true;
    }
}
