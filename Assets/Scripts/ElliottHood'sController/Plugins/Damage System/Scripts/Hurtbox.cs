using System;
using UnityEngine;

namespace DamageSystem 
{
    [RequireComponent(typeof(Collider2D))]
    public class Hurtbox : MonoBehaviour
    {
        [SerializeField] private float _startingHealth = 20;
        [Tooltip("Intangibility prevents hitboxes from detecting this hurtbox")]
        [SerializeField] private bool _intangible = false;
        [Tooltip("Invincibility prevents health loss")]
        [SerializeField] private bool _invincible = false;
        [Tooltip("Non-substantial hitboxes don't trigger the 'first hit' event on hitboxes. Useful for background objects")]
        [SerializeField] private bool _substantial = true;
        public bool Invincible => _invincible;
        public bool Intangible => _intangible;
        public bool Substantial => _substantial;
        public Team Team { get; set; }
        public float CurrentHealth { get; set; }
        public bool Dead { get; private set; }

        public Action<HitEventInfo> OnHit;
        public Action OnDeath;
        
        private Collider2D _collision;
        public Vector2 CenterPosition => (Vector2)transform.position + _collision.offset;

        private void Awake()
        {
            gameObject.layer = Layers.Hurtbox;
            _collision = GetComponent<Collider2D>();
            CurrentHealth = _startingHealth;
        }

        public void TakeDamage(HitInfo hitInfo)
        {
            if (Intangible || Dead) return;
            
            SetHealth(CurrentHealth - hitInfo.damage);
            OnHit?.Invoke(new HitEventInfo
            {
                hitInfo = hitInfo,
                hurtbox = this,
            });
            DeathCheck();
        }

        public void SetHealth(float newHealth)
        {
            if (Invincible)
                return;

            newHealth = Mathf.Max(newHealth, 0);

            if (Mathf.Approximately(CurrentHealth, newHealth))
                return;

            CurrentHealth = newHealth;
            DeathCheck();
        }

        private void DeathCheck()
        {
            if (Dead) return;
            
            Dead = CurrentHealth <= 0;

            if (Dead)
            {
                OnDeath?.Invoke();
            }
        }
    }
}