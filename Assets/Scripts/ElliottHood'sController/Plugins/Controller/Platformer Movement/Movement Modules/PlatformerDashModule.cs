using System;
using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private float _dashDuration = 0.2f;
        [SerializeField] private float _dashCooldown = 0.5f;
        [SerializeField] private int _dashAmount = 1;
        [SerializeField] private float _floatAfterDashTime = 0.2f;
        [Header("Buffered Dash Settings")]
        [SerializeField] private float _dashPauseTime = 0.08f;     
        [SerializeField] private float _inputBufferWindow = 0.25f;   
        private Queue<Vector2> _inputBuffer = new Queue<Vector2>();
        [Header("Wave Dash Settings")]
        [SerializeField] private float _waveDashSpeed = 15f;

        private float _bufferTimer = 0f;
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
            RecordInputs();

            
            HandleDash();
            
            TryStartDash();

            TryRestoreDash();
        }
        private void RecordInputs()
        {
            Vector2 input = Controller.Input.move.GetValue();

            // Only record meaningful movement inputs
            if (input.sqrMagnitude > 0.05f)
            {
                _inputBuffer.Enqueue(input.normalized);
                _bufferTimer += Time.deltaTime;

                // Keep buffer window tight for responsiveness
                if (_bufferTimer > _inputBufferWindow)
                {
                    _inputBuffer.Dequeue();
                    _bufferTimer = 0f;
                }
            }
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
                StartCoroutine(DashRoutine());
            }
        }

        private IEnumerator DashRoutine()
        {
            _isDashing = true;
            _fighterController.UpdateState(FighterController.States.Dash);
            yield return new WaitForSeconds(_dashPauseTime); 
            _dashDirection = ResolveBufferedDirection(); // Determine dash direction using buffered inputs
            
            // Fallback to facing direction if no clear input
            if (_dashDirection.sqrMagnitude < 0.1f)
            {
                float facingDir = Controller.FacingLeft ? -1f : 1f;
                _dashDirection = new Vector2(facingDir, 0f);
            }
            
            // Start actual dash
            motor.Rb.linearDamping = 0f; // no drag during dash
            motor.Rb.gravityScale = 0f; // no gravity during dash
            motor.IgnoreFallGravity = true; // tells other classes that there is no gravity

            float dashStart = Time.time;
            while (Time.time < dashStart + _dashDuration)
            {
                HandleDash();
                yield return null;
            }
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
        private Vector2 ResolveBufferedDirection()
        {
            Vector2 result = Vector2.zero;
            List<Vector2> list = new List<Vector2>(_inputBuffer);

            // Start from most recent input backwards
            for (int i = list.Count - 1; i >= 0; i--)
            {
                Vector2 dir = list[i];

                // Skip conflicting directions (e.g., left vs right)
                if ((result.x != 0 && Mathf.Sign(dir.x) != Mathf.Sign(result.x)) ||
                    (result.y != 0 && Mathf.Sign(dir.y) != Mathf.Sign(result.y)))
                    continue;

                result += dir;
                if (result.magnitude >= 1.5f) break; 
            }

            return result.normalized;
        }
    }
}