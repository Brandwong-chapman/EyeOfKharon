using System;
using UnityEngine;

namespace ControllerSystem.Platformer2D
{
    public class PlatformerHorizontalMovementModule : PlatformerMotorModule
    {
        [Tooltip("Higher values makes changing directions feel less slippery")]
        [SerializeField] private float _turnAroundSpeedMultiplier = 1.5f;
        [SerializeField] private float _groundAccelerationTime = 0.1f;
        [SerializeField] private float _airAccelerationTime = 0.1f;
        [SerializeField] private float _groundSpeed = 25 / 3f;
        [SerializeField] private float _airSpeed = 25 / 3f;
        [Tooltip("Applies only when not inputting movement")]
        [SerializeField] private float _groundDrag = 20;
        [Tooltip("Applies only when not inputting movement")]
        [SerializeField] private float _airDrag = 5;

        private PlatformerCrouchModule _crouchModule;
        
        private void Awake()
        {
            _crouchModule = GetComponent<PlatformerCrouchModule>();
        }

        public override void HandleMovement()
        {
            if (Controller.InputtingHorizontalMovement && (_crouchModule == null || !_crouchModule.Crouching)) // Stop movement if crouching
            {
                float targetSpeed = motor.Grounded ? _groundSpeed : _airSpeed;
                float acceleration = motor.Grounded ? _groundAccelerationTime : _airAccelerationTime;

                float targetVelocity = targetSpeed * Mathf.Sign(Controller.Input.move.GetValue().x);
                float differenceInVelocity = targetVelocity - motor.Rb.linearVelocity.x;

                float horizontalForce = differenceInVelocity / acceleration;

                // Boost force when turning around
                if (!Mathf.Approximately(Mathf.Sign(Controller.Input.move.GetValue().x), Mathf.Sign(motor.Rb.linearVelocity.x)))
                {
                    horizontalForce *= _turnAroundSpeedMultiplier;
                }

                motor.Rb.AddForce(new Vector2(horizontalForce * Time.fixedDeltaTime, 0), ForceMode2D.Impulse);
            }
            else
            {
                float dragForce = motor.Rb.linearVelocity.x * -1;

                dragForce *= motor.Grounded ? _groundDrag : _airDrag;

                motor.Rb.AddForce(new Vector2(dragForce * Time.fixedDeltaTime, 0), ForceMode2D.Impulse);
            }
        }
    }
}