using InputManagement;
using UnityEngine;

public class Timer
{
    private float duration;
    private float startTime = float.MaxValue;

    public bool finished => startTime + duration <= Time.time;
    public float progress => (Time.time - startTime) / duration;
    public float timeElapsed => Mathf.Max(Time.time - startTime, 0);
    public float timeUntilCompletion => Mathf.Max(duration - (Time.time - startTime), 0);

    public Timer(float duration = 1)
    {
        this.duration = duration;
        ButtonInputProvider x = new ButtonInputProvider();
    }

    public void Start(float duration)
    {
        this.duration = duration;
        startTime = Time.time;
    }

    public void End()
    {
        duration = Time.time - startTime - 0.01f;
    }
}
