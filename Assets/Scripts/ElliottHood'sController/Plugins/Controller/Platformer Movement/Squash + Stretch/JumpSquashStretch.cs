using DG.Tweening;
using UnityEngine;
using ControllerSystem.Platformer2D;

public class JumpSquashStretch : MonoBehaviour
{ 
    [SerializeField] private PlatformerMotor _platformerMotor;
    
    [Header("Jumping")]
    [SerializeField] private PlatformerJumpModule _jumpModule;
    [SerializeField] private TweenSettings _jumpEffector;

    [Header("Landing")]
    [SerializeField] private TweenSettings _landEffector;

    [Header("Crouching")]
    [SerializeField] private PlatformerCrouchModule _crouchModule;
    [SerializeField] private TweenSettings _crouchEffector;
    [SerializeField] private TweenSettings _uncrouchEffector;

    private bool _crouchSquishApplied;
    private Vector3 _defaultScale;
    private Tween _currentTween;

    [System.Serializable]
    private class TweenSettings
    {
        public float intensity = 0.3f;
        public float duration = 0.3f;
        public Ease ease = Ease.OutQuad;
    }
    
    private void Awake()
    {
        _defaultScale = transform.localScale;

        _platformerMotor.OnLand += PlatformerMotorOnLand;
        _jumpModule.OnJump += PlatformMovement_OnJump;
        _jumpModule.OnDoubleJump += PlatformMovement_OnJump;
    }

    private void FixedUpdate()
    {
        if (_crouchModule == null)
            return;
        if (_crouchModule.Crouching)
        {
            if (_crouchSquishApplied) return;
            _crouchSquishApplied = true;
            PlaySquash(_crouchEffector, squashDown: true);
        }
        else
        {
            if (!_crouchSquishApplied) return;
            _crouchSquishApplied = false;
            PlaySquash(_uncrouchEffector, squashDown: false);
        }
    }

    private void PlatformerMotorOnLand(PositionInfo e)
    {
        PlaySquash(_landEffector, squashDown: true);
    }

    private void PlatformMovement_OnJump(PositionInfo e)
    {
        PlaySquash(_jumpEffector, squashDown: false);
    }

    private void PlaySquash(TweenSettings settings, bool squashDown)
    {
        _currentTween?.Kill();

        Vector3 squashScale = new Vector3(
            _defaultScale.x * (squashDown ? 1 + settings.intensity : 1 - settings.intensity),
            _defaultScale.y * (squashDown ? 1 - settings.intensity : 1 + settings.intensity),
            _defaultScale.z
        );

        _currentTween = transform.DOScale(squashScale, settings.duration * 0.5f)
            .SetEase(settings.ease)
            .OnComplete(() =>
            {
                _currentTween = transform.DOScale(_defaultScale, settings.duration * 0.5f)
                    .SetEase(settings.ease);
            });
    }
}
