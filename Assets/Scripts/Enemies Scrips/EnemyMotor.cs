using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyMotor : MonoBehaviour
{
    [Header("Motor Settings")]
    public float acceleration = 10f;
    public float deceleration = 8f;
    public float gravityScale = 3f;
    public LayerMask groundLayer;
    public bool isFlying = false;

    [HideInInspector] public Vector2 currentVelocity; // Smoothed velocity
    private Rigidbody2D _rb;
    private Collider2D _collider;

    private Vector2 _desiredVelocity;
    private Vector2 _externalVelocity;  // For knockback etc.
    private bool _ignoreGravity;

    public bool Grounded { get; private set; }
    public Rigidbody2D Rigidbody2D => _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
        if (!isFlying) CheckGrounded();
        ApplyGravity();
    }

    public void Move(Vector2 velocity)
    {
        _desiredVelocity = velocity;
    }

    public void ApplyExternalVelocity(Vector2 velocity)
    {
        _externalVelocity = velocity;
    }

    public void Stop()
    {
        _desiredVelocity = Vector2.zero;
    }

    private void ApplyMovement()
    {
        Vector2 targetVel = _desiredVelocity + _externalVelocity;

        if (isFlying)
        {
            // Smooth flying movement
            currentVelocity = Vector2.Lerp(currentVelocity, targetVel, acceleration * Time.fixedDeltaTime);
            _rb.linearVelocity = currentVelocity;
        }
        else
        {
            // Grounded movement
            float smoothedX = Mathf.Lerp(_rb.linearVelocity.x, targetVel.x, acceleration * Time.fixedDeltaTime);
            _rb.linearVelocity = new Vector2(smoothedX, _rb.linearVelocity.y);

            // Update currentVelocity so it reflects smoothed movement
            currentVelocity = new Vector2(smoothedX, _rb.linearVelocity.y);
            
        }

        // Reset external velocity each tick
        _externalVelocity = Vector2.zero;
    }

    private void CheckGrounded()
    {
        Bounds bounds = _collider.bounds;
        Vector2 origin = new Vector2(bounds.center.x, bounds.min.y - 0.05f);
        float radius = bounds.extents.x * 0.9f;

        Grounded = Physics2D.OverlapCircle(origin, radius, groundLayer);
    }

    private void ApplyGravity()
    {
        _rb.gravityScale = (!isFlying && !_ignoreGravity) ? gravityScale : 0f;
    }

    public void EnableGravity(bool enable)
    {
        _ignoreGravity = !enable;
    }
}
