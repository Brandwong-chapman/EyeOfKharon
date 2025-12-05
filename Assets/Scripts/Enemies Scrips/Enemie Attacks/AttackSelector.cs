using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/AttackSelector")]
public class AttackSelector : ScriptableObject
{
    

[Header("Weight Influence")]
public float distanceWeightInfluence = 1.0f;   // Higher = distance matters more
public float airborneBonus = 2.0f;             // Multiplier if player airborne
public float dashBonus = 2.0f;                 // Multiplier if player is dashing
public EnemyAttack ChooseAttack(EnemyController enemy)
    {
        float dist = Vector2.Distance(enemy.transform.position, enemy.playerPosition.position);
        bool playerAirborne = enemy.PlayerIsAirborne;
        bool playerDashing = enemy.PlayerIsDashing;

        var valid = enemy.data.attacks.Where(a =>
                dist >= a.minRange &&
                dist <= a.maxRange
            // Add these later when player motor is hooked in:
            // (!a.requiresPlayerAirborne || !enemy.playerMotor.Grounded) &&
            // (!a.requiresPlayerDashing || enemy.playerMotor.IsDashing)
        ).ToList();

        if (valid.Count == 0)
            return null;

        float totalWeight = valid.Sum(a => a.weight);
        float rand = Random.Range(0, totalWeight);

        float cumulative = 0f;
        foreach (var attack in valid)
        {
            cumulative += attack.weight;
            if (rand <= cumulative)
                return attack;
        }

        return valid[valid.Count - 1];
    }

}