using System;
using UnityEngine;

namespace ControllerSystem.Platformer2D
{
    public class PlatformerJumpModule : PlatformerMotorModule
    {
        public override void HandleMovement()
        {
            HandleHeadBonk();
            HandleJump();
            HandleGravity();
            HandleCornerNudge();
        }
        
        #region Double Jump Counter

        private float RemainingDoubleJumps { get; set; }
        public override void Initialize(PlatformerMotor newMotor)
        {
            base.Initialize(newMotor);
            
            ResetDoubleJumps();
            motor.OnLand += MotorOnOnLand;
        }

        public void OnDestroy()
        {
            if (motor != null)
            {
                motor.OnLand -= MotorOnOnLand;
            }
        }

        private void MotorOnOnLand(PositionInfo e)
        {
            ResetDoubleJumps();
        }
        
        #endregion
        
        #region Jump

        [Serializable]
        public class JumpConfig
        {
            public int NumDoubleJumps = 1;
            public float JumpHeight = 16.66667f;
            public float JumpEndEarlyForce = 0.6f;
            public float JumpEndEarlyWindowDuration = 0.2f;
        }
        [SerializeField] private JumpConfig _jumpSettings;
        
        [Serializable]
        public class CoyoteTimeSettings
        {
            [Header("Coyote Time")]
            public float CoyoteTimeThreshold = 0.1f;
            public float CoyoteTimeMaxDistance = 1f;
        }
        [SerializeField] CoyoteTimeSettings _coyoteTimeSettings;


        private bool _jumpEndedEarly;
        private float _lastJumpInitiated;
        private bool CoyoteTimeIsValid =>
            motor.LastGroundedTime + _coyoteTimeSettings.CoyoteTimeThreshold > Time.time &&
            Vector2.SqrMagnitude((Vector2)transform.position - motor.LastGroundedPosition) < (_coyoteTimeSettings.CoyoteTimeMaxDistance * _coyoteTimeSettings.CoyoteTimeMaxDistance);

        private bool WithinJumpEndEarlyWindow =>
            _lastJumpInitiated + _jumpSettings.JumpEndEarlyWindowDuration > Time.time;
        
        private Func<bool> _jumpOverrideHandler;

        protected virtual bool CanJump()
        {
            return (motor.Grounded || CoyoteTimeIsValid) && _lastJumpInitiated + _coyoteTimeSettings.CoyoteTimeThreshold < Time.time; //don't let them jump twice in a row
        }
        protected virtual bool CanDoubleJump()
        {
            return !motor.Grounded && RemainingDoubleJumps > 0;
        }

        /// <summary>
        /// Function should return true if jump was already handled
        /// </summary>
        /// <param name="handler"></param>
        public void SetJumpOverrideHandler(Func<bool> handler)
        {
            _jumpOverrideHandler = handler;
        }

        protected virtual void HandleJump()
        {
            HandleJumpEndEarly();

            // Jump override (wall jump, spring, etc.)
            if (_jumpOverrideHandler != null && _jumpOverrideHandler.Invoke())
                return;

            // Normal jump / double jump
            if (CanJump() && Controller.Input.jump.TryUseBuffer())
            {
                Jump();
                InvokeEvent(Events.Jump);
            }
            else if (CanDoubleJump() && Controller.Input.jump.TryUseBuffer())
            {
                RemainingDoubleJumps--;
                Jump();
                InvokeEvent(Events.DoubleJump);
                if (RemainingDoubleJumps <= 0)
                {
                    InvokeEvent(Events.FinalDoubleJump);
                }
            }
        }

        protected virtual void HandleJumpEndEarly()
        {
            // Send the player back down when they release jump
            if (!motor.Grounded && WithinJumpEndEarlyWindow && !Controller.Input.jump.GetHeld() && !_jumpEndedEarly)
            {
                _jumpEndedEarly = true;

                if (!motor.Weightless)
                {
                    motor.Rb.AddForce(new Vector2(0, -_jumpSettings.JumpEndEarlyForce * motor.Rb.linearVelocity.y), ForceMode2D.Impulse);
                }
            }
        }

        protected virtual void Jump()
        {
            ResetJumpEndedEarly();

            if (motor.Weightless)
            {
                _jumpEndedEarly = true;
            }

            // Add jump velocity
            motor.Rb.linearVelocity = new Vector2(motor.Rb.linearVelocity.x, _jumpSettings.JumpHeight);

            // Check for a head bonk
            if (PerformHeadBonkCheck(motor.Collider, GroundCheck.GroundLayerMask))
            {
                BonkHead(motor.Rb.linearVelocity.y);
            }

            // Knockback partially overrides vertical and horizontal velocity. This prevents your jump from getting 'eaten' when in a knockback state
            if (Controller is FighterController fighter)
            {
                fighter.InterruptYKnockback();
            }

            motor.Grounded = false;
        }

        internal void ResetJumpEndedEarly()
        {
            _jumpEndedEarly = false;
            _lastJumpInitiated = Time.time;
        }

        public void ResetDoubleJumps()
        {
            RemainingDoubleJumps = _jumpSettings.NumDoubleJumps;
        }

        #endregion
        
        #region Gravity

        [Serializable]
        public class GravitySettings
        {
            public float Gravity = 1.75f;
            public float FallGravity = 5f;
            public float FallCap = 7f;
            [Header("Fastfall Settings")] 
            public bool FastFallEnabled = true;
            public float FastFallGravityCap = 10f;
            public float FastFallGravityMultiplier = 1.5f;
        }
        [SerializeField] private GravitySettings _gravitySettings;
        
        public bool Rising => motor.Rb.linearVelocity.y > 0.1f && !motor.Grounded;
        public bool Falling => motor.Rb.linearVelocity.y < -0.1f && !motor.Grounded;
        private bool FastFalling => _gravitySettings.FastFallEnabled && Falling && Controller.Input.move.GetValue().y < 0;
        public float FallCap => FastFalling ? _gravitySettings.FastFallGravityCap : _gravitySettings.FallCap;
        
        private void HandleGravity()
        {
            // Cap the fall speed
            if (motor.Rb.linearVelocity.y < -_gravitySettings.FallCap)
            {
                motor.Rb.linearVelocity = new Vector2(motor.Rb.linearVelocity.x, -FallCap + motor.CounteractGravityVelocity);
            }
            
            // Fastfall
            float gravityMultiplier = FastFalling ? _gravitySettings.FastFallGravityMultiplier : 1;
            
            // Increase gravity when falling
            if (motor.Rb.linearVelocity.y < 0 && !motor.Grounded)
            {
                motor.SetGravity(_gravitySettings.FallGravity * gravityMultiplier);
            }
            else
            {
                motor.SetGravity(_gravitySettings.Gravity * gravityMultiplier);
            }
        }
        
        #endregion

        #region Head Bonk Forgiveness

        [Serializable]
        private class HeadbonkSettings
        {
            [Tooltip("Higher -> longer weightless duration. ")]
            public float YVelocityToWeightlessDurationMultiplier = 0.15f;
            public float MaxWeightlessDuration = 0.1f;
            [Tooltip("The minimum y velocity needed to register a headbonk")]
            public float YVelocityThreshold = 1;
        }
        [SerializeField] private HeadbonkSettings _headbonkSettings;

        private void HandleHeadBonk()
        {
            bool headBonkDetected = motor.LastVelocity.y > _headbonkSettings.YVelocityThreshold && motor.Rb.linearVelocity.y < 0.1f && PerformHeadBonkCheck(motor.Collider, GroundCheck.GroundLayerMask);
            if (headBonkDetected)
            {
                BonkHead(motor.LastVelocity.y);
            }
        }

        private void BonkHead(float yVelocity)
        {
            motor.Rb.linearVelocity = new Vector2(motor.Rb.linearVelocity.x, 0);
            float weightlessDuration = Mathf.Min(_headbonkSettings.MaxWeightlessDuration, _headbonkSettings.YVelocityToWeightlessDurationMultiplier * yVelocity);

            _jumpEndedEarly = true;
            motor.MakeWeightless(weightlessDuration);
        }

        private static Collider2D PerformHeadBonkCheck(Collider2D collider, LayerMask groundMask)
        {
            Rect checkRect = GetHeadBonkCheckRect(collider);
            return Physics2D.OverlapBox(checkRect.center, checkRect.size, 0f, groundMask);
        }

        private static Rect GetHeadBonkCheckRect(Collider2D collider)
        {
            Bounds bounds = collider.bounds;

            float width = bounds.size.x - PlatformerColliderManager.COLLIDER_SPACING * 2;
            float height = PlatformerColliderManager.GROUND_CHECK_THICKNESS;

            Vector2 center = new Vector2(bounds.center.x, bounds.max.y + height / 2);

            return new Rect(center - new Vector2(width / 2, height / 2), new Vector2(width, height));
        }

#if UNITY_EDITOR

        private static void DrawHeadBonkCheckGizmo(Collider2D collider, Color color)
        {
            Rect checkRect = GetHeadBonkCheckRect(collider);
            UnityEditor.Handles.color = color;
            UnityEditor.Handles.DrawSolidRectangleWithOutline(
                new Vector3[]
                {
                    new Vector3(checkRect.xMin, checkRect.yMin),
                    new Vector3(checkRect.xMax, checkRect.yMin),
                    new Vector3(checkRect.xMax, checkRect.yMax),
                    new Vector3(checkRect.xMin, checkRect.yMax)
                },
                new Color(color.r, color.g, color.b, 0.1f), // fill
                color // outline
            );
        }
        
#endif

        #endregion
        
        #region Corner Nudge Forgiveness

        [Serializable]
        private class CornerNudgeSettings
        {
            public Collider2D Collider;
            public float BaseRayDistance = 0.15f;
            public int CornerRayCount = 6;
            public float InwardStep = 0.05f;
            [Tooltip("Extra horizontal offset for the outermost ray (helps catch corner cases when moving diagonally)")]
            public float OutsideThreshold = 0.025f;
            [Tooltip("Max angle (degrees) difference allowed between surface normal and vertical for snapping")]
            public float MaxSurfaceAngle = 5f;
        }

        [SerializeField] private CornerNudgeSettings _cornerNudgeSettings;

        private void HandleCornerNudge()
        {
            if (!Rising || _cornerNudgeSettings.Collider == null)
                return;

            Bounds bounds = _cornerNudgeSettings.Collider.bounds;

            TrySnapSide(bounds, isRightSide: false);
            TrySnapSide(bounds, isRightSide: true);
        }

        private bool IsValidSurfaceNormal(Vector2 normal)
        {
            // Angle between hit normal and down vector (0 degrees means perfectly down)
            float angle = Vector2.Angle(normal, Vector2.down);
            return angle <= _cornerNudgeSettings.MaxSurfaceAngle;
        }

        private void TrySnapSide(Bounds bounds, bool isRightSide)
        {
            float sign = isRightSide ? 1f : -1f;
            Vector2 baseCorner = isRightSide
                ? new Vector2(bounds.max.x, bounds.max.y)
                : new Vector2(bounds.min.x, bounds.max.y);

            baseCorner += new Vector2(_cornerNudgeSettings.OutsideThreshold * sign, 0);

            float rayLength = GetDynamicRayLength();

            // Outer ray check
            RaycastHit2D outerHit = Physics2D.Raycast(baseCorner, Vector2.up, rayLength, GroundCheck.GroundLayerMask);
            if (!outerHit.collider || !IsValidSurfaceNormal(outerHit.normal))
            {
                return;
            }

            Vector2 firstClearPoint = Vector2.zero;
            bool foundClear = false;

            for (int i = 1; i < _cornerNudgeSettings.CornerRayCount; i++)
            {
                float offset = i * _cornerNudgeSettings.InwardStep;
                Vector2 origin = baseCorner + new Vector2(-sign * offset, 0);
                RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.up, rayLength, GroundCheck.GroundLayerMask);

                if (!hit.collider)
                {
                    firstClearPoint = origin;
                    foundClear = true;
                    break;
                }
                else if (!IsValidSurfaceNormal(hit.normal))
                {
                    // Treat as clear because surface normal is not suitable for snapping
                    firstClearPoint = origin;
                    foundClear = true;
                    break;
                }
            }

            if (foundClear)
            {
                float edgeX = firstClearPoint.x - sign * bounds.extents.x;
                Vector2 position = motor.Rb.position;
                position.x = edgeX;
                motor.Rb.position = position;
            }
        }

        private float GetDynamicRayLength()
        {
            float yVel = motor.Rb.linearVelocity.y;

            // Only extend if moving upwards
            if (yVel <= 0)
                return _cornerNudgeSettings.BaseRayDistance;

            // Predict upward movement next FixedUpdate
            float predictedMovement = yVel * Time.fixedDeltaTime;

            return Mathf.Max(_cornerNudgeSettings.BaseRayDistance, predictedMovement + 0.01f); // add buffer if needed
        }

        #region Gizmos
        
        #if UNITY_EDITOR
        private void DrawCornerCorrectionGizmos()
        {
            if (_cornerNudgeSettings?.Collider == null)
                return;

            Bounds bounds = _cornerNudgeSettings.Collider.bounds;

            DrawCornerRays(bounds, isRightSide: false);
            DrawCornerRays(bounds, isRightSide: true);
        }

        private void DrawCornerRays(Bounds bounds, bool isRightSide)
        {
            float sign = isRightSide ? 1f : -1f;
            Vector2 baseCorner = isRightSide
                ? new Vector2(bounds.max.x, bounds.max.y)
                : new Vector2(bounds.min.x, bounds.max.y);

            baseCorner += new Vector2(_cornerNudgeSettings.OutsideThreshold * sign, 0);

            float rayLength = Application.isPlaying ? GetDynamicRayLength() : _cornerNudgeSettings.BaseRayDistance;

            for (int i = 0; i < _cornerNudgeSettings.CornerRayCount; i++)
            {
                float offset = i * _cornerNudgeSettings.InwardStep;
                Vector2 origin = baseCorner + new Vector2(-sign * offset, 0);

                RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.up, rayLength, GroundCheck.GroundLayerMask);

                Gizmos.color = hit.collider ? Color.red : Color.green;
                Gizmos.DrawLine(origin, origin + Vector2.up * rayLength);
            }
        }
        #endif
        
        #endregion

        #endregion
        
        #region Invoking Events

        public event Action<PositionInfo> OnJump;
        public event Action<PositionInfo> OnDoubleJump;
        public event Action<PositionInfo> OnFinalDoubleJump;
        
        protected enum Events
        {
            Jump,
            DoubleJump,
            FinalDoubleJump
        }
        
        protected void InvokeEvent(Events e)
        {
            switch (e)
            {
                case Events.Jump:
                    OnJump?.Invoke(new PositionInfo
                    {
                        Position = transform.position
                    });
                    break;
                case Events.DoubleJump:
                    OnDoubleJump?.Invoke(new PositionInfo
                    {
                        Position = transform.position
                    });
                    break;
                case Events.FinalDoubleJump:
                    OnFinalDoubleJump?.Invoke(new PositionInfo
                    {
                        Position = transform.position
                    });
                    break;
            }
        }
        
        #endregion
        
        #if UNITY_EDITOR
        
        private void OnDrawGizmosSelected()
        {
            DrawCornerCorrectionGizmos();
            DrawHeadBonkCheckGizmo(GetComponent<PlatformerMotor>().Collider, Color.blue);
        }
        
        #endif
    }
}
