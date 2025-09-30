using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputManagement
{
    /// <summary>
    /// Attached PlayerInput component should use UnityEvents, and each input action should correspond to one of the functions in this script
    /// 
    /// In Edit > Project Settings > Script Execution Order, the value for this script should be negative, so that input is obtained before other scripts run
    /// 
    /// When Creating A New Input:
    /// - Add mapping in inputActions
    /// - Add corresponding variable in FrameInput
    /// - Add new inputAction function in the InputActions region of this script
    /// - Add C# Event from PlayerInput component to the new function in the scenne view
    /// 
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    [DefaultExecutionOrder(-100)]
    public class InputManager : MonoBehaviour
    {
        public InputState Input => inputEnabled ? input : null;
        private InputState input = new InputState();

        #region PlayerInput

        [SerializeField] private PlayerInput playerInput;
        public PlayerInput PlayerInput => playerInput;

        private void OnValidate()
        {
            GetComponent<PlayerInput>().notificationBehavior = PlayerNotifications.InvokeUnityEvents;
        }

        #endregion


        #region Enabling / Disabling

        private bool inputEnabled = true;

        public void SetEnabled(bool enabled)
        {
            inputEnabled = enabled;
        }

        #endregion


        #region Input Mode

        private const string PLAYER_ACTION_MAP = "Player";
        private const string UI_ACTION_MAP = "UI";

        public enum InputMode
        {
            Player,
            UI
        }

        public void SwitchInputMode(InputMode inputMode)
        {
            switch (inputMode)
            {
                case InputMode.Player:
                    PlayerInput.SwitchCurrentActionMap(PLAYER_ACTION_MAP);
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    break;
                case InputMode.UI:
                    PlayerInput.SwitchCurrentActionMap(UI_ACTION_MAP);
                    Cursor.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    break;
                default:
                    break;
            }
        }

        #endregion


        #region Input Actions

        /// <summary>
        /// Movement / aim callback structure
        /// </summary>
        /// <param name="context"></param>
        public void Move(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                input.move.SetValue(context.ReadValue<Vector2>());
            }
            else if (context.canceled)
            {
                input.move.SetValue(Vector2.zero);
            }
        }
        public void Aim(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                input.aim.SetValue(context.ReadValue<Vector2>());
            }
            else if (context.canceled)
            {
                input.aim.SetValue(Vector2.zero);
            }
        }


        /// <summary>
        /// Copy paste this code for more input actions
        /// </summary>
        /// <param name="context"></param>

        public void Jump(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                input.jump.OnPress();
            }
            else if (context.canceled)
            {
                input.jump.OnRelease();
            }
        }

        public void Primary(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                input.primary.OnPress();
            }
            else if (context.canceled)
            {
                input.primary.OnRelease();
            }
        }

        public void Secondary(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                input.secondary.OnPress();
            }
            else if (context.canceled)
            {
                input.secondary.OnRelease();
            }
        }

        public void Interact(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                input.interact.OnPress();
            }
            else if (context.canceled)
            {
                input.interact.OnRelease();
            }
        }

        public void Crouch(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                input.crouch.OnPress();
            }
            else if (context.canceled)
            {
                input.crouch.OnRelease();
            }
        }

        public void Sprint(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                input.sprint.OnPress();
            }
            else if (context.canceled)
            {
                input.sprint.OnRelease();
            }
        }

        public void Pause(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                input.pause.OnPress();
            }
            else if (context.canceled)
            {
                input.pause.OnRelease();
            }
        }
        
        public void Dash(InputAction.CallbackContext context)
        {
            Debug.Log($"[InputManager] Dash callback called. context.performed: {context.performed}");
            if (context.performed)
            {
                input.dash.OnPress();
            }
            else if (context.canceled)
            {
                input.dash.OnRelease();
            }
        }

        #endregion


        #region Controller Shake

        private Gamepad gamepad => PlayerInput.devices[0] as Gamepad;
        private Coroutine stoppingRumbleCoroutine;

        public void RumbleController(float lowFrequency = 0.5f, float highFrequency = 1.0f, float duration = 0.1f)
        {
            if (gamepad == null)
                return;

            gamepad.SetMotorSpeeds(lowFrequency, highFrequency);

            if (stoppingRumbleCoroutine != null)
                StopCoroutine(stoppingRumbleCoroutine);
            stoppingRumbleCoroutine = StartCoroutine(StopRumbleAfterTime(duration));
        }

        private IEnumerator StopRumbleAfterTime(float duration)
        {
            yield return new WaitForSecondsRealtime(duration);
            try
            {
                gamepad.SetMotorSpeeds(0, 0);
            }
            catch { }
        }

        #endregion

    }

}