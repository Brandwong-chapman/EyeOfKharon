using System;
using UnityEngine;
//Brandon's Code
namespace ControllerSystem.Platformer2D
{
    public class PlatformerDashModule : PlatformerMotorModule
    {
        /// <summary>
        /// Handles dash movement when the Dash action is triggered.
        /// </summary>
        [SerializeField] private float _dashSpeed = 15f;
        [SerializeField] private float _waveDashSpeed = 15f;
        [SerializeField] private float _dashDuration = 0.2f;
        [SerializeField] private float _dashCooldown = 0.5f;
        [SerializeField] private int _dashAmount = 1;
        [SerializeField] private float _reducedGravityDuration = 0.5f; // half a second

        //[Tooltip("Applies only when not inputting movement")]
        private bool _isDashing;
        private bool _isReducedGravity;
        private float _lastDashTime;
        private float _dashEndTime; 
        private float _reducedGravityEndTime;
        private Vector2 _dashDirection;
        
        private PlatformerHorizontalMovementModule _movementModule;
       
        private void Awake()
        {
            _movementModule = GetComponent<PlatformerHorizontalMovementModule>();
        }
        
        public override void Initialize(PlatformerMotor newMotor)
        {
            base.Initialize(newMotor);

            _movementModule = GetComponent<PlatformerHorizontalMovementModule>();
        }

        public override void HandleMovement()
        {
           
            if (_isDashing)
            {
                Debug.Log($"[DASH LOOP] Time: {Time.time:F2}, End: {_dashEndTime:F2}, Dashing: {_isDashing}, Dir: {_dashDirection}");
                if (Time.time >= _dashEndTime)
                {
                    EndDash();
                }
                else
                {
                    motor.Rb.linearVelocity = _dashDirection * _dashSpeed;

                }
            }
            else
            {
                
                if (Controller.Input.dash.GetHeld())
                {
                    TryStartDash();
                }

                TryRestoreDash();
            
                if (_isReducedGravity && Time.time >= _reducedGravityEndTime)
                {
                    EndReducedGravity();
                }
            }
        }

        private void TryStartDash()
        {

            if (_dashAmount != 0)
            {
                --_dashAmount;
                motor.Rb.linearDamping = (float)0;
                Vector2 input = Controller.Input.move.GetValue();
                if (input.sqrMagnitude < 0.1f)
                {
                    // fallback to facing direction if no input
                    float facingDir = Controller.FacingLeft ? -1f : 1f;
                    input = new Vector2(facingDir, 0);
                    Debug.Log($"[DASH START] No input, fallback to facing: {facingDir}");
                }

                _dashDirection = input.normalized;
                _isDashing = true;
                _lastDashTime = Time.time;
                _dashEndTime = Time.time + _dashDuration;
                motor.Rb.gravityScale = 0;

                // Apply initial velocity instantly
                motor.Rb.linearVelocity = _dashDirection * _dashSpeed;
                Debug.Log(
                    $"[DASH START] Dir: {_dashDirection}, " +
                    $"Input: {input}, FacingLeft: {Controller.FacingLeft}, " +
                    $"Duration: {_dashDuration}, EndTime: {_dashEndTime}, Vel: {motor.Rb.linearVelocity}"
                );
            }

            
        }
        private void EndDash()
        {
            _isDashing = false;
            _isReducedGravity = true;
            _reducedGravityEndTime = Time.time + _reducedGravityDuration;
            
            motor.IgnoreFallGravity = true;
            
            motor.Rb.linearDamping = _movementModule.FindDragForce();
            motor.Rb.linearVelocity = Vector2.zero;
            
            Debug.Log($"[END DASH] Time: {Time.time:F2}, End: {_dashEndTime:F2}, Dir: {_dashDirection}");
        }
        private void EndReducedGravity()
        {
            _isReducedGravity = false;
            motor.IgnoreFallGravity = false;
            motor.Rb.linearDamping = (float)1.7;
        }
        
        private void TryRestoreDash()
        {
            // Only restore if dash is consumed
            if (_dashAmount > 0) return;

            // Condition: half a second has passed since last dash
            if (Time.time - _lastDashTime < _dashCooldown) return;

            // Condition: player must be grounded
            if (!motor.Grounded) return;

            // Restore dash
            _dashAmount = 1;
        }

        public bool IsDashing()
        {
            return _isDashing;
        }

    }
}