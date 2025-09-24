using System;
using ControllerSystem.Platformer2D;
using UnityEngine;

/// <summary>
/// ONLY WORKS ON COLLIDERS WITH THE 'Climbable' TAG!!
/// </summary>
[RequireComponent(typeof(PlatformerJumpModule))]
public class PlatformerWallModule : PlatformerMotorModule
{
    [Tooltip("The relative height multiplier for wall detection. (aligned to the bottom of the collider)")]
    [SerializeField] private float _wallDetectionHeightMultiplier = 1/16f;
    private PlatformerJumpModule _jumpModule;

    public override void Initialize(PlatformerMotor newMotor)
    {
        base.Initialize(newMotor);

        if (newMotor.Controller is FighterController fighter)
        {
            fighter.OnStateChanged += OnFighterUpdateState;
        }
        InitializeJumpModuleListeners();
    }

    private void OnFighterUpdateState(FighterController.StateUpdateInfo obj)
    {
        print(obj.OldState);
        print(obj.NewState);
        _state = WallState.None;
    }

    private void OnDestroy()
    {
        if (Controller != null && Controller is FighterController fighter)
        {
            fighter.OnStateChanged += OnFighterUpdateState;
        }
        ClearJumpModuleListeners();
    }

    public override void HandleMovement()
    {
        HandleWallDetection();
        HandleStickyWallStates();
    }

    #region Wall Detection
    
    private bool _touchingLeftWall;
    private bool _touchingRightWall;
    private bool _pressingIntoWall;

    /// <summary>
    /// Returns the direction to the wall we're touching, or 0 if we're not touching a wall.
    /// </summary>
    /// <returns></returns>
    public int GetWallClingDirection()
    {
        switch (_lastWallTouched)
        {
            case WallSide.Left:
                return -1;
            case WallSide.Right:
                return 1;
            default:
                return 0;
        }
    }

    private enum WallSide { None, Left, Right }

    private WallSide _lastWallTouched = WallSide.None;
    
    private void HandleWallDetection()
    {
        Rect leftRect = GetWallCheckRect(motor.Collider.bounds, true);
        _touchingLeftWall = WallCheckRectHits(leftRect);
        Rect rightRect = GetWallCheckRect(motor.Collider.bounds, false);
        _touchingRightWall = WallCheckRectHits(rightRect);
        
        if (_touchingLeftWall || _touchingRightWall)
        {
            _lastWallContactTime = Time.time;
            _lastWallTouched = _touchingLeftWall ? WallSide.Left : WallSide.Right;
        }
    }
    
    #endregion

    #region Wall Cling, Climb, and Slide

    public const float INTO_WALL_INPUT_THRESHOLD = 0.1f;
    public const float CLIMB_INPUT_THRESHOLD = 0.6f;
    
    [SerializeField] private float _wallSlideSpeed = 5f;
    [SerializeField] private float _wallClimbSpeed = 6f;
    [SerializeField] private float _wallSlideAcceleration = 5f;
    [Tooltip("Whether or not to prevent slipping off of a wall when sliding down it")]
    private float _wallSlideVelocity;

    [SerializeField] private float _wallHangSnapRange = 0.1f;
    [SerializeField] private float _ledgeSlipGraceTime = 0.1f;
    private float _lastLedgeSlipTime;

    private WallState _state;
    private WallState _lastFrameWallState = WallState.None;
    public WallState State => _state; 
    public enum WallState
    {
        None,
        Cling,
        Slide,
        Climb,
        Hang
    }

    private void HandleStickyWallStates()
    {
        _state = DetermineWallState();
        
        if (_state != WallState.Slide && _state != WallState.Hang)
            _wallSlideVelocity = 0;
        
        if (_state == WallState.None)
            return;

        Controller.FacingLeft = _touchingLeftWall;

        switch (_state)
        {
            case WallState.Cling:
                HandleWallCling();
                break;
            case WallState.Slide:
                HandleWallSlide();
                break;
            case WallState.Climb:
                HandleWallClimb();
                break;
            case WallState.Hang:
                HandleWallHanging();
                break;
        }
        
        _lastFrameWallState = _state;
    }

    private WallState DetermineWallState()
    {
        // Can't stick to the wall if you're not touchin it
        if (!_touchingLeftWall && !_touchingRightWall)
        {
            return WallState.None;
        }
        
        // Calculate state
        Vector2 input = Controller.Input.move.GetValue();
        _pressingIntoWall = (input.x > INTO_WALL_INPUT_THRESHOLD && _touchingRightWall) || (input.x < - INTO_WALL_INPUT_THRESHOLD && _touchingLeftWall);
        bool inputtingClimb = input.y > CLIMB_INPUT_THRESHOLD && _pressingIntoWall;

        // Must be actively moving into the wall to start sticking to it
        if (_state == WallState.None)
        {
            // Don't start clinging if we're not pressing into the wall
            if (!_pressingIntoWall)
                return WallState.None;

            if (inputtingClimb)
            {
                // Allow clinging if we'd be moving faster by climbing
                bool wouldMoveVerticallyFasterByClimbing = motor.Rb.linearVelocity.y < _wallClimbSpeed;
                if (!wouldMoveVerticallyFasterByClimbing)
                    return WallState.None;
            }
            else
            {
                // Don't start clinging if we're still rising 
                if (_jumpModule.Rising)
                    return WallState.None;
            }
            
            // Don't start clinging if the hanging slip grace time hasn't expired yet
            if (Time.time - _lastLedgeSlipTime < _ledgeSlipGraceTime)
                return WallState.None;
            
            // Cling successful!
            _jumpModule.ResetDoubleJumps();
        }
        
        WallState calculatedWallState;
        if (inputtingClimb)
        {
            calculatedWallState = WallState.Climb;
        }
        else if (_pressingIntoWall && ShouldHangCollisionDetection())
        {
            calculatedWallState = WallState.Hang;
        }
        else if (_pressingIntoWall && input.y > -CLIMB_INPUT_THRESHOLD && input.y < CLIMB_INPUT_THRESHOLD)
        {
            calculatedWallState =  WallState.Cling;
        }
        else
        {
            calculatedWallState =  WallState.Slide;
        }

        // The only wall state we can activate without being airborne first is climbing
        if (motor.Grounded && calculatedWallState != WallState.Climb)
            return WallState.None;
        
        return calculatedWallState;
    }

    private void HandleWallCling()
    {
        motor.Rb.linearVelocity = new Vector2(0, motor.CounteractGravityVelocity);
    }

    private void HandleWallClimb()
    {
        motor.Rb.linearVelocity = new Vector2(0f, _wallClimbSpeed + motor.CounteractGravityVelocity);
    }

    private void HandleWallSlide()
    {
        _wallSlideVelocity += _wallSlideAcceleration * Time.fixedDeltaTime;
        _wallSlideVelocity = Mathf.Min(_wallSlideVelocity, _wallSlideSpeed);

        float verticalInput = Controller.Input.move.GetValue().y;

        // Holding down = skip acceleration (instant full speed)
        if (verticalInput < -CLIMB_INPUT_THRESHOLD)
        {
            _wallSlideVelocity = _wallSlideSpeed;
        }

        motor.Rb.linearVelocity = new Vector2(motor.Rb.linearVelocity.x, -_wallSlideVelocity + motor.CounteractGravityVelocity);
    }

    
    #region Hanging
    private void HandleWallHanging()
    {
        Rigidbody2D rb = motor.Rb;

        // Snap to wall edge (once on enter)
        if (_state != _lastFrameWallState)
        {
            float snapYOffset = 0f;
            float maxSnapDistance = 1f; // how far up to check (adjust if needed)
            float stepSize = 0.005f;    // resolution of the snap steps

            for (float offset = 0f; offset < maxSnapDistance; offset += stepSize)
            {
                float yOffset = offset;
                Bounds offsetBounds = ShiftBounds(motor.Collider.bounds, yOffset);
                Rect wallCheck = GetWallCheckRect(offsetBounds, _touchingLeftWall);

                if (WallCheckRectHits(wallCheck))
                {
                    snapYOffset = yOffset;
                    break;
                }
            }

            rb.position += new Vector2(0, snapYOffset);
        }

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        motor.MakeWeightless(Time.fixedDeltaTime * 2);

        // Let go logic
        if (!_pressingIntoWall)
        {
            _state = WallState.None;
            _lastLedgeSlipTime = Time.time;
        }
    }
    private bool ShouldHangCollisionDetection()
    {
        Rigidbody2D rb = motor.Rb;
        Vector2 predictedPosition = rb.position + new Vector2(0, (-_wallSlideSpeed) * Time.fixedDeltaTime);
        predictedPosition.y -= _wallHangSnapRange; // Add an extra buffer threshold for the check
        Bounds predictedBounds = ShiftBounds(motor.Collider.bounds, predictedPosition.y - rb.position.y);
        Rect nextWallCheck = GetWallCheckRect(predictedBounds, _touchingLeftWall);
    
        return !WallCheckRectHits(nextWallCheck);
    }
    
    #endregion
    
    #endregion
    
    #region Jumping 

    private void InitializeJumpModuleListeners()
    {
        _jumpModule = GetComponent<PlatformerJumpModule>();
        _jumpModule.OnJump += JumpModuleOnOnJump;
        _jumpModule.OnDoubleJump += JumpModuleOnOnJump;
        
        _jumpModule.SetJumpOverrideHandler(HandleWallJump);
    }

    public void ClearJumpModuleListeners()
    {
        if (_jumpModule != null)
        {
            _jumpModule.OnJump -= JumpModuleOnOnJump;
            _jumpModule.OnDoubleJump -= JumpModuleOnOnJump;
            
            _jumpModule?.SetJumpOverrideHandler(null);
        }
    }

    private void JumpModuleOnOnJump(PositionInfo e)
    {
        _state = WallState.None;
    }
    
    #endregion
    
    #region Wall Jump

    [Header("WallJump")] 
    [SerializeField] private float _wallJumpForceX = 8f;
    [SerializeField] private float _wallJumpForceY = 16.66667f;
    [SerializeField] private float _wallJumpCoyoteTime = 0.1f;
    
    private float _lastWallContactTime;

    private bool WallCoyoteTimeValid => Time.time - _lastWallContactTime <= _wallJumpCoyoteTime;
    /// <summary>
    /// -1 or 1 based on the movement direction
    /// </summary>
    public event Action<int> OnWallJump;
    
    private bool HandleWallJump()
    {
        // Only allow wall jump if not grounded and we recently touched a wall
        if (motor.Grounded || (!_touchingLeftWall && !_touchingRightWall && !WallCoyoteTimeValid))
            return false;
        
        if (!Controller.Input.jump.TryUseBuffer())
            return false;

        WallJump();

        return true;
    }

    private void WallJump()
    {
        // Determine jump direction
        int direction = _lastWallTouched == WallSide.Left ? 1 :
            _lastWallTouched == WallSide.Right ? -1 : 0;
    
        motor.Rb.linearVelocity = new Vector2(direction * _wallJumpForceX, _wallJumpForceY);

        // Clear cling/slide/climb state
        _state = WallState.None;

        // Reset jump early state
        _jumpModule.ResetJumpEndedEarly();

        motor.Grounded = false;

        OnWallJump?.Invoke(direction);
    }

    #endregion
    
    #region Wall Checks
    
    private Bounds ShiftBounds(Bounds original, float yOffset)
    {
        Bounds shifted = new Bounds(original.center, original.size);
        shifted.center += new Vector3(0, yOffset, 0);
        return shifted;
    }

    private Rect GetWallCheckRect(Bounds bounds, bool leftWall)
    {
        float width = PlatformerColliderManager.GROUND_CHECK_THICKNESS;
        float height = bounds.size.y * _wallDetectionHeightMultiplier;

        float xEdge = leftWall ? bounds.min.x : bounds.max.x;
        float centerX = xEdge + (leftWall ? -width : width) / 2;

        float centerY = bounds.center.y;

        Vector2 size = new Vector2(width, height);
        Vector2 center = new Vector2(centerX, centerY);

        return new Rect(center - size / 2, size);
    }
    
    private bool WallCheckRectHits(Rect rect)
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(rect.center, rect.size, 0f, GroundCheck.GroundLayerMask);
        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject.CompareTag(Tags.Climbable))
                return true;
        }
        return false;
    }
    
    #endregion
    
    #if UNITY_EDITOR
            
    private void OnDrawGizmosSelected()
    {
        // Call GetComponent here so it works in Editor
        PlatformerMotor platformerMotor = GetComponent<PlatformerMotor>();
        
        DrawRectGizmo(GetWallCheckRect(platformerMotor.Collider.bounds, true), Color.red);
        DrawRectGizmo(GetWallCheckRect(platformerMotor.Collider.bounds, false), Color.red);
    }

    private void DrawRectGizmo(Rect checkRect, Color color)
    {
        UnityEditor.Handles.color = color;
        UnityEditor.Handles.DrawSolidRectangleWithOutline(
            new Vector3[]
            {
                new Vector3(checkRect.xMin, checkRect.yMin),
                new Vector3(checkRect.xMax, checkRect.yMin),
                new Vector3(checkRect.xMax, checkRect.yMax),
                new Vector3(checkRect.xMin, checkRect.yMax)
            },
            new Color(color.r, color.g, color.b, 0.1f), // fill
            color // outline
        );
    }
            
    #endif
}
