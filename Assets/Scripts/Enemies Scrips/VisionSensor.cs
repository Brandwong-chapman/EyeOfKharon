using UnityEngine;

public class VisionSensor : MonoBehaviour
{
    public float viewRange = 5f;
    public Transform player;

    private Collider2D selfCollider;

    private void Awake()
    {
        selfCollider = GetComponent<Collider2D>();
    }

    public bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector2 origin = transform.position;
        Vector2 dir = player.position - transform.position;
        float dist = dir.magnitude;

        if (dist > viewRange) return false;

        // Raycast that hits ALL colliders
        RaycastHit2D hit = Physics2D.Raycast(origin, dir.normalized, dist);

        // If nothing hit → no visibility
        if (!hit.collider)
            return false;

        // Ignore hitting self
        if (hit.collider == selfCollider)
        {
            // Perform a second raycast starting slightly ahead
            Vector2 tinyOffset = dir.normalized * 0.05f;
            origin += tinyOffset;

            hit = Physics2D.Raycast(origin, dir.normalized, dist - 0.05f);
            if (!hit.collider)
                return false;
        }

        Debug.DrawRay(origin, dir.normalized * dist,
            hit.collider.CompareTag("Player") ? Color.green : Color.red);

        return hit.collider.CompareTag("Player");
    }
}