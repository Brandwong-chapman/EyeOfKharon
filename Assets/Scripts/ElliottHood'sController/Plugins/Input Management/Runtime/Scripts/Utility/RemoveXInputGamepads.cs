using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Prevents a bug with XInput and controllers, in which multiple players get spawned and controlled by one controller
/// </summary>
[RequireComponent(typeof(PlayerInputManager))]
public class RemoveXInputGamepads : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<PlayerInputManager>().onPlayerJoined += _playerInputManager_onPlayerJoined;
    }

    private void _playerInputManager_onPlayerJoined(PlayerInput obj)
    {
        PlayerInput_onControlsChanged(obj);
        obj.onControlsChanged += PlayerInput_onControlsChanged;
    }

    /// <summary>
    /// 
    /// Ugly workaround to prevent input being copied with an xinput controller
    /// 
    /// https://forum.unity.com/threads/dualshock-4-ps4-gamepad-acts-as-2-seperate-devices-at-the-same-time.1399474/#post-9440450
    /// 
    /// https://forum.unity.com/threads/switch-pro-controller-is-registered-as-two-controllers-at-the-same-time-mixed-a-b-and-x-y-events.1441414/
    /// </summary>
    private void PlayerInput_onControlsChanged(PlayerInput obj)
    {
        Gamepad gamepad = obj.GetDevice<Gamepad>();
        if (gamepad is UnityEngine.InputSystem.Switch.SwitchProControllerHID)
        {
            foreach (var item in Gamepad.all)
            {
                if ((item is UnityEngine.InputSystem.XInput.XInputController) && (Math.Abs(item.lastUpdateTime - gamepad.lastUpdateTime) < 0.1))
                {
                    Debug.Log($"Switch Pro controller detected and a copy of XInput was active at almost the same time. Disabling XInput device. `{gamepad}`; `{item}`");
                    InputSystem.DisableDevice(item);
                }
            }
        }
    }
}
