using System;
using UnityEngine;

public static class GameEvents
{
    public static event Action<bool> OnPauseChanged;

    public static void PauseChanged(bool isPaused)
    {
        OnPauseChanged?.Invoke(isPaused);
    }
}
