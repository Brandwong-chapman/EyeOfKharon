using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ControllerSystem.Platformer2D
{
    // Handles dash mechanics for a 2D platformer character
    public class PlatformerDashModule : PlatformerMotorModule
    {
        #region Dash Settings
        [Header("Dash Settings")]
        [SerializeField] private float _dashSpeed = 15f;          
        [SerializeField] private float _dashDuration = 0.2f;      
        [SerializeField] private float _dashCooldown = 0.5f;      
        [SerializeField] private int _dashAmount = 1;  // Number of dashes player can perform before touching ground
        [SerializeField] private float _floatAfterDashTime = 0.2f; // Short float time after dash to ease movement
        #endregion

        #region Buffered Dash Settings
        [Header("Buffered Dash Settings")]
        [SerializeField] private float _dashPauseTime = 0.08f;    // Short pause before actual dash starts
        [SerializeField] private float _inputBufferWindow = 0.25f; // How long input is stored in buffer
        private Queue<Vector2> _inputBuffer = new Queue<Vector2>(); // Stores recent movement inputs for buffered dash
        #endregion

        #region Wave Dash Settings
        [Header("Wave Dash Settings")]
        [SerializeField] private float _waveDashSpeed = 15f;       
        [SerializeField, Range(0f, 90f)] private float _waveDashAngle = 20f; // Angle away from wall or surface
        [SerializeField] private float _waveDashWindow = 0.2f;     // Time window after dash to trigger wavedash
        [SerializeField] private float _waveDashCooldown = 0.2f;   // Cooldown between wavedashes
        private bool _waveDashAvailable = false;                  // Whether wavedash is currently available
        private float _waveDashWindowTimer = 0f;                  // Timer for wavedash availability
        private Vector2 _waveDashDirection;                       // Direction for wavedash
        private bool _isWaveDashing = false;                      // Whether currently performing wavedash
        #endregion

        // Internal dash tracking
        private float _bufferTimer = 0f;       // Timer for input buffer
        private bool _isDashing;               // True while performing a dash
        private float _lastDashTime;           // Time of last dash
        private float _defaltGravity;          // Default gravity to restore after dash
        private Vector2 _dashDirection;        // Current dash direction
        private Vector2 _lastSurfaceNormal = Vector2.up; // Normal of the surface player last collided with

        // References to other movement components
        private PlatformerHorizontalMovementModule _movementModule;
        private FighterController _fighterController;
        private PlatformerJumpModule _jumpController;


        private void Awake()
        {
            _movementModule = GetComponent<PlatformerHorizontalMovementModule>();
            _jumpController = GetComponent<PlatformerJumpModule>();
            _fighterController = GetComponent<FighterController>();
        }

        public override void Initialize(PlatformerMotor newMotor)
        {
            base.Initialize(newMotor);
            _movementModule = GetComponent<PlatformerHorizontalMovementModule>();
            _fighterController = GetComponent<FighterController>();
            _jumpController = GetComponent<PlatformerJumpModule>();
            _defaltGravity = motor.Rb.linearDamping; // Store default damping to restore after dash

        }

        public override void HandleMovement()
        {
            RecordInputs();             
            HandleDash();              
            TryStartDash();             
            UpdateWaveDashWindow();   
            TryWaveDash();              
            TryRestoreDash();           
        }

        #region Input Buffer
        private void RecordInputs()
        {
            // Records player input into a buffer for buffered dash
            Vector2 input = Controller.Input.move.GetValue();
            if (input.sqrMagnitude > 0.05f)
            {
                _inputBuffer.Enqueue(input.normalized);
                _bufferTimer += Time.deltaTime;

                // Remove oldest input if buffer exceeds window
                if (_bufferTimer > _inputBufferWindow)
                {
                    _inputBuffer.Dequeue();
                    _bufferTimer = 0f;
                }
            }
        }

        private Vector2 ResolveBufferedDirection()
        {
            // Calculate dash direction based on buffered input
            Vector2 result = Vector2.zero;
            List<Vector2> list = new List<Vector2>(_inputBuffer);

            for (int i = list.Count - 1; i >= 0; i--)
            {
                Vector2 dir = list[i];

                // Skip if direction opposes already accumulated result
                if ((result.x != 0 && Mathf.Sign(dir.x) != Mathf.Sign(result.x)) ||
                    (result.y != 0 && Mathf.Sign(dir.y) != Mathf.Sign(result.y)))
                    continue;

                result += dir;

                if (result.magnitude >= 1.5f) break;
            }

            return result.normalized;
        }
        #endregion

        #region Dash
        private void HandleDash()
        {
            // Apply dash velocity while dashing
            if (_isDashing)
            {
                if (motor.Grounded && Mathf.Abs(_dashDirection.y) < 0.1f)
                    motor.Rb.linearVelocity = new Vector2(_dashDirection.x * _dashSpeed, 0f);
                else
                    motor.Rb.linearVelocity = _dashDirection * _dashSpeed;
            }
           
        }

        private void TryStartDash()
        {
            // Check if player can start dash
            if (_dashAmount != 0 && !_isDashing && Controller.Input.dash.GetHeld())
            {
                if (motor.Grounded && _dashDirection.y < -.1)
                {
                    
                }
                else
                {
                    --_dashAmount;
                    _lastDashTime = Time.time;
                    StartCoroutine(DashRoutine());
                }
            } 
        }

        private IEnumerator DashRoutine()
        {
            // Performs dash with pause and gravity disabling
            _isDashing = true;
            _fighterController.UpdateState(FighterController.States.Dash);

            yield return new WaitForSeconds(_dashPauseTime);

            _dashDirection = ResolveBufferedDirection();

            if (_dashDirection.sqrMagnitude < 0.1f)
            {
                // Default dash direction if no input
                float facingDir = Controller.FacingLeft ? -1f : 1f;
                _dashDirection = new Vector2(facingDir, 0f);
            }

            motor.Rb.linearDamping = 0f;
            motor.Rb.gravityScale = 0f;
            motor.IgnoreFallGravity = true;

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
            // Reset player state after dash
            _isDashing = false;
            _fighterController.UpdateState(FighterController.States.Movement);
            motor.Rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            if (motor.Grounded && Mathf.Abs(_dashDirection.y) < 0.1f)
                motor.Rb.linearVelocity = new Vector2(_dashDirection.x * _dashSpeed, 0f);
            else
                motor.Rb.linearVelocity = Vector2.zero;

            motor.Rb.linearDamping = _movementModule.FindDragForce();
            StartCoroutine(dashFloat(_floatAfterDashTime));
        }

        private IEnumerator dashFloat(float duration)
        {
            // Brief float period after dash
            yield return new WaitForSeconds(duration);
            motor.IgnoreFallGravity = false;
            motor.Rb.linearDamping = _defaltGravity;
        }

        private void TryRestoreDash()
        {
            // Restore dash if grounded and cooldown elapsed
            if (!_isDashing && motor.Grounded && Time.time >= _lastDashTime + _dashCooldown)
                _dashAmount = 1;
        }

        public void EndDashEarly()
        {
            // Stop dash immediately
            if (!_isDashing) return;
            StopAllCoroutines();
            EndDash();
        }
        #endregion

        #region WaveDash
        private void TryWaveDash()
        {
            // Attempt wavedash if diagonal dash, jump input, and available
            bool diagonalDash = Mathf.Abs(_dashDirection.x) > 0.1f && Mathf.Abs(_dashDirection.y) > 0.1f;
            
            if (_waveDashAvailable && diagonalDash && Controller.Input.jump.TryUseBuffer() && !_isWaveDashing)
            {
                StartCoroutine(WaveDashCooldown());
                ApplyWaveDash();
                _waveDashAvailable = false;
            }
        }

        IEnumerator WaveDashCooldown()
        {
            _waveDashWindowTimer = 0;
            _isWaveDashing = true;
            yield return new WaitForSeconds(_waveDashCooldown);
            _isWaveDashing = false;
        }

        private bool IsTouchingWall()
        {
            // Returns true if last surface normal indicates wall
            return Mathf.Abs(_lastSurfaceNormal.x) > 0.5f;
        }
        #endregion

        #region Collision Normal Tracking
        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Track surface normals to calculate wave dash direction
            if (collision.contactCount > 0)
            {
                _lastSurfaceNormal = collision.GetContact(0).normal;

                // If dashing when hitting surface, wavedash becomes available
                if (_isDashing)
                {
                    _waveDashAvailable = true;
                    _waveDashWindowTimer = _waveDashWindow;
                }
            }
        }
        
        private void UpdateWaveDashWindow()
        {
            // Countdown wavedash window
            if (_waveDashWindowTimer > 0f)
                _waveDashWindowTimer -= Time.deltaTime;
            else
                _waveDashAvailable = false;
        }

        private void ApplyWaveDash()
        {
            _jumpController.LockJumpForDuration(_waveDashCooldown);
            // Calculate wavedash vector along surface tangent
            Vector2 tangent = new Vector2(-_lastSurfaceNormal.y, _lastSurfaceNormal.x);

            // Flip tangent if player moving in opposite direction
            if (Vector2.Dot(tangent, _dashDirection) < 0)
                tangent = -tangent;

            float angleSign = Mathf.Sign(Vector3.Cross(tangent, _dashDirection).z); // 2D cross product
            float angleRad = _waveDashAngle * Mathf.Deg2Rad * angleSign;

            Vector2 wavedashDir = RotateVector(tangent, -angleRad);
            motor.Rb.linearVelocity = wavedashDir.normalized * _waveDashSpeed;
        }

        private Vector2 RotateVector(Vector2 v, float angleRad)
        {
            // Rotate 2D vector by angle in radians
            float cos = Mathf.Cos(angleRad);
            float sin = Mathf.Sin(angleRad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }
        #endregion
    }
}
