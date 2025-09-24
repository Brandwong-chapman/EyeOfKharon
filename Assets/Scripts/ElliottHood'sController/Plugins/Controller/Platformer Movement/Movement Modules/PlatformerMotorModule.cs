using UnityEngine;

namespace ControllerSystem.Platformer2D
{
    public abstract class PlatformerMotorModule : MonoBehaviour
    {
        protected PlatformerMotor motor;
        protected EntityController Controller => motor.Controller;
        
        public virtual void Initialize(PlatformerMotor newMotor)
        {
            motor = newMotor;
        }
        public abstract void HandleMovement();
    }
}