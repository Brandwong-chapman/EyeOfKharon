using System;
using System.Collections;
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
                if (!_isDashing && Controller.Input.dash.GetHeld())
                {
                    TryStartDash();
                }

                TryRestoreDash();
            
                if (_isReducedGravity && Time.time >= _reducedGravityEndTime)
                {
                    EndReducedGravity();
                }
        }

        private void TryStartDash()
        {
            if (_dashAmount != 0 && !_isDashing)
            {
                --_dashAmount;
                _lastDashTime = Time.time; // Mark when dash started
                motor.Rb.linearDamping = (float)0; // no drag during dash
                Vector2 input = Controller.Input.move.GetValue();
                if (input.sqrMagnitude < 0.1f)
                {
                    // fallback to facing direction if no input
                    float facingDir = Controller.FacingLeft ? -1f : 1f;
                    input = new Vector2(facingDir, 0);
                }

                _dashDirection = input.normalized;
                
                StartCoroutine(DashRoutine());
            }
        }
        
        private IEnumerator DashRoutine()
        {
            _isDashing = true;
            _fighterController.UpdateState(FighterController.States.Dash);

            motor.Rb.gravityScale = 0;
            motor.Rb.linearVelocity = _dashDirection * _dashSpeed;

            yield return new WaitForSeconds(_dashDuration);

            EndDash();
        }
        
        private void EndDash()
        {
            _isDashing = false;
            _fighterController.UpdateState(FighterController.States.Movement);
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
            // Only restore dash when grounded AND cooldown is over AND not currently dashing
            if (!_isDashing && motor.Grounded && Time.time >= _lastDashTime + _dashCooldown)
            {
                _dashAmount = 1;
            }
        }
    }
}