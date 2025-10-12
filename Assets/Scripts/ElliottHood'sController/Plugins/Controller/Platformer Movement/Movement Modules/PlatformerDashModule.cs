using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ControllerSystem.Platformer2D
{
    public class PlatformerDashModule : PlatformerMotorModule
    {
        [Header("Dash Settings")]
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
        [SerializeField] private float _waveDashSpeed = 15f; // magnitude of impulse
        [SerializeField, Range(0f, 90f)] private float _waveDashAngle = 20f; // degrees away from surface
        [SerializeField] private float _waveDashWindow = 0.2f; // seconds player can trigger wavedash


        private bool _waveDashAvailable = false;
        private float _waveDashWindowTimer = 0f;
        private Vector2 _waveDashDirection;
        private bool _isWaveDashing = false;
        
        

        private float _bufferTimer = 0f;
        private bool _isDashing;
        private float _lastDashTime;
        private float _defaltGravity;
        private Vector2 _dashDirection;

        private Vector2 _lastSurfaceNormal = Vector2.up;

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

            UpdateWaveDashWindow();
            TryWaveDash();

            TryRestoreDash();
        }

        #region Input Buffer

        private void RecordInputs()
        {
            Vector2 input = Controller.Input.move.GetValue();

            if (input.sqrMagnitude > 0.05f)
            {
                _inputBuffer.Enqueue(input.normalized);
                _bufferTimer += Time.deltaTime;

                if (_bufferTimer > _inputBufferWindow)
                {
                    _inputBuffer.Dequeue();
                    _bufferTimer = 0f;
                }
            }
        }

        private Vector2 ResolveBufferedDirection()
        {
            Vector2 result = Vector2.zero;
            List<Vector2> list = new List<Vector2>(_inputBuffer);

            for (int i = list.Count - 1; i >= 0; i--)
            {
                Vector2 dir = list[i];

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
            if (_dashAmount != 0 && !_isDashing && Controller.Input.dash.GetHeld())
            {
                --_dashAmount;
                _lastDashTime = Time.time;
                StartCoroutine(DashRoutine());
            }
        }

        private IEnumerator DashRoutine()
        {
            _isDashing = true;
            _fighterController.UpdateState(FighterController.States.Dash);

            yield return new WaitForSeconds(_dashPauseTime);

            _dashDirection = ResolveBufferedDirection();

            if (_dashDirection.sqrMagnitude < 0.1f)
            {
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
            yield return new WaitForSeconds(duration);
            motor.IgnoreFallGravity = false;
            motor.Rb.linearDamping = _defaltGravity;
        }

        private void TryRestoreDash()
        {
            if (!_isDashing && motor.Grounded && Time.time >= _lastDashTime + _dashCooldown)
                _dashAmount = 1;
        }

        public void EndDashEarly()
        {
            if (!_isDashing) return;
            StopAllCoroutines();
            EndDash();
        }

        #endregion

        #region WaveDash
        
        private void TryWaveDash()
        {
            bool diagonalDash = Mathf.Abs(_dashDirection.x) > 0.1f && Mathf.Abs(_dashDirection.y) > 0.1f;
            if (_waveDashAvailable && diagonalDash && Controller.Input.jump.TryUseBuffer())
            {
                ApplyWaveDash();
                _waveDashAvailable = false;
            }
        }

        private bool IsTouchingWall()
        {
            return Mathf.Abs(_lastSurfaceNormal.x) > 0.5f;
        }

        #endregion

        #region Collision Normal Tracking

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.contactCount > 0)
            {
                _lastSurfaceNormal = collision.GetContact(0).normal;
                if (_isDashing)
                    _waveDashAvailable = true;
                _waveDashWindowTimer = _waveDashWindow;
            }
        }
        
        private Vector2 GetSurfaceNormal()
        {
            return _lastSurfaceNormal;
        }
        
        private void UpdateWaveDashWindow()
        {
            if (_waveDashWindowTimer > 0f)
                _waveDashWindowTimer -= Time.deltaTime;
            else
                _waveDashAvailable = false;
        }

        private void ApplyWaveDash()
        {
            // Determine tangent along the surface
            Vector2 tangent = new Vector2(-_lastSurfaceNormal.y, _lastSurfaceNormal.x);

            // Determine which side of the tangent to use based on player input / facing
            if (Vector2.Dot(tangent, _dashDirection) < 0)
                tangent = -tangent;
            
            float angleSign = Mathf.Sign(Vector3.Cross(tangent, _dashDirection).z); // 2D cross product
            float angleRad = _waveDashAngle * Mathf.Deg2Rad * angleSign;
            // Rotate tangent by angle away from surface
            Vector2 wavedashDir = RotateVector(tangent, -angleRad);
            // Apply instantaneous velocity
            motor.Rb.linearVelocity = wavedashDir.normalized * _waveDashSpeed;
        }
        
        private Vector2 RotateVector(Vector2 v, float angleRad)
        {
            float cos = Mathf.Cos(angleRad);
            float sin = Mathf.Sin(angleRad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);        }

        #endregion
    }
}
