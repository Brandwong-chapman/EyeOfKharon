using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

public class LevelUI : ChangePanel
{
    public GameObject PausePanel;

    private void OnEnable()
    {
        GameEvents.OnPauseChanged += HandlePauseChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnPauseChanged -= HandlePauseChanged;
    }

    private void HandlePauseChanged(bool isPaused)
    {
        if (isPaused)
        {
            Show(PausePanel);
            Debug.Log("Game paused — muting audio");
            AudioListener.pause = true;
        }
        else
        {
            Hide(PausePanel);
            Debug.Log("Game resumed — unmuting audio");
            AudioListener.pause = false;
        }
    }

        
}
