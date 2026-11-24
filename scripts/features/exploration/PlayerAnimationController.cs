using Godot;

namespace RPG2D.Features.Exploration
{
    public partial class PlayerAnimationController : Node
    {
        private static readonly StringName _idleState = "Idle";
        private static readonly StringName _walkState = "Walk";

        private AnimationTree _animationTree = null!;
        private AnimationNodeStateMachinePlayback _animationPlayback = null!;
        private Vector2 _facingDirection;
        private StringName _currentAnimationState = new();

        public override void _Ready()
        {
            _animationTree = GetNode<AnimationTree>("../AnimationTree");
            _animationTree.Active = true;

            _animationPlayback = _animationTree
                .Get("parameters/playback")
                .As<AnimationNodeStateMachinePlayback>();
        }

        public void UpdateFromMovement(Vector2 movement)
        {
            if (movement == Vector2.Zero)
            {
                TransitionTo(_idleState);
                return;
            }

            Vector2 facingDirection = GetFacingDirection(movement);

            if (facingDirection != _facingDirection)
            {
                _facingDirection = facingDirection;
                _animationTree.Set("parameters/Idle/blend_position", _facingDirection);
                _animationTree.Set("parameters/Walk/blend_position", _facingDirection);
            }

            TransitionTo(_walkState);
        }

        private static Vector2 GetFacingDirection(Vector2 movement)
        {
            if (Mathf.Abs(movement.X) > Mathf.Abs(movement.Y))
            {
                return new Vector2(Mathf.Sign(movement.X), 0.0f);
            }

            return new Vector2(0.0f, -Mathf.Sign(movement.Y));
        }

        private void TransitionTo(StringName targetState)
        {
            if (_currentAnimationState == targetState)
            {
                return;
            }

            _animationPlayback.Travel(targetState);
            _currentAnimationState = targetState;
        }
    }
}