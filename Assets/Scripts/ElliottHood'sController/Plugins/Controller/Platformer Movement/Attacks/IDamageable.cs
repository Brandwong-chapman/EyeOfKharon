using UnityEngine;

namespace ControllerSystem.Platformer2D.BasicAttack{

    public interface IDamageable
    {
        void TakeDamage(float amount);
        void ApplyKnockback(Vector2 force);
    }
}