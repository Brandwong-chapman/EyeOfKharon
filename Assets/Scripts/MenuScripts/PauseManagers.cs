using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : ChangePanel
{
    
    public enum GameState { Paused, HalfTime, Normal }
    
    private GameState CurrentGameState;
    private GameState OldGameState;
    
    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }
    
    public void ChangeGameState(GameState newGameState)
    {
        if (CurrentGameState == newGameState)
            return;
        OldGameState = CurrentGameState;
        CurrentGameState = newGameState;
        switch (newGameState)
        {
            case GameState.Paused:
                GameTimeScale.Pause();
                break;
            case GameState.Normal:
                GameTimeScale.Resume();
                break;
            default:
                GameTimeScale.SlowTime(0.5f);
                break;
        }
    }

    public void TogglePause()
    {
            if (CurrentGameState != GameState.Paused)
                ChangeGameState(GameState.Paused);
            else
                ChangeGameState(OldGameState);
            GameEvents.HandleGameStateChanged(CurrentGameState);
    }
        
}
