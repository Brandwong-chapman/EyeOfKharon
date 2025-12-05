using UnityEngine;

namespace ControllerSystem.Platformer2D.BasicAttack{
    public class AttackHitbox : MonoBehaviour
    {
        public AttackConfigSO Config;
        public PlatformerBasicAttackModule Owner;
        public string OwnerTag;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(OwnerTag)) return;
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