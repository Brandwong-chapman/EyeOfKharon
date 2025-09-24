using DamageSystem;
using ControllerSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An EntityController with a RigidBody2D that can take Damage and Knockback
///
/// On getting hit:
/// - Player enters HitStop, and the knockback is stored
/// - Once HitStop ends, the knockback is released and the player moves
///     - NOTE: THE KNOCKBACK SYSTEM IS RUDEMENTARY AND WILL LIKELY NEED TWEAKS.
///     - IT ALSO MAY NOT BE IMPORTANT FOR YOUR GAME
///     - Right now it works by overriding the players velocity, decaying until it disappears completely
/// 
/// Hitstop: a moment where the attacker and player freeze for a moment
/// - Makes hits feel more impactful
/// - In single player games, this can be simplified by setting the TimeScale to 0 for a brief moment
/// 
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class FighterController : EntityController
{
    [SerializeField] private Motor _motor;
    public Rigidbody2D Rb { get; private set; }
    
    protected virtual void FixedUpdate()
    {
        HandleHitstop();

        if (InMovementState())
        {
            _motor.HandleLocalControllerMovement();
        }
    }

    public virtual bool InMovementState()
    {
        return CurrentState == States.Movement && !_inHitstop;
    }

    protected virtual void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
        ForceUpdateState(States.Movement);
    }

    #region State Machine

    public enum States
    {
        Movement,
        Ability,
        Dead,
    }

    public class StateUpdateInfo
    {
        public States OldState;
        public States NewState;
    }

    public event Action<StateUpdateInfo> OnStateChanged;
    public States CurrentState { get; private set; }

    public override bool CanAnimateFlip => CurrentState == States.Movement;

    public void UpdateState(States newState)
    {
        if (CurrentState == newState)
            return;
        ForceUpdateState(newState);
    }

    private void ForceUpdateState(States newState)
    {
        States oldState = CurrentState;
        CurrentState = newState;

        OnStateChanged?.Invoke(new StateUpdateInfo
        {
            OldState = oldState,
            NewState = newState
        });

        UpdateMotorEnabledState(newState);
    }

    private void UpdateMotorEnabledState(States newState)
    {
        _motor.enabled = newState == States.Movement;
    }

    #endregion


    #region Hurtbox

    public Hurtbox Hurtbox;
    private Vector2 _storedHitstopVelocity;

    protected virtual void Start()
    {
        Hurtbox.OnHit += Hurtbox_OnHit;
        Hurtbox.OnDeath += Hurtbox_OnDeath;
    }

    private void OnDestroy()
    {
        if (Hurtbox != null)
        {
            Hurtbox.OnHit -= Hurtbox_OnHit;
            Hurtbox.OnDeath -= Hurtbox_OnDeath;
        }
    }

    private void Hurtbox_OnDeath()
    {
        UpdateState(States.Dead);
    }

    [Serializable]
    public class KnockbackBehaviourSettings
    {
        public float KnockbackMultiplier = 1;
        [Tooltip("The amount of previous momentum to keep")]
        [Range(0, 1)]
        public float MomentumRetention = 0.8f;
        [Tooltip("The amount of vertical knockback thats added to every hit. Affected by the knockback multiplier")]
        public float MinimumUpVelocityAddition = 1f;
        [Tooltip("The duration of time over which the knockback gets added")]
        public float KnockbackDuration = 0.2f;
        public AnimationCurve KnockbackFalloffCurve;
        [Tooltip("The angle range from straight down in which players cannot cancel their vertical knockback with a jump")]
        public float SpikeAngleRange = 15;
        public float KnockbackStiffness = 0.9f;
    }

    public KnockbackBehaviourSettings KnockbackSettings;
    private Coroutine _knockbackCoroutine;
    private bool _yKnockbackInterrupted = false;

    private void Hurtbox_OnHit(HitEventInfo e)
    {
        EnterHitstop(e.hitInfo.hitStop, () =>
        {
            if (_knockbackCoroutine != null)
                StopCoroutine(_knockbackCoroutine);
            _knockbackCoroutine = StartCoroutine(ApplyKnockback(e.hitInfo.knockback));
        });
    }

    /// <summary>
    /// Unlocks your Y velocity in a knockback state
    /// </summary>
    public void InterruptYKnockback()
    {
        _yKnockbackInterrupted = true;
    }

    private IEnumerator ApplyKnockback(Vector2 inputtedKnockback)
    {
        _yKnockbackInterrupted = false;

        // Cancel some momentum
        Rb.linearVelocity *= KnockbackSettings.MomentumRetention;

        // Apply some upward force and the knockback multiplier
        Vector2 calculatedKnockback = inputtedKnockback + new Vector2(0, KnockbackSettings.MinimumUpVelocityAddition);
        calculatedKnockback *= KnockbackSettings.KnockbackMultiplier;

        // Continuously apply knockback for a duration
        float timer = KnockbackSettings.KnockbackDuration;
        while (timer > 0)
        {
            yield return new WaitForFixedUpdate();
            // Calculate a multiplier based on how long it's been since impact (value of the animation curve)
            float normalizedKnockbackTime = 1 - (timer / KnockbackSettings.KnockbackDuration);
            float knockbackTimeMultiplier = KnockbackSettings.KnockbackFalloffCurve.Evaluate(normalizedKnockbackTime);

            ApplyLerpKnockback(knockbackTimeMultiplier);

            // Apply the force (accounting for timescale)
            Rb.AddForce(calculatedKnockback * Time.fixedDeltaTime);

            timer -= Time.fixedDeltaTime;
        }
        yield break;

        // Knockback that works by lerping the player's velocity to a target velocity. 
        void ApplyLerpKnockback(float knockbackTimeMultiplier)
        {
            // Calculate desired velocity for this specific knockback
            Vector2 desiredVelocity = calculatedKnockback * knockbackTimeMultiplier;

            // Calculate how closely to have the player's momentum match the desired knockback
            float t = KnockbackSettings.KnockbackStiffness * knockbackTimeMultiplier * Time.fixedDeltaTime;

            // Weaker knockback will have a lower t value, and stronger knockback will have a higher t value
            t *= desiredVelocity.magnitude;

            // Optionally disable y knockback (Used so that jumps don't get eaten when hit horizontally)
            if (_yKnockbackInterrupted)
            {
                // If the player is being sent virtually straight down, don't allow them to jump out of it
                bool withinSpikeRange = Vector2.Angle(Vector2.down, inputtedKnockback) < KnockbackSettings.SpikeAngleRange;

                if (!withinSpikeRange)
                {
                    desiredVelocity.y = Rb.linearVelocity.y;
                }
            }

            // Lerp the players velocity towards the desired knockback velocity
            Rb.linearVelocity = Vector2.Lerp(Rb.linearVelocity, desiredVelocity, t);
        }
    }

    /// --- HITSTOP ---

    private bool _inHitstop;
    private readonly Timer _hitstopTimer = new Timer();
    private readonly List<Action> _onHitstopCompleted = new List<Action>();

    public void EnterHitstop(float duration, Action completed = null)
    {
        _inHitstop = true;
        _hitstopTimer.Start(duration);

        // Store velocity
        _storedHitstopVelocity = Rb.linearVelocity;

        // Store events (like knockback) for when hitstun ends
        if (completed != null)
            _onHitstopCompleted.Add(completed);
    }

    private void HandleHitstop()
    {
        if (!_inHitstop)
            return;

        if (_hitstopTimer.finished)
        {
            ExitHitstop();
        }
        else
        {
            // To keep the player from falling, you have to counteract gravity
            float counteractGravityVelocity = -Physics2D.gravity.y * Rb.gravityScale * Time.fixedDeltaTime;
            Rb.linearVelocity = new Vector2(0, counteractGravityVelocity);
        }
    }

    private void ExitHitstop()
    {
        // Add velocity back
        Rb.linearVelocity = _storedHitstopVelocity;

        // Invoke delayed events
        foreach (Action action in _onHitstopCompleted)
        {
            action.Invoke();
        }
        _onHitstopCompleted.Clear();
        _inHitstop = false;
    }

    #endregion
}
