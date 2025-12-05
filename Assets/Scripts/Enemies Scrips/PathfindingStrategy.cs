using UnityEngine;

public abstract class PathfindingStrategy : ScriptableObject
{
    protected EnemyController enemy;

    public virtual void Initialize(EnemyController enemy)
    {
        this.enemy = enemy;
    }

    public virtual void UpdatePath() { }

    public abstract Vector2 GetMoveDirection();
}