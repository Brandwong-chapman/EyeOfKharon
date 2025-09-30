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
        private FighterController  _fighterController;

       
        private void Awake()
        {
            _movementModule = GetComponent<PlatformerHorizontalMovementModule>();
            _fighterController = GetComponent<FighterController>();

        }
        
        public override void Initialize(PlatformerMotor newMotor)
        {
            base.Initialize(newMotor);

            _movementModule = GetComponent<PlatformerHorizontalMovementModule>();
            _fighterController = GetComponent<FighterController>();

        }

        public override void HandleMovement()
        {
           
            if (_isDashing)
            {
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
                motor.Rb.linearDamping = (float)0; // no drag during dash
                Vector2 input = Controller.Input.move.GetValue();
                if (input.sqrMagnitude < 0.1f)
                {
                    // fallback to facing direction if no input
                    float facingDir = Controller.FacingLeft ? -1f : 1f;
                    input = new Vector2(facingDir, 0);
                }

                _dashDirection = input.normalized;
                _fighterController.UpdateState(FighterController.States.Dash);
                _isDashing = true;
                _lastDashTime = Time.time;
                _dashEndTime = Time.time + _dashDuration;
                motor.Rb.gravityScale = 0;

                // Apply initial velocity instantly
                motor.Rb.linearVelocity = _dashDirection * _dashSpeed;
            }

            
        }
        private void EndDash()
        {
            _fighterController.UpdateState(FighterController.States.Movement);
            _isDashing = false;
            _isReducedGravity = true;
            _reducedGravityEndTime = Time.time + _reducedGravityDuration;
            
            motor.IgnoreFallGravity = true;
            
            motor.Rb.linearDamping = _movementModule.FindDragForce();
            motor.Rb.linearVelocity = Vector2.zero;
        }
        private void EndReducedGravity()
        {
            _isReducedGravity = false;
            motor.IgnoreFallGravity = false;
            motor.Rb.linearDamping = (float)1.7;
        }
        
        private void TryRestoreDash()
        {
            if (!(_dashAmount > 0  && Time.time - _lastDashTime < _dashCooldown) && motor.Grounded)
            {
                _dashAmount = 1;
            }
        }
    }
}