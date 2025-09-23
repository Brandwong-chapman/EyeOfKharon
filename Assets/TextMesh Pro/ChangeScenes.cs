using UnityEngine;

public class ChangeScenes : MonoBehaviour
{
   
    public void QuitGame()
    {
        //Quits the game
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); 
#endif
    }
    
}
