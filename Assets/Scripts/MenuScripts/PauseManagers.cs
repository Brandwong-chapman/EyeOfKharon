using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : ChangePanel
{
    private bool isPaused = false;
    

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
            isPaused = !isPaused;
            
            if (isPaused)
                GameTimeScale.Pause();
            else
                GameTimeScale.Resume();
            GameEvents.PauseChanged(isPaused);
        }
        
}
