using UnityEngine;

public static class ChangePanel{

    private static void Hide(GameObject screen)
    {
        screen.SetActive(false);
    }

    private static void Show(GameObject screen)
    {
        screen.SetActive(true);
    }

    public static void HideAll (GameObject[] screens)
    {
        for (int i = 0; i < (screens.Length); ++i)
        {
            Hide(screens[i]);
        }
    }
    
    public static void HideAllExeptFor (GameObject[] screens, GameObject[] remainingScreens)
    {
        DebugLogger.Log(LogChannel.UI, $"Hiding: {screens.Length} screens", LogLevel.Verbose);
        DebugLogger.Log(LogChannel.UI, $"Showing: {remainingScreens.Length} screens", LogLevel.Verbose);
        foreach (var screen in screens)
        {
            if (System.Array.Exists(remainingScreens, rs => rs == screen))
                Show(screen);
            else
                Hide(screen);
        }
    }
    
    
}
