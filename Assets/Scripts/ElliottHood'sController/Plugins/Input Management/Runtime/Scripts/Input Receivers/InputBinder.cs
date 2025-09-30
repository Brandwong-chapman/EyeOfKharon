using UnityEngine;
using UnityEngine.Events;

namespace InputManagement
{
    public class InputBinder : MonoBehaviour
    {
        [SerializeField] private ButtonInputReceiver inputReceiver;
        [SerializeField] private InputManager inputManager;
        [SerializeField] private ButtonType buttonType;
        [SerializeField] private UnityEvent OnExecuteInputUnityEvent;
        private enum ButtonType
        {
            jump,
            primary,
            secondary,
            interact,
            crouch,
            sprint,
            pause,
            dash
        }

        private void Start()
        {
            ButtonInputProvider buttonInput = null;
            switch (buttonType)
            {
                case ButtonType.jump:
                    buttonInput = inputManager.Input.jump;
                    break;
                case ButtonType.primary:
                    buttonInput = inputManager.Input.primary;
                    break;
                case ButtonType.secondary:
                    buttonInput = inputManager.Input.secondary;
                    break;
                case ButtonType.interact:
                    buttonInput = inputManager.Input.interact;
                    break;
                case ButtonType.crouch:
                    buttonInput = inputManager.Input.crouch;
                    break;
                case ButtonType.sprint:
                    buttonInput = inputManager.Input.sprint;
                    break;
                case ButtonType.pause:
                    buttonInput = inputManager.Input.pause;
                    break;
                case ButtonType.dash:
                    buttonInput = inputManager.Input.dash;
                    break;
                default:
                    Debug.Log("That input type is not yet supported.");
                    break;
            }

            buttonInput.AddInputReceiver(inputReceiver);
            inputReceiver.OnExecuteInput += InputReceiver_OnExecuteInput;
        }

        private void InputReceiver_OnExecuteInput(object sender, System.EventArgs e)
        {
            OnExecuteInputUnityEvent.Invoke();
        }
    }
}