using System.Collections;
using DamageSystem;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Attacks/JumpAttack")]
public class JumpAttack : EnemyAttack
{
    public Vector2 jumpOffset = new Vector2(0, 2f);
    public float damageAmount = 3f;
    public float jumpForce = 7.5f;   // Added since it was missing
    public float horizontalLaunchForce = 4f;
    public float windupTime = 0.2f;
    public float attackDuration = 0.4f;
    public float attackRange = 1.5f;
    public float recoveryTime = 0.25f;
    
    [Header("Hitbox Prefab")]
    public Hitbox landingHitboxPrefab;
    public HitInfo landingHitInfo = HitInfo.Default;
    
    public override void Execute(EnemyController controller)
    {
        Debug.Log($"{controller.data.enemyName} performed Jump Attack!");

        // Move enemy slightly above player
        controller.StartCoroutine(JumpAttackRoutine(controller));
        // Apply damage if in range
   
    }

    public override Vector2 GetDesiredPosition(Vector2 playerPos, Vector2 enemyPos)
    {
        return new Vector2(playerPos.x, enemyPos.y);
    }
    private IEnumerator JumpAttackRoutine(EnemyController controller)
    {
        controller.ChangeState(EnemyController.EnemyState.Attack);
        controller.motor.Stop();

        yield return new WaitForSeconds(windupTime);

        Vector2 dir = (controller.playerPosition.position - controller.transform.position).normalized;
        controller.Rigidbody2D.linearVelocity = new Vector2(dir.x * horizontalLaunchForce, jumpForce);        // wait until grounded again
        float fallTimer = 1.25f;
        while (!controller.motor.Grounded && fallTimer > 0f)
        {
            fallTimer -= Time.deltaTime;
            yield return null;
        }

        // enable landing hitbox
        Hitbox hitbox = Instantiate(landingHitboxPrefab, controller.transform.position, Quaternion.identity, controller.transform);
        hitbox.Activate(landingHitInfo);
        
        yield return new WaitForSeconds(attackDuration);
        Debug.Log($"{controller.data.enemyName} ended Jump Attack!");
        if (hitbox != null)
        {
            hitbox.Deactivate();
            Destroy(hitbox.gameObject);
        }
        
        controller.ChangeState(EnemyController.EnemyState.Movement);
        //controller.motor.StartMoving(); // Re-enable movement if your motor has this
    }
}