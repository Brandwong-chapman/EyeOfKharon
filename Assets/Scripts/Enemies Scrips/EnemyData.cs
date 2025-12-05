using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/EnemyData")]
public class EnemyData : ScriptableObject {
    [Header("General")]
    public string enemyName;
    public float maxHP;
    public float moveSpeed;
    public float knockbackResistance;
    
    [Header("Movement Settings")]
    public MovementType movementType;
    public MovementAI movementAI;                
    public PathfindingStrategy pathfindingStrategy; 
    
    [Header("Combat")]
    public EnemyAttack[] attacks;
    public AttackSelector attackSelector;


    public enum MovementType
    {
        Ground,
        Flying,
        WallClimbing
    }
    
}

