using UnityEngine;

public abstract class MovementAI : ScriptableObject
{
    public abstract void Move(EnemyController enemy, Vector2 direction);
    public abstract void Stop(EnemyController enemy);
}