using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Movement/FlyingMovement")]
public class FlyingMovementAI : MovementAI
{
    public float speed = 5f;
    public float smoothing = 8f;
    public override void Move(EnemyController enemy, Vector2 direction)
    {
        if (direction == Vector2.zero) 
        {
            enemy.motor.Stop();
            return;
        }

        // Smoothly move in physics time
        Vector2 desired = direction.normalized * speed;
        enemy.motor.Move(Vector2.Lerp(enemy.motor.currentVelocity, desired, smoothing * Time.fixedDeltaTime));
    }

    public override void Stop(EnemyController enemy)
    {
        enemy.motor.Stop();
    }
}