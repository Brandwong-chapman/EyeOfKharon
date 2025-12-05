using System;
using UnityEngine;

public static class GameEvents
{
    public static event Action<PauseManager.GameState> OnGameStateChanged;
    
    public static void HandleGameStateChanged(PauseManager.GameState currentGameState)
    {
        OnGameStateChanged?.Invoke(currentGameState);
    }
}
