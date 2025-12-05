using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance {get; private set;}
    
    public AudioSource backgroundMusic;
    public AudioSource playerSFX1;
    public AudioSource playerSFX2;
    public AudioSource worldSFXSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        if (instance != null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySoundFromSource(AudioSource audioSource, string soundName)
    {
        
    }

    private IEnumerator Fade(AudioSource audioSource, float change, float duration, bool isDegressing)
    {
        if (isDegressing)
            change *= -1;
        while (audioSource.volume > 0)
        {
            audioSource.volume += change;
            yield return new WaitForSeconds(duration);
        }
    }
    
    
    
}
