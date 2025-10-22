using UnityEngine;

namespace ControllerSystem.Platformer2D.BasicAttack{
    public class AttackHitbox : MonoBehaviour
    {
        public AttackConfigSO Config;
        public PlatformerBasicAttackModule Owner;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out IDamageable target))
            {
                target.TakeDamage(Config.Damage);
                Vector2 direction = (other.transform.position - Owner.transform.position).normalized;
                Vector2 knockback = direction * Config.KnockbackForce;

                target.ApplyKnockback(knockback);
                PlatformerBasicAttackModule.OnAttackHit?.Invoke(target);
            }
        }
    }
}