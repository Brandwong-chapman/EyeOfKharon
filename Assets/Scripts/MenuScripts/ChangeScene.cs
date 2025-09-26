using UnityEngine;
using UnityEngine.SceneManagement;
public class ChangeScene : MonoBehaviour
{
    public static int levelAmount;
    private static int currentLevel =-1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public void Quit()
    {
        //Quits the game
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); 
#endif
    }
    
    
    public void LoadMainMenu()
    {
        currentLevel = -1;
        SceneManager.LoadScene("Menu");
    }
    
    public void LoadDebugRoom()
    {
        currentLevel = 0;
        SceneManager.LoadScene("Test Level");
    }
    
    public void LoadLevel(int level)
    {
        if (level < levelAmount & level > 0)
        {
            currentLevel = level;
            SceneManager.LoadScene($"Level{level}");
        }else
            LoadMainMenu();
    }
    
    public void LoadNextLevel()
    {
        ++levelAmount;
        LoadLevel(levelAmount);
    }
}
