using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

[CreateAssetMenu(menuName = "Enemy/Pathfinding/FlyingPathfinding")]
public class FlyingPathfinding : PathfindingStrategy
{
    private List<Vector2> currentPath = new List<Vector2>();
    private int currentWaypoint = 0;
    private Seeker seeker;
    private const float waypointThreshold = 0.15f;

    public override void Initialize(EnemyController enemy)
    {
        base.Initialize(enemy);
        seeker = enemy.GetComponent<Seeker>();
        enemy.StartCoroutine(PathUpdateRoutine());
    }

    private IEnumerator PathUpdateRoutine()
    {
        while (true)
        {
            UpdatePath();
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void UpdatePath()
    {
        if (enemy.playerPosition == null || seeker == null || !seeker.IsDone())
            return;

        seeker.StartPath(
            enemy.transform.position,
            enemy.playerPosition.position, // <--- use .position
            p =>
            {
                if (!p.error)
                {
                    currentPath.Clear();
                    foreach (Vector3 wp in p.vectorPath)
                        currentPath.Add(wp);
                    currentWaypoint = 0;
                }
            });
    }


    public override Vector2 GetMoveDirection()
    {
        if (currentPath.Count == 0 || currentWaypoint >= currentPath.Count)
            return Vector2.zero;

        Vector2 currentPos = enemy.transform.position;
        Vector2 target = currentPath[currentWaypoint];
        Vector2 toTarget = target - currentPos;

        if (toTarget.magnitude < waypointThreshold)
        {
            currentWaypoint++;
            if (currentWaypoint >= currentPath.Count)
                return Vector2.zero;

            target = currentPath[currentWaypoint];
            toTarget = target - currentPos;
        }

        return toTarget.normalized;
    }
}