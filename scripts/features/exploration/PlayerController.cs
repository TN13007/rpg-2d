using Godot;

namespace RPG2D.Features.Exploration
{
    public partial class PlayerController : CharacterBody2D
    {
        private const int WalkFrameCount = 4;
        private const float AnimationFramesPerSecond = 6.0f;

        private Sprite2D _sprite = null!;
        private int _facingColumn;
        private int _walkFrame;
        private float _animationTimer;

        [Export]
        public float MoveSpeed { get; set; } = 90.0f;

        public override void _Ready()
        {
            _sprite = GetNode<Sprite2D>("Sprite");
            _sprite.FrameCoords = new Vector2I(_facingColumn, 0);
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

            UpdateAnimation(movement, (float)delta);
        }

        private void UpdateAnimation(Vector2 movement, float delta)
        {
            if (movement == Vector2.Zero)
            {
                _walkFrame = 0;
                _animationTimer = 0.0f;
                _sprite.FrameCoords = new Vector2I(_facingColumn, 0);
                return;
            }

            if (Mathf.Abs(movement.X) > Mathf.Abs(movement.Y))
            {
                _facingColumn = movement.X < 0.0f ? 2 : 3;
            }
            else
            {
                _facingColumn = movement.Y < 0.0f ? 1 : 0;
            }

            _animationTimer += delta;
            float frameDuration = 1.0f / AnimationFramesPerSecond;

            while (_animationTimer >= frameDuration)
            {
                _animationTimer -= frameDuration;
                _walkFrame = (_walkFrame + 1) % WalkFrameCount;
            }

            _sprite.FrameCoords = new Vector2I(_facingColumn, _walkFrame);
        }
    }
}
