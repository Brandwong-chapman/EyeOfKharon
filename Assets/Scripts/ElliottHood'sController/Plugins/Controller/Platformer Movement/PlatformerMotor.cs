using System;
using UnityEngine;

namespace ControllerSystem.Platformer2D
{
    /// <summary>
    /// Handles universal information like GroundChecks and weightlessness, that are typically accessed by many modules
    /// Note: This script takes input in FixedUpdate... This is not recommended, but also isn't really noticeable
    /// </summary>
    public class PlatformerMotor : Motor
    {
        [SerializeField] private Collider2D _collider;
        [SerializeField] private GroundCheck _groundCheck;
        
        public Collider2D Collider => _collider;
        public Vector2 LastVelocity { get; private set; }
        public float CounteractGravityVelocity => -Physics2D.gravity.y * Rb.gravityScale * Time.fixedDeltaTime;
        private Vector3 GroundContactPoint => new Vector2(transform.position.x, Collider.bounds.min.y);
        private PlatformerMotorModule[] _modules;

        protected override void Awake()
        {
            base.Awake();
            _modules = GetComponents<PlatformerMotorModule>();
            foreach (PlatformerMotorModule platformerMotorModule in _modules)
            {
                platformerMotorModule.Initialize(this);
            }
        }

        protected virtual void FixedUpdate()
        {
            HandleLandingCheck();
        }

        public override void HandleLocalControllerMovement()
        {
            foreach (PlatformerMotorModule platformerMotorModule in _modules)
            {
                platformerMotorModule.HandleMovement();
            }

            LastVelocity = Rb.linearVelocity;
        }

        protected void OnEnable()
        {
            LastVelocity = Rb.linearVelocity;
        }

        #region Ground Check

        public bool Grounded { get; set; }
        private bool _wasGrounded;
        private const float MAX_GROUNDED_Y_VELOCITY = 1.5f;

        public float LastGroundedTime { get; private set; }
        public Vector2 LastGroundedPosition { get; private set; }
        
        public event Action<PositionInfo> OnLand;
        public event EventHandler OnGroundedUpdate;

        private void HandleLandingCheck()
        {
            _wasGrounded = Grounded;
            Grounded = _groundCheck.Grounded && Rb.linearVelocity.y < MAX_GROUNDED_Y_VELOCITY;
            if (Grounded)
            {
                LastGroundedPosition = transform.position;
                if (!_wasGrounded)
                {
                    Land();
                    NewGroundedState();
                }
            }
            else if (_wasGrounded)
            {
                // Leave the ground (coyote time)
                LastGroundedTime = Time.time;
                NewGroundedState();
            }
        }

        /// <summary>
        /// Called when landing or leaving the ground
        /// </summary>
        protected virtual void NewGroundedState()
        {
            OnGroundedUpdate?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void Land()
        {
            OnLand?.Invoke(new PositionInfo
            {
                Position = GroundContactPoint
            });
        }

        #endregion

        #region Weightlessness

        public bool Weightless => _weightlessUntilTime >= Time.time;
        private float _weightlessUntilTime = -1;

        public void MakeWeightless(float duration)
        {
            float weightlessUntil = Time.time + duration;
            if (weightlessUntil < _weightlessUntilTime)
                return;

            _weightlessUntilTime = weightlessUntil;
            SetGravity(0);
        }

        #endregion

        #region Rigidbody Settings
        
        public void SetGravity(float gravity)
        {
            if (Weightless)
            {
                Rb.gravityScale = 0;
            }
            else
            {
                Rb.gravityScale = gravity;
            }
        }

        #endregion
    }

    public class PositionInfo
    {
        public Vector3 Position;
    }
}