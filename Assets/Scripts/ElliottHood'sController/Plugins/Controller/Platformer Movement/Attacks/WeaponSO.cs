using UnityEngine;

namespace ControllerSystem.Platformer2D.BasicAttack{

    [CreateAssetMenu(fileName = "NewWeapon", menuName = "Platformer/Weapon")]
    public class WeaponSO : ScriptableObject
    {
        [Header("Attack Variants")]
        public AttackConfigSO ForwardAttack;
        public AttackConfigSO UpAttack;
        public AttackConfigSO DownAttack;

    }
}