using InputManagement;
using UnityEngine;

public class DoubleTapInputReceiver : ButtonInputReceiver
{
    [SerializeField] private float doubleTapWindow = 0.3f;
    private float lastClickWindow = Mathf.NegativeInfinity;

    protected override void InterceptInput()
    {
        if (buttonInputProvider.GetPressedThisFrame())
        {
            if (lastClickWindow + doubleTapWindow > Time.time)
            {
                ResolveInput();
                lastClickWindow = Mathf.NegativeInfinity;
            }
            else
            {
                lastClickWindow = Time.time;
            }
        }
    }
}
