using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Movement/GroundedMovementAI")]
public class GroundedMovementAI : MovementAI
{
    public float speed = 3f;
    public LayerMask groundLayer;
    public float groundCheckDistance = 1f;

    public override void Move(EnemyController enemy, Vector2 direction)
    {
        if (direction == Vector2.zero)
        {
            enemy.motor.Stop();
            return;
        }

        // For grounded enemies, use smoothed velocity
        Vector2 desired = new Vector2(direction.x * speed, enemy.motor.currentVelocity.y);
        enemy.motor.Move(desired); // sets _desiredVelocity
        
    }
    public override void Stop(EnemyController enemy)
    {
        enemy.motor.Move(new Vector2(0f, enemy.motor.currentVelocity.y));
        
    }
}