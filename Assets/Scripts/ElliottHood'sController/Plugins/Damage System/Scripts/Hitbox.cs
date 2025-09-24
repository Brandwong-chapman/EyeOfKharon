using UnityEngine;
using UnityEngine.Events;

namespace DamageSystem
{
    [RequireComponent(typeof(Collider2D))]
    public class Hitbox : MonoBehaviour
    {
        [SerializeField] private HitInfo _hitInfo = HitInfo.Default;
        private Collider2D _collision;
        private bool _hitSomethingSubstantial;

        private void Awake()
        {
            gameObject.layer = Layers.Hitbox;
            _collision = GetComponent<Collider2D>();
            _collision.isTrigger = true;
        }

        /// <summary>
        /// Only triggers on the first substantial Hurtbox per activation.
        /// Should be used for things like particles, screenshake, and hitstop on the attackers end
        /// </summary>
        public readonly UnityEvent<HitEventInfo> OnFirstHit = new UnityEvent<HitEventInfo>();
        /// <summary>
        /// Triggers when this hitbox hits any hurtbox
        /// </summary>
        public readonly UnityEvent<HitEventInfo> OnHit = new UnityEvent<HitEventInfo>();

        public void Activate(HitInfo hitinfo, Vector2 direction)
        {
            hitinfo.knockback = hitinfo.knockback.magnitude * direction.normalized;
            Activate(hitinfo);
        }

        [ContextMenu("Activate")]
        public void Activate() => Activate(_hitInfo);

        public void Activate(HitInfo hitinfo)
        {
            // Deactivate first
            if (_collision.enabled)
                Deactivate();

            // Reset parameters
            _collision.enabled = true;
            _hitSomethingSubstantial = false;

            // Initialize hitInfo and events
            _hitInfo = hitinfo;
            _hitInfo.hitbox = this;
        }

        public void Deactivate()
        {
            _collision.enabled = false;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Make sure that hits aren't triggered by any other 2D triggers that are incidentally a child of this object
            if (!_collision.enabled)
                return;

            // Check that the collision has a valid hurtbox
            if (!collision.TryGetComponent(out Hurtbox hurtbox))
                return;

            // Check that we can hurt the hitbox
            if (hurtbox.Dead || hurtbox.Intangible || FriendlyFireManager.ViolatesFriendlyFire(_hitInfo.team, hurtbox.Team))
                return;

            _hitInfo.hitPosition = collision.ClosestPoint(transform.position);
            DamageHurtbox(hurtbox);
        }

        private void DamageHurtbox(Hurtbox hurtbox)
        {
            // First Substantial Hit Events
            bool firstHit = !_hitSomethingSubstantial && hurtbox.Substantial;

            if (hurtbox.Substantial)
                _hitSomethingSubstantial = true;

            hurtbox.TakeDamage(_hitInfo);
            InvokeHitEvents(_hitInfo, hurtbox, firstHit);
        }

        private void InvokeHitEvents(HitInfo hitInfo, Hurtbox hurtbox, bool firstHit)
        {
            HitEventInfo hitEventInfo = new HitEventInfo
            {
                hitInfo = hitInfo,
                hurtbox = hurtbox
            };

            if (firstHit)
            {
                OnFirstHit.Invoke(hitEventInfo);
            }
            OnHit.Invoke(hitEventInfo);
        }
    }
}
