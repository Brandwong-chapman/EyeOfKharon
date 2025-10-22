using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ControllerSystem.Platformer2D.BasicAttack{
    
        [CreateAssetMenu(fileName = "NewAttackConfig", menuName = "Platformer/Attack Config")]
        public class AttackConfigSO : ScriptableObject
        {
            [Header("Attack Settings")]
            public string AttackName = "Slash";
            public float Damage = 10f;
            public float KnockbackForce = 8f;
            public Vector2 KnockbackDirection = Vector2.right;

            [Header("Timing")]
            public float WindupTime = 0.1f;
            public float ActiveTime = 0.15f;
            public float CooldownTime = 0.3f;

            [Header("Visuals")]
            public AnimationClip Animation;
            public GameObject HitboxPrefab;
            public AudioClip AttackSound;
            
            [Header("Positioning")]
            public Vector2 SpawnOffset = Vector2.zero;
        }
    }
