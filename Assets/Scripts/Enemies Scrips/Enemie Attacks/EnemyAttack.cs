using UnityEngine;

public abstract class EnemyAttack : ScriptableObject
{
    [Header("Attack Conditions")]
    public float minRange = 0.5f;
    public float maxRange = 3f;
    public bool requiresPlayerAirborne = false;
    public bool requiresPlayerDashing = false;
    public float weight = 1f; // Probability bias

    [Header("Positioning Options")]
    public bool useOffset = false;
    public Vector2 offsetFromPlayer; // ex: (0, 2) for jump strike
    public bool useMultipleOffsets = false;
    public Vector2[] possibleOffsets;

    // The enemy decides when to actually execute the attack animation later
    public abstract void Execute(EnemyController controller);

    public virtual Vector2 GetDesiredPosition(Vector2 playerPosition, Vector2 enemyPos)
    {
        if (useMultipleOffsets && possibleOffsets.Length > 0)
            return playerPosition + possibleOffsets[Random.Range(0, possibleOffsets.Length)];
        else if (useOffset)
            return playerPosition + offsetFromPlayer;
        else
            return playerPosition; // default = go directly to player
    }
}