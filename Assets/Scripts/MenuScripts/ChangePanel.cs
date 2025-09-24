using UnityEngine;

public class ChangePanel: MonoBehaviour{

    protected void Hide(GameObject screen)
    {
        screen.SetActive(false);
    }

    protected void Show(GameObject screen)
    {
        screen.SetActive(true);
    }
    
    protected void HideAllExeptFor (GameObject[] screens, GameObject[] remainingScreens)
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
