using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

public class LevelUI : ChangePanel
{
    public GameObject PausePanel;
    public GameObject MainPauseMenuPanel;
    public GameObject ConcrolsPanel;

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
            AudioListener.pause = true; // mutes audio when AudioListener connected  
        }
        else
        {
            Hide(PausePanel);
            AudioListener.pause = false; // unmutes audio when AudioListener connected  
        }
        
    }
    
    public void ShowControlScreen()
    {
        Hide(MainPauseMenuPanel);
        Show(ConcrolsPanel);
    }
    
    public void ShowMainPauseMenuScreen()
        {
            Show(MainPauseMenuPanel);
            Hide(ConcrolsPanel);
        }
        
}
