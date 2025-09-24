using InputManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ControllerSystem {

    /// <summary>
    /// The general controller base code shared by each type of controller
    /// </summary>
    [SelectionBase]
    public class EntityController : MonoBehaviour
    {
        public InputManager InputManager;
        public InputState Input => InputManager.Input;
        public virtual bool CanAnimateFlip => true;

        #region Input Shortcuts That Rely On Controller Position / Orientation

        [SerializeField] private Transform _bodyCenter;

        public bool FacingLeft;
        public Vector2 BodyCenter => _bodyCenter.position;
        private bool UsingKeyboardAndMouse => InputManager.PlayerInput.currentControlScheme == "Keyboard&Mouse";
        public Vector2 NormalizedAimDirection => NormalizedAimDir();
        public Vector2 NormalizedMoveDirection => NormalizeInputDirection(InputManager.Input.move.GetValue());
        public bool InputtingHorizontalMovement => Mathf.Abs(InputManager.Input.move.GetValue(0).x) > 0.5f;
        
        private Vector2 NormalizedAimDir()
        {
            if (UsingKeyboardAndMouse)
            {
                // Use mouse and player position if on mouse and keyboard
                Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
                Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

                return ((Vector2)mouseWorldPosition - BodyCenter).normalized;
            }
            else
            {
                Vector2 aimDir = InputManager.Input.aim.GetValue();
                if (aimDir != Vector2.zero)
                {
                    // Use the aim input if the player is specifically aiming in a direction that they are not moving
                    return InputManager.Input.aim.GetValue().normalized;
                }
                else
                {
                    // Fallback to the move direction
                    return NormalizedMoveDirection;
                }
            }
        }

        private Vector2 NormalizeInputDirection(Vector2 aimDir)
        {
            // Can't be 0
            if (aimDir == Vector2.zero)
            {
                aimDir = new Vector2(FacingLeft ? -1 : 1, 0);
            }
            return aimDir.normalized;
        }

        #endregion

    }
}