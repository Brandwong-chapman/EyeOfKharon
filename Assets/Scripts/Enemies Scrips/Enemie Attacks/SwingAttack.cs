using System.Collections;
using DamageSystem;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Attacks/SwingAttack")]
public class SwingAttack : EnemyAttack
{
    public float windupTime = 0.2f;
    public float activeTime  = 0.4f;
    public float recoveryTime = 0.25f;
    
    [Header("Hitbox Prefab")]
    public Hitbox swingHitboxPrefab;
    public HitInfo hitInfo = HitInfo.Default;
    public Vector2 hitboxOffset = new Vector2(1.0f, 0f);

    public override void Execute(EnemyController controller)
    {
        Debug.Log($"{controller.data.enemyName} performed Swing Attack!");

        controller.StartCoroutine(SwingRoutine(controller));
    }
    public override Vector2 GetDesiredPosition(Vector2 playerPos, Vector2 enemyPos)
    {
        // Stand near the player on the ground
        return new Vector2(playerPos.x, enemyPos.y);
    }

    private IEnumerator SwingRoutine(EnemyController controller)
    {
        controller.motor.Stop();
        controller.ChangeState(EnemyController.EnemyState.Attack);

        yield return new WaitForSeconds(windupTime);

        // spawn + activate hitbox
        // spawn + activate hitbox
        Vector2 spawnPos = (Vector2)controller.transform.position + 
                           new Vector2(hitboxOffset.x * controller.FacingDirection, hitboxOffset.y);

        Hitbox hitbox = Instantiate(swingHitboxPrefab, spawnPos, Quaternion.identity, controller.transform);

// Flip hitbox to face correct direction
        hitbox.transform.localScale = new Vector3(controller.FacingDirection, 1f, 1f);

        hitbox.Activate(hitInfo);
        
        yield return new WaitForSeconds(activeTime );

        hitbox.Deactivate();
        Destroy(hitbox.gameObject);

        yield return new WaitForSeconds(recoveryTime);
        controller.ChangeState(EnemyController.EnemyState.Movement);
    }
}