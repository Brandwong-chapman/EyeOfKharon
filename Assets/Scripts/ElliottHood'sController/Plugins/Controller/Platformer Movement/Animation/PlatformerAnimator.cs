using UnityEngine;

namespace ControllerSystem.Platformer2D
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Animator))]
    public class PlatformerAnimator : EntityAnimator
    {
        // These should match the names of the Aseprite animations
        private static readonly int Idle = Animator.StringToHash("Idle");
        private static readonly int Run = Animator.StringToHash("Run");
        private static readonly int Jump = Animator.StringToHash("Jump");
        private static readonly int Fall = Animator.StringToHash("Fall");
        private static readonly int Crouch = Animator.StringToHash("Crouch");
        private static readonly int WallClimb = Animator.StringToHash("WallClimb");
        private static readonly int WallCling = Animator.StringToHash("WallCling");
        private static readonly int WallSlide = Animator.StringToHash("WallSlide");
        private static readonly int WallHang = Animator.StringToHash("WallHang");
        private static readonly int Dead = Animator.StringToHash("Dead");
        
        [SerializeField] private float _jumpAnimationLength = 0.25f;
        [SerializeField] private FighterController _fighterController;
        [SerializeField] private PlatformerJumpModule _platformerJumpModule;
        [SerializeField] private PlatformerCrouchModule _crouchModule;
        [SerializeField] private PlatformerWallModule _platformerWallModule;
        private PlatformerMotor PlatformerMotor => (PlatformerMotor)motor;
        private float _lockInJumpAnimationUntilTime;

        protected override void Awake()
        {
            base.Awake();
            _platformerJumpModule.OnJump += PlatformerMovement_OnJump;
        }

        protected virtual void OnDestroy()
        {
            if (_platformerJumpModule != null)
            {
                _platformerJumpModule.OnJump -= PlatformerMovement_OnJump;
            }
        }

        protected virtual void PlatformerMovement_OnJump(PositionInfo e)
        {
            _lockInJumpAnimationUntilTime = Time.time + _jumpAnimationLength;
        }

        protected override void Animate()
        {
            base.Animate();
            HandleAnimation();
        }

        private void HandleAnimation()
        {
            int state = _fighterController.CurrentState == FighterController.States.Dead ? Dead : GetMovementState();

            SwitchAnimState(state);
        }

        private int GetMovementState()
        {
            int state;
            if (_platformerWallModule != null && _platformerWallModule.State != PlatformerWallModule.WallState.None)
            {
                state = GetWallState();
            }
            else if (PlatformerMotor.Grounded)
            {
                state = GetGroundedState();
            }
            else
            {
                state = GetAirState();
            }

            return state;
        }

        protected virtual int GetWallState()
        {
            return _platformerWallModule.State switch
            {
                PlatformerWallModule.WallState.Cling => WallCling,
                PlatformerWallModule.WallState.Slide => WallSlide,
                PlatformerWallModule.WallState.Climb => WallClimb,
                PlatformerWallModule.WallState.Hang => WallHang,
                _ => -1
            };
        }

        protected virtual int GetGroundedState()
        {
            int state;
            if (_crouchModule != null && _crouchModule.Crouching)
            {
                state = Crouch;
            }
            else if (motor.Controller.InputtingHorizontalMovement && Mathf.Abs(motor.Rb.linearVelocity.x) > 0.1f)
            {
                state = Run;
            }
            else
            {
                state = Idle;
            }

            return state;
        }
        
        protected virtual int GetAirState()
        {
            int state;
            if (_platformerJumpModule.Rising || _lockInJumpAnimationUntilTime >= Time.time)
            {
                state = Jump;
            }
            else
            {
                state = Fall;
            }

            return state;
        }
    }
}