using System;
using UnityEngine;

namespace DamageSystem
{
    /// <summary>
    /// Information given to the hurtbox when it gets hit
    /// </summary>
    [Serializable]
    public struct HitInfo
    {
        public float damage;
        public Vector2 knockback;
        public float hitStop;
        public Team team;
        [HideInInspector]
        public Hitbox hitbox;
        [HideInInspector]
        public Vector2 hitPosition;

        public static HitInfo Default => new HitInfo { 
            damage = 3f,
            knockback = Vector2.up,
            hitStop = 0.1f,
            team = Team.Neutral
        };

        public HitInfo(HitInfo reference)
        {
            damage = reference.damage;
            knockback = reference.knockback;
            hitStop = reference.hitStop;
            team = reference.team;
            hitbox = reference.hitbox;
            hitPosition = reference.hitPosition;
        }
    }

    /// <summary>
    /// Neutral hurtboxes/hitboxes can hit and be hit by anything.
    /// Hitboxes will not hit hurtboxes of the same team
    /// </summary>
    public enum Team
    {
        Neutral,
        Player,
        Enemy
    }

    public class HitEventInfo
    {
        public HitInfo hitInfo;
        public Hurtbox hurtbox;
    }
}