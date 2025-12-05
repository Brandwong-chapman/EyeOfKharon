using System.Collections;
using UnityEngine;
using DamageSystem;

[CreateAssetMenu(menuName = "Enemy/Attacks/ProjectileAttack")]
public class BasicProjectileAttack : EnemyAttack
{
    public EnemyBasicProjectile projectilePrefab;
    public HitInfo hitInfo = HitInfo.Default;
    public float windupTime = 0.3f; // delay before fire
    public float recoveryTime = 0.5f;

    public override void Execute(EnemyController controller)
    {
        controller.StartCoroutine(FireRoutine(controller));
    }

    private IEnumerator FireRoutine(EnemyController controller)
    {
        controller.motor.Stop();
        controller.ChangeState(EnemyController.EnemyState.Attack);

        yield return new WaitForSeconds(windupTime);

        // Direction aimed at player's center
        Vector2 dir = ((Vector2)controller.PlayerCenter - (Vector2)controller.transform.position).normalized;
        
        // Spawn position slightly in front of enemy
        Vector3 spawnPos = controller.transform.position + (Vector3)(dir * 0.6f);

        EnemyBasicProjectile proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        proj.Fire(dir, hitInfo);

        yield return new WaitForSeconds(recoveryTime);

        controller.ChangeState(EnemyController.EnemyState.Movement);
    }

}