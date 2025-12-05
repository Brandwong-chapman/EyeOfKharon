using UnityEngine;
using DamageSystem;

public class EnemyBasicProjectile : MonoBehaviour
{
    public float speed = 8f;
    public float lifetime = 3f;
    public HitInfo hitInfo = HitInfo.Default;
    
    private Vector2 direction;
    
    public void Fire(Vector2 dir, HitInfo info)
    {
        direction = dir.normalized;
        hitInfo = info;
        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out Hurtbox hurtbox))
            return;

        // respect friendly fire & intangibility automatically
        if (hurtbox.Dead || hurtbox.Intangible || 
            FriendlyFireManager.ViolatesFriendlyFire(hitInfo.team, hurtbox.Team))
            return;

        hitInfo.hitPosition = other.ClosestPoint(transform.position);
        hurtbox.TakeDamage(hitInfo); 

        Destroy(gameObject);
    }
}

