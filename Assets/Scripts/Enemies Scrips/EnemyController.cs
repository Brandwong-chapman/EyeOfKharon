using System;
using System.Collections;
using System.Windows.Input;
using ControllerSystem.Platformer2D;
using ControllerSystem.Platformer2D.BasicAttack;
using Pathfinding;
using UnityEngine;

[RequireComponent(typeof(EnemyMotor), typeof(VisionSensor))]
public class EnemyController : MonoBehaviour, IDamageable
{
    [Header("References")]
    public EnemyData data;
    public FighterController fighterController;   // Player controller reference
    public Transform playerPosition => fighterController.transform;
    public Rigidbody2D Rigidbody2D => motor.GetComponent<Rigidbody2D>();
    [Header("AI Settings")]
    public float updateInterval = 0.25f; // Update AI logic every 0.25s
    
    public int FacingDirection { get; private set; } = 1;
    
    private float _updateTimer;
    public Seeker seeker { get; private set; }
    public enum EnemyState { Movement, Attack, Dead }
    public EnemyState CurrentEnemyState { get; private set; }

    public event Action<EnemyState, EnemyState> OnStateChanged;

    // --- PLAYER STATE PROPERTIES ---
    public bool PlayerIsAirborne => !fighterController.InMovementState();
    public bool PlayerIsDashing => fighterController.CurrentState == FighterController.States.Dash;
    public bool PlayerIsAttacking => fighterController.CurrentState == FighterController.States.Attack;

    public Vector2 PlayerCenter => fighterController.Hurtbox.transform.position;
    public float DistanceToPlayer => Vector2.Distance(transform.position, PlayerCenter);
    
    public event Action OnPlayerSpotted;
    public event Action<float> OnHealthChanged;
    public event Action OnDeath;
    public EnemyMotor motor { get; private set; }
    public VisionSensor vision { get; private set; }
    public ICommand currentCommand { get; private set; }

    public float currentHP;
    
    //pathfinding fields
    private Path path;
    private Vector2 moveTarget;
    
    private EnemyAttack currentAttack;
    
    private Vector3 _initialScale;

    private void Awake()
    {
        motor = GetComponent<EnemyMotor>();
        vision = GetComponent<VisionSensor>();
        _initialScale = transform.localScale; // store original scale
        currentHP = data.maxHP;

        if (data.pathfindingStrategy != null)
            data.pathfindingStrategy.Initialize(this);
    }

    private void SetFacing(float xMovement)
    {
        var oldFacingDirection = FacingDirection;
        FacingDirection = xMovement switch
        {
            > 0.05f => 1,
            < -0.05f => -1,
            _ => FacingDirection
        };

        if (FacingDirection != oldFacingDirection)
            transform.localScale = new Vector3(_initialScale.x * FacingDirection, _initialScale.y, _initialScale.z);
    }


    void Start()
    {
        StartCoroutine(AIUpdateLoop());
    }

    private IEnumerator AIUpdateLoop()
    {
        while (true)
        {
            if (CurrentEnemyState == EnemyState.Movement && data.pathfindingStrategy != null)
            {
                // Pick attack only once
                if (currentAttack == null)
                    currentAttack = data.attackSelector.ChooseAttack(this);

                // Determine move target
                moveTarget = currentAttack != null
                    ? currentAttack.GetDesiredPosition(fighterController.Hurtbox.transform.position, transform.position)
                    : (Vector2)fighterController.transform.position;

                // Move toward player
                Vector2 diff = moveTarget - (Vector2)transform.position;
                if (!data.movementAI || data.movementType != EnemyData.MovementType.Flying)
                    diff.y = 0;
                SetFacing(diff.x);

                if (diff.magnitude > 0.1f)
                    data.movementAI.Move(this, diff.normalized);
                else if (currentAttack != null)
                {
                    // In range → perform attack once
                    ChangeState(EnemyState.Attack);
                    currentAttack.Execute(this);
                    currentAttack = null;
                }
            }

            yield return new WaitForSeconds(updateInterval);
        }
    }

    public void ExecuteCommand(ICommand command)
    {
        currentCommand = command;
        currentCommand.Execute(this);
    }


    
    public void TakeDamage(float amount, Vector2 force)
    {
        currentHP -= amount;
        if (currentHP <= 0)
        {
            //stateMachine.ChangeState(new DeadState());
        }
    }
    public void ChangeState(EnemyState newEnemyState)
    {
        if (CurrentEnemyState == newEnemyState)
            return;

        var old = CurrentEnemyState;
        CurrentEnemyState = newEnemyState;
        OnStateChanged?.Invoke(old, newEnemyState);

        if (newEnemyState == EnemyState.Movement)
            motor.enabled = true;
        else
            motor.enabled = false;
    }
    
    private void FixedUpdate()
    {
        if (CurrentEnemyState != EnemyState.Movement) return;

        Vector2 targetPos = currentAttack != null
            ? currentAttack.GetDesiredPosition(fighterController.Hurtbox.transform.position, transform.position)
            : (Vector2)fighterController.transform.position;

        Vector2 diff = targetPos - (Vector2)transform.position;
        if (data.movementType != EnemyData.MovementType.Flying)
            diff.y = 0;
        SetFacing(diff.x);

        if (diff.magnitude > 0.1f)
        {
            // Smooth movement
            data.movementAI.Move(this, diff.normalized);
        }
        else if (currentAttack != null)
        {
            // Execute attack once in range
            ChangeState(EnemyState.Attack);
            currentAttack.Execute(this);
            currentAttack = null;
        }
        
    }
    
  
    
    public void TakeDamage(float amount)
    {
        currentHP -= amount;
        Debug.Log($"{name} took {amount} damage! HP left: {currentHP}");

        if (currentHP <= 0)
            Die();
    }
    public void ApplyKnockback(Vector2 force)
    {
        Vector2 modifiedForce = force * (1f - data.knockbackResistance);
        motor.ApplyExternalVelocity(modifiedForce);
    }


    private void Die()
    {
        Debug.Log($"{name} died!");
        Destroy(gameObject);
    }

  
  
}
