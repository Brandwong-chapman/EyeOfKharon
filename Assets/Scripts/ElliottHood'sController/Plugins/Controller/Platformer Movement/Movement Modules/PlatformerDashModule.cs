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
        [SerializeField] private float _floatAfterDashTime = 0.2f;

        //[Tooltip("Applies only when not inputting movement")]
        private bool _isDashing;
        private float _lastDashTime;
        private float _dashEndTime;
        private float _defaltGravity;
        private Vector2 _dashDirection;

        private PlatformerHorizontalMovementModule _movementModule;
        private FighterController _fighterController;


        private void Awake()
        {
            _movementModule = GetComponent<PlatformerHorizontalMovementModule>();
            _fighterController = GetComponent<FighterController>();
            _defaltGravity = motor.Rb.linearDamping;
        }

        public override void Initialize(PlatformerMotor newMotor)
        {
            base.Initialize(newMotor);

            _movementModule = GetComponent<PlatformerHorizontalMovementModule>();
            _fighterController = GetComponent<FighterController>();

        }

        public override void HandleMovement()
        {
            HandleDash();
            
            TryStartDash();

            TryRestoreDash();
        }

        private void HandleDash()
        {
            if (_isDashing)
            {
                // parts of the TilesMap (the ground) can collide with the player thus slowing down the dash. This makes sure it is always going at the correct speed when Grounded
                if (motor.Grounded && Mathf.Abs(_dashDirection.y) < 0.1f)
                {
                    motor.Rb.linearVelocity = new Vector2(_dashDirection.x * _dashSpeed, 0f);
                }
                else
                {
                    motor.Rb.linearVelocity = _dashDirection * _dashSpeed;
                }
            }
        }

        private void TryStartDash()
        {
            if (_dashAmount != 0 && !_isDashing && Controller.Input.dash.GetHeld())
            {
                --_dashAmount;
                _lastDashTime = Time.time; // Mark when dash started
                Vector2 input = Controller.Input.move.GetValue();
                
                // Dashes to were the player is facing when no direction is given
                if (input.sqrMagnitude < 0.1f)
                {
                    float facingDir = Controller.FacingLeft ? -1f : 1f;
                    input = new Vector2(facingDir, 0);

                }
                
                _dashDirection = input.normalized; // makes input sick to the 8 directions 
                motor.Rb.linearVelocity = _dashDirection * _dashSpeed;

                StartCoroutine(DashRoutine());
            }
        }

        private IEnumerator DashRoutine()
        {
            _isDashing = true;
            _fighterController.UpdateState(FighterController.States.Dash);
            
            motor.Rb.linearDamping = 0f; // no drag during dash
            motor.Rb.gravityScale = 0f; // no gravity during dash
            motor.IgnoreFallGravity = true; // tells other classes that there is no gravity

            yield return new WaitForSeconds(_dashDuration);

            EndDash();

        }

        private void EndDash()
        {
            _isDashing = false;
            _fighterController.UpdateState(FighterController.States.Movement);

            motor.Rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            if (motor.Grounded && Mathf.Abs(_dashDirection.y) < 0.1f)
            {
                motor.Rb.linearVelocity = new Vector2(_dashDirection.x * _dashSpeed, 0f);
            }
            else
            {
                motor.Rb.linearVelocity = Vector2.zero;
            }

            motor.Rb.linearDamping = _movementModule.FindDragForce();
            StartCoroutine(dashFloat(_floatAfterDashTime));
        }
        
        private IEnumerator dashFloat(float duration)
        {
            yield return new WaitForSeconds(duration);
            motor.IgnoreFallGravity = false;
            motor.Rb.linearDamping = _defaltGravity;
        }

        private void TryRestoreDash()
        {
            if (!_isDashing && motor.Grounded && Time.time >= _lastDashTime + _dashCooldown)
            {
                _dashAmount = 1;
            }
        }

        public void EndDashEarly()
        {
            if (!_isDashing) return; 

            StopAllCoroutines(); // Stop DashRoutine if running
            EndDash(); 
        }
    }
}