using System;
using UnityEngine;

public static  class GameEvent
{
    public static event Action<bool> OnPauseChanged;

    public static void PauseChanged(bool isPaused)
    {
        OnPauseChanged?.Invoke(isPaused);
    }
}
