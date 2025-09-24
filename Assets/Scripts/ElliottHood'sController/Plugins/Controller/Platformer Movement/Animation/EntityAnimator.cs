using ControllerSystem;
using UnityEngine;

/// <summary>
/// Basic animation support. Handles basic animations for a 2D controller with a motor
/// Uses the transform.localScale.x to determine if the entity is facing left or right
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public abstract class EntityAnimator : MonoBehaviour
{
    [SerializeField] protected Motor motor;
    [SerializeField] private Transform _flipAnchor;
    protected SpriteRenderer sr;
    protected Animator anim;
    private int _currentAnimState;

    protected virtual void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    public virtual bool CanAnimate()
    {
        try
        {
            FighterController controller = motor.Controller as FighterController;
            return controller != null && controller.CurrentState != FighterController.States.Ability;
        }
        catch
        {
            return false;
        }
    }

    private void Update()
    {
        if (!CanAnimate())
            return;

        Animate();
    }

    protected virtual void Animate()
    {
        HandleFlip();
    }

    private void HandleFlip()
    {
        if (!motor.Controller.CanAnimateFlip) return;
        if (!motor.Controller.InputtingHorizontalMovement) return;
            
        FlipToFaceDirection(motor.Controller.Input.move.GetValue().x);
    }

    public void FlipToFaceDirection(float direction)
    {
        if (direction == 0)
            return;

        _flipAnchor.localScale = new Vector3(direction < 0 ? -1 : 1, 1, 1);
        motor.Controller.FacingLeft = direction < 0;
    }

    public void SwitchAnimState(int animState)
    {
        if (_currentAnimState == animState)
            return;

        anim.CrossFadeInFixedTime(animState, 0, 0);
        _currentAnimState = animState;
    }
}
