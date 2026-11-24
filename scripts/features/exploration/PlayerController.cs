using Godot;

namespace RPG2D.Features.Exploration
{
    public partial class PlayerController : CharacterBody2D
    {
        private PlayerAnimationController _animationController = null!;

        [Export]
        public float MoveSpeed { get; set; } = 90.0f;

        public override void _Ready()
        {
            _animationController = GetNode<PlayerAnimationController>("PlayerAnimationController");
        }

        public override void _PhysicsProcess(double delta)
        {
            Vector2 movement = Input.GetVector(
                "move_left",
                "move_right",
                "move_up",
                "move_down"
            );

            if (movement.LengthSquared() > 1.0f)
            {
                movement = movement.Normalized();
            }

            Velocity = movement * MoveSpeed;
            MoveAndSlide();

            _animationController.UpdateFromMovement(movement);
        }
    }
}
