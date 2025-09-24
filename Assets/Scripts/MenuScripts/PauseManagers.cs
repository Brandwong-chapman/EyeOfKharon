using System;
using NUnit.Framework;
using UnityEngine;

public class PauseManager : ChangePanel
{
    private bool isPaused = false;
    

    private void FixedUpdate()
    {
        TogglePause();
    }

    public void TogglePause()
    {
        if ((Input.GetKeyDown(KeyCode.Escape)))
        {
            isPaused = !isPaused;
            
            if (isPaused)
                GameTimeScale.Pause();
            else
                GameTimeScale.Resume();
            
        }
        GameEvent.PauseChanged(isPaused);
    }
}