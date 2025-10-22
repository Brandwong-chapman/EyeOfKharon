using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace ControllerSystem.Platformer2D.BasicAttack{
    public enum AttackDirection { Forward, Up, Down }

    public class PlatformerBasicAttackModule : PlatformerMotorModule
    {
        [Header("Weapon Reference")]
        [SerializeField] private WeaponSO _currentWeapon;
        [SerializeField] private Transform _attackOrigin;
        private bool _isAttacking;
        private Coroutine _attackRoutine;

        public static Action<IDamageable> OnAttackHit;

        public override void HandleMovement()
        {
            HandleAttackInput();
        }

        private void HandleAttackInput()
        {
            if (_isAttacking || _currentWeapon == null) return;

            if (Controller.Input.primary.TryUseBuffer())
            {
                AttackDirection direction = GetAttackDirection();
                AttackConfigSO config = GetAttackFromDirection(direction);
                if (config != null)
                {
                    _attackRoutine = StartCoroutine(PerformAttack(config, direction));
                }
            }
        }

        private IEnumerator PerformAttack(AttackConfigSO config, AttackDirection direction)
        {
            _isAttacking = true;
            
            // flip knockback direction based on facing
            Vector2 facing = Controller.FacingLeft ? Vector2.right : Vector2.left;
            config.KnockbackDirection = facing;

            // Windup phase
            yield return new WaitForSeconds(config.WindupTime);

            // Active phase (spawn hitbox)
            Vector2 facingDir = Controller.FacingLeft ? Vector2.right : Vector2.left;
            Vector2 spawnOffset = config.SpawnOffset;

            // Flip X offset if facing left
            if (!Controller.FacingLeft)
                spawnOffset.x *= -1;

            Vector3 spawnPos = _attackOrigin.position + (Vector3)spawnOffset;

// Instantiate as a child of the player
            var hitbox = Instantiate(config.HitboxPrefab, spawnPos, Quaternion.identity, _attackOrigin);

// Optional rotation for up/down attacks
            if (direction == AttackDirection.Up)
                hitbox.transform.localRotation = Quaternion.Euler(0, 0, 90);
            else if (direction == AttackDirection.Down)
                hitbox.transform.localRotation = Quaternion.Euler(0, 0, -90);
            else
                hitbox.transform.localRotation = Quaternion.identity;

// Assign the hitbox references
            var hb = hitbox.GetComponent<AttackHitbox>();
            hb.Config = config;
            hb.Owner = this;

            
            yield return new WaitForSeconds(config.ActiveTime);
            Destroy(hitbox);

            // Cooldown phase
            yield return new WaitForSeconds(config.CooldownTime);
            _isAttacking = false;
        }

        private AttackConfigSO GetAttackFromDirection(AttackDirection direction)
        {
            return direction switch
            {
                AttackDirection.Up => _currentWeapon.UpAttack,
                AttackDirection.Down => _currentWeapon.DownAttack,
                _ => _currentWeapon.ForwardAttack,
            };
        }

        private AttackDirection GetAttackDirection()
        {
            Vector2 input = Controller.Input.move.GetValue();

            if (!motor.Grounded && input.y < -0.5f)
                return AttackDirection.Down;
            else if (input.y > 0.5f)
                return AttackDirection.Up;
            else
                return AttackDirection.Forward;
        }
        
        public void SetWeapon(WeaponSO newWeapon)
        {
            _currentWeapon = newWeapon;
        }
    }
}